using Assets.Scripts.Define;
using Eternity.FlatBuffer;
using System;
using UnityEngine;

public interface ISpacecraftSoundProperty
{
    KHeroType GetHeroType();
    Transform GetRootTransform();
    uint GetItemID();
    bool IsMain();
	bool IsSeal();
}

public sealed class SpacecraftSoundComponent : EntityComponent<ISpacecraftSoundProperty>
{
    private ISpacecraftSoundProperty m_SpacecraftSoundProperty;
    private int m_MusicComboID = -1;
    private WwiseMusicPalce m_WwiseMusicPalce = WwiseMusicPalce.Palce_1st;
    private CfgEternityProxy m_Cgeternityproxy;

	/// <summary>
	/// 语音助手
	/// </summary>
	private readonly static string SHELLY_NAME = "hud_text_id_1017";

    public override void OnInitialize(ISpacecraftSoundProperty property)
    {
        m_SpacecraftSoundProperty = property;

        m_Cgeternityproxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;

        if (m_SpacecraftSoundProperty.GetHeroType() == KHeroType.htMonster)
        {
            /// TODO 怪物后续走表
            m_MusicComboID = 1001;
        }
        else if (m_SpacecraftSoundProperty.GetHeroType() == KHeroType.htPlayer)
		{
            m_MusicComboID = m_Cgeternityproxy.GetItemByKey(property.GetItemID()).ItemUnion<Warship>().Value.MusicComboID;
        }

        if (!m_SpacecraftSoundProperty.IsMain())
        {
            m_WwiseMusicPalce = WwiseMusicPalce.Palce_3st;
        }

		/// TODO.
		/// 处理探查掉落音效，先在newhero中处理跑通流程，后续根据状态协议处理
		KHeroType heroType = m_SpacecraftSoundProperty.GetHeroType();
		if (heroType == KHeroType.htNormalChest)
		{
			PlayVideoSound((int)m_Cgeternityproxy.GetGamingConfig(1).Value.Treasure.Value.Sound.Value.OrdinaryChestRefresh);
		}
		else if ((heroType == KHeroType.htRareChestGuard || heroType == KHeroType.htNormalChestGuard) && m_SpacecraftSoundProperty.IsSeal())
		{
			PlaySystemSound(WwiseMusicSpecialType.SpecialType_Voice_treasure_event2, (object obj) =>
			{
				PlayVideoSound((int)m_Cgeternityproxy.GetGamingConfig(1).Value.Treasure.Value.Sound.Value.JammerRefresh);
			});
		}
    }

    public override void OnAddListener()
    {
        base.OnAddListener();

        AddListener(ComponentEventName.ShowDeadSlidingFx, OnShowDeadSlidingFx);
        AddListener(ComponentEventName.ShowDeadExplosionFx, OnShowDeadExplosionFx);
        AddListener(ComponentEventName.Relive, OnRelive);
		AddListener(ComponentEventName.SealEnd, OnSealEnd);
		AddListener(ComponentEventName.PlaySound, OnPlaySound);
		AddListener(ComponentEventName.PlayVideoSound, OnPlayVideoSound);
		AddListener(ComponentEventName.PlaySystemSound, OnPlaySystemSound);
		AddListener(ComponentEventName.PlayFragmentationSound, OnPlayFragmentationSound);
		AddListener(ComponentEventName.PlayDropSound, OnPlayDropSound);
	}

    private void OnRelive(IComponentEvent obj)
    {
        WwiseUtil.PlaySound(m_MusicComboID, WwiseMusicSpecialType.SpecialType_Rebirth, m_WwiseMusicPalce, false, m_SpacecraftSoundProperty.GetRootTransform());

        //语音
        if (m_SpacecraftSoundProperty.IsMain())
        {
            WwiseUtil.PlaySound(WwiseManager.voiceComboID, WwiseMusicSpecialType.SpecialType_Voice_Ship_Reborn, WwiseMusicPalce.Palce_1st, false, null);
        }
    }

    private void OnShowDeadExplosionFx(IComponentEvent obj)
    {
		Model model = m_Cgeternityproxy.GetItemModelByKey(m_SpacecraftSoundProperty.GetItemID());
		if (model.DieSound > 0)
		{
			PlaySound((int)model.DieSound, m_SpacecraftSoundProperty.GetRootTransform());
		}
		//else
		//{
		/// TODO 系统语音
		//	if (m_SpacecraftSoundProperty.GetHeroType() == KHeroType.htPlayer)
		//	{
		//		WwiseUtil.PlaySound(m_MusicComboID, WwiseMusicSpecialType.SpecialType_Die_End, m_WwiseMusicPalce, false, m_SpacecraftSoundProperty.GetRootTransform());
		//	}
		//	/// TODO.死亡爆炸音效先写死
		//	else if (m_SpacecraftSoundProperty.GetHeroType() == KHeroType.htMonster
		//		|| m_SpacecraftSoundProperty.GetHeroType() == KHeroType.htDisturbor
		//		|| m_SpacecraftSoundProperty.GetHeroType() == KHeroType.htNormalChestGuard
		//		|| m_SpacecraftSoundProperty.GetHeroType() == KHeroType.htRareChestGuard)
		//	{
		//		WwiseUtil.PlaySound((int)WwiseMusic.Music_Npc_Dead_End, false, m_SpacecraftSoundProperty.GetRootTransform());
		//	}
		//}

        //语音
        if (m_SpacecraftSoundProperty.IsMain())
        {
            WwiseUtil.PlaySound(WwiseManager.voiceComboID, WwiseMusicSpecialType.SpecialType_Voice_Ship_Destroyed, WwiseMusicPalce.Palce_1st, false, null);
        }
    }

    private void OnShowDeadSlidingFx(IComponentEvent obj)
    {
        if (m_SpacecraftSoundProperty.GetHeroType() == KHeroType.htPlayer)
        {
            WwiseUtil.PlaySound(m_MusicComboID, WwiseMusicSpecialType.SpecialType_Die_Begin, m_WwiseMusicPalce, false, m_SpacecraftSoundProperty.GetRootTransform());
        }
        else if (m_SpacecraftSoundProperty.GetHeroType() == KHeroType.htMonster)
        {
            WwiseUtil.PlaySound((int)WwiseMusic.Music_Npc_Dead_Begin, false, m_SpacecraftSoundProperty.GetRootTransform());
            Log("Music_Npc_Dead_Begin");
        }
    }
	
	private void OnSealEnd(IComponentEvent componentEvent)
	{
		PlayVideoSound((int)m_Cgeternityproxy.GetGamingConfig(1).Value.Treasure.Value.Sound.Value.TreasureRefresh);
	}

	private void OnPlaySound(IComponentEvent componentEvent)
	{
		PlaySound playSound = componentEvent as PlaySound;

		PlaySound(playSound.SoundID, playSound.Transform);
	}

	private void OnPlayVideoSound(IComponentEvent componentEvent)
	{
		PlayVideoSound playVideoSound = componentEvent as PlayVideoSound;

		PlayVideoSound(playVideoSound.GroupID, playVideoSound.Action, playVideoSound.NpcId);
	}

	private void OnPlaySystemSound(IComponentEvent componentEvent)
	{
		PlaySystemSound playSystemSound = componentEvent as PlaySystemSound;

		PlaySystemSound(playSystemSound.SoundID, playSystemSound.EndAction);
	}

	private void OnPlayFragmentationSound(IComponentEvent componentEvent)
	{
		Model model = m_Cgeternityproxy.GetItemModelByKey(m_SpacecraftSoundProperty.GetItemID());
		if (model.FragmentationSound > 0)
		{
			PlaySound((int)model.FragmentationSound, m_SpacecraftSoundProperty.GetRootTransform());
		}
	}

	private void OnPlayDropSound(IComponentEvent componentEvent)
	{
		Model model = m_Cgeternityproxy.GetItemModelByKey(m_SpacecraftSoundProperty.GetItemID());
		if (model.DropSound > 0)
		{
			PlaySound((int)model.DropSound, m_SpacecraftSoundProperty.GetRootTransform());
		}
	}

	private void PlaySound(int sound, Transform trans = null)
	{
		WwiseUtil.PlaySound(sound, false, trans);
	}

	private void PlayVideoSound(int groupId, Action action = null, uint npcId = 0)
	{
		PlayParameter playParameter = new PlayParameter();
		playParameter.groupId = groupId;
		playParameter.action = action;
		playParameter.npcId = npcId;
		GameFacade.Instance.SendNotification(NotificationName.VideoPhoneChange, playParameter);
	}

	private void PlaySystemSound(WwiseMusicSpecialType sound, Action<object> playEndAction = null)
	{
		WwiseUtil.PlaySound(WwiseManager.voiceComboID, sound, WwiseMusicPalce.Palce_1st, false, null, playEndAction);
	}
}
