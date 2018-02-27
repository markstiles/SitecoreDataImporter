using NUnit.Framework;
using Sitecore.SharedSource.DataImporter.Logger;
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

        
    }
}
