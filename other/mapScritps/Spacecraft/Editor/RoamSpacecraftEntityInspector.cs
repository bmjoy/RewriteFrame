#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Map
{
    [CustomEditor(typeof(RoamSpacecraftEntity))]
    public class RoamSpacecraftEntityInspector : Editor
    {
        private RoamSpacecraftEntity m_Target;
        /// <summary>
        /// 当前选的编号index
        /// </summary>
        private int m_CurSelectIdIndex = -1;

        /// <summary>
        /// 当前跃迁编号index
        /// </summary>
        private int m_CurLeapIndex = -1;
        /// <summary>
        /// 移动能力
        /// </summary>
        private bool m_MoveAbleFold;

        /// <summary>
        /// 最大速度
        /// </summary>
        private bool m_Move_MaxSpeed;

        /// <summary>
        /// 加速度
        /// </summary>
        private bool m_Move_Speed;

        /// <summary>
        /// 减速度
        /// </summary>
        private bool m_Move_DeSpeed;
        /// <summary>
        /// /转向能力
        /// </summary>
        private bool m_TurnAbleFold;

        /// <summary>
        /// 拟态表现
        /// </summary>
        private bool m_MimicryFold;

        /// <summary>
        /// 转向拟态
        /// </summary>
        private bool m_Mimicry_Turn;

        /// <summary>
        /// 升降拟态
        /// </summary>
        private bool m_Mimicry_Ver;
        /// <summary>
        /// 跃迁能力
        /// </summary>
        private bool m_TranslationFold;

        private int m_LastSelectIdIndex = -1;
        private SpacecraftSpeedMode m_LastSpeedMode;
        private string m_LastModelName;
        private void OnEnable()
        {
            m_CurSelectIdIndex = -1;
            m_Target = target as RoamSpacecraftEntity;
            if (m_Target.m_CurSpacecraftId>0)
            {
                List<EditorSpacecraft> spacecraftList = m_Target.m_SpacecraftDatas;
                if(spacecraftList != null && spacecraftList.Count>0)
                {
                    for(int iSpace = 0;iSpace<spacecraftList.Count;iSpace++)
                    {
                        if(spacecraftList[iSpace].spacecraft_id == m_Target.m_CurSpacecraftId)
                        {
                            m_CurSelectIdIndex = iSpace;
                            break;
                        }
                    }
                }
                if(m_CurSelectIdIndex<0)
                {
                    m_CurSelectIdIndex = m_Target.m_ShowIdList.IndexOf(m_Target.m_CurSpacecraftId.ToString());
                }
            }
            
            m_LastSelectIdIndex = m_CurSelectIdIndex;
            m_LastSpeedMode = m_Target.m_SpeedMode;
            m_LastModelName = m_Target.m_ModelName;
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.BeginHorizontal();
            m_CurSelectIdIndex = EditorGUILayout.Popup("编号ID",m_CurSelectIdIndex, m_Target.m_ShowIdList.ToArray(),GUILayout.MinWidth(100));
            if(GUILayout.Button("新增",GUILayout.MinWidth(50)))
            {
                m_Target.AddSpacecraftId();
                OnEnable();
            }
            EditorGUILayout.EndHorizontal();


            EditorGUILayout.BeginHorizontal();
            m_Target.m_SpeedMode = (SpacecraftSpeedMode)EditorGUILayout.EnumPopup("速度模式", m_Target.m_SpeedMode);
            if(m_Target.m_SpeedMode == SpacecraftSpeedMode.OverloadMode)
            {
                if(GUILayout.Button("复制巡航",GUILayout.MaxWidth(80)))
                {
                    m_Target.CopyCruiseMode();
                }
            }
            EditorGUILayout.EndHorizontal();

            if(!Application.isPlaying)
            {
                if (m_Target.m_LeapList != null && m_Target.m_LeapList.Count > 0)
                {
                    EditorGUILayout.BeginHorizontal();
                    m_CurLeapIndex = EditorGUILayout.Popup("跃迁点", m_CurLeapIndex, m_Target.m_ShowLeapIndexList.ToArray(), GUILayout.MinWidth(100));
                    if (GUILayout.Button("跃迁", GUILayout.MinWidth(50)))
                    {
                        m_Target.Transition(m_CurLeapIndex);
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
           

            EditorGUI.indentLevel = 0;
            m_MoveAbleFold = EditorGUILayout.Foldout( m_MoveAbleFold, "移动能力");
            if(m_MoveAbleFold)
            {
                EditorGUI.indentLevel = 1;
                m_Move_MaxSpeed = EditorGUILayout.Foldout(m_Move_MaxSpeed,"最大速度");
                if(m_Move_MaxSpeed)
                {
                    EditorGUI.indentLevel = 2;
                    m_Target.MaxHorSpeed = EditorGUILayout.FloatField("最大水平速度", m_Target.MaxHorSpeed);
                    m_Target.MaxVerSpeed = EditorGUILayout.FloatField("最大升降速度", m_Target.MaxVerSpeed);
                    // m_Target.m_CurEditorModeData.movebale.max_horspeed = EditorGUILayout.Vector4Field("最大水平速度", m_Target.m_CurEditorModeData.movebale.max_horspeed);
                    //m_Target.m_CurEditorModeData.movebale.max_verspeed = EditorGUILayout.Vector2Field("最大升降速度", m_Target.m_CurEditorModeData.movebale.max_verspeed);
                }
                EditorGUI.indentLevel = 1;
                m_Move_Speed = EditorGUILayout.Foldout(m_Move_Speed, "加速度");
                if (m_Move_Speed)
                {
                    EditorGUI.indentLevel = 2;
                    m_Target.m_CurEditorModeData.movebale.hor_forspeed = EditorGUILayout.FloatField("水平前进加速度", m_Target.m_CurEditorModeData.movebale.hor_forspeed);
                    m_Target.m_CurEditorModeData.movebale.hor_backspeed = EditorGUILayout.FloatField("水平后退加速度", m_Target.m_CurEditorModeData.movebale.hor_backspeed);
                    //m_Target.m_CurEditorModeData.movebale.hor_movespeed = EditorGUILayout.Vector2Field("左右平移加速度", m_Target.m_CurEditorModeData.movebale.hor_movespeed);
                    //m_Target.m_CurEditorModeData.movebale.ver_movespeed = EditorGUILayout.Vector2Field("升降加速度", m_Target.m_CurEditorModeData.movebale.ver_movespeed);
                    m_Target.HorMoveSpeed = EditorGUILayout.FloatField("左右平移加速度", m_Target.HorMoveSpeed);
                    m_Target.VerMoveSpeed = EditorGUILayout.FloatField("升降加速度", m_Target.VerMoveSpeed);
                }
                EditorGUI.indentLevel = 1;
                m_Move_DeSpeed = EditorGUILayout.Foldout(m_Move_DeSpeed,"减速度");
                if(m_Move_DeSpeed)
                {
                    EditorGUI.indentLevel = 2;
                    m_Target.HorDeSpeed = EditorGUILayout.FloatField("水平减速度", m_Target.HorDeSpeed);
                    //m_Target.m_CurEditorModeData.movebale.hor_despeed = EditorGUILayout.Vector2Field("水平减速度", m_Target.m_CurEditorModeData.movebale.hor_despeed);
                    m_Target.m_CurEditorModeData.movebale.ver_despeed = EditorGUILayout.FloatField("升降减速度", m_Target.m_CurEditorModeData.movebale.ver_despeed);
                }
            }
            EditorGUI.indentLevel = 0;
            m_TurnAbleFold = EditorGUILayout.Foldout(m_TurnAbleFold, "转向能力");
            if(m_TurnAbleFold)
            {
                EditorGUI.indentLevel = 2;
                m_Target.MaxTurnAngle = EditorGUILayout.FloatField("最大转向角度", m_Target.MaxTurnAngle);
                m_Target.TurnSpeed = EditorGUILayout.FloatField("转向角加速度", m_Target.TurnSpeed);
                m_Target.TurnDespeed = EditorGUILayout.FloatField("转向角减速度", m_Target.TurnDespeed);
                //m_Target.m_CurEditorModeData.turnable.max_turnangle = EditorGUILayout.Vector3Field("最大转向角度", m_Target.m_CurEditorModeData.turnable.max_turnangle);
                //m_Target.m_CurEditorModeData.turnable.turn_speed = EditorGUILayout.Vector3Field("转向角加速度", m_Target.m_CurEditorModeData.turnable.turn_speed);
                //m_Target.m_CurEditorModeData.turnable.turn_despeed = EditorGUILayout.Vector3Field("转向角减速度", m_Target.m_CurEditorModeData.turnable.turn_despeed);
            }
            EditorGUI.indentLevel = 0;
            m_MimicryFold = EditorGUILayout.Foldout(m_MimicryFold, "拟态表现");
            if(m_MimicryFold)
            {
                EditorGUI.indentLevel = 1;
                m_Mimicry_Turn = EditorGUILayout.Foldout(m_Mimicry_Turn, "转向拟态");
                if(m_Mimicry_Turn)
                {
                    EditorGUI.indentLevel = 2;
                    m_Target.m_CurEditorModeData.mimicry.turn_maxangle = EditorGUILayout.FloatField("拟态最大倾角", m_Target.m_CurEditorModeData.mimicry.turn_maxangle);
                    m_Target.m_CurEditorModeData.mimicry.turn_angelespeed = EditorGUILayout.FloatField("拟态角加速度", m_Target.m_CurEditorModeData.mimicry.turn_angelespeed);
                }
                EditorGUI.indentLevel = 1;
                m_Mimicry_Ver = EditorGUILayout.Foldout(m_Mimicry_Ver, "升降拟态");
                if (m_Mimicry_Ver)
                {
                    EditorGUI.indentLevel = 2;
                    m_Target.m_CurEditorModeData.mimicry.ver_maxangle = EditorGUILayout.FloatField("拟态最大倾角", m_Target.m_CurEditorModeData.mimicry.ver_maxangle);
                    m_Target.m_CurEditorModeData.mimicry.ver_anglespeed = EditorGUILayout.FloatField("拟态角加速度", m_Target.m_CurEditorModeData.mimicry.ver_anglespeed);
                }
            }
            if(m_Target.m_SpeedMode == SpacecraftSpeedMode.CruiseMode)
            {
                EditorGUI.indentLevel = 0;
                m_TranslationFold = EditorGUILayout.Foldout(m_TranslationFold, "跃迁能力");
                if (m_TranslationFold)
                {
                    EditorGUI.indentLevel = 2;
                    m_Target.m_CurEditorModeData.transition.originspeed = EditorGUILayout.FloatField("跃迁起始速度", m_Target.m_CurEditorModeData.transition.originspeed);
                    m_Target.m_CurEditorModeData.transition.speed = EditorGUILayout.FloatField("跃迁加速度", m_Target.m_CurEditorModeData.transition.speed);
                    m_Target.m_CurEditorModeData.transition.despeed = EditorGUILayout.FloatField("跃迁减速度", m_Target.m_CurEditorModeData.transition.despeed);
                }
            }
           

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("还原"))
            {
                m_Target.ReductionDefault();
            }
            if (GUILayout.Button("保存"))
            {
                m_Target.Save();
            }
            if(GUILayout.Button("同步到场景"))
            {
                m_Target.RefreshSpaceCraftProperty();
                m_Target.RefreshRuningSpaceCraftProperty();
            }
            EditorGUILayout.EndHorizontal();
            Refresh();
        }

        void Refresh()
        {
            if (m_CurSelectIdIndex>=0 &&(m_CurSelectIdIndex != m_LastSelectIdIndex || m_Target.m_SpeedMode != m_LastSpeedMode))
            {
               // m_Target.m_CurSpacecraftId = -1;
                bool isFind = false;
                List<EditorSpacecraft> spacecraftDatas = m_Target.m_SpacecraftDatas;
                if(spacecraftDatas != null && spacecraftDatas.Count>0)
                {
                    if (spacecraftDatas.Count > m_CurSelectIdIndex)
                    {
                        EditorSpacecraft editorData = spacecraftDatas[m_CurSelectIdIndex];
                        if (editorData != null)
                        {
                            m_Target.m_CurSpacecraftId = editorData.spacecraft_id;
                        }
                    }

                    EditorSpacecraft spaceCraft = spacecraftDatas.Find(x => x.spacecraft_id == m_Target.m_CurSpacecraftId);
                    if(spaceCraft != null && spaceCraft.spacecraft_modes != null && spaceCraft.spacecraft_modes.Length>0)
                    {
                        EditorSpacecraftMode[] modes = spaceCraft.spacecraft_modes;
                        for(int iMode = 0;iMode <modes.Length;iMode++)
                        {
                            if(modes[iMode].speed_mode == (int)m_Target.m_SpeedMode)
                            {
                                m_Target.m_CurEditorModeData.Copy(modes[iMode]);
                                isFind = true;
                                break;
                            }
                        }
                    }
                }
                if(!isFind)
                {
                    m_Target.m_CurEditorModeData.Init();
                    m_Target.m_CurEditorModeData.speed_mode = (int)m_Target.m_SpeedMode;
                }
                m_LastSelectIdIndex = m_CurSelectIdIndex;
                m_LastSpeedMode = m_Target.m_SpeedMode;
                m_Target.RefreshShowIdList();
            }

            GameObject targetObj = m_Target.GetPlayerPrefab();
            if (targetObj != null &&m_LastModelName != targetObj.name)
            {
                if(GUILayout.Button("同步预设信息"))
                {
                    m_Target.InitSpacecraftEditor();
                    m_CurSelectIdIndex = -1;
                    m_LastSelectIdIndex = m_CurSelectIdIndex;
                    m_LastModelName = m_Target.m_ModelName;
                }
            }
        }
    }
}

#endif