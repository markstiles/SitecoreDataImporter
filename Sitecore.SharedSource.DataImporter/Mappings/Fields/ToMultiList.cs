using Sitecore.Data.Items;
using Sitecore.SharedSource.DataImporter.Logger;

namespace Sitecore.SharedSource.DataImporter.Mappings.Fields
{
   
    public class ToMultiList : ToListItem
    {
        public ToMultiList(Item i, ILogger l) : base(i, l)
		{
        }
    }
}