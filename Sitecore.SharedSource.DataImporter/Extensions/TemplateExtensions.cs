using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.Data.Items;

namespace Sitecore.SharedSource.DataImporter.Extensions
{
    public static class TemplateExtensions
    {
        /// <summary>
        /// This checks if the current template id matches the one provided and 
        /// continues checking through the basetemplates to see if any of those match as well
        /// </summary>
        /// <param name="template">the template your checking</param>
        /// <param name="templateId">the template id you'd like to match</param>
        /// <returns></returns>
        public static bool IsID(this TemplateItem template, string templateId)
        {
            bool ret = false;
            if (template == null || string.IsNullOrEmpty(templateId))
                return ret;

            if (template.ID.ToString().Equals(templateId))
                ret = true;
            else
                ret = (null != template.BaseTemplates.FirstOrDefault(t => IsID(t, templateId)));
            
            return ret;
        }
    }
}
