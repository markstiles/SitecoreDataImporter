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
	public interface IBaseField
    {
        string Name { get; set; }

        Item InnerItem { get; set; }

        string ToWhatField { get; set; }

        string HandlerClass { get; set; }

        string HandlerAssembly { get; set; }

        #region Methods
        
        string GetItemField(Item i, string fieldName);
        
        /// <summary>
        /// This uses the imported value to modify the newly created item. 
        /// </summary>
        /// <param name="map">provides settings related to the import</param>
        /// <param name="newItem">the newly created item</param>
        /// <param name="importValue">the imported value</param>
        void FillField(IDataMap map, ref Item newItem, object importRow);
                
        #endregion Methods
    }
}
