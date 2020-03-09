using System.Globalization;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.SharedSource.DataImporter.Logger;
using Sitecore.SharedSource.DataImporter.Providers;

namespace Sitecore.SharedSource.DataImporter.Mappings.Fields
{
   
    public class ToNumber : ToText, IBaseField
    {
        public CultureInfo TargetCulture { get; set; }
        public CultureInfo ImportCulture { get; set; }

        public ToNumber(Item i, ILogger l) : base(i, l) {
            string tCulture = GetItemField(i, "TargetCulture");
            TargetCulture = (string.IsNullOrEmpty(tCulture)) ? CultureInfo.CurrentCulture : new CultureInfo(tCulture);
            string iCulture = GetItemField(i, "ImportCulture");
            ImportCulture = (string.IsNullOrEmpty(iCulture)) ? CultureInfo.CurrentCulture : new CultureInfo(iCulture);
        }

        public override void FillField(IDataMap map, ref Item newItem, object importRow, string importValue) {

            if (string.IsNullOrEmpty(importValue))
                return;

            double value = 0;
            var style = NumberStyles.Number;
            if (!double.TryParse(importValue.Trim(), style, ImportCulture, out value))
                return;

            Field f = newItem.Fields[NewItemField];
            if (f == null)
                return;
               
            f.Value = value.ToString(TargetCulture ?? Context.Culture);
        }
    }
}