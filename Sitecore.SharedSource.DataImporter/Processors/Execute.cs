﻿using Sitecore.Data.Items;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;


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
    }
}