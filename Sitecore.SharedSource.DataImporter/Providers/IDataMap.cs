using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Globalization;
using Sitecore.SharedSource.DataImporter.Mappings.Fields;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sitecore.SharedSource.DataImporter.Providers {
    public interface IDataMap {

        #region Properties 

        Database ToDB { get; set; }

        int ItemNameMaxLength { get; set; }

        List<IBaseField> FieldDefinitions { get; set; }

        string DatabaseConnectionString { get; set; }

        #endregion Properties

        #region Fields

        string Query { get; set; }

        Item ImportToWhere { get; set; }

        CustomItemBase ImportToWhatTemplate { get; set; }

        string[] ItemNameFields { get; set; }

        Language ImportToLanguage { get; set; }

        bool FolderByDate { get; set; }

        bool FolderByName { get; set; }

        string DateField { get; set; }

        TemplateItem FolderTemplate { get; set; }

        #endregion Fields

        #region Methods

        /// <summary>
        /// gets the data to be imported
        /// </summary>
        /// <returns></returns>
        IEnumerable<object> GetImportData();

        /// <summary>
        /// this is used to process custom fields or properties
        /// </summary>
        void ProcessCustomData(ref Item newItem, object importRow);

        /// <summary>
        /// Defines how the subclass will retrieve a field value
        /// </summary>
        string GetFieldValue(object importRow, string fieldName);

        /// <summary>
        /// The method that process the import
        /// </summary>
        /// <returns>Log message of the import</returns>
        string Process();

        #endregion Methods
    }
}
