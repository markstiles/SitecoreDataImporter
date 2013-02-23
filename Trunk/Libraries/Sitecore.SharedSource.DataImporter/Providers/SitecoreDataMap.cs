using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.SharedSource.DataImporter.Extensions;
using Sitecore.Data.Items;
using Sitecore.Data;
using Sitecore.SharedSource.DataImporter.Mappings.Fields;
using Sitecore.SharedSource.DataImporter.Mappings.Properties;
using System.Web;
using System.Collections;
using Sitecore.SharedSource.DataImporter.Mappings;
using Sitecore.SharedSource.DataImporter.Utility;

namespace Sitecore.SharedSource.DataImporter.Providers
{
	public class SitecoreDataMap : BaseDataMap {
		
		#region Properties
		
		public List<IBaseProperty> PropertyDefinitions {
			get {
				return _propDefinitions;
			}
			set {
				_propDefinitions = value;
			}
		}
		private List<IBaseProperty> _propDefinitions = new List<IBaseProperty>();
		
		#endregion Properties

		#region Constructor

		public SitecoreDataMap(Database db, string connectionString, Item importItem) : base(db, connectionString, importItem) {
		    
		}

		#endregion Constructor

        #region Override Methods

        public override IEnumerable<object> GetImportData()
        {
            return SitecoreDB.SelectItems(StringUtility.CleanXPath(this.Query));
        }

        public override void ProcessCustomData(ref Item newItem, object importRow)
        {
            Item row = importRow as Item;
            //add in the property mappings
            foreach (IBaseProperty d in this.PropertyDefinitions)
                d.FillField(this, ref newItem, row);
        }
        
        protected override string GetFieldValue(object importRow, string fieldName)
        {
            Item item = importRow as Item;
            return item[fieldName];
        }
		
        #endregion Override Methods

        #region Methods

        #endregion Methods
	}
}
