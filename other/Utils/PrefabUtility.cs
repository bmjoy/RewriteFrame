#if UNITY_EDITOR
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Leyoutech.Utility
{
	public static class PrefabUtility
	{
		private static MethodInfo PREFAB_UTILITY_CREATE_VARIANT_METHOD_INFO;
		private static object[] PREFAB_UTILITY_CREATE_VARIANT_PARAAMETERS;

		/// <summary>
		/// 创建Prefab的变体
		/// </summary>
		/// <param name="prefabAsset">注意：是Prefab的Asset，不能用Prefab的实例</param>
		/// <param name="outputAssetPath">创建的Prefab的Path，Example："Assets/Test.prefab"</param>
		public static void CreateVariant(GameObject prefabAsset, string outputAssetPath)
		{
			if (PREFAB_UTILITY_CREATE_VARIANT_METHOD_INFO == null)
			{
				PREFAB_UTILITY_CREATE_VARIANT_METHOD_INFO = typeof(UnityEditor.PrefabUtility).GetMethod("CreateVariant"
					, BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
				PREFAB_UTILITY_CREATE_VARIANT_PARAAMETERS = new object[2];
			}

			PREFAB_UTILITY_CREATE_VARIANT_PARAAMETERS[0] = prefabAsset;
			PREFAB_UTILITY_CREATE_VARIANT_PARAAMETERS[1] = outputAssetPath;
			PREFAB_UTILITY_CREATE_VARIANT_METHOD_INFO.Invoke(null, PREFAB_UTILITY_CREATE_VARIANT_PARAAMETERS);
		}

		public static string ConvertPrefabPropertyModificationToString(GameObject prefab, PropertyModification propertyModification)
		{
			StringBuilder stringBuilder = new StringBuilder();

			stringBuilder.Append(ObjectUtility.CalculateTransformIndexPathStringReverseOrder(ObjectUtility.FindTransform(propertyModification.target), prefab.transform))
				.Append(propertyModification.propertyPath)
				.Append("_")
				.Append(propertyModification.value);
           
			if (propertyModification.objectReference != null)
			{
				stringBuilder.Append("_");
				Object reference = propertyModification.objectReference;
				int instanceId = reference.GetInstanceID();
				string assetPath = AssetDatabase.GetAssetPath(reference);
				if (string.IsNullOrEmpty(assetPath))
				{

				}
				else
				{
					stringBuilder.Append(instanceId);
                    if (AssetDatabase.IsForeignAsset(reference)
						&& !AssetDatabase.IsMainAsset(reference))
					{
                        Transform referenceTrans = ObjectUtility.FindTransform(reference);
                        if (referenceTrans == null)
                        {
                            referenceTrans = ObjectUtility.FindTransform(propertyModification.target);
                        }
                        stringBuilder.Append("_")
							.Append(ObjectUtility.CalculateTransformIndexPathStringReverseOrder(referenceTrans));
					}
				}
			}


			return stringBuilder.ToString();
		}
	}
}
#endif