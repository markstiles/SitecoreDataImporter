using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore;
using Sitecore.Data.Items;
using Sitecore.Data.Fields;

namespace Sitecore.SharedSource.DataImporter.Comparers
{
	//------------------------------------------------------------------------------------------------- 
	// <copyright file="FieldValueComparer.cs" company="Sitecore Shared Source">
	// Copyright (c) Sitecore.  All rights reserved.
	// </copyright>
	// <summary>
	// Defines the Sitecore.Sharedsource.Data.Comparers.ItemComparers.FieldValueComparer type.
	// </summary>
	// <license>
	// http://sdn.sitecore.net/Resources/Shared%20Source/Shared%20Source%20License.aspx
	// </license>
	// <url>http://trac.sitecore.net/FieldValueComparer/</url>
	//-------------------------------------------------------------------------------------------------
	
	#region Enums

	// <summary>
	// Potential sort states of an item relative to another.
	// </summary>
	public enum SortState {
		// <summary>
		// Indicates that the first item sorts before the second.
		// </summary>
		Correct = -1,

		// <summary>
		// Indicates that the items sort equivalently.
		// </summary>
		Equal = 0,

		// <summary>
		// Indicates that the second item sorts before the first.
		// </summary>
		Reverse = 1,
	}

	// <summary>
	// Data template field types for sorting purposes.
	// </summary>
	public enum FieldType {
		// <summary>
		// Unknown field type.
		// </summary>
		Unknown = -1,

		// <summary>
		// The plain text field type.
		// </summary>
		Text = 0,

		// <summary>
		// The integer field type.
		// </summary>
		Integer = 1,

		// <summary>
		// The date field type, also for Datetime fields.
		// </summary>
		Date = 2,

		// <summary>
		// The checkbox field type.
		// </summary>
		Checkbox = 3,

		// <summary>
		// The File field type.
		// </summary>
		File = 4,

		// <summary>
		// The Image field type.
		// </summary>
		Image = 5,

		// <summary>
		// The Number field type.
		// </summary>
		Number = 6,

		// <summary>
		// The Link field type.
		// </summary>
		Link = 7,

		// <summary>
		// The Delimited field type.
		// </summary>
		Delimited = 8,

		Droplink = 9
	}

	#endregion enums

	// <summary>
	// Comparer to sort <see cref="Item" /> by a common field.
	// </summary>
	public class FieldValueComparer : System.Collections.Generic.IComparer<Item> {
		#region Constructors

		// <summary>
		// Initializes a new instance of the FieldValueComparer class.
		// </summary>
		// <param name="fieldName">The name of the field by which to sort.</param>
		public FieldValueComparer(string fieldName) {
			this.Init(fieldName, FieldType.Unknown);
		}

		// <summary>
		// Initializes a new instance of the FieldValueComparer class.
		// </summary>
		// <param name="fieldName">The name of the field by which to sort.</param>
		// <param name="fieldType">The data type of the field by which to sort.</param>
		public FieldValueComparer(string fieldName, FieldType fieldType) {
			this.Init(fieldName, fieldType);
		}

		// <summary>
		// Initializes a new instance of the FieldValueComparer class.
		// </summary>
		// <param name="fieldID">The ID of the field by which to sort.</param>
		public FieldValueComparer(Sitecore.Data.ID fieldID) {
			this.Init(fieldID.ToString(), FieldType.Unknown);
		}

		// <summary>
		// Initializes a new instance of the FieldValueComparer class.
		// </summary>
		// <param name="fieldID">The ID of the field by which to sort.</param>
		// <param name="fieldType">The data type of the field by which to sort.</param>
		public FieldValueComparer(Sitecore.Data.ID fieldID, FieldType fieldType) {
			this.Init(fieldID.ToString(), fieldType);
		}

		#endregion

		#region Properties

		// <summary>
		// Gets or sets the name or ID of field by which to sort.
		// </summary>
		public string FieldIdentifier {
			get;
			set;
		}

		// <summary>
		// Gets or sets the data template field type.
		// </summary>
		public FieldType FieldType {
			get;
			set;
		}

		#endregion

		#region Public Methods

		// <summary>
		// Returns the field type if recognized from the name of the field type.
		// </summary>
		// <param name="typeName">The name of the field type.</param>
		// <returns>The field type if recognized from the name of the field type.</returns>
		public static FieldType GetFieldType(string typeName) {
			typeName = typeName.ToLower();

			if (typeName == "single-line text") {
				return FieldType.Text;
			}

			if (typeName == "checkbox") {
				return FieldType.Checkbox;
			}

			if (typeName == "file") {
				return FieldType.File;
			}

			if (typeName == "image") {
				return FieldType.Image;
			}

			if (typeName == "number") {
				return FieldType.Image;
			}

			return FieldType.Unknown;
		}

		// <summary>
		// Compare the two fields.
		// </summary>
		// <param name="firstField">The first field to compare.</param>
		// <param name="secondField">The second field to compare.</param>
		// <returns>
		// -1 if the first item sorts before the second, 0 if the items sort equivalently,
		// or 1 if the first item sorts after the second.
		// </returns>
		public int CompareFieldValues(Sitecore.Data.Fields.Field firstField, Sitecore.Data.Fields.Field secondField) {
			if (this.FieldType == FieldType.Unknown) {
				this.FieldType = this.GetFieldType(firstField, secondField);
			}

			if (this.FieldType == FieldType.Date) {
				DateTime firstDate = ((DateField)firstField).DateTime;
				DateTime secondDate = ((DateField)secondField).DateTime;
				
				return firstDate.CompareTo(secondDate);
			}

			if (this.FieldType == FieldType.Integer) {
				int firstInt = Int32.Parse(firstField.Value);
				int secondInt = Int32.Parse(secondField.Value);
				return firstInt.CompareTo(secondInt);
			}

			if (this.FieldType == FieldType.Number) {
				return System.Double.Parse(firstField.Value).CompareTo(
				  System.Double.Parse(secondField.Value));
			}

			if (this.FieldType == FieldType.Checkbox) {
				if (firstField.Value == "1" && secondField.Value != "1") {
					return (int)SortState.Correct;
				}

				if (firstField.Value != "1" && secondField.Value == "1") {
					return (int)SortState.Reverse;
				}

				return (int)SortState.Equal;
			}

			if (this.FieldType == FieldType.File || this.FieldType == FieldType.Image) {
				return ((Sitecore.Data.Fields.FileField)firstField).Src.CompareTo(
				  ((Sitecore.Data.Fields.FileField)secondField).Src);
			}

			return firstField.Value.CompareTo(secondField.Value);
		}

		// <summary>Compare the two items by a common field value.
		// </summary>
		// <param name="firstItem">The first item to compare.</param>
		// <param name="secondItem">The second item to compare.</param>
		// <returns>-1 if the first item sorts before the second,
		// 0 if the items sort equivalently,
		// or 1 if the first item sorts after the second.
		// </returns>
		public virtual int Compare(Sitecore.Data.Items.Item firstItem, Sitecore.Data.Items.Item secondItem) {
			Sitecore.Diagnostics.Assert.IsNotNull(firstItem, "firstItem");
			Sitecore.Diagnostics.Assert.IsNotNull(secondItem, "secondItem");
			Sitecore.Data.Fields.Field firstField = firstItem.Fields[this.FieldIdentifier];
			Sitecore.Data.Fields.Field secondField = secondItem.Fields[this.FieldIdentifier];

			if (secondField == null && firstField == null) {
				return (int)SortState.Equal;
			}

			if (firstField != null && secondField == null) {
				return (int)SortState.Correct;
			}

			if (firstField == null) {
				return (int)SortState.Reverse;
			}

			if (String.IsNullOrEmpty(secondField.Value) && !String.IsNullOrEmpty(firstField.Value)) {
				return (int)SortState.Correct;
			}

			if (String.IsNullOrEmpty(firstField.Value) && !String.IsNullOrEmpty(secondField.Value)) {
				return (int)SortState.Reverse;
			}

			if (firstField.Value == secondField.Value) {
				return (int)SortState.Equal;
			}

			return this.CompareFieldValues(firstField, secondField);
		}

		#endregion

		#region Protected Methods

		// <summary>
		// Gets the field type of the fields.
		// </summary>
		// <param name="firstField">The first field.</param>
		// <param name="secondField">The second field.</param>
		// <returns>The name of the data template field type.</returns>
		protected FieldType GetFieldType(Sitecore.Data.Fields.Field firstField, Sitecore.Data.Fields.Field secondField) {
			int test;
			string typeName = firstField.Type.ToLower();

			if (typeName == "integer" || firstField.ID == Sitecore.FieldIDs.Sortorder || (Int32.TryParse(firstField.Value, out test) && Int32.TryParse(secondField.Value, out test))) {
				return FieldType.Integer;
			}

			if (typeName == "date" || typeName == "datetime" || (Sitecore.DateUtil.IsIsoDate(firstField.Value) && Sitecore.DateUtil.IsIsoDate(secondField.Value))) {
				return FieldType.Date;
			}

			return GetFieldType(typeName);
		}

		#endregion

		#region Private Methods

		// <summary>
		// Initialize this item field comparer.
		// </summary>
		// <param name="field">The name or ID of the field by which to sort.</param>
		// <param name="fieldType">The type of the field.</param>
		private void Init(string field, FieldType fieldType) {
			this.FieldIdentifier = field;
			this.FieldType = fieldType;
		}

		#endregion
	}

}
