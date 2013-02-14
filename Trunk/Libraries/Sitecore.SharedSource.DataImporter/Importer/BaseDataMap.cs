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

namespace Sitecore.SharedSource.DataImporter.Importer {
	public abstract class BaseDataMap {
		
		#region Properties

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
					char[] comSplitr = { ',' };
					_NameFields = this.ItemNameDataField.Split(comSplitr);
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

		private List<BaseField> _fieldDefinitions = new List<BaseField>();
		public List<BaseField> FieldDefinitions {
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

		#endregion Properties

		#region Constructor

		public BaseDataMap(Database db, Item importItem) {

			SitecoreDB = db;
			
			SitecoreParentItemPath = SitecoreDB.Items[importItem.Fields["Import To Where"].Value].Paths.Path;
			Parent = SitecoreDB.GetItem(this.SitecoreParentItemPath);
			string SitecoreTemplate = SitecoreDB.Items[importItem.Fields["Import To What Template"].Value].Paths.Path.Replace("/sitecore/templates/", "");
			NewItemTemplate = SitecoreDB.Templates[SitecoreTemplate];
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
				foreach (Item child in Fields.Children) {
					//create an item to get the class / assembly name from
					BaseMapping bm = new BaseMapping(child);
					//create the object from the class and cast as base field to add it to field definitions
					BaseField bf = (BaseField)Sitecore.Reflection.ReflectionUtil.CreateObject(bm.HandlerAssembly, bm.HandlerClass, new object[] { child });
					FieldDefinitions.Add(bf);
				}
			}

			Item Props = importItem.Children["Properties"];
			if (Props.IsNotNull()) {
				foreach (Item child in Props.Children) {
					//create an item to get the class / assembly name from
					BaseMapping bm = new BaseMapping(child);
					//create the object from the class and cast as base field to add it to field definitions
					BaseProperty bp = (BaseProperty)Sitecore.Reflection.ReflectionUtil.CreateObject(bm.HandlerAssembly, bm.HandlerClass, new object[] { child });
					((SitecoreDataMap)this).PropertyDefinitions.Add(bp);
				}
			}
		}

		#endregion Constructor

		#region Methods

		public static string StripInvalidChars(string val) {

			val = val.Trim();
			val = val.Replace(" - ", " ");
			val = val.Replace("—", "");
			//val = val.Replace("–", "_");
			val = val.Replace("–", "");
			//val = val.Replace(" ", "-");
			val = val.Replace("-", "");
			val = val.Replace("+", "");
			val = val.Replace("!", "");
			val = val.Replace("@", "");
			val = val.Replace("#", "");
			val = val.Replace("$", "");
			val = val.Replace("%", "");
			val = val.Replace("^", "");
			val = val.Replace("*", "");
			val = val.Replace("=", "");
			val = val.Replace("<", "");
			val = val.Replace(">", "");
			val = val.Replace("&", "");
			val = val.Replace(",", "");
			val = val.Replace("/", "");
			val = val.Replace(@"\", "");
			val = val.Replace("|", "");
			val = val.Replace(";", "");
			val = val.Replace(":", "");
			val = val.Replace("\"", "");
			val = val.Replace("’", "");
			val = val.Replace("é", "e");
			val = val.Replace("(", "");
			val = val.Replace(")", "");
			val = val.Replace("]", "");
			val = val.Replace("[", "");
			val = val.Replace("}", "");
			val = val.Replace("{", "");
			val = val.Replace("'", "");
			val = val.Replace(".", "");
			val = val.Replace("?", "");
			val = val.Replace("&", "");
			val = val.Replace("`", "");
			val = val.Replace("?", "");
			val = val.Replace("“", "");
			val = val.Replace("”", "");
			val = val.Replace("‘", "");
			val = val.Replace("§", "");
			val = val.Replace("€", "");

			//Cleanup double underscores
			val = val.Replace("      ", " ");
			val = val.Replace("     ", " ");
			val = val.Replace("    ", " ");
			val = val.Replace("   ", " ");
			val = val.Replace("  ", " ");
			val = val.Replace("___", "");
			val = val.Replace("__", "");
			val = val.Replace("   ", " ");
			val = val.Replace("  ", " ");

			//Remove all underscores
			val = val.Replace("_", "");
			char[] c = new char[] { '-', '-' };
			val = val.Trim(c);

			return val.Trim();
		}

		public static string TrimText(string val, int maxLength, string endingString) {
			string strRetVal = val;
			if (val.Length > maxLength) {
				strRetVal = val.Substring(0, maxLength) + endingString;
			}

			return strRetVal;
		}

		public abstract void Process();

		public Item GetParentNode(Item importRow, string newItemName) {
			Item thisParent = Parent;
			if (this.FolderByDate) {
				DateTime date = DateTime.Now;
				if (DateTime.TryParse(importRow.Fields[this.DateField].Value, out date)) {
					thisParent = GetDateParentNode(Parent, date, this.FolderTemplate);
				}
			} else if (this.FolderByName) {
				thisParent = GetNameParentNode(Parent, newItemName.Substring(0, 1), this.FolderTemplate);
			}
			return thisParent;
		}
		
		public Item GetParentNode(DataRow importRow, string newItemName) {

			Item thisParent = Parent;
			if (this.FolderByDate) {
				DateTime date = DateTime.Now;
				if (DateTime.TryParse(importRow[this.DateField].ToString(), out date)) {
					thisParent = GetDateParentNode(Parent, date, this.FolderTemplate);
				}
			} else if (this.FolderByName) {
				thisParent = GetNameParentNode(Parent, newItemName.Substring(0, 1), this.FolderTemplate);
			}
			return thisParent;
		}

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

		public string GetNewItemName(DataRow importRow) {
			
			List<string> nameValues = new List<string>();
			foreach (string nameField in NameFields) {
				nameValues.Add(importRow[nameField].ToString());
			}
			return GetNewItemNameHelper(nameValues);
		}

		public string GetNewItemName(Item importRow) {
			
			List<string> nameValues = new List<string>();
			foreach (string nameField in NameFields) {
				nameValues.Add(importRow.Fields[nameField].Value);
			}
			return GetNewItemNameHelper(nameValues);
		}
		
		private string GetNewItemNameHelper(List<string> nameValues) {
			
			string strItemName = "";
			//if there are multiple 
			foreach (string name in nameValues) {
				strItemName += StripInvalidChars(name);
			}
			return TrimText(strItemName, this.ItemNameMaxLength, string.Empty);
		}

		#endregion Methods
	}
}
