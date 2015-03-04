using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Globalization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sitecore.SharedSource.DataImporter.Tests.Items {
    public class FakeItem : Item {

        public static string NullIDValue =       "{00000000-0000-0000-0000-000000000000}";
        public static string ItemIDValue =       "{11111111-1111-1111-1111-111111111111}";
        public static string TemplateIDValue =   "{22222222-2222-2222-2222-222222222222}";
        public static string BranchIDValue =     "{33333333-3333-3333-3333-333333333333}";
        
        public FakeItem(FieldList fieldList, Database db, string itemName = "fake")
            : base(
                new ID(ItemIDValue), 
                new ItemData(
                    new ItemDefinition(
                        new ID(ItemIDValue), 
                        itemName,
                        new ID(TemplateIDValue),
                        new ID(BranchIDValue)
                    ), 
                    Language.Invariant, 
                    new Sitecore.Data.Version(1), 
                    fieldList
                ), 
                db
            ) { 
 
        }
    }
}
