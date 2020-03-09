
using System;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.SharedSource.DataImporter.Providers;
using Sitecore.Diagnostics;
using Sitecore.SharedSource.DataImporter.Logger;
using Sitecore.SharedSource.DataImporter.Services;

namespace Sitecore.SharedSource.DataImporter.Mappings.Fields
{
	public class ToRichText : ToText
	{
		#region Properties

		public IEnumerable<string> UnwantedTags { get; set; }

		public IEnumerable<string> UnwantedAttributes { get; set; }

		public Item MediaParentItem { get; set; }

		public Database FromDB { get; set; }

        protected HtmlService HtmlService { get; set; }

        protected MediaService MediaService { get; set; }

        #endregion Properties

        #region Constructor

        public ToRichText(Item i, ILogger l) : base(i, l)
		{
			Assert.IsNotNull(i, "i");
			//store fields
			UnwantedTags = GetItemField(i, "Unwanted Tags").Split(comSplitr, StringSplitOptions.RemoveEmptyEntries);
			UnwantedAttributes = GetItemField(i, "Unwanted Attributes")
				.Split(comSplitr, StringSplitOptions.RemoveEmptyEntries);
			MediaParentItem = i.Database.GetItem(GetItemField(i, "Media Parent Item"));
			FromDB = Factory.GetDatabase("master");
            HtmlService = new HtmlService();
            MediaService = new MediaService(l);
        }

		#endregion Constructor

		#region IBaseField

		public override void FillField(IDataMap map, ref Item newItem, object importRow, string importValue)
		{
			Assert.IsNotNull(newItem, "item");
			Field f = newItem.Fields[NewItemField];
			if (f != null)
				f.Value = CleanHtml(map, newItem.Paths.FullPath, importValue);
		}

		#endregion IBaseField

		public string CleanHtml(IDataMap map, string itemPath, string html)
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
				HandleNextNode(nodes, map, itemPath);
			}

			var cleanedHtml = document.DocumentNode.InnerHtml;

			bool modified = false;
			string fixedHtml = HtmlService.FixOrphanedText(cleanedHtml, out modified);
			if (modified)
			{
				map.Logger.Log("Fixed Orphaned Text in Rich Text.", itemPath);
			}

			return fixedHtml;
		}

		private void HandleNextNode(Queue<HtmlNode> nodes, IDataMap map, string itemPath)
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
				if (nodeName.Equals("img") || nodeName.Equals("script"))
				{
					// see if it exists
					string imgSrc = node.Attributes.Contains("src") ? node.Attributes["src"].Value : string.Empty;
					if (!string.IsNullOrEmpty(imgSrc))
					{
						MediaItem newImg = HandleMedia(map, itemPath, imgSrc);
						if (newImg != null)
						{
							string newSrc = string.Format("-/media/{0}.ashx", newImg.ID.ToShortID());
							// replace the node with sitecore tag
							node.SetAttributeValue("src", newSrc);
						}
					}
				}
				else if (nodeName.Equals("a") || nodeName.Equals("link"))
				{
					if (nodeName.Equals("a") && node.Attributes.Contains("target"))
					{
						string target = node.Attributes.Contains("target") ? node.Attributes["target"].Value : string.Empty;
						if (target.Equals("_blank", StringComparison.InvariantCultureIgnoreCase))
						{
							node.SetAttributeValue("rel", "noopener noreferrer");
						}
					}

					// see if it exists
					string linkHref = node.Attributes.Contains("href") ? node.Attributes["href"].Value : string.Empty;
					if (!string.IsNullOrEmpty(linkHref))
					{
						MediaItem newImg = HandleMedia(map, itemPath, linkHref);
						if (newImg != null)
						{
							string newHref = string.Format("-/media/{0}.ashx", newImg.ID.ToShortID());
							// replace the node with sitecore tag
							node.SetAttributeValue("href", newHref);
						}
					}
				}
			}
			else
			{
				//Keep node as is
			}
		}

		public MediaItem HandleMedia(IDataMap map, string itemPath, string url)
		{
			Assert.IsNotNull(map, "map");
			Assert.IsNotNull(itemPath, "itemPath");
			Assert.IsNotNull(url, "url");

			//get file info
			if (!url.StartsWith("~/media/") && !url.StartsWith("/~/media/") && !url.StartsWith("~/link.aspx?_id="))
			{
				return null;
			}

			var id = url.Replace("/~/media/", string.Empty).Replace("~/media/", string.Empty).Replace("~/link.aspx?_id=", string.Empty).Split(new[] { '.', '&'}).FirstOrDefault();

            var guid = Guid.Empty;
			var originalItem = FromDB.GetItem(Guid.TryParse(id, out guid) ? new ID(guid).ToString() : $"/sitecore/media library/{id.Replace("-", " ")}");

			if (originalItem == null || !originalItem.Paths.IsMediaItem) return null;

			return MediaService.FindOrCreateMediaItem(map, originalItem);
		}
	}
}
