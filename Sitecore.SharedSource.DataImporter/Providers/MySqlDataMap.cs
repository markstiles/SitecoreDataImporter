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
using MySql.Data.MySqlClient;

namespace Sitecore.SharedSource.DataImporter.Providers {
    //To use this you'll need the .NET MySQL connector: http://dev.mysql.com/downloads/connector/net/
    //connection string should resemble: <add name="Name" connectionString="DbType=Mysql;Server=server.address.com;Port=3306;Database=DBName;Uid=user;Pwd=secret"/>
    public class MySqlDataMap : SqlDataMap {

        #region Properties

        #endregion Properties

        #region Constructor

        public MySqlDataMap(Database db, string ConnectionString, Item importItem)
            : base(db, ConnectionString, importItem) {
        }

        #endregion Constructor

        #region Override Methods

        /// <summary>
        /// uses a MySqlConnection to retrieve data
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<object> GetImportData() {
            DataSet ds = new DataSet();
            MySqlConnection conSQL = new MySqlConnection(this.DatabaseConnectionString);
            conSQL.Open();

            MySqlDataAdapter adapter = new MySqlDataAdapter();
            MySqlCommand cmd = new MySqlCommand(this.Query, conSQL);
            adapter.SelectCommand = cmd;
            adapter.Fill(ds);
            conSQL.Close();

            DataTable dt = ds.Tables[0].Copy();

            return (from DataRow dr in dt.Rows
                    select dr).Cast<object>();
        }

        #endregion Override Methods

        #region Methods

        #endregion Methods
    }
}
