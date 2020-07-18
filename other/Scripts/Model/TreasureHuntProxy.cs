using PureMVC.Patterns.Proxy;
using Assets.Scripts.Proto;
using Assets.Scripts.Define;
using Leyoutech.Core.Effect;
using UnityEngine;
using System.Collections.Generic;

public class TreasureHuntProxy : Proxy
{
	private GameplayProxy m_GameplayProxy;

	private CfgEternityProxy m_CfgEternityProxy;

	private EffectController m_Effect;

	/// <summary>
	/// 当前正在播放音效的探测器
	/// </summary>
	private uint m_CurUid = 0;

	private Transform m_Transform = null;


	public TreasureHuntProxy() : base(ProxyName.TreasureHuntProxy)
	{
		m_GameplayProxy = Facade.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;
		m_CfgEternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
	}

	public void SyncDiscoverPrecious(S2C_SYNC_PLAYER_DISCOVER_PRECIOUS msg)
	{
		int type = msg.discover_type;
		/// 发现信号 0是飞出
		if (type == (int)PlayerDiscoverPreciousType.DiscoverSignal)
		{
			if (msg.is_in != 0)
			{
				SpacecraftEntity main = m_GameplayProxy.GetEntityById<SpacecraftEntity>(m_GameplayProxy.GetMainPlayerUID());
				m_Effect = EffectManager.GetInstance().CreateEffect(m_CfgEternityProxy.GetGamingConfig(1).Value.Treasure.Value.Effect.Value.DiscoverCameraEffect, EffectManager.GetEffectGroupNameInSpace(true));
				m_Effect.transform.SetParent(main.GetSkinRootTransform(), false);
				m_Effect.SetCreateForMainPlayer(true);

				WwiseUtil.PlaySound((int)WwiseMusic.InTreasure_FX_Sound, false, null);
				WwiseUtil.PlaySound(WwiseManager.voiceComboID, WwiseMusicSpecialType.SpecialType_Voice_treasure_event1, WwiseMusicPalce.Palce_1st, false, null);
                MsgDetectorShow msgDetectorShow = new MsgDetectorShow();
                msgDetectorShow.Show = true;
                GameplayProxy gameplayProxy = Facade.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;
                Vector3 pos = gameplayProxy. ServerAreaOffsetToClientPosition(new Vector3 (0, (float)msg.fY, 0));
                msgDetectorShow.Height = pos.y;//todo 宝藏的高度
                msgDetectorShow.MaxHeight = (float)msg.fR * 2f;//触发范围最大高度
                Facade.SendNotification(NotificationName.MSG_DETECTOR_SHOW, msgDetectorShow);
				Facade.SendNotification(NotificationName.MSG_INTERACTIVE_SHOWTIP, HudNpcInteractiveFlagPanel.InteractiveTipType.Precious);
			}
			else
			{
                MsgDetectorShow msgDetectorShow = new MsgDetectorShow();
                msgDetectorShow.Show = false;
                Facade.SendNotification(NotificationName.MSG_DETECTOR_SHOW, msgDetectorShow);
                Facade.SendNotification(NotificationName.MSG_INTERACTIVE_HIDETIP, HudNpcInteractiveFlagPanel.InteractiveTipType.Precious);
			}
		}
        /// 发现宝藏 0是飞出
         else if (msg.is_in != 0 && type == (int)PlayerDiscoverPreciousType.DiscoverPrecious)
         {
            MsgDetectorShow msgDetectorShow = new MsgDetectorShow();
            msgDetectorShow.Show = false;
            // WwiseUtil.PlaySound(WwiseManager.voiceComboID, WwiseMusicSpecialType.SpecialType_Voice_treasure_event2, WwiseMusicPalce.Palce_1st, false, null);
        }
    }

    public void SyncDeteCtorDistance(S2C_SYNC_DETECTOR_DISTANCE msg)
	{
		///if (msg.is_active == 0)
		///{
		///	return;
		///}

		SpacecraftEntity entity = m_GameplayProxy.GetEntityById<SpacecraftEntity>((uint)msg.hero_uid);
		if (entity)
		{
			ChangeMaterialComponent motionComponent = entity.GetEntityComponent<ChangeMaterialComponent>();
			if (motionComponent == null)
			{
				return;
			}

			motionComponent.ChangeEffect(msg.distance, msg.treasure_signal_tid, msg.is_active != 0);
		}
	}

	/// <summary>
	/// 记录当前正在播放的探测器信息
	/// </summary>
	/// <param name="uid"></param>
	/// <param name="transform"></param>
	public void SetCurDetectorSoundInfo(uint uid, Transform transform)
	{
		m_CurUid = uid;
		m_Transform = transform;
	}

	public uint GetCurDetectorUID()
	{
		return m_CurUid;
	}

	public Transform GetCurDetectorTransfrom()
	{
		return m_Transform;
	}

	/// <summary>
	/// 干扰器与封印守卫的音效
	/// 和干扰器数量相关
	/// </summary>
	public void DisturborSoundEffect(uint ownerHeroID)
	{
		if (ownerHeroID == 0)
		{
			return;
		}

		SpacecraftEntity ownerEntity = m_GameplayProxy.GetEntityById<SpacecraftEntity>(ownerHeroID);
		/// WwiseUtil.PlaySound((int)WwiseMusic.Treasure_Disturbor_Sound5, false, ownerEntity.GetSkinTransform());
		ownerEntity.SendEvent(ComponentEventName.PlaySound, new PlaySound()
		{
			SoundID = (int)WwiseMusic.Treasure_Disturbor_Sound5,
			Transform = ownerEntity.GetSkinTransform()
		});

		List<SpacecraftEntity> entites = m_GameplayProxy.GetEntities<SpacecraftEntity>();
		List<SpacecraftEntity> entitesCache = new List<SpacecraftEntity>();
		int count = 0;
		for (int i = 0; i < entites.Count; i++)
		{
			SpacecraftEntity entity = entites[i];
			if (!entity.IsDead() && entity.GetHeroType() == KHeroType.htDisturbor && entity.m_EntityFatherOwnerID == ownerHeroID)
			{
				count++;
				entitesCache.Add(entity);
			}
		}

		Debug.Log("KHeroType.htDisturbor count" + count);
		/// TODO.
		int soundId = 0;
		if (count == 6)
		{
			soundId = (int)WwiseMusic.Treasure_Disturbor_Sound1;
		}
		else if (count == 5 || count == 4)
		{
			soundId = (int)WwiseMusic.Treasure_Disturbor_Sound2;
		}
		else if (count == 3 || count == 2)
		{
			soundId = (int)WwiseMusic.Treasure_Disturbor_Sound3;
		}
		else if (count == 1)
		{
			soundId = (int)WwiseMusic.Treasure_Disturbor_Sound4;
		}

		if (soundId != 0)
		{
			entitesCache.Add(ownerEntity);
			for (int i = 0; i < entitesCache.Count; i++)
			{
				/// WwiseUtil.PlaySound(soundId, false, entitesCache[i].GetSkinTransform());
				entitesCache[i].SendEvent(ComponentEventName.PlaySound, new PlaySound()
				{
					SoundID = soundId,
					Transform = entitesCache[i].GetSkinTransform()
				});
			}
		}
	}
}