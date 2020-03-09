﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sitecore.SharedSource.DataImporter.Extensions;
using Sitecore.Data.Items;
using Sitecore.Data;
using Sitecore.SharedSource.DataImporter.Mappings.Fields;
using Sitecore.SharedSource.DataImporter.Mappings.Properties;
using Sitecore.SharedSource.DataImporter.Mappings;
using Sitecore.Collections;
using System.IO;
using Sitecore.Data.Fields;
using System.Configuration;
using Sitecore.Globalization;
using Sitecore.Data.Managers;
using Sitecore.Layouts;
using Sitecore.SharedSource.DataImporter.Mappings.Templates;
using Sitecore.SharedSource.DataImporter.Logger;
using Sitecore.SharedSource.DataImporter.Mappings.ReferenceFields;
using Sitecore.SharedSource.DataImporter.Comparers;

namespace Sitecore.SharedSource.DataImporter.Providers
{
	public class SitecoreDataMap : BaseDataMap
	{

		#region Static IDs

		/// <summary>
		/// template id of the properties folder
		/// </summary>
		public static readonly string PropertiesFolderTemplateID = "{8452785D-FFE7-47F3-911E-F219F5BDEA3A}";

		/// <summary>
		/// template id of the templates folder
		/// </summary>
		public static readonly string TemplatesFolderTemplateID = "{3D915406-97F6-4E94-AC50-B7CAF468A50F}";

		/// <summary>
		/// template id of the components folder
		/// </summary>
		public static readonly string ComponentsFolderTemplateID = "{4E8E2F3D-2327-4BBA-A14F-C586391892CA}";

		#endregion Static IDs

		#region Properties

		private Database _FromDB;

		public Database FromDB
		{
			get
			{
				if (_FromDB == null)
				{
					var csNames = from ConnectionStringSettings c in ConfigurationManager.ConnectionStrings
												where c.ConnectionString.Equals(DatabaseConnectionString)
												select c.Name;
					if (!csNames.Any())
						throw new NullReferenceException("The database connection string wasn't found.");

					List<Database> dbs = Sitecore.Configuration.Factory.GetDatabases()
						.Where(a => a.ConnectionStringName.Equals(csNames.First()))
						.ToList();

					if (!dbs.Any())
						throw new NullReferenceException(
							"No database in the Sitecore configuration using the connection string was found.");

					_FromDB = dbs.First();
				}

				return _FromDB;
			}
		}

		private DeviceItem _defaultDevice;

		public DeviceItem DefaultDevice
		{
			get
			{
				if (_defaultDevice == null)
				{
					_defaultDevice = ToDB.GetItem("{FE5D7FDF-89C0-4D99-9AA3-B5FBD009C9F3}");
				}

				return _defaultDevice;
			}
		}

		/// <summary>
		/// List of properties
		/// </summary>
		public List<IBaseProperty> PropertyDefinitions { get; set; }

		/// <summary>
		/// List of properties
		/// </summary>
		public List<IBaseFieldWithReference> ReferenceFieldDefinitions { get; set; }

		/// <summary>
		/// List of template mappings
		/// </summary>
		public Dictionary<string, TemplateMapping> TemplateMappingDefinitions { get; set; }

		/// <summary>
		/// List of template mappings
		/// </summary>
		public IList<ComponentMapping> ComponentMappingDefinitions { get; set; }

		public Item ImportRoot { get; set; }
		public bool DeleteOnOverwrite { get; set; }

		public bool SkipIfExists { get; set; }
		public DateTime SkipIfUpdatedAfter { get; set; }

		#endregion Properties

		#region Fields

		public Language ImportFromLanguage { get; set; }

		public bool RecursivelyFetchChildren { get; set; }

		public Language[] AllLanguages { get; set; }

		#endregion Fields

		#region Constructor

		public SitecoreDataMap(Database db, string connectionString, Item importItem, ILogger l)
			: base(db, connectionString, importItem, l)
		{

			//get 'from' language
			ImportFromLanguage = GetImportItemLanguage("Import From Language", FromDB);

			//get recursive setting
			RecursivelyFetchChildren = ImportItem.GetItemBool("Recursively Fetch Children");

			//populate property definitions
			PropertyDefinitions = GetPropDefinitions(ImportItem);

			ReferenceFieldDefinitions = GetReferenceFieldDefinitions(importItem);

			//populate template definitions
			TemplateMappingDefinitions = GetTemplateDefinitions(ImportItem);

			ComponentMappingDefinitions = GetComponentDefinitions(ImportItem);

			ImportRoot = GetImportRootItem();
			DeleteOnOverwrite = ImportItem.GetItemBool("Delete On Overwrite");

			AllLanguages = LanguageManager.GetLanguages(ToDB).ToArray();

			SkipIfExists = importItem.GetItemBool("Skip Already Imported Items");
			SkipIfUpdatedAfter = importItem.GetItemDate("Skip If Updated After");
		}

		#endregion Constructor

		#region Constructor Helpers

		public Item GetImportRootItem()
		{

			Item toWhere = null;

			//check field value
			string toWhereID = ImportItem.GetItemField("Import Root", Logger);
			if (string.IsNullOrEmpty(toWhereID))
			{
				Logger.Log("the 'Import Root' field is not set on the import item",
                    ImportItem.Paths.FullPath, ProcessStatus.ImportDefinitionError, "Import To Where");
				return null;
			}

			//check item
			toWhere = FromDB.Items[toWhereID];
			if (toWhere.IsNull())
				Logger.Log("the 'Import Root' item is null on the import item",
                    ImportItem.Paths.FullPath, ProcessStatus.ImportDefinitionError, "Import Root", toWhereID);

			return toWhere;
		}

		#endregion Constructor Helpers

		#region IDataMap Methods

		/// <summary>
		/// uses the sitecore database and xpath query to retrieve data
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<object> GetImportData()
		{
			var items = new List<object>();
			foreach (var query in Query.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
			{
				IEnumerable<Item> queriedItems = FromDB.SelectItems(query);

				if (SkipIfExists)
					queriedItems = queriedItems.Where(i => ToDB.GetItem(i.ID) == null);
				
				items.AddRange(queriedItems);

                foreach (var i in queriedItems)
                {
                    items.Add(i);
                    if (!RecursivelyFetchChildren)
                        continue;
                    
                    items.AddRange(i.Axes.GetDescendants());
                }
            }

			return items;
		}

		/// <summary>
		/// deals with the sitecore properties
		/// </summary>
		/// <param name="newItem"></param>
		/// <param name="importRow"></param>
		/// <param name="propDefinitions"></param>
		public override void ProcessCustomData(ref Item newItem, object importRow, List<IBaseProperty> propDefinitions = null)
		{
			Item row = importRow as Item;

			List<IBaseProperty> l = propDefinitions ?? GetPropDefinitionsByRow(importRow);

			//add in the property mappings
			foreach (IBaseProperty d in l)
				d.FillField(this, ref newItem, row);
		}

		/// <summary>
		/// deals with the sitecore properties
		/// </summary>
		/// <param name="newItem"></param>
		/// <param name="importRow"></param>
		/// <param name="referenceFields"></param>
		public void ProcessReferenceFields(ref Item newItem, object importRow, List<IBaseFieldWithReference> referenceFields = null)
		{
			Item row = importRow as Item;

			List<IBaseFieldWithReference> l = referenceFields ?? GetReferenceFieldDefinitionsByRow(importRow);

			//add in the property mappings
			foreach (IBaseFieldWithReference d in l)
			{
				var fieldName = d.GetExistingFieldName();
				d.FillField(this, ref newItem, row, fieldName);
			}
		}

		/// <summary>
		/// gets a field value from an item
		/// </summary>
		/// <param name="importRow"></param>
		/// <param name="fieldName"></param>
		/// <returns></returns>
		public override string GetFieldValue(object importRow, string fieldName)
		{
			//check for tokens
			if (fieldName.Equals("$name"))
				return ((Item)importRow).Name;

			if (fieldName.Equals("$parentname"))
				return ((Item)importRow).Parent.Name;

			Item item = importRow as Item;
			
			Field f = item.Fields[fieldName];
			return (f != null) ? item[fieldName] : string.Empty;
		}

		public override CustomItemBase GetNewItemTemplate(object importRow)
		{

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
		public override List<IBaseField> GetFieldDefinitionsByRow(object importRow)
		{

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
        
		private Item GetLastPublishableVersion(Item oldItem, Language oldLanguage)
		{
			var oldLangItem = FromDB.GetItem(oldItem.ID, oldLanguage);
			
			string wfIdString = oldLangItem.Fields[FieldIDs.WorkflowState].Value;
			if (!string.IsNullOrEmpty(wfIdString))
			{
				Item wftarget = FromDB.GetItem(wfIdString);
				if (wftarget?.Fields[WorkflowFieldIDs.FinalState]?.Value == "1")
				{
					return oldLangItem;
				}
			}
			else
			{
				return oldLangItem;
			}

			foreach (Item oldVersion in oldLangItem.Versions.GetOlderVersions().Reverse().Skip(1))
			{
				string oldWfIdString = oldVersion.Fields[FieldIDs.WorkflowState].Value;
				if (!string.IsNullOrEmpty(oldWfIdString))
				{
					Item wftarget = FromDB.GetItem(oldWfIdString);
					if (wftarget?.Fields[WorkflowFieldIDs.FinalState]?.Value == "1")
					{
						return oldVersion;
					}
				}
			}

			return oldLangItem;
		}

		public override Item CreateNewItem(Item parent, object importRow, string newItemName)
		{
			CustomItemBase nItemTemplate = GetNewItemTemplate(importRow);

			var oldItem = (Item)importRow;
            Item newItem = null;

            foreach (var language in oldItem.Languages)
			{
				ImportToLanguage =
					AllLanguages.FirstOrDefault(l => l.Name.StartsWith(language.Name, StringComparison.InvariantCultureIgnoreCase));
				ImportFromLanguage = language;

				var oldLangItem = GetLastPublishableVersion(oldItem, ImportFromLanguage);

				if (oldLangItem.Versions.Count <= 0) continue;

				using (new LanguageSwitcher(ImportToLanguage))
				{
					//get the parent in the specific language
					parent = ToDB.GetItem(parent.ID);

					newItem = ToDB.GetItem(oldLangItem.ID);
					if (newItem == null || SkipIfUpdatedAfter == DateTime.MinValue || newItem.Statistics.Updated < SkipIfUpdatedAfter)
					{
						if (newItem != null) //add version for lang
						{
							if (DeleteOnOverwrite || newItem.Version.Number > 2)
							{
								newItem.Versions.RemoveAll(false);
							}
							if (newItem.Versions.Count == 0)
							{
								newItem = newItem.Versions.AddVersion();
							}
							else if (!oldLangItem.Versions.IsLatestVersion())
							{
								newItem = newItem.Versions.GetVersions().FirstOrDefault();
							}
						}

						//if not found then create one
						if (newItem == null)
						{
							newItem = ItemManager.AddFromTemplate(newItemName, nItemTemplate.ID, parent, oldItem.ID);
						}

						if (newItem == null)
							throw new NullReferenceException("the new item created was null");

                        newItem.Editing.BeginEdit();
                        //if found and is default template, change it to the correct one
						if (ImportToWhatTemplate != null &&
							newItem.TemplateID == ImportToWhatTemplate.ID &&
							nItemTemplate.ID != ImportToWhatTemplate.ID &&
							!(nItemTemplate is BranchItem))
						{
							newItem.ChangeTemplate(new TemplateItem(nItemTemplate.InnerItem));
						}

						ProcessFields(oldLangItem, ref newItem);

						ProcessReferenceFields(ref newItem, oldLangItem);
                        
						ProcessCustomData(ref newItem, oldLangItem);

                        newItem.Editing.EndEdit(false, false);

                        newItem.Database.Caches.ItemCache.RemoveItem(newItem.ID);
                    }
					else if(SkipIfUpdatedAfter > DateTime.MinValue && newItem.Statistics.Updated > SkipIfUpdatedAfter)
					{
						Logger.Log("Item has been updated since last migration, skipping...", newItem.Paths.FullPath, ProcessStatus.Info, $"Language: {newItem.Language.Name}", $"Version: {newItem.Version.Number}");
					}

					// Now for draft versions
					if (!oldLangItem.Versions.IsLatestVersion())
					{
						oldLangItem = oldLangItem.Versions.GetLatestVersion(oldLangItem.Language);

						if (newItem.Versions.Count == 1)
						{
							newItem = newItem.Versions.AddVersion();
						}
						else
						{
							newItem = newItem.Versions.GetLatestVersion(newItem.Language);

							if (SkipIfUpdatedAfter > DateTime.MinValue && newItem.Statistics.Updated > SkipIfUpdatedAfter)
							{
								Logger.Log("Item has been updated since last migration, skipping...", newItem.Paths.FullPath, ProcessStatus.Info, $"Language: {newItem.Language.Name}", $"Version: {newItem.Version.Number}");
								continue;
							}
                        }
						
                        newItem.Editing.BeginEdit();
                        //if found and is default template, change it to the correct one
						if (newItem.TemplateID == ImportToWhatTemplate.ID && nItemTemplate.ID != ImportToWhatTemplate.ID &&
							!(nItemTemplate is BranchItem))
						{
							newItem.ChangeTemplate(new TemplateItem(nItemTemplate.InnerItem));
						}

						ProcessFields(oldLangItem, ref newItem);

						ProcessReferenceFields(ref newItem, oldLangItem);
                        
						ProcessCustomData(ref newItem, oldLangItem);
                        newItem.Editing.EndEdit(false, false);

                        newItem.Database.Caches.ItemCache.RemoveItem(newItem.ID);
                    }
				}
			}

            return newItem;
        }
        
		protected void ProcessFields(object importRow, ref Item newItem)
		{
			//add in the field mappings
			List<IBaseField> fieldDefs = GetFieldDefinitionsByRow(importRow);
			ProcessFields(importRow, ref newItem, fieldDefs);
		}

		protected void ProcessFields(object importRow, ref Item newItem, List<IBaseField> fieldDefs)
		{
			//add in the field mappings
			foreach (IBaseField d in fieldDefs)
			{
				string importValue = string.Empty;
				try
				{
					var fieldNames = d.GetExistingFieldNames();

					IEnumerable<string> values = d is ToRichText ? GetRTEFieldValues(fieldNames,importRow) : GetFieldValues(fieldNames, importRow);

					importValue = String.Join(d.GetFieldValueDelimiter(), values.Where(v => !string.IsNullOrEmpty(v)));
					d.FillField(this, ref newItem, importRow, importValue);
				}
				catch (Exception ex)
				{
					Logger.Log($"the FillField failed: {ex}", newItem.Paths.FullPath, ProcessStatus.FieldError, d.ItemName(), importValue);
					Diagnostics.Log.Error(ex.Message, ex, this);
				}
			}
		}

		protected TemplateMapping GetTemplateMapping(Item item)
		{
			string tID = item.TemplateID.ToString();
			return (TemplateMappingDefinitions.ContainsKey(tID))
					? TemplateMappingDefinitions[tID]
					: null;
		}

		protected List<IBaseProperty> GetPropDefinitionsByRow(object importRow)
		{
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

		protected List<IBaseFieldWithReference> GetReferenceFieldDefinitionsByRow(object importRow)
		{
			List<IBaseFieldWithReference> l = new List<IBaseFieldWithReference>();
			TemplateMapping tm = GetTemplateMapping((Item)importRow);
			if (tm == null)
				return ReferenceFieldDefinitions;

			//get the template fields
			List<IBaseFieldWithReference> tempProps = tm.ReferenceFieldDefinitions;

			//filter duplicates in template fields from global fields
			List<string> names = tempProps.Select(a => a.Name).ToList();
			l.AddRange(tempProps);
			l.AddRange(ReferenceFieldDefinitions.Where(a => !names.Contains(a.Name)));

			return l;
		}

		protected List<IBaseProperty> GetPropDefinitions(Item i)
		{

			List<IBaseProperty> l = new List<IBaseProperty>();

			//check for properties folder
			Item Props = i.GetChildByTemplate(PropertiesFolderTemplateID);
			if (Props.IsNull())
			{
				Logger.Log($"there is no 'Properties' folder on '{i.DisplayName}'", i.Paths.FullPath);
				return l;
			}

			//check for any children
			if (!Props.HasChildren)
			{
				Logger.Log($"there are no properties to import on '{i.DisplayName}'", i.Paths.FullPath);
				return l;
			}

			ChildList c = Props.GetChildren();
			foreach (Item child in c)
			{
				//create an item to get the class / assembly name from
				BaseMapping bm = new BaseMapping(child, Logger);

				//check for assembly
				if (string.IsNullOrEmpty(bm.HandlerAssembly))
				{
					Logger.Log("the 'Handler Assembly' is not defined", child.Paths.FullPath, ProcessStatus.ImportDefinitionError, "Handler Assembly", bm.HandlerAssembly);
					continue;
				}

				//check for class
				if (string.IsNullOrEmpty(bm.HandlerClass))
				{
					Logger.Log("the Handler Class is not defined", child.Paths.FullPath, ProcessStatus.ImportDefinitionError, "Handler Class", bm.HandlerClass);
					continue;
				}

				//create the object from the class and cast as base field to add it to field definitions
				IBaseProperty bp = null;
				try
				{
					bp = (IBaseProperty)Sitecore.Reflection.ReflectionUtil.CreateObject(bm.HandlerAssembly, bm.HandlerClass, new object[] { child, Logger });
				}
				catch (FileNotFoundException)
				{
					Logger.Log("the binary could not be found", child.Paths.FullPath, ProcessStatus.ImportDefinitionError, "Handler Assembly", bm.HandlerAssembly);
				}

				if (bp != null)
					l.Add(bp);
				else
					Logger.Log("the class type could not be instantiated", child.Paths.FullPath, ProcessStatus.ImportDefinitionError, "Handler Class", bm.HandlerClass);
			}

			return l;
		}

		protected Dictionary<string, TemplateMapping> GetTemplateDefinitions(Item i)
		{

			Dictionary<string, TemplateMapping> d = new Dictionary<string, TemplateMapping>();

			//check for templates folder
			Item Temps = i.GetChildByTemplate(TemplatesFolderTemplateID);
			if (Temps.IsNull())
			{
				Logger.Log($"there is no 'Templates' folder on '{i.DisplayName}'", i.Paths.FullPath);
				return d;
			}

			//check for any children
			if (!Temps.HasChildren)
			{
				Logger.Log($"there are no templates mappings to import on '{i.DisplayName}'", i.Paths.FullPath);
				return d;
			}

			ChildList c = Temps.GetChildren();
			foreach (Item child in c)
			{
				//create an item to get the class / assembly name from
				TemplateMapping tm = new TemplateMapping(child);
				tm.FieldDefinitions = GetFieldDefinitions(child);
				tm.PropertyDefinitions = GetPropDefinitions(child);
				tm.ReferenceFieldDefinitions = GetReferenceFieldDefinitions(child);

				//check for 'from' template
				if (string.IsNullOrEmpty(tm.FromWhatTemplate))
				{
					Logger.Log("the template mapping field 'FromWhatTemplate' is not defined", child.Paths.FullPath, ProcessStatus.ImportDefinitionError, "From What Template");
					continue;
				}

				//check for 'to' template
				if (string.IsNullOrEmpty(tm.ToWhatTemplate))
				{
					Logger.Log("the template mapping field 'ToWhatTemplate' is not defined", child.Paths.FullPath, ProcessStatus.ImportDefinitionError, "To What Template");
					continue;
				}

				d.Add(tm.FromWhatTemplate, tm);
			}

			return d;
		}

		protected IList<ComponentMapping> GetComponentDefinitions(Item i)
		{
			List<ComponentMapping> d = new List<ComponentMapping>();

			//check for templates folder
			Item temps = i.GetChildByTemplate(ComponentsFolderTemplateID);
			if (temps.IsNull())
			{
				Logger.Log($"there is no 'Components' folder on '{i.DisplayName}'", i.Paths.FullPath);
				return d;
			}

			//check for any children
			if (!temps.HasChildren)
			{
				Logger.Log($"there are no component mappings to import on '{i.DisplayName}'", i.Paths.FullPath);
				return d;
			}

			ChildList c = temps.Children;
			foreach (Item child in c)
			{
				//create an item to get the class / assembly name from
				ComponentMapping tm = new ComponentMapping(child);
				tm.FieldDefinitions = GetFieldDefinitions(child);
				tm.PropertyDefinitions = GetPropDefinitions(child);
				tm.ReferenceFieldDefinitions = GetReferenceFieldDefinitions(child);

				d.Add(tm);
			}

			return d;
		}

		public override Item GetParentNode(object importRow, string newItemName)
		{
			var item = FolderByDate ? GetParentNodeBasedOnLegacy(importRow, newItemName) : base.GetParentNode(importRow, newItemName);

            var path = ((Item)importRow).Paths.Path;
			path = path.Replace(ImportRoot.Paths.Path, ImportToWhere.Paths.Path);
			path = path.Substring(0, path.LastIndexOf("/"));
			item = GetPathParentNode(path);
			
			return item;
		}

		protected virtual Item GetParentNodeBasedOnLegacy(object importRow, string newItemName)
		{
			Item legacyItem = (Item)importRow;
			return GetDateParentNodeBasedOnLegacy(ImportToWhere, legacyItem.Paths.ContentPath, FolderTemplate);
		}

		protected Item GetDateParentNodeBasedOnLegacy(Item parentNode, string legacyPath, TemplateItem folderType)
		{
			string[] pathParts = legacyPath.Split('/').Where(p => StringService.IsInt(p) || p.StartsWith("hip", StringComparison.InvariantCultureIgnoreCase) || p.Equals("legacy", StringComparison.InvariantCultureIgnoreCase)).ToArray();
			if (pathParts.Any())
			{
				//get year folder
				Item year = GetChild(parentNode, pathParts[0]);
				if (year == null)
				{
					//build year folder if you have to
					year = parentNode.Add(pathParts[0], folderType);
				}
				//set the parent to year
				parentNode = year;
			}

			if (pathParts.Length > 1)
			{
				//get month folder
				Item month = GetChild(parentNode, pathParts[1]);
				if (month == null)
				{
					//build month folder if you have to
					month = parentNode.Add(pathParts[1], folderType);
				}
				//set the parent to year
				parentNode = month;
			}

			if (pathParts.Length > 2)
			{
				//get day folder
				Item day = GetChild(parentNode, pathParts[2]);
				if (day == null)
				{
					//build day folder if you have to
					day = parentNode.Add(pathParts[2], folderType);
				}
				//set the parent to year
				parentNode = day;
			}

			return parentNode;
		}

		protected virtual Item GetPathParentNode(string path)
		{
			var item = ToDB.GetItem(path);
			if (item == null)
			{
				var parentPath = path.Substring(0, path.LastIndexOf("/"));
				var itemName = path.Substring(path.LastIndexOf("/") + 1);
				var parent = GetPathParentNode(parentPath);
				item = parent.Add(itemName, new TemplateID(ImportToWhatTemplate.ID));
			}
			return item;
		}
        
		#endregion Methods
	}
}
