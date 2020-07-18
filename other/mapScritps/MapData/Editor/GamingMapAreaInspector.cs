#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Map
{
	[CustomEditor(typeof(GamingMapArea))]
	public class GamingMapAreaInspector : Editor
	{
		private GamingMapArea m_Target;

        private int m_SelectRelieveIndex = -1;
        private int m_LastSelectRelieveIndex = -1;

        private List<MissionReleaseVO> m_MissionList;
        private List<int> m_NeedDeleteMissions;
        private bool m_MissionShow;
        private void OnEnable()
		{
			m_Target = target as GamingMapArea;
            m_Target.LoadRelieveCreature();
            if(m_Target.m_RelieveIdCache != null)
            {
                m_SelectRelieveIndex = m_Target.m_RelieveIdCache.IndexOf(m_Target.m_RelieveCreatue);
            }
            m_LastSelectRelieveIndex = m_SelectRelieveIndex;
            m_NeedDeleteMissions = new List<int>();
            RefreshMissionIds();
        }

        private void RefreshMissionIds()
        {
            List<MissionReleaseVO> missionList = ConfigVO<MissionReleaseVO>.Instance.GetList();
            if (missionList == null)
            {
                return;
            }
            m_MissionList = missionList.FindAll((x) => x.type == (int)MissionReleaseType.AreaMission);
        }

        public override void OnInspectorGUI()
		{
            EditorGUILayout.LabelField("区域Id", m_Target.m_AreaId.ToString());
			m_Target.m_AreaName = EditorGUILayout.TextField("区域名字", m_Target.m_AreaName);
            m_Target.m_AreaType = (AreaType)EditorGUILayout.EnumPopup(new GUIContent("区域类型"),m_Target.m_AreaType);
            if(m_Target.m_RelieveCache != null)
            {
                m_SelectRelieveIndex = EditorGUILayout.Popup("复活npc", m_SelectRelieveIndex, m_Target.m_RelieveCache.ToArray());
                if (m_SelectRelieveIndex != m_LastSelectRelieveIndex)
                {
                    m_Target.m_RelieveCreatue = m_Target.m_RelieveIdCache[m_SelectRelieveIndex];
                    m_LastSelectRelieveIndex = m_SelectRelieveIndex;
                }
            }
            
            SerializedProperty creatureRootProperty = serializedObject.FindProperty("m_CreatureRoot");
			EditorGUILayout.PropertyField(creatureRootProperty, new GUIContent("CreatureRoot"));

			SerializedProperty locationRootProperty = serializedObject.FindProperty("m_LocationRoot");
			EditorGUILayout.PropertyField(locationRootProperty, new GUIContent("LocationRoot"));

			SerializedProperty teleportRootProperty = serializedObject.FindProperty("m_TeleportRoot");
			EditorGUILayout.PropertyField(locationRootProperty, new GUIContent("TeleportRoot"));

            SerializedProperty leapRootProperty = serializedObject.FindProperty("m_LeapRoot");
            EditorGUILayout.PropertyField(leapRootProperty, new GUIContent("LeapRoot"));

            SerializedProperty triggerRootProperty = serializedObject.FindProperty("m_TriggerRoot");
            EditorGUILayout.PropertyField(triggerRootProperty, new GUIContent("TriggerRoot"));

            EditorGUI.indentLevel = 0;
            GUILayout.BeginHorizontal();
            m_MissionShow = EditorGUILayout.Foldout(m_MissionShow, "任务发布规则列表");
            if (GUILayout.Button("新增"))
            {
                OnAdd();
            }
            GUILayout.EndHorizontal();
            if (m_MissionShow)
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
                        if (GUILayout.Button("X"))
                        {
                            m_NeedDeleteMissions.Add(missionId);
                        }
                        GUILayout.EndHorizontal();
                    }
                    if (m_NeedDeleteMissions != null && m_NeedDeleteMissions.Count > 0)
                    {
                        for (int iMission = 0; iMission < m_NeedDeleteMissions.Count; iMission++)
                        {
                            m_Target.m_MissionList.Remove(m_NeedDeleteMissions[iMission]);
                        }
                        m_NeedDeleteMissions.Clear();
                    }
                }
            }
            EditorGUI.indentLevel = 0;
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
                    if (!m_Target.m_MissionList.Contains(m_MissionList[index].ID))
                    {
                        m_Target.m_MissionList.Add(m_MissionList[index].ID);
                    }
                }

            });
        }
    }
   
}
#endif