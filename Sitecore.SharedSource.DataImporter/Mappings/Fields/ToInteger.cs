using System.Globalization;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.SharedSource.DataImporter.Logger;
using Sitecore.SharedSource.DataImporter.Providers;

namespace Sitecore.SharedSource.DataImporter.Mappings.Fields
{
 
    public class ToInteger : ToText
    {
        public ToInteger(Item i, ILogger l) : base(i, l)
        {
        }

        public override void FillField(IDataMap map, ref Item newItem, object importRow)
        {
            var importValue = string.Join(Delimiter, map.GetFieldValues(ExistingDataNames, importRow));
            if (string.IsNullOrEmpty(importValue))
                return;

            int value = 0;
            if (!int.TryParse(importValue.Trim(), out value))
            {
                map.Logger.Log("Couldn't parse the integer value", newItem.Paths.FullPath, LogType.FieldError, Name, importValue);
                return;
            }
            
            Field f = newItem.Fields[ToWhatField];
            if (f == null)
                return;
                
            f.Value = value.ToString(CultureInfo.InvariantCulture);
        }
    }
}