using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sitecore.Data.Items;
using Sitecore.SharedSource.DataImporter.Mappings.Fields;

namespace Sitecore.SharedSource.DataImporter.Mappings.Templates
{
	public interface IComponentMapping
	{
        string Id { get; set; }
        string FromDevice { get; set; }
        string FromPlaceholder { get; set; }
        string FromComponent { get; set; }
        string ToDevice { get; set; }
        string ToTemplate { get; set; }
        string ToComponent { get; set; }
        string ToPlaceholder { get; set; }
        string ToDatasourcePath { get; set; }
        string ToDatasourceFolder { get; set; }
        string ToParameters { get; set; }
        bool OverwriteExisting { get; set; }
        bool IsSXA { get; set; }

        List<IBaseField> FieldDefinitions { get; set; }
	}
}
