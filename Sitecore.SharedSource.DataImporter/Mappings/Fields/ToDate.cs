using System;
using System.Globalization;
using System.Linq;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.SharedSource.DataImporter.Extensions;
using Sitecore.SharedSource.DataImporter.Logger;
using Sitecore.SharedSource.DataImporter.Providers;
namespace Sitecore.SharedSource.DataImporter.Mappings.Fields
{

	/// <summary>
	/// This field converts a date value to a sitecore date field value
	/// </summary>
	public class ToDate : ToText
	{

		#region Properties

		#endregion Properties

		#region Constructor

		//constructor
		public ToDate(Item i, ILogger l) : base(i, l)
		{

		}

		#endregion Constructor

		#region IBaseField

		public override void FillField(IDataMap map, ref Item newItem, object importRow, string importValue)
		{
			Field f = newItem.Fields[NewItemField];
			if (f == null)
				return;

			if (string.IsNullOrEmpty(importValue))
			{
				f.Value = string.Empty;
				return;
			}

			//try to parse date value 
			DateTime date;
			string cleanImportValue = importValue.Split(':').FirstOrDefault() ?? string.Empty;
			if (!DateTime.TryParse(cleanImportValue, out date)
					&& !DateTime.TryParseExact(cleanImportValue, new string[] { "d/M/yyyy", "d/M/yyyy HH:mm:ss", "yyyyMMddTHHmmss", "yyyyMMddTHHmmssZ" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
			{
				map.Logger.Log("Date parse error", newItem.Paths.FullPath, ProcessStatus.DateParseError, ItemName(), cleanImportValue);
				return;
			}

			f.Value = date.ToDateFieldValue();
		}

		#endregion IBaseField
	}
}
