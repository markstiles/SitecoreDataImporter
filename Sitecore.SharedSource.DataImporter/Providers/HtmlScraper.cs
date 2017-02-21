using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.SharedSource.DataImporter.HtmlAgilityPack;
using Sitecore.SharedSource.DataImporter.HtmlScraper;
using System.Data;
using Sitecore.Collections;
using Sitecore.SharedSource.DataImporter.Mappings;

/// <summary>
/// TODO: based on Dev Breakfast meeting input:
///     
///       -Make the field mapping to use the same concept of this module by adding field mappings under the provider
///       -Handle varios xpath mapping logic to target html element 
/// 
/// </summary>

namespace Sitecore.SharedSource.DataImporter.Providers
{
    public class HtmlScraper : BaseDataMap 
    {
        public ImportConfig Config { get; set; }      
        private Item ImportItem = null;
        private string ItemNameColumn = "ItemName";
        private string PathColumn = "Path";
        private NameValueCollection mappings;

        public HtmlScraper(Database db, string ConnectionString, Item importItem)
            : base(db, ConnectionString, importItem)
        {
            ImportItem = importItem;
        }

        #region Override Methods

        /// <summary>
        /// uses the query field to retrieve file data
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<object> GetImportData()
        {
            mappings = GetMappings();
            DataTable dt = new DataTable();
            dt.Columns.Add(ItemNameColumn);
            dt.Columns.Add(PathColumn);

            ImportConfig config = new ImportConfig(ImportItem, SitecoreDB, this.Query);
            config.ImportLocation = this.Parent;
            Config = config;

            //ImportContent(config);
            //List<string> lines = new List<string>();

            if (ItemNameDataField == "[URL]")
            {
                ItemNameDataField = ItemNameColumn;
            }

            //Adding columns to the table from field mapping
            foreach (var key in mappings)
            {
                string toField = mappings[key.ToString()];
                dt.Columns.Add(toField);
            }

            List<string> urls = config.StoredURLs;
            foreach (var url in urls)
            {
                bool ignoreroots = config.IgnoreRootDirectories ? true : false;

                string relativeURL = url.Replace("http://", "").Replace("https://", "");
                Char[] splitChars = new Char[] { '/' };
                //parts is the directory list in array format, ie. second array is the child of very first etc..
                List<string> levels = ignoreroots ? relativeURL.Split(splitChars, StringSplitOptions.RemoveEmptyEntries).Skip(1).ToList() :
                relativeURL.Split(splitChars, StringSplitOptions.RemoveEmptyEntries).ToList();

                BuildData(config, levels, url, dt);
            }

            return (dt.Rows).Cast<object>();
        }


        /// <summary>
        /// There is no custom data for this type
        /// </summary>
        /// <param name="newItem"></param>
        /// <param name="importRow"></param>
        public override void ProcessCustomData(ref Item newItem, object importRow)
        {
        }

        protected override Item GetParentNode(object importRow, string newItemName)
        {
            Item parentItem = null;
            DataRow dataRow = importRow as DataRow;
            string parentPath = dataRow[PathColumn].ToString();
            string itemName = dataRow[ItemNameColumn].ToString();
            parentPath = parentPath.Replace(itemName, "");

            
            if (Config.MaintainHierarchy)
            {
                parentItem = this.SitecoreDB.GetItem(parentPath);
            }

            if (parentItem == null)
            {
                parentItem = base.GetParentNode(importRow, newItemName);
            }

            return parentItem;
        }

        /// <summary>
        /// gets a field value from an item
        /// </summary>
        /// <param name="importRow"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        protected override string GetFieldValue(object importRow, string fieldName)
        {
            string toFieldName = fieldName;
            if (fieldName != ItemNameColumn)
            {                
                toFieldName = mappings[fieldName];
            }

            //if (toFieldName.Contains("_"))
            //{
            //    int removeIndex = toFieldName.IndexOf("_");
            //    toFieldName = toFieldName.Substring(0, removeIndex);
            //}


            DataRow item = importRow as DataRow;
            object f = item[toFieldName];
            return (f != null) ? f.ToString() : string.Empty;
        }

        #endregion Override Methods

        public string RemoveInvalidChars(string data)
        {
            data = ItemUtil.ProposeValidItemName(data);
            return data;
        }


        private NameValueCollection DirectroyBuilder(List<string> directroies, ImportConfig selectedConfig)
        {
            NameValueCollection dirs = new NameValueCollection();
            string prevDir = string.Empty;
            string rootPath = selectedConfig.ImportLocation.Paths.FullPath;

            foreach (string dir in directroies)
            {
                string directroy = RemoveInvalidChars(dir);
                string fullPath = string.Empty;

                if (string.IsNullOrEmpty(prevDir))
                {
                    fullPath = rootPath + "/" + directroy;
                    dirs.Add(fullPath, dir);
                }
                else
                {
                    fullPath = prevDir + "/" + directroy;
                    dirs.Add(fullPath, dir);
                }

                prevDir = fullPath;

            }
            return dirs;
        }


        private void BuildData(ImportConfig storedConfig, List<string> levels, string url, DataTable dataTable)
        {
            
            Item location = storedConfig.ImportLocation;
           
            bool ignoreroot = storedConfig.IgnoreRootDirectories;
            NameValueCollection directroies = Config.MaintainHierarchy ? DirectroyBuilder(levels, storedConfig) : null;

            if (directroies == null)
            {
                directroies = new NameValueCollection();
                foreach (var l in levels)
                {
                    directroies.Add(l, "");
                }
            }

            string dirPath = string.Empty;
            string prevDir = string.Empty;

            foreach (string dir in directroies)
            {
                List<string> updatedFields = new List<string>();
                bool wasupdated = false;
                DataRow dr = dataTable.NewRow();
                dataTable.Rows.Add(dr);

                string name = string.Empty;
                string urlVal = directroies[dir].ToString();

                if (Config.MaintainHierarchy)
                {
                    dirPath = dir;
                    name = dir.Split('/').Last();

                    if (!string.IsNullOrEmpty(prevDir))
                    {
                        location = this.SitecoreDB.GetItem(prevDir);
                    }
                    else
                    {
                        location = this.SitecoreDB.GetItem(dir);
                    }

                    if (location == null)
                    {
                        location = storedConfig.ImportLocation;
                    }
                }
                else
                {
                    name = RemoveInvalidChars(dir);
                    dirPath = location.Paths.FullPath + "/" + name;
                    urlVal = dir;
                }

                dr[PathColumn] = dirPath;
                if (ItemNameDataField == "[URL]" || ItemNameDataField == ItemNameColumn)
                {                  
                    dr[ItemNameColumn] = name;
                }

                string currentDirURL = string.Empty;
                currentDirURL = url.Substring(0, url.IndexOf(urlVal));
                currentDirURL = currentDirURL.EndsWith("/") ? (currentDirURL + urlVal) : (currentDirURL + "/" + urlVal);
                HtmlNode.ElementsFlags.Remove("form");
                HtmlDocument doc = new HtmlDocument();
                var contentHtml = WebContentRequest(currentDirURL);
                doc.LoadHtml(contentHtml);

                foreach (var key in mappings)
                {
                    string toFieldName = mappings[key.ToString()];
                    bool isOverride = toFieldName.StartsWith("!");

                    //This is for if 2 mapping target the same field so the next one to not override the first update, 
                    //becuase each could apply for different URL  
                    wasupdated = IsDataInList(updatedFields, toFieldName);
                    if (!wasupdated || isOverride)
                    {
                        toFieldName = toFieldName.Replace("!", "");
                        string value = FetchContent(doc, key, mappings, storedConfig);
                        dr[toFieldName] = value;
                        updatedFields.Add(toFieldName);
                    }                    
                }                   
                prevDir = dir;
            }
        }



        public string FetchContent(HtmlDocument doc, object key, NameValueCollection mappings, ImportConfig storedConfig)
        {
            string rowValue = string.Empty;
            bool textonly = storedConfig.ImportTextOnly;
            List<string> updatedIndexedFields = new List<string>();
            string htmlObj = string.Empty;
            string fieldname = string.Empty;
            HtmlNode node = null;
           
            //TODO: index logic seems to be workign good, 
            //think of adding logic where instead index using + to append data on same field
            //Mapping examples added: .content/1/p/3:Main Content, title:Title, #corp-info-container:Main Content

            htmlObj = key.ToString();
            fieldname = mappings[key.ToString()];
            node = HandleNodesLookup(htmlObj, doc);

            if (node != null)
            {              
               rowValue = textonly ? node.InnerText : node.InnerHtml;
            }

            return rowValue;
        }


        private NameValueCollection GetMappings()
        {
            NameValueCollection mappings = new NameValueCollection();
            Item fieldDefinitions = GetItemByTemplate(ImportItem, FieldsFolderID);
            ChildList fields = fieldDefinitions.GetChildren();
            foreach (Item field in fields)
            {
                BaseMapping baseMap = new BaseMapping(field);
                string fromFieldName = baseMap.OldItemField;
                string toFieldName = baseMap.NewItemField + "_" + Guid.NewGuid().ToString().Replace("-", ""); 
                mappings.Add(fromFieldName, toFieldName);
            }
            return mappings;
        }


        /// <summary>
        /// Calculate the node path logic and return the node to be used for the field value
        /// </summary>
        /// <param name="htmlObj"></param>
        /// <param name="doc"></param>
        /// <returns></returns>
        private HtmlNode HandleNodesLookup(string htmlObj, HtmlDocument doc)
        {

            HtmlNode node = null;
            List<HtmlNode> nodes = new List<HtmlNode>();
            bool isMultiNodesData = htmlObj.Contains("/*");
            htmlObj = isMultiNodesData ? htmlObj.Replace("/*","") : htmlObj;
            try
            {
                //Not sure if this check is needed so to do somthing else. Check it latter.  
                //if (htmlObj.Contains("/"))
                //{
                //    nodes = HandleXPathQuery(htmlObj, doc);
                //}
                //else
                //{
                //    nodes = HandleXPathQuery(htmlObj, doc);
                //}

                nodes = HandleXPathQuery(htmlObj, doc);

                if (isMultiNodesData)
                {
                    //TODO: this is worked for * but run some more test with differnt type mapping options of *
                    node = doc.CreateNode(HtmlNodeType.Element, 0);
                    foreach (HtmlNode n in nodes)
                    {
                        node.AppendChild(n);
                    }
                }
                else
                {
                    node = nodes.FirstOrDefault();
                }

            }
            catch
            {

            }

            return node;
        }



        private string FormatXpath(string data, string option) {
            string formated = data;
            string value = data;

            switch (option)
            {
                case ".":
                    value = value.Replace(".", "");
                    formated = "[@class='" + value + "']";
                    break;
                case "#":
                    value = value.Replace("#", "");
                    formated = "[@id='" + value + "']";
                    break;
            }

            return formated;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="selector"></param>
        /// <param name="doc"></param>
        /// <returns></returns>
        private List<HtmlNode> HandleXPathQuery(string selector, HtmlDocument doc)
        {
            string xPath = "//";
            string attrName = "";
            List<HtmlNode> nodes = new List<HtmlNode>();
            List<string> dataItems = selector.Split('/').ToList();
            bool isTagName = true;

            if (selector.StartsWith(".") || selector.StartsWith("#"))
            {
                isTagName = false;
            }

            if (dataItems != null && dataItems.Any() && selector.Contains("/"))
            {
                foreach (var data in dataItems)
                {
                    string option = data.ToCharArray().FirstOrDefault().ToString();
                    attrName = FormatXpath(data, option);
                    selector = selector.Replace(data, attrName);
                    selector = selector.Replace("/[", "/*[");
                }

                if (!isTagName)
                {
                    xPath += "*" + selector;
                }
                else {
                    xPath += selector;
                }
                
            }
            else
            {
                if (selector.StartsWith("."))
                {
                    attrName = FormatXpath(selector, ".");
                    selector = "*"+ selector.Replace(selector, attrName);
                }
                else if (selector.StartsWith("#"))
                {
                    attrName = FormatXpath(selector, "#");
                    selector = "*"+ selector.Replace(selector, attrName);
                }

                xPath += selector;
            }

            xPath = HandleIndex(xPath);

            nodes = doc.DocumentNode.SelectNodes(xPath).ToList();
            return nodes;
        }

        /// <summary>
        /// 
        /// ie. [1]/p[3]
        /// </summary>
        /// <param name="data"></param>
        private string HandleIndex(string data)
        {
            //data = data.Replace("/*", "");
            string[] splits = data.ToString().Split('/');
            //string indexUpdates = string.Empty;

            foreach (var s in splits)
            {
                int indexOut;
                //If true, then it is numeric index
                if (int.TryParse(s, out indexOut))
                {

                    //string lookUp = "/" + s + "/";
                    string lookUp2 = "/" + s;
                    string replace = "[" + s + "]";
                    //indexUpdates += data.Replace(lookUp, replace);
                    data = data.Replace(lookUp2, replace);
                }
            }


            return data;
            //switch (splits)
            //{
            //    case "*":
            //        break;
            //    default:
            //        int indexOut;
            //        //If true, then it is numeric index
            //        if (int.TryParse(splits, out indexOut))
            //        {
            //            data = data.Replace("/" + splits, "[" + splits + "]");
            //        }
            //        break;
            //}

        }


        private bool IsDataInList(List<string> dataList, string data)
        {
            return dataList.Any(f => data.ToLower() == f.ToLower());
        }


        private string WebContentRequest(string url)
        {
            string content = string.Empty;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "GET";


                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.OK:
                            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                            {
                                content = reader.ReadToEnd();
                            }
                            break;
                    }
                }
            }
            catch (Exception x)
            {
                //TODO: Add tracking log error under the config
            }

            return content;
        }
    }
}
