using System;
using System.Collections.Generic;

public class ComponentEventDispatcher
{
    private const uint RECURSIVE_DEPTH_MAX = 10;

    private uint m_RecursiveDepth = 0;

    private Dictionary<ComponentEventName, Action<IComponentEvent>> m_ListenTable = new Dictionary<ComponentEventName, Action<IComponentEvent>>();

    public void AddListener(ComponentEventName eventName, Action<IComponentEvent> handler)
    {
        if (!m_ListenTable.ContainsKey(eventName))
        {
            m_ListenTable.Add(eventName, handler);
        }
        else
        {
            m_ListenTable[eventName] += handler;
        }
    }

    public void SendEvent(ComponentEventName eventName, IComponentEvent entityEvent)
    {
        if (m_ListenTable.ContainsKey(eventName))
        {
            m_RecursiveDepth++;
            if (m_RecursiveDepth > RECURSIVE_DEPTH_MAX)
            {
                throw new Exception("SendEvent Recursive Depth is Max");
            }
            if (m_ListenTable[eventName] == null)
            {
                throw new Exception("m_ListenTable[] is null " + eventName);
            }
            try
            {
                m_ListenTable[eventName]?.Invoke(entityEvent);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogException(ex);
                //throw ex;
            }
            finally
            {
                m_RecursiveDepth--;
            }
        }
    }

    public void RemoveListener(ComponentEventName eventName, Action<IComponentEvent> handler)
    {
        if (m_ListenTable.ContainsKey(eventName))
        {
            m_ListenTable[eventName] -= handler;
        }
    }

    public void RemoveAllListener()
    {
        m_ListenTable.Clear();
    }
}
