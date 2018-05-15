using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sitecore.Data.Items;
using Sitecore.SharedSource.DataImporter.Providers;
using Sitecore.Data.Fields;
using Sitecore.SharedSource.DataImporter.Mappings.Properties;
using Sitecore.SharedSource.DataImporter.Utility;
using System.Net;
using System.IO;
using Sitecore.Resources.Media;
using Sitecore.Data;
using Sitecore.Data.Managers;
using Sitecore.Diagnostics;
using Sitecore.SharedSource.DataImporter.Logger;

namespace Sitecore.SharedSource.DataImporter.Mappings.Fields {
    public class DateToText : BaseMapping, IBaseFieldWithReference
    {
		#region Properties
		protected readonly char[] comSplitr = { ',' };

		#endregion Properties

		#region Constructor

		//constructor
		public DateToText(Item i, ILogger l) : base(i) { }

        #endregion Constructor

        #region IBaseProperty

        public string Name { get; set; }

        public void FillField(IDataMap map, ref Item newItem, Item importRow, string fieldName)
        {
			Assert.IsNotNull(importRow, "importRow");
			Assert.IsNotNull(newItem, "newItem");

			importRow.Fields.ReadAll();
            var fieldNames = GetExistingFieldName().Split(comSplitr, StringSplitOptions.RemoveEmptyEntries);
			var fields = fieldNames.Select(f => (DateField)importRow.Fields[f]);
			var values = fields.Select(f => f?.DateTime.ToString("MMMM d, yyyy")).Where(f => !string.IsNullOrEmpty(f));
			var value = string.Join(GetItemField(InnerItem, "Delimiter"), values);
			Field field = newItem.Fields[NewItemField];
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
