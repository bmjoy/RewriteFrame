using Assets.Scripts.Define;
using PureMVC.Interfaces;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static BattleWeapon_Missile;

public class HudCrossHairPanel : HudBase
{
    private const string ASSET_ADDRESS = Leyoutech.Core.Loader.Config.AssetAddressKey.PRELOADUI_HUD_CROSSHAIRPANEL;

    /// <summary>
    /// 准星锁定的移动插值速度
    /// </summary>
    private const float m_LerpValue = 0.3f;
    /// <summary>
    /// 根节点
    /// </summary>
    private RectTransform m_Root;
    /// <summary>
    /// 画布相机
    /// </summary>
    private Camera m_Camera;
    /// <summary>
    /// 准星边框
    /// </summary>
    private RectTransform m_FrameBox;
    /// <summary>
    /// 准星中点
    /// </summary>
    private RectTransform m_Point;
    /// <summary>
    /// 准星边框
    /// </summary>
    private RectTransform m_Frame;
    /// <summary>
    /// 当前准星
    /// </summary>
    private RectTransform m_Crosshair;
    /// <summary>
    /// 弹着点容器
    /// </summary>
    private RectTransform m_PointBox;
    /// <summary>
    /// 弹着点模板
    /// </summary>
    private RectTransform m_PointTemplate;
    /// <summary>
    /// 目标列表容器
    /// </summary>
    private RectTransform m_TargetBox;
    /// <summary>
    /// 准星样式
    /// </summary>
    private WeaponAndCrossSight.WeaponAndCrossSightTypes m_WeaponStyle = WeaponAndCrossSight.WeaponAndCrossSightTypes.Null;
    /// <summary>
    /// 当前目标
    /// </summary>
    private SpacecraftEntity m_CurrentTarget;
    /// <summary>
    /// 当前目标是否很远
    /// </summary>
    private bool m_CurrentTargetIsFar = false;
    /// <summary>
    /// 自动锁定距离
    /// </summary>
    private float m_AutoLockDistance = 0.0f;
    /// <summary>
    /// 自动锁定距离乘数
    /// </summary>
    private float m_AutoLockDistanceMulti = 1.0f;
    /// <summary>
    /// 自动锁定是否启用
    /// </summary>
    private bool m_AutoLockEnabled = false;

    /// <summary>
    /// 上次开火失败了
    /// </summary>
    private bool m_lastFireFail;

    /// <summary>
    /// 最后一次武器开火的时间点
    /// </summary>
    private float m_lastShotTime;
    /// <summary>
    /// 每个弹着点与对应的死亡时间
    /// </summary>
    private Dictionary<RectTransform, float> m_PointToDeathTime = new Dictionary<RectTransform, float>();
    /// <summary>
    /// 当前的目标列表
    /// </summary>
    private Dictionary<ulong, WeaponAndCrossSight_Missile.TargetLockInfo> m_CurrentTargetList = new Dictionary<ulong, WeaponAndCrossSight_Missile.TargetLockInfo>();
    /// <summary>
    /// 当前目标列表清空时间
    /// </summary>
    private float m_CurrentTargetListClearTime;

    /// <summary>
    /// 子准星开火是间
    /// </summary>
    private float m_SubCrosshairBeginTime = 0;
    /// <summary>
    /// 子准星持续时间
    /// </summary>
    private float m_SubCrosshairDuration = 0;
    /// <summary>
    /// 子准星坐标列表
    /// </summary>
    private List<Vector2> m_subCrosshairPositions = new List<Vector2>();

    //最大导弹数量
    private int MaxMissileCount = 0;
    //导弹的锁定一层的时间
    private float MissileLockTime = 0;


    /// <summary>
    /// 射线数据
    /// </summary>
    private RaycastProxy m_RaycastProxy;

    /// <summary>
    /// 是否处理6Dof模式
    /// </summary>
    private bool m_IsIn6DofMode;

	public HudCrossHairPanel() : base(UIPanel.HudCrossHairPanel, ASSET_ADDRESS, PanelType.Hud) { }

    public override void Initialize()
    {
		m_RaycastProxy = Facade.RetrieveProxy(ProxyName.RaycastProxy) as RaycastProxy;
		m_Root = FindComponent<RectTransform>("Content");
        m_Camera = m_Root.GetComponentInParent<Canvas>().worldCamera;
        m_PointBox = FindComponent<RectTransform>("Points");
        m_PointTemplate = FindComponent<RectTransform>("Points/point");
    }

    public override void OnShow(object msg)
    {
        base.OnShow(msg);

        OnMainWeaponChanged();

        StartUpdate();
    }

    public override NotificationName[] ListNotificationInterests()
    {
        return new NotificationName[]
        {
            NotificationName.MSG_CHANGE_BATTLE_STATE,
            NotificationName.PlayerWeaponToggleEnd,
            NotificationName.PlayerWeaponPowerChanged,
            NotificationName.PlayerWeaponShot,
            NotificationName.PlayerWeaponCrosshairScale,
            NotificationName.PlayerMissileWeaponTargetSelection,
            NotificationName.PlayerWeaponSelectedTarget,
            NotificationName.MSG_HUMAN_ENTITY_ON_ADDED,
            NotificationName.PlayerWeaponFireFail,
            NotificationName.PlayerWeaponCrosshairScale_ShotGun,
            NotificationName.Enter6DofMode,
            NotificationName.Exit6DofMode
        };
    }

    public override void HandleNotification(INotification notification)
    {
        switch (notification.Name)
        {
            case NotificationName.MSG_CHANGE_BATTLE_STATE:
            case NotificationName.PlayerWeaponToggleEnd:
                OnMainWeaponChanged();
                break;
            case NotificationName.PlayerWeaponShot:
                OnWeaponFire(notification.Body as MsgPlayerWeaponShot);
                break;
            case NotificationName.PlayerWeaponCrosshairScale:
                OnWeaponCrosshairOffset(notification.Body as MsgPlayerWeaponCrosshair);
                break;
            case NotificationName.PlayerMissileWeaponTargetSelection:
                OnTargetListChanged(notification.Body as PlayerMissileWeaponTargetSelectionInfo);
                break;
            case NotificationName.PlayerWeaponSelectedTarget:
                ChangeCrosshairColor((notification.Body as WeaponSelectedTargetInfo).selectedTarget);
                break;
            case NotificationName.PlayerWeaponCrosshairScale_ShotGun:
                OnWeaponCrosshairSonArea(notification.Body as MsgPlayerWeaponCrosshair_ShotGun);
                break;
            case NotificationName.MSG_HUMAN_ENTITY_ON_ADDED:
                {
                    MsgEntityInfo entityInfo = (MsgEntityInfo)notification.Body;
                    if (entityInfo.IsMain)
                        OnMainWeaponChanged();
                }
                break;
            case NotificationName.PlayerWeaponFireFail:
                OnWeaponFireFail();
                break;
            case NotificationName.Enter6DofMode:
                OnEnter6DofMode();
                break;
            case NotificationName.Exit6DofMode:
                OnExit6DofMode();
                break;
        }
    }

    /// <summary>
    /// 当前目标
    /// </summary>
    public SpacecraftEntity CurrentTarget
    {
        get { return m_CurrentTarget; }
    }

    /// <summary>
    /// 开枪失败时
    /// </summary>
    private void OnWeaponFireFail()
    {
        m_lastFireFail = true;
    }

    /// <summary>
    /// 主武器改变时
    /// </summary>
    private void OnMainWeaponChanged()
    {
        m_lastFireFail = false;

        m_CurrentTargetList.Clear();

        m_Crosshair = null;
        m_Point = null;
        m_Frame = null;
        m_FrameBox = null;
        m_TargetBox = null;

        for (int i = 0; i < m_Root.childCount; i++)
        {
            m_Root.GetChild(i).gameObject.SetActive(false);
        }

        PlayerSkillProxy skillProxy = Facade.RetrieveProxy(ProxyName.PlayerSkillProxy) as PlayerSkillProxy;

        IWeapon currentWeapon = skillProxy.GetCurrentWeapon();

        m_WeaponStyle = (int)WeaponAndCrossSight.WeaponAndCrossSightTypes.Null;
        m_AutoLockEnabled = currentWeapon != null && currentWeapon.GetConfig().AssistKey != 0;

        SpacecraftEntity main = GetMainEntity();
        if (main && IsBattling())
        {
            IReformer reformer = skillProxy.UsingReformer() ? skillProxy.GetReformer() : null;

            if (reformer != null)
                m_WeaponStyle = (WeaponAndCrossSight.WeaponAndCrossSightTypes)reformer.GetConfig().Reticle;
            else if (currentWeapon != null)
                m_WeaponStyle = (WeaponAndCrossSight.WeaponAndCrossSightTypes)currentWeapon.GetConfig().Reticle;
        }

        m_Crosshair = m_Root.GetChild((int)m_WeaponStyle).GetComponent<RectTransform>();
       // m_Crosshair.gameObject.SetActive(true);
        m_Point = m_Crosshair.Find("point") != null ? m_Crosshair.Find("point").GetComponent<RectTransform>() : null;
        m_Frame = m_Crosshair.Find("frame") != null ? m_Crosshair.Find("frame").GetComponent<RectTransform>() : null;
        m_FrameBox = m_Crosshair.Find("FrameBox") != null ? m_Crosshair.Find("FrameBox").GetComponent<RectTransform>() : null;
        m_TargetBox = m_Crosshair.Find("Targets") != null ? m_Crosshair.Find("Targets").GetComponent<RectTransform>() : null;
	}

    /// <summary>
    /// 进入6Dof模式
    /// </summary>
    private void OnEnter6DofMode()
    {
        m_IsIn6DofMode = true;
    }

    /// <summary>
    /// 退出6Dof模式
    /// </summary>
    private void OnExit6DofMode()
    {
        m_Root.anchoredPosition = Vector2.zero;
        m_IsIn6DofMode = false;
    }

    /// <summary>
    /// 改变准星颜色
    /// </summary>
    /// <param name="hasTarget">目标Entity</param>
    private void ChangeCrosshairColor(SpacecraftEntity target)
    {
        if (m_Crosshair == null) return;

        GameObjectList list = m_Crosshair.GetComponent<GameObjectList>();
        if (list != null)
        {
            Color color = target != null ? Color.red : Color.white;

            foreach (GameObject item in list.gameObjects)
            {
                foreach(Image img in item.GetComponentsInChildren<Image>())
                {
                    img.color = color;
                }
            }
        }
    }

    /// <summary>
    /// 更新准星的位置
    /// </summary>
    protected override void Update()
    {
        RaycastTarget_OLD();

        m_Root.gameObject.SetActive(!IsWatchOrUIInputMode() && !IsDead() && !IsLeaping());

        if (m_Crosshair == null)
            return;

        if (m_WeaponStyle == WeaponAndCrossSight.WeaponAndCrossSightTypes.Null)
		{
			if (m_CurrentTarget != null)
			{
				GameplayProxy sceneProxy = Facade.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;
				SpacecraftEntity main = sceneProxy.GetEntityById<SpacecraftEntity>(sceneProxy.GetMainPlayerUID());

				m_CurrentTarget = null;
				m_RaycastProxy.SetCurrentTarget(null);
				main.SetTarget(null, null);
			}
			return;
		}

        UpdateCrossHairPosition();
        UpdateWeaponBullets();
        UpdateWeaponCrosshairOffset();
        UpdateTargetList();
        UpdateShotPoint();

		//RaycastTarget_OLD();
    }

    private void UpdateCrossHairPosition()
    {
        if (!m_IsIn6DofMode)
            return;
        
        GameplayProxy sceneProxy = Facade.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;
        SpacecraftEntity main = sceneProxy.GetEntityById<SpacecraftEntity>(sceneProxy.GetMainPlayerUID());

        Vector3 offset = main.GetMouseOffset();
        float x = Screen.width / 2 + offset.x;
        float y = Screen.height / 2 + offset.y;

        Vector2 point;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(m_Root.parent.GetComponent<RectTransform>(), new Vector2(x, y), m_Camera, out point))
        {
            m_Root.anchoredPosition = point;
        }
    }

    /// <summary>
    /// 更新武器的子弹信息
    /// </summary>
    private void UpdateWeaponBullets()
    {
        if (m_Crosshair != null && m_WeaponStyle != WeaponAndCrossSight.WeaponAndCrossSightTypes.Null)
        {
            Transform powerSlider = FindComponent<Transform>(m_Crosshair, "Slider");
            Animator powerSliderAnimator = FindComponent<Animator>(m_Crosshair, "Slider");
            if (powerSlider == null)
                return;

            PlayerSkillProxy skillProxy = Facade.RetrieveProxy(ProxyName.PlayerSkillProxy) as PlayerSkillProxy;
            if (skillProxy.UsingReformer())
            {
                powerSlider.gameObject.SetActive(false);
                return;
            }

            IWeapon currentWeapon = skillProxy.GetCurrentWeapon();
            if (currentWeapon == null)
            {
                powerSlider.gameObject.SetActive(false);
                return;
            }

            WeaponPowerVO weaponVO1 = skillProxy.GetWeaponPowerOfMainPlayer(0);
            WeaponPowerVO weaponVO2 = skillProxy.GetWeaponPowerOfMainPlayer(1);

            WeaponPowerVO power = weaponVO1 != null && skillProxy.GetWeaponByUID(weaponVO1.WeaponUID) == currentWeapon ? weaponVO1 : weaponVO2;

            float curr = 0;
            float total = 0;

			if (power != null)
			{
				if (m_WeaponStyle == WeaponAndCrossSight.WeaponAndCrossSightTypes.MachineGun  )
				{
					curr = power.CurrentValue;
					total = power.MaxValue;
				}
				else if (m_WeaponStyle == WeaponAndCrossSight.WeaponAndCrossSightTypes.Missile)
				{
					foreach (var entity in m_CurrentTargetList.Values)
					{
						curr += entity.LockTimes;
					}
                    total = MaxMissileCount;// skillProxy.GetCurrentBattleWeapon().GetRelativeHeightOfReticle().MissileCountInOneShot;
				}
				else if ((int)m_WeaponStyle ==(int)WeaponAndCrossSight.WeaponAndCrossSightTypes.ShotGun)
				{
					curr = power.CurrentValue;
					total = power.MaxValue;
				}
			}

			float weaponPowerRatio = total != 0 ? curr / total : 0;

            if (powerSliderAnimator)
            {
                if (power.ForceCooldown)
                    powerSliderAnimator.SetInteger("State", 2);
                else if (weaponPowerRatio <= 0.7f)
                    powerSliderAnimator.SetInteger("State", 0);
                else if (weaponPowerRatio > 0.7f)
                    powerSliderAnimator.SetInteger("State", 1);

                if (m_lastFireFail)
                    powerSliderAnimator.SetTrigger("FireFail");
            }

            m_lastFireFail = false;

            RadialSlider radialSlider = powerSlider.GetComponent<RadialSlider>();
            if(radialSlider)
            {
                radialSlider.FillAmount = weaponPowerRatio;
                return;
            }

            BulletSlider bulletSlider = powerSlider.GetComponent<BulletSlider>();
            if(bulletSlider)
            {
                bulletSlider.BulletCount = (int)total;
                bulletSlider.FillAmount = weaponPowerRatio;
                return;
            }

            Slider commonSlider = powerSlider.GetComponent<Slider>();
            if(commonSlider)
            {
                commonSlider.minValue = 0;
                commonSlider.maxValue = 1;
                commonSlider.value = weaponPowerRatio;
                return;
            }
        }
    }

    #region 扩散型准星

    /// <summary>
    /// 武器准星偏移
    /// </summary>
    /// <param name="msg">MsgPlayerWeaponCrosshairScale</param>
    private void OnWeaponCrosshairOffset(MsgPlayerWeaponCrosshair crosshair)
    {
		UpdateCrosshairOffset(crosshair);
	}

    /// <summary>
    /// 更新准星偏移
    /// </summary>
    /// <param name="crosshair">MsgPlayerWeaponCrosshairScale</param>
	private void UpdateCrosshairOffset(MsgPlayerWeaponCrosshair crosshair)
	{
        //主准星的放大缩小
        if (m_FrameBox != null && (crosshair.HorizontalRelativeHeight >= 0 || crosshair.VerticalRelativeHeight >= 0))
        {
            //float x = Screen.width / 2 + crosshair.HorizontalRelativeHeight * Screen.width / 2;
            //float y = Screen.height / 2 + crosshair.VerticalRelativeHeight * Screen.height / 2;

            /*
            Vector2 screen = UIManager.Instance.ScreenSize;
            float x = Screen.width / 2 + crosshair.HorizontalRelativeHeight * screen.x / 2;
            float y = Screen.height / 2 + crosshair.VerticalRelativeHeight * screen.y / 2;

            Vector2 point;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(m_FrameBox.parent.GetComponent<RectTransform>(), new Vector2(x, y), m_Camera, out point))
            {
                m_FrameBox.sizeDelta = point * 2;
            }
            */

            Vector2 screen = UIManager.Instance.ScreenSize;
            float x = Screen.width / 2 + crosshair.HorizontalRelativeHeight * screen.x / 2;
            float y = Screen.height / 2 + crosshair.VerticalRelativeHeight * screen.y / 2;

            Vector2 point;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(m_Root.parent.GetComponent<RectTransform>(), new Vector2(x, y), m_Camera, out point))
            {
                m_FrameBox.sizeDelta = point * 2;
            }
            m_Crosshair.gameObject.SetActive(true);

            MaxMissileCount = crosshair.MissileCountInOneShot;
            MissileLockTime = crosshair.MissileLockTime;
        }
	}

    /// <summary>
    /// 通知子区域情况
    /// </summary>
    /// <param name="crosshair"></param>
     private void OnWeaponCrosshairSonArea(MsgPlayerWeaponCrosshair_ShotGun crosshair)
    {
        //子准星的列表
        if (m_Crosshair != null && m_Crosshair.Find("SubCrosshairs") != null)
        {
            RectTransform subCrosshairBox = m_Crosshair.Find("SubCrosshairs").GetComponent<RectTransform>();
            RectTransform subCrosshairTemplate = subCrosshairBox.GetChild(0).GetComponent<RectTransform>();

            m_subCrosshairPositions.Clear();
            m_SubCrosshairBeginTime = Time.time;
            m_SubCrosshairDuration = crosshair.RemainingRestoreDuration;

            int index = 0;
            for (int i = 0; i < crosshair.SubAimAreaRelativeHeight.Count; i++)
            {
                Vector2 size = crosshair.SubAimAreaRelativeHeight[i];
                Vector2 position = crosshair.SubAimAreaScreenPosition[i];

                RectTransform view = i < subCrosshairBox.childCount ? subCrosshairBox.GetChild(i).GetComponent<RectTransform>() : Object.Instantiate(subCrosshairTemplate, subCrosshairBox);
                Animator animator = view.GetComponent<Animator>();

                Vector2 positionPoint;
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(m_Root.parent.GetComponent<RectTransform>(), position, m_Camera, out positionPoint))
                    view.anchoredPosition = positionPoint - new Vector2(Screen.width / 2, Screen.height / 2);

                m_subCrosshairPositions.Add(positionPoint);

                Vector2 sizePoint;
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(m_Root.parent.GetComponent<RectTransform>(), position + (new Vector2(Screen.width * size.x, Screen.height * size.y)), m_Camera, out sizePoint))
                    view.sizeDelta = sizePoint - positionPoint;

                view.gameObject.SetActive(true);
                animator.enabled = false;

                index++;
            }

            for (int i = index; i < subCrosshairBox.childCount; i++)
            {
                subCrosshairBox.GetChild(i).gameObject.SetActive(false);
            }
        }

    }

    /// <summary>
    /// 武器准星偏移动画插值
    /// </summary>
    private void UpdateWeaponCrosshairOffset()
    {
        //恢复子准星
        if (m_subCrosshairPositions.Count > 0 && m_Crosshair != null && m_Crosshair.Find("SubCrosshairs") != null)
        {
            RectTransform subCrosshairBox = m_Crosshair.Find("SubCrosshairs").GetComponent<RectTransform>();

            float t = Mathf.Clamp01((Time.time - m_SubCrosshairBeginTime) / m_SubCrosshairDuration);
            for (int i = 0; i < subCrosshairBox.childCount; i++)
            {
                RectTransform child = subCrosshairBox.GetChild(i).GetComponent<RectTransform>();
                Animator animator = child.GetComponent<Animator>();

                if (i < m_subCrosshairPositions.Count)
                    child.anchoredPosition = Vector2.Lerp(m_subCrosshairPositions[i], Vector2.zero, t);
                else
                    child.gameObject.SetActive(false);

                if (Mathf.Approximately(t, 1.0f))
                {
                    child.gameObject.SetActive(i == 0);
                    animator.enabled = i == 0;
                }
            }

            if(Mathf.Approximately(t,1.0f))
                m_subCrosshairPositions.Clear();
        }
    }

    #endregion

    #region 多目标型准星

    /// <summary>
    /// 当目标列表改变时
    /// </summary>
    /// <param name="msg">PlayerMissileWeaponTargetSelectionInfo</param>
    private void OnTargetListChanged(PlayerMissileWeaponTargetSelectionInfo msg)
    {
        if (msg.TargeList == null || msg.TargeList.Count == 0)
        {
            if (m_CurrentTargetListClearTime <= 0 && m_CurrentTargetList.Count > 0)
            {
                int selectedCount = 0;
                foreach (var info in m_CurrentTargetList.Values)
                {
                    if (info.LockTimes > 0)
                        selectedCount++;
                }

                if (selectedCount > 0)
                {
                    m_CurrentTargetListClearTime = Time.time + 3.0f;
                    foreach (var info in m_CurrentTargetList.Values)
                    {
                        info.LockState = WeaponAndCrossSight_Missile.TargetLockState.Locked;
                    }
                }
                else
                    m_CurrentTargetList.Clear();
            }
        }
        else
        {
            m_CurrentTargetList.Clear();
            foreach (ulong entityId in msg.TargeList.Keys)
            {
                m_CurrentTargetList.Add(entityId, msg.TargeList[entityId]);
            }
        }
    }

    /// <summary>
    /// 更新目标列表
    /// </summary>
    private void UpdateTargetList()
    {
        if (m_TargetBox == null)
            return;

        int index = 0;

        if (m_CurrentTargetList != null)
        {
            if (m_CurrentTargetListClearTime > 0 && Time.time > m_CurrentTargetListClearTime)
            {
                m_CurrentTargetList.Clear();
                m_CurrentTargetListClearTime = -1;
            }
        }

        if (m_CurrentTargetList != null)
        {
            GameplayProxy gameplay = Facade.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;
            PlayerSkillProxy skillProxy = Facade.RetrieveProxy(ProxyName.PlayerSkillProxy) as PlayerSkillProxy;

            RectTransform template = m_TargetBox.GetChild(0).GetComponent<RectTransform>();

            //BattleWeapon_Missile_CenterLock missile = skillProxy.GetCurrentBattleWeapon() as BattleWeapon_Missile_CenterLock;
            //float time = 0;
            //if (missile != null)
            //{
            //    // 临时代码
            //    time = (skillProxy.GetCurrentBattleWeapon() as BattleWeapon_Missile_CenterLock).GetLockonTime();
            //}

            foreach (ulong entityId in m_CurrentTargetList.Keys)
            {
                BaseEntity entity = GameplayManager.Instance.GetEntityManager().GetEntityById<BaseEntity>((uint)entityId);
                if (!entity) continue;


                var targetInfo = m_CurrentTargetList[entityId];

                //屏幕外忽略
                Vector3 viewportPoint = Camera.main.WorldToViewportPoint(entity.GetRootTransform().position);
                if (!(viewportPoint.x >= 0 && viewportPoint.y >= 0 && viewportPoint.x <= 1 && viewportPoint.y <= 1 && viewportPoint.z > Camera.main.nearClipPlane))
                    continue;

                Vector2 viewPosition;
                Vector3 screenPosition = Camera.main.WorldToScreenPoint(entity.GetRootTransform().position);
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(m_TargetBox, screenPosition, m_Camera, out viewPosition))
                {
                    RectTransform view = index < m_TargetBox.childCount ? m_TargetBox.GetChild(index).GetComponent<RectTransform>() : Object.Instantiate(template, m_TargetBox);
                    view.gameObject.SetActive(true);
                    view.anchoredPosition = viewPosition;

                    RectTransform loadingBox = view.Find("Loading").GetComponent<RectTransform>();
                    loadingBox.gameObject.SetActive(targetInfo.LockState == WeaponAndCrossSight_Missile.TargetLockState.Locking);
                    if (loadingBox.gameObject.activeSelf)
                        loadingBox.Find("Image_loading").GetComponent<Image>().fillAmount = (MissileLockTime - targetInfo.LockTimeRemaining) / MissileLockTime;

                    RectTransform lockedBox = view.Find("Locked").GetComponent<RectTransform>();
                    lockedBox.gameObject.SetActive(targetInfo.LockTimes >= 1);

                    RectTransform countBox = view.Find("Count").GetComponent<RectTransform>();
                    countBox.gameObject.SetActive(targetInfo.LockTimes > 1);
                    if (countBox.gameObject.activeSelf)
                        countBox.Find("Label_Count").GetComponent<TMP_Text>().text = targetInfo.LockTimes.ToString();

                    index++;
                }
            }
        }

        for (int i = index; i < m_TargetBox.childCount; i++)
        {
            m_TargetBox.GetChild(i).gameObject.SetActive(false);
        }
    }

    #endregion

    #region 弹着点部分

    /// <summary>
    /// 开枪的时候
    /// </summary>
    /// <param name="msg">MsgPlayerWeaponShot</param>
    private void OnWeaponFire(MsgPlayerWeaponShot msg)
    {
        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(m_PointBox, msg.screenPoint, m_Camera, out localPoint))
        {
            RectTransform point = null;
            for (int i = 0; i < m_PointBox.childCount; i++)
            {
                RectTransform current = m_PointBox.GetChild(i).GetComponent<RectTransform>();
                if (!current.gameObject.activeSelf)
                {
                    point = current;
                    break;
                }
            }

            if (!point)
            {
                point = Object.Instantiate(m_PointTemplate, m_PointBox);
            }

            point.anchoredPosition = localPoint;
            point.gameObject.SetActive(true);

            m_PointToDeathTime.Add(point, Time.time + 0.1f);
            m_lastShotTime = Time.time;

            if (m_Point)
                m_Point.GetComponent<Image>().enabled = false;
        }
    }

    /// <summary>
    /// 更新所有弹着点
    /// </summary>
    private void UpdateShotPoint()
    {
        float now = Time.time;
        for (int i = 0; i < m_PointBox.childCount; i++)
        {
            RectTransform point = m_PointBox.GetChild(i).GetComponent<RectTransform>();
            if (m_PointToDeathTime.ContainsKey(point) && m_PointToDeathTime[point] < now)
            {
                point.gameObject.SetActive(false);
                m_PointToDeathTime.Remove(point);
            }
        }

        if (now - m_lastShotTime > 0.3f)
        {
            if (m_Point)
                m_Point.GetComponent<Image>().enabled = true;
        }
    }

    #endregion



    #region 待删除代码

    /// <summary>
    /// 目标检测
    /// </summary>
    private void RaycastTarget_OLD()
    { 
        //检测目标
		m_CurrentTarget =  m_RaycastProxy.RaycastTarget(m_AutoLockDistance);
		m_CurrentTargetIsFar = m_RaycastProxy.GetCurrentTargetIsFar();
		//准星点追踪
		GameplayProxy sceneProxy = Facade.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;
        SpacecraftEntity main = sceneProxy.GetEntityById<SpacecraftEntity>(sceneProxy.GetMainPlayerUID());

        if (main != null && m_Point && m_Frame)
        {
            SpacecraftEntity target = main.GetTarget();

            m_Point.gameObject.SetActive(true);
            m_Frame.gameObject.SetActive(true);


            //通过阵营或者类型判断
            bool targetIsEnemy = false;
            if (target != null)
            {
				//是否可攻击
				targetIsEnemy = sceneProxy.CanAttackToTarget(main, target);
			}

            if (target != null && !m_CurrentTargetIsFar)
            {
                m_Point.anchoredPosition = Vector2.zero;
                ChangeCrosshairColor(target);
            }
            else
            {
                bool locked = false;

                Vector2 iconPosition = Vector2.zero;
                if (target != null)
                {
                    Vector3 targetPosition = target.transform.position;
                    Vector3 viewportPoint = Camera.main.WorldToViewportPoint(targetPosition);
                    if (viewportPoint.x >= 0 && viewportPoint.y >= 0 && viewportPoint.x <= 1 && viewportPoint.y <= 1 && viewportPoint.z >= Camera.main.nearClipPlane)
                    {
                        Vector2 position2D = Vector2.zero;
                        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(m_Root, Camera.main.WorldToScreenPoint(targetPosition), m_Camera, out position2D))
                        {
                            iconPosition = position2D;
                            locked = true;
                        }
                    }
                }

                m_Point.anchoredPosition = m_AutoLockDistance > 0 ? Vector2.Lerp(m_Point.anchoredPosition, iconPosition, m_LerpValue) : Vector2.zero;
                ChangeCrosshairColor(target);
            }

            float radius = Mathf.Max(m_Frame.sizeDelta.x, m_Frame.sizeDelta.y) / 2.0f;
            Vector2 centerPoint = RectTransformUtility.WorldToScreenPoint(m_Camera, m_Frame.transform.position);
            Vector2 rightPoint = RectTransformUtility.WorldToScreenPoint(m_Camera, m_Frame.transform.TransformPoint(Vector3.right * radius));

            m_AutoLockDistance = m_AutoLockEnabled ? Mathf.Abs(centerPoint.x - rightPoint.x) : 0;
        }
    }

    #endregion
}
