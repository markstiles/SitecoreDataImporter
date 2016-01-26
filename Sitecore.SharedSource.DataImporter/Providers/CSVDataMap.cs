using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Data;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using Sitecore.SharedSource.DataImporter.Mappings.Fields;
using System.IO;
using System.Reflection;
using System.Runtime.Remoting;
using Sitecore.Web.UI.WebControls;
using Sitecore.SharedSource.DataImporter.Extensions;
using Sitecore.SharedSource.DataImporter.Logger;

namespace Sitecore.SharedSource.DataImporter.Providers {
    public class CSVDataMap : BaseDataMap {

		#region Properties

		public string FieldDelimiter { get; set; }

		public string EncodingType { get; set; }

		#endregion Properties

        #region Constructor

		public CSVDataMap(Database db, string ConnectionString, Item importItem, ILogger l)
            : base(db, ConnectionString, importItem, l) {

			FieldDelimiter = ImportItem.GetItemField("Field Delimiter", Logger);
            EncodingType = ImportItem.GetItemField("Encoding Type", Logger);
		}

        #endregion Constructor

        #region IDataMap Methods

        /// <summary>
		/// uses the query field to retrieve file data
		/// </summary>
		/// <returns></returns>
        public override IEnumerable<object> GetImportData() {

			if (!File.Exists(this.Query)) {
				Logger.LogError("Error", string.Format("the file: '{0}' could not be found. Try moving the file under the webroot.", this.Query));
				return Enumerable.Empty<object>();
			}

			Encoding et = Encoding.GetEncoding("utf-8");
			int ei = -1;
			if(!EncodingType.Equals("")) {
				et = (int.TryParse(EncodingType, out ei)) 
                    ? Encoding.GetEncoding(ei) 
                    : Encoding.GetEncoding(EncodingType);
			}

			byte[] bytes = GetFileBytes(this.Query);
			string data = et.GetString(bytes);

			//split urls by breaklines
			List<string> lines = SplitString(data, "\n");
			
			return lines;
        }
		
		/// <summary>
		/// There is no custom data for this type
		/// </summary>
		/// <param name="newItem"></param>
		/// <param name="importRow"></param>
        public override void ProcessCustomData(ref Item newItem, object importRow) { }

		/// <summary>
		/// gets a field value from an item
		/// </summary>
		/// <param name="importRow"></param>
		/// <param name="fieldName"></param>
		/// <returns></returns>
        public override string GetFieldValue(object importRow, string fieldName) {
			
			string item = importRow as string;
			List<string> cols = SplitString(item, FieldDelimiter);
			
			int pos = -1;
			string s = string.Empty;
			if(int.TryParse(fieldName, out pos) && (cols[pos] != null))
				s = cols[pos];
			return s;
		}

        #endregion IDataMap Methods

        #region Methods

        protected List<string> SplitString(string str, string splitter){
            // string split options set to none so that empty columns are allowed
            // useful for importing large csv files, so you don't have to check the content
			return str.Split(new string[] { splitter }, StringSplitOptions.None).ToList();
		}

		protected byte[] GetFileBytes(string filePath) {
			//open the file selected
			FileInfo f = new FileInfo(filePath);
			FileStream s = f.OpenRead();
			byte[] bytes = new byte[s.Length];
			s.Position = 0;
			s.Read(bytes, 0, int.Parse(s.Length.ToString()));
			return bytes;
		}

        #endregion Methods
    }
}
