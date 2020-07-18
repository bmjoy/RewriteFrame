using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class Base3DViewer: MonoBehaviour
{
    /// <summary>
    /// 层字典
    /// </summary>
    private static Dictionary<int, bool> m_AllUsedLayers = new Dictionary<int, bool>();

    /// <summary>
    /// 层ID
    /// </summary>
    private int m_Layer = -1;

    /// <summary>
    /// 设置模型的层
    /// </summary>
    /// <param name="root">根节点</param>
    /// <param name="layer">空白层</param>
    protected void SetToLayer(GameObject root, bool blankLayer)
    {
		if (root == null)
		{
			return;
		}
        if (m_Layer != 0)
        {
            if (m_AllUsedLayers.ContainsKey(m_Layer))
            {
                m_AllUsedLayers.Remove(m_Layer);
            }
            m_Layer = 0;
        }

        int layer = 0;

        if (blankLayer)
        {
            for (int i = 31; i >= 1; i--)
            {
                if (!m_AllUsedLayers.ContainsKey(i))
                {
                    m_AllUsedLayers[i] = true;
                    layer = i;
                    break;
                }
            }
        }

        Transform[] transfroms = root.gameObject.GetComponentsInChildren<Transform>(true);
        foreach (Transform transform in transfroms)
        {
            transform.gameObject.layer = layer;
        }

        Light[] lights = root.gameObject.GetComponentsInChildren<Light>(true);
        foreach (var light in lights)
        {
            light.cullingMask = 1 << layer;
        }

        Camera[] cameras = root.gameObject.GetComponentsInChildren<Camera>(true);
        foreach (Camera camera in cameras)
        {
            camera.cullingMask = 1 << layer;
        }

        foreach (Collider collider in root.gameObject.GetComponentsInChildren<Collider>(true))
        {
            collider.enabled = layer == 0;
        }

        foreach(PostProcessLayer post in root.gameObject.GetComponentsInChildren<PostProcessLayer>(true))
        {
            post.volumeLayer = 1 << layer;
        }

        m_Layer = layer;
    }
}
