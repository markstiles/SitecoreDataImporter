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

public partial class _Default : Page
{
	protected Database currentDB;
    protected StringBuilder log;

    protected readonly string BaseImportMapID = "{FFB11C18-4EC6-434B-BA51-C81B640539B4}";

	protected void Page_Load(object sender, EventArgs e) {

        log = new StringBuilder();

		currentDB = Sitecore.Configuration.Factory.GetDatabase("master");

		if (!IsPostBack) {
			foreach (ConnectionStringSettings c in ConfigurationManager.ConnectionStrings) {
				ddlConnStr.Items.Add(new ListItem(c.Name, c.ConnectionString));
			}
			populateImportDDL();
        } 
    }

    protected void Log(string errorType, string message)
    {
        log.AppendFormat("{0} : {1}", errorType, message).AppendLine().AppendLine();
    }

	protected void populateImportDDL() {

		List<string> importTypes = new List<string>();
		Item importNode = currentDB.Items["/sitecore/System/Modules/Data Imports"];
		IEnumerable<Item> imports = from Item import in importNode.Axes.GetDescendants()
                                    where import.Template.IsID(BaseImportMapID)
                                    select import;
        ddlImport.Items.Clear();
		
		if (imports.Any()) {
            //set sql imports drop down
            foreach (Item i in imports) {
			    ddlImport.Items.Add(new ListItem(i.DisplayName, i.ID.ToString()));
		    }
        }
	}

	protected void btnRefresh_Click(object sender, EventArgs e) {
		populateImportDDL();
	}

	protected void btnImport_Click(object sender, EventArgs e) {

		Item importItem = currentDB.Items[ddlImport.SelectedValue];

		if (importItem != null) {

			//new import
            TextField hc = importItem.Fields["Handler Class"];
            TextField ha = importItem.Fields["Handler Assembly"];
            if (ha != null && !string.IsNullOrEmpty(ha.Value)) {
                if (hc != null && !string.IsNullOrEmpty(hc.Value)) {
                    BaseDataMap map = null;
                    try {
                        map = (BaseDataMap)Sitecore.Reflection.ReflectionUtil.CreateObject(ha.Value, hc.Value, new object[] { currentDB, ddlConnStr.SelectedValue, importItem });
                    } catch (FileNotFoundException fnfe) {
                        Log("Error", "the binary specified could not be found");
                    }
                    if (map != null)
                    log.Append(map.Process());
                    else
                        Log("Error", "the data map provided could not be instantiated");
                } else {
                    Log("Error", "import handler class is not defined");
                }
            } else {
                Log("Error", "import handler assembly is not defined");
            }

			txtMessage.Text = log.ToString();
		}
	}
}
