using System;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;
using Sitecore.Data.Items;
using Sitecore.SharedSource.DataImporter.Logger;
using Sitecore.SharedSource.DataImporter.Processors.Models;

namespace Sitecore.SharedSource.DataImporter.Services
{
	public class HtmlService
    {
        protected ILogger Logger;

        public HtmlService(ILogger logger)
        {
            Logger = logger;
        }

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

        /// <summary>
        /// Calculate the node path logic and return the node to be used for the field value
        /// </summary>
        /// <param name="htmlObj"></param>
        /// <param name="doc"></param>
        /// <returns></returns>
        public HtmlNode HandleNodesLookup(string htmlObj, HtmlDocument doc, bool useXPath = false)
        {
            HtmlNode node = null;
            List<HtmlNode> nodes = new List<HtmlNode>();
            bool isMultiNodesData = htmlObj.Contains("/*");

            if (!useXPath)
                htmlObj = isMultiNodesData ? htmlObj.Replace("/*", "") : htmlObj;

            try
            {
                if (useXPath)
                {
                    nodes = doc.DocumentNode.SelectNodes(htmlObj).ToList();
                    isMultiNodesData = true;
                }
                else
                {
                    nodes = HandleXPathQuery(htmlObj, doc);
                }

                if (isMultiNodesData)
                {
                    node = new HtmlNode(HtmlNodeType.Element, doc, 0);
                    foreach (HtmlNode n in nodes)
                    {
                        node.AppendChild(n);
                    }
                }
                else
                {
                    node = nodes.FirstOrDefault();
                }

            }
            catch (Exception ex)
            {
                //This is just means selector was looking for in dom was not found.
            }

            return node;
        }

        public List<HtmlNode> HandleXPathQuery(string selector, HtmlDocument doc)
        {
            string xPath = "//";
            string attrName = "";
            List<HtmlNode> nodes = new List<HtmlNode>();
            List<string> dataItems = selector.Split('/').ToList();
            bool isTagName = !(selector.StartsWith(".") || selector.StartsWith("#"));

            if (dataItems.Any() && selector.Contains("/"))
            {
                foreach (var data in dataItems)
                {
                    string option = data.ToCharArray().FirstOrDefault().ToString();
                    attrName = FormatXpath(data, option);
                    selector = selector.Replace(data, attrName);
                    selector = (selector.Contains("/[@"))
                        ? selector.Replace("/[@", "[@")
                        : selector.Replace("/[", "/*[");
                }

                xPath += (!isTagName) ? "*" + selector : selector;
            }
            else
            {
                if (selector.StartsWith("."))
                {
                    attrName = FormatXpath(selector, ".");
                    selector = "*" + selector.Replace(selector, attrName);
                }
                else if (selector.StartsWith("#"))
                {
                    attrName = FormatXpath(selector, "#");
                    selector = "*" + selector.Replace(selector, attrName);
                }

                xPath += selector;
            }

            xPath = HandleIndex(xPath);
            nodes = doc.DocumentNode.SelectNodes(xPath).ToList();

            return nodes;
        }

        public string FormatXpath(string data, string option)
        {
            string formated = data;
            string value = data;
            bool contains = false;

            if (value.EndsWith("!"))
            {

                contains = true;
                value = value.Replace("!", "");
            }

            if (option == ".")
            {
                value = value.Replace(".", "");
                formated = (contains)
                    ? "[contains(@class, '" + value + "')]"
                    : "[@class='" + value + "']";
            }
            else if (option == "#")
            {
                value = value.Replace("#", "");
                formated = "[@id='" + value + "']";
            }
            else if (option == "@" && value.Contains("="))
            {
                string[] attrData = value.Split('=');
                formated = "[" + attrData[0] + "='" + attrData[1] + "']";
            }

            return formated;
        }

        /// <summary>
        /// ie. [1]/p[3]
        /// </summary>
        /// <param name="data"></param>
        public string HandleIndex(string data)
        {
            string[] splits = data.Split('/');

            foreach (var s in splits)
            {
                int indexOut;
                if (!int.TryParse(s, out indexOut))
                    continue;

                string lookUp2 = "/" + s;
                string replace = "[" + s + "]";
                data = data.Replace(lookUp2, replace);
            }

            return data;
        }

        public string RemoveInvalidChars(List<ItemNameCleanup> itemNameCleanups, string data, bool root, bool report = true)
        {
            string originalName = data;

            if (data.Contains(".") && !root)
            {
                int index = data.IndexOf('.');
                data = data.Remove(index);
            }

            data = ItemUtil.ProposeValidItemName(data);

            foreach (var cleanup in itemNameCleanups)
            {
                if (!data.Contains(cleanup.Find))
                    continue;

                data = data.Replace(cleanup.Find, cleanup.Replace);

                if (!report)
                    continue;

                Logger.Log($"Name Change To: {data}", cleanup.CleanupItem.Paths.FullPath, Providers.ProcessStatus.Info, $"Name > From: {originalName}");
            }

            return data;
        }
    }
}