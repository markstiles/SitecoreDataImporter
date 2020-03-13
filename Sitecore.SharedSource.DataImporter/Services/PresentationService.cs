using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Layouts;
using Sitecore.SecurityModel;
using Sitecore.SharedSource.DataImporter.Logger;
using Sitecore.SharedSource.DataImporter.Providers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Xml;

namespace Sitecore.SharedSource.DataImporter.Services
{
    public class PresentationService
    {
        protected ILogger Logger { get; set; }

        public PresentationService(ILogger logger)
        {
            Logger = logger;
        }

        public RenderingDefinition GetRendering(DeviceDefinition device, string placeholder, string componentId)
        {
            var rendering = device.Renderings?.Cast<RenderingDefinition>()
                .FirstOrDefault(a =>
                    a.DynamicProperties.Any(b =>
                        b.LocalName.Equals("id") &&
                        b.Value.Equals(componentId))
                    && a.DynamicProperties.Any(c =>
                        c.LocalName.Equals("ph") &&
                        c.Value.Equals(placeholder)));

            return rendering;
        }
        
        public void AddComponent(Item newItem, Item datasource, string placeholder, string componentId, string deviceId, string parameters)
        {
            //todo: add logic to convert this to sxa path local:/data/something  only when needed
            var datasourcePath = datasource.Paths.FullPath.Replace(newItem.Paths.FullPath, "local:");

            LayoutField layoutField = new LayoutField(newItem.Fields[FieldIDs.FinalLayoutField]);
            LayoutDefinition layoutDefinition = LayoutDefinition.Parse(layoutField.Value);
            DeviceDefinition deviceDefinition = layoutDefinition.GetDevice(deviceId);
            var rendering = new RenderingDefinition
            {
                ItemID = componentId,
                Datasource = datasourcePath, 
                Placeholder = placeholder,
                Parameters = parameters
            };

            deviceDefinition.AddRendering(rendering);
            
            layoutField.Value = layoutDefinition.ToXml();
        }

        public void RemoveComponent(Item newItem, RenderingDefinition rendering, string deviceId)
        {
            LayoutField layoutField = new LayoutField(newItem.Fields[FieldIDs.FinalLayoutField]);
            LayoutDefinition layoutDefinition = LayoutDefinition.Parse(layoutField.Value);
            DeviceDefinition deviceDefinition = layoutDefinition.GetDevice(deviceId);

            for (int i = 0; i < deviceDefinition.Renderings.Count; i++)
            {
                var tempRend = (RenderingDefinition)deviceDefinition.Renderings[i];
                if (tempRend.UniqueId == rendering.UniqueId)
                {
                    deviceDefinition.Renderings.RemoveAt(i);
                    break;
                }
            }

            layoutField.Value = layoutDefinition.ToXml();
        }

        public Item CreateDatasource(IDataMap map, Item newItem, DeviceDefinition device, string name, string datasourceFolderId, string datasourcePath, string componentId, bool overwriteExisting)
        {
            if (!ID.IsID(datasourceFolderId))
            {
                Logger.Log("The datasource folder is not an ID", newItem.Paths.FullPath, Providers.LogType.PresentationService, "datasourceFolderId", datasourceFolderId);
                return null;
            }                

            var componentItem = map.ToDB.GetItem(new ID(componentId));
            if (componentItem == null)
            {
                Logger.Log("The component is null", newItem.Paths.FullPath, Providers.LogType.PresentationService, "componentId", componentId);
                return null;
            }

            var datasourceTemplateValue = componentItem.Fields["Datasource Template"]?.Value;
            if (string.IsNullOrWhiteSpace(datasourceTemplateValue))
            {
                Logger.Log("The datasource template value is empty", componentItem.Paths.FullPath, Providers.LogType.PresentationService);
                return null;
            }

            var dsTemplate = (TemplateItem)map.ToDB.GetItem(datasourceTemplateValue);
            if (dsTemplate == null)
            {
                Logger.Log("The datasource template is null", componentItem.Paths.FullPath, Providers.LogType.PresentationService, "datasourceTemplateValue", datasourceTemplateValue);
                return null;
            }

            var folderTemplateItem = map.ToDB.GetTemplate(new ID(datasourceFolderId));
            if (folderTemplateItem == null)
            {
                Logger.Log("The folder template is null", componentItem.Paths.FullPath, Providers.LogType.PresentationService, "datasourceFolderId", datasourceFolderId);
                return null;
            }

            var curItem = newItem;
            var paths = datasourcePath.Contains("/")
                ? datasourcePath.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries)
                : new string[] { name };

            foreach (var p in paths)
            {
                var childItem = curItem.Axes.GetChild(p);
                if(childItem == null)
                {
                    var templateItem = p == name ? dsTemplate : folderTemplateItem;
                    curItem = curItem.Add(p, templateItem);
                    continue;
                }
                else if (p != name || overwriteExisting)
                {
                    curItem = childItem;
                    continue;
                }

                var i = 0;
                do
                {
                    i++;
                }
                while (curItem.Axes.GetChild($"{p}-{i}") != null);
                
                curItem = curItem.Add($"{p}-{i}", dsTemplate);
            }

            return curItem;
        }

        public Item GetDatasourceByName(IDataMap map, Item newItem, RenderingDefinition rendering, string name)
        {
            var ds = rendering.DynamicProperties.FirstOrDefault(a => a.LocalName.Equals("ds"));
            if (string.IsNullOrWhiteSpace(ds?.Value))
            {
                Logger.Log("The rendering has no datasource property", newItem.Paths.FullPath, Providers.LogType.PresentationService, "rendering", rendering.ToXml());
                return null;
            }

            Item dsItem = null;
            if (ID.IsID(ds.Value))
                dsItem = map.ToDB.GetItem(new ID(ds.Value));
            else if(ds.Value.Contains("local:"))
                dsItem = map.ToDB.GetItem($"{newItem.Paths.FullPath}{ds.Value.Replace("local:", "")}");
            else
                dsItem = map.ToDB.GetItem(ds.Value);

            if (dsItem == null)
            {
                Logger.Log("The datasource item is null", newItem.Paths.FullPath, Providers.LogType.PresentationService, "ds.Value", ds.Value);
                return null;
            }
            else if (!dsItem.Name.Equals(name))
            {
                Logger.Log($"The datasource item name '{dsItem.Name}' doesn't equal '{name}'", dsItem.Paths.FullPath, Providers.LogType.PresentationService, "dsItem.Name", dsItem.Name);
                return null;
            }

            return dsItem;
        }
    }
}