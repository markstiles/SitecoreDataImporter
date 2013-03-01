using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.Data.Items;
using Sitecore.SharedSource.DataImporter.Extensions;
using System.Data;
using System.Collections;
using Sitecore.SharedSource.DataImporter.Providers;
using Sitecore.Data.Fields;

namespace Sitecore.SharedSource.DataImporter.Mappings.Fields {
	
    /// <summary>
    /// This field converts a date value to a sitecore date field value
    /// </summary>
    public class ToDate : ToText {

		#region Properties 

		#endregion Properties
		
		#region Constructor

		//constructor
		public ToDate(Item i) : base(i) {
			
		}

		#endregion Constructor
		
		#region Methods

        public override void FillField(BaseDataMap map, ref Item newItem, string importValue)
        {
            //try to parse date value
            DateTime date = DateTime.Parse(importValue);
			Field f = newItem.Fields[NewItemField];
            if(f != null)
                f.Value = date.ToDateFieldValue();
		}

		#endregion Methods
	}
}
