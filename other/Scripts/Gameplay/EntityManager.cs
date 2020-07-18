using Assets.Scripts.Lib.Net;
using DebugPanel;
using Leyoutech.Core.Generic;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public interface ISendEventToEntity
{
	void SendEventToEntity(uint entityId, ComponentEventName eventName, IComponentEvent entityEvent);
	void SendEventToEntity<EntityType>(ComponentEventName eventName, IComponentEvent entityEvent) where EntityType : BaseEntity;
}

public class EntityManager : ISendEventToEntity
{
    /// <summary>
    /// 唯一ID
    /// </summary>
    private UniqueIDCreator m_IdCreator = new UniqueIDCreator();

    /// <summary>
    /// 当前客户端索引
    /// </summary>
    private uint m_CurrentClientIndex = 0;

	/// <summary>
	/// 所有EntityId对应Entity的表
	/// </summary>
	private Dictionary<ulong, BaseEntity> m_EntityTable = new Dictionary<ulong, BaseEntity>();

	/// <summary>
	/// 
	/// </summary>
	private Dictionary<Type, Dictionary<ulong, BaseEntity>> m_EntityTypeTable = new Dictionary<Type, Dictionary<ulong, BaseEntity>>();

	/// <summary>
	/// Component工厂
	/// </summary>
	private ComponentsFactory m_ComponentsFactory;

	/// <summary>
	/// Entity根节点
	/// </summary>
	private Transform m_EntityRoot;

	private ForDebug _ForDebug;

	public EntityManager(ComponentsFactory componentsFactory)
	{
		m_ComponentsFactory = componentsFactory;

		m_EntityRoot = new GameObject("EntityRoot").transform;

		DebugPanelInstance.GetInstance().RegistGUI(TabName.Entity, DoGUI, true);
	}

	public void SwitchMap(Scene scene)
	{
		RemoveAllEntity();

		GameObject.Destroy(m_EntityRoot.gameObject);

		m_EntityRoot = new GameObject("EntityRoot").transform;
		SceneManager.MoveGameObjectToScene(m_EntityRoot.gameObject, scene);
	}

	/// <summary>
	/// 创建Entity
	/// </summary>
	/// <typeparam name="EntityType">Entity类型</typeparam>
	/// <typeparam name="RespondType">服务器返回协议类型</typeparam>
	/// <typeparam name="VOType">数据类型</typeparam>
	/// <param name="uId">服务器ID</param>
	/// <param name="tId">模板ID</param>
	/// <param name="respond">服务器返回协议</param>
	/// <returns></returns>
	public EntityType CreateEntityByRespond<EntityType, RespondType>(uint uId, uint tId, uint itemId, ulong playerId, RespondType respond) where EntityType : GameEntity<RespondType> where RespondType : KProtoBuf
	{
		ulong entityId = uId;

		if (respond == null)
		{
			throw new Exception("respond is null");
		}

		GameObject gameObject = new GameObject(typeof(EntityType).Name + "_" + entityId);
		gameObject.transform.SetParent(m_EntityRoot);

		ComponentEventDispatcher componentEventDispatcher = new ComponentEventDispatcher();

		EntityType entity = gameObject.AddComponent<EntityType>();
        entity.m_Index = m_IdCreator.Next();
        entity.SetEntityId(entityId);
		entity.SetTemplateID(tId);
        entity.SetItemID(itemId);
		entity.SetSendEventToEntity(this);
		entity.SetEntityEventDispatcher(componentEventDispatcher);
		entity.SetComponentsFactory(m_ComponentsFactory);
		entity.SetPlayerId(playerId);

		entity.InitializeByRespond(respond);
        entity.InitializeComponents();
        entity.AfterInitializeComponents();

		AddEntity<EntityType, RespondType>(entityId, entity);

		return entity;
	}

    /// <summary>
    /// 创建子弹Entity
    /// </summary>
    /// <typeparam name="EntityType">Entity类型</typeparam>
    /// <param name="tId">模板ID</param>
    /// <returns></returns>
    public EntityType CreateBulletEntity<EntityType, RespondType>(RespondType respond) where EntityType : GameEntity<RespondType> where RespondType : KProtoBuf
    {
        ulong entityId = GetClientId();

        GameObject gameObject = new GameObject(typeof(EntityType).Name + "_" + entityId);
        gameObject.transform.SetParent(m_EntityRoot,false);
        ComponentEventDispatcher componentEventDispatcher = new ComponentEventDispatcher();

        EntityType entity = gameObject.AddComponent<EntityType>();
        entity.m_Index = m_IdCreator.Next();
        gameObject .name = string.Format("{0}_{1}", typeof(EntityType).Name , entity.m_Index);
        entity.SetEntityId(entityId);
        entity.SetSendEventToEntity(this);
        entity.SetEntityEventDispatcher(componentEventDispatcher);
        entity.SetComponentsFactory(m_ComponentsFactory);

        entity.InitializeByRespond(respond);
        try
        {
            entity.InitializeComponents();
        }
        catch (Exception ex)
        {
            Debug.LogError("InitializeComponents Error " + ex);
            throw;
        }
        entity.AfterInitializeComponents();

        AddEntity<EntityType, RespondType>(entityId, entity);

        return entity;
    }


    /// <summary>
    /// 创建Entity
    /// </summary>
    /// <typeparam name="EntityType">Entity类型</typeparam>
    /// <param name="tId">模板ID</param>
    /// <returns></returns>
    public EntityType CreateEntity<EntityType>(GameObject gameObject = null) where EntityType : GameEntity<KProtoBuf>
	{
		ulong entityId = GetClientId();

		if (gameObject == null)
		{
			gameObject = new GameObject(typeof(EntityType).Name + "_" + entityId);
			gameObject.transform.SetParent(m_EntityRoot);
		}

		ComponentEventDispatcher componentEventDispatcher = new ComponentEventDispatcher();

		EntityType entity = gameObject.AddComponent<EntityType>();
        entity.m_Index = m_IdCreator.Next();
        entity.SetEntityId(entityId);
		entity.SetSendEventToEntity(this);
		entity.SetEntityEventDispatcher(componentEventDispatcher);
		entity.SetComponentsFactory(m_ComponentsFactory);

		entity.Initialize();
		entity.InitializeComponents();
		entity.AfterInitializeComponents();

		AddEntity<EntityType, KProtoBuf>(entityId, entity);

		return entity;
	}

	/// <summary>
	/// 删除Entity
	/// </summary>
	/// <param name="EntityId">唯一ID</param>
	public void RemoveEntity(ulong entityId)
	{
		BaseEntity entity;
		if (m_EntityTable.TryGetValue(entityId, out entity))
		{
			DropItemManager.Instance.RemoveInfo(entityId);
			entity.OnRemoveEntity();

			m_EntityTable.Remove(entityId);
			m_EntityTypeTable[entity.GetType()].Remove(entityId);

            GameFacade.Instance.SendNotification(NotificationName.MSG_ENTITY_ON_REMOVE, null);
        }
    }

    /// <summary>
    /// 获取所有Entity
    /// </summary>
    /// <returns></returns>
    public Dictionary<ulong, BaseEntity>GetAllEntities()
    {
        return m_EntityTable;
    }

    /// <summary>
    /// 获取Entity
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="entityId"></param>
    /// <returns></returns>
    public EntityType GetEntityById<EntityType>(uint entityId) where EntityType : BaseEntity
	{
		BaseEntity entity = null;
		if (m_EntityTable.TryGetValue(entityId, out entity))
		{
			return entity as EntityType;
		}

		return null;
	}

    /// <summary>
    /// 获取一类Entity
    /// </summary>
    /// <typeparam name="EntityType"></typeparam>
    /// <returns></returns>
    public Dictionary<ulong, BaseEntity> GetEntities<EntityType>() where EntityType : BaseEntity
    {
        Dictionary<ulong, BaseEntity> keyValuePairs;
        if (m_EntityTypeTable.TryGetValue(typeof(EntityType), out keyValuePairs))
        {
            return keyValuePairs;
        }

        return null;
    }

	/// <summary>
	/// 获取Entity通过是否拥有某类Component
	/// </summary>
	/// <typeparam name="EntityComponentType"></typeparam>
	/// <returns></returns>
	public List<BaseEntity> GetEntitiesByComponent<EntityComponentType>() where EntityComponentType : BaseEntityComponent
	{
		List<BaseEntity> entities = new List<BaseEntity>();
		foreach (var entity in m_EntityTable.Values)
		{
			if (entity.IsHaveComponent<EntityComponentType>())
			{
				entities.Add(entity);
			}
		}

		return entities;
	}

	/// <summary>
	/// 删除所有Entity
	/// </summary>
	public void RemoveAllEntity()
	{
		foreach (var entity in m_EntityTable.Values)
		{
			entity.OnRemoveEntity();
		}

		m_EntityTable.Clear();
		m_EntityTypeTable.Clear();
	}

	/// <summary>
	/// 像一类Entity发送事件
	/// </summary>
	/// <typeparam name="EntityType"></typeparam>
	/// <param name="eventName"></param>
	/// <param name="entityEvent"></param>
	public void SendEventToEntity<EntityType>(ComponentEventName eventName, IComponentEvent entityEvent) where EntityType : BaseEntity
	{
		Dictionary<ulong, BaseEntity> keyValuePairs;
		if (m_EntityTypeTable.TryGetValue(typeof(EntityType), out keyValuePairs))
		{
			foreach (var entity in keyValuePairs.Values)
			{
				entity.SendEvent(eventName, entityEvent);
			}
		}
	}

	/// <summary>
	/// 向Entity发送事件
	/// </summary>
	/// <param name="entityId"></param>
	/// <param name="eventName"></param>
	/// <param name="entityEvent"></param>
	public void SendEventToEntity(uint entityId, ComponentEventName eventName, IComponentEvent entityEvent)
	{
		BaseEntity entity = GetEntityById<BaseEntity>(entityId);
		if (entity != null)
		{
			entity.SendEvent(eventName, entityEvent);
		}
	}

	/// <summary>
	/// 获取客户端Id
	/// </summary>
	/// <returns></returns>
	private ulong GetClientId()
	{
		return (ulong)uint.MaxValue + 1 + (++m_CurrentClientIndex);
	}

	private void AddEntity<EntityType, RespondType>(ulong entityId, EntityType entity)
		where EntityType : GameEntity<RespondType>
		where RespondType : KProtoBuf
	{
		m_EntityTable.Add(entityId, entity);

		Dictionary<ulong, BaseEntity> keyValuePairs;
		if (!m_EntityTypeTable.TryGetValue(typeof(EntityType), out keyValuePairs))
		{
			keyValuePairs = new Dictionary<ulong, BaseEntity>();
			m_EntityTypeTable.Add(typeof(EntityType), keyValuePairs);
		}
		keyValuePairs.Add(entityId, entity);

		switch (entity)
		{
			case HumanEntity val:
				{
					MsgEntityInfo msg = MessageSingleton.Get<MsgEntityInfo>();
					msg.Uid = val.UId();
					msg.Tid = val.GetTemplateID();
					msg.IsMain = val.IsMain();
					GameFacade.Instance.SendNotification(NotificationName.MSG_HUMAN_ENTITY_ON_ADDED, msg);
				}
				break;
			case SpacecraftEntity val:
				{
					MsgEntityInfo msg = MessageSingleton.Get<MsgEntityInfo>();
					msg.Uid = val.UId();
					msg.Tid = val.GetTemplateID();
					msg.IsMain = val.IsMain();
					GameFacade.Instance.SendNotification(NotificationName.MSG_HUMAN_ENTITY_ON_ADDED, msg);
				}
				break;
		}
	}

	private void DoGUI(Config config)
	{
#if UNITY_EDITOR
		if (config.IsEditor)
		{
			foreach (KeyValuePair<ulong, BaseEntity> kv in m_EntityTable)
			{
				BaseEntity baseEntity = kv.Value;
				baseEntity.DoGUI(config);
				UnityEditor.EditorGUILayout.Space();
			}
		}
		else
#endif
		{
			config.BeginToolbarHorizontal();
			float rowRaminWidth = config.PanelWidth * 0.64f;
			foreach (KeyValuePair<ulong, BaseEntity> kv in m_EntityTable)
			{
				BaseEntity baseEntity = kv.Value;

				string display = string.Format("{0}{1} - {2}"
					, baseEntity.IsMain()
						? "Main "
						: ""
					, baseEntity.UId()
					, baseEntity.GetHeroType());

				rowRaminWidth -= config.CalcToolbarButtonWidth(display);
				if (rowRaminWidth < 0)
				{
					rowRaminWidth = config.PanelWidth * 0.9f;
                    config.EndHorizontal();
					config.BeginToolbarHorizontal();
				}

				if (config.ToolbarButton(baseEntity == _ForDebug.SelectedEntity, display))
				{
					_ForDebug.SelectedEntity = baseEntity;
				}
			}
            config.EndHorizontal();
		}

		if (!config.IsEditor
			&& _ForDebug.SelectedEntity != null)
		{
			_ForDebug.SelectedEntity.DoGUI(config);
		}
	}

	private struct ForDebug
	{
		public BaseEntity SelectedEntity;
	}
}