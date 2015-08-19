using System.Globalization;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.SharedSource.DataImporter.Providers;

namespace Sitecore.SharedSource.DataImporter.Mappings.Fields
{
   
    public class ToNumber : ToText, IBaseField
    {
        public CultureInfo TargetCulture { get; set; }
        public CultureInfo ImportCulture { get; set; }

        public ToNumber(Item i) : base(i)
        {
            if (i.Fields["TargetCulture"] != null)
            {
                TargetCulture = new CultureInfo(i.Fields["TargetCulture"].Value);
            }
            if (i.Fields["ImportCulture"] != null)
            {
                ImportCulture = new CultureInfo(i.Fields["ImportCulture"].Value);
            }
        }

        public override void FillField(BaseDataMap map, ref Item newItem, string importValue)
        {
            double value = 0;
            var style = NumberStyles.Number;
            if (double.TryParse(importValue.Trim(), style, ImportCulture, out value))
            {
                // store the imported value as is
                Field f = newItem.Fields[NewItemField];
                if (f != null)
                {
                    f.Value = value.ToString(TargetCulture ?? Context.Culture);
                }
            }
        }
    }
}