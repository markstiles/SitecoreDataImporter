using System;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Sitecore.Data.Items;
using System.Collections.Generic;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.SharedSource.DataImporter.Providers;
using System.Text;
using System.IO;
using CsvHelper;
using Sitecore.Web;
using Sitecore.SharedSource.DataImporter.Logger;

namespace Sitecore.SharedSource.DataImporter.Editors
{ 
    public partial class Default : Page
    {
	    protected Database currentDB;
        protected StringBuilder log;
	    protected Item importItem; 

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

            repJobs.DataSource = Jobs;
            repJobs.DataBind();

            if (!IsPostBack) {
				    ddlConnStr.Items.Add(new ListItem(ConfigurationManager.ConnectionStrings["legacy"].Name, ConfigurationManager.ConnectionStrings["legacy"].ConnectionString));
			    
            } 
        }

        #region Logging

        protected void Log(string message) {
            log.Append(message).AppendLine().AppendLine();
        }

        protected void Log(string errorType, string message) {
            log.AppendFormat("{0} : {1}", errorType, message).AppendLine().AppendLine();
        }

        #endregion Loggging

        #region Import

        protected void btnImport_Click(object sender, EventArgs e) {

            //check import item
            if (importItem == null)
            {
                Log("Error", "Import item is null");
                txtMessage.Text = log.ToString();
                return;
            }

            //check handler assembly
            TextField ha = importItem.Fields["Handler Assembly"];
            if (ha == null || string.IsNullOrEmpty(ha.Value))
            {
                Log("Error", "Import handler assembly is not defined");
                txtMessage.Text = log.ToString();
                return;
            }

            //check handler class
            TextField hc = importItem.Fields["Handler Class"];
            if (hc == null || string.IsNullOrEmpty(hc.Value))
            {
                Log("Error", "Import handler class is not defined");
                txtMessage.Text = log.ToString();
                return;
            }

            //check db
            if (currentDB == null)
            {
                Log("Error", "Database is null");
                txtMessage.Text = log.ToString();
                return;
            }

            //check conn str
            if (string.IsNullOrEmpty(ddlConnStr.SelectedValue))
            {
                Log("Error", "Connection string is empty");
                txtMessage.Text = log.ToString();
                return;
			}
			//check logger assembly
			TextField la = importItem.Fields["Logger Assembly"];
			if (ha == null || string.IsNullOrEmpty(ha.Value))
			{
				Log("Error", "Import logger assembly is not defined");
				txtMessage.Text = log.ToString();
				return;
			}

			//check handler class
			TextField lc = importItem.Fields["Logger Class"];
			if (hc == null || string.IsNullOrEmpty(hc.Value))
			{
				Log("Error", "Import logger class is not defined");
				txtMessage.Text = log.ToString();
				return;
			}

			//try to instantiate object
			IDataMap map = null;
			ILogger l = (ILogger) Sitecore.Reflection.ReflectionUtil.CreateObject(
				la.Value,
				lc.Value,
				new object[0]);

			try
            {
                map = (IDataMap)Sitecore.Reflection.ReflectionUtil.CreateObject(
                    ha.Value,
                    hc.Value,
                    new object[] { currentDB, ddlConnStr.SelectedValue, importItem, l }
                );
				map.Name = importItem.Name;

			}
            catch (FileNotFoundException fnfe)
            {
                var n = fnfe.Message;
                Log("Error", string.Format("the binary {0} could not be found", ha.Value));
                txtMessage.Text = log.ToString();
                return;
            }

            //run process
            if (map == null)
            {
                Log("Error", "the data map provided could not be instantiated");
                txtMessage.Text = log.ToString();
                return;
            }
        
            var jobOptions = new Sitecore.Jobs.JobOptions(
                                    "DataImport",
                                    importItem.DisplayName,
                                    Sitecore.Context.Site.Name,
                                    this,
                                    "HandleImport",
                                    new object[] { map, l });

            Sitecore.Jobs.JobManager.Start(jobOptions);

            repJobs.DataSource = Jobs;
            repJobs.DataBind();       
	    }
        protected void HandleImport(IDataMap map, ILogger l) {

            ImportProcessor p = new ImportProcessor(map);
            p.Process();
			
            
        }

        #endregion Import

        #region Media Import

        protected void btnMediaImport_Click(object sender, EventArgs e)
		{

			//check import item
			if (importItem == null)
			{
				Log("Error", "Import item is null");
				txtMessage.Text = log.ToString();
				return;
			}

			//check handler assembly
			TextField ha = importItem.Fields["Handler Assembly"];
			if (ha == null || string.IsNullOrEmpty(ha.Value))
			{
				Log("Error", "Import handler assembly is not defined");
				txtMessage.Text = log.ToString();
				return;
			}

			//check handler class
			TextField hc = importItem.Fields["Handler Class"];
			if (hc == null || string.IsNullOrEmpty(hc.Value))
			{
				Log("Error", "Import handler class is not defined");
				txtMessage.Text = log.ToString();
				return;
			}

			//check db
			if (currentDB == null)
			{
				Log("Error", "Database is null");
				txtMessage.Text = log.ToString();
				return;
			}

			//check conn str
			if (string.IsNullOrEmpty(ddlConnStr.SelectedValue))
			{
				Log("Error", "Connection string is empty");
				txtMessage.Text = log.ToString();
				return;
			}

			//try to instantiate object
			IDataMap map = null;
			ILogger l = new DefaultLogger();
			try
			{
				map = (IDataMap)Sitecore.Reflection.ReflectionUtil.CreateObject(
					ha.Value,
					hc.Value,
					new object[] { currentDB, ddlConnStr.SelectedValue, importItem, l }
				);
			}
			catch (FileNotFoundException fnfe)
			{
			    var n = fnfe.Message;
				Log("Error", string.Format("the binary {0} could not be found", ha.Value));
				txtMessage.Text = log.ToString();
				return;
			}

			//run process
			if (map == null)
			{
				Log("Error", "the data map provided could not be instantiated");
				txtMessage.Text = log.ToString();
				return;
			}

			var jobOptions = new Sitecore.Jobs.JobOptions(
									"MediaLibraryImport",
									"Media",
									Sitecore.Context.Site.Name,
									this,
									"HandleMediaImport",
									new object[] { map, l });

			Sitecore.Jobs.JobManager.Start(jobOptions);

			repJobs.DataSource = Jobs;
			repJobs.DataBind();
		}

	    protected void HandleMediaImport(IDataMap map, ILogger l)
	    {
		    //var handler = map as PmbiDataMap;
			//handler?.TransferMediaLibrary();
	    }

        #endregion Media Import

        #region Post Import

        protected void btnPostImport_Click(object sender, EventArgs e)
        {
            var jobOptions = new Sitecore.Jobs.JobOptions(
                                    "PostImport",
                                    "Post Import",
                                    Sitecore.Context.Site.Name,
                                    this,
                                    "HandlePostImport",
                                    new object[] { });

            Sitecore.Jobs.JobManager.Start(jobOptions);

            repJobs.DataSource = Jobs;
            repJobs.DataBind();
        }
		

        #endregion Post Import

        public IEnumerable<Sitecore.Jobs.Job> Jobs
        {
            get
            {
                return Sitecore.Jobs.JobManager.GetJobs().Where(j => j.Name == "DataImporter" || !j.IsDone).OrderBy(job => job.QueueTime);
            }
        }
		
    }
}