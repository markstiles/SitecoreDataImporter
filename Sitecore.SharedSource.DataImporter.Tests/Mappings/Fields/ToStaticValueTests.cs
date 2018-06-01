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
    public class ToStaticValueTests : BaseFakeDBTestFixture
    {
        public ToStaticValue _sut;
        public ILogger _log;
        public IDataMap _dataMap;
        public Db _database;

        [SetUp]
        public void SetUp()
        {
            _database = GetSampleDb();

            var defItem = _database.GetItem(TestingConstants.ToStaticValue.DefinitionId);

            _log = new DefaultLogger();
            _sut = new ToStaticValue(defItem, _log);
            _dataMap = Substitute.For<IDataMap>();
        }

        [Test]
        public void FillField_ValidValue_PopulatesField()
        {
            var newItem = _database.GetItem(TestingConstants.ToStaticValue.NewItemId);

            using (new EditContext(newItem))
            {
                _sut.FillField(_dataMap, ref newItem, "Some Valid Value");
            }

            var field = newItem.Fields[TestingConstants.ToStaticValue.ToFieldName];
            Assert.AreEqual(TestingConstants.ToStaticValue.ValueFieldValue, field.Value);
        }

        [Test]
        public void FillField_EmptyValue_LeavesEmptyField()
        {
            var newItem = _database.GetItem(TestingConstants.ToStaticValue.NewItemId);

            using (new EditContext(newItem))
            {
                _sut.FillField(_dataMap, ref newItem, "");
            }

            var field = newItem.Fields[TestingConstants.ToStaticValue.ToFieldName];
            Assert.AreEqual(TestingConstants.ToStaticValue.ValueFieldValue, field.Value);
        }
    }
}
