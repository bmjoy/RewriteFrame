using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Define;
using Assets.Scripts.Lib.Net;
using Assets.Scripts.Proto;
using Eternity.Runtime.Item;
using Game.Frame.Net;
using UnityEngine;

public class ProtocolLogic : BaseNetController
{
    private EntityManager m_EntityManager;
	private GameplayProxy m_GameplayProxy;
	private PlayerSkillProxy m_PlayerSkillProxy;

	public void OnInitialize(EntityManager entityManager)
    {
        m_EntityManager = entityManager;

        ListenGameServer();

		m_GameplayProxy = GameFacade.Instance.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;
		m_PlayerSkillProxy = GameFacade.Instance.RetrieveProxy(ProxyName.PlayerSkillProxy) as PlayerSkillProxy;
	}

    public void SendGsAppleScene()
    {
        C2S_APPLY_SCENE_OBJ msg = SingleInstanceCache.GetInstanceByType<C2S_APPLY_SCENE_OBJ>();
        msg.protocolID = (byte)KC2S_Protocol.c2s_apply_scene_obj;
        NetworkManager.Instance.SendToGameServer(msg);
    }

    public void SendOpenPvd(bool isOpen)
    {
        C2S_OPEN_PVD msg = SingleInstanceCache.GetInstanceByType<C2S_OPEN_PVD>();
        msg.protocolID = (byte)KC2S_Protocol.c2s_open_pvd;
        if (isOpen)
        {
            msg.flag = 1;
        }
        else
        {
            msg.flag = 0;
        }
        NetworkManager.Instance.SendToGameServer(msg);
    }

    private void ListenGameServer()
    {
        #region FFF
        NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_sync_scene_time_frame, FFF, typeof(S2C_SYNC_SCENE_TIME_FRAME));
        NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_remove_scene_obj, FFF, typeof(S2C_REMOVE_SCENE_OBJ));
        NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_monster_road_point, FFF, typeof(S2C_MONSTER_ROAD_POINT));
        NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_line_info_respond, FFF, typeof(S2C_LINE_INFO_RESPOND));
        NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_sync_scene_obj_end, FFF, typeof(S2C_SYNC_SCENE_OBJ_END));
        NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_hero_pos_info, FFF, typeof(S2C_HERO_POS_INFO));
        NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_battle_startedframe, FFF, typeof(S2C_BATTLE_STARTEDFRAME));
        #endregion FFF

        NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_sync_new_hero, OnSyncNewHero, typeof(S2C_SYNC_NEW_HERO));
        NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_sync_hero_state, OnHeroState, typeof(S2C_SYNC_HERO_STATE));
        NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_monster_road_point, OnMonsterRoadPoint, typeof(S2C_MONSTER_ROAD_POINT));
        NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_force_change_pos, OnForceChangePos, typeof(S2C_FORCE_CHANGE_POS));
        NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_remove_scene_obj, OnRemoveSceneObj, typeof(S2C_REMOVE_SCENE_OBJ));
        NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_sync_scene_obj_end, OnSyncSceneObjEnd, typeof(S2C_SYNC_SCENE_OBJ_END));
		NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_attr_change, OnHeroAttributeChanged, typeof(S2C_ATTR_CHANGE));
        NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_sync_contact_geometry, OnSyncContactGeometry, typeof(S2C_SYNC_CONTACT_GEOMETRY));
		NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_enter_battle, OnEnterBattle, typeof(S2C_ENTER_BATTLE));
		NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_leave_battle, OnLeaveBattle, typeof(S2C_LEAVE_BATTLE));
		NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_sync_fightship_visible_item_list, OnSyncFightShipItems, typeof(S2C_SYNC_FIGHTSHIP_VISIBLE_ITEM_LIST));
		NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_sync_player_discover_precious, OnSyncDiscoverPrecious, typeof(S2C_SYNC_PLAYER_DISCOVER_PRECIOUS));
		NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_sync_detector_distance, OnSyncDeteCtorDistance, typeof(S2C_SYNC_DETECTOR_DISTANCE));
		NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_sync_camp_changed, OnCampChange, typeof(S2C_SYNC_CAMP_CHANGED));
		NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_sync_seal_changed, OnSealChange, typeof(S2C_SYNC_SEAL_CHANGED));

		// 技能
		NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_cast_skill, OnCastSkill, typeof(S2C_CAST_SKILL));
		NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_sing_skill, OnSingSkill, typeof(S2C_SING_SKILL));
		NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_stop_skill, OnStopSkill, typeof(S2C_STOP_SKILL));
		NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_cast_skill_fail_notify, OnCastSkillFail, typeof(S2C_CAST_SKILL_FAIL_NOTIFY));
		NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_skill_effect, OnSkillEffect, typeof(S2C_SKILL_EFFECT));
		NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_heal_effect, OnHealEffect, typeof(S2C_HEAL_EFFECT));
		NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_effect_immuno, OnEffectImmuno, typeof(S2C_EFFECT_IMMUNO));

		NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_weapon_value, OnWeaponPowerChanged, typeof(S2C_WEAPON_VALUE));
		NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_sync_hero_hpmp_anger, OnHeroHpMpAnger, typeof(S2C_SYNC_HERO_HPMP_ANGER));
		NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_add_buff_notify, OnBuffAdd, typeof(S2C_ADD_BUFF_NOTIFY));
		NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_del_buff_notify, OnBuffRemove, typeof(S2C_DEL_BUFF_NOTIFY));

        NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_error_code, OnErrorCode, typeof(S2C_ERROR_CODE));

        NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_relive_time, OnMainPlayerDeath, typeof(S2C_RELIVE_TIME));
        NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_hero_death, OnHeroDeath, typeof(S2C_HERO_DEATH));
        NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_hero_relive, OnHerorelive, typeof(S2C_HERO_RELIVE));
        NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_human_move_new, OnHumanMoveNew, typeof(S2C_HUMAN_MOVE_NEW));
        NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_sync_personal_drop, OnSyncMineDropItem, typeof(S2C_SYNC_PERSONAL_DROP));
        NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_notify_personal_drop_result, OnNotifyDropItemResult, typeof(S2C_NOTIFY_PERSONAL_DROP_RESULT));
		NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_sync_boss_hud, OnSyncBossHud, typeof(S2C_SYNC_BOSS_HUD));//boss出现弹出提示
		NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_open_cehst_by_key_result, OnOpenCehstByKeyResult, typeof(S2C_OPEN_CEHST_BY_KEY_RESULT));

	}

    private void OnHumanMoveNew(KProtoBuf buffer)
    {
        S2C_HUMAN_MOVE_NEW msg = buffer as S2C_HUMAN_MOVE_NEW;

        m_EntityManager.SendEventToEntity(msg.heroID, ComponentEventName.HumanMoveRespond, new HumanMoveRespond()
        {
            IsRun = msg.run_flag == 1,
            Position = VectorsToVector3(msg.position),
            Rotation = QuaternionToQuaternion(msg.rotation),
            TargetPosition = VectorsToVector3(msg.engine_axis)
        });
    }

    private void OnHerorelive(KProtoBuf buffer)
    {
        S2C_HERO_RELIVE respond = buffer as S2C_HERO_RELIVE;
        Debug.Log("OnHerorelive" + "heroID:" + respond.heroID);

        m_EntityManager.SendEventToEntity(respond.heroID, ComponentEventName.Relive, null);
    }

    /// <summary>
    /// 主玩家死亡
    /// </summary>
    /// <param name="buffer"></param>
    private void OnMainPlayerDeath(KProtoBuf buffer)
    {
        S2C_RELIVE_TIME respond = buffer as S2C_RELIVE_TIME;
        //Debug.LogError("OnMainPlayerDeath");

        GameplayProxy gameplayProxy = GameFacade.Instance.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;
        m_EntityManager.SendEventToEntity(m_GameplayProxy.GetMainPlayerUID(), ComponentEventName.Dead, new DeadEvent()
        {
            CD = respond.cd,
            KillerNpctemplateID = respond.killer_id,
            KillerPlayerName = respond.killer_player_name,
            ReliveOptions = respond.relive_type_list
        });
    }

    /// <summary>
    /// 英雄死亡
    /// </summary>
    /// <param name="buf"></param>
    private void OnHeroDeath(KProtoBuf buf)
    {
        S2C_HERO_DEATH respond = buf as S2C_HERO_DEATH;
		//NetworkManager.Instance.GetDropItemController().GetDropItemManager().SetDropItemInfoByDeath(m_GameplayProxy, respond.heroID, respond.drop_list);
		//Debug.Log("OnHeroDeath heroID:" + respond.heroID);

		//ShipItemsProxy shipItemsProxy = GameFacade.Instance.RetrieveProxy(ProxyName.ShipItemsProxy) as ShipItemsProxy;
		//shipItemsProxy.RemoveShipItems(respond.heroID);
        MSAIBossProxy msab = GameFacade.Instance.RetrieveProxy(ProxyName.MSAIBossProxy) as MSAIBossProxy;

		//李贺新的战场AI召唤出来的小怪在某种状态下不播死亡爆炸特效
		if (!msab.IsChangeDeadEff(respond.heroID))
		{
			// TODO.
			List<DropInfo> myDropList = new List<DropInfo>();
			foreach (var item in respond.drop_list)
			{
				var dInfo = new DropInfo();
				dInfo.player_uid = item.player_uid;
				dInfo.chest_tid = item.chest_tid;
				dInfo.ship_tlv = item.ship_tlv;
				dInfo.quality = item.ship_tlv;
				myDropList.Add(dInfo);
			}
			m_EntityManager.SendEventToEntity(respond.heroID, ComponentEventName.Dead, new DeadEvent()
			{
				Uid = respond.heroID,
				DropList = myDropList
			});
		}
		else
		{
			//m_EntityManager.SendEventToEntity(respond.heroID, ComponentEventName.SpacecraftStop, null);
			//msab.AddMonsterEffect(respond.heroID);
		}

        GameFacade.Instance.SendNotification(NotificationName.EntityDeath, new EntityDeathInfo() { heroID = respond.heroID, killerID = respond.KillerID });
    }

	/// <summary>
	/// 同步掉落物
	/// </summary>
	/// <param name="buf"></param>
	private void OnSyncMineDropItem(KProtoBuf buf)
	{
		///S2C_SYNC_PERSONAL_DROP respond = buf as S2C_SYNC_PERSONAL_DROP;
		///MineDropItemManager.Instance.CreateDropItemByRespond(respond);
	}

	/// <summary>
	/// 拾取结果
	/// </summary>
	/// <param name="buf"></param>
	private void OnNotifyDropItemResult(KProtoBuf buf)
	{
		S2C_NOTIFY_PERSONAL_DROP_RESULT respond = buf as S2C_NOTIFY_PERSONAL_DROP_RESULT;
		MineDropItemManager.Instance.NotifyDropItemResult(respond);
	}

	/// <summary>
	/// Boss 出现弹出提示
	/// </summary>
	/// <param name="buf"></param>
	private void OnSyncBossHud(KProtoBuf buf)
	{
		S2C_SYNC_BOSS_HUD respond = buf as S2C_SYNC_BOSS_HUD;
		GameFacade.Instance.SendNotification(NotificationName.MSG_MESSAGE_BOSS_SHOW, respond);
	}

	private void OnErrorCode(KProtoBuf buffer)
    {
        S2C_ERROR_CODE respond = buffer as S2C_ERROR_CODE;
        Debug.LogErrorFormat("!!!!!!!!! OnErrorCode {0}", (KErrorCode)respond.error);
    }

    private void OnSyncContactGeometry(KProtoBuf buffer)
    {
        S2C_SYNC_CONTACT_GEOMETRY respond = buffer as S2C_SYNC_CONTACT_GEOMETRY;

        GameplayProxy gameplayProxy = GameFacade.Instance.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;

        DrawGeometry.Instance.Clear();
        foreach (GeometryInfo item in respond.geometry_list)
        {
            if (item.draw_type == 0)
            {
                //Debug.LogError("Draw Cube center_pos:" + JsonUtility.ToJson(item.center_pos)
                //    + " box_size: " + JsonUtility.ToJson(item.box_size)
                //    + " rotation: " + item.rotation);

                DrawGeometry.Instance.DrawPrimitive(
                    PrimitiveType.Cube,
                    gameplayProxy.ServerAreaOffsetToClientPosition(VectorsToVector3(item.center_pos)),
                    VectorsToVector3(item.box_size),
                    QuaternionToQuaternion(item.rotation)
                );
            }
            else if (item.draw_type == 1)
            {
                //Debug.LogError("Draw Direcrion start_pos:" + JsonUtility.ToJson(item.start_pos)
                //    + "ship_rotation:" + JsonUtility.ToJson(item.ship_rotation)
                //    + " move_rotation: " + JsonUtility.ToJson(item.move_dir));

                DrawGeometry.Instance.DrawDirection(
                    gameplayProxy.ServerAreaOffsetToClientPosition(VectorsToVector3(item.start_pos)),
                    QuaternionToQuaternion(item.ship_rotation),
                    Color.red
                );

                if (VectorsToVector3(item.move_dir) != Vector3.zero)
                {
                    DrawGeometry.Instance.DrawDirection(
                      gameplayProxy.ServerAreaOffsetToClientPosition(VectorsToVector3(item.start_pos)),
                      Quaternion.LookRotation(VectorsToVector3(item.move_dir)),
                      Color.blue
                  );
                }  
            }
            else if (item.draw_type == 2)
            {
                //Debug.LogError("Draw Capsule center_pos:" + JsonUtility.ToJson(item.center_pos)
                //    + " capsule_scale: " + JsonUtility.ToJson(item.capsule_scale)
                //    + " rotation: " + JsonUtility.ToJson(item.rotation));

                DrawGeometry.Instance.DrawPrimitive(
                    PrimitiveType.Capsule,
                    gameplayProxy.ServerAreaOffsetToClientPosition(VectorsToVector3(item.center_pos)),
                    VectorsToVector3(item.capsule_scale),
                    QuaternionToQuaternion(item.rotation)
                );
            }
        }
    }

    private void FFF(KProtoBuf buf)
    {

    }

    private void OnSyncNewHero(KProtoBuf buffer)
    {
        S2C_SYNC_NEW_HERO respond = buffer as S2C_SYNC_NEW_HERO;
        if (m_EntityManager.GetEntityById<BaseEntity>(respond.id) != null)
        {
            Debug.LogErrorFormat("已经创建UId为{0}的实体", respond.id);
            return;
        }

        //Debug.LogErrorFormat("====《创建UId为{0}的实体", respond.id);

        CfgEternityProxy cfgEternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
        if (cfgEternityProxy.IsSpace())
        {
            m_EntityManager.CreateEntityByRespond<SpacecraftEntity, S2C_SYNC_NEW_HERO>(respond.id, respond.templateID, respond.item_tid, respond.ownerPlayerID, respond);
        }
        else
        {
            m_EntityManager.CreateEntityByRespond<HumanEntity, S2C_SYNC_NEW_HERO>(respond.id, respond.templateID, respond.item_tid, respond.ownerPlayerID, respond);
        }

        Debug.Log("OnSyncNewHero\n" + JsonUtility.ToJson(respond));
    }

    private void OnHeroState(KProtoBuf buffer)
    {
        //S2C_SYNC_HERO_STATE respond = buffer as S2C_SYNC_HERO_STATE;

        ////Debug.LogError("OnHeroState " + respond.move_state);

        //m_EntityManager.SendEventToEntity(respond.heroID, ComponentEventName.RespondState,
        //new RespondStateEvent()
        //{
        //    IsMonster = respond.type == 1,
        //    State = respond.state,
        //    MoveState = (KMoveState)respond.move_state
        //});
    }

    private void OnMonsterRoadPoint(KProtoBuf buffer)
    {
        S2C_MONSTER_ROAD_POINT respond = buffer as S2C_MONSTER_ROAD_POINT;

        SpacecraftEntity spacecraftEntity = m_EntityManager.GetEntityById<SpacecraftEntity>(respond.heroID);
        if (spacecraftEntity != null)
        {
            spacecraftEntity.RespondMoveEvent(respond);
        }
    }

    private void OnForceChangePos(KProtoBuf buffer)
    {
        S2C_FORCE_CHANGE_POS respond = buffer as S2C_FORCE_CHANGE_POS;

        //Debug.LogError("OnForceChangePos " + respond.position.x + " " + respond.position.y + " " + respond.position.z);
        //Debug.LogError("OnForceChangePos " + JsonUtility.ToJson(new Quaternion(respond.rotation.x, respond.rotation.y, respond.rotation.z, respond.rotation.w).eulerAngles));

        m_EntityManager.SendEventToEntity(respond.heroID, ComponentEventName.RespondForceChangePos, new RespondForceChangePos()
        {
            Forced = respond.forced != 0,
            EntiryId = respond.heroID,
            AreaUid = respond.area_id,
            Position = VectorsToVector3(respond.position),
            Rotation = QuaternionToQuaternion(respond.rotation),
            LineVelocity = VectorsToVector3(respond.velocity),
            AngularVelocity = VectorsToVector3(respond.angularVelocity),
            EngineAxis = VectorsToVector3(respond.engineAxis),
            RotateAxis = VectorsToVector3(respond.rotateAxis),
            IsResetAll = respond.reset_all == 1
        });
    }

    private void OnRemoveSceneObj(KProtoBuf buffer)
    {
        S2C_REMOVE_SCENE_OBJ respond = buffer as S2C_REMOVE_SCENE_OBJ;

        Debug.LogWarningFormat("====》删除UId为{0}的实体", respond.objID);

        if (DropItemManager.Instance.CheckDropItemPickUpSuccess(respond.objID))
		{
			//Debug.LogErrorFormat("OnRemoveSceneObj 1 {0}", respond.objID);
			DropItemManager.Instance.DestoryChestGameObject(respond.objID);
			UIManager.Instance.StartCoroutine(DelayToRemoveEntityBecauseOfDropItem(respond.objID));
		}
		else
		{
			BaseEntity baseEntity = m_EntityManager.GetEntityById<BaseEntity>(respond.objID);
			ShipItemsProxy shipItemsProxy = GameFacade.Instance.RetrieveProxy(ProxyName.ShipItemsProxy) as ShipItemsProxy;
			shipItemsProxy.RemoveShipItems(respond.objID);
			if (baseEntity == null)
            {
                Debug.LogErrorFormat("客户端无法找到UID为{0}的实体", respond.objID);
                return;
            }
			ulong uid = 0;
			if (baseEntity.GetHeroType() == KHeroType.htPlayer)
			{
				uid = baseEntity.GetPlayerId();
			}
			//NetworkManager.Instance.GetDropItemController().GetDropItemManager().AddDestroyEffect(respond.objID);
			m_EntityManager.RemoveEntity(respond.objID);
			GameFacade.Instance.SendNotification(NotificationName.MSG_TEAM_BATTLE_UPDATE, uid);//发送组队队员离开

		}
	}
    //策划需要有掉落物的拾取成功延时几秒删除
    private IEnumerator DelayToRemoveEntityBecauseOfDropItem(uint key)
    {
        yield return new WaitForSeconds(6.0f);
        //Debug.LogErrorFormat("DelayToRemoveEntityBecauseOfDropItem 1 {0}", key);
        m_EntityManager.RemoveEntity(key);

    }
    private void OnSyncSceneObjEnd(KProtoBuf buffer)
    {
        S2C_SYNC_SCENE_OBJ_END respond = buffer as S2C_SYNC_SCENE_OBJ_END;

        C2S_LOADING_COMPLETE msg = SingleInstanceCache.GetInstanceByType<C2S_LOADING_COMPLETE>();
        msg.protocolID = (byte)KC2S_Protocol.c2s_loading_complete;
        NetworkManager.Instance.SendToGameServer(msg);
	}

	private void OnHeroAttributeChanged(KProtoBuf buf)
	{
		//S2C_ATTR_CHANGE respond = buf as S2C_ATTR_CHANGE;
  //      SpacecraftEntity entity = m_GameplayProxy.GetEntityById<SpacecraftEntity>(respond.hero_id);

  //      if (entity != null)
  //      {
  //          for (int i = 0; i < respond.attr_list.Count; i++)
  //          {
  //              entity.SetAttribute((AttributeName)respond.attr_list[i].id, respond.attr_list[i].value);
  //          }
  //      }
    }

	private void OnCastSkill(KProtoBuf buffer)
	{
		S2C_CAST_SKILL respond = (S2C_CAST_SKILL)buffer;
		m_EntityManager.SendEventToEntity(respond.casterID, ComponentEventName.Event_s2c_cast_skill, new S2C_CAST_SKILL_Event()
		{
			msg = respond
		});
	}

	private void OnSingSkill(KProtoBuf buffer)
	{
		S2C_SING_SKILL respond = (S2C_SING_SKILL)buffer;
		m_EntityManager.SendEventToEntity(respond.casterID, ComponentEventName.Event_s2c_sing_skill, new S2C_SING_SKILL_Event()
		{
			msg = respond
		});
	}

	private void OnStopSkill(KProtoBuf buffer)
	{
		S2C_STOP_SKILL respond = (S2C_STOP_SKILL)buffer;
		m_EntityManager.SendEventToEntity(respond.casterID, ComponentEventName.Event_s2c_stop_skill, new S2C_STOP_SKILL_Event()
		{
			msg = respond
		});
	}

	private void OnCastSkillFail(KProtoBuf buffer)
	{
		S2C_CAST_SKILL_FAIL_NOTIFY respond = (S2C_CAST_SKILL_FAIL_NOTIFY)buffer;
		m_EntityManager.SendEventToEntity(m_GameplayProxy.GetMainPlayerUID(), ComponentEventName.Event_s2c_cast_skill_fail_notify
											, new S2C_CAST_SKILL_FAIL_NOTIFY_Event()
											{
												msg = respond
											});
	}

	private void OnSkillEffect(KProtoBuf buffer)
	{
		S2C_SKILL_EFFECT respond = (S2C_SKILL_EFFECT)buffer;
		m_EntityManager.SendEventToEntity(m_GameplayProxy.GetMainPlayerUID(), ComponentEventName.Event_s2c_skill_effect, new S2C_SKILL_EFFECT_Event()
		{
			msg = respond
		});
	}

	void OnHealEffect(KProtoBuf buf)
	{
		S2C_HEAL_EFFECT respond = buf as S2C_HEAL_EFFECT;
		m_EntityManager.SendEventToEntity(m_GameplayProxy.GetMainPlayerUID(), ComponentEventName.Event_s2c_heal_effect, new S2C_HEAL_EFFECT_Event()
		{
			msg = respond
		});
	}

	private void OnEffectImmuno(KProtoBuf buf)
	{
		S2C_EFFECT_IMMUNO respond = buf as S2C_EFFECT_IMMUNO;
		m_EntityManager.SendEventToEntity(m_GameplayProxy.GetMainPlayerUID(), ComponentEventName.Event_s2c_effect_immuno, new S2C_EFFECT_IMMUNO_Event()
		{
			msg = respond
		});
	}

	private void OnWeaponPowerChanged(KProtoBuf buffer)
	{
        S2C_WEAPON_VALUE msg = buffer as S2C_WEAPON_VALUE;
		SpacecraftEntity entity = m_GameplayProxy.GetEntityById<SpacecraftEntity>(m_GameplayProxy.GetMainPlayerUID());
		if (entity == null)
			return;

		m_PlayerSkillProxy.ChangeCurrentWeaponByServer(msg.cur_weapon_uid);

		foreach (WEAPONVALUE info in msg.infos)
		{
			ulong weaponUID = info.weapon_oid;

			WeaponPowerVO power = entity.GetWeaponPower(weaponUID);
			if (power == null)
			{
				entity.SetWeaponPower(weaponUID, new WeaponPowerVO());
				power = entity.GetWeaponPower(weaponUID);
			}

			power.WeaponUID = info.weapon_oid;
			power.CurrentValue = info.cur_value;
			power.MaxValue = info.max_value;
			power.SafeValue = info.safty_valve;

			IWeapon weapon = m_PlayerSkillProxy.GetWeaponByUID(power.WeaponUID);
			if (weapon != null && weapon.GetConfig().ClipType != (int)WeaponL1.Treasure)
			{
				if (power.CurrentValue <= 0)
					power.ForceCooldown = true;
				else if (power.CurrentValue >= power.SafeValue)
					power.ForceCooldown = false;
			}
			else
			{
				if (power.CurrentValue >= power.MaxValue)
					power.ForceCooldown = true;
				else if (power.CurrentValue <= power.SafeValue)
					power.ForceCooldown = false;
			}
		}

		GameFacade.Instance.SendNotification(NotificationName.MSG_CHARACTER_WEAPON_POWER_CHANGED);
		entity.SendEvent(ComponentEventName.WeaponPowerChanged, null);
	}

	/// <summary>
	/// 同步HP、MP
	/// </summary>
	/// <param name="buf"></param>
	private void OnHeroHpMpAnger(KProtoBuf buf)
	{
		S2C_SYNC_HERO_HPMP_ANGER msg = buf as S2C_SYNC_HERO_HPMP_ANGER;
		SpacecraftEntity entity = m_GameplayProxy.GetEntityById<SpacecraftEntity>(msg.heroID);
		if (entity != null)
		{
			entity.SetAttribute(AttributeName.kHP, msg.hp);
            entity.SetAttribute(AttributeName.kShieldValue, msg.shield_value);
			entity.SetAttribute(AttributeName.kPowerValue, msg.energyPower);
			//新屬性目前沒有
			//entity.SetAttribute(AttributeName.DefenseShield, msg.defense_shield_value);
			//entity.SetAttribute(AttributeName.ManaShield, msg.mana_shield_value);
			//entity.SetAttribute(AttributeName.SuperMagnetic, msg.superMagnetic);

			double oldPeerless = entity.GetAttribute(AttributeName.kConverterValue); /// entity.GetPeerless();
			/// entity.SetPeerless((uint)msg.current_peerless);
			entity.SetAttribute(AttributeName.kConverterValue, msg.current_peerless);

			if (entity.IsMain() && oldPeerless != entity.GetAttribute(AttributeName.kConverterMax)
                && entity.GetAttribute(AttributeName.kConverterValue) == entity.GetAttribute(AttributeName.kConverterMax))
            {
				entity.SendEvent(ComponentEventName.MaxPeerlessReached, null);
            }
		}
	}
	
	private void OnBuffAdd(KProtoBuf buf)
	{
		var msg = buf as S2C_ADD_BUFF_NOTIFY;

		SpacecraftEntity entity = m_GameplayProxy.GetEntityById<SpacecraftEntity>(msg.herID);
		if (entity != null)
		{
			AddBuffEvent buffEvent = new AddBuffEvent();
			buffEvent.buff = new BuffVO(msg.wBuffID, msg.byOverLap, Time.time, msg.nTime / 1000.0f, msg.link_id, msg.is_master !=0);
			m_EntityManager.SendEventToEntity(msg.herID, ComponentEventName.BuffAdd, buffEvent);

			MSAIBossProxy mSAI = GameFacade.Instance.RetrieveProxy(ProxyName.MSAIBossProxy) as MSAIBossProxy;
			mSAI.CheckBuffPlaySound(msg.wBuffID, true);
		}
	}

	private void OnBuffRemove(KProtoBuf buf)
	{
		var msg = buf as S2C_DEL_BUFF_NOTIFY;

		SpacecraftEntity entity = m_GameplayProxy.GetEntityById<SpacecraftEntity>(msg.herID);
		if (entity != null)
		{
			RemoveBuffEvent buffEvent = new RemoveBuffEvent();
			buffEvent.buffID = msg.wBuffID;
			m_EntityManager.SendEventToEntity(msg.herID, ComponentEventName.BuffRemove, buffEvent);
		}

		MSAIBossProxy mSAI = GameFacade.Instance.RetrieveProxy(ProxyName.MSAIBossProxy) as MSAIBossProxy;
		mSAI.CheckBuffPlaySound(msg.wBuffID, false);
	}

	private void OnEnterBattle(KProtoBuf buf)
	{
		S2C_ENTER_BATTLE msg = buf as S2C_ENTER_BATTLE;

		Debug.Log("进入战斗");
	}

	private void OnLeaveBattle(KProtoBuf buf)
	{
		S2C_LEAVE_BATTLE msg = buf as S2C_LEAVE_BATTLE;

		Debug.Log("脱离战斗");
	}

	private void OnSyncFightShipItems(KProtoBuf buf)
	{
		S2C_SYNC_FIGHTSHIP_VISIBLE_ITEM_LIST msg = buf as S2C_SYNC_FIGHTSHIP_VISIBLE_ITEM_LIST;
		Debug.Log("OnSyncFightShipItems");

		ShipItemsProxy shipItemsProxy = GameFacade.Instance.RetrieveProxy(ProxyName.ShipItemsProxy) as ShipItemsProxy;
		shipItemsProxy.InitShipItemsByByRespond(msg);
	}


	/// <summary>
	/// 同步玩家进入宝藏区域
	/// </summary>
	/// <param name="buf"></param>
	private void OnSyncDiscoverPrecious(KProtoBuf buf)
	{
		S2C_SYNC_PLAYER_DISCOVER_PRECIOUS msg = buf as S2C_SYNC_PLAYER_DISCOVER_PRECIOUS;
		TreasureHuntProxy treasureHuntProxy = GameFacade.Instance.RetrieveProxy(ProxyName.TreasureHuntProxy) as TreasureHuntProxy;
		treasureHuntProxy.SyncDiscoverPrecious(msg);
	}

	/// <summary>
	/// 收到服务器开宝箱的回复
	/// </summary>
	/// <param name="buf"></param>
	private void OnOpenCehstByKeyResult(KProtoBuf buf)
	{
		//S2C_OPEN_CEHST_BY_KEY_RESULT msg = buf as S2C_OPEN_CEHST_BY_KEY_RESULT;
		//GameFacade.Instance.SendNotification(NotificationName.MSG_S2C_OPEN_CEHST_BY_KEY_RESULT);
		//Debug.Log("收到回复" + msg.hero_uid);
	}
	/// <summary>
	/// 同步探测器距离宝藏的位置
	/// </summary>
	/// <param name="buf"></param>
	private void OnSyncDeteCtorDistance(KProtoBuf buf)
	{
		S2C_SYNC_DETECTOR_DISTANCE msg = buf as S2C_SYNC_DETECTOR_DISTANCE;
		TreasureHuntProxy treasureHuntProxy = GameFacade.Instance.RetrieveProxy(ProxyName.TreasureHuntProxy) as TreasureHuntProxy;
		treasureHuntProxy.SyncDeteCtorDistance(msg);
	}

	private Vector3 VectorsToVector3(Vectors vectors)
    {
        return new Vector3(vectors.x, vectors.y, vectors.z);
    }

    private Quaternion QuaternionToQuaternion(MsgQuaternion msgQuaternion)
    {
        return new Quaternion(msgQuaternion.x, msgQuaternion.y, msgQuaternion.z, msgQuaternion.w);
    }

	private void OnCampChange(KProtoBuf buf)
	{
		S2C_SYNC_CAMP_CHANGED msg = buf as S2C_SYNC_CAMP_CHANGED;
		SpacecraftEntity entity = m_GameplayProxy.GetEntityById<SpacecraftEntity>((uint)msg.hero_uid);
		if (entity != null)
		{
			entity.SetCampID((uint)msg.camp_id);
		}
	}

	private void OnSealChange(KProtoBuf buf)
	{
		S2C_SYNC_SEAL_CHANGED msg = buf as S2C_SYNC_SEAL_CHANGED;
		SpacecraftEntity entity = m_GameplayProxy.GetEntityById<SpacecraftEntity>((uint)msg.hero_uid);
		if (entity != null)
		{
			entity.SetSeal(msg.is_seal);
		}
	}
}
