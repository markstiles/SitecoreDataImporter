using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Sitecore.Data.Items;
using Sitecore.FakeDb;
using Sitecore.SharedSource.DataImporter.Logger;
using Sitecore.SharedSource.DataImporter.Mappings.Fields;
using Sitecore.SharedSource.DataImporter.Providers;

namespace Sitecore.SharedSource.DataImporter.Tests.Mappings.Fields
{
    [TestFixture]
    public class ToTextTests : BaseFakeDBTestFixture
    {
        public ToText _sut;
        public ILogger _log;
        public IDataMap _dataMap;
        public Db _database;

        [SetUp]
        public void SetUp()
        {
            _database = GetSampleDb();

            var defItem = _database.GetItem(TestingConstants.ToText.DefinitionId);

            _log = new DefaultLogger();
            _sut = new ToText(defItem, _log);
            _dataMap = Substitute.For<IDataMap>();
        }
        
    }
}
