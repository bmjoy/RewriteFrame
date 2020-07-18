#if UNITY_EDITOR
using Leyoutech.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Map
{
    public enum GridOffest
    {
        Min = 0,
        GRID_1X1X1 = 0,
        GRID_3X3X3 = 1,
        GRID_5X5X5 = 2,
        GRID_7X7X7 = 3,
        GRID_9X9X9 = 4,
        GRID_11X11X11 = 5,
        GRID_13X13X13 = 6,
        GRID_15X15X15 = 7,
        GRID_17X17X17 = 8,
        GRID_19X19X19 = 9,
        GRID_21X21X21=10,
        GRID_23X23X23 =11,
        GRID_25X25X25=12,
        GRID_27X27X27=13,
        GRID_29X29X29=14,
        GRID_31X31X31=15,
        GRID_33X33X33 = 16,
        GRID_35X35X35 =17,
        GRID_37X37X37 = 18,
        GRID_39X39X39 = 19,
        GRID_41X41X41 = 20,
        GRID_43X43X43 = 21,
        GRID_45X45X45 = 22,
        GRID_47X47X47 = 23,
        GRID_49X49X49 = 24,
        GRID_51X51X51 = 25,
        GRID_53X53X53 = 26,
        GRID_55X55X55 = 27,
        GRID_57X57X57 = 28,
        Max = 28
    }

    /// <summary>
    /// 拆分区域前的一些配置
    /// </summary>
    public class SplitAreaWindow : EditorWindow
    {
        private ExportData m_ExportData;
        /// <summary>
        /// 切分区域配置(在导出前)
        /// </summary>
        private EditorSplitAreaSetting m_EditorSplitAreaSetting;
        /// <summary>
        /// 区域层信息
        /// </summary>
        private List<EditorSplitAreaLayerSetting> m_EditorSplitAreaLayerSettings;
        public static void OpenWindow(ExportData exportData)
        {
            if(exportData == null)
            {
                return;
            }
            SplitAreaWindow win = GetWindow<SplitAreaWindow>();
            win.titleContent = new GUIContent("拆分区域配置");
            win.Show();
            win.Init(exportData);
        }


        private Scene m_OwnerScene;
        /// <summary>
        /// 获取当前scene
        /// </summary>
        /// <returns></returns>
        public Scene GetOwnerScene()
        {
            if (m_OwnerScene.name == null)
            {
                InitOwnerScene();
            }
            DebugUtility.Assert(m_OwnerScene.name != null && m_OwnerScene.path != null, "获取的当前Scene不对");
            return m_OwnerScene;
        }

        /// <summary>
		/// 获取当前打开的Scene
		/// </summary>
		private void InitOwnerScene()
        {
            for (int iScene = 0; iScene < SceneManager.sceneCount; iScene++)
            {
                Scene scene = SceneManager.GetSceneAt(iScene);
                if (!scene.isLoaded)
                {
                    continue;
                }
                GameObject[] rootObjs = scene.GetRootGameObjects();
                if (rootObjs != null && rootObjs.Length > 0)
                {
                    for (int iRoot = 0; iRoot < rootObjs.Length; iRoot++)
                    {
                        Map map = rootObjs[iRoot].GetComponent<Map>();
                        if (map != null && map == this)
                        {
                            m_OwnerScene = scene;
                            break;
                        }
                    }
                }
            }
        }

        public void Init(ExportData exportData)
        {
            m_ExportData = exportData;
            if(m_ExportData == null)
            {
                return;
            }
            //TODO:1、读取Area对应的导出配置，如果没有则创建
            //Scene scene = GetOwnerScene();
            //string[] paths = scene.path.Split('/');
            //if (paths != null && paths.Length > 0)
            //{
            //    string ownerSceneFloder = scene.path.Substring(0, scene.path.Length - paths[paths.Length - 1].Length);
            //    string ownerAreaPath = string.Format("{0}{1}", ownerSceneFloder, m_OwnerScene.name);
            //    FileUtility.ExistsDirectoryOrCreate(ownerAreaPath);
            //    AssetDatabase.Refresh();
            //}

            Map map = m_ExportData.m_Map;
            if(map == null || map.gameObject == null)
            {
                return;
            }
            List<AreaSpawner> areaSpawners = m_ExportData.m_AreaSpawners;
            if(areaSpawners == null || areaSpawners.Count<0)
            {
                return;
            }
            string settingPath = string.Format("{0}/SplitArea_{1}_{2}.asset", map.GetOwnerAreaPath(), map.Uid, areaSpawners[0].m_AreaUid);
            m_EditorSplitAreaSetting = AssetDatabase.LoadAssetAtPath<EditorSplitAreaSetting>(settingPath);
            if(m_EditorSplitAreaSetting == null)
            {
                m_EditorSplitAreaSetting = new EditorSplitAreaSetting();
                AssetDatabase.CreateAsset(m_EditorSplitAreaSetting, settingPath);
                m_EditorSplitAreaSetting = AssetDatabase.LoadAssetAtPath<EditorSplitAreaSetting>(settingPath);
            }
            if(m_EditorSplitAreaSetting == null)
            {
                Debug.LogError("创建区域导出配置失败");
                return;
            }
            m_EditorSplitAreaLayerSettings = m_EditorSplitAreaSetting.m_EditorSplitAreaLayerSettings;
            if(m_EditorSplitAreaLayerSettings == null)
            {
                m_EditorSplitAreaLayerSettings = new List<EditorSplitAreaLayerSetting>();
                m_EditorSplitAreaSetting.m_EditorSplitAreaLayerSettings = m_EditorSplitAreaLayerSettings;
            }
        }

        private void OnGUI()
        {
            DrawUI();
        }

        private void OnDisable()
        {
            if(m_EditorSplitAreaSetting != null)  
            {
                EditorUtility.SetDirty(m_EditorSplitAreaSetting);
            }
        }

        #region 界面刷新

        private Vector2 m_ScrollPos = new Vector2(0, 0);

        private Vector2 m_DynamicScrollPos = new Vector2(0,0);
        /// <summary>
        /// 层名称
        /// </summary>
        public string m_Name;
        /// <summary>
        /// 虚拟格子大小
        /// </summary>
        public int m_Size;
        /// <summary>
        /// 优先级
        /// </summary>
        public int m_Priority;
        /// <summary>
        /// 最小物件AABB盒大小
        /// </summary>
        public float m_MinAABB;
        /// <summary>
        /// 最大物件AABB盒大小
        /// </summary>
        public float m_MaxAABB;

        /// <summary>
        /// 格子偏移
        /// </summary>
        public int m_GridOffest;
        

        void DrawUI()
        {
            if(m_EditorSplitAreaSetting == null)
            {
                return;
            }
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("开始拆解区域",GUILayout.Height(50)))
            {
                if(m_ExportData == null)
                {
                    EditorUtility.DisplayDialog("拆解区域", "拆解失败!拆解数据为空", "OK");
                    return; 
                }
                Map map = m_ExportData.m_Map;
                List<AreaSpawner> areaSpawners = m_ExportData.m_AreaSpawners;
                new Exporter().BeginExport(map, map.ExportSetting, m_ExportData.m_ExportParameter, m_ExportData.m_AreaSpawners);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if(GUILayout.Button("Analyze",GUILayout.Height(50)))
            {
                if (m_ExportData == null)
                {
                    EditorUtility.DisplayDialog("Analyze", "Analyze失败!拆解数据为空", "OK");
                    return;
                }
                Map map = m_ExportData.m_Map;
                new AnalyzeArea().Start(map, m_ExportData.m_AreaSpawners[0]);
            }
            GUILayout.EndHorizontal();


            GUILayout.BeginHorizontal();
            if (GUILayout.Button("层名字", GUILayout.Width(200)))
            {

            }
            if (GUILayout.Button("优先级", GUILayout.Width(80)))
            {

            }
            if (GUILayout.Button("虚拟格子大小", GUILayout.Width(100)))
            {

            }
            if (GUILayout.Button("物件AABB MIN", GUILayout.Width(150)))
            {

            }
            if (GUILayout.Button("物件AABB MAX", GUILayout.Width(150)))
            {

            }
            if(GUILayout.Button("偏移",GUILayout.Width(150)))
            {

            }
            GUILayout.Label("屏幕占比:",GUILayout.MaxWidth(80));
            m_EditorSplitAreaSetting.m_Rate = EditorGUILayout.FloatField( m_EditorSplitAreaSetting.m_Rate,GUILayout.MaxWidth(100));
            GUILayout.EndHorizontal();

            GUILayout.BeginVertical();
            m_ScrollPos = EditorGUILayout.BeginScrollView(m_ScrollPos);
            if(m_EditorSplitAreaLayerSettings != null)
            {
                for (int iLayer = m_EditorSplitAreaLayerSettings.Count - 1; iLayer >= 0; iLayer--)
                {
                    EditorSplitAreaLayerSetting layerInfo = m_EditorSplitAreaLayerSettings[iLayer];
                    GUILayout.BeginHorizontal();
                    layerInfo.m_LayerName = GUILayout.TextField(layerInfo.m_LayerName, GUILayout.MaxWidth(200));
                    layerInfo.m_Priority = EditorGUILayout.IntField(layerInfo.m_Priority, GUILayout.MaxWidth(80));
                    layerInfo.m_GridSize = EditorGUILayout.IntField(layerInfo.m_GridSize, GUILayout.MaxWidth(100));
                    layerInfo.m_MinAABBSize = EditorGUILayout.FloatField(layerInfo.m_MinAABBSize, GUILayout.MaxWidth(150));
                    layerInfo.m_MaxAABBSize = EditorGUILayout.FloatField(layerInfo.m_MaxAABBSize,GUILayout.MaxWidth(150));
                    layerInfo.m_Offest = (int)(GridOffest)EditorGUILayout.EnumPopup((GridOffest)layerInfo.m_Offest, GUILayout.MaxWidth(150));
                    if (GUILayout.Button("删除"))
                    {
                        m_EditorSplitAreaLayerSettings.Remove(layerInfo);
                    }
                    GUILayout.EndHorizontal();
                }
            }
            
            GUILayout.EndScrollView();
            GUILayout.BeginHorizontal();
            //添加层
            m_Name = EditorGUILayout.TextField(m_Name, GUILayout.MaxWidth(200));
            m_Priority = EditorGUILayout.IntField(m_Priority, GUILayout.MaxWidth(80));
            m_Size = EditorGUILayout.IntField(m_Size, GUILayout.MaxWidth(50));
            m_MinAABB = EditorGUILayout.FloatField(m_MinAABB, GUILayout.MaxWidth(150));
            m_MaxAABB = EditorGUILayout.FloatField(m_MaxAABB, GUILayout.MaxWidth(150));
            m_GridOffest = (int)(GridOffest)EditorGUILayout.EnumPopup((GridOffest)m_GridOffest, GUILayout.MaxWidth(150));
            if (GUILayout.Button("添加",GUILayout.MaxWidth(100)))
            {
                EditorSplitAreaLayerSetting layerInfo = new EditorSplitAreaLayerSetting();
                layerInfo.m_MaxAABBSize = m_MaxAABB;
                layerInfo.m_MinAABBSize = m_MinAABB;
                layerInfo.m_LayerName = m_Name;
                layerInfo.m_Priority = m_Priority;
                layerInfo.m_GridSize = m_Size;
                layerInfo.m_Offest = m_GridOffest;
                m_EditorSplitAreaLayerSettings.Add(layerInfo);
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }
#endregion
    }

    
}

#endif