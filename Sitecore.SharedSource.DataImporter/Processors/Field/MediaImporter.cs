using Sitecore.Data.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore.SharedSource.DataImporter.Mappings;
using System.IO;
using HtmlAgilityPack;
using Sitecore.SharedSource.DataImporter.Logger;
using Sitecore.SharedSource.DataImporter.Services;
using Sitecore.SharedSource.DataImporter.Processors.Models;
using Sitecore.Data.Fields;
using Sitecore.SecurityModel;
using Sitecore.SharedSource.DataImporter.Providers;

namespace Sitecore.SharedSource.DataImporter.Processors.Field
{
    public class MediaImporter : IFieldProcessor
    {
        protected UrlImportMap UrlImportMap;
        protected ILogger Logger;
        protected MediaService MediaService;

        public MediaImporter(UrlImportMap urlImportMap, ILogger logger)
        {
            UrlImportMap = urlImportMap;
            Logger = logger;
            MediaService = new MediaService(logger);
        }

        public void Run(Item processor, Item itemToProcess, Item fieldMapping)
        {
            var MediaTypes = MediaService.RetrieveMediaTypes(processor);
            var importExternalMediaCheck = (CheckboxField)processor.Fields["Import External Media URLs"];
            var baseUri = new Uri(UrlImportMap.BaseUrl);
            var baseMap = new BaseMapping(fieldMapping, Logger);
            var document = new HtmlDocument();
            var content = itemToProcess.Fields[baseMap.NewItemField].Value;
            document.LoadHtml(content);

            using (new SecurityDisabler())
            {
                foreach (var mediaType in MediaTypes)
                {
                    var nodes = document.DocumentNode.SelectNodes($"//{mediaType.Identifier}/@{mediaType.Attribute}");
                    if (nodes == null)
                        continue;

                    //select nodes in html where path ends with extension listed in config
                    List<HtmlNode> targetedNodes = nodes.Where(n => n.Attributes[mediaType.Attribute].Value.Trim().ToLower()
                                        .EndsWith(mediaType.Extension.Trim().ToLower())).ToList();

                    if (targetedNodes == null)
                    {
                        targetedNodes = nodes.Where(n => n.Attributes[mediaType.Attribute].Value.Trim().ToLower()
                                        .Contains(mediaType.Extension.Trim().ToLower())).ToList();
                    }
                    else {
                        targetedNodes.AddRange(nodes.Where(n => n.Attributes[mediaType.Attribute].Value.Trim().ToLower()
                                        .Contains(mediaType.Extension.Trim().ToLower())).ToList());
                    }
                    
                    foreach (var child in targetedNodes)
                    {
                        try
                        {
                            //Make sure the selected tags have media items to import, with existing parameters to follow
                            string source = child.Attributes[mediaType.Attribute].Value;
                            string mediaExtension = Path.GetExtension(source);
                            if (string.IsNullOrEmpty(source))
                                continue;

                          
                            Uri mediaSource = new Uri(source, UriKind.RelativeOrAbsolute);

                            if (mediaSource.IsAbsoluteUri)
                            {
                                if ((mediaSource.Host != baseUri.Host))
                                {
                                    continue;
                                }
                            }
                            else
                            {
                                //This is for internal/relative path images
                                mediaSource = new Uri(baseUri, source);
                            }

                            var root = processor.Fields["Root Destination"].Value;
                            string destination = MediaService.RetrieveDestination(processor, processor.Database.GetItem(root).Paths.Path, source, mediaSource, baseUri);

                            Item importedMediaItem = MediaService.UploadMedia(mediaSource.ToString(), destination, itemToProcess);

                            if (importedMediaItem != null)
                            {
                                var mediaUrl = "-/media/" + importedMediaItem.ID.ToShortID().ToString() + ".ashx";

                                //Swap the old link with the new link to our media library
                                HtmlNode newChild = child.Clone();
                                newChild.Attributes[mediaType.Attribute].Value = mediaUrl;
                                content = content.Replace(child.OuterHtml, newChild.OuterHtml);

                                itemToProcess.Editing.BeginEdit();
                                itemToProcess.Fields[baseMap.NewItemField].Value = content;
                                itemToProcess.Editing.EndEdit();
                                Logger.Log($"Media Importer: Link updated for: {importedMediaItem.Name}", itemToProcess.Paths.FullPath, Providers.ProcessStatus.Info, baseMap.NewItemField);
                            }
                        }
                        catch(Exception ex)
                        {
                            Logger.Log($"Media Importer: There was an error importing media from {string.Concat(baseUri, child.Attributes[mediaType.Attribute].Value)}. Error: {ex.Message}.", itemToProcess.Paths.FullPath, Providers.ProcessStatus.Error, baseMap.NewItemField);
                        }
                    }
                }
            }
        }

    }
}
