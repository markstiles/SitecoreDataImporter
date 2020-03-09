using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.SharedSource.DataImporter.Logger;

namespace Sitecore.SharedSource.DataImporter.Providers
{
	public class SitecoreDeleteMap : SitecoreDataMap
	{
		public SitecoreDeleteMap(Database db, string connectionString, Item importItem, ILogger l) : base(db, connectionString, importItem, l)
		{
		}

		public override Item CreateNewItem(Item parent, object importRow, string newItemName)
		{
			Item item = (Item) importRow;

            item?.Delete();

            return null;
		}
	}
}