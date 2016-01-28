using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Resources.Media;
using Sitecore.SharedSource.DataImporter.Providers;
using Sitecore.SharedSource.DataImporter.Utility;

namespace Sitecore.SharedSource.DataImporter.Mappings.Fields {

    public class ToRichText : ToText
    {

        #region Properties

        public IEnumerable<string> UnwantedTags { get; set; }

        public IEnumerable<string> UnwantedAttributes { get; set; }

        public Item MediaParentItem { get; set; }

        #endregion Properties

        #region Constructor

        public ToRichText(Item i)
            : base(i)
        {
            //store fields
            UnwantedTags = GetItemField(i, "Unwanted Tags").Split(comSplitr, StringSplitOptions.RemoveEmptyEntries);
            UnwantedAttributes = GetItemField(i, "Unwanted Attributes")
                .Split(comSplitr, StringSplitOptions.RemoveEmptyEntries);
            MediaParentItem = i.Database.GetItem(GetItemField(i, "Media Parent Item"));
        }

        #endregion Constructor

        #region IBaseField

        public override void FillField(IDataMap map, ref Item newItem, string importValue)
        {

            Field f = newItem.Fields[NewItemField];
            if (f != null)
                f.Value = CleanHtml(map, newItem.Paths.FullPath, importValue);
        }

        #endregion IBaseField

        public string CleanHtml(IDataMap map, string itemPath, string html)
        {
            if (String.IsNullOrEmpty(html))
                return html;

            var document = new HtmlDocument();
            document.LoadHtml(html);

            HtmlNodeCollection tryGetNodes = document.DocumentNode.SelectNodes("./*|./text()");

            if (tryGetNodes == null || !tryGetNodes.Any())
                return html;

            var nodes = new Queue<HtmlNode>(tryGetNodes);

            int i = 0;
            while (nodes.Count > 0)
            {
                var node = nodes.Dequeue();
                var nodeName = node.Name.ToLower();
                var parentNode = node.ParentNode;
                var childNodes = node.SelectNodes("./*|./text()");

                if (childNodes != null)
                {
                    foreach (var child in childNodes)
                        nodes.Enqueue(child);
                }

                if (UnwantedTags.Any(tag => tag == nodeName))
                {
                    // if this node is one to remove
                    if (childNodes != null)
                    {
                        // make sure children are added back
                        foreach (var child in childNodes)
                            parentNode.InsertBefore(child, node);
                    }

                    parentNode.RemoveChild(node);
                }
                else if (node.HasAttributes)
                {
                    // if it's not being removed
                    foreach (string s in UnwantedAttributes) // remove unwanted attributes
                        node.Attributes.Remove(s);

                    //replace images
                    if (nodeName.Equals("img"))
                    {
                        // see if it exists
                        string imgSrc = node.Attributes["src"].Value;
                        MediaItem newImg = HandleImage(map, MediaParentItem, itemPath, imgSrc);
                        if (newImg != null)
                        {
                            string newSrc = string.Format("-/media/{0}.ashx", newImg.ID.ToShortID().ToString());
                            // replace the node with sitecore tag
                            node.SetAttributeValue("src", newSrc);
                        }
                    }
                }

                i++;
            }

            return document.DocumentNode.InnerHtml;
        }

        public MediaItem HandleImage(IDataMap map, Item parentItem, string itemPath, string url)
        {
            // see if the url is badly formed
            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute)) {
                map.Logger.LogError("Malformed Image URL", string.Format("item '{0}', image url '{1}'", itemPath, url));
                return null;
            }

            //get file info
            List<string> uri = url.Split(new string[] { "?" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            List<string> parts = uri[0].Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            string filePath = parts[parts.Count - 1].Trim();
            string[] fileParts = filePath.Split(new string[] {"."}, StringSplitOptions.RemoveEmptyEntries);
            string fileName = (fileParts.Length > 0) ? StringUtility.GetValidItemName(fileParts[0], map.ItemNameMaxLength) : string.Empty;
            
            //date info
            string newFilePath = string.Format("{0}/{1}", parentItem.Paths.FullPath, fileName);

            // see if it exists in med lib

            IEnumerable<Item> matches = parentItem.Axes.GetDescendants()
                .Where(a => a.Paths.FullPath.EndsWith(fileName));

            if (matches != null && matches.Any()) {
                if (matches.Count().Equals(1))
                    return new MediaItem(matches.First());

                map.Logger.LogError("Sitecore Image Lookup Conflict", string.Format("item '{0}', image name '{1}', {2} matches", itemPath, filePath, matches.Count()));
                return null;
            }
            
            MediaItem m = ImportImage(url, filePath, string.Format("{0}/{1}", parentItem.Paths.FullPath, newFilePath));
            if (m == null)
                map.Logger.LogError("Image Not Found", string.Format("item '{0}', image '{1}'", itemPath, url));

            return m;
        }
        
        public MediaItem ImportImage(string url, string fileName, string newPath) {

            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            if (request == null)
                return null;

            try {
                // download data 
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                Stream stream1 = response.GetResponseStream();
                MemoryStream stream2 = new MemoryStream();
                stream1.CopyTo(stream2);

                // Create the options
                MediaCreatorOptions options = new MediaCreatorOptions();
                options.FileBased = false;
                options.IncludeExtensionInItemName = false;
                options.KeepExisting = false;
                options.Versioned = false;
                options.Destination = newPath;
                options.Database = Sitecore.Configuration.Factory.GetDatabase("master");
                
                // upload to sitecore
                MediaCreator creator = new MediaCreator();
                MediaItem mediaItem = creator.CreateFromStream(stream2, fileName, options);

                response.Close();

                return mediaItem;
            } catch (WebException ex) {
                return null;
            }
        }
    }
}
