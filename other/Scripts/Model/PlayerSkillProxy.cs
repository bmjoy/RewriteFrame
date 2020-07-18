using Assets.Scripts.Define;
using Eternity.FlatBuffer;
using Eternity.FlatBuffer.Enums;
using Eternity.Runtime.Item;
using PureMVC.Patterns.Proxy;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkillProxy : Proxy
{
	/// <summary>
	/// 当前逻辑: 武器只有两个
	/// 据说以后还会改, 先不写很复杂的逻辑
	/// </summary>
	private const int WEAPON_CONTAINER_SIZE = 2;

	/// <summary>
	/// 玩家的当前飞船的技能列表. 飞船姿态改变时重新填充
	/// Key: SkillID, Value: PlayerSkillVO>
	/// </summary>
	private Dictionary<int, PlayerSkillVO> m_SkillIDToSkill = new Dictionary<int, PlayerSkillVO>();
	/// <summary>
	/// 缓存了每个飞船的每个姿态对应的技能列表. 
	/// 1. 用于填充UI上的技能列表
	/// 2. 记录所有技能的冷却时间. 防止换了船或者姿态以后冷却信息丢失
	/// </summary>
	private Dictionary<uint, List<PlayerSkillVO>> m_ShipIDToSkillList;

	/// <summary>
	/// 公共CD的结束时间, 以 Time.time 作为时间
	/// </summary>
	//private float m_GlobalCDEndTime;
	private Dictionary<SkillType, float> m_SkillTypeToGlobalCDEndTime;

	private CfgSkillSystemProxy m_CfgSkillProxy;
	
	/// <summary>
	/// 当前使用的武器, 在武器物品包里的格子索引
	/// 默认现在就两个武器, 服务器也是这么做的
	/// </summary>
	private int m_CurrentWeaponIndex;

	// Cache
	GameplayProxy m_GameplayProxy;

	#region Public 函数

	/// <summary>
	/// 目标是否在射程内 (考虑目标体积, 粗略检查)
	/// </summary>
	/// <param name="caster"></param>
	/// <param name="target"></param>
	/// <param name="range"></param>
	/// <returns></returns>
	public static bool TargetIsInRange_SimpleCheck(SpacecraftEntity caster, SpacecraftEntity target, float range)
	{
		float distanceToTarget = Vector3.Distance(caster.GetRootTransform().position, target.GetRootTransform().position);
		float validRange = range;
		if (target.GetPresentation().GetCapsuleCollider() != null)
		{
			// 粗略计算, 使用目标胶囊体长度的一半加技能射程作为这次判断的有效距离
			// 假设只有一个胶囊体, 也不该有多个
			CapsuleCollider targetCapsule = target.GetPresentation().GetCapsuleCollider();
			validRange += targetCapsule.height / 2;
		}

		return distanceToTarget < validRange;
	}

	/// <summary>
	/// 打出一条射线, 检测碰到得一个可攻击的目标
	/// </summary>
	/// <param name="targetPoint"></param>
	/// <param name="maxDistance"></param>
	/// <param name="hitInfo"></param>
	/// <returns>是否拾取到了技能目标</returns>
	public static bool PickSkillTargetEntityByRaycast(Vector3 targetPoint, float maxDistance, out RaycastHit hitInfo)
	{
		Vector3 mainCamPos = CameraManager.GetInstance().GetMainCamereComponent().GetPosition();
		Vector3 dir = CameraManager.GetInstance().GetMainCamereComponent().GetForward();

		Ray ray = new Ray(mainCamPos, dir);
		if (Physics.Raycast(ray, out hitInfo, maxDistance, LayerUtil.GetLayersIntersectWithSkillProjectile(true)))
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	public PlayerSkillProxy() : base(ProxyName.PlayerSkillProxy)
	{
		m_CurrentWeaponIndex = 0;
		m_SkillTypeToGlobalCDEndTime = new Dictionary<SkillType, float>();
		m_SkillTypeToGlobalCDEndTime[SkillType.ShipSkill] = Time.time;
		m_SkillTypeToGlobalCDEndTime[SkillType.WeaponSkill] = Time.time;
		m_SkillTypeToGlobalCDEndTime[SkillType.UNKNOWN] = 0;

		m_GameplayProxy = GameFacade.Instance.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;
		m_CfgSkillProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgSkillSystemProxy) as CfgSkillSystemProxy;

        m_CfgEternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
        m_ShipProxy = GameFacade.Instance.RetrieveProxy(ProxyName.ShipProxy) as ShipProxy;
        m_PackageProxy = Facade.RetrieveProxy(ProxyName.PackageProxy) as PackageProxy;

        shipCDVO = new PlayerSkillCDVO(CdType.Ship);
    }

    #region Ship Skill
    private CfgEternityProxy m_CfgEternityProxy = null;
    private ShipProxy m_ShipProxy = null;
    private PackageProxy m_PackageProxy = null;

	private PlayerShipSkillVO[] skillVOs = null;

    public PlayerShipSkillVO[] GetShipSkills()
    {
        if (skillVOs == null)
        {
            RefreshShipSkills();
        }
        return skillVOs;
    }

    public PlayerShipSkillVO GetShipSkillByIndex(int index)
    {
        PlayerShipSkillVO[] skills = GetShipSkills();
        if (skills != null && index >= 0 && index < skills.Length)
        {
            return skills[index];
        }
        return null;
    }

    public void RefreshShipSkills()
    {
        skillVOs = null;

        List<PlayerShipSkillVO> skills = new List<PlayerShipSkillVO>();
        IShip currentShip = m_ShipProxy.GetAppointWarShip();
        if (currentShip != null)
        {
            ISkill[] shipSkills = currentShip.GetSkillContainer().GetSkills();
            List<ISkill> tempList = new List<ISkill>(shipSkills);
            tempList.Sort((skill1, skill2) =>
            {
                int pos1 = m_PackageProxy.GetItem<ItemSkillVO>(skill1.GetUID()).Position;
                int pos2 = m_PackageProxy.GetItem<ItemSkillVO>(skill2.GetUID()).Position;
                return pos1.CompareTo(pos2);
            });

            foreach (var skill in tempList)
            {
                int skillID = (int)skill.GetTID();
                if (m_CfgEternityProxy.IsSkillExist(skillID))
                {
                    PlayerShipSkillVO skillVO = new PlayerShipSkillVO((int)skill.GetTID());
                    skills.Add(skillVO);
                }
            }
        }

        if (skills.Count > 0)
        {
            skillVOs = skills.ToArray();
        }

		SendNotification(NotificationName.MSG_SHIP_SKILL_CHANGED);
    }
    #endregion

    #region Skill CD
    private Dictionary<int, PlayerSkillCDVO> m_SkillCDDic = new Dictionary<int, PlayerSkillCDVO>();
    private PlayerSkillCDVO shipCDVO = null;

    public bool IsInCD(int skillID)
    {
        CdType[] cdTypes = m_CfgEternityProxy.GetSkillReleaseCDTypes(skillID);

        if (cdTypes == null || cdTypes.Length == 0)
        {
            return false;
        }

        if (Array.IndexOf(cdTypes, CdType.Ship) >= 0 && shipCDVO.IsInCD())
        {
            return true;
        }

        if (Array.IndexOf(cdTypes, CdType.Skill) >= 0 && m_SkillCDDic.TryGetValue(skillID, out PlayerSkillCDVO skillCDVO))
        {
            return skillCDVO.IsInCD();
        }

        return false;
    }

    public PlayerSkillCDVO GetCDVO(int skillID)
    {
        CdType[] cdTypes = m_CfgEternityProxy.GetSkillReleaseCDTypes(skillID);

        if (cdTypes == null || cdTypes.Length == 0)
        {
            return null;
        }

        if (Array.IndexOf(cdTypes, CdType.Ship) >= 0)
        {
            return shipCDVO;
        }

        if (Array.IndexOf(cdTypes, CdType.Skill) >= 0 && m_SkillCDDic.TryGetValue(skillID, out PlayerSkillCDVO skillCDVO))
        {
            return skillCDVO;
        }

        return null;
    }

    public PlayerSkillCDVO GetActiveCDVO(int skillID)
    {
        CdType[] cdTypes = m_CfgEternityProxy.GetSkillReleaseCDTypes(skillID);

        if (cdTypes == null || cdTypes.Length == 0)
        {
            return null;
        }

        if (Array.IndexOf(cdTypes, CdType.Ship) >= 0)
        {
            if (shipCDVO.IsInCD())
            {
                return shipCDVO;
            }
        }

        if (Array.IndexOf(cdTypes, CdType.Skill) >= 0 && m_SkillCDDic.TryGetValue(skillID, out PlayerSkillCDVO skillCDVO))
        {
            if (skillCDVO.IsInCD())
            {
                return skillCDVO;
            }
        }
        return null;
    }

    public void AddCD(int skillID, CdType cdType, float cdTime)
    {
        PlayerSkillCDVO cdVO = null;
        if (cdType == CdType.Ship)
        {
            cdVO = shipCDVO;
        }
        else if (cdType == CdType.Skill)
        {
            if (!m_SkillCDDic.TryGetValue(skillID, out cdVO))
            {
                cdVO = new PlayerSkillCDVO(skillID, cdType);
                m_SkillCDDic.Add(skillID, cdVO);
            }
        }

        if (cdVO != null)
        {
            cdVO.SetCD(cdTime);
			SendNotification(NotificationName.MSG_SHIP_SKILL_CD_CHANGED);
		}
    }

    #endregion

    /// <summary>
    /// 获取技能表
    /// </summary>
    /// <returns>玩家技能数据列表</returns>
    public PlayerSkillVO[] GetSkills()
	{
		if (m_ShipIDToSkillList == null || m_ShipIDToSkillList.Count == 0)
		{
			m_ShipIDToSkillList = new Dictionary<uint, List<PlayerSkillVO>>();
			RebuildSkillList();
		}

		ShipProxy shipProxy = GameFacade.Instance.RetrieveProxy(ProxyName.ShipProxy) as ShipProxy;
		IShip currentShip = shipProxy.GetAppointWarShip();
		if (currentShip != null)
		{
			if (m_ShipIDToSkillList.ContainsKey(currentShip.GetTID()))
			{
				return m_ShipIDToSkillList[currentShip.GetTID()].ToArray();
			}
		}
		
		return new PlayerSkillVO[] { };
	}

	public bool HaveSkill(int skillID)
	{
		return m_SkillIDToSkill.ContainsKey(skillID);
	}

	public PlayerSkillVO GetSkillByID(int skillID)
	{
		// 蓄力技能, 在蓄力结束后, 释放的是另外一个技能. 这个技能没有配置在船身上
		// 实际上客户端存储技能实例的信息只是为了保存冷却时间. 这个逻辑检查留给服务器了
		if (!m_SkillIDToSkill.ContainsKey(skillID))
		{
			PlayerSkillVO newSkillVO = PlayerSkillVO.CreateSkillVO(skillID);
			if (newSkillVO != null)
			{
				m_SkillIDToSkill.Add(skillID, newSkillVO);
				return m_SkillIDToSkill[skillID];
			}
			else
			{
				return null;
			}
		}
		else
		{
			return m_SkillIDToSkill[skillID];
		}
	}

	public PlayerSkillVO GetSkillByIndex(int index)
	{
		PlayerSkillVO[] skills = GetSkills();
		if (index >= 0 && index < skills.Length)
		{
			return skills[index];
		}
		else
		{
			return null;
		}
	}

	public SkillType GetSkillType(int skillID)
	{
		int skillTypeInt = m_SkillIDToSkill[skillID].GetSkillGrow().SkillType;
		if (skillTypeInt < 0 || skillTypeInt >= (int)SkillType.UNKNOWN)
		{
			return SkillType.UNKNOWN;
		}
		else
		{
			return (SkillType)skillTypeInt;
		}
	}

	public SkillCategory GetSkillCategory(int skillID)
	{
		int skillCateInt = m_SkillIDToSkill[skillID].GetSkillGrow().SkillType;
		if (skillCateInt < 0 || skillCateInt >= (int)SkillType.UNKNOWN)
		{
			return SkillCategory.UNKNOWN;
		}
		else
		{
			return (SkillCategory)skillCateInt;
		}
	}

	/// <summary>
	/// 技能释放完成后开始CD
	/// </summary>
	/// <param name="skillID"></param>
	public void StartCD(int skillID)
	{
		Debug.Assert(m_SkillIDToSkill.ContainsKey(skillID));
		if (!m_SkillIDToSkill.ContainsKey(skillID))
			return;

		// 开始公共CD
		SkillSystemGrow skillGrowVO = m_CfgSkillProxy.GetSkillGrow(skillID);
		if (m_SkillTypeToGlobalCDEndTime[GetSkillType(skillID)] < Time.time + skillGrowVO.GlobalCd / 1000.0f)
			m_SkillTypeToGlobalCDEndTime[GetSkillType(skillID)] = Time.time + skillGrowVO.GlobalCd / 1000.0f;

		// 开始技能本身的CD
		m_SkillIDToSkill[skillID].StartCD();
	}

	/// <summary>
	/// 技能是否正在CD中, 考虑自身CD和公共CD
	/// </summary>
	/// <param name="skillID"></param>
	/// <returns></returns>
	public bool IsOnCD(int skillID)
	{
		Debug.Assert(m_SkillIDToSkill.ContainsKey(skillID));
		if (!m_SkillIDToSkill.ContainsKey(skillID))
			return false;

		PlayerSkillVO skill = m_SkillIDToSkill[skillID];
		// 如果是武器技能, 则不考虑公共CD
		return skill.GetRemainingCD() > 0
			|| Time.time < m_SkillTypeToGlobalCDEndTime[GetSkillType(skillID)];
	}

	/// <summary>
	/// 技能是否正在CD中, 考虑自身CD和公共CD
	/// </summary>
	/// <param name="skillID"></param>
	/// <returns></returns>
	public bool IsOnTriggerCD(int skillID)
	{
		Debug.Assert(m_SkillIDToSkill.ContainsKey(skillID));
		if (!m_SkillIDToSkill.ContainsKey(skillID))
			return false;

		PlayerSkillVO skill = m_SkillIDToSkill[skillID];
		// 如果是武器技能, 则不考虑公共CD
		return !skill.IsTriggerCDFinished();
	}

	public float GetRemainingCD(int skillID)
	{
		Debug.Assert(m_SkillIDToSkill.ContainsKey(skillID));
		if (!m_SkillIDToSkill.ContainsKey(skillID))
			return 0f;

		PlayerSkillVO skill = m_SkillIDToSkill[skillID];
		float skillCD = skill.GetRemainingCD();
		float remainingGlobalCD = Mathf.Clamp(m_SkillTypeToGlobalCDEndTime[(SkillType)skill.GetSkillGrow().SkillType] - Time.time, 0, 99999f);
		if (remainingGlobalCD > skillCD)
			skillCD = remainingGlobalCD;

		return skillCD;
	}
	
	public PlayerSkillVO GetCurrentWeaponSkill()
	{
		IWeapon weapon = GetCurrentWeapon();
		if (weapon != null)
		{
			int skillID = (int)weapon.GetBaseConfig().SkillId;
			return GetSkillByID(skillID);
		}

		return null;
	}


    /// <summary>
    /// 获取当前技能武器VO信息
    /// </summary>
    /// <returns></returns>
    public PlayerShipSkillVO GetCurrentWeaponSkillVO()
    {
        IWeapon weapon = GetCurrentWeapon();
        if (weapon != null)
        {
            int skillID = (int)weapon.GetBaseConfig().SkillId;
            PlayerShipSkillVO skillVO = new PlayerShipSkillVO((int)skillID);
            return skillVO;
        }
        return null;
    }


    public bool CanCurrentWeaponRelease()
	{
        SpacecraftEntity mainEntity = m_GameplayProxy.GetEntityById<SpacecraftEntity>(m_GameplayProxy.GetMainPlayerUID());
        if (mainEntity.GetCurrentState().GetMainState() != EnumMainState.Fight || mainEntity.GetCurrSkillId() >0) //非战斗状态 || 有技能在释放中
            return false;


            if (UsingReformer())
			return true;

		IWeapon curWeapon = GetCurrentWeapon();
		if (curWeapon == null)
			return false;

		return CanWeaponRelease(curWeapon.GetUID());
	}

	public bool CanWeaponRelease(ulong UID)
	{
		SpacecraftEntity mainEntity = m_GameplayProxy.GetEntityById<SpacecraftEntity>(m_GameplayProxy.GetMainPlayerUID());
		WeaponPowerVO powerVO = mainEntity.GetWeaponPower(UID);
		int weaponType = ItemTypeUtil.GetWeaponType(GetWeaponByUID(UID));
		// 武器是否有弹药或者是否已经冷却
		if (weaponType < 0 || (powerVO != null && powerVO.ForceCooldown))
		{
			Debug.LogFormat("power.current: {0}. ForceCooldown: {1}", powerVO.CurrentValue, powerVO.ForceCooldown);

			return false;
		}

		return true;
	}

	public WeaponPowerVO GetCurrentWeaponPowerOfMainPlayer()
	{
		GameplayProxy gameplayProxy = GameFacade.Instance.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;
		SpacecraftEntity mainEntity = gameplayProxy.GetEntityById<SpacecraftEntity>(gameplayProxy.GetMainPlayerUID());
		IWeapon curWeapon = GetCurrentWeapon();
		return curWeapon != null ? mainEntity.GetWeaponPower(curWeapon.GetUID()) : null;
	}

	public WeaponPowerVO GetWeaponPowerOfMainPlayer(int index)
	{
		SpacecraftEntity mainEntity = m_GameplayProxy.GetEntityById<SpacecraftEntity>(m_GameplayProxy.GetMainPlayerUID());
		if (mainEntity == null)
		{
			return null;
		}
		else
		{
			IWeapon weapon = GetWeaponByIndex(index);
			return weapon != null ? mainEntity.GetWeaponPower(weapon.GetUID()) : null;
		}
	}

	#endregion

	#region Private 函数

	private string GetString(int id)
	{
		CfgLanguageProxy cfgLanguageProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgLanguageProxy) as CfgLanguageProxy;
		return cfgLanguageProxy.GetLocalization(id);
	}
	#endregion

	#region 玩家技能信息改变. 暂时当做黑盒处理

	/// <summary>
	/// 重建技能列表
	/// </summary>
	public void RebuildSkillList()
	{
		RefreshShipSkills();

		ShipProxy shipProxy = GameFacade.Instance.RetrieveProxy(ProxyName.ShipProxy) as ShipProxy;

		IShip currentShip = shipProxy.GetAppointWarShip();
		if (currentShip != null)
		{
			if (m_ShipIDToSkillList == null)
			{
				m_ShipIDToSkillList = new Dictionary<uint, List<PlayerSkillVO>>();
			}

			// 缓存飞船自带技能
			if (!m_ShipIDToSkillList.ContainsKey(currentShip.GetTID()))
			{
				ISkill[] shipSkills = currentShip.GetSkillContainer().GetSkills();
				List<PlayerSkillVO> listToCache = new List<PlayerSkillVO>();
				for (int iSkill = 0; iSkill < shipSkills.Length; iSkill++)
				{
					int skillID = (int)shipSkills[iSkill].GetTID();
					PlayerSkillVO newSkill = PlayerSkillVO.CreateSkillVO(skillID);
					if (newSkill != null)
					{
						listToCache.Add(newSkill);
						if (!m_SkillIDToSkill.ContainsKey(skillID))
							m_SkillIDToSkill.Add(skillID, newSkill);
					}
				}

				m_ShipIDToSkillList.Add(currentShip.GetTID(), listToCache);
			}

			// 缓存武器技能
			IWeaponContainer container = currentShip.GetWeaponContainer();
			if (container != null)
			{
				IWeapon[] weapons = container.GetWeapons();
				if (weapons != null)
				{
					for (int iWeapon = 0; iWeapon < weapons.Length; iWeapon++)
					{
						if (weapons[iWeapon] != null)
						{
							int skillID = (int)weapons[iWeapon].GetBaseConfig().SkillId;
							PlayerSkillVO newSkill = PlayerSkillVO.CreateSkillVO(skillID);
							if (newSkill != null)
							{
								if (!m_SkillIDToSkill.ContainsKey(skillID))
								{
									m_SkillIDToSkill.Add(skillID, newSkill);
								}

								if (!m_ShipIDToSkillList[currentShip.GetTID()].Contains(newSkill))
									m_ShipIDToSkillList[currentShip.GetTID()].Add(newSkill);
							}
						}
					}
				}
			}
		}
	}
	
	/// <summary>
	/// 获取当前武器. 不包括转化炉
	/// </summary>
	/// <returns>DiyShipWeapon</returns>
	public IWeapon GetCurrentWeapon()
	{
		ShipProxy shipProxy = GameFacade.Instance.RetrieveProxy(ProxyName.ShipProxy) as ShipProxy;
		if (shipProxy.GetAppointWarShip() != null && shipProxy.GetAppointWarShip().GetWeaponContainer() != null)
		{
			IWeapon[] weapons = shipProxy.GetAppointWarShip().GetWeaponContainer().GetWeapons();
			for (int i=0;i<weapons.Length;i++)
			{
				if (weapons[i].GetPos()==m_CurrentWeaponIndex)
				{
					return weapons[i];
				}
			}
		}

		return null;
	}

	public int GetCurrentWeaponIndex()
	{
		return m_CurrentWeaponIndex;
	}


    /// <summary>
    /// 按照武器在武器包中的格子位置获取武器
    /// </summary>
    /// <returns>DiyShipWeapon</returns>
    public IWeapon GetWeaponByIndex(int index)
    {
		ShipProxy shipProxy = GameFacade.Instance.RetrieveProxy(ProxyName.ShipProxy) as ShipProxy;
		if (shipProxy.GetAppointWarShip() != null && shipProxy.GetAppointWarShip().GetWeaponContainer() != null)
		{
			IWeapon[] weapons = shipProxy.GetAppointWarShip().GetWeaponContainer().GetWeapons();
			for (int i = 0; i < weapons.Length; i++)
			{
				if (weapons[i].GetPos() == index)
				{
					return weapons[i];
				}
			}
		}

		return null;
	}

	/// <summary>
	/// 按照武器的物品UID获取武器
	/// </summary>
	/// <returns>DiyShipWeapon</returns>
	public IWeapon GetWeaponByUID(ulong uid)
	{
        ShipProxy shipProxy = GameFacade.Instance.RetrieveProxy(ProxyName.ShipProxy) as ShipProxy;
		if (shipProxy.GetAppointWarShip() != null && shipProxy.GetAppointWarShip().GetWeaponContainer() != null)
		{
			IWeapon[] weapons = shipProxy.GetAppointWarShip().GetWeaponContainer().GetWeapons();
			if (weapons != null)
			{
				foreach (IWeapon weapon in weapons)
				{
					if (weapon.GetUID() == uid)
						return weapon;
				}
			}
			else
			{
				return null;
			}
		}

		return null;
	}

	/// <summary>
	/// 按照武器类型获取武器
	/// 武器类型: 炮塔, 导弹, 激光
	/// </summary>
	/// <returns>DiyShipWeapon</returns>
	public IWeapon GetWeaponByWeaponType(Eternity.Runtime.Item.WeaponL1 weaponType)
	{
        ShipProxy shipProxy = GameFacade.Instance.RetrieveProxy(ProxyName.ShipProxy) as ShipProxy;
        if (shipProxy.GetAppointWarShip() != null && shipProxy.GetAppointWarShip().GetWeaponContainer() != null)
		{
			IWeapon[] weapons = shipProxy.GetAppointWarShip().GetWeaponContainer().GetWeapons();
			if (weapons != null)
			{
				for (int iWeapon = 0; iWeapon < weapons.Length; iWeapon++)
				{
					if (ItemTypeUtil.GetWeaponType(weapons[iWeapon]) == (int)weaponType)
					{
						return weapons[iWeapon];
					}
				}
			}
			else
			{
				return null;
			}
		}

		return null;
	}

	/// <summary>
	/// 主角正在释放武器技能
	/// </summary>
	/// <returns></returns>
	public bool IsMainPlayerUsingWeaponSkill()
	{
		SpacecraftEntity mainPlayer = m_GameplayProxy.GetEntityById<SpacecraftEntity>(m_GameplayProxy.GetMainPlayerUID());
		if (mainPlayer == null)
			return false;

		int skillID = mainPlayer.GetCurrSkillId();
		if (skillID > 0 && m_CfgSkillProxy.IsWeaponSkill(skillID)
			&& (mainPlayer.GetCurrentSkillState() == SkillState.Channelling
				|| mainPlayer.GetCurrentSkillState() == SkillState.ManualChannelling
				|| mainPlayer.GetCurrentSkillState() == SkillState.AutoChannelling
				|| mainPlayer.GetCurrentSkillState() == SkillState.RapidFire))
		{
			return true;
		}

		if (mainPlayer.IsReleasingTriggerSkill()
			&& mainPlayer.GetTriggerSkillID() > 0
			&& m_CfgSkillProxy.IsWeaponSkill(mainPlayer.GetTriggerSkillID()))
		{
			return true;
		}

		return false;
	}

	/// <summary>
	/// 切换武器
	/// </summary>
	public void ToggleCurrentWeapon()
	{
// 		bool canToggleWeapon = true;
// 		SpacecraftEntity mainPlayer = m_GameplayProxy.GetEntityById<SpacecraftEntity>(m_GameplayProxy.GetMainPlayerUID());
// 		if (mainPlayer != null)
// 		{
// 			if (!mainPlayer.GetCanToggleWeapon() || IsMainPlayerUsingWeaponSkill() || UsingReformer())
// 			{
// 				canToggleWeapon = false;
// 			}
// 		}
// 
// 		if (canToggleWeapon)
// 		{
			int newIndex = (m_CurrentWeaponIndex + 1) % WEAPON_CONTAINER_SIZE;
			IWeapon newxtWeapon = GetWeaponByIndex(newIndex);
			if (newxtWeapon == null)
				return;

			// 现在由服务器通知换武器
			//m_CurrentWeaponIndex = (m_CurrentWeaponIndex + 1) % WEAPON_CONTAINER_SIZE;
			//SendNotification(NotificationName.PlayerWeaponToggleEnd);
			NetworkManager.Instance.GetSkillController().RequestChangeWeapon(newxtWeapon.GetUID());
		//}
	}

    public void ChangeCurrentWeaponByServer(ulong weaponUID)
    {
        int weaponIndex = GetWeaponIndexByUID(weaponUID);

        if (m_CurrentWeaponIndex != weaponIndex)
        {
            m_CurrentWeaponIndex = weaponIndex;
            SendNotification(NotificationName.PlayerWeaponToggleEnd);
        }
	}

	public int GetWeaponIndexByUID(ulong uid)
	{
		ShipProxy shipProxy = GameFacade.Instance.RetrieveProxy(ProxyName.ShipProxy) as ShipProxy;
		IShip currentShip = shipProxy.GetAppointWarShip();
		IWeaponContainer container = currentShip.GetWeaponContainer();
		if (container != null)
		{
			IWeapon[] weapons = container.GetWeapons();
			if (weapons != null)
			{
				for (int iWeapon = 0; iWeapon < weapons.Length; iWeapon++)
				{
					if (weapons[iWeapon].GetUID() == uid)
					{
						return weapons[iWeapon].GetPos();
					}
				}
			}
		}

		return 0;
	}

	/// <summary>
	/// 获取第一个可用武器的index
	/// </summary>
	/// <returns></returns>
	public int GetFirstAvailableWeaponIndex()
	{
		int firstAvailableWeaponIndex = -1;
		ShipProxy shipProxy = GameFacade.Instance.RetrieveProxy(ProxyName.ShipProxy) as ShipProxy;
		IShip currentShip = shipProxy.GetAppointWarShip();
		IWeaponContainer container = currentShip.GetWeaponContainer();
		if (container != null)
		{
			IWeapon[] weapons = container.GetWeapons();
			if (weapons != null)
			{
				for (int iWeapon = 0; iWeapon < weapons.Length; iWeapon++)
				{
					if (weapons[iWeapon] != null)
					{
						firstAvailableWeaponIndex = firstAvailableWeaponIndex < 0 ? weapons[iWeapon].GetPos() : firstAvailableWeaponIndex;
					}
				}
			}
		}

		return firstAvailableWeaponIndex;
	}

	/// <summary>
	/// 获取当前武器的运行时武器信息. 可用于计算弹道扩散
	/// </summary>
	/// <returns></returns>
	public BattleWeaponBase GetCurrentBattleWeapon()
	{
		SpacecraftEntity mainPlayer = m_GameplayProxy.GetEntityById<SpacecraftEntity>(m_GameplayProxy.GetMainPlayerUID());
		IWeapon weapon = GetCurrentWeapon();
		if (weapon != null && mainPlayer != null)
		{
			if (mainPlayer.HaveBattleWeapon(weapon.GetUID()))
			{
				return mainPlayer.GetBattleWeapon(weapon.GetUID());
			}
			else
			{
				return null;
			}
		}
		else
		{
			return null;
		}
	}

    /// <summary>
    /// 获取当前武器武器准星对象
    /// </summary>
    /// <returns></returns>
    public WeaponAndCrossSight GetCurrentWeaponAndCrossSight()
    {
        SpacecraftEntity mainPlayer = m_GameplayProxy.GetEntityById<SpacecraftEntity>(m_GameplayProxy.GetMainPlayerUID());
        if (mainPlayer == null)
            return null;

        WeaponAndCrossSight result = null;

        if (UsingReformer())
        {
            //转化炉武器
            IReformer reformer = GetReformer();
            if (reformer != null && mainPlayer.HaveWeaponAndCrossSight(reformer.GetUID()))
            {
                result = mainPlayer.GetWeaponAndCrossSight(reformer.GetUID());
            }
        }
        else
        {
            //普通武器
            IWeapon weapon = GetCurrentWeapon();
            if (weapon != null && mainPlayer.HaveWeaponAndCrossSight(weapon.GetUID()))
                {
                result = mainPlayer.GetWeaponAndCrossSight(weapon.GetUID());
            }
        }

        return result;
    }


    public bool UsingReformer()
	{
		SpacecraftEntity mainEntity = m_GameplayProxy.GetEntityById<SpacecraftEntity>(m_GameplayProxy.GetMainPlayerUID());
        if (mainEntity != null)
            //return mainEntity.HasState(ComposableState.PeerlessStatus);
            return mainEntity.GetCurrentState().IsHasSubState(EnumSubState.Peerless);
        else
            return false;
	}

	public IReformer GetReformer()
	{
		ShipProxy shipProxy = GameFacade.Instance.RetrieveProxy(ProxyName.ShipProxy) as ShipProxy;
		if (shipProxy.GetAppointWarShip() != null && shipProxy.GetAppointWarShip().GetReformerContainer() != null)
		{
			return shipProxy.GetAppointWarShip().GetReformerContainer().GetReformer();
		}
		else
		{
			return null;
		}
	}

	public int GetReformerSkillID()
	{
		ShipProxy shipProxy = GameFacade.Instance.RetrieveProxy(ProxyName.ShipProxy) as ShipProxy;
		if (shipProxy.GetAppointWarShip() != null && shipProxy.GetAppointWarShip().GetReformerContainer() != null)
		{
			IReformer reformer = shipProxy.GetAppointWarShip().GetReformerContainer().GetReformer();
			if (reformer != null)
				return (int)reformer.GetBaseConfig().SkillId;
			else
				return -1;
		}

		return -1;
	}

	public PlayerSkillVO GetReformerSkill()
	{
        ShipProxy shipProxy = GameFacade.Instance.RetrieveProxy(ProxyName.ShipProxy) as ShipProxy;
		if (shipProxy.GetAppointWarShip() != null && shipProxy.GetAppointWarShip().GetReformerContainer() != null)
		{
			IReformer reformer = shipProxy.GetAppointWarShip().GetReformerContainer().GetReformer();
			if (reformer != null)
				return GetSkillByID((int)reformer.GetBaseConfig().SkillId);
			else
				return null;
		}

		return null;
	}
	#endregion
}