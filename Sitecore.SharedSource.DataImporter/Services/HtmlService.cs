using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;

namespace Sitecore.SharedSource.DataImporter.Services
{
	public class HtmlService
    {
		protected string[] InlineElements = new[] { "a", "abbr", "acronym", "b", "bdo", "big", "br", "cite", "dfn", "em", "i", "kbd", "q", "small", "strong", "sub", "sup", "time", "tt", "var" };
		protected string[] OnlyIfInsideText = new[] {"img", "label", "button", "span"};

        public string FixOrphanedText(string html, out bool modified)
		{
			modified = false;
			if (string.IsNullOrEmpty(html)) return string.Empty;

			var document = new HtmlDocument();
			document.LoadHtml(html);

			var nodes = new Queue<HtmlNode>(document.DocumentNode.ChildNodes);
			var orphanedNodes = new List<HtmlNode>();
			bool deletingBrs = false;
			foreach (var node in nodes)
			{
				var nodeName = node.Name.ToLower();

				if (deletingBrs && (nodeName.Equals("br") || (node.NodeType == HtmlNodeType.Text && string.IsNullOrWhiteSpace(node.InnerText))))
				{
					node.ParentNode.RemoveChild(node);
					continue;
				}
				
				deletingBrs = false;
				if ((node.NodeType == HtmlNodeType.Text || InlineElements.Contains(nodeName)) && !string.IsNullOrWhiteSpace(node.InnerText))
				{
					orphanedNodes.Add(node);
				}
				else if(orphanedNodes.Any() && OnlyIfInsideText.Contains(nodeName))
				{
					orphanedNodes.Add(node);
				}
				else if (orphanedNodes.Any())
				{
					var p = document.CreateElement("p");
					foreach (var orphanedNode in orphanedNodes)
					{
						p.AppendChild(orphanedNode);
					}
					foreach (var orphanedNode in orphanedNodes)
					{
						node.ParentNode.RemoveChild(orphanedNode);
					}

					node.ParentNode.InsertBefore(p, node);

					if (nodeName.Equals("br"))
					{
						node.ParentNode.RemoveChild(node);
						deletingBrs = true;
					}

					orphanedNodes = new List<HtmlNode>();
					modified = true;
				}
			}

			if (orphanedNodes.Any())
			{
				var p = document.CreateElement("p");
				foreach (var orphanedNode in orphanedNodes)
				{
					p.AppendChild(orphanedNode);
				}
				foreach (var orphanedNode in orphanedNodes)
				{
					document.DocumentNode.RemoveChild(orphanedNode);
				}

				document.DocumentNode.AppendChild(p);
				modified = true;
			}

			return document.DocumentNode.InnerHtml;
		}
	}
}