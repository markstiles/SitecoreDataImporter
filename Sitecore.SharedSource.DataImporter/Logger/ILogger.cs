using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sitecore.SharedSource.DataImporter.Logger {
    public interface ILogger {
        bool LoggedError { get; set; }
        void Log(string message);
        void Log(string type, string message);
        void LogError(string type, string message);
        string GetLog();
        void Clear();
    }
}
