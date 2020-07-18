using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class UnityCollisionEventProxy : MonoBehaviour
{
    public static List<int> FilterLayerList = new List<int> {
        GameConstant.LayerTypeID.SceneOnly,
        GameConstant.LayerTypeID.SkillCrossSpacecraftBlock,
    };

    private Action<Collision> m_OnCollisionEnterAction = delegate { };
    private Action<Collision> m_OnCollisionStayAction = delegate { };
    private Action<Collision> m_OnCollisionExitAction = delegate { };

    public void AddOnCollisionCallback(Action<Collision> enter, Action<Collision> stay, Action<Collision> exit)
    {
        m_OnCollisionEnterAction += enter;
        m_OnCollisionStayAction += stay;
        m_OnCollisionExitAction += exit;
    }

    public void RemoveCollisionCallback(Action<Collision> enter, Action<Collision> stay, Action<Collision> exit)
    {
        m_OnCollisionEnterAction -= enter;
        m_OnCollisionStayAction -= stay;
        m_OnCollisionExitAction -= exit;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (Filter(collision))
        {
            return;
        }

        m_OnCollisionEnterAction.Invoke(collision);
    }

    private void OnCollisionStay(Collision collision)
    {
        if (Filter(collision))
        {
            return;
        }

        m_OnCollisionStayAction.Invoke(collision);
    }

    private void OnCollisionExit(Collision collision)
    {
        if (Filter(collision))
        {
            return;
        }

        m_OnCollisionExitAction.Invoke(collision);
    }

    private bool Filter(Collision collision)
    {
        return !FilterLayerList.Contains(collision.collider.gameObject.layer);
    }
}
