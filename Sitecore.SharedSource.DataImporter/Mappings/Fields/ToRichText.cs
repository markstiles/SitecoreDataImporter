using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.Resources.Media;
using Sitecore.SharedSource.DataImporter.Providers;
using Sitecore.SharedSource.DataImporter.Utility;
using Sitecore.Diagnostics;
using Sitecore.Links;
using Sitecore.SharedSource.DataImporter.Logger;

namespace Sitecore.SharedSource.DataImporter.Mappings.Fields {

	public class ToRichText : MediaFileMapping, IBaseFieldWithReference
	{

		#region Properties

		public IEnumerable<string> UnwantedTags { get; set; }

		public IEnumerable<string> UnwantedAttributes { get; set; }
		public Dictionary<string, string> ReplaceTags { get; set; }
	    public Dictionary<string, string> ReplaceStrings { get; set; }

        protected readonly char[] comSplitr = { ',' };
		protected readonly char[] pipeSplitr = { '|' };
		public string Name { get; set; }

		#endregion Properties


		public string GetExistingFieldName()
		{
			return GetItemField(InnerItem, "From What Fields");
		}

		#region Constructor

		public ToRichText(Item i, ILogger logger)
			: base(i, logger)
		{
			Assert.IsNotNull(i, "i");
			//store fields
			UnwantedTags = GetItemField(i, "Unwanted Tags").Split(comSplitr, StringSplitOptions.RemoveEmptyEntries);
			UnwantedAttributes = GetItemField(i, "Unwanted Attributes")
				.Split(comSplitr, StringSplitOptions.RemoveEmptyEntries);

			ReplaceTags = new Dictionary<string, string>();
		    AddReplacements(i, "Replace Tags", ReplaceTags);
            ReplaceStrings = new Dictionary<string, string>();
		    AddReplacements(i, "Replace Strings", ReplaceStrings);
        }

	    private void AddReplacements(Item i,string fieldName, Dictionary<string, string> dictionary)
	    {
	        foreach (string pair in GetItemField(i, fieldName).Split(pipeSplitr, StringSplitOptions.RemoveEmptyEntries))
	        {
	            var tags = pair.Split(comSplitr, StringSplitOptions.RemoveEmptyEntries);
	            if (tags?.Length != 2 || string.IsNullOrWhiteSpace(tags[0]) || string.IsNullOrWhiteSpace(tags[1]))
	                continue;
	            dictionary.Add(tags[0], tags[1]);
	        }
	    }

	    #endregion Constructor

		#region IBaseField

		public void FillField(IDataMap map, ref Item newItem, Item importRow, string fieldName)
		{
			Assert.IsNotNull(newItem, "item");
			importRow.Fields.ReadAll();
			if (importRow.Fields[fieldName] == null)
			{
				return;
			}
			string importValue = importRow.Fields[fieldName].Value;
			Field f = newItem.Fields[NewItemField];
		    if (f != null)
		    {
		        f.Value = CleanHtml(map, newItem.Paths.FullPath, importValue, importRow);
		        f.Value = DoStringReplacements(f.Value);
		    }
				
		}

		#endregion IBaseField

		public string CleanHtml(IDataMap map, string itemPath, string html, Item importRow)
		{
			if (String.IsNullOrEmpty(html))
				return html;

			var document = new HtmlDocument();
			document.LoadHtml(html);

			HtmlNodeCollection tryGetNodes = document.DocumentNode.SelectNodes("./*|./text()");

			if (tryGetNodes == null || !tryGetNodes.Any())
				return html;

			var nodes = new Queue<HtmlNode>(tryGetNodes);
			
			while (nodes.Any())
			{
				HandleNextNode(nodes, map, itemPath, importRow);
			}

			return document.DocumentNode.InnerHtml;
		}

	    private string DoStringReplacements(string baseString)
	    {
            //replace all strings in the base with the values
	        return !ReplaceStrings.Any() ? baseString : ReplaceStrings.Aggregate(baseString, (current, replaceString) => current.Replace(replaceString.Key, replaceString.Value));
	    }

		private void HandleNextNode(Queue<HtmlNode> nodes, IDataMap map, string itemPath, Item importRow)
		{
			var node = nodes.Dequeue();
			var nodeName = node.Name.ToLower();
			var parentNode = node.ParentNode;
			var childNodes = node.SelectNodes("./*|./text()");

			if (childNodes != null)
			{
				foreach (var child in childNodes)
					nodes.Enqueue(child);
			}

			if (UnwantedTags.Any(tag => tag == nodeName))
			{
				// if this node is one to remove
				if (childNodes != null)
				{
					// make sure children are added back
					foreach (var child in childNodes)
						parentNode.InsertBefore(child, node);
				}

				parentNode.RemoveChild(node);
			}
			else if (node.HasAttributes)
			{
				// if it's not being removed
				foreach (string s in UnwantedAttributes) // remove unwanted attributes
					node.Attributes.Remove(s);

				//replace images
				if (nodeName.Equals("img"))
				{
					// see if it exists
					string imgSrc = node.Attributes["src"].Value;
					DynamicLink dynamicLink;
					if (!DynamicLink.TryParse(imgSrc, out dynamicLink))
						return;
					MediaItem mediaItem = importRow.Database.GetItem(dynamicLink.ItemId, dynamicLink.Language ?? map.ImportToLanguage);
					var mediaParentItem = BuildMediaPath(map.ToDB, mediaItem.InnerItem.Paths.ParentPath);
					MediaItem newImg = HandleMediaItem(map, mediaParentItem, itemPath, mediaItem);
					if (newImg != null)
					{
						string newSrc = string.Format("-/media/{0}.ashx", newImg.ID.ToShortID().ToString());
						// replace the node with sitecore tag
						node.SetAttributeValue("src", newSrc);
					}
				}
			}
			else if (ReplaceTags.ContainsKey(nodeName))
			{
				// Replace tag
				node.Name = ReplaceTags[nodeName];
			}
			else
			{
				//Keep node as is
			}
		}
	}
}
