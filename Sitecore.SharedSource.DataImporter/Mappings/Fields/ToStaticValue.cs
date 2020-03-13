using System.Collections.Generic;
using System.Linq;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.SharedSource.DataImporter.Providers;
using Sitecore.Diagnostics;
using Sitecore.SharedSource.DataImporter.Logger;

namespace Sitecore.SharedSource.DataImporter.Mappings.Fields
{
	/// <summary>
	/// this is used to set a field to a specific predetermined value when importing data.
	/// </summary>
	public class ToStaticValue : BaseField
	{
		#region Properties
        
		public string Value { get; set; }

		#endregion Properties

		#region Constructor

		public ToStaticValue(Item i, ILogger l) : base(i, l)
		{
			Value = GetItemField(i, "Value");
		}

		#endregion Constructor

		#region IBaseField
        
		public override void FillField(IDataMap map, ref Item newItem, object importRow)
		{
			Assert.IsNotNull(newItem, "newItem");
			//ignore import value and store value provided
			Field f = newItem.Fields[ToWhatField];
			if (f != null)
				f.Value = Value;
		}
        
		#endregion IBaseField
	}
}
