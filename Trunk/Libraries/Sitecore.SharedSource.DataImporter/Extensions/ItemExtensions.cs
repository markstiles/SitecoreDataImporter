using System;
using System.Text;
using System.Collections.Generic;
using Sitecore.Data.Items;
using Sitecore.Resources.Media;
using System.Web.UI.WebControls;
using System.Web;
using System.Text.RegularExpressions;
using Sitecore.Layouts;
using Sitecore.Data.Fields;
using System.Globalization;
using System.Collections;
using System.Linq;
using Sitecore.Collections;


namespace Sitecore.SharedSource.DataImporter.Extensions
{
    public static class ItemExtensions
    {
		/// <summary>
		/// This will determine if the current language version is empty by checking the __Created date field
		/// </summary>
		/// <param name="i">
		/// The item to check
		/// </param>
		/// <returns>
		/// true if created value is null or empty false otherwise
		/// </returns>
		public static bool IsNullVersion(this Item i) {
			return string.IsNullOrEmpty(i.Fields["__Created"].Value);
		}

		/// <summary>
		/// This will get the default language version of the item
		/// </summary>
		/// <param name="i">
		/// the item to check
		/// </param>
		/// <returns>
		/// Returns the default language version of the item(english by default)
		/// </returns>
		public static Item GetDefaultVersion(this Item i) {
			return Sitecore.Context.Database.GetItem(i.ID, Sitecore.Data.Managers.LanguageManager.DefaultLanguage);
		}

		/// <summary>
		/// This will determine if the current item is not null
		/// </summary>
		/// <param name="curItem">The item to check for null</param>
		/// <returns>Returns true it the current item is null false otherwise</returns>
		public static bool IsNotNull(this Item curItem) {
			return !IsNull(curItem);
		}

		/// <summary>
		/// This will determine if the current item is null
		/// </summary>
		/// <param name="curItem">The item to check for null</param>
		/// <returns>Returns true it the current item is null false otherwise</returns>
		public static bool IsNull(this Item curItem) {
			return (curItem == null) ? true : false;
		}

		/// <summary>
		/// This will return true if the two items have the same ID
		/// </summary>
		/// <param name="curItem">the first item to compare</param>
		/// <param name="compareItem">the second item to compare</param>
		/// <returns>
		/// true if the first and second items id's are equal 
		/// false otherwise
		/// </returns>
		public static bool EqualTo(this Item curItem, Item compareItem){
			return (curItem.ID.Equals(compareItem.ID)) ? true : false;
		}

		/// <summary>
		/// This will tell you if the item is an ancestor of the item provided
		/// </summary>
		/// <param name="ancestor">the item to check for being an ancestor</param>
		/// <param name="curItem">the item whose ancestry will be checked</param>
		/// <returns>
		/// true if the ancestor is equal to the curItem or one of it's ancestors
		/// false if the ancestor is does not match the curItem or one of it's ancestors
		/// </returns>
		public static bool IsAnAncestorOf(this Item ancestor, Item curItem) {
			
			Item temp = curItem;
			while (!temp.TemplateName.Equals("Main section")) {
				if (temp.ID.Equals(ancestor.ID)) {
					return true;
				}
				temp = temp.Parent;
			}
			return false;
		}

		/// <summary>
		/// This will return the item as an object of the type specified
		/// </summary>
		/// <typeparam name="T">
		/// The Type specified
		/// </typeparam>
		/// <param name="item">
		/// The item to convert
		/// </param>
		/// <param name="create">
		/// The method in which to convert it
		/// </param>
		/// <returns>
		/// Returns an object of the type specified
		/// </returns>
		public static T As<T>(this Item item, Func<Item, T> create) {
			if(item != null){
				return create(item);
			}else {
				return default(T);
			}
		}

		/// <summary>
		/// This returns the first child it finds that has the required template or a null
		/// </summary>
		/// <param name="parentItem">
		/// parentItem
		/// </param>
		/// <param name="templateName">
		/// this is the template name of the items you want
		/// </param>
		/// <returns>
		/// Returns the first item that matches the templatename or null
		/// </returns>
		public static Item ChildByTemplate(this Item Parent, string Templatename) {

			try {
				return (from child in Parent.GetChildren().ToArray() where child.TemplateName.Equals(Templatename) select child).First();
			}
			catch {
				return null;
			}
		}

		/// <summary>
		/// This gets a child item that matches the template name and item name provided
		/// </summary>
		/// <param name="parentItem">
		/// The parent to search under for a result
		/// </param>
		/// <param name="templateName">
		/// The template name of the child to find
		/// </param>
		/// <param name="itemName">
		/// The item name of the child to find
		/// </param>
		/// <returns>
		/// Returns and item that matches the criteria or null
		/// </returns>
		public static Item ChildByTemplateAndName(this Item parentItem, string templateName, string itemName) {

			try {
				return (from child in parentItem.GetChildren().ToArray() where (child.TemplateName.Equals(templateName) && child.DisplayName.Equals(itemName)) select child).First();
			}
			catch {
				return null;
			}
		}

		/// <summary>
		/// This gets a child item that matches the template name or item name provided
		/// </summary>
		/// <param name="parentItem">
		/// The parent to search under for a result
		/// </param>
		/// <param name="templateName">
		/// The template name of the child to find
		/// </param>
		/// <param name="itemName">
		/// The item name of the child to find
		/// </param>
		/// <returns>
		/// Returns and item that matches one of the criteria or null
		/// </returns>
		public static Item ChildByTemplateOrName(this Item parentItem, string templateName, string itemName) {

			try {
				return (from child in parentItem.GetChildren().ToArray() where (child.TemplateName.Equals(templateName) || child.DisplayName.Equals(itemName)) select child).First();
			}
			catch {
				return null;
			}
		}

		/// <summary>
		/// This gets a child item that matches the item name provided
		/// </summary>
		/// <param name="parentItem">
		/// The parent to search under for a result
		/// </param>
		/// <param name="itemName">
		/// The item name of the child to find
		/// </param>
		/// <returns>
		/// Returns and item that matches one of the criteria or null
		/// </returns>
		public static Item ChildByName(this Item parentItem, string itemName) {

			try {
				return (from child in parentItem.GetChildren().ToArray() where (child.DisplayName.Equals(itemName)) select child).First();
			}
			catch {
				return null;
			}
		}

		/// <summary>
		/// this returns all the children who have a required template
		/// </summary>
		/// <param name="parentItem">
		/// parentItem Item
		/// </param>
		/// <param name="templateName">
		/// this is the template name of the items that you want
		/// </param>
		/// <returns>
		/// Returns a list of items that match the template name
		/// </returns>
		public static List<Item> ChildrenByTemplate(this Item parentItem, string templateName) {
			
			List<string> types = new List<string>();
			types.Add(templateName);
			return ChildrenByTemplates(parentItem, types);			
		}

		/// <summary>
		/// This returns a list of child items based on a list of templates names provided
		/// </summary>
		/// <param name="parentItem">
		/// parentItem Item to search for children
		/// </param>
		/// <param name="templateNames">
		/// The list of template names to look for
		/// </param>
		/// <returns>
		/// Returns a list of items that match the templatenames provided
		/// </returns>
		public static List<Item> ChildrenByTemplates(this Item parentItem, List<string> templateNames) {

			try {
				return (from child in parentItem.GetChildren().ToArray() where templateNames.Contains(child.TemplateName) select child).ToList();
			}
			catch {
				return null;
			}
		}

		/// <summary>
		/// This will look for children of a specified templatename recursively. It will only recursively query under items that match the templatename.
		/// </summary>
		/// <param name="parentItem">
		/// parentItem item to search under
		/// </param>
		/// <param name="templateName">
		/// templateName of the items you want to return
		/// </param>
		/// <returns>
		/// Returns a list of Items that match the template name
		/// </returns>
		public static List<Item> ChildrenByTemplateRecursive(this Item parentItem, string templateNames) {

			List<string> types = new List<string>();
			types.Add(templateNames);
			return ChildrenByTemplatesRecursive(parentItem, types);
		}

		/// <summary>
		/// This will look for children of a specified templatenames recursively. It will only recursively query under items that match the templatenames.
		/// </summary>
		/// <param name="parentItem">
		/// parentItem item to search under
		/// </param>
		/// <param name="templateNames">
		/// templateNames of the items you want to return
		/// </param>
		/// <returns>
		/// Returns a list of items that match the templatenames provided
		/// </returns>
		public static List<Item> ChildrenByTemplatesRecursive(this Item parentItem, List<string> templateNames) {
			return ChildrenByTemplatesRecursive(parentItem, templateNames, new List<string>());
		}

		public static List<Item> ChildrenByTemplatesRecursive(this Item parentItem, List<string> templateNames, string ignoreTemplateName) {
			List<string> ignore = new List<string>();
			ignore.Add(ignoreTemplateName);
			return ChildrenByTemplatesRecursive(parentItem, templateNames, ignore);
		}

		public static List<Item> ChildrenByTemplatesRecursive(this Item parentItem, List<string> templateNames, List<string> ignoreTemplates) {

			try {
				List<Item> list = new List<Item>();
				//get the first level of items
				List<Item> thisLevel = (from child in parentItem.GetChildren().ToArray() where templateNames.Contains(child.TemplateName) select child).ToList();

				//foreach item found look for children of it's type
				foreach (Item i in thisLevel) {
					//if this item's templatename is not in the ignore list then add it
					if (!ignoreTemplates.Contains(i.TemplateName)) {
						list.Add(i);
					}
					//either way continue to search below it for values
					list.AddRange(i.ChildrenByTemplatesRecursive(templateNames, ignoreTemplates));
				}

				return list;
			}
			catch {
				return null;
			}
		}

		/// <summary>
        /// This runs the xpath statement in the string
        /// </summary>
        /// <param name="s">
		/// The XPath statement
		/// </param>
        /// <returns>
		/// Returns an Item array of results
		/// </returns>
		public static Sitecore.Data.Items.Item[] ExecuteXPath(this string s) {
			return s.ExecuteXPath(Sitecore.Context.Database);
        }

		/// <summary>
		/// This runs the xpath statement in the string
		/// </summary>
		/// <param name="s">
		/// The XPath statement
		/// </param>
		/// <param name="DB">
		/// The Database where to query for the items
		/// </param>
		/// <returns>
		/// Returns an Item array of results
		/// </returns>
        public static Sitecore.Data.Items.Item[] ExecuteXPath(this string s, Sitecore.Data.Database DB) {
            return DB.SelectItems(s.CleanXPath());
        }

        /// <summary>
        /// Executes the XPath statement and returns a single Sitecore item.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static Sitecore.Data.Items.Item ExecuteXPathScalar(this string s) {
            return s.ExecuteXPathScalar(Sitecore.Context.Database);
        }

        /// <summary>
        /// Executes the XPath statement and returns a single Sitecore item.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static Sitecore.Data.Items.Item ExecuteXPathScalar(this string s, Sitecore.Data.Database DB) {
            return DB.GetItem(s);
        }

        /// <summary>
		/// This will take the items path and turn it into a url
        /// </summary>
        /// <param name="item">
		/// The item to base the url on
		/// </param>
        /// <returns></returns>
		public static string ItemURL(this Sitecore.Data.Items.Item item) {
            return Sitecore.Links.LinkManager.GetItemUrl(item);
        }

		public static List<string> Placeholders(this Item item, string placeholder, string ignoreSublayout) {
			List<string> s = new List<string>();
			
			Sitecore.Data.Items.DeviceItem dev = Sitecore.Context.Device;
			string rend = item.Fields["__renderings"].Value;
			Sitecore.Layouts.LayoutDefinition layout = LayoutDefinition.Parse(rend);
			Sitecore.Layouts.DeviceDefinition device = layout.GetDevice(dev.ID.ToString());
			foreach (Sitecore.Layouts.RenderingDefinition rd in device.Renderings) {
				bool addIt = true;

				if (!ignoreSublayout.Equals("")) {
					Item i = Sitecore.Context.Database.Items[rd.ItemID];
					if (i.DisplayName.Equals(ignoreSublayout)) {
						addIt = false;
					}
				}
				if(!placeholder.Equals("") && !rd.Placeholder.Equals(placeholder)){
					addIt = false;
				}

				if (addIt) {
					s.Add(rd.Placeholder);
				}
			}
			return s;
		}

		public static List<DeviceDefinition> Devices(this Item thisItem) {

			List<DeviceDefinition> list = new List<DeviceDefinition>();

			Sitecore.Layouts.LayoutDefinition layout = LayoutDefinition.Parse(thisItem["__Renderings"]);
			ArrayList al = layout.Devices;
			foreach (DeviceDefinition dd in al) {
				list.Add(dd);
			}

			return list;
		}

		public static bool HasLayout(this Item thisItem) {

			//Sitecore.Data.Database db = Sitecore.Context.Database;
			return (!thisItem["__Renderings"].Equals("") && thisItem.Devices().Count > 0) ? true : false;
			//string rend = thisItem.Fields["__renderings"].Value;
			//if (!rend.Equals("")) {
			//    Sitecore.Layouts.LayoutDefinition layout = LayoutDefinition.Parse(rend);
			//    Sitecore.Data.Items.DeviceItem dev = Sitecore.Context.Device;
			//    Sitecore.Layouts.DeviceDefinition device = layout.GetDevice(dev.ID.ToString());
			//    if (!device.Layout.Equals("")) {
			//        Item l = db.Items[device.Layout];
			//        return (l != null) ? true : false;
			//    }
			//}
			//return false;
		}

        /// <summary>
        /// Will return true if the sublayout path you've provided is applied to this item
        /// </summary>
        /// <param name="item">
        /// The item whose sublayouts are to be checked
        /// </param>
        /// <param name="sublayoutPath">
        /// The path after "/sitecore/layout/Sublayouts/"
        /// </param>
        /// <returns></returns>
        public static bool HasSublayout(this Sitecore.Data.Items.Item item, String sublayoutPath)
        {

            Sitecore.Data.Database db = Sitecore.Context.Database;

            // get the Renderings field of the item, which 
            // corresponds to the Layout field in the Content Editor
            string rend = item.Fields["__renderings"].Value;

            // Use the LayoutDefinition to parse the field value
            Sitecore.Layouts.LayoutDefinition layout = new LayoutDefinition();

            layout = LayoutDefinition.Parse(rend);

            // Get the current device from the Context 
            Sitecore.Data.Items.DeviceItem dev = Sitecore.Context.Device;

            // Get the device definition for the current device from the
            // Test Item’s Layout field
            Sitecore.Layouts.DeviceDefinition device = layout.GetDevice(dev.ID.ToString());

            // we are looking for references to the Document rendering
            // so retrieve that rendering from the master database
            Item docRendering = db.Items["/sitecore/layout/Sublayouts/" + sublayoutPath];

            // Look for the document rendering in the definition of 
            // the current device in the Test Item’s layout field
            Sitecore.Layouts.RenderingDefinition rendering = device.GetRendering(docRendering.ID.ToString());

            // If the rendering is not null, then it is referenced
            // on the current device in the layout field for the
            // Test Item
            return (rendering != null) ? true : false;
        }

        /// <summary>
        /// Return true if the placeholder key is applied to the presentation settings of this item
        /// </summary>
        /// <param name="item">
        /// The item being queried
        /// </param>
        /// <param name="placeholderKey">
        /// The key of the sitecore placeholder to search for
        /// </param>
        /// <returns></returns>
        public static bool HasPlaceholderApplied(this Sitecore.Data.Items.Item item, string placeholderKey)
        {

            string rend = item.Fields["__renderings"].Value;
            Sitecore.Layouts.LayoutDefinition layout = new LayoutDefinition();
            layout = LayoutDefinition.Parse(rend);
            Sitecore.Data.Items.DeviceItem dev = Sitecore.Context.Device;
            Sitecore.Layouts.DeviceDefinition device = layout.GetDevice(dev.ID.ToString());

            foreach (Sitecore.Layouts.RenderingDefinition rd in device.Renderings) {
                if (rd.Placeholder.Equals(placeholderKey)) {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks to see if the item has the base template specified
        /// </summary>
        /// <param name="item">
        /// The item whose base templates will be checked 
        /// </param>
        /// <param name="baseTemplateName">
        /// The base template name to check for
        /// </param>
        /// <returns></returns>
        public static bool HasBaseTemplate(this Sitecore.Data.Items.Item item, string baseTemplateName)
        {

            TemplateItem[] bases = item.Template.BaseTemplates;

            foreach (TemplateItem temp in bases) {
                if (temp.DisplayName.Equals(baseTemplateName))
                    return true;
            }

            return false;
        }

        public static IEnumerable<Item> GetAncestors(this Item item, string GUIDTopParent)
        {
            List<Item> list = new List<Item>();
            
            Item parent = item;
            Sitecore.Data.ID parentID= new Sitecore.Data.ID(GUIDTopParent);
            while (parent.Parent != null )
            {
                parent = parent.Parent;
                list.Add(parent);
                if (parent.ID == parentID)
                    break;
            }
            list.Reverse();
            return list;

        }

       
    }

	public static class ItemListExtensions
	{
		/// <summary>
		/// This will return the list of items as a list of objects of the type specified
		/// </summary>
		/// <typeparam name="T">
		/// The Type specified
		/// </typeparam>
		/// <param name="items">
		/// The items to convert
		/// </param>
		/// <param name="create">
		/// The method in which to convert it
		/// </param>
		/// <returns>
		/// Returns an object of the type specified
		/// </returns>
		public static List<T> As<T>(this List<Item> items, Func<Item, T> create) {
			if(items != null){
				List<T> list = new List<T>();
				foreach (Item i in items) {
					list.Add(create(i));
				}
				return list;
			}
			else {
                return new List<T>();
			}
		}

		/// <summary>
		/// This will return the list of items as a list of objects of the type specified
		/// </summary>
		/// <typeparam name="T">
		/// The Type specified
		/// </typeparam>
		/// <param name="items">
		/// The items to convert
		/// </param>
		/// <param name="create">
		/// The method in which to convert it
		/// </param>
		/// <returns>
		/// Returns an object of the type specified
		/// </returns>
		public static List<T> As<T>(this Item[] items, Func<Item, T> create) {
			return As<T>(items.ToList(), create);
		}

        /// <summary>
        /// This will randomize a Sitecore item array
        /// </summary>
        /// <param name="source">
        /// The item array to randomize
        /// </param>
        /// <returns>
        /// Returns a randomized Sitecore item array
        /// </returns>
        public static Sitecore.Data.Items.Item[] Randomize(this Sitecore.Data.Items.Item[] source)
        {
            Random rnd = new Random();
            for (int inx = source.Length - 1; inx > 0; --inx) {
                int position = rnd.Next(inx);
                object temp = source[inx];
                source[inx] = source[position];
                source[position] = (Sitecore.Data.Items.Item)temp;
            }

            return source;
        }

        /// <summary>
        /// Randomizes a generic list of Sitecore items
        /// </summary>
        /// <param name="source">The items to randomize</param>
        /// <returns>The randomized generic list</returns>
        public static List<Sitecore.Data.Items.Item> Randomize(this List<Sitecore.Data.Items.Item> source)
        {
            return source.ToArray().Randomize().ToList();
        }

	}
}