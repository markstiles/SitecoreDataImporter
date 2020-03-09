using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Sitecore.Data;
using Sitecore.Data.Items;

namespace Sitecore.SharedSource.DataImporter.Services
{
	public class StringService
	{
        public List<Match> FindIDsInText(string value)
        {
            var guidRegex = @"[({]?[a-fA-F0-9]{8}[-]?([a-fA-F0-9]{4}[-]?){3}[a-fA-F0-9]{12}[})]?";
            var matchList = Regex.Matches(value, guidRegex, RegexOptions.None);

            var matches = matchList.Cast<Match>().ToList();

            return matches;
        }

        public string GetValidItemName(string nameValue, int maxLength)
		{
			string newItemName = StripInvalidChars(ItemUtil.ProposeValidItemName(nameValue));

			return TrimText(newItemName, maxLength, string.Empty);
		}

		public string TrimText(string val, int maxLength, string endingString)
		{
			string strRetVal = val;
			return (val.Length > maxLength) ? val.Substring(0, maxLength) + endingString : strRetVal;
		}

		public string StripInvalidChars(string val)
		{
			StringBuilder sb = new StringBuilder();
			Dictionary<char, char> invalid = Sitecore.Configuration.Settings.InvalidItemNameChars.ToDictionary<char, char>(a => a);
			foreach (char c in val)
			{
				if (!invalid.ContainsKey(c))
					sb.Append(c);
			}

			return sb.ToString().Trim();
		}

		public bool IsInt(string val)
		{
			int i = 0;
			return int.TryParse(val, out i);
		}
	}
}
