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
		
		#region Properties
		
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
		/// template id of the properties folder
		/// </summary>
		public static readonly string PropertiesFolderID = "{8452785D-FFE7-47F3-911E-F219F5BDEA3A}";

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

		/// <summary>
		/// template id of the templates folder
		/// </summary>
		public static readonly string TemplatesFolderID = "{3D915406-97F6-4E94-AC50-B7CAF468A50F}";

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

			Item fLang = SitecoreDB.GetItem(importItem.Fields["Import From Language"].Value);
			ImportFromLanguage = LanguageManager.GetLanguage(fLang.Name);

			CheckboxField cf = importItem.Fields["Recursively Fetch Children"];
			RecursivelyFetchChildren = cf.Checked;

            //deal with sitecore properties if any
            Item Props = GetItemByTemplate(importItem, PropertiesFolderID);
            if (Props.IsNotNull()) {
                ChildList c = Props.GetChildren();
                if (c.Any()) {
                    foreach (Item child in c) {
                        //create an item to get the class / assembly name from
                        BaseMapping bm = new BaseMapping(child);
                        if (!string.IsNullOrEmpty(bm.HandlerAssembly)) {
                            if (!string.IsNullOrEmpty(bm.HandlerClass)) {
                                //create the object from the class and cast as base field to add it to field definitions
                                IBaseProperty bp = null;
                                try {
                                    bp = (IBaseProperty)Sitecore.Reflection.ReflectionUtil.CreateObject(bm.HandlerAssembly, bm.HandlerClass, new object[] { child });
                                } catch (FileNotFoundException fnfe) {
                                    Log("Error", string.Format("the property:{0} binary {1} specified could not be found", child.Name, bm.HandlerAssembly));
                                }
                                if (bp != null)
                                    PropertyDefinitions.Add(bp);
                                else
                                    Log("Error", string.Format("the property: '{0}' class type {1} could not be instantiated", child.Name, bm.HandlerClass));
                            } else {
                                Log("Error", string.Format("the property: '{0}' Handler Class {1} is not defined", child.Name, bm.HandlerClass));
                            }
                        } else {
                            Log("Error", string.Format("the property: '{0}' Handler Assembly {1} is not defined", child.Name, bm.HandlerAssembly));
                        }
                    }
                } else {
                    Log("Warn", "there are no properties to import");
                }
            }

			Item Temps = GetItemByTemplate(importItem, TemplatesFolderID);
			if (Temps.IsNotNull()) {
				ChildList c = Temps.GetChildren();
				if (c.Any()) {
					foreach (Item child in c) {
						//create an item to get the class / assembly name from
						TemplateMapping tm = new TemplateMapping(child);
						if (string.IsNullOrEmpty(tm.FromWhatTemplate)) {
							Log("Error", string.Format("the template mapping field 'FromWhatTemplate' on '{0}' is not defined", child.Name));
							break;
						}
						if (string.IsNullOrEmpty(tm.ToWhatTemplate)) {
							Log("Error", string.Format("the template mapping field 'ToWhatTemplate' on '{0}' is not defined", child.Name));
							break;
						}
						TemplateMappingDefinitions.Add(tm.FromWhatTemplate, tm);
					}
				}
			}
		}

		#endregion Constructor

        #region Override Methods

        /// <summary>
        /// uses the sitecore database and xpath query to retrieve data
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<object> GetImportData()
        {
			var csNames = from ConnectionStringSettings c in ConfigurationManager.ConnectionStrings
					where c.ConnectionString.Equals(DatabaseConnectionString)
					select c.Name;
			if(!csNames.Any()) {
				Log("Warn", "The connection string wasn't found.");
				return Enumerable.Empty<object>();
			}
			
			List<Database> dbs = Sitecore.Configuration.Factory.GetDatabases().Where(a => a.ConnectionStringName.Equals(csNames.First())).ToList();
			if(!dbs.Any()) {
				Log("Warn", "No database in the Sitecore configuration using the connection string was found.");
				return Enumerable.Empty<object>();
			}
			return dbs.First().SelectItems(StringUtility.CleanXPath(this.Query));
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
			Item item = importRow as Item;
			Item langItem = SitecoreDB.GetItem(item.ID, ImportFromLanguage);

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

        #endregion Methods
	}
}
