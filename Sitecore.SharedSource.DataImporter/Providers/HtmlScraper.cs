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
        public ImportConfig SelectedConfig { get; set; }      
        private Item ImportItem = null;

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
            List<string> lines = new List<string>();

            SelectedConfig = new ImportConfig(ImportItem, this.SitecoreDB, this.Query);
            SelectedConfig.ImportLocation = Parent;
            ImportContent(SelectedConfig);

            //DataSet ds = new DataSet();
            //SqlConnection dbCon = new SqlConnection(this.DatabaseConnectionString);
            //dbCon.Open();

            //SqlDataAdapter adapter = new SqlDataAdapter(this.Query, dbCon);
            //adapter.Fill(ds);
            //dbCon.Close();

            //DataTable dt = ds.Tables[0].Copy();

            //return (from DataRow dr in dt.Rows
            //        select dr).Cast<object>();

            return lines;
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

            return this.Parent;
        }

        /// <summary>
        /// gets a field value from an item
        /// </summary>
        /// <param name="importRow"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        protected override string GetFieldValue(object importRow, string fieldName)
        {

            return "";
        }

        #endregion Override Methods

        private void ImportContent(ImportConfig config)
        {
            List<string> urls = config.StoredURLs;
            foreach (var url in urls)
            {
                bool ignoreroots = config.IgnoreRootDirectories ? true : false;

                string relativeURL = url.Replace("http://", "").Replace("https://", "");
                Char[] splitChars = new Char[] { '/' };
                //parts is the directory list in array format, ie. second array is the child of very first etc..
                List<string> levels = ignoreroots ? relativeURL.Split(splitChars, StringSplitOptions.RemoveEmptyEntries).Skip(1).ToList() :
                relativeURL.Split(splitChars, StringSplitOptions.RemoveEmptyEntries).ToList();

                Item loc = AddToLocation(config, levels, url);
            }
        }

        private bool UseSmartDirectory(ImportConfig selectedConfig)
        {
            return selectedConfig.EnableSmartDirectory;

        }

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

        private Item AddToLocation(ImportConfig storedConfig, List<string> levels, string url)
        {
            //ran some directory tests (seems good for now, if needed later)
            Item location = storedConfig.ImportLocation;
            bool smart = UseSmartDirectory(storedConfig);
            bool ignoreroot =  storedConfig.IgnoreRootDirectories;
            NameValueCollection directroies = smart ? DirectroyBuilder(levels, storedConfig) : null;

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
                string name = string.Empty;
                string urlVal = directroies[dir].ToString();

                if (smart)
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


                string currentDirURL = string.Empty;
                currentDirURL = url.Substring(0, url.IndexOf(urlVal));
                currentDirURL = currentDirURL.EndsWith("/") ? (currentDirURL + urlVal) : (currentDirURL + "/" + urlVal);

                HtmlNode.ElementsFlags.Remove("form");
                HtmlDocument doc = new HtmlDocument();
                var contentHtml = WebContentRequest(currentDirURL);
                doc.LoadHtml(contentHtml);

                foreach (var im in storedConfig.ImportMappings)
                {
                    Item i = IsItemAdded(dirPath);
                    if (i == null)
                    {
                        InsertContent(name, location, im.TemplateItem, contentHtml, im, storedConfig);
                    }
                    else
                    {
                        InsertContent(name, location, im.TemplateItem, contentHtml, im, storedConfig, i);
                    }
                }

                prevDir = dir;
            }

            return location;
        }

        public void InsertContent(string name, Item InsertLocation, TemplateItem template, string contentHtml, ImportMappings im, ImportConfig storedConfig, Item updatedItem = null)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(contentHtml);
            bool textonly = storedConfig.ImportTextOnly;
            using (new Sitecore.SecurityModel.SecurityDisabler())
            {
                List<string> updatedFields = new List<string>();
                List<string> updatedIndexedFields = new List<string>();
                Item i = updatedItem != null ? updatedItem : InsertLocation.Add(RemoveInvalidChars(name), template);
                string htmlObj = string.Empty;
                string fieldname = string.Empty;
                HtmlNode node = null;
                i.Editing.BeginEdit();

                bool wasupdated = false;
                foreach (var key in im.Mappings)
                {
                    bool isOverride = false;
                    bool isOneToOneMap = false;

                    //TODO: index logic seems to be workign good, 
                    //think of adding logic where instead index using + to append data on same field
                    //Mapping examples added: .content/1/p/3:Main Content, title:Title, #corp-info-container:Main Content


                    htmlObj = key.ToString();
                    fieldname = im.Mappings[key.ToString()];
                    fieldname = fieldname.Replace("\r", "");

                    if (htmlObj.StartsWith("!"))
                    {
                        isOverride = true;                       
                        htmlObj = htmlObj.Replace("!", "");
                    }

                    node = HandleNodesLookup(htmlObj, doc);


                    if (node == null) { continue; }

                    //This is for if 2 mapping target the same field so hte next one to not override the first update, 
                    //becuase each could apply for different URL  
                    wasupdated = IsDataInList(updatedFields, fieldname);

                    if (isOverride || !wasupdated)
                    {
                        //TODO: use dataType to handle each field accordingly 
                        //var dataType = i.Fields[fieldname].Type.ToLower();
                        i.Fields[fieldname].Value = textonly ? node.InnerText : node.InnerHtml;
                        updatedFields.Add(fieldname);
                    }


                }

                i.Editing.EndEdit();
            }

        }

        //TODO: ^^^ work on this next.
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
            try
            {


                //Not sure if this check is needed so to do somthing else. Check it latter.  
                if (htmlObj.Contains("/"))
                {
                    nodes = HandleStartWith(htmlObj, doc);
                }
                else
                {
                    nodes = HandleStartWith(htmlObj, doc);
                }

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


        /// <summary>
        ///
        /// </summary>
        /// <param name="htmlObj"></param>
        /// <param name="doc"></param>
        /// <returns></returns>
        private List<HtmlNode> HandleStartWith(string htmlObj, HtmlDocument doc)
        {
            string xPath = "//";
            string attrName = "";
            List<HtmlNode> nodes = new List<HtmlNode>();

            if (htmlObj.StartsWith("."))
            {
                attrName = htmlObj.Split('/').First();
                htmlObj = htmlObj.Replace(attrName, "");
                attrName = attrName.Replace(".", "");
                htmlObj = HandleIndex(htmlObj);
                xPath += "*[@class='" + attrName + "']" + htmlObj + "";

                // //div[@class='content']/p[3]
                // //*[@class='content'][1]/p[3]"
                nodes = doc.DocumentNode.SelectNodes(xPath).ToList();

            }
            else if (htmlObj.StartsWith("#"))
            {
                attrName = htmlObj.Split('/').First();
                htmlObj = htmlObj.Replace(attrName, "");
                attrName = attrName.Replace("#", "");
                htmlObj = HandleIndex(htmlObj);
                xPath += "*[@id='" + attrName + "']" + htmlObj + "";
                nodes = doc.DocumentNode.SelectNodes(xPath).ToList();
            }
            else
            {
                //attrName = htmlObj.Split('/').First();
                //htmlObj = htmlObj.Replace(attrName, "");
                //attrName = attrName.Replace("#", "");
                htmlObj = HandleIndex(htmlObj);
                xPath += htmlObj;
                nodes = doc.DocumentNode.SelectNodes(xPath).ToList();

            }

            return nodes;
        }

        /// <summary>
        /// 
        /// ie. [1]/p[3]
        /// </summary>
        /// <param name="data"></param>
        private string HandleIndex(string data)
        {
            data = data.Replace("/*", "");
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
            return dataList.Any(f => data.ToLower().Contains(f.ToLower()));
        }

        private Item IsItemAdded(string path)
        {
            Item i = this.SitecoreDB.GetItem(path);

            return i;
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
