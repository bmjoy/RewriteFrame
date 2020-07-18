using behaviac;
using Assets.Scripts.Define;
using Crucis.Protocol;
using Eternity.FlatBuffer;
using Leyoutech.Core.Effect;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Debug = UnityEngine.Debug;

public interface ISpacecraftBehaviorProperty
{
    uint GetUId();
    uint GetItemID();
	Npc GetNPCTemplateVO();
	HeroState GetCurrentState();
    HeroState GetPreviousState();
    Transform GetRootTransform();
    int GetBehaviorId();
    void AddOnCollisionCallback(Action<Collision> enter, Action<Collision> stay, Action<Collision> exit);
    void RemoveCollisionCallback(Action<Collision> enter, Action<Collision> stay, Action<Collision> exit);
    void SetIsBlock(bool block);
    bool GetIsBlock();
    Rigidbody GetRigidbody();
    bool IsMain();
	void SetSkinVisiable(bool isShow);
    EnumMotionMode GetMotionMode();
    void SetMotionType(MotionType motionType);
    void SetSkinVisiableForce(bool isShow);
    void SetIsForceRefreshMotionMode(bool isChangeMotionType);
    BehaviorController GetBehaviorController();
    void SetSynceColliderEnable(bool isEnable);
	SpacecraftEntity GetOwner();
}

public class SpacecraftBehaviorComponent : EntityComponent<ISpacecraftBehaviorProperty>, IStateModel
{
    private enum ExDataType
    {
        DeadUIData,
    }

    private ISpacecraftBehaviorProperty m_Property;
    private AgentState m_Agent;
    private readonly HashSet<GameObject> m_CollisionGameObjectSet = new HashSet<GameObject>();
    private readonly Dictionary<uint, EffectController> m_EffectTable = new Dictionary<uint, EffectController>();
    private readonly Dictionary<ExDataType, object> m_ExDataTable = new Dictionary<ExDataType, object>();
	private Dictionary<string, int> m_Index = new Dictionary<string, int>();
	private CfgEternityProxy m_CfgEternityProxy;
    private bool m_IsChangeMainState;
	private ulong m_ServerPreviousState;

	public override void OnInitialize(ISpacecraftBehaviorProperty property)
    {
        m_Property = property;

        m_CfgEternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;

        m_Agent = new AgentState(this);
#if !BEHAVIAC_RELEASE
        m_Agent.CustomName = m_Property.GetRootTransform().name;
#endif

        BehaviorManager.Instance.SwitchTree(m_Agent, GetTreeRelativePath());
        BehaviorManager.Instance.AddAgent(m_Agent);
    }

    public override void OnAddListener()
    {
        m_Property.AddOnCollisionCallback(OnCollisionEnter, OnCollisionStay, OnCollisionExit);

        AddListener(ComponentEventName.G2C_ChangeState, OnG2CChangeState);
    }

    private void OnG2CChangeState(IComponentEvent obj)
    {
        G2C_ChangeState g2C_ChangeState = obj as G2C_ChangeState;

        BehaviorManager.Instance.LogFormat(m_Agent, $"OnG2CChangeState Clinet_PreviousState:{m_Property.GetPreviousState().GetState()} Clinet_PreviousMainState:{m_Property.GetPreviousState().GetMainState()} Client_PreviousSubState:{m_Property.GetPreviousState().GetState()}");
        BehaviorManager.Instance.LogFormat(m_Agent, $"OnG2CChangeState Clinet_CurrentState:{m_Property.GetCurrentState().GetState()} Clinet_CurrentMainState:{m_Property.GetCurrentState().GetMainState()} Client_PreviousSubState:{m_Property.GetCurrentState().GetState()}");

        ulong currentState = m_Property.GetCurrentState().GetState();
        EnumMainState oldMainState = m_Property.GetCurrentState().GetMainState();
        EnumMainState newMainState = HeroState.GetMainState(g2C_ChangeState.CurrentState);

		/// TODO.
		/// 没有人形态行为树，临时过滤
		if ((int)newMainState == 11)
		{
			BehaviorManager.Instance.LogFormat(m_Agent, $"Not have human tree, pass behavior logic.");
			return;
		}
		m_Property.GetCurrentState().SetState(g2C_ChangeState.CurrentState);
		m_ServerPreviousState = g2C_ChangeState.PreviousState;

        m_IsChangeMainState = oldMainState != newMainState;

        BehaviorManager.Instance.LogFormat(m_Agent, $"OnG2CChangeState Server_PreviousState:{g2C_ChangeState.PreviousState} Server_PreviousMainState:{HeroState.GetMainState(g2C_ChangeState.PreviousState)} Server_PreviousSubState:{HeroState.GetString(g2C_ChangeState.PreviousState)}");
        BehaviorManager.Instance.LogFormat(m_Agent, $"OnG2CChangeState Server_CurrentState:{g2C_ChangeState.CurrentState} Server_CurrentMainState:{HeroState.GetMainState(g2C_ChangeState.CurrentState)} Server_CurrentSubState:{HeroState.GetString(g2C_ChangeState.CurrentState)}");

        if (m_IsChangeMainState)
        {
            BehaviorManager.Instance.AddSwitchCommandTable(m_Agent, GetTreeRelativePath());
        }

        if (g2C_ChangeState.ExData != null)
        {
            if (g2C_ChangeState.ExData.DieExMessage != null && !m_ExDataTable.ContainsKey(ExDataType.DeadUIData))
            {
                m_ExDataTable.Add(ExDataType.DeadUIData, g2C_ChangeState.ExData.DieExMessage);
            }
        }

		SendEvent(ComponentEventName.SpacecraftChangeState, new SpacecraftChangeState()
		{
			OldMainState = oldMainState,
			NewMainState = newMainState
		});

        behaviac.Workspace.Instance.Update();
    }

#if BEHAVIAC_TEST
	public void ChangeMainStateTest(EnumMainState state)
	{
		EnumMainState currentMainState = m_Property.GetCurrentState().GetMainState();

		m_Property.GetCurrentState().SetMainState(state);
		///m_Property.GetPreviousState().SetMainState(currentMainState);
		ulong mstate = (ulong)currentMainState;
		m_ServerPreviousState = mstate << 60;
		m_IsChangeMainState = m_Property.GetCurrentState().GetMainState() != currentMainState;

		BehaviorManager.Instance.LogFormat(m_Agent, $"OnG2CChangeState PreviousState:{m_Property.GetPreviousState().GetMainState()} currentMainState:{currentMainState} CurrentState:{m_Property.GetCurrentState().GetMainState()}\n");
	}
#endif

	public override void OnDestroy()
    {
        m_Agent.OnDestroy();
        m_Agent = null;

        m_CollisionGameObjectSet.Clear();
        m_Property.RemoveCollisionCallback(OnCollisionEnter, OnCollisionStay, OnCollisionExit);

        BehaviorManager.Instance.RemoveAgent(m_Agent);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision == null)
        {
            return;
        }

        m_CollisionGameObjectSet.Add(collision.gameObject);

        RefreshBlock();
    }

    private void OnCollisionStay(Collision collision)
    {
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision == null)
        {
            return;
        }

        if (m_CollisionGameObjectSet.Contains(collision.gameObject))
        {
            m_CollisionGameObjectSet.Remove(collision.gameObject);
        }

        RefreshBlock();
    }

    private void RefreshBlock()
    {
        m_Property.SetIsBlock(m_CollisionGameObjectSet.Count > 0);

        //Debug.LogError(string.Format("RefreshBlock IsBlcok:{0}", m_Property.GetIsBlock()));
    }

    private string GetTreeRelativePath()
    {
        return m_CfgEternityProxy.GetBehaviorTreeRelativePath(m_Property.GetBehaviorId(), m_Property.GetCurrentState().GetMainState());
    }

    private void ClearFx()
    {
        foreach (var item in m_EffectTable)
        {
            item.Value.StopFX();
        }
        m_EffectTable.Clear();
    }

    private uint GetEffectId(EnumEffectType effectType)
    {
        Model model = m_CfgEternityProxy.GetItemModelByKey(m_Property.GetItemID());
        uint id = (uint)m_CfgEternityProxy.GetEffectIdByEffectType(model.Id, effectType);

        BehaviorManager.Instance.LogFormat(m_Agent, $"GetEffectId effectType:{effectType} EffectId:{id}");

        return id;
    }

	private int GetGroupId(EnumSoundType type)
	{
		Npc npc = m_Property.GetNPCTemplateVO();
		NpcCombat nc = m_CfgEternityProxy.GetNpcCombatByKey(npc.Id);
		return nc.DeadVideoPhone;
	}

	private int GetModelSoundId(EnumSoundType type)
	{
		Model model = m_CfgEternityProxy.GetItemModelByKey(m_Property.GetItemID());
        uint soundId = 0;
        switch (type)
        {
            case EnumSoundType.DeathExplosion:
                soundId = model.DieSound;
                break;
            case EnumSoundType.DeadSlide:
                soundId = model.GlideSound;
                break;
            default:
                BehaviorManager.Instance.LogFormat(m_Agent, $"GetModelSoundId type:{type} not have this sound in model.csv");
                break;
        }
        BehaviorManager.Instance.LogFormat(m_Agent, $"GetModelSoundId type:{type} soundId:{soundId}");
        return (int)soundId;
    }

	private int GetModelComboID()
	{
		Model model = m_CfgEternityProxy.GetItemModelByKey(m_Property.GetItemID());
		BehaviorManager.Instance.LogFormat(m_Agent, $"GetModelComboID MusicComboId:{model.MusicComboId}");
		return model.MusicComboId;
	}

    public void PlayStateAnima(string name)
    {
        SendEvent(ComponentEventName.PlayStateAnima, new PlayStateAnima()
        {
            Name = name
        });
    }

    private void SendChangeState()
    {
        SendEvent(ComponentEventName.ChangHeroState, null);

        HeroState currentHeroStata = m_Property.GetCurrentState();
        HeroState previousHeroState = m_Property.GetPreviousState();
        //Debug.LogErrorFormat($"currentHeroStata={currentHeroStata.GetMainState()} OldMainState={previousHeroState.GetMainState()}");
        GameFacade.Instance.SendNotification(NotificationName.MSG_CHANGE_BATTLE_STATE, new ChangeBattleStateNotify()
        {
            IsSelf = m_Property.IsMain(),
            NewState = currentHeroStata,
            OldStata = previousHeroState
        });
    }

#region implementing the interface
	public void OnEnterMainState(EnumMainState newStateType)
    {
        m_Property.SetIsForceRefreshMotionMode(true);

        m_Property.GetPreviousState().SetState(m_ServerPreviousState);
		m_IsChangeMainState = false;

        SendChangeState();

        BehaviorManager.Instance.LogFormat(m_Agent, string.Format($"OnEnterMainState {newStateType}"));
        //Debug.LogErrorFormat($"OnEnterMainState {newStateType}");
    }

    public void OnExitMainState()
    {
		m_Index.Clear();
		BehaviorManager.Instance.LogFormat(m_Agent, string.Format($"OnExitMainState ClientPreviousMainState:{m_Property.GetPreviousState().GetMainState()} ServerPreviousMainState:{HeroState.GetMainState(m_ServerPreviousState)}"));
    }

    public void OnEnterSubState(EnumSubState subState)
    {
        m_Property.GetPreviousState().SetState(m_ServerPreviousState);

        m_Property.SetIsForceRefreshMotionMode(true);

        if (m_Property.GetMotionMode() == EnumMotionMode.Dof6ReplaceOverload)
        {
            if (subState == EnumSubState.Overload)
            {
                SendEvent(ComponentEventName.ResetRotation, new ResetRotation()
                {
                    Type = MotionType.Dof4
                });

				m_Property.SetMotionType(MotionType.Dof6);
				SendEvent(ComponentEventName.ChangeMotionType, null);

                GameFacade.Instance.SendNotification(NotificationName.Enter6DofMode);
            }
        }

        bool isMain = m_Property.IsMain();
        switch (subState)
        {
            case EnumSubState.Online:
                break;
            case EnumSubState.Relive:
                break;
            case EnumSubState.Peerless:
                if (isMain)
                {
                    GameFacade.Instance.SendNotification(NotificationName.PlayerWeaponToggleEnd);
                }
                break;
            case EnumSubState.Overload:
                break;
            case EnumSubState.DeadLeap:
                break;
            case EnumSubState.DeadExplosion:
                break;
            case EnumSubState.DeadSlide:
                break;
            case EnumSubState.DeadCorpse:
                break;
            case EnumSubState.BackToanchor:
                break;
            case EnumSubState.LeapPrepare:
                break;
            case EnumSubState.Leaping:
                if (isMain)
                {
                    MineDropItemManager.Instance.DestoryAllDropGameObject();
                }
                /// 关闭碰撞
                m_Property.SetSynceColliderEnable(false);
                break;
            case EnumSubState.LeapCancel:
                break;
            case EnumSubState.LeapArrive:
                if (isMain)
                {
                    UIManager.Instance.OpenPanel(UIPanel.HudAreaNamePanel);
                    GameFacade.Instance.SendNotification(NotificationName.ChangeArea);
                    MineDropItemManager.Instance.CheckSyncDropItem();
                }
                /// 开启碰撞
                m_Property.SetSynceColliderEnable(true);
                /// 连续跃迁
                (GameFacade.Instance.RetrieveProxy(ProxyName.AutoNavigationProxy) as AutoNavigationProxy)?.CheckNextAutoLeap();
                break;
            default:
                break;
        }

        SendChangeState();

		BehaviorManager.Instance.LogFormat(m_Agent, string.Format($"OnEnterSubState subState:{subState}"));
    }

	public void RemoveSubState(EnumSubState subState)
	{
		m_Property.GetCurrentState().RemoveSubState(subState);
	}

	public void OnExitSubState(EnumSubState subState)
    {
        if (m_Property.GetMotionMode() == EnumMotionMode.Dof6ReplaceOverload)
        {
            if (subState == EnumSubState.Overload)
            {
                SendEvent(ComponentEventName.ResetRotation, new ResetRotation()
                {
                    Type = MotionType.Dof6
                });

				m_Property.SetMotionType(MotionType.Dof4);
				SendEvent(ComponentEventName.ChangeMotionType, null);

                GameFacade.Instance.SendNotification(NotificationName.Exit6DofMode);
            }
        }

        SendChangeState();

		BehaviorManager.Instance.LogFormat(m_Agent, string.Format($"OnExitSubState subState:{subState}"));
    }

	public void CloseAllPanel(bool isMain)
    {
        if (isMain && !m_Property.IsMain())
        {
            return;
        }
        InputManager.Instance.SceneInputMap = HotKeyMapID.None;
        UIManager.Instance.CloseAllWindow();
        UIManager.Instance.CloseAllDialog();
    }

    public bool IsBlock()
    {
        return m_Property.GetIsBlock();
    }

    public bool IsCanMove()
    {
        Rigidbody rigidbody = m_Property.GetRigidbody();
        if (rigidbody == null)
        {
            return false;
        }

        return rigidbody.velocity.magnitude > 0.01f || rigidbody.angularVelocity.magnitude > 0.01f;
    }

    public void OpenUIPanel(UIPanel panelType, bool isMain)
    {
        if (isMain && !m_Property.IsMain())
        {
            return;
        }
        CfgEternityProxy cfgEternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
        //设置输入模式
        InputManager.Instance.SceneInputMap = cfgEternityProxy.IsSpace() ? HotKeyMapID.SHIP : HotKeyMapID.HUMAN;
        object msg = null;
        if (panelType == UIPanel.RevivePanel)
        {
            if (m_ExDataTable.TryGetValue(ExDataType.DeadUIData, out var obj))
            {
                DieExMessage dieExMessage = (DieExMessage)obj;

                string killerName = "error_name";
                if (dieExMessage.KillerId > 0)
                {
                    killerName = TableUtil.GetNpcName((uint)dieExMessage.KillerId);
                }
                else
                {
                    killerName = dieExMessage.KillerName;
                }

                bool isShowHallRelive = false;
                foreach (var item in dieExMessage.ReliveTypes)
                {
                    if (item == (short)PlayerReliveType.relive_hall)
                    {
                        isShowHallRelive = true;
                        break;
                    }
                }

                m_ExDataTable.Remove(ExDataType.DeadUIData);

                msg = new ShowRelviePanelNotify()
                {
                    IsShowHallRelive = isShowHallRelive,
                    Countdown = dieExMessage.Cd,
                    KillerName = killerName
                };
            }
        }
        //Debug.LogError("msg:" + JsonUtility.ToJson(msg));
        UIManager.Instance.OpenPanel(panelType, msg);
        //BehaviorManager.Instance.LogFormat(m_Agent, string.Format($"OpenUIPanel panelType:{panelType}"));
    }

    public void PlayEffect(EnumEffectType effectType)
    {
		uint effectId = GetEffectId(effectType);
        if (effectId == 0)
        {
            return;
        }

		string path = m_CfgEternityProxy.GetEffectPath(effectId);
		EffectController effectController = EffectManager.GetInstance().CreateEffect(path, EffectManager.GetEffectGroupNameInSpace(m_Property.IsMain()));
		effectController.transform.SetParent(m_Property.GetRootTransform(), false);
		effectController.SetCreateForMainPlayer(m_Property.IsMain());

		effectController.PlayFX();
		if (!m_EffectTable.ContainsKey(effectId))
		{
			m_EffectTable.Add(effectId, effectController);
		}
		BehaviorManager.Instance.LogFormat(m_Agent, string.Format($"PlayEffect effectType:{effectType} effectId:{effectId} path:{path}"));
    }

	public void PlayWorldEffect(EnumEffectType effectType)
	{
		uint effectId = GetEffectId(effectType);
		if (effectId == 0)
		{
			return;
		}

		Transform parent = m_Property.GetOwner().GetSkinRootTransform();
		Vector3 position = parent.position;
		Quaternion rotation = parent.rotation;
		parent = null;

		string path = m_CfgEternityProxy.GetEffectPath(effectId);
		EffectController effectController = EffectManager.GetInstance().CreateEffect(path, EffectManager.GetEffectGroupNameInSpace(m_Property.IsMain()));
		effectController.transform.SetParent(null, false);
		effectController.transform.localPosition = position;
		effectController.transform.localRotation = rotation;
		effectController.SetCreateForMainPlayer(m_Property.IsMain());

		effectController.PlayFX();
		if (!m_EffectTable.ContainsKey(effectId))
		{
			m_EffectTable.Add(effectId, effectController);
		}
		BehaviorManager.Instance.LogFormat(m_Agent, string.Format($"PlayEffect effectType:{effectType} effectId:{effectId} path:{path}"));
	}

	public void StopEffect(EnumEffectType effectType)
    {
        uint effectId = GetEffectId(effectType);
        if (effectId == 0)
        {
            return;
        }

        string path = m_CfgEternityProxy.GetEffectPath((uint)effectId);
        if (m_EffectTable.ContainsKey(effectId))
        {
            m_EffectTable[effectId].StopFX();
            m_EffectTable.Remove(effectId);
        }

        BehaviorManager.Instance.LogFormat(m_Agent, string.Format($"StopEffect effectType:{effectType} effectId:{effectId} path:{path}"));
    }

	public void CreateIndex(string key)
	{
		BehaviorManager.Instance.LogFormat(m_Agent, string.Format($"CreateIndex key:{key}"));
		m_Index.Add(key, 0);
	}

	public int GetIndexByKey(string key)
	{
		BehaviorManager.Instance.LogFormat(m_Agent, string.Format($"GetIndexByKey key:{key} m_Index[key]:{m_Index[key]}"));
		return m_Index[key];
	}

	public void PlayerGlobalSound(int soundID, string key)
    {
		Sound? sound = m_CfgEternityProxy.GetSoundByKey((uint)soundID);
		SoundEvent soundEvent = m_CfgEternityProxy.GetSoundEvent((uint)sound.Value.EventId);
		WwiseUtil.PlaySound(soundID, false, soundEvent.Type ? m_Property.GetRootTransform() : null,
			(object obj) =>
			{
				if (!string.IsNullOrEmpty(key) && m_Index.ContainsKey(key))
				{
					m_Index[key] ++;
				}
			});

		BehaviorManager.Instance.LogFormat(m_Agent, $"PlayerGlobalSound soundID:{soundID} is3D:{soundEvent.Type}");
	}

	public void PlayVideoSound(int groupID, string key)
	{
		PlayParameter playParameter = new PlayParameter();
		playParameter.groupId = groupID;
		playParameter.action = () =>
		{
			if (!string.IsNullOrEmpty(key) && m_Index.ContainsKey(key))
			{
				m_Index[key]++;
			}
		};
		GameFacade.Instance.SendNotification(NotificationName.VideoPhoneChange, playParameter);
		BehaviorManager.Instance.LogFormat(m_Agent, $"PlayVideoSound groupID:{groupID}");
	}

	public void PlayVideoSoundToNpc(EnumSoundType type, string key)
	{
		int groupId = GetGroupId(type);
		if (groupId == 0)
		{
			BehaviorManager.Instance.LogFormat(m_Agent, $"PlayVideoSoundToNpc type:{type} groupId{groupId}");
			return;
		}
		PlayParameter playParameter = new PlayParameter();
		playParameter.groupId = groupId;
		playParameter.action = () =>
		{
			if (!string.IsNullOrEmpty(key) && m_Index.ContainsKey(key))
			{
				m_Index[key]++;
			}
		};
		GameFacade.Instance.SendNotification(NotificationName.VideoPhoneChange, playParameter);
		BehaviorManager.Instance.LogFormat(m_Agent, $"PlayVideoSoundToNpc type:{type} groupId{groupId}");
	}

	public void PlayModelSound(EnumSoundType type, string key)
	{
		int soundID = GetModelSoundId(type);
		if (soundID == 0)
		{
			BehaviorManager.Instance.LogFormat(m_Agent, $"PlayModelSound type:{type} soundID:{soundID} not have sound in model.csv");
			return;
		}

		Sound? sound = m_CfgEternityProxy.GetSoundByKey((uint)soundID);
		SoundEvent soundEvent = m_CfgEternityProxy.GetSoundEvent((uint)sound.Value.EventId);
		WwiseUtil.PlaySound(soundID, false, soundEvent.Type ? m_Property.GetRootTransform() : null,
			(object obj) =>
			{
				if (!string.IsNullOrEmpty(key) && m_Index.ContainsKey(key))
				{
					m_Index[key]++;
				}
			});

		BehaviorManager.Instance.LogFormat(m_Agent, $"PlayModelSound type:{type} soundID:{soundID} is3D:{soundEvent.Type}");
	}

	public void PlayComboSound(EnumSoundComboType type, string key)
	{
		int comboID = GetModelComboID();
		if (comboID == 0)
		{
			BehaviorManager.Instance.LogFormat(m_Agent, $"PlayComboSound comboID:{comboID} not have comboID in model.csv");
			return;
		}
		int soundID = (int)type;
		SoundComboData data = m_CfgEternityProxy.GetSoundComboDataByKeyAndType(comboID, soundID);
		bool canPlay = (data.Place == 1 && m_Property.IsMain()) || data.Place == 3;
		if (canPlay)
		{
			Sound? sound = m_CfgEternityProxy.GetSoundByKey((uint)data.MusicId);
			SoundEvent soundEvent = m_CfgEternityProxy.GetSoundEvent((uint)sound.Value.EventId);
			WwiseUtil.PlaySound(comboID, (WwiseMusicSpecialType)soundID,m_Property.IsMain() ? WwiseMusicPalce.Palce_1st : WwiseMusicPalce.Palce_3st, false, soundEvent.Type ? m_Property.GetRootTransform() : null,
			(object obj) =>
			{
				if (!string.IsNullOrEmpty(key) && m_Index.ContainsKey(key))
				{
					m_Index[key]++;
				}
			});
		}
		BehaviorManager.Instance.LogFormat(m_Agent, $"PlayComboSound comboID:{comboID} type:{type} canPlay:{canPlay}");
	}

	public void PlayComboSystemSound(EnumSoundComboType type, string key)
	{
		int soundID = (int)type;
		SoundComboData data = m_CfgEternityProxy.GetSoundComboDataByKeyAndType(WwiseManager.voiceComboID, soundID);
		bool canPlay = (data.Place == 1 && m_Property.IsMain()) || data.Place == 3;
		if (canPlay)
		{
			WwiseUtil.PlaySound(WwiseManager.voiceComboID, (WwiseMusicSpecialType)soundID, WwiseMusicPalce.Palce_1st, false, null,
				(object obj) =>
				{
					if (!string.IsNullOrEmpty(key) && m_Index.ContainsKey(key))
					{
						m_Index[key]++;
					}
				});
		}

		BehaviorManager.Instance.LogFormat(m_Agent, $"PlayComboSound comboID:{WwiseManager.voiceComboID} type:{type} canPlay:{canPlay}");
	}

    public bool IsChangeMainState()
    {
        return m_IsChangeMainState;
    }

    public bool SpeedComparison(float comparisonValue, EnumComparisonType comparisonType)
    {
        /// 没有刚体认为速为0
        Vector3 velocity = Vector3.zero;

        Rigidbody rigidbody = m_Property.GetRigidbody();
        if (rigidbody != null)
        {
            velocity = rigidbody.velocity;
        }

        switch (comparisonType)
        {
            case EnumComparisonType.Equal:
                return Math.Abs(velocity.magnitude - comparisonValue) <= 0.001f;
            case EnumComparisonType.More:
                return velocity.magnitude > comparisonValue;
            case EnumComparisonType.Less:
                return velocity.magnitude < comparisonValue;
            default:
                return false;
        }
    }

    public bool CheckMainState(EnumMainState state)
    {
        return state == m_Property.GetCurrentState().GetMainState();
    }

    public EnumMainState GetLastMainState()
    {
        return m_Property.GetPreviousState().GetMainState();
    }

    public bool IsHasSubState(EnumSubState subState)
    {
        return m_Property.GetCurrentState().IsHasSubState(subState);
    }

    public void SetSkinVisible(bool isShow)
    {
        m_Property.SetSkinVisiableForce(isShow);
    }

    public float GetDeadEffectDelayTime()
    {
        Model model = m_CfgEternityProxy.GetItemModelByKey(m_Property.GetItemID());
        BehaviorManager.Instance.LogFormat(m_Agent, string.Format($"GetDeadEffectDelayTime model.BodyTime:{model.BodyTime}"));
        return model.BodyTime;
    }

	public void CheckDeadDrop()
	{
		///SendEvent(ComponentEventName.CheckDeadDrop, null);
	}

    public void DoMotion(EnumDoMotionType doMotionType)
    {
        BehaviorController behaviorController = m_Property.GetBehaviorController();
        if (behaviorController == null)
        {
            BehaviorManager.Instance.LogErrorFormat(m_Agent, "behaviorController is null");
            return;
        }

        switch (doMotionType)
        {
            case EnumDoMotionType.DoSlide:
                behaviorController.Slide(0.1f);
                BehaviorManager.Instance.LogFormat(m_Agent, "DoMotion Slide");
                break;
            case EnumDoMotionType.DoStop:
                behaviorController.StopMove();
                BehaviorManager.Instance.LogFormat(m_Agent, "DoMotion StopMove");
                break;
            default:
                break;
        }
    }

	public bool IsMainPlayer()
	{
		return m_Property.IsMain();
	}

	public void ChangeCamera(EnumCMType type)
	{
		if (m_Property.IsMain())
		{
			MainCameraComponent mainCameraComponent = CameraManager.GetInstance().GetMainCamereComponent();
			switch (type)
			{
				case EnumCMType.LeapPrepare:
					mainCameraComponent.RequestChangeCM(MainCameraComponent.CMType.LeapPrepare);
					break;
				case EnumCMType.Leaping:
					mainCameraComponent.RequestChangeCM(MainCameraComponent.CMType.Leaping);
					break;
				case EnumCMType.LeapFinish:
					mainCameraComponent.RequestChangeCM(MainCameraComponent.CMType.LeapFinish);
					break;
				case EnumCMType.Spacecraft:
					if (m_Property.GetMotionMode() == EnumMotionMode.Dof6ReplaceOverload)
					{
						mainCameraComponent.RequestChangeCM(MainCameraComponent.CMType.Jet);
					}
					else
					{
						mainCameraComponent.RequestChangeCM(MainCameraComponent.CMType.Spacecraft);
					}
					break;
				default:
					break;
			}
			BehaviorManager.Instance.LogFormat(m_Agent, $"ChangeCamera type:{type}");
		}
	}

	#endregion
}
