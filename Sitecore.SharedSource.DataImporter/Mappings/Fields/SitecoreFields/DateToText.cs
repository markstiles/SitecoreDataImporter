using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore.Data.Items;
using Sitecore.SharedSource.DataImporter.Providers;
using Sitecore.Data.Fields;
using Sitecore.Diagnostics;
using Sitecore.SharedSource.DataImporter.Logger;
using Sitecore.SharedSource.DataImporter.Mappings.Fields;

namespace Sitecore.SharedSource.DataImporter.Mappings.Fields.SitecoreFields
{
    public class DateToText : BaseField
    {
		#region Properties

		protected readonly char[] comSplitr = { ',' };

		#endregion Properties

		#region Constructor
        
		public DateToText(Item i, ILogger l) : base(i, l) { }

        #endregion Constructor

        #region IBaseProperty
        
        public override void FillField(IDataMap map, ref Item newItem, object importRow)
        {
            var importItem = (Item)importRow;

            Assert.IsNotNull(importRow, "importRow");
			Assert.IsNotNull(newItem, "newItem");

            importItem.Fields.ReadAll();
            var fieldNames = GetExistingFieldName().Split(comSplitr, StringSplitOptions.RemoveEmptyEntries);
			var fields = fieldNames.Select(f => (DateField)importItem.Fields[f]);
			var values = fields.Select(f => f?.DateTime.ToString("MMMM d, yyyy")).Where(f => !string.IsNullOrEmpty(f));
			var value = string.Join(GetItemField(InnerItem, "Delimiter"), values);
			Field field = newItem.Fields[ToWhatField];
			if (field != null)
				field.Value = value;
		}

        #endregion IBaseProperty
        
        public string GetExistingFieldName()
        {
            return GetItemField(InnerItem, "From What Fields");
        }
    }
}
