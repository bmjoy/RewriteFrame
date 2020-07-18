using Assets.Scripts.Define;
using Assets.Scripts.Proto;
using DebugPanel;
using DG.Tweening;
using Eternity.FlatBuffer;
using System;
using System.Collections.Generic;
using UnityEngine;
using crucis.attributepipeline;
using Leyoutech.Core.Effect;
using Track;
using Gameplay.Battle.Skill;
using System.Collections;

public partial class SpacecraftEntity : GameEntity<S2C_SYNC_NEW_HERO>,
	IInitializeAttributeProperty,
	ISyncMapPositionProperty,
	ISpacecraftRefreshAxisProperty,
	ISpacecraftAvatarProperty,
	ISpacecraftPlayerAnimatorProperty,
	ISpacecraftMotionProperty,
	ISpacecraftMimesisComponentProperty,
	ISpacecraftInputProperty,
	ISpacecraftCinemachineProperty,
	IInputSampleProperty,
	ISpacecraftCheckOffsetProperty,
	ISetOffsetComponentProperty,
	IPicktargetProperty,
	IBuffProperty,
	Interaction.IInteractable,
	Interaction.IOwnerPlayer,
	ICampProperty,
	ISpacecraftDeadProperty,
	ISkillRefactorProperty,
	ISpacecraftReliveProperty,
	IBattleWeaponProperty,
	ISpacecraftSoundProperty,
	ISpacecraftHitWarningProperty,
	IChangeMaterialProperty,
	IMineFractureProperty,
    ISpacecraftBehaviorProperty,
    ISpacecraftCollisionProperty,
    Track.ITrackObject
#if NewSkill
    ,IPerceptronTarget
    ,ISpacecraftSkillProperty //技能组件
#endif
{

	/// <summary>
	/// 旋转节点
	/// </summary>
	private Transform m_SkinRootTransform;

	/// <summary>
	/// 新运动模型
	/// </summary>
	private Transform m_VirtualCameraTransform;
	private Vector3 m_PosValue;

	/// <summary>
	/// 皮肤节点
	/// </summary>
	private Transform m_SkinTransform;

	/// <summary>
	/// 出生时服务器坐标
	/// </summary>
	private Vector3 m_BornServerPosition;

	/// <summary>
	/// 刚体
	/// </summary>
	private Rigidbody m_Rigidbody;

	/// <summary>
	/// 摇杆向量
	/// </summary>
	private Vector3 m_EngineAxis;

	/// <summary>
	/// 摇杆向量
	/// </summary>
	private Vector3 m_RotateAxis;

	private Vector3 m_CameraRot;

    /// <summary>
    /// 鼠标偏移增量
    /// </summary>
    private Vector3 m_MouseDelta;

    /// <summary>
    /// 属性表
    /// </summary>
    private AttributeTable m_Attributes = new AttributeTable();

	/// <summary>
	/// 服务器创建角色消息缓存
	/// </summary>
	private S2C_SYNC_NEW_HERO m_NewHeroRespondCache;

	/// <summary>
	/// 静态数据
	/// </summary>
	private Npc m_NpcTmpVO;
	/// <summary>
	/// 出生时服务器人物旋转
	/// </summary>
	private Quaternion m_BornServerRotation;

	private SpacecraftEntity m_Target;
	private Collider m_TargetCollider;

	private List<Collider> m_ShipColliders;
	private List<Collider> m_RunTimeColliders;

	/// <summary>
	/// 飞船的Prefab上挂的脚本
	/// </summary>
	private SpacecraftPresentation m_SpacecraftPresentation;

	private float m_OverloadProgress;

	private bool m_BurstReady = false;
	/// <summary>
	/// 无双模式. (其实就是开启了转化炉)
	/// </summary>
	private bool m_Burst;

	/// <summary>
	/// 当前飞船实装的武器
	/// </summary>
	private Dictionary<ulong, BattleWeaponBase> m_BattleWeapons;
    /// <summary>
    /// 当前飞船实装的武器&准星
    /// </summary>
    private Dictionary<ulong, WeaponAndCrossSight> m_WeaponAndCrossSightDic;

	/// <summary>
	/// 武器能量, 弹夹弹药数量
	/// ulong: 武器的物品UID
	/// value: 武器弹药数据
	/// </summary>
	private Dictionary<ulong, WeaponPowerVO> m_WeaponPowers = new Dictionary<ulong, WeaponPowerVO>();

	/// <summary>
	/// 当前正在释放的技能ID
	/// 如果小于0, 就没在释放技能
	/// </summary>
	private int m_CurrentSkillID = -1;

	/// <summary>
	/// 当前的技能状态
	/// </summary>
	private SkillState m_CurrentSkillState;

	/// <summary>
	/// 是否正在持续释放Trigger技能
	/// </summary>
	private bool m_ReleasingTriggerSkill;

	/// <summary>
	/// Trigger技能的ID, 因为Trigger一般是瞬发技能, 所以不会一直保存技能ID
	/// </summary>
	private int m_TriggerSkillID;

	/// <summary>
	/// 传送点ID
	/// </summary>
	private ulong m_TeleportId;
	/// <summary>
	/// 掉落物ID
	/// </summary>
	private ulong m_DropItemUid;
	/// <summary>
	/// 开火后不可以切换到巡航模式, 等这个倒计时走完才能切换
	/// </summary>
	private float m_FireCountdown;

	/// <summary>
	/// 受击提示音的冷却时间
	/// 每次受击时置为
	/// </summary>
	private float m_UnderAttackWarningToneCountdown;

	private Dictionary<uint, Buff> m_BuffTable;

	private string m_NickName;


	/// <summary>
	/// key: 对方阵营, value: 当前单位对key的阵营的声望值
	/// 对应表camp_list.xlsx
	/// </summary>
	private Dictionary<uint, uint> CampPrestigeMap;

	private GameplayProxy m_GameplayProxy;
	private PlayerSkillProxy m_PlayerSkillProxy;
	private CfgEternityProxy m_CfgEternityProxy;

	private string m_AssetName;

	// FIXME BUFF. 整理飞船身上的特效
	/// <summary>
	/// 挂在飞船身上的所有循环特效
	/// </summary>
	private List<EffectController> m_VFXList;

	/// <summary>
	/// 隐身
	/// </summary>
	private bool m_Invisible;

	/// <summary>
	/// 运动状态
	/// </summary>
	private MotionType m_MotionType;

	/// <summary>
	/// 当前是否能切换武器
	/// </summary>
	private bool m_CanToggleWeapon;

	private int m_LODLevel;

	/// <summary>
	/// 属性
	/// </summary>
	private AttributeManager m_AttManager = new AttributeManager();

	Action<Collision> m_OnCollisionEnterAction = delegate { };
	Action<Collision> m_OnCollisionStayAction = delegate { };
	Action<Collision> m_OnCollisionExitAction = delegate { };

	/// <summary>
	/// 所有道具列表
	/// </summary>
	private Dictionary<ulong, ItemContainer> m_Items = new Dictionary<ulong, ItemContainer>();

	private GameObject m_SyncTarget;

	private Vector2 m_DefaultCMAxisValue;

	/// <summary>
	/// 弱点属性
	/// </summary>
	private uint m_WeakEffect = 0;

	/// <summary>
	/// 是否封印1是封印
	/// </summary>
	private byte m_IsSeal;

    /// <summary>
    /// 是否碰撞
    /// </summary>
    private bool m_IsBlock = false;
    public void SetIsBlock(bool block)
    {
        m_IsBlock = block;
    }
    public bool GetIsBlock()
    {
        return m_IsBlock;
    }

    /// <summary>
    /// 状态行为树ID
    /// </summary>
    private int m_BehaviorId;
    public int GetBehaviorId()
    {
        return m_BehaviorId;
    }

	private void InitializeCameraParams(WarshipCameraDofFour warshipCameraDofFour, WarshipCameraDofSix warshipCameraDofSix)
	{
		SetMimesisRollMaxAngles4Dof(warshipCameraDofFour.MimesisMaximumAngle);
		SetMimesisYMaxAngles4Dof(warshipCameraDofFour.YaxisMimicRotationAngle);
		SetMimesisXDownMaxAngles4Dof(warshipCameraDofFour.XaxisDownMimicAngle);
		SetMimesisXUpMaxAngles4Dof(warshipCameraDofFour.XaxisUpMimicAngle);
		SetTurnBegin4DofLerpSpeed(warshipCameraDofFour.OffCenterSpeed);
		SetTurnEnd4DofLerpSpeed(warshipCameraDofFour.BackCenterSpeed);
		SetTurn4DofFactorX(warshipCameraDofFour.DragLeftAndRightOffset);
		SetTurn4DofFactorUp(warshipCameraDofFour.DragUpOffset);
		SetTurn4DofFactorDown(warshipCameraDofFour.DragDownOffset);

		SetMimesisRollTimeScale4Dof(warshipCameraDofFour.MimesisVelocity);
		SetVirtualOffset4Dof(warshipCameraDofFour.AdkeyOffsetDistance);
		SetVirtualOffsetRate4Dof(warshipCameraDofFour.AdkeyOffsetSpeed);
		SetVirtualRecoverRate4Dof(warshipCameraDofFour.AdkeyRecoverySpeed);
		SetMouseDeltaFactor4Dof(warshipCameraDofFour.XaxisMaxSpeedMouse, warshipCameraDofFour.YaxisMaxSpeedMouse);
		SetStickDeltaFactor4Dof(warshipCameraDofFour.XaxisMaxSpeedGamepad, warshipCameraDofFour.YaxisMaxSpeedGamepad);

		SetMimesisMaximumAngle6Dof(warshipCameraDofSix.MimesisMaximumAngle);
		SetMouseDeltaFactor6Dof(warshipCameraDofSix.XaxisMaxSpeedMouse, warshipCameraDofSix.YaxisMaxSpeedMouse);
		SetStickDeltaFactor6Dof(warshipCameraDofSix.XaxisMaxSpeedGamepad, warshipCameraDofSix.YaxisMaxSpeedGamepad);
		SetCrossRecoverTime6Dof(warshipCameraDofSix.ZeroSpeedGamepad);
		SetTurnBegin6DofLerpSpeed(warshipCameraDofSix.OffCenterSpeed);
		SetTurnEnd6DofLerpSpeed(warshipCameraDofSix.BackCenterSpeed);
		SetTurn6DofFactor(warshipCameraDofSix.DragOffset);

		SetDof6LookAtZ(warshipCameraDofSix.TurningDegree);
    }

	public uint HeroGroupId;
	public override void InitializeByRespond(S2C_SYNC_NEW_HERO respond)
	{
        InitBaseProperty(respond);

		m_GameplayProxy = GameFacade.Instance.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;
		m_PlayerSkillProxy = GameFacade.Instance.RetrieveProxy(ProxyName.PlayerSkillProxy) as PlayerSkillProxy;
		m_CfgEternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;

		ServerListProxy serverListProxy = GameFacade.Instance.RetrieveProxy(ProxyName.ServerListProxy) as ServerListProxy;
		// 玩家的船 respond.ownerHeroID 为0
		m_IsMain = respond.ownerPlayerID == serverListProxy.GetCurrentCharacterVO().UId && respond.ownerHeroID == 0;

		//Debug.LogError(respond.ownerPlayerID+ "respond.ownerPlayerID ");
		//Debug.LogError("serverListProxy.GetCurrentCharacterVO().UId" + serverListProxy.GetCurrentCharacterVO().UId);

		if (m_IsMain)
		{
			m_GameplayProxy = GameFacade.Instance.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;
			m_GameplayProxy.SetMainPlayerUID(respond.id);

			MainCameraComponent mainCameraComponent = CameraManager.GetInstance().GetMainCamereComponent();

			/// 4/6dof camera
			uint camera_4dof = m_CfgEternityProxy.GetItemByKey(respond.item_tid).ItemUnion<Warship>().Value.CameraParameterDof4;
			WarshipCameraDofFour warshipCameraDofFour = m_CfgEternityProxy.GetWarshipCameraDof4(camera_4dof);
			float[] dof4_FollowOffsetArray = warshipCameraDofFour.GetFollowOffsetArray();
			Vector3 dof4_FollowOffset = new Vector3(dof4_FollowOffsetArray[0], dof4_FollowOffsetArray[1], dof4_FollowOffsetArray[2]);
			float[] dof4_TrackedObjectOffsetArray = warshipCameraDofFour.GetTrackedObjectOffsetArray();
			Vector3 dofd_TrackedObjectOffset = new Vector3(dof4_TrackedObjectOffsetArray[0], dof4_TrackedObjectOffsetArray[1], dof4_TrackedObjectOffsetArray[2]);
			mainCameraComponent.ApplyCMDofParam(MotionType.Dof4, dof4_FollowOffset, dofd_TrackedObjectOffset, warshipCameraDofFour.FieldOfView, 0, 0, 0, warshipCameraDofFour.WarshipYaxisOffset);

			uint camera_6dof = m_CfgEternityProxy.GetItemByKey(respond.item_tid).ItemUnion<Warship>().Value.CameraParameterDof4;
			WarshipCameraDofSix warshipCameraDofSix = m_CfgEternityProxy.GetWarshipCameraDof6(camera_6dof);
			float[] dof6_FollowOffsetArray = warshipCameraDofSix.GetFollowOffsetArray();
			Vector3 dof6_FollowOffset = new Vector3(dof6_FollowOffsetArray[0], dof6_FollowOffsetArray[1], dof6_FollowOffsetArray[2]);
			float[] dof6_TrackedObjectOffsetArray = warshipCameraDofSix.GetTrackedObjectOffsetArray();
			Vector3 dof6_TrackedObjectOffset = new Vector3(dof6_TrackedObjectOffsetArray[0], dof6_TrackedObjectOffsetArray[1], dof6_TrackedObjectOffsetArray[2]);
			float[] dampingArray = warshipCameraDofSix.GetDampingArray();

			mainCameraComponent.ApplyCMDofParam(MotionType.Dof6, dof6_FollowOffset, dof6_TrackedObjectOffset, warshipCameraDofSix.FieldOfView, dampingArray[0], dampingArray[1], dampingArray[2], warshipCameraDofSix.WarshipYaxisOffset);

			uint camera = m_CfgEternityProxy.GetItemByKey(respond.item_tid).ItemUnion<Warship>().Value.CameraParameter;
			WarshipCamera warshipCamera = m_CfgEternityProxy.GetWarshipCamera(camera).Value;
			m_DefaultCMAxisValue = new Vector2(warshipCamera.CMSpacecraftAxisDefaultX, warshipCamera.CMSpacecraftAxisDefaultY);
			mainCameraComponent.ApplyCMParam(warshipCamera);

			InitializeCameraParams(warshipCameraDofFour, warshipCameraDofSix);
		}

		m_NewHeroRespondCache = respond;
		m_EntityFatherOwnerID = respond.ownerHeroID;
		HeroGroupId = respond.heroGroup;

		m_IsSeal = respond.is_seal;

		if (respond.item_tid != 0)
		{
			m_AssetName = m_CfgEternityProxy.GetItemModelAssetNameByKey(respond.item_tid);
		}

        bool isRigidbody = false;
		bool isMovement = false;

        if (respond.ownerPlayerID != 0)
		{
			m_HeroType = KHeroType.htPlayer;
			if (m_IsMain)
			{
				Interaction.InteractionManager.GetInstance().RegisterOwnerPlayer(this);
			}

			isRigidbody = true;
			isMovement = true;

            m_BehaviorId = m_CfgEternityProxy.GetWarshipByKey((uint)respond.item_tid).StateBehaviorClient;

            Warship ship = m_CfgEternityProxy.GetWarshipByKey(respond.item_tid);
			m_MotionMode = (EnumMotionMode)ship.ExerciseType;
			/// 新运动模型才需要的
			if (m_MotionMode == EnumMotionMode.Dof6ReplaceOverload)
			{
				m_VirtualCameraTransform = new GameObject("VirtualCamera").transform;
				m_VirtualCameraTransform.SetParent(transform);
			}
		}
        else
		{
			m_NpcTmpVO = m_CfgEternityProxy.GetNpcByKey((uint)respond.templateID);
			m_HeroType = (KHeroType)m_NpcTmpVO.NpcType;

			m_TeleportId = respond.teleport_id_;

			if (m_NpcTmpVO.Function.HasValue || m_TeleportId > 0)
			{
				Interaction.InteractionManager.GetInstance().RegisterInteractable(this);
				//NetworkManager.Instance.GetTaskController().SendRequetNpcCanAcceptTaskInfo(respond.templateID);
			}

			isRigidbody = m_NpcTmpVO.IsRigid;
			isMovement = m_NpcTmpVO.Movable;

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

            m_BehaviorId = m_NpcTmpVO.StateBehaviorClient;
        }

		transform.name += "_" + m_HeroType.ToString();

		m_SkinRootTransform = new GameObject("Rotation").transform;
		m_SkinRootTransform.SetParent(transform);

		m_BornServerPosition = new Vector3(respond.posX, respond.posY, respond.posZ);
		m_BornServerRotation = new Quaternion(respond.faceDirX, respond.faceDirY, respond.faceDirZ, respond.faceDirW);

		#region 拖拽逻辑
		Vector3 clientPosition = Vector3.zero;
		GameplayProxy gameplayProxy = GameFacade.Instance.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;
		if (m_IsMain)
		{
			gameplayProxy.SetCurrentAreaUid(respond.area_id);
			transform.name += "(Self)";
			gameplayProxy.InitializeServerPositionOffset(m_BornServerPosition, out clientPosition);

			Vector3 worldPosition;
			gameplayProxy.ServerAreaOffsetPositionToWorldPosition(out worldPosition, m_BornServerPosition);
			Map.MapManager.GetInstance().SetPlayerPosition(worldPosition, clientPosition);
			Map.MapManager.GetInstance().ForceUpdate();

			PlayerSkillProxy skillProxy = GameFacade.Instance.RetrieveProxy(ProxyName.PlayerSkillProxy) as PlayerSkillProxy;
			skillProxy.RebuildSkillList();
		}
		else
		{
			clientPosition = gameplayProxy.ServerAreaOffsetToClientPosition(m_BornServerPosition);
		}
		#endregion

		transform.position = clientPosition;
		transform.rotation = m_BornServerRotation;

		//Debug.LogErrorFormat("name:{0} position{1}", transform.name, JsonUtility.ToJson(clientPosition));

		if (isMovement)
		{
			m_SyncTarget = new GameObject(transform.name + "(Shadow)");
			m_SyncTarget.transform.parent = transform.parent;
			m_SyncTarget.transform.position = transform.position;
			m_SyncTarget.transform.rotation = transform.rotation;
			m_SyncTarget.transform.localScale = transform.localScale;

			UnityCollisionEventProxy unityCollisionEventProxy = m_SyncTarget.AddComponent<UnityCollisionEventProxy>();

			AddRigidbody(m_SyncTarget, false);
		}

		if (isRigidbody)
		{
			AddRigidbody(gameObject, true);
		}

		//if (m_HeroType == KHeroType.htPlayer 
		//    || m_HeroType == KHeroType.htMonster
		//    || m_HeroType == KHeroType.htEliteMonster1
		//    || m_HeroType == KHeroType.htEliteMonster2
		//    || m_HeroType == KHeroType.htEliteMonster3
		//    || m_HeroType == KHeroType.htBoss
		//    || m_HeroType == KHeroType.htCampFlag)
		//{
		//    m_SyncTarget = new GameObject(transform.name + "(Shadow)");
		//    m_SyncTarget.transform.parent = transform.parent;
		//    m_SyncTarget.transform.position = transform.position;
		//    m_SyncTarget.transform.rotation = transform.rotation;
		//    m_SyncTarget.transform.localScale = transform.localScale;

		//    UnityCollisionEventProxy unityCollisionEventProxy = m_SyncTarget.AddComponent<UnityCollisionEventProxy>();
		//    unityCollisionEventProxy.FilterLayerName = "SceneOnly";

		//    AddRigidbody(m_SyncTarget, false);
		//    AddRigidbody(gameObject, true);
		//}

		m_ShipColliders = new List<Collider>();
		m_RunTimeColliders = new List<Collider>();
		m_VFXList = new List<EffectController>();
		m_BuffTable = new Dictionary<uint, Buff>();
		CampPrestigeMap = new Dictionary<uint, uint>();
		m_CanToggleWeapon = true;

		//掉落物的逻辑 怪死了给他加可交互属性 和创建出宝箱
		if (respond.moveState == (uint)KMoveState.mosDeath)
		{
			SetSkinVisiable(false); //死亡如果服务器发来new消息会创建一堆尸体 先给隐藏吧  觉得没必要创建
			if (DropItemManager.Instance.CheckIsDropItem(respond.id))
			{
				m_DropItemUid = respond.id;
				///DropItemManager.Instance.CreateDropItem(respond.id, this, 1);
				Interaction.InteractionManager.GetInstance().RegisterInteractable(this);
			}
		}


		//音效组合,Listener 的目标
		if (m_HeroType == KHeroType.htPlayer)
		{
			int MusicComboID = m_CfgEternityProxy.GetItemByKey(respond.item_tid).ItemUnion<Warship>().Value.MusicComboID;
			if (MusicComboID > 0)
			{
				WwiseUtil.LoadSoundCombo(MusicComboID);
			}

			if (m_IsMain)
			{
				WwiseUtil.SetListenerTarget(transform);
			}
		}

		SendEvent(ComponentEventName.NewSpacecraftEntity, new NewSpacecraftEvent()
		{
			HeroType = GetHeroType()
		});

		int spacecraftLayer = LayerUtil.GetLayerByHeroType(GetHeroType(), IsMain());
		LayerUtil.SetGameObjectToLayer(gameObject, spacecraftLayer, true);

		m_NickName = respond.szPlayerName;

		// 阵营
		m_CampID = respond.campID;

		m_LODLevel = 0;

		m_BattleWeapons = new Dictionary<ulong, BattleWeaponBase>();
        m_WeaponAndCrossSightDic = new Dictionary<ulong, WeaponAndCrossSight>();
        m_AttManager = new AttributeManager();

		m_WeakEffect = respond.weakness_effect_id;
		InitAttManager();

		TrackCaptureManager.GetInstance()
			.InitializeCapturer(string.Format("Spacecraft {0}{1} - {2}"
				, IsMain()
					? "Main "
					: ""
				, UId()
				, m_HeroType), this)
			.StartCapture();

		Debug.LogFormat("heroPos | ID: {3}.  Pos: {0}, {1}, {2}", respond.posX, respond.posY, respond.posZ, respond.id);

		if (m_HeroType == KHeroType.htBeEscortedNpc)
		{
			TaskTrackingProxy taskTrackingProxy = GameFacade.Instance.RetrieveProxy(ProxyName.TaskTrackingProxy) as TaskTrackingProxy;
			taskTrackingProxy.AddNpcInfo(respond.item_tid, m_BornServerPosition);
		}

		if (GetHeroType() == KHeroType.htMine && m_EntityFatherOwnerID > 0)
		{
			m_GameplayProxy.AddEntityToEntityGroup(m_EntityFatherOwnerID, this);
			SpacecraftEntity owner = m_GameplayProxy.GetEntityById<SpacecraftEntity>(m_EntityFatherOwnerID);
			if (owner != null)
			{
				m_GameplayProxy.AddEntityToEntityGroup(m_EntityFatherOwnerID, owner);
			}
		}

        if (GetSyncTarget() != null)
        {
            m_behaviorController = GetSyncTarget().gameObject.AddComponent<BehaviorController>();
        }

        InitMotion();
    }

	public override void InitializeComponents()
	{
		AddEntityComponent<InitializeAttributeComponent, IInitializeAttributeProperty>(this);

		/// TODO.宝藏特殊处理
		/// 服务器创建后立马死亡,不创建模型避免闪一下模型
		if (!IsNotHaveAva())
		{
			AddEntityComponent<SpacecraftAvatarComponent, ISpacecraftAvatarProperty>(this);
		}

		AddEntityComponent<SpacecraftRefreshAxisComponent, ISpacecraftRefreshAxisProperty>(this);

		if (m_IsMain)
		{
			AddEntityComponent<SyncMapPositionComponent, ISyncMapPositionProperty>(this);
			AddEntityComponent<InputSampleComponent, IInputSampleProperty>(this);
			AddEntityComponent<SpacecraftInputComponent, ISpacecraftInputProperty>(this);
			AddEntityComponent<SpacecraftCinemachineComponent, ISpacecraftCinemachineProperty>(this);
			AddEntityComponent<SpacecraftCheckeOffsetComponent, ISpacecraftCheckOffsetProperty>(this);
			AddEntityComponent<BattleWeaponComponent, IBattleWeaponProperty>(this);
			AddEntityComponent<SpacecraftHitWarningComponent, ISpacecraftHitWarningProperty>(this);

			// TEST
			AddComponent<DebugForSkillComponentRefactor>();
            // TEST
        }
        else
		{
			AddEntityComponent<SetOffsetComponent, ISetOffsetComponentProperty>(this);
		}

#if NewSkill
        AddEntityComponent<PerceptronTargetComponent, IPerceptronTarget>(this);
#endif

        if (m_SyncTarget != null)
		{
			AddEntityComponent<SpacecraftPlayerAnimatorComponent, ISpacecraftPlayerAnimatorProperty>(this);
			AddEntityComponent<SpacecraftMimesisComponent, ISpacecraftMimesisComponentProperty>(this);
			AddEntityComponent<SpacecraftMotionComponent, ISpacecraftMotionProperty>(this);
            AddEntityComponent<SpacecraftCollisionComponent, ISpacecraftCollisionProperty>(this);
        }
        AddEntityComponent<SkillComponentRefactor, ISkillRefactorProperty>(this);
		AddEntityComponent<PickTargetComponent, IPicktargetProperty>(this);
		AddEntityComponent<BuffComponent, IBuffProperty>(this);
		AddEntityComponent<CampComponent, ICampProperty>(this);
		AddEntityComponent<SpacecraftDeadComponent, ISpacecraftDeadProperty>(this);
		AddEntityComponent<SpacecraftReliveComponent, ISpacecraftReliveProperty>(this);
		AddEntityComponent<SpacecraftSoundComponent, ISpacecraftSoundProperty>(this);

#if NewSkill
        AddEntityComponent<SpacecraftSkillComponent, ISpacecraftSkillProperty>(this);
#endif

		if (m_HeroType == KHeroType.htDetector)
		{
			// TEST
			AddEntityComponent<ChangeMaterialComponent, IChangeMaterialProperty>(this);
			// TEST
		}
		else if (m_HeroType == KHeroType.htMine)
		{
			AddEntityComponent<MineFractureComponent, IMineFractureProperty>(this);
		}

        if (m_BehaviorId > 0)
        {
            AddEntityComponent<SpacecraftBehaviorComponent, ISpacecraftBehaviorProperty>(this);
        }
    }

	private void PlayVideoSoundEvent(int groupID, Action action = null, uint npcId = 0)
	{
		SendEvent(ComponentEventName.PlayVideoSound, new PlayVideoSound()
		{
			GroupID = groupID,
			Action = action,
			NpcId = npcId
		});
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

	private void AddWeaponEffects(IWeaponContainer weaponContainer)
	{
		if (weaponContainer != null)
		{
			IWeapon[] weapons = weaponContainer.GetWeapons();
			if (weapons != null)
			{
				for (int i = 0; i < weapons.Length; i++)
				{
					IWeapon weapon = weapons[i];
					ulong uid = weapon.GetUID();
					m_AttManager.InitAttacker(uid);
					int clv = weapon.GetLv();
					WeaponAttr? wattr = weapon.GetConfig().WeaponAttrData;
					if (wattr.HasValue)
					{
						for (int j = 0; j < wattr.Value.AttrsLength; j++)
						{
							WeaponAttrData? data = wattr.Value.Attrs(j);
							if (data.HasValue)
							{
								int lv = data.Value.Lv;
								if (lv == clv)
								{
									if (data.Value.Effect != 0)
									{
										m_AttManager.AddEffect(uid, (uint)data.Value.Effect);
									}
								}
							}
						}
					}

					// 武器mods
					AddModEffects(uid, weapon.GetGeneralModContainer());
					AddModEffects(uid, weapon.GetExclusivelyModContainer());
				}
			}
		}
	}

	private void AddEquipmentEffects(IEquipmentContainer equipmentContainer)
	{
		if (equipmentContainer != null)
		{
			IEquipment[] equipments = equipmentContainer.GetEquipments();
			if (equipments != null)
			{
				for (int i = 0; i < equipments.Length; i++)
				{
					IEquipment equipment = equipments[i];
					int clv = equipment.GetLv();
					EquipmentAttr? eAttr = equipment.GetConfig().Attr;
					if (eAttr.HasValue)
					{
						for (int j = 0; j < eAttr.Value.AttrsLength; j++)
						{
							EquipmentAttrData? data = eAttr.Value.Attrs(j);
							if (data.HasValue)
							{
								int lv = data.Value.Lv;
								if (lv == clv)
								{
									if (data.Value.Effect != 0)
									{
										m_AttManager.AddEffect(0, (uint)data.Value.Effect);
									}
								}
							}
						}
					}
				}
			}
		}
	}

	private void AddModEffects(ulong uid, IModContainer modContainer)
	{
		if (modContainer != null)
		{
			IMod[] smods = modContainer.GetMods();
			if (smods != null)
			{
				for (int i = 0; i < smods.Length; i++)
				{
					IMod mod = smods[i];
					int clv = mod.GetLv();
					ModAttr? mAttr = mod.GetConfig().Attr;
					if (mAttr.HasValue)
					{
						for (int j = 0; j < mAttr.Value.AttrsLength; j++)
						{
							ModAttrData? data = mAttr.Value.Attrs(j);
							if (data.HasValue)
							{
								int lv = data.Value.Lv;
								if (lv == clv)
								{
									if (data.Value.Effect != 0)
									{
										m_AttManager.AddEffect(uid, (uint)data.Value.Effect);
									}
								}
							}
						}
					}
				}
			}
		}
	}

	private void AddReformerEffects(IReformerContainer reformerContainer)
	{
		if (reformerContainer != null)
		{
			IReformer reformer = reformerContainer.GetReformer();
			if (reformer != null)
			{
				int clv = reformer.GetLv();
				WeaponAttr? wattr = reformer.GetConfig().WeaponAttrData;
				if (wattr.HasValue)
				{
					for (int j = 0; j < wattr.Value.AttrsLength; j++)
					{
						WeaponAttrData? data = wattr.Value.Attrs(j);
						if (data.HasValue)
						{
							int lv = data.Value.Lv;
							if (lv == clv)
							{
								if (data.Value.Effect != 0)
								{
									m_AttManager.AddEffect(0, (uint)data.Value.Effect);
								}
							}
						}
					}
				}
			}
		}
	}

	private void AddBuffEffects()
	{
		foreach (var buff in m_BuffTable)
		{
			for (int i = 0; i < buff.Value.VO.StackCount; i++)
			{
				int effect = buff.Value.GetEffect();
				if (effect != 0)
				{
					m_AttManager.AddEffect(0, (uint)effect);
				}
			}
		}
	}

	public void InitAttManager()
	{
		if (m_AttManager != null)
		{
			m_AttManager.Clear();
			ShipItemsProxy shipItemsProxy = GameFacade.Instance.RetrieveProxy(ProxyName.ShipItemsProxy) as ShipItemsProxy;
			IShip ship = shipItemsProxy.GetCurrentWarShip(this.EntityId());
			if (ship != null)
			{
				// 武器
				AddWeaponEffects(ship.GetWeaponContainer());

				// 船装备
				AddEquipmentEffects(ship.GetEquipmentContainer());

				// 船mods
				AddModEffects(0, ship.GetGeneralModContainer());
				AddModEffects(0, ship.GetExclusivelyModContainer());

				// 转化炉
				AddReformerEffects(ship.GetReformerContainer());

				// 船buffs
				AddBuffEffects();

				// hero
				if (m_HeroType == KHeroType.htPlayer)
				{
					int slv = ship.GetLv();
					Warship warship = ship.GetConfig();
					WarshipAttr? warshipattr = warship.WarshipAttrData;
					if (warshipattr.HasValue)
					{
						int count = warshipattr.Value.AttrsLength;
						for (int i = 0; i < count; i++)
						{
							WarshipAttrData? data = warshipattr.Value.Attrs(i);
							if (data.HasValue)
							{
								int lv = data.Value.WarshipLv;
								if (lv == slv)
								{
									if (data.Value.Effect != 0)
									{
										m_AttManager.AddEffect(0, (uint)data.Value.Effect);
									}
								}
							}
						}
					}

					if (warship.SpeedEffect != 0)
					{
						m_AttManager.AddEffect(0, warship.SpeedEffect);
					}
				}
				else
				{
					if (m_NpcTmpVO.SpeedEffect != 0)
					{
						m_AttManager.AddEffect(0, (uint)m_NpcTmpVO.SpeedEffect);
					}
					if (m_NpcTmpVO.Effect != 0)
					{
						m_AttManager.AddEffect(0, (uint)m_NpcTmpVO.Effect);
					}
				}
			}

			/// 弱点effect
			if (m_WeakEffect != 0)
			{
				m_AttManager.AddEffect(0, m_WeakEffect);
			}

			m_AttManager.Run();
			var value = m_AttManager.Compare();
			if (value.Item2)
			{
				SendEvent(ComponentEventName.SpacecraftUpdateSpeed, null);
			}
		}
	}

	/// <summary>
	/// 所有属性
	/// </summary>
	/// <param oid='武器oid'></param>
	/// <param id='AttributeName'></param>
	/// <returns></returns>
	/// 船体+所有武器
	float GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName id)
	{
		return Convert.ToSingle(m_AttManager.getPlayerAttribute().GetValue(id));
	}

	/// <summary>
	/// 防御所有属性
	/// </summary>
	/// <param oid='武器oid'></param>
	/// <param id='AttributeName'></param>
	/// <returns></returns>
	public float GetDefenderAttribute(crucis.attributepipeline.attribenum.AttributeName id)
	{
		return Convert.ToSingle(m_AttManager.getDefenderAttribute().GetValue(id));
	}

	/// <summary>
	/// 武器攻击属性
	/// </summary>
	/// <param oid='武器oid'></param>
	/// <param id='AttributeName'></param>
	/// <returns></returns>
	/// AttributeName 现在只有kAttack
	/// 例如：oid=武器1时，得到的是'船+武器1'的属性值
	/// 例如：oid=0，得到的是'船+所有武器'的属性值
	public float GetAttackerAttribute(ulong oid, crucis.attributepipeline.attribenum.AttributeName id)
	{
		return Convert.ToSingle(m_AttManager.getAttackerAttribute(oid).GetValue(id));
	}

	/// <summary>
	/// 武器属性
	/// </summary>
	/// <param oid='武器oid'></param>
	/// <param id='AttributeName'></param>
	/// <returns></returns>
	/// 例如：oid=武器1时，得到的是'船+武器1'的属性值
	/// 例如：oid=0，得到的是'船+所有武器'的属性值
	public float GetWeaponAttribute(ulong oid, crucis.attributepipeline.attribenum.AttributeName id)
	{
		return Convert.ToSingle(m_AttManager.getWeaponAttribute(oid).GetValue(id));
	}

	public override void OnRemoveEntity()
	{
		TrackCaptureManager.GetInstance().DestroyCapturer(this);

		if (m_HeroType == KHeroType.htPlayer)
		{
			Interaction.InteractionManager.GetInstance().UnregisterOwnerPlayer(this);
		}
		else
		{
			Interaction.InteractionManager.GetInstance().UnregisterInteractable(this);
		}
		if (m_HeroType == KHeroType.htBeEscortedNpc)
		{
			TaskTrackingProxy taskTrackingProxy = GameFacade.Instance.RetrieveProxy(ProxyName.TaskTrackingProxy) as TaskTrackingProxy;
			taskTrackingProxy.RemoveNpcInfo(m_NpcTmpVO.Id);
		}
		//卸载音效组合
		if (m_HeroType == KHeroType.htPlayer)
		{
			int MusicComboID = m_CfgEternityProxy.GetItemByKey(GetItemID()).ItemUnion<Warship>().Value.MusicComboID;
			if (MusicComboID > 0)
			{
				WwiseUtil.UnLoadSoundCombo(MusicComboID);
			}
		}
		else
		{
			/// 非死亡移除npc时也需要
			if (!IsDead())
			{
				Npc npcVO = GetNPCTemplateVO();
				if (npcVO.SoundDead > 0)
				{
					SendEvent(ComponentEventName.PlaySound, new PlaySound()
					{
						SoundID = (int)npcVO.SoundDead
					});
				}
			}
		}

		if (GetHeroType() == KHeroType.htMine && m_EntityFatherOwnerID > 0)
		{
			m_GameplayProxy.RemoveEntityFromEntityGroup(m_EntityFatherOwnerID, this);
		}

		GameObject.Destroy(m_SyncTarget);

		ClearBattleWeapon();
        ClearAllWeaponAndCrossSight();

		base.OnRemoveEntity();
	}

	private void AddRigidbody(GameObject go, bool isKinematic)
	{
		m_Rigidbody = go.AddComponent<Rigidbody>();
		m_Rigidbody.useGravity = false;
		m_Rigidbody.drag = 0;
		m_Rigidbody.angularDrag = 0;
		m_Rigidbody.constraints = RigidbodyConstraints.None;
		m_Rigidbody.velocity = Vector3.zero;
		m_Rigidbody.isKinematic = isKinematic;
		if (isKinematic)
		{
			m_Rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
		}
		else
		{
			m_Rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
		}
	}

	public void RespondMoveEvent(S2C_MONSTER_ROAD_POINT respond)
	{
		SendEvent(ComponentEventName.RespondMobnsterRoadPoint, new RespondMoveEvent()
		{
			IsMonster = true,
			Position = new Vector3(m_NewHeroRespondCache.posX, m_NewHeroRespondCache.posY, m_NewHeroRespondCache.posZ),
			Rotation = new Quaternion(m_NewHeroRespondCache.faceDirX, m_NewHeroRespondCache.faceDirY, m_NewHeroRespondCache.faceDirZ, m_NewHeroRespondCache.faceDirW),
			LineVelocity = new Vector3(m_NewHeroRespondCache.line_velocity_x, m_NewHeroRespondCache.line_velocity_y, m_NewHeroRespondCache.line_velocity_z),
			AngularVelocity = new Vector3(m_NewHeroRespondCache.angular_velocity_x, m_NewHeroRespondCache.angular_velocity_y, m_NewHeroRespondCache.angular_velocity_z),
			EngineAxis = new Vector3(m_NewHeroRespondCache.engine_axis_x, m_NewHeroRespondCache.engine_axis_y, m_NewHeroRespondCache.engine_axis_z),
			RotateAxis = new Vector3(m_NewHeroRespondCache.rotate_axis_x, m_NewHeroRespondCache.rotate_axis_y, m_NewHeroRespondCache.rotate_axis_z),
			syncState = false,
			ServerPoints = respond.monster_road_point,

			behaviorType = (uint)(respond.behavior_type),
			Offset = new Vector3(respond.bind_hero_offset.x, respond.bind_hero_offset.y, respond.bind_hero_offset.z),
			bindEntiryId = respond.bind_hero_id,

			bindBeRefresh = Convert.ToBoolean(respond.bind_be_refresh),
			bindBeImme = Convert.ToBoolean(respond.bind_be_imme)
		});
	}

	protected override void DoGUIOverride(Config config)
	{
		GUILayout.Label("GM:", config.LabelStyle);
		config.BeginToolbarHorizontal();
		if (IsMain()
			&& config.ToolbarButton(false, "Kill(Other)"))
		{
			NetworkManager.Instance.GetChatController().SendGM_OtherKillPlayer();
		}
		if (config.ToolbarButton(false, "Kill(Self)"))
		{
			NetworkManager.Instance.GetChatController().SendGM_KillPlayer(UId());
		}
		config.EndHorizontal();

		GUILayout.Label("State:", config.LabelStyle);
		config.BeginHorizontal();
		float rowWidth = 0;
		//foreach (ComposableState iterComposableState in Enum.GetValues(typeof(ComposableState)))
		//{
		//	GUIContent content = new GUIContent(iterComposableState.ToString());
		//	config.BoxStyle.CalcMinMaxWidth(content, out float minWidth, out float maxWidth);
		//	rowWidth += maxWidth;
		//	if (rowWidth >= config.PanelWidth * 0.64f)
		//	{
		//		rowWidth = 0;
		//		config.EndHorizontal();
		//		config.BeginHorizontal();
		//	}

		//	GUILayout.Box(content
		//		, HasState(iterComposableState)
		//			? config.ImportantBoxStyle
		//			: config.BoxStyle);
		//}
		config.EndHorizontal();
	}


#if UNITY_EDITOR
	private void OnDrawGizmos()
	{
		DispatchDrawGizmo();
	}
#endif

	#region Interface
	public uint GetUId()
	{
		return UId();
	}

	public S2C_SYNC_NEW_HERO GetNewHeroRespond()
	{
		return m_NewHeroRespondCache;
	}

	public void SetAttribute(AttributeName key, double value)
	{
		m_Attributes[key] = value;
	}

	/// <summary>
	/// 所有属性
	/// </summary>
	/// <returns></returns>
	/// 兼容之前接口
	/// 有些是客户端自己算的，有些是服务器发下来的，有些是单独拿武器的
	public double GetAttribute(AttributeName key)
	{
		double val = 0;
		switch (key)
		{
			case AttributeName.kHPMax:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kHPMax);
				break;
			//注掉的是读服务器发下来的
			//case AttributeName.kHP:
			//	val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kHP);
			//	break;
			case AttributeName.kShieldMax:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kShieldMax);
				break;
			//case AttributeName.kShieldValue:
			//	val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kShieldValue);
			//	break;
			case AttributeName.kPowerMax:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kPowerMax);
				break;
			//case AttributeName.kPowerValue:
			//	val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kPowerValue);
			//	break;
			case AttributeName.kCartridgeCapacity:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kCartridgeCapacity);
				break;
			case AttributeName.kCartridgeReloadTime:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kCartridgeReloadTime);
				break;
			case AttributeName.kCartridgeCost:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kCartridgeCost);
				break;
			case AttributeName.kGuidanceCapacity:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kGuidanceCapacity);
				break;
			case AttributeName.kGuidanceReloadTime:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kGuidanceReloadTime);
				break;
			case AttributeName.kGuidanceCost:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kGuidanceCost);
				break;
			case AttributeName.kLightHeatMax:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kLightHeatMax);
				break;
			case AttributeName.kLightHeatCDTime:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kLightHeatCDTime);
				break;
			case AttributeName.kLightHeatCDEfficiency:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kLightHeatCDEfficiency);
				break;
			case AttributeName.kLightHeatSafeValue:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kLightHeatSafeValue);
				break;
			case AttributeName.kLightHeatIncrementOnce:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kLightHeatIncrementOnce);
				break;
			//case AttributeName.kConverterMax:
			//	val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kConverterMax);
			//	break;
			case AttributeName.kConverterAccumulateEfficiency:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kConverterAccumulateEfficiency);
				break;
			case AttributeName.kConverterCostEfficiency:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kConverterCostEfficiency);
				break;
			//case AttributeName.kConverterValue:
			//	val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kConverterValue);
			//	break;

			/// 武器上的属性用 GetWeaponAttribute
			///case AttributeName.kLightHeatCodingTime:
			///	val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kLightHeatCodingTime);
			///	break;
			///case AttributeName.kLightHeatCastSpeed:
			///	val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kLightHeatCastSpeed);
			///	break;

			case AttributeName.kWeaponAccuracy:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kWeaponAccuracy);
				break;
			case AttributeName.kWeaponStability:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kWeaponStability);
				break;
			case AttributeName.kRoleLevel:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kRoleLevel);
				break;
			case AttributeName.kWarShipLevel:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kWarShipLevel);
				break;
			case AttributeName.kDuanLevel:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kDuanLevel);
				break;
			case AttributeName.kPickupDistance:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kPickupDistance);
				break;
			case AttributeName.kSpeedWalk:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kSpeedWalk);
				break;
			case AttributeName.kSpeedRun:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kSpeedRun);
				break;
			case AttributeName.kSpeedTurn:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kSpeedTurn);
				break;
			case AttributeName.kSpeedJump:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kSpeedJump);
				break;
			case AttributeName.kWatchpointY:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kWatchpointY);
				break;
			case AttributeName.kWatchpointX:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kWatchpointX);
				break;
			case AttributeName.kCameraNear:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kCameraNear);
				break;
			case AttributeName.kCameraFar:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kCameraFar);
				break;
			case AttributeName.kAttack:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kAttack);
				break;
			case AttributeName.kDefend:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kDefend);
				break;
			case AttributeName.kInvisible:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kInvisible);
				break;
			case AttributeName.kFrontSpeedMax:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kFrontSpeedMax);
				break;
			case AttributeName.kBackSpeedMax:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kBackSpeedMax);
				break;
			case AttributeName.kRightSpeedMax:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kRightSpeedMax);
				break;
			case AttributeName.kLeftSpeedMax:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kLeftSpeedMax);
				break;
			case AttributeName.kUpSpeedMax:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kUpSpeedMax);
				break;
			case AttributeName.kDownSpeedMax:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kDownSpeedMax);
				break;
			case AttributeName.kFrontSpeedAccelerate:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kFrontSpeedAccelerate);
				break;
			case AttributeName.kBackSpeedAccelerate:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kBackSpeedAccelerate);
				break;
			case AttributeName.kRightSpeedAccelerate:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kRightSpeedAccelerate);
				break;
			case AttributeName.kLeftSpeedAccelerate:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kLeftSpeedAccelerate);
				break;
			case AttributeName.kUpSpeedAccelerate:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kUpSpeedAccelerate);
				break;
			case AttributeName.kDownSpeedAccelerate:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kDownSpeedAccelerate);
				break;
			case AttributeName.kFrontBackSpeedDecelerate:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kFrontBackSpeedDecelerate);
				break;
			case AttributeName.kLeftRightSpeedDecelerate:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kLeftRightSpeedDecelerate);
				break;
			case AttributeName.kUpDownSpeedDecelerate:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kUpDownSpeedDecelerate);
				break;
			case AttributeName.kXAngleSpeedMax:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kXAngleSpeedMax);
				break;
			case AttributeName.kYAngleSpeedMax:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kYAngleSpeedMax);
				break;
			case AttributeName.kZAngleSpeedMax:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kZAngleSpeedMax);
				break;
			case AttributeName.kXAngleSpeedAccelerate:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kXAngleSpeedAccelerate);
				break;
			case AttributeName.kYAngleSpeedAccelerate:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kYAngleSpeedAccelerate);
				break;
			case AttributeName.kZAngleSpeedAccelerate:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kZAngleSpeedAccelerate);
				break;
			case AttributeName.kXAngleSpeedDecelerate:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kXAngleSpeedDecelerate);
				break;
			case AttributeName.kYAngleSpeedDecelerate:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kYAngleSpeedDecelerate);
				break;
			case AttributeName.kZAngleSpeedDecelerate:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kZAngleSpeedDecelerate);
				break;
			case AttributeName.kJumpSpeedBegine:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kJumpSpeedBegine);
				break;
			case AttributeName.kJumpSpeedAccelerate:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kJumpSpeedAccelerate);
				break;
			case AttributeName.kJumpSpeedDecelerate:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kJumpSpeedDecelerate);
				break;
			case AttributeName.kXMaxAngle:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kXMaxAngle);
				break;
			case AttributeName.kYMaxAngle:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kYMaxAngle);
				break;
			case AttributeName.kZMaxAngle:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kZMaxAngle);
				break;
			case AttributeName.kXMimicryMaxAngleAccelerate:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kXMimicryMaxAngleAccelerate);
				break;
			case AttributeName.kYMimicryMaxAngleAccelerate:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kYMimicryMaxAngleAccelerate);
				break;
			case AttributeName.kZMimicryMaxAngleAccelerate:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kZMimicryMaxAngleAccelerate);
				break;
			case AttributeName.kXMimicryMaxAngle:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kXMimicryMaxAngle);
				break;
			case AttributeName.kYMimicryMaxAngle:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kYMimicryMaxAngle);
				break;
			case AttributeName.kZMimicryMaxAngle:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kZMimicryMaxAngle);
				break;
			case AttributeName.kFightFrontSpeedMax:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kFightFrontSpeedMax);
				break;
			case AttributeName.kFightBackSpeedMax:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kFightBackSpeedMax);
				break;
			case AttributeName.kFightRightSpeedMax:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kFightRightSpeedMax);
				break;
			case AttributeName.kFightLeftSpeedMax:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kFightLeftSpeedMax);
				break;
			case AttributeName.kFightUpSpeedMax:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kFightUpSpeedMax);
				break;
			case AttributeName.kFightDownSpeedMax:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kFightDownSpeedMax);
				break;
			case AttributeName.kFightFrontSpeedAccelerate:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kFightFrontSpeedAccelerate);
				break;
			case AttributeName.kFightBackSpeedAccelerate:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kFightBackSpeedAccelerate);
				break;
			case AttributeName.kFightRightSpeedAccelerate:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kFightRightSpeedAccelerate);
				break;
			case AttributeName.kFightLeftSpeedAccelerate:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kFightLeftSpeedAccelerate);
				break;
			case AttributeName.kFightUpSpeedAccelerate:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kFightUpSpeedAccelerate);
				break;
			case AttributeName.kFightDownSpeedAccelerate:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kFightDownSpeedAccelerate);
				break;
			case AttributeName.kFightFrontBackSpeedDecelerate:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kFightFrontBackSpeedDecelerate);
				break;
			case AttributeName.kFightLeftRightSpeedDecelerate:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kFightLeftRightSpeedDecelerate);
				break;
			case AttributeName.kFightUpDownSpeedDecelerate:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kFightUpDownSpeedDecelerate);
				break;
			case AttributeName.kFightXMaxAngleSpeed:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kFightXMaxAngleSpeed);
				break;
			case AttributeName.kFightYMaxAngleSpeed:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kFightYMaxAngleSpeed);
				break;
			case AttributeName.kFightZMaxAngleSpeed:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kFightZMaxAngleSpeed);
				break;
			case AttributeName.kFightXAngleSpeedAccelerate:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kFightXAngleSpeedAccelerate);
				break;
			case AttributeName.kFightYAngleSpeedAccelerate:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kFightYAngleSpeedAccelerate);
				break;
			case AttributeName.kFightZAngleSpeedAccelerate:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kFightZAngleSpeedAccelerate);
				break;
			case AttributeName.kFightXAngleSpeedDecelerate:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kFightXAngleSpeedDecelerate);
				break;
			case AttributeName.kFightYAngleSpeedDecelerate:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kFightYAngleSpeedDecelerate);
				break;
			case AttributeName.kFightZAngleSpeedDecelerate:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kFightZAngleSpeedDecelerate);
				break;
			case AttributeName.kFightXMaxAngle:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kFightXMaxAngle);
				break;
			case AttributeName.kFightYMaxAngle:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kFightYMaxAngle);
				break;
			case AttributeName.kFightZMaxAngle:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kFightZMaxAngle);
				break;
			case AttributeName.kXFightMimicryAngleSpeedAccelerate:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kXFightMimicryAngleSpeedAccelerate);
				break;
			case AttributeName.kYFightMimicryAngleSpeedAccelerate:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kYFightMimicryAngleSpeedAccelerate);
				break;
			case AttributeName.kZFightMimicryAngleSpeedAccelerate:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kZFightMimicryAngleSpeedAccelerate);
				break;
			case AttributeName.kXFightMimicryAngle:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kXFightMimicryAngle);
				break;
			case AttributeName.kYFightMimicryAngle:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kYFightMimicryAngle);
				break;
			case AttributeName.kZFightMimicryAngle:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kZFightMimicryAngle);
				break;
			case AttributeName.kOverloadFrontSpeedMax:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kOverloadFrontSpeedMax);
				break;
			case AttributeName.kOverloadBackSpeedMax:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kOverloadBackSpeedMax);
				break;
			case AttributeName.kOverloadRightSpeedMax:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kOverloadRightSpeedMax);
				break;
			case AttributeName.kOverloadLeftSpeedMax:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kOverloadLeftSpeedMax);
				break;
			case AttributeName.kOverloadUpSpeedMax:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kOverloadUpSpeedMax);
				break;
			case AttributeName.kOverloadDownSpeedMax:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kOverloadDownSpeedMax);
				break;
			case AttributeName.kOverloadFrontSpeedAccelerate:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kOverloadFrontSpeedAccelerate);
				break;
			case AttributeName.kOverloadBackSpeedAccelerate:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kOverloadBackSpeedAccelerate);
				break;
			case AttributeName.kOverloadRightSpeedAccelerate:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kOverloadRightSpeedAccelerate);
				break;
			case AttributeName.kOverloadLeftSpeedAccelerate:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kOverloadLeftSpeedAccelerate);
				break;
			case AttributeName.kOverloadUpSpeedAccelerate:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kOverloadUpSpeedAccelerate);
				break;
			case AttributeName.kOverloadDownSpeedAccelerate:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kOverloadDownSpeedAccelerate);
				break;
			case AttributeName.kOverloadFrontBackSpeedDecelerate:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kOverloadFrontBackSpeedDecelerate);
				break;
			case AttributeName.kOverloadLeftRightSpeedDecelerate:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kOverloadLeftRightSpeedDecelerate);
				break;
			case AttributeName.kOverloadUpDownSpeedDecelerate:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kOverloadUpDownSpeedDecelerate);
				break;
			case AttributeName.kXOverloadAngleSpeedMax:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kXOverloadAngleSpeedMax);
				break;
			case AttributeName.kYOverloadAngleSpeedMax:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kYOverloadAngleSpeedMax);
				break;
			case AttributeName.kZOverloadAngleSpeedMax:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kZOverloadAngleSpeedMax);
				break;
			case AttributeName.kXOverloadAngleSpeedAccelerate:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kXOverloadAngleSpeedAccelerate);
				break;
			case AttributeName.kYOverloadAngleSpeedAccelerate:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kYOverloadAngleSpeedAccelerate);
				break;
			case AttributeName.kZOverloadAngleSpeedAccelerate:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kZOverloadAngleSpeedAccelerate);
				break;
			case AttributeName.kXOverloadAngleSpeedDecelerate:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kXOverloadAngleSpeedDecelerate);
				break;
			case AttributeName.kYOverloadAngleSpeedDecelerate:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kYOverloadAngleSpeedDecelerate);
				break;
			case AttributeName.kZOverloadAngleSpeedDecelerate:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kZOverloadAngleSpeedDecelerate);
				break;
			case AttributeName.kXOverloadMaxAngle:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kXOverloadMaxAngle);
				break;
			case AttributeName.kYOverloadMaxAngle:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kYOverloadMaxAngle);
				break;
			case AttributeName.kZOverloadMaxAngle:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kZOverloadMaxAngle);
				break;
			case AttributeName.kXOverloadMimicryAngleSpeedAccelerate:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kXOverloadMimicryAngleSpeedAccelerate);
				break;
			case AttributeName.kYOverloadMimicryAngleSpeedAccelerate:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kYOverloadMimicryAngleSpeedAccelerate);
				break;
			case AttributeName.kZOverloadMimicryAngleSpeedAccelerate:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kZOverloadMimicryAngleSpeedAccelerate);
				break;
			case AttributeName.kXOverloadMimicryMaxAngle:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kXOverloadMimicryMaxAngle);
				break;
			case AttributeName.kYOverloadMimicryMaxAngle:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kYOverloadMimicryMaxAngle);
				break;
			case AttributeName.kZOverloadMimicryMaxAngle:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kZOverloadMimicryMaxAngle);
				break;
			case AttributeName.kOverloadPowerCostEfficiency:
				val = GetPlayerAttribute(crucis.attributepipeline.attribenum.AttributeName.kOverloadPowerCostEfficiency);
				break;
			default:
				val = m_Attributes[key];
				break;
		}
		return val;
	}

	public string GetModelName()
	{
		return m_AssetName;
	}

	public Transform GetSkinTransform()
	{
		return m_SkinTransform;
	}

	public void SetSkinTransform(Transform transform)
	{
		m_SkinTransform = transform;
	}

	public Transform GetSkinRootTransform()
	{
		return m_SkinRootTransform;
	}

	public Vector3 GetServerBornPosition()
	{
		return m_BornServerPosition;
	}

	public Quaternion BornServerRotation()
	{
		return m_BornServerRotation;
	}

	public T AddComponent<T>() where T : Component
	{
		return gameObject.AddComponent<T>();
	}

	public Vector3 GetRotateAxis()
	{
		return m_RotateAxis;
	}

	public void SetRotateAxis(Vector3 vector)
	{
		m_RotateAxis = vector;
	}

	public Vector3 GetMouseDelta()
    {
        return m_MouseDelta;
    }

    public void SetMouseDelta(Vector3 vector)
    {
        m_MouseDelta = vector;
    }

    public Vector3 GetEngineAxis()
	{
		return m_EngineAxis;
	}

	public void SetEngineAxis(Vector3 vector)
	{
		m_EngineAxis = vector;
	}


	public Transform GetVirtualCameraTransform()
	{
		return m_VirtualCameraTransform != null ? m_VirtualCameraTransform : transform;
	}

	public void ChangeVirtualLocalPositionX(float x)
	{
		if (m_VirtualCameraTransform)
		{
			m_PosValue.x = x;
			m_VirtualCameraTransform.localPosition = m_PosValue;
		}
	}

	public Rigidbody GetRigidbody()
	{
		if (m_SyncTarget == null)
		{
			return null;
		}

		return m_SyncTarget.GetComponent<Rigidbody>();
	}

	public SpacecraftEntity GetTarget()
	{
		return m_Target;
	}

	public Collider GetTargetCollider()
	{
		return m_TargetCollider;
	}

	public void SetTarget(SpacecraftEntity target, Collider targetCollider)
	{
		if (m_Target != target || m_TargetCollider != targetCollider)
		{
			// 通知上一个目标Entity, 他已经不是目标了
			BeSelectedAsTarget selectedForTarget = new BeSelectedAsTarget();
			selectedForTarget.isSelected = false;
			if (m_Target != null && IsMain())
			{
				GameplayProxy gameplayProxy = GameFacade.Instance.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;
				bool targetIsEnemy = gameplayProxy.CanAttackToTarget(this, m_Target);
				if (targetIsEnemy)
					SendEventToEntity(m_Target.GetUId(), ComponentEventName.SpacecraftIsSelectedAsTarget, selectedForTarget);

				if (m_Target.GetHeroType() == KHeroType.htMine)
				{
					ulong ownerID = m_Target.m_EntityFatherOwnerID != 0 ? m_Target.m_EntityFatherOwnerID : m_Target.UId();
					List<SpacecraftEntity> mineList = m_GameplayProxy.GetAllEntitiesFromEntityGroup(ownerID);
					for (int iMine = 0; iMine < mineList.Count; iMine++)
					{
						SendEventToEntity(mineList[iMine].GetUId(), ComponentEventName.SpacecraftIsSelectedAsTarget, selectedForTarget);
					}
				}
			}

			if (target != null && (!target.IsDead() || target.GetHeroType() == KHeroType.htMine))
			{
				m_Target = target;
				m_TargetCollider = targetCollider;
			}
			else
			{
				m_Target = null;
			}

			ChangeTargetEvent eventInfo = new ChangeTargetEvent();
			eventInfo.newTarget = m_Target;
			SendEvent(ComponentEventName.ChangeTarget, eventInfo);

			// 通知新的目标Entity, 他变成目标了
			selectedForTarget.isSelected = true;
			if (m_Target != null && IsMain())
			{
				GameplayProxy gameplayProxy = GameFacade.Instance.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;
				bool targetIsEnemy = gameplayProxy.CanAttackToTarget(this, m_Target);
				if (targetIsEnemy)
					SendEventToEntity(m_Target.GetUId(), ComponentEventName.SpacecraftIsSelectedAsTarget, selectedForTarget);

				if (m_Target.GetHeroType() == KHeroType.htMine)
				{
					ulong ownerID = m_Target.m_EntityFatherOwnerID != 0 ? m_Target.m_EntityFatherOwnerID : m_Target.UId();
					List<SpacecraftEntity> mineList = m_GameplayProxy.GetAllEntitiesFromEntityGroup(ownerID);
					for (int iMine = 0; iMine < mineList.Count; iMine++)
					{
						SendEventToEntity(mineList[iMine].GetUId(), ComponentEventName.SpacecraftIsSelectedAsTarget, selectedForTarget);
					}
				}
			}
		}
	}

	/// <summary>
	/// 绝对不允许在这个返回的List内增删元素
	/// </summary>
	/// <returns></returns>
	public List<Collider> GetAllColliders()
	{
		return m_ShipColliders;
	}

	// TODO, 应该写一个IEnumertor, 遍历可用的所有Collider
	public void ResetLODColliders(List<Collider> colliders)
	{
		m_ShipColliders.Clear();
		if (colliders != null)
			m_ShipColliders.AddRange(colliders);

		m_ShipColliders.AddRange(m_RunTimeColliders);
	}

	/// <summary>
	/// 运行时因为特效等逻辑, 动态添加的Collider
	/// </summary>
	/// <param name="newCollider"></param>
	public void AddCollider_Runtime(Collider newCollider)
	{
		m_RunTimeColliders.Add(newCollider);
		m_ShipColliders.Add(newCollider);
	}

	/// <summary>
	/// 运行时因为移除带有Collider的特效等逻辑, 动态删除的Collider
	/// </summary>
	/// <param name="newCollider"></param>
	public void RemoveCollider_Runtime(Collider unusedCollider)
	{
		m_RunTimeColliders.Remove(unusedCollider);
		m_ShipColliders.Remove(unusedCollider);
		m_SpacecraftPresentation._RemoveCollider(unusedCollider);
	}

	public Npc GetNPCTemplateVO()
	{
		return m_NpcTmpVO;
	}

	public void SetPresentation(SpacecraftPresentation container)
	{
		m_SpacecraftPresentation = container;
	}

	public SpacecraftPresentation GetPresentation()
	{
		return m_SpacecraftPresentation;
	}

	public bool GetBurstReady()
	{
		return m_BurstReady;
	}

	public void SetBurstReady(bool value)
	{
		m_BurstReady = value;
	}

	public float GetOverloadProgress()
	{
		return m_OverloadProgress;
	}

	public void SetOverloadProgress(float value)
	{
		m_OverloadProgress = value;
	}

	public float GetFireCountdown()
	{
		return m_FireCountdown;
	}

	public void SetFireCountdown(float countdown)
	{
		m_FireCountdown = countdown;
	}

	public float GetUnderAttackWarningToneCountdown()
	{
		return m_UnderAttackWarningToneCountdown;
	}

	public void SetUnderAttackWarningToneCountdown(float countdown)
	{
		m_UnderAttackWarningToneCountdown = countdown;
	}

    public SpacecraftEntity GetOwner()
    {
        return this;
    }

    public void AddOnCollisionCallback(Action<Collision> enter, Action<Collision> stay, Action<Collision> exit)
	{
		if (m_SyncTarget == null)
		{
			return;
		}

		UnityCollisionEventProxy proxy = m_SyncTarget.GetComponent<UnityCollisionEventProxy>();
		if (proxy == null)
		{
			return;
		}

		proxy.AddOnCollisionCallback(enter, stay, exit);
	}

	public void RemoveCollisionCallback(Action<Collision> enter, Action<Collision> stay, Action<Collision> exit)
	{
		if (m_SyncTarget == null)
		{
			return;
		}

		UnityCollisionEventProxy proxy = m_SyncTarget.GetComponent<UnityCollisionEventProxy>();
		if (proxy == null)
		{
			return;
		}

		proxy.RemoveCollisionCallback(enter, stay, exit);
	}

	public void SetWeaponPower(ulong weaponUID, WeaponPowerVO powerVO)
	{
		m_WeaponPowers[weaponUID] = powerVO;
	}

	/// <summary>
	/// 指定位置武器的弹药信息
	/// 如果还没有存储这个信息会返回NULL
	/// </summary>
	/// <param name="index"></param>
	/// <returns></returns>
	public WeaponPowerVO GetWeaponPower(ulong weaponUID)
	{
		if (!m_WeaponPowers.ContainsKey(weaponUID))
			return null;
		else
			return m_WeaponPowers[weaponUID];
	}

	public int GetCurrentSkillID()
	{
		return m_CurrentSkillID;
	}

	public void SetCurrentSkillID(int skillID)
	{
		m_CurrentSkillID = skillID;
	}

	public SkillState GetCurrentSkillState()
	{
		return m_CurrentSkillState;
	}

	public void SetCurrentSkillState(SkillState skillState)
	{
		m_CurrentSkillState = skillState;
	}

	public bool IsReleasingTriggerSkill()
	{
		return m_ReleasingTriggerSkill;
	}

	public void SetReleasingTriggerSkill(bool releasingTriggerSkill)
	{
		m_ReleasingTriggerSkill = releasingTriggerSkill;
	}

	public int GetTriggerSkillID()
	{
		return m_TriggerSkillID;
	}

	public void SetTriggerSkillID(int skillID)
	{
		m_TriggerSkillID = skillID;
	}

	public void AddBuff(BuffVO buff)
	{
		m_BuffTable.Add(buff.ID, new Buff(buff, this));
		InitAttManager();
		GameFacade.Instance.SendNotification(NotificationName.MSG_BUFFERICON_CHANGE, this);//需要发送自己的信息
	}

	public Buff GetBuff(uint id)
	{
		if (m_BuffTable.ContainsKey(id))
			return m_BuffTable[id];
		else
			return null;
	}

	/// <summary>
	/// Mono的Dictionary的Values会产生GC. (我没试验, 查的)
	/// https://www.zhihu.com/question/58073029
	/// </summary>
	/// <returns></returns>
	public Dictionary<uint, Buff> GetAllBuffs()
	{
		return m_BuffTable;
	}

	public void RemoveBuff(uint id)
	{
		if (m_BuffTable.ContainsKey(id))
			m_BuffTable.Remove(id);
		InitAttManager();
		GameFacade.Instance.SendNotification(NotificationName.MSG_BUFFERICON_CHANGE, this);//需要发送自己的信息
	}

	public void SetPrestige(uint campType, uint prestige)
	{
		CampPrestigeMap[campType] = prestige;
	}

	/// <summary>
	/// 获取对某个阵营的声望值. 如果查找不到对应阵营的声望值则返回0
	/// </summary>
	/// <param name="campType"></param>
	/// <returns></returns>
	public uint GetPrestige(uint campType)
	{
		uint value = 0;
		CampPrestigeMap.TryGetValue(campType, out value);
		return value;
	}

	public bool GetCanBeAttack()
	{
		if (m_HeroType == KHeroType.htPlayer)
			return true;
		return m_NpcTmpVO.CanBeAttack;


	}

	public void SetCampID(uint campID)
	{
		m_CampID = campID;
		/// 处理守卫音效
		if (m_HeroType == KHeroType.htNormalChestGuard)
		{
			PlayVideoSoundEvent((int)m_CfgEternityProxy.GetGamingConfig(1).Value.Treasure.Value.Sound.Value.OrdinaryGuardRefresh);
		}
		else if (m_HeroType == KHeroType.htRareChestGuard)
		{
			PlayVideoSoundEvent((int)m_CfgEternityProxy.GetGamingConfig(1).Value.Treasure.Value.Sound.Value.EliteGuardRefresh);
		}
	}


	/// <summary>
	/// 设置封印状态
	/// </summary>
	/// <param name="seal"></param>
	public void SetSeal(byte seal)
	{
		if (m_IsSeal == 1 && seal == 0)
		{
			SendEvent(ComponentEventName.SealEnd, null);
		}
		m_IsSeal = seal;
	}

	/// <summary>
	/// 是否封印
	/// </summary>
	/// <returns></returns>
	public bool IsSeal()
	{
		return m_IsSeal == 1;
	}

	public void AddVFX(EffectController vfx)
	{
		m_VFXList.Add(vfx);
	}

	public void RemoveVFX(EffectController vfx)
	{
		m_VFXList.Remove(vfx);
	}

	public List<EffectController> GetAllVFXs()
	{
		return m_VFXList;
	}

	public void SetInvisible(bool invisible)
	{
		m_Invisible = invisible;
	}

	public bool IsInvisible()
	{
		return m_Invisible;
	}

	public float GetInteractableRadius()
	{
		return m_NpcTmpVO.InteractionVolume;
	}

	public float GetInteractableDistanceSquare()
	{
		return m_NpcTmpVO.TriggerRange * m_NpcTmpVO.TriggerRange;
	}

	public bool IsAlive()
	{
		return this != null;
	}

	public Vector3 GetWorldPosition()
	{
		return transform.position;
	}

	public void SetFocus(bool focus)
	{
		if (focus)
		{
			if (DropItemManager.Instance.CheckIsDropItem(GetUId()))
			{
				DropItemManager.Instance.AutoPickUp(GetUId());
			}
			else
			{
				MissionProxy missionProxy = GameFacade.Instance.RetrieveProxy(ProxyName.MissionProxy) as MissionProxy;
				if (missionProxy.GetAcceptTaskTidBy(GetTemplateID()) == null
					&& missionProxy.GetCanSubmitMissionBy(GetTemplateID()) == null
					&& missionProxy.GetTalkMissionTalkTidBy(GetTemplateID()) == null
					&& missionProxy.GetGoingMissionBy(GetTemplateID()) == null
					&& m_TeleportId == 0
					&& !m_NpcTmpVO.Function.HasValue)
				{
					return;
				}

				MsgInteractiveInfo msg = MessageSingleton.Get<MsgInteractiveInfo>();
				msg.Describe = null;
				if (missionProxy.GetCanSubmitMissionBy(GetTemplateID()) == null)
				{
					string fKeyText = missionProxy.GetTalkMissionFKeyText(GetTemplateID());
					if (!string.IsNullOrEmpty(fKeyText))
					{
						msg.Describe = fKeyText;
					}
				}

				msg.Tid = GetTemplateID();
				msg.MustUseHumanFBox = m_NpcTmpVO.Function.HasValue;
				GameFacade.Instance.SendNotification(NotificationName.MSG_INTERACTIVE_SHOWFLAG, msg);
			}

		}
		else
		{
			GameFacade.Instance.SendNotification(NotificationName.MSG_INTERACTIVE_HIDEFLAG);
			if (m_NpcTmpVO.DialogueTurn == 1)
				GetSkinTransform().DORotateQuaternion(m_BornServerRotation, 0.5f);
		}
	}

	public void SetDropItemUId(ulong id)
	{
		m_DropItemUid = id;
	}

	public void OnInteracted()
	{
		MissionProxy missionProxy = GameFacade.Instance.RetrieveProxy(ProxyName.MissionProxy) as MissionProxy;
		LookAtMainPlayerOnInteraction();
		if (m_TeleportId > 0)
		{
			//切地图
			NetworkManager.Instance.GetSceneController().SwitchMap((uint)m_TeleportId);
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
                UIManager.Instance.OpenPanel(panelName);
			}
			SetFocus(true);
		}
		else if (m_DropItemUid > 0 && DropItemManager.Instance.CheckIsDropItem((uint)m_DropItemUid)) //发现id不对
		{
			NetworkManager.Instance.GetDropItemController().OnSendPickUpProtocol(m_DropItemUid);
		}
		if (m_NpcTmpVO.Function.HasValue && m_NpcTmpVO.Function.Value.Arg2Length > 0)
		{
			SetFocus(true);
		}
	}

	private void LookAtMainPlayerOnInteraction()
	{
		//npc 转向对着人物
		if (m_NpcTmpVO.DialogueTurn == 1)
		{
			GameplayProxy gamePlayProxy = GameFacade.Instance.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;
			SpacecraftEntity mainPlayer = gamePlayProxy.GetEntityById<SpacecraftEntity>(gamePlayProxy.GetMainPlayerUID());

			Transform mptf = (mainPlayer != null) ? mainPlayer.GetSkinTransform() : null;

			if (mptf)
				GetSkinTransform().DOLookAt(new Vector3(mptf.position.x, GetSkinTransform().position.y, mptf.position.z), 0.5f).SetAutoKill(true);

		}
	}

	public string GetDisplay()
	{
		return gameObject.name;
	}


	public void ResetBattleWeapons(Dictionary<ulong, BattleWeaponBase> weapons)
	{
		if (m_BattleWeapons != null)
		{
			foreach (var item in m_BattleWeapons)
			{
				item.Value.OnRelease();
			}
		}
		m_BattleWeapons = weapons;
	}

    /// <summary>
    /// 获取武器&准星对象
    /// </summary>
    /// <param name="uid"></param>
    /// <returns></returns>
    public WeaponAndCrossSight GetWeaponAndCrossSight(ulong uid)
    {
        if (HaveWeaponAndCrossSight(uid))
            return m_WeaponAndCrossSightDic[uid];
        else
            return null;
    }

    /// <summary>
    /// 删除转化炉的武器对象，（只有一个）
    /// </summary>
    public void DeleReformerWeaponAndCrossSight()
    {
        if (m_WeaponAndCrossSightDic != null)
        {
            List<WeaponAndCrossSight> dele = new List<WeaponAndCrossSight>();
            foreach (var item in m_WeaponAndCrossSightDic)
            {
                if (!item.Value.IsOrdinary())
                {
                    dele.Add(item.Value);
                }
            }
            for (int i = 0; i < dele.Count; i++)
            {
                m_WeaponAndCrossSightDic.Remove(dele[i].GetUId());
            }
        }
    }



    /// <summary>
    /// 清除所有的武器&准星 对象
    /// </summary>
    public void ClearAllWeaponAndCrossSight()
    {
        if (m_WeaponAndCrossSightDic != null)
        {
            foreach (var item in m_WeaponAndCrossSightDic)
            {
                if (item.Value != null)
                    item.Value.OnRelease();
            }
            m_WeaponAndCrossSightDic.Clear();
        }
    }

    /// <summary>
    /// 是否包含 武器&准星 对象
    /// </summary>
    /// <param name="uid"></param>
    /// <returns></returns>
    public bool HaveWeaponAndCrossSight(ulong uid)
    {
        return m_WeaponAndCrossSightDic.ContainsKey(uid);
    }

    /// <summary>
    /// 增加武器&准星 对象
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="battleWeapon"></param>
    public void AddWeaponAndCrossSight(ulong uid, WeaponAndCrossSight Weapon)
    {
        m_WeaponAndCrossSightDic[uid] = Weapon;
    }

	public BattleWeaponBase GetBattleWeapon(ulong uid)
	{
		if (HaveBattleWeapon(uid))
			return m_BattleWeapons[uid];
		else
			return null;
	}

	public Dictionary<ulong, BattleWeaponBase>.ValueCollection GetAllBattleWeapons()
	{
		return m_BattleWeapons.Values;
	}

	public void ClearBattleWeapon()
	{
		if (m_BattleWeapons != null)
		{
			foreach (var item in m_BattleWeapons)
			{
				item.Value.OnRelease();
			}
			m_BattleWeapons.Clear();
		}
	}

	public bool HaveBattleWeapon(ulong uid)
	{
		return m_BattleWeapons.ContainsKey(uid);
	}

	public void AddBattleWeapon(ulong uid, BattleWeaponBase battleWeapon)
	{
		m_BattleWeapons[uid] = battleWeapon;
	}

	public void SetMotionType(MotionType motionType)
	{
		m_MotionType = motionType;
	}

	public MotionType GetMotionType()
	{
		return m_MotionType;
	}

	public Transform GetSyncTarget()
	{
		if (m_SyncTarget == null)
		{
			return null;
		}

		return m_SyncTarget.transform;
	}

	public void SetSynceColliderEnable(bool isEnable)
	{
		Transform syncTarget = GetSyncTarget();
		if (syncTarget != null)
		{
			foreach (var item in syncTarget.GetComponentsInChildren<Collider>())
			{
				item.enabled = isEnable;
			}
		}
	}

	public void SetCanToggleWeapon(bool can)
	{
		m_CanToggleWeapon = can;
	}

	public bool GetCanToggleWeapon()
	{
		return m_CanToggleWeapon;
	}

	public Vector2 GetDefaultCMAxisValue()
	{
		return m_DefaultCMAxisValue;
	}

	public void SetLODLevel(int LODLevel)
	{
		m_LODLevel = LODLevel;
	}

	public int GetLODLevel()
	{
		return m_LODLevel;
	}

	public void SetSkinVisiable(bool isShow)
	{
		if (GetPresentation() != null)
		{
			GetPresentation().SetVisibilityOfEntireSpacecraft(isShow);
		}
	}

    public void SetSkinVisiableForce(bool isShow)
    {
        if (GetPresentation() != null)
        {
            GetPresentation().SetVisibilityOfEntireSpacecraft(isShow, true);
        }
    }

	Vector3 ITrackObject.GetPosition()
	{
		return transform.position;
	}

	Quaternion ITrackObject.GetRotation()
	{
		return transform.rotation;
	}

	string ITrackObject.GetUserData()
	{
		return string.Empty;
	}
	#endregion
}