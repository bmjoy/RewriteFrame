//#define USE_SPILITAREA
#if UNITY_EDITOR && !USE_SPILITAREA
using Leyoutech.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Map
{
    [ExecuteInEditMode]
    public class AreaPreview : MonoBehaviour
    {
        #region 属性
        [Tooltip("Area信息")]
        public AreaInfo AreaInfo;
        [Tooltip("是否生成所有unit")]
        public bool InstantiateAllUnit;
        [Tooltip("是否删除所有unit")]
        public bool DestroyAllUnit;
        [Tooltip("是否刷新区域预览")]
        public bool UpdateVoxelPreview;
        [Tooltip("是否移除区域预览")]
        public bool RemoveVoxelPreview;
        [Tooltip("需要预览的格子")]
        public Vector3 VoxelPreviewIndex;
        [Tooltip("是否要显示格子Gizmos")]
        public bool DisplayVoxelGizmos;
        [Tooltip("格子Gizmos颜色")]
        public Color VoxelGizmosColor = Color.gray;
        [Tooltip("是否刷新格子预览")]
        public bool AutoUpdateVoxelPreview;

        private MapPreview m_Owner;
        private AreaDetailInfo m_AreaDetailInfo;
        private Transform m_UnitsRoot;
        private Transform m_VoxelRoot;
        private int m_WaitInstantiateCount = 0;
        private bool m_VoxelPreviewing = false;
        private Vector3 m_PreviewVoxelCenter;
        private Vector3 m_LastVoxelPreviewIndex;
        #endregion

        #region 生命周期
        private void OnEnable()
        {
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGUI;
        }

        private void OnDisable()
        {
            EditorApplication.hierarchyWindowItemOnGUI -= OnHierarchyGUI;
        }

        public void Initialie(MapPreview owner, AreaInfo area)
        {
            m_Owner = owner;
            AreaInfo = area;

            gameObject.name = Constants.AREA_PREVIEW_GAMEOBJECT_NAME_STARTWITHS + AreaInfo.Uid;
            transform.SetParent(owner.transform, false);

            m_UnitsRoot = new GameObject("Units").transform;
            m_UnitsRoot.SetParent(transform);
            m_VoxelRoot = new GameObject(Constants.VOXEL_PREVIEW_GAMEOBJECT_NAME_FORMAT).transform;
            m_VoxelRoot.SetParent(transform);
        }
        public IEnumerator DoUpdate()
        {
            transform.SetPositionAndRotation(AreaInfo.Position, AreaInfo.Rotation);
            yield return null;

            if (AreaInfo == null)
            {
                yield break;
            }

            if (InstantiateAllUnit)
            {
                if (TryLoadAreaDetailInfo())
                {
                    InstantiateAllUnit = false;
                    DoInstantiateAllUnit();
                }
            }
            else if (DestroyAllUnit)
            {
                DestroyAllUnit = false;
                DoDestroyAllUnit();
            }
            else if (AutoUpdateVoxelPreview
                || UpdateVoxelPreview)
            {
                if (TryLoadAreaDetailInfo())
                {
                    UpdateVoxelPreview = false;
                }
            }
            else if (RemoveVoxelPreview)
            {
                RemoveVoxelPreview = false;
                DoRemoveVoxelPreview();
            }
        }

        #endregion
        
        #region 区域预览相关
        protected void OnDrawGizmosSelected()
        {
            if (DisplayVoxelGizmos
                && m_VoxelPreviewing)
            {
                Gizmos.color = VoxelGizmosColor;
                // Gizmos.DrawCube(m_PreviewVoxelCenter, Vector3.one * m_AreaDetailInfo.VoxelGridInfo.VoxelSize);
            }
        }

        private bool TryLoadAreaDetailInfo()
        {
            if (m_AreaDetailInfo == null)
            {
                string areaDetailInfoPath = string.Format("{0}/{1}{2}/{3}.asset"
                    , MapEditorUtility.GetOrCreateMapEditorSetting().AssetExportDirectory
                    , Constants.EXPORT_MAP_FOLDER_NAME_STARTWITHS
                    , m_Owner.GetMapUid()
                    , AreaInfo.DetailInfoAddressableKey);
                m_AreaDetailInfo = AssetDatabase.LoadAssetAtPath<AreaDetailInfo>(areaDetailInfoPath);
                if (areaDetailInfoPath == null)
                {
                    Debug.LogError(string.Format("cant find file ({0})", areaDetailInfoPath));
                }
            }

            return m_AreaDetailInfo != null;
        }
        

        private void DoRemoveVoxelPreview()
        {
            m_LastVoxelPreviewIndex = Vector3.one * -1;
            m_VoxelPreviewing = false;
            ObjectUtility.DestroyAllChildern(m_VoxelRoot);
        }

        private void DoInstantiateAllUnit()
        {
            if (m_WaitInstantiateCount != 0)
            {
                return;
            }

            DoDestroyAllUnit();
            AreaLayerInfo[] areaLayerInfos = m_AreaDetailInfo.AreaLayerInfos;
            if (areaLayerInfos != null && areaLayerInfos.Length > 0)
            {
                for (int iLayer = 0; iLayer < areaLayerInfos.Length; iLayer++)
                {
                    List<SceneUnitInfo> units = areaLayerInfos[iLayer].m_Units;
                    if (units != null && units.Count > 0)
                    {
                        m_WaitInstantiateCount += units.Count;
                        for (int iUnit = 0; iUnit < units.Count; iUnit++)
                        {
                            SceneUnitInfo iterUnitInfo = units[iUnit];
                            InstantiateUnit(iterUnitInfo, m_UnitsRoot);
                        }
                    }

                }
            }
        }

        private void DoDestroyAllUnit()
        {
            ObjectUtility.DestroyAllChildern(m_UnitsRoot);
        }

        private void InstantiateUnit(SceneUnitInfo unitInfo, Transform parent)
        {
            if (m_Owner.UseAsset)
            {
                AssetUtil.InstanceAssetAsync(m_AreaDetailInfo.AssetInfos[unitInfo.AssetIndex].AddressableKey, (pathOrAddress, uObj, userData) =>
                {
                    if (uObj != null)
                    {
                        GameObject resultObj = uObj as GameObject;
                        if (resultObj != null)
                        {
                            m_WaitInstantiateCount--;
                            Transform t = resultObj.transform;
                            t.SetParent(m_UnitsRoot);
                            t.localPosition = unitInfo.LocalPosition;
                            t.localRotation = unitInfo.LocalRotation;
                            t.localScale = unitInfo.LocalScale;
                        }
                    }

                });
            }
            else
            {
                m_WaitInstantiateCount--;
                string path = string.Format("{0}/{1}Map_{2}/{3}.prefab"
                    , MapEditorUtility.GetOrCreateMapEditorSetting().AssetExportDirectory
                    , Constants.UNIT_PREFAB_EXPORT_DIRECTORY
                    , m_Owner.MapInfo.Uid
                    , m_AreaDetailInfo.AssetInfos[unitInfo.AssetIndex].AddressableKey);

                GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if(asset == null)
                {
                    string[] paths = AssetDatabase.FindAssets(m_AreaDetailInfo.AssetInfos[unitInfo.AssetIndex].AddressableKey);
                    if (paths != null && paths.Length > 0)
                    {
                        path = AssetDatabase.GUIDToAssetPath(paths[0]);
                        asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    }
                }

                if(asset != null)
                {
                    Transform t = Instantiate(asset).transform;
                    t.SetParent(parent);
                    t.localPosition = unitInfo.LocalPosition;
                    t.localRotation = unitInfo.LocalRotation;
                    t.localScale = unitInfo.LocalScale;
                }
                
            }
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
                    AreaPreview areaPreview = selectedGameObject.GetComponent<AreaPreview>();
                    if (areaPreview != null)
                    {
                        List<GUIContent> contentList = new List<GUIContent>();
                        contentList.Add(new GUIContent("生成所有Unit"));
                        contentList.Add(new GUIContent("删除所有Unit"));
                        RefreshAreaPreviewUI(contentList, areaPreview);

                    }
                }
            }
        }

        /// <summary>
        /// 刷新右键MapPreview的界面
        /// </summary>
        private void RefreshAreaPreviewUI(List<GUIContent> contentList, AreaPreview areaPreview)
        {
            if (areaPreview == null)
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
                        case 0://生成所有Unit
                            areaPreview.InstantiateAllUnit = true;
                            break;
                        case 1://删除所有Unit
                            areaPreview.DestroyAllUnit = true;
                            break;
                    }
                }, userData);
            Event.current.Use();
        }
    }
    #endregion
}
#endif