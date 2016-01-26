using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.Data.Items;
using System.Data;
using System.Collections;
using Sitecore.SharedSource.DataImporter.Providers;

namespace Sitecore.SharedSource.DataImporter.Mappings.Fields {
	/// <summary>
	/// The IBaseField is the interface required for all Data Importer fields to function properly
	/// </summary>
	public interface IBaseField {

		#region Methods

	    /// <summary>
	    /// gets the name of the field
	    /// </summary>
	    string ItemName();

        /// <summary>
        /// This uses the imported value to modify the newly created item. 
        /// </summary>
        /// <param name="map">provides settings related to the import</param>
        /// <param name="newItem">the newly created item</param>
        /// <param name="importValue">the imported value</param>
        void FillField(BaseDataMap map, ref Item newItem, string importValue);

        /// <summary>
        /// returns a list of the field names from the import row that you want to import into this field 
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> GetExistingFieldNames();

        /// <summary>
        /// returns a delimiter to use between the fields being imported
        /// </summary>
        /// <returns></returns>
        string GetFieldValueDelimiter();

		#endregion Methods
	}
}
