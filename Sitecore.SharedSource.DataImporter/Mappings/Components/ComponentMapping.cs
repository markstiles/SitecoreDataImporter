using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sitecore.Data.Items;
using Sitecore.SharedSource.DataImporter.Mappings.Fields;
using Sitecore.SharedSource.DataImporter.Mappings.Properties;
using Sitecore.SharedSource.DataImporter.Mappings.ReferenceFields;

namespace Sitecore.SharedSource.DataImporter.Mappings.Templates
{
	public class ComponentMapping
	{

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

		public string FromWhatRendering { get; set; }
		public string ToWhatRendering { get; set; }

		public string FromWhatPlaceholder { get; set; }
		public string ToWhatPlaceholder { get; set; }

		public string ReplaceWhatRendering { get; set; }
		public string DefaultDatasource { get; set; }
		public string RenderingParameters { get; set; }
		public bool UseParentAsDatasource { get; set; }
		public string Id { get; set; }

		/// <summary>
		/// the definitions of fields to import
		/// </summary>
		public List<IBaseField> FieldDefinitions { get; set; }

		public List<IBaseFieldWithReference> ReferenceFieldDefinitions { get; set; }

		/// <summary>
		/// List of properties
		/// </summary>
		public List<IBaseProperty> PropertyDefinitions { get; set; }

		#endregion

		//constructor
		public ComponentMapping(Item i)
		{
			Id = i.ID.ToString();
			FromWhatTemplate = i.Fields["From What Template"].Value;
			ToWhatTemplate = i.Fields["To What Template"].Value;
			ComponentName = i.Fields["Component Name"].Value;
			FolderName = i.Fields["Folder Name"].Value;
			FromWhatRendering = i.Fields["From What Rendering"].Value;
			ToWhatRendering = i.Fields["To What Rendering"].Value;
			FromWhatPlaceholder = i.Fields["From What Placeholder"].Value;
			ToWhatPlaceholder = i.Fields["To What Placeholder"].Value;
			ReplaceWhatRendering = i.Fields["Replace What Rendering"].Value;
			DefaultDatasource = i.Fields["Default Datasource"].Value;
			RenderingParameters = i.Fields["Rendering Parameters"].Value;
			UseParentAsDatasource = i.Fields["Use Parent As Datasource"].Value == "1";
		}
	}
}
