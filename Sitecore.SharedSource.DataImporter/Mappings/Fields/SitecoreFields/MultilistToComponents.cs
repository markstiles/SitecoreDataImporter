using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Layouts;
using Sitecore.SharedSource.DataImporter.Logger;
using Sitecore.SharedSource.DataImporter.Mappings.Fields.Models;
using Sitecore.SharedSource.DataImporter.Providers;
using Sitecore.SharedSource.DataImporter.Services;

namespace Sitecore.SharedSource.DataImporter.Mappings.Fields.SitecoreFields
{
    public class MultilistToComponents : BaseField
    {
        #region Properties

        public string Device { get; set; }
        public string DatasourceFolder { get; set; }
        public IEnumerable<string> ExistingDataNames { get; set; }
        public string Delimiter { get; set; }

        protected readonly char[] comSplitr = { ',' };
        protected Dictionary<string, ComponentMap> ComponentMaps { get; set; }
        protected PresentationService PresentationService { get; set; }
        protected MediaService MediaService { get; set; }
        protected FieldService FieldService { get; set; }

        #endregion Properties

        #region Constructor

        public MultilistToComponents(Item i, ILogger l) : base(i, l)
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
                    Fields = new Dictionary<string, IBaseField>(),
                    Parameters = GetItemField(m, "Parameters")
                };

                foreach (Item f in m.GetChildren())
                {
                    var bf = FieldService.BuildBaseField(f);
                    var a = GetItemField(f, "From What Field");
                    if (string.IsNullOrWhiteSpace(a) || bf == null)
                        continue;

                    c.Fields.Add(a, bf);
                }

                ComponentMaps.Add(c.FromWhatTemplate, c);
            }
        }

        #endregion Constructor

        #region IBaseField
        
        public override void FillField(IDataMap map, ref Item newItem, object importRow)
        {
            var importValue = string.Join(Delimiter, map.GetFieldValues(ExistingDataNames, importRow));

            if (string.IsNullOrEmpty(importValue))
            {
                var path = importRow is Item ? ((Item)importRow).Paths.FullPath : "N/A";
                Logger.Log($"There was no import value", path, LogType.MultilistToComponent);
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
                    Logger.Log($"There was no component mapping found", i.Paths.FullPath, LogType.MultilistToComponent, i.TemplateName, itemTemplate);
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
                        Logger.Log($"There was no datasource created for matching device:{Device} - placeholder:{cm.Placeholder} - component:{cm.Component}", i.Paths.FullPath, LogType.MultilistToComponent, "device xml", deviceItem.ToXml());
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
                        Logger.Log($"There was no rendering matching device:{Device} - placeholder:{cm.Placeholder} - component:{cm.Component}", i.Paths.FullPath, LogType.MultilistToComponent, "device xml", deviceItem.ToXml());
                        continue;
                    }

                    var datasource = PresentationService.GetDatasourceByName(map, newItem, rendering, dsName);                    
                    if (datasource == null)
                    {
                        Logger.Log($"There was no datasource found matching name:{dsName}", i.Paths.FullPath, LogType.MultilistToComponent, "rendering xml", rendering.ToXml());
                        continue;
                    }

                    SetFields(i, datasource, cm, sitecoreMap);
                }
            }            
        }

        public virtual void SetFields(Item importItem, Item datasource, ComponentMap cm, SitecoreDataMap sitecoreMap)
        {
            datasource.Editing.BeginEdit();
            foreach (var kvp in cm.Fields)
            {
                kvp.Value.FillField(sitecoreMap, ref datasource, importItem);
            }
            datasource.Editing.EndEdit(false, false);
            datasource.Database.Caches.ItemCache.RemoveItem(datasource.ID);
        }
                
        #endregion IBaseField
    }
}
