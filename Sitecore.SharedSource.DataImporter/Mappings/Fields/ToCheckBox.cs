using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.SharedSource.DataImporter.Logger;
using Sitecore.SharedSource.DataImporter.Providers;
using Sitecore.StringExtensions;

namespace Sitecore.SharedSource.DataImporter.Mappings.Fields
{

	public class ToCheckBox : ToText
	{
		#region properties

		public List<string> PositiveValuesList { get; set; }
		public List<string> NegativeValuesList { get; set; }

		#endregion

		public ToCheckBox(Item i, ILogger l) : base(i, l)
		{
			string pValues = GetItemField(i, "PositiveValues");
			string nValues = GetItemField(i, "NegativeValues");

			const string delimiter = ",";

			PositiveValuesList = (!string.IsNullOrEmpty(pValues))
					? pValues.Split(new string[] { delimiter }, StringSplitOptions.RemoveEmptyEntries).ToList()
					: new List<string>();

			NegativeValuesList = (!string.IsNullOrEmpty(nValues))
					? nValues.Split(new string[] { delimiter }, StringSplitOptions.RemoveEmptyEntries).ToList()
					: new List<string>();
		}

		#region public methods

		public override void FillField(IDataMap map, ref Item newItem, object importRow)
		{
			Field f = newItem.Fields[ToWhatField];
			if (f == null)
				return;

            var importValue = string.Join(Delimiter, map.GetFieldValues(ExistingDataNames, importRow));
            if (importValue.IsNullOrEmpty())
			{
				f.Value = "0";
				return;
			}

			bool b = PositiveValuesList.Contains(importValue);
			if (b || NegativeValuesList.Contains(importValue))
				f.Value = (b) ? "1" : "0";
			else
				map.Logger.Log("Couldn't parse the boolean value", newItem.Paths.FullPath, LogType.FieldError, Name, importValue);
		}

		#endregion
	}
}