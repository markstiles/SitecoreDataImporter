using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.Data.Items;
using System.Data;
using Sitecore.SharedSource.DataImporter.Providers;
using Sitecore.Links;
using Sitecore.Data.Fields;

namespace Sitecore.SharedSource.DataImporter.Mappings.Properties {
	public class UrlToText : BaseMapping, IBaseProperty {
		#region Properties

		#endregion Properties

		#region Constructor

		//constructor
		public UrlToText(Item i)
			: base(i) {
		}

		#endregion Constructor

		#region IBaseProperty

        public string Name { get; set; }

		//fills it's own field
        public void FillField(BaseDataMap map, ref Item newItem, Item importRow)
        {
            Field f = newItem.Fields[NewItemField];
            if(f != null)
                f.Value = LinkManager.GetDynamicUrl(importRow);
        }

        #endregion IBaseProperty
    }
}
