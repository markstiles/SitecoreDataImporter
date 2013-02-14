using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.Data.Items;
using Sitecore.Resources.Media;
using Sitecore.Data.Fields;
using System.Web.UI.WebControls;

namespace Sitecore.SharedSource.DataImporter.Extensions {
	public static class LinkFieldExtensions {

		public static string GetUrl(this Sitecore.Data.Fields.LinkField LinkField) {

			string url = "";

			switch (LinkField.LinkType) {
				case "internal":
				case "external":
				case "mailto":
				case "anchor":
				case "javascript":
					url = LinkField.Url;
					break;
				case "media":
					MediaItem media = new MediaItem(LinkField.TargetItem);
					url = Sitecore.StringUtil.EnsurePrefix('/', MediaManager.GetMediaUrl(media));
					break;
				case "":
				default:
					break;
			}

			return url;

		}

		/// <summary>
		/// Converts a Sitecore link field into a hyperlink
		/// </summary>
		/// <param name="LinkField">The Sitecore link field to parse</param>
		/// <param name="hyperlinkToFill">The HyperLink object to fill</param>
		public static void FillHyperLink(this Sitecore.Data.Fields.LinkField LinkField, HyperLink hyperlinkToFill) {
			Sitecore.Data.Database db = Sitecore.Context.Database;

			//set text and tooltip
			hyperlinkToFill.Text = LinkField.Text;
			hyperlinkToFill.ToolTip = LinkField.Title;
			hyperlinkToFill.Target = LinkField.Target;

			if (!(LinkField.Url == "")) {
				if (LinkField.LinkType.Equals("javascript")) {
					// javascript (do not fill out TargetItem, fill out Url, e,g. javascript:alert('hello').
					hyperlinkToFill.Attributes.Add("onclick", LinkField.Url + "; return false;");
				} else if (LinkField.LinkType.Equals("external")) {
					// external (do not fill out TargetItem)
					hyperlinkToFill.NavigateUrl = LinkField.Url;
				} else if (LinkField.LinkType.Equals("internal")) {
					// internal (fill out TargetItem = item in Sitecore tree)

					string strURL = LinkField.Url.Replace("/Home", string.Empty);

					if ((strURL == ".aspx")) {
						strURL = "/";
					}

					hyperlinkToFill.NavigateUrl = strURL;
					//sb.Append(" href=\"" + WiseBusiness.Utils.Util.ProcessLink(LinkField.Url + ".aspx") + "\"");
				} else if (LinkField.LinkType.Equals("media")) {
					/// - media (fill out TargetItem = item in media library).
					Item asset = db.Items["/sitecore/media library" + LinkField.Url];
					//this GUID refers to the File Path of the File template
					if (asset.Fields["File Path"].Value.Equals("")) {
						hyperlinkToFill.NavigateUrl = "/media/" + MediaManager.GetMediaUrl(asset);
					} else {
						hyperlinkToFill.NavigateUrl = asset.Fields["File Path"].Value;
					}
				} else if (LinkField.LinkType.Equals("mailto")) {
					/// - mailto (do not fill out TargetItem, fill out url).
					hyperlinkToFill.NavigateUrl = LinkField.Url;
				} else if (LinkField.LinkType.Equals("anchor")) {
					/// - anchor (do not fill out TargetItem, fill out Anchor + Url).
					hyperlinkToFill.NavigateUrl = LinkField.Url;
					if (LinkField.Anchor != "") {
						hyperlinkToFill.NavigateUrl += "#" + LinkField.Anchor;
					}
				}
			}
		}
	}

	public static class DelimitedFieldExtensions {
		/// <summary>
		/// This will return a list of items based on the base template class
		/// </summary>
		/// <typeparam name="T">any class type</typeparam>
		/// <param name="df">Uses the id's in this field to create items</param>
		/// <param name="creator">creator should take the items created from the id's in the delimited field as the first lambda parameter</param>
		/// <returns></returns>
		public static List<T> As<T>(this DelimitedField df, Func<Item, T> creator) {

			List<T> itemList = new List<T>();
			foreach (string i in df.Items) {
				itemList.Add(creator(Sitecore.Context.Database.Items[i]));
			}
			return itemList;
		}

		/// <summary>
		/// Returns the delimited field values as a comma separated  string
		/// </summary>
		/// <param name="dField">
		/// The Sitecore delimited field
		/// </param>
		/// <param name="FieldName">
		/// The Field Name in the Sitecore Item that the delimited field points to
		/// </param>
		/// <returns>
		/// Returns a comma separated string of values
		/// </returns>
		public static string ToString(this Sitecore.Data.Fields.DelimitedField dField, string FieldName) {
			return dField.ToString(FieldName, ",");
		}

		/// <summary>
		/// Returns the delimited field values as a string delimited by the value specified
		/// </summary>
		/// <param name="dField">
		/// The Sitecore delimited field
		/// </param>
		/// <param name="FieldName">
		/// The Field Name in the Sitecore Item that the delimited field points to
		/// </param>
		/// <param name="delimiter">
		/// The delimiter to use when building the string of values
		/// </param>
		/// <returns>
		/// Returns a comma separated string of values
		/// </returns>
		public static string ToString(this Sitecore.Data.Fields.DelimitedField dField, string FieldName, string delimiter) {

			List<string> stringList = dField.ToList(FieldName);
			StringBuilder sb = new StringBuilder();

			foreach (string str in stringList) {
				if (sb.Length > 0)
					sb.Append(delimiter);

				//Look up item's value
				sb.Append(str);
			}

			return sb.ToString();
		}

		/// <summary>
		/// This is used to get a list string from the list of items that the delimited field values point to
		/// </summary>
		/// <param name="dField">
		/// The delimited field to parse
		/// </param>
		/// <param name="FieldName">
		///	The field in the items that the delimited field points to from which to pull the string value from
		/// </param>
		/// <returns>
		/// This is used to get a list string from the list of items that the delimited field values point to
		/// </returns>
		public static List<string> ToList(this Sitecore.Data.Fields.DelimitedField dField, string FieldName) {

			List<Item> itemList = dField.ToList();
			List<string> stringList = new List<string>();

			foreach (Item i in itemList) {
				try {
					stringList.Add(i.Fields[FieldName].Value);
				} catch { }
			}

			return stringList;
		}

		/// <summary>
		/// This is used to get a list of the items that the delimited field values point to
		/// </summary>
		/// <param name="dField">
		/// The delimited field to parse
		/// </param>
		/// <returns>
		/// Returns a list of items that the delimited field values point to
		/// </returns>
		public static List<Item> ToList(this Sitecore.Data.Fields.DelimitedField dField) {

			List<Item> itemList = new List<Item>();

			foreach (string itemID in dField.Items) {
				try {
					itemList.Add(Sitecore.Context.Database.Items[itemID]);
				} catch { }
			}

			return itemList;
		}

		/// <summary>
		/// this will convert a delimited field into a list of the specified type
		/// </summary>
		/// <typeparam name="T">
		/// Type to convert to 
		/// </typeparam>
		/// <param name="dField">
		/// delimited field
		/// </param>
		/// <param name="create">
		/// create function for the type
		/// </param>
		/// <returns>
		/// returns a list of the type specified
		/// </returns>
		public static List<T> ToList<T>(this Sitecore.Data.Fields.DelimitedField dField, Func<Item, T> create) {

			List<T> itemList = new List<T>();

			foreach (string itemID in dField.Items) {
				try {
					itemList.Add(create(Sitecore.Context.Database.Items[itemID]));
				} catch { }
			}

			return itemList;
		}
	}

	public static class ImageFieldExtensions {

		/// <summary>
		/// Applies the Sitecore ImageField attributes to an Image.
		/// (properties set: alt text, height, width, url)
		/// </summary>
		/// <param name="imageField">The Sitecore image field used as the datasource</param>
		/// <param name="imageToFill">The image object to fill</param>
		public static void FillImage(this Sitecore.Data.Fields.ImageField imageField, Image imageToFill) {
			imageToFill.AlternateText = imageField.Alt;
			imageToFill.Height = new Unit(imageField.Height);
			imageToFill.Width = new Unit(imageField.Width);
			imageToFill.ImageUrl = imageField.GetImageUrl();
		}

		public static string GetImageUrl(this Sitecore.Data.Fields.ImageField imageField) {

			if (imageField != null && imageField.MediaItem != null) {
				var image = new MediaItem(imageField.MediaItem);
				return Sitecore.StringUtil.EnsurePrefix('/', Sitecore.Resources.Media.MediaManager.GetMediaUrl(image));
			} else {
				return string.Empty;
			}
		}
	}
}
