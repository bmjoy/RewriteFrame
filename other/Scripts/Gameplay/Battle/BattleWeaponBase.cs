using Eternity.FlatBuffer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// 把武器的 稳定度, 准确性, 控制力 称为一级属性
// 把武器的特有属性(比如机枪的横向最大扩散角度), 称为二级属性
// 需要有一张表把一级属性映射为指定武器的二级属性
// 这样会带来一个M*N的映射关系. 如果使用老的 公式类型, 参数1, 参数2, 参数3. 列就更多了. 所以需要直接在Excel中填公式, 程序去找一个解析公式的插件
public struct WeaponSpreadParam
{
	public BattleWeaponBase.BattleWeaponType WeaponType;
	public float Accuracy;
	public float Stability;
	public float param1;
	public float param2;
	public float param3;
	public float param4;
	public float param5;
	public float param6;
	public float param7;
}

/// <summary>
/// 实装武器的基类
/// 实装武器用于模拟各种类型的武器在战斗时除了自带技能以外的差异. 比如: 冲锋枪射击时的后坐力对弹道散射的影响
/// </summary>
public class BattleWeaponBase
{
	// TEST
	static public BattleWeaponType Test_Weapon_Type = BattleWeaponType.MachineGun;
	// TEST

	public enum BattleWeaponType
	{
		/// <summary>
		/// 未知武器类型. 错误
		/// </summary>
		UNKNOWN=0,
		/// <summary>
		/// 机关枪
		/// 初始水平扩散角, 最大水平扩散角, 水平/垂直 角度比例, 射击角度扩散函数, 每次射击对射击角度扩散的归一化影响因子, 归一化影响因子随时间收缩的函数
		/// TODO: 这里临时把最后一个参数定义为 归一化影响因子随时间变化的函数 , 而不是 射击扩散角度随时间变化的函数. 
		///		是因为如果使用后者, 每次收缩以后, 下次开枪时需要通过当前的 扩散角度 去求 归一化影响因子, 太费劲了. 先不做. 以后直接让策划写函数
		/// </summary>
		MachineGun = 1,
		Missile = 2,
		Shotgun = 3,
		/// <summary>
		/// 矿枪, 采用持续引导激光类型
		/// </summary>
		Mining = 4,
	}
	protected CfgEternityProxy m_CfgEternityProxy;
	protected BattleWeaponType m_Type;
	protected IWeapon m_WeaponItem;
	protected WeaponSpreadParam m_WeaponSpreadParam;
	protected Weapon m_WeaponConfig;

	/// <summary>
	/// 发射间隔
	/// </summary>
	protected float m_FireInterval;

	// Cache
	protected SpacecraftEntity m_MainPlayer;
	protected GameplayProxy m_GameplayProxy;
	protected PlayerSkillProxy m_PlayerSkillProxy;


	public static BattleWeaponType GetBattleWeaponTypeByReticle(int reticle)
	{
		switch (reticle)
		{
			case 1:
				return BattleWeaponType.MachineGun;
			case 2:
				return BattleWeaponType.Missile;
			case 3:
				return BattleWeaponType.Shotgun;
			default:
				return BattleWeaponType.UNKNOWN;
		}
	}

	public BattleWeaponBase(IWeapon weapon)
	{
		m_CfgEternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
		m_GameplayProxy = GameFacade.Instance.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;
		m_PlayerSkillProxy = GameFacade.Instance.RetrieveProxy(ProxyName.PlayerSkillProxy) as PlayerSkillProxy;
		m_MainPlayer = m_GameplayProxy.GetMainPlayer();
		m_WeaponItem = weapon;
		m_Type = GetBattleWeaponTypeByReticle(weapon.GetConfig().Reticle);

		m_WeaponConfig = GetWeaponConfig();
	}

	public BattleWeaponType GetWeaponType()
	{
		return m_Type;
	}

	private SkillHotkey m_SkillCastEvent;
	private bool m_SkillKeyPressed;

	private void SendKeyPressContinualForTriggerSkill(bool keyPress)
	{
		if (keyPress)
			WeaponOperationByHotKey(m_SkillCastEvent);
	}

	/// <summary>
	/// 操作武器的处理. 有些武器不是按下就释放技能的, 比如导弹. 所以在这里包一层
	/// </summary>
	/// <param name="hotkey"></param>
	public void WeaponOperation(SkillHotkey skillHotkey)
	{
		bool press = skillHotkey.ActionPhase == HotkeyPhase.Started;

		// 解决快捷键响应方式改变的问题
		m_SkillCastEvent = skillHotkey;
		// 武器技能的持续释放才在这里模拟, 否则在技能组件里模拟
		m_SkillKeyPressed = press && m_SkillCastEvent.IsWeaponSkill;
		
		WeaponOperationByHotKey(skillHotkey);
	}

	public void WeaponOperationByHotKey(SkillHotkey skillHotkey)
	{
		if (m_PlayerSkillProxy.UsingReformer())
		{
			SkillCastEvent castSkillEvent = new SkillCastEvent();
			castSkillEvent.IsWeaponSkill = true;
			castSkillEvent.SkillIndex = skillHotkey.SkillIndex;
			castSkillEvent.KeyPressed = skillHotkey.ActionPhase == HotkeyPhase.Started;

//#if NewSkill
//#else
            m_MainPlayer.SendEvent(ComponentEventName.CastSkill, castSkillEvent);
//#endif
        }
        else
		{
			WeaponOperationImplementation(skillHotkey);
		}
	}

	public virtual void StopFire()
	{
	}

	/// <summary>
	/// 武器是否可以攻击 (是否已经冷却)
	/// </summary>
	/// <returns></returns>
	public virtual bool CanWeaponFire()
	{
		return true;
	}

	/// <summary>
	/// 计算开火间隔
	/// </summary>
	/// <returns></returns>
	public virtual float CalculateFireInterval()
	{
		return 1;
	}

	public virtual void WeaponOperationImplementation(SkillHotkey skillHotkey)
	{
		bool press = skillHotkey.ActionPhase == HotkeyPhase.Started;
		if (m_PlayerSkillProxy.CanCurrentWeaponRelease() || !press)
		{
			SkillCastEvent castSkillEvent = new SkillCastEvent();
			castSkillEvent.IsWeaponSkill = true;
			castSkillEvent.SkillIndex = skillHotkey.SkillIndex;
			castSkillEvent.KeyPressed = press;

			m_MainPlayer.SendEvent(ComponentEventName.CastSkill, castSkillEvent);
		}

		if (!m_PlayerSkillProxy.CanCurrentWeaponRelease())
		{
			GameFacade.Instance.SendNotification(NotificationName.PlayerWeaponFireFail);
		}
	}

	/// <summary>
	/// 计算武器下一次攻击的目标
	/// 这是BattleWeapon类的主要作用. 因为释放技能攻击目标这种事是技能系统的责任, 五花八门的武器其实主要是给技能提供攻击目标
	/// </summary>
	/// <returns></returns>
	public virtual SkillTarget CalculateNextTarget()
	{
#if NewSkill
        return CalulateTargetDataByShootingDirection(CameraManager.GetInstance().GetMainCamereComponent().GetForward());
#else
        return null;
#endif
    }

    /// <summary>
    /// 计算武器下一次攻击的所有目标. 用于: 导弹武器
    /// </summary>
    /// <returns></returns>
    public virtual void CalculateNextTargets(ref List<SkillTarget> targetList)
	{
		targetList.Clear();
		targetList.Add(CalculateNextTarget());
	}

	/// <summary>
	/// 武器开火后的回调
	/// 主要用于改变武器的战斗状态. 比如说冲锋枪, 让准星随着射击 扩散/收缩; 比如说导弹, 让攻击流程结束, 可以进行下次锁定
	/// </summary>
	public virtual void PostWeaponFire()
	{
	}

	/// <summary>
	/// 武器技能结束
	/// </summary>
	public virtual void OnWeaponSkillFinished(bool success)
	{
	}

	/// <summary>
	/// 当武器切换为主武器
	/// </summary>
	public virtual void OnToggledToMainWeapon()
	{
	}

	/// <summary>
	/// 获取准星的屏幕占比
	/// </summary>
	public virtual MsgPlayerWeaponCrosshair GetRelativeHeightOfReticle()
	{
		MsgPlayerWeaponCrosshair crosshair = new MsgPlayerWeaponCrosshair();
		return crosshair;
	}

	/// <summary>
	/// 正常Update
	/// 冲锋枪: 用于准星随时间回复
	/// </summary>
	/// <param name="delta"></param>
	public virtual void OnUpdate(float delta)
	{
		SendKeyPressContinualForTriggerSkill(m_SkillKeyPressed);
	}

	/// <summary>
	/// 重置武器运行时数据
	/// 比如: 玩家复活
	/// </summary>
	public virtual void ResetWeaponRuntimeData()
	{
	}

	/// <summary>
	/// 通过屏幕中心打出射线选择的目标改变了
	/// </summary>
	/// <param name="eventInfo"></param>
	public virtual void OnTargetChanged(ChangeTargetEvent eventInfo)
	{
		// 锁定多目标, 同时让他们描红的逻辑, 还需要仔细考虑. 这里暂时先注释. 选择目标把它描红的逻辑让HUD执行
		//NotifyHUDTargetSelected(eventInfo.newTarget);
	}

	public virtual void NotifyHUDTargetSelected(SpacecraftEntity targetSelected)
	{
		WeaponSelectedTargetInfo targetInfoNotify = MessageSingleton.Get<WeaponSelectedTargetInfo>();
		// FIXME. 动态内存分配
		targetInfoNotify.selectedTarget = targetSelected;
		GameFacade.Instance.SendNotification(NotificationName.PlayerWeaponSelectedTarget, targetInfoNotify);
	}

	/// <summary>
	/// 把准星信息通知HUD
	/// </summary>
	public virtual void NotifyHUDCrosshair()
	{
		Camera cam = CameraManager.GetInstance()?.GetMainCamereComponent()?.GetCamera();
		if (cam != null)
		{
			MsgPlayerWeaponCrosshair crosshairOffset = GetRelativeHeightOfReticle();
			GameFacade.Instance.SendNotification(NotificationName.PlayerWeaponCrosshairScale, crosshairOffset);
		}
	}

	/// <summary>
	/// 获取武器配置信息. Weapon表
	/// </summary>
	/// <returns></returns>
	protected Weapon GetWeaponConfig()
	{
		return m_CfgEternityProxy.GetWeapon(m_WeaponItem.GetTID());
	}

	/// <summary>
	/// 根据这次射击的弹道方向, 计算命中的目标Entity和目标点
	/// </summary>
	/// <param name="camDirAffectedByRecoil"></param>
	/// <returns></returns>
	protected SkillTarget CalulateTargetDataByShootingDirection(Vector3 camDirAffectedByRecoil)
	{
		// 计算射线碰到的目标点, 并重新计算从Launcher到目标点的方向
		PlayerSkillVO skillVO = m_PlayerSkillProxy.GetCurrentWeaponSkill();
		SkillSystemGrow skillGrow = skillVO.GetSkillGrow();
		float maxDistance = skillGrow.Range * SceneController.SPACE_ACCURACY_TO_CLIENT;
		Ray ray = new Ray(CameraManager.GetInstance().GetMainCamereComponent().GetPosition(), camDirAffectedByRecoil);
		RaycastHit hitInfo = new RaycastHit();
		if (Physics.Raycast(ray, out hitInfo, maxDistance, LayerUtil.GetLayersIntersectWithSkillProjectile(true)))
		{
			return new SkillTarget(hitInfo.collider.attachedRigidbody?.GetComponent<SpacecraftEntity>(), hitInfo.point);
		}
		else
		{
			Vector3 skillReleasingDir = (ray.GetPoint(maxDistance) - m_MainPlayer.GetRootTransform().position).normalized;
			return new SkillTarget(null, m_MainPlayer.GetRootTransform().position + skillReleasingDir * maxDistance);
		}
	}

    /// <summary>
    /// 销毁时调用
    /// </summary>
    public virtual  void OnRelease()
    {

    }
}
