using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sitecore.SharedSource.DataImporter.Extensions;
using Sitecore.Data.Items;
using Sitecore.Data;
using Sitecore.SharedSource.DataImporter.Mappings.Fields;
using Sitecore.Collections;
using Sitecore.Data.Fields;
using System.Configuration;
using Sitecore.Globalization;
using Sitecore.Data.Managers;
using Sitecore.SharedSource.DataImporter.Mappings.Templates;
using Sitecore.SharedSource.DataImporter.Logger;
using Sitecore.SharedSource.DataImporter.Services;

namespace Sitecore.SharedSource.DataImporter.Providers
{
	public class SitecoreDataMap : BaseDataMap
	{
		#region Static IDs
        
		public static readonly string TemplatesFolderTemplateID = "{3D915406-97F6-4E94-AC50-B7CAF468A50F}";
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
		public Dictionary<string, TemplateMapping> TemplateMappings { get; set; }
		public List<IComponentMapping> ComponentMappings { get; set; }
		public Item ImportRoot { get; set; }
		public bool DeleteOnOverwrite { get; set; }
		public bool SkipIfExists { get; set; }
		public DateTime SkipIfUpdatedAfter { get; set; }
        public PresentationService PresentationService { get; set; }
        public Language ImportFromLanguage { get; set; }
		public bool RecursivelyFetchChildren { get; set; }
		public Language[] AllLanguages { get; set; }

		#endregion 

		#region Constructor

		public SitecoreDataMap(Database db, string connectionString, Item importItem, ILogger l)
			: base(db, connectionString, importItem, l)
		{

			//get 'from' language
			ImportFromLanguage = GetImportItemLanguage("Import From Language", FromDB);

			//get recursive setting
			RecursivelyFetchChildren = ImportItem.GetItemBool("Recursively Fetch Children");
            
			//populate template definitions
			TemplateMappings = GetTemplateDefinitions(ImportItem);

			ComponentMappings = GetComponentDefinitions(ImportItem);

			ImportRoot = GetImportRootItem();
			DeleteOnOverwrite = ImportItem.GetItemBool("Delete On Overwrite");

			AllLanguages = LanguageManager.GetLanguages(ToDB).ToArray();

			SkipIfExists = importItem.GetItemBool("Skip Already Imported Items");
			SkipIfUpdatedAfter = importItem.GetItemDate("Skip If Updated After");

            PresentationService = new PresentationService(l);
        }

		#endregion 

		#region Constructor Helpers

		public Item GetImportRootItem()
		{

			Item toWhere = null;

			//check field value
			string toWhereID = ImportItem.GetItemField("Import Root", Logger);
			if (string.IsNullOrEmpty(toWhereID))
			{
				Logger.Log("the 'Import Root' field is not set on the import item",
                    ImportItem.Paths.FullPath, LogType.ImportDefinitionError, "Import To Where");
				return null;
			}

			//check item
			toWhere = FromDB.Items[toWhereID];
			if (toWhere.IsNull())
				Logger.Log("the 'Import Root' item is null on the import item",
                    ImportItem.Paths.FullPath, LogType.ImportDefinitionError, "Import Root", toWhereID);

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
			var oldItem = (Item)importRow;
            Item newItem = null;

            foreach (var language in oldItem.Languages)
			{
				ImportToLanguage = AllLanguages.FirstOrDefault(l => l.Name.StartsWith(language.Name, StringComparison.InvariantCultureIgnoreCase));
				ImportFromLanguage = language;

				var oldLangItem = GetLastPublishableVersion(oldItem, ImportFromLanguage);
				if (oldLangItem.Versions.Count <= 0)
                    continue;

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
                            var nItemTemplate = GetNewItemTemplate(importRow);
                            newItem = ItemManager.AddFromTemplate(newItemName, nItemTemplate.ID, parent, oldItem.ID);

                            newItem.Editing.BeginEdit();
                            ProcessComponents(newItem, oldItem);
                            newItem.Editing.EndEdit(false, false);
                            newItem.Database.Caches.ItemCache.RemoveItem(newItem.ID);
                        }

						if (newItem == null)
							throw new NullReferenceException("the new item created was null");
                    }
					else if(SkipIfUpdatedAfter > DateTime.MinValue && newItem.Statistics.Updated > SkipIfUpdatedAfter)
					{
						Logger.Log("Item has been updated since last migration, skipping...", newItem.Paths.FullPath, LogType.Info, $"Language: {newItem.Language.Name}", $"Version: {newItem.Version.Number}");
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
								Logger.Log("Item has been updated since last migration, skipping...", newItem.Paths.FullPath, LogType.Info, $"Language: {newItem.Language.Name}", $"Version: {newItem.Version.Number}");
								continue;
							}
                        }
                    }
				}
			}
            
            return newItem;
        }

		protected TemplateMapping GetTemplateMapping(Item item)
		{
			string tID = item.TemplateID.ToString();
			return (TemplateMappings.ContainsKey(tID))
					? TemplateMappings[tID]
					: null;
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
                tm.ComponentMappings = GetComponentDefinitions(child);

                //check for 'from' template
                if (string.IsNullOrEmpty(tm.FromWhatTemplate))
				{
					Logger.Log("the template mapping field 'FromWhatTemplate' is not defined", child.Paths.FullPath, LogType.ImportDefinitionError, "From What Template");
					continue;
				}

				//check for 'to' template
				if (string.IsNullOrEmpty(tm.ToWhatTemplate))
				{
					Logger.Log("the template mapping field 'ToWhatTemplate' is not defined", child.Paths.FullPath, LogType.ImportDefinitionError, "To What Template");
					continue;
				}

				d.Add(tm.FromWhatTemplate, tm);
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

		protected List<IComponentMapping> GetComponentDefinitions(Item i)
		{
			List<IComponentMapping> d = new List<IComponentMapping>();

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
				ComponentMapping cm = new ComponentMapping(child);
				cm.FieldDefinitions = GetFieldDefinitions(child);

				d.Add(cm);
			}

            return d;
		}

        public void ProcessComponents(Item newItem, Item importItem)
        {
            var components = new List<IComponentMapping>();
            components.AddRange(ComponentMappings);
            
            TemplateMapping tm = GetTemplateMapping(newItem);
            if (tm != null)
                components.AddRange(tm.ComponentMappings);
            
            foreach (var cm in components)
            {
                using (new LanguageSwitcher(ImportToLanguage))
                {
                    try
                    {
                        var dsName = cm.ToDatasourcePath.Contains("/")
                            ? cm.ToDatasourcePath.Substring(cm.ToDatasourcePath.LastIndexOf("/") + 1)
                            : cm.ToDatasourcePath;

                        var oldDeviceDef = PresentationService.FindDeviceDefinition(importItem, cm.FromDevice);
                        if (oldDeviceDef == null)
                            continue;

                        var oldRendering = PresentationService.FindRendering(oldDeviceDef, cm.FromPlaceholder, cm.FromComponent);
                        if (oldRendering == null)
                        {
                            Logger.Log($"There was no rendering matching device:{cm.FromDevice} - placeholder:{cm.FromPlaceholder} - component:{cm.FromComponent}", importItem.Paths.FullPath, LogType.MultilistToComponent, "device xml", oldDeviceDef.ToXml());
                            continue;
                        }

                        var oldDatasource = PresentationService.FindDatasource(this, importItem, oldRendering);
                        if (oldDatasource == null)
                        {
                            Logger.Log($"There was no datasource found matching name:{dsName}", importItem.Paths.FullPath, LogType.MultilistToComponent, "rendering xml", oldRendering.ToXml());
                            continue;
                        }

                        var newDeviceDef = PresentationService.FindDeviceDefinition(newItem, cm.ToDevice);
                        if (newDeviceDef == null)
                            continue;

                        if (!cm.OverwriteExisting)
                        {
                            var createDatasource = !string.IsNullOrWhiteSpace(cm.ToDatasourcePath);
                            var newDatasource = createDatasource 
                                ? PresentationService.CreateDatasource(this, newItem, newDeviceDef, dsName, cm.ToDatasourceFolder, cm.ToDatasourcePath, cm.ToComponent, cm.OverwriteExisting)
                                : null;
                            if (newDatasource == null && createDatasource)
                            {
                                Logger.Log($"There was no datasource created for device:{cm.ToDevice} - placeholder:{cm.ToPlaceholder} - component:{cm.ToComponent}", newItem.Paths.FullPath, LogType.MultilistToComponent, "device xml", newDeviceDef.ToXml());    
                                continue;
                            }

                            SetDatasourceFields(oldDatasource, newDatasource, cm);
                            PresentationService.AddComponent(newItem, newDatasource, cm.ToPlaceholder, cm.ToComponent, cm.ToDevice, cm.ToParameters, cm.IsSXA);
                        }
                        else
                        {
                            var newRendering = PresentationService.FindRendering(newDeviceDef, cm.ToPlaceholder, cm.ToComponent);
                            if (newRendering == null)
                            {
                                Logger.Log($"There was no rendering matching device:{cm.ToDevice} - placeholder:{cm.ToPlaceholder} - component:{cm.ToComponent}", importItem.Paths.FullPath, LogType.MultilistToComponent, "device xml", newDeviceDef.ToXml());
                                continue;
                            }

                            var newDatasource = PresentationService.FindDatasource(this, newItem, newRendering);
                            if (newDatasource == null)
                            {
                                Logger.Log($"There was no datasource found matching name:{dsName}", newItem.Paths.FullPath, LogType.MultilistToComponent, "rendering xml", newRendering.ToXml());
                                continue;
                            }

                            SetDatasourceFields(oldDatasource, newDatasource, cm);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log("SitecoreDataMap.ProcessComponents", string.Format("failed to import component {0} on item {1}", cm.FromComponent, importItem.Paths.FullPath));
                    }
                }
            }
        }

        public virtual void SetDatasourceFields(Item oldDatasource, Item newDatasource, IComponentMapping cm)
        {
            newDatasource.Editing.BeginEdit();
            foreach (var fieldMap in cm.FieldDefinitions)
            {
                fieldMap.FillField(this, ref newDatasource, oldDatasource);
            }
            newDatasource.Editing.EndEdit(false, false);
            newDatasource.Database.Caches.ItemCache.RemoveItem(newDatasource.ID);
        }

        #endregion Methods
    }
}
