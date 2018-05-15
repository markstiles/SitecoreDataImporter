using Sitecore.Data.Items;
using Sitecore.SharedSource.DataImporter.Logger;

namespace Sitecore.SharedSource.DataImporter.Mappings.Fields
{
    public class ToChecklist : ToListItem
    {
        #region properties

        #endregion properties

        public ToChecklist(Item i, ILogger l) : base(i, l)
		{
        }
    }
}