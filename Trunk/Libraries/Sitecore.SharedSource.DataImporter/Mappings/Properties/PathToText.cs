using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.SharedSource.DataImporter.Comparers;
using Sitecore.Data.Items;
using Sitecore.Data.Fields;
using System.Web;
using Sitecore.SharedSource.DataImporter.Extensions;

namespace Sitecore.SharedSource.DataImporter.Mappings.Properties
{
	public class PathToText : BaseProperty {

		#region Properties

		#endregion Properties

		#region Constructor

		//constructor
		public PathToText(Item i) : base(i) {

		}

		#endregion Constructor

		#region Methods

		//fills it's own field
		public override void FillField(ref Item newItem, Item importRow) {

			newItem.Fields[NewItemField].Value = importRow.Paths.Path;
		}

		#endregion Methods
	}
}
