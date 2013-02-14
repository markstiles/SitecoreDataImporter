using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.Data.Items;
using System.Data;

namespace Sitecore.SharedSource.DataImporter.Mappings.Fields {
	
	public abstract class BaseField : BaseMapping {

		#region Properties

		#endregion Properties

		#region Constructor

		public BaseField(Item i) : base(i) {

		}

		#endregion Constructor

		#region Methods

		public abstract void FillField(ref Item newItem, DataRow importRow);
		
		public abstract void FillField(ref Item newItem, Item importRow);

		#endregion Methods
	}
}
