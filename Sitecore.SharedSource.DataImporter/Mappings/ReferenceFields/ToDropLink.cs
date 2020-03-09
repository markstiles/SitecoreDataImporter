using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.SharedSource.DataImporter.Logger;
using Sitecore.SharedSource.DataImporter.Providers;

namespace Sitecore.SharedSource.DataImporter.Mappings.ReferenceFields
{
	public class ToDropLink : BaseMapping, IBaseFieldWithReference
	{
		public ToDropLink(Item i, ILogger l) : base(i, l)
		{
		}

        public string Name { get; set; }

        public void FillField(IDataMap map, ref Item newItem, Item importRow, string fieldName)
		{
			LinkField oldField = importRow.Fields[fieldName];

			if (oldField?.TargetItem == null) return;

			newItem.Fields[NewItemField].Value = oldField.TargetID.ToString();
		}

		public string GetExistingFieldName()
		{
			return GetItemField(InnerItem, "From What Fields");
		}
	}
}