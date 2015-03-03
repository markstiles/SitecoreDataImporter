using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sitecore.SharedSource.DataImporter.Logger {
    public class DefaultLogger : ILogger {

        public bool LoggedError { get; set; }

        /// <summary>
        /// the log is returned with any messages indicating the status of the import
        /// </summary>
        private StringBuilder log;

        public DefaultLogger(){
            LoggedError = false;
            log = new StringBuilder();
        }

        public void Log(string message) {
            log.AppendFormat("{0}", message).AppendLine().AppendLine();
        }

        /// <summary>
        /// Used to log status information while the import is processed
        /// </summary>
        /// <param name="errorType"></param>
        /// <param name="message"></param>
        public void Log(string type, string message) {
            log.AppendFormat("{0} : {1}", type, message).AppendLine().AppendLine();
        }

        public void LogError(string type, string message) {
            LoggedError = true;
            Log(type, message);
        }

        public string GetLog(){
            return log.ToString();
        }

        public void Clear() {
            log.Clear();
        }
    }
}
