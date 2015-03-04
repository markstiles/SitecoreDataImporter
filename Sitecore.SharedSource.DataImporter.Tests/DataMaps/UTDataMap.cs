using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Globalization;
using Sitecore.SharedSource.DataImporter.Providers;
using Sitecore.SharedSource.DataImporter.Mappings.Fields;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sitecore.SharedSource.DataImporter.Logger;
using Sitecore.SharedSource.DataImporter.Tests.Items;

namespace Sitecore.SharedSource.DataImporter.Tests.DataMaps {
    public class UTDataMap : IDataMap {

        #region Properties

        public ILogger Logger { get; set; }

        public Database ToDB { get; set; }

        public int ItemNameMaxLength { get; set; }

        public List<IBaseField> FieldDefinitions { get; set; }

        public string DatabaseConnectionString { get; set; }

        #endregion Properties

        #region Fields

        public string Query { get; set; }

        public Item ImportToWhere { get; set; }

        public CustomItemBase ImportToWhatTemplate { get; set; }

        public string[] ItemNameFields { get; set; }

        public Language ImportToLanguage { get; set; }

        public bool FolderByDate { get; set; }

        public bool FolderByName { get; set; }

        public string DateField { get; set; }

        public TemplateItem FolderTemplate { get; set; }

        #endregion Fields

        public UTDataMap() {

        }

        #region Methods

        public virtual IEnumerable<object> GetImportData() {
            return new List<object>() { "string" };
        }

        public virtual void ProcessCustomData(ref Item newItem, object importRow) {
            return;
        }

        public virtual string GetFieldValue(object importRow, string fieldName) {
            return "FieldValue";
        }

        public virtual CustomItemBase GetNewItemTemplate(object importRow) {
            return null;
        }

        public virtual List<IBaseField> GetFieldDefinitionsByRow(object importRow) {
            return new List<IBaseField>();
        }

        public virtual string BuildNewItemName(object importRow) {
            return "NewName";
        }

        public virtual void CreateNewItem(Item parent, object importRow, string newItemName) {
            return;
        }

        public virtual Item GetParentNode(object importRow, string newItemName) {
            return new FakeItem(new FieldList(), Sitecore.Configuration.Factory.GetDatabase("master"));
        }

        #endregion Methods
    }
}
