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
    public class DateToTextTests : BaseFakeDBTestFixture
    {
        public DateToText _sut;
        public ILogger _log;
        public IDataMap _dataMap;
        public Db _database;

        [SetUp]
        public void SetUp()
        {
            _database = GetSampleDb();

            var defItem = _database.GetItem(TestingConstants.DateToText.DefinitionId);
            
            _log = new DefaultLogger();
            _sut = new DateToText(defItem, _log);
            _dataMap = Substitute.For<IDataMap>();
        }
        
        [Test]
        public void FillField_ValidValue_PopulatesField()
        {
            var oldItem = _database.GetItem(TestingConstants.DateToText.OldItemId);
            var newItem = _database.GetItem(TestingConstants.DateToText.NewItemId);

            using (new EditContext(newItem))
            {
                _sut.FillField(_dataMap, ref newItem, oldItem, "");
            }

            var field = newItem.Fields[TestingConstants.DateToText.ToFieldName];
            Assert.IsNotNull(field);
            var dateValue = DateUtil.IsoDateToDateTime(TestingConstants.DateToText.FromFieldValue)
                .ToString("MMMM d, yyyy");
            Assert.AreEqual(dateValue, field.Value);
        }
    }
}

