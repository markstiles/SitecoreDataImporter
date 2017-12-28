using Sitecore.SharedSource.DataImporter.HtmlAgilityPack;
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
        public static HtmlNode HandleNodesLookup(string htmlObj, HtmlDocument doc)
        {

            HtmlNode node = null;
            List<HtmlNode> nodes = new List<HtmlNode>();
            bool isMultiNodesData = htmlObj.Contains("/*");
            htmlObj = isMultiNodesData ? htmlObj.Replace("/*", "") : htmlObj;
            try
            {
                //Not sure if this check is needed so to do somthing else. Check it latter.  
                //if (htmlObj.Contains("/"))
                //{
                //    nodes = HandleXPathQuery(htmlObj, doc);
                //}
                //else
                //{
                //    nodes = HandleXPathQuery(htmlObj, doc);
                //}

                nodes = HandleXPathQuery(htmlObj, doc);

                if (isMultiNodesData)
                {
                    //TODO: this is worked for * but run some more test with differnt type mapping options of *
                    node = doc.CreateNode(HtmlNodeType.Element, 0);
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
            catch
            {

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
                    selector = selector.Replace("/[", "/*[");
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

            switch (option)
            {
                case ".":
                    value = value.Replace(".", "");
                    formated = "[@class='" + value + "']";
                    break;
                case "#":
                    value = value.Replace("#", "");
                    formated = "[@id='" + value + "']";
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
    }
}
