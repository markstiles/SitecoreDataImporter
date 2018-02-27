using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.Resources.Media;
using Sitecore.SharedSource.DataImporter.Logger;
using Sitecore.SharedSource.DataImporter.Providers;
using Sitecore.SharedSource.DataImporter.Utility;
using ProcessStatus = System.Web.ProcessStatus;

namespace Sitecore.SharedSource.DataImporter.Mappings.Fields
{
	public abstract class MediaFileMapping:BaseMapping
	{
		public MediaFileMapping(Item i, ILogger l) : base(i)
		{
		}

		protected MediaItem HandleMediaItem(IDataMap map, Item parentItem, string itemPath, MediaItem item)
		{
			var itemName = StringUtility.GetValidItemName(item.Name, 100);
			//date info
			string newFilePath = string.Format("{0}/{1}", parentItem.Paths.FullPath, itemName);

			// see if it exists in med lib

			IEnumerable<Item> matches = parentItem.Axes.GetDescendants()
				.Where(a => a.Paths.FullPath.EndsWith(itemName));

			if (matches != null && matches.Any())
			{
				if (matches.Count().Equals(1))
					return new MediaItem(matches.First());

				map.Logger.Log("MediaFileMapping.HandleMediaItem", string.Format("Sitecore image lookup matched {0} for item {1}", matches.Count(), itemPath));
				return null;
			}

			ItemManager.AddFromTemplate(itemName, TemplateIDs.UnversionedImage, BuildMediaPath(map.ToDB, item.InnerItem.Paths.ParentPath), item.ID);

			MediaItem m = ImportMediaItem(item.GetMediaStream(), itemName + "." + item.Extension, newFilePath);
			if (m == null)
				map.Logger.Log("MediaFileMapping.HandleMediaItem", string.Format("Image Not Found for item '{0}'", itemPath));

			return m;
		}

		private MediaItem ImportMediaItem(Stream mediaStream, string fileName, string newPath)
		{
			MediaItem mediaItem;
			using (MemoryStream stream2 = new MemoryStream())
			{
				mediaStream.CopyTo(stream2);

				// Create the options
				MediaCreatorOptions options = new MediaCreatorOptions();
				options.FileBased = false;
				options.IncludeExtensionInItemName = false;
				options.OverwriteExisting = true;
				options.Versioned = false;
				options.Destination = newPath;
				options.Database = Sitecore.Configuration.Factory.GetDatabase("master");
				stream2.Flush();
				// upload to sitecore
				MediaCreator creator = new MediaCreator();
				mediaItem = creator.CreateFromStream(stream2, fileName, options);
			}

			return mediaItem;
		}

		protected Item BuildMediaPath(Database toDb, string path)
		{
			var item = toDb.GetItem(path);
			if (item == null)
			{
				var parentPath = path.Substring(0, path.LastIndexOf("/"));
				var itemName = StringUtility.GetValidItemName(path.Substring(path.LastIndexOf("/") + 1), 100);
				var parent = BuildMediaPath(toDb, parentPath);
				item = parent.Add(itemName, new TemplateID(TemplateIDs.MediaFolder));
			}
			return item;
		}
	}
}