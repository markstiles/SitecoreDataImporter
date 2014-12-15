using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Sitecore.SharedSource.DataImporter.Extensions;
using Sitecore.Data.Fields;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.SecurityModel;
using System.Collections.Specialized;
using Sitecore.Collections;
using StrDic = Sitecore.Collections.StringDictionary;
using Sitecore.Sites;
using Sitecore.Web;
using Sitecore.Globalization;
using Sitecore.Web.UI.WebControls;
using Sitecore.Layouts;

namespace Sitecore.SharedSource.DataImporter.Tests {
	[TestFixture, Category("Extension Tests")]
	public class ExtensionTests {

		[SetUp]
		public void SetUp() {
			
		}

		[Test]
		public void ToDateFieldTest() {
			DateTime dt = new DateTime(2010, 1, 1, 1, 1, 1);
			string correctDFV = "20100101T010101";
			string dfv = dt.ToDateFieldValue();
			Assert.AreEqual(correctDFV, dfv);
		}

		[Test]
		public void IsIDTest() {
			TemplateItem i = Sitecore.Configuration.Factory.GetDatabase("master").Items["/sitecore/content"].Template;
			bool isMain = i.IsID("{E3E2D58C-DF95-4230-ADC9-279924CECE84}");
			bool isStandard = i.IsID("{1930BBEB-7805-471A-A3BE-4858AC7CF696}");
			bool isNon = i.IsID("{00000000-0000-0000-0000-000000000000}");
			Assert.IsTrue(isMain);
			Assert.IsTrue(isStandard);
			Assert.IsFalse(isNon);
		}
	}
}
