#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Map
{
    /// <summary>
    /// 导出GamingMap数据
    /// </summary>
    public class ExportGamingMapData : ExportBase
    {
        private GamingMap m_GamingMap;
        private Map m_Map;
        /// <summary>
        /// 需要导出的区域id列表
        /// </summary>
        private List<ulong> m_ExportAreaIds;

        public override ExporterHandle BeginExport(params object[] exportPara)
        {
            if (exportPara == null || exportPara.Length < 2)
            {
                Debug.LogError("ExportGamingMapData 参数异常");
                return null;
            }
            m_Map = exportPara[0] as Map;
            m_GamingMap = exportPara[1] as GamingMap;
            if(exportPara.Length>=3)
            {
                m_ExportAreaIds = exportPara[2] as List<ulong>;
            }
            return base.BeginExport(exportPara);
        }
        

        protected override void DoEnd()
        {
            base.DoEnd();
            m_Map = null;
            m_GamingMap = null;
        }


        protected override IEnumerator DoExport()
        {
            List<AreaSpawner> areaSpawnerList = m_Map.GetAreaSpawnerList();
            if (areaSpawnerList != null && areaSpawnerList.Count > 0)
            {
                for (int iArea = 0; iArea < areaSpawnerList.Count; iArea++)
                {
                    AreaSpawner areaSpawner = areaSpawnerList[iArea];
                    if (areaSpawner != null)
                    {
                        if(m_ExportAreaIds != null && m_ExportAreaIds.Contains(areaSpawner.GetAreaId()))
                        {
                            m_GamingMap.LoadGamingMapArea(areaSpawner, false);
                        }
                        yield return null;
                    }
                }
            }
            yield return null;
            m_GamingMap.InitGamingArea();
            yield return null;
            m_GamingMap.BeginExport();
            yield return null;
            IEnumerator updateAreaEnum = m_GamingMap.UpdataGamingAreas(true);
            if (updateAreaEnum != null)
            {
                while (updateAreaEnum.MoveNext())
                {
                    yield return null;
                }
            }
            yield return null;
            //刷新LeapOverview
            if (m_GamingMap.m_LeapOverview != null)
            {
                IEnumerator leapOverviewUpdate = m_GamingMap.m_LeapOverview.OnUpdate(m_GamingMap);
                if (leapOverviewUpdate != null)
                {
                    while (leapOverviewUpdate != null && leapOverviewUpdate.MoveNext())
                    {
                        yield return null;
                    }
                }
            }
            yield return null;
            EditorGamingMapData.SaveGamingMapToJson(m_GamingMap, m_ExportAreaIds);
            yield return null;
            m_GamingMap.EndExport();
            yield return null;
        }
    }
}

#endif