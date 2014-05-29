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

namespace Sitecore.SharedSource.DataImporter.Providers {
	public class CSVDataMap : BaseDataMap {

		#region Properties
		
		private static ASCIIEncoding _encoding = null;
		public static ASCIIEncoding encoding {
			get {
				if (_encoding == null) {
					_encoding = new System.Text.ASCIIEncoding();
				}
				return _encoding;
			}
		}

		#endregion Properties

        #region Constructor

		public CSVDataMap(Database db, string ConnectionString, Item importItem)
            : base(db, ConnectionString, importItem) {
        }

        #endregion Constructor

        #region Override Methods

		/// <summary>
		/// uses the query field to retrieve file data
		/// </summary>
		/// <returns></returns>
        public override IEnumerable<object> GetImportData() {

			if (!File.Exists(this.Query))
				return Enumerable.Empty<object>();

			byte[] bytes = GetFileBytes(this.Query);
			string data = encoding.GetString(bytes);

			//split urls by breaklines
			List<string> lines = SplitString(data, "\n");
			
			return lines;
        }
		
		/// <summary>
		/// There is no custom data for this type
		/// </summary>
		/// <param name="newItem"></param>
		/// <param name="importRow"></param>
		public override void ProcessCustomData(ref Item newItem, object importRow) {
		}

		/// <summary>
		/// gets a field value from an item
		/// </summary>
		/// <param name="importRow"></param>
		/// <param name="fieldName"></param>
		/// <returns></returns>
		protected override string GetFieldValue(object importRow, string fieldName) {
			
			string item = importRow as string;
			List<string> lines = SplitString(item, ",");
			
			int pos = -1;
			string s = string.Empty;
			if(int.TryParse(fieldName, out pos) && (lines[pos] != null))
				s = lines[pos];
			return s;
		}

		#endregion Override Methods

        #region Methods
		
		protected List<string> SplitString(string str, string splitter){
			return str.Split(new string[] { splitter }, StringSplitOptions.RemoveEmptyEntries).ToList();
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
