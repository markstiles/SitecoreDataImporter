using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Sitecore.SharedSource.DataImporter.Utility;

namespace Sitecore.SharedSource.DataImporter.Tests {
	[TestFixture, Category("Utility Tests")]
	public class UtilityTests {

		[Test]
		public void CleanXPathTest() {
			string s1 = "{00000000-0000-0000-0000-000000000000}";
			string s2 = "/a-b/";
			string s3 = "/#a-b#/";

			string r1 = StringUtility.CleanXPath(s1);
			string r2 = StringUtility.CleanXPath(s2);
			Assert.AreEqual(r1, s1);
			Assert.AreEqual(r2, s3);
		}

		[Test]
		public void StripInvalidCharsTest() {
			string s1 = "a>b<c:d\"[e]f?g/h\\i|j";
			string r1 = StringUtility.StripInvalidChars(s1);
			Assert.AreEqual("abcdefghij", r1);
		}

		[Test]
		public void TrimTextTest() {
			string s1 = "123456789";
			string r1 = StringUtility.TrimText(s1, 5, string.Empty);
			Assert.AreEqual(r1, "12345");
		}
	}
}
