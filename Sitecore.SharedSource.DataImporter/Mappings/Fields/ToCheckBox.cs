using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.SharedSource.DataImporter.Providers;
using Sitecore.StringExtensions;

namespace Sitecore.SharedSource.DataImporter.Mappings.Fields
{
    
    public class ToCheckBox : ToText, IBaseField
    {
        #region properties
        private string positiveValues;

        public string PositiveValues
        {
            get { return positiveValues; }
            set { positiveValues = value; }
        }
  
        private string negativeValues;

        public string NegativeValues
        {
            get { return negativeValues; }
            set { negativeValues = value; }
        }
                
        public List<string> PositiveValuesList { get; set; }
        public List<string> NegativeValuesList { get; set; }

        private List<string> messages;
        private const string FileLink = @"D:/data/log.txt";

        #endregion

        public ToCheckBox(Item i) : base(i)
        {
            if (i.Fields["PositiveValues"] != null)
            {
                PositiveValues = i.Fields["PositiveValues"].Value;
            }

            if (i.Fields["NegativeValues"] != null)
            {
                NegativeValues = i.Fields["NegativeValues"].Value;
            }

            messages = new List<string>();
            SetupLists();
        }

        #region public methods

        public override void FillField(BaseDataMap map, ref Item newItem, string importValue)
        {
            string checkBoxValue = "0";
            if (!importValue.IsNullOrEmpty())
            {
                importValue = importValue.Trim().ToLower();
            }

            if (PositiveValuesList.Any() && NegativeValuesList.Any())
            {  
                if (PositiveValuesList.Select(x => x.Trim().ToLower()).Contains(importValue))
                {
                    checkBoxValue = "1";
                }
                else if (NegativeValuesList.Select(x => x.Trim().ToLower()).Contains(importValue))
                {
                    checkBoxValue = "0";
                }
               
                Field f = newItem.Fields[NewItemField];
                //store the imported value as is         
                Log("f", f.ToString());
                if (f != null)
                {
                    
                    f.Value = checkBoxValue;                    
                }
                //Log("******************* end loop", "*********************");
                //System.IO.File.WriteAllLines(FileLink,messages);
            }
            
        }

        #endregion

        #region private methods

        private void SetupLists()
        {        
            const string delimiter = ",";

            if (!string.IsNullOrEmpty(PositiveValues))
            {                
                Log("positive values", PositiveValues);
                PositiveValuesList = PositiveValues.Split(new string[] { delimiter }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }
            if (!string.IsNullOrEmpty(NegativeValues))
            {
                Log("negative values", NegativeValues);
                NegativeValuesList = NegativeValues.Split(new string[] { delimiter }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }
        }

        private void Log(string message, string value)
        {
            messages.Add(string.Concat(message,": ", value));
        }

        #endregion private methods
    }
}