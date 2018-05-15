using Sitecore.Data.Items;

namespace Sitecore.SharedSource.DataImporter.Mappings
{
    public interface IBaseMapping
    {
        Item InnerItem { get; set; }

        string NewItemField { get; set; }

        string HandlerClass { get; set; }

        string HandlerAssembly { get; set; }

        string ItemName();

        string GetItemField(Item i, string fieldName);
    }
}