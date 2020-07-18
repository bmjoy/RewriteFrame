using System.Collections;
using System.Collections.Generic;
using System.Text;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Rendering;

namespace Leyoutech.Utility
{
	public static class RendererUtility
	{
		/// <summary>
		/// 计算一个物体距离相机多远时，会在屏幕上占relativeSizeInScreen
		/// </summary>
		/// <returns></returns>
		public static float CacluateToCameraDistance(float diameter, float relativeHeightInScreen, float halfTanFov)
		{
			return diameter / relativeHeightInScreen * 0.5f / halfTanFov;
		}

		/// <summary>
		/// 计算一个物体占屏幕的比例
		/// </summary>
		public static float CaculateRelativeHeightInScreen(float diameter, float toCameraDistance, float halfTanFov)
		{
			return diameter / (halfTanFov * toCameraDistance * 2.0f);
		}

		/// <summary>
		/// <see cref="CaculateRelativeHeightInScreen(float, float, float)"/>
		/// </summary>
		public static float CaculateRelativeHeightInScreen(float diameter, float toCameraDistance, Camera camera)
		{
			float halfTanFov = CaculateHalfTanCameraFov(camera.fieldOfView);
			return CaculateRelativeHeightInScreen(diameter, toCameraDistance, halfTanFov);
		}

		/// <summary>
		/// 计算tan(fov * 0.5f)
		/// </summary>
		public static float CaculateHalfTanCameraFov(float fov)
		{
			return Mathf.Tan(fov * 0.5f * Mathf.Deg2Rad);
		}

		/// <summary>
		/// 生成RendererInfo
		/// </summary>
		public static string GenerateRendererInfo(Renderer renderer)
		{
			StringBuilder stringBuilder = new StringBuilder();
			GenerateRendererInfo(stringBuilder, renderer);
			return stringBuilder.ToString();
		}

		/// <summary>
		/// 生成RendererInfo
		/// </summary>
		public static void GenerateRendererInfo(StringBuilder stringBuilder, Renderer renderer)
		{
			stringBuilder.Append("Renderer").Append('\n')
				.Append("name: ").Append(renderer.name).Append('\n')
				.Append("InstanceID: ").Append(renderer.GetInstanceID()).Append('\n')
				.Append("renderingLayerMask: ").Append(renderer.renderingLayerMask).Append('\n')
				.Append("rendererPriority: ").Append(renderer.rendererPriority).Append('\n')
				.Append("sortingLayerName: ").Append(renderer.sortingLayerName).Append('\n')
				.Append("sortingLayerID: ").Append(renderer.sortingLayerID).Append('\n')
				.Append("sortingOrder: ").Append(renderer.sortingOrder).Append('\n')
				.Append("allowOcclusionWhenDynamic: ").Append(renderer.allowOcclusionWhenDynamic).Append('\n')
				.Append("isPartOfStaticBatch: ").Append(renderer.isPartOfStaticBatch).Append('\n')
				.Append("lightmapIndex: ").Append(renderer.lightmapIndex).Append('\n')
				.Append("realtimeLightmapIndex: ").Append(renderer.realtimeLightmapIndex).Append('\n')
				.Append("lightmapScaleOffset: ").Append(renderer.lightmapScaleOffset).Append('\n')
				.Append("realtimeLightmapScaleOffset: ").Append(renderer.realtimeLightmapScaleOffset).Append('\n')
				.Append("reflectionProbeUsage: ").Append(renderer.reflectionProbeUsage).Append('\n')
				.Append("lightProbeUsage: ").Append(renderer.lightProbeUsage).Append('\n')
				.Append("receiveShadows: ").Append(renderer.receiveShadows).Append('\n')
				.Append("motionVectorGenerationMode: ").Append(renderer.motionVectorGenerationMode).Append('\n')
				.Append("bounds: ").Append(renderer.bounds).Append('\n')
				.Append("enabled: ").Append(renderer.enabled).Append('\n')
				.Append("isVisible: ").Append(renderer.isVisible).Append('\n');

			stringBuilder.Append("\n\n");
			Material[] materials = renderer.materials;
			stringBuilder.Append("Materials ").Append(materials.Length).Append('\n');
			for (int iMaterial = 0; iMaterial < materials.Length; iMaterial++)
			{
				GenerateMaterialInfo(stringBuilder, materials[iMaterial]);
				stringBuilder.Append('\n');
			}
		}

		/// <summary>
		/// 生成MaterialInfo
		/// </summary>
		public static string GenerateMaterialInfo(Material material)
		{
			StringBuilder stringBuilder = new StringBuilder();
			GenerateMaterialInfo(stringBuilder, material);
			return stringBuilder.ToString();
		}

		/// <summary>
		/// 生成MaterialInfo
		/// </summary>
		public static void GenerateMaterialInfo(StringBuilder stringBuilder, Material material)
		{
			string enabledPassName = string.Empty;
			string disabledPassName = string.Empty;
			for (int iPass = 0; iPass < material.passCount; iPass++)
			{
				string iterPassName = material.GetPassName(iPass);
				if (material.GetShaderPassEnabled(iterPassName))
				{
					enabledPassName += $"{iterPassName}; ";
				}
				else
				{
					disabledPassName += $"{iterPassName}; ";
				}
			}

			stringBuilder.Append("Material").Append('\n')
				.Append("name: ").Append(material.name).Append('\n')
				.Append("InstanceID: ").Append(material.GetInstanceID()).Append('\n')
				.Append("enableInstancing: ").Append(material.enableInstancing).Append('\n')
				.Append("doubleSidedGI: ").Append(material.doubleSidedGI).Append('\n')
				.Append("globalIlluminationFlags: ").Append(material.globalIlluminationFlags).Append('\n')
				.Append("renderQueue: ").Append(material.renderQueue).Append('\n')
				.Append("mainTextureScale: ").Append(material.mainTextureScale).Append('\n')
				.Append("mainTextureOffset: ").Append(material.mainTextureOffset).Append('\n')
				.Append("mainTexture Name: ").Append(material.mainTexture == null ? "<Notset>" : material.mainTexture.name).Append('\n')
				.Append("color: ").Append(material.color).Append('\n')
				.Append("passCount: ").Append(material.passCount).Append('\n')
				.Append("enabledPassName: ").Append(enabledPassName).Append('\n')
				.Append("disabledPassName: ").Append(disabledPassName).Append('\n')
				.Append("materialKeyword: ").Append(string.Join("; ", material.shaderKeywords)).Append('\n');

			GenerateShaderInfo(stringBuilder, material, material.shader);
		}

		/// <summary>
		/// 生成Shader信息
		/// </summary>
		public static void GenerateShaderInfo(StringBuilder stringBuilder, Shader shader)
		{
			GenerateShaderInfo(stringBuilder, null, shader);
		}

		/// <summary>
		/// 生成Shader信息
		/// </summary>
		public static void GenerateShaderInfo(StringBuilder stringBuilder, Material material, Shader shader)
		{
#if UNITY_EDITOR
			string enabledShaderGlobalKeywordStr = string.Empty;
			string[] shaderGlobalKeywords = UnityEditorReflectionUtility.ShaderUtil.GetShaderGlobalKeywords(shader);
			for (int iKeyword = 0; iKeyword < shaderGlobalKeywords.Length; iKeyword++)
			{
				if (Shader.IsKeywordEnabled(shaderGlobalKeywords[iKeyword])
					|| (material != null
						&& material.IsKeywordEnabled(shaderGlobalKeywords[iKeyword])))
				{
					enabledShaderGlobalKeywordStr = $"{enabledShaderGlobalKeywordStr}{shaderGlobalKeywords[iKeyword]}; ";
				}
			}
			string enabledShaderLocalKeywordStr = string.Empty;
			string[] shaderLocalKeywords = UnityEditorReflectionUtility.ShaderUtil.GetShaderLocalKeywords(shader);
			for (int iKeyword = 0; iKeyword < shaderLocalKeywords.Length; iKeyword++)
			{
				if (Shader.IsKeywordEnabled(shaderLocalKeywords[iKeyword])
					|| (material != null
						&& material.IsKeywordEnabled(shaderLocalKeywords[iKeyword])))
				{
					enabledShaderLocalKeywordStr = $"{enabledShaderLocalKeywordStr}{shaderLocalKeywords[iKeyword]}; ";
				}
			}
#endif

			stringBuilder.Append("Shader").Append('\n')
				.Append("name: ").Append(shader.name).Append('\n')
				.Append("InstanceID: ").Append(shader.GetInstanceID()).Append('\n')
				.Append("renderQueue: ").Append(shader.renderQueue).Append('\n')
#if UNITY_EDITOR
				.Append("globalKeywords: ").Append(shaderGlobalKeywords.Length).Append("| ").Append(string.Join("; ", shaderGlobalKeywords)).Append('\n')
				.Append("enabledGlobalKeywords: ").Append(enabledShaderGlobalKeywordStr.Length).Append("| ").Append(enabledShaderGlobalKeywordStr).Append('\n')
				.Append("localKeywords: ").Append(shaderLocalKeywords.Length).Append("| ").Append(string.Join("; ", shaderLocalKeywords)).Append('\n')
				.Append("enabledLocalKeywords: ").Append(enabledShaderLocalKeywordStr.Length).Append("| ").Append(enabledShaderLocalKeywordStr).Append('\n')
#endif
				.Append("maximumLOD: ").Append(shader.maximumLOD).Append('\n')
				.Append("isSupported: ").Append(shader.isSupported).Append('\n')
				.Append("passCount: ").Append(shader.passCount).Append('\n');
		}

		/// <summary>
		/// PassName转换为PassType
		/// </summary>
		public static bool TryConvertPassNameToPassType(string passName, out PassType passType)
		{
			switch (passName.Trim())
			{
				case "FORWARD":
					passType = PassType.ForwardBase;
					return true;
				case "FORWARD_DELTA":
					passType = PassType.ForwardAdd;
					return true;
				case "ShadowCaster":
					passType = PassType.ShadowCaster;
					return true;
				case "DEFERRED":
					passType = PassType.Deferred;
					return true;
				case "META":
					passType = PassType.Meta;
					return true;
			}

			passType = default;
			return false;
		}

#if UNITY_EDITOR
		/// <summary>
		/// 间隔渲染材质球
		/// </summary>
		/// <param name="materialAssetPaths">材质球的路径</param>
		/// <param name="reportPath">报告的路径，如果为空则用默认路径</param>
		public static IEnumerator IntervalRenderMaterials_Co(List<string> materialAssetPaths, string reportPath = null)
		{
			EditorWindow.GetWindow(UnityEditorReflectionUtility.GetGameWindowType()
				, false
				, "Game"
				, true).Focus();

			GameObject materialGameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			Object.DestroyImmediate(materialGameObject.GetComponent<SphereCollider>());
			MeshRenderer meshRenderer = materialGameObject.GetComponent<MeshRenderer>();
			Camera camera = Camera.main;
			bool createdCamera = false;
			if (camera == null)
			{
				createdCamera = true;
				camera = new GameObject().AddComponent<Camera>();
			}

			List<string> handledMaterialAssetPaths = new List<string>();
			for (int iMaterial = 0; iMaterial < materialAssetPaths.Count; iMaterial++)
			{
				if (EditorUtility.DisplayCancelableProgressBar("IntervalRenderMaterials"
						, string.Format("{0}/{1}", iMaterial, materialAssetPaths.Count)
						, (float)iMaterial / materialAssetPaths.Count))
				{
					materialGameObject.transform.position = camera.transform.position + camera.transform.forward * 32.0f;
					break;
				}

				string iterAssetPath = materialAssetPaths[iMaterial];
				Material iterMaterial = AssetDatabase.LoadMainAssetAtPath(iterAssetPath) as Material;
				if (iterMaterial == null)
				{
					continue;
				}

				meshRenderer.sharedMaterial = iterMaterial;
				yield return null;
				yield return null;
				meshRenderer.sharedMaterial = null;
				Resources.UnloadAsset(iterMaterial);
				handledMaterialAssetPaths.Add(iterAssetPath);
			}

			if (string.IsNullOrEmpty(reportPath))
			{
				reportPath = Application.dataPath + "/../../IntervalRenderMaterials.txt";
			}
			System.IO.File.WriteAllText(reportPath, string.Join("\n", handledMaterialAssetPaths));
			DebugUtility.Log(CRenderer.RendererManager.LOG_TAG
				, $"IntervalRenderMaterials: 渲染了{handledMaterialAssetPaths.Count}个材质球，详细见文件({DebugUtility.FormatPathToHyperLink(reportPath)})");

			if (createdCamera)
			{
				Object.DestroyImmediate(camera.gameObject);
			}

			Object.DestroyImmediate(materialGameObject);
			Resources.UnloadUnusedAssets();

			EditorUtility.ClearProgressBar();
		}

		/// <summary>
		/// 清除材质球上不使用的Property
		/// </summary>
		/// <returns>Has changed</returns>
		public static bool CleanUnusedProperties(Material material, StringBuilder record = null)
		{
			SerializedObject so_Material = new SerializedObject(material);
			SerializedProperty sp_Saved = so_Material.FindProperty("m_SavedProperties");
			SerializedProperty sp_Cache = null;
			sp_Saved.Next(true);
			bool changed = false;
			do
			{
				if (!sp_Saved.isArray)
				{
					continue;
				}

				for (int iProerty = sp_Saved.arraySize - 1; iProerty >= 0; --iProerty)
				{
					sp_Cache = sp_Saved.GetArrayElementAtIndex(iProerty);
					if (material.HasProperty(sp_Cache.displayName))
					{
						continue;
					}

					if (record != null)
					{
						record.AppendLine($"Delete property: {sp_Saved.name}/{sp_Cache.displayName}");
					}
					changed = true;
					sp_Saved.DeleteArrayElementAtIndex(iProerty);
				}
			}
			while (sp_Saved.Next(false));

			if (changed)
			{
				so_Material.ApplyModifiedProperties();
			}
			return changed;
		}
#endif
	}
}