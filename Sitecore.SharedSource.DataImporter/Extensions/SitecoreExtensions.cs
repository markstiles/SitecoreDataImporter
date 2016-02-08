using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.SharedSource.DataImporter.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sitecore.SharedSource.DataImporter.Providers;

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
                logger.Log("N/A", "the item is null", ProcessStatus.ImportDefinitionError, fieldName);
                return string.Empty;
            }

            //check field
            Field f = i.Fields[fieldName];
            if (f == null) {
                logger.Log(i.Paths.FullPath, "the field is null", ProcessStatus.ImportDefinitionError, fieldName);
                return string.Empty;
            }

            //check value
            string s = f.Value;
            if (string.IsNullOrEmpty(s))
                logger.Log(i.Paths.FullPath, "the field was empty", ProcessStatus.ImportDefinitionError, fieldName);

            return s;
        }
    }
}
