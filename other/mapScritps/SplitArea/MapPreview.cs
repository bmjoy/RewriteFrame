#if UNITY_EDITOR
using Leyoutech.Utility;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Map
{
    [ExecuteInEditMode]
	public class MapPreview : MonoBehaviour
	{
        #region 属性
        [Tooltip("导出的map信息")]
        public MapInfo MapInfo;
        [Tooltip("是否生成区域预览")]
        public bool InstantiateAllArea = false;
        [Tooltip("是否删除区域预览")]
        public bool DestroyAllArea = false;
        [Tooltip("是否打开格子预览")]
        public bool OpenMapVoxelGridPreview = false;
        /// <summary>
        /// 是否使用资源管理模式
        /// </summary>
        public bool UseAsset = false;
        /// <summary>
        /// 区域预览
        /// </summary>
        private AreaPreview[] m_AreaPreviews;
        private IEnumerator m_DoUpdateEnumerator;
        private MapVoxelGridPreview m_MapVoxelGridPreview;
        /// <summary>
        /// 加载进来的MapScene
        /// </summary>
        private Scene m_MapScene;

        #endregion

        #region 生命周期
        private void OnEnable()
        {
            m_DoUpdateEnumerator = DoUpdate();
            EditorApplication.update += OnUpdate;
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGUI;
        }

        private void OnDisable()
        {
            EditorApplication.update -= OnUpdate;
            m_DoUpdateEnumerator = null;
            EditorApplication.hierarchyWindowItemOnGUI -= OnHierarchyGUI;
        }

        private void OnUpdate()
        {
            if(m_DoUpdateEnumerator != null)
            {
                m_DoUpdateEnumerator.MoveNext();
            }
           
        }
        #endregion

        #region 地图预览相关
        public uint GetMapUid()
        {
            return MapInfo.Uid;
        }

        private IEnumerator DoUpdate()
        {
            while (true)
            {
                gameObject.name = Constants.MAP_PREVIEW_GAMEOBJECT_NAME_STARTWITHS
                    + (MapInfo != null ? MapInfo.Uid.ToString() : "");
                transform.position = Vector3.zero;
                transform.rotation = Quaternion.identity;
                transform.localScale = Vector3.one;

                yield return null;
                if (MapInfo == null)
                {
                    continue;
                }

                if (InstantiateAllArea)
                {
                    InstantiateAllArea = false;
                    DoInstantiateAllArea();
                }
                else if (DestroyAllArea)
                {
                    DestroyAllArea = false;
                    DoDestroyAllArea();
                }
                else if (OpenMapVoxelGridPreview)
                {
                    OpenMapVoxelGridPreview = false;
                    if (m_MapVoxelGridPreview == null)
                    {
                        GameObject mapVoxelGridPreviewGameObject = new GameObject();
                        mapVoxelGridPreviewGameObject.transform.SetParent(transform);
                        m_MapVoxelGridPreview = mapVoxelGridPreviewGameObject.AddComponent<MapVoxelGridPreview>();
                        m_MapVoxelGridPreview.Initialize(MapInfo);
                    }
                    Selection.activeGameObject = m_MapVoxelGridPreview.gameObject;
                }

                if (m_AreaPreviews != null)
                {
                    for (int iArea = 0; iArea < m_AreaPreviews.Length; iArea++)
                    {
                        yield return null;
                        IEnumerator iterAreaUpdateEnumerator = m_AreaPreviews[iArea].DoUpdate();
                        while (iterAreaUpdateEnumerator.MoveNext())
                        {
                            yield return null;
                        }
                    }
                }
            }
        }

        private void DoInstantiateAllArea()
        {
            if (m_AreaPreviews != null)
            {
                DoDestroyAllArea();
            }

            m_AreaPreviews = new AreaPreview[MapInfo.AreaInfos.Length];
            for (int iArea = 0; iArea < MapInfo.AreaInfos.Length; iArea++)
            {
                AreaInfo iterAreaInfo = MapInfo.AreaInfos[iArea];
                GameObject iterArea = new GameObject();
                AreaPreview iterAreaPreview = iterArea.AddComponent<AreaPreview>();
                m_AreaPreviews[iArea] = iterAreaPreview;
                iterAreaPreview.Initialie(this, iterAreaInfo);
            }
        }

        /// <summary>
        /// 卸载MapScene
        /// </summary>
        public void UnLoadMapScene()
        {
            if (m_MapScene != null && m_MapScene.isLoaded)
            {
                if (Application.isPlaying)
                {
                    SceneManager.UnloadSceneAsync(m_MapScene);
                }
                else
                {
                    EditorSceneManager.CloseScene(m_MapScene, true);
                }
            }
        }
        /// <summary>
        /// 加载MapScene
        /// </summary>
        public void LoadMapScene()
        {
            UnLoadMapScene();
            string sceneName = Constants.EXPORT_MAP_FOLDER_NAME_STARTWITHS + GetMapUid();
            MapEditorSetting mapEditorSetting = MapEditorUtility.GetOrCreateMapEditorSetting();
            string scenePath = string.Format("{0}/{1}/{2}{3}.unity"
                , mapEditorSetting.AssetExportDirectory
                , sceneName
                , Constants.SCENE_EXPORT_FILENAMESTARTWITHS
                , GetMapUid());
            if (!File.Exists(scenePath))
            {
                EditorUtility.DisplayDialog("提示", "未导出对应的MapScene", "确定");
                return;
            }
            if (Application.isPlaying)
            {
                Scene scene = SceneManager.GetSceneByPath(scenePath);
                if (!scene.isLoaded)
                {
                    m_MapScene = scene;
                    SceneManager.LoadScene(scenePath, LoadSceneMode.Additive);
                }
            }
            else
            {
                m_MapScene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            }

        }

        private void DoDestroyAllArea()
        {
            m_AreaPreviews = null;
            ObjectUtility.DestroyAllChildern(transform);
        }

        /// <summary>
        /// 右键响应
        /// </summary>
        /// <param name="instanceID"></param>
        /// <param name="selectionRect"></param>
        private void OnHierarchyGUI(int instanceID, Rect selectionRect)
        {
            if (Event.current != null && selectionRect.Contains(Event.current.mousePosition)
                && Event.current.button == 1 && Event.current.type <= EventType.MouseUp)
            {
                GameObject selectedGameObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
                if (selectedGameObject)
                {

                    MapPreview mapPreview = selectedGameObject.GetComponent<MapPreview>();
                    if (mapPreview != null)
                    {
                        List<GUIContent> contentList = new List<GUIContent>();
                        contentList.Add(new GUIContent("生成所有区域"));
                        contentList.Add(new GUIContent("删除所有区域"));
                        contentList.Add(new GUIContent("加载MapScene"));
                        if (mapPreview.m_MapScene != null && mapPreview.m_MapScene.isLoaded)
                        {
                            contentList.Add(new GUIContent("卸载MapScene"));
                        }
                        RefreshMapPreviewUI(contentList, mapPreview);

                    }
                }
            }
        }

        /// <summary>
        /// 刷新右键MapPreview的界面
        /// </summary>
        private void RefreshMapPreviewUI(List<GUIContent> contentList, MapPreview mapPreview)
        {
            if (mapPreview == null)
            {
                return;
            }

            Vector2 mousePosition = Event.current.mousePosition;
            GameObject userData = Selection.activeGameObject;
            int selected = -1;
            EditorUtility.DisplayCustomMenu(new Rect(mousePosition.x, mousePosition.y, 0, 0), contentList.ToArray(), selected,
                delegate (object data, string[] opt, int select)
                {
                    switch (select)
                    {
                        case 0://生成所有区域
                            mapPreview.InstantiateAllArea = true;
                            break;
                        case 1://删除所有区域
                            mapPreview.DestroyAllArea = true;
                            break;
                        case 2://加载对应MapScene
                            mapPreview.LoadMapScene();
                            break;
                        case 3:
                            mapPreview.UnLoadMapScene();
                            break;
                    }
                }, userData);
            Event.current.Use();
        }

        #endregion
    }
}
#endif