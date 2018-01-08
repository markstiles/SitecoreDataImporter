using Sitecore.Data.Items;
using Sitecore.SharedSource.DataImporter.HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sitecore.SharedSource.DataImporter.HtmlScraper;
using Sitecore.SharedSource.DataImporter.Mappings;
using System.Net;
using System.IO;
using Sitecore.Resources.Media;
using Sitecore.Configuration;
using System.Drawing;

namespace Sitecore.SharedSource.DataImporter.Processors
{
    public class MediaImporter
    {
        public void Run(Item processor, Item itemToProcess, Item fieldMapping)
        {
            //SHOULD BE FROM SITECORE
            Uri baseUri = new Uri("http://www.upmc.com");
            BaseMapping baseMap = new BaseMapping(fieldMapping);
            //SHOULD BE MOVED INTO SITECORE
            //SHOULD BE MOVED INTO SITECORE. COULD TRY HAVING A LOOKUP BY NAME ON THE TEMPLATES FOLDER TO MATCH AGAINST THE EXTENSION.
            var imageTemplate = itemToProcess.Database.GetTemplate("{C97BA923-8009-4858-BDD5-D8BE5FCCECF7}");
            var jpgTemplate = itemToProcess.Database.GetTemplate("{EB3FB96C-D56B-4AC9-97F8-F07B24BB9BF7}");
            var pdfTemplate = itemToProcess.Database.GetTemplate("{CC80011D-8EAE-4BFC-84F1-67ECD0223E9E}");
            var fileTemplate = itemToProcess.Database.GetTemplate("{611933AC-CE0C-4DDC-9683-F830232DB150}");
            Dictionary<string, string> TagMappings = new Dictionary<string, string>(){
                { "img" , "src" },
                { "a" , "href" }
            };
            Dictionary<string, TemplateItem> MediaMappings = new Dictionary<string, TemplateItem>() {
                { "jpg", jpgTemplate },
                { "pdf", pdfTemplate },
                { "png", imageTemplate }
            };

            HtmlDocument document = new HtmlDocument();
            string content = itemToProcess.Fields[baseMap.NewItemField].Value;
            document.LoadHtml(content);

            using (new SecurityModel.SecurityDisabler())
            {
                foreach(var mapping in TagMappings)
                {
                    var nodes = document.DocumentNode.SelectNodes(string.Format("//{0}/@{1}",mapping.Key,mapping.Value));
                    if (nodes == null)
                    {
                        continue;
                    }
                    foreach (var child in nodes)
                    {
                        string source = child.Attributes[mapping.Value].Value;
                        if (string.IsNullOrEmpty(Path.GetExtension(source)))
                        {
                            continue;
                        }
                        if (!MediaMappings.ContainsKey(Path.GetExtension(source).ToLower().Replace(".","")))
                        {
                            continue;
                        }
                        Item importedMediaItem = null;
                        Uri mediaSource = new Uri(baseUri, source);
                        //SHOULD BE MOVED INTO SITECORE
                        string destination = string.Empty;
                        if(mediaSource.Host == baseUri.Host)
                        {
                            destination = string.Concat("/sitecore/media library/MigratedMedia/", Path.GetDirectoryName(source), "/");
                        }
                        else
                        {
                            destination = string.Concat("/sitecore/media library/MigratedMedia/", Path.GetDirectoryName(mediaSource.PathAndQuery), "/");
                        }
                        destination = destination.Replace("\\", "/").Replace("//", "/");


                        if (string.IsNullOrEmpty(source))
                        {
                            continue;
                        }
                        var webRequest = WebRequest.Create(mediaSource);
                        using (var webResponse = webRequest.GetResponse())
                        {
                            using (var stream = webResponse.GetResponseStream())
                            {
                                if (stream == null)
                                {
                                    continue;
                                }
                                using (var memoryStream = new MemoryStream())
                                {
                                    stream.CopyTo(memoryStream);

                                    var mediaCreator = new MediaCreator();
                                    if (itemToProcess.Database.GetItem(string.Concat(destination, ItemUtil.ProposeValidItemName(Path.GetFileNameWithoutExtension(source)))) != null)
                                    {
                                        continue;
                                    }
                                    var options = new MediaCreatorOptions
                                    {
                                        Versioned = true,
                                        IncludeExtensionInItemName = false,
                                        Database = itemToProcess.Database,
                                        Destination = destination
                                    };
                                    importedMediaItem = mediaCreator.CreateFromStream(memoryStream, Path.GetFileNameWithoutExtension(source), options);
                                    Image image;

                                    importedMediaItem.Editing.BeginEdit();

                                    importedMediaItem.Name = ItemUtil.ProposeValidItemName(Path.GetFileNameWithoutExtension(source));
                                    //SHOULD BE MOVED INTO SITECORE AND MADE INTO A LOOP
                                    if (Path.GetExtension(source) == "jpg")
                                    {
                                        importedMediaItem.ChangeTemplate(jpgTemplate);
                                    }
                                    else if (Path.GetExtension(source) == "pdf")
                                    {
                                        importedMediaItem.ChangeTemplate(pdfTemplate);
                                    }
                                    else
                                    {
                                        importedMediaItem.ChangeTemplate(fileTemplate);
                                    }

                                    importedMediaItem.Editing.BeginEdit();
                                    if(importedMediaItem.Fields["Extension"] != null)
                                    {
                                        importedMediaItem.Fields["Extension"].Value = Path.GetExtension(source);
                                    }
                                    try
                                    {
                                        image = Image.FromStream(memoryStream);
                                        if (importedMediaItem.Fields["Height"] != null)
                                        {
                                            importedMediaItem.Fields["Height"].Value = image.Height.ToString();
                                        }
                                        if (importedMediaItem.Fields["Width"] != null)
                                        {
                                            importedMediaItem.Fields["Width"].Value = image.Width.ToString();
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        //Stream is not an image, its a pdf
                                    }

                                    if (importedMediaItem.Fields["alt"] != null)
                                    {
                                        if (child.Attributes.Contains("alt"))
                                        {
                                            importedMediaItem.Fields["alt"].Value = child.Attributes["alt"].Value;
                                        }
                                        else
                                        {
                                            importedMediaItem.Fields["alt"].Value = importedMediaItem.Name;
                                        }
                                    }

                                    importedMediaItem.Editing.EndEdit();
                                }
                            }
                        }
                        importedMediaItem = itemToProcess.Database.GetItem(string.Concat(destination, ItemUtil.ProposeValidItemName(Path.GetFileNameWithoutExtension(source))));
                        if (importedMediaItem != null)
                        {
                            //RESEARCH WHY THE ADDTIIONAL SITECORE MODULES PORTION ARRIVED. GETMEDIAURL SHOULD WORK WITHOUT FURTHER MODIFICATIONS
                            var theURL = MediaManager.GetMediaUrl(importedMediaItem);
                            var mediaUrl = HashingUtils.ProtectAssetUrl(theURL).Replace("/sitecore-modules/shell", "");

                            HtmlNode newChild = child.Clone();
                            newChild.Attributes[mapping.Value].Value = mediaUrl;

                            content = content.Replace(child.OuterHtml, newChild.OuterHtml);
                            itemToProcess.Editing.BeginEdit();
                            itemToProcess.Fields[baseMap.NewItemField].Value = content;
                            itemToProcess.Editing.EndEdit();
                        }
                    }
                }
            }
        }

    }
}
