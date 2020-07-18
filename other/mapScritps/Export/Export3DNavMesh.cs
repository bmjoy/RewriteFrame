#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Map
{
    /// <summary>
    /// 导出空间寻路
    /// </summary>
    public class Export3DNavMesh:ExportBase
    {
        private Map m_Map;
        /// <summary>
        /// 最小体素大小
        /// </summary>
        private RootScenceRecast.ROOT_SCENCE_CUT_SIZE m_SceneCutSize;

        public override ExporterHandle BeginExport(params object[] exportPara)
        {
            if(exportPara == null || exportPara.Length<2)
            {
                Debug.LogError("Export3DNavMesh 参数异常");
                return null;
            }
            m_Map = exportPara[0] as Map;
            m_SceneCutSize = (RootScenceRecast.ROOT_SCENCE_CUT_SIZE)exportPara[1];
            return base.BeginExport(exportPara);
        }
        

        protected override void DoEnd()
        {
            base.DoEnd();
            m_Map = null;
        }


        protected override IEnumerator DoExport()
        {
            DontDestroyAtExport[] exports = UnityEngine.Object.FindObjectsOfType<DontDestroyAtExport>();
            if (exports != null && exports.Length > 0)
            {
                for (int iExport = 0; iExport < exports.Length; iExport++)
                {
                    DontDestroyAtExport export = exports[iExport];
                    export.gameObject.SetActive(false);
                }
            }
            yield return null;

            List<AreaSpawner> areaSpawners = m_Map.GetAreaSpawnerList();
            if (areaSpawners != null && areaSpawners.Count > 0)
            {
                for (int iArea = 0; iArea < areaSpawners.Count; iArea++)
                {
                    AreaSpawner areaSpawner = areaSpawners[iArea];
                    IEnumerator areaUpdateEnum = areaSpawner.DoUpdate(m_Map, false);
                    while (areaUpdateEnum.MoveNext())
                    {
                        yield return null;
                    }
                }
            }

            IEnumerator loadAllAreaEnumerator = m_Map.LoadAllArea(true);
            while (loadAllAreaEnumerator.MoveNext())
            {
                yield return null;
            }


            IEnumerator export3DNavMeshEnum = EditorGamingMapData.Export3DNavMesh(m_SceneCutSize, m_Map);
            while (export3DNavMeshEnum.MoveNext())
            {
                yield return null;
            }
        }
    }
}

#endif