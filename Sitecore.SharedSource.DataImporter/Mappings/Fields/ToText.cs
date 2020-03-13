using System;
using System.Collections.Generic;
using HtmlAgilityPack;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.SharedSource.DataImporter.Providers;
using Sitecore.Diagnostics;
using Sitecore.SharedSource.DataImporter.Logger;

namespace Sitecore.SharedSource.DataImporter.Mappings.Fields
{
	/// <summary>
	/// this stores the plain text import value as is into the new field
	/// </summary>
	public class ToText : BaseField
	{
		#region Properties
        
		protected readonly char[] comSplitr = { ',' };
		public IEnumerable<string> ExistingDataNames { get; set; }
		public string Delimiter { get; set; }
		public bool StripHtml { get; set; }
		public bool SkipIfEmpty { get; set; }

		#endregion Properties

		#region Constructor

		public ToText(Item i, ILogger l) : base(i, l)
		{
			//store fields
			ExistingDataNames = GetItemField(i, "From What Fields").Split(comSplitr, StringSplitOptions.RemoveEmptyEntries);
			Delimiter = GetItemField(i, "Delimiter");
			StripHtml = GetItemField(i, "Strip HTML") == "1";
			SkipIfEmpty = GetItemField(i, "Skip If Empty") == "1";
		}

		#endregion Constructor

		#region IBaseField
        
		public override void FillField(IDataMap map, ref Item newItem, object importRow)
		{
            var importValue = string.Join(Delimiter, map.GetFieldValues(ExistingDataNames, importRow));

            Assert.IsNotNull(newItem, "newItem");
			if (SkipIfEmpty && string.IsNullOrEmpty(importValue)) return;

			//store the imported value as is
			Field f = newItem.Fields[ToWhatField];
			if (f != null)
				f.Value = StripHtml ? RemoveHtml(importValue) : importValue;
		}

		protected string RemoveHtml(string html)
		{
			if (String.IsNullOrEmpty(html))
				return html;

			var document = new HtmlDocument();
			document.LoadHtml(html);

			return document.DocumentNode.InnerText;
		}

		private void HandleNextNode(Queue<HtmlNode> nodes)
		{
			var node = nodes.Dequeue();
			var parentNode = node.ParentNode;
			var childNodes = node.SelectNodes("./*|./text()");

			if (childNodes != null)
			{
				foreach (var child in childNodes)
					nodes.Enqueue(child);
			}

			// if this node is one to remove
			if (childNodes != null)
			{
				// make sure children are added back
				foreach (var child in childNodes)
					parentNode.InsertBefore(child, node);
			}

			parentNode.RemoveChild(node);
		}
        
		#endregion IBaseField
	}
}
