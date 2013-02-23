using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.Data.Items;

namespace Sitecore.SharedSource.DataImporter.Extensions
{
    public static class TemplateExtensions
    {
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
