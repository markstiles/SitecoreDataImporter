using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Sitecore.Data;
using Sitecore.SharedSource.DataImporter.Areas.DataImport.Models;
using Sitecore.Jobs;
using Sitecore.Data.Items;
using Sitecore.Data.Fields;
using Sitecore.SharedSource.DataImporter.Providers;
using Sitecore.SharedSource.DataImporter.Logger;
using System.IO;
using System.Configuration;
using Sitecore.SharedSource.DataImporter.Services;

namespace Sitecore.SharedSource.DataImporter.Controllers
{
    public class DataImporterController : Controller
    {
        #region Constructor
        
        public DataImporterController()
        {
            
        }

        #endregion

        #region Import
        
        public ActionResult Index(string id, string language, string version, string db)
        {
            return View("ImportForm");
        }

        public ActionResult RunImport(string id, string db, string importType)
        {
            var currentDB = (!string.IsNullOrWhiteSpace(db))
                ? Configuration.Factory.GetDatabase(db)
                : Context.ContentDatabase;

            Item importItem = null;
            if (ID.IsID(id))
                importItem = currentDB.GetItem(ID.Parse(id));
                        
            //check import item
            if (importItem == null)
                return GetResult("", "Import item is null");
 
            //check handler assembly
            TextField ha = importItem.Fields["Handler Assembly"];
            if (ha == null || string.IsNullOrWhiteSpace(ha.Value))
                return GetResult("", "Import handler assembly is not defined");
            
            //check handler class
            TextField hc = importItem.Fields["Handler Class"];
            if (hc == null || string.IsNullOrWhiteSpace(hc.Value))
                return GetResult("", "Import handler class is not defined");
            
            //check db
            if (currentDB == null)
                return GetResult("", "Database is null");

            //check conn str
            Field connStrField = importItem.Fields["Connection String Name"];
            if (connStrField == null)
                return GetResult("", "Connection String Name is not set");

            var connStr = "";
            var connName = connStrField.Value;
            if (string.IsNullOrWhiteSpace(connName))
                return GetResult("", "Connection String Name is empty");

            foreach (ConnectionStringSettings c in ConfigurationManager.ConnectionStrings)
            {
                if (!c.Name.ToLower().Equals(connName.ToLower()))
                    continue;

                connStr = c.ConnectionString;
                break;
            }

            if (string.IsNullOrWhiteSpace(connStr))
                return GetResult("", "Connection string is empty");

            //try to instantiate object
            IDataMap map = null;
            ILogger l = new DefaultLogger();
            try
            {
                map = (IDataMap)Reflection.ReflectionUtil.CreateObject(
                        ha.Value,
                        hc.Value,
                        new object[] { currentDB, connStr, importItem, l }
                );
            }
            catch (FileNotFoundException fnfe)
            {
                var n = fnfe.Message;
                return GetResult("", $"the binary {ha.Value} could not be found");
            }

            //run process
            if (map == null)
            {
                var dbName = currentDB?.Name;
                var importItemName = importItem?.Name;
                var loggerType = l.GetType().ToString();

                return GetResult("", $"the data map provided could not be instantiated. Database:{dbName}, Connection String: {connStr}, Import Item: {importItemName}, Logger: {loggerType}");
            }

            string handleName = $"{importType}Import-{DateTime.UtcNow:yyyy/MM/dd-hh:mm}";

            var importService = new ImportProcessor(map, l);
            
            var jobOptions = new JobOptions(
                handleName,
                importItem.DisplayName,
                Context.Site.Name,
                importService,
                "Process",
                new object[] { });
            
            JobManager.Start(jobOptions);

            return GetResult(handleName, "");
        }

        public ActionResult GetJobStatus(string handleName)
        {
            Job j = JobManager.GetJob(handleName);

            var result = new JobStatusViewModel()
            {
                Current = j?.Status.Processed ?? 0,
                Total = j?.Status.Total ?? 0,
                Completed = j?.IsDone ?? true
            };

            return Json(result);
        }

        protected ActionResult GetResult(string handleName, string error)
        {
            return Json(new {
                Failed = !string.IsNullOrWhiteSpace(error),
                HandleName = handleName,
                Error = error
            }, JsonRequestBehavior.AllowGet);
        }
        
        #endregion

        #region Jobs

        public ActionResult GetJobs()
        {
            var jobs = JobManager.GetJobs().OrderBy(job => job.QueueTime).Select(a => new JobViewModel {
                Name = a.Name,
                Category = a.Category,
                State = a.Status.State.ToString(),
                Processed = a.Status.Processed.ToString(),
                Total = a.Status.Total.ToString(),
                Priority = a.Options.Priority.ToString(),
                QueueTime = a.QueueTime.ToLocalTime().ToString()
            });

            return Json(new { Jobs = jobs });
        }

        #endregion
    }
}