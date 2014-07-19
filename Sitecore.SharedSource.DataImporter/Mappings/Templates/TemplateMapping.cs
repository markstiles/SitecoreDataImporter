using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sitecore.Data.Items;

namespace Sitecore.SharedSource.DataImporter.Mappings.Templates {
	public class TemplateMapping {

		#region Properties

		private string _FromWhatTemplate;
		/// <summary>
		/// the template the old item is from
		/// </summary>
		public string FromWhatTemplate {
			get {
				return _FromWhatTemplate;
			}
			set {
				_FromWhatTemplate = value;
			}
		}

		private string _ToWhatTemplate;
		/// <summary>
		/// the template the new item is going to
		/// </summary>
		public string ToWhatTemplate {
			get {
				return _ToWhatTemplate;
			}
			set {
				_ToWhatTemplate = value;
			}
		}

		#endregion

		//constructor
		public TemplateMapping(Item i) {
			FromWhatTemplate = i.Fields["From What Template"].Value;
			ToWhatTemplate = i.Fields["To What Template"].Value;
		}

		

	}
}
