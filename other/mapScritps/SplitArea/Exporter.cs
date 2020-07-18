#if UNITY_EDITOR 
using Leyoutech.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.SceneManagement;
using static Map.LightmapSetting;
using static UnityEngine.ParticleSystem;
using Object = UnityEngine.Object;

namespace Map
{
    public class Exporter
    {
        #region 属性
        private const float ACTION_COUNT = 10.0f;

        /// <summary>
        /// 避免引用同一个Prefab的Unit导出多次Prefab
        /// </summary>
        private HashSet<string> m_ExportedUnitAddressableKeys;
        private IEnumerator m_ExportEnumerator;

        /// <summary>
        /// 计时用的
        /// </summary>
        private System.Diagnostics.Stopwatch m_Stopwatch;
        /// <summary>
        /// 当前的ActionName，Cache下来以便<see cref="EndAction"/>时记录一些信息
        /// </summary>
        private string m_CurrentActionName;
        /// <summary>
        /// 当前执行的Action的Index，用于计算导出进度
        /// </summary>
        private int m_CurrentActionIndex;
        /// <summary>
        /// 当前是否在Actiion中
        /// 这个变量是为了检测调用<see cref="BeginAction"/>后忘记调用<see cref="EndAction"/>
        /// </summary>
        private bool m_IsActioning;
        /// <summary>
        /// 导出中的Map
        /// </summary>
        private Map m_ExportingMap;
        /// <summary>
        /// 导出的设置
        /// </summary>
        private ExportSetting m_ExportSetting;
        /// <summary>
        /// <see cref="Map.ExpectedFov"/>
        /// <see cref="RendererUtility.CaculateHalfTanCameraFov"/>
        /// </summary>
        private float m_HalfTanCameraFov;
        /// <summary>
        /// 导出过程的详细信息
        /// </summary>
        private StringBuilder m_DetailStringBuilder;
        /// <summary>
        /// Map导出的文件夹名（每个Map导出到一个文件夹）
        /// </summary>
        private string m_ExportFolderName;
        /// <summary>
        /// Abort时是否抛异常
        /// </summary>
        private bool m_ThrowExceptionAtAbort;
        private ExporterHandle m_ExporterHandle;


        private List<Component> m_Components = new List<Component>();
        private List<ParticleSystem> m_ParticleCache = new List<ParticleSystem>();

        private UnitInfosSetting m_UnitInfoSetting;
        
        /// <summary>
        /// 临时变量
        /// </summary>
        private Stack<int> m_RendererIndexs = new Stack<int>();
        private List<RendererInfo> m_RendererInfos = new List<RendererInfo>();
        private Dictionary<string, int> m_AssetInfos = new Dictionary<string, int>();
        private Dictionary<string, AreaLayerInfo> m_AreaLayerInfos = new Dictionary<string, AreaLayerInfo>();
        private List<ParticleSystem> m_ParticleSystems = new List<ParticleSystem>();
        private EditorSplitAreaSetting m_EditorSplitAreaSetting;
        #endregion

        #region 生命周期

        private void OnBeginExport(Map map)
        {
            map.enabled = true;
            map.AutoVoxelSize = true;
            if(!map.ExportSetting.OnlyExportMapScene)
            {
                map.ExportSetting.ExportMapScene = true;
            }
        }
        public ExporterHandle BeginExport(Map map
           , ExportSetting exportSetting
           , ExportParameter exportParameter, List<AreaSpawner> areaSpawnerList, bool isAll = false)
        {
            m_ExporterHandle = new ExporterHandle();
            OnBeginExport(map);
           
            if (!map.CheckCanExport())
            {
                m_ExporterHandle.IsDone = false;
                EditorUtility.DisplayDialog("MapEditor", "导出失败!有Area格子数为0", "OK");
                return m_ExporterHandle;
            }
            string[] unitInfoSettingPath = AssetDatabase.FindAssets("t: UnitInfosSetting");
            if(unitInfoSettingPath == null || unitInfoSettingPath.Length<=0)
            {
                UnitInfosSetting.Create();
                unitInfoSettingPath = AssetDatabase.FindAssets("t: UnitInfosSetting");
            }
            if(unitInfoSettingPath != null && unitInfoSettingPath.Length>0)
            {
                m_UnitInfoSetting = AssetDatabase.LoadAssetAtPath<UnitInfosSetting>(AssetDatabase.GUIDToAssetPath(unitInfoSettingPath[0]));
            }
            
            bool canExport = true;
            string areaName = "";
            for (int iArea = 0; iArea < areaSpawnerList.Count; iArea++)
            {
                AreaSpawner areaSpawner = areaSpawnerList[iArea];
                if (!areaSpawner.CheckCanExport())
                {
                    canExport = false;
                    areaName = areaSpawner.GetAreaSpawnObjName();
                    break;
                }
            }

            if (!canExport)
            {
                m_ExporterHandle.IsDone = false;
                EditorUtility.DisplayDialog("MapEditor", string.Format("导出失败!{0}格子数为0||未找到导出配置", areaName), "OK");
                return m_ExporterHandle;
            }



            m_ExportFolderName = Constants.EXPORT_MAP_FOLDER_NAME_STARTWITHS + map.Uid;
            if (IsReadyForExport())
            {
                m_ThrowExceptionAtAbort = exportParameter.ThrowExceptionAtAbort;
                ClearInfo();
                m_DetailStringBuilder = new StringBuilder();
                m_ExportingMap = map;
                m_ExportSetting = exportSetting;

                m_ExportedUnitAddressableKeys = exportParameter.ExportedUnitAddressableKeys == null
                    ? new HashSet<string>()
                    : exportParameter.ExportedUnitAddressableKeys;
                m_ExportEnumerator = ExportSpawnerArea(areaSpawnerList, isAll);
                m_Stopwatch = new System.Diagnostics.Stopwatch();
                m_CurrentActionIndex = -1;
                m_IsActioning = false;
                EditorApplication.update += OnUpdate;
            }
            else
            {
                m_ExporterHandle.IsDone = true;
            }

            return m_ExporterHandle;
        }

        /// <summary>
        /// 是否准备好导出
        /// </summary>
        private bool IsReadyForExport()
        {
            MapEditorSetting setting = MapEditorUtility.GetMapEditorSetting();
            bool isReady = DebugUtility.AssertMust(setting != null, "setting != null");
            isReady &= DebugUtility.AssertMust(!string.IsNullOrWhiteSpace(setting.AssetExportDirectory), "!string.IsNullOrWhiteSpace(setting.AssetExportDirectory)");
            if (isReady)
            {
                string exportDirectory = FileUtility.ConvertProjectPathToAbsolutePath(setting.AssetExportDirectory);
                if (FileUtility.ExistsDirectoryOrCreate(exportDirectory))
                {
                    string exportUnitDirectory = string.Format("{0}/{1}"
                           , exportDirectory
                           , Constants.UNIT_PREFAB_EXPORT_DIRECTORY);
                    isReady = DebugUtility.AssertMust(FileUtility.ExistsDirectoryOrCreate(exportUnitDirectory), string.Format("创建目录({0})失败", exportUnitDirectory));
                    if (isReady)
                    {
                        string exportMapDirectory = string.Format("{0}/{1}"
                           , exportDirectory
                           , m_ExportFolderName);
                        isReady = DebugUtility.AssertMust(FileUtility.ExistsDirectoryOrCreate(exportMapDirectory), string.Format("创建目录({0})失败", exportMapDirectory));
                    }
                }
                else
                {
                    isReady = DebugUtility.AssertMust(false, string.Format("创建目录({0})失败", exportDirectory));
                }
            }
            return isReady;
        }
        
        private void OnUpdate()
        {
            bool moveNext;
            try
            {
                moveNext = m_ExportEnumerator.MoveNext();
            }
            catch (AbortExportMapException e)
            {
                moveNext = false;
                Lightmapping.ForceStop();
                Debug.LogError("Export map be abort");
                if (m_ThrowExceptionAtAbort)
                {
                    throw e;
                }
            }
            catch (System.Exception e)
            {
                moveNext = false;
                Debug.LogError(string.Format("Export map({0}) failed: \n{1}", m_ExportingMap.Uid, e.ToString()));
            }

            if (!moveNext)
            {
                EndExport();
            }
            else
            {
                UpdateProgressBar();
            }
        }

        private void EndExport()
        {
            if(m_UnitInfoSetting != null)
            {
                EditorUtility.SetDirty(m_UnitInfoSetting);
            }
            CleanUnuseAsset();
            if(m_ExportingMap != null)
            {
                m_ExportingMap.EndExportMap();
            }
            Debug.Log(m_DetailStringBuilder.ToString());
            m_DetailStringBuilder.Clear();
            EditorUtility.ClearProgressBar();

            EditorApplication.update -= OnUpdate;
            m_Stopwatch = null;
            m_CurrentActionName = null;
            m_ExportedUnitAddressableKeys.Clear();
            m_ExportedUnitAddressableKeys = null;

            m_ExportingMap = null;

            m_ExporterHandle.IsDone = true;
            System.GC.Collect();
            
        }

        private IEnumerator FindAllUnitName(HashSet<string> allUnits,HashSet<string> commonUnits = null,string exportAssetPath = "")
        {
            //统计用到的unit
            string[] areaDetailInfos = AssetDatabase.FindAssets("t: AreaDetailInfo");
            if (areaDetailInfos != null && areaDetailInfos.Length > 0)
            {
                for (int iArea = 0; iArea < areaDetailInfos.Length; iArea++)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(areaDetailInfos[iArea]);
                    if (!string.IsNullOrEmpty(assetPath))
                    {
                        AreaDetailInfo areaDetailInfo = AssetDatabase.LoadAssetAtPath<AreaDetailInfo>(assetPath);
                        if (areaDetailInfo != null)
                        {
                            uint mapId = uint.MinValue;
                            string detailName = areaDetailInfo.name;
                            if (!string.IsNullOrEmpty(detailName))
                            {
                                string[] nameArray = detailName.Split('_');
                                if (nameArray != null && nameArray.Length == 3)
                                {
                                    mapId = uint.Parse(nameArray[1]);
                                }
                            }

                            AssetInfo[] assetInfos = areaDetailInfo.AssetInfos;
                            if (assetInfos != null && assetInfos.Length > 0)
                            {
                                bool merge = !string.IsNullOrEmpty(exportAssetPath);
                                for (int iAsset = 0; iAsset < assetInfos.Length; iAsset++)
                                {
                                    string addressableKey = assetInfos[iAsset].AddressableKey;
                                    if (merge && mapId != uint.MinValue)
                                    {
                                        EditorUtility.DisplayProgressBar("提示", addressableKey, iAsset * 1.0f / assetInfos.Length);
                                        string[] paths = AssetDatabase.FindAssets(string.Format("t:Prefab {0}", addressableKey), new string[] { exportAssetPath });
                                        if (paths != null && paths.Length == 1)
                                        {
                                            string originPath = MapEditorUtility.GetFullPath(AssetDatabase.GUIDToAssetPath(paths[0]));
                                            string unitFold = string.Format("{0}/{1}Map_{2}", exportAssetPath
                                                , Constants.UNIT_PREFAB_EXPORT_DIRECTORY
                                                , mapId);
                                            string targetPath = MapEditorUtility.GetFullPath(string.Format("{0}/{1}.prefab"
                                                    , unitFold
                                                    , addressableKey));
                                            if (originPath != targetPath)
                                            {
                                                if (File.Exists(originPath))
                                                {
                                                    File.Move(originPath, targetPath);
                                                    yield return null;
                                                }
                                            }

                                        }
                                    }
                                    if (!allUnits.Add(addressableKey))
                                    {
                                        if (commonUnits != null)
                                        {
                                            commonUnits.Add(addressableKey);
                                        }
                                    }
                                }
                            }
                            
                        }
                    }
                    
                   
                    
                }
            }
            if(!string.IsNullOrEmpty(exportAssetPath))
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private void ClearInfo()
        {
            m_RendererIndexs.Clear();
            m_RendererInfos.Clear();
            m_AssetInfos.Clear();
            m_AreaLayerInfos.Clear();
        }

        /// <summary>
        /// 查找是否有共用unit 有的话 移到公共目录中
        /// </summary>
        /// <returns></returns>
        public IEnumerator FindCommonUnits()
        {
            HashSet<string> allUnits = new HashSet<string>();
            HashSet<string> commonUnits = new HashSet<string>();

            MapEditorSetting mapEditorSetting = MapEditorUtility.GetMapEditorSetting();
           
            IEnumerator findEnumerator = FindAllUnitName(allUnits, commonUnits, mapEditorSetting.AssetExportDirectory);
            while(findEnumerator.MoveNext())
            {
                yield return null;
            }
            if (commonUnits == null || commonUnits.Count <= 0)
            {
                yield break;
            }
            yield return null;

            string originAssetPath = string.Format("{0}/{1}"
                        , mapEditorSetting.AssetExportDirectory
                        , Constants.UNIT_PREFAB_EXPORT_DIRECTORY);
            string exportAssetPath = MapEditorUtility.GetFullPath(originAssetPath);
            HashSet<string>.Enumerator iter = commonUnits.GetEnumerator();
            int unitIndex = 0;
            while (iter.MoveNext())
            {
                EditorUtility.DisplayProgressBar("merge","", (++unitIndex) * 1.0f / commonUnits.Count);
                string[] unitFiles = Directory.GetFiles(exportAssetPath, string.Format("{0}.prefab", iter.Current), SearchOption.AllDirectories);
                if (unitFiles != null && unitFiles.Length > 0)
                {
                    UTF8Encoding utf8 = new UTF8Encoding(false);
                    for (int iFile = 0; iFile < unitFiles.Length; iFile++)
                    {
                        string unitFile = unitFiles[iFile];
                        if (unitFile.EndsWith(".prefab"))
                        {
                            string[] unitNames = unitFile.Split('\\');
                            if(unitNames != null && unitNames.Length>0)
                            {
                                string unitName = unitNames[unitNames.Length-1];
                                unitName = unitName.Replace(".prefab", "");
                                if (commonUnits.Contains(unitName) && File.Exists(unitFile))
                                {
                                    string targetFile = string.Format("{0}/{1}.prefab", originAssetPath, unitName);
                                    if (!unitFile.Equals(targetFile))
                                    {
                                        File.Move(unitFile, targetFile);
                                    }
                                }
                            }
                            
                        }
                        yield return null;
                    }
                }
            }
            AssetDatabase.Refresh();
            EditorUtility.ClearProgressBar();
        }

        /// <summary>
        /// 清除不必要的unit
        /// </summary>
        public IEnumerator ClearUnUsedUnit(uint mapId = uint.MinValue)
        {
            HashSet<string> allUnits = new HashSet<string>();
            IEnumerator findEnumerator = FindAllUnitName(allUnits);
            while (findEnumerator.MoveNext())
            {
                yield return null;
            }
            yield return null;
            MapEditorSetting mapEditorSetting = MapEditorUtility.GetMapEditorSetting();
            string exportAssetPath = string.Format("{0}/{1}"
                        , mapEditorSetting.AssetExportDirectory
                        , Constants.UNIT_PREFAB_EXPORT_DIRECTORY);
            if(mapId != uint.MinValue)
            {
                exportAssetPath += string.Format("Map_{0}",mapId);
            }
            exportAssetPath = MapEditorUtility.GetFullPath(exportAssetPath);

            string[] unitFiles = Directory.GetFiles(exportAssetPath,"*.prefab", SearchOption.AllDirectories);

            if (unitFiles != null && unitFiles.Length > 0)
            {
                UTF8Encoding utf8 = new UTF8Encoding(false);
                for (int iFile = 0; iFile < unitFiles.Length; iFile++)
                {
                    string unitFile = unitFiles[iFile];
                    if (unitFile.EndsWith(".prefab"))
                    {
                        string unitName = unitFile.Replace(exportAssetPath, "");
                        unitName = unitName.Replace(".prefab","");
                        EditorUtility.DisplayProgressBar("check", unitName, iFile*1.0f / unitFiles.Length);
                        string[] unitNames = unitName.Split('\\');
                        if (unitNames != null && unitNames.Length>0 &&
                            !allUnits.Contains(unitNames[unitNames.Length-1]) && File.Exists(unitFile))
                        {
                            File.Delete(unitFile);
                        }
                    }
                    yield return null;
                }
            }
            AssetDatabase.Refresh();
            EditorUtility.ClearProgressBar();
        }

        private void BeginAction(string actionName)
        {
            DebugUtility.Assert(!m_IsActioning, "!m_IsActioning");
            m_IsActioning = true;
            m_CurrentActionName = actionName;
            m_CurrentActionIndex++;
            UpdateProgressBar();
            m_Stopwatch.Restart(); 
        } 
        /// <summary>
        /// 之所以需要一个这个方法，是为了在导出过程中可以随时Abort
        /// </summary>
        private void UpdateProgressBar()
        {
            if (EditorUtility.DisplayCancelableProgressBar("Export Map", m_CurrentActionName, m_CurrentActionIndex / ACTION_COUNT))
            {
                EndExport();
                throw new AbortExportMapException();
            }
        }

        private void EndAction()
        {
            m_Stopwatch.Stop();
            m_DetailStringBuilder.AppendLine(string.Format("Execute ({0}) for {1} ms", m_CurrentActionName, m_Stopwatch.ElapsedMilliseconds));
            m_IsActioning = false;
        }
        #endregion

        #region util函数
        
        private int SortLayerByPriority(EditorSplitAreaLayerSetting x, EditorSplitAreaLayerSetting y)
        {
            return x.m_Priority.CompareTo(y.m_Priority);
        }

        private EditorSplitAreaLayerSetting CalcuateIncludeLayer(List<EditorSplitAreaLayerSetting> worldLayer, float diameter)
        {
            for (int iWorld = worldLayer.Count - 1; iWorld >= 0; iWorld--)
            {
                EditorSplitAreaLayerSetting layerInfo = worldLayer[iWorld];
                if (diameter >= layerInfo.m_MinAABBSize && diameter <= layerInfo.m_MaxAABBSize)
                {
                    return layerInfo;
                }
            }
            return worldLayer[worldLayer.Count - 1];
        }
        
        /// <summary>
        /// 计算所在的层和格子
        /// </summary>
        private IEnumerator CalcuateInLayerGrid(Vector3 centerPos, float diameter, AreaLayerInfo areaLayerInfo, int unitIndex)
        {
            if (unitIndex < 0)
            {
                Debug.LogError("未找到unitIndex");
                yield break;
            }
           
            float gridSize = areaLayerInfo.m_GridSize;
           
            Vector3 minGridIndex = MathUtility.EachFloorToInt((centerPos - Vector3.one * diameter / 2) / gridSize);
            Vector3 maxGridIndex = MathUtility.EachFloorToInt((centerPos + Vector3.one * diameter / 2) / gridSize);
            int index = 0;
            for (int iGridX = (int)minGridIndex.x; iGridX <= maxGridIndex.x; iGridX++)
            {
                for (int iGridY = (int)minGridIndex.y; iGridY <= maxGridIndex.y; iGridY++)
                {
                    for (int iGridZ = (int)minGridIndex.z; iGridZ <= maxGridIndex.z; iGridZ++)
                    {
                        //添加虚拟格子索引
                        Vector3Int gridIndex = new Vector3Int(iGridX, iGridY, iGridZ);
                        long hashCode = AreaLayerInfo.GetHashCode(gridIndex);
                        AreaVirtualGridInfo gridInfo;

                        if (areaLayerInfo.AreaVirtualGridIndexCache.Add(hashCode))
                        {
                            gridInfo = CreateVirtualGridInfo(gridSize, iGridX, iGridY, iGridZ, hashCode, areaLayerInfo);
                        }
                        else
                        {
                            gridInfo = areaLayerInfo.AreaVirtualGridInfoCache[hashCode];
                        }

                        if (gridInfo.m_UnitIndexCache.Add(unitIndex))
                        {
                            gridInfo.m_UnitIndexs.Add(unitIndex);
                        }
                        areaLayerInfo.AreaVirtualGridInfoCache[hashCode] = gridInfo;
                        if((++index)%1000 ==0)
                        {
                            yield return null;
                        }
                    }
                }
            }
           
        }

        public float GetHalfTanCameraFov()
        {
            return m_HalfTanCameraFov;
        }

        public StringBuilder GetDetailStringBuilder()
        {
            return m_DetailStringBuilder;
        }

        /// <summary>
        /// 按Uid升序
        /// </summary>
        private int SortAreaInfoByUid(AreaInfo x, AreaInfo y)
        {
            return (int)x.Uid - (int)y.Uid;
        }

        #endregion

        #region 烘焙相关
        private IEnumerator Bake()
        {
            //先烘焙后面的逻辑跟之前的一样
            if (m_ExportingMap.OpenLightBake)
            {
                BeginAction("Load All Area");
                IEnumerator loadAreaEnumerator = m_ExportingMap.LoadAllArea();
                while (loadAreaEnumerator.MoveNext())
                {

                }
                EndAction();
                yield return null;
                BeginAction("Lightmapping bake");
				//Lightmapping.Clear();
				Lightmapping.BakeAsync();
				while(Lightmapping.buildProgress<1f)
				{
					yield return null;
				}
				EndAction();
                yield return null;
				BeginAction("ClearLightMapSetting");
				ClearLightMapSetting();
				EndAction();
				yield return null;
				BeginAction("SaveLightMapSetting");
				//保存光照信息
				SaveLightMapSetting();
				EndAction();
				yield return null;
            }
            yield return null;
            //烘焙完后 要把Area保存 因为上面保存着光照信息 留给导出用
            m_ExportingMap.SaveOtherArea();
            yield return null;
        }

        /// <summary>
        /// 清除光照贴图信息
        /// </summary>
        private void ClearLightMapSetting()
        {
            LightmapSetting[] lightMaps = Transform.FindObjectsOfType<LightmapSetting>();
            if (lightMaps == null || lightMaps.Length <= 0)
            {
                return;
            }

            for (int iLight = lightMaps.Length - 1; iLight >= 0; iLight--)
            {
                LightmapSetting lightSetting = lightMaps[iLight];
                if (lightSetting == null)
                {
                    continue;
                }

                Object.DestroyImmediate(lightSetting);
            }
        }

        /// <summary>
        /// 保存光照贴图信息（因为导出时，会丢失光照信息，所以通过个脚本记录下）
        /// </summary>
        private void SaveLightMapSetting()
        {
            //TODO:每个预设保存是否需要生成lightmapsetting
            List<GameObject> prefabList = new List<GameObject>();
            Area[] areas = Object.FindObjectsOfType<Area>();
            if(areas != null && areas.Length>0)
            {
                for(int iArea =0;iArea<areas.Length;iArea++)
                {
                    MapEditorUtility.GetAllPrefab(areas[iArea].transform, prefabList);
                }
            }

            if(prefabList != null&& prefabList.Count>0)
            {
                List<LightInfo> lightInfos = new List<LightInfo>();
                Stack<int> rendererIndexs = new Stack<int>();
                for (int iPrefab =0;iPrefab<prefabList.Count;iPrefab++)
                {
                    GameObject unit = prefabList[iPrefab];
                    bool isLightStatic = MapEditorUtility.CheckStaticEditorFlags(unit, StaticEditorFlags.ContributeGI);
                    if (!isLightStatic)
                    {
                        continue;
                    }
                    MeshRenderer[] mrs = unit.GetComponentsInChildren<MeshRenderer>();
                    if (mrs == null && mrs.Length<=0)
                    {
                        continue;
                    }
                    lightInfos.Clear();
                    LightmapSetting lightSetting = unit.GetOrAddComponent<LightmapSetting>();
                    lightSetting.Clear();
                    for (int iRender = 0; iRender < mrs.Length; iRender++)
                    {
                        MeshRenderer meshRender = mrs[iRender];
#if UNITY_2019_1_OR_NEWER
                        isLightStatic = MapEditorUtility.CheckStaticEditorFlags(meshRender.gameObject, StaticEditorFlags.ContributeGI);
#else
                        isLightStatic = MapEditorUtility.CheckStaticEditorFlags(meshRender.gameObject, StaticEditorFlags.LightmapStatic);
#endif
                        if (!isLightStatic)
                        {
                            continue;
                        }
                        rendererIndexs.Clear();
                        LightInfo info = new LightInfo();
                        info.m_Render = meshRender;
                        info.m_BakedLightmapIndex = meshRender.lightmapIndex;
                        info.m_BakedLightmapScaleOffset = meshRender.lightmapScaleOffset;
                        info.m_RealLightmapIndex = meshRender.realtimeLightmapIndex;
                        info.m_RealLightmapScaleOffest = meshRender.realtimeLightmapScaleOffset;
                        GetChildIndex(unit.transform, meshRender.transform, rendererIndexs, info);
                        Leyoutech.Utility.ArrayUtility.Reverse(ref info.m_ChildIndex);
                        lightInfos.Add(info);
                    }
                    lightSetting.SetLightInfo(lightInfos.ToArray());
                }
            }
            
        }
        private void GetChildIndex(Transform parent, Transform child, Stack<int> childIndexs, LightInfo info)
        {
            if (parent == null || child == null)
            {
                return;
            }
            if (parent == child)
            {
                info.m_ChildIndex = childIndexs.ToArray();
                return;
            }

            for (int iChild = 0; iChild < parent.childCount; iChild++)
            {
                Transform childTrans = parent.GetChild(iChild);
                if (childTrans == child)
                {
                    childIndexs.Push(iChild);
                    info.m_ChildIndex = childIndexs.ToArray();
                    return;
                }
                else
                {
                    if (childTrans.childCount > 0)
                    {
                        childIndexs.Push(iChild);
                        GetChildIndex(childTrans, child, childIndexs, info);
                        childIndexs.Pop();
                    }
                }
            }
        }
        #endregion

        #region 导出数据
        /// <summary>
        /// 保存AreaDetailInfo和AreaInfo
        /// </summary>
        /// <param name="map"></param>
        /// <param name="areaDetailInfo"></param>
        /// <param name="areaInfo"></param>
        private void ExportAreaDetailInfo(Map map, AreaSpawner area)
        {
            AreaDetailInfo areaDetailInfo = area._AreaDetailInfo;
            AreaInfo areaInfo = area._AreaInfo;
            string addressableKey = string.Format(Constants.AREA_DETAIL_INFO_FILENAME_FORMAT, map.Uid, areaInfo.Uid);
            if (MapEditorUtility.CreateAssetAtExportDirectory(ref areaDetailInfo, m_ExportFolderName, addressableKey))
            {
                areaInfo.DetailInfoAddressableKey = addressableKey;
            }
        }

        /// <summary>
        /// 保存AreaDetailInfo到二进制文件
        /// </summary>
        private void SaveAreaDetailInfoBinary(Map map, AreaSpawner area)
        {
            AreaDetailInfo areaDetailInfo = area._AreaDetailInfo;
            AreaInfo areaInfo = area._AreaInfo;
            string addressableKey = string.Format(Constants.AREA_DETAIL_INFO_FILENAME_FORMAT, map.Uid, areaInfo.Uid);
            string fileName = string.Format("{0}/{1}", m_ExportFolderName, addressableKey);
            MapEditorSetting mapEditorSetting = MapEditorUtility.GetOrCreateMapEditorSetting();
            if (string.IsNullOrEmpty(mapEditorSetting.AssetExportDirectory))
            {
                return;
            }

            string path = string.Format("{0}/{1}.bytes"
                , mapEditorSetting.AssetExportDirectory
                , fileName);
            path = MapEditorUtility.GetFullPath(path);
            areaDetailInfo.Serialize(path);
        }

        /// <summary>
        /// 保存MapInfo到文件并设置AddressableKey
        /// </summary>
        private void ExportMapInfo(ref MapInfo mapInfo,bool isAll = true)
        {
            mapInfo.SceneAddressableKey = Constants.SCENE_ADDRESSKEY_STARTWITHS + mapInfo.Uid;
            string addressableKey = Constants.MAP_INFO_FILENAME_STARTWITHS + mapInfo.Uid;

            if(!isAll)
            {
                MapEditorSetting mapEditorSetting = MapEditorUtility.GetOrCreateMapEditorSetting();
                if (string.IsNullOrEmpty(mapEditorSetting.AssetExportDirectory))
                {
                    return;
                }

                string path = string.Format("{0}/{1}/{2}.asset"
                , mapEditorSetting.AssetExportDirectory
                , m_ExportFolderName
                , addressableKey);
                string fullPath = MapEditorUtility.GetFullPath(path);
                if (File.Exists(fullPath))
                {
                    EditorUtility.SetDirty(mapInfo);
                    return;
                }
            }
            if (!MapEditorUtility.CreateAssetAtExportDirectory(ref mapInfo, m_ExportFolderName, addressableKey))
            {
                Debug.LogError("导出失败");
            }
        }

        /// <summary>
        /// 保存MapInfo到二进制文件
        /// </summary>
        private void SaveMapInfoBinary(MapInfo mapInfo)
        {
            string addressableKey = Constants.MAP_INFO_FILENAME_STARTWITHS + mapInfo.Uid;
            string fileName = string.Format("{0}/{1}", m_ExportFolderName, addressableKey);
            MapEditorSetting mapEditorSetting = MapEditorUtility.GetOrCreateMapEditorSetting();
            if (string.IsNullOrEmpty(mapEditorSetting.AssetExportDirectory))
            {
                return;
            }

            string path = string.Format("{0}/{1}.bytes"
                , mapEditorSetting.AssetExportDirectory
                , fileName);
            path = MapEditorUtility.GetFullPath(path);
            mapInfo.Serialize(path);
        }

        private void GenerateRenderInfo(GameObject unit,  List<RendererInfo> rendererInfos)
        {
            LightmapSetting lightSetting = unit.GetComponent<LightmapSetting>();
            if (lightSetting == null)
            {
                return;
            }
            LightInfo[] lightInfos = lightSetting.m_LightInfos;
            if (lightInfos == null || lightInfos.Length <= 0)
            {
                return;
            }

            for (int iLight = 0; iLight < lightInfos.Length; iLight++)
            {
                LightInfo lightInfo = lightInfos[iLight];
                if (lightInfo == null || lightInfo.m_Render == null)
                {
                    continue;
                }

                Renderer renderer = lightInfo.m_Render;
                if (renderer)
                {
                    RendererInfo rendererInfo = new RendererInfo();

                    rendererInfo.LightmapIndex = lightInfo.m_BakedLightmapIndex;
                    rendererInfo.LightmapScaleOffset = lightInfo.m_BakedLightmapScaleOffset;
                    rendererInfo.RealLightmapIndex = lightInfo.m_RealLightmapIndex;
                    rendererInfo.RealLightmapScaleOffset = lightInfo.m_RealLightmapScaleOffest;
                    rendererInfo.TransformIndexs = lightInfo.m_ChildIndex;
                    rendererInfos.Add(rendererInfo);
                }
            }
        }
        
        private void FillUnitInfo(ref SceneUnitInfo unitInfo, GameObject obj, Area area, Quaternion areaInverseRotation)
        {
            unitInfo.LocalPosition = area.transform.InverseTransformPoint(obj.transform.position);
            unitInfo.LocalRotation = areaInverseRotation * obj.transform.rotation;
            unitInfo.LocalScale = ObjectUtility.CalculateLossyScale(obj.transform, area.transform);

            MapEditorUtility.CaculateAABB(obj.transform, ref unitInfo._AABB);
            unitInfo._Diameter = MathUtility.CaculateLongestSide(unitInfo._AABB);
        }

        
        private void GenerateAreaSpawnerInfo(AreaSpawner areaSpawner)
        {
            areaSpawner._AreaInfo = new AreaInfo();
            areaSpawner._AreaDetailInfo = ScriptableObject.CreateInstance<AreaDetailInfo>();

            areaSpawner._AreaInfo.Uid = areaSpawner.GetAreaId();
            areaSpawner._AreaInfo.Position = areaSpawner.GetAreaPosition();
            areaSpawner._AreaInfo.Rotation = areaSpawner.GetAreaRotation();
            areaSpawner._AreaInfo.AABB = areaSpawner.GetAABB();
            areaSpawner._AreaInfo.Diameter = areaSpawner.GetDiameter();
        }

#endregion

#region 导出区域
        /// <summary>
		/// 导出这个Scene，工作流程：
		///		复制这个Scene文件到ExportScene
		///		打开ExportScene
		///		删除ExportScene中的GameObject
		///		保存Scene
		/// </summary>
		private void ExportMapScene(Scene scene
            , uint mapUid)
        {
            MapEditorSetting mapEditorSetting = MapEditorUtility.GetOrCreateMapEditorSetting();
            string exportPath = string.Format("{0}/{1}/{2}{3}.unity"
                , mapEditorSetting.AssetExportDirectory
                , m_ExportFolderName
                , Constants.SCENE_EXPORT_FILENAMESTARTWITHS
                , mapUid);

            // 导出Scene
            FileUtil.DeleteFileOrDirectory(exportPath);
            FileUtil.CopyFileOrDirectory(scene.path, exportPath);
            AssetDatabase.ImportAsset(exportPath);
            Scene exportScene = EditorSceneManager.OpenScene(exportPath, OpenSceneMode.Additive);
            GameObject[] rootGameObjects = exportScene.GetRootGameObjects();
            for (int iRootGameObject = 0; iRootGameObject < rootGameObjects.Length; iRootGameObject++)
            {
                GameObject iterGameObject = rootGameObjects[iRootGameObject];
                DontDestroyAtExport dontDestroy = iterGameObject.GetComponent<DontDestroyAtExport>();
                if (dontDestroy)
                {
                    Object.DestroyImmediate(dontDestroy);
                }
                else
                {
                    Object.DestroyImmediate(iterGameObject);
                }
            }
            
            EditorSceneManager.SaveScene(exportScene);
            EditorSceneManager.CloseScene(exportScene, true);

            // 设置AddressabelKey
            AssetDatabase.SetLabels(AssetDatabase.LoadAssetAtPath(exportPath, typeof(SceneAsset)), Constants.EXPORT_MAPASSET_LABELS);
        }
        

        private void CleanUnuseAsset()
        {
            System.GC.Collect();
            Resources.UnloadUnusedAssets();
        }
        

        private IEnumerator ExportSpawnerArea(List<AreaSpawner> spawnerAreaList, bool isAll = false)
        {
            if(m_ExportSetting.OnlyExportMapScene)
            {
                BeginAction("Export Scene");
                ExportMapScene(SceneManager.GetActiveScene(), m_ExportingMap.Uid);
                EndAction();
                yield break;
            }

           
            m_ExportingMap.BeginExportMap();
            yield return null;
            IEnumerator mapUpdateEnum = m_ExportingMap._DoUpdate_Co(true);
            int mapDoUpdateTimes = m_ExportingMap.GetDoUpdateTimes();
            while (mapUpdateEnum.MoveNext()
                && m_ExportingMap.GetDoUpdateTimes() == mapDoUpdateTimes)
            {
                yield return null;
            }

            BeginAction("Calculate Global");
            m_HalfTanCameraFov = RendererUtility.CaculateHalfTanCameraFov(m_ExportingMap.ExpectedFov);
            EndAction();

            IEnumerator bakeEnumerator = Bake();
            while (bakeEnumerator.MoveNext())
            {
                yield return null;
            }
            if (spawnerAreaList != null && spawnerAreaList.Count > 0)
            {
                BeginAction("Create MapInfo");
                MapInfo mapInfo = null;
                if (isAll)
                {
                    mapInfo = ScriptableObject.CreateInstance<MapInfo>();
                }
                else
                {
                    string addressableKey = Constants.MAP_INFO_FILENAME_STARTWITHS + m_ExportingMap.Uid;
                    string fileName = string.Format("{0}/{1}", m_ExportFolderName, addressableKey);
                    MapEditorSetting mapEditorSetting = MapEditorUtility.GetOrCreateMapEditorSetting();
                    if (string.IsNullOrEmpty(mapEditorSetting.AssetExportDirectory))
                    {
                        yield break;
                    }

                    string path = string.Format("{0}/{1}.asset"
                        , mapEditorSetting.AssetExportDirectory
                        , fileName);
                    mapInfo = AssetDatabase.LoadAssetAtPath<MapInfo>(path);
                    if(mapInfo == null)
                    {
                        mapInfo = ScriptableObject.CreateInstance<MapInfo>();
                    }
                }
               
                mapInfo.Uid = m_ExportingMap.Uid;
                EndAction();

                for (int iArea = 0; iArea < spawnerAreaList.Count; iArea++)
                {
                    AreaSpawner areaSpawner = spawnerAreaList[iArea];

                    //如果没有配置的话 自动生成
                    string settingPath = string.Format("{0}/SplitArea_{1}_{2}.asset", m_ExportingMap.GetOwnerAreaPath(), m_ExportingMap.Uid, areaSpawner.m_AreaUid);
                    EditorSplitAreaSetting editorSplitAreaSetting = AssetDatabase.LoadAssetAtPath<EditorSplitAreaSetting>(settingPath);
                    if(editorSplitAreaSetting!= null)
                    {
                        if (m_ExportSetting.DestroySplitSetting)
                        {
                            AssetDatabase.DeleteAsset(settingPath);
                            editorSplitAreaSetting = null;
                            AssetDatabase.Refresh();
                            
                        }
                    }
                    if (editorSplitAreaSetting == null || editorSplitAreaSetting.m_EditorSplitAreaLayerSettings == null
                        || editorSplitAreaSetting.m_EditorSplitAreaLayerSettings.Count<=0)
                    {
                        IEnumerator generateEnumerator = new AnalyzeArea().StartGenerate(m_ExportingMap, areaSpawner);
                        while (generateEnumerator.MoveNext())
                        {
                            yield return null;
                        }
                    }
                    
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
                                SceneManager.MoveGameObjectToScene(rootObjArray[index], m_ExportingMap.GetOwnerScene());
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
                        EditorGamingMapData.CorrectAreaColliderCenter(exportArea);
                        yield return null;
                        yield return null;
                        areaSpawner.SetArea(exportArea);
                        IEnumerator areaSpawnerEnumerator = areaSpawner.DoUpdate(m_ExportingMap, true);
                        while (areaSpawnerEnumerator.MoveNext())
                        {
                            yield return null;
                        }


                        GenerateAreaSpawnerInfo(areaSpawner);
                        yield return null;

                        //导出的时候就没必要在刷新了
                        areaSpawner.enabled = false;
                        exportArea.enabled = false;
                        IEnumerator generateAreaLayerEnumerator = GenerateAreaLayerInfo(areaSpawner, exportArea);
                        while (generateAreaLayerEnumerator.MoveNext())
                        {
                            yield return null;
                        }
                    }
                    CleanUnuseAsset();
                    yield return null;
                    yield return null;
                }
                yield return null;
                // Export AreaDetailInfo
                BeginAction("Export AreaDetailInfo");
                for (int iArea = 0; iArea < spawnerAreaList.Count; iArea++)
                {
                    AreaSpawner iterArea = spawnerAreaList[iArea];
                    ExportAreaDetailInfo(m_ExportingMap, iterArea);
                    //保存到二进制文件中
                    SaveAreaDetailInfoBinary(m_ExportingMap, iterArea);
                }
                EndAction();

                yield return null;
                // Save AreaInfos To MapInfo
                BeginAction("Save AreaInfos To MapInfo");
                List<AreaInfo> areaInfos = new List<AreaInfo>();
                if(!isAll)
                {
                    AreaInfo[] originAreas = mapInfo.AreaInfos;
                    if(originAreas != null && originAreas.Length>0)
                    {
                        for(int iArea = 0;iArea<originAreas.Length;iArea++)
                        {
                            areaInfos.Add(originAreas[iArea]);
                        }
                    }
                }
                for (int iArea = 0; iArea < spawnerAreaList.Count; iArea++)
                {
                    AreaSpawner iterArea = spawnerAreaList[iArea];
                    if (!isAll)
                    {
                        AreaInfo areaInfo = areaInfos.Find((x) => x.Uid == iterArea._AreaInfo.Uid);
                        if(areaInfo != null)
                        {
                            areaInfos.Remove(areaInfo);
                        }
                    }
                   
                    areaInfos.Add(iterArea._AreaInfo);
                }
                areaInfos.Sort(SortAreaInfoByUid);
                for (int iArea = 0; iArea < areaInfos.Count; iArea++)
                {
                    areaInfos[iArea].Index = iArea;
                }
                mapInfo.AreaInfos = areaInfos.ToArray();
                EndAction();

                yield return null;
                // Generate Map Voxels
                BeginAction("Generate Map Voxels");
                new MapVoxelGenerator().GenerateVoxels(this, m_ExportingMap, ref mapInfo);
                EndAction();

                yield return null;
                // Export Scene
                if (m_ExportSetting.ExportMapScene)
                {
                    BeginAction("Export Scene");
                    ExportMapScene(SceneManager.GetActiveScene(), mapInfo.Uid);
                    EndAction();
                }

                yield return null;
                // Export MapInfo
                BeginAction("Export MapInfo");
                ExportMapInfo(ref mapInfo,isAll);
                //MapInfo生成二进制文件
                SaveMapInfoBinary(mapInfo);
                EndAction();

                yield return null;
                Area[] areaArray = Object.FindObjectsOfType<Area>();
                if (areaArray != null && areaArray.Length > 0)
                {
                    for (int iArea = areaArray.Length - 1; iArea >= 0; iArea--)
                    {
                        GameObject.DestroyImmediate(areaArray[iArea].gameObject);
                        if (iArea % 10 == 0)
                        {
                            yield return null;
                        }
                    }
                }
                Selection.activeObject = mapInfo;
            }

            //TODO:清除没用的unit
            if (m_ExportSetting.CleanUnUsedUnit)
            {
                IEnumerator clearUnusedUnitEnumerator = ClearUnUsedUnit();
                while (clearUnusedUnitEnumerator.MoveNext())
                {
                    yield return null;
                }
            }

            ///合并公用units
            if (m_ExportSetting.MergeCommonUnit)
            {
                IEnumerator findCommonUnitsEnumerator = FindCommonUnits();
                while (findCommonUnitsEnumerator.MoveNext())
                {
                    yield return null;
                }
            }
        }
        

        /// <summary>
        /// 创建虚拟格子信息
        /// </summary>
        private AreaVirtualGridInfo CreateVirtualGridInfo(float gridSize, int gridX, int gridY, int gridZ,
            long hashCode, AreaLayerInfo areaLayerInfo)
        {
            AreaVirtualGridInfo gridInfo = new AreaVirtualGridInfo();
            gridInfo.m_UnitIndexs = new List<int>();
            gridInfo.m_UnitIndexCache = new HashSet<int>();
            gridInfo.m_IndexX = gridX;
            gridInfo.m_IndexY = gridY;
            gridInfo.m_IndexZ = gridZ;
            gridInfo.m_Position = new Vector3(gridX * gridSize, gridY * gridSize, gridZ * gridSize);
            areaLayerInfo.AreaVirtualGridInfos.Add(gridInfo);
            areaLayerInfo.AreaVirtualGridIndexs.Add(hashCode);
            
            return gridInfo;
        }

        private IEnumerator ExportPrefab(GameObject childObj,Area area, Quaternion areaInverseRotation)
        {
            if(childObj == null)
            {
                yield break;
            }

            if (!childObj.activeSelf || childObj.hideFlags == HideFlags.DontSave)
            {
                yield break;
            }
            if (childObj.GetComponent<SemaphoreMark>() || childObj.GetComponent<IRootMark>()
                   || childObj.GetComponent<TreasureInfoMark>())
            {
                yield break;
            }
            
            MapEditorUtility.SelectChildParticleSystem(childObj);
            yield return null;
            SceneUnitInfo unitInfo = new SceneUnitInfo();
            FillUnitInfo(ref unitInfo, childObj, area, areaInverseRotation);
            GameObject sourcePrefab = UnityEditor.PrefabUtility.GetCorrespondingObjectFromSource(childObj);
            if (sourcePrefab == null)
            {
                //TODO:继续往子节点进行遍历
                int childCount = childObj.transform.childCount;
                if(childCount>0)
                {
                    for(int iChild =0;iChild<childCount;iChild++)
                    {
                        IEnumerator childExportEnumerator = ExportPrefab(childObj.transform.GetChild(iChild).gameObject,area,areaInverseRotation);
                        while(childExportEnumerator.MoveNext())
                        {
                            yield return null;
                        }
                    }
                }
            }
            else
            {
                string originAssetPath = AssetDatabase.GetAssetPath(sourcePrefab);
                string assetAddress = AssetDatabase.AssetPathToGUID(originAssetPath) + MapEditorUtility.GetPrefabOverrideModification(childObj);
                assetAddress = StringUtility.CalculateMD5Hash(assetAddress);
                if (string.IsNullOrEmpty(assetAddress))
                {
                    yield break;
                }
                assetAddress = string.Format(Constants.UNIT_PREFAB_ADDRESSKEY_FORMAT, assetAddress);
                if (m_UnitInfoSetting != null)
                {
                    if (!m_UnitInfoSetting.UnitName.Contains(assetAddress))
                    {
                        m_UnitInfoSetting.RelatedPrefab.Add(originAssetPath);
                        m_UnitInfoSetting.UnitName.Add(assetAddress);
                    }
                    else
                    {
                        int index = m_UnitInfoSetting.UnitName.IndexOf(assetAddress);
                        m_UnitInfoSetting.RelatedPrefab[index] = originAssetPath;
                    }
                }
                if (!m_ExportedUnitAddressableKeys.Contains(assetAddress))
                {
                    MapEditorSetting mapEditorSetting = MapEditorUtility.GetOrCreateMapEditorSetting();
                    m_ExportedUnitAddressableKeys.Add(assetAddress);
                    string unitFold = string.Format("{0}/{1}Map_{2}", mapEditorSetting.AssetExportDirectory
                        , Constants.UNIT_PREFAB_EXPORT_DIRECTORY
                        , m_ExportingMap.Uid);
                    if (!Directory.Exists(unitFold))
                    {
                        Directory.CreateDirectory(unitFold);
                    }
                    //TODO:创建预设
                    string exportAssetPath = string.Format("{0}/{1}.prefab"
                            , unitFold
                            , assetAddress);

                    GameObject unitPrefabAsset = UnityEditor.PrefabUtility.SaveAsPrefabAsset(childObj, exportAssetPath);
                    //需要移除不需要的脚本
                    GameObject unitInstantiate = UnityEditor.PrefabUtility.LoadPrefabContents(exportAssetPath);
#if true
                    SceneUnit exportedUnit = unitInstantiate.GetComponent<SceneUnit>();
                    if (exportedUnit)
                    {
                        exportedUnit.OnExportPrefab();
                        Object.DestroyImmediate(exportedUnit);
                    }
#endif

                    LightmapSetting lightMap = unitInstantiate.GetComponent<LightmapSetting>();
                    if (lightMap != null)
                    {
                        Object.DestroyImmediate(lightMap);
                    }

                    LightmapSetting[] childLightMap = unitInstantiate.GetComponentsInChildren<LightmapSetting>();
                    if (childLightMap != null && childLightMap.Length > 0)
                    {
                        for (int iMap = childLightMap.Length - 1; iMap >= 0; iMap--)
                        {
                            Object.DestroyImmediate(childLightMap[iMap]);
                        }
                    }

                    unitPrefabAsset = UnityEditor.PrefabUtility.SaveAsPrefabAsset(unitInstantiate, exportAssetPath);
                    UnityEditor.PrefabUtility.UnloadPrefabContents(unitInstantiate);
                    yield return null;
                }

                int assetIndex;
                if (!m_AssetInfos.TryGetValue(assetAddress, out assetIndex))
                {
                    assetIndex = m_AssetInfos.Count;
                    m_AssetInfos.Add(assetAddress, m_AssetInfos.Count);
                }

                unitInfo.AssetIndex = assetIndex;

                if (m_ExportSetting.ExportLightmap)
                {
                    m_RendererIndexs.Clear();
                    m_RendererInfos.Clear();
                   // GenerateRendererInfo(childObj.transform, m_RendererInfos, m_RendererIndexs);
                    GenerateRenderInfo(childObj, m_RendererInfos);
                    unitInfo.RendererInfos = m_RendererInfos.ToArray();
                }

                //TODO:计算所在的层和所在的格子
                float diameter = unitInfo._Diameter;//AABB盒的最长边，用来计算属于哪个层和哪些格子
                EditorSplitAreaLayerSetting editorLayerSetting = CalcuateIncludeLayer(m_EditorSplitAreaSetting.m_EditorSplitAreaLayerSettings
                    , diameter);
                if (editorLayerSetting == null)
                {
                    Debug.LogError(string.Format("Unit({0})未找到对应的层，请检查层设置"
                        , ObjectUtility.CalculateTransformPath(childObj.transform)), childObj);
                    yield break;
                }
                AreaLayerInfo areaLayerInfo = null;
                if (m_AreaLayerInfos.TryGetValue(editorLayerSetting.m_LayerName, out areaLayerInfo))
                {
                    areaLayerInfo.m_Units.Add(unitInfo);
                    BeginAction(string.Format("Calcuate Grid:{0}", childObj.name));
                    //unit的AABB盒得相对于区域的根节点 因为根节点可能不在世界的中心位置 但运行时 是在世界的中心位置
                    Vector3 abCenter = area.transform.InverseTransformPoint(unitInfo._AABB.center);
                    yield return null;
                    IEnumerator calcuateEnumerator = CalcuateInLayerGrid(abCenter, diameter, areaLayerInfo, areaLayerInfo.m_Units.IndexOf(unitInfo));
                    while (calcuateEnumerator.MoveNext())
                    {
                        yield return null;
                    }
                    EndAction();
                }
                else
                {
                    Debug.LogError(string.Format("层{0}未生成", editorLayerSetting.m_LayerName));
                    yield break;
                }
            }
        }
        /// <summary>
        /// 生成区域层信息
        /// </summary>
        /// <param name="areaSpawner"></param>
        /// <param name="area"></param>
        /// <returns></returns>
        private IEnumerator GenerateAreaLayerInfo(AreaSpawner areaSpawner, Area area)
        {
            string settingPath = string.Format("{0}/SplitArea_{1}_{2}.asset", m_ExportingMap.GetOwnerAreaPath(), m_ExportingMap.Uid, areaSpawner.m_AreaUid);
            m_EditorSplitAreaSetting = AssetDatabase.LoadAssetAtPath<EditorSplitAreaSetting>(settingPath);
            if (m_EditorSplitAreaSetting == null)
            {
                EditorUtility.DisplayDialog("拆解", "未找到拆解区域配置", "ok");
                yield break;
            }

            m_EditorSplitAreaSetting.m_EditorSplitAreaLayerSettings.Sort(SortLayerByPriority);
            Quaternion areaInverseRotation = Quaternion.Inverse(areaSpawner._AreaInfo.Rotation);
            int layerCount = m_EditorSplitAreaSetting.m_EditorSplitAreaLayerSettings.Count;
            ClearInfo();
            //要保存的信息
            for (int iLayer = 0; iLayer < layerCount; iLayer++)
            {
                EditorSplitAreaLayerSetting areaLayerSetting = m_EditorSplitAreaSetting.m_EditorSplitAreaLayerSettings[iLayer];
                if (areaLayerSetting == null)
                {
                    continue;
                }
                AreaLayerInfo layerInfo = new AreaLayerInfo();
                string layerName = areaLayerSetting.m_LayerName;
                layerInfo.m_GridSize = areaLayerSetting.m_GridSize;
                layerInfo.m_Priority = areaLayerSetting.m_Priority;
                layerInfo.m_Offest = areaLayerSetting.m_Offest;
                m_AreaLayerInfos.Add(layerName, layerInfo);
            }
            yield return null;
            int childCount = area.transform.childCount;
            if (childCount <= 0)
            {
                yield break;
            }

            //TODO:跟美术商量过 拆分的粒度由美术自己决定 
            IEnumerator exportPrefabEnumer = ExportPrefab(area.gameObject, area, areaInverseRotation);
            while (exportPrefabEnumer.MoveNext())
            {
                yield return null;
            }

            //TODO:保存层信息
            Dictionary<string, AreaLayerInfo>.Enumerator iter = m_AreaLayerInfos.GetEnumerator();
            List<AreaLayerInfo> layerInfos = new List<AreaLayerInfo>();
            while (iter.MoveNext())
            {
                layerInfos.Add(iter.Current.Value);
            }
            areaSpawner._AreaDetailInfo.AreaLayerInfos = layerInfos.ToArray();
            areaSpawner._AreaDetailInfo.AssetInfos = new AssetInfo[m_AssetInfos.Count];
            int assetAddedCount = 0;
            foreach (KeyValuePair<string, int> kv in m_AssetInfos)
            {
                areaSpawner._AreaDetailInfo.AssetInfos[assetAddedCount++].AddressableKey = kv.Key;
            }
            m_AssetInfos.Clear();
            yield return null;
        }

#endregion

#region 数据结构
        
        private class VoxelGenerator
        {
            /// <summary>
            /// 按照在屏幕空间的大小,由大到小排序
            /// </summary>
            protected int SortUnitData(UnitInVoxel x, UnitInVoxel y)
            {
                float offset = y.RelativeSize - x.RelativeSize;
                return Mathf.Abs(offset) > 1.0f
                    ? (int)offset
                    : offset > 0
                        ? 1
                        : -1;
            }

            protected void GenerateVoxelGridInfo(float halfTanCameraFov
                , VoxelGrid voxelGrid
                , out VoxelGridInfo voxelGridInfo
                , out List<VoxelData> voxels
                , IEnumerator<UnitData> foreachEnumerator
                , out DebugInfo debugInfo
                , float minDisplayRelativeSize)
            {
                debugInfo = new DebugInfo();

                voxelGridInfo = new VoxelGridInfo();
                voxelGridInfo.VoxelSize = voxelGrid.VoxelSize;
                voxelGridInfo.VoxelCounts = voxelGrid.GetVoxelCounts();
                voxelGridInfo.LDBVoxelPosition = voxelGrid.GetLDBVoxelPosition();

                int voxelCount = (int)(voxelGridInfo.VoxelCounts.x
                    * voxelGridInfo.VoxelCounts.y
                    * voxelGridInfo.VoxelCounts.z);
                voxels = new List<VoxelData>(voxelCount);

                // new Voxels
                for (int iVoxelX = 0; iVoxelX < voxelGridInfo.VoxelCounts.x; iVoxelX++)
                {
                    for (int iVoxelY = 0; iVoxelY < voxelGridInfo.VoxelCounts.y; iVoxelY++)
                    {
                        for (int iVoxelZ = 0; iVoxelZ < voxelGridInfo.VoxelCounts.z; iVoxelZ++)
                        {
                            VoxelData iterVoxel = new VoxelData();
                            iterVoxel.Center = voxelGridInfo.CaculateVoxelCenter(iVoxelX, iVoxelY, iVoxelZ);
                            iterVoxel.Units = new List<UnitInVoxel>();
                            voxels.Add(iterVoxel);
                        }
                    }
                }

                // Caculate Voxel
                HashSet<int> verifyRepeat = new HashSet<int>(); // 用于判断是否把一个Unit重复添加到同一个Voxel中
                while (foreachEnumerator.MoveNext())
                {
                    UnitData unitData = foreachEnumerator.Current;
                    if (unitData.Diameter <= 0)
                    {
                        continue;
                    }
                    float toCameraDistance = RendererUtility.CacluateToCameraDistance(unitData.Diameter, minDisplayRelativeSize, halfTanCameraFov);
                    // l:Left,-x d:Down,-y b:Back,-z
                    Vector3 ldbVoxelIndex = MathUtility.EachFloorToInt(voxelGridInfo.CaculateVoxelIndexVector(unitData.Position - Vector3.one * toCameraDistance));
                    // r:Right,x u:Up,y f:Front,z
                    Vector3 rufVoxelIndex = MathUtility.EachCeilToInt(voxelGridInfo.CaculateVoxelIndexVector(unitData.Position + Vector3.one * toCameraDistance));

                    verifyRepeat.Clear();
                    for (int iVoxelX = (int)ldbVoxelIndex.x; iVoxelX <= rufVoxelIndex.x; iVoxelX++)
                    {
                        for (int iVoxelY = (int)ldbVoxelIndex.y; iVoxelY <= rufVoxelIndex.y; iVoxelY++)
                        {
                            for (int iVoxelZ = (int)ldbVoxelIndex.z; iVoxelZ <= rufVoxelIndex.z; iVoxelZ++)
                            {
                                int voxelIndex = voxelGridInfo.ConvertIndexVectorToIndex(iVoxelX, iVoxelY, iVoxelZ);
                                if (!verifyRepeat.Add(voxelIndex))
                                {
                                    Debug.LogError(string.Format("把Unit Index({0})重复添加到Voxel({1}, {2}, {3})中，应该是程序逻辑写的有问题"
                                        , unitData.Index, iVoxelX, iVoxelY, iVoxelZ));
                                }
                                float relativeSize = RendererUtility.CaculateRelativeHeightInScreen(unitData.Diameter
                                    , Vector3.Distance(voxels[voxelIndex].Center, unitData.Position)
                                    , halfTanCameraFov);
                                voxels[voxelIndex].Units.Add(new UnitInVoxel(unitData.Index, relativeSize));
                            }
                        }
                    }
                }

                // Save to MapInfo
                voxelGridInfo.VoxelInfos = new VoxelInfo[voxels.Count];
                debugInfo.MaxUnitVoxelIndex = 0;
                debugInfo.MaxUnitCountInOneVoxel = 0;
                for (int iVoxel = 0; iVoxel < voxels.Count; iVoxel++)
                {
                    VoxelData iterVoxel = voxels[iVoxel];
                    if (iterVoxel.Units.Count > debugInfo.MaxUnitCountInOneVoxel)
                    {
                        debugInfo.MaxUnitCountInOneVoxel = iterVoxel.Units.Count;
                        debugInfo.MaxUnitVoxelIndex = iVoxel;
                    }
                    voxelGridInfo.VoxelInfos[iVoxel].Indexs = new int[iterVoxel.Units.Count];
                    if (iterVoxel.Units.Count > 0)
                    {
                        iterVoxel.Units.Sort(SortUnitData);
                    }
                    verifyRepeat.Clear();
                    for (int iUnit = 0; iUnit < iterVoxel.Units.Count; iUnit++)
                    {
                        int unitIndex = iterVoxel.Units[iUnit].Index;
                        voxelGridInfo.VoxelInfos[iVoxel].Indexs[iUnit] = unitIndex;

                        if (!verifyRepeat.Add(unitIndex))
                        {
                            Debug.LogError(string.Format("Voxel({0})中有重复的Unit({1})"
                                , iVoxel, unitIndex));
                        }
                    }
                }
                verifyRepeat = null;
            }

            protected class VoxelData
            {
                public Vector3 Center;
                public List<UnitInVoxel> Units;
            }

            protected struct UnitInVoxel
            {
                public int Index;
                public float RelativeSize;

                public UnitInVoxel(int indexInMap, float relativeSize)
                {
                    Index = indexInMap;
                    RelativeSize = relativeSize;
                }
            }

            protected struct UnitData
            {
                public int Index;
                public Vector3 Position;
                public float Diameter;
            }

            protected struct DebugInfo
            {
                public int MaxUnitCountInOneVoxel;
                public int MaxUnitVoxelIndex;
            }
        }

        private class MapVoxelGenerator : VoxelGenerator
        {
            private MapInfo m_MapInfo;

            public void GenerateVoxels(Exporter exporter, Map map, ref MapInfo mapInfo)
            {
                m_MapInfo = mapInfo;
                List<VoxelData> voxels;
                DebugInfo debugInfo;
                GenerateVoxelGridInfo(exporter.GetHalfTanCameraFov()
                    , map.VoxelGrid
                    , out mapInfo.VoxelGridInfo
                    , out voxels
                    , ForeachUnitData()
                    , out debugInfo
                    , Constants.AREA_MIN_DISPLAY_RELATIVE_SIZE);

                exporter.GetDetailStringBuilder().AppendLine(string.Format("Map({0})中，一共有{1}个Voxel，{2}个Area。Area数量最多的Voxel是{3}，有{4}个Area"
                    , map.Uid
                    , voxels.Count
                    , mapInfo.AreaInfos.Length
                    , mapInfo.VoxelGridInfo.ConvertIndexToIndexVector(debugInfo.MaxUnitVoxelIndex)
                    , debugInfo.MaxUnitCountInOneVoxel));
            }

            protected IEnumerator<UnitData> ForeachUnitData()
            {
                for (int iUnit = 0; iUnit < m_MapInfo.AreaInfos.Length; iUnit++)
                {
                    AreaInfo iterAreaInfo = m_MapInfo.AreaInfos[iUnit];
                    UnitData unitData = new UnitData();
                    unitData.Index = iterAreaInfo.Index;
                    unitData.Position = iterAreaInfo.AABB.center;
                    unitData.Diameter = iterAreaInfo.Diameter;
                    yield return unitData;
                }
            }
        }
#endregion
        
        /// <summary>
        /// 清除无用unit
        /// </summary>
        [MenuItem("Custom/清除无用Unit")]
        public static void CleanUnUseUnits()
        {
           IEnumerator clearEnumerator = new Exporter().ClearUnUsedUnit();
            while(clearEnumerator.MoveNext())
            {

            }
        }

        [MenuItem("Custom/合并Unit")]
        public static void MergereUnits()
        {
            IEnumerator mergeEnumerator = new Exporter().FindCommonUnits();
            while (mergeEnumerator.MoveNext())
            {

            }
        }

        [MenuItem("Custom/打包/导出场景数据(清除重新导出)")]
        public static void CleanAndExportAllMap()
        {
            new ExportAllMapData().BeginExport(true);
        }

        [MenuItem("Assets/Map/导出选中Map")]
        public static void ExportSelectMap()
        {
            Object[] selectObjs = Selection.objects;
            if (selectObjs == null || selectObjs.Length<=0)
            {
                return;
            }

            List<string> mapPaths = new List<string>();
            for (int iObj =0;iObj<selectObjs.Length;iObj++)
            {
                if(selectObjs[iObj] is SceneAsset)
                {
                    mapPaths.Add(AssetDatabase.GetAssetPath(selectObjs[iObj]));
                }
            }

            if(mapPaths.Count<=0)
            {
                return;
            }
            new ExportAllMapData().BeginExport(false, mapPaths);
        }
    }

    
}
#endif