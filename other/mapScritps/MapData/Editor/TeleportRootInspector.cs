#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Map
{
	[CustomEditor(typeof(TeleportRoot))]
	public class TeleportRootInspector : Editor
	{
		private TeleportRoot m_Target;
		private bool m_ShowTeleport;

		private List<TeleportVO> m_Teleports;
		/// <summary>
		/// 编辑器上是否要展开Teleport
		/// </summary>
		private bool[] m_ShowTeleportPoup;

		/// <summary>
		/// 编辑器上显示Teleport名字
		/// </summary>
		private string[] m_ShowTeleportNames;
		private void OnEnable()
		{
			m_Target = target as TeleportRoot;
			m_Teleports = m_Target.GetTeleportList();
			if (m_Teleports != null && m_Teleports.Count > 0)
			{
				m_ShowTeleportPoup = new bool[m_Teleports.Count];
			}
			m_ShowTeleportNames = m_Target.GetTeleportShowNames();
		}

		public override void OnInspectorGUI()
		{
			EditorGUI.indentLevel = 0;
			GUILayout.BeginVertical("box");
			if (m_Teleports != null && m_Teleports.Count > 0)
			{
				for (int iTeleport = 0; iTeleport < m_Teleports.Count; iTeleport++)
				{
					m_ShowTeleportPoup[iTeleport] = EditorGUILayout.Foldout(m_ShowTeleportPoup[iTeleport], m_ShowTeleportNames[iTeleport]);
					if (m_ShowTeleportPoup[iTeleport])
					{
						EditorGUI.indentLevel = 1;
						int[] chanelList = m_Teleports[iTeleport].ChanelList;
						if (chanelList != null && chanelList.Length > 0)
						{
							GUILayout.BeginVertical("box");
							if(m_Target.m_SelectLocation == null)
							{
                                m_Target.m_SelectLocation = new List<TeleportRoot.TeleportInfo>();
							}
							for (int iChanel = 0; iChanel < chanelList.Length; iChanel++)
							{
								GUILayout.BeginVertical("box");
								TeleportChanelVO chanelVO = ConfigVO<TeleportChanelVO>.Instance.GetData(chanelList[iChanel]);
								if (chanelVO != null)
								{
                                    TeleportRoot.TeleportInfo info = m_Target.GetTeleportInfo(chanelVO.ID);
                                    if (info == null)
									{
                                        info = new TeleportRoot.TeleportInfo(chanelVO.ID, -1);
                                        m_Target.m_SelectLocation.Add(info);
									}
									EditorGUILayout.LabelField("通道ID", chanelList[iChanel].ToString());
                                    EditorGamingMap gamingMap = EditorGamingMapData.LoadGamingMapFromJson(chanelVO.EndGamingMap);
                                    if(gamingMap != null)
                                    {
                                        EditorGUILayout.LabelField("终点地图", gamingMap.gamingmapName);
                                        EditorArea area = gamingMap.GetAreaByAreaId(chanelVO.EndGamingMapArea);
                                        if(area != null)
                                        {
                                            EditorGUILayout.LabelField("终点区域", area.areaName);
                                        }
                                    }
                                    
									EditorLocation[] locations = GamingMap.GetEditorLocations(chanelVO.EndGamingMap, chanelVO.EndGamingMapArea);
									if (locations != null && locations.Length > 0)
									{
										string[] m_LocationIdArray = null;
                                        List<string> locationIds = new List<string>();
										for (int iLocation = 0; iLocation < locations.Length; iLocation++)
										{
                                            EditorLocation location = locations[iLocation];
                                            if(location.locationType ==(int)LocationType.TeleportIn)
                                            {
                                                string locationStr = string.Format("{0}_{1}", locations[iLocation].locationName, locations[iLocation].locationId);
                                                locationIds.Add(locationStr);
                                            }
										}
                                        m_LocationIdArray = locationIds.ToArray();
                                        info.m_SelectIndex = EditorGUILayout.Popup("Location ID", info.m_SelectIndex, m_LocationIdArray);
										
									}
								}
								GUILayout.EndVertical();
							}
							GUILayout.EndVertical();
						}
					}
				}
			}
			GUILayout.EndVertical();
		}
		
	}
}
#endif