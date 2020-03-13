using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.Data.Items;
using Sitecore.Data;
using Sitecore.SharedSource.DataImporter.Mappings.Fields;
using Sitecore.SharedSource.DataImporter.Extensions;
using Sitecore.SharedSource.DataImporter.Mappings;
using System.Globalization;
using Sitecore.Collections;
using System.IO;
using System.Text.RegularExpressions;
using Sitecore.Globalization;
using Sitecore.Data.Managers;
using Sitecore.Configuration;
using Sitecore.SharedSource.DataImporter.Logger;
using Sitecore.SharedSource.DataImporter.Services;
using Sitecore.Data.Fields;

namespace Sitecore.SharedSource.DataImporter.Providers
{
	/// <summary>
	/// The BaseDataMap is the base class for any data provider. It manages values stored in sitecore 
	/// and does the bulk of the work processing the fields
	/// </summary>
	public abstract class BaseDataMap : IDataMap
	{

		#region Static IDs

		public static readonly string FieldsFolderTemplateID = "{98EF4356-8BFE-4F6A-A697-ADFD0AAD0B65}";

		public static readonly string CommonFolderTemplateID = "{A87A00B1-E6DB-45AB-8B54-636FEC3B5523}";
        
		#endregion Static IDs

		#region Properties

		public Item ImportItem { get; set; }
        protected StringService StringService { get; set; }
        protected ProcessorService ProcessorService { get; set; }
        protected FieldService FieldService { get; set; }

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
        public List<Item> PreProcessors { get; set; }
        public List<Item> PostProcessors { get; set; }

		#endregion IDataMap Fields

		#region Constructor

		public BaseDataMap(Database db, string connectionString, Item importItem, ILogger l)
		{
			if (l == null)
				throw new Exception("The provided Logger is null");

			//instantiate log
			Logger = l;

			//setup import details
			ToDB = db;
			DatabaseConnectionString = connectionString;
			ImportItem = importItem;

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
			ImportToLanguage = GetImportItemLanguage("Import To Language", ToDB);

			//foldering information
			FolderByDate = ImportItem.GetItemBool("Folder By Date");
			FolderByName = ImportItem.GetItemBool("Folder By Name");
			FolderByPath = ImportItem.GetItemBool("Folder By Path");
			DateField = ImportItem.GetItemField("Date Field", Logger);
			FolderTemplate = GetImportFolderTemplate();

			//populate field definitions
			FieldDefinitions = GetFieldDefinitions(ImportItem);

            PreProcessors = GetProcessors("Item Pre Processors");
            PostProcessors = GetProcessors("Item Post Processors");

            StringService = new StringService();
            ProcessorService = new ProcessorService(l);
            FieldService = new FieldService(l);
        }

		#endregion Constructor

		#region Constructor Helpers

		public int GetNameLength()
		{
			int i = 100;
			return (int.TryParse(Settings.GetSetting("MaxItemNameLength"), out i))
					? i
					: 100;
		}

		public Item GetImportToWhereItem()
		{

			Item toWhere = null;

			//check field value
			string toWhereID = ImportItem.GetItemField("Import To Where", Logger);
			if (string.IsNullOrEmpty(toWhereID))
			{
				Logger.Log("the 'To Where' field is not set on the import item", ImportItem.Paths.FullPath, LogType.ImportDefinitionError, "Import To Where");
				return null;
			}

			//check item
			toWhere = ToDB.Items[toWhereID];
			if (toWhere.IsNull())
				Logger.Log("the 'To Where' item is null on the import item", ImportItem.Paths.FullPath, LogType.ImportDefinitionError, "Import To Where", toWhereID);

			return toWhere;
		}

		public CustomItemBase GetImportToTemplate()
		{

			CustomItemBase template = null;

			//check field value
			string templateID = ImportItem.GetItemField("Import To What Template", Logger);
			if (string.IsNullOrEmpty(templateID))
			{
				Logger.Log("the 'To What Template' field is not set", ImportItem.Paths.FullPath, LogType.ImportDefinitionError, "Import To What Template");
				return null;
			}

			//check template item
			Item templateItem = ToDB.Items[templateID];
			if (templateItem.IsNull())
			{
				Logger.Log("the 'To What Template' item is null", ImportItem.Paths.FullPath, LogType.ImportDefinitionError, "Import To What Template");
				return null;
			}

			//determine template item type
			if ((BranchItem)templateItem != null)
			{
				template = (BranchItem)templateItem;
			}
			else
			{
				template = (TemplateItem)templateItem;
			}

			return template;
		}

		public Language GetImportItemLanguage(string fieldName, Database db)
		{

			Language l = LanguageManager.DefaultLanguage;

			//check the field
			string langID = ImportItem.GetItemField(fieldName, Logger);
			if (string.IsNullOrEmpty(langID))
			{
				Logger.Log("The 'Import Language' field is not set on the import item", ImportItem.Paths.FullPath, LogType.ImportDefinitionError, fieldName);
				return l;
			}

			//check item
			Item iLang = db.GetItem(langID);
			if (iLang.IsNull())
			{
				Logger.Log("The 'Import Language' Item is null on the import item", ImportItem.Paths.FullPath, LogType.ImportDefinitionError, fieldName);
				return l;
			}
			
			//check language
			l = LanguageManager.GetLanguage(iLang.Name);
			if (l == null)
			{
				Logger.Log("The 'Import Language' name is not valid on the import item", ImportItem.Paths.FullPath, LogType.ImportDefinitionError, fieldName);
			}

			return l;
		}

		public TemplateItem GetImportFolderTemplate()
		{

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

        public List<Item> GetProcessors(string fieldName)
        {
            var list = new List<Item>();

            var items = ((DelimitedField)ImportItem.Fields[fieldName]).Items;
            foreach (var idValue in items)
            {
                if (!ID.IsID(idValue))
                    continue;

                var procItem = ImportItem.Database.GetItem(new ID(idValue));
                if (procItem == null)
                    continue;

                list.Add(procItem);
            }

            return list;
        }

		#endregion Constructor Helpers

		#region Methods

		public virtual List<IBaseField> GetFieldDefinitions(Item i)
		{
			List<IBaseField> l = new List<IBaseField>();

			//check for fields folder
			Item Fields = i.GetChildByTemplate(FieldsFolderTemplateID);
			if (Fields.IsNull())
			{
				Logger.Log("there is no 'Fields' folder on the import item", i.Paths.FullPath, LogType.ImportDefinitionError);
				return l;
			}

			//check for any children
			if (!Fields.HasChildren)
			{
				Logger.Log("there are no fields to import on  on the import item", i.Paths.FullPath, LogType.ImportDefinitionError);
				return l;
			}

			ChildList c = Fields.Children;
			foreach (Item child in c)
			{
                var bf = FieldService.BuildBaseField(child);
                if (bf != null)
					l.Add(bf);
				else
					Logger.Log("the field's class type could not be instantiated", child.Paths.FullPath, LogType.ImportDefinitionError, child.Name);
			}

			return l;
		}
        
		/// <summary>
		/// retrieves all the import field values specified
		/// </summary>
		public virtual IEnumerable<string> GetFieldValues(IEnumerable<string> fieldNames, object importRow)
		{
			List<string> list = new List<string>();
			foreach (string f in fieldNames)
			{
				try
				{
					var fieldValue = GetFieldValue(importRow, f);
					list.Add(fieldValue);
				}
				catch (ArgumentException)
				{
					Logger.Log((string.IsNullOrEmpty(f))
							? "the 'From' field name is empty"
							: "the field doesn't exist in the import row",
                            "N/A", LogType.FieldError, f);
				}
			}
			return list;
		}

		public virtual IEnumerable<string> GetRTEFieldValues(IEnumerable<string> fieldNames, object importRow)
		{
			List<string> list = new List<string>();
			foreach (string f in fieldNames)
			{
				try
				{
					var fieldValue = GetFieldValue(importRow, f);

					list.Add(fieldValue);
				}
				catch (ArgumentException)
				{
					Logger.Log((string.IsNullOrEmpty(f))
							? "the 'From' field name is empty"
							: "the field doesn't exist in the import row",
                        "N/A", LogType.FieldError, f);
				}
			}

			return list.Select(v => Regex.IsMatch(v, @"<(\s*[(\/?)\w+]*)") ? v : $"<p>{v}</p>");
		}

		/// <summary>
		/// will begin looking for or creating date folders to get a parent node to create the new items in
		/// </summary>
		/// <param name="parentNode">current parent node to create or search folder under</param>
		/// <param name="dt">date time value to folder by</param>
		/// <param name="folderType">folder template type</param>
		/// <returns></returns>
		public Item GetDateParentNode(Item parentNode, DateTime dt, TemplateItem folderType)
		{
			//get year folder
			Item year = GetChild(parentNode, dt.Year.ToString());
			if (year == null)
			{
				//build year folder if you have to
				year = parentNode.Add(dt.Year.ToString(), folderType);
			}
			//set the parent to year
			parentNode = year;

			//get month folder
			Item month = GetChild(parentNode, dt.ToString("MM"));
			if (month == null)
			{
				//build month folder if you have to
				month = parentNode.Add(dt.ToString("MM"), folderType);
			}
			//set the parent to year
			parentNode = month;

			//get day folder
			Item day = GetChild(parentNode, dt.ToString("dd"));
			if (day == null)
			{
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
		public Item GetNameParentNode(Item parentNode, string letter, TemplateItem folderType)
		{
			//get letter folder
			Item letterItem = GetChild(parentNode, letter);
			if (letterItem == null) //build year folder if you have to
				letterItem = parentNode.Add(letter, folderType);

			//set the parent to year
			return letterItem;
		}
        
		public Item GetChild(Item i, string childName)
		{
			if (string.IsNullOrEmpty(childName)) return null;

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
		
        public virtual void PreProcessItem(object importRow, Item newItem)
        {
            foreach(var procItem in PreProcessors)
            {
                ProcessorService.ExecuteProcessor(this, procItem, importRow, newItem);
            }
        }

        public virtual void PostProcessItem(object importRow, Item newItem)
        {
            foreach (var procItem in PostProcessors)
            {
                ProcessorService.ExecuteProcessor(this, procItem, importRow, newItem);
            }
        }
        
        /// <summary>
        /// Defines how the subclass will retrieve a field value
        /// </summary>
        public abstract string GetFieldValue(object importRow, string fieldName);

		public virtual CustomItemBase GetNewItemTemplate(object importRow)
		{
			//Create new item
			if (ImportToWhatTemplate == null || ImportToWhatTemplate.InnerItem.IsNull())
				throw new NullReferenceException("The 'Import To What Template' item is null");
			return ImportToWhatTemplate;
		}

		public virtual List<IBaseField> GetFieldDefinitionsByRow(object importRow)
		{
			return FieldDefinitions;
		}

		public virtual string BuildNewItemName(object importRow)
		{
			if (!ItemNameFields.Any())
				throw new NullReferenceException("there are no 'Name' fields specified");

			StringBuilder strItemName = new StringBuilder();
			foreach (string nameField in ItemNameFields)
			{
				try
				{
					strItemName.Append(GetFieldValue(importRow, nameField));
				}
				catch (ArgumentException)
				{
					throw new NullReferenceException(string.Format("the field name: '{0}' does not exist in the import row", nameField));
				}
			}

			string nameValue = strItemName.ToString();
			if (string.IsNullOrEmpty(nameValue))
				throw new NullReferenceException(string.Format("the name fields: '{0}' are empty in the import row", string.Join(",", ItemNameFields)));
			return StringService.GetValidItemName(nameValue, this.ItemNameMaxLength).Trim();
		}

		public virtual Item CreateNewItem(Item parent, object importRow, string newItemName)
		{
			CustomItemBase nItemTemplate = GetNewItemTemplate(importRow);
            Item newItem;

            using (new LanguageSwitcher(ImportToLanguage))
			{
				//get the parent in the specific language
				parent = ToDB.GetItem(parent.ID);
                
				//search for the child by name
				newItem = GetChild(parent, newItemName);
				if (newItem?.Versions.Count == 0) //add version for lang
					newItem = newItem.Versions.AddVersion();

				//if not found then create one
				if (newItem == null)
				{
					if (nItemTemplate is BranchItem)
						newItem = parent.Add(newItemName, (BranchItem)nItemTemplate);
					else
						newItem = parent.Add(newItemName, (TemplateItem)nItemTemplate);
				}

				if (newItem == null)
					throw new NullReferenceException("the new item created was null");
            }

            return newItem;
		}

		/// <summary>
		/// gets the parent of the new item created. will create folders based on name or date if configured to
		/// </summary>
		public virtual Item GetParentNode(object importRow, string newItemName)
		{
			Item thisParent = ImportToWhere;
			if (FolderByDate)
			{
				DateTime date = DateTime.Now;
				string dateValue = string.Empty;

				try
				{
					dateValue = GetFieldValue(importRow, DateField);
				}
				catch (ArgumentException)
				{
					Logger.Log((string.IsNullOrEmpty(DateField))
							? "the date name field is empty"
							: "the field name does not exist in the import row", newItemName, LogType.DateParseError, DateField);
				}

				if (string.IsNullOrEmpty(dateValue))
				{
					Logger.Log("Couldn't folder by date. The date value was empty", newItemName, LogType.DateParseError, DateField);
					return thisParent;
				}

				if (!DateTime.TryParse(dateValue, out date)
						&& !DateTime.TryParseExact(dateValue, new string[] { "d/M/yyyy", "d/M/yyyy HH:mm:ss", "yyyyMMdd'T'HHmmss" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
				{
					Logger.Log("date could not be parsed", newItemName, LogType.DateParseError, DateField, dateValue);
					return thisParent;
				}

				thisParent = GetDateParentNode(ImportToWhere, date, FolderTemplate);
			}
			else if (FolderByName)
			{
				string[] nameParts = newItemName.Split(' ');
				string firstLetterLastWord = nameParts.First().Substring(0, 1);
				int val = 0;
				var folderName = int.TryParse(firstLetterLastWord, out val) ? "123" : firstLetterLastWord;
				thisParent = GetNameParentNode(ImportToWhere, folderName.ToUpper(), FolderTemplate);
			}
			return thisParent;
		}

		#endregion IDataMap Methods
	}
}