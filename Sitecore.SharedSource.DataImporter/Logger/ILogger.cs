using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sitecore.SharedSource.DataImporter.Providers;

namespace Sitecore.SharedSource.DataImporter.Logger {
    public interface ILogger {
        bool LoggedError { get; set; }

        void Log(string message, string affectedItem, ProcessStatus pResult = ProcessStatus.Info, string fieldName = "", string fieldValue = "");
        string GetLog();
        void Clear();
        Dictionary<string, List<ImportRow>> GetLogRecords();
    }
}
