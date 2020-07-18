using Assets.Scripts.Define;
using Eternity.FlatBuffer;
using PureMVC.Interfaces;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HudTargetInfoPanel : HudBase
{
	private const string ASSET_ADDRESS = Leyoutech.Core.Loader.Config.AssetAddressKey.PRELOADUI_HUD_TARGETINFOPANEL;
	/// <summary>
	/// 画布相机
	/// </summary>
	private Camera m_Camera;
	/// <summary>
	/// 根节点
	/// </summary>
	private RectTransform m_Root;
	/// <summary>
	/// 敌方模板
	/// </summary>
	private RectTransform m_EnemyTemplate;
	/// <summary>
	/// 召唤怪物模板
	/// </summary>
	private RectTransform m_UAVTemplate;
	/// <summary>
	/// 敌方Boss模板
	/// </summary>
	private RectTransform m_BossTemplate;
	/// <summary>
	/// 友方模板
	/// </summary>
	private RectTransform m_FriendTemplate;
	/// <summary>
	/// Npc模板
	/// </summary>
	private RectTransform m_NpcTemplate;
	/// <summary>
	/// 矿物模板
	/// </summary>
	private RectTransform m_MineralTemplate;
	/// <summary>
	/// 检测器模板
	/// </summary>
	private RectTransform m_DetectorTemplate;
	/// <summary>
	/// 护送的npc
	/// </summary>
	private RectTransform m_EscortTemplate;
	/// <summary>
	/// 模板回收池
	/// </summary>
	private Dictionary<Transform, Transform> m_TemplateToPool = new Dictionary<Transform, Transform>();

	/// <summary>
	/// 已经建立的面板
	/// </summary>
	private Dictionary<uint, EntityView> m_UIDToRectTranform1 = new Dictionary<uint, EntityView>();
	/// <summary>
	/// 已经建立的面板
	/// </summary>
	private Dictionary<uint, EntityView> m_UIDToRectTranform2 = new Dictionary<uint, EntityView>();

	/// <summary>
	/// 外部引用
	/// </summary>
	private CfgEternityProxy m_CfgEternityProxy;
	/// <summary>
	/// 鼠标是否已移动
	/// </summary>
	private bool m_MouseMoved;

	/// <summary>
	/// 数据
	/// </summary>
	private CfgSkillSystemProxy m_SkillProxy;

	/// <summary>
	///队伍数据 
	/// </summary>
	private TeamProxy m_TeamProxy;

	/// <summary>
	/// 射线数据
	/// </summary>
	private RaycastProxy m_RaycastProxy;
    /// <summary>
    /// 主角
    /// </summary>
    private SpacecraftEntity m_MainEntity;
    /// <summary>
    /// GameplayProxy
    /// </summary>
    private GameplayProxy m_GameplayProxy;
    /// <summary>
    /// 临时变量记录最后一次血条段数
    /// </summary>
    private int m_OldPrivew = -1;
    /// <summary>
	/// 目标血条LineGrid
	/// </summary>
	private Transform m_LineGrid;
    /// <summary>
    /// 目标血条
    /// </summary>
    private Image m_BloodImage;

	private bool m_RunIconBoxActive = false;

	private struct EntityView
	{
		/// <summary>
		/// 实体ID
		/// </summary>
		public uint uid;
		/// <summary>
		/// 关联的UI
		/// </summary>
		public RectTransform view;
		/// <summary>
		/// 对应的UI池
		/// </summary>
		public Transform viewPool;
	}

	public HudTargetInfoPanel() : base(UIPanel.HudTargetInfoPanel, ASSET_ADDRESS, PanelType.Hud) { }

	public override void Initialize()
	{
		m_SkillProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgSkillSystemProxy) as CfgSkillSystemProxy;
		m_TeamProxy = Facade.RetrieveProxy(ProxyName.CfgSkillSystemProxy) as TeamProxy;
		m_RaycastProxy = Facade.RetrieveProxy(ProxyName.RaycastProxy) as RaycastProxy;
        m_GameplayProxy = Facade.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;
        m_Root = FindComponent<RectTransform>("Content");
		m_Camera = m_Root.GetComponentInParent<Canvas>().worldCamera;
		m_EnemyTemplate = FindComponent<RectTransform>("Templates/Slider_enemy");
		m_UAVTemplate = FindComponent<RectTransform>("Templates/Slider_UAV");
		m_BossTemplate = FindComponent<RectTransform>("Templates/Slider_enemyBoss");
		m_FriendTemplate = FindComponent<RectTransform>("Templates/Slider_friend");
		m_NpcTemplate = FindComponent<RectTransform>("Templates/Slider_npc");
		m_MineralTemplate = FindComponent<RectTransform>("Templates/Slider_mineral");
		m_DetectorTemplate = FindComponent<RectTransform>("Templates/Slider_npc1");
		m_EscortTemplate = FindComponent<RectTransform>("Templates/Slider_escort");
	}

	public override void OnShow(object msg)
	{
		base.OnShow(msg);

		RectTransform poolBox = FindComponent<RectTransform>("TemplatePool");
		if (poolBox == null)
		{
			poolBox = new GameObject("TemplatePool", typeof(RectTransform)).GetComponent<RectTransform>();
			poolBox.SetParent(GetTransform());
			poolBox.gameObject.SetActive(false);
			poolBox.SetAsFirstSibling();
		}

		m_TemplateToPool.Clear();

		RectTransform templates = FindComponent<RectTransform>("Templates");
		for (int i = 0; i < templates.childCount; i++)
		{
			Transform template = templates.GetChild(i);
			Transform templatePool = poolBox.Find(template.name);
			if (!templatePool)
			{
				templatePool = new GameObject(template.name, typeof(RectTransform)).GetComponent<RectTransform>();
				templatePool.SetParent(poolBox);
			}
			m_TemplateToPool.Add(template, templatePool);
		}

		m_CfgEternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;

		AddHotKey(HotKeyMapID.SHIP, HotKeyID.ShipCamera, OnCameraChanged);

		StartUpdate();
	}

	public override void OnHide(object msg)
	{
		//回收所有视图
		foreach (EntityView info in m_UIDToRectTranform1.Values)
		{
			info.view.SetParent(info.viewPool);
		}
		m_UIDToRectTranform1.Clear();

		foreach (EntityView info in m_UIDToRectTranform2.Values)
		{
			info.view.SetParent(info.viewPool);
		}
		m_UIDToRectTranform2.Clear();

		base.OnHide(msg);
	}

    public override NotificationName[] ListNotificationInterests()
    {
        return new NotificationName[]
        {
            NotificationName.MSG_BUFFERICON_CHANGE,
           
        };
    }

    public override void HandleNotification(INotification notification)
    {
        switch ((NotificationName)notification.Name)
        {
            case NotificationName.MSG_BUFFERICON_CHANGE:
                OnAddBufferIcon((SpacecraftEntity)(notification.Body));
                break;
			case NotificationName.MSG_CHANGE_BATTLE_STATE:
				OnShowTargetHudComponent((SpacecraftEntity)(notification.Body));
				break;
			default:
                break;
        }
    }

	public void OnShowTargetHudComponent(SpacecraftEntity entity)
	{
		m_RunIconBoxActive = entity.GetCurrentState().IsHasSubState(EnumSubState.BackToanchor);
	}

	/// <summary>
	/// 操作相机时
	/// </summary>
	/// <param name="callback"></param>
	private void OnCameraChanged(HotkeyCallback callback)
	{
		m_MouseMoved = true;
	}

	/// <summary>
	/// 更新视图
	/// </summary>
	protected override void Update()
	{
		if (!IsDead() && !IsWatchOrUIInputMode() && !IsLeaping())
		{
			//GameplayProxy sceneProxy = Facade.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;
			//SpacecraftEntity main = sceneProxy.GetEntityById<SpacecraftEntity>(sceneProxy.GetMainPlayerUID());
            if (m_MainEntity==null)
            {
                m_MainEntity= m_GameplayProxy.GetEntityById<SpacecraftEntity>(m_GameplayProxy.GetMainPlayerUID()); 
            }
            if (!m_MainEntity)
                return;
            List<SpacecraftEntity> entitys = m_GameplayProxy.GetEntities<SpacecraftEntity>();

			//按距离排序
			entitys.Sort((a, b) =>
			{
				Vector3 distanceA = a.transform.position - m_MainEntity.transform.position;
				Vector3 distanceB = b.transform.position - m_MainEntity.transform.position;
				return (int)(distanceB.sqrMagnitude - distanceA.sqrMagnitude);
			});

            bool isInSpace = IsInSpace();
            for (int i = 0; i < entitys.Count; i++)
			{
				SpacecraftEntity entity = entitys[i];
				KHeroType heroType = entity.GetHeroType();
				Vector3 titleOffset = Vector3.zero;
                float visibilityDistance = GameConstant.DEFAULT_VISIBILITY_METRE_FOR_SHIP;

                //忽略自已
                // if (entity == main)
                //    continue;

                //忽略不需要显示的NPC
                if (heroType != KHeroType.htPlayer)
				{
					Npc entityVO = m_CfgEternityProxy.GetNpcByKey(entity.GetTemplateID());
					if (entityVO.Display == 0)
						continue;

					if (entityVO.HeroHeaderLength >= 3)
						titleOffset = new Vector3(entityVO.HeroHeader(0), entityVO.HeroHeader(1), entityVO.HeroHeader(2));

                    visibilityDistance = entityVO.MissionTargetHiddenDistance * (isInSpace ? GameConstant.METRE_PER_UNIT : 1);
                }

                //忽略死亡的( 除了矿石 )
                if (heroType != KHeroType.htMine && heroType != KHeroType.htPreicous && (entity.GetAttribute(AttributeName.kHP) <= 0 || entity.GetCurrentState().GetMainState() == EnumMainState.Dead))
                    continue;
				if (entity.m_EntityFatherOwnerID > 0 && heroType == KHeroType.htMine)
					continue;
				//忽略不支持的
				RectTransform template = GetTemplate(m_MainEntity, entity);
				if (!template)
					continue;

				//忽略过远的
				float distance = (entity.transform.position - m_MainEntity.transform.position).magnitude;
				if (entity.GetHeroType() == KHeroType.htMine && distance > entity.GetNPCTemplateVO().TriggerRange)
					continue;
				distance = distance * GameConstant.METRE_PER_UNIT;
				if (distance >= visibilityDistance)
					continue;

				//忽略屏幕外的
				Vector3 screenPosition = Camera.main.WorldToScreenPoint(entity.transform.TransformPoint(titleOffset));
				if (!(screenPosition.z > Camera.main.nearClipPlane && screenPosition.x >= 0 && screenPosition.x <= Camera.main.pixelWidth && screenPosition.y > 0 && screenPosition.y <= Camera.main.pixelHeight))
					continue;

				Vector2 anchoredPosition;
				if (RectTransformUtility.ScreenPointToLocalPointInRectangle(m_Root, screenPosition, m_Camera, out anchoredPosition))
				{
					Transform templatePool = m_TemplateToPool[template];

					uint uid = entity.GetUId();
                    bool isNew = false;

					EntityView info;
					if (m_UIDToRectTranform1.ContainsKey(uid))
					{
						info = m_UIDToRectTranform1[uid];
						m_UIDToRectTranform1.Remove(uid);
						m_UIDToRectTranform2[uid] = info;
					}
					else
					{
						RectTransform view = templatePool.childCount > 0 ? templatePool.GetChild(0).GetComponent<RectTransform>() : Object.Instantiate(template, m_Root);
						view.SetParent(m_Root);
						view.transform.SetAsLastSibling();
						view.gameObject.SetActive(true);

						info = new EntityView() { uid = uid, view = view, viewPool = templatePool };
						m_UIDToRectTranform2[uid] = info;
                        isNew = true;


                        OnTargetCreate(entity, view);
					}

					OnTargetUpdate(m_MainEntity, entity, info.view, anchoredPosition, distance, visibilityDistance, m_MouseMoved, isNew);
				}
			}
		}

		m_MouseMoved = false;

        //回收无效的
        foreach (EntityView info in m_UIDToRectTranform1.Values)
		{
			info.view.SetParent(info.viewPool);
		}
		m_UIDToRectTranform1.Clear();

		Dictionary<uint, EntityView> tmp = m_UIDToRectTranform1;
		m_UIDToRectTranform1 = m_UIDToRectTranform2;
		m_UIDToRectTranform2 = tmp;
	}

	/// <summary>
	/// 获取模板
	/// </summary>
	/// <param name="main">主角</param>
	/// <param name="target">目标</param>
	/// <returns>模板</returns>
	private RectTransform GetTemplate(SpacecraftEntity main, SpacecraftEntity target)
	{
		switch (target.GetHeroType())
		{
			case KHeroType.htMonster:
				return m_CfgEternityProxy.GetRelation(main.GetCampID(), target.GetCampID()) == CampRelation.Enemy ? m_EnemyTemplate : m_UAVTemplate;
			case KHeroType.htEliteMonster2:
				return m_BossTemplate;
			case KHeroType.htPlayer:
				return m_FriendTemplate;
			case KHeroType.htNpc:
				return m_NpcTemplate;
			case KHeroType.htMine:
				return m_MineralTemplate;
			case KHeroType.htPreicous:
			case KHeroType.htDetector:
				return m_DetectorTemplate;
			case KHeroType.htRareChestGuard:
				return m_BossTemplate;
			case KHeroType.htNormalChestGuard:
				return m_EnemyTemplate;
			case KHeroType.htDisturbor:
				return m_EnemyTemplate;
			case KHeroType.htBeEscortedNpc:
				return m_EscortTemplate;
			default:
				Debug.Log(target + "===类型不对==" + target.GetHeroType());
				return m_EnemyTemplate;
		}
		return null;
	}
   
    /// <summary>
    /// 创建时
    /// </summary>
    private void OnTargetCreate(SpacecraftEntity target, RectTransform view)
	{
		KHeroType entityType = target.GetHeroType();

		if (entityType == KHeroType.htMine)
		{
			TMP_Text nameField = view.Find("Name/Name/Label_Name").GetComponent<TMP_Text>();
			nameField.text = entityType == KHeroType.htPlayer ? target.GetName() : TableUtil.GetNpcName(target.GetTemplateID());

            //血段
            m_LineGrid = FindComponent<Transform>(view, "Slider/Image_Bar/LineGrid");

            int bloodVolumeLength = m_CfgEternityProxy.GetDoppingBloodVolumeLengthByKey((uint)target.GetTemplateID()) - 1;
            if (m_OldPrivew != bloodVolumeLength)
            {
                m_OldPrivew = bloodVolumeLength;
                while (m_LineGrid.childCount - 2 < bloodVolumeLength)
                {
                    Transform lineItem = Object.Instantiate(m_LineGrid.GetChild(1), m_LineGrid);
                    lineItem.gameObject.SetActive(true);
                }
                for (int i = bloodVolumeLength + 2; i < m_LineGrid.childCount; i++)
                {
                    m_LineGrid.GetChild(i).gameObject.SetActive(false);
                }
            }
        }
		else
		{
			//目标名称
			TMP_Text nameField = view.Find("Name/Name").GetComponent<TMP_Text>();
			nameField.text = entityType == KHeroType.htPlayer ? target.GetName() : TableUtil.GetNpcName(target.GetTemplateID());
           
            //目标图标
            Image image = view.Find("Name/Icon/ImageIcon").GetComponent<Image>();
			image.gameObject.SetActive(entityType != KHeroType.htPlayer);
			if (image.gameObject.activeSelf)
			{
				Npc entityVO = m_CfgEternityProxy.GetNpcByKey(target.GetTemplateID());
				if (entityVO.NameBoardIcon == 0)
				{
					image.color = Color.clear;
				}
				else
				{
					image.color = Color.white;
					UIUtil.SetIconImage(image, entityVO.NameBoardIcon);
				}
			}
		}
	}

	/// <summary>
	/// 更新目标
	/// </summary>
	/// <param name="main">主角</param>
	/// <param name="target">目标</param>
	/// <param name="view">UI</param>
	/// <param name="anchoredPosition">坐标</param>
	/// <param name="distance">距离</param>
	private void OnTargetUpdate(SpacecraftEntity main, SpacecraftEntity target, RectTransform view, Vector3 anchoredPosition, float distance,float visibleDistance, bool mouseMoved, bool IsNew)
	{
		TaskTrackingProxy taskTrackingProxy = Facade.RetrieveProxy(ProxyName.TaskTrackingProxy) as TaskTrackingProxy;

		float hp = (float)target.GetAttribute(AttributeName.kHP);
		float hpMax = (float)target.GetAttribute(AttributeName.kHPMax);
		float mp = (float)target.GetAttribute(AttributeName.kShieldValue);
		float mpMax = (float)target.GetAttribute(AttributeName.kShieldMax);

		float hpProgress = hpMax <= 0 ? 0.0f : hp / hpMax;
		float mpProgress = mpMax <= 0 ? 0.0f : mp / mpMax;

		//坐标
		view.anchoredPosition = anchoredPosition;

		if (target.GetHeroType() == KHeroType.htMine)
		{
            m_BloodImage = FindComponent<Image>(view, "Slider/Image_Bar");
            m_BloodImage.fillAmount = hpProgress;
            TMP_Text hpText = FindComponent<TMP_Text>(view, "Name/Name/Label_Value");//血值比率
            if (hpText)
			{
				int textID = 1025;
				if (hpProgress <= 0.0f)
					textID = 1021;
				else if (hpProgress <= 0.2f)
					textID = 1022;
				else if (hpProgress <= 0.4f)
					textID = 1023;
				else if (hpProgress <= 0.6f)
					textID = 1024;

				hpText.text = GetLocalization("hud_text_id_" + textID);
			}

			//动画
			Animator animator = view.GetComponent<Animator>();
			if (animator)
			{
				SpacecraftEntity currentTarget = m_RaycastProxy.Raycast();

				animator.SetBool("isBattle", IsBattling());
				animator.SetBool("isTarget", currentTarget == target);
				if (mouseMoved)
					animator.SetTrigger("mouseMoved");
			}
		}
		else
		{
			//血值
			Slider mpSlider = FindComponent<Slider>(view, "Slider/Slider_MP");
			if (mpSlider)
			{
				mpSlider.value = mpProgress;
			}

			//护甲
			Slider hpSlider = FindComponent<Slider>(view, "Slider/Slider_Hp");
			if (hpSlider)
			{
				hpSlider.value = hpProgress;
			}

			//距离
			TMP_Text distanceField = FindComponent<TMP_Text>(view, "Name/Distance");
			if (distanceField)
			{
				distanceField.text = FormatMetre(distance);
			}

			//任务
			RectTransform missionBox = FindComponent<RectTransform>(view, "MissionIconBox");
			if (missionBox)
            {
                bool needShowMissionFlag = distance < visibleDistance;
                if (needShowMissionFlag && !taskTrackingProxy.GetAreadyAddMissionInfo(target.GetUId(), target.GetTemplateID()))
                {
                    TaskTrackingProxy.TrackingInfo tracking = taskTrackingProxy.GetNpcMission(target.GetUId(), target.GetTemplateID());
                    MissionType missionType = tracking != null ? tracking.MissionType : MissionType.None;

					missionBox.gameObject.SetActive(missionType != MissionType.None);
					if (missionBox.gameObject.activeSelf)
					{
                        Animator missionAniamtor = FindComponent<Animator>(missionBox, "IconMissionElement");
						Image missionIcon = FindComponent<Image>(missionAniamtor, "Icon");

						UIUtil.SetIconImage(missionIcon, GameConstant.FUNCTION_ICON_ATLAS_ASSETADDRESS, GetMissionIcon(missionType));

                        missionAniamtor.SetBool("Finished", tracking != null && tracking.MissionState == MissionState.Finished);
                    }
				}
				else
				{
					missionBox.gameObject.SetActive(false);
				}
			}

			//逃跑
			Transform escapeTransform = FindComponent<Transform>(view, "RunIconBox ");
			if (escapeTransform != null && escapeTransform.gameObject.activeSelf != m_RunIconBoxActive)
			{
				escapeTransform.gameObject.SetActive(m_RunIconBoxActive);
			}

			//动画
			Animator animator = view.GetComponent<Animator>();
			Transform buffIconBoxTras = view.Find("BuffIconBox");
			RectTransform bufferIconBox = null;
			if (buffIconBoxTras)
			{
				bufferIconBox = buffIconBoxTras.GetComponent<RectTransform>();
			}
			if (animator)
			{
				if (distance < GameConstant.DEFAULT_VISIBILITY_METRE_FOR_SHIP / 2)
				{
					if (main.GetTarget() == target)
					{
						if (bufferIconBox) { bufferIconBox.localPosition = m_NormalBufferPos; }
						animator.SetInteger("State", 2);
					}
					else
					{
						if (bufferIconBox) { bufferIconBox.localPosition = m_NormalBufferPos; }
						animator.SetInteger("State", 1);
					}
				}
				else
				{
					if (bufferIconBox) { bufferIconBox.localPosition = m_AwayFromBufferPos; }
					animator.SetInteger("State", 0);
				}

				if (target == main)
				{
					if (bufferIconBox) { bufferIconBox.localPosition = m_SelfBufferPos; }
					animator.SetInteger("State", 0);
					view.Find("Icon").gameObject.SetActive(false);
				}
            }

            if(IsNew)
            {
                OnAddBufferIcon(target);
            }
        }
    }
	#region 临时坐标变量
	/// <summary>
	/// 自己buffer坐标
	/// </summary>
	Vector3 m_SelfBufferPos = new Vector3(0, 250, 0);
	/// <summary>
	/// 远离buffer坐标
	/// </summary>
	Vector3 m_AwayFromBufferPos = new Vector3(0, 120, 0);
	/// <summary>
	/// 正常buffer坐标
	/// </summary>
	Vector3 m_NormalBufferPos = new Vector3(0, 165, 0);

	#endregion

	/// <summary>
	/// 显示BuffIcon
	/// </summary>
	public void OnAddBufferIcon(SpacecraftEntity target)
	{
        RectTransform view = null;
        if (m_UIDToRectTranform1.ContainsKey(target.GetUId()))
        {
            view = m_UIDToRectTranform1[target.GetUId()].view;
        }
        else
        {
            if (m_UIDToRectTranform2.ContainsKey(target.GetUId()))
            {
                view = m_UIDToRectTranform2[target.GetUId()].view;
            }
            else
            {
                return;
            }
        }
            
        Transform bufferIconBox = view.Find("BuffIconBox");
		if (bufferIconBox == null)
		{
			return;
		}
		bufferIconBox.gameObject.SetActive(true);
		Transform iconTemplate = bufferIconBox.GetChild(0);
		iconTemplate.gameObject.SetActive(false);
		int index = 0;
        
		if (target.GetAllBuffs().Count > 0)
		{
			foreach (KeyValuePair<uint, Buff> buffID2Value in target.GetAllBuffs())
			{
				SkillBuff configVO = m_SkillProxy.GetBuff((int)buffID2Value.Key);
               

                Transform iconTras;
				if (configVO.ByteBuffer != null)
				{
                    if ((BufferType)configVO.BuffHudShow == BufferType.None)
                    {
                        continue;
                    }
                   // Debug.LogError(target.GetName() + "==============" + configVO.Id + "========"
                   //+ target.GetAllBuffs().Count + "=======" + (BufferType)configVO.BuffHudShow);
                    index++;
					if (bufferIconBox.childCount >= index)
					{
						iconTras = bufferIconBox.GetChild(index - 1);
					}
					else
					{
						iconTras = Object.Instantiate(iconTemplate);
						iconTras.SetParent(bufferIconBox);
						iconTras.localScale = Vector3.one;
						iconTras.localPosition = Vector3.zero;
						iconTras.SetAsLastSibling();
					}
					bufferIconBox.localScale = Vector3.one;
					if (m_MainEntity == target)
					{
						bufferIconBox.localScale = Vector3.one * 1.5f;
					}
                    if ((uint)configVO.BuffHudIcon > 0)
                        UIUtil.SetIconImage(iconTras.GetComponent<Image>(), (uint)configVO.BuffHudIcon);
                    else
                        Debug.LogWarning("报错"+ configVO.Id);
					//Debug.Log(configVO.BuffHudIcon + "-=-=-=-=-=-" + configVO.BuffHudShow);
					switch ((BufferType)configVO.BuffHudShow)
					{
						case BufferType.All://   1 全体
							iconTras.gameObject.SetActive(true);
							break;
						case BufferType.Friend://	2 友方

							if (m_TeamProxy.GetMember(target.GetPlayerId()) != null)
							{
								iconTras.gameObject.SetActive(true);
							}
							break;
						case BufferType.Enemy://   3 敌方
							if (m_MainEntity != target && m_TeamProxy.GetMember(target.GetPlayerId()) == null)
							{
								iconTras.gameObject.SetActive(true);
							}
							break;
						case BufferType.Self://   4 自己
							if (m_MainEntity = target)
							{
								iconTras.gameObject.SetActive(true);
							}
							break;
						default:
							break;
					}
				}
			}
           // Debug.Log(index);
			for (int i = index ; i < bufferIconBox.childCount; i++)
			{
				bufferIconBox.GetChild(i).gameObject.SetActive(false);
			}
		}
		else
		{
			for (int i = 0; i < bufferIconBox.childCount; i++)
			{
				bufferIconBox.GetChild(i).gameObject.SetActive(false);
			}
		}

	}


	/// <summary>
	/// 格式化单位米
	/// </summary>
	/// <param name="metre">米数</param>
	/// <returns>text</returns>
	private string FormatMetre(float metre)
	{
		return metre < 1000 ? string.Format("[{0:N1}m]", metre) : string.Format("[{0:N1}km]", metre / 1000.0f);
	}
    /// <summary>
    /// buffer 类型
    /// </summary>
	public enum BufferType
	{
        None=0,//不显示
		All = 1,//全体
		Friend,//队友
		Enemy,//敌人
        Self,//自己
    }

}
