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
    public class ChildrenToTextTests : BaseFakeDBTestFixture
    {
        public ChildrenToText _sut;
        public ILogger _log;
        public IDataMap _dataMap;
        public Db _database;

        [SetUp]
        public void SetUp()
        {
            _database = GetSampleDb();
            var defItem = _database.GetItem(TestingConstants.ChildrenToText.DefinitionId);

            _log = new DefaultLogger();
            _sut = new ChildrenToText(defItem, _log);
            _dataMap = Substitute.For<IDataMap>();
        }

        [Test]
        public void FillField_NoChildren_EmptyField()
        {
            var oldItem = _database.GetItem(TestingConstants.ChildrenToText.OldItemNoChildren);
            var newItem = _database.GetItem(TestingConstants.ChildrenToText.NewItemId);

            using (new EditContext(newItem))
            {
                _sut.FillField(_dataMap, ref newItem, oldItem);
            }

            var field = newItem.Fields[TestingConstants.ChildrenToText.ToFieldName];
            Assert.IsNotNull(field);
            Assert.AreEqual(string.Empty, field.Value);
        }

        [Test]
        public void FillField_ValidChildren_PopulatesField()
        {
            var oldItem = _database.GetItem(TestingConstants.ChildrenToText.OldItemWithChildren);
            var newItem = _database.GetItem(TestingConstants.ChildrenToText.NewItemId);

            using (new EditContext(newItem))
            {
                _sut.FillField(_dataMap, ref newItem, oldItem);
            }

            var field = newItem.Fields[TestingConstants.ChildrenToText.ToFieldName];
            Assert.IsNotNull(field);
            Assert.AreEqual(string.Join("|", oldItem.Children.Select(a => a.ID.ToString())), field.Value);
        }
    }
}
