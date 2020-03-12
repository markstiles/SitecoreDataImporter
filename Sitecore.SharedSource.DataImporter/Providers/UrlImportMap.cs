using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using Sitecore.Data;
using Sitecore.Data.Items;
using System.Data;
using HtmlAgilityPack;
using Sitecore.SharedSource.DataImporter.Mappings;
using Sitecore.Data.Fields;
using Sitecore.SharedSource.DataImporter.Processors.Field;
using Sitecore.SharedSource.DataImporter.Logger;
using Sitecore.SharedSource.DataImporter.Mappings.Fields;
using Sitecore.SharedSource.DataImporter.Mappings.Properties;
using Sitecore.SharedSource.DataImporter.Services;
using Sitecore.SharedSource.DataImporter.Processors.Models;
using System.Xml.Linq;

namespace Sitecore.SharedSource.DataImporter.Providers
{
    public class UrlImportMap : BaseDataMap
    {
        protected Item ImportItem = null;
        protected string ItemNameColumn = "ItemName";
        protected string PathColumn = "Path";
        protected string ActionColumn = "ActionColumn";
        protected string RequestedURL = "RequestedURL";
        protected string FromWhatField = "From What Fields";

        protected string Name { get; set; }
        protected bool ImportTextOnly { get; set; }
        protected List<string> AllowedExtensions { get; set; }
        protected List<PreProcessor> PreProcessors { get; set; }
        protected List<PostProcessors> PostProcessors { get; set; }
        protected string URLCount { get; set; }
        protected Item SelectedMapping { get; set; }
        protected List<string> StoredURLs { get; set; }

        public List<ItemNameCleanup> ItemNameCleanups { get; set; }
        public string BaseUrl { get; set; }
        public List<string> ExcludeDirectories { get; set; }
        public bool MaintainHierarchy { get; set; }
        public bool IgnoreRootDirectories { get; set; }
        public Item ImportLocation { get; set; }

        protected NameValueCollection mappings;
        protected ProcessorService ProcessorService;
        protected HtmlService HtmlService;

        public UrlImportMap(Database db, string ConnectionString, Item importItem, ILogger l)
            : base(db, ConnectionString, importItem, l)
        {
            ImportItem = importItem;
            ProcessorService = new ProcessorService(l);
            HtmlService = new HtmlService(l);
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
            
            AllowedExtensions = new List<string>();
            ExcludeDirectories = new List<string>();
            Name = ImportItem.Name;
            int count = 0;
            StoredURLs = new List<string>();
            IgnoreRootDirectories = ImportItem.Fields["Ignore Root Directories"].Value == "1" ? true : false;
            MaintainHierarchy = ImportItem.Fields["Maintain Hierarchy"].Value == "1" ? true : false;
            ImportTextOnly = ImportItem.Fields["Import Text Only"].Value == "1" ? true : false;
            AllowedExtensions = ImportItem.Fields["Allowed URL Extensions"].Value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            ItemNameCleanups = GetItemNameCleanups(ImportItem, ToDB);
            PreProcessors = GetPreProcessors(ImportItem, ToDB);
            PostProcessors = GetPostProcessors(ImportItem, ToDB);
            BaseUrl = ImportItem.Fields["Base URL"].Value;
            URLCount = ImportItem.Fields["Top x URLs"].Value;
            ExcludeDirectories = ImportItem.Fields["Exclude Directories"].Value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            SelectedMapping = ImportItem;
            string storedValues = !string.IsNullOrEmpty(Query) ? Query : ImportItem.Fields["Query"].Value;
            storedValues.Split(new[] { '\r' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            List<string> urls = storedValues.Split(new[] { '\n' }).ToList();
            urls = urls.Where(u => !u.StartsWith("//")).ToList();
            List<string> sitemapURLs = urls.Where(u => u.Trim().EndsWith(".xml")).ToList();
            List<string> pageURLs = urls.Where(u => !u.Trim().EndsWith(".xml")).ToList();

            if (sitemapURLs.Any())
                ImportFromSitemap(sitemapURLs, StoredURLs);
            
            if (pageURLs.Any())
                StoredURLs.AddRange(pageURLs);
            
            StoredURLs = StoredURLs.Select(u => u.TrimEnd('\r', '\n').ToLower()).ToList();

            if (AllowedExtensions != null && AllowedExtensions.Any())
            {
                StoredURLs = StoredURLs.Where(x => AllowedExtensions.Any(e => x.EndsWith(("." + e.Trim())))).ToList();

                if (pageURLs.Any(p => p.Contains("?")))
                {
                    var queryPages = pageURLs;
                    queryPages = queryPages.Select(u => u.TrimEnd('\r', '\n').ToLower()).ToList();
                    queryPages = queryPages.Where(x => AllowedExtensions.Any(e => x.Contains(("." + e.Trim() + "?")))).ToList();
                    StoredURLs.AddRange(queryPages);
                }
            }

            if (int.TryParse(URLCount, out count))
                StoredURLs = StoredURLs.Take(count).ToList();
            
            ImportLocation = ImportToWhere;

            if (ItemNameFields.FirstOrDefault() == "[URL]")
                ItemNameFields[0] = ItemNameColumn;
            
            //Adding columns to the table from field mapping
            foreach (var key in mappings)
            {
                string toField = mappings[key.ToString()];
                dt.Columns.Add(toField);
            }
            
            foreach (var url in StoredURLs)
            {
                string relativeURL = url.Replace("http://", "").Replace("https://", "");
                Char[] splitChars = { '/' };

                //parts are the directory list in array format, ie. second array is the child of first etc..
                List<string> levels = IgnoreRootDirectories ? relativeURL.Split(splitChars, StringSplitOptions.RemoveEmptyEntries).Skip(1).ToList() :
                relativeURL.Split(splitChars, StringSplitOptions.RemoveEmptyEntries).ToList();

                if (ExcludeDirectories != null && ExcludeDirectories.Any())
                    levels.RemoveAll(x => ExcludeDirectories.Contains(x.ToLower()));
                
                BuildData(levels, url, dt);
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
            Logger.Log("Item Added/Updated", newItem.Paths.FullPath, ProcessStatus.Info, "", requestedURL);
            
            foreach (var field in FieldDefinitions)
            {
                MultilistField processors = field.InnerItem.Fields["Field Post Processors"];
                foreach (var targetId in processors.TargetIDs)
                {
                    Item processor = ToDB.GetItem(targetId);
                    if (processor == null) { continue; }

                    ProcessorService.ExecuteFieldProcessor(this, processor, newItem, field.InnerItem);
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


            if (MaintainHierarchy)
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
                string fromFieldName = baseMap.InnerItem.Fields[FromWhatField].Value;
                string toFieldName = baseMap.NewItemField + "_" + Guid.NewGuid().ToString().Replace("-", "");
                mappings.Add(fromFieldName, toFieldName);
            }

            return mappings;
        }

        /// <summary>
        /// Use this function to do any end of process custom reports 
        /// </summary>
        public virtual NameValueCollection DirectoryBuilder(List<string> directories)
        {
            NameValueCollection dirs = new NameValueCollection();
            string prevDir = string.Empty;
            string rootPath = ImportLocation.Paths.FullPath;
            bool root = true;
            foreach (string dir in directories)
            {
                string directroy = HtmlService.RemoveInvalidChars(ItemNameCleanups, dir, root);
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

        public virtual void BuildData(List<string> levels, string url, DataTable dataTable)
        {
            Item location = ImportLocation;
            bool ignoreroot = IgnoreRootDirectories;
            NameValueCollection directories = MaintainHierarchy ? DirectoryBuilder(levels) : null;
            
            //if MaintainHierarchy false then build it again with single 
            if (directories == null)
            {
                directories = new NameValueCollection();
                foreach (var l in levels)
                {
                    directories.Add(l, "");
                }
            }
            
            //This is to be sure there are no duplicates in dataTable
            foreach (DataRow dr in dataTable.Rows)
            {
                string key = dr[PathColumn].ToString();
                var checkKey = directories[key];
                if (checkKey == null)
                    continue;
                
                directories.Remove(key);
            }

            string dirPath = string.Empty;
            bool root = true;

            foreach (string dir in directories)
            {
                List<string> updatedFields = new List<string>();
                bool wasupdated = false;
                DataRow dr = dataTable.NewRow();
                dataTable.Rows.Add(dr);

                string name = string.Empty;
                string urlVal = directories[dir];

                if (MaintainHierarchy)
                {
                    dirPath = dir;
                    name = dir.Split('/').Last();
                }
                else
                {
                    name = HtmlService.RemoveInvalidChars(ItemNameCleanups, dir, root);
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

                RunPreProcessors(doc, dr, currentDirURL);
                
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
                    string value = FetchContent(doc, key, mappings);
                    dr[toFieldName] = value;
                    updatedFields.Add(toFieldName);
                }
                root = false;
            }
        }

        public virtual void RunPreProcessors(HtmlDocument doc, DataRow dr, string currentDirURL)
        {
            foreach (var processor in PreProcessors)
            {
                string returnValue = ProcessorService.ExecutePreProcessor(this, processor.ProcessItem, doc, currentDirURL, ImportToWhatTemplate.ID.ToString());

                if (string.IsNullOrEmpty(returnValue))
                    continue;
                
                dr[ActionColumn] = returnValue;
            }
        }

        public virtual string FetchContent(HtmlDocument doc, object key, NameValueCollection mappings)
        {
            string rowValue = string.Empty;
            bool textOnly = false;
            bool useXPath = false;
            try
            {
                int index = mappings[key.ToString()].IndexOf('_');
                string toWhatField = mappings[key.ToString()].Substring(0, index);
                IBaseField mappingItem = FieldDefinitions.FirstOrDefault(f => 
                        f.InnerItem.Fields[FromWhatField].Value == key.ToString()
                        && f.InnerItem.Fields["To What Field"].Value == toWhatField);

                useXPath = mappingItem.InnerItem.Fields["Use XPath For From"].Value == "1";

                textOnly = mappingItem.InnerItem.Fields["Strip HTML"]?.Value == "1" 
                           || ImportTextOnly;
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
                htmlObj = htmlObj.Replace($"/{attrName}", "");
                attrName = attrName.Replace("@", "");
            }

            node = useXPath 
                ? HtmlService.HandleNodesLookup(htmlObj, doc, true) 
                : HtmlService.HandleNodesLookup(htmlObj, doc);

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
        
        public virtual List<PostProcessors> GetPostProcessors(Item config, Database db)
        {
            List<PostProcessors> processors = new List<PostProcessors>();
            MultilistField processorItems = config.Fields["Post Processors"];
            foreach (var id in processorItems.TargetIDs)
            {
                var item = db.GetItem(id);
                PostProcessors processor = new PostProcessors();
                processor.Identifier = item.Fields["Identifier"] != null ? item.Fields["Identifier"].Value : string.Empty;
                processor.Action = item.Fields["Action"] != null ? item.Fields["Action"].Value : string.Empty;
                processor.Type = item.Fields["Type"] != null ? item.Fields["Type"].Value : string.Empty;
                processor.Method = item.Fields["Method"] != null ? item.Fields["Method"].Value : string.Empty;
                processor.ProcessItem = item;
                processors.Add(processor);
            }
            return processors;
        }

        public virtual void ImportFromSitemap(List<string> sitemapURLs, List<string> storedList)
        {
            foreach (var s in sitemapURLs)
            {

                XDocument xdoc = XDocument.Load(s);
                var ns = xdoc.Root.Name.Namespace;
                List<XElement> xLocs = xdoc.Root.Elements(ns + "url").Elements(ns + "loc").ToList();
                List<string> urls = xLocs.Select(u => u.Value).ToList();


                storedList.AddRange(urls);
            }
        }

        public virtual List<ItemNameCleanup> GetItemNameCleanups(Item config, Database db)
        {
            List<ItemNameCleanup> cleanups = new List<ItemNameCleanup>();
            MultilistField cleanupItems = config.Fields["Item Name Cleanups"];
            foreach (var id in cleanupItems.TargetIDs)
            {
                var cleanupItem = db.GetItem(id);
                ItemNameCleanup cleanup = new ItemNameCleanup();
                cleanup.Find = cleanupItem.Fields["Find"].Value;
                cleanup.Replace = cleanupItem.Fields["Replace"].Value;
                cleanup.CleanupItem = cleanupItem;
                cleanups.Add(cleanup);
            }
            return cleanups;
        }

        public virtual List<PreProcessor> GetPreProcessors(Item config, Database db)
        {
            List<PreProcessor> processors = new List<PreProcessor>();
            MultilistField processorItems = config.Fields["Pre Processors"];
            foreach (var id in processorItems.TargetIDs)
            {
                var item = db.GetItem(id);
                PreProcessor processor = new PreProcessor();
                processor.Identifier = item.Fields["Identifier"] != null ? item.Fields["Identifier"].Value : string.Empty;
                processor.Action = item.Fields["Action"] != null ? item.Fields["Action"].Value : string.Empty;
                processor.Type = item.Fields["Type"] != null ? item.Fields["Type"].Value : string.Empty;
                processor.Method = item.Fields["Method"] != null ? item.Fields["Method"].Value : string.Empty;
                processor.ProcessItem = item;
                processors.Add(processor);
            }
            return processors;
        }
    }
}