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
    public class ListToGuidTests : BaseFakeDBTestFixture
    {
        public ListToGuid _sut;
        public ILogger _log;
        public IDataMap _dataMap;
        public Db _database;

        [SetUp]
        public void SetUp()
        {
            _database = GetSampleDb();

            var defItem = _database.GetItem(TestingConstants.ListToGuid.DefinitionId);

            _log = new DefaultLogger();
            _sut = new ListToGuid(defItem, _log);
            _dataMap = Substitute.For<IDataMap>();
            _dataMap.ItemNameMaxLength = 20;
        }

        [Test]
        public void FillField_ValidValue_PopulatesField()
        {
            var newItem = _database.GetItem(TestingConstants.ListToGuid.NewItemId);
            
            _database.Configuration.Settings["InvalidItemNameChars"] = @"\/:?|[]";
            using (new EditContext(newItem))
            {
                _sut.FillField(_dataMap, ref newItem, TestingConstants.ListToGuid.SourceItemName);
            }

            var field = newItem.Fields[TestingConstants.ListToGuid.ToFieldName];
            Assert.AreEqual(TestingConstants.ListToGuid.SourceItemId.ToString(), field.Value);
        }

        [Test]
        public void FillField_InvalidValue_LeavesEmptyField()
        {
            var newItem = _database.GetItem(TestingConstants.ListToGuid.NewItemId);

            using (new EditContext(newItem))
            {
                _sut.FillField(_dataMap, ref newItem, "not a real value");
            }

            var field = newItem.Fields[TestingConstants.ListToGuid.ToFieldName];
            Assert.AreEqual("", field.Value);
        }

        [Test]
        public void FillField_EmptyValue_LeavesEmptyField()
        {
            var newItem = _database.GetItem(TestingConstants.ListToGuid.NewItemId);

            using (new EditContext(newItem))
            {
                _sut.FillField(_dataMap, ref newItem, "");
            }

            var field = newItem.Fields[TestingConstants.ListToGuid.ToFieldName];
            Assert.AreEqual("", field.Value);
        }
    }
}
