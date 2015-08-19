using System.Globalization;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.SharedSource.DataImporter.Providers;

namespace Sitecore.SharedSource.DataImporter.Mappings.Fields
{
 
    public class ToInteger : ToText, IBaseField
    {
        public ToInteger(Item i) : base(i)
        {
        }

        public override void FillField(BaseDataMap map, ref Item newItem, string importValue)
        {
            int value = 0;
            if (int.TryParse(importValue.Trim(), out value))
            {
                // store the imported value as is
                Field f = newItem.Fields[NewItemField];
                if (f != null)
                {
                    f.Value = value.ToString(CultureInfo.InvariantCulture);
                }
            }
        }
    }
}