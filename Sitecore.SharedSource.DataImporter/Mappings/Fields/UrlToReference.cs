using System;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.SharedSource.DataImporter.Providers;

namespace Sitecore.SharedSource.DataImporter.Mappings.Fields {
    /// <summary>
    /// Class form importing ReferenceFields like DropList
    /// </summary>
    public class UrlToReference : ToText {
       
        #region Properties

        private const string SiteHomeItemFieldName = "SiteHomeItem";

        /// <summary>
        /// Home item to user when creating Reference Field paths
        /// </summary>
        public string SiteHomeItemID { get; set; }

        #endregion Properties

        #region Constructor

        //constructor
        public UrlToReference(Item i)
            : base(i) {
            SiteHomeItemID = GetItemField(i, SiteHomeItemFieldName);
        }

        #endregion Constructor

        #region IBaseField

        public override void FillField(IDataMap map, ref Item newItem, string importValue) {

            if (string.IsNullOrEmpty(importValue))
                return;

            //get the field as a link field and store the url
            ReferenceField lf = newItem.Fields[NewItemField];

            if (lf != null) {
                //Try to get link using and item path
                //i.e: /sitecore/content/sitename/path-to-target
                Item importItem = map.ToDB.GetItem(importValue);

                //Try to get link from URL
                string pathFromUrl = string.Empty;
                if (importItem == null && importValue.StartsWith("http")) {
                    pathFromUrl = GetItemPathFromUrl(map, importValue);
                    importItem = map.ToDB.GetItem(pathFromUrl);
                }

                if (importItem != null)
                    lf.Value = importItem.ID.ToString();
                else {
                    string errorMessage = string.Format("Could not find item at the following locations for field: {0}: \n", NewItemField);
                    errorMessage += string.Format("Original Value: {0}\n", importValue);

                    if (!string.IsNullOrEmpty(pathFromUrl))
                        errorMessage += string.Format("Path from URL: {0}\n", pathFromUrl);

                    throw new Exception(errorMessage);
                }
            }
        }

        // if you have the full URL with protocol and host
        private string GetItemPathFromUrl(IDataMap map, string url) {
            Item homeItem = map.ToDB.GetItem(SiteHomeItemID);

            if (homeItem == null) {
                throw new Exception(string.Format("For URL importing {0} must have a value and point to a valid item. Current Value: '{1}'", SiteHomeItemFieldName, SiteHomeItemID));
            }

            string path = homeItem.Paths.FullPath + new Uri(url).PathAndQuery;

            // remove query string
            if (path.Contains("?"))
                path = path.Split('?')[0];

            path = path.Replace(".aspx", "");

            return path;
        }

        // if you have just the path after the hostname
        private Item GetItemFromPath(IDataMap map, string path) {
            // remove query string
            if (path.Contains("?"))
                path = path.Split('?')[0];

            path = path.Replace(".aspx", "");

            return map.ToDB.GetItem(path);
        }

        //// if you have the full URL with protocol and host
        //private Item GetItemFromUrl(IDataMap map, string url)
        //{
        //    Item homeItem = map.ToDB.GetItem(SiteHomeItemID);

        //    if(homeItem == null)
        //    {
        //        throw new Exception(string.Format("For URL importing {0} must have a value and point to a valid item. Current Value: '{1}'", SiteHomeItemFieldName, SiteHomeItemID));
        //    }

        //    string path = homeItem.Paths.FullPath + new Uri(url).PathAndQuery;

        //    return GetItemFromPath(map, path);    
        //}

        //// if you have just the path after the hostname
        //private Item GetItemFromPath(IDataMap map, string path)
        //{
        //    // remove query string
        //    if (path.Contains("?"))
        //        path = path.Split('?')[0];

        //    path = path.Replace(".aspx", "");

        //    return map.ToDB.GetItem(path);
        //}

        #endregion IBaseField
    }
}
