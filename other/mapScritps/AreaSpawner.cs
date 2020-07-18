#if UNITY_EDITOR
using EditorExtend;
using Leyoutech.Utility;
using Map;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Map
{
    [ExecuteInEditMode]
    public class AreaSpawner : MonoBehaviour
    {
        /// <summary>
        /// 地图ID
        /// </summary>
        public Map m_Map;
        /// <summary>
        /// Area Uid
        /// </summary>
        [ReadOnly]
        public ulong m_AreaUid;

        /// <summary>
        /// 当前引用的Area
        /// </summary>
        public Area m_Area;
        /// <summary>
        /// Area场景名称
        /// </summary>
        [ReadOnly]
        private string m_AreaScenePath;
        /// <summary>
        /// Area的AABB
        /// </summary>
        [ReadOnly]
        public Bounds m_AABB;
        /// <summary>
        /// 这个Area导出的AreaInfo
        /// </summary>
        [ReadOnly]
        public AreaInfo _AreaInfo;
        /// <summary>
        /// 这个Area导出的AreaDetailInfo
        /// </summary> 
        [ReadOnly]
        public AreaDetailInfo _AreaDetailInfo;
        /// <summary>
        /// 这个Area下的Voxel
        /// </summary>
        [ReadOnly]
        public VoxelGrid VoxelGrid;
        /// <summary>
        /// Area的直径
        /// </summary>
        [ReadOnly]
        public float m_Diameter;

        private MeshRenderer[] m_renders;
        [Tooltip("区域位置")]
        [ReadOnly]
        public Vector3 m_AreaPosition;

        [ReadOnly]
        public Quaternion m_AreaRotation;
        /// <summary>
        /// 当前Obj的名字
        /// </summary>
        private string m_areaSpawnObjName;

        //[EditorExtend.Button("导入Area", "LoadArea", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)]
        //public bool _LoadAreaTag;

        //[EditorExtend.Button("保存Area", "SaveArea", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)]
        //public bool _SaveAreaTag;

        //[EditorExtend.Button("删除", "DelArea", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)]
        //public bool _DelAreaTag;

        //[EditorExtend.Button("导出区域", "ExportArea", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)]
        //public bool _ExportArea;

        protected void OnEnable()
        {
            if (Application.isPlaying)
            {
                return;
            }
            m_renders = gameObject.GetComponentsInChildren<MeshRenderer>();
            //找AreaScene路径 因为名字美术会改的
            InitAreaScenePath();
            // m_AreaScenePath = string.Format("{0}/Area_{1}_{2}.unity", m_Map.GetOwnerAreaPath(), m_Map.Uid, m_AreaUid);
        }

        /// <summary>
        /// 初始化查找AreaScene的路径
        /// </summary>
        private void InitAreaScenePath()
        {
            if (m_Map == null)
            {
                return;
            }
            string areaPath = m_Map.GetOwnerAreaPath();
            if (string.IsNullOrEmpty(areaPath))
            {
                return;
            }
            //找AreaScene路径 因为名字美术会改的
            string[] resAssets = AssetDatabase.FindAssets(string.Format("{0} t:Scene", m_AreaUid), new string[] { areaPath });
            if (resAssets != null && resAssets.Length > 0)
            {
                string assetPath = resAssets[0];
                for (int iRes = 0; iRes < resAssets.Length; iRes++)
                {
                    string resPath = AssetDatabase.GUIDToAssetPath(resAssets[iRes]);
                    string[] resSplit = resPath.Split('/');
                    if (resSplit != null && resSplit.Length > 0)
                    {
                        string lastName = resSplit[resSplit.Length - 1];
                        string[] lastNameSplit = lastName.Split('_');
                        if (lastNameSplit != null && lastNameSplit.Length > 0)
                        {
                            if (lastNameSplit[lastNameSplit.Length - 1].Equals(string.Format("{0}.unity", m_AreaUid)))
                            {
                                assetPath = resAssets[iRes];
                                break;
                            }
                        }
                    }
                }
                m_AreaScenePath = AssetDatabase.GUIDToAssetPath(assetPath);
            }
        }

        public Quaternion GetAreaRotation()
        {
            return m_AreaRotation;
        }

        public string GetAreaSpawnObjName()
        {
            return m_areaSpawnObjName;
        }

        public Vector3 GetAreaPosition()
        {
            return m_AreaPosition;
        }

        public ulong GetAreaId()
        {
            return m_AreaUid;
        }

        public float GetDiameter()
        {
            if (m_Area != null)
            {
                m_Diameter = m_Area.GetDiameter();
            }
            return m_Diameter;
        }

        public Area GetArea()
        {
            return m_Area;
        }

        public Bounds GetAABB()
        {
            if (m_Area != null)
            {
                m_AABB = m_Area.GetAABB();
            }
            return m_AABB;
        }

        public void SetArea(Area area)
        {
            m_Area = area;
        }

        /// <summary>
        /// 导入Area
        /// </summary>
        public void LoadArea(bool closeOther = true,bool loadRecast = false)
        {
            m_Map.SwitchSpawnerArea(this, closeOther, loadRecast);
        }
        

        public void ExportArea()
        {
            ExportData exportData = new ExportData();
            exportData.m_Map = m_Map;
            exportData.m_ExportParameter = MapEditorUtility.CreateExportParameter();
            exportData.m_AreaSpawners = new List<AreaSpawner>() { this };
            SplitAreaWindow.OpenWindow(exportData);
            //老版导出设置
            //new Exporter().BeginExport(m_Map, m_Map.ExportSetting, new ExportParameter(),new List<AreaSpawner>() { this});
        }
        /// <summary>
        /// 删除AreaSpawner
        /// </summary>
        public void DelArea()
        {
            bool confirm = EditorUtility.DisplayDialog("提示", "是否删除该Area", "确定", "取消");
            if (confirm)
            {
                m_Map.RemoveSpawnerArea(this);
            }
        }

        public void SaveArea()
        {
            m_Map.SaveArea(this);
        }


        public string GetAreaScenePath()
        {
            return m_AreaScenePath;
        }

        public void Create(Map map, Area area = null, ulong areaId = 0)
        {
            m_Map = map;
            if (area != null)
            {
                m_Area = area;
                m_AABB = m_Area.GetAABB();
                VoxelGrid = m_Area.VoxelGrid;
                m_Diameter = m_Area.GetDiameter();
            }
            else
            {
                GameObject areaObj = new GameObject(string.Format("Area_{0}", areaId));
                m_Area = areaObj.AddComponent<Area>();
                m_Area.Uid = areaId;
            }
            m_AreaUid = m_Area.Uid;
            m_AreaScenePath = string.Format("{0}/Area_{1}_{2}.unity", m_Map.GetOwnerAreaPath(), m_Map.Uid, m_AreaUid);
            gameObject.name = string.Format("AreaSpawner_{0}_{1}", m_Map.Uid, m_AreaUid);
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            if (scene != null)
            {
                SceneManager.MoveGameObjectToScene(m_Area.gameObject, scene);
                SceneManager.SetActiveScene(scene);
                EditorSceneManager.SaveScene(scene, m_AreaScenePath);
                EditorSceneManager.UnloadSceneAsync(scene);
            }
            EditorSceneManager.CloseScene(scene, true);
        }


        public void Clear()
        {
            m_Map = null;
            if (!string.IsNullOrEmpty(m_AreaScenePath))
            {
                AssetDatabase.DeleteAsset(m_AreaScenePath);
            }

            if (m_Area != null)
            {
                GameObject.DestroyImmediate(m_Area.gameObject);
                m_Area = null;
            }
        }

        /// <summary>
        /// 条件是否满足 能否导出
        /// </summary>
        /// <returns></returns>
        public bool CheckCanExport()
        {
            Vector3 voxelCounts = VoxelGrid.GetVoxelCounts();
            int voxelCount = (int)(voxelCounts.x
                    * voxelCounts.y
                    * voxelCounts.z);
            if (voxelCount <= 0)
            {
                return false;
            }

            //string settingPath = string.Format("{0}/SplitArea_{1}_{2}.asset", m_Map.GetOwnerAreaPath(), m_Map.Uid, this.m_AreaUid);
            //EditorSplitAreaSetting setting = AssetDatabase.LoadAssetAtPath<EditorSplitAreaSetting>(settingPath);
            //if (setting == null)
            //{
            //    return false;
            //}
            return true;
        }
        

        public IEnumerator DoUpdate(Map map, bool isExporting)
        {
            InitAreaScenePath();
            yield return null;
            yield return null;
            if (m_renders != null && m_renders.Length > 0)
            {
                for (int iRender = 0; iRender < m_renders.Length; iRender++)
                {
                    m_renders[iRender].enabled = map.debugShowStarMap;
                    if (iRender % 5 == 0)
                    {
                        yield return null;
                    }
                }
            }
            yield return null;
            m_Map = map;
            if (m_Area == null)
            {
                List<Area> m_areaList = m_Map.GetAreaCache();
                m_areaList.Clear();
                Area[] areaArray = Object.FindObjectsOfType<Area>();
                if (areaArray != null && areaArray.Length > 0)
                {
                    for (int iArea = 0; iArea < areaArray.Length; iArea++)
                    {
                        if (areaArray[iArea].Uid == m_AreaUid)
                        {
                            m_areaList.Add(areaArray[iArea]);
                        }
                    }
                }
                if (m_areaList.Count == 1)
                {
                    m_Area = m_areaList[0];
                }
                //DebugUtility.Assert(m_areaList.Count <= 1, string.Format("Area错误，找到{0}个uid为{1}的Area",m_areaList.Count,m_AreaUid));
                if (m_areaList.Count > 1)
                {
                    Debug.LogError(string.Format("Area错误，找到{0}个uid为{1}的Area", m_areaList.Count, m_AreaUid));
                }
                m_areaList.Clear();
                yield return null;
            }
            if (m_Area != null)
            {
                m_AreaUid = m_Area.Uid;
                m_AreaPosition = m_Area.transform.position;
                m_AreaRotation = m_Area.transform.rotation;
                if (isExporting)
                {
                    ParticleSystem[] particleSystems = m_Area.GetComponentsInChildren<ParticleSystem>();
                    ObjectUtility.SelectionComponent(particleSystems);
                    yield return null;

                    for (int iParticle = 0; iParticle < particleSystems.Length; iParticle++)
                    {
                        particleSystems[iParticle].Simulate(Constants.PARTICLE_SIMULATE_TIME_WHEN_EXPOTEING);
                    }
                    yield return null;
                }
            }
            //m_AreaScenePath = string.Format("{0}/Area_{1}_{2}.unity", m_Map.GetOwnerAreaPath(), m_Map.Uid, m_AreaUid);
            m_areaSpawnObjName = gameObject.name;
            if (!m_areaSpawnObjName.Contains(m_AreaUid.ToString()))
            {
                m_areaSpawnObjName = string.Format("{0}_{1}", m_areaSpawnObjName, m_AreaUid);
            }
            gameObject.name = m_areaSpawnObjName;

            transform.localScale = m_Diameter * Vector3.one;
            //这个节点上不需要碰撞盒
            Collider collider = GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = false;
            }
            if (m_Area != null)
            {
                IEnumerator areaEnumerator = m_Area.DoUpdate_Co(map, -1, isExporting, this);
                while (m_Area != null && m_Area.gameObject != null &&areaEnumerator.MoveNext())
                {
                    if (!isExporting)
                    {
                        yield return null;
                    }
                }
                if (m_Area != null)
                {
                    m_AABB = m_Area.GetAABB();
                    m_Diameter = m_Area.GetDiameter();
                    VoxelGrid = m_Area.VoxelGrid;
                }

            }
        }
    }
}

#endif