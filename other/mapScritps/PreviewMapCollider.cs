using Map;
#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static Map.EditorGamingMapData;

[ExecuteInEditMode]
public class PreviewMapCollider : MonoBehaviour
{
    [MenuItem("Custom/预览地图碰撞")]
    public static void Export()
    {
        PreviewMapCollider[] colliders = UnityEngine.Object.FindObjectsOfType<PreviewMapCollider>();
        if (colliders != null && colliders.Length > 0)
        {
            for (int iCollider = colliders.Length - 1; iCollider >= 0; iCollider--)
            {
                GameObject obj = colliders[iCollider].gameObject;
                if (obj != null)
                {
                    GameObject.DestroyImmediate(obj);
                }
            }
        }

        GameObject colliderObj = new GameObject(typeof(PreviewMapCollider).Name);
        colliderObj.AddComponent<PreviewMapCollider>();
    }

    public uint m_MapId;

    [EditorExtend.Button("生成", "Generate", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)]
    public bool _Generate;

    private IEnumerator m_Spawner;

    private EditorCollider m_ColliderData;

    private void OnEnable()
    {
        EditorApplication.update += OnUpdate;
    }

    private void OnDisable()
    {
        EditorApplication.update -= OnUpdate;
    }

    private void OnUpdate()
    {
        if(m_Spawner != null)
        {
            if(!m_Spawner.MoveNext())
            {
                m_Spawner = null;
            }
        }
    }

    private IEnumerator SpawnerCollider()
    {
        EditorColliderArea[] arealist = m_ColliderData.areaList;
        if(arealist != null && arealist.Length>0)
        {
            for(int iArea = 0;iArea<arealist.Length;iArea++)
            {
                EditorColliderArea editorArea = arealist[iArea];
                if(editorArea == null)
                {
                    continue;
                }
                GameObject areaObj = new GameObject(editorArea.areaId.ToString());
                areaObj.transform.SetParent(transform);
                areaObj.transform.localPosition = Vector3.zero;
                areaObj.transform.localRotation = Quaternion.identity;
                areaObj.transform.localScale = Vector3.one;

                EditorDecorate[] decorate_list = editorArea.decorateList;
                if(decorate_list != null && decorate_list.Length>0)
                {
                    for(int iDecorate = 0;iDecorate<decorate_list.Length;iDecorate++)
                    {
                        EditorDecorate decorate = decorate_list[iDecorate];
                        CreateColliderObj(decorate, areaObj.transform);
                        yield return null;
                    }
                }
            }
            yield return null;
            GameObject commonObj = new GameObject("DontExport");
            commonObj.transform.SetParent(transform);
            commonObj.transform.localPosition = Vector3.zero;
            commonObj.transform.localRotation = Quaternion.identity;
            commonObj.transform.localScale = Vector3.one;

            EditorDecorate[] commonDecorate_list = m_ColliderData.commondecorateList;
            if (commonDecorate_list != null && commonDecorate_list.Length > 0)
            {
                for (int iDecorate = 0; iDecorate < commonDecorate_list.Length; iDecorate++)
                {
                    EditorDecorate decorate = commonDecorate_list[iDecorate];
                    CreateColliderObj(decorate, commonObj.transform);
                    yield return null;
                }
            }
        }
        yield return null;
    }

    private GameObject CreateColliderObj(EditorDecorate data,Transform parent)
    {
        if(data == null)
        {
            return null;
        }

        GameObject obj = null;
        Vector3 position = Vector3.zero;
        Quaternion rotation = new Quaternion(data.dir.x,data.dir.y,data.dir.z,data.dir.w);
        Vector3 scale = Vector3.one;

        Vector3 size = Vector3.one;
        size.x = Mathf.Abs(data.dirMax.x-data.dirMin.x);
        size.y = Mathf.Abs(data.dirMax.y-data.dirMin.y);
        size.z = Mathf.Abs(data.dirMax.z-data.dirMin.z);

        scale = size;

        float raduis = 0;
        position = new Vector3(data.dirMin.x + size.x * 0.5f, data.dirMin.y + size.y * 0.5f, data.dirMin.z + size.z * 0.5f);
        
        switch ((COLLIDER_TYPE)data.type)
        {
            case COLLIDER_TYPE.PX_BOX:
                obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                scale = new Vector3(size.x, size.y, size.z);
                break;
            case COLLIDER_TYPE.PX_CAPSULE:
                obj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                float height = Mathf.Max(size.x, size.y, size.z);
                if(height == size.x)
                {
                    raduis = Mathf.Max(size.y,size.z);
                    float scaleSize = height / 2;
                    scale = new Vector3(scaleSize, raduis, raduis);
                }
                else if(height == size.y)
                {
                    raduis = Mathf.Max(size.x, size.z) ;
                    float scaleSize = height / 2;
                    scale = new Vector3(raduis, scaleSize, raduis);
                }
                else if(height == size.z)
                {
                    raduis = Mathf.Max(size.y, size.x);
                    float scaleSize = height / 2;
                    scale = new Vector3(raduis, raduis, scaleSize);
                }
                
                break;
            case COLLIDER_TYPE.PX_SPHERE:
                obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                raduis = Mathf.Max(size.x,size.y,size.z);
                scale = new Vector3(raduis,raduis,raduis);
                break;
        }
        obj.name = data.scenePath;
        obj.transform.SetParent(parent);
        obj.transform.position = position;
        obj.transform.rotation = rotation;
        obj.transform.localScale = scale;

        CapsuleCollider capCollider = obj.GetComponent<CapsuleCollider>();
        if (capCollider != null)
        {
            Vector3 originScale = capCollider.transform.localScale;
            capCollider.transform.localScale = new Vector3(originScale.y,originScale.x,originScale.z);
            capCollider.transform.Rotate(Vector3.forward, -90);
            //capCollider.direction = 2;
        }
        return obj;
    }

    public void Generate()
    {
        int childCount = transform.childCount;
        for(int iChild = childCount -1;iChild >= 0; iChild--)
        {
            GameObject.DestroyImmediate(transform.GetChild(iChild).gameObject);
        }
        m_ColliderData = EditorGamingMapData.LoadMapCollider(m_MapId);
        if(m_ColliderData == null)
        {
            Debug.LogError("地图碰撞配置错误");
            return;
        }
        m_Spawner = SpawnerCollider();
    }
    
}
#endif