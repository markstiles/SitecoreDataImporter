using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.Data.Items;
using System.Data;
using System.Collections;
using Sitecore.SharedSource.DataImporter.Providers;

namespace Sitecore.SharedSource.DataImporter.Mappings.ReferenceFields
{
	/// <summary>
	/// The IBaseField is the interface required for all Data Importer fields to function properly
	/// </summary>
	public interface IBaseFieldWithReference : IBaseMapping {

		#region Methods


		/// <summary>
		/// Used to differentiate fields from each other
		/// </summary>
		/// <returns></returns>
		string Name { get; set; }
        
        /// <summary>
        /// This uses the imported value to modify the newly created item. 
        /// </summary>
        /// <param name="map">provides settings related to the import</param>
        /// <param name="newItem">the newly created item</param>
        /// <param name="importValue">the imported value</param>
        void FillField(IDataMap map, ref Item newItem, Item importRow, string fieldName);

        /// <summary>
        /// returns a list of the field names from the import row that you want to import into this field 
        /// </summary>
        /// <returns></returns>
        string GetExistingFieldName();

        #endregion Methods
    }
}
