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
using Sitecore.SharedSource.DataImporter.Logger;

namespace Sitecore.SharedSource.DataImporter.Providers {
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
                    if (!csNames.Any())
                        throw new NullReferenceException("The database connection string wasn't found.");

                    List<Database> dbs = Sitecore.Configuration.Factory.GetDatabases()
                        .Where(a => a.ConnectionStringName.Equals(csNames.First()))
                        .ToList();

                    if (!dbs.Any())
                        throw new NullReferenceException("No database in the Sitecore configuration using the connection string was found.");

                    _FromDB = dbs.First();
                }
                return _FromDB;
            }
        }

        /// <summary>
        /// List of properties
        /// </summary>
        public List<IBaseProperty> PropertyDefinitions { get; set; }
        
        /// <summary>
        /// List of template mappings
        /// </summary>
        public Dictionary<string, TemplateMapping> TemplateMappingDefinitions { get; set; }
        
        #endregion Properties

        #region Fields

        public Language ImportFromLanguage { get; set; }

        public bool RecursivelyFetchChildren { get; set; }

        #endregion Fields

        #region Constructor

        public SitecoreDataMap(Database db, string connectionString, Item importItem, ILogger l)
            : base(db, connectionString, importItem, l) {

            //get 'from' language
            ImportFromLanguage = GetImportItemLanguage("Import From Language");

            //get recursive setting
            RecursivelyFetchChildren = ImportItem.GetItemBool("Recursively Fetch Children");

            //populate property definitions
            PropertyDefinitions = GetPropDefinitions(ImportItem);

            //populate template definitions
            TemplateMappingDefinitions = GetTemplateDefinitions(ImportItem);
        }

        #endregion Constructor

        #region IDataMap Methods

        /// <summary>
        /// uses the sitecore database and xpath query to retrieve data
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<object> GetImportData() {
            return FromDB.SelectItems(StringUtility.CleanXPath(Query));
        }

        /// <summary>
        /// deals with the sitecore properties
        /// </summary>
        /// <param name="newItem"></param>
        /// <param name="importRow"></param>
        public override void ProcessCustomData(ref Item newItem, object importRow) {
            Item row = importRow as Item;

            List<IBaseProperty> l = GetPropDefinitionsByRow(importRow);
            
            //add in the property mappings
            foreach (IBaseProperty d in l)
                d.FillField(this, ref newItem, row);

            //recursively get children
            if (RecursivelyFetchChildren)
                ProcessChildren(ref newItem, ref row);
        }

        /// <summary>
        /// gets a field value from an item
        /// </summary>
        /// <param name="importRow"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public override string GetFieldValue(object importRow, string fieldName) {
            //check for tokens
            if (fieldName.Equals("$name"))
                return ((Item)importRow).Name;

            Item item = importRow as Item;
            Item langItem = FromDB.GetItem(item.ID, ImportFromLanguage);

            Field f = langItem.Fields[fieldName];
            return (f != null) ? langItem[fieldName] : string.Empty;
        }

        public override CustomItemBase GetNewItemTemplate(object importRow) {

            TemplateMapping tm = GetTemplateMapping((Item)importRow);
            if (tm == null)
                return base.GetNewItemTemplate(importRow);

            BranchItem b = (BranchItem)ToDB.Items[tm.ToWhatTemplate];
            return (CustomItemBase)b;
        }

        /// <summary>
        /// if a template definition has custom field imports then use that before the global field definitions
        /// </summary>
        /// <param name="importRow"></param>
        /// <returns></returns>
        public override List<IBaseField> GetFieldDefinitionsByRow(object importRow) {

            List<IBaseField> l = new List<IBaseField>();

            //get the template
            TemplateMapping tm = GetTemplateMapping((Item)importRow);
            if (tm == null)
                return FieldDefinitions;

            //get the template fields
            List<IBaseField> tempFields = tm.FieldDefinitions;

            //filter duplicates in template fields from global fields
            List<string> names = tempFields.Select(a => a.Name).ToList();
            l.AddRange(tempFields);
            l.AddRange(FieldDefinitions.Where(a => !names.Contains(a.Name)));

            return l;
        }

        #endregion IDataMap Methods

        #region Methods

        protected virtual void ProcessChildren(ref Item newParent, ref Item oldParent) {
            if (!oldParent.HasChildren)
                return;

            foreach (Item importRow in oldParent.GetChildren()) {

                string newItemName = BuildNewItemName(importRow);
                if (string.IsNullOrEmpty(newItemName))
                    continue;

                CreateNewItem(newParent, importRow, newItemName);
            }
        }

        protected TemplateMapping GetTemplateMapping(Item item) {
            string tID = item.TemplateID.ToString();
            return (TemplateMappingDefinitions.ContainsKey(tID))
                ? TemplateMappingDefinitions[tID]
                : null;
        }

        protected List<IBaseProperty> GetPropDefinitionsByRow(object importRow) {
            List<IBaseProperty> l = new List<IBaseProperty>();
            TemplateMapping tm = GetTemplateMapping((Item)importRow);
            if (tm == null) 
                return PropertyDefinitions;

            //get the template fields
            List<IBaseProperty> tempProps = tm.PropertyDefinitions;

            //filter duplicates in template fields from global fields
            List<string> names = tempProps.Select(a => a.Name).ToList();
            l.AddRange(tempProps);
            l.AddRange(PropertyDefinitions.Where(a => !names.Contains(a.Name)));
            
            return l;
        }

        protected List<IBaseProperty> GetPropDefinitions(Item i) {

            List<IBaseProperty> l = new List<IBaseProperty>();

            //check for properties folder
            Item Props = i.GetChildByTemplate(PropertiesFolderTemplateID);
            if (Props.IsNull()) {
                Logger.Log("Warn", string.Format("there is no 'Properties' folder on '{0}'", i.DisplayName));
                return l;
            }

            //check for any children
            if (!Props.HasChildren) {
                Logger.Log("Warn", string.Format("there are no properties to import on '{0}'", i.DisplayName));
                return l;
            }

            ChildList c = Props.GetChildren();
            foreach (Item child in c) {
                //create an item to get the class / assembly name from
                BaseMapping bm = new BaseMapping(child);

                //check for assembly
                if (string.IsNullOrEmpty(bm.HandlerAssembly)) {
                    Logger.LogError("Error", string.Format("the 'Handler Assembly' {1} is not defined for the '{0}' property", child.Name, bm.HandlerAssembly));
                    continue;
                }

                //check for class
                if (string.IsNullOrEmpty(bm.HandlerClass)) {
                    Logger.LogError("Error", string.Format("the Handler Class {1} is not defined for the '{0}' property", child.Name, bm.HandlerClass));
                    continue;
                }

                //create the object from the class and cast as base field to add it to field definitions
                IBaseProperty bp = null;
                try {
                    bp = (IBaseProperty)Sitecore.Reflection.ReflectionUtil.CreateObject(bm.HandlerAssembly, bm.HandlerClass, new object[] { child });
                } catch (FileNotFoundException fnfe) {
                    Logger.LogError("Error", string.Format("the binary {1} specified could not be found for the '{0}' property", child.Name, bm.HandlerAssembly));
                }

                if (bp != null)
                    l.Add(bp);
                else
                    Logger.LogError("Error", string.Format("the class type {1} could not be instantiated for the '{0}' property ", child.Name, bm.HandlerClass));
            }

            return l;
        }

        protected Dictionary<string, TemplateMapping> GetTemplateDefinitions(Item i) {

            Dictionary<string, TemplateMapping> d = new Dictionary<string, TemplateMapping>();

            //check for templates folder
            Item Temps = i.GetChildByTemplate(TemplatesFolderTemplateID);
            if (Temps.IsNull()) {
                Logger.Log("Warn", string.Format("there is no 'Templates' folder on '{0}'", i.DisplayName));
                return d;
            }

            //check for any children
            if (!Temps.HasChildren) {
                Logger.Log("Warn", string.Format("there are no templates mappings to import on '{0}'", i.DisplayName));
                return d;
            }

            ChildList c = Temps.GetChildren();
            foreach (Item child in c) {
                //create an item to get the class / assembly name from
                TemplateMapping tm = new TemplateMapping(child);
                tm.FieldDefinitions = GetFieldDefinitions(child);
                tm.PropertyDefinitions = GetPropDefinitions(child);

                //check for 'from' template
                if (string.IsNullOrEmpty(tm.FromWhatTemplate)) {
                    Logger.LogError("Error", string.Format("the template mapping field 'FromWhatTemplate' on '{0}' is not defined", child.Name));
                    continue;
                }

                //check for 'to' template
                if (string.IsNullOrEmpty(tm.ToWhatTemplate)) {
                    Logger.LogError("Error", string.Format("the template mapping field 'ToWhatTemplate' on '{0}' is not defined", child.Name));
                    continue;
                }

                d.Add(tm.FromWhatTemplate, tm);
            }

            return d;
        }

        #endregion Methods
    }
}
