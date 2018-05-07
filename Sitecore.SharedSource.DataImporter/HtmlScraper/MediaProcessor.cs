using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sitecore.SharedSource.DataImporter.HtmlScraper
{
    public class MediaProcessor
    {
        public List<MediaType> MediaTypes { get; set; }
        public string RootDestination { get; set; }
        public bool KeepHeirarchy { get; set; }
     

        public MediaProcessor(Item processor)
        {
            MediaTypes = RetrieveMediaTypes(processor);
            var root = processor.Fields["Root Destination"].Value;
            RootDestination = processor.Database.GetItem(root).Paths.Path;
            CheckboxField importExternalMediaCheck = (CheckboxField)processor.Fields["Import External Media URLs"];
        }

        public string RetrieveDestination(Item processor, string rootDestination, string source, Uri mediaSource, Uri baseUri)
        {
            CheckboxField keepHeirarchy = (CheckboxField)processor.Fields["Keep Folder Heirarchy"];
            string finalDestination;
            string cleanSource = source;

            if (cleanSource.Contains("?")) {
                cleanSource = cleanSource.Remove(cleanSource.IndexOf('?'));
            }

            if (keepHeirarchy.Checked)
            {
                if (mediaSource.Host == baseUri.Host)
                {
                    finalDestination = string.Concat(rootDestination, Path.GetDirectoryName(cleanSource), "/").Replace("%20", "-");
                }
                else
                {
                    finalDestination = string.Concat(rootDestination, Path.GetDirectoryName(mediaSource.PathAndQuery), "/").Replace("%20", "-");
                }
            }
            else
            {
                finalDestination = rootDestination + "/";
            }
            return finalDestination.Replace("\\", "/").Replace("//", "/").Replace("/-/media/", "/");
        }


        private List<MediaType> RetrieveMediaTypes(Item processor)
        {
            List<MediaType> mediaTypes = new List<MediaType>();

            MultilistField mappings = processor.Fields["Media Types"];
            foreach (var id in mappings.TargetIDs)
            {
                var map = processor.Database.GetItem(id);
                MediaType media = new MediaType();
                media.Identifier = map.Fields["Identifier"].Value;
                media.Attribute = map.Fields["Attribute"].Value;
                media.Extension = map.Fields["Extension"].Value;
                mediaTypes.Add(media);
            }
            return mediaTypes;
        }

    }
}
