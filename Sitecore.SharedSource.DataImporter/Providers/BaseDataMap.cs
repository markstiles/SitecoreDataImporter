using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.Data.Items;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.SharedSource.DataImporter.Mappings.Fields;
using Sitecore.SharedSource.DataImporter.Extensions;
using Sitecore.SharedSource.DataImporter.Mappings;
using System.Globalization;
using Sitecore.SharedSource.DataImporter.Mappings.Properties;
using System.Data;
using System.Collections;
using System.Web;
using Sitecore.SharedSource.DataImporter.Utility;
using Sitecore.Collections;
using System.IO;
using Sitecore.Globalization;
using Sitecore.Data.Managers;
using Sitecore.Configuration;
using Sitecore.SharedSource.DataImporter.Logger;
using Sitecore.SharedSource.DataImporter.Mappings.Processors;

namespace Sitecore.SharedSource.DataImporter.Providers {
    /// <summary>
    /// The BaseDataMap is the base class for any data provider. It manages values stored in sitecore 
    /// and does the bulk of the work processing the fields
    /// </summary>
    public abstract class BaseDataMap : IDataMap {

        #region Static IDs

        public static readonly string FieldsFolderTemplateID = "{98EF4356-8BFE-4F6A-A697-ADFD0AAD0B65}";

        public static readonly string PostProcessorFolderTemplateID = "{BE3A36A8-9589-401A-8549-D0E06FDB4F13}";

        public static readonly string PostProcessorTemplateID = "{4CDC6570-140F-426B-85B7-5DE0C67B31FA}";

        public static readonly string CommonFolderTemplateID = "{A87A00B1-E6DB-45AB-8B54-636FEC3B5523}";

        public static readonly string ReferenceFieldsFolderTemplateID = "{045308E2-BD25-4B5D-B5C8-49EC6CD173C3}";

        #endregion Static IDs

        #region Properties

        public Item ImportItem { get; set; }
		public string Name { get; set; }

        #endregion Properties

        #region IDataMap Properties

        public ILogger Logger { get; set; }

        /// <summary>
        /// the reference to the sitecore database that you'll import into
        /// </summary>
        public Database ToDB { get; set; }

        public int ItemNameMaxLength { get; set; }

        /// <summary>
        /// the definitions of fields to import
        /// </summary>
        public List<IBaseField> FieldDefinitions { get; set; }

        public List<IPostProcessor> PostProcessors { get; set; }

        /// <summary>
        /// the connection string to the database you're importing from
        /// </summary>
        public string DatabaseConnectionString { get; set; }

        #endregion IDataMap Properties

        #region IDataMap Fields

        /// <summary>
        /// the query used to retrieve the data
        /// </summary>
        public string Query { get; set; }

        /// <summary>
        /// the parent item where the new items will be imported into
        /// </summary>
        public Item ImportToWhere { get; set; }

        /// <summary>
        /// the template to create new items with
        /// </summary>
        public CustomItemBase ImportToWhatTemplate { get; set; }

        public string[] ItemNameFields { get; set; }

        public Language ImportToLanguage { get; set; }
        
        /// <summary>
        /// tells whether or not to folder new items by a date
        /// </summary>
        public bool FolderByDate { get; set; }

        /// <summary>
        /// tells whether or not to folder new items by first letter of their name
        /// </summary>
        public bool FolderByName { get; set; }

        /// <summary>
        /// tells whether or not to folder new items by first letter of their name
        /// </summary>
        public bool FolderByPath { get; set; }

        /// <summary>
        /// the name of the field that stores a date to folder by
        /// </summary>
        public string DateField { get; set; }

        /// <summary>
        /// the template used to create the folder items
        /// </summary>
        public TemplateItem FolderTemplate { get; set; }
        public Item ProviderItem { get; set; }

        #endregion IDataMap Fields

        #region Constructor

        public BaseDataMap(Database db, string connectionString, Item importItem, ILogger l) {
            if (l == null)
                throw new Exception("The provided Logger is null");

            //instantiate log
            Logger = l;

            //setup import details
            ToDB = db;
            DatabaseConnectionString = connectionString;
            ImportItem = importItem;
            ProviderItem = importItem;

            //determine the item name max length
            ItemNameMaxLength = GetNameLength();

            //get query
            Query = ImportItem.GetItemField("Query", Logger);

            //get parent item
            ImportToWhere = GetImportToWhereItem();

            //get new item template
            ImportToWhatTemplate = GetImportToTemplate();

            //get item name field
            ItemNameFields = ImportItem.GetItemField("Pull Item Name from What Fields", Logger).Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

            //get import language
            ImportToLanguage = GetImportItemLanguage("Import To Language");

            //foldering information
            FolderByDate = ImportItem.GetItemBool("Folder By Date");
            FolderByName = ImportItem.GetItemBool("Folder By Name");
            FolderByPath = ImportItem.GetItemBool("Folder By Path");

            //populate field definitions
            FieldDefinitions = GetFieldDefinitions(ImportItem);

            //populate post processors
            PostProcessors = GetPostProcessors(ImportItem);
        }

        #endregion Constructor

        #region Constructor Helpers

        public int GetNameLength() {
            int i = 100;
            return (int.TryParse(Settings.GetSetting("MaxItemNameLength"), out i))
                ? i
                : 100;
        }

        public Item GetImportToWhereItem() {

            Item toWhere = null;

            //check field value
            string toWhereID = ImportItem.GetItemField("Import To Where", Logger);
            if (string.IsNullOrEmpty(toWhereID)) {
                Logger.Log(ImportItem.Paths.FullPath, "the 'To Where' field is not set on the import item", ProcessStatus.ImportDefinitionError, "Import To Where");
                return null;
            }

            //check item
            toWhere = ToDB.Items[toWhereID];
            if (toWhere.IsNull())
                Logger.Log(ImportItem.Paths.FullPath, "the 'To Where' item is null on the import item", ProcessStatus.ImportDefinitionError, "Import To Where", toWhereID);

            return toWhere;
        }

        public CustomItemBase GetImportToTemplate() {

            CustomItemBase template = null;

            //check field value
            string templateID = ImportItem.GetItemField("Import To What Template", Logger);
            if (string.IsNullOrEmpty(templateID)) {
                Logger.Log(ImportItem.Paths.FullPath, "the 'To What Template' field is not set", ProcessStatus.ImportDefinitionError, "Import To What Template");
                return null;
            }

            //check template item
            Item templateItem = ToDB.Items[templateID];
            if (templateItem.IsNull()) {
                Logger.Log(ImportItem.Paths.FullPath, "the 'To What Template' item is null", ProcessStatus.ImportDefinitionError, "Import To What Template");
                return null;
            }

            //determine template item type
            if ((BranchItem)templateItem != null) {
                template = (BranchItem)templateItem;
            } else {
                template = (TemplateItem)templateItem;
            }

            return template;
        }

        public Language GetImportItemLanguage(string fieldName) {

            Language l = LanguageManager.DefaultLanguage;

            //check the field
            string langID = ImportItem.GetItemField(fieldName, Logger);
            if (string.IsNullOrEmpty(langID)) {
                Logger.Log(ImportItem.Paths.FullPath, "The 'Import Language' field is not set on the import item", ProcessStatus.ImportDefinitionError, fieldName);
                return l;
            }

            //check item
            Item iLang = ToDB.GetItem(langID);
            if (iLang.IsNull()) {
                Logger.Log(ImportItem.Paths.FullPath, "The 'Import Language' Item is null on the import item", ProcessStatus.ImportDefinitionError, fieldName);
                return l;
            }

            //check language
            l = LanguageManager.GetLanguage(iLang.Name);
            if (l == null) {
                Logger.Log(ImportItem.Paths.FullPath, "The 'Import Language' name is not valid on the import item", ProcessStatus.ImportDefinitionError, fieldName);
            }

            return l;
        }

        public TemplateItem GetImportFolderTemplate() {

            if (!FolderByName && !FolderByDate && !FolderByPath)
                return null;

            //setup a default type to an ordinary folder
            TemplateItem defaultTemplate = ToDB.Templates[CommonFolderTemplateID];

            //if they specify a type then use that
            string folderID = ImportItem.GetItemField("Folder Template", Logger);
            if (string.IsNullOrEmpty(folderID))
                return defaultTemplate;

            //check the folder template
            TemplateItem fTemplate = ToDB.Templates[folderID];
            return (fTemplate == null) ? defaultTemplate : fTemplate;
        }

        #endregion Constructor Helpers

        #region Methods

        protected virtual List<IBaseField> GetFieldDefinitions(Item i) {

            List<IBaseField> l = new List<IBaseField>();

            //check for fields folder
            Item Fields = i.GetChildByTemplate(FieldsFolderTemplateID);
            if (Fields.IsNull()) {
                Logger.Log(i.Paths.FullPath, "there is no 'Fields' folder on the import item", ProcessStatus.ImportDefinitionError);
                return l;
            }

            //check for any children
            if (!Fields.HasChildren) {
                Logger.Log(i.Paths.FullPath, "there are no fields to import on  on the import item", ProcessStatus.ImportDefinitionError);
                return l;
            }

            ChildList c = Fields.GetChildren();
            foreach (Item child in c)
			{
				if (child.TemplateID == new ID("{F9FDB469-FC36-4A48-AB45-1B4832AA350E}"))
				{
					var fieldFolder = ((LookupField) child.Fields["Fields"]).TargetItem;
					foreach (Item sharedChild in fieldFolder.GetChildren())
					{
					    //create an item to get the class / assembly name from
					    BaseMapping bm = new BaseMapping(child);
					    var field = GenerateType<IBaseField>(sharedChild, bm.HandlerClass, bm.HandlerAssembly, new object[] { child, Logger });
                        if(field != null)
                            l.Add(field);
					}
				}
				else
				{
				    BaseMapping bm = new BaseMapping(child);
				    var field = GenerateType<IBaseField>(child, bm.HandlerClass, bm.HandlerAssembly, new object[] { child, Logger });
                    if(field != null)
                        l.Add(field);
				}
			}

            return l.Where(x => x != null).ToList();
        }

        protected virtual List<IPostProcessor> GetPostProcessors(Item i)
        {
            List<IPostProcessor> processors = new List<IPostProcessor>();

            //check for fields folder
            Item ProcessorFolder = i.GetChildByTemplate(PostProcessorFolderTemplateID);
            if (ProcessorFolder.IsNull())
            {
                Logger.Log(i.Paths.FullPath, "there is no 'Post Processors' folder on the import item", ProcessStatus.ImportDefinitionError);
                return processors;
            }

            //check for any children
            if (!ProcessorFolder.HasChildren)
            {
                Logger.Log(i.Paths.FullPath, "there are no post processors to run after the import", ProcessStatus.ImportDefinitionError);
                return processors;
            }

            List<Item> childItems = ProcessorFolder
                .GetChildren()
                .Where(c => c.TemplateID.Guid == new Guid(PostProcessorTemplateID))
                .ToList();
            foreach (Item child in childItems)
            {
                var handlerClass = child.GetItemField("Processor Class", Logger);
                var handlerAssembly = child.GetItemField("Processor Assembly", Logger);
                var processor = GenerateType<IPostProcessor>(child, handlerClass, handlerAssembly, new object[] { });
                if(processor != null)
                    processors.Add(processor);
            }

            return processors.Where(x => x != null).ToList();
        }
        
        public T GenerateType<T>(Item child, string handlerClass, string handlerAssembly, object[] instantiationParameters)
        {
            //check for assembly
            if (string.IsNullOrEmpty(handlerAssembly))
            {
                Logger.Log("BaseDataMap.GenerateFieldMapping", string.Format("the field's Handler Assembly is not defined on item {0}: {1}", child.Paths.FullPath, handlerAssembly));
                return default(T);
            }

            //check for class
            if (string.IsNullOrEmpty(handlerClass))
            {
                Logger.Log("BaseDataMap.GenerateFieldMapping", string.Format("the field's Handler Class is not defined on item {0}: {1}", child.Paths.FullPath, handlerClass));
                return default(T);
            }

            //create the object from the class and cast as base field to add it to field definitions
            try
            {
                T typeObj = (T)Sitecore.Reflection.ReflectionUtil.CreateObject(handlerAssembly, handlerClass, instantiationParameters);
                if (typeObj != null)
                    return typeObj;
            }
            catch (FileNotFoundException)
            {
                Logger.Log("BaseDataMap.GenerateFieldMapping", string.Format("the field's binary specified could not be found on item {0} : {1}", child.Paths.FullPath, handlerAssembly));
            }

            Logger.Log("BaseDataMap.GenerateFieldMapping", string.Format("the field's class type could not be instantiated {0} :{1}", child.Paths.FullPath, handlerClass));

            return default(T);
        }

        /// <summary>
        /// retrieves all the import field values specified
        /// </summary>
        protected virtual IEnumerable<string> GetFieldValues(IEnumerable<string> fieldNames, object importRow) {
            List<string> list = new List<string>();
            foreach (string f in fieldNames) {
                try {
                    list.Add(GetFieldValue(importRow, f));
                } catch (ArgumentException ex) {
                    Logger.Log("N/A", (string.IsNullOrEmpty(f))
                        ? "the 'From' field name is empty"
                        : "the field doesn't exist in the import row", 
                        ProcessStatus.FieldError, f);
                }
            }
            return list;
        }

        /// <summary>
        /// will begin looking for or creating date folders to get a parent node to create the new items in
        /// </summary>
        /// <param name="parentNode">current parent node to create or search folder under</param>
        /// <param name="dt">date time value to folder by</param>
        /// <param name="folderType">folder template type</param>
        /// <returns></returns>
        protected Item GetDateParentNode(Item parentNode, DateTime dt, TemplateItem folderType) {
            //get year folder
            Item year = GetChild(parentNode, dt.Year.ToString());
            if (year == null) {
                //build year folder if you have to
                year = parentNode.Add(dt.Year.ToString(), folderType);
            }
            //set the parent to year
            parentNode = year;

            //get month folder
            Item month = GetChild(parentNode, dt.ToString("MM"));
            if (month == null) {
                //build month folder if you have to
                month = parentNode.Add(dt.ToString("MM"), folderType);
            }
            //set the parent to year
            parentNode = month;

            //get day folder
            Item day = GetChild(parentNode, dt.ToString("dd"));
            if (day == null) {
                //build day folder if you have to
                day = parentNode.Add(dt.ToString("dd"), folderType);
            }
            //set the parent to year
            parentNode = day;

            return parentNode;
        }

        /// <summary>
        /// will begin looking for or creating letter folders to get a parent node to create the new items in
        /// </summary>
        /// <param name="parentNode">current parent node to create or search folder under</param>
        /// <param name="letter">the letter to folder by</param>
        /// <param name="folderType">folder template type</param>
        /// <returns></returns>
        protected Item GetNameParentNode(Item parentNode, string letter, TemplateItem folderType) {
            //get letter folder
            Item letterItem = GetChild(parentNode, letter);
            if (letterItem == null) //build year folder if you have to
                letterItem = parentNode.Add(letter, folderType);

            //set the parent to year
            return letterItem;
        }

        public static Item GetChild(Item i, string childName)
        {
            string childPath = string.Format("{0}/{1}", i.Paths.FullPath, childName);
            return i.Database.GetItem(childPath);
        }

        #endregion Methods

        #region IDataMap Methods

        /// <summary>
        /// gets the data to be imported
        /// </summary>
        /// <returns></returns>
        public abstract IEnumerable<object> GetImportData();
        
        /// <summary>
        /// this is used to process custom fields or properties
        /// </summary>
        public abstract void ProcessCustomData(ref Item newItem, object importRow);

        /// <summary>
        /// Defines how the subclass will retrieve a field value
        /// </summary>
        public abstract string GetFieldValue(object importRow, string fieldName);

        public virtual CustomItemBase GetNewItemTemplate(object importRow) {
            //Create new item
            if (ImportToWhatTemplate == null || ImportToWhatTemplate.InnerItem.IsNull())
                throw new NullReferenceException("The 'Import To What Template' item is null");
            return ImportToWhatTemplate;
        }

        public virtual List<IBaseField> GetFieldDefinitionsByRow(object importRow) {
            return FieldDefinitions;
        }

        public string BuildNewItemName(object importRow) {
            if (!ItemNameFields.Any())
                throw new NullReferenceException("there are no 'Name' fields specified");

            StringBuilder strItemName = new StringBuilder();
            foreach (string nameField in ItemNameFields) {
                try {
                    strItemName.Append(GetFieldValue(importRow, nameField));
                } catch (ArgumentException ex) {
                    throw new NullReferenceException(string.Format("the field name: '{0}' does not exist in the import row", nameField));
                }
            }

            string nameValue = strItemName.ToString();
            if (string.IsNullOrEmpty(nameValue))
                throw new NullReferenceException(string.Format("the name fields: '{0}' are empty in the import row", string.Join(",", ItemNameFields)));
            return StringUtility.GetValidItemName(strItemName.ToString().Trim(), this.ItemNameMaxLength).Trim();
        }

        public virtual Item CreateNewItem(Item parent, object importRow, string newItemName) {

            CustomItemBase nItemTemplate = GetNewItemTemplate(importRow);

            using (new LanguageSwitcher(ImportToLanguage)) {
                //get the parent in the specific language
                parent = ToDB.GetItem(parent.ID);

                Item newItem;
                //search for the child by name
                newItem = GetChild(parent, newItemName);
                if (newItem != null) //add version for lang
                    newItem = newItem.Versions.AddVersion();

                //if not found then create one
                if (newItem == null) {
                    if (nItemTemplate is BranchItem)
                        newItem = parent.Add(newItemName, (BranchItem)nItemTemplate);
                    else
                        newItem = parent.Add(newItemName, (TemplateItem)nItemTemplate);
                }

                if (newItem == null)
                    throw new NullReferenceException("the new item created was null");

                using (new EditContext(newItem, true, false)) {
                    //add in the field mappings
                    List<IBaseField> fieldDefs = GetFieldDefinitionsByRow(importRow);
                    foreach (IBaseField d in fieldDefs)
                    {
                        string importValue = string.Empty;
                        try {
                            IEnumerable<string> values = GetFieldValues(d.GetExistingFieldNames(), importRow);
                            importValue = String.Join(d.GetFieldValueDelimiter(), values);
                            d.FillField(this, ref newItem, importValue);
                        } catch (Exception ex) {
                            Logger.Log(newItem.Paths.FullPath, "the FillField failed", ProcessStatus.FieldError, d.ItemName(), importValue);
                        }
                    }

                    //calls the subclass method to handle custom fields and properties
                    ProcessCustomData(ref newItem, importRow);
                }

                return newItem;
            }
        }

        /// <summary>
        /// gets the parent of the new item created. will create folders based on name or date if configured to
        /// </summary>
        public virtual Item GetParentNode(object importRow, string newItemName) { 
            
			Item thisParent = ImportToWhere;
            if (FolderByDate) {
				FolderTemplate = GetImportFolderTemplate();
				DateField = ImportItem.GetItemField("Date Field", Logger);
                DateTime date = DateTime.Now;
                string dateValue = string.Empty;

                try {
                    dateValue = GetFieldValue(importRow, DateField);
                } catch (ArgumentException ex) {
                    Logger.Log(newItemName, (string.IsNullOrEmpty(DateField))
                        ? "the date name field is empty"
                        : "the field name does not exist in the import row", ProcessStatus.DateParseError, DateField);
                }

                if (string.IsNullOrEmpty(dateValue)) {
                    Logger.Log(newItemName, "Couldn't folder by date. The date value was empty", ProcessStatus.DateParseError, DateField);
                    return thisParent;
                }

                if (!DateTime.TryParse(dateValue, out date) 
                    && !DateTime.TryParseExact(dateValue, new string[] { "d/M/yyyy", "d/M/yyyy HH:mm:ss" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out date)) {
                    Logger.Log(newItemName, "date could not be parsed", ProcessStatus.DateParseError, DateField, dateValue);
                    return thisParent;
                }

                thisParent = GetDateParentNode(ImportToWhere, date, FolderTemplate);
            } else if (FolderByName) {
				FolderTemplate = GetImportFolderTemplate();
                thisParent = GetNameParentNode(ImportToWhere, newItemName.Substring(0, 1), FolderTemplate);
            }
            return thisParent;
        }

        #endregion IDataMap Methods
    }
}