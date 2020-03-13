using HtmlAgilityPack;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.SharedSource.DataImporter.Logger;
using Sitecore.SharedSource.DataImporter.Providers;

namespace Sitecore.SharedSource.DataImporter.Mappings.Fields
{
	public class HtmlFragmentToText : ToText
	{
		public string FromNode { get; set; }
		public string FromAttributeValue { get; set; }

		public HtmlFragmentToText(Item i, ILogger l) : base(i, l)
		{
			FromNode = GetItemField(i, "From Node");
			FromAttributeValue = GetItemField(i, "From Attribute Value");
		}

		public override void FillField(IDataMap map, ref Item newItem, object importRow)
		{
			Assert.IsNotNull(newItem, "newItem");

            var importValue = string.Join(Delimiter, map.GetFieldValues(ExistingDataNames, importRow));
            if (string.IsNullOrEmpty(importValue))
				return;

			//store the imported value as is
			Field f = newItem.Fields[ToWhatField];
            string fragment = GetFragment(importValue);

            if (f != null && !string.IsNullOrEmpty(fragment))
                f.Value = fragment;				
		}

		private string GetFragment(string importValue)
		{
			var document = new HtmlDocument();
			document.LoadHtml(importValue);

			var node = document.DocumentNode.SelectSingleNode(FromNode);

			if (node == null)
                return null;

			var returnValue = string.IsNullOrEmpty(FromAttributeValue)
				? node.InnerHtml
                : node.Attributes[FromAttributeValue]?.Value;

            return returnValue;
		}
	}
}