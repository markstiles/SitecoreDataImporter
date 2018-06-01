using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Sitecore.Data.Items;
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

        [SetUp]
        public void SetUp()
        {
            var defItem = GetSampleDb().Database.GetItem(TestingConstants.ChildrenToText.DefinitionId);

            _log = new DefaultLogger();
            _sut = new ChildrenToText(defItem, _log);
            _dataMap = Substitute.For<IDataMap>();
        }

        [Test]
        public void FillField_NoChildren_EmptyField()
        {
            var oldItem = GetSampleDb().Database.GetItem(TestingConstants.ChildrenToText.OldItemNoChildren);
            var newItem = GetSampleDb().Database.GetItem(TestingConstants.ChildrenToText.NewItemId);

            using (new EditContext(newItem))
            {
                _sut.FillField(_dataMap, ref newItem, oldItem);
            }

            var field = newItem.Fields[TestingConstants.ChildrenToText.ToFieldValue];
            Assert.IsNotNull(field);
            Assert.AreEqual(string.Empty, field.Value);
        }

        [Test]
        public void FillField_ValidChildren_PopulatesField()
        {
            var oldItem = GetSampleDb().Database.GetItem(TestingConstants.ChildrenToText.OldItemWithChildren);
            var newItem = GetSampleDb().Database.GetItem(TestingConstants.ChildrenToText.NewItemId);

            using (new EditContext(newItem))
            {
                _sut.FillField(_dataMap, ref newItem, oldItem);
            }

            var field = newItem.Fields[TestingConstants.ChildrenToText.ToFieldValue];
            Assert.IsNotNull(field);
            Assert.AreEqual(string.Join("|", oldItem.Children.Select(a => a.ID.ToString())), field.Value);
        }
    }
}
