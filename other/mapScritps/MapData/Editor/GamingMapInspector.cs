#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Map
{
	[CustomEditor(typeof(GamingMap))]
	public class GamingMapInspector : Editor
	{
		private GamingMap m_Target;
		private int m_SelectTypeIndex;
		private bool m_AreaShow;
        private bool m_MissionShow;

        private int m_SelectSpaceIndex = -1;
        private int m_LastSelectSpaceIndex = -1;

        private int m_SelectFixedStarIndex = -1;
        private int m_LastSelectFixedStarIndex = -1;

        private List<int> m_NeedDeleteMissions;

        private List<MissionReleaseVO> m_MissionList;

        private void OnEnable()
		{
			m_Target = target as GamingMap;
			m_SelectTypeIndex = (int)m_Target.m_Type;
            m_NeedDeleteMissions = new List<int>();
            RefreshHumanSpaceIds();
            RefreshFixedStarIds();
            RefreshMissionIds();
        }

        private void RefreshMissionIds()
        {
            List<MissionReleaseVO> missionList = ConfigVO<MissionReleaseVO>.Instance.GetList();
            if (missionList == null)
            {
                return;
            }
            m_MissionList = missionList.FindAll((x) => x.type == (int)MissionReleaseType.MapMission);
        }

        private void RefreshHumanSpaceIds()
        {
            m_Target.LoadHumanSpaceIds();
            if (m_Target.m_HumanSpaceIds != null && m_Target.m_HumanSpaceIds.Count > 0)
            {
                m_SelectSpaceIndex = m_Target.m_HumanSpaceIds.IndexOf(m_Target.m_SpaceGamingMapId);
                m_LastSelectSpaceIndex = m_SelectSpaceIndex;
            }
        }

        private void RefreshFixedStarIds()
        {
            m_Target.LoadFixedStarIds();
            if(m_Target.m_FixedStarIds != null && m_Target.m_FixedStarIds.Count>0)
            {
                m_SelectFixedStarIndex = m_Target.m_FixedStarIds.IndexOf(m_Target.m_FixedStarId);
                m_LastSelectFixedStarIndex = m_SelectFixedStarIndex;
            }
        }
		
		public override void OnInspectorGUI()
		{
			if (m_Target == null)
			{
				return;
			}
			serializedObject.Update();
			EditorGUILayout.LabelField("Uid", m_Target.m_Uid.ToString());
			EditorGUILayout.LabelField("MapId", m_Target.m_MapId.ToString());
            m_Target.m_MaxPlayerNum = EditorGUILayout.IntField("最大人数",m_Target.m_MaxPlayerNum);
            m_Target.m_RemoveSecond = EditorGUILayout.IntField("销毁时间(秒)", m_Target.m_RemoveSecond);
			m_SelectTypeIndex = EditorGUILayout.Popup(new GUIContent("类型:"), m_SelectTypeIndex, EditorGamingMapData.GAMINGMAPTYPE_NAME);
            if (m_Target.m_Type != (GamingMapType)m_SelectTypeIndex)
            {
                m_Target.m_Type = (GamingMapType)m_SelectTypeIndex;
                RefreshHumanSpaceIds();
            }
            
            m_Target.m_PathType = (KMapPathType)EditorGUILayout.EnumPopup(new GUIContent("寻路类型:"), m_Target.m_PathType);
            if (m_Target.m_Type != GamingMapType.mapMainCity&& m_Target.m_Type != GamingMapType.mapSpaceStation)
            {
                m_SelectSpaceIndex = EditorGUILayout.Popup(new GUIContent("人形复活地图id:"), m_SelectSpaceIndex, m_Target.m_HumanSpaceIdStrs.ToArray());
            }

            if(m_Target.m_Type == GamingMapType.mapDeepSpace)
            {
                m_SelectFixedStarIndex = EditorGUILayout.Popup(new GUIContent("所属恒星:"), m_SelectFixedStarIndex, m_Target.m_FixedStarIdStrs.ToArray());
            }
            

			EditorGUI.indentLevel = 0;
			m_AreaShow = EditorGUILayout.Foldout(m_AreaShow, "Area列表");
			if(m_AreaShow)
			{
				EditorGUI.indentLevel = 1;
				if (m_Target.m_GamingAreaList != null && m_Target.m_GamingAreaList.Count>0)
				{
					for(int iGaming = 0;iGaming<m_Target.m_GamingAreaList.Count;iGaming++)
					{
						EditorGUILayout.ObjectField(m_Target.m_GamingAreaList[iGaming],typeof(GamingMapArea),true);
					}
				}
				
			}

			EditorGUI.indentLevel = 0;

			SerializedProperty nameProperty = serializedObject.FindProperty("m_MapName");
			EditorGUILayout.PropertyField(nameProperty, new GUIContent("名字"));

			serializedObject.ApplyModifiedProperties();

            EditorGUI.indentLevel = 0;
            GUILayout.BeginHorizontal();
            m_MissionShow = EditorGUILayout.Foldout(m_MissionShow, "任务发布规则列表");
            if(GUILayout.Button("新增"))
            {
                OnAdd();
            }
            GUILayout.EndHorizontal();
            if(m_MissionShow)
            {
                EditorGUI.indentLevel = 1;
                if (m_Target.m_MissionList != null && m_Target.m_MissionList.Count > 0)
                {
                    m_NeedDeleteMissions.Clear();
                    for (int iMission = 0; iMission < m_Target.m_MissionList.Count; iMission++)
                    {
                        int missionId = m_Target.m_MissionList[iMission];
                        GUILayout.BeginHorizontal();
                        GUILayout.Label(missionId.ToString());
                        if(GUILayout.Button("X"))
                        {
                            m_NeedDeleteMissions.Add(missionId);
                        }
                        GUILayout.EndHorizontal();
                    }
                    if(m_NeedDeleteMissions != null && m_NeedDeleteMissions.Count>0)
                    {
                        for(int iMission =0;iMission<m_NeedDeleteMissions.Count;iMission++)
                        {
                            m_Target.m_MissionList.Remove(m_NeedDeleteMissions[iMission]);
                        }
                        m_NeedDeleteMissions.Clear();
                    }
                }
            }
            EditorGUI.indentLevel = 0;
            RefreshSpaceGmaingMapId();
            RefreshFixedStarId();

        }

        private void OnAdd()
        {
            ConfigInfoWindow.OpenWindow(() =>
            {
                return m_MissionList != null ? m_MissionList.Count : 0;
            }, (index) =>
            {
                string str = "";
                if (m_MissionList != null && m_MissionList.Count > index)
                {
                    str = $"{m_MissionList[index].ID}";
                }
                return str;
            }, (index) =>
            {
                if (m_MissionList != null && m_MissionList.Count > index)
                {
                    if(!m_Target.m_MissionList.Contains(m_MissionList[index].ID))
                    {
                        m_Target.m_MissionList.Add(m_MissionList[index].ID);
                    }
                }

            });
        }
		
        private void RefreshFixedStarId()
        {
            if(m_LastSelectFixedStarIndex  == m_SelectFixedStarIndex)
            {
                return;
            }

            if(m_Target.m_FixedStarIds != null && m_Target.m_FixedStarIds.Count>0)
            {
                m_Target.m_FixedStarId = m_Target.m_FixedStarIds[m_SelectFixedStarIndex];
            }
            m_LastSelectFixedStarIndex = m_SelectFixedStarIndex;
        }

        private void RefreshSpaceGmaingMapId()
        {
            if(m_LastSelectSpaceIndex == m_SelectSpaceIndex)
            {
                return;
            }

            if(m_Target.m_HumanSpaceIds != null && m_Target.m_HumanSpaceIds.Count>0)
            {
                m_Target.m_SpaceGamingMapId = m_Target.m_HumanSpaceIds[m_SelectSpaceIndex];
            }

            m_LastSelectSpaceIndex = m_SelectSpaceIndex;
        }
	}
}
#endif
