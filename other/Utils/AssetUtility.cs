#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Leyoutech.Utility
{
	public static class AssetUtility
	{
		public const string BUILTIN_GUID = "0000000000000000f000000000000000";
		public const string LIBRARY_GUID = "0000000000000000e000000000000000";

#if UNITY_EDITOR
		public static string GetAssetGuid(Object asset)
		{
			string assetPath = AssetDatabase.GetAssetPath(asset);
			string guid = AssetDatabase.AssetPathToGUID(assetPath);
			return guid;
		}

		/// <summary>
		/// 判断Asset是否是Builtin或Library目录下的
		/// </summary>
		public static bool IsBuiltInOrLibraryWithAssetPath(string assetPath)
		{
			string guid = AssetDatabase.AssetPathToGUID(assetPath);
			return guid == BUILTIN_GUID
				|| guid == LIBRARY_GUID;
		}

		/// <summary>
		/// 判断Asset是否是Builtin或Library目录下的
		/// </summary>
		public static bool IsBuiltInOrLibraryAsset(Object asset)
		{
			return IsBuiltInOrLibraryWithAssetPath(AssetDatabase.GetAssetPath(asset));
		}
#endif
	}
}