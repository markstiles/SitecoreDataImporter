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
using Sitecore.FakeDb;

namespace Sitecore.SharedSource.DataImporter.Tests.Extensions
{
	[TestFixture]
	public class ExtensionTests : BaseFakeDBTestFixture
    {

	    public Db _database;

        [SetUp]
        public void Setup()
        {
            _database = GetSampleDb();

        }

        [Test]
        public void ToDateFieldTest()
        {
            DateTime dt = new DateTime(2010, 1, 1, 1, 1, 1);
            string correctDFV = "20100101T010101";
            string dfv = dt.ToDateFieldValue();
            Assert.AreEqual(correctDFV, dfv);
        }
        
        [Test]
	    public void IsId_NullTemplate_Returns_False()
	    {
	        var val = TemplateExtensions.IsID(null, TestingConstants.TemplateExtensions.TemplateID.ToString());

	        Assert.AreEqual(false, val);
	    }

	    [Test]
	    public void IsId_InvalidTemplateString_Returns_False()
	    {
	        Item i = _database.GetItem(TestingConstants.TemplateExtensions.TemplateID);

	        var val = i.Template.IsID(null);

	        Assert.AreEqual(false, val);
	    }

	    [Test]
	    public void IsId_TemplateString_Matches_ItemTemplateReturns_True()
	    {
	        Item i = _database.GetItem(TestingConstants.TemplateExtensions.PageID);

	        var val = i.Template.IsID(TestingConstants.TemplateExtensions.TemplateID.ToString());

	        Assert.AreEqual(true, val);
	    }

	    [Test]
	    public void IsId_TemplateString_IsBaseTemplate_Returns_True()
	    {
	        Item i = _database.GetItem(TestingConstants.TemplateExtensions.PageID);

	        var val = i.Template.IsID(TestingConstants.TemplateExtensions.InheritedTemplateID.ToString());

	        Assert.AreEqual(true, val);
	    }
    }
}
