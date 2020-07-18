#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Map
{ 
    /// <summary>
    /// 导出所有Map数据
    /// </summary>
    public class ExportAllMapData:ExportBase
    {
        private List<string> m_MapPaths;
        /// <summary>
        /// 导出之前是否要清除所有unit
        /// </summary>
        private bool m_IsCleanAll;
        public override ExporterHandle BeginExport(params object[] exportPara)
        {
            if(exportPara != null && exportPara.Length>0)
            {
                m_IsCleanAll = (bool)exportPara[0];
                if (exportPara.Length>1)
                {
                    m_MapPaths = (List<string>)exportPara[1];
                }
            }
            return base.BeginExport(exportPara);
        }

        protected override void DoBegin()
        {
            base.DoBegin();

            if(m_MapPaths == null || m_MapPaths.Count<=0)
            {
                m_MapPaths = ExportBase.GetMapScenePath();
            }
        }

        protected override void DoEnd()
        {
            base.DoEnd();
            m_MapPaths = null;
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(0);
            }
        }

        protected override IEnumerator DoExport()
        {
            if(m_IsCleanAll)
            {
                //TODO:导出前先清除所有的unit
                MapEditorSetting mapEditorSetting = MapEditorUtility.GetMapEditorSetting();
                string exportAssetPath = string.Format("{0}/{1}"
                            , mapEditorSetting.AssetExportDirectory
                            , Constants.UNIT_PREFAB_EXPORT_DIRECTORY);
                exportAssetPath = MapEditorUtility.GetFullPath(exportAssetPath);

                string[] unitFiles = Directory.GetFiles(exportAssetPath, "*.prefab",SearchOption.AllDirectories);

                if (unitFiles != null && unitFiles.Length > 0)
                {
                    UTF8Encoding utf8 = new UTF8Encoding(false);
                    for (int iFile = 0; iFile < unitFiles.Length; iFile++)
                    {
                        EditorUtility.DisplayProgressBar("删除所有unit", unitFiles[iFile], iFile * 1.0f / unitFiles.Length);
                        File.Delete(unitFiles[iFile]);
                        yield return null;
                    }
                }
                EditorUtility.ClearProgressBar();
                AssetDatabase.Refresh();
            }
            yield return null;
            if(m_MapPaths != null && m_MapPaths.Count>0)
            {
                for(int iMap = 0;iMap<m_MapPaths.Count;iMap++)
                {
                    string mapPath = m_MapPaths[iMap];
                    Scene iterScene = EditorSceneManager.OpenScene(mapPath, OpenSceneMode.Single);
                    GameObject[] rootGameObjects = iterScene.GetRootGameObjects();
                    for (int iRootGameObject = 0; iRootGameObject < rootGameObjects.Length; iRootGameObject++)
                    {
                        GameObject iterGameObject = rootGameObjects[iRootGameObject];
                        Map iterMap = iterGameObject.GetComponent<Map>();
                        if (iterMap)
                        {
                            ExporterHandle handle = iterMap.ExportMap();
                            while (!handle.IsDone)
                            {
                                yield return null;
                            }
                        }
                    }
                }
            }
            yield return null;
            ///导出所有结束后 把公用unit放到单独目录下
            IEnumerator findCommonUnitsEnumerator = new Exporter().FindCommonUnits();
            while (findCommonUnitsEnumerator.MoveNext())
            {
                yield return null;
            }
        }
    }
}
#endif
