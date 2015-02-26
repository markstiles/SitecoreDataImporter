using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.SharedSource.DataImporter.Extensions;
using Sitecore.Data.Items;
using Sitecore.Data;
using Sitecore.SharedSource.DataImporter.Mappings.Fields;
using Sitecore.SharedSource.DataImporter.Mappings.Properties;
using System.Web;
using System.Collections;
using Sitecore.SharedSource.DataImporter.Mappings;
using Sitecore.SharedSource.DataImporter.Utility;
using Sitecore.Collections;
using System.IO;
using Sitecore.Data.Fields;
using System.Configuration;
using Sitecore.Globalization;
using Sitecore.Data.Managers;
using Sitecore.SharedSource.DataImporter.Mappings.Templates;

namespace Sitecore.SharedSource.DataImporter.Providers
{
	public class SitecoreDataMap : BaseDataMap {

        #region Static IDs

        /// <summary>
        /// template id of the properties folder
        /// </summary>
        public static readonly string PropertiesFolderTemplateID = "{8452785D-FFE7-47F3-911E-F219F5BDEA3A}";

        /// <summary>
        /// template id of the templates folder
        /// </summary>
        public static readonly string TemplatesFolderTemplateID = "{3D915406-97F6-4E94-AC50-B7CAF468A50F}";

        #endregion Static IDs

		#region Properties

		private Database _FromDB;
		public Database FromDB {
			get {
				if (_FromDB == null) {
					var csNames = from ConnectionStringSettings c in ConfigurationManager.ConnectionStrings
						where c.ConnectionString.Equals(DatabaseConnectionString)
						select c.Name;
					if(!csNames.Any())
						throw new NullReferenceException("The database connection string wasn't found.");
					
					List<Database> dbs = Sitecore.Configuration.Factory.GetDatabases()
						.Where(a => a.ConnectionStringName.Equals(csNames.First()))
						.ToList();
					
					if(!dbs.Any())
						throw new NullReferenceException("No database in the Sitecore configuration using the connection string was found.");

					_FromDB = dbs.First();
				}
				return _FromDB;
			}
		}

        /// <summary>
        /// List of properties
        /// </summary>
		public List<IBaseProperty> PropertyDefinitions {
			get {
				return _propDefinitions;
			}
			set {
				_propDefinitions = value;
			}
		}
		private List<IBaseProperty> _propDefinitions = new List<IBaseProperty>();

		/// <summary>
		/// List of template mappings
		/// </summary>
		public Dictionary<string, TemplateMapping> TemplateMappingDefinitions {
			get {
				return _tempMapDefinitions;
			}
			set {
				_tempMapDefinitions = value;
			}
		}
		private Dictionary<string, TemplateMapping> _tempMapDefinitions = new Dictionary<string, TemplateMapping>();

		private Language _ImportFromLanguage;
		public Language ImportFromLanguage {
			get {
				return _ImportFromLanguage;
			}
			set {
				_ImportFromLanguage = value;
			}
		}

		private bool _RecursivelyFetchChildren;
		public bool RecursivelyFetchChildren {
			get {
				return _RecursivelyFetchChildren;
			}
			set {
				_RecursivelyFetchChildren = value;
			}
		}
		
		#endregion Properties

		#region Constructor

		public SitecoreDataMap(Database db, string connectionString, Item importItem) : base(db, connectionString, importItem) {

            //get 'from' language
            ImportFromLanguage = GetImportItemLanguage("Import From Language");

            //get recursive setting
            RecursivelyFetchChildren = GetImportItemBool("Recursively Fetch Children");
            
            //populate property definitions
            GetPropDefinitions(ImportItem);

            //populate template definitions
            GetTemplateDefinitions(ImportItem);
		}

		#endregion Constructor

        #region Override Methods

        /// <summary>
        /// uses the sitecore database and xpath query to retrieve data
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<object> GetImportData()
        {
			return FromDB.SelectItems(StringUtility.CleanXPath(this.Query));
        }

        /// <summary>
        /// deals with the sitecore properties
        /// </summary>
        /// <param name="newItem"></param>
        /// <param name="importRow"></param>
        public override void ProcessCustomData(ref Item newItem, object importRow)
        {
            Item row = importRow as Item;
            //add in the property mappings
            foreach (IBaseProperty d in this.PropertyDefinitions)
                d.FillField(this, ref newItem, row);

			//recursively get children
			if (RecursivelyFetchChildren)
				ProcessChildren(ref newItem, ref row);
        }

		protected virtual void ProcessChildren(ref Item newParent, ref Item oldParent){
			if (!oldParent.HasChildren)
				return;

			foreach (Item importRow in oldParent.GetChildren()) {
				
				string newItemName = GetNewItemName(importRow);
				if (string.IsNullOrEmpty(newItemName))
					continue;

				CreateNewItem(newParent, importRow, newItemName);
			}
		}
        
        /// <summary>
        /// gets a field value from an item
        /// </summary>
        /// <param name="importRow"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        protected override string GetFieldValue(object importRow, string fieldName)
        {
			//check for tokens
			if (fieldName.Equals("$name"))
				return ((Item)importRow).Name;

			Item item = importRow as Item;
			Item langItem = FromDB.GetItem(item.ID, ImportFromLanguage);

			Field f = langItem.Fields[fieldName];
			return (f != null) ? langItem[fieldName] : string.Empty;
        }

		public override CustomItemBase GetNewItemTemplate(object importRow) {

			Item iRow = (Item)importRow;
			string tID = iRow.TemplateID.ToString();
			if(!TemplateMappingDefinitions.ContainsKey(tID))
				return base.GetNewItemTemplate(importRow);

			TemplateMapping tm = TemplateMappingDefinitions[tID];
			BranchItem b = (BranchItem)SitecoreDB.Items[tm.ToWhatTemplate];
			return (CustomItemBase)b;
		}

        #endregion Override Methods

        #region Methods

        public void GetPropDefinitions(Item i) {
            
            //check for properties folder
            Item Props = GetItemByTemplate(i, PropertiesFolderTemplateID);
            if (Props.IsNull()) {
                Log("Warn", "there is no 'Properties' folder");
                return;
            }

            //check for any children
            if (!Props.HasChildren) {
                Log("Warn", "there are no properties to import");
                return;
            }

            ChildList c = Props.GetChildren();
            foreach (Item child in c) {
                //create an item to get the class / assembly name from
                BaseMapping bm = new BaseMapping(child);

                //check for assembly
                if (string.IsNullOrEmpty(bm.HandlerAssembly)) {
                    Log("Error", string.Format("the 'Handler Assembly' {1} is not defined for the '{0}' property", child.Name, bm.HandlerAssembly));
                    continue;
                }

                //check for class
                if (string.IsNullOrEmpty(bm.HandlerClass)) {
                    Log("Error", string.Format("the Handler Class {1} is not defined for the '{0}' property", child.Name, bm.HandlerClass));
                    continue;
                }
                 
                //create the object from the class and cast as base field to add it to field definitions
                IBaseProperty bp = null;
                try {
                    bp = (IBaseProperty)Sitecore.Reflection.ReflectionUtil.CreateObject(bm.HandlerAssembly, bm.HandlerClass, new object[] { child });
                } catch (FileNotFoundException fnfe) {
                    Log("Error", string.Format("the binary {1} specified could not be found for the '{0}' property", child.Name, bm.HandlerAssembly));
                }

                if (bp != null)
                    PropertyDefinitions.Add(bp);
                else
                    Log("Error", string.Format("the class type {1} could not be instantiated for the '{0}' property ", child.Name, bm.HandlerClass));
            }
        }

        public void GetTemplateDefinitions(Item i) {
            
            //check for templates folder
            Item Temps = GetItemByTemplate(i, TemplatesFolderTemplateID);
			if (Temps.IsNull()) {
                Log("Warn", "there is no 'Templates' folder");
                return;
            }

            //check for any children
            if (!Temps.HasChildren) {
                Log("Warn", "there are no templates mappings to import");
                return;
            }

			ChildList c = Temps.GetChildren();
			foreach (Item child in c) {
				//create an item to get the class / assembly name from
				TemplateMapping tm = new TemplateMapping(child);
				
                //check for 'from' template
                if (string.IsNullOrEmpty(tm.FromWhatTemplate)) {
					Log("Error", string.Format("the template mapping field 'FromWhatTemplate' on '{0}' is not defined", child.Name));
					continue;
				}

                //check for 'to' template
				if (string.IsNullOrEmpty(tm.ToWhatTemplate)) {
					Log("Error", string.Format("the template mapping field 'ToWhatTemplate' on '{0}' is not defined", child.Name));
					continue;
				}

				TemplateMappingDefinitions.Add(tm.FromWhatTemplate, tm);
			}
        }

        #endregion Methods
	}
}
