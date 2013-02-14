using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.SharedSource.DataImporter.Extensions;
using Sitecore.Data.Items;
using Sitecore.Data;
using Sitecore.SharedSource.DataImporter.Mappings.Fields;
using Sitecore.SharedSource.DataImporter.Mappings.Properties;
using System.Web;

namespace Sitecore.SharedSource.DataImporter.Importer
{
	public class SitecoreDataMap : BaseDataMap {
		
		#region Properties
		
		private string _xpathQuery;
		public string XPathQuery {
			get {
				return _xpathQuery;
			}
			set {
				_xpathQuery = value;
			}
		}

		public List<BaseProperty> PropertyDefinitions {
			get {
				return _propDefinitions;
			}
			set {
				_propDefinitions = value;
			}
		}
		private List<BaseProperty> _propDefinitions = new List<BaseProperty>();
		
		#endregion Properties

		#region Constructor

		public SitecoreDataMap(Database db, Item importItem) : base(db, importItem) {
			
			//fill sitecore data map properties
			XPathQuery = importItem.Fields["XPath Query"].Value;
		}

		#endregion Constructor

		#region Methods

		public override void Process() {

			Item[] importItems = SitecoreDB.SelectItems(this.XPathQuery.CleanXPath());

			//Loop through the data source
			foreach (Item importRow in importItems) {

				string newItemName = GetNewItemName(importRow);
				
				Item thisParent = GetParentNode(importRow, newItemName);

				try {
					if (!string.IsNullOrEmpty(newItemName)) {
						//Create new item
						Sitecore.Data.Items.Item newItem = thisParent.Add(newItemName, NewItemTemplate);
						if (newItem != null) {
							using (new EditContext(newItem, true, false)) {

								//add in the field mappings
								foreach (BaseField d in this.FieldDefinitions) {
									d.FillField(ref newItem, importRow);
								}

								//add in the property mappings
								foreach (BaseProperty d in this.PropertyDefinitions) {
									d.FillField(ref newItem, importRow);
								}
							}
						}
					}
				} catch (Exception ex) {
					HttpContext.Current.Response.Write(ex.ToString() + "<br/>name: " + newItemName + "<br/>possibly a bad class name on the field map");
				}
			}
		}
		#endregion Methods
	}
}
