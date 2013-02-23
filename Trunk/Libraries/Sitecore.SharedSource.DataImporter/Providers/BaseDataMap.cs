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

namespace Sitecore.SharedSource.DataImporter.Providers
{
	public abstract class BaseDataMap {
		
		#region Properties

        protected StringBuilder log;

		private Item _Parent;
		public Item Parent {
			get {
				return _Parent;
			}
			set {
				_Parent = value;
			}
		}

		private Database _SitecoreDB;
		public Database SitecoreDB {
			get {
				return _SitecoreDB;
			}
			set {
				_SitecoreDB = value;
			}
		}

		private string _scParentItemPath;
		public string SitecoreParentItemPath {
			get {
				return _scParentItemPath;
			}
			set {
				_scParentItemPath = value;
			}
		}

		private TemplateItem _NewItemTemplate ;
		public TemplateItem NewItemTemplate  {
			get {
				return _NewItemTemplate;
			}
			set {
				_NewItemTemplate = value;
			}
		}

		private string _itemNameDataField;
		public string ItemNameDataField {
			get {
				return _itemNameDataField;
			}
			set {
				_itemNameDataField = value;
			}
		}

		private string[] _NameFields;
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
		public int ItemNameMaxLength {
			get {
				return _itemNameMaxLength;
			}
			set {
				_itemNameMaxLength = value;
			}
		}

		private List<IBaseField> _fieldDefinitions = new List<IBaseField>();
		public List<IBaseField> FieldDefinitions {
			get {
				return _fieldDefinitions;
			}
			set {
				_fieldDefinitions = value;
			}
		}

		private bool _FolderByDate;
		public bool FolderByDate {
			get {
				return _FolderByDate;
			}
			set {
				_FolderByDate = value;
			}
		}

		private bool _FolderByName;
		public bool FolderByName {
			get {
				return _FolderByName;
			}
			set {
				_FolderByName = value;
			}
		}

		private string _DateField;
		public string DateField {
			get {
				return _DateField;
			}
			set {
				_DateField = value;
			}
		}

		private Sitecore.Data.Items.TemplateItem _FolderTemplate;
		public Sitecore.Data.Items.TemplateItem FolderTemplate {
			get {
				return _FolderTemplate;
			}
			set {
				_FolderTemplate = value;
			}
		}

        private string _dbConnectionString;
        public string DatabaseConnectionString
        {
            get
            {
                return _dbConnectionString;
            }
            set
            {
                _dbConnectionString = value;
            }
        }

        private string _Query;
        public string Query
        {
            get
            {
                return _Query;
            }
            set
            {
                _Query = value;
            }
        }

		#endregion Properties

		#region Constructor

        public BaseDataMap(Database db, string connectionString, Item importItem)
        {
            log = new StringBuilder();

            //setup import details
			SitecoreDB = db;
            DatabaseConnectionString = connectionString;
            //get query
            Query = importItem.Fields["Query"].Value;
            if (string.IsNullOrEmpty(Query)) {
                Log("Error", "the 'Query' field was not set");
            }
            //get parent path
            string parentID = importItem.Fields["Import To Where"].Value;
            if (!string.IsNullOrEmpty(parentID)) {
                Item parent = SitecoreDB.Items[parentID];
                if(parent.IsNotNull()){
                    SitecoreParentItemPath = parent.Paths.Path;
                    Parent = SitecoreDB.GetItem(this.SitecoreParentItemPath);
                } else {
                    Log("Error", "the 'To Where' item is null");
                }
			} else {
                Log("Error", "the 'To Where' field is not set");
            }
            //get new item template
            string templateID = importItem.Fields["Import To What Template"].Value;
			if (!string.IsNullOrEmpty(templateID)) {
                Item templateItem = SitecoreDB.Items[templateID];
                if(templateItem.IsNotNull())
                    NewItemTemplate = templateItem;
                else
                    Log("Error", "the 'To What Template' item is null");
            } else {
                Log("Error", "the 'To What Template' field is not set");
            }
			ItemNameDataField = importItem.Fields["Pull Item Name from What Fields"].Value;
			ItemNameMaxLength = int.Parse(importItem.Fields["Item Name Max Length"].Value);

			FolderByDate = ((CheckboxField)importItem.Fields["Folder By Date"]).Checked;
			FolderByName = ((CheckboxField)importItem.Fields["Folder By Name"]).Checked;
			DateField = importItem.Fields["Date Field"].Value;
			if (FolderByName || FolderByDate) {

				//setup a default type to an ordinary folder
				Sitecore.Data.Items.TemplateItem FolderItem = SitecoreDB.Templates["{A87A00B1-E6DB-45AB-8B54-636FEC3B5523}"];
				//if they specify a type then use that
				string folderID = importItem.Fields["Folder Template"].Value;
				if (!string.IsNullOrEmpty(folderID)) {
					FolderItem = SitecoreDB.Templates[folderID];
				}
				FolderTemplate = FolderItem;
			}

			//Handle Fields
			Item Fields = importItem.Children["Fields"];
            if (Fields.IsNotNull()) {
                ChildList c = Fields.GetChildren();
                if (c.Any()) {
                    foreach (Item child in c) {
                        //create an item to get the class / assembly name from
                        BaseMapping bm = new BaseMapping(child);
                        if (!string.IsNullOrEmpty(bm.HandlerAssembly)){
                            if (!string.IsNullOrEmpty(bm.HandlerClass)) {
                                //create the object from the class and cast as base field to add it to field definitions
                                IBaseField bf = null;
                                try {
                                    bf = (IBaseField)Sitecore.Reflection.ReflectionUtil.CreateObject(bm.HandlerAssembly, bm.HandlerClass, new object[] { child });
                                } catch (FileNotFoundException fnfe) {
                                    Log("Error", "the binary specified could not be found");
                                }
                                if (bf != null)
                                    FieldDefinitions.Add(bf);
                                else
                                    Log("Error", string.Format("the field: '{0}' class type could not be instantiated", child.Name));
                            } else {
                                Log("Error", string.Format("the field: '{0}' Handler Class is not defined", child.Name));
                            }
                        } else {
                            Log("Error", string.Format("the field: '{0}' Handler Assembly is not defined", child.Name));
                        }
                    }
                } else {
                    Log("Warn", "there are no fields to import");
                }
            } else {
                Log("Warn", "there is no 'Fields' folder"); 
            }

			Item Props = importItem.Children["Properties"];
			if (Props.IsNotNull()) {
				ChildList c = Props.GetChildren();
                if (c.Any()) {
                    foreach (Item child in c) {
                        //create an item to get the class / assembly name from
                        BaseMapping bm = new BaseMapping(child);
                        //create the object from the class and cast as base field to add it to field definitions
                        IBaseProperty bp = (IBaseProperty)Sitecore.Reflection.ReflectionUtil.CreateObject(bm.HandlerAssembly, bm.HandlerClass, new object[] { child });
                        ((SitecoreDataMap)this).PropertyDefinitions.Add(bp);
                    }
                } else {
                    Log("Warn", "there are no properties to import");
                }
			} 
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

        #region Static Methods

        public static Item GetDateParentNode(Item parentNode, DateTime dt, TemplateItem folderType)
        {

            //get year folder
            Item year = parentNode.Children[dt.Year.ToString()];
            if (year == null)
            {
                //build year folder if you have to
                year = parentNode.Add(dt.Year.ToString(), folderType);
            }
            //set the parent to year
            parentNode = year;

            //get month folder
            Item month = parentNode.Children[dt.ToString("MM")];
            if (month == null)
            {
                //build month folder if you have to
                month = parentNode.Add(dt.ToString("MM"), folderType);
            }
            //set the parent to year
            parentNode = month;

            //get day folder
            Item day = parentNode.Children[dt.ToString("dd")];
            if (day == null)
            {
                //build day folder if you have to
                day = parentNode.Add(dt.ToString("dd"), folderType);
            }
            //set the parent to year
            parentNode = day;

            return parentNode;
        }

        public static Item GetNameParentNode(Item parentNode, string letter, TemplateItem folderType)
        {

            //get letter folder
            Item letterItem = parentNode.Children[letter];
            if (letterItem == null)
            {
                //build year folder if you have to
                letterItem = parentNode.Add(letter, folderType);
            }
            //set the parent to year
            return letterItem;
        }

        #endregion Static Methods
        
        #region Methods

        protected void Log(string errorType, string message)
        {
            log.AppendFormat("{0} : {1}", errorType, message).AppendLine().AppendLine();
        }

        /// <summary>
        /// processes each field against the data provided by subclasses
        /// </summary>
        public string Process()
        {
            IEnumerable<object> importItems;
            try {
                importItems = GetImportData();
            } catch (Exception ex) {
                importItems = Enumerable.Empty<object>();
                Log("Connection Error", ex.Message);
            }
            
            //Loop through the data source
            foreach (object importRow in importItems)
            {
                if (!this.NameFields.Any()) {
                    Log("Error", "there are no 'Name' fields specified");
                    break;
                }
                
                string newItemName = GetNewItemName(importRow);
                if (string.IsNullOrEmpty(newItemName))
                    continue;

                Item thisParent = GetParentNode(importRow, newItemName);
                if (thisParent.IsNull()) {
                    Log("Error", "The new item's parent is null");   
                    break;
                }

                try {
                    //Create new item
                    if(NewItemTemplate == null || NewItemTemplate.InnerItem.IsNull()) {
                        Log("Error", "The 'Import To What Template' item is null");
                        break;
                    }
                    
                    Sitecore.Data.Items.Item newItem = thisParent.Add(newItemName, NewItemTemplate);
                    if (newItem == null) {
                        Log("Error", "the new item created was null");
                        continue;
                    }

                    using (new EditContext(newItem, true, false)) {
                        //add in the field mappings
                        foreach (IBaseField d in this.FieldDefinitions) { 
                            IEnumerable<string> values = GetFieldValues(d.GetExistingFieldNames(), importRow);
                            d.FillField(this, ref newItem, String.Join(d.GetFieldValueDelimiter(), values));
                        }

                        //calls the subclass method to handle custom fields and properties
                        ProcessCustomData(ref newItem, importRow);
                    }
                } catch (Exception ex) {
                    Log("Error", ex.Message);
                }
            }
            //if no messages then you're good
            if (log.Length < 1) 
                Log("Success", "the import completed successfully");

            return log.ToString();
        }

        /// <summary>
        /// creates an item name based on the name field values in the importRow
        /// </summary>
        public string GetNewItemName(object importRow) {
            StringBuilder strItemName = new StringBuilder();
            foreach (string nameField in NameFields) {
                try {
                    strItemName.Append(GetFieldValue(importRow, nameField));
                } catch (ArgumentException ex) {
                    if (string.IsNullOrEmpty(this.ItemNameDataField))
                        Log("Field Error", "the 'Name' field is empty");
                    else
                        Log("Field Error", string.Format("the field name: '{0}' does not exist in the import row", nameField));
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
        protected Item GetParentNode(object importRow, string newItemName)
        {
            Item thisParent = Parent;
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
                if(!string.IsNullOrEmpty(dateValue)){
                    if (DateTime.TryParse(dateValue, out date))
                        thisParent = GetDateParentNode(Parent, date, this.FolderTemplate);
                    else
                        Log("Error", "the date value could not be parsed");
                } else {
                    Log("Error", "the date value was empty");
                }
            } else if (this.FolderByName) {
                thisParent = GetNameParentNode(Parent, newItemName.Substring(0, 1), this.FolderTemplate);
            }
            return thisParent;
        }

		#endregion Methods
	}
}
