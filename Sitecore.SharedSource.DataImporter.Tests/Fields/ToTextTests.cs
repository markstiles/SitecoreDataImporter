using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Sitecore.Data.Items;
using Sitecore.FakeDb;
using Sitecore.FakeDb.Serialization;
using Sitecore.SharedSource.DataImporter.Extensions;
using Sitecore.SharedSource.DataImporter.Mappings.Fields;
using Sitecore.SharedSource.DataImporter.Providers;

namespace Sitecore.SharedSource.DataImporter.Tests.Fields
{
    [TestFixture, Category("ToText Tests")]
    public class ToTextTests
    {
        [SetUp]
        public void Setup()
        {
 
        }
        
        [Test]
        public void HowToDeserializeItem() {
            using (Db db = new Db {
                new DsDbItem("/sitecore/content/home"),
                new DsDbTemplate("/sitecore/templates/Sample/Sample Item")
            })
            {
                var home = db.GetItem("/sitecore/content/home");
                Assert.IsNotNull(home);

                var sampleTemp = db.GetItem("/sitecore/templates/Sample/Sample Item");
                Assert.IsNotNull(home);
            }
        }
    }
}
