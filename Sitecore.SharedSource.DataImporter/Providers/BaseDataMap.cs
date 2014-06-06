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

namespace Sitecore.SharedSource.DataImporter.Providers
{
    /// <summary>
    /// The BaseDataMap is the base class for any data provider. It manages values stored in sitecore 
    /// and does the bulk of the work processing the fields
    /// </summary>
	public abstract class BaseDataMap {
		
		#region Properties

        /// <summary>
        /// the log is returned with any messages indicating the status of the import
        /// </summary>
        protected StringBuilder log;

        /// <summary>
        /// template id of the fields folder
        /// </summary>
        public static readonly string FieldsFolderID = "{98EF4356-8BFE-4F6A-A697-ADFD0AAD0B65}";

        /// <summary>
        /// template id of the properties folder
        /// </summary>
        public static readonly string PropertiesFolderID = "{8452785D-FFE7-47F3-911E-F219F5BDEA3A}";

		private Item _Parent;
		/// <summary>
		/// the parent item where the new items will be imported into
		/// </summary>
        public Item Parent {
			get {
				return _Parent;
			}
			set {
				_Parent = value;
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

		private CustomItemBase _NewItemTemplate ;
		/// <summary>
		/// the template to create new items with
		/// </summary>
        public CustomItemBase NewItemTemplate  {
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

		private Item _ImportLanguage;
		public Item ImportLanguage {
			get {
				return _ImportLanguage;
			}
			set {
				_ImportLanguage = value;
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
        /// <summary>
        /// the query used to retrieve the data
        /// </summary>
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
            //instantiate log
            log = new StringBuilder();

            //setup import details
			SitecoreDB = db;
            DatabaseConnectionString = connectionString;
            //get query
            Query = importItem.Fields["Query"].Value;
            if (string.IsNullOrEmpty(Query)) {
                Log("Error", "the 'Query' field was not set");
            }
            //get parent and store it
            string parentID = importItem.Fields["Import To Where"].Value;
            if (!string.IsNullOrEmpty(parentID)) {
                Item parent = SitecoreDB.Items[parentID];
                if(parent.IsNotNull())
                    Parent = parent;
                else
                    Log("Error", "the 'To Where' item is null");
			} else {
                Log("Error", "the 'To Where' field is not set");
            }
            //get new item template
            string templateID = importItem.Fields["Import To What Template"].Value;
			if (!string.IsNullOrEmpty(templateID)) {
                Item templateItem = SitecoreDB.Items[templateID];
                if(templateItem.IsNotNull()) {
			        if ((BranchItem) templateItem != null) {
			            NewItemTemplate = (BranchItem)templateItem;
                    } else {
                        NewItemTemplate = (TemplateItem)templateItem;
                    }
                } else {
                    Log("Error", "the 'To What Template' item is null");
                }
            } else {
                Log("Error", "the 'To What Template' field is not set");
            }
			//more properties
            ItemNameDataField = importItem.Fields["Pull Item Name from What Fields"].Value;
			ItemNameMaxLength = int.Parse(importItem.Fields["Item Name Max Length"].Value);
			ImportLanguage = SitecoreDB.GetItem(importItem.Fields["Import Language"].Value);
			if(ImportLanguage == null)
                Log("Error", "The 'Import Language' field is not set");

			//foldering information
			FolderByDate = ((CheckboxField)importItem.Fields["Folder By Date"]).Checked;
			FolderByName = ((CheckboxField)importItem.Fields["Folder By Name"]).Checked;
			DateField = importItem.Fields["Date Field"].Value;
			if (FolderByName || FolderByDate) {
				//setup a default type to an ordinary folder
				Sitecore.Data.Items.TemplateItem FolderItem = SitecoreDB.Templates["{A87A00B1-E6DB-45AB-8B54-636FEC3B5523}"];
				//if they specify a type then use that
				string folderID = importItem.Fields["Folder Template"].Value;
				if (!string.IsNullOrEmpty(folderID))
					FolderItem = SitecoreDB.Templates[folderID];
				FolderTemplate = FolderItem;
			}

			//start handling fields
            Item Fields = GetItemByTemplate(importItem, FieldsFolderID);
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
                                    Log("Error", string.Format("the field:{0} binary {1} specified could not be found", child.Name, bm.HandlerAssembly));
                                }
                                if (bf != null)
                                    FieldDefinitions.Add(bf);
                                else
                                    Log("Error", string.Format("the field: '{0}' class type {1} could not be instantiated", child.Name, bm.HandlerClass));
                            } else {
                                Log("Error", string.Format("the field: '{0}' Handler Class {1} is not defined", child.Name, bm.HandlerClass));
                            }
                        } else {
                            Log("Error", string.Format("the field: '{0}' Handler Assembly {1} is not defined", child.Name, bm.HandlerAssembly));
                        }
                    }
                } else {
                    Log("Warn", "there are no fields to import");
                }
            } else {
                Log("Warn", "there is no 'Fields' folder"); 
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

        /// <summary>
        /// will begin looking for or creating date folders to get a parent node to create the new items in
        /// </summary>
        /// <param name="parentNode">current parent node to create or search folder under</param>
        /// <param name="dt">date time value to folder by</param>
        /// <param name="folderType">folder template type</param>
        /// <returns></returns>
        public static Item GetDateParentNode(Item parentNode, DateTime dt, TemplateItem folderType) {
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
        public static Item GetNameParentNode(Item parentNode, string letter, TemplateItem folderType) {
            //get letter folder
            Item letterItem = parentNode.Children[letter];
            if (letterItem == null) {
                //build year folder if you have to
                letterItem = parentNode.Add(letter, folderType);
            }
            //set the parent to year
            return letterItem;
        }

        #endregion Static Methods
        
        #region Methods

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

					Language l;
					Language.TryParse(ImportLanguage.Name, out l);			
					using (new LanguageSwitcher(l)) {
						//get the parent in the specific language
						thisParent = SitecoreDB.GetItem(thisParent.ID);
						
						Item newItem;
						//search for the child by name
						newItem = thisParent.GetChildren()[newItemName];
						if(newItem != null) //add version for lang
							newItem = newItem.Versions.AddVersion();

						//if not found then create one
						if (newItem == null) {
							if (NewItemTemplate is BranchItem)
								newItem = thisParent.Add(newItemName, (BranchItem)NewItemTemplate);
							else
								newItem = thisParent.Add(newItemName, (TemplateItem)NewItemTemplate);
						}

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
					}
                } catch (Exception ex) {
                    Log("Error", ex.Message);
                }
            }
            //if no messages then you're good
            if (log.Length < 1 || !log.ToString().Contains("Error")) 
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
