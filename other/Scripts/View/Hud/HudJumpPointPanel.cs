using Assets.Scripts.Define;
using Eternity.FlatBuffer;
using Map;
using PureMVC.Interfaces;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class HudJumpPointPanel : HudBase
{
    private const string ASSET_ADDRESS = Leyoutech.Core.Loader.Config.AssetAddressKey.PRELOADUI_HUD_JUMPPOINTSPANEL;

    /// <summary>
    /// 按住时长
    /// </summary>
    private const float HOLD_TIME = 0.3f;
    /// <summary>
    /// 跃迁点的缩放量
    /// </summary>
    private const float LEAP_POINT_SCALE = 0.85f;
    /// <summary>
    /// 跃迁点的热区半径
    /// </summary>
    private const float LEAP_POINT_HIT_RANGE = 100;
    /// <summary>
    /// 箭头的缩放量
    /// </summary>
    private const float ARROW_SCALE = 0.4f;


    /// <summary>
    /// 根节点
    /// </summary>
    private RectTransform m_Root;
    /// <summary>
    /// 画布相机
    /// </summary>
    private Camera m_Camera;

    /// <summary>
    /// 缩放定位容器
    /// </summary>
    private RectTransform m_ScaleBox;
    /// <summary>
    /// 准星容器
    /// </summary>
    private RectTransform m_Crosshair;
    /// <summary>
    /// 跃迁准星
    /// </summary>
    private RectTransform m_JumpCrosshair;
    /// <summary>
    /// 矿石准星
    /// </summary>
    private RectTransform m_MineralCrosshair;

    /// <summary>
    /// 跃迁点容器
    /// </summary>
    private RectTransform m_PointBox;
    /// <summary>
    /// 跃迁点模板
    /// </summary>
    private RectTransform m_PointTemplate;
    /// <summary>
    /// 箭头容器
    /// </summary>
    private RectTransform m_ArrowBox;
    /// <summary>
    /// 箭头模板
    /// </summary>
    private RectTransform m_ArrowTemplate;

    /// <summary>
    /// 准备中容器
    /// </summary>
    private RectTransform m_ReadyBox;
    /// <summary>
    /// 准备中热键容器
    /// </summary>
    private RectTransform m_ReadyHotkeyBox;

    /// <summary>
    /// 任务图标模板
    /// </summary>
    private RectTransform m_MissionTemplate;
    /// <summary>
    /// 空闲的任务图标容器
    /// </summary>
    private RectTransform m_MissionIdleBox;

    /// <summary>
    /// 热键
    /// </summary>
    private RectTransform m_HotKeyBox;


    /// <summary>
    /// 当前场的跃迁点配置列表
    /// </summary>
    private List<LeapItem> m_LeapCfgs = new List<LeapItem>();
    /// <summary>
    /// 当前碰到的跃迁点
    /// </summary>
    private int m_CurrentIndex = -1;
    /// <summary>
    /// 跃迁状态
    /// </summary>
    private LEAP_PHASE m_LeapState;

	/// <summary>
	/// 射线数据
	/// </summary>
	private RaycastProxy m_RaycastProxy;
    /// <summary>
    /// 任务追踪代理
    /// </summary>
    private TaskTrackingProxy m_TaskTracking;

    private enum LEAP_PHASE { NORMAL,READY,LEAP};

    public HudJumpPointPanel() : base(UIPanel.HudJumpPointPanel, ASSET_ADDRESS, PanelType.Hud) { }

    public override void Initialize()
    {
		m_RaycastProxy = Facade.RetrieveProxy(ProxyName.RaycastProxy) as RaycastProxy;
        m_TaskTracking = Facade.RetrieveProxy(ProxyName.TaskTrackingProxy) as TaskTrackingProxy;

		m_Root = FindComponent<RectTransform>("Content");
        m_Camera = m_Root.GetComponentInParent<Canvas>().worldCamera;
        m_ScaleBox = FindComponent<RectTransform>("Content/ScaleBox");
        m_Crosshair = FindComponent<RectTransform>("Content/Crosshair");
        m_JumpCrosshair = FindComponent<RectTransform>("Content/Crosshair/JumpPoint");
        m_MineralCrosshair = FindComponent<RectTransform>("Content/Crosshair/MineralPoint");

        m_PointBox = FindComponent<RectTransform>("Content/Points");
        m_PointTemplate = FindComponent<RectTransform>("PointTemp");
        m_ArrowBox = FindComponent<RectTransform>("Content/Arrows");
        m_ArrowTemplate = FindComponent<RectTransform>("ArrowTemp");

        m_ReadyBox = FindComponent<RectTransform>("Content/JumpText");
        m_ReadyHotkeyBox = FindComponent<RectTransform>("Content/JumpText/HotKeyBox");

        m_MissionIdleBox = FindComponent<RectTransform>("MissionIdleBox");
        m_MissionTemplate = FindComponent<RectTransform>("IconMissionElement");

        m_HotKeyBox = FindComponent<RectTransform>("HotKeyRole");
        m_HotKeyBox.gameObject.SetActive(false);
    }

    public override void OnShow(object msg)
    {
        base.OnShow(msg);

        m_LeapState = LEAP_PHASE.NORMAL;
        m_CurrentIndex = -1;
        m_Root.gameObject.SetActive(true);

        ResetJumpPoints();

        AddHotKey("f_hold", HotKeyMapID.SHIP, HotKeyID.LeapStart, HOLD_TIME, OnHoldKeyEvent, m_HotKeyBox, "Transition", HotkeyManager.HotkeyStyle.HUMAN_INTERACTIVE);
        AddHotKey("f_press", HotKeyMapID.SHIP, HotKeyID.LeapStop, OnPressKeyEvent, m_HotKeyBox, "Cancel the transition", HotkeyManager.HotkeyStyle.HUMAN_INTERACTIVE);

        SetHotKeyVisible("f_hold", true);
        SetHotKeyVisible("f_press", false);

        StartUpdate();
    }

    public override void OnHide(object msg)
    {
        m_LeapCfgs.Clear();

        m_CurrentIndex = -1;

        m_HotKeyBox.SetParent(GetTransform());

        base.OnHide(msg);
    }

    public override NotificationName[] ListNotificationInterests()
    {
        return new NotificationName[]
        {
            NotificationName.MSG_CHANGE_BATTLE_STATE
        };
    }

    public override void HandleNotification(INotification notification)
    {
        switch (notification.Name)
        {
            case NotificationName.MSG_CHANGE_BATTLE_STATE:
                OnStateChanged();
                break;
        }
    }

    #region 热键处理

    /// <summary>
    /// 是否可以跃迁
    /// </summary>
    /// <returns></returns>
    private bool IsCanLeap()
    {
        GameplayProxy gameplayProxy = GameFacade.Instance.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;
        SpacecraftEntity entity = gameplayProxy.GetEntityById<SpacecraftEntity>(gameplayProxy.GetMainPlayerUID());

        return (entity.GetCurrentState().GetMainState() == EnumMainState.Cruise || entity.GetCurrentState().GetMainState() == EnumMainState.Born)
            && !entity.GetCurrentState().IsHasSubState(EnumSubState.Peerless)
            && !entity.GetCurrentState().IsHasSubState(EnumSubState.LeapPrepare)
            && !entity.GetCurrentState().IsHasSubState(EnumSubState.Leaping)
            && !entity.GetCurrentState().IsHasSubState(EnumSubState.LeapCancel);
    }

    /// <summary>
    /// 是否可以取消跃迁
    /// </summary>
    /// <returns></returns>
    private bool IsCanCancelLeap()
    {
        GameplayProxy gameplayProxy = GameFacade.Instance.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;
        SpacecraftEntity entity = gameplayProxy.GetEntityById<SpacecraftEntity>(gameplayProxy.GetMainPlayerUID());

        return !entity.GetCurrentState().IsHasSubState(EnumSubState.LeapCancel);
    }

    /// <summary>
    /// 热键按下时
    /// </summary>
    /// <param name="state"></param>
    private void OnHoldKeyEvent(HotkeyCallback state)
    {
        if (state.performed)
        {
            if (m_CurrentIndex != -1)
            {
                if (IsCanLeap())
                {     
                    NetworkManager.Instance.GetSceneController().RequestStartLeap(MapManager.GetInstance().GetCurrentAreaUid(), m_LeapCfgs[m_CurrentIndex].LeapId);
                }
                else if (IsCanCancelLeap())
                {
                    NetworkManager.Instance.GetSceneController().RequestStopLeap(MapManager.GetInstance().GetCurrentAreaUid(), m_LeapCfgs[m_CurrentIndex].LeapId);
                }
            }
        }
    }

    private void OnPressKeyEvent(HotkeyCallback state)
    {
        if (state.performed)
        {
            if (m_CurrentIndex != -1)
            {
                if (IsCanLeap())
                {
                    NetworkManager.Instance.GetSceneController().RequestStartLeap(MapManager.GetInstance().GetCurrentAreaUid(), m_LeapCfgs[m_CurrentIndex].LeapId);
                }
                else if (IsCanCancelLeap())
                {
                    NetworkManager.Instance.GetSceneController().RequestStopLeap(MapManager.GetInstance().GetCurrentAreaUid(), m_LeapCfgs[m_CurrentIndex].LeapId);
                }
            }
        }
    }

    #endregion

    #region 消息处理

    /// <summary>
    /// 输入状态改变时
    /// </summary>
    protected override void OnInputMapChanged()
    {
        m_Root.gameObject.SetActive(!IsWatchOrUIInputMode());
    }

    /// <summary>
    /// 跃迁开始
    /// </summary>
    /// <param name="leapID">跃迁点ID</param>
    private void OnLeapStart(ulong leapID)
    {
        for (int i = 0; i < m_LeapCfgs.Count; i++)
        {
            if (m_LeapCfgs[i].LeapId == leapID)
            {
                m_CurrentIndex = i;
                break;
            }
        }
        //Debug.LogError("OnLeapStart : " + leapID + "   :  " + m_CurrentIndex);

        SetLeapState(LEAP_PHASE.READY);
    }

    /// <summary>
    /// 状态改变
    /// </summary>
    private void OnStateChanged()
    {
        GameplayProxy proxy = Facade.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;
        SpacecraftEntity entity = proxy.GetEntityById<SpacecraftEntity>(proxy.GetMainPlayerUID());
        if (entity != null)
        {
            //Debug.LogError(entity.GetCurrentState().IsHasSubState(EnumSubState.LeapPrepare)+", "+ entity.GetCurrentState().IsHasSubState(EnumSubState.Leaping)+", "+ entity.GetCurrentState().IsHasSubState(EnumSubState.LeapCancel));
            if(entity.GetCurrentState().IsHasSubState(EnumSubState.LeapPrepare))
            {
                OnLeapStart(proxy.GetLeapTargetAreaUid());
                SetLeapState(LEAP_PHASE.READY);
            }
            else if(entity.GetCurrentState().IsHasSubState(EnumSubState.Leaping))
            {
                SetLeapState(LEAP_PHASE.LEAP);
            }
            else
            {
                SetLeapState(LEAP_PHASE.NORMAL);
            }
        }
        else
        {
            SetLeapState(LEAP_PHASE.NORMAL);
        }
    }

    /// <summary>
    /// 设置跃迁状态
    /// </summary>
    /// <param name="newState">新的跃迁状态</param>
    private void SetLeapState(LEAP_PHASE newState)
    {
        if (m_LeapState == newState) return;

        //Debug.LogError("    ->  " + m_LeapState + "  >  " + newState + "    :  " + m_CurrentIndex);
        m_LeapState = newState;

        m_ReadyBox.gameObject.SetActive(m_LeapState == LEAP_PHASE.READY);

        if(m_LeapState ==  LEAP_PHASE.NORMAL && m_CurrentIndex != -1)
        {
            m_HotKeyBox.SetParent(m_PointBox.GetChild(m_CurrentIndex).Find("JumpTips/TextBox/HotKeyBox"));
            m_HotKeyBox.gameObject.SetActive(false);
            m_HotKeyBox.gameObject.SetActive(true);
            SetHotKeyVisible("f_hold", true);
            SetHotKeyVisible("f_press", false);
        }
        else if (m_LeapState == LEAP_PHASE.READY)
        {
            m_HotKeyBox.SetParent(m_ReadyHotkeyBox);
            m_HotKeyBox.gameObject.SetActive(false);
            m_HotKeyBox.gameObject.SetActive(true);
            SetHotKeyVisible("f_hold", false);
            SetHotKeyVisible("f_press", true);
        }
        else
        {
            m_HotKeyBox.gameObject.SetActive(false);
            m_HotKeyBox.SetParent(GetTransform());
            SetHotKeyVisible("f_hold", false);
            SetHotKeyVisible("f_press", false);
        }

    }

    #endregion


    /// <summary>
    /// 重建所有跳点
    /// </summary>
    private void ResetJumpPoints()
    {
        CfgEternityProxy eternityProxy = Facade.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
        GameLocalizationProxy localizationProxy = Facade.RetrieveProxy(ProxyName.GameLocalizationProxy) as GameLocalizationProxy;

        m_LeapCfgs.Clear();

        int index = 0;

        Eternity.FlatBuffer.Map map = eternityProxy.GetCurrentMapData();
        for (int i = 0; i < map.AreaListLength; i++)
        {
            Eternity.FlatBuffer.Area area = map.AreaList(i).Value;
            for (int j = 0; j < area.LeapListLength; j++)
            {
                LeapItem leapVO = area.LeapList(j).Value;
                
                m_LeapCfgs.Add(leapVO);

                if (index >= m_PointBox.childCount)
                    Object.Instantiate(m_PointTemplate, m_PointBox);

                if (index >= m_ArrowBox.childCount)
                    Object.Instantiate(m_ArrowTemplate, m_ArrowBox);

                Transform leapPointItem = m_PointBox.GetChild(index);
                Image icon = leapPointItem.Find("Icon/Image_Icon").GetComponent<Image>();
                TMP_Text nameField1 = leapPointItem.Find("JumpTips/TextBox/Name").GetComponent<TMP_Text>();
                TMP_Text distanceField = leapPointItem.Find("JumpTips/TextBox/Distance").GetComponent<TMP_Text>();

                Transform leapPointArrow = m_ArrowBox.GetChild(index);
                Image arrowIcon = FindComponent<Image>(leapPointArrow, "Icon");

                if (leapVO.IconConfId == 0)
                {
                    UIUtil.SetIconImage(icon, 31320);

                    arrowIcon.gameObject.SetActive(false);
                }
                else
                {
                    UIUtil.SetIconImage(icon, leapVO.IconConfId);

                    arrowIcon.gameObject.SetActive(true);
                    UIUtil.SetIconImage(arrowIcon, leapVO.IconConfId);
                }

                nameField1.text = localizationProxy.GetString("leap_name_" + eternityProxy.GetCurrentGamingMapId() + "_" + leapVO.LeapId);
                distanceField.text = "";

                //Debug.LogError("LeapType : "+leapVO.LeapType+", LeapID : " + leapVO.LeapId + " , MainLeapId : " + leapVO.MainLeapId + ", Name : " + nameField1.text + ", VisibleLeapList[" + string.Join(",", leapVO.GetVisibleLeapListArray()) + "]");

                index++;
            }
        }

        for (int i = m_PointBox.childCount - 1; i >= index; i--)
        {
            m_PointBox.GetChild(i).gameObject.SetActive(false);
        }

        for (int i = m_ArrowBox.childCount - 1; i >= index; i--)
        {
            m_ArrowBox.GetChild(i).gameObject.SetActive(false);
        }
    }

    private List<uint> m_HaveMissionFlagPoints = new List<uint>();

    /// <summary>
    /// 更新UI
    /// </summary>
    protected override void Update()
    {
        GameplayProxy gameplayProxy = Facade.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;
        SpacecraftEntity main = gameplayProxy.GetEntityById<SpacecraftEntity>(gameplayProxy.GetMainPlayerUID());

        if (main == null) return;

        bool needCrosshair = !IsWatchOrUIInputMode() && !IsDead() && !IsLeaping() && !IsBattling();
        bool hasMineral = false;
        if (needCrosshair)
        {
			SpacecraftEntity target = m_RaycastProxy.Raycast();
			hasMineral = target != null && target.GetHeroType() == Assets.Scripts.Define.KHeroType.htMine && target.GetAttribute(AttributeName.kHP) > 0;
		}
		m_Crosshair.gameObject.SetActive(needCrosshair);
        m_JumpCrosshair.gameObject.SetActive(!hasMineral);
        m_MineralCrosshair.gameObject.SetActive(hasMineral);

        float w = m_Root.rect.width;
        float h = m_Root.rect.height;

        m_ScaleBox.localScale = w > h ? new Vector3(1, h / w, 1) : new Vector3(w / h, 1, 1);

        m_HaveMissionFlagPoints.Clear();
        List<TaskTrackingProxy.TrackingInfo> trackingList = m_TaskTracking.GetAllTrackings();
        for (int i = 0; i < trackingList.Count; i++)
        {
            if (trackingList[i].Mode == TaskTrackingProxy.TrackingMode.LeapPoint)
                m_HaveMissionFlagPoints.Add((uint)trackingList[i].LeapPointID);
        }

        ulong currentAreaID = MapManager.GetInstance().GetCurrentAreaUid();
        uint[] visibleLeapIDs = null;
        for (int i = 0; i < m_LeapCfgs.Count; i++)
        {
            if (m_LeapCfgs[i].LeapId == currentAreaID)
            {
                visibleLeapIDs = m_LeapCfgs[i].GetVisibleLeapListArray();
                break;
            }
        }

        int hitedPointIndex = -1;
        bool allowVisible = !IsDead() && !IsWatchOrUIInputMode();// && !IsBattling();
        Vector2 screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);
        for (int i = 0; i < m_LeapCfgs.Count; i++)
        {
            RectTransform point = m_PointBox.GetChild(i).GetComponent<RectTransform>();
            RectTransform arrow = m_ArrowBox.GetChild(i).GetComponent<RectTransform>();

            if (!allowVisible)
            {
                arrow.gameObject.SetActive(false);
                point.gameObject.SetActive(false);
                continue;
            }

            LeapItem leapVO = m_LeapCfgs[i];
            uint leapID = (uint)leapVO.LeapId;

            if (visibleLeapIDs != null && System.Array.IndexOf(visibleLeapIDs, leapID) == -1)
            {
                arrow.gameObject.SetActive(false);
                point.gameObject.SetActive(false);
                continue;
            }

            Vector3 targetWorldPosition = gameplayProxy.WorldPositionToServerAreaOffsetPosition(new Vector3((float)leapVO.Position.Value.X, (float)leapVO.Position.Value.Y, (float)leapVO.Position.Value.Z));

            //目标过远，忽略
            float distance = Vector3.Distance(targetWorldPosition, main.transform.position);
            if (distance <= leapVO.Range * GameConstant.METRE_PER_UNIT)
            {
                arrow.gameObject.SetActive(false);
                point.gameObject.SetActive(false);
                continue;
            }

            Vector3 viewportPoint = Camera.main.WorldToViewportPoint(targetWorldPosition);
            if (viewportPoint.x >= 0 && viewportPoint.y >= 0 && viewportPoint.x <= 1 && viewportPoint.y <= 1 && viewportPoint.z >= Camera.main.nearClipPlane)
            {
                //屏幕内
                Vector3 screenPoint = Camera.main.WorldToScreenPoint(targetWorldPosition);

                Vector2 iconPosition;
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(m_PointBox, screenPoint, m_Camera, out iconPosition))
                {
                    arrow.gameObject.SetActive(false);
                    point.gameObject.SetActive(true);

                    point.anchoredPosition = iconPosition;
                    point.localScale = Vector3.one * LEAP_POINT_SCALE;
                    point.Find("JumpTips/TextBox/Distance").GetComponent<TMP_Text>().text = FormatMetre(distance * GameConstant.METRE_PER_UNIT);

                    if (hitedPointIndex == -1)
                    {
                        Vector2 crosshairPosition;
                        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(m_Crosshair, screenPoint, m_Camera, out crosshairPosition))
                        {
                            //确定选中项
                            if (crosshairPosition.magnitude <= m_Crosshair.rect.width * 0.5f)
                                hitedPointIndex = i;
                        }
                    }
                }
            }
            else
            {
                if(m_HaveMissionFlagPoints.Contains(leapID))
                {
                    arrow.gameObject.SetActive(false);
                    point.gameObject.SetActive(false);
                    continue;
                }

                   //屏幕外
                   point.gameObject.SetActive(false);
                arrow.gameObject.SetActive(true);

                Vector3 inCameraPosition = Camera.main.transform.InverseTransformPoint(targetWorldPosition);

                Vector2 arrowPosition = m_ScaleBox.InverseTransformPoint(m_Root.TransformPoint(((Vector2)inCameraPosition)));
                arrowPosition = arrowPosition.normalized * (Mathf.Max(m_Root.rect.width, m_Root.rect.height) / 2);
                arrowPosition = m_Root.InverseTransformPoint(m_ScaleBox.TransformPoint(arrowPosition));


                arrow.anchoredPosition = arrowPosition;
                arrow.localScale = Vector3.one * ARROW_SCALE;

                RectTransform arrowArrow = FindComponent<RectTransform>(arrow, "Arrow");
                arrowArrow.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(inCameraPosition.y, inCameraPosition.x) * Mathf.Rad2Deg - 90);
            }
        }

        //如果处理战斗状态，强制取消选中
        if (IsBattling())
            hitedPointIndex = -1;

        //只能在NORMAL状态下换目标
        if (m_LeapState == LEAP_PHASE.NORMAL && m_CurrentIndex != hitedPointIndex)
        {
            if (m_CurrentIndex != -1)
            {
                RectTransform point = m_PointBox.GetChild(m_CurrentIndex).GetComponent<RectTransform>();
                Animator tipAnimator = point.Find("JumpTips").GetComponent<Animator>();
                Animator iconBgAnimator = point.Find("IconBg").GetComponent<Animator>();
                Animator iconAnimator = point.Find("Icon").GetComponent<Animator>();
                Transform missionBox = point.Find("MissionBox");

                tipAnimator.ResetTrigger("Closed");
                tipAnimator.ResetTrigger("Open");
                tipAnimator.SetTrigger("Closed");

                iconBgAnimator.ResetTrigger("Closed");
                iconBgAnimator.ResetTrigger("Open");
                iconBgAnimator.SetTrigger("Closed");

                iconAnimator.SetBool("selected", false);

                missionBox.gameObject.SetActive(true);
            }
            //Debug.LogError(m_LeapState + "=>" + m_CurrentIndex + " -> " + hitedPointIndex);
            m_CurrentIndex = hitedPointIndex;

            if (m_CurrentIndex != -1)
            {
                RectTransform point = m_PointBox.GetChild(m_CurrentIndex).GetComponent<RectTransform>();
                Animator tipAnimator = point.Find("JumpTips").GetComponent<Animator>();
                Animator iconBgAnimator = point.Find("IconBg").GetComponent<Animator>();
                Animator iconAnimator = point.Find("Icon").GetComponent<Animator>();
                Transform missionBox = point.Find("MissionBox");

                tipAnimator.ResetTrigger("Closed");
                tipAnimator.ResetTrigger("Open");
                tipAnimator.SetTrigger("Open");

                iconBgAnimator.ResetTrigger("Closed");
                iconBgAnimator.ResetTrigger("Open");
                iconBgAnimator.SetTrigger("Open");

                iconAnimator.SetBool("selected", true);

                missionBox.gameObject.SetActive(false);

                m_HotKeyBox.SetParent(point.Find("JumpTips/TextBox/HotKeyBox"));
                m_HotKeyBox.gameObject.SetActive(true);
                SetHotKeyVisible("f_hold", true);
                SetHotKeyVisible("f_press", false);
            }
            else
            {
                m_HotKeyBox.SetParent(GetTransform());
                m_HotKeyBox.gameObject.SetActive(false);
                SetHotKeyVisible("f_hold", false);
                SetHotKeyVisible("f_press", false);
            }
        }

        UpdateMissionState();
    }

    /// <summary>
    /// 更新任务状态
    /// </summary>
    private void UpdateMissionState()
    {
        TaskTrackingProxy trakingProxy = Facade.RetrieveProxy(ProxyName.TaskTrackingProxy) as TaskTrackingProxy;

        for (int i = 0; i < m_LeapCfgs.Count; i++)
        {
            LeapItem leapVO = m_LeapCfgs[i];
            uint leapID = (uint)leapVO.LeapId;

            RectTransform point = m_PointBox.GetChild(i).GetComponent<RectTransform>();
            if(point.gameObject.activeSelf)
            {
                RectTransform box1 = point.Find("MissionBox").GetComponent<RectTransform>();
                RectTransform box2 = point.Find("JumpTips/MissionBox").GetComponent<RectTransform>();

                int index = 0;

                List<TaskTrackingProxy.TrackingInfo> trackingList = trakingProxy.GetLeapTrackings(leapID);
                if (trackingList != null)
                {
                    for (int j = 0; j < trackingList.Count; j++)
                    {
                        TaskTrackingProxy.TrackingInfo tracking = trackingList[j];
                        RectTransform icon1 = index < box1.childCount ? box1.GetChild(j).GetComponent<RectTransform>() : null;
                        if (icon1 == null)
                        {
                            if (m_MissionIdleBox.childCount > 0)
                                icon1 = m_MissionIdleBox.GetChild(0).GetComponent<RectTransform>();
                            else
                                icon1 = Object.Instantiate(m_MissionTemplate, box1);

                            icon1.SetParent(box1);
                            icon1.gameObject.SetActive(true);
                        }

                        RectTransform icon2 = index < box2.childCount ? box2.GetChild(j).GetComponent<RectTransform>() : null;
                        if (icon2 == null)
                        {
                            if (m_MissionIdleBox.childCount > 0)
                                icon2 = m_MissionIdleBox.GetChild(0).GetComponent<RectTransform>();
                            else
                                icon2 = Object.Instantiate(m_MissionTemplate, box2);

                            icon2.SetParent(box2);
                            icon2.gameObject.SetActive(true);
                        }

                        string missionIcon = GetMissionIcon(tracking.MissionType);
                        UIUtil.SetIconImage(icon1.Find("Icon").GetComponent<Image>(), GameConstant.FUNCTION_ICON_ATLAS_ASSETADDRESS, missionIcon);
                        UIUtil.SetIconImage(icon2.Find("Icon").GetComponent<Image>(), GameConstant.FUNCTION_ICON_ATLAS_ASSETADDRESS, missionIcon);

                        bool missionFinished = tracking.MissionState == MissionState.Finished;
                        icon1.GetComponent<Animator>().SetBool("Finished", missionFinished);
                        icon2.GetComponent<Animator>().SetBool("Finished", missionFinished);

                        index++;
                    }
                }

                while(index<box1.childCount)
                {
                    box1.GetChild(index).gameObject.SetActive(false);
                    box1.GetChild(index).SetParent(m_MissionIdleBox);
                }
                while (index < box2.childCount)
                {
                    box2.GetChild(index).gameObject.SetActive(false);
                    box2.GetChild(index).SetParent(m_MissionIdleBox);
                }
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
        return metre < 1000 ? string.Format("{0:N1}m", metre) : string.Format("{0:N1}km", metre / 1000.0f);
    }
}