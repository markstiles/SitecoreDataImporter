using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.SharedSource.DataImporter.HtmlScraper;
using System.Data;
using HtmlAgilityPack;
using Sitecore.SharedSource.DataImporter.Mappings;
using Sitecore.Data.Fields;
using Sitecore.SharedSource.DataImporter.Processors;
using Sitecore.SharedSource.DataImporter.Reporting;
using Sitecore.SharedSource.DataImporter.Logger;
using Sitecore.SharedSource.DataImporter.Mappings.Fields;
using Sitecore.SharedSource.DataImporter.Mappings.Properties;

namespace Sitecore.SharedSource.DataImporter.Providers
{
    public class UrlImportMap : BaseDataMap
    {
        public ImportConfig Config { get; set; }
        private Item ImportItem = null;
        private string ItemNameColumn = "ItemName";
        private string PathColumn = "Path";
        private string ActionColumn = "ActionColumn";
        private string RequestedURL = "RequestedURL";
        private NameValueCollection mappings;
        
        public UrlImportMap(Database db, string ConnectionString, Item importItem, ILogger l)
            : base(db, ConnectionString, importItem, l)
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

            ImportConfig config = new ImportConfig(ImportItem, ToDB, this.Query);
            config.ImportLocation = ImportToWhere;
            Config = config;
            
            if (ItemNameFields.FirstOrDefault() == "[URL]")
                ItemNameFields[0] = ItemNameColumn;
            
            //Adding columns to the table from field mapping
            foreach (var key in mappings)
            {
                string toField = mappings[key.ToString()];
                dt.Columns.Add(toField);
            }

            List<string> urls = config.StoredURLs;

            foreach (var url in urls)
            {
                string relativeURL = url.Replace("http://", "").Replace("https://", "");
                Char[] splitChars = { '/' };

                //parts are the directory list in array format, ie. second array is the child of first etc..
                List<string> levels = config.IgnoreRootDirectories ? relativeURL.Split(splitChars, StringSplitOptions.RemoveEmptyEntries).Skip(1).ToList() :
                relativeURL.Split(splitChars, StringSplitOptions.RemoveEmptyEntries).ToList();

                if (config.ExcludeDirectories != null && config.ExcludeDirectories.Any())
                    levels.RemoveAll(x => config.ExcludeDirectories.Contains(x.ToLower()));
                
                BuildData(config, levels, url, dt);
            }
            
            return (dt.Rows).Cast<object>();
        }
                
        /// <summary>
        /// There is no custom data for this type
        /// </summary>
        /// <param name="newItem"></param>
        /// <param name="importRow"></param>
        public override void ProcessCustomData(ref Item newItem, object importRow, List<IBaseProperty> propDefinitions = null)
        {
            DataRow dataRow = importRow as DataRow;
            string requestedURL = dataRow[RequestedURL].ToString();
            ImportReporter.Write(newItem, Level.Info, "", "", "Item Added/Updated", requestedURL);
            
            foreach (var field in FieldDefinitions)
            {
                //Check for warnings
                MultilistField WarningTags = field.InnerItem.Fields[DataImporter.HtmlScraper.Constants.FieldNames.WarningTriggerTags];
                if (WarningTags.Count > 0)
                    WriteTagWarnings.Run(newItem, field.InnerItem, Logger);

                //"To What Field"
                MultilistField processors = field.InnerItem.Fields[DataImporter.HtmlScraper.Constants.FieldNames.FieldPostProcessors];

                foreach (var targetId in processors.TargetIDs)
                {
                    Item processor = ToDB.GetItem(targetId);
                    if (processor == null) { continue; }

                    Processor.Execute(processor, newItem, field.InnerItem);
                }
            }
        }
        
        public override CustomItemBase GetNewItemTemplate(object importRow)
        {
            DataRow dataRow = importRow as DataRow;
            string templateID = dataRow[ActionColumn].ToString();
            if (string.IsNullOrEmpty(templateID))
                return ImportToWhatTemplate;

            BranchItem templateItem = ToDB.GetItem(templateID);

            return templateItem;
        }

        public override Item GetParentNode(object importRow, string newItemName)
        {
            Item parentItem = null;
            DataRow dataRow = importRow as DataRow;
            string parentPath = dataRow[PathColumn].ToString();
            int lastIndexOfPath = parentPath.LastIndexOf('/');

            try
            {
                parentPath = parentPath.Remove(lastIndexOfPath);
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString(), parentPath, ProcessStatus.NewItemError, PathColumn, parentPath);
            }


            if (Config.MaintainHierarchy)
                parentItem = ToDB.GetItem(parentPath);
            
            if (parentItem == null)
                parentItem = base.GetParentNode(importRow, newItemName);
            
            return parentItem;
        }

        /// <summary>
        /// gets a field value from an item
        /// </summary>
        /// <param name="importRow"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public override string GetFieldValue(object importRow, string fieldName)
        {
            string toFieldName = fieldName;
            if (fieldName != ItemNameColumn)
                toFieldName = mappings[fieldName];
            
            DataRow item = importRow as DataRow;
            object f = item[toFieldName];

            return f != null ? f.ToString() : string.Empty;
        }

        #endregion Override Methods
        
        protected NameValueCollection GetMappings()
        {
            NameValueCollection mappings = new NameValueCollection();
            foreach (IBaseField field in FieldDefinitions)
            {
                BaseMapping baseMap = new BaseMapping(field.InnerItem, Logger);
                string fromFieldName = baseMap.InnerItem.Fields[DataImporter.HtmlScraper.Constants.FieldNames.FromWhatField].Value;
                string toFieldName = baseMap.NewItemField + "_" + Guid.NewGuid().ToString().Replace("-", "");
                mappings.Add(fromFieldName, toFieldName);
            }

            return mappings;
        }

        /// <summary>
        /// Use this function to do any end of process custom reports 
        /// </summary>
        public virtual NameValueCollection DirectoryBuilder(List<string> directories, ImportConfig selectedConfig)
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

        public virtual void BuildData(ImportConfig config, List<string> levels, string url, DataTable dataTable)
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
                if (checkKey == null)
                    continue;
                
                directroies.Remove(key);
            }

            string dirPath = string.Empty;
            bool root = true;

            foreach (string dir in directroies)
            {
                List<string> updatedFields = new List<string>();
                bool wasupdated = false;
                DataRow dr = dataTable.NewRow();
                dataTable.Rows.Add(dr);

                string name = string.Empty;
                string urlVal = directroies[dir];

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
                dr[ItemNameColumn] = name;

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
                    wasupdated = updatedFields.Any(f => toFieldName.ToLower() == f.ToLower());
                    if (wasupdated && !isOverride)
                        continue;
                    
                    //toFieldName = toFieldName.Replace("!", "");
                    string value = FetchContent(doc, key, mappings, config);
                    dr[toFieldName] = value;
                    updatedFields.Add(toFieldName);
                }
                root = false;
            }
        }

        public virtual void RunPreProcessors(ImportConfig config, HtmlDocument doc, DataRow dr, string currentDirURL)
        {
            foreach (var processor in config.PreProcessors)
            {
                string returnValue = Processor.Execute(processor.ProcessItem, doc, currentDirURL, ImportToWhatTemplate.ID.ToString());

                if (string.IsNullOrEmpty(returnValue))
                    continue;
                
                dr[ActionColumn] = returnValue;
            }
        }

        public virtual string FetchContent(HtmlDocument doc, object key, NameValueCollection mappings, ImportConfig storedConfig)
        {
            string rowValue = string.Empty;
            bool textOnly = false;
            bool useXPath = false;
            try
            {
                int index = mappings[key.ToString()].IndexOf('_');
                string toWhatField = mappings[key.ToString()].Substring(0, index);
                IBaseField mappingItem = FieldDefinitions.FirstOrDefault(f => 
                        f.InnerItem.Fields[HtmlScraper.Constants.FieldNames.FromWhatField].Value == key.ToString()
                        && f.InnerItem.Fields[HtmlScraper.Constants.FieldNames.ToWhatField].Value == toWhatField);

                useXPath = mappingItem.InnerItem.Fields[HtmlScraper.Constants.FieldNames.UseXpath].Value == "1";

                textOnly = mappingItem.InnerItem.Fields[HtmlScraper.Constants.FieldNames.ImportTextOnly].Value == "1" 
                           || storedConfig.ImportTextOnly;
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString(), rowValue, ProcessStatus.GetImportDataError, key.ToString(), rowValue);
            }

            string htmlObj = string.Empty;
            bool isAttrValue = false;
            string attrName = string.Empty;
            HtmlNode node = null;

            htmlObj = key.ToString();

            if (htmlObj.Contains("/@"))
            {
                isAttrValue = true;
                List<string> attrItems = htmlObj.Split('/').Where(a => a.StartsWith("@")).ToList();
                attrName = attrItems.FirstOrDefault(a => !a.Contains("="));
                htmlObj = htmlObj.Replace("/" + attrName, "");
                attrName = attrName.Replace("@", "");
            }

            node = useXPath 
                ? Helper.HandleNodesLookup(htmlObj, doc, true) 
                : Helper.HandleNodesLookup(htmlObj, doc);

            if (node == null)
                return rowValue;

            rowValue = isAttrValue && node.Attributes[attrName] != null
                ? node.Attributes[attrName].Value
                : (textOnly ? node.InnerText : node.InnerHtml);

            return rowValue;
        }
        
        public virtual string WebContentRequest(string url)
        {
            string content = string.Empty;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            HttpWebResponse response = null;
            StreamReader reader = null;

            try
            {
                response = (HttpWebResponse) request.GetResponse();
                if (response.StatusCode != HttpStatusCode.OK)
                    return content;

                reader = new StreamReader(response.GetResponseStream());
                content = reader.ReadToEnd();
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString(), url, ProcessStatus.NewItemError, "", url);
            }
            finally
            {
                if(response != null)
                    response.Close();

                if(reader != null)
                    reader.Close();
            }

            return content;
        }
    }
}