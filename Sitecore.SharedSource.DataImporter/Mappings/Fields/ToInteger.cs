using System.Globalization;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.SharedSource.DataImporter.Providers;

namespace Sitecore.SharedSource.DataImporter.Mappings.Fields
{
 
    public class ToInteger : ToText
    {
        public ToInteger(Item i) : base(i)
        {
        }

        public override void FillField(IDataMap map, ref Item newItem, string importValue) {
        
            int value = 0;
            if (!int.TryParse(importValue.Trim(), out value))
            {
                map.Logger.LogError("Integer Parse Error", string.Format("Couldn't parse the integer value of the {0} field", ItemName()));
                return;
            }
            
            Field f = newItem.Fields[NewItemField];
            if (f == null)
                return;
                
            f.Value = value.ToString(CultureInfo.InvariantCulture);
        }
    }
}