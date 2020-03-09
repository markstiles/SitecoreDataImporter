using Sitecore.Data.Items;
using Sitecore.Data.Fields;
using Sitecore.SharedSource.DataImporter.Logger;

namespace Sitecore.SharedSource.DataImporter.Mappings
{

	/// <summary>
	/// this is the class that all fields/properties should extend. 
	/// </summary>
	public class BaseMapping : IBaseMapping
	{

		#region Properties

		public Item InnerItem { get; set; }

		/// <summary>
		/// the field on the new item that the imported data should be stored in
		/// </summary>
		public string NewItemField { get; set; }

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

        public BaseMapping(Item i, ILogger l)
		{
			InnerItem = i;
			NewItemField = GetItemField(i, "To What Field");
			HandlerClass = GetItemField(i, "Handler Class");
			HandlerAssembly = GetItemField(i, "Handler Assembly");
            Logger = l;
		}

		#endregion Constructor

		#region Methods
		public string ItemName()
		{
			return (InnerItem != null) ? InnerItem.DisplayName : string.Empty;
		}

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

		#endregion Methods
	}
}
