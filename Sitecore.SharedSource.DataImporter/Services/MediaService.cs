using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Resources.Media;
using Sitecore.SharedSource.DataImporter.Logger;
using Sitecore.SharedSource.DataImporter.Providers;

namespace Sitecore.SharedSource.DataImporter.Services
{
	public class MediaService
    {
        protected ILogger Logger { get; set; }
        protected StringService StringService { get; set; }

        public MediaService(ILogger logger)
        {
            Logger = logger;
            StringService = new StringService();
        }

		private string[] _fieldNames = new[]
		{
			"Artist", "Copyright", "Date Time", "Image Description", "Make", "Model", "Software", "Title", "Keywords",
			"Description", "Source", "Terms Of Use", "CountryCode", "LocationDescription", "Latitude", "Longitude", "ZipCode",
			"Parent Asset Id", "Asset Id", "Status", "Caption", "Photographer Affiliation", "DAM Keywords", "Notes",
			"Photographer", "Special Instructions", "File Info", "File Name", "Expiration Date", "Alt","__Updated",
			"__Updated by","__Created","__Created by"
		};

		public MediaItem ImportMedia(MediaItem originalItem)
		{
			if (originalItem == null) return null;

			MediaItem mediaItem;
			using (MemoryStream stream2 = new MemoryStream())
			{
				var stream = originalItem.GetMediaStream();

				if (stream == null) return null;

				stream.CopyTo(stream2);

                var invalidChars = new List<string> { "(", ")" };
                var newItemPath = originalItem.InnerItem.Paths.FullPath;
                foreach (var s in invalidChars) {
                    newItemPath = newItemPath.Replace(s, "");
                }

				// Create the options
				MediaCreatorOptions options = new MediaCreatorOptions();
				options.FileBased = false;
				options.IncludeExtensionInItemName = false;
				options.OverwriteExisting = true;
				options.Versioned = false;
				options.Destination = newItemPath;
				options.Database = Sitecore.Configuration.Factory.GetDatabase("master");
				stream2.Flush();
				// upload to sitecore
				MediaCreator creator = new MediaCreator();
				mediaItem = creator.CreateFromStream(stream2, $"{originalItem.Name}.{originalItem.Extension}", options);
			}

			return mediaItem;
		}

		public void CopyFields(MediaItem originalItem, MediaItem newItem)
		{
			if (originalItem == null || newItem == null) return;

            newItem.InnerItem.Editing.BeginEdit();
            foreach (var fieldName in _fieldNames)
			{
				var newField = newItem.InnerItem.Fields[fieldName];
				var oldField = originalItem.InnerItem.Fields[fieldName];
				if (newField != null && oldField != null)
				{
					newField.Value = oldField.Value;
				}
			}
            newItem.InnerItem.Editing.EndEdit(false, false);

            newItem.Database.Caches.ItemCache.RemoveItem(newItem.ID);
        }
        
		public MediaItem FindOrCreateMediaItem(IDataMap map, MediaItem originalItem)
		{
			// see if it exists in med lib
			var newMediaItem = map.ToDB.GetItem(originalItem.InnerItem.Paths.FullPath);

			MediaItem m = null;
			if (newMediaItem != null)
			{
				m = new MediaItem(newMediaItem);
			}
			else
			{
				var parent = map.ToDB.GetItem(originalItem.InnerItem.Paths.ParentPath);
				if (parent == null)
				    BuildMediaPath(map.ToDB, originalItem.InnerItem.Paths.ParentPath);
				
				m = ImportMedia(originalItem);
			}

			if (m == null)
				map.Logger.Log("Image Not Found", $"item '{m.Path}', image '{m.Name}'");
			else
				CopyFields(originalItem, m);
			
			return m;
		}

		public Item BuildMediaPath(Database toDb, string path)
		{
			var item = toDb.GetItem(path);
			if (item == null)
			{
				var parentPath = path.Substring(0, path.LastIndexOf("/"));
				var itemName = path.Substring(path.LastIndexOf("/") + 1);
				var parent = BuildMediaPath(toDb, parentPath);
				item = parent.Add(itemName, new TemplateID(TemplateIDs.MediaFolder));
			}
			return item;
		}
        
        public virtual bool IsMediaFile(Item item)
        {
            if (!item.Paths.FullPath.StartsWith("/sitecore/media library/"))
                return false;

            var bases = GetBaseTemplates(item).ToList();

            return bases
                .Any(a =>
                    a.ID.Guid.Equals(TemplateIDs.UnversionedFile.Guid)
                    || a.ID.Guid.Equals(TemplateIDs.VersionedFile.Guid));
        }

        public virtual IEnumerable<TemplateItem> GetBaseTemplates(Item i)
        {
            return Enumerable.Concat(new List<TemplateItem> { i.Template }, i.Template.BaseTemplates.SelectMany(GetBaseTemplates));
        }

        public virtual IEnumerable<TemplateItem> GetBaseTemplates(TemplateItem t)
        {

            if (t.ID.Guid.Equals(TemplateIDs.StandardTemplate.Guid))
                return new TemplateItem[0];

            return new[] { t }
                    .Concat(t.BaseTemplates.SelectMany(GetBaseTemplates));
        }
        
        public string TransferImages(SitecoreDataMap map, string fieldValue)
        {
            if (map == null)
                return fieldValue;

            var matchList = StringService.FindIDsInText(fieldValue);
            var newFieldValue = string.Copy(fieldValue);

            foreach (var match in matchList)
            {
                var oldItem = map.FromDB.GetItem(new ID(new Guid(match.Value)));
                if (oldItem == null)
                    continue;

                if (!IsMediaFile(oldItem))
                    continue;

                var mediaItem = new MediaItem(oldItem);
                var newMediaItem = FindOrCreateMediaItem(map, mediaItem);
                var guidFormat = match.Value.Length == 38 ? "B" : "N";

                newFieldValue = newFieldValue.Replace(match.Value, newMediaItem.ID.Guid.ToString(guidFormat));
            }

            return newFieldValue;
        }

        public string GetYouTubeId(string fieldValue, Database fromDB)
        {
            if (string.IsNullOrWhiteSpace(fieldValue))
                return "";

            if (fieldValue.Contains("youtu"))
            {
                var newValue = StripYTPaths(fieldValue);
                Logger.Log($"YouTube video value: {fieldValue}", "N/A", Providers.ProcessStatus.VideoInfo, "", newValue);
                return newValue;
            }

            var videos = fieldValue.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var v in videos)
            {
                if (!ID.IsID(v))
                    continue;

                var videoItem = fromDB.GetItem(new ID(v));
                var videoValue = ((LinkField)videoItem?.Fields["Video Link"])?.Url;
                if (string.IsNullOrWhiteSpace(videoValue))
                    continue;

                var newValue = StripYTPaths(videoValue);
                Logger.Log($"YouTube video value: {videoValue}", "N/A", Providers.ProcessStatus.VideoInfo, "", newValue);

                return newValue;
            }

            return "";
        }

        protected string StripYTPaths(string value)
        {
            var stripList = new List<string>
            {
                "https://youtu.be/",
                "https://www.youtube.com/embed/",
                "https://www.youtube.com/v/"
            };

            foreach (var s in stripList)
            {
                value = value.Replace(s, "");
            }

            if (value.Contains("?"))
                value = value.Split(new string[] { "?" }, StringSplitOptions.RemoveEmptyEntries)[0];

            return value;
        }
        
        public MediaItem GetImage(Item item, IEnumerable<string> ExistingDataNames)
        {
            MediaItem mediaItem = null;
            foreach (var fieldName in ExistingDataNames)
            {
                mediaItem = GetImage(item, fieldName);
                if (mediaItem == null)
                    continue;

                return mediaItem;
            }

            return null;
        }
        
        protected MediaItem GetImage(Item item, string fieldName)
        {
            var mediaItem = ((ImageField)item.Fields[fieldName])?.MediaItem;

            return mediaItem;
        }
    }
}