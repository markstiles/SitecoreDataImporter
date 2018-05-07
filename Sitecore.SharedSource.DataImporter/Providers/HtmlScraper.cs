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
using Sitecore.Data.Fields;
using Sitecore.SharedSource.DataImporter.Processors;
using System.Reflection;
using Sitecore.SharedSource.DataImporter.Reporting;

/// <summary>
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
        private string ActionColumn = "ActionColumn";
        private string RequestedURL = "RequestedURL";
        private NameValueCollection mappings;
        private List<Item> MappingFields;

      
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
            dt.Columns.Add(RequestedURL);
            dt.Columns.Add(ActionColumn);

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
                //parts are the directory list in array format, ie. second array is the child of first etc..
                List<string> levels = ignoreroots ? relativeURL.Split(splitChars, StringSplitOptions.RemoveEmptyEntries).Skip(1).ToList() :
                relativeURL.Split(splitChars, StringSplitOptions.RemoveEmptyEntries).ToList();

                if (config.ExcludeDirectories != null && config.ExcludeDirectories.Any())
                {
                    levels.RemoveAll(x => config.ExcludeDirectories.Contains(x.ToLower()));
                }
                

                BuildData(config, levels, url, dt);
            }


            return (dt.Rows).Cast<object>();
        }

        private void RunProcessor(Item fieldMap, Item newItem)
        {
            //Check for warnings
            MultilistField WarningTags = fieldMap.Fields[DataImporter.HtmlScraper.Constants.FieldNames.WarningTriggerTags];
            if(WarningTags.Count > 0)
            {
                WriteTagWarnings.Run(newItem, fieldMap);
            }

            //"To What Field"
            List<Item> processorList = new List<Item>();
            MultilistField processors = fieldMap.Fields[DataImporter.HtmlScraper.Constants.FieldNames.FieldPostProcessors];

            foreach (var targetId in processors.TargetIDs)
            {
                Item processor = this.SitecoreDB.GetItem(targetId);

                if (processor == null) { continue; }

               Processor.Execute(processor, newItem, fieldMap);
            }
        }


        /// <summary>
        /// There is no custom data for this type
        /// </summary>
        /// <param name="newItem"></param>
        /// <param name="importRow"></param>
        public override void ProcessCustomData(ref Item newItem, object importRow)
        {
            DataRow dataRow = importRow as DataRow;
            string requestedURL = dataRow[RequestedURL].ToString();
            ImportReporter.Write(newItem, Level.Info, "","", "Item Added/Updated", requestedURL);
            //string preprocessorMessages = dataRow[DOMParserMessages].ToString();
            //foreach (string message in preprocessorMessages.Split(new char[] { ',' }).ToList().Where(s => !string.IsNullOrWhiteSpace(s) && !string.IsNullOrWhiteSpace(s)))
            //{
            //    ImportReporter.Write(newItem, Level.Warning,message, "HTML Source", "DOM Parsing", requestedURL);
            //}

            foreach (var field in MappingFields)
            {
                RunProcessor(field, newItem);
            }
        }

        public override CustomItemBase GetNewItemTemplate(object importRow)
        {
            DataRow dataRow = importRow as DataRow;
            string templateID = dataRow[ActionColumn].ToString();

            if (!string.IsNullOrEmpty(templateID))
            {

                BranchItem templateItem = SitecoreDB.GetItem(templateID);

                return (CustomItemBase)templateItem;
            }
            else
            {
                return NewItemTemplate;

            }
        }

        protected override Item GetParentNode(object importRow, string newItemName)
        {
            Item parentItem = null;
            DataRow dataRow = importRow as DataRow;
            string parentPath = dataRow[PathColumn].ToString();
            string itemName = dataRow[ItemNameColumn].ToString();
            int lastIndexOfPath = parentPath.LastIndexOf('/');

            try {
                parentPath = parentPath.Remove(lastIndexOfPath);
            }
            catch (Exception ex)
            {
                
            }


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



        /// <summary>
        /// Use this function to do any end of process custom reports 
        /// </summary>
        public override void ImportEndReport()
        {
            RunPostProcessors();
            ImportReporter.Print();
        }

        private NameValueCollection DirectoryBuilder(List<string> directories, ImportConfig selectedConfig)
        {
            NameValueCollection dirs = new NameValueCollection();
            string prevDir = string.Empty;
            string rootPath = selectedConfig.ImportLocation.Paths.FullPath;
            bool root = true;
            foreach (string dir in directories)
            {
                string directroy = Helper.RemoveInvalidChars(Config, dir, root);
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

                root = false;
            }
            return dirs;
        }


        private void BuildData(ImportConfig config, List<string> levels, string url, DataTable dataTable)
        {
            
            Item location = config.ImportLocation;
           
            bool ignoreroot = config.IgnoreRootDirectories;
            NameValueCollection directroies = Config.MaintainHierarchy ? DirectoryBuilder(levels, config) : null;


            //if MaintainHierarchy false then build it again with single 
            if (directroies == null)
            {
                directroies = new NameValueCollection();
                foreach (var l in levels)
                {
                    directroies.Add(l, "");
                }
            }


            //This is to be sure there are no duplicates in dataTable
            foreach (DataRow dr in dataTable.Rows)
            {
                string key = dr[PathColumn].ToString();
                var checkKey = directroies[key];

                if (checkKey != null)
                {
                    directroies.Remove(key);
                }
            }



            string dirPath = string.Empty;
            string prevDir = string.Empty;
            bool root = true;

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
                }
                else
                {
                    name = Helper.RemoveInvalidChars(Config, dir, root);
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
                dr[RequestedURL] = currentDirURL;
                var contentHtml = WebContentRequest(currentDirURL);
                doc.LoadHtml(contentHtml);

                RunPreProcessors(config, doc, dr, currentDirURL);


                foreach (var key in mappings)
                {
                    string toFieldName = mappings[key.ToString()];
                    bool isOverride = toFieldName.StartsWith("!");

                    //This is for if 2 mapping target the same field so the next one to not override the first update, 
                    //becuase each could apply for different URL  
                    wasupdated = IsDataInList(updatedFields, toFieldName);
                    if (!wasupdated || isOverride)
                    {
                        //toFieldName = toFieldName.Replace("!", "");
                        string value = FetchContent(doc, key, mappings, config);
                        dr[toFieldName] = value;
                        updatedFields.Add(toFieldName);
                    }                    
                }                   
                prevDir = dir;
                root = false;
            }
        }

       
        private void RunPreProcessors(ImportConfig config, HtmlDocument doc, DataRow dr, string currentDirURL)
        {

            foreach (var processor in config.PreProcessors)
            {
                string returnValue = Processor.Execute(processor.ProcessItem, doc, currentDirURL, NewItemTemplate.ID.ToString());

                if (!string.IsNullOrEmpty(returnValue))
                {
                    dr[ActionColumn] = returnValue;
                }
            }
        }

        private void RunPostProcessors()
        {
            ImportConfig config = new ImportConfig(ImportItem, SitecoreDB, this.Query);
            config.ImportLocation = this.Parent;

            foreach (var processor in config.PostProcessors)
            {
                Processor.Execute(processor.ProcessItem, config);
            }
        }

        public string FetchContent(HtmlDocument doc, object key, NameValueCollection mappings, ImportConfig storedConfig)
        {
            string rowValue = string.Empty;
            bool textOnly = false;

            try
            {
                int index = mappings[key.ToString()].IndexOf('_');
                string toWhatField = mappings[key.ToString()].Substring(0, index);
                Item mappingItem = MappingFields.Where(f => f.Fields[DataImporter.HtmlScraper.Constants.FieldNames.FromWhatField].Value == key.ToString()
                    && f.Fields[DataImporter.HtmlScraper.Constants.FieldNames.ToWhatField].Value == toWhatField).FirstOrDefault();

                if (mappingItem != null)
                {
                    textOnly = mappingItem.Fields[DataImporter.HtmlScraper.Constants.FieldNames.ImportTextOnly].Value == "1" ? true : storedConfig.ImportTextOnly;
                }
            }
            catch { }

            
            List<string> updatedIndexedFields = new List<string>();
            string htmlObj = string.Empty;
            bool isAttrValue = false;
            string attrName = string.Empty;
            string fieldname = string.Empty;
            HtmlNode node = null;

            htmlObj = key.ToString();
            fieldname = mappings[key.ToString()];

            if (htmlObj.Contains("/@"))
            {
                isAttrValue = true;
                List<string> attrItems = htmlObj.Split('/').Where(a => a.StartsWith("@")).ToList();
                attrName = attrItems.Where(a => !a.Contains("=")).FirstOrDefault();
                htmlObj = htmlObj.Replace("/" + attrName, "");
                attrName = attrName.Replace("@", "");
            }

            node = Helper.HandleNodesLookup(htmlObj, doc);

            if (node != null)
            {
                if (isAttrValue && node.Attributes[attrName] != null)
                {
                    rowValue = node.Attributes[attrName].Value;
                }
                else {
                    rowValue = textOnly ? node.InnerText : node.InnerHtml;
                }
            }

            return rowValue;
        }


        private NameValueCollection GetMappings()
        {
            NameValueCollection mappings = new NameValueCollection();
            Item fieldDefinitions = GetItemByTemplate(ImportItem, FieldsFolderID);
            ChildList fields = fieldDefinitions.GetChildren();
            MappingFields = new List<Item>();
            foreach (Item field in fields)
            {
                MappingFields.Add(field);
                BaseMapping baseMap = new BaseMapping(field);
                string fromFieldName = baseMap.OldItemField;
                string toFieldName = baseMap.NewItemField + "_" + Guid.NewGuid().ToString().Replace("-", ""); 
                mappings.Add(fromFieldName, toFieldName);
            }
            return mappings;
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