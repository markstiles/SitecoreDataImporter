using Sitecore.Data.Items;
using Sitecore.Data.Fields;
using Sitecore.SharedSource.DataImporter.Logger;
using System.Collections.Generic;
using Sitecore.SharedSource.DataImporter.Providers;

namespace Sitecore.SharedSource.DataImporter.Mappings.Fields
{
	/// <summary>
	/// this is the class that all fields/properties should extend. 
	/// </summary>
	public abstract class BaseField : IBaseField
	{
        #region Properties

        public string Name { get; set; }

		public Item InnerItem { get; set; }

		/// <summary>
		/// the field on the new item that the imported data should be stored in
		/// </summary>
		public string ToWhatField { get; set; }

		/// <summary>
		/// the class that represents the field
		/// </summary>
		public string HandlerClass { get; set; }

		/// <summary>
		/// the assembly that the class representing this field is stored in
		/// </summary>
		public string HandlerAssembly { get; set; }

        public ILogger Logger { get; set; }

        #endregion Properties

        #region Constructor

        public BaseField(Item i, ILogger l)
		{
			InnerItem = i;
            Name = InnerItem?.DisplayName ?? string.Empty;
			ToWhatField = GetItemField(i, "To What Field");
			HandlerClass = GetItemField(i, "Handler Class");
			HandlerAssembly = GetItemField(i, "Handler Assembly");
            Logger = l;
		}

		#endregion Constructor

		#region Methods
        
		public string GetItemField(Item i, string fieldName)
		{
			//check item
			if (i == null)
				return string.Empty;

			//check field
			Field f = i.Fields[fieldName];
			if (f == null)
				return string.Empty;

			//check value
			return f.Value;
		}

        #endregion

        #region Abstract Methods
        
        public abstract void FillField(IDataMap map, ref Item newItem, object importRow);

        #endregion
    }
}
