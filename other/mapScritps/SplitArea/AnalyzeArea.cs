#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EditorExtend;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System;
using Leyoutech.Utility;
using System.Text;
using BatchRendering;

namespace Map
{
    public class AnalyzeArea
    {
        public enum AreaLayer
        {
            SmallLayer,
            MediumLayer,
            BigLayer,
            Resident//常驻层
        }
        private EditorSplitAreaSetting m_EditorSplitAreaSetting;
        private List<EditorSplitAreaLayerSetting> m_EditorSplitAreaLayerSettings;

        private IEnumerator m_AnalyzeEnumerator;

        public void Start(Map map,AreaSpawner areaSpawner)
        {
            InitInfo(map,areaSpawner);
            m_AnalyzeEnumerator = DoAnalyzeArea(map, areaSpawner);
            EditorApplication.update += OnUpdate;
        }

        public IEnumerator StartGenerate(Map map,AreaSpawner areaSpawner)
        {
            InitInfo(map, areaSpawner);
            IEnumerator analyzeAreaEnumerator = DoAnalyzeArea(map, areaSpawner,false);
            while (analyzeAreaEnumerator.MoveNext())
            {
                yield return null;
            }
        }

        private void InitInfo(Map map,ulong areaUid)
        {
            string settingPath = string.Format("{0}/SplitArea_{1}_{2}.asset", map.GetOwnerAreaPath(), map.Uid, areaUid);
            m_EditorSplitAreaSetting = AssetDatabase.LoadAssetAtPath<EditorSplitAreaSetting>(settingPath);
            if (m_EditorSplitAreaSetting == null)
            {
                m_EditorSplitAreaSetting = new EditorSplitAreaSetting();
                AssetDatabase.CreateAsset(m_EditorSplitAreaSetting, settingPath);
                m_EditorSplitAreaSetting = AssetDatabase.LoadAssetAtPath<EditorSplitAreaSetting>(settingPath);
            }
            if (m_EditorSplitAreaSetting == null)
            {
                Debug.LogError("创建区域导出配置失败");
                return;
            }
            m_EditorSplitAreaLayerSettings = m_EditorSplitAreaSetting.m_EditorSplitAreaLayerSettings;
            if (m_EditorSplitAreaLayerSettings == null)
            {
                m_EditorSplitAreaLayerSettings = new List<EditorSplitAreaLayerSetting>();
                m_EditorSplitAreaSetting.m_EditorSplitAreaLayerSettings = m_EditorSplitAreaLayerSettings;
            }
        }

        private void InitInfo(Map map,AreaSpawner areaSpawner)
        {
            InitInfo(map,areaSpawner.m_AreaUid);
        }
        
        private IEnumerator DoAnalyzeArea(Map map, AreaSpawner areaSpawner,bool showDialog = true)
        {
            string areaName = areaSpawner.GetAreaScenePath();
            Area exportArea = null;
            Scene areaScene = EditorSceneManager.OpenScene(areaName, OpenSceneMode.Additive);
            if (areaScene != null)
            {
                SceneManager.SetActiveScene(areaScene);
                GameObject[] rootObjArray = areaScene.GetRootGameObjects();
                if (rootObjArray != null && rootObjArray.Length > 0)
                {
                    for (int index = 0; index < rootObjArray.Length; index++)
                    {
                        SceneManager.MoveGameObjectToScene(rootObjArray[index], map.GetOwnerScene());
                        Area area = rootObjArray[index].GetComponent<Area>();
                        if (area != null)
                        {
                            exportArea = area;
                        }
                    }

                }
                EditorSceneManager.CloseScene(areaScene, true);
            }
            yield return null;
            if (exportArea != null)
            {
                areaSpawner.SetArea(exportArea);
                IEnumerator areaSpawnerEnumerator = areaSpawner.DoUpdate(map, true);
                while (areaSpawnerEnumerator.MoveNext())
                {
                    yield return null;
                }
                //TODO: 1、计算area的直径  2、计算所有unit的直径 3、算出推荐格子大小 对应层AABB范围
                //如果大于等于Area直径的90%以上的unit数量大于0 算一个层
                Bounds areaAABB = exportArea.GetAABB();
                float areaDiameter = Mathf.Max(areaAABB.size.x, areaAABB.size.y, areaAABB.size.z);
                float consultDiameter = areaDiameter * 0.9f;
                CalcuateLayer(map, exportArea, areaDiameter);
                EditorUtility.SetDirty(m_EditorSplitAreaSetting);
                GameObject.DestroyImmediate(exportArea.gameObject);
            }
            EditorUtility.ClearProgressBar();
            if(showDialog)
            {
                EditorUtility.DisplayDialog("Analyze", "分析成功", "OK");
            }
            AssetDatabase.Refresh();
        }

        struct UnitInfo
        {
            public int InstanceId;
            public Bounds AABB;
            public GameObject Unit;
        }
        private List<ParticleSystem> m_ParticleSystems = new List<ParticleSystem>();
        
        private void CalcuateAllPrefab(GameObject obj, Dictionary<int, UnitInfo> unitInfos)
        {
            if (obj == null)
            {
                return;
            }

            if (!obj.activeSelf || obj.hideFlags == HideFlags.DontSave)
            {
                return;
            }
            if (obj.GetComponent<SemaphoreMark>() || obj.GetComponent<IRootMark>()
                   || obj.GetComponent<TreasureInfoMark>())
            {
                return;
            }
            MapEditorUtility.SelectChildParticleSystem(obj);
            GameObject sourcePrefab = UnityEditor.PrefabUtility.GetCorrespondingObjectFromSource(obj); ;
            if (sourcePrefab == null)
            {
                //TODO:继续往子节点进行遍历
                int childCount = obj.transform.childCount;
                if (childCount > 0)
                {
                    for (int iChild = 0; iChild < childCount; iChild++)
                    {
                        CalcuateAllPrefab(obj.transform.GetChild(iChild).gameObject, unitInfos);
                    }
                }
            }
            else
            {
                UnitInfo unitInfo = new UnitInfo();
                unitInfo.InstanceId = obj.GetInstanceID();
                unitInfo.Unit = obj;
                MapEditorUtility.CaculateAABB(obj.transform,ref unitInfo.AABB);
                unitInfos.Add(unitInfo.InstanceId, unitInfo);
            }
        }
        

        private void CalcuateLayer(Map map, Area exportArea, float areaDiameter)
        {
            Dictionary<int, UnitInfo> unitInfos = new Dictionary<int, UnitInfo>();
            CalcuateAllPrefab(exportArea.gameObject, unitInfos);
            List<GameObject> unitObjs = new List<GameObject>();
            foreach(var unitInfo in unitInfos)
            {
                unitObjs.Add(unitInfo.Value.Unit);
            }

            if(unitObjs != null && unitObjs.Count>0)
            {
                float totalDiameter = 0f;
                unitObjs.Sort((unit1,unit2) =>
                {
                    UnitInfo info1 = unitInfos[unit1.GetInstanceID()];
                    UnitInfo info2 = unitInfos[unit2.GetInstanceID()];
                    Bounds unit1AABB = info1.AABB;
                    float unit1Diameter = Mathf.Max(unit1AABB.size.x, unit1AABB.size.y, unit1AABB.size.z);
                    Bounds unit2AABB = info2.AABB;
                    float unit2Diameter = Mathf.Max(unit2AABB.size.x, unit2AABB.size.y, unit2AABB.size.z);
                    return unit1Diameter.CompareTo(unit2Diameter);

                });
                for (int iUnit =0;iUnit<unitObjs.Count;iUnit++)
                {
                    GameObject unit = unitObjs[iUnit];
                    UnitInfo unitInfo = unitInfos[unit.GetInstanceID()];
                    Bounds unitAABB = unitInfo.AABB;
                    EditorUtility.DisplayProgressBar("analyze", unit.name, iUnit / unitObjs.Count);
                    float unitDiameter = Mathf.Max(unitAABB.size.x, unitAABB.size.y, unitAABB.size.z);
                    totalDiameter += unitDiameter;
                }

                m_EditorSplitAreaLayerSettings.Clear();

                float avergeDiameter = totalDiameter / unitObjs.Count;

                EditorSplitAreaLayerSetting layerInfo = new EditorSplitAreaLayerSetting();
                layerInfo.m_MaxAABBSize = areaDiameter;
                layerInfo.m_MinAABBSize = avergeDiameter;
                layerInfo.m_LayerName = AreaLayer.BigLayer.ToString();
                layerInfo.m_Priority = (int)AreaLayer.BigLayer;
                layerInfo.m_GridSize = Mathf.Max(Mathf.CeilToInt(areaDiameter), 1);


                float avergeHalfDiameter = avergeDiameter / 3;
                float halfDiameter = avergeDiameter - 2 * avergeHalfDiameter;
                EditorSplitAreaLayerSetting layerInfoMedium = new EditorSplitAreaLayerSetting();
                layerInfoMedium.m_MaxAABBSize = avergeDiameter;
                layerInfoMedium.m_MinAABBSize = halfDiameter;
                layerInfoMedium.m_LayerName = AreaLayer.MediumLayer.ToString();
                layerInfoMedium.m_Priority = (int)AreaLayer.MediumLayer;
                layerInfoMedium.m_GridSize = Mathf.Max(Mathf.CeilToInt(avergeDiameter), 1);

                EditorSplitAreaLayerSetting layerInfoSmall = new EditorSplitAreaLayerSetting();
                layerInfoSmall.m_MaxAABBSize = halfDiameter;
                layerInfoSmall.m_MinAABBSize = 0;
                layerInfoSmall.m_LayerName = AreaLayer.SmallLayer.ToString();
                layerInfoSmall.m_Priority = (int)AreaLayer.SmallLayer;
                layerInfoSmall.m_GridSize = Mathf.Max(Mathf.CeilToInt(halfDiameter), 1);


                float layer1MaxDiameter = -1f;//层1中的unit最大直径
                float layer2MaxDiameter = -1f;//层2中的unit最大直径
                float layer3MaxDiameter = -1f;//层3中的unit最大直径
                                              //TODO:计算27宫格偏移
                for (int iUnit = 0; iUnit < unitObjs.Count; iUnit++)
                {
                    GameObject unit = unitObjs[iUnit];

                    Bounds unitAABB = unitInfos[unit.GetInstanceID()].AABB;
                    EditorUtility.DisplayProgressBar("analyze", unit.name, iUnit / unitObjs.Count);
                    float unitDiameter = Mathf.Max(unitAABB.size.x, unitAABB.size.y, unitAABB.size.z);
                    if (unitDiameter > avergeDiameter && unitDiameter <= areaDiameter)//big layer
                    {
                        if (layer1MaxDiameter < unitDiameter)
                        {
                            layer1MaxDiameter = unitDiameter;
                        }
                    }
                    else if (unitDiameter > halfDiameter && unitDiameter <= avergeDiameter)//medium layer
                    {
                        if (layer2MaxDiameter < unitDiameter)
                        {
                            layer2MaxDiameter = unitDiameter;
                        }
                    }
                    else if (unitDiameter > 0 && unitDiameter <= halfDiameter)//small layer
                    {
                        if (layer3MaxDiameter < unitDiameter)
                        {
                            layer3MaxDiameter = unitDiameter;
                        }
                    }
                }


                float halfTanCameraFov = RendererUtility.CaculateHalfTanCameraFov(map.ExpectedFov);
                float toCameraDistance = RendererUtility.CacluateToCameraDistance(layer1MaxDiameter, m_EditorSplitAreaSetting.m_Rate, halfTanCameraFov);
                toCameraDistance = Mathf.Min(toCameraDistance,exportArea.GetDiameter());
                
                layerInfo.m_Offest = Mathf.Clamp(Mathf.CeilToInt(toCameraDistance / layerInfo.m_GridSize), (int)GridOffest.Min, (int)GridOffest.Max);
                toCameraDistance = RendererUtility.CacluateToCameraDistance(layer2MaxDiameter, m_EditorSplitAreaSetting.m_Rate, halfTanCameraFov);
                layerInfoMedium.m_Offest = Mathf.Clamp(Mathf.CeilToInt(toCameraDistance / layerInfoMedium.m_GridSize), (int)GridOffest.Min, (int)GridOffest.Max);
                toCameraDistance = RendererUtility.CacluateToCameraDistance(layer3MaxDiameter, m_EditorSplitAreaSetting.m_Rate, halfTanCameraFov);
                layerInfoSmall.m_Offest = Mathf.Clamp(Mathf.CeilToInt(toCameraDistance / layerInfoSmall.m_GridSize), (int)GridOffest.Min, (int)GridOffest.Max);
                m_EditorSplitAreaLayerSettings.Add(layerInfo);
                m_EditorSplitAreaLayerSettings.Add(layerInfoMedium);
                m_EditorSplitAreaLayerSettings.Add(layerInfoSmall);
            }
        }
        
     
        private void EndAnalyze()
        {
            m_AnalyzeEnumerator = null;
            EditorApplication.update -= OnUpdate;
        }

        private void OnUpdate()
        {
            bool moveNext;
            try
            {
                moveNext = m_AnalyzeEnumerator.MoveNext();
            }
            catch (AbortExportMapException e)
            {
                moveNext = false;
            }
            catch (System.Exception e)
            {
                moveNext = false;
            }

            if(!moveNext)
            {
                EndAnalyze();
            }
        }
    }
}

#endif