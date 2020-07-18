#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace Map
{
    [ExecuteInEditMode]
	public class MapEntity<T> : MonoBehaviour where T : MapEntityRoot
	{
		/// <summary>
		/// Npc根节点
		/// </summary>
		public T m_Root;

		/// <summary>
		/// 是否显示模型
		/// </summary>
        [HideInInspector]
		public bool m_ShowModel = true;
        /// <summary>
        /// 模型名称
        /// </summary>
        [System.NonSerialized]
        public string m_ModelPath;
		private float lastRefreshPosTime;
		/// <summary>
		/// 删除
		/// </summary>
		public virtual void DestroySelf()
		{
			DestroyImmediate(gameObject);
		}
        

		protected virtual string GetModelPath()
		{
			return "";
		}

		protected void RefreshModel()
		{
			if (m_ShowModel)
			{
				ShowModel();
			}
			else
			{
				HideModel();
			}
		}

		public void RefreshPosition(bool force = false)
		{
			if ((Time.realtimeSinceStartup - lastRefreshPosTime >= 10) || force)
			{
				if (transform.childCount > 0)
				{
					transform.localPosition = transform.localPosition + transform.GetChild(0).localPosition;
					transform.GetChild(0).localPosition = Vector3.zero;
				}
				lastRefreshPosTime = Time.realtimeSinceStartup;
			}

            if (Time.frameCount % 10 == 0)
            {
                if(transform.childCount > 0)
                {
                    Transform childTrans = transform.GetChild(0);
                    if(childTrans != null)
                    {
                        Collider collider = childTrans.gameObject.GetComponent<Collider>();
                        RefreshColliderState(collider, childTrans);
                    }
                }
            }
		}

        private void RefreshColliderState(Collider collider,Transform childTrans)
        {
            if(collider == null)
            {
                return;
            }
            collider.enabled = false;
            if (collider is BoxCollider)
            {
                BoxCollider boxCollider = collider as BoxCollider;
                Collider[] colliders = Physics.OverlapBox(childTrans.position + boxCollider.center, boxCollider.size);
                if (colliders != null && colliders.Length > 0)
                {
                    for (int iCollider = 0; iCollider < colliders.Length; iCollider++)
                    {
                        if (!CanOverlap(colliders[iCollider]))
                        {
                            continue;
                        }
                    }
                }
            }
            else if (collider is CapsuleCollider)
            {
                CapsuleCollider capsualCollider = collider as CapsuleCollider;
                Vector3 starPoint = childTrans.position + capsualCollider.center + Vector3.up * (-capsualCollider.radius * 0.5f);
                Vector3 endPoint = childTrans.position + capsualCollider.center + Vector3.up * (capsualCollider.radius * 0.5f);
                Collider[] colliders =  Physics.OverlapCapsule(starPoint, endPoint, capsualCollider.radius);
                if(colliders != null && colliders.Length>0)
                {
                    for(int iCollider = 0;iCollider<colliders.Length;iCollider++)
                    {
                        if (!CanOverlap(colliders[iCollider]))
                        {
                            continue;
                        }
                    }
                }
            }
            else if (collider is SphereCollider)
            {
                SphereCollider sphereCollider = collider as SphereCollider;
                Collider[] colliders = Physics.OverlapSphere(childTrans.position + sphereCollider.center, sphereCollider.radius);
                if (colliders != null && colliders.Length > 0)
                {
                    for (int iCollider = 0; iCollider < colliders.Length; iCollider++)
                    {
                        if (!CanOverlap(colliders[iCollider]))
                        {
                            continue;
                        }
                    }
                }
            }
        }

        private bool CanOverlap(Collider collider)
        {
            if (collider == null)
            {
                return false;
            }
            PostProcessVolume volume = collider.GetComponent<PostProcessVolume>();
            if (volume != null)
            {
                return false;
            }
            if (m_Root != null && m_Root.m_GamingMapArea != null &&
                m_Root.m_GamingMapArea.m_GamingMap != null && collider.gameObject.scene != m_Root.m_GamingMapArea.m_GamingMap.GetOwnerScene())
            {
                return false;
            }
            Debug.LogError(string.Format("{0}与{1}相交", gameObject.name, collider.gameObject.name));
            return true;
        }


        /// <summary>
        /// 显示模型
        /// </summary>
        public void ShowModel()
		{
			if (string.IsNullOrEmpty(m_ModelPath))
			{
				if (transform.childCount > 0)
				{
					for (int iChild = transform.childCount - 1; iChild >= 0; iChild--)
					{
						DestroyImmediate(transform.GetChild(iChild).gameObject);
					}
				}
			}
           
            if (transform.childCount > 0 )
			{
				for (int iChild = 0; iChild < transform.childCount; iChild++)
				{
					transform.GetChild(iChild).gameObject.SetActive(true);
				}
				return;
			}

            string modelPath = GetModelPath();
            GamingMap map = m_Root.GetGamingMap();
			if (!string.IsNullOrEmpty(modelPath))
			{
                GameObject teleportTemplete = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
				if (teleportTemplete != null)
                {
                    GameObject locationObj = GameObject.Instantiate(teleportTemplete);
                    Collider[] colliders = locationObj.GetComponents<Collider>();
                    if ((colliders==null|| colliders.Length<=0)&&m_Root.GetGamingMapType() != GamingMapType.mapMainCity)
                    {
                        CapsuleCollider collider = EditorGamingMapData.CalculateCapsuleCollider(locationObj);
                        if(collider != null)
                        {
                            GamingMapArea area = m_Root.m_GamingMapArea;
                            if(area != null)
                            {
                                area.SetMaxWarShipHeight(collider.height);
                            }
                        }
                    }
                    locationObj.transform.SetParent(transform);
                    locationObj.transform.localPosition = Vector3.zero;
                    locationObj.transform.localRotation = Quaternion.identity;
                    locationObj.transform.localScale = Vector3.one;
                }
			}
		}

		/// <summary>
		/// 删除模型
		/// </summary>
		protected void HideModel()
		{
			int childCount = transform.childCount;
			if (childCount > 0)
			{
				for (int iChild = 0; iChild < childCount; iChild++)
				{
					transform.GetChild(iChild).gameObject.SetActive(false);
				}
			}
		}

        protected Vector3 GetPositon(EditorPosition position)
        {
            if(position == null)
            {
                return Vector3.zero;
            }

            return new Vector3(position.x,position.y,position.z);
        }

        protected Quaternion GetRotation(EditorRotation rotation)
        {
            if(rotation == null)
            {
                return Quaternion.identity;
            }
            return new Quaternion(rotation.x, rotation.y, rotation.z, rotation.w);
        }
        

		public virtual EditorPosition GetEditorPosition()
		{
			return null;
		}

		public virtual EditorRotation GetEditorRotation()
		{
			return null;
		}

        /// <summary>
        /// 是否显示落地
        /// </summary>
        /// <returns></returns>
        public bool IsShowLand()
        {
            if (m_Root == null)
            {
                return false;
            }
            GamingMapType mapType = m_Root.GetGamingMapType();
            return mapType == GamingMapType.mapMainCity;
        }

        /// <summary>
        /// 强制落地
        /// </summary>
        public void ForceLand()
        {
            UnityEngine.AI.NavMeshHit hit;
            if (UnityEngine.AI.NavMesh.SamplePosition(transform.position, out hit, 100, UnityEngine.AI.NavMesh.AllAreas))
            {
                transform.position = hit.position;
            }
        }
    }
}
#endif
