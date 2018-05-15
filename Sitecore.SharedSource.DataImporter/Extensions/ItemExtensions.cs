using Sitecore.Data.Items;

namespace Sitecore.SharedSource.DataImporter.Extensions
{
    public static class ItemExtensions
    {
		/// <summary>
		/// This will determine if the current item is not null
		/// </summary>
		/// <param name="curItem">The item to check for null</param>
		/// <returns>Returns true it the current item is null false otherwise</returns>
		public static bool IsNotNull(this Item curItem) {
			return !IsNull(curItem);
		}

		/// <summary>
		/// This will determine if the current item is null
		/// </summary>
		/// <param name="curItem">The item to check for null</param>
		/// <returns>Returns true it the current item is null false otherwise</returns>
		public static bool IsNull(this Item curItem) {
			return (curItem == null) ? true : false;
		}  
    }
}