using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sitecore.SharedSource.DataImporter.Comparers
{
	//------------------------------------------------------------------------------------------------- 
	// <copyright file="TitleComparer.cs" company="Sitecore Shared Source">
	// Copyright (c) Sitecore.  All rights reserved.
	// </copyright>
	// <summary>
	// Defines the Sitecore.Sharedsource.Data.Comparers.ItemComparers.TitleComparer type.
	// </summary>
	// <license>
	// http://sdn.sitecore.net/Resources/Shared%20Source/Shared%20Source%20License.aspx
	// </license>
	// <url>http://trac.sitecore.net/FieldValueComparer/</url>
	//-------------------------------------------------------------------------------------------------

	// <summary>
	// Example subclass of Sitecore.Sharedsource.Data.Comparers.ItemComparers.FieldValueComparer
	// to sort items by the field named Title.
	// </summary>
	public class TitleComparer : FieldValueComparer
	{
		// <summary>
		// Initializes a new instance of the TitleComparer class. All isstances compare
		// items by the values in the field named Title.
		// </summary>
		public TitleComparer() : base("title") {
		}
	}
}
