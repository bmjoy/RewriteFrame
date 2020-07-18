using UnityEngine;

namespace Leyoutech.Utility
{
	public static class ArrayUtility
	{
		/// <summary>
		/// 颠倒顺序，例如
		/// Input: 3, 8, 4, 7, 5
		/// Ouput: 5, 7, 4, 8, 3
		/// </summary>
		public static void Reverse<T>(ref T[] array)
		{
			for (int iItem = 0; iItem < Mathf.FloorToInt(array.Length * 0.5f); iItem++)
			{
				int swapIndex = array.Length - 1 - iItem;
				T swap = array[iItem];
				array[iItem] = array[swapIndex];
				array[swapIndex] = swap;
			}
		}
	}
}