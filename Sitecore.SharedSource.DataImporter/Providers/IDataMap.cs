using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Globalization;
using Sitecore.SharedSource.DataImporter.Logger;
using Sitecore.SharedSource.DataImporter.Mappings.Fields;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sitecore.SharedSource.DataImporter.Providers
{
	public interface IDataMap
	{

        #region Properties 

        Item ImportItem { get; set; }

        ILogger Logger { get; set; }

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

        List<Item> PreProcessors { get; set; }

        List<Item> PostProcessors { get; set; }

		#endregion Fields

		#region Methods

		/// <summary>
		/// gets the data to be imported
		/// </summary>
		/// <returns></returns>
		IEnumerable<object> GetImportData();

        void PreProcessItem(object importRow, Item newItem);

        void PostProcessItem(object importRow, Item newItem);

		/// <summary>
		/// Defines how the subclass will retrieve a field value
		/// </summary>
		string GetFieldValue(object importRow, string fieldName);

		/// <summary>
		/// Gets the template type for the new item that will house the importRow data
		/// </summary>
		/// <param name="importRow"></param>
		/// <returns></returns>
		CustomItemBase GetNewItemTemplate(object importRow);

		/// <summary>
		/// Gets the field definitions for any given row of imported data
		/// </summary>
		/// <param name="importRow"></param>
		/// <returns></returns>
		List<IBaseField> GetFieldDefinitionsByRow(object importRow);

		/// <summary>
		/// creates an item name based on the name field values in the data map pulled from the 
		/// </summary>
		string BuildNewItemName(object importRow);

		/// <summary>
		/// Creates new items for the import row based on the language, name, folder etc. settings
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="importRow"></param>
		/// <param name="newItemName"></param>
		Item CreateNewItem(Item parent, object importRow, string newItemName);

		/// <summary>
		/// Determines what the new parent item should be for the current import row based on foldering settings.
		/// </summary>
		/// <param name="importRow"></param>
		/// <param name="newItemName"></param>
		/// <returns></returns>
		Item GetParentNode(object importRow, string newItemName);

        IEnumerable<string> GetFieldValues(IEnumerable<string> fieldNames, object importRow);

        #endregion Methods
    }
}
