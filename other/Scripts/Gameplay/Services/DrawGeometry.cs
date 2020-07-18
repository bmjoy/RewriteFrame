using System.Collections.Generic;
using UnityEngine;

public sealed class DrawGeometry : Singleton<DrawGeometry>
{
    readonly private List<GameObject> m_GeometryGameObjectList = new List<GameObject>();

    private bool m_IsEnable = false;

    public void SwitchEnable()
    {
        m_IsEnable = !m_IsEnable;
    }

    public void Clear()
    {
        foreach (var item in m_GeometryGameObjectList)
        {
            Destroy(item);
        }

        m_GeometryGameObjectList.Clear();
    }

    public void DrawPrimitive(PrimitiveType type, Vector3 center, Vector3 scale, Quaternion roatation)
    {
        if (!m_IsEnable)
        {
            return;
        }

        GameObject geometry = GameObject.CreatePrimitive(type);
        geometry.transform.localPosition = center;
        geometry.transform.localScale = scale;
        geometry.transform.localRotation = roatation;
        geometry.GetComponent<Collider>().enabled = false;
        if (type == PrimitiveType.Capsule)
        {
            geometry.GetComponent<CapsuleCollider>().direction = 2;
        }

        m_GeometryGameObjectList.Add(geometry);
    }

    public void DrawDirection(Vector3 startPoint, Quaternion rotation, Color color)
    {
        if (!m_IsEnable)
        {
            return;
        }

        GameObject parent = GameObject.CreatePrimitive(PrimitiveType.Cube);

        GameObject arrow = GameObject.CreatePrimitive(PrimitiveType.Cube);
        arrow.transform.parent = parent.transform;
        arrow.transform.localPosition = new Vector3(0, 0, 1);
        arrow.transform.localRotation = Quaternion.identity;
        arrow.transform.localScale = new Vector3(0.5f, 0.5f, 3);

        parent.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);

        foreach (var item in parent.GetComponentsInChildren<Collider>())
        {
            item.enabled = false;
        }

        foreach (var item in arrow.GetComponentsInChildren<Renderer>())
        {
            item.material.color = color;
        }

        parent.transform.localPosition = startPoint;
        parent.transform.localRotation = rotation;

        m_GeometryGameObjectList.Add(parent);
    }
}
