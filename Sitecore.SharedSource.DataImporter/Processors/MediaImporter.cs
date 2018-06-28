using Sitecore.Data.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore.SharedSource.DataImporter.HtmlScraper;
using Sitecore.SharedSource.DataImporter.Mappings;
using System.IO;
using HtmlAgilityPack;
using Sitecore.SharedSource.DataImporter.Processors.Helpers;
using Sitecore.SharedSource.DataImporter.Reporting;

namespace Sitecore.SharedSource.DataImporter.Processors
{
    public class MediaImporter
    {
        
        public void Run(Item processor, Item itemToProcess, Item fieldMapping)
        {
            ImportConfig config = new ImportConfig(fieldMapping.Parent.Parent, fieldMapping.Database, "");
            Uri baseUri = new Uri(config.BaseUrl);
            BaseMapping baseMap = new BaseMapping(fieldMapping);
            MediaProcessor mediaProcessor = new MediaProcessor(processor);
            HtmlDocument document = new HtmlDocument();
            string content = itemToProcess.Fields[baseMap.NewItemField].Value;
            document.LoadHtml(content);

            using (new SecurityModel.SecurityDisabler())
            {
                foreach (var mediaType in mediaProcessor.MediaTypes)
                {
                    var nodes = document.DocumentNode.SelectNodes(string.Format("//{0}/@{1}",mediaType.Identifier,mediaType.Attribute));
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

                            string destination = mediaProcessor.RetrieveDestination(processor, mediaProcessor.RootDestination, source, mediaSource, baseUri);

                            Item importedMediaItem = MediaUpload.UploadMedia(mediaSource.ToString(), destination, itemToProcess);

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
                                ImportReporter.Write(itemToProcess, Level.Info, string.Format("Link updated for: {0}",importedMediaItem.Name), baseMap.NewItemField, "Media Importer");
                            }
                        }
                        catch(Exception ex)
                        {
                            ImportReporter.Write(itemToProcess, Level.Error,string.Format("There was an error importing media from {0}. Error: {1}.", string.Concat(baseUri, child.Attributes[mediaType.Attribute].Value),ex.Message),baseMap.NewItemField,"Media Import");
                            //Error importing media and/or updating links to media
                        }
                    }
                }
            }
        }

    }
}
