using UnityEngine;

namespace Leyoutech.Utility
{
	public static class GizmosUtility
	{
		public static void DrawBounds(Bounds bounds)
		{
			Gizmos.DrawCube(bounds.center, bounds.size);
		}

		public static void DrawBounds(Bounds bounds, Color color)
		{
			Gizmos.color = color;
			DrawBounds(bounds);
		}
	}
}