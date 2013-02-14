using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Sitecore.SharedSource.DataImporter.Extensions {
	
	public static class StringExtensions {
		/// <summary>
		/// This method checks to see if there are any sections in the xpath that contain dashes and wraps them in pound signs if it does unless it's a guid
		/// </summary>
		/// <param name="s">
		/// The XPath string
		/// </param>
		/// <returns>
		/// Returns a string that will run without validation errors
		/// </returns>
		public static string CleanXPath(this string s) {

			string scQuery = s;

			//loop through each match and replace it in the query with the escaped pattern
			char[] splitArr = { '/' };
			string[] strArr = scQuery.Split(splitArr);

			//search for {6E729CE5-558A-4851-AA30-4BB019E5F9DB}
			string re1 = ".*?";	// Non-greedy match on filler
			string re2 = "([A-Z0-9]{8}-[A-Z0-9]{4}-[A-Z0-9]{4}-[A-Z0-9]{4}-[A-Z0-9]{12})";	// SQL GUID 1

			Regex r = new Regex(re1 + re2, RegexOptions.IgnoreCase | RegexOptions.Singleline);

			for (int z = 0; z <= strArr.Length - 1; z++) {
				Match m = r.Match(strArr[z]);

				//if it contains a dash and it's not a guid
				if ((strArr[z].Contains("-") || (strArr[z].StartsWith("0"))) && !m.Success) {
					strArr[z] = "#" + strArr[z] + "#";
				}
			}
			scQuery = string.Join("/", strArr);

			return scQuery;
		}


		public static string TruncateWithEllipsis(this string s, int maxLength) {
			if (s.Length <= maxLength) {
				return s;
			}

			var strTruncated = s;
			// remove the space for the ellipsis
			maxLength -= 3;

			int lastSpaceIndex = strTruncated.LastIndexOf(" ", maxLength, StringComparison.CurrentCultureIgnoreCase);

			strTruncated = strTruncated.Remove(lastSpaceIndex > -1 ? lastSpaceIndex : maxLength);

			return string.Format("{0}...", strTruncated);
		}
	}

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
