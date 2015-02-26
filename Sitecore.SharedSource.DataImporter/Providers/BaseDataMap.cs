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

namespace Sitecore.SharedSource.DataImporter.Providers {
    /// <summary>
    /// The BaseDataMap is the base class for any data provider. It manages values stored in sitecore 
    /// and does the bulk of the work processing the fields
    /// </summary>
    public abstract class BaseDataMap {

        public Item ImportItem { get; set; }

        /// <summary>
        /// the log is returned with any messages indicating the status of the import
        /// </summary>
        protected StringBuilder log;

        #region Static IDs

        public static readonly string FieldsFolderTemplateID = "{98EF4356-8BFE-4F6A-A697-ADFD0AAD0B65}";

        public static readonly string CommonFolderTemplateID = "{A87A00B1-E6DB-45AB-8B54-636FEC3B5523}";

        #endregion Static IDs

        #region Properties

        private Item _ImportToWhere;
        /// <summary>
        /// the parent item where the new items will be imported into
        /// </summary>
        public Item ImportToWhere {
            get {
                return _ImportToWhere;
            }
            set {
                _ImportToWhere = value;
            }
        }

        private Database _SitecoreDB;
        /// <summary>
        /// the reference to the sitecore database that you'll import into and query from
        /// </summary>
        public Database SitecoreDB {
            get {
                return _SitecoreDB;
            }
            set {
                _SitecoreDB = value;
            }
        }

        private CustomItemBase _NewItemTemplate;
        /// <summary>
        /// the template to create new items with
        /// </summary>
        public CustomItemBase NewItemTemplate {
            get {
                return _NewItemTemplate;
            }
            set {
                _NewItemTemplate = value;
            }
        }

        private string _itemNameDataField;
        /// <summary>
        /// the sitecore field value of fields used to build the new item name
        /// </summary>
        public string ItemNameDataField {
            get {
                return _itemNameDataField;
            }
            set {
                _itemNameDataField = value;
            }
        }

        private string[] _NameFields;
        /// <summary>
        /// the string array of fields used to build the new item name
        /// </summary>
        public string[] NameFields {
            get {
                if (_NameFields == null) {
                    string[] comSplitr = { "," };
                    _NameFields = this.ItemNameDataField.Split(comSplitr, StringSplitOptions.RemoveEmptyEntries);
                }
                return _NameFields;
            }
            set {
                _NameFields = value;
            }
        }

        private int _itemNameMaxLength;
        /// <summary>
        /// max length for item names
        /// </summary>
        public int ItemNameMaxLength {
            get {
                return _itemNameMaxLength;
            }
            set {
                _itemNameMaxLength = value;
            }
        }

        private Language _ImportToLanguage;
        public Language ImportToLanguage {
            get {
                return _ImportToLanguage;
            }
            set {
                _ImportToLanguage = value;
            }
        }

        private List<IBaseField> _fieldDefinitions = new List<IBaseField>();
        /// <summary>
        /// the definitions of fields to import
        /// </summary>
        public List<IBaseField> FieldDefinitions {
            get {
                return _fieldDefinitions;
            }
            set {
                _fieldDefinitions = value;
            }
        }

        private bool _FolderByDate;
        /// <summary>
        /// tells whether or not to folder new items by a date
        /// </summary>
        public bool FolderByDate {
            get {
                return _FolderByDate;
            }
            set {
                _FolderByDate = value;
            }
        }

        private bool _FolderByName;
        /// <summary>
        /// tells whether or not to folder new items by first letter of their name
        /// </summary>
        public bool FolderByName {
            get {
                return _FolderByName;
            }
            set {
                _FolderByName = value;
            }
        }

        private string _DateField;
        /// <summary>
        /// the name of the field that stores a date to folder by
        /// </summary>
        public string DateField {
            get {
                return _DateField;
            }
            set {
                _DateField = value;
            }
        }

        private Sitecore.Data.Items.TemplateItem _FolderTemplate;
        /// <summary>
        /// the template used to create the folder items
        /// </summary>
        public Sitecore.Data.Items.TemplateItem FolderTemplate {
            get {
                return _FolderTemplate;
            }
            set {
                _FolderTemplate = value;
            }
        }

        private string _dbConnectionString;
        /// <summary>
        /// the connection string to the database you're importing from
        /// </summary>
        public string DatabaseConnectionString {
            get {
                return _dbConnectionString;
            }
            set {
                _dbConnectionString = value;
            }
        }

        private string _Query;
        /// <summary>
        /// the query used to retrieve the data
        /// </summary>
        public string Query {
            get {
                return _Query;
            }
            set {
                _Query = value;
            }
        }

        #endregion Properties

        #region Constructor

        public BaseDataMap(Database db, string connectionString, Item importItem) {
            //instantiate log
            log = new StringBuilder();

            //setup import details
            SitecoreDB = db;
            DatabaseConnectionString = connectionString;
            ImportItem = importItem;

            //get query
            Query = GetImportItemField("Query");

            //get parent item
            ImportToWhere = GetImportToWhereItem();

            //get new item template
            NewItemTemplate = GetImportToTemplate();

            //get item name field
            ItemNameDataField = GetImportItemField("Pull Item Name from What Fields");

            //get item name max length
            ItemNameMaxLength = int.Parse(GetImportItemField("Item Name Max Length"));

            //get import language
            ImportToLanguage = GetImportItemLanguage("Import To Language");

            //foldering information
            FolderByDate = GetImportItemBool("Folder By Date");
            FolderByName = GetImportItemBool("Folder By Name");
            DateField = GetImportItemField("Date Field");
            FolderTemplate = GetImportFolderTemplate();

            //populate field definitions
            GetFieldDefinitions(ImportItem);
        }

        #endregion Constructor

        #region Abstract Methods

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
        protected abstract string GetFieldValue(object importRow, string fieldName);

        #endregion Abstract Methods

        #region Constructor Helpers

        public bool GetImportItemBool(string fieldName) {
            CheckboxField cb = (CheckboxField)ImportItem.Fields[fieldName];
            return (cb == null) ? false : cb.Checked;
        }

        public string GetImportItemField(string fieldName) {
            return GetItemField(ImportItem, fieldName);
        }

        public string GetItemField(Item i, string fieldName) {
            //check item
            if (i == null) {
                Log("Error", "the item is null");
                return string.Empty;
            }

            //check field
            Field f = i.Fields[fieldName];
            if (f == null) {
                Log("Error", string.Format("the field '{0}' on the item '{1}' is null", fieldName, i.DisplayName));
                return string.Empty;
            }

            //check value
            string s = f.Value;
            if (string.IsNullOrEmpty(s))
                Log("Warn", string.Format("the '{0}' field was not set", fieldName));

            return s;
        }

        public Item GetImportToWhereItem() {

            Item toWhere = null;

            //check field value
            string toWhereID = GetImportItemField("Import To Where");
            if (string.IsNullOrEmpty(toWhereID)) {
                Log("Error", "the 'To Where' field is not set");
                return null;
            }

            //check item
            toWhere = SitecoreDB.Items[toWhereID];
            if (toWhere.IsNull())
                Log("Error", "the 'To Where' item is null");

            return toWhere;
        }

        public CustomItemBase GetImportToTemplate() {

            CustomItemBase template = null;

            //check field value
            string templateID = GetImportItemField("Import To What Template");
            if (string.IsNullOrEmpty(templateID)) {
                Log("Error", "the 'To What Template' field is not set");
                return null;
            }

            //check template item
            Item templateItem = SitecoreDB.Items[templateID];
            if (templateItem.IsNull()) {
                Log("Error", "the 'To What Template' item is null");
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
            string langID = GetImportItemField(fieldName);
            if (string.IsNullOrEmpty(langID)) {
                Log("Error", "The 'Import Language' field is not set");
                return l;
            }

            //check item
            Item iLang = SitecoreDB.GetItem(langID);
            if (iLang.IsNull()) {
                Log("Error", "The 'Import Language' Item is null");
                return l;
            }

            //check language
            l = LanguageManager.GetLanguage(iLang.Name);
            if (l == null) {
                Log("Error", "The 'Import Language' name is not valid");
            }

            return l;
        }

        public TemplateItem GetImportFolderTemplate() {

            if (!FolderByName && !FolderByDate)
                return null;

            //setup a default type to an ordinary folder
            TemplateItem defaultTemplate = SitecoreDB.Templates[CommonFolderTemplateID];

            //if they specify a type then use that
            string folderID = GetImportItemField("Folder Template");
            if (string.IsNullOrEmpty(folderID))
                return defaultTemplate;

            //check the folder template
            TemplateItem fTemplate = SitecoreDB.Templates[folderID];
            return (fTemplate == null) ? defaultTemplate : fTemplate;
        }

        public void GetFieldDefinitions(Item i) {

            //check for fields folder
            Item Fields = GetItemByTemplate(i, FieldsFolderTemplateID);
            if (Fields.IsNull()) {
                Log("Warn", "there is no 'Fields' folder");
                return;
            }

            //check for any children
            if (!Fields.HasChildren) {
                Log("Warn", "there are no fields to import");
                return;
            }

            ChildList c = Fields.GetChildren();
            foreach (Item child in c) {
                //create an item to get the class / assembly name from
                BaseMapping bm = new BaseMapping(child);

                //check for assembly
                if (string.IsNullOrEmpty(bm.HandlerAssembly)) {
                    Log("Error", string.Format("the field: '{0}' Handler Assembly {1} is not defined", child.Name, bm.HandlerAssembly));
                    continue;
                }

                //check for class
                if (string.IsNullOrEmpty(bm.HandlerClass)) {
                    Log("Error", string.Format("the field: '{0}' Handler Class {1} is not defined", child.Name, bm.HandlerClass));
                    continue;
                }

                //create the object from the class and cast as base field to add it to field definitions
                IBaseField bf = null;
                try {
                    bf = (IBaseField)Sitecore.Reflection.ReflectionUtil.CreateObject(bm.HandlerAssembly, bm.HandlerClass, new object[] { child });
                } catch (FileNotFoundException fnfe) {
                    Log("Error", string.Format("the field:{0} binary {1} specified could not be found", child.Name, bm.HandlerAssembly));
                }

                if (bf != null)
                    FieldDefinitions.Add(bf);
                else
                    Log("Error", string.Format("the field: '{0}' class type {1} could not be instantiated", child.Name, bm.HandlerClass));
            }
        }

        #endregion Constructor Helpers

        #region Methods

        /// <summary>
        /// will begin looking for or creating date folders to get a parent node to create the new items in
        /// </summary>
        /// <param name="parentNode">current parent node to create or search folder under</param>
        /// <param name="dt">date time value to folder by</param>
        /// <param name="folderType">folder template type</param>
        /// <returns></returns>
        public Item GetDateParentNode(Item parentNode, DateTime dt, TemplateItem folderType) {
            //get year folder
            Item year = parentNode.Children[dt.Year.ToString()];
            if (year == null) {
                //build year folder if you have to
                year = parentNode.Add(dt.Year.ToString(), folderType);
            }
            //set the parent to year
            parentNode = year;

            //get month folder
            Item month = parentNode.Children[dt.ToString("MM")];
            if (month == null) {
                //build month folder if you have to
                month = parentNode.Add(dt.ToString("MM"), folderType);
            }
            //set the parent to year
            parentNode = month;

            //get day folder
            Item day = parentNode.Children[dt.ToString("dd")];
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
        public Item GetNameParentNode(Item parentNode, string letter, TemplateItem folderType) {
            //get letter folder
            Item letterItem = parentNode.Children[letter];
            if (letterItem == null) {
                //build year folder if you have to
                letterItem = parentNode.Add(letter, folderType);
            }
            //set the parent to year
            return letterItem;
        }

        /// <summary>
        /// searches under the parent for an item whose template matches the id provided
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="TemplateID"></param>
        /// <returns></returns>
        protected Item GetItemByTemplate(Item parent, string TemplateID) {
            IEnumerable<Item> x = from Item i in parent.GetChildren()
                                  where i.Template.IsID(TemplateID)
                                  select i;
            return (x.Any()) ? x.First() : null;
        }

        /// <summary>
        /// Used to log status information while the import is processed
        /// </summary>
        /// <param name="errorType"></param>
        /// <param name="message"></param>
        protected void Log(string errorType, string message) {
            log.AppendFormat("{0} : {1}", errorType, message).AppendLine().AppendLine();
        }

        /// <summary>
        /// processes each field against the data provided by subclasses
        /// </summary>
        public string Process() {
            IEnumerable<object> importItems;
            try {
                importItems = GetImportData();
            } catch (Exception ex) {
                importItems = Enumerable.Empty<object>();
                Log("Import Error", ex.Message);
            }

            long line = 0;

            try {
                //Loop through the data source
                foreach (object importRow in importItems) {
                    line++;

                    string newItemName = GetNewItemName(importRow);
                    if (string.IsNullOrEmpty(newItemName))
                        continue;

                    Item thisParent = GetParentNode(importRow, newItemName);
                    if (thisParent.IsNull())
                        throw new NullReferenceException("The new item's parent is null");

                    CreateNewItem(thisParent, importRow, newItemName);
                }
            } catch (Exception ex) {
                Log("Error (line: " + line + ")", ex.Message);
            }

            //if no messages then you're good
            if (log.Length < 1 || !log.ToString().Contains("Error"))
                Log("Success", "the import completed successfully");

            return log.ToString();
        }

        public void CreateNewItem(Item parent, object importRow, string newItemName) {

            CustomItemBase nItemTemplate = GetNewItemTemplate(importRow);

            using (new LanguageSwitcher(ImportToLanguage)) {
                //get the parent in the specific language
                parent = SitecoreDB.GetItem(parent.ID);

                Item newItem;
                //search for the child by name
                newItem = parent.GetChildren()[newItemName];
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
                    foreach (IBaseField d in this.FieldDefinitions) {
                        IEnumerable<string> values = GetFieldValues(d.GetExistingFieldNames(), importRow);

                        d.FillField(this, ref newItem, String.Join(d.GetFieldValueDelimiter(), values));
                    }

                    //calls the subclass method to handle custom fields and properties
                    ProcessCustomData(ref newItem, importRow);
                }
            }
        }

        public virtual CustomItemBase GetNewItemTemplate(object importRow) {
            //Create new item
            if (NewItemTemplate == null || NewItemTemplate.InnerItem.IsNull())
                throw new NullReferenceException("The 'Import To What Template' item is null");
            return NewItemTemplate;
        }

        /// <summary>
        /// creates an item name based on the name field values in the importRow
        /// </summary>
        public string GetNewItemName(object importRow) {
            if (!NameFields.Any())
                throw new NullReferenceException("there are no 'Name' fields specified");

            StringBuilder strItemName = new StringBuilder();
            foreach (string nameField in NameFields) {
                try {
                    strItemName.Append(GetFieldValue(importRow, nameField));
                } catch (ArgumentException ex) {
                    if (string.IsNullOrEmpty(this.ItemNameDataField))
                        throw new NullReferenceException("the 'Name' field is empty");
                    else
                        throw new NullReferenceException(string.Format("the field name: '{0}' does not exist in the import row", nameField));
                }
            }
            return StringUtility.GetNewItemName(strItemName.ToString(), this.ItemNameMaxLength);
        }

        /// <summary>
        /// retrieves all the import field values specified
        /// </summary>
        public IEnumerable<string> GetFieldValues(IEnumerable<string> fieldNames, object importRow) {
            List<string> list = new List<string>();
            foreach (string f in fieldNames) {
                try {
                    list.Add(GetFieldValue(importRow, f));
                } catch (ArgumentException ex) {
                    if (string.IsNullOrEmpty(f))
                        Log("Field Error", "the 'From' field name is empty");
                    else
                        Log("Field Error", string.Format("the field name: '{0}' does not exist in the import row", f));
                }
            }
            return list;
        }

        /// <summary>
        /// gets the parent of the new item created. will create folders based on name or date if configured to
        /// </summary>
        protected Item GetParentNode(object importRow, string newItemName) {
            Item thisParent = ImportToWhere;
            if (this.FolderByDate) {
                DateTime date = DateTime.Now;
                string dateValue = string.Empty;
                try {
                    dateValue = GetFieldValue(importRow, this.DateField);
                } catch (ArgumentException ex) {
                    if (string.IsNullOrEmpty(this.DateField))
                        Log("Field Error", "the date name field is empty");
                    else
                        Log("Field Error", string.Format("the field name: '{0}' does not exist in the import row", this.DateField));
                }
                if (!string.IsNullOrEmpty(dateValue)) {
                    if (DateTime.TryParse(dateValue, out date))
                        thisParent = GetDateParentNode(ImportToWhere, date, this.FolderTemplate);
                    else
                        Log("Error", "the date value could not be parsed");
                } else {
                    Log("Error", "the date value was empty");
                }
            } else if (this.FolderByName) {
                thisParent = GetNameParentNode(ImportToWhere, newItemName.Substring(0, 1), this.FolderTemplate);
            }
            return thisParent;
        }

        #endregion Methods
    }
}
