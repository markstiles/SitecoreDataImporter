using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sitecore.SharedSource.DataImporter.Providers;

namespace Sitecore.SharedSource.DataImporter.Logger {
    public class DefaultLogger : ILogger {

        public bool LoggedError { get; set; }

        /// <summary>
        /// the log is returned with any messages indicating the status of the import
        /// </summary>
        private StringBuilder log;

        private Dictionary<string, List<ImportRow>> LogRecords = new Dictionary<string, List<ImportRow>>();

        public DefaultLogger(){
            LoggedError = false;
            log = new StringBuilder();
        }
        
        public void Log(string message, string affectedItem, LogType pResult = LogType.Info, string fieldName = "", string fieldValue = "")
        {
            if (pResult.ToString().ToLower().Contains("error"))
                LoggedError = true;

            //log for ui messaging
            log.AppendFormat("{0} : {1}", pResult, message).AppendLine();
            
            //records are for csv file logging
            string fileName = pResult.ToString();
            if (!LogRecords.ContainsKey(fileName))
                LogRecords.Add(fileName, new List<ImportRow>());

            LogRecords[fileName].Add(new ImportRow { ErrorMessage = message, AffectedItem = affectedItem, FieldName = fieldName, FieldValue = fieldValue });
        }

        public string GetLog(){
            return log.ToString();
        }

        public Dictionary<string, List<ImportRow>> GetLogRecords()
        {
            return LogRecords;
        }

        public void Clear() {
            log.Clear();
        }
    }
}
