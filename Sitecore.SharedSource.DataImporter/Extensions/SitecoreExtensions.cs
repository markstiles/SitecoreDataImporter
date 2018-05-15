using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sitecore.SharedSource.DataImporter.Providers;
using log4net;
using Sitecore.SharedSource.DataImporter.Logger;

namespace Sitecore.SharedSource.DataImporter.Extensions {
    public static class SitecoreExtensions {

        /// <summary>
        /// searches under the parent for an item whose template matches the id provided
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="TemplateID"></param>
        /// <returns></returns>
        public static Item GetChildByTemplate(this Item parent, string TemplateID) {
			if (parent == null)
			{
				return null;
			}
            IEnumerable<Item> x = from Item i in parent.GetChildren()
                                  where i.Template.IsID(TemplateID)
                                  select i;
            return (x.Any()) ? x.First() : null;
        }

        public static bool GetItemBool(this Item i, string fieldName) {
            CheckboxField cb = (CheckboxField)i?.Fields[fieldName];
            return (cb == null) ? false : cb.Checked;
        }

        public static string GetItemField(this Item i, string fieldName, ILogger logger) {
			try
			{

				//check field
				Field f = i.Fields[fieldName];
				if (f == null)
				{
					logger?.Log("GetItemField", string.Format("the field {0} is null on item {1}", fieldName,i.Paths.FullPath));
					return string.Empty;
				}

				//check value
				string s = f.Value;
				if (string.IsNullOrEmpty(s))
					logger.Log("GetItemField", string.Format("the field {0} was empty on item {1}",  fieldName, i.Paths.FullPath));

				return s;
			}
			catch (Exception ex)
			{
				logger.Log("GetItemField", string.Format("Error getting field {0}", fieldName));
				return string.Empty;
			}
		}
    }
}
