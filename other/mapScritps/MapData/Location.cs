#if UNITY_EDITOR
using EditorExtend;
using Map;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Map
{
	[ExecuteInEditMode]
	public class Location : MapEntity<LocationRoot>
	{
		#region 公开属性
		/// <summary>
		/// 点的唯一id
		/// </summary>
		public ulong m_Uid;

		/// <summary>
		/// 自增的id
		/// </summary>
		public int m_LocationId;
		/// <summary>
		/// 点名称
		/// </summary>
		public string m_LocationName;
		/// <summary>
		/// 点类型
		/// </summary>
		public LocationType m_LocationType;
		#endregion

		#region 公开方法
		public IEnumerator OnUpdate(LocationRoot root, bool showModel)
		{
			m_Root = root;
			m_ShowModel = showModel;
			RefreshModel();
			yield return null;
			RefreshPosition();
			yield return null;
            gameObject.name = string.Format("{0}_{1}", m_LocationName,m_Uid);
            yield return null;
        }

		public void Init(ulong uid, int locationId, LocationRoot root)
		{
			m_Root = root;
			m_Uid = uid;
			m_LocationId = locationId;
            gameObject.name = string.Format("{0}_{1}", m_LocationName, m_Uid);
            transform.localPosition = Vector3.zero;
        }

		public void Init(EditorLocation location, LocationRoot root)
		{
			m_Root = root;
			m_Uid = location.locationId;
			m_LocationName = location.locationName;
			m_LocationType = (LocationType)location.locationType;
			transform.position = GetPositon(location.position);
			transform.rotation = GetRotation(location.rotation);
            string strUid = m_Uid.ToString();
            if (strUid.Length > 3)
			{
				string locationIdStr = strUid.Substring(strUid.Length - 3, 3);
				if (!string.IsNullOrEmpty(locationIdStr))
				{
					m_LocationId = int.Parse(locationIdStr);
				}
			}

            gameObject.name = string.Format("{0}_{1}", m_LocationName, m_Uid);
        }
        
        #endregion

        #region 基类方法
        protected override string GetModelPath()
		{
            if(!string.IsNullOrEmpty(m_ModelPath))
            {
                return m_ModelPath;
            }
            string assetName = "";
            if(m_Root.GetGamingMapType() == GamingMapType.mapMainCity)
            {
                assetName = "Human_F_01_skin";
            }
            else
            {
                // assetName = "BUSF_E_Multirole_final";
                assetName = GamingMapEditorUtility.GetMaxShipPath();
            }

            if(string.IsNullOrEmpty(assetName))
            {
                m_ModelPath = GamingMapEditorUtility.GetLocationTempletePath();
            }
            else
            {
                if (!assetName.Contains("Assets"))
                {
                    string[] resAssets = AssetDatabase.FindAssets(string.Format("{0} t:Prefab", assetName));
                    if (resAssets != null && resAssets.Length > 0)
                    {
                        for (int iRes = 0; iRes < resAssets.Length; iRes++)
                        {
                            string path = AssetDatabase.GUIDToAssetPath(resAssets[iRes]);
                            string[] resSplit = path.Split('/');
                            if (resSplit != null && resSplit.Length > 0)
                            {
                                string lastName = resSplit[resSplit.Length - 1];
                                if (lastName.Equals(string.Format("{0}.prefab", assetName)))
                                {
                                    m_ModelPath = path;
                                    return assetName;
                                }
                            }
                        }

                    }

                }
                else
                {
                    m_ModelPath = assetName;
                }
            }
           
            return m_ModelPath;
		}

		public override EditorPosition GetEditorPosition()
		{
			return new EditorPosition(transform.position);
		}

		public override EditorRotation GetEditorRotation()
		{
			return new EditorRotation(transform.rotation);
		}
		#endregion
	}
}
#endif