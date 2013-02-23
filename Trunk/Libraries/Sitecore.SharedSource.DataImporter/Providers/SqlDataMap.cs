using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Data;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using Sitecore.SharedSource.DataImporter.Mappings.Fields;
using Sitecore.SharedSource.DataImporter.Extensions;
using System.Collections;

namespace Sitecore.SharedSource.DataImporter.Providers
{
	public class SqlDataMap : BaseDataMap {
		
		#region Properties

		#endregion Properties

		#region Constructor

		public SqlDataMap(Database db, string connectionString, Item importItem) : base (db, connectionString, importItem) {
		}
		
		#endregion Constructor

        #region Override Methods

        public override IEnumerable<object> GetImportData()
        {
            DataSet ds = new DataSet();
            SqlConnection dbCon = new SqlConnection(this.DatabaseConnectionString);
            dbCon.Open();

            SqlDataAdapter adapter = new SqlDataAdapter(this.Query, dbCon);
            adapter.Fill(ds);
            dbCon.Close();

            DataTable dt = ds.Tables[0].Copy();
            
            return (from DataRow dr in dt.Rows
                    select dr).Cast<object>();
        }

        public override void ProcessCustomData(ref Item newItem, object importRow)
        {
        }
        
        protected override string GetFieldValue(object importRow, string fieldName)
        {
            DataRow item = importRow as DataRow;
            return item[fieldName].ToString();
        }

        #endregion Override Methods

        #region Methods

        #endregion Methods
    }
}
