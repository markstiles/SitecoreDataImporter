using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using System.Data.SqlClient;
using Sitecore.Data.Items;
using System.Collections.Generic;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Shell.Web.UI;
using Sitecore.SharedSource.DataImporter.Providers;
using Sitecore.SharedSource.DataImporter.Extensions;
using System.Text;
using System.IO;
using Sitecore.Web;

public partial class Default : Page
{
	protected Database currentDB;
    protected StringBuilder log;
	protected Item importItem; 

    protected readonly string BaseImportMapID = "{FFB11C18-4EC6-434B-BA51-C81B640539B4}";

	/* Querystring Params available
		id=%7b59A62A95-9E5D-4478-BDC9-1E793823C48F%7d
		la=en
		language=en
		vs=1
		version=1
		database=master
		readonly=0
		db=master
	*/

	protected void Page_Load(object sender, EventArgs e) {

        log = new StringBuilder();

		string dbName = WebUtil.GetQueryString("db");
		currentDB = (!string.IsNullOrEmpty(dbName)) 
			? Sitecore.Configuration.Factory.GetDatabase(dbName)
			: Sitecore.Context.ContentDatabase;

		string idStr = WebUtil.GetQueryString("id");
		if(Sitecore.Data.ID.IsID(idStr))
			importItem = currentDB.GetItem(Sitecore.Data.ID.Parse(idStr));

		if (!IsPostBack) {
			foreach (ConnectionStringSettings c in ConfigurationManager.ConnectionStrings) {
				ddlConnStr.Items.Add(new ListItem(c.Name, c.ConnectionString));
			}
        } 
    }

    protected void Log(string message) {
        log.Append(message).AppendLine().AppendLine();
    }

    protected void Log(string errorType, string message)
    {
        log.AppendFormat("{0} : {1}", errorType, message).AppendLine().AppendLine();
    }

	protected void btnImport_Click(object sender, EventArgs e) {
		
		HandleImport();

		txtMessage.Text = log.ToString();
	}

    protected void HandleImport() {
        
        //check import item
        if (importItem == null) {
            Log("Error", "Import item is null");
            return;
        }

        //check handler class
        TextField ha = importItem.Fields["Handler Assembly"];
        if (ha == null || string.IsNullOrEmpty(ha.Value)) {
            Log("Error", "Import handler assembly is not defined");
            return;
        }

        TextField hc = importItem.Fields["Handler Class"];
        if (hc == null || string.IsNullOrEmpty(hc.Value)) {
            Log("Error", "Import handler class is not defined");
            return;
        }

        if(currentDB == null){
            Log("Error", "Database is null");
            return;
        }

        if(string.IsNullOrEmpty(ddlConnStr.SelectedValue)){
            Log("Error", "Connection string is empty");
            return;
        }

        BaseDataMap map = null;
        try {
            map = (BaseDataMap)Sitecore.Reflection.ReflectionUtil.CreateObject(ha.Value, hc.Value, new object[] { currentDB, ddlConnStr.SelectedValue, importItem });
        } catch (FileNotFoundException fnfe) {
            Log("Error", string.Format("the binary {0} could not be found", ha.Value));
            return;
        }
        
        if (map != null)
            Log(map.Process());
        else
            Log("Error", "the data map provided could not be instantiated");
    }
}
