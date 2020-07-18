using Assets.Scripts.Proto;
using DebugPanel;
using System;

public abstract class BaseEntityComponent
{
    /// <summary>
    /// 唯一标识
    /// </summary>
    private int m_HashCode = 0;

    /// <summary>
    /// 向当前Entity下的所有Component发送事件
    /// </summary>
    private ComponentEventDispatcher m_ComponentEventDispatcher;

    /// <summary>
    /// 向其他Entity下的所有Component发送事件
    /// </summary>
    private ISendEventToEntity m_SendEventToEntity;

    /// <summary>
    /// 在Initialize调用
    /// </summary>
    public virtual void OnAfterInitialize()
    {

    }

    /// <summary>
    /// 销毁
    /// </summary>
    public virtual void OnDestroy()
    {
        if (m_ComponentEventDispatcher == null)
        {
            throw new Exception("m_ComponentEventDispatcher == null");
        }
        m_ComponentEventDispatcher.RemoveAllListener();
    }

    /// <summary>
    /// Unity的OnUpdate事件
    /// </summary>
    /// <param name="delta"></param>
    public virtual void OnUpdate(float delta)
    {

    }

    /// <summary>
    /// Unity的OnFixedUpdate事件
    /// </summary>
    /// <param name="delta"></param>
    public virtual void OnFixedUpdate()
    {

    }

    /// <summary>
    /// Unity的OnLateUpdate事件
    /// </summary>
    /// <param name="delta"></param>
    public virtual void OnLateUpdate()
    {

    }

	/// <summary>
	/// Unity的OnDrawGizmo事件
	/// </summary>
	public virtual void OnDrawGizmo()
	{

	}

    public void SetEntityEventDispatcher(ComponentEventDispatcher entityEventDispatcher)
    {
        m_ComponentEventDispatcher = entityEventDispatcher;
    }

    /// <summary>
    /// 向其他Entity下的所有Component发送事件
    /// </summary>

    public void SetSendEventToEntity(ISendEventToEntity sendEventToEntity)
    {
        m_SendEventToEntity = sendEventToEntity;
    }

    /// <summary>
    /// 向当前Entity下的所有Component发送事件
    /// </summary>
    protected void SendEvent(ComponentEventName eventName, IComponentEvent entityEvent)
    {
        if (m_ComponentEventDispatcher == null)
        {
            throw new Exception("m_ComponentEventDispatcher == null");
        }
        m_ComponentEventDispatcher.SendEvent(eventName, entityEvent);
    }

    /// <summary>
    /// 添加监听事件
    /// </summary>
    /// <param name="eventName"></param>
    /// <param name="handler"></param>
    protected void AddListener(ComponentEventName eventName, Action<IComponentEvent> handler)
    {
        if (m_ComponentEventDispatcher == null)
        {
            throw new Exception("m_ComponentEventDispatcher == null");
        }
        m_ComponentEventDispatcher.AddListener(eventName, handler);
    }

    /// <summary>
    /// 删除监听事件
    /// </summary>
    /// <param name="eventName"></param>
    /// <param name="handler"></param>
    protected void RemoveListener(ComponentEventName eventName, Action<IComponentEvent> handler)
    {
        if (m_ComponentEventDispatcher == null)
        {
            throw new Exception("m_ComponentEventDispatcher == null");
        }
        m_ComponentEventDispatcher.RemoveListener(eventName, handler);
    }

    /// <summary>
    /// 设置Component唯一标识
    /// </summary>
    /// <typeparam name="DerivedComponentType"></typeparam>
    public void SetHashCode<DerivedComponentType>() where DerivedComponentType : BaseEntityComponent
    {
        m_HashCode = typeof(DerivedComponentType).GetHashCode();
    }

    /// <summary>
    /// 返回Component唯一标识
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode()
    {
        return m_HashCode;
    }

	public virtual void DoGUI(Config config)
	{
	}

	/// <summary>
	/// 向Entity发送事件对象
	/// </summary>
	/// <param name="entityId"></param>
	/// <param name="eventName"></param>
	/// <param name="entityEvent"></param>
	protected void SendEventToEntity(uint entityId, ComponentEventName eventName, IComponentEvent entityEvent)
    {
        m_SendEventToEntity.SendEventToEntity(entityId, eventName, entityEvent);
    }

    /// <summary>
    /// 向Entity发送事件对象
    /// </summary>
    /// <typeparam name="EntityType"></typeparam>
    /// <param name="eventName"></param>
    /// <param name="entityEvent"></param>
    protected void SendEventToEntity<EntityType>(ComponentEventName eventName, IComponentEvent entityEvent) where EntityType : BaseEntity
    {
        m_SendEventToEntity.SendEventToEntity<EntityType>(eventName, entityEvent);
    }

    protected void Log(string str)
    {
        UnityEngine.Debug.LogFormat("[wzc]{0} {1} {2}", "<color=#1EF8E1>", str, "</color>");
    }

    protected void Log(string model, string str)
    {
        UnityEngine.Debug.LogFormat("[wzc][{0}]{1} {2} {3}", model, "<color=#1EF8E1>", str, "</color>");
    }

    protected void Log(string format, params object[] args)
    {
        UnityEngine.Debug.LogFormat("[wzc]{0} {1} {2}", "<color=#1EF8E1>", string.Format(format, args), "</color>");
    }
}

public abstract class EntityComponent<PropertyType> : BaseEntityComponent
{
    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="property"></param>
    public abstract void OnInitialize(PropertyType property);

    /// <summary>
    /// 添加监听事件
    /// </summary>
    public virtual void OnAddListener()
    {

    }

    /// <summary>
    /// 发送协议 TODO:网络层重构
    /// </summary>
    /// <param name="msg"></param>
    protected void SendToGameServer(C2S_HEADER msg)
    {
		NetworkManager.Instance.SendToGameServer(msg);
    }
}

