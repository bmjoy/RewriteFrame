#if UNITY_EDITOR
using EditorExtend;
using Leyoutech.Utility;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Map
{
	/// <summary>
	/// SceneObjectUnit的限制：
	///		必须是一个Prefab
	///		必须是Addressable的Asset
	/// </summary>
	public class SceneUnit : MonoBehaviour
	{
		/// <summary>
		/// 这个Unit所属的Map
		/// </summary>
		protected Map m_OwnerMap;
		/// <summary>
		/// 这个Unit所属的Area
		/// </summary>
		protected Area m_OwnerArae;
		/// <summary>
		/// 物体世界空间的AABB，用于计算这个Unit需要在哪些Voxel中显示
		/// </summary>
		[ReadOnly]
		public Bounds m_AABB;
		/// <summary>
		/// 这个Unit引用的Prefab
		/// </summary>
		protected GameObject m_Prefab;
		/// <summary>
		/// 是否合法
		/// </summary>
		protected bool m_IsValid;
		/// <summary>
		/// 是否有AABB
		/// </summary>
		protected bool m_HasAABB;
		/// <summary>
		/// <see cref="m_Prefab"/>的Override
		/// 值等于<see cref="PrefabUtility.GetPropertyModifications"/>的结果转化为字符串
		/// 如果这个Prefab没有Override，则这个值等于<see cref="String.Empty"/>
		/// 
		/// 用于判断：
		///		通过一个Prefab创建出来的多个Prefab的实例，是否有相同的修改
		///		如果多个Prefab实例的修改相同，则导出时只导出一个Prefab Variant
		/// </summary>
		protected string m_PrefabOverrideModificationMd5;

		public GameObject GetPrefab()
		{
			return m_Prefab;
		}

		public string GetPrefabOverrideModificationMd5()
		{
            if(string.IsNullOrEmpty(m_PrefabOverrideModificationMd5))
            {
                //CaculatePrefabOverrideModification();
            }
			return m_PrefabOverrideModificationMd5;
		}
        

		public bool HasAABB()
		{
			return m_HasAABB;
		}

		public Bounds GetAABB()
		{
			return m_AABB;
		}

		public bool IsValid()
		{
			return m_IsValid;
		}

		public void FillUnitInfo(ref SceneUnitInfo unitInfo, Area area, Quaternion areaInverseRotation)
		{
			unitInfo.LocalPosition = area.transform.InverseTransformPoint(transform.position);
			unitInfo.LocalRotation = areaInverseRotation * transform.rotation;
			unitInfo.LocalScale = ObjectUtility.CalculateLossyScale(transform, area.transform);

			unitInfo._AABB = m_AABB;
			unitInfo._Diameter = MathUtility.CaculateLongestSide(m_AABB);
		}

		public virtual void DoUpdate(Map map, Area area, bool isExporting)
		{
			SceneUnit[] units = gameObject.GetComponents<SceneUnit>();
			if (units.Length > 1)
			{
				for (int iUnit = 0; iUnit < units.Length; iUnit++)
				{
					if (units[iUnit] != this)
					{
						DestroyImmediate(units[iUnit]);
					}
				}
			}

			m_OwnerMap = map;
			m_OwnerArae = area;
			m_Prefab = UnityEditor.PrefabUtility.GetCorrespondingObjectFromSource(gameObject);
			m_HasAABB = CaculateAABB(isExporting, ref m_AABB);
			m_IsValid = CheckIsValidUnit(isExporting);

			// 计算Prefab的override性能挺耗的，所以只在导出时计算
			if (m_IsValid
				&& isExporting)
			{
				//CaculatePrefabOverrideModification();
			}
		}

		public virtual bool CheckIsValidUnit(bool isExporting)
		{
			if (!m_HasAABB
				&& isExporting)
			{
				//Debug.LogError(string.Format("Unit({0})不是一个合法的SceneObjectUnit\n因为计算不出它的AABB，可能是因为这个Unit下只有粒子特效"
				//	, ObjectUtility.CalculateTransformPath(transform))
				//	, gameObject);
				return false;
			}

			if (m_Prefab == null
				|| m_Prefab.transform.parent != null)
			{
				//Debug.LogError(string.Format("Unit({0})不是一个合法的SceneObjectUnit\nSceneObjectUnit必须是一个Prefab的根节点"
				//	, ObjectUtility.CalculateTransformPath(transform))
				//	, gameObject);
				return false;
			}
			else if (HasAddedGameObjectsOrComponents())
			{
				//Debug.LogError(string.Format("Unit({0})不是一个合法的SceneObjectUnit\n这个节点下面有新加的GameObject或Component，但是没有Apply Prefab"
				//	, ObjectUtility.CalculateTransformPath(transform))
				//	, gameObject);
				return false;
			}
			else
			{
				return true;
			}
		}

		/// <summary>
		/// 导出Prefab时触发
		/// </summary>
		public virtual void OnExportPrefab()
		{
			SceneUnit[] units = transform.GetComponentsInChildren<SceneUnit>();
			for (int iUnit = 0; iUnit < units.Length; iUnit++)
			{
				SceneUnit iterSceneUnit = units[iUnit];
				if (iterSceneUnit != this)
				{
					DestroyImmediate(units[iUnit]);
				}
			}
		}

		protected void OnEnable()
		{
			// 这个脚本只在编辑器下使用
			if (Application.isPlaying)
			{
				Destroy(this);
				return;
			}
		}

		/// <summary>
		/// 计算这个Unit的AABB
		/// </summary>
		/// <returns>false:无法计算出这个Unit的AABB</returns>
		protected virtual bool CaculateAABB(bool isExporting, ref Bounds aabb)
		{
			Renderer[] renderers = transform.GetComponentsInChildren<Renderer>();
			// 用来防止Encapsulate(new Bounds())
			bool initializedAABB = false;
			for (int iRenderer = 0; iRenderer < renderers.Length; iRenderer++)
			{
				Renderer iterRenderer = renderers[iRenderer];
				if (iterRenderer is ParticleSystemRenderer)
				{
					continue;
				}

				if (initializedAABB)
				{
					aabb.Encapsulate(iterRenderer.bounds);
				}
				else
				{
					initializedAABB = true;
					aabb = iterRenderer.bounds;
				}
			}

			ParticleSystem[] particleSystems = transform.GetComponentsInChildren<ParticleSystem>();
			for (int iParticle = 0; iParticle < particleSystems.Length; iParticle++)
			{
				ParticleSystem iterParticle = particleSystems[iParticle];
				Bounds particleAABB;
				// 只有导出时才计算粒子的AABB，否则只计算粒子的坐标
				if (isExporting)
				{
					ParticleSystemRenderer iterRenderer = iterParticle.GetComponent<ParticleSystemRenderer>();
					if (iterRenderer)
					{
						particleAABB = iterRenderer.bounds;

					}
					else
					{
						particleAABB = new Bounds(iterParticle.transform.position, Vector3.zero);
					}
				}
				else
				{
					particleAABB = new Bounds(iterParticle.transform.position, Vector3.zero);
				}

				if (initializedAABB)
				{
					aabb.Encapsulate(particleAABB);
				}
				else
				{
					initializedAABB = true;
					aabb = particleAABB;
				}

			}

			return initializedAABB;
		}

		/// <summary>
		/// 这个Prefab的Modification是否是Override
		/// </summary>
		protected virtual bool IsPrefabOverrideModification(PropertyModification propertyModification)
		{
			bool isOverrideModification;
			if (propertyModification.target == m_Prefab.GetComponent<SceneUnit>())
			{
				isOverrideModification = false;
			}
			else
			{
				isOverrideModification = true;
			}
			return isOverrideModification;
		}

		/// <summary>
		/// <see cref="m_PrefabOverrideModificationMd5"/>
		/// </summary>
		private void CaculatePrefabOverrideModification()
		{
			if (!UnityEditor.PrefabUtility.HasPrefabInstanceAnyOverrides(gameObject, false))
			{
				m_PrefabOverrideModificationMd5 = string.Empty;
			}
			else
			{
                List<string> modificationStrings = null;
                if(m_OwnerMap != null)
                {
                    modificationStrings = m_OwnerMap.GetStringsCache();
                }
                else
                {
                    modificationStrings = new List<string>();
                }

                if(m_Prefab == null)
                {
                    m_Prefab = UnityEditor.PrefabUtility.GetCorrespondingObjectFromSource(gameObject);
                }

                PropertyModification[] modifications = UnityEditor.PrefabUtility.GetPropertyModifications(gameObject);
				for (int iModification = 0; iModification < modifications.Length; iModification++)
				{
					PropertyModification iterModification = modifications[iModification];
					bool isOverrideModification;
					if (iterModification.target == m_Prefab)
					{
						isOverrideModification = !Constants.PREFAB_ROOT_GAMEOBJECT_IGNORE_MODIFICATION_PROPERTY_PATHS.Contains(iterModification.propertyPath);
					}
					else if (iterModification.target == m_Prefab.transform)
					{
						isOverrideModification = !Constants.PREFAB_ROOT_TRANSFORM_IGNORE_MODIFICATION_PROPERTY_PATHS.Contains(iterModification.propertyPath);
					}
                    else if(CheckChildOverride(iterModification))
                    {
                        isOverrideModification = false;
                    }
					else
					{
						isOverrideModification = IsPrefabOverrideModification(iterModification);
					}

					if (isOverrideModification)
					{
                        Debug.Log("sceneunit:"+gameObject.name);
						modificationStrings.Add(Leyoutech.Utility.PrefabUtility.ConvertPrefabPropertyModificationToString(m_Prefab, iterModification));
					}
				}

				if (modificationStrings.Count > 0)
				{
					StringBuilder stringBuilder = StringUtility.AllocStringBuilderCache();
					modificationStrings.Sort();
					for (int iModification = 0; iModification < modificationStrings.Count; iModification++)
					{
						stringBuilder.Append(modificationStrings[iModification]);
					}
					modificationStrings.Clear();
					m_PrefabOverrideModificationMd5 = StringUtility.CalculateMD5Hash(StringUtility.ReleaseStringBuilderCacheAndReturnString());
				}
				else
				{
					m_PrefabOverrideModificationMd5 = string.Empty;
				}
			}
		}

        /// <summary>
        /// 检查子物体是否能override
        /// </summary>
        /// <returns></returns>
        private bool CheckChildOverride(PropertyModification modification)
        {
            if (modification == null)
            {
                return false;
            }

            if(modification.target is Transform)
            {
                Transform targetTrans = modification.target as Transform;
                Transform targetParentTrans = targetTrans.parent;
                if(targetParentTrans != null && targetParentTrans.name.Equals("Collider"))//临时解决方案
                {
                    return true;
                }
                //if (Constants.PREFAB_CHILD_TRANSFORM_IGNORE_MODIFICATION_PROPERTY_PATHS.Contains(modification.propertyPath))
                //{
                //    float originValue=0;
                //    if(modification.propertyPath.Equals("m_LocalPosition.x"))
                //    {
                //        originValue = targetTrans.localPosition.x;
                //    }
                //    else if(modification.propertyPath.Equals("m_LocalPosition.y"))
                //    {
                //        originValue = targetTrans.localPosition.y;
                //    }
                //    else if(modification.propertyPath.Equals("m_LocalPosition.z"))
                //    {
                //        originValue = targetTrans.localPosition.z;
                //    }

                //    string originStr = originValue.ToString();
                //    int degree = 0;
                //    if(originValue != 0)
                //    {
                //        degree = originStr.Length - originStr.IndexOf(".") - 1;
                //    }

                //    string valueStr = modification.value;
                //    if(!string.IsNullOrEmpty(valueStr))
                //    {
                //        float value = float.Parse(valueStr);
                //        valueStr = value.ToString(string.Format("f{0}",degree));
                //    }
                //    if(valueStr.Equals(originStr))
                //    {
                //        return true;
                //    }
                //}
            }
            return false;
        }

		protected bool HasAddedGameObjectsOrComponents()
		{
			List<AddedGameObject> gameObjects = UnityEditor.PrefabUtility.GetAddedGameObjects(gameObject);
			for (int iGameObject = 0; iGameObject < gameObjects.Count;iGameObject++)
			{
				if ((gameObjects[iGameObject].instanceGameObject.hideFlags & HideFlags.DontSave) == 0)
				{
					return true;
				}
			}
			List<AddedComponent> components = UnityEditor.PrefabUtility.GetAddedComponents(gameObject);
			for (int iComponent = 0; iComponent < components.Count; iComponent++)
			{
				if ((components[iComponent].instanceComponent.hideFlags & HideFlags.DontSave) == 0)
				{
                    if(!(components[iComponent].instanceComponent is LightmapSetting))
                    {
                        return true;
                    }
					
				}
			}

			return false;
		}
	}
}
#endif