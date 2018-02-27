using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Sitecore.Data.Items;

namespace Sitecore.SharedSource.DataImporter.Utility
{
    public static class StringUtility
    {
		private static IReadOnlyCollection<string> EscapeWords = new []{"ancestor", "and", "child", "descendant", "div", "false", "following", "mod", "or", "parent", "preceding", "self", "true", "xor"};
        /// <summary>
        /// This method wraps each section in pound signs unless it's a guid or field query
        /// </summary>
        /// <param name="s">
        /// The XPath string
        /// </param>
        /// <returns>
        /// Returns a string that will run without validation errors
        /// </returns>
        public static string CleanXPath(string s)
        {

            string scQuery = s;

            //loop through each match and replace it in the query with the escaped pattern
            char[] splitArr = { '/' };
            string[] strArr = scQuery.Split(splitArr);

            //search for {6E729CE5-558A-4851-AA30-4BB019E5F9DB}
            string re1 = ".*?";	// Non-greedy match on filler
            string re2 = "([A-Z0-9]{8}-[A-Z0-9]{4}-[A-Z0-9]{4}-[A-Z0-9]{4}-[A-Z0-9]{12})";	// SQL GUID 1

            Regex r = new Regex(re1 + re2, RegexOptions.IgnoreCase | RegexOptions.Singleline);

            for (int z = 0; z <= strArr.Length - 1; z++)
            {
                Match m = r.Match(strArr[z]);

				//if it's not a guid, empty, or starts with an asterisk
				if ((strArr[z].Contains("-") || (strArr[z].StartsWith("0")) || strArr[z].Split(' ').Intersect(EscapeWords).Any()) && !m.Success)
				{
                    strArr[z] = "#" + strArr[z] + "#";
                }
            }
            scQuery = string.Join("/", strArr);

            return scQuery;
        }

        /// <summary>
        /// This is used to get a trimmed down name suitable for Sitecore
        /// </summary>
        public static string GetValidItemName(string nameValue, int maxLength)
        {
            string newItemName = StripInvalidChars(ItemUtil.ProposeValidItemName(nameValue));

            return TrimText(newItemName, maxLength, string.Empty);
        }

        public static string TrimText(string val, int maxLength, string endingString)
        {
            string strRetVal = val;
            return (val.Length > maxLength) ? val.Substring(0, maxLength) + endingString : strRetVal;
        }

        public static string StripInvalidChars(string val)
        {
			StringBuilder sb = new StringBuilder();
			Dictionary<char,char> invalid = Sitecore.Configuration.Settings.InvalidItemNameChars.ToDictionary<char, char>(a => a);
			foreach(char c in val){
				if(!invalid.ContainsKey(c))
					sb.Append(c);
			}
			
            return sb.ToString().Trim();
        }
    }
}
