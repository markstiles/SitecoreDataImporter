using System;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.SharedSource.DataImporter.Logger;
using System.Collections.Generic;
using System.Linq;
using Sitecore.SharedSource.DataImporter.Providers;

namespace Sitecore.SharedSource.DataImporter.Extensions
{
	public static class SitecoreExtensions
	{

		/// <summary>
		/// searches under the parent for an item whose template matches the id provided
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="TemplateID"></param>
		/// <returns></returns>
		public static Item GetChildByTemplate(this Item parent, string TemplateID)
		{
			IEnumerable<Item> x = from Item i in parent.GetChildren()
														where i.Template.IsID(TemplateID)
														select i;
			return x.FirstOrDefault();
		}

		public static bool GetItemBool(this Item i, string fieldName)
		{
			CheckboxField cb = (CheckboxField)i.Fields[fieldName];
			return cb?.Checked ?? false;
		}

		public static string GetItemField(this Item i, string fieldName, ILogger logger)
		{
			//check item
			if (i == null)
			{
				logger.Log("the item is null", "N/A", LogType.ImportDefinitionError, fieldName);
				return string.Empty;
			}

			//check field
			Field f = i.Fields[fieldName];
			if (f == null)
			{
				logger.Log("the field is null", i.Paths.FullPath, LogType.ImportDefinitionError, fieldName);
				return string.Empty;
			}

			//check value
			string s = f.Value;
			if (string.IsNullOrEmpty(s))
				logger.Log("the field was empty", i.Paths.FullPath, LogType.ImportDefinitionError, fieldName);

			return s;
		}


		public static DateTime GetItemDate(this Item i, string fieldName)
		{
			DateField dateField = i.Fields[fieldName];

			return dateField?.DateTime ?? DateTime.MinValue;
		}
	}
}
