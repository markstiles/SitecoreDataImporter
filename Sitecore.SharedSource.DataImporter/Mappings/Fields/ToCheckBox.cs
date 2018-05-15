using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Pipelines.RenderField;
using Sitecore.SharedSource.DataImporter.Logger;
using Sitecore.SharedSource.DataImporter.Providers;
using Sitecore.StringExtensions;

namespace Sitecore.SharedSource.DataImporter.Mappings.Fields
{
    
    public class ToCheckBox : ToText
    {
        #region properties
        
        public List<string> PositiveValuesList { get; set; }
        public List<string> NegativeValuesList { get; set; }

        #endregion

        public ToCheckBox(Item i, ILogger l) : base(i, l)
		{
            string pValues = GetItemField(i, "PositiveValues");
            string nValues = GetItemField(i, "NegativeValues");

            const string delimiter = ",";

            PositiveValuesList = (!string.IsNullOrEmpty(pValues)) 
                ? pValues.Split(new string[] { delimiter }, StringSplitOptions.RemoveEmptyEntries).ToList()
                : new List<string>();
            
            NegativeValuesList = (!string.IsNullOrEmpty(nValues)) 
                ? nValues.Split(new string[] { delimiter }, StringSplitOptions.RemoveEmptyEntries).ToList()
                : new List<string>();
        }

        #region public methods

        public override void FillField(IDataMap map, ref Item newItem, string importValue)
        {
            if (importValue.IsNullOrEmpty())
                return;
            
            bool b = false;
            if (!bool.TryParse(importValue.Trim().ToLower(), out b)) {
                map.Logger.Log("ToCheckBox.FillField", string.Format("Couldn't parse the boolean value {0} for item {1}", importValue, newItem.Paths.FullPath));
                return;
            }
               
            Field f = newItem.Fields[NewItemField];
            if (f == null)
                return;
    
            f.Value = (b) ? "1" : "0";                    
        }

        #endregion
    }
}