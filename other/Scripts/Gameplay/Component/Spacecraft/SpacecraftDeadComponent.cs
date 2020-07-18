using Assets.Scripts.Define;
using Eternity.FlatBuffer;
using Utils.Timer;
using System;
using UnityEngine;
using Assets.Scripts.Proto;
using System.Collections.Generic;

public interface ISpacecraftDeadProperty
{
    bool IsMain();
    uint UId();
    uint GetItemID();
	Npc GetNPCTemplateVO();
	uint GetUId();
    KHeroType GetHeroType();
	bool IsNotHaveAva();
	Rigidbody GetRigidbody();
	SpacecraftPresentation GetPresentation();
	SpacecraftEntity GetOwner();
	void AddOnCollisionCallback(Action<Collision> enter, Action<Collision> stay, Action<Collision> exit);
    void RemoveCollisionCallback(Action<Collision> enter, Action<Collision> stay, Action<Collision> exit);
	void SetTarget(SpacecraftEntity target, Collider targetCollider);
    HeroState GetCurrentState();
    bool IsDead();
}

public sealed class SpacecraftDeadComponent : EntityComponent<ISpacecraftDeadProperty>
{
    /// <summary>
    /// 死亡特效状态
    /// </summary>
    private enum DeadState
    {
        None,
        /// <summary>
        /// 滑行
        /// </summary>
        Sliding,
        /// <summary>
        /// 停止
        /// </summary>
        Stay
    }

    private ISpacecraftDeadProperty m_SpacecraftDeadProperty;

    /// <summary>
    /// 死亡特效TimerId
    /// </summary>
    //private uint m_ShowDeadFxTimerId;

    /// <summary>
    /// 死亡状态
    /// </summary>
    //private DeadState m_DeadFxState = DeadState.None;

    /// <summary>
    /// 死亡服务器数据
    /// </summary>
    private List<DropInfo> m_DropInfoList;

	// Cache
	GameplayProxy m_GameplayProxy;

	public override void OnInitialize(ISpacecraftDeadProperty property)
    {
        m_SpacecraftDeadProperty = property;
        m_GameplayProxy = GameFacade.Instance.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;
    }

    public override void OnAddListener()
    {
        AddListener(ComponentEventName.Dead, OnDead);
        //AddListener(ComponentEventName.Relive, OnRelive);
		AddListener(ComponentEventName.CheckDeadDrop, OnCheckDeadDrop);

		//m_SpacecraftDeadProperty.AddOnCollisionCallback(OnCollisionEnter, OnCollisionStay, OnCollisionExit);
	}

    public override void OnAfterInitialize()
    {
        //if (m_SpacecraftDeadProperty.IsMain())
        //{
        //    CfgEternityProxy eternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
        //    if (m_GameplayProxy.GetDeadMapID() != 0 && m_GameplayProxy.GetDeadMapID() != eternityProxy.GetCurrentMapData().GamingmapId)
        //    {
        //        SendEvent(ComponentEventName.Relive, null);
        //    }

        //    m_GameplayProxy.SetDeadMapID(0);
        //}
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        //m_SpacecraftDeadProperty.RemoveCollisionCallback(OnCollisionEnter, OnCollisionStay, OnCollisionExit);

        //RealTimerUtil.Instance.Unregister(m_ShowDeadFxTimerId);
    }

    public override void OnFixedUpdate()
    {
        base.OnFixedUpdate();

        //if (m_DeadFxState == DeadState.Sliding)
        //{
        //    if (m_SpacecraftDeadProperty.GetRigidbody() == null
        //     || m_SpacecraftDeadProperty.GetRigidbody().velocity.magnitude <= 0.1f)
        //    {
        //        OnSpacecraftSlidingEnd();
        //    }
        //}
    }

    //private void OnCollisionEnter(Collision obj)
    //{
    //    OnSpacecraftSlidingEnd();
    //}

    //private void OnCollisionStay(Collision obj)
    //{
    //}

    //private void OnCollisionExit(Collision obj)
    //{
    //}

    //private void OnRelive(IComponentEvent obj)
    //{
    //    m_DeadFxState = DeadState.None;

    //    RealTimerUtil.Instance.Unregister(m_ShowDeadFxTimerId);
    //}

    private void OnDead(IComponentEvent obj)
    {
        DeadEvent deadEvent = obj as DeadEvent;
        if (!m_SpacecraftDeadProperty.IsDead())
        {
            return;
        }

        m_DropInfoList = deadEvent.DropList;

		/// TODO.宝藏特殊处理
		/// 服务器创建后立马死亡
		//if (m_SpacecraftDeadProperty.IsNotHaveAva())
		//{
		//	if (m_DropInfoList != null && m_DropInfoList.Count > 0)
		//	{
		//		DropItemManager.Instance.SetDropItemInfoByDeath(m_SpacecraftDeadProperty.UId(), m_DropInfoList);
		//	}
		//	return;
		//}

		/// npc死亡
		if (m_SpacecraftDeadProperty.GetHeroType() != KHeroType.htPlayer)
		{
			Npc npcVO = m_SpacecraftDeadProperty.GetNPCTemplateVO();
			if (npcVO.SoundDead > 0)
			{
				SendEvent(ComponentEventName.PlaySound, new PlaySound()
				{
					SoundID = (int)npcVO.SoundDead
				});
			}
			/// 导演死检查音乐盒子
			if (m_SpacecraftDeadProperty.GetHeroType() == KHeroType.htPlotMonster)
			{
				MSAIBossProxy msab = GameFacade.Instance.RetrieveProxy(ProxyName.MSAIBossProxy) as MSAIBossProxy;
				msab.DestroySoundBox(m_SpacecraftDeadProperty.UId());
			}
			/// TODO.干扰器死亡
			else if (m_SpacecraftDeadProperty.GetHeroType() == KHeroType.htDisturbor)
			{
				/// 自己的停掉
				SpacecraftEntity ownerEntity = m_SpacecraftDeadProperty.GetOwner();
				ownerEntity.SendEvent(ComponentEventName.PlaySound, new PlaySound()
				{
					SoundID = (int)WwiseMusic.Treasure_Disturbor_Sound5,
					Transform = ownerEntity.GetSkinTransform()
				});

				TreasureHuntProxy treasure = GameFacade.Instance.RetrieveProxy(ProxyName.TreasureHuntProxy) as TreasureHuntProxy;
				treasure.DisturborSoundEffect(m_SpacecraftDeadProperty.GetOwner().m_EntityFatherOwnerID);
			}
		}

		m_SpacecraftDeadProperty.SetTarget(null, null);

        #region 待服务器在new_hero协议中添加复活状态逻辑后删除
        if (m_SpacecraftDeadProperty.IsMain())
        {
            m_GameplayProxy = GameFacade.Instance.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;

            if (m_SpacecraftDeadProperty.IsMain())
            {
                CfgEternityProxy eternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
                m_GameplayProxy.SetDeadMapID(eternityProxy.GetCurrentMapData().GamingmapId);
            }
        }
        #endregion

        ShowRelivePanel(deadEvent);

        SpacecraftPresentation presentation = m_SpacecraftDeadProperty.GetPresentation();
		if (presentation != null)
        {
		    if (presentation.SpacecraftDeadType != SpacecraftPresentation.SpacecraftType.Warship)
		    {
			    OnDeviceDead(deadEvent);
		    }
        }
	}

	private void OnCheckDeadDrop(IComponentEvent obj)
	{
		//if (m_DropInfoList != null && m_DropInfoList.Count > 0)
		//{
		//	DropItemManager.Instance.SetDropItemInfoByDeath(m_SpacecraftDeadProperty.UId(), m_DropInfoList);
		//}
	}

	private void OnDeviceDead(DeadEvent deadEvent)
	{
        if (!m_SpacecraftDeadProperty.IsDead())
        {
            return;
        }

        ///SendEvent(ComponentEventName.ShowDeviceDeadFX, null);
        SendEvent(ComponentEventName.PlayDeviceDeadAnimation, null);
	}

	private void ShowRelivePanel(DeadEvent deadEvent)
    {
        //if (m_SpacecraftDeadProperty.IsMain())
        //{
        //    string killerName = "error_name";
        //    if (deadEvent.KillerNpctemplateID > 0)
        //    {
        //        killerName = TableUtil.GetNpcName(deadEvent.KillerNpctemplateID);
        //    }
        //    else
        //    {
        //        killerName = deadEvent.KillerPlayerName;
        //    }

        //    bool isShowHallRelive = false;
        //    foreach (var item in deadEvent.ReliveOptions)
        //    {
        //        if (item == (short)PlayerReliveType.relive_hall)
        //        {
        //            isShowHallRelive = true;
        //            break;
        //        }
        //    }

        //    GameFacade.Instance.SendNotification(NotificationName.MainHeroDeath, new ShowRelviePanelNotify()
        //    {
        //        IsShowHallRelive = isShowHallRelive,
        //        Countdown = deadEvent.CD,
        //        KillerName = killerName,
        //    });
        //}

		SpacecraftEntity mainPlayer = m_GameplayProxy.GetMainPlayer();
		if (mainPlayer.GetTarget() == m_GameplayProxy.GetEntityById<SpacecraftEntity>(m_SpacecraftDeadProperty.GetUId()))
		{
			mainPlayer.SetTarget(null, null);
		}
	}

    //private void OnSpacecraftSlidingEnd()
    //{
    //    if (m_DeadFxState != DeadState.Sliding)
    //    {
    //        return;
    //    }

    //    SendEvent(ComponentEventName.SpacecraftStop, null);

    //    /// 死亡停留特效
    //    ShowDeadExplosionFx(true);
    //}
}

