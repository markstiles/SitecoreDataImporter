using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.Data.Items;
using Sitecore.Data;
using System.IO;
using Microsoft.VisualBasic.FileIO;
using Sitecore.Resources.Media;
using Sitecore.SharedSource.DataImporter.Extensions;
using Sitecore.SharedSource.DataImporter.Logger;

namespace Sitecore.SharedSource.DataImporter.Providers
{
	public class CSVDataMap : BaseDataMap
	{

		#region Properties

		public string FieldDelimiter { get; set; }

		public string EncodingType { get; set; }

		#endregion Properties

		#region Constructor

		public CSVDataMap(Database db, string ConnectionString, Item importItem, ILogger l)
						: base(db, ConnectionString, importItem, l)
		{

			FieldDelimiter = ImportItem.GetItemField("Field Delimiter", Logger);
			EncodingType = ImportItem.GetItemField("Encoding Type", Logger);
		}

		#endregion Constructor

		#region IDataMap Methods

		/// <summary>
		/// uses the query field to retrieve file data
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<object> GetImportData()
		{
		    var isSitecorePath = Query.StartsWith("/sitecore/");

            if (!isSitecorePath && !File.Exists(this.Query))
			{
				Logger.Log($"the file: '{Query}' could not be found. Try moving the file under the webroot.", "N/A", LogType.Error);
				return Enumerable.Empty<object>();
			}
            
			string data = (isSitecorePath) ? GetContentString(Query) : GetFileString(Query);

		    var cleanData = data
		        .Replace("‘", "'")
		        .Replace("–", "&mdash;")
                .Replace("…", "&hellip;")
                .Replace("�", "");
                
			//split urls by breaklines
			List<string> lines = SplitString(cleanData, "\n");
			lines.RemoveAt(0);

			return lines;
		}
        
		/// <summary>
		/// gets a field value from an item
		/// </summary>
		/// <param name="importRow"></param>
		/// <param name="fieldName"></param>
		/// <returns></returns>
		public override string GetFieldValue(object importRow, string fieldName)
		{

			string item = importRow as string;
			List<string> cols = SplitColumns(item, FieldDelimiter);

			int pos = -1;
			string s = string.Empty;
			if (int.TryParse(fieldName, out pos) && (cols[pos] != null))
				s = cols[pos];
			return s;
		}

		#endregion IDataMap Methods

		#region Methods

		protected List<string> SplitString(string str, string splitter)
		{
			// string split options set to none so that empty columns are allowed
			// useful for importing large csv files, so you don't have to check the content
			return str.Split(new string[] { splitter }, StringSplitOptions.None).ToList();
		}

	    protected List<string> SplitColumns(string str, string splitter)
	    {
	        TextFieldParser parser = new TextFieldParser(new StringReader(str))
	        {
	            HasFieldsEnclosedInQuotes = true,
	            Delimiters = new[] { splitter }
	        };

	        var fields = parser.ReadFields();
	        parser.Close();

	        // string split options set to none so that empty columns are allowed
	        // useful for importing large csv files, so you don't have to check the content
	        return fields?.ToList() ?? new List<string>();
	    }

        protected string GetContentString(string contentPath)
	    {
	        var fileItem = (MediaItem)ToDB.GetItem(Query);
	        if (fileItem == null)
	            return string.Empty;

	        using (var reader = new StreamReader(MediaManager.GetMedia(fileItem).GetStream().Stream))
	        {
	            string text = reader.ReadToEnd();

	            return text;
	        }
        }
        
        protected string GetFileString(string filePath)
		{
			//open the file selected
			FileInfo f = new FileInfo(filePath);
			FileStream s = f.OpenRead();
			byte[] bytes = new byte[s.Length];
			s.Position = 0;
			int currentBytesRead = 0;
			int totalBytesRead = 0;
			while (s.Read(bytes, 0, int.Parse(s.Length.ToString())) > 0)
			{
				totalBytesRead += currentBytesRead;
			}

		    Encoding et = Encoding.GetEncoding("utf-8");
		    int ei = -1;
		    if (!EncodingType.Equals(""))
		    {
		        et = (int.TryParse(EncodingType, out ei))
		            ? Encoding.GetEncoding(ei)
		            : Encoding.GetEncoding(EncodingType);
		    }

            return et.GetString(bytes);
		}

		#endregion Methods
	}
}
