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
using Sitecore.SharedSource.DataImporter.Logger;
using Sitecore.SharedSource.DataImporter.Mappings.Properties;

namespace Sitecore.SharedSource.DataImporter.Providers
{
	public class SqlDataMap : BaseDataMap
	{

		#region Properties

		#endregion Properties

		#region Constructor

		public SqlDataMap(Database db, string connectionString, Item importItem, ILogger l) : base(db, connectionString, importItem, l) { }

		#endregion Constructor

		#region IDataMap Methods

		/// <summary>
		/// uses a SqlConnection to get data
		/// </summary>
		/// <returns></returns>
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
        
		/// <summary>
		/// gets custom data from a DataRow
		/// </summary>
		/// <param name="importRow"></param>
		/// <param name="fieldName"></param>
		/// <returns></returns>
		public override string GetFieldValue(object importRow, string fieldName)
		{
			DataRow item = importRow as DataRow;
			object f = item[fieldName];
			return (f != null) ? f.ToString() : string.Empty;
		}

		#endregion IDataMap Methods

		#region Methods

		#endregion Methods
	}
}
