using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Layouts;
using Sitecore.SharedSource.DataImporter.Logger;
using Sitecore.SharedSource.DataImporter.Models;
using Sitecore.SharedSource.DataImporter.Providers;
using Sitecore.SharedSource.DataImporter.Services;

namespace Sitecore.SharedSource.DataImporter.Mappings.Fields
{
    public class ListToComponents : BaseMapping, IBaseField
    {
        #region Properties

        public string Name { get; set; }

        public string Device { get; set; }
        
        public string DatasourceFolder { get; set; }

        public IEnumerable<string> ExistingDataNames { get; set; }

        public string Delimiter { get; set; }

        protected readonly char[] comSplitr = { ',' };

        protected Dictionary<string, ComponentMap> ComponentMaps { get; set; }

        protected PresentationService PresentationService { get; set; }

        protected MediaService MediaService { get; set; }

        #endregion Properties

        #region Constructor

        public ListToComponents(Item i, ILogger l) : base(i, l)
        {
            ExistingDataNames = GetItemField(i, "From What Field").Split(comSplitr, StringSplitOptions.RemoveEmptyEntries);
            Delimiter = GetItemField(i, "Delimiter");
            Device = GetItemField(i, "Device");
            DatasourceFolder = GetItemField(i, "Datasource Folder");
            PresentationService = new PresentationService(l);
            ComponentMaps = new Dictionary<string, ComponentMap>();
            MediaService = new MediaService(l);

            var maps = InnerItem.GetChildren();
            
            foreach (Item m in maps)
            {
                var c = new ComponentMap
                {
                    FromWhatTemplate = GetItemField(m, "From What Template"),
                    Component = GetItemField(m, "Component"),
                    Placeholder = GetItemField(m, "Placeholder"),
                    DatasourcePath = GetItemField(m, "Datasource Path"),
                    OverwriteExisting = GetItemField(m, "Overwrite Existing") == "1",
                    Fields = new Dictionary<string, string>(),
                    Parameters = GetItemField(m, "Parameters")
                };

                foreach (Item f in m.GetChildren())
                {
                    var a = GetItemField(f, "From What Field");
                    var b = GetItemField(f, "To What Field");
                    if (string.IsNullOrWhiteSpace(a) || string.IsNullOrWhiteSpace(b))
                        continue;

                    c.Fields.Add(a, b);
                }

                ComponentMaps.Add(c.FromWhatTemplate, c);
            }
        }

        #endregion Constructor

        #region IBaseField
        
        public virtual void FillField(IDataMap map, ref Item newItem, object importRow, string importValue)
        {
            var importItem = (Item)importRow;
            if (string.IsNullOrEmpty(importValue))
            {
                Logger.Log($"There was no import value", importItem.Paths.FullPath, ProcessStatus.ListToComponentLog);
                return;
            }                
            
            var layoutField = newItem.Fields[FieldIDs.FinalLayoutField];
            if (layoutField == null)
                return;

            var layout = LayoutDefinition.Parse(layoutField.Value);
            var deviceItem = layout.GetDevice(Device);
            var entries = importValue.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
            var sitecoreMap = (SitecoreDataMap)map;
            var itemList = entries.Select(a => sitecoreMap.FromDB.GetItem(new ID(a))).Where(b => b != null).ToList();

            foreach(var i in itemList)
            {
                var itemTemplate = i.TemplateID.ToString();
                if (!ComponentMaps.ContainsKey(itemTemplate))
                {
                    Logger.Log($"There was no component mapping found", i.Paths.FullPath, ProcessStatus.ListToComponentLog, i.TemplateName, itemTemplate);
                    continue;
                }
                                    
                var cm = ComponentMaps[itemTemplate];
                var dsName = cm.DatasourcePath.Contains("/")
                    ? cm.DatasourcePath.Substring(cm.DatasourcePath.LastIndexOf("/") + 1)
                    : cm.DatasourcePath;

                if (!cm.OverwriteExisting)
                {
                    var datasource = PresentationService.CreateDatasource(map, newItem, deviceItem, dsName, DatasourceFolder, cm.DatasourcePath, cm.Component, cm.OverwriteExisting);
                    if (datasource == null)
                    {
                        Logger.Log($"There was no datasource created for matching device:{Device} - placeholder:{cm.Placeholder} - component:{cm.Component}", i.Paths.FullPath, ProcessStatus.ListToComponentLog, "device xml", deviceItem.ToXml());
                        continue;
                    }

                    SetFields(i, datasource, cm, sitecoreMap);
                    PresentationService.AddComponent(newItem, datasource, cm.Placeholder, cm.Component, Device, cm.Parameters);
                }
                else
                {
                    var rendering = PresentationService.GetRendering(deviceItem, cm.Placeholder, cm.Component);
                    if (rendering == null)
                    {
                        Logger.Log($"There was no rendering matching device:{Device} - placeholder:{cm.Placeholder} - component:{cm.Component}", i.Paths.FullPath, ProcessStatus.ListToComponentLog, "device xml", deviceItem.ToXml());
                        continue;
                    }

                    var datasource = PresentationService.GetDatasourceByName(map, newItem, rendering, dsName);                    
                    if (datasource == null)
                    {
                        Logger.Log($"There was no datasource found matching name:{dsName}", i.Paths.FullPath, ProcessStatus.ListToComponentLog, "rendering xml", rendering.ToXml());
                        continue;
                    }

                    SetFields(i, datasource, cm, sitecoreMap);
                }
            }            
        }

        public virtual void SetFields(Item importItem, Item datasource, ComponentMap cm, SitecoreDataMap sitecoreMap)
        {
            var heading = "";
            if (cm.FromWhatTemplate == "{FF334EDB-E32E-4E47-8AB2-84CFB6014C5E}" //text two column
                || cm.FromWhatTemplate == "{EA4AB904-F949-442E-B7B9-24EB7CA2371F}") // text single column
            {
                var headingValue = importItem.Fields["Heading"]?.Value;
                if (!string.IsNullOrWhiteSpace(headingValue))
                    heading = $"<h2>{headingValue}</h2>";
            }

            datasource.Editing.BeginEdit();
            foreach (var kvp in cm.Fields)
            {
                var newField = datasource.Fields[kvp.Value];
                if (newField == null)
                {
                    Logger.Log($"the new field wasn't found: {kvp.Value}", datasource.Paths.FullPath, ProcessStatus.ListToComponentLog);
                    continue;
                }

                var fieldValue = importItem.Fields[kvp.Key]?.Value;
                if (string.IsNullOrWhiteSpace(fieldValue) && string.IsNullOrWhiteSpace(heading))
                {
                    Logger.Log("the old field value was null or empty", importItem.Paths.FullPath, ProcessStatus.ListToComponentLog, kvp.Key);
                    continue;
                }

                if (kvp.Value.Equals("Video"))
                    fieldValue = MediaService.GetYouTubeId(fieldValue, sitecoreMap.FromDB);
                else if (kvp.Key.Equals("Media"))
                {
                    var tempMedia = GetMedia(importItem);
                    var newMedia = tempMedia != null 
                        ? MediaService.FindOrCreateMediaItem(sitecoreMap, tempMedia)
                        : null;
                    fieldValue = newMedia != null 
                        ? $"<image mediaid=\"{newMedia.ID}\" />"
                        : "";
                }
                else if (kvp.Value.Equals("Image") || kvp.Value.Equals("Text") || kvp.Value.Equals("Link"))
                    fieldValue = MediaService.TransferImages(sitecoreMap, fieldValue);
                
                newField.Value = $"{heading}{fieldValue}";
            }
            datasource.Editing.EndEdit(false, false);

            datasource.Database.Caches.ItemCache.RemoveItem(datasource.ID);
        }

        protected MediaItem GetMedia(Item item)
        {
            var mediaList = ((DelimitedField)item.Fields["Media"])?.Items;
            if (mediaList == null || !mediaList.Any())
                return null;

            foreach (var m in mediaList)
            {
                var mItem = item.Database.GetItem(m);

                if (MediaService.IsMediaFile(mItem))
                    return new MediaItem(mItem);
                
                var mediaItem = ((ImageField)mItem.Fields["Image"])?.MediaItem;
                if (mediaItem == null)
                    continue;

                return new MediaItem(mediaItem);
            }

            return null;
        }

        public IEnumerable<string> GetExistingFieldNames()
        {
            return ExistingDataNames;
        }

        public string GetFieldValueDelimiter()
        {
            return Delimiter;
        }

        #endregion IBaseField
    }
}
