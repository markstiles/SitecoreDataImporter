using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.SharedSource.DataImporter.Providers;
using Sitecore.Diagnostics;
using Sitecore.SharedSource.DataImporter.Logger;
using Sitecore.SharedSource.DataImporter.Mappings.Properties;

namespace Sitecore.SharedSource.DataImporter.Mappings.Fields {
    /// <summary>
    /// this stores the plain text import value as is into the new field
    /// </summary>
    public class FromChildValueToText : BaseMapping, IBaseProperty
    {
        public string ChildTemplate { get; set; }
        public string FieldName { get; set; }

        #region Constructor

        public FromChildValueToText(Item i, ILogger l) : base(i)
		{
		    ChildTemplate = GetItemField(i, "Child Template");
		    FieldName = GetItemField(i, "From What Fields");
        }

        #endregion Constructor

        #region IBaseField

        public string Name { get; set; }

        public void FillField(IDataMap map, ref Item newItem, Item importRow)
        {
            if (importRow.Children == null || string.IsNullOrEmpty(FieldName) || string.IsNullOrEmpty(ChildTemplate))
            {
                return;
            }
            var templateList = ChildTemplate.Split('|');
            var firstChild = importRow.Children.FirstOrDefault(x => templateList.Contains(x.TemplateID.ToString()));
            if (firstChild == null)
            {
                return;
            }
            var value = GetItemField(firstChild, FieldName);
            if (string.IsNullOrEmpty(value))
            {
                return;
            }
            Field f = newItem.Fields[NewItemField];
            if (f != null)
            {
                f.Value = value;
            }
        }

        #endregion IBaseField
    }
}
