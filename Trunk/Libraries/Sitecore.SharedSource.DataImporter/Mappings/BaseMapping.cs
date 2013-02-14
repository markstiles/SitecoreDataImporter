using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.Data.Items;

namespace Sitecore.SharedSource.DataImporter.Mappings {
	public class BaseMapping {

		#region Properties

		private string _newItemField;
		public string NewItemField {
			get {
				return _newItemField;
			}
			set {
				_newItemField = value;
			}
		}

		private string _HandlerClass;
		public string HandlerClass {
			get {
				return _HandlerClass;
			}
			set {
			_HandlerClass = value;
			}
		}

		private string _HandlerAssembly;
		public string HandlerAssembly {
			get {
				return _HandlerAssembly;
			}
			set {
				_HandlerAssembly = value;
			}
		}

		#endregion Properties

		#region Constructor

		public BaseMapping(Item i) {
			NewItemField = i.Fields["To What Field"].Value;
			HandlerClass = i.Fields["Handler Class"].Value;
			HandlerAssembly = i.Fields["Handler Assembly"].Value;
		}

		#endregion Constructor

		#region Methods

		#endregion Methods
	}
}
