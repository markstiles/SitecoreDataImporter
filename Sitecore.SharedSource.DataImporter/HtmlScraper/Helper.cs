using Sitecore.Data.Items;
using Sitecore.SharedSource.DataImporter.HtmlAgilityPack;
using Sitecore.SharedSource.DataImporter.Reporting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sitecore.SharedSource.DataImporter.HtmlScraper
{
    public class Helper
    {
        /// <summary>
        /// Calculate the node path logic and return the node to be used for the field value
        /// </summary>
        /// <param name="htmlObj"></param>
        /// <param name="doc"></param>
        /// <returns></returns>
        public static HtmlNode HandleNodesLookup(string htmlObj, HtmlDocument doc, bool useXPath = false)
        {
            HtmlNode node = null;
            List<HtmlNode> nodes = new List<HtmlNode>();
            bool isMultiNodesData = htmlObj.Contains("/*");

            if (!useXPath) {
                htmlObj = isMultiNodesData ? htmlObj.Replace("/*", "") : htmlObj;
            }

            try
            {
               
                if (useXPath)
                {
                    nodes = doc.DocumentNode.SelectNodes(htmlObj).ToList();
                    isMultiNodesData = true;
                }
                else {
                    nodes = HandleXPathQuery(htmlObj, doc);
                }

                if (isMultiNodesData)
                {
                    if (node == null)
                        node = doc.CreateNode(HtmlNodeType.Element, 0);
                    else
                    {
                        node.ChildNodes.Clear();
                    }
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
            catch(Exception ex)
            {
                //This is just means selector was looking for in dom was not found.
            }

            return node;
        }

        private static List<HtmlNode> HandleXPathQuery(string selector, HtmlDocument doc)
        {
            string xPath = "//";
            string attrName = "";
            List<HtmlNode> nodes = new List<HtmlNode>();
            List<string> dataItems = selector.Split('/').ToList();
            bool isTagName = true;

            if (selector.StartsWith(".") || selector.StartsWith("#"))
            {
                isTagName = false;
            }

            if (dataItems != null && dataItems.Any() && selector.Contains("/"))
            {
                foreach (var data in dataItems)
                {
                    string option = data.ToCharArray().FirstOrDefault().ToString();
                    attrName = FormatXpath(data, option);
                    selector = selector.Replace(data, attrName);

                    if (selector.Contains("/[@"))
                    {
                        selector = selector.Replace("/[@", "[@");
                    }
                    else {
                        selector = selector.Replace("/[", "/*[");
                    }
                }

                if (!isTagName)
                {
                    xPath += "*" + selector;
                }
                else
                {
                    xPath += selector;
                }

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

        private static string FormatXpath(string data, string option)
        {
            string formated = data;
            string value = data;
            bool contains = false;

            if (value.EndsWith("!")) {

                contains = true;
                value = value.Replace("!", "");
            }

            switch (option)
            {
                case ".":
                    value = value.Replace(".", "");

                    if (contains)
                    {
                        formated = "[contains(@class, '" + value + "')]";
                    }
                    else {
                        formated = "[@class='" + value + "']";
                    }

                    break;
                case "#":
                    value = value.Replace("#", "");
                    formated = "[@id='" + value + "']";
                    break;
                case "@":

                    if (value.Contains("=")) {

                        string[] attrData = value.Split('=');
                        formated = "["+attrData[0]+"='" + attrData[1] + "']";
                    }

                    break;
            }

            return formated;
        }


        /// <summary>
        /// 
        /// ie. [1]/p[3]
        /// </summary>
        /// <param name="data"></param>
        private static string HandleIndex(string data)
        {
            //data = data.Replace("/*", "");
            string[] splits = data.ToString().Split('/');
            //string indexUpdates = string.Empty;

            foreach (var s in splits)
            {
                int indexOut;
                //If true, then it is numeric index
                if (int.TryParse(s, out indexOut))
                {

                    //string lookUp = "/" + s + "/";
                    string lookUp2 = "/" + s;
                    string replace = "[" + s + "]";
                    //indexUpdates += data.Replace(lookUp, replace);
                    data = data.Replace(lookUp2, replace);
                }
            }


            return data;
            //switch (splits)
            //{
            //    case "*":
            //        break;
            //    default:
            //        int indexOut;
            //        //If true, then it is numeric index
            //        if (int.TryParse(splits, out indexOut))
            //        {
            //            data = data.Replace("/" + splits, "[" + splits + "]");
            //        }
            //        break;
            //}

        }

        public static string RemoveInvalidChars(ImportConfig Config, string data, bool root, bool report = true)
        {
            string originalName = data;

            if (data.Contains(".") && !root)
            {
                int index = data.IndexOf('.');
                data = data.Remove(index);
            }

            data = ItemUtil.ProposeValidItemName(data);

            foreach (var cleanup in Config.ItemNameCleanups)
            {
                if (data.Contains(cleanup.Find))
                {
                    data = data.Replace(cleanup.Find, cleanup.Replace);

                    if (report)
                    {
                        ImportReporter.Write(cleanup.CleanupItem, Level.Info, " To: " + data + "", "Name > From: " + originalName, "Name Change", "");
                    }
                }
            }

            return data;
        }
    }
}
