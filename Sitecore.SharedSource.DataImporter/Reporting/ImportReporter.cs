using Sitecore.Data.Items;
using Sitecore.SharedSource.DataImporter.Mappings;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Sitecore.SharedSource.DataImporter.Reporting
{
    public static class ImportReporter
    {
        public static Dictionary<string, ItemReport> ItemReports = new Dictionary<string, ItemReport>();

        public static void Print()
        {
            try
            {
                string timeStamp = DateTime.Now.ToString("yyyyMMdd.HHmmss", CultureInfo.InvariantCulture);
                Dictionary<Level, string> logStreams = new Dictionary<Level, string>();
                string tempPath = HttpContext.Current.Server.MapPath("~/temp/");
                string dataImportPathFolder = "DataImport";


                if (!Directory.Exists(tempPath))
                {
                    Directory.CreateDirectory(tempPath);
                }

                if (!Directory.Exists(tempPath + dataImportPathFolder))
                {
                    Directory.CreateDirectory(tempPath + dataImportPathFolder);
                }

                string reportPath = tempPath + dataImportPathFolder + "\\";

                List<LogEntry> infoEntries = new List<LogEntry>();
                List<LogEntry> warningsEntries = new List<LogEntry>();
                List<LogEntry> errorEntries = new List<LogEntry>();


                foreach (var item in ItemReports.Values)
                {
                    var exportData = item.LogEntries.Where(l => l.Type == Level.Info);

                    if (exportData.Count() > 0) {
                        infoEntries.AddRange(exportData.ToList());
                    }

                    exportData = item.LogEntries.Where(l => l.Type == Level.Warning);
                    if (exportData.Count() > 0)
                    {
                        warningsEntries.AddRange(exportData.ToList());
                    }

                    exportData = item.LogEntries.Where(l => l.Type == Level.Error);
                    if (exportData.Count() > 0) {
                        errorEntries.AddRange(exportData.ToList());
                    }
                }

                if (infoEntries.Any())
                {
                    logStreams.Add(Level.Info, reportPath + "ContentMigrationReport.info." + timeStamp + ".csv");
                }

                if (warningsEntries.Any())
                {
                    logStreams.Add(Level.Warning, reportPath + "ContentMigrationReport.warnings." + timeStamp + ".csv");
                }

                if (errorEntries.Any())
                {
                    logStreams.Add(Level.Error, reportPath + "ContentMigrationReport.errors." + timeStamp + ".csv");

                    //errorEntries.Add(new LogEntry("No errors have been logged", string.Empty, Level.Error, string.Empty, string.Empty, string.Empty));
                }


                foreach (var log in logStreams)
                {
                    FileStream stream = new FileStream(log.Value, FileMode.Create);
                    using (CsvFileWriter writer = new CsvFileWriter(stream))
                    {
                        CsvRow row = new CsvRow();
                        row.Add(string.Format("{0},{1},{2},{3},{4},{5},{6}", "Item ID", "Item Name", "Item Path", "Item Fetch URL", "Field Name", "Message", "Operation"));
                        writer.WriteRow(row);

                        foreach (var item in ItemReports.Values)
                        {
                            if (item.LogEntries.Count > 0)
                            {
                                foreach (var logEntry in item.LogEntries)
                                {
                                    if (logEntry.Type == log.Key)
                                    {
                                        row = new CsvRow();
                                        row.Add(string.Format("{0},{1},{2},{3},{4},{5},{6}", logEntry.ID, item.ItemName, item.NewItemPath, item.ItemFetchPath, logEntry.FieldName, logEntry.Text, logEntry.Operation));
                                        writer.WriteRow(row);
                                    }
                                }
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
               Diagnostics.Log.Error("DIM-Report: Unable to creat report files for DataImport module.", typeof(ImportReporter));
            }
            
        }


        public static void Write(Item item,Level level,string message,string fieldName, string operation, string itemFetchURL = "")
        {
            ItemReport itemReport;

            if (!ItemReports.ContainsKey(item.Paths.Path))
            {
                ItemReports.Add(item.Paths.Path, new ItemReport());
            }
            if (!ItemReports.TryGetValue(item.Paths.Path, out itemReport))
            {
                //throw new NullReferenceException("Could not create the item report.");
            }

            if (!string.IsNullOrEmpty(itemFetchURL))
            {
                itemReport.ItemFetchPath = itemFetchURL;
            }

            if (string.IsNullOrEmpty(itemReport.ItemName))
            {
                itemReport.ItemName = item.Name;
            }

            if (string.IsNullOrEmpty(itemReport.NewItemPath))
            {
                itemReport.NewItemPath = item.Paths.Path;
            }

            // public LogEntry(string itemFetchURL, Level type, string fieldName, string operation, string text)
            LogEntry entry = new LogEntry(
                item.ID.ToString(),
                itemFetchURL,
                level,
                fieldName,
                operation,
                message
                );

            if (!itemReport.LogEntries.Exists(e => e.FieldName == entry.FieldName && e.Text == entry.Text && e.Type == entry.Type))
            {
                itemReport.LogEntries.Add(entry);
            }
        }
    }
    public class ItemReport
    {
        public string ItemFetchPath { get; set; }
        public string NewItemPath { get; set; }
        public string ItemName { get; set; }

        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }

        public List<LogEntry> LogEntries = new List<LogEntry>();

        public ItemReport()
        {
            DateCreated = DateTime.Now;
        }
    }

    public class LogEntry
    {
        public string ID { get; set; }
        public Level Type { get; set; }
        public string Text { get; set; }
        public string FieldName { get; set; }
        public string Operation { get; set; }
        public string ItemFetchURL { get; set; }

        public LogEntry(string id, string itemFetchURL, Level type, string fieldName, string operation, string text)
        {
            ID = id;
            Type = type;
            Text = text;
            FieldName = fieldName;
            Operation = operation;
            ItemFetchURL = itemFetchURL;
        }
    }

    public enum Level{
        Info,
        Error,
        Warning
    }

    public class CsvRow : List<string>
    {
        public string LineText { get; set; }
    }

    //Class gotten from https://www.codeproject.com/Articles/415732/Reading-and-Writing-CSV-Files-in-Csharp
    /// <summary>
    /// Class to write data to a CSV file
    /// </summary>
    public class CsvFileWriter : StreamWriter
    {
        public CsvFileWriter(Stream stream)
            : base(stream)
        {
        }

        public CsvFileWriter(string filename)
            : base(filename)
        {
        }

        /// <summary>
        /// Writes a single row to a CSV file.
        /// </summary>
        /// <param name="row">The row to be written</param>
        public void WriteRow(CsvRow row)
        {
            StringBuilder builder = new StringBuilder();
            bool firstColumn = true;
            foreach (string value in row)
            {
                // Add separator if this isn't the first value
                if (!firstColumn)
                    builder.Append(',');
                // Implement special handling for values that contain comma or quote
                // Enclose in quotes and double up any double quotes
                if (value.IndexOfAny(new char[] { '"', ',' }) != -1)
                    builder.AppendFormat("\"{0}\"", value.Replace("\"", "\"\""));
                else
                    builder.Append(value);
                firstColumn = false;
            }
            row.LineText = builder.ToString().Trim('"').Replace("\r", "").Replace("\n", "");
            WriteLine(row.LineText);
        }
    }
}
