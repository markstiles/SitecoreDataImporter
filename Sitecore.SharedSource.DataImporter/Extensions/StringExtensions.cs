using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sitecore.SharedSource.DataImporter.Extensions
{
    public static class StringExtensions
    {
        public static bool IsNotNull(this string value)
        {
            return !string.IsNullOrEmpty(value);
        }
    }
}
