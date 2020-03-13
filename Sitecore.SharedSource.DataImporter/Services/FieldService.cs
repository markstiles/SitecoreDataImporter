using Sitecore.Data.Items;
using Sitecore.SharedSource.DataImporter.Logger;
using Sitecore.SharedSource.DataImporter.Mappings.Fields;
using Sitecore.SharedSource.DataImporter.Providers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace Sitecore.SharedSource.DataImporter.Services
{
    public class FieldService
    {
        protected ILogger Logger { get; set; }

        public FieldService(ILogger logger)
        {
            Logger = logger;
        }

        public IBaseField BuildBaseField(Item fieldItem)
        {
            //create an item to get the class / assembly name from
            var handlerAssembly = fieldItem.Fields["Handler Assembly"].Value;
            var handlerClass = fieldItem.Fields["Handler Class"].Value;

            //check for assembly
            if (string.IsNullOrEmpty(handlerAssembly))
            {
                Logger.Log("the field's Handler Assembly is not defined", fieldItem.Paths.FullPath, LogType.ImportDefinitionError, fieldItem.Name, handlerAssembly);
                return null;
            }

            //check for class
            if (string.IsNullOrEmpty(handlerClass))
            {
                Logger.Log("the field's Handler Class is not defined", fieldItem.Paths.FullPath, LogType.ImportDefinitionError, fieldItem.Name, handlerClass);
                return null;
            }

            //create the object from the class and cast as base field to add it to field definitions
            IBaseField bf = null;
            try
            {
                var obj = Sitecore.Reflection.ReflectionUtil.CreateObject(handlerAssembly, handlerClass, new object[] { fieldItem, Logger });

                if (obj is IBaseField)
                    bf = (IBaseField)obj;
            }
            catch (FileNotFoundException)
            {
                Logger.Log("the field's binary specified could not be found", fieldItem.Paths.FullPath, LogType.ImportDefinitionError, fieldItem.Name, handlerAssembly);
            }

            return bf;
        }
    }
}