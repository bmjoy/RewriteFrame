using System.Collections.Generic;
using UnityEngine.Events;

public class UIViewAction
{
    /// <summary>
    /// 所属状态
    /// </summary>
    private UIViewState m_State;
    /// <summary>
    /// ID
    /// </summary>
    private string m_ID;
    /// <summary>
    /// 可见性
    /// </summary>
    private bool m_Visible = true;
    /// <summary>
    /// 可用性
    /// </summary>
    private bool m_Enabled = true;
    /// <summary>
    /// 状态ID
    /// </summary>
    private int m_StateID = 0;
    /// <summary>
    /// 状态列表
    /// </summary>
    public List<UIViewActionState> StateList = new List<UIViewActionState>();
    /// <summary>
    /// 热键的回调
    /// </summary>
    public UnityAction<HotkeyCallback> Callback;

    public UIViewAction(string id, UIViewState state)
    {
        m_ID = id;
        m_State = state;
    }

    public string ID
    {
        get { return m_ID; }
    }

    /// <summary>
    /// 热键可见性
    /// </summary>
    public bool Visible
    {
        get { return m_Visible; }
        set
        {
            //if(m_Visible!=value)
            {
                m_Visible = value;
                m_State.OnActionVisibleChanged?.Invoke(m_ID, m_Visible);
            }
        }
    }
    /// <summary>
    /// 热键可用性
    /// </summary>
    public bool Enabled
    {
        get { return m_Enabled; }
        set
        {
            //if(m_Enabled!=value)
            {
                m_Enabled = value;
                m_State.OnActionEnableChanged?.Invoke(m_ID, m_Enabled);
            }
        }
    }

    /// <summary>
    /// 状态ID
    /// </summary>
    public int State
    {
        get { return m_StateID; }
        set
        {
            //if(m_StateID != value)
            {
                m_StateID = value;
                m_State.OnActionStateChanged?.Invoke(m_ID);
            }
        }
    }

    /// <summary>
    /// 发布事件
    /// </summary>
    /// <param name="callback">热键状态</param>
    public void FireEvent(HotkeyCallback callback)
    {
        Callback?.Invoke(callback);
    }
}
