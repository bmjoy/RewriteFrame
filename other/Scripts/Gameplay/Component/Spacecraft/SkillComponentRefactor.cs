using Assets.Scripts.Define;
using Assets.Scripts.Proto;
using Eternity.FlatBuffer;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public interface ISkillRefactorProperty
{
    HeroState GetCurrentState();
    T AddComponent<T>() where T : Component;
	Transform GetRootTransform();
	Rigidbody GetRigidbody();
	Vector3 GetEngineAxis();
	Vector3 GetRotateAxis();
	bool IsMain();
	uint GetUId();
	Npc GetNPCTemplateVO();
	KHeroType GetHeroType();
	uint GetTemplateID();
	SpacecraftEntity GetOwner();
	void SetFireCountdown(float countdown);
	SpacecraftEntity GetTarget();
	Transform GetSkinRootTransform();
	SpacecraftPresentation GetPresentation();
	void AddCollider_Runtime(Collider newCollider);

	int GetCurrentSkillID();
	void SetCurrentSkillID(int skillID);
	SkillState GetCurrentSkillState();
	void SetCurrentSkillState(SkillState skillState);

	bool IsReleasingTriggerSkill();
	void SetReleasingTriggerSkill(bool releasingTriggerSkill);
	int GetTriggerSkillID();
	void SetTriggerSkillID(int skillID);

	float GetUnderAttackWarningToneCountdown();
	void SetUnderAttackWarningToneCountdown(float countdown);
}

/// <summary>
/// 技能组件
/// 
/// 技能按键 和 技能 的对应表. <SkillKey, SkillBase>
/// 
/// 
/// 注册技能输入消息(). 技能按键按下抬起 1, 2, 3, 武器技能按键按下抬起.
///		所有按键都响应同一个方法, 把按键作为参数传入. 释放对应的技能, 或者给技能发输入消息
///	
/// 释放技能(). 检查是不是有这个技能, 创建技能, 检查是不是能释放. 释放技能. 把技能放入 [技能按键 和 技能 的对应表]
/// 
/// 处理网络消息(). 检查是不是存在对应技能, 如果不存在就报错. 如果存在就给技能发网络消息
/// 
/// </summary>
public class SkillComponentRefactor : EntityComponent<ISkillRefactorProperty>
{
	private ISkillRefactorProperty m_Property;

	private SkillState m_State;

	/// <summary>
	/// [SkillID, ReleasedSkillDuringPress]
	/// 
	/// 针对当前这次按下, 是不是已经释放过技能了
	/// 只要释放过技能, 这个变量就被设为 true
	/// 只要松开技能按键, 这个变量就被重新设为 false
	/// 用于区别处理 Trigger类技能
	/// </summary>
	private Dictionary<int, bool> m_ReleasedSkillDuringPress;

	/// <summary>
	/// 技能ID 和 技能 的对应表. [SkillID, SkillBase]
	/// </summary>
	private Dictionary<int, SkillBase> m_IDToSkill;
	private List<int> m_RemoveSkillList;

	/// <summary>
	/// 弱点声音播放CD
	/// </summary>
	private float m_WeakSoundCD;
	private float m_WeakSoundTmpCD;

	/// <summary>
	/// 弱点声音播放概率
	/// </summary>
	private float m_WeakSoundProbability;


	/// <summary>
	/// 以前HotKeyManager的通知方式是, 按下后每帧都通知. 技能系统对Trigger技能的实现, 也是基于此
	/// 现在HotKeyManager 只在状态改变时才通知. 所以这里自己记录按下状态, 如果HotKeyManager通知按键按下, 则每帧都模拟按键按下
	/// </summary>
	private bool m_SkillKeyPressed = false;
	/// <summary>
	/// 用于模拟按键每帧的按下消息
	/// <see cref="m_SkillKeyPressed"/>
	/// </summary>
	private SkillCastEvent m_SkillCastEvent;

	// 快捷方式变量
	private CfgSkillSystemProxy m_CfgSkillProxy;
	private PlayerSkillProxy m_SkillProxy;
	private CfgLanguageProxy m_CfgLanguageProxy;
	private GameplayProxy m_GameplayProxy;
	private CfgEternityProxy m_CfgEternityProxy;

	#region 初始化
	public override void OnInitialize(ISkillRefactorProperty property)
	{
		m_Property = property;

		m_State = SkillState.Idle;
		m_Property.SetCurrentSkillState(m_State);

		m_IDToSkill = new Dictionary<int, SkillBase>();
		m_RemoveSkillList = new List<int>();
		m_ReleasedSkillDuringPress = new Dictionary<int, bool>();
        
		m_CfgSkillProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgSkillSystemProxy) as CfgSkillSystemProxy;
		m_SkillProxy = GameFacade.Instance.RetrieveProxy(ProxyName.PlayerSkillProxy) as PlayerSkillProxy;
		m_CfgLanguageProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgLanguageProxy) as CfgLanguageProxy;
		m_GameplayProxy = GameFacade.Instance.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;

		m_CfgEternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
		m_WeakSoundCD = m_CfgEternityProxy.GetGamingConfig(1).Value.Mine.Value.Sound.Value.WeaknessSoundCd;
		m_WeakSoundTmpCD = m_WeakSoundCD;
		m_WeakSoundProbability = m_CfgEternityProxy.GetGamingConfig(1).Value.Mine.Value.Sound.Value.WeaknessSoundRate;

		SkillBase.s_CfgSkillProxy = m_CfgSkillProxy;
		SkillBase.s_SkillProxy = m_SkillProxy;
		SkillBase.s_CfgLanguageProxy = m_CfgLanguageProxy;
		SkillBase.s_GameplayProxy = m_GameplayProxy;
		SkillBase.s_CfgEternityProxy = m_CfgEternityProxy;
		SetPreditionForAllSkillType();

		// UNDONE, 声音

		PreloadSkill();
	}

	/// <summary>
	/// 注册消息回调
	/// </summary>
	public override void OnAddListener()
	{
		AddListener(ComponentEventName.Event_s2c_cast_skill, OnCastSkill_ServerMessage);
		AddListener(ComponentEventName.Event_s2c_sing_skill, OnSingSkill_ServerMessage);
		AddListener(ComponentEventName.Event_s2c_stop_skill, OnStopSkill_ServerMessage);
		AddListener(ComponentEventName.Event_s2c_cast_skill_fail_notify, OnCastSkillFail_ServerMessage);
		AddListener(ComponentEventName.Dead, OnOwnerDead);

		if (m_Property.IsMain())
		{
			AddListener(ComponentEventName.PostWeaponFire, OnWeaponFire);

//#if NewSkill
//#else
            AddListener(ComponentEventName.CastSkill, OnSkillHotkey);
//#endif
			AddListener(ComponentEventName.Event_s2c_skill_effect, OnSkillEffect_ServerMessage);
			AddListener(ComponentEventName.Event_s2c_heal_effect, OnSkillHealEffect_ServerMessage);
			//AddListener(ComponentEventName.Event_s2c_effect_immuno, OnSkillImmuno);
		}
	}
	
	public override void OnDrawGizmo()
	{
	}

	public override void OnDestroy()
	{
		StopAllSkill();
		base.OnDestroy();
	}


	#endregion

	#region 技能释放主体逻辑

	/// <summary>
	/// 响应客户端的输入释放技能
	/// 会检查是否已经存在指定ID的技能实例, 如果目前没有对应的技能实例则创建并释放
	/// </summary>
	/// <param name="skillID"></param>
	private void ReleaseSkillByClientInput(int skillID)
	{
		if (!CanCreateSkill(skillID) || IsReleasingSkill(skillID) || m_SkillProxy.IsMainPlayerUsingWeaponSkill())
		{
			// UNDONE, 此处应有报错
			NotifyUnsuccessfulSkillRelease(skillID);
			return;
		}

		SkillBase newSkill = CreateSkill(skillID);
		if (!newSkill.CanReleaseSkill())
		{
			NotifyUnsuccessfulSkillRelease(skillID);
			return;
		}

		if (newSkill != null)
		{
			if (m_RemoveSkillList.Contains(skillID))
				m_RemoveSkillList.Remove(skillID);

			m_IDToSkill[skillID] = newSkill;
			m_Property.SetCurrentSkillID(skillID);

			m_State = newSkill.ReleaseSkillByClientInput();
			m_Property.SetCurrentSkillState(m_State);
		}
		else
		{
			Debug.LogErrorFormat("技能释放失败. ID: {0}", skillID);
		}
	}

	/// <summary>
	/// 响应服务器消息释放技能
	/// 如果目前存在指定ID的技能实例, 则停止原来的技能, 释放新的技能.  (完全相信服务器)
	/// </summary>
	/// <param name="skillID"></param>
	private void ReleaseSkillByServerMessage(int skillID, uint targetID, Vector3 offset, int launcherIndex, bool ignorePrediction = false)
	{
		if (IsReleasingSkill(skillID))
		{
			StopSkill(skillID, false);
		}

		if (IsPredict(skillID))
			return;

		SkillBase newSkill = CreateSkill(skillID);
		if (newSkill != null)
		{
			if (m_RemoveSkillList.Contains(skillID))
				m_RemoveSkillList.Remove(skillID);

			m_IDToSkill[skillID] = newSkill;
			m_Property.SetCurrentSkillID(skillID);
		}

		SpacecraftEntity target = m_GameplayProxy.GetEntityById<SpacecraftEntity>(targetID) as SpacecraftEntity;

		m_State = newSkill.ReleaseSkillByServerMessage(targetID, offset, launcherIndex, ignorePrediction);
		m_Property.SetCurrentSkillState(m_State);
	}

	// TODO. 这是一个临时接口
	// 因为本来是打算把CastSkill消息里的targetPosition也放入TargetList的, 但是服务器改动实在太大, 当前时间比较紧迫, 没有那么多时间去测试了
	// 暂时这里写代码区分单目标和多目标技能
	private void ReleaseSkillByServerMessage_MultiTarget(int skillID, List<S2C_TargetInfo> targetInfoList, Vector3 offset, int launcherIndex)
	{
		if (IsReleasingSkill(skillID))
		{
			StopSkill(skillID, false);
		}

		if (IsPredict(skillID))
			return;

		SkillBase newSkill = CreateSkill(skillID);
		if (newSkill != null)
		{
			if (m_RemoveSkillList.Contains(skillID))
				m_RemoveSkillList.Remove(skillID);

			m_IDToSkill[skillID] = newSkill;
			m_Property.SetCurrentSkillID(skillID);
		}

		m_State = newSkill.ReleaseSkillByServerMessage_MultiTarget(targetInfoList, offset, launcherIndex);
		m_Property.SetCurrentSkillState(m_State);
	}

	/// <summary>
	/// 给技能实例提供Update接口
	/// </summary>
	/// <param name="delta"></param>
	public override void OnUpdate(float delta)
	{
		for (int iSkill = 0; iSkill < m_RemoveSkillList.Count; iSkill++)
		{
			m_IDToSkill.Remove(m_RemoveSkillList[iSkill]);
		}
		m_RemoveSkillList.Clear();

		foreach (var pair in m_IDToSkill)
		{
			m_State = pair.Value.OnUpdate();
			if (pair.Key == m_Property.GetCurrentSkillID())
				m_Property.SetCurrentSkillState(m_State);
		}

		if (m_Property.IsMain())
		{
			//  调试代码
			if (Input.GetKeyDown(KeyCode.Keypad0))
			{
				m_State = SkillState.Idle;
				m_Property.SetCurrentSkillState(m_State);
				m_IDToSkill.Clear();
				m_RemoveSkillList.Clear();
				m_Property.SetCurrentSkillID(-1);
				Debug.LogError("清理技能状态");
			}
		}

		/// 弱点提示cd
		if (m_WeakSoundTmpCD < m_WeakSoundCD)
		{
			m_WeakSoundTmpCD += Time.deltaTime;
		}

		SendKeyPressContinualForTriggerSkill(m_SkillKeyPressed);
	}

	/// <summary>
	/// 把外部控制消息转发给技能实例
	/// 包括 硬件输入消息, 网络消息
	/// </summary>
	/// <param name="skillID"></param>
	/// <param name="controlEvent"></param>
	private void SendEventToSkill(int skillID, SkillControlEvent controlEvent, SkillEventParam param)
	{
		// 当技能释放失败时, 报错. 这种时候也不检查客户端是否有对应技能, 不过按理说请求都发给服务器了, 报错, 应该客户端还在释放这个技能.
		if (controlEvent == SkillControlEvent.ServerMsg_OnFail)
		{
			// UNDONE 打印报错提示

			if (!IsReleasingSkill(skillID))
			{
				// 可能有我没考虑到的逻辑
				Assert.IsTrue(false, skillID.ToString());
			}
			else
			{
				StopSkill(skillID, false);
			}
		}

		// 如果找不到服务器要求释放的技能, 表明这个技能已经结束了或者从来没有创建过
		// 已知在如下情况下会出现这种情况
		// 1. 开启了客户端预言. 等释放技能的消息回来, 技能已经结束了
		// 2. 客户端和服务器某种状态或者数据不一致. 客户端没有成功创建技能
		if (!IsReleasingSkill(skillID))
		{
			//Assert.IsTrue(false, skillID.ToString()); // 处理非预言情况
			return;
		}

		m_State = m_IDToSkill[skillID].HandleEvent(controlEvent, param);
		m_Property.SetCurrentSkillState(m_State);
	}
	#endregion

	#region 外部消息处理

	private void OnWeaponFire(IComponentEvent componentEvent)
	{
	}

	private void OnSkillHotkey(IComponentEvent componentEvent)
	{
		m_SkillCastEvent = componentEvent as SkillCastEvent;

		OnSkillHotkeyImplementation(m_SkillCastEvent);
		
		// 不是武器技能的持续释放才在这里模拟, 否则在武器模拟
		m_SkillKeyPressed = m_SkillCastEvent.KeyPressed && !m_SkillCastEvent.IsWeaponSkill;
	}

	private void SendKeyPressContinualForTriggerSkill(bool keyPress)
	{
		if (keyPress)
			OnSkillHotkeyImplementation(m_SkillCastEvent);
	}

	/// <summary>
	/// 释放技能
	/// 也代表了 吟唱 / 其他人蓄力 的结束
	/// </summary>
	/// <param name="buf"></param>
	private void OnCastSkill_ServerMessage(IComponentEvent componentEvent)
	{
		S2C_CAST_SKILL_Event respond = componentEvent as S2C_CAST_SKILL_Event;
		int skillID = (int)(respond.msg.modifySkillID > 0 ? respond.msg.modifySkillID : respond.msg.skillID);
		if (respond.msg.modifySkillID > 0)
		{
			StopSkill((int)respond.msg.skillID);
		}

		if (IsReleasingSkill(skillID))
		{
			SendEventToSkill(skillID, SkillControlEvent.ServerMsg_OnCast
				, new SkillEventParam()
				{
					ServerMessage = respond.msg
				});
		}
		else
		{
			uint targetID = respond.msg.target_info_list.Count != 0 ? respond.msg.target_info_list[0].targetID : 0;
			if (!DoesSkillHaveMultiTargets(skillID) || targetID == 0)
			{
				ReleaseSkillByServerMessage(skillID
										, targetID
										, new Vector3(respond.msg.x, respond.msg.y, respond.msg.z)
										, respond.msg.hanging_point_id
										, respond.msg.modifySkillID > 0);
			}
			else
			{
				ReleaseSkillByServerMessage_MultiTarget(skillID
													, respond.msg.target_info_list
													, new Vector3(respond.msg.x, respond.msg.y, respond.msg.z)
													, respond.msg.hanging_point_id);
			}
		}
	}

	/// <summary>
	/// 其他人开始吟唱技能
	/// </summary>
	/// <param name="buf"></param>
	private void OnSingSkill_ServerMessage(IComponentEvent componentEvent)
	{
		S2C_SING_SKILL_Event respond = componentEvent as S2C_SING_SKILL_Event;

		if (IsReleasingSkill((int)respond.msg.skillID))
		{
			SendEventToSkill((int)respond.msg.skillID, SkillControlEvent.ServerMsg_OnSing
				, new SkillEventParam()
				{
					ServerMessage = respond.msg
				});
		}
		else
		{
			if (!m_Property.IsMain())
			{
				ReleaseSkillByServerMessage((int)respond.msg.skillID
										, respond.msg.target_id
										, new Vector3(respond.msg.x, respond.msg.y, respond.msg.z)
										, respond.msg.hanging_point_id);
			}
			else
			{
				// 只有其他玩家和怪物才可能因为服务器通知而被动释放技能. 
				// 主角是出现这种情况原因:
				// 1. 客户端预言技能, 已经释放完了
				// 2. 服务器消息顺序错误
			}
		}
	}
	
	/// <summary>
	/// 服务器通知客户端技能中断.
	/// 包括: 
	///		吟唱被打断(非主动结束)
	///		蓄力被打断(非主动结束)
	///		主动结束引导
	///		被动结束引导
	///		
	/// 之所以主动结束引导要发这个消息, 是因为吟唱和蓄力的正常结束可以用 S2C_CAST_SKILL 来作为标志
	/// </summary>
	/// <param name="buf"></param>
	private void OnStopSkill_ServerMessage(IComponentEvent componentEvent)
	{
		SkillBase.SLog("Server Stop");

		S2C_STOP_SKILL_Event respond = componentEvent as S2C_STOP_SKILL_Event;

		int skillID = (int)respond.msg.skillID;
		SendEventToSkill(skillID, SkillControlEvent.ServerMsg_OnStop
			, new SkillEventParam()
			{
				ServerMessage = respond.msg
			});
	}

	/// <summary>
	/// 技能释放失败的通知
	/// </summary>
	/// <param name="componentEvent"></param>
	private void OnCastSkillFail_ServerMessage(IComponentEvent componentEvent)
	{
		SkillBase.SLog("Server Fail");

		S2C_CAST_SKILL_FAIL_NOTIFY_Event respond = componentEvent as S2C_CAST_SKILL_FAIL_NOTIFY_Event;

		if (IsReleasingSkill((int)respond.msg.skillID))
		{
			StopSkill((int)respond.msg.skillID, false);
		}

        Debug.Log("OnCastSkillFail => " + (CastSpellCode)respond.msg.code);
	}

	/// <summary>
	/// 技能命中效果
	/// </summary>
	/// <param name="buf"></param>
	private void OnSkillEffect_ServerMessage(IComponentEvent componentEvent)
	{
		S2C_SKILL_EFFECT_Event respond = componentEvent as S2C_SKILL_EFFECT_Event;

        uint MainID = m_GameplayProxy.GetMainPlayerUID();

        SpacecraftEntity caster = m_GameplayProxy.GetEntityById<SpacecraftEntity>(respond.msg.wCasterID) as SpacecraftEntity;
		SpacecraftEntity target = m_GameplayProxy.GetEntityById<SpacecraftEntity>(respond.msg.wTargetHeroID) as SpacecraftEntity;

		if ((caster != null && caster.IsMain()) || 
            (target != null && target.IsMain()) || 
            (caster != null && caster.GetEntityFatherOwnerID()== MainID) ||
            (target != null && target.GetEntityFatherOwnerID() == MainID)
            )
		{
			SkillHurtInfo hurtInfo = MessageSingleton.Get<SkillHurtInfo>();
			hurtInfo.TargetID = respond.msg.wTargetHeroID;
			hurtInfo.IsCrit = respond.msg.crit_type != 0;
			hurtInfo.PenetrationDamage = (int)respond.msg.PenetrationDamage;
			hurtInfo.IsDodge = respond.msg.isdoge != 0;
			hurtInfo.EffectID = (int)(respond.msg.byAttackEvent == (byte)EffectType.TriggerAnomaly ? respond.msg.wTriggerID : 0);
			hurtInfo.IsWeak = respond.msg.crit_type == 3;

			for (int iDamage = 0; iDamage < respond.msg.count; iDamage++)
			{
				hurtInfo.Damage = (int)(respond.msg.wDamage / respond.msg.count);
				GameFacade.Instance.SendNotification(NotificationName.SkillHurt, hurtInfo);
			}

			// 采矿的表现 (矿物碎裂)
			SpacecraftEntity currentTargetEntity = m_Property.GetTarget();
			if (currentTargetEntity != null && currentTargetEntity.GetHeroType() == KHeroType.htMine && caster.GetTargetCollider() != null)
			{
				MineDamage mineEvent = new MineDamage();
				mineEvent.HitCollider = caster.GetTargetCollider();
				mineEvent.Damage = respond.msg.wDamage;
				currentTargetEntity.SendEvent(ComponentEventName.MineDamage, mineEvent);
			}
		}

		if (target && target.IsMain())
		{
			if (m_Property.GetUnderAttackWarningToneCountdown() == 0)
			{
				WwiseUtil.PlaySound(WwiseManager.voiceComboID, WwiseMusicSpecialType.SpecialType_Voice_Be_Attacked, WwiseMusicPalce.Palce_1st, false, null);
			}

			float countDown = m_CfgEternityProxy.GetGamingConfig(1).Value.Sound.Value.CountDown.Value.UnderAttackWarningTone;
			m_Property.SetUnderAttackWarningToneCountdown(countDown);
		}

		/// 播放弱点攻击声音提示
		/// TODO.crit_type暂时没有枚举
		if (caster && caster.IsMain())
		{
			if (m_WeakSoundTmpCD >= m_WeakSoundCD && respond.msg.crit_type == 3)
			{
				float val = Random.value;
				if (val <= m_WeakSoundProbability)
				{
					SendEvent(ComponentEventName.PlayVideoSound, new PlayVideoSound()
					{
						GroupID = (int)m_CfgEternityProxy.GetGamingConfig(1).Value.Mine.Value.Sound.Value.WeaknessSoundVideo
					});
					m_WeakSoundTmpCD = 0;
				}
			}
		}
	}

	/// <summary>
	/// 技能治疗效果
	/// </summary>
	/// <param name="buf"></param>
	private void OnSkillHealEffect_ServerMessage(IComponentEvent componentEvent)
	{
		S2C_HEAL_EFFECT_Event respond = componentEvent as S2C_HEAL_EFFECT_Event;

		SpacecraftEntity caster = m_GameplayProxy.GetEntityById<SpacecraftEntity>(respond.msg.wCasterID) as SpacecraftEntity;
		SpacecraftEntity target = m_GameplayProxy.GetEntityById<SpacecraftEntity>(respond.msg.wTargetHeroID) as SpacecraftEntity;
		if ((caster != null && caster.IsMain()) || (target != null && target.IsMain()))
		{
			BuffHurtInfo hurtInfo = MessageSingleton.Get<BuffHurtInfo>();
			hurtInfo.targetID = respond.msg.wTargetHeroID;
			hurtInfo.type = (EffectType)respond.msg.Healtype;
			hurtInfo.value = (int)respond.msg.wHealValue;
			GameFacade.Instance.SendNotification(NotificationName.BuffHurt, hurtInfo);
		}
	}

	private void OnOwnerDead(IComponentEvent componentEvent)
	{
		StopAllSkill();
	}
	
	private void OnSkillHotkeyImplementation(SkillCastEvent skillCast)
	{
		PlayerSkillVO skillVO;
		if (skillCast.IsWeaponSkill)
		{
			if (m_SkillProxy.UsingReformer())
			{
				skillVO = m_SkillProxy.GetReformerSkill();
			}
			else
			{
				skillVO = m_SkillProxy.GetCurrentWeaponSkill();
				if (skillVO != null && !m_CfgSkillProxy.IsWeaponSkill(skillVO.GetID()))
				{
					Debug.LogErrorFormat("技能 {0} 被配置在武器上, 但它并不是一个武器技能! 找策划确认一下技能表和武器表", skillVO.GetID());
				}
			}
		}
		else
		{
			skillVO = m_SkillProxy.GetSkillByIndex(skillCast.SkillIndex);
			if (skillVO != null && m_CfgSkillProxy.IsWeaponSkill(skillVO.GetID()))
			{
				Debug.LogErrorFormat("技能 {0} 被配置在飞船上, 但它其实是一个武器技能! 找策划确认一下技能表和武器表", skillVO.GetID());
			}
		}

		if (skillVO != null)
		{
			int skillID = skillVO.GetID();

			bool keyPressed = skillCast.KeyPressed;

			if (keyPressed && !IsReleasingSkill(skillID))
			{
				// 释放Trigger技能的过程中, 可以释放非Trigger技能. 此时Trigger技能中断, 等非Trigger技能结束后继续放
				// 不是Trigger技能的话, 每次技能按键按下, 只能释放一次
				if ((m_CfgSkillProxy.IsTriggerSkill(skillID) && !OneOfReleasingSkillsIsNotTriggerSkill())
					|| (!m_CfgSkillProxy.IsTriggerSkill(skillID) && !SkillHasReleasedDuringPress(skillID)))
				{
					ReleaseSkillByClientInput(skillID);
					SetSkillReleaseDuringPress(skillID, true);

					if (m_CfgSkillProxy.IsTriggerSkill(skillID))
					{
						m_Property.SetTriggerSkillID(skillID);
						m_Property.SetReleasingTriggerSkill(true);
					}
				}
			}
			else
			{
				SkillControlEvent skillEvent = keyPressed ? SkillControlEvent.Event_SkillButtonPress : SkillControlEvent.Event_SkillButtonRelease;
				SendEventToSkill(skillID, skillEvent, null);

				if (!keyPressed)
				{
					SetSkillReleaseDuringPress(skillID, false);
					m_Property.SetReleasingTriggerSkill(false);
					m_Property.SetTriggerSkillID(-1);
				}
			}
		}
	}
	#endregion

	#region 释放技能

	private bool CanCreateSkill(int skillID)
	{
		SkillBase.SLog("CanCreateSkill");

		SkillSystemGrow skillGrow = m_CfgSkillProxy.GetSkillGrow(skillID);
		if (skillGrow.ByteBuffer == null)
		{
			SkillBase.SLog("没有这个技能");
			Debug.LogError("没有这个技能: " + skillID);
			return false;
		}

		if (m_Property.IsMain() && m_Property.GetCurrentState().GetMainState() == EnumMainState.Cruise)
		{
			SkillBase.SLog("巡航模式不能放技能");
			Debug.Log("巡航模式不能放技能");
			return false;
		}

		if (m_SkillProxy.IsOnCD(skillID) || m_SkillProxy.IsOnTriggerCD(skillID))
		{
			SkillBase.SLog("IsOnCD");
			return false;
		}

		return true;
	}

	/// <summary>
	/// 创建一个新的技能
	/// </summary>
	/// <param name="skillID"></param>
	/// <returns></returns>
	private SkillBase CreateSkill(int skillID)
	{
		SkillCategory skillType = (SkillCategory)m_CfgSkillProxy.GetSkillGrow(skillID).CastWay;

		SkillBase newSkillInstance = null;
		switch (skillType)
		{
			case SkillCategory.Immediate:
			case SkillCategory.Immediate_Trigger:
				newSkillInstance = new SkillImmediate();
				break;
			case SkillCategory.Immediate_MultiShot:
				newSkillInstance = new SkillImmediateMultiShot();
				break;
			case SkillCategory.Chanting:
			case SkillCategory.Chanting_Trigger:
				newSkillInstance = new SkillChanting();
				break;
			case SkillCategory.Charge:
				newSkillInstance = new SkillCharging();
				break;
			case SkillCategory.AutoChannel:
				newSkillInstance = new SkillChannellingBeam(SkillChannellingBeam.ChannellingType.Auto);
				break;
			case SkillCategory.ManualChannel:
				newSkillInstance = new SkillChannellingBeam(SkillChannellingBeam.ChannellingType.Manual);
				break;
			case SkillCategory.RapidFire:
				newSkillInstance = new SkillRapidFire();
				break;
			case SkillCategory.Immediate_MultiShot_MultiTarget:
				newSkillInstance = new SkillImmediateMultiShot_MultiTarget();
				break;
			case SkillCategory.Immediate_MultiTarget:
				newSkillInstance = new SkillImmediate_MultiTarget();
				break;
			case SkillCategory.SelectSkillByBuff:
				newSkillInstance = new SkillSelectByBuff();
				break;
			case SkillCategory.MiningBeam:
				newSkillInstance = new SkillMiningBeam(SkillMiningBeam.ChannellingType.Manual);
				break;
			default:
				newSkillInstance = new SkillBase();
				break;
		}

		if (newSkillInstance == null)
			return null;

		// FIXME 技能, 重构这部分代码
		// 消除动态分配, 弄一个池子
		if (m_Property.IsMain())
		{
			if (m_SkillProxy.GetSkillByID(skillID) != null)
				newSkillInstance.Initialize(m_Property, m_SkillProxy.GetSkillByID(skillID), OnSkillStopped);
			else
				return null;
		}
		else
		{
			PlayerSkillVO skillVO = PlayerSkillVO.CreateSkillVO(skillID);
			if (skillVO != null)
				newSkillInstance.Initialize(m_Property, skillVO, OnSkillStopped);
			else
				return null;
		}

		return newSkillInstance;
	}

	private void StopSkill(int skillID, bool success = true)
	{
		if (IsReleasingSkill(skillID))
			m_IDToSkill[skillID].Finish(success);
	}

	private void StopAllSkill()
	{
		List<int> allSkillID = new List<int>();
		foreach (var pair in m_IDToSkill)
		{
			allSkillID.Add(pair.Key);
		}

		for (int i = 0; i < allSkillID.Count; i++)
		{
			m_IDToSkill[allSkillID[i]].Finish();
		}
	}

	private bool IsReleasingSkill(int skillID)
	{
		return m_IDToSkill.ContainsKey(skillID) && !m_RemoveSkillList.Contains(skillID);
	}

	/// <summary>
	/// 在这一次的按键按下的过程中, 这个技能是不是已经释放过了
	/// </summary>
	/// <param name="skillID"></param>
	/// <returns></returns>
	private bool SkillHasReleasedDuringPress(int skillID)
	{
		if (!m_ReleasedSkillDuringPress.ContainsKey(skillID))
		{
			return false;
		}
		else
		{
			return m_ReleasedSkillDuringPress[skillID];
		}
	}

	/// <summary>
	/// 标记一下, 指定技能在这次按下中是否已经释放过了
	/// </summary>
	/// <param name="skillID"></param>
	/// <param name="released"></param>
	private void SetSkillReleaseDuringPress(int skillID, bool pressed)
	{
		Debug.LogFormat("MissileTest| 设置技能的按下状态: {0}, {1}", skillID, pressed);
		m_ReleasedSkillDuringPress[skillID] = pressed;
	}

	/// <summary>
	/// 有任意一个非Trigger技能正在释放
	/// 用于在按住Trigger技能的按键时, 可以释放其他非Trigger技能
	/// </summary>
	/// <returns></returns>
	private bool OneOfReleasingSkillsIsNotTriggerSkill()
	{
		foreach (var pair in m_IDToSkill)
		{
			if (!m_CfgSkillProxy.IsTriggerSkill(pair.Key))
				return true;
		}

		return false;
	}

	private bool DoesSkillHaveMultiTargets(int skillID)
	{
		return m_CfgSkillProxy.SkillSupportMultiTargets(skillID);
	}

	private void NotifyUnsuccessfulSkillRelease(int skillID)
	{
		UnsuccessfulReleaseOfSkillEvent skillEvent = new UnsuccessfulReleaseOfSkillEvent()
		{
			SkillID = skillID
		};
		m_Property.GetOwner().SendEvent(ComponentEventName.UnsuccessfulReleaseOfSkill, skillEvent);
	}

	#endregion

	#region 初始化和预加载

	/// <summary>
	/// 设置所有技能类型, 是否使用客户端预言
	/// </summary>
	private void SetPreditionForAllSkillType()
	{
		// 开启客户端预言意味着技能释放和停止完全不理会服务器消息, 仅靠客户端的输入自行模拟
		// 只有主角才会有 客户端预言 这回事

		// SelectSkillByBuff 类型的技能, 子技能是Immediate类型, 而且子技能是服务器通知释放的
		// , 如果Immediate 是预言的, 那么IsPredict()这个判断就成了false, 就释放不出来
		SkillBase.s_SkillPredictionMap[SkillCategory.Immediate] = false; 
		SkillBase.s_SkillPredictionMap[SkillCategory.Immediate_MultiShot] = false;
		SkillBase.s_SkillPredictionMap[SkillCategory.Immediate_Trigger] = true;
		SkillBase.s_SkillPredictionMap[SkillCategory.Chanting] = false;
		SkillBase.s_SkillPredictionMap[SkillCategory.Chanting_Trigger] = false;
		SkillBase.s_SkillPredictionMap[SkillCategory.Charge] = false;
		SkillBase.s_SkillPredictionMap[SkillCategory.Charge_MultiShot] = false;
		SkillBase.s_SkillPredictionMap[SkillCategory.ManualChannel] = true;
		SkillBase.s_SkillPredictionMap[SkillCategory.AutoChannel] = true;
		SkillBase.s_SkillPredictionMap[SkillCategory.RapidFire] = true;
		SkillBase.s_SkillPredictionMap[SkillCategory.Immediate_MultiShot_MultiTarget] = false;
		SkillBase.s_SkillPredictionMap[SkillCategory.Immediate_MultiTarget] = true;
		SkillBase.s_SkillPredictionMap[SkillCategory.SelectSkillByBuff] = false;
		SkillBase.s_SkillPredictionMap[SkillCategory.MiningBeam] = true;
	}

	private bool IsPredict(int skillID)
	{
		SkillCategory skillType = (SkillCategory)m_CfgSkillProxy.GetSkillGrow(skillID).CastWay;
		if (!SkillBase.s_SkillPredictionMap.ContainsKey(skillType) || !m_Property.IsMain())
			return false;
		else
			return SkillBase.s_SkillPredictionMap[skillType];
	}

	/// <summary>
	/// 预加载玩家飞船的所有技能的资源
	/// </summary>
	private void PreloadSkill()
	{
		// 读取当前Entity的飞船的所有技能
		List<int> shipSkillIDList = new List<int>();

		// TODO 技能. 现在技能使用物品的方式存储. 自己的技能由服务器通知. 服务器不会告知其他玩家的技能
		if (m_Property.IsMain())
		{
			PlayerSkillVO[] skills = m_SkillProxy.GetSkills();
			for (int iSkill = 0; iSkill < skills.Length; iSkill++)
			{
				shipSkillIDList.Add(skills[iSkill].GetID());
			}
		}
		else if (m_Property.GetNPCTemplateVO().ByteBuffer != null)
		{
		}

		// 预加载技能所需资源
		for (int iSkill = 0; iSkill < shipSkillIDList.Count; ++iSkill)
		{
			SkillSystemGrow skillGrow = m_CfgSkillProxy.GetSkillGrow(shipSkillIDList[iSkill]);
			if (skillGrow.ByteBuffer == null)
				continue;

			SkillSystemPath skillPath = m_CfgSkillProxy.GetSkillPath(skillGrow.PathID);
			if (skillPath.ByteBuffer == null)
				continue;

			for (int iFx = 0; iFx < skillPath.FxIdLength; iFx++)
			{
				SkillSystemFx skillFx = m_CfgSkillProxy.GetSkillFx(skillPath.GetFxIdArray()[iFx]);
				// 加载技能特效资源
				if (!string.IsNullOrEmpty(skillFx.CastFxBegin) && !"None".Equals(skillFx.CastFxBegin))
					LoadAssetAsync(skillFx.CastFxBegin);
				if (!string.IsNullOrEmpty(skillFx.CastFxLoop) && !"None".Equals(skillFx.CastFxLoop))
					LoadAssetAsync(skillFx.CastFxLoop);
				if (!string.IsNullOrEmpty(skillFx.CastFxEnd) && !"None".Equals(skillFx.CastFxEnd))
					LoadAssetAsync(skillFx.CastFxEnd);
				if (!string.IsNullOrEmpty(skillFx.LauncherFx) && !"None".Equals(skillFx.LauncherFx))
					LoadAssetAsync(skillFx.LauncherFx);
				if (!string.IsNullOrEmpty(skillFx.AssetName) && !"None".Equals(skillFx.AssetName))
					LoadAssetAsync(skillFx.AssetName);
				if (!string.IsNullOrEmpty(skillFx.HitFx) && !"None".Equals(skillFx.HitFx))
					LoadAssetAsync(skillFx.HitFx);

				// UNDONE 技能, 音效
				// 加载音频资源
				//AudioProxy audioProxy = DataManager.GetProxy<AudioProxy>();
				//if (skillFx.BeforeAudio > 0)
				//{
				//	AudioVO audioVO = audioProxy.GetAudioByKey(skillFx.BeforeAudio);
				//	if (audioVO.ByteBuffer != null && !"None".Equals(audioVO.AssetBundle) && !"None".Equals(audioVO.AssetName))
				//		AssetManager.LoadAsset<GameObject>(audioVO.AssetName, PreloadCallback);
				//}
				//if (skillFx.LauncherAudio > 0)
				//{
				//	AudioVO audioVO = audioProxy.GetAudioByKey(skillFx.LauncherAudio);
				//	if (audioVO.ByteBuffer != null && !"None".Equals(audioVO.AssetBundle) && !"None".Equals(audioVO.AssetName))
				//		AssetManager.LoadAsset<GameObject>(audioVO.AssetName, PreloadCallback);
				//}
				//if (skillFx.HitAudio > 0)
				//{
				//	AudioVO audioVO = audioProxy.GetAudioByKey(skillFx.HitAudio);
				//	if (audioVO.ByteBuffer != null && !"None".Equals(audioVO.AssetBundle) && !"None".Equals(audioVO.AssetName))
				//		AssetManager.LoadAsset<GameObject>(audioVO.AssetName, PreloadCallback);
				//}
				//if (skillFx.SkillAudio > 0)
				//{
				//	AudioVO audioVO = audioProxy.GetAudioByKey(skillFx.SkillAudio);
				//	if (audioVO.ByteBuffer != null && !"None".Equals(audioVO.AssetBundle) && !"None".Equals(audioVO.AssetName))
				//		AssetManager.LoadAsset<GameObject>(audioVO.AssetName, PreloadCallback);
				//}
			}
		}
	}

    /// <summary>
    /// 加载资源
    /// </summary>
    /// <param name="path"></param>
    private  void LoadAssetAsync(string path)
    {
        AssetUtil.LoadAssetAsync(path,
            (pathOrAddress, returnObject, userData) =>
            {
                if (returnObject != null)
                {
					GameObject prefab = returnObject as GameObject;
					if (prefab.CountPooled() < 10)
						prefab.CreatePool(10, path);
                }
                else
                {
                    Debug.LogError(string.Format("资源加载成功，但返回null,  pathOrAddress = {0}", pathOrAddress));
                }
            });
    }

	#endregion

	/// <summary>
	/// 技能结束的回调. 用于清理对于技能的引用
	/// </summary>
	public void OnSkillStopped(int skillID)
	{
		//m_IDToSkill.Remove(skillID);
		m_RemoveSkillList.Add(skillID);

		if (m_Property.GetCurrentSkillID() == skillID)
			m_Property.SetCurrentSkillID(-1);

		Debug.LogFormat("MissileTest| 清理技能的按下状态. ID: {0}", skillID);
		m_ReleasedSkillDuringPress.Remove(skillID);
	}
}
