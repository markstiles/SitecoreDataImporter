using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sitecore.SharedSource.DataImporter.HtmlScraper
{
    public static class ImportReporter
    {
        public static Dictionary<string, ItemReport> ItemReports = new Dictionary<string, ItemReport>();

        public static void Print()
        {
            FileStream stream = new FileStream(@"C:\\inetpub\\wwwroot\\upmc.local\\ContentMigrationReport.csv", FileMode.OpenOrCreate);
            using (CsvFileWriter writer = new CsvFileWriter(stream))
            {
                CsvRow row = new CsvRow();
                row.Add(string.Format("{0},{1},{2},{3},{4}","Item Name","Item Path","Type","Message","Field Name"));
                writer.WriteRow(row);

                foreach (var item in ItemReports.Values)
                {
                    if(item.MessageLog.Count > 0)
                    {
                        foreach (var message in item.MessageLog)
                        {
                            row = new CsvRow();
                            row.Add(string.Format("{0},{1},{2},{3},{4}", item.ItemName, item.NewItemPath, message.Type, message.Text, message.FieldName));
                            writer.WriteRow(row);
                        }
                    }
                }
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

        public List<LogEntry> MessageLog = new List<LogEntry>();

        public ItemReport()
        {
            DateCreated = DateTime.Now;
        }
    }

    public class LogEntry
    {
        public Level Type { get; set; }
        public string Text { get; set; }
        public string FieldName { get; set; }

        public LogEntry(Level type, string text, string fieldName)
        {
            Type = type;
            Text = text;
            FieldName = fieldName;
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
