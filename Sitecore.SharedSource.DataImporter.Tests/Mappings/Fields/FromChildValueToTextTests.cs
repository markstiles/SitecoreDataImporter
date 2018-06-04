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
    public class FromChildValueToTextTests : BaseFakeDBTestFixture
    {
        public FromChildValueToText _sut;
        public ILogger _log;
        public IDataMap _dataMap;
        public Db _database;

        [SetUp]
        public void SetUp()
        {
            _database = GetSampleDb();
            var defItem = _database.GetItem(TestingConstants.FromChildValueToText.DefinitionId);

            _log = new DefaultLogger();
            _sut = new FromChildValueToText(defItem, _log);
            _dataMap = Substitute.For<IDataMap>();
        }

        [Test]
        public void FillField_NoChildren_EmptyField()
        {
            var oldItem = _database.GetItem(TestingConstants.FromChildValueToText.OldItemNoChildrenId);
            var newItem = _database.GetItem(TestingConstants.FromChildValueToText.NewItemId);

            using (new EditContext(newItem))
            {
                _sut.FillField(_dataMap, ref newItem, oldItem);
            }

            var field = newItem.Fields[TestingConstants.FromChildValueToText.ToFieldName];
            Assert.IsNotNull(field);
            Assert.AreEqual(string.Empty, field.Value);
        }

        [Test]
        public void FillField_ValidChildren_PopulatesField()
        {
            var oldItem = _database.GetItem(TestingConstants.FromChildValueToText.OldItemId);
            var newItem = _database.GetItem(TestingConstants.FromChildValueToText.NewItemId);

            using (new EditContext(newItem))
            {
                _sut.FillField(_dataMap, ref newItem, oldItem);
            }

            var field = newItem.Fields[TestingConstants.FromChildValueToText.ToFieldName];
            Assert.IsNotNull(field);
            Assert.AreEqual(TestingConstants.FromChildValueToText.FromFieldValue, field.Value);
        }
    }
}
