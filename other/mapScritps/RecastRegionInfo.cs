#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Map
{
    [ExecuteInEditMode]
    public class RecastRegionInfo : MonoBehaviour
    {
        public float CubeCellSize = 1f;
        //public int m_Layer = 2;
        public static RecastCube CreateRecastCube(RecastRegionInfo regionInfo,Transform parentTrans)
        {
            if(regionInfo == null)
            {
                return null;
            }

            GameObject recastArea = new GameObject("RecastRegion");
            if(parentTrans != null)
            {
                recastArea.transform.SetParent(parentTrans);
            }

            recastArea.transform.position = regionInfo.transform.position;
            recastArea.transform.localScale = regionInfo.transform.lossyScale;
            RecastCube recastCube = recastArea.AddComponent<RecastCube>();
            recastCube.CubeCellSize = regionInfo.CubeCellSize;
            recastCube.UpdateDatas();
            //recastCube.m_Layer = regionInfo.m_Layer;
            return recastCube;
        }

        [MenuItem("Custom/创建自定义Recast区域")]
        public static GameObject CreateRegion()
        {
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            if(Selection.activeTransform != null)
            {
                obj.transform.SetParent(Selection.activeTransform);
            }
            obj.transform.localPosition = Vector3.zero;
            obj.name = "RecastRegion";
            obj.AddComponent<RecastRegionInfo>();
            obj.GetComponent<Collider>().enabled = false;
            Renderer regionRender = obj.GetComponent<Renderer>();
            if(regionRender)
            {
                regionRender.enabled = false;
            }
            Selection.activeGameObject = obj;
            return obj;
        }
    }
}

#endif