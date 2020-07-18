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
	public class TeleportRoot : MapEntityRoot
	{
		#region 私有属性
		/// <summary>
		/// json数据
		/// </summary>
		private EditorTeleport[] m_EditorTeleport;

		/// <summary>
		/// 传送门显示的名称
		/// </summary>
		private string[] m_TeleportShowNames;
		/// <summary>
		/// 创建传送门携程
		/// </summary>
		private IEnumerator m_CreateTeleportEnumerator;
		#endregion

		#region 公开属性
		/// <summary>
		/// 当前包括的配置数据
		/// </summary>
		public List<TeleportVO> m_TeleportList;
		/// <summary>
		/// key为chanel id
		/// </summary>
		//public Dictionary<int, int> m_SelectLocation;

        public List<TeleportInfo> m_SelectLocation;
        [System.Serializable]
        public class TeleportInfo
        {
            /// <summary>
            /// 选中的频道Id
            /// </summary>
            public int m_ChanelId;
            /// <summary>
            /// 选中的序列id
            /// </summary>
            public int m_SelectIndex;

            public TeleportInfo(int channelId,int selectIndex)
            {
                m_ChanelId = channelId;
                m_SelectIndex = selectIndex;
            }
        }
        #endregion

        #region 私有方法
        private void OnDisable()
        {
            m_SelectLocation = null;
        }
        #endregion

        #region 公开方法

        /// <summary>
        /// 获取传送点信息
        /// </summary>
        /// <param name="channelId"></param>
        /// <returns></returns>
        public TeleportInfo GetTeleportInfo(int channelId)
        {
            if(m_SelectLocation == null || m_SelectLocation.Count<=0)
            {
                return null;
            }
            for(int iSelect = 0;iSelect<m_SelectLocation.Count;iSelect++)
            {
                if(m_SelectLocation[iSelect].m_ChanelId == channelId)
                {
                    return m_SelectLocation[iSelect];
                }
            }
            return null;
        }

		public void Init(EditorTeleport[] teleports, GamingMapArea area)
		{
			m_GamingMapArea = area;
			m_EditorTeleport = teleports;
			GetTeleportList();
			GetTeleportShowNames();

            //m_SelectLocation = new Dictionary<int, int>();
            m_SelectLocation = new List<TeleportInfo>();

            if (m_EditorTeleport != null && m_EditorTeleport.Length > 0)
			{
				for (int iTeleport = 0;  m_EditorTeleport != null && iTeleport < m_EditorTeleport.Length; iTeleport++)
				{
					EditorChanel[] chanelList = m_EditorTeleport[iTeleport].chanelList;
					if (chanelList != null && chanelList.Length > 0)
					{
						for (int iChanel = 0; iChanel < chanelList.Length; iChanel++)
						{
							EditorChanel chanel = chanelList[iChanel];
							TeleportChanelVO chanelVO = ConfigVO<TeleportChanelVO>.Instance.GetData(chanel.chanelId);
							if (chanelVO != null)
							{
								EditorLocation[] locations = GamingMap.GetEditorLocations(chanelVO.EndGamingMap, chanelVO.EndGamingMapArea);
								if (locations != null && locations.Length > 0)
								{

                                    List<EditorLocation> locationList = new List<EditorLocation>();
                                    for (int iLocation = 0; iLocation < locations.Length; iLocation++)
                                    {
                                        EditorLocation location = locations[iLocation];
                                        if (location.locationType == (int)LocationType.TeleportIn)
                                        {
                                            locationList.Add(location);
                                        }
                                    }

                                    for (int iLocation = 0; iLocation < locationList.Count; iLocation++)
                                    {
                                        if (locationList[iLocation].locationId == chanel.eLocation)
                                        {
                                            m_SelectLocation.Add(new TeleportInfo(chanel.chanelId, iLocation));
                                            break;
                                        }
                                    }
                                }

							}
						}
					}
				}
			}
		}

		public List<TeleportVO> GetTeleportList()
		{
			if (m_GamingMapArea == null)
			{
				return null;
			}
			m_TeleportList = m_GamingMapArea.GetTeleportList();
			return m_TeleportList;
		}

		public string[] GetTeleportShowNames()
		{
			if (m_TeleportShowNames == null || m_TeleportShowNames.Length <= 0)
			{
				if (m_TeleportList != null && m_TeleportList.Count > 0)
				{
					m_TeleportShowNames = new string[m_TeleportList.Count];
					for (int iTeleport = 0; iTeleport < m_TeleportList.Count; iTeleport++)
					{
						m_TeleportShowNames[iTeleport] = m_TeleportList[iTeleport].ID.ToString();
					}
				}
			}
			return m_TeleportShowNames;
		}
		public IEnumerator OnUpdate(GamingMapArea mapArea)
		{
			m_GamingMapArea = mapArea;
            GetTeleportList();
            yield return null;
            if (m_CreateTeleportEnumerator != null)
			{
				while (m_CreateTeleportEnumerator.MoveNext())
				{
					yield return null;
				}
				m_CreateTeleportEnumerator = null;
				m_EditorTeleport = null;
			}
			yield return null;
		}

		#endregion

		#region 基类方法
		public override void BeginExport()
		{
			base.BeginExport();
		}


		public override void Clear(bool needDestroy = true)
		{
			base.Clear(needDestroy);
		}
		#endregion



	}
}
#endif