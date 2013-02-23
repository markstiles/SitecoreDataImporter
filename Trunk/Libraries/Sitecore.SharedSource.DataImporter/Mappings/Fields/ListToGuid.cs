using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.SharedSource.DataImporter;
using Sitecore.Data.Items;
using Sitecore.Data;
using System.Data;
using Sitecore.SharedSource.DataImporter.Extensions;
using System.Collections;
using Sitecore.SharedSource.DataImporter.Providers;
using Sitecore.SharedSource.DataImporter.Utility;
using Sitecore.Data.Fields;

namespace Sitecore.SharedSource.DataImporter.Mappings.Fields {
	public class ListToGuid : ToText {

		#region Properties

		private string _SourceList;
		public string SourceList {
			get {
				return _SourceList;
			}
			set {
				_SourceList = value;
			}
		}

		#endregion Properties

		#region Constructor

		//constructor
		public ListToGuid(Item i) : base(i) {
			SourceList = i.Fields["Source List"].Value;
		}

		#endregion Constructor

		#region Methods

        public override void FillField(BaseDataMap map, ref Item newItem, string importValue)
        {
            Item i = newItem.Database.GetItem(SourceList);
			if (i != null) {
                IEnumerable<Item> t = from Item c in i.GetChildren()
                                      where c.DisplayName.Equals(StringUtility.GetNewItemName(importValue, map.ItemNameMaxLength))
                                      select c;

                if (t.Any()) {
                    Field f = newItem.Fields[NewItemField];
                    if(f != null)
                        f.Value = t.First().ID.ToString();
                }
			}
		}

		#endregion Methods
	}
}
