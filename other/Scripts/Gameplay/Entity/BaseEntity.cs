using Assets.Scripts.Define;
using Assets.Scripts.Proto;
using DebugPanel;
using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseEntity : SkillDataFromBaseEntity // 需要存放在baseEntity 内的 技能数据 * 这么继承，只是都存放到baseEntity 内比较杂乱，放出来一些
{
    public const string LOG_TAG = "Entity";
    public const string LEAP_LOG_TAG = "Leap";

    /// <summary>
    /// 单位唯一索引
    /// </summary>
    public long m_Index { get; set; }

    /// <summary>
    /// 名字
    /// </summary>
    private string m_Name;

    /// <summary>
    /// 等级
    /// </summary>
    private uint m_Level;

    /// <summary>
    /// 类型
    /// </summary>
    protected KHeroType m_HeroType;

    protected bool m_IsMain;

    /// <summary>
    /// 是否可交互
    /// </summary>
    protected bool m_IsActive = true;

    /// <summary>
    /// 唯一ID
    /// </summary>
    private ulong m_EntityId = 0;

    /// <summary>
    /// PlayerId
    /// </summary>
    private ulong m_PlayerId = 0;

    /// <summary>
    /// 模板ID
    /// </summary>
    private uint m_TemplateID = 0;

    /// <summary>
    /// 道具ID
    /// </summary>
    private uint m_ItemID = 0;

    /// <summary>
    /// 事件注册 分发
    /// </summary>
    private ComponentEventDispatcher m_EntityEventDispatcher;

    /// <summary>
    /// 向Entity发送事件对象
    /// </summary>
    private ISendEventToEntity m_SendEventToEntity;

    /// <summary>
    /// Component工厂
    /// </summary>
    private ComponentsFactory m_ComponentsFactory;

    /// <summary>
    /// Component的HashCode集合，用于检测Component是否重复添加
    /// </summary>
    private HashSet<int> m_ComponentHashCodeSet = new HashSet<int>();

    /// <summary>
    /// Component列表
    /// </summary>
    private List<BaseEntityComponent> m_ComponentList = new List<BaseEntityComponent>();

    private ForDebug _ForDebug;

    /// <summary>
    /// 阵营ID
    /// </summary>
    protected uint m_CampID;

    /// <summary>
    /// 移动状态
    /// </summary>
    private KMoveState m_MoveState;

    private Gameplay.Battle.Timeline.PerceptronTarget m_perceptronTarget;

    /// <summary>
    /// Update 本次循环末尾事件
    /// </summary>
    private Action m_OnUpdateEnd;

    /// <summary>
    /// FixedUpdate 本次循环末尾事件
    /// </summary>
    private Action m_OnFixedUpdateEnd;

    /// <summary>
    /// LateUpdate 本次循环末尾事件
    /// </summary>
    private Action m_OnLateUpdateEnd;

    /// <summary>
    /// enity 的所属于哪个父enity 的ID
    /// </summary>
    public uint m_EntityFatherOwnerID;

    /// <summary>
    /// 当前飞船武器跟准星索引信息，key 武器Uid,value 配置名字 这里应该    Dictionary<ulong, string>
    /// </summary>
    private Dictionary<ulong, int> m_CrossSightDic;


    public bool IsDead()
    {
        return GetCurrentState().GetMainState() == EnumMainState.Dead;
    }

    public bool IsLeap()
    {
        return GetCurrentState().IsHasSubState(EnumSubState.LeapPrepare)
            || GetCurrentState().IsHasSubState(EnumSubState.LeapTurnEnd)
            || GetCurrentState().IsHasSubState(EnumSubState.Leaping)
            || GetCurrentState().IsHasSubState(EnumSubState.LeapArrive);
    }

    public Transform GetRootTransform()
    {
        return transform;
    }

    /// <summary>
    /// 阵营ID
    /// </summary>
    /// <returns></returns>
    public uint GetCampID()
    {
        return m_CampID;
    }

    /// <summary>
    /// 当前状态
    /// </summary>
    private readonly HeroState m_CurrentState = new HeroState();

    /// <summary>
    /// 上一个状态
    /// </summary>
    private readonly HeroState m_PreviousState = new HeroState();

    public void InitBaseProperty(S2C_SYNC_NEW_HERO respond)
    {
        m_Name = respond.szPlayerName;
        m_Level = respond.uLevel;
    }

    /// <summary>
    /// 当前状态
    /// </summary>
    public HeroState GetCurrentState()
    {
        return m_CurrentState;
    }

    /// <summary>
    /// 上一个状态
    /// </summary>
    public HeroState GetPreviousState()
    {
        return m_PreviousState;
    }

    public uint GetLevel()
    {
        return m_Level;
    }

    public string GetName()
    {
        return m_Name;
    }

    private void Update()
    {
        OnUpdata();
        if (m_OnUpdateEnd != null)
            m_OnUpdateEnd();
    }

    protected void OnUpdata()
    {
        DispatchUpdate(Time.deltaTime);
    }


    /// <summary>
    /// 调用所有Component的OnUpdate
    /// </summary>
    /// <param name="delta"></param>
    protected void DispatchUpdate(float delta)
    {
        foreach (var component in m_ComponentList)
        {
            component.OnUpdate(delta);
        }
    }

    private void FixedUpdate()
    {
        OnFixedUpdate();

        if (m_OnFixedUpdateEnd != null)
            m_OnFixedUpdateEnd();
    }

    protected void OnFixedUpdate()
    {
        DispatchFixedUpdate();
    }

    /// <summary>
    /// 调用所有Component的OnFixedUpdate
    /// </summary>
    protected void DispatchFixedUpdate()
    {
        foreach (var component in m_ComponentList)
        {
            component.OnFixedUpdate();
        }
    }

    private void LateUpdate()
    {
        OnLateUpdate();

        if (m_OnLateUpdateEnd != null)
            m_OnLateUpdateEnd();
    }

    protected void OnLateUpdate()
    {
        DispatchLateUpdate();
    }

    /// <summary>
    /// 调用所有Component的OnLateUpdate
    /// </summary>
    protected void DispatchLateUpdate()
    {
        foreach (var component in m_ComponentList)
        {
            component.OnLateUpdate();
        }
    }

    /// <summary>
    /// 调用所有Component的OnDrawGizmo
    /// </summary>
    protected void DispatchDrawGizmo()
    {
        foreach (var component in m_ComponentList)
        {
            component.OnDrawGizmo();
        }
    }

    /// <summary>
    /// 获取EntityId
    /// </summary>
    /// <returns></returns>
    public ulong EntityId()
    {
        return m_EntityId;
    }

    /// <summary>
    /// 获取UId
    /// </summary>
    /// <returns></returns>
    public uint UId()
    {
        return (uint)(m_EntityId & 0x00000000ffffffff);
    }

    /// <summary>
    /// 客户端Id
    /// </summary>
    /// <returns></returns>
    public uint ClientId()
    {
        return (uint)(m_EntityId & 0xffffffff00000000);
    }

    /// <summary>
	/// 获取模板ID
	/// </summary>
	/// <returns></returns>
	public uint GetTemplateID()
    {
        return m_TemplateID;
    }

    /// <summary>
    /// 设置模板ID
    /// </summary>
    /// <param name="templateID"></param>
    public void SetTemplateID(uint templateID)
    {
        m_TemplateID = templateID;
    }

    /// <summary>
    /// 获取道具ID
    /// </summary>
    /// <returns></returns>
    public uint GetItemID()
    {
        return m_ItemID;
    }

    /// <summary>
    /// 设置道具ID
    /// </summary>
    /// <param name="itemID"></param>
    public void SetItemID(uint itemID)
    {
        m_ItemID = itemID;
    }

    /// <summary>
    /// 设置EntityId
    /// </summary>
    /// <param name="entityId"></param>
    public void SetEntityId(ulong entityId)
    {
        m_EntityId = entityId;
    }

    /// <summary>
    /// 设置PlayerId
    /// </summary>
    /// <param name="playerId"></param>
    public void SetPlayerId(ulong playerId)
    {
        m_PlayerId = playerId;
    }
    /// <summary>
    /// 设置PlayerId
    /// </summary>
    /// <param name="playerId"></param>
    public ulong GetPlayerId()
    {
        return m_PlayerId;
    }
    /// <summary>
    /// 添加创建Component的HashCode集合
    /// </summary>
    /// <param name="componentsFactory">Component工场</param>
    public abstract void InitializeComponents();

    /// <summary>
    /// 在InitializeComponents之后的调用的初始化
    /// </summary>
    public void AfterInitializeComponents()
    {
        foreach (BaseEntityComponent component in m_ComponentList)
        {
            component.OnAfterInitialize();
        }
    }

    /// <summary>
    /// 返回英雄类型
    /// </summary>
    /// <returns></returns>
    public KHeroType GetHeroType()
    {
        return m_HeroType;
    }

    /// <summary>
    /// TODO.特殊处理出生立马死亡的npc类型
    /// </summary>
    /// <returns></returns>
    public bool IsNotHaveAva()
    {
        switch (m_HeroType)
        {
            case KHeroType.htPreicous:
                return true;
            case KHeroType.htNormalChest:
                return true;
            default:
                return false;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="entityEventDispatcher"></param>
    public void SetEntityEventDispatcher(ComponentEventDispatcher entityEventDispatcher)
    {
        m_EntityEventDispatcher = entityEventDispatcher;
    }

    /// <summary>
    /// 设置向Entity发送事件对象
    /// </summary>
    /// <param name="sendEventToEntity"></param>
    public void SetSendEventToEntity(ISendEventToEntity sendEventToEntity)
    {
        m_SendEventToEntity = sendEventToEntity;
    }

    /// <summary>
    /// 设置组建工厂的引用
    /// </summary>
    /// <param name="componentsFactory"></param>
    public void SetComponentsFactory(ComponentsFactory componentsFactory)
    {
        m_ComponentsFactory = componentsFactory;
    }

    /// <summary>
    /// 发送事件
    /// </summary>
    /// <param name="eventName"></param>
    /// <param name="entityEvent"></param>
    public void SendEvent(ComponentEventName eventName, IComponentEvent entityEvent)
    {
        m_EntityEventDispatcher.SendEvent(eventName, entityEvent);
    }

    /// <summary>
    /// 是否拥有Component
    /// </summary>
    /// <typeparam name="EntityComponentType"></typeparam>
    /// <returns></returns>
    public bool IsHaveComponent<EntityComponentType>() where EntityComponentType : BaseEntityComponent
    {
        int targetHashCode = typeof(EntityComponentType).GetHashCode();
        return m_ComponentHashCodeSet.Contains(targetHashCode);
    }

    public void DoGUI(Config config)
    {
#if UNITY_EDITOR
        if (config.IsEditor)
        {
            _ForDebug.Foldout = UnityEditor.EditorGUILayout.Foldout(_ForDebug.Foldout
                , string.Format("{0}{1} - {2}"
                    , IsMain()
                        ? "Main "
                        : ""
                    , UId()
                    , m_HeroType));
        }

        if (_ForDebug.Foldout
            || !config.IsEditor)
#endif
        {
            DoGUIOverride(config);

            for (int iComponent = 0; iComponent < m_ComponentList.Count; iComponent++)
            {
                m_ComponentList[iComponent].DoGUI(config);
            }
        }
    }

    public bool IsMain()
    {
        return m_IsMain;
    }

    /// <summary>
    /// 设置是否可交互
    /// </summary>
    /// <param name="isActive"></param>
    public void SetIsActive(bool isActive)
    {
        m_IsActive = isActive;
    }

    /// <summary>
    /// 返回交互状态
    /// </summary>
    /// <returns></returns>
    public bool GetIsActive()
    {
        return m_IsActive;
    }

    protected virtual void DoGUIOverride(Config config)
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

    /// <summary>
    /// 删除所有事件监听
    /// </summary>
    protected void RemoveAllListener()
    {
        m_EntityEventDispatcher.RemoveAllListener();
    }

    /// <summary>
    /// 移除所有Component
    /// </summary>
    protected void RemoveAllComponents()
    {
        foreach (var component in m_ComponentList)
        {
            component.OnDestroy();
        }

        m_ComponentList.Clear();
    }

    /// <summary>
    /// 销毁Entity时调用
    /// </summary>
    public virtual void OnRemoveEntity()
    {
        RemoveAllListener();
        RemoveAllComponents();
        Destroy(gameObject);
    }



    /// <summary>
    /// 添加Component
    /// </summary>
    /// <typeparam name="EntityComponentType"></typeparam>
    /// <param name="component"></param>
    /// <returns></returns>
    protected EntityComponentType AddEntityComponent<EntityComponentType, PropertyType>(PropertyType property) where EntityComponentType : EntityComponent<PropertyType>, new()
    {
        EntityComponentType component = m_ComponentsFactory.CreateComponent<EntityComponentType, PropertyType>(property, m_EntityEventDispatcher, m_SendEventToEntity);
        if (component == null)
        {
            return null;
        }

        int key = component.GetHashCode();
        if (m_ComponentHashCodeSet.Add(key))
        {
            m_ComponentList.Add(component);

            return component;
        }

        return null;
    }

    /// <summary>
    /// 获取Component
    /// </summary>
    /// <typeparam name="EntityComponentType"></typeparam>
    /// <returns></returns>
    public EntityComponentType GetEntityComponent<EntityComponentType>() where EntityComponentType : BaseEntityComponent
    {
        int targetHashCode = typeof(EntityComponentType).GetHashCode();
        foreach (BaseEntityComponent component in m_ComponentList)
        {
            if (targetHashCode == component.GetHashCode())
            {
                return component as EntityComponentType;
            }
        }

        return null;
    }

    private struct ForDebug
    {
#if UNITY_EDITOR
        public bool Foldout;
#endif
    }

    public void SetPerceptronTarget(Gameplay.Battle.Timeline.PerceptronTarget perceptronTarget)
    {
        m_perceptronTarget = perceptronTarget;
    }

    public Gameplay.Battle.Timeline.PerceptronTarget GetPerceptronTarget()
    {
        return m_perceptronTarget;
    }

    public BaseEntity GetOwner()
    {
        return this;
    }

    /// <summary>
    /// Update 本次循环末尾事件
    /// </summary>
    public void SetOnUpdateEnd(Action action)
    {
        m_OnUpdateEnd = action;
    }

    /// <summary>
    /// FixedUpdate 本次循环末尾事件
    /// </summary>
    public void SetOnFixedUpdateEnd(Action action)
    {
        m_OnFixedUpdateEnd = action;
    }

    /// <summary>
    /// LateUpdate 本次循环末尾事件
    /// </summary>
    public void SetOnLateUpdateEnd(Action action)
    {
        m_OnLateUpdateEnd = action;
    }

    /// <summary>
    ///  enity 的所属于哪个父enity 的ID
    /// </summary>
    /// <returns></returns>
    public uint GetEntityFatherOwnerID()
    {
        return m_EntityFatherOwnerID;
    }


    /// <summary>
    /// 重置保存武器&准星对应关系
    /// </summary>
    /// <param name="weapons"></param>
    public void ResetSaveCrossSightDic(Dictionary<ulong, int> crossSightDic)
    {
        m_CrossSightDic = crossSightDic;
    }

    /// <summary>
    /// 重置保存武器&准星对应关系
    /// </summary>
    /// <param name="weapons"></param>
    public int GetCrossSightByWeaponUId(ulong weaponUId)
    {
        if (m_CrossSightDic.ContainsKey(weaponUId))
            return m_CrossSightDic[weaponUId];
        else
            return -1;
    }
}