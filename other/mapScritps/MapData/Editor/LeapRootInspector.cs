#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Map
{
    [CustomEditor(typeof(LeapRoot))]
    public class LeapRootInspector : Editor
    {
        private LeapRoot m_Target;
        private bool m_ShowVisiable;

        private string[] m_MainIdDisplayArray;
        private ulong[] m_MainIdArray;

        /// <summary>
        /// 所选中的主跃迁id
        /// </summary>
        private int m_SelectMainIdIndex;
        

        private void OnEnable()
        {
            m_Target = target as LeapRoot;
            List<Leap> leapList = m_Target.GetLeap();
            m_Target.m_MainLeapCache.Clear();
            if(leapList != null && leapList.Count>0)
            {
                for(int iLeap =0;iLeap<leapList.Count;iLeap++)
                {
                    Leap leap = leapList[iLeap];
                    if(leap.m_LeapType == LeapType.Main && leap.m_LeapId != m_Target.m_LeapId)
                    {
                        m_Target.m_MainLeapCache.Add(leap);
                    }
                }
            }

            if(m_Target.m_MainLeapCache != null && m_Target.m_MainLeapCache.Count>0)
            {
                m_MainIdDisplayArray = new string[m_Target.m_MainLeapCache.Count];
                m_MainIdArray = new ulong[m_Target.m_MainLeapCache.Count];
                for (int iMain = 0;iMain<m_Target.m_MainLeapCache.Count;iMain++)
                {
                    Leap leap = m_Target.m_MainLeapCache[iMain];
                    m_MainIdDisplayArray[iMain] = string.Format("{0}_{1}", leap.m_LeapId, leap.m_LeapName);
                    m_MainIdArray[iMain] = leap.m_LeapId;
                    if(m_Target.m_MainLeapId == leap.m_LeapId)
                    {
                        m_SelectMainIdIndex = iMain;
                    }
                }
            }
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.indentLevel = 0;
            EditorGUILayout.LabelField("leap_id:", m_Target.m_LeapId.ToString());
            m_Target.m_LeapName = EditorGUILayout.TextField("跃迁名字:", m_Target.m_LeapName);
            EditorGUILayout.LabelField("跃迁描述:");
            m_Target.m_LeapDescription = EditorGUILayout.TextArea(m_Target.m_LeapDescription,GUILayout.Height(30));
            //m_Target.m_Icon = (Sprite)EditorGUILayout.ObjectField("Icon资源:", m_Target.m_Icon, typeof(Sprite),false);

            IconVO iconVo = ConfigVO<IconVO>.Instance.GetData(m_Target.m_IconConfId);
            if (iconVo != null)
            {
                EditorGUILayout.LabelField("Icon ID", string.Format("{0}_{1}", iconVo.ID, iconVo.assetName));
            }
            else
            {
                EditorGUILayout.LabelField("Icon ID");
            }

            if (GUILayout.Button("选择"))
            {
                OnSelect();
            }

            m_Target.m_LeapType = (LeapType)EditorGUILayout.EnumPopup("跃迁类型:", m_Target.m_LeapType);

            if(m_Target.m_LeapType == LeapType.Main)
            {
                m_Target.m_MainLeapId = 0;
                EditorGUILayout.LabelField("主跃迁Id:", m_Target.m_MainLeapId.ToString());
                m_Target.m_GamingMapArea.RefreshAreaInfo();
            }
            else if(m_Target.m_LeapType == LeapType.Child)
            {
                if(m_MainIdDisplayArray != null)
                {
                    m_SelectMainIdIndex = EditorGUILayout.Popup("主跃迁Id:", m_SelectMainIdIndex, m_MainIdDisplayArray);
                    if (m_SelectMainIdIndex >= 0)
                    {
                        if (m_MainIdArray != null && m_MainIdArray.Length > m_SelectMainIdIndex)
                        {
                            m_Target.m_MainLeapId = m_MainIdArray[m_SelectMainIdIndex];
                        }
                    }
                }
                else
                {
                    m_Target.m_MainLeapId = 0;
                }
                m_Target.m_GamingMapArea.RefreshAreaInfo();
            }
            
            m_Target.m_Range = EditorGUILayout.Slider("球体半径:",m_Target.m_Range, 0.1f, 100f);
            m_Target.m_Offest = EditorGUILayout.Slider("碰撞盒偏移:",m_Target.m_Offest,0.1f,5f);
            m_Target.m_AutoVisible = EditorGUILayout.Toggle("是否直接可见:",m_Target.m_AutoVisible);
            if(m_Target.m_VisibleLeapList != null)
            {
                EditorGUI.indentLevel = 0;
                m_ShowVisiable = EditorGUILayout.Foldout(m_ShowVisiable, "可见跃迁点");
                if(m_ShowVisiable)
                {
                    EditorGUI.indentLevel = 1;
                    for(int iVisible = 0;iVisible<m_Target.m_VisibleLeapList.Length;iVisible++)
                    {
                        EditorGUILayout.LabelField(m_Target.m_VisibleLeapList[iVisible].ToString());
                    }
                }
            }
            
        }

        private void OnSelect()
        {
            ConfigInfoWindow.OpenWindow(() =>
            {
                List<IconVO> iconList = ConfigVO<IconVO>.Instance.GetList();
                return iconList != null ? iconList.Count : 0;
            }, (index) =>
            {
                List<IconVO> iconList = ConfigVO<IconVO>.Instance.GetList();
                string str = "";
                if(iconList != null && iconList.Count>index)
                {
                    str = $"{iconList[index].ID}_{iconList[index].squareName}";
                }
                return str;
            }, (index) =>
            {
                List<IconVO> iconList = ConfigVO<IconVO>.Instance.GetList();
                if(iconList != null && iconList.Count>index)
                {
                    m_Target.m_IconConfId = iconList[index].ID;
                }
                
            });
        }
    }
}

#endif