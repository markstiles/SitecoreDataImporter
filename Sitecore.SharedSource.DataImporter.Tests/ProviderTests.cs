using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.SharedSource.DataImporter.Providers;

namespace Sitecore.SharedSource.DataImporter.Tests {
	[TestFixture, Category("Provider Tests")]
	public class ProviderTests {

		public static readonly string CSVImportID = "{8AEC7343-3684-4E07-B319-DAD66A39920A}";
		public static readonly string SQLImportID = "{C489B55C-E72E-44AF-A3BF-4CE81946F766}";
		public static readonly string MySQLImportID = "{50BEB572-1A04-491A-8BEE-E14A94C29087}";
		public static readonly string SitecoreImportID = "{D506CACD-442F-4FAD-B6E7-B40889669C73}";

		private Database _mdb;
		public Database MasterDB {
			get {
				if(_mdb == null)
					_mdb = Sitecore.Configuration.Factory.GetDatabase("master");
				return _mdb;
			}
		}

		[SetUp]
		public void SetUp() {

		}

        #region IDataMap Tests

        //constructor / property 

		//GetDateParentNode 

		//GetNameParentNode 

		//GetItemByTemplate  

		//Log 

		//Process 

		//CreateNewItem 

		//GetNewItemTemplate 

		//GetNewItemName 

		//GetFieldValues test or maybe just rely on instances to implement the sub-method

        //GetParentNode 

        #endregion IDataMap Tests

        #region CSVDataMap Tests

        //constructor / property 
		[Test]
		public void CSV_ConstructorTest() {
			if (MasterDB == null)
				Assert.IsNull(MasterDB, "Master DB is null");

			Item importItem = MasterDB.GetItem(Sitecore.Data.ID.Parse(CSVImportID));
			if (importItem == null)
				Assert.IsNull(importItem, "Import definition is null.");

			//new import
			TextField ha = importItem.Fields["Handler Assembly"];
			if (ha == null || string.IsNullOrEmpty(ha.Value))
				Assert.IsNull(ha.Value, "Import handler assembly is not defined.");

			if (!File.Exists(string.Format(@"C:\inetpub\wwwroot\testsite-sc-7.1\Website\bin\{0}.dll", ha.Value)))
				Assert.IsNull(ha.Value, string.Format("{0} assembly to test doesn't exist", ha.Value));

			TextField hc = importItem.Fields["Handler Class"];
			if (hc == null || string.IsNullOrEmpty(hc.Value))
				Assert.IsNull(hc.Value, "Import handler class is not defined");

			string CSVFile = @"C:\inetpub\wwwroot\testsite-sc-7.1\Website\temp\test.csv";
			if (!File.Exists(CSVFile))
				Assert.IsNull(hc.Value, "Import file doesn't exist");

			CSVDataMap map = new CSVDataMap(MasterDB, CSVFile, importItem);
            //CATCH EXCEPTIONS WITH ALL FIELDS. IDataMap IS THROWING EXCEPTIONS BECAUSE OF EMPTY FIELDS. PULL LOG AND DUMP IT AS ERROR.
			if (map == null)
				Assert.IsNull(map, "the data map provided could not be instantiated");

			//test all properties 

			//map.Process();	
		}

		//GetImportData

		//ProcessCustomData

		//GetFieldValue

		//SplitString

		//GetFileBytes

		#endregion CSVDataMap Tests

		#region MySqlDataMap Tests

		//GetImportData

		#endregion MySqlDataMap Tests

		#region SitecoreDataMap Tests

		//constructor / property 

		//GetImportData

		//ProcessCustomData

		//ProcessChildren

		//GetFieldValue

		//GetNewItemTemplate

		#endregion SitecoreDataMap Tests

		#region SqlDataMap Tests

		//GetImportData

		//ProcessCustomData

		//GetFieldValue

		#endregion SqlDataMap Tests
	}
}
