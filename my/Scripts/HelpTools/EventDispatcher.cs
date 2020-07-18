using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 消息传递
/// </summary>
public class EventDispatcher
{
    public delegate void EventHandler(params object[] objs);
    private Dictionary<EventEnum, EventHandler> listeners = new Dictionary<EventEnum, EventHandler>();

    private static EventDispatcher globalDispatcher = new EventDispatcher();
    public static EventDispatcher Global
    {
        get
        {
            return globalDispatcher;
        }
    }

    void AddEventListener(EventEnum evt, EventHandler handler)
    {
        if (handler == null)
        {
            Debug.LogWarningFormat("Cannot bind a null handler with message {0}", evt);
            return;
        }

        if (listeners.ContainsKey(evt))
            listeners[evt] += handler;
        else
            listeners.Add(evt, handler);
    }

    void RemoveEventListener(EventEnum evt, EventHandler handler)
    {
        if (handler == null)
        {
            Debug.LogWarningFormat("Cannot unbind a null handler with event {0}", evt);
            return;
        }

        if (listeners.ContainsKey(evt))
        {
            listeners[evt] -= handler;
            if (listeners[evt] == null)
                listeners.Remove(evt);
        }
        else
        {
            Debug.LogWarningFormat("There is no handler {0} bind with event {1}", handler.ToString(), evt);
        }
    }

    private const int MAX_STACK_DEEP = 8;
    private int stackDeep = 0;
    private readonly string szErrorMessage = "DispatchEvent Error, Event:{0}, Error:{1}\n{2}";

    public void DispatchEvent(EventEnum evt, params object[] objs)
    {

        try
        {
            stackDeep++;
            if (stackDeep > MAX_STACK_DEEP) // 避免形成消息环
                throw new System.StackOverflowException("Event stack overflow");

            if (listeners.ContainsKey(evt))
            {
                listeners[evt](objs);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogErrorFormat(szErrorMessage, evt, e.Message, e.StackTrace);
        }
        finally
        {
            stackDeep--;
        }
    }

    public void Regist(EventEnum evt, EventHandler handler)
    {
        AddEventListener(evt, handler);
    }

    public void Unregist(EventEnum evt, EventHandler handler)
    {
        RemoveEventListener(evt, handler);
    }

    public bool HasEvent(EventEnum evt)
    {
        return listeners.ContainsKey(evt);
    }

    public void ClearEvent(EventEnum evt)
    {
        if (HasEvent(evt))
        {
            listeners[evt] = null;
            listeners.Remove(evt);
        }
    }
}
