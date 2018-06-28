using Sitecore.Data.Items;
using Sitecore.SharedSource.DataImporter.HtmlScraper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;


namespace Sitecore.SharedSource.DataImporter.Processors
{
    public class Processor
    {
        public static void Execute(Item processor, Item itemToProcess, Item fieldMapping)
        {
            string type = processor.Fields["Type"].Value;
            string method = processor.Fields["Method"].Value;
            string nameSpaceInfo = (type.Split(',')[0]).Trim();
            string dllInfo = (type.Split(',')[1]).Trim();
            dllInfo += ".dll";
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";

            Assembly assembly = Assembly.LoadFile(path + dllInfo);
            Type assemblyType = assembly.GetType(nameSpaceInfo);
            MethodInfo myMethod = assemblyType.GetMethod(method);
            
            object[] parameters = {processor, itemToProcess, fieldMapping};

            object obj = Activator.CreateInstance(assemblyType);
            myMethod.Invoke(obj, parameters);
        }

        public static string Execute(Item processor, HtmlDocument doc, string currentDirURL, string defaultTemplateID)
        {
            string type = processor.Fields["Type"].Value;
            string method = processor.Fields["Method"].Value;
            string nameSpaceInfo = (type.Split(',')[0]).Trim();
            string dllInfo = (type.Split(',')[1]).Trim();
            dllInfo += ".dll";
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";

            Assembly assembly = Assembly.LoadFile(path + dllInfo);
            Type assemblyType = assembly.GetType(nameSpaceInfo);
            MethodInfo myMethod = assemblyType.GetMethod(method);

            object[] parameters = {processor, doc, currentDirURL, defaultTemplateID};

            object obj = Activator.CreateInstance(assemblyType);
            var value = myMethod.Invoke(obj, parameters);

            return value.ToString();
        }

        public static void Execute(Item processor, ImportConfig config)
        {
            string type = processor.Fields["Type"].Value;
            string method = processor.Fields["Method"].Value;
            string nameSpaceInfo = (type.Split(',')[0]).Trim();
            string dllInfo = (type.Split(',')[1]).Trim();
            dllInfo += ".dll";
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";

            Assembly assembly = Assembly.LoadFile(path + dllInfo);
            Type assemblyType = assembly.GetType(nameSpaceInfo);
            MethodInfo myMethod = assemblyType.GetMethod(method);

            object[] parameters = { processor, config};

            object obj = Activator.CreateInstance(assemblyType);
            myMethod.Invoke(obj, parameters);
        }

    }
}
