﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Layouts;
using Sitecore.SharedSource.DataImporter.Logger;
using Sitecore.SharedSource.DataImporter.Providers;
using Sitecore.SharedSource.DataImporter.Services;

namespace Sitecore.SharedSource.DataImporter.Mappings.Fields
{
    public class FieldToComponent : BaseField
    {
        #region Properties
        
        public string Component { get; set; }
        public string Placeholder { get; set; }
        public string DatasourcePath { get; set; }
        public string Device { get; set; }
        public bool OverwriteExisting { get; set; }        
        public string DatasourceFolder { get; set; }
        public string Parameters { get; set; }
        public bool IsSXA { get; set; }
        public IEnumerable<string> ExistingDataNames { get; set; }
        public string Delimiter { get; set; }
        protected readonly char[] comSplitr = { ',' };
        protected PresentationService PresentationService { get; set; }
        protected MediaService MediaService { get; set; }
        
        #endregion Properties

        #region Constructor

        public FieldToComponent(Item i, ILogger l) : base(i, l)
        {
            ExistingDataNames = GetItemField(i, "From What Field").Split(comSplitr, StringSplitOptions.RemoveEmptyEntries);
            Delimiter = GetItemField(i, "Delimiter");
            Component = GetItemField(i, "Component");
            Placeholder = GetItemField(i, "Placeholder");
            DatasourcePath = GetItemField(i, "Datasource Path");
            Device = GetItemField(i, "Device");
            OverwriteExisting = GetItemField(i, "Overwrite Existing") == "1";
            DatasourceFolder = GetItemField(i, "Datasource Folder");
            Parameters = GetItemField(i, "Parameters");
            IsSXA = GetItemField(i, "Is SXA") == "1";
            PresentationService = new PresentationService(l);
            MediaService = new MediaService(l);
        }

        #endregion Constructor

        #region IBaseField

        public override void FillField(IDataMap map, ref Item newItem, object importRow)
        {
            var importValue = string.Join(Delimiter, map.GetFieldValues(ExistingDataNames, importRow));

            if (string.IsNullOrWhiteSpace(Component) || !ID.IsID(Component))
            {
                var path = importRow is Item ? ((Item)importRow).Paths.FullPath : "N/A";
                Logger.Log($"The Component value is empty or is not an id", path, LogType.FieldToComponent, "Component", Component);

                return;
            }

            var deviceItem = PresentationService.FindDeviceDefinition(newItem, Device);
            var dsName = DatasourcePath.Contains("/")
                ? DatasourcePath.Substring(DatasourcePath.LastIndexOf("/") + 1)
                : DatasourcePath;

            if (!OverwriteExisting)
            {
                var datasource = PresentationService.CreateDatasource(map, newItem, deviceItem, dsName, DatasourceFolder, DatasourcePath, Component, OverwriteExisting);
                if (datasource == null)
                    return;

                var isSet = SetField(map, datasource, importRow, importValue);
                if (!isSet)
                {
                    datasource.Delete();
                }
                else
                {
                    PresentationService.AddComponent(newItem, datasource, Placeholder, Component, Device, Parameters, IsSXA);
                }
            }
            else
            {
                var rendering = PresentationService.FindRendering(deviceItem, Placeholder, Component);
                if (rendering == null)
                {
                    Logger.Log($"There was no rendering matching device:{Device} - placeholder:{Placeholder} - component:{Component}", newItem.Paths.FullPath, LogType.MultilistToComponent, "device xml", deviceItem.ToXml());
                    return;
                }

                var datasource = PresentationService.FindDatasourceByName(map, newItem, rendering, dsName);
                if (datasource == null)
                {
                    Logger.Log($"There was no datasource found matching name:{dsName}", newItem.Paths.FullPath, LogType.MultilistToComponent, "rendering xml", rendering.ToXml());
                    return;
                }
                
                var isSet = SetField(map, datasource, importRow, importValue);
                if (!isSet)
                {
                    datasource.Delete();
                    PresentationService.RemoveComponent(newItem, rendering, Device);
                }
            }
        }

        public virtual bool SetField(IDataMap map, Item datasource, object importRow, string importValue)
        {
            var f = datasource.Fields[ToWhatField];
            if (f == null)
            {
                Logger.Log($"The {ToWhatField} is null", datasource.Paths.FullPath, LogType.FieldToComponent);
                return false;
            }

            datasource.Editing.BeginEdit();
            
            if (ToWhatField == "Text")
                importValue = MediaService.TransferImages((SitecoreDataMap)map, importValue);

            f.Value = importValue;
            datasource.Editing.EndEdit(false, false);

            datasource.Database.Caches.ItemCache.RemoveItem(datasource.ID);

            return true;
        }

        #endregion IBaseField
    }
}
