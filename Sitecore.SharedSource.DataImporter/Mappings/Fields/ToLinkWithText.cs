using System;
using System.Web;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.SharedSource.DataImporter.Logger;
using Sitecore.SharedSource.DataImporter.Providers;

namespace Sitecore.SharedSource.DataImporter.Mappings.Fields {

    /// <summary>
    /// this field uses the url stored in the field and converts it to a LinkField value
    /// </summary>
    public class ToLinkWithText : ToText {
        
        #region Properties

        #endregion Properties

        #region Constructor

        //constructor
        public ToLinkWithText(Item i, ILogger l) : base(i, l) {

        }

        #endregion Constructor

        #region IBaseField

        public override void FillField(IDataMap map, ref Item newItem, object importRow)
        {
            var importValue = string.Join(Delimiter, map.GetFieldValues(ExistingDataNames, importRow));
            if (string.IsNullOrEmpty(importValue))
                return;

            var values = importValue.Split(new[] {Delimiter}, StringSplitOptions.RemoveEmptyEntries);

            var text = values.Length > 0 ? HttpUtility.HtmlEncode(values[0]) : string.Empty;
            var url = values.Length > 1 ? values[1] : string.Empty;

            //get the field as a link field and store the url
            Field lf = newItem.Fields[ToWhatField];
            if (lf != null)
                lf.Value = $"<link text=\"{text}\" linktype=\"external\" url=\"{url}\" anchor=\"\" class=\"link-item__external\" target=\"_blank\" />";
        }
        
        #endregion IBaseField
    }
}
