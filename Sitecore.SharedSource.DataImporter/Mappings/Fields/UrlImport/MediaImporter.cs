using Sitecore.Data.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore.SharedSource.DataImporter.Mappings;
using System.IO;
using HtmlAgilityPack;
using Sitecore.SharedSource.DataImporter.Logger;
using Sitecore.SharedSource.DataImporter.Services;
using Sitecore.Data.Fields;
using Sitecore.SecurityModel;
using Sitecore.SharedSource.DataImporter.Providers;

namespace Sitecore.SharedSource.DataImporter.Processors.Field
{
    public class MediaImporter
    {
        protected IDataMap DataMap;
        protected ILogger Logger;
        protected MediaService MediaService;

        public MediaImporter(IDataMap dataMap, ILogger logger)
        {
            DataMap = dataMap;
            Logger = logger;
            MediaService = new MediaService(logger);
        }

        public void Run(Item processor, Item itemToProcess, Item fieldMapping)
        {
            var isUrlMap = DataMap is UrlImportMap;
            if (!isUrlMap)
                return;

            var UrlImportMap = (UrlImportMap)DataMap;

            var MediaTypes = MediaService.RetrieveMediaTypes(processor);
            var importExternalMediaCheck = (CheckboxField)processor.Fields["Import External Media URLs"];
            var baseUri = new Uri(UrlImportMap.BaseUrl);
            var toWhatField = fieldMapping.Fields["To What Field"].Value;
            var document = new HtmlDocument();
            var content = itemToProcess.Fields[toWhatField].Value;
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
                            if (!mediaSource.IsAbsoluteUri)
                                mediaSource = new Uri(baseUri, source);
                            else if (mediaSource.IsAbsoluteUri && mediaSource.Host != baseUri.Host)
                                continue;
                            
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
                                itemToProcess.Fields[toWhatField].Value = content;
                                itemToProcess.Editing.EndEdit();
                                Logger.Log($"Media Importer: Link updated for: {importedMediaItem.Name}", itemToProcess.Paths.FullPath, Providers.LogType.Info, toWhatField);
                            }
                        }
                        catch(Exception ex)
                        {
                            Logger.Log($"Media Importer: There was an error importing media from {string.Concat(baseUri, child.Attributes[mediaType.Attribute].Value)}. Error: {ex.Message}.", itemToProcess.Paths.FullPath, Providers.LogType.Error, toWhatField);
                        }
                    }
                }
            }
        }
    }
}
