using Assets.Scripts.Define;
using Assets.Scripts.Proto;
using System;
using UnityEngine;
using Eternity.FlatBuffer;
using PureMVC.Patterns.Facade;
using DG.Tweening;
using crucis.attributepipeline;
using System.Collections;

/// <summary>
/// 人形态实体类
/// </summary>
public class HumanEntity : GameEntity<S2C_SYNC_NEW_HERO>,
	ISyncMapPositionProperty,
	IHumanAvatarProperty,
	IHumanAnimatorProperty,
	IHumanInputProperty,
	IHumanMotionProperty,
	IInputSampleProperty,
	IHumanCorrectionPositionYProperty,
	Interaction.IInteractable,
	Interaction.IOwnerPlayer
{
	private CfgEternityProxy m_CfgEternityProxy;


	Player? m_Player;

	/// <summary>
	/// 静态数据
	/// </summary>
	private Npc m_NpcTmpVO;

	/// <summary>
	/// 旋转节点 用于计算人物的移动方向 使移动方向与人物的皮肤旋转分离
	/// </summary>
	private Transform m_MovementRotationTransform;

	/// <summary>
	/// 皮肤节点
	/// </summary>
	private Transform m_SkinTransform;

	/// <summary>
	/// 动画播放逻辑
	/// </summary>
	private AnimatorService m_AnimatorService;

	/// <summary>
	/// 出生时服务器人物坐标
	/// </summary>
	private Vector3 m_BornServerPosition;

	/// <summary>
	/// 出生时服务器人物旋转
	/// </summary>
	private Quaternion m_BornServerRotation;

	/// <summary>
	/// 传送点ID
	/// </summary>
	private ulong m_TeleportId;

	private string m_AssetName;

	/// <summary>
	/// 属性
	/// </summary>
	private AttributeManager m_AttManager = new AttributeManager();

	public override void InitializeByRespond(S2C_SYNC_NEW_HERO respond)
	{
		InitBaseProperty(respond);

		m_CfgEternityProxy = (CfgEternityProxy)GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy);

		ServerListProxy serverListProxy = GameFacade.Instance.RetrieveProxy(ProxyName.ServerListProxy) as ServerListProxy;
		m_IsMain = respond.ownerPlayerID == serverListProxy.GetCurrentCharacterVO().UId;
		if (m_IsMain)
		{
			GameplayProxy gameplayProxy = GameFacade.Instance.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;
			gameplayProxy.SetMainPlayerUID(respond.id);

			gameplayProxy.SetCurrentAreaUid(respond.area_id);
		}

		if (respond.item_tid != 0)
		{
			CfgEternityProxy cfgEternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
			m_AssetName = cfgEternityProxy.GetItemModelAssetNameByKey(respond.item_tid);
		}

		if (respond.ownerPlayerID != 0)
		{
			m_HeroType = KHeroType.htPlayer;

			if (m_IsMain)
			{
				Interaction.InteractionManager.GetInstance().RegisterOwnerPlayer(this);
			}

			m_Player = m_CfgEternityProxy.GetPlayerByItemTId((int)GetItemID());
		}
		else
		{
			m_NpcTmpVO = (GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy).GetNpcByKey((uint)respond.templateID);
			m_HeroType = (KHeroType)m_NpcTmpVO.NpcType;
			m_TeleportId = respond.teleport_id_;

			if (m_NpcTmpVO.Function.HasValue || m_TeleportId > 0)
			{
				Interaction.InteractionManager.GetInstance().RegisterInteractable(this);
				//NetworkManager.Instance.GetTaskController().SendRequetNpcCanAcceptTaskInfo(respond.templateID);
			}

			if (m_NpcTmpVO.InteractionDelay > 0)
			{
				m_IsActive = false;
				UIManager.Instance.StartCoroutine(Excute(m_NpcTmpVO.InteractionDelay, () =>
				{
					m_IsActive = true;
				}));
			}
			else
			{
				m_IsActive = true;
			}
		}

		transform.name += "_" + m_HeroType.ToString();

		m_MovementRotationTransform = new GameObject("Rotation").transform;
		m_MovementRotationTransform.SetParent(transform);

		m_BornServerPosition = new Vector3(respond.posX, respond.posY, respond.posZ);
		SetLocalPosition(m_BornServerPosition);

		m_BornServerRotation = new Quaternion(respond.faceDirX, respond.faceDirY, respond.faceDirZ, respond.faceDirW);
		SetMovementLocalRotation(m_BornServerRotation);

		if (m_IsMain)
		{
            CameraManager.GetInstance().GetMainCamereComponent().SetFollowAndLookAtCMFreeLookAxisValue(MainCameraComponent.CMType.Character, transform, transform, m_BornServerRotation.eulerAngles.y, 0.5f);
            CameraManager.GetInstance().GetMainCamereComponent().RequestChangeCM(MainCameraComponent.CMType.Character);

            transform.name += "(Self)";

#if ENABLE_SYNCHRONOUS_HUMAN_SELF_LOG
            FightLogToFile.Instance.Write("===== InitializeByRespond =====\n");
            FightLogToFile.Instance.Write("time " + Utils.Timer.ClockUtil.Instance().GetMillisecond() + "\n");
            FightLogToFile.Instance.WriteToJson("S2C_SYNC_NEW_HERO", respond);
            FightLogToFile.Instance.Write("\n");
#endif

		}
		if (m_HeroType == KHeroType.htPlayer)
		{
			//音效组合,Listener 的目标
			if (m_Player.HasValue)
			{
				WwiseUtil.LoadSoundCombo(m_Player.Value.MusicComboID);
			}
		}


		int humanLayer = m_HeroType == KHeroType.htPlayer
						? IsMain()
							? GameConstant.LayerTypeID.MainPlayer
							: GameConstant.LayerTypeID.HumanOtherPlayer
						: GameConstant.LayerTypeID.HumanNPC;
		LayerUtil.SetGameObjectToLayer(gameObject, humanLayer, true);
		m_AttManager = new AttributeManager();
		InitAttManager();
	}

	/// <summary>
	/// 延迟调用
	/// </summary>
	/// <param name="seconds">秒数</param>
	/// <param name="callBack">回调函数</param>
	/// <returns></returns>
	private static IEnumerator Excute(float seconds, Action callBack)
	{
		yield return new WaitForSeconds(seconds);
		callBack();
	}

	public void InitAttManager()
	{
		if (m_AttManager != null)
		{
			if (m_Player.HasValue && m_Player.Value.Effect != 0)
				m_AttManager.AddEffect(0, (uint)m_Player.Value.Effect);
			m_AttManager.Run();
		}
	}

	public float GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName id)
	{
		return Convert.ToSingle(m_AttManager.getPlayerAttribute().GetValue(id));
	}

	public override void OnRemoveEntity()
	{
		if (m_HeroType == KHeroType.htPlayer)
		{
			Interaction.InteractionManager.GetInstance().UnregisterOwnerPlayer(this);
		}
		else
		{
			Interaction.InteractionManager.GetInstance().UnregisterInteractable(this);
		}

		//卸载音效组合
		if (m_HeroType == KHeroType.htPlayer && m_Player.HasValue)
		{
			WwiseUtil.UnLoadSoundCombo(m_Player.Value.MusicComboID);
		}
		base.OnRemoveEntity();
	}


	public override void InitializeComponents()
	{
		AddEntityComponent<HumanAvatarComponent, IHumanAvatarProperty>(this);

		if (m_HeroType == KHeroType.htPlayer)
		{
			if (m_IsMain)
			{
				AddEntityComponent<SyncMapPositionComponent, ISyncMapPositionProperty>(this);
				AddEntityComponent<InputSampleComponent, IInputSampleProperty>(this);
				AddEntityComponent<HumanInputComponent, IHumanInputProperty>(this);
			}
			AddEntityComponent<HumanMotionComponent, IHumanMotionProperty>(this);
		}
		else
		{
			AddEntityComponent<HumanCorrectionPositionYComponent, IHumanCorrectionPositionYProperty>(this);
		}
		AddEntityComponent<HumanAnimatorComponent, IHumanAnimatorProperty>(this);
	}

	private void Update()
	{
		DispatchUpdate(Time.deltaTime);
	}

	private void FixedUpdate()
	{
		DispatchFixedUpdate();
	}

	#region Interface
	public Vector3 GetLocalPosition()
	{
		return transform.localPosition;
	}

	public void SetLocalPosition(Vector3 position)
	{
		transform.localPosition = position;
	}

	public void SetMovementLocalRotation(Quaternion rotation)
	{
		m_MovementRotationTransform.localRotation = rotation;
	}

	public Vector3 InverseTransformDirection(Vector3 direction)
	{
		return transform.InverseTransformDirection(direction);
	}

	public Vector3 TransformDirection(Vector3 direction)
	{
		return m_MovementRotationTransform.TransformDirection(direction);
	}

	public Transform GetMovementRotationTransform()
	{
		return m_MovementRotationTransform;
	}

	public uint GetUId()
	{
		return UId();
	}

	public Vector3 GetBornServerPosition()
	{
		return m_BornServerPosition;
	}

	public Quaternion GetBornServerRotation()
	{
		return m_BornServerRotation;
	}

	public string GetModelName()
	{
		return m_AssetName;
	}

	public Transform GetModleParent()
	{
		return transform;
	}

	public float GetRunSpeed()
	{
		float speed = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kSpeedRun);
		return speed;
	}

	public void SetSkinTransform(Transform transform)
	{
		m_SkinTransform = transform;
	}

	public Transform GetSkinTransform()
	{
		return m_SkinTransform;
	}

	public Transform GetTransform()
	{
		return transform;
	}

	public Transform GetRootTransform()
	{
		return transform;
	}

	public float GetInteractableRadius()
	{
		return m_NpcTmpVO.InteractionVolume;
	}

	public float GetInteractableDistanceSquare()
	{
		return m_NpcTmpVO.TriggerRange * m_NpcTmpVO.TriggerRange;
	}

	public bool GetCanBeAttack()
	{
		return m_NpcTmpVO.CanBeAttack;
	}

	public bool IsAlive()
	{
		return this != null;
	}

	public Vector3 GetWorldPosition()
	{
		// HACK 交互时坐标
		return transform.position + Vector3.one;
	}

	public void SetFocus(bool focus)
	{
		if (focus)
		{
			MsgInteractiveInfo msg = MessageSingleton.Get<MsgInteractiveInfo>();
			msg.Describe = null;

			MissionProxy missionProxy = GameFacade.Instance.RetrieveProxy(ProxyName.MissionProxy) as MissionProxy;
			if (missionProxy.GetCanSubmitMissionBy(GetTemplateID()) == null)
			{
				string fKeyText = missionProxy.GetTalkMissionFKeyText(GetTemplateID());
				if (!string.IsNullOrEmpty(fKeyText))
				{
					msg.Describe = fKeyText;
				}
			}

			msg.Tid = GetTemplateID();
			msg.MustUseHumanFBox = false;
			GameFacade.Instance.SendNotification(NotificationName.MSG_INTERACTIVE_SHOWFLAG, msg);
		}
		else
		{
			GameFacade.Instance.SendNotification(NotificationName.MSG_INTERACTIVE_HIDEFLAG);
			if (m_NpcTmpVO.DialogueTurn == 1)
				GetSkinTransform().DORotateQuaternion(m_BornServerRotation, 0.5f);
		}
	}

	public void OnInteracted()
	{
		MissionProxy missionProxy = GameFacade.Instance.RetrieveProxy(ProxyName.MissionProxy) as MissionProxy;
		LookAtMainPlayerOnInteraction();
		if (m_TeleportId > 0)
		{
			//手表升级传送等级限制
			if (m_NpcTmpVO.Function.HasValue && m_NpcTmpVO.Function.Value.Type == 5)
			{
				//可升级进行传送
				PlayerInfoVo player = NetworkManager.Instance.GetPlayerController().GetPlayerInfo();
				double exp = m_CfgEternityProxy.GetPlayerUpa(player.WatchLv).Exp;
				if (player.WatchExp >= exp && exp > 0)
				{
					NetworkManager.Instance.GetSceneController().SwitchMap((uint)m_TeleportId);
				}
				else
				{
					//经验值不足提示
					DialogueInfo info = new DialogueInfo();
					info.DialogueTid = (uint)(exp > 0 ? 510000 : 510001);
					info.SoundParent = transform;
					info.NpcTid = m_NpcTmpVO.Id;
					info.NeedSendToServer = false;
					GameFacade.Instance.SendNotification(NotificationName.MSG_DIALOGUE_SHOW, info);
					SetFocus(true);
				}
			}
			else
			{
				//切地图
				NetworkManager.Instance.GetSceneController().SwitchMap((uint)m_TeleportId);
			}
		}
		else if (missionProxy.OpenMissionPanel(GetUId(), m_NpcTmpVO, transform))
		{
			SetFocus(true);
			return;
		}
		else if (m_NpcTmpVO.Function.HasValue && !string.IsNullOrEmpty(m_NpcTmpVO.Function.Value.Arg1))
		{
			if (Enum.TryParse(m_NpcTmpVO.Function.Value.Arg1, out UIPanel panelName))
			{
				if (m_NpcTmpVO.Function.Value.Arg3 > 0)
				{
					UIManager.Instance.OpenPanel(panelName, m_NpcTmpVO.Function.Value.Arg3);
				}
				else
				{
					UIManager.Instance.OpenPanel(panelName);
				}
			}
			SetFocus(true);
		}
		else if (m_NpcTmpVO.Function.HasValue && m_NpcTmpVO.Function.Value.Type == 4)
		{
			NetworkManager.Instance.GetPlayerController().RequestLevelUpWatch();
			SetFocus(true);
		}
		else if (m_NpcTmpVO.Function.HasValue && m_NpcTmpVO.Function.Value.Arg2Length > 0)
		{
			missionProxy.OpenNpcChat(m_NpcTmpVO, transform, 0, m_NpcTmpVO.Id);
			SetFocus(true);
		}
	}

	private void LookAtMainPlayerOnInteraction()
	{
		//npc 转向对着人物
		if (m_NpcTmpVO.DialogueTurn == 1)
		{
			GameplayProxy gamePlayProxy = GameFacade.Instance.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;
			HumanEntity mainPlayer = gamePlayProxy.GetEntityById<HumanEntity>(gamePlayProxy.GetMainPlayerUID());

			Transform mptf = (mainPlayer != null) ? mainPlayer.GetSkinTransform() : null;

			if (mptf)
				GetSkinTransform().DOLookAt(new Vector3(mptf.position.x, GetSkinTransform().position.y, mptf.position.z), 0.5f).SetAutoKill(true);

		}
	}


	public string GetDisplay()
	{
		return gameObject.name;
	}

	#endregion
}
