using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Sitecore.FakeDb;
using Sitecore.SharedSource.DataImporter.Services;

namespace Sitecore.SharedSource.DataImporter.Tests.Utility {
	[TestFixture, Category("Utility Tests")]
	public class UtilityTests : BaseFakeDBTestFixture {
        
		[Test]
		public void StripInvalidCharsTest()
        {
            using (Db db = GetSampleDb())
		    {
                var StringService = new StringService();

                db.Configuration.Settings["InvalidItemNameChars"] = @"\/:?|[]";
                string s1 = "abc:d[e]f?g/h\\i|j";
		        string r1 = StringService.StripInvalidChars(s1);
		        Assert.AreEqual("abcdefghij", r1);
		    }
		}

		[Test]
		public void TrimTextTest()
        {
            var StringService = new StringService();

            string s1 = "123456789";
			string r1 = StringService.TrimText(s1, 5, string.Empty);
			Assert.AreEqual(r1, "12345");
		}
	}
}
