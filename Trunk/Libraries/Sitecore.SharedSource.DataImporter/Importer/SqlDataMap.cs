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

namespace Sitecore.SharedSource.DataImporter.Importer
{
	public class SqlDataMap : BaseDataMap {
		
		#region Properties

		private string _dbConnectionString;
		public string DatabaseConnectionString {
			get {
				return _dbConnectionString;
			}
			set {
				_dbConnectionString = value;
			}
		}

		private string _dbSelectCommandTxt;
		public string DatabaseSelectCommandText {
			get {
				return _dbSelectCommandTxt;
			}
			set {
				_dbSelectCommandTxt = value;
			}
		}

		#endregion Properties

		#region Constructor

		public SqlDataMap(Database db, string ConnectionString, Item importItem) : base (db, importItem) {

			DatabaseSelectCommandText = importItem.Fields["SQL Query"].Value;
			DatabaseConnectionString = ConnectionString;
			
		}
		
		#endregion Constructor
		
		#region Methods
		
		public override void Process() {

			DataTable dt = ReturnDataTable(this.DatabaseSelectCommandText, this.DatabaseConnectionString);

			//Loop through the data source
			foreach (DataRow importRow in dt.Rows) {

				string newItemName = GetNewItemName(importRow);
				
				//before you create new items check to see if there are foldering settings
				Item thisParent = GetParentNode(importRow, newItemName);

				try {
					if (!string.IsNullOrEmpty(newItemName)) {
						//Create new item
						Sitecore.Data.Items.Item newItem = thisParent.Add(newItemName, NewItemTemplate);

						if (newItem != null) {
							using (new EditContext(newItem, true, false)) {
								
								//Set the appropriate field values for the new item
								foreach (BaseField d in this.FieldDefinitions) {
									d.FillField(ref newItem, importRow);
								}
							}
						}
					}
				} catch (Exception ex) {
					HttpContext.Current.Response.Write(ex.ToString() + "<br/>name: " + newItemName + "<br/>possibly a bad class name on the field map");
				}
			}
		}

		public static DataTable ReturnDataTable(string sqlString, string connectionString) {
			DataSet ds = new DataSet();
			SqlConnection dbCon = new SqlConnection(connectionString);
			dbCon.Open();

			SqlDataAdapter adapter = new SqlDataAdapter(sqlString, dbCon);
			adapter.Fill(ds);
			dbCon.Close();

			return ds.Tables[0].Copy();
		}

		#endregion Methods
	}
}
