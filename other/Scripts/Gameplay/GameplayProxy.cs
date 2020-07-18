using Assets.Scripts.Define;
using Eternity.FlatBuffer.Enums;
using PureMVC.Patterns.Proxy;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GameplayProxy : Proxy
{
    /// <summary>
    /// 玩家客户端坐标的绝对值最大值
    /// </summary>
    private const float MaxOffset = 1000f;

	/// <summary>
	/// 射线检测的最大长度
	/// </summary>
	private const float RAYCAST_LENGTH = 10f;

	public UnityEngine.SceneManagement.Scene GameMainScene;

	/// <summary>
	/// 缓存一个主角的UID
	/// </summary>
	private uint m_MainPlayerUID;

    /// <summary>
    /// Entity管理类
    /// </summary>
    private EntityManager m_EntityManager;

    /// <summary>
    /// 玩家相对于起始服务器位置服务器坐标的偏移量
    /// </summary>
    private Vector3 m_TotalPositionOffset;

    /// <summary>
    /// 导航网格
    /// </summary>
    private NavMeshPath m_NavMeshPath;

	/// <summary>
	/// [EntityUID, 所有OwnerHeroID == EntityUID的Entity]
	/// 目前用于矿物的分组
	/// </summary>
	private Dictionary<ulong, List<SpacecraftEntity>> m_EntityGroupTable;

	public GameplayProxy() : base(ProxyName.GameplayProxy)
    {
    }

	public uint GetMainPlayerUID()
	{
		return m_MainPlayerUID;
	}

	public void SetMainPlayerUID(uint uid)
	{
		m_MainPlayerUID = uid;
	}

	public SpacecraftEntity GetMainPlayer()
	{
		return m_EntityManager.GetEntityById<SpacecraftEntity>(m_MainPlayerUID);
	}

    /// 临时代码
    public bool IsHasLeapBreak()
    {
        SpacecraftEntity spacecraftEntity = GetMainPlayer();
        if (spacecraftEntity != null)
        {
            BehaviorController behaviorController = spacecraftEntity.GetSyncTarget().gameObject.GetComponent<BehaviorController>();
            if (behaviorController != null)
            {
                return behaviorController.HasBehaviorType(BehaviorController.BehaviorType.BT_LeapBreak);
            }
        }

        return false;
    }

	public void Initialize(EntityManager entityManager)
    {
        m_EntityManager = entityManager;
		m_EntityGroupTable = new Dictionary<ulong, List<SpacecraftEntity>>();

		m_NavMeshPath = new NavMeshPath();
    }

    public void Clear()
    {
        ClearTotalPositionOffset();
    }

    public void ClearTotalPositionOffset()
    {
        m_TotalPositionOffset = Vector3.zero;
        Log("wzc", $"ClearTotalPositionOffset m_TotalPositionOffset={m_TotalPositionOffset}");
    }

    public NavMeshPath GetNavMeshPath()
    {
        return m_NavMeshPath;
    }

	/// <summary>
	/// 获取Entity
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="entityId"></param>
	/// <returns></returns>
	public EntityType GetEntityById<EntityType>(uint entityId) where EntityType : BaseEntity
	{
		return m_EntityManager.GetEntityById<EntityType>(entityId);
	}

    /// <summary>
    /// 获取主玩家皮肤节点
    /// </summary>
    /// <returns></returns>
    public Transform GetMainPlayerSkinTransform()
    {
        CfgEternityProxy cfgEternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
        if (!cfgEternityProxy.IsSpace())
        {
            HumanEntity humanEntity = GetEntityById<HumanEntity>(GetMainPlayerUID());
            if (humanEntity != null)
            {
                return humanEntity.GetSkinTransform();
            }
        }
        else
        {
            SpacecraftEntity spacecraftEntity = GetEntityById<SpacecraftEntity>(GetMainPlayerUID());
            if (spacecraftEntity != null)
            {
                return spacecraftEntity.GetSkinTransform();
            }
        }

        return null;
	}


    /// <summary>
    /// 获取态皮肤节点通过模板ID
    /// </summary>
    /// <param name="templateID">模板ID</param>
    /// <param name="heroType">类型</param>
    /// <returns></returns>
    public Transform GetSkinTransformByTemplateID(int templateID, KHeroType heroType)
    {
        CfgEternityProxy cfgEternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
        if (cfgEternityProxy.IsSpace())
        {
            Dictionary<ulong, BaseEntity> entitys = m_EntityManager.GetEntities<SpacecraftEntity>();
            if (entitys != null)
            {
                foreach (SpacecraftEntity spacecraftEntity in entitys.Values)
                {
                    if (spacecraftEntity.GetTemplateID() == templateID && spacecraftEntity.GetHeroType() == heroType)
                    {
                        return spacecraftEntity.GetSkinTransform();
                    }
                }
            }
        }
        else
        {
            Dictionary<ulong, BaseEntity> entitys = m_EntityManager.GetEntities<HumanEntity>();
            if (entitys != null)
            {
                foreach (HumanEntity humanEntity in entitys.Values)
                {
                    if (humanEntity.GetTemplateID() == templateID && humanEntity.GetHeroType() == heroType)
                    {
                        return humanEntity.GetSkinTransform();
                    }
                }
            }
        }

        return null;
    }

    /// <summary>
    /// 返回某类Entity的HeroType类型的List heroType为KHeroType.htInvalid是全部类型
    /// </summary>
    /// <typeparam name="EntityType">Entity类型</typeparam>
    /// <param name="heroType"></param>
    /// <returns></returns>
    public List<EntityType> GetEntities<EntityType>(KHeroType heroType = KHeroType.htInvalid) where EntityType : BaseEntity
    {
        List<EntityType> list = new List<EntityType>();
        if (m_EntityManager != null)
        {
            Dictionary<ulong, BaseEntity> entitys = m_EntityManager.GetEntities<EntityType>();
            if (entitys != null)
            {
                var values = entitys.Values;
                if (heroType == KHeroType.htInvalid)
                {
                    foreach (var entity in values)
                    {
                        list.Add(entity as EntityType);
                    }
                }
                else
                {
                    foreach (var entity in values)
                    {
                        if (entity.GetHeroType() == heroType)
                        {
                            list.Add(entity as EntityType);
                        }
                    }
                }
            }
        }
        return list;
	}

	/// <summary>
	/// 获取所有OwnerHeroID相同的Entity, 即同一个主人的召唤物
	/// </summary>
	/// <typeparam name="EntityType"></typeparam>
	/// <param name="heroType"></param>
	/// <returns></returns>
	public List<SpacecraftEntity> GetEntitiesByOwnerHeroID(uint ownerHeroID)
	{
		List<SpacecraftEntity> list = new List<SpacecraftEntity>();
		if (m_EntityManager != null)
		{
			Dictionary<ulong, BaseEntity> entitys = m_EntityManager.GetEntities<SpacecraftEntity>();
			if (entitys != null)
			{
				var values = entitys.Values;
				foreach (var entity in values)
				{
					SpacecraftEntity spacecraftEntity = entity as SpacecraftEntity;
					if (spacecraftEntity.m_EntityFatherOwnerID == ownerHeroID)
					{
						list.Add(entity as SpacecraftEntity);
					}
				}
			}
		}
		return list;
	}

	/// <summary>
	/// 获取除自己以外的所有玩家
	/// </summary>
	/// <returns></returns>
	public List<BaseEntity> GetPlayEntitiesExcludeSelf()
    {
        List<BaseEntity> list = new List<BaseEntity>();
        if (m_EntityManager != null)
        {
            Dictionary<ulong, BaseEntity> entitys = m_EntityManager.GetAllEntities();
            if (entitys != null)
            {
                foreach (var entity in entitys.Values)
                {
                    if (entity.GetHeroType() == KHeroType.htPlayer
                        && entity.UId() != GetMainPlayerUID())
                    {
                        list.Add(entity);
                    }
                }
            }
        }

        return list;
    }

    public void SetPositionOffset(Vector3 offset)
    {
        List<BaseEntity> entities = m_EntityManager.GetEntitiesByComponent<SetOffsetComponent>();
        foreach (var entity in entities)
        {
            entity.SendEvent(ComponentEventName.SetOffset, new SetOffsetEvent() { Offset = offset });
        }
    }

    public Vector3 ServerAreaOffsetToClientPosition(Vector3 serverPosition)
    {
        return serverPosition - m_TotalPositionOffset;
    }

    public Vector3 ClientToServerAreaOffset(Vector3 clinetPosition)
    {
        return clinetPosition + m_TotalPositionOffset;
    }

    public bool ServerAreaOffsetPositionToWorldPosition(out Vector3 worldPosition, Vector3 areaOffsetPosition)
    {
        return ServerAreaOffsetPositionToWorldPosition(out worldPosition, areaOffsetPosition, m_CurrentArarUid);
    }

    public bool ServerAreaOffsetPositionToWorldPosition(out Vector3 worldPosition, Vector3 areaOffsetPosition, ulong areaId)
    {
        worldPosition = Vector3.zero;

        Map.AreaInfo areaInfo = Map.MapManager.GetInstance().FindAreaInfoByUidFromCurrentMap(areaId);
        if (areaInfo != null)
        {
            worldPosition = areaInfo.Position + areaOffsetPosition;
            return true;
        }

        return false;
    }

    public Vector3 WorldPositionToServerAreaOffsetPosition(Vector3 worldPosition)
    {
        return WorldPositionToServerAreaOffsetPosition(worldPosition, m_CurrentArarUid);
    }

    public Vector3 WorldPositionToServerAreaOffsetPosition(Vector3 worldPosition, ulong areaId)
    {
        Map.AreaInfo areaInfo = Map.MapManager.GetInstance().FindAreaInfoByUidFromCurrentMap(areaId);
        if (areaInfo != null)
        {
            return worldPosition - areaInfo.Position;
        }

        return Vector3.zero;
    }

    public bool ClientPositionToWorldPosition(out Vector3 worldPosition, Vector3 clientPosition)
    {
        return ServerAreaOffsetPositionToWorldPosition(out worldPosition, ClientToServerAreaOffset(clientPosition));
    }

    public Vector3 WorldPositionToClientPosition(Vector3 worldPosition)
    {
        return ServerAreaOffsetToClientPosition(WorldPositionToServerAreaOffsetPosition(worldPosition));
    }

    public void InitializeServerPositionOffset(Vector3 playerBornServerPosition, out Vector3 clientPosition)
    {
        m_TotalPositionOffset = GetServerPositionOffset(playerBornServerPosition, out clientPosition);
        Log("wzc", $"IsNeedOffset InitializeServerPositionOffset={m_TotalPositionOffset}");
    }

    public bool ResetServerPositionOffset(Vector3 playerBornServerPosition, out Vector3 clientPosition)
    {
        Vector3 totalPositionOffset = GetServerPositionOffset(playerBornServerPosition, out clientPosition);

        bool isNeedOffset = totalPositionOffset != m_TotalPositionOffset;

        m_TotalPositionOffset = totalPositionOffset;

        Log("wzc", $"ResetServerPositionOffset m_TotalPositionOffset={m_TotalPositionOffset}");

        return isNeedOffset;
    }

    private Vector3 GetServerPositionOffset(Vector3 playerBornServerPosition, out Vector3 clientPosition)
    {
        if (playerBornServerPosition.magnitude >= MaxOffset)
        {
            clientPosition = Vector3.zero;

            return playerBornServerPosition - clientPosition;
        }

        clientPosition = playerBornServerPosition;

        return Vector3.zero;
    }

    public bool IsNeedOffset(ref Vector3 playerClientPosition, ref Vector3 totalPositionOffset)
    {
        Vector3 lastPlyerClinetPositon = playerClientPosition;

        bool isConvert = false;
		if (playerClientPosition.magnitude >= MaxOffset)
        {
            isConvert = true;
            playerClientPosition = Vector3.zero;
        }

        if (isConvert)
        {
            m_TotalPositionOffset += lastPlyerClinetPositon - playerClientPosition;
            totalPositionOffset = m_TotalPositionOffset;
            Log("wzc", $"IsNeedOffset m_TotalPositionOffset={m_TotalPositionOffset}");
        }

        return isConvert;
    }

	/// <summary>
	/// 使用传入射线与当前Avatar的所有Collider做碰撞检测
	/// 返回第一个碰撞点, 也就是最外层的碰撞点
	/// </summary>
	public bool GetHitPoint(SpacecraftEntity spacecraft, Ray ray, out Collider hitCollider, out Vector3 hitPoint)
	{
		hitCollider = null;

		// 这里使用Motion.HorizontAxix 的位置而不是transoform.position是因为HeroEntity.transform.position是服务器的位置, 是跳动的
		// Motion.HorizontAxis的位置才是客户端插值后的位置
		float rayDistance = RAYCAST_LENGTH;

		bool hit = false;
		hitPoint = spacecraft.transform.position;
		float nearestHitDistance = float.MaxValue;
		foreach (Collider collider in spacecraft.GetAllColliders())
		{
			RaycastHit hitInfo;
			if (collider.Raycast(ray, out hitInfo, rayDistance))
			{
				// 用射线与所有Collider求交, 找到距射线发射位置最近的碰撞点
				// TODO 祝锐: 缓存一个最外层的最大的Collider, 不要每次遍历所有的Collider
				float sqrHitPointToOrigin = (hitInfo.point - ray.origin).sqrMagnitude;
				if (sqrHitPointToOrigin < nearestHitDistance)
				{
					nearestHitDistance = sqrHitPointToOrigin;
					hitPoint = hitInfo.point;
					hitCollider = collider;
					hit = true;
				}
			}
		}

		return hit;
	}

	public bool CanAttackToTarget(SpacecraftEntity attacker, SpacecraftEntity target)
	{
		if (attacker == null || target == null)
		{
			return false;
		}

		//优先判断是否可被攻击
		if (!target.GetCanBeAttack())
		{
			return false;
		}

        CfgEternityProxy cfgEternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;

		//直接调用获取阵营关系的函数，岂不简单些？
		CampRelation relation = cfgEternityProxy.GetRelation(attacker.GetCampID(), target.GetCampID());
		return relation == CampRelation.Enemy;

        //int attCamp = attacker.GetCampID();
        //int tarCamp = target.GetCampID();
		//CampVO_MergeObject? campVO = cfgCampProxy.GetCampRelationVO(attCamp, tarCamp);
		//if (campVO == null)
		//{
		//	Debug.LogErrorFormat("阵营信息不存在. Camp1: {0}, Camp2: {1}", attCamp, tarCamp);
		//	return false;
		//}
		//else
		//{
		//	int prestage = attacker.GetPrestige(tarCamp);
		//	// 通过声望值判断是否可攻击
		//	return prestage > campVO.Value.PrestigeEnemyMin && prestage < campVO.Value.PrestigeEnemyMax;
		//}
	}

    /// <summary>
    ///是否属于某个阵营关系
    /// </summary>
    /// <param name="factiontype">想去判断的阵营关系</param>
    /// <param name="selfCampType">阵营类型1（读表定义的阵营）</param>
    /// <param name="targetCampType">阵营类型2（读表定义的阵营）</param>
    /// <returns></returns>
    public bool IsBelongToThisFactionType(FactionType factiontype, BaseEntity entity1, BaseEntity entity2)
    {
        //左边：FactionType，右边 ：CampRelation
        //自己= 自己，友方（包含自己） = 友方+中立，敌方 = 敌方，All = 友方+中立+敌方

        //自己
        if (factiontype == FactionType.Self)
        {
            return (entity1.UId() == entity2.UId());
        }

        CfgEternityProxy cfgEternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
        CampRelation campRelation = cfgEternityProxy. GetRelation(entity1.GetCampID(), entity2.GetCampID());

        //友方
        if (factiontype == FactionType.Friendly)
        {
            return (campRelation == CampRelation.Friend || campRelation == CampRelation.Neutrality);
        }

        //敌方
        if (factiontype == FactionType.Enemy)
        {
            return campRelation == CampRelation.Enemy;
        }

        //所有
        if (factiontype == FactionType.All)
        {
            return campRelation != CampRelation.Undefined;
        }

        return false;
    }




    //TODO.
    //预留复活接口
    public bool CanReviveToTarget(SpacecraftEntity attacker, SpacecraftEntity target)
	{
		if (attacker == null || target == null)
			return false;

		return false;
	}

	public bool IsInScreen(Vector3 worldPosition)
	{
		Vector3 viewportPos = Camera.main.WorldToViewportPoint(worldPosition);
		bool isFront = Camera.main.transform.InverseTransformPoint(worldPosition).z >= Camera.main.nearClipPlane;
		return viewportPos.x >= 0 && viewportPos.y >= 0 && viewportPos.x <= 1 && viewportPos.y <= 1 && isFront;
	}
	
	public SpacecraftEntity GetNeareastEnemy()
	{
		Dictionary<ulong, BaseEntity> entities = m_EntityManager.GetEntities<SpacecraftEntity>();
		float minDistance = float.MaxValue;
		SpacecraftEntity nearestEntity = null;
		foreach (KeyValuePair<ulong, BaseEntity> entityInfo in entities)
		{
			SpacecraftEntity spacecraftEntity = entityInfo.Value as SpacecraftEntity;
			Vector3 playerWorldPos = GetEntityById<SpacecraftEntity>(m_MainPlayerUID).transform.position;
			Vector3 targetWorldPos = spacecraftEntity.transform.position;
			if (IsInScreen(targetWorldPos))
			{
				float distance = (playerWorldPos - targetWorldPos).magnitude;
				if (distance < minDistance)
				{
					nearestEntity = spacecraftEntity;
					minDistance = distance;
				}
			}
		}

		return nearestEntity;
	}

    private ulong m_DeadMapID = 0;
    public ulong GetDeadMapID()
    {
        return m_DeadMapID;
    }

    public void SetDeadMapID(ulong mapID)
    {
        m_DeadMapID = mapID;
    }

    /// <summary>
    /// 跃迁终点区域偏移坐标
    /// </summary>
    private Vector3 m_LeapEndAreaOffsetPosition = Vector3.zero;
    public Vector3 GetLeapEndAreaOffsetPosition()
    {
        return m_LeapEndAreaOffsetPosition;
    }
    public void SetLeapEndAreaOffsetPosition(Vector3 vector)
    {
        m_LeapEndAreaOffsetPosition = vector;
    }

    /// <summary>
    /// 跃迁起点区域偏移坐标
    /// </summary>
    private Vector3 m_LeapStartAreaOffsetPosition = Vector3.zero;
    public Vector3 GetLeapStartAreaOffsetPosition()
    {
        return m_LeapStartAreaOffsetPosition;
    }
    public void SetLeapStartAreaOffsetPosition(Vector3 vector)
    {
        m_LeapStartAreaOffsetPosition = vector;
    }

    /// <summary>
    /// 当前区域ID
    /// </summary>
    private ulong m_CurrentArarUid = 0;
    public ulong GetCurrentAreaUid()
    {
        return m_CurrentArarUid;
    }
    public void SetCurrentAreaUid(ulong areaUid)
    {
        m_CurrentArarUid = areaUid;
    }

    /// <summary>
    /// 跃迁区域ID
    /// </summary>
    private ulong m_LeapTargetAreaUid = 0;
    public ulong GetLeapTargetAreaUid()
    {
        return m_LeapTargetAreaUid;
    }
    public void SetLeapTargetAreaUid(ulong areaUid)
    {
        m_LeapTargetAreaUid = areaUid;
    }

    /// <summary>
    /// 跃迁点区域坐标偏移
    /// </summary>
    private Vector3 m_LeapTargetAreaOffset = Vector3.zero;
    public Vector3 GetLeapTargetAreaOffset()
    {
        return m_LeapTargetAreaOffset;
    }
    public void SetLeapTargetAreaOffset(Vector3 offset)
    {
        m_LeapTargetAreaOffset = offset;
	}

	public void AddEntityToEntityGroup(ulong ownerHeroID, SpacecraftEntity entity)
	{
		if (!m_EntityGroupTable.ContainsKey(ownerHeroID))
		{
			m_EntityGroupTable.Add(ownerHeroID, new List<SpacecraftEntity>());
		}

		if (!m_EntityGroupTable[ownerHeroID].Contains(entity))
			m_EntityGroupTable[ownerHeroID].Add(entity);
	}

	public void RemoveEntityFromEntityGroup(ulong ownerHeroID, SpacecraftEntity entity)
	{
		if (!m_EntityGroupTable.ContainsKey(ownerHeroID))
		{
			return;
		}

		m_EntityGroupTable[ownerHeroID].Remove(entity);
	}

	public List<SpacecraftEntity> GetAllEntitiesFromEntityGroup(ulong ownerHeroID)
	{
		if (m_EntityGroupTable.ContainsKey(ownerHeroID))
		{
			return m_EntityGroupTable[ownerHeroID];
		}
		else
		{
			return new List<SpacecraftEntity>();
		}
	}

	public bool IsThereAEntityGroup(ulong ownerHeroID)
	{
		return m_EntityGroupTable.ContainsKey(ownerHeroID);
	}

    public void LogTotalPositionOffset()
    {
        Log("wzc", $"LogTotalPositionOffset m_TotalPositionOffset={m_TotalPositionOffset}");
    }

    private void Log(string tag, string format, params object[] args)
    {
        Debug.LogFormat($"[{tag}] <color=#1EF8E1> {format} </color>");
    }
}