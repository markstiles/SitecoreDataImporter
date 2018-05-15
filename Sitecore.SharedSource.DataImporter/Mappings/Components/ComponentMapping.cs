using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sitecore.Data.Items;
using Sitecore.SharedSource.DataImporter.Extensions;
using Sitecore.SharedSource.DataImporter.Mappings.Fields;
using Sitecore.SharedSource.DataImporter.Mappings.Properties;
using Sitecore.SharedSource.DataImporter.Utility;
using log4net;
using Sitecore.SharedSource.DataImporter.Logger;

namespace Sitecore.SharedSource.DataImporter.Mappings.Templates {
	public class ComponentMapping {

		#region Properties

		/// <summary>
		/// the template the old item is from
		/// </summary>
        public string FromWhatTemplate { get; set; }

		/// <summary>
		/// the template the new item is going to
		/// </summary>
        public string ToWhatTemplate { get; set; }

        /// <summary>
        /// the name of the new item
        /// </summary>
        public string ComponentName { get; set; }
		/// <summary>
		/// the name of the component's folder
		/// </summary>
		public string FolderName { get; set; }
		/// <summary>
		/// the name of the component's folder
		/// </summary>
		public IEnumerable<string> RequiredFields { get; set; }

		/// <summary>
		/// the definitions of fields to import
		/// </summary>
		public List<IBaseField> FieldDefinitions { get; set; }
		/// <summary>
		/// the definitions of fields to import
		/// </summary>
		public List<IBaseFieldWithReference> ReferenceFieldDefinitions { get; set; }
		public IEnumerable<ComponentMapping> ComponentMappingDefinitions { get; set; }

		public Dictionary<string,TemplateMapping> TemplateMappingDefinitions { get; set; }

		/// <summary>
		/// List of properties
		/// </summary>
		public List<IBaseProperty> PropertyDefinitions { get; set; }
		public string Query { get; set; }
		public string Rendering { get; set; }
		public bool PreserveComponentId { get; set; }
		public string Placeholder { get; set; }
		public ILogger Logger { get; set; }
        
		#endregion

		//constructor
		public ComponentMapping(Item i, ILogger logger) {
			FromWhatTemplate = i.Fields["From What Template"].Value;
			ToWhatTemplate = i.Fields["To What Template"].Value;
            ComponentName = i.Fields["Component Name"].Value;
			FolderName = i.Fields["Folder Name"].Value;
			RequiredFields = i.Fields["Required Fields"].Value?.Split(new []{';'}, StringSplitOptions.RemoveEmptyEntries);
			Rendering = i.Fields["Rendering"].Value;
			Query = i.Fields["Query"].Value;
			PreserveComponentId = i.GetItemBool("Preseve Component ID");
			Placeholder = i.Fields["Placeholder"].Value;
			Logger = logger;
		}

		public Item[] GetImportItems(Item parent)
		{
			var items = new List<Item>();
			foreach(var queryLine in Query.Split(new[] { Environment.NewLine }, StringSplitOptions.None))
			{
				var query = queryLine;
				if (string.IsNullOrEmpty(query))
				{
					return new[] {parent};
				}
				if (query == "./")
				{
					items.Add(parent);
					continue;
				}
				if (query.StartsWith("./")) 
				{
					query = query.Replace("./", StringUtil.EnsurePostfix('/', parent.Paths.FullPath));
				}

				var cleanQuery = StringUtility.CleanXPath(query);
				Logger.Log("ComponentMapping.GetImportItems", string.Format("Running query: {0}", cleanQuery));
				items.AddRange(parent.Database.SelectItems(cleanQuery));
			}
			return items.ToArray();
		}
	}
}
