#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Map
{
    /// <summary>
    /// 导出基类
    /// </summary>
    public class ExportBase
    {
        /// <summary>
        /// 导出携程 为了避免一帧内干完造成卡顿
        /// </summary>
        protected IEnumerator m_ExportEnumerator;

        /// <summary>
        /// 当前导出进度
        /// </summary>
        private ExporterHandle m_ExportHandler;

        public virtual ExporterHandle BeginExport(params object[] exportPara)
        {
            DoBegin();
            return m_ExportHandler;
        }

        protected virtual void DoBegin()
        {
            m_ExportHandler = new ExporterHandle();
            m_ExportHandler.IsDone = false;
            EditorApplication.update += OnUpdate;
            m_ExportEnumerator = DoExport();

        }
        
        protected virtual IEnumerator DoExport()
        {
            yield return null;
        }

        protected virtual void DoEnd()
        {
            m_ExportHandler.IsDone = true;
            m_ExportEnumerator = null;
            EditorApplication.update -= OnUpdate;
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
            }
            catch (System.Exception e)
            {
                moveNext = false;
            }

            if (!moveNext)
            {
                EndExport();
            }

        }

        private void EndExport()
        {
            DoEnd();
        }


        /// <summary>
        /// 获取所有的GamingMap路径
        /// </summary>
        /// <returns></returns>
        public static List<string> GetGamingMapScenePath()
        {
            List<string> m_GamingMapList = new List<string>();
            string m_GamingMapPath = "";
            GamingMapEditorSetting gamingSetting = GamingMapEditorUtility.GetGamingMapEditorSetting();
            if (gamingSetting != null)
            {
                m_GamingMapPath = gamingSetting.m_GamingMapPath;
            }
            string[] GamingList = AssetDatabase.FindAssets("t:Scene", new string[] { m_GamingMapPath });
            if (GamingList != null && GamingList.Length > 0)
            {
                for (int iGame = 0; iGame < GamingList.Length; iGame++)
                {
                    string gamingPath = GamingList[iGame];
                    string assetPath = AssetDatabase.GUIDToAssetPath(GamingList[iGame]);
                    if (assetPath.Contains("Area"))
                    {
                        continue;
                    }
                    m_GamingMapList.Add(assetPath);
                }
            }
            return m_GamingMapList;
        }

        /// <summary>
        /// 获取所有的Map路径
        /// </summary>
        /// <returns></returns>
        public static List<string> GetMapScenePath()
        {
            List<string> mapPaths = new List<string>();
            string mapScenePath = "";
            MapEditorSetting mapSetting = MapEditorUtility.GetMapEditorSetting();
            if (mapSetting != null)
            {
                mapScenePath = mapSetting.m_MapSavePath;
            }
            string[] mapPathArray = AssetDatabase.FindAssets("t:Scene", new string[] { mapScenePath });
            if (mapPathArray != null && mapPathArray.Length > 0)
            {
                for (int iMap = 0; iMap < mapPathArray.Length; iMap++)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(mapPathArray[iMap]);
                    string[] pathSplit = assetPath.Split('/');
                    if (pathSplit != null && pathSplit.Length > 0)
                    {
                        if (pathSplit[pathSplit.Length - 1].Contains("Map_"))
                        {
                            mapPaths.Add(assetPath);
                        }
                    }

                }
            }
            return mapPaths;
        }
    }

}
#endif