using Assets.Scripts.Define;
using Leyoutech.Core.Effect;
using Leyoutech.Core.Loader.Config;
using Game.VFXController;
using PureMVC.Interfaces;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Effect3DViewer;
using Eternity.FlatBuffer;

public class HudMineralHologram : HudBase
{
    private const string ASSET_ADDRESS = AssetAddressKey.PRELOADUI_HUD_MINERALHOLOGRAM;

    private const string ASSET_UI3DShip = AssetAddressKey.PRELOADUI_UI3D_MINERALPANEL;

    private const string ASSET_UI3DEffect = AssetAddressKey.PRELOADUI_UI_CAIKUANG;

    /// <summary>
    /// 最近的目标
    /// </summary>
    private SpacecraftEntity m_NearestTarget;
    /// <summary>
    /// 上一次的最近目标
    /// </summary>
    private SpacecraftEntity m_NearestTargetLast;
    /// <summary>
    /// 目标查找计数
    /// </summary>
    private uint m_NearestTargetCounter = 0;
    /// <summary>
    /// 根节点
    /// </summary>
    private Animator m_Root;
    /// <summary>
    /// 目标血条
    /// </summary>
    private Image m_HpBar;
	/// <summary>
	/// 目标血条LineGrid
	/// </summary>
	private RectTransform m_LineGridRect;
	/// <summary>
	/// 目标血条RectTransform
	/// </summary>
	private RectTransform m_HpBarRect;
	/// <summary>
	/// 目标血条LineGrid
	/// </summary>
	private Transform m_LineGrid;
	/// <summary>
	/// 目标血条HorizontalLayoutGroup
	/// </summary>
	private HorizontalLayoutGroup m_LineGridHorizontalLayoutGroup;
	/// <summary>
	/// 目标血量
	/// </summary>
	private TMP_Text m_HpBarLabel;
    /// <summary>
    /// 目标描述
    /// </summary>
    private TMP_Text m_HpBarState;
    /// <summary>
    /// 目标视图
    /// </summary>
    private CanvasGroup m_TargetImage;
    /// <summary>
    /// 目标模型
    /// </summary>
    private Effect3DViewer m_TargetViewer;
    /// <summary>
    /// 目标矿石id列表
    /// </summary>
    private List<uint> m_TargetList;
	/// <summary>
	/// 临时变量记录最后一次血条段数
	/// </summary>
	private int m_OldPrivew = -1;
	/// <summary>
	/// 获取场景实体Proxy
	/// </summary>
	private GameplayProxy m_GameplayProxy;
	/// <summary>
	/// 附近矿区
	/// </summary>
	private List<SpacecraftEntity> m_NearestTargetList = new List<SpacecraftEntity>();
	/// <summary>
	/// 当前对准的矿
	/// </summary>
	private SpacecraftEntity m_CurrentTarget = null;
	/// <summary>
	/// 射线数据
	/// </summary>
	private RaycastProxy m_RaycastProxy;
	/// <summary>
	/// 主角
	/// </summary>
	private SpacecraftEntity m_MainSpacecraftEntity;
	/// <summary>
	/// PlayerSkillProxy
	/// </summary>
	private PlayerSkillProxy m_PlayerSkillProxy;
	/// <summary>
	/// CfgEternityProxy
	/// </summary>
	private CfgEternityProxy m_CfgEternityProxy;
    /// <summary>
    /// 临时变量标记是否显示提示
    /// </summary>
    private bool m_IsShow;
    /// <summary>
    /// 临时变量标记是否隐藏提示
    /// </summary>
    private bool m_IsHide;
    /// <summary>
    /// 临时变量标记是否播放显示音效
    /// </summary>
    private int m_IsPlayShowMusic = 0;
    /// <summary>
    /// 临时变量标记是否播放隐藏音效
    /// </summary>
    private int m_IsPlayHideMusic = 0;

    public HudMineralHologram() : base(UIPanel.HudMineralHologram, ASSET_ADDRESS, PanelType.Hud) { }

    public override void Initialize()
    {
		m_RaycastProxy = Facade.RetrieveProxy(ProxyName.RaycastProxy) as RaycastProxy;
		m_PlayerSkillProxy = Facade.RetrieveProxy(ProxyName.PlayerSkillProxy) as PlayerSkillProxy;
		m_GameplayProxy = Facade.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;
		m_CfgEternityProxy = Facade.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
		m_Root = FindComponent<Animator>("Content");
		m_LineGridHorizontalLayoutGroup = FindComponent<HorizontalLayoutGroup>("Content/Slider/LineGrid");
		m_LineGridRect = FindComponent<RectTransform>("Content/Slider/Image_Bar/LineGrid");
		m_HpBarRect = FindComponent<RectTransform>("Content/Slider/Image_Bar");
		m_LineGrid = FindComponent<Transform>("Content/Slider/Image_Bar/LineGrid");
		m_HpBar = FindComponent<Image>("Content/Slider/Image_Bar");
        m_HpBarLabel = FindComponent<TMP_Text>("Content/Slider/Label_Value");
        m_HpBarState = FindComponent<TMP_Text>("Content/Slider/Label_Message");
		m_TargetImage = FindComponent<CanvasGroup>("Content/Bg/Model");
        m_TargetList = new List<uint>();
    }

    public override void OnShow(object msg)
    {
        base.OnShow(msg);

        if (m_TargetImage)
        {
            m_TargetViewer = m_TargetImage.gameObject.GetComponent<Effect3DViewer>();
            if (m_TargetViewer == null)
                m_TargetViewer = m_TargetImage.gameObject.AddComponent<Effect3DViewer>();
            m_TargetViewer.AutoAdjustBestRotationAndDistance = true;
            m_TargetViewer.TextureSize = 256;
        }
        m_NearestTargetCounter = 0;

        StartUpdate();
    }

    public override void OnHide(object msg)
    {
        m_TargetViewer = null;

        base.OnHide(msg);
    }

    public override NotificationName[] ListNotificationInterests()
    {
        return new NotificationName[]
        {
            NotificationName.SkillHurt,
            NotificationName.BuffHurt,
            NotificationName.HurtImmuno
        };
    }

    public override void HandleNotification(INotification notification)
    {
        switch (notification.Name)
        {
            case NotificationName.SkillHurt:
                OnAnyHurt((notification.Body as SkillHurtInfo).TargetID);
                break;
            case NotificationName.BuffHurt:
                OnAnyHurt((notification.Body as BuffHurtInfo).targetID);
                break;
            case NotificationName.HurtImmuno:
                OnAnyHurt((notification.Body as HurtImmuno).targetID);
                break;
        }
    }

    /// <summary>
    /// 收到任何伤害消息时
    /// </summary>
    /// <param name="targetID">目标ID</param>
    private void OnAnyHurt(uint targetID)
    {
        if (m_TargetViewer.EffectArray != null)
        {
            foreach (EffectController effect in m_TargetViewer.EffectArray)
            {
                VFXController effectObject = effect.GetEffectObject();
                if (effectObject)
                {
                    Animator animator = effectObject.GetComponent<Animator>();
                    if (animator)
                    {
                        animator.SetTrigger("hurting");
                    }
                }
            }
        }
    }
   
    /// <summary>
    /// 每帧更新
    /// </summary>
    protected override void Update()
    {
        if (!m_TargetViewer) return;

        bool needShow = !IsWatchOrUIInputMode() && !IsDead() && !IsLeaping();
        bool hasTarget = false;
		if (m_MainSpacecraftEntity == null)
		{
			m_MainSpacecraftEntity = m_GameplayProxy.GetEntityById<SpacecraftEntity>(m_GameplayProxy.GetMainPlayerUID());
		}
        SpacecraftEntity target = m_NearestTarget;
		//每隔5帧重新查找一下最近矿物
		if (m_NearestTargetCounter % 5 == 0)
            target = FindNearestTarget();
        m_NearestTargetCounter++;

        if (target && target.GetAttribute(AttributeName.kHP) <= 0)
            target = null;

		//加载模型
		LoadViewerModel(target);
        if (target)
        {
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
        //更新UI
        m_Root.SetBool("Show", needShow && target != null);
        UpdateUI();
        
        if (needShow && target != null&& m_IsPlayShowMusic == 0)
        {
            m_IsPlayShowMusic = 1;
            m_IsPlayHideMusic = 0;
            WwiseUtil.PlaySound((int)WwiseMusic.Music_Hud_Resource_Open, false, null);
        }
        if (m_IsPlayShowMusic == 1&&!(needShow && target != null)&& m_IsPlayHideMusic == 0)
        {
            m_IsPlayHideMusic = 1;
            m_IsPlayShowMusic = 0;
            WwiseUtil.PlaySound((int)WwiseMusic.Music_Hud_Resource_Close, false, null);
        }

        //更新全息效果的透明度
        if (needShow)
        {
			UpdateCurrentTarget();
            hasTarget = m_CurrentTarget != null;
        }
        m_TargetImage.alpha = needShow && hasTarget ? 1.0f : 0.1f;

        //提示玩家切换到战斗模式
        if (needShow && !IsBattling() && m_CurrentTarget != null)
        {
            if (!m_IsShow)
            {
                m_IsShow = true;
                m_IsHide = false;
                Facade.SendNotification(NotificationName.MSG_INTERACTIVE_SHOWTIP, HudNpcInteractiveFlagPanel.InteractiveTipType.Collector);
            }
        }
        else
        {
            if (!m_IsHide)
            {
                m_IsHide = true;
                m_IsShow = false;
                Facade.SendNotification(NotificationName.MSG_INTERACTIVE_HIDETIP, HudNpcInteractiveFlagPanel.InteractiveTipType.Collector);
            }
        }
    }
  
	/// <summary>
	/// 查找最近的矿物
	/// </summary>
	/// <returns>找到的矿物</returns>
	private SpacecraftEntity FindNearestTarget()
    {
		m_NearestTargetList.Clear();
        float distance = float.MaxValue;
        SpacecraftEntity target = null;
        foreach (SpacecraftEntity ship in m_GameplayProxy.GetEntities<SpacecraftEntity>(KHeroType.htMine))
        {
            if (ship.GetHeroType() != KHeroType.htMine) continue;
            if (ship.GetCurrentState().GetMainState() == EnumMainState.Dead) continue;
            if (ship.GetAttribute(AttributeName.kHP) <= 0) continue;
            if (ship.m_EntityFatherOwnerID > 0) continue;
            float magnitude = (ship.transform.position - m_MainSpacecraftEntity.transform.position).sqrMagnitude;
            if ((ship.transform.position - m_MainSpacecraftEntity.transform.position).magnitude < ship.GetNPCTemplateVO().TriggerRange)
            {
				target = ship;
				m_NearestTargetList.Add(ship);
			}
        }
		if (m_NearestTargetList.Count > 1)//重叠区域
		{
			if (m_CurrentTarget != null)
			{
                if (m_CurrentTarget.m_EntityFatherOwnerID>0)
                {
                    m_CurrentTarget = m_GameplayProxy.GetEntityById<SpacecraftEntity>(m_CurrentTarget.m_EntityFatherOwnerID);

                }
                if (m_NearestTargetList.Contains(m_CurrentTarget))
				{
					target = m_CurrentTarget;
				}
			}
			else
			{
				if (m_NearestTargetLast == null)
				{
					for (int i = 0; i < m_NearestTargetList.Count; i++)
					{
						float magnitude = (m_NearestTargetList[i].transform.position - m_MainSpacecraftEntity.transform.position).sqrMagnitude;
						if (magnitude < distance)
						{
							distance = magnitude;
							target = m_NearestTargetList[i];
						}
					}
				}
				else
				{
					target = m_NearestTargetLast;
				}
			}
		}
		else
		{
			if (target && (target.transform.position - m_MainSpacecraftEntity.transform.position).magnitude > target.GetNPCTemplateVO().TriggerRange)
			{
				target = null;
			}
		}
        return target;
    }

    /// <summary>
    /// 寻找大矿及其附属矿
    /// </summary>
    /// <returns></returns>
    private ModelInfo[] FindSubsidiaryMine()
    {
        List<ModelInfo> target = new List<ModelInfo>();
        ModelInfo mine = new ModelInfo();
        mine.perfab = m_CfgEternityProxy.GetModel(m_NearestTarget.GetNPCTemplateVO().Model).AssetName;
        mine.position = Vector3.zero;
        mine.rotation = Vector3.zero;
        mine.scale = Vector3.one;
        target.Add(mine);
        m_TargetList.Add(m_NearestTarget.UId());
        Transform mainShip = m_NearestTarget.GetSkinRootTransform();
        foreach (SpacecraftEntity ship in m_GameplayProxy.GetEntities<SpacecraftEntity>(KHeroType.htMine))
        {
            if (ship.GetHeroType() != KHeroType.htMine) continue;
            if (ship.GetCurrentState().GetMainState() == EnumMainState.Dead) continue;
            if (ship.GetAttribute(AttributeName.kHP) <= 0) continue;
            if (ship.m_EntityFatherOwnerID == m_NearestTarget.UId())
            {
                m_TargetList.Add(ship.UId());
                ModelInfo model = new ModelInfo();
                model.perfab = m_CfgEternityProxy.GetModel(ship.GetNPCTemplateVO().Model).AssetName;

                Transform curr = ship.GetSkinRootTransform();
                Vector3 localPoint = mainShip.InverseTransformPoint(curr.TransformPoint(Vector3.zero));

                model.position = localPoint;
                model.rotation = curr.localEulerAngles;
                model.scale = Vector3.one;
                target.Add(model);
            }
        }
        return target.ToArray();
    }

    /// <summary>
    /// 寻找单矿(小矿)
    /// </summary>
    /// <returns></returns>
    private ModelInfo[] FindSingleMine()
    {
        List<ModelInfo> target = new List<ModelInfo>();
        ModelInfo model = new ModelInfo();
        model.perfab = m_CfgEternityProxy.GetModel(m_NearestTarget.GetNPCTemplateVO().Model).AssetName;
        model.position = Vector3.zero;
        model.rotation = Vector3.zero;
        model.scale = Vector3.one;
        target.Add(model);
        m_TargetList.Add(m_NearestTarget.UId());
        return target.ToArray();
    }

	/// <summary>
	/// 加载模型
	/// </summary>
	/// <param name="target"></param>
	private void LoadViewerModel(SpacecraftEntity target)
	{
		if (m_NearestTarget != target)
		{
			m_NearestTarget = target;
			if (m_NearestTarget)
			{
				m_TargetList.Clear();
				if (m_NearestTarget.m_EntityFatherOwnerID == 0 && m_NearestTarget.HeroGroupId == 0)
				{
					m_TargetViewer.LoadModel(ASSET_UI3DShip, FindSingleMine(), ASSET_UI3DEffect);
				}
				else
				{
					m_TargetViewer.LoadModel(ASSET_UI3DShip, FindSubsidiaryMine(), ASSET_UI3DEffect);
				}

				if (!m_NearestTargetLast)
				{
					PlaySound(WwiseMusicSpecialType.SpecialType_Voice_minera_event1);
				}
			}
			else
			{
				m_TargetViewer.ClearModel();
			}

			if (m_NearestTargetLast && m_NearestTargetLast.GetAttribute(AttributeName.kHP) <= 0)
			{
				PlaySound(WwiseMusicSpecialType.SpecialType_Voice_minera_event2);
			}
			m_NearestTargetLast = m_NearestTarget;
		}
	}

	/// <summary>
	/// 更新UI
	/// </summary>
	private void UpdateUI()
	{
		if (m_NearestTarget)
		{
			m_NearestTarget.GetSkinRootTransform();

			float hp = (float)m_NearestTarget.GetAttribute(AttributeName.kHP);
			float hpMax = (float)m_NearestTarget.GetAttribute(AttributeName.kHPMax);

			float hpProgress = hpMax <= 0 ? 0 : hp / hpMax;
			//int phase = Mathf.Min(4, Mathf.FloorToInt(hpProgress / (1.0f / 5)));

			m_HpBar.fillAmount = hpProgress;
			m_HpBarLabel.text = Mathf.FloorToInt(hpProgress * 100) + "%";
			m_HpBarState.text = "";// GetLocalization("hud_text_id_" + (1021 + phase));

			if (m_TargetViewer && m_TargetViewer.Camera && m_TargetViewer.ModelArray!=null)
			{
				if (m_TargetList.Count > 0)
				{
					for (int i = 0; i < m_TargetViewer.ModelArray.Length; i++)
					{
						SpacecraftEntity entity = m_GameplayProxy.GetEntityById<SpacecraftEntity>(m_TargetList[i]);
						if (entity != null)
						{
							Transform skinTransform = entity.GetSkinTransform();
							if (skinTransform != null)
							{
								for (int k = 0; k < m_TargetViewer.ModelArray[i].transform.childCount; k++)
								{
									Transform child = m_TargetViewer.ModelArray[i].transform;
									if (child.GetChild(k).GetComponent<EffectController>() != null)
										continue;
									string name = child.GetChild(k).name;
									if (skinTransform.Find(name) == null)
									{
										child.GetChild(k).gameObject.SetActive(false);
									}
									else
									{
										child.GetChild(k).gameObject.SetActive(true);
									}
								}
							}
						}
						else
						{
							GameObject currentModel = m_TargetViewer.ModelArray[i];
							for (int j = 0; j < currentModel.transform.childCount; j++)
							{
								if (currentModel.transform.GetChild(j).GetComponent<EffectController>() != null) continue;
								currentModel.transform.GetChild(j).gameObject.SetActive(false);
							}
						}
					}
				}
				Transform skin = m_NearestTarget.GetSkinTransform();
				if (skin)
				{
                    Camera mainCamera = CameraManager.GetInstance().GetMainCamereComponent().GetCamera();
                    GameplayProxy sceneProxy = Facade.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;
                    SpacecraftEntity mainHero = sceneProxy.GetEntityById<SpacecraftEntity>(sceneProxy.GetMainPlayerUID());

                    float bestDistance = m_TargetViewer.ModelBoundsDiagonalLength / 2.0f / Mathf.Tan(mainCamera.fieldOfView * Mathf.Deg2Rad / 2.0f);
                    Vector3 bestDirection = mainCamera.transform.position - mainHero.transform.position;

                    m_TargetViewer.ModelBox.rotation = skin.rotation;
                    m_TargetViewer.Camera.transform.position = m_TargetViewer.ModelBox.position + bestDirection.normalized * bestDistance;
                    m_TargetViewer.Camera.transform.LookAt(m_TargetViewer.ModelBox);
				}
			}

			if (m_TargetViewer.EffectArray != null)
			{
				foreach (EffectController effect in m_TargetViewer.EffectArray)
				{
					VFXController effectObject = effect.GetEffectObject();
					if (effectObject)
					{
						Animator animator = effectObject.GetComponent<Animator>();
						if (animator)
						{
							animator.SetFloat("Color", 1 - hpProgress);
						}
					}
				}
			}
		}
		else
		{
			m_HpBarState.text = GetLocalization("hud_text_id_" + 1021);
		}
	}

	private void UpdateCurrentTarget()
	{
		m_CurrentTarget = null;
		if (IsBattling())
		{
			if (m_MainSpacecraftEntity)
			{
				m_CurrentTarget = m_MainSpacecraftEntity.GetTarget();
			}
		}
		else
		{
			m_CurrentTarget = m_RaycastProxy.Raycast();
		}

		if (m_CurrentTarget != null)
		{
			if ((m_CurrentTarget.GetHeroType() != KHeroType.htMine || m_CurrentTarget.GetAttribute(AttributeName.kHP) <= 0))
			{
				m_CurrentTarget = null;
			}
		}
	}

	/// <summary>
	/// 播放音效
	/// </summary>
	/// <param name="sound">音效类型</param>
	private void PlaySound(WwiseMusicSpecialType sound)
    {
        //TaskVoiceInfo msg = new TaskVoiceInfo();
        //msg.audioID = (int)sound;
        //msg.isVoice = true;
        //msg.text = "";
        //msg.isReplaceOther = true;
        //msg.place = WwiseMusicPalce.Palce_1st;
        //Facade.SendNotification(NotificationName.Voice, msg);
        /// 超级电脑语音非语音助手
        WwiseUtil.PlaySound(WwiseManager.voiceComboID, sound, WwiseMusicPalce.Palce_1st, false, null);
    }
}
