using Sitecore.Configuration;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Resources.Media;
using Sitecore.SecurityModel;
using Sitecore.SharedSource.DataImporter.Reporting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Sitecore.SharedSource.DataImporter.Processors.Helpers
{
    public class MediaUpload
    {
        public static MediaItem UploadMedia(string mediaURl, string destination, Item item, ImageField imageField = null)
        {

            string uploadedMediaPath = string.Empty;
            MediaItem mediaItem = null;
            string cleanMediaURl = mediaURl;
            if (mediaURl.Contains("?"))
            {
                cleanMediaURl = cleanMediaURl.Remove(cleanMediaURl.IndexOf('?'));
            }
            string extension = Path.GetExtension(cleanMediaURl);
            string mediaName = Path.GetFileName(cleanMediaURl);
            mediaName = mediaName.Replace(extension, "").Replace("-", " ").Replace("_", " ").Trim();

            //int nameStartIndex = mediaURl.LastIndexOf('/');
            //mediaURl.Substring(nameStartIndex).Replace(extension, "");

            try
            {
                var webRequest = WebRequest.Create(mediaURl);
                using (var webResponse = webRequest.GetResponse())
                {
                    using (var stream = webResponse.GetResponseStream())
                    {
                        if (stream != null)
                        {
                            using (var memoryStream = new MemoryStream())
                            {
                                stream.CopyTo(memoryStream);

                                var mediaCreator = new MediaCreator();
                                var options = new MediaCreatorOptions
                                {
                                    Versioned = false,
                                    IncludeExtensionInItemName = true,
                                    Database = item.Database,
                                    Destination = destination + mediaName
                                };

                                using (new SecurityDisabler())
                                {
                                    mediaItem = item.Database.GetItem(options.Destination);
                                    if (mediaItem == null)
                                    {
                                        var creator = new MediaCreator();
                                        mediaItem = MediaManager.Creator.CreateFromStream(memoryStream, mediaName + extension, options);
                                        ImportReporter.Write(item, Level.Info, string.Format("New media item has been imported from {0}", mediaURl), extension, "Media Import");
                                    }
                                    mediaItem.BeginEdit();

                                    if (imageField != null)
                                    {
                                        imageField.MediaID = mediaItem.ID;
                                    }
                                    
                                    uploadedMediaPath = mediaItem.MediaPath;
                                    mediaItem.Alt = mediaName;
                                    mediaItem.EndEdit();
                                }
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                ImportReporter.Write(item, Level.Error, string.Format("Error during media import from {0}. Error: {1}", mediaURl, ex.Message), extension, "Media Import");

            }

            return mediaItem;
        }

    }
}
