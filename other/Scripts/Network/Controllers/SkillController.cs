using Assets.Scripts.Lib.Net;
using Assets.Scripts.Proto;
using Crucis.Protocol;
using Crucis.Protocol.GameSession;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Frame.Net
{
    public class SkillController : AbsRpcController
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public SkillController() : base()
        {
			//ListenGameServer(KS2C_Protocol.s2c_cast_skill, OnCastSkill, typeof(S2C_CAST_SKILL));
			//ListenGameServer(KS2C_Protocol.s2c_sing_skill, OnSingSkill, typeof(S2C_SING_SKILL));
			//ListenGameServer(KS2C_Protocol.s2c_stop_skill, OnStopSkill, typeof(S2C_STOP_SKILL));
			//ListenGameServer(KS2C_Protocol.s2c_cast_skill_fail_notify, OnCastSkillFail, typeof(S2C_CAST_SKILL_FAIL_NOTIFY));
			//ListenGameServer(KS2C_Protocol.s2c_skill_effect, OnSkillEffect, typeof(S2C_SKILL_EFFECT));
			//ListenGameServer(KS2C_Protocol.s2c_weapon_value, OnWeaponPowerChanged, typeof(S2C_WEAPON_VALUE));
			//ListenGameServer(KS2C_Protocol.s2c_sync_hero_hpmp_anger, OnHeroHpMpAnger, typeof(S2C_SYNC_HERO_HPMP_ANGER));
		}

		#region S2C


		//private void OnCastSkill(KProtoBuf buffer)
		//{
		//	S2C_CAST_SKILL respond = (S2C_CAST_SKILL)buffer;
		//	m_EntityManager.SendEventToEntity(respond.casterID, ComponentEventName.Event_s2c_cast_skill, new S2C_CAST_SKILL_Event()
		//	{
		//		casterID = respond.casterID,
		//		skillID = respond.skillID,
		//		modifySkillID = respond.modifySkillID,
		//		targetID = respond.targetID,
		//		x = respond.x,
		//		y = respond.y,
		//		z = respond.z,
		//		hanging_point_id = respond.hanging_point_id
		//	});
		//}

		//private void OnSingSkill(KProtoBuf buffer)
		//{
		//	S2C_SING_SKILL respond = (S2C_SING_SKILL)buffer;
		//	m_EntityManager.SendEventToEntity(respond.casterID, ComponentEventName.Event_s2c_sing_skill, new S2C_SING_SKILL_Event()
		//	{
		//		casterID = respond.casterID,
		//		skillID = respond.skillID,
		//		target_id = respond.target_id,
		//		x = respond.x,
		//		y = respond.y,
		//		z = respond.z
		//	});
		//}

		//private void OnStopSkill(KProtoBuf buffer)
		//{
		//	S2C_STOP_SKILL respond = (S2C_STOP_SKILL)buffer;
		//	m_EntityManager.SendEventToEntity(respond.casterID, ComponentEventName.Event_s2c_stop_skill, new S2C_STOP_SKILL_Event()
		//	{
		//		casterID = respond.casterID,
		//		skillID = respond.skillID,
		//		modifySkillID = respond.modifySkillID
		//	});
		//}

		//private void OnCastSkillFail(KProtoBuf buffer)
		//{
		//	S2C_CAST_SKILL_FAIL_NOTIFY respond = (S2C_CAST_SKILL_FAIL_NOTIFY)buffer;
		//	m_EntityManager.SendEventToEntity(m_GameplayProxy.GetMainPlayerUID(), ComponentEventName.Event_s2c_cast_skill_fail_notify
		//		, new S2C_CAST_SKILL_FAIL_NOTIFY_Event()
		//		{
		//			skillID = respond.skillID,
		//			code = respond.code
		//		});
		//}

		//private void OnSkillEffect(KProtoBuf buffer)
		//{
		//	S2C_SKILL_EFFECT respond = (S2C_SKILL_EFFECT)buffer;
		//	m_EntityManager.SendEventToEntity(m_GameplayProxy.GetMainPlayerUID(), ComponentEventName.Event_s2c_skill_effect, new S2C_SKILL_EFFECT_Event()
		//	{
		//		wTargetHeroID = respond.wTargetHeroID,
		//		wCasterID = respond.wCasterID,
		//		wTriggerID = respond.wTriggerID,
		//		wDamage = respond.wDamage,
		//		PenetrationDamage = respond.PenetrationDamage,
		//		crit_type = respond.crit_type,
		//		isdoge = respond.isdoge,
		//		byAttackEvent = respond.byAttackEvent
		//	});
		//}

		//private void OnWeaponPowerChanged(KProtoBuf buffer)
		//{
		//	S2C_WEAPON_VALUE msg = buffer as S2C_WEAPON_VALUE;

		//	HeroVO heroVO = m_PlayerProxy.MainHeroVO;
		//	if (heroVO == null)
		//		return;

		//	foreach (WEAPONVALUE info in msg.infos)
		//	{
		//		KWeaponPos weaponType = (KWeaponPos)info.weapon_type;

		//		if (!heroVO.WeaponPowers.ContainsKey(weaponType))
		//			heroVO.WeaponPowers.Add(weaponType, new WeaponPowerVO());

		//		WeaponPowerVO power = heroVO.WeaponPowers[weaponType];
		//		power.WeaponType = info.weapon_type;
		//		power.CurrentValue = info.cur_value;
		//		power.MaxValue = info.max_value;
		//		power.SafeValue = info.safty_valve;

		//		if (power.SafeValue > 0)
		//		{
		//			if (power.CurrentValue >= power.MaxValue)
		//				power.ForceCooldown = true;
		//			else if (power.CurrentValue <= power.SafeValue)
		//				power.ForceCooldown = false;
		//		}
		//	}

		//	GameFacade.Instance.SendNotification(NotificationName.MSG_CHARACTER_WEAPON_POWER_CHANGED);
		//}

		///// <summary>
		///// 同步HP、MP
		///// </summary>
		///// <param name="buf"></param>
		//private void OnHeroHpMpAnger(KProtoBuf buf)
		//{
		//	S2C_SYNC_HERO_HPMP_ANGER msg = buf as S2C_SYNC_HERO_HPMP_ANGER;
		//	SceneProxy sceneProxy = GameFacade.Instance.RetrieveProxy(ProxyName.SceneProxy) as SceneProxy;
		//	SpacecraftEntity entity = m_GameplayProxy.GetEntityById<SpacecraftEntity>(msg.heroID);

		//	if (entity != null)
		//	{
		//		entity.SetAttribute(AircraftAttributeType.HP, msg.hp);
		//		entity.SetAttribute(AircraftAttributeType.DefenseShield, msg.defense_shield_value);
		//		entity.SetAttribute(AircraftAttributeType.ManaShield, msg.mana_shield_value);
		//		entity.SetAttribute(AircraftAttributeType.Shield, msg.shield_value);
		//		entity.SetAttribute(AircraftAttributeType.SuperMagnetic, msg.superMagnetic);
		//		entity.SetAttribute(AircraftAttributeType.EnergyPower, msg.energyPower);
		//		entity.SetPeerless((uint)msg.current_peerless);
		//		if (msg.heroID == m_GameplayProxy.GetMainPlayerUID())
		//		{
		//			Debug.Log("无双值修改: " + msg.current_peerless);
		//		}

		//		GameFacade.Instance.SendNotification(NotificationName.MSG_HP_CHANGED, msg.heroID);
		//	}
		//}

		#endregion

		#region C2S

		/// <summary>
		/// 请求放技能
		/// </summary>
		/// <param name="skillID"></param>
		/// <param name="castOperation"></param>
		/// <param name="casterUID"></param>
		/// <param name="targetUID"></param>
		/// <param name="targetDirEuler"></param>
		/// <param name="cameraDirEuler"></param>
		public void SendCastSkill(uint skillID, SkillCastOperation castOperation, uint casterUID, uint targetUID, UnityEngine.Quaternion targetDir)
		{
			C2S_CAST_SKILL_TPS msg = SingleInstanceCache.GetInstanceByType<C2S_CAST_SKILL_TPS>();
			msg.protocolID = (ushort)KC2S_Protocol.c2s_cast_skill_tps;
			msg.cast_type = (short)castOperation;
			msg.skill_id = skillID;
			msg.caster_id = casterUID;
			msg.target_info_list.Clear();

			C2S_TargetInfo targetInfo = new C2S_TargetInfo();
			targetInfo.target_id = targetUID;
			targetInfo.count = 1;

			msg.target_info_list.Add(targetInfo);

			msg.x = targetDir.x;
			msg.y = targetDir.y;
			msg.z = targetDir.z;
			msg.w = targetDir.w;

			NetworkManager.Instance.SendToGameServer(msg);
		}

		public void SendCastSkill(uint skillID, SkillCastOperation castOperation, uint casterUID, List<C2S_TargetInfo> skillTargets
			, UnityEngine.Quaternion targetDir)
		{
			C2S_CAST_SKILL_TPS msg = SingleInstanceCache.GetInstanceByType<C2S_CAST_SKILL_TPS>();
			msg.protocolID = (ushort)KC2S_Protocol.c2s_cast_skill_tps;
			msg.cast_type = (short)castOperation;
			msg.skill_id = skillID;
			msg.caster_id = casterUID;
			msg.target_info_list.Clear();
			msg.target_info_list = skillTargets;

			msg.x = targetDir.x;
			msg.y = targetDir.y;
			msg.z = targetDir.z;
			msg.w = targetDir.w;

			NetworkManager.Instance.SendToGameServer(msg);
		}

		/// <summary>
		/// 速射炮技能改变目标
		/// </summary>
		public void SendRapidFireChangeTarget(uint skillID, uint casterUID, uint targetUID, Quaternion targetDir)
		{
			// UNDONE 技能重构, 这里应该附带技能释放点
			C2S_CHANGE_SKILL_TARGET msg = new C2S_CHANGE_SKILL_TARGET();
			msg.protocolID = (ushort)KC2S_Protocol.c2s_change_skill_target;
			msg.skill_id = skillID;
			msg.target_id = (int)targetUID;

			msg.x = targetDir.x;
			msg.y = targetDir.y;
			msg.z = targetDir.z;
			msg.w = targetDir.w;

			NetworkManager.Instance.SendToGameServer(msg);
		}

		/// <summary>
		/// 请求改变当前武器
		/// 默认只有两个武器
		/// </summary>
		public void RequestChangeWeapon(ulong weaponUID)
		{
			//C2S_CHANGE_MAIN_WEAPON msg = SingleInstanceCache.GetInstanceByType<C2S_CHANGE_MAIN_WEAPON>();
			//msg.protocolID = (ushort)KC2S_Protocol.c2s_change_main_weapon;
			//msg.use_weapon_oid = weaponUID;
			//NetworkManager.Instance.SendToGameServer(msg);

            c2s_WeaponChange msg = new c2s_WeaponChange();
            msg.UseWeaponUid = weaponUID;
            RequestWeaponChangeRPC.RequestWeaponChange(msg);
        }

        /// <summary>
        /// 请求改变当前武器
        /// 默认只有两个武器
        /// </summary>
        public void SendMissileLockTarget(bool lockTarget)
		{
// 			C2S_LOCK_TARGET msg = SingleInstanceCache.GetInstanceByType<C2S_LOCK_TARGET>();
// 			msg.protocolID = (ushort)KC2S_Protocol.c2s_lock_target;
// 			msg.type = (short)(lockTarget ? 0 : 1);
// 			NetworkManager.Instance.SendToGameServer(msg);


            c2s_lock_target msg = new c2s_lock_target();
            msg.Type = (uint)(lockTarget ? 0 : 1);
            RequestLockTargetRPC.RequestLockTarget(msg);
        }
        #endregion
    }
}
