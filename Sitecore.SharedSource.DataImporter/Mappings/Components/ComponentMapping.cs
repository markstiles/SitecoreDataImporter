using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sitecore.Data.Items;
using Sitecore.SharedSource.DataImporter.Mappings.Fields;

namespace Sitecore.SharedSource.DataImporter.Mappings.Templates
{
	public class ComponentMapping
	{
		#region Properties
        
		public string FromWhatTemplate { get; set; }
		public string ToWhatTemplate { get; set; }
		public string ComponentName { get; set; }
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

		public List<IBaseField> FieldDefinitions { get; set; }
                
		#endregion

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
