using Sitecore.Data.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sitecore.SharedSource.DataImporter.Processors
{
    public interface IProcessor
    {
        void Run(object importRow, Item newItem);
    }
}