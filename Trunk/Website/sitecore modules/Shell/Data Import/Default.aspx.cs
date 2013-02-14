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
using Sitecore.SharedSource.DataImporter.Importer;

public partial class _Default : Page
{
	Database currentDB;

	protected void Page_Load(object sender, EventArgs e) {

		currentDB = Sitecore.Configuration.Factory.GetDatabase("master");

		if (!IsPostBack) {
			foreach (ConnectionStringSettings c in ConfigurationManager.ConnectionStrings) {
				ddlConnStr.Items.Add(new ListItem(c.Name, c.ConnectionString));
			}
			populateImportDDLs();
		}
    }

	protected void populateImportDDLs() {

		List<string> importTypes = new List<string>();
		Item importNode = currentDB.Items["/sitecore/System/Modules/Data Imports"];
		Item[] importDescendants = importNode.Axes.GetDescendants();
		List<Item> SQLImports = importDescendants.Where(a => a.TemplateName.Equals("SQL Import Map")).ToList();
		List<Item> SitecoreImports = importDescendants.Where(a => a.TemplateName.Equals("Sitecore Import Map")).ToList();

		ddlSQLImport.Items.Clear();
		ddlSCImport.Items.Clear();

		//set sql imports drop down
		foreach (Item sql in SQLImports) {
			ddlSQLImport.Items.Add(new ListItem(sql.DisplayName, sql.ID.ToString()));
		}
		//set sitecore imports drop down
		foreach (Item sc in SitecoreImports) {
			ddlSCImport.Items.Add(new ListItem(sc.DisplayName, sc.ID.ToString()));
		}
	}

	protected void btnRefresh_Click(object sender, EventArgs e) {
		populateImportDDLs();
	}

	protected void btnSQLImport_Click(object sender, EventArgs e) {

		Item importItem = currentDB.Items[ddlSQLImport.SelectedValue];

		if (importItem != null) {

			SqlDataMap map = new SqlDataMap(currentDB, ddlConnStr.SelectedValue, importItem);
			
			try {
				map.Process();
				ltlSQLMessage.Text = "Finished " + importItem.DisplayName;
			}
			catch (Exception ex) {
				ltlSQLMessage.Text = ex.ToString();
			}
		}
	}

	protected void btnSCImport_Click(object sender, EventArgs e) {

		Item importItem = currentDB.Items[ddlSCImport.SelectedValue];

		if (importItem != null) {

			//new import
			SitecoreDataMap map = new SitecoreDataMap(currentDB, importItem);
			
			try {
				map.Process();
				ltlSCMessage.Text = "Finished " + importItem.DisplayName;
			}
			catch(Exception ex){
				ltlSCMessage.Text = ex.ToString();
			}
		}
	}
}
