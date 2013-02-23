using System;
using System.Text;
using System.Collections.Generic;
using Sitecore.Data.Items;
using Sitecore.Resources.Media;
using System.Web.UI.WebControls;
using System.Web;
using System.Text.RegularExpressions;
using Sitecore.Layouts;
using Sitecore.Data.Fields;
using System.Globalization;
using System.Collections;
using System.Linq;
using Sitecore.Collections;
using Sitecore.SharedSource.DataImporter.Utility;

namespace Sitecore.SharedSource.DataImporter.Extensions
{
    public static class ItemExtensions
    {
		/// <summary>
		/// This will determine if the current item is not null
		/// </summary>
		/// <param name="curItem">The item to check for null</param>
		/// <returns>Returns true it the current item is null false otherwise</returns>
		public static bool IsNotNull(this Item curItem) {
			return !IsNull(curItem);
		}

		/// <summary>
		/// This will determine if the current item is null
		/// </summary>
		/// <param name="curItem">The item to check for null</param>
		/// <returns>Returns true it the current item is null false otherwise</returns>
		public static bool IsNull(this Item curItem) {
			return (curItem == null) ? true : false;
		}  
    }
}