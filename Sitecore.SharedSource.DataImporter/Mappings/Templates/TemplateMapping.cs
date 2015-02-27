using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sitecore.Data.Items;
using Sitecore.SharedSource.DataImporter.Mappings.Fields;
using Sitecore.SharedSource.DataImporter.Mappings.Properties;

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

        private List<IBaseField> _fieldDefinitions = new List<IBaseField>();
        /// <summary>
        /// the definitions of fields to import
        /// </summary>
        public List<IBaseField> FieldDefinitions {
            get {
                return _fieldDefinitions;
            }
            set {
                _fieldDefinitions = value;
            }
        }

        /// <summary>
        /// List of properties
        /// </summary>
        public List<IBaseProperty> PropertyDefinitions {
            get {
                return _propDefinitions;
            }
            set {
                _propDefinitions = value;
            }
        }
        private List<IBaseProperty> _propDefinitions = new List<IBaseProperty>();

		#endregion

		//constructor
		public TemplateMapping(Item i) {
			FromWhatTemplate = i.Fields["From What Template"].Value;
			ToWhatTemplate = i.Fields["To What Template"].Value;
		}
	}
}
