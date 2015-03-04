using NUnit.Framework;
using Sitecore.SharedSource.DataImporter.Logger;
using Sitecore.SharedSource.DataImporter.Tests.DataMaps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sitecore.SharedSource.DataImporter.Tests {
    [TestFixture, Category("Processor Tests")]
    public class ProcessorTests {

        protected DefaultLogger l;

        [SetUp]
        public void Setup() {
            l = new DefaultLogger();
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ImportProcessor_NullMapTest() {            
            ImportProcessor p1 = new ImportProcessor(null, l); 
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ImportProcessor_NullLoggerTest() {
            UTDataMap dm = new UTDataMap();
            ImportProcessor p1 = new ImportProcessor(dm, null);
        }

        [Test]
        public void ImportProcessor_ImportErrorTest() {
            NullImportDataMap dm = new NullImportDataMap();
            ImportProcessor p = new ImportProcessor(dm, l);
            p.Process();

            Assert.IsTrue(l.GetLog().Contains("Import Error"));
        }

        [Test]
        public void ImportProcessor_EmptyItemNameTest() {
            EmptyItemNameDataMap dm = new EmptyItemNameDataMap();
            ImportProcessor p = new ImportProcessor(dm, l);
            p.Process();

            Assert.IsTrue(l.GetLog().Contains("Get Name Error"));
        }

        [Test]
        public void ImportProcessor_NullParentTest() {
            NullParentDataMap dm = new NullParentDataMap();
            ImportProcessor p = new ImportProcessor(dm, l);
            p.Process();

            Assert.IsTrue(l.GetLog().Contains("Get Parent Error"));
        }

        [Test]
        public void ImportProcessor_SuccessTest() {
            UTDataMap dm = new UTDataMap();
            ImportProcessor p = new ImportProcessor(dm, l);
            p.Process();

            Assert.IsTrue(l.GetLog().Contains("Success"));
        }
    }
}
