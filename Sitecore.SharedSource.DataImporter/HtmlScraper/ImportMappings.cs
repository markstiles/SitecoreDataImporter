using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.Data.Items;
using System.Collections.Specialized;
using Sitecore.Data;

namespace Sitecore.SharedSource.DataImporter.HtmlScraper
{
    public class ImportMappings 
    {
        public Item TemplateItem { get; set; }
        public NameValueCollection Mappings { get; set; }
        
    }
    public class MapData
    {
       
        public List<ImportMappings> MappedData { get; set; }

        public MapData()
        {

        }
        public MapData(Item selectedConfig, Database db) 
        {
             //var masterDB = Sitecore.Configuration.Factory.GetDatabase("master");
            
            //List<Item> mappingList = selectedConfig.GetChildren().Where(t => t.TemplateName == "Mapping").ToList();
            ImportMappings mappingSet = new ImportMappings();
            mappingSet.Mappings = new NameValueCollection();
            MappedData = new List<ImportMappings>();

            mappingSet.TemplateItem = db.GetItem(selectedConfig.Fields["Import To What Template"].Value);
            string fieldVal = selectedConfig.Fields["Mappings"].Value.Trim();
               
                string[] mappingsData = fieldVal.Split(new[] { '\n' });

                //map[0] is htmlObject XPATH
                //map[1] is the sitecore field name
                //map[2] if there is it will be one to one mapping URL separated with |
                foreach (var m in mappingsData)
                {
                    string[] map = m.Split(':');
                    mappingSet.Mappings.Add(map[0], map[1]);                                 
                }
                MappedData.Add(mappingSet);
        }    
    }
}