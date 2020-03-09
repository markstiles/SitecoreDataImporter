using System;
using System.Collections.Generic;
using Sitecore.Layouts;

namespace Sitecore.SharedSource.DataImporter.Comparers
{
	public class RenderingPlaceholderComparer : IComparer<RenderingReference>
	{
		private static readonly string[] _placeholderKeys = new[]
		{
			"Default Body Components",
			"Body Middle Components Placeholder",
			"Default Body Footer Components",
			"Body Components"
		};

		public int Compare(RenderingReference x, RenderingReference y)
		{
			int xPlaceholderIndex = GetPlaceholderIndex(x);
			int yPlaceholderIndex = GetPlaceholderIndex(y);

			if (xPlaceholderIndex == yPlaceholderIndex) return 0;

			if (yPlaceholderIndex < xPlaceholderIndex) return 1;

			return -1;
		}

		private int GetPlaceholderIndex(RenderingReference rendering)
		{
			var placeholderKey = rendering.Placeholder.ToLower().Replace("/content/", string.Empty);

			for (int i = 0; i < _placeholderKeys.Length; i++)
			{
				if (_placeholderKeys[i].Equals(placeholderKey, StringComparison.InvariantCultureIgnoreCase))
				{
					return i;
				}
			}

			return _placeholderKeys.Length;
		}
	}
}