using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Sitecore.SharedSource.DataImporter.Extensions {
	
	public static class DateTimeExtensions {
		/// <summary>
		/// Gets a DateField from a DateTime
		/// </summary>
		/// <param name="date">The current date field</param>
		/// <returns></returns>
		public static string ToDateFieldValue(this DateTime date) {
			return date.ToString("yyyyMMddTHHmmss");
		}
	}
}
