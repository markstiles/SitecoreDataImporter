using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.SharedSource.DataImporter.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sitecore.SharedSource.DataImporter.Extensions {
    public static class SitecoreExtensions {

        /// <summary>
        /// searches under the parent for an item whose template matches the id provided
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="TemplateID"></param>
        /// <returns></returns>
        public static Item GetChildByTemplate(this Item parent, string TemplateID) {
            IEnumerable<Item> x = from Item i in parent.GetChildren()
                                  where i.Template.IsID(TemplateID)
                                  select i;
            return (x.Any()) ? x.First() : null;
        }

        public static bool GetItemBool(this Item i, string fieldName) {
            CheckboxField cb = (CheckboxField)i.Fields[fieldName];
            return (cb == null) ? false : cb.Checked;
        }

        public static string GetItemField(this Item i, string fieldName, ILogger logger) {
            //check item
            if (i == null) {
                logger.LogError("Error", "the item is null");
                return string.Empty;
            }

            //check field
            Field f = i.Fields[fieldName];
            if (f == null) {
                logger.LogError("Error", string.Format("the field '{0}' on the item '{1}' is null", fieldName, i.Paths.FullPath));
                return string.Empty;
            }

            //check value
            string s = f.Value;
            if (string.IsNullOrEmpty(s))
                logger.Log("Warn", string.Format("the '{0}' field was not set on '{1}'", fieldName, i.Paths.FullPath));

            return s;
        }
    }
}
