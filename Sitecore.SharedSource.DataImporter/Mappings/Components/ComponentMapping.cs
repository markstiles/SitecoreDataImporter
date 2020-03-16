using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sitecore.Data.Items;
using Sitecore.SharedSource.DataImporter.Mappings.Fields;

namespace Sitecore.SharedSource.DataImporter.Mappings.Templates
{
	public class ComponentMapping : IComponentMapping
    {
        #region Properties

        public string Id { get; set; }
        public string FromDevice { get; set; }
		public string FromPlaceholder { get; set; }
		public string FromComponent { get; set; }
		public string ToDevice { get; set; }
		public string ToTemplate { get; set; }
		public string ToComponent { get; set; }
		public string ToPlaceholder { get; set; }
		public string ToDatasourcePath { get; set; }
        public string ToDatasourceFolder { get; set; }
        public string ToParameters { get; set; }
		public bool OverwriteExisting { get; set; }
		public bool IsSXA { get; set; }
        
        public List<IBaseField> FieldDefinitions { get; set; }
                
		#endregion

		public ComponentMapping(Item i)
		{
            Id = i.ID.ToString();
            FromDevice = i.Fields["From Device"].Value;
            FromPlaceholder = i.Fields["From Placeholder"].Value;
            FromComponent = i.Fields["From Component"].Value;

            ToDevice = i.Fields["To Device"].Value;
            ToTemplate = i.Fields["To Template"].Value;
            ToComponent = i.Fields["To Component"].Value;
            ToPlaceholder = i.Fields["To Placeholder"].Value;
            ToDatasourcePath = i.Fields["To Datasource Path"].Value;
            ToDatasourceFolder = i.Fields["To Datasource Folder"].Value;
            ToParameters = i.Fields["To Parameters"].Value;
            
            OverwriteExisting = i.Fields["Overwrite Existing"].Value == "1";
            IsSXA = i.Fields["Is SXA"].Value == "1";
            
            FieldDefinitions = new List<IBaseField>();
		}
	}
}
