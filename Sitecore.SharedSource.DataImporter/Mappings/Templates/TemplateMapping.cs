using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sitecore.Data.Items;
using Sitecore.SharedSource.DataImporter.Mappings.Fields;

namespace Sitecore.SharedSource.DataImporter.Mappings.Templates
{
	public class TemplateMapping
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
		/// the definitions of fields to import
		/// </summary>
		public List<IBaseField> FieldDefinitions { get; set; }
        
		#endregion

		//constructor
		public TemplateMapping(Item i)
		{
			FromWhatTemplate = i.Fields["From What Template"].Value;
			ToWhatTemplate = i.Fields["To What Template"].Value;
		}
	}
}
