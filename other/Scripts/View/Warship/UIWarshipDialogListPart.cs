using Eternity.FlatBuffer;
using Eternity.FlatBuffer.Enums;
using Leyoutech.Core.Loader.Config;
using PureMVC.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class UIWarshipDialogListPart : BaseViewPart
{
    /// <summary>
    /// list预设地址
    /// </summary>
    private const string ASSET_ADDRESS = AssetAddressKey.PRELOADUI_WARSHIPCONTENTPANEL;
    /// <summary>
    /// 背包数据
    /// </summary>
    private PackageProxy m_PackageProxy;
    /// <summary>
    /// 创角Proxy
    /// </summary>
    private ServerListProxy m_ServerListProxy;
    /// <summary>
    /// 角色Proxy
    /// </summary>
    private CfgEternityProxy m_CfgEternityProxy;
    /// <summary>
    /// 船Proxy
    /// </summary>
    private ShipProxy m_ShipProxy;
    /// <summary>
    /// 获取场景船Proxy
    /// </summary>
    private SceneShipProxy m_SceneShipProxy;

    #region
    /// <summary>
	/// CanvasGroup组件
	/// </summary>
	private CanvasGroup m_CanvasGroup;

    /// <summary>
	/// 船单个物体
	/// </summary>
	private Transform m_ShipCell;
    /// <summary>
    /// 武器链表
    /// </summary>
    private Transform[] m_WeaponCellList;
    /// <summary>
    /// 转化炉单体
    /// </summary>
    private Transform m_ReformerCell;
    /// <summary>
    /// 装备链表
    /// </summary>
    private Transform[] m_EquipmentCellList;

    /// <summary>
	/// 道具字典
	/// </summary>
	private Dictionary<string, int> m_ItemIdDic;
    /// <summary>
    ///  当前船数据接口--------------------------------------
    /// </summary>
    private IShip m_CurrentShip;
    /// <summary>
    /// 武器数组数据接口
    /// </summary>
    private IWeapon[] m_Weapons;
    /// <summary>
    /// 转化炉数据接口
    /// </summary>
    private IReformer m_Reformer;
    /// <summary>
    /// 装备数组数据接口
    /// </summary>
    private IEquipment[] m_Equipment;
    /// <summary>
	/// 上次选中的按钮
	/// </summary>
	private Toggle m_BeforeToggle;
    /// <summary>
    /// 是否是船面板
    /// </summary>
    private bool m_ShipPanelInited;
    /// <summary>
    /// 船面板根物体
    /// </summary>
    private GameObject m_ShipPanelContentRoot;
    /// <summary>
    /// 船面板空物体
    /// </summary>
    private GameObject m_ShipPanelEmptyRoot;
    /// <summary>
    /// 船面板图标Image组件
    /// </summary>
    private Image m_ShipPanelIcon;
    /// <summary>
    /// 船面板名字Text组件
    /// </summary>
    private TMP_Text m_ShipPanelNameText;
    /// <summary>
    /// 船面板质量Image组件
    /// </summary>
    private Image m_ShipPanelQualityImage;
    /// <summary>
    /// 船的等级Text组件
    /// </summary>
    private TMP_Text m_ShipPanelLvText;
    /// <summary>
    /// 船mod
    /// </summary>
    private GameObject[] m_ShipMods;

    #endregion

    #region

    /// <summary>
	/// 打开界面的初始船
	/// </summary>
	//private IShip m_LastShip;

    /// <summary>
	/// 当前状态
	/// </summary>
	private WarshipPanelState m_CurrentState;
    private WarshipDialogPanel m_WarshipDialogPanel;
    private bool m_ShowAppiontIcon=true;
    /// <summary>
    /// 任命标记
    /// </summary>
    private Transform m_AppointIcon;
    #endregion

    public override void OnShow(object msg)
    {
        base.OnShow(msg);
      
        LoadViewPart(ASSET_ADDRESS, OwnerView.ListBox);
        OwnerView.PageBox.gameObject.SetActive(false);
        m_WarshipDialogPanel = OwnerView as WarshipDialogPanel;
        m_BeforeToggle = m_WarshipDialogPanel.BeforeToggle;
        m_CfgEternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
        m_ShipProxy = GameFacade.Instance.RetrieveProxy(ProxyName.ShipProxy) as ShipProxy;
        m_PackageProxy = GameFacade.Instance.RetrieveProxy(ProxyName.PackageProxy) as PackageProxy;
        m_ServerListProxy = GameFacade.Instance.RetrieveProxy(ProxyName.ServerListProxy) as ServerListProxy;
        m_SceneShipProxy = GameFacade.Instance.RetrieveProxy(ProxyName.SceneShipProxy) as SceneShipProxy;
        m_WarshipDialogPanel.AppointShip = m_ShipProxy.GetAppointWarShip();
    
        if (m_WarshipDialogPanel.LastShip != null)
        {
            if (m_ShipProxy.GetAppointWarShip().GetTID() != m_WarshipDialogPanel.LastShip.GetTID())
            {
                m_SceneShipProxy.HideShip();
                m_SceneShipProxy.ShowShip();
            }
        }
        m_WarshipDialogPanel.LastShip = m_ShipProxy.GetAppointWarShip();
        if (msg != null)
        {
            m_CurrentShip = msg as IShip;
        }
        else
        {
            m_CurrentShip = m_ShipProxy.GetAppointWarShip();
        }
        m_ShowAppiontIcon = true;
        if (m_WarshipDialogPanel.AppointShip.GetTID()!= m_CurrentShip.GetTID())
        {
            m_ShowAppiontIcon = false;
        }
        if (m_CurrentShip == null)
        {
            return;
        }
        if (m_CurrentShip == null)
        {
            Debug.Log("没有战船");
            return;
        }

        m_Weapons = new IWeapon[m_CurrentShip.GetWeaponContainer().GetCurrentSizeMax()];
        IWeapon[] tempWeapons = m_CurrentShip.GetWeaponContainer().GetWeapons();
        if (tempWeapons != null)
        {
            for (int i = 0; i < tempWeapons.Length; i++)
            {
                m_Weapons[tempWeapons[i].GetPos()] = tempWeapons[i];
            }
            tempWeapons = null;
        }

        m_Equipment = new IEquipment[m_CurrentShip.GetEquipmentContainer().GetCurrentSizeMax()];
        IEquipment[] tempEquips = m_CurrentShip.GetEquipmentContainer().GetEquipments();
        if (tempEquips != null)
        {
            for (int i = 0; i < tempEquips.Length; i++)
            {
                m_Equipment[tempEquips[i].GetPos()] = tempEquips[i];
            }
            tempEquips = null;
        }

        m_Reformer = m_CurrentShip.GetReformerContainer().GetReformer();
    }


    public override void OnHide()
    {
        base.OnHide();
        if (m_WarshipDialogPanel.LastShip != null)
        {
            if (m_ShipProxy.GetAppointWarShip().GetTID() != m_WarshipDialogPanel.LastShip.GetTID())
            {
                m_SceneShipProxy.HideShip();
                m_SceneShipProxy.ShowShip();

            }
        }
        m_WarshipDialogPanel.LastShip = m_ShipProxy.GetAppointWarShip();
        m_WarshipDialogPanel.BeforeToggle = m_BeforeToggle;
    }

    protected override void OnViewPartLoaded()
    {
        //GetTransform().localPosition = new Vector3(-210,360,300);
        ShowCharacter();
        m_CanvasGroup = GetTransform().GetOrAddComponent<CanvasGroup>();
        m_ShipCell = FindComponent<Transform>("Item/Content1");
        m_AppointIcon = FindComponent<Transform>("AppointIcon");
        m_AppointIcon.gameObject.SetActive(m_ShowAppiontIcon);
        m_WeaponCellList = new Transform[2] {
            FindComponent<Transform>("Item/Content2"),
            FindComponent<Transform>("Item/Content3")
        };
        m_ReformerCell = FindComponent<Transform>("Item/Content4");
        m_EquipmentCellList = new Transform[6]
        {
            FindComponent<Transform>("Item/Content5"),
            FindComponent<Transform>("Item/Content6"),
            FindComponent<Transform>("Item/Content7"),
            FindComponent<Transform>("Item/Content8"),
            FindComponent<Transform>("Item/Content9"),
            FindComponent<Transform>("Item/Content10")
        };

        m_ItemIdDic = new Dictionary<string, int>();
        ToggleGroup toggleGroup = GetTransform().GetOrAddComponent<ToggleGroup>();
        int i;
        for (i = 0; i < m_WeaponCellList.Length; i++)
        {
            m_ItemIdDic.Add(m_WeaponCellList[i].name, i);
            m_WeaponCellList[i].GetComponent<Toggle>().group = toggleGroup;
            UIEventListener.UIEventListener.AttachListener(m_WeaponCellList[i], i).onClick = OnWeaponClick;
            UIEventListener.UIEventListener.AttachListener(m_WeaponCellList[i], i).onSelect = OnToggleValueChange;
            UIEventListener.UIEventListener.AttachListener(m_WeaponCellList[i], i).onEnter = OnCellEnter;
            UIEventListener.UIEventListener.AttachListener(m_WeaponCellList[i], i).onExit = OnCellExit;
        }
        for (i = 0; i < m_EquipmentCellList.Length; i++)
        {
            m_ItemIdDic.Add(m_EquipmentCellList[i].name, i);
            m_EquipmentCellList[i].GetComponent<Toggle>().group = toggleGroup;
            UIEventListener.UIEventListener.AttachListener(m_EquipmentCellList[i], i).onClick = OnEquipmentClick;
            UIEventListener.UIEventListener.AttachListener(m_EquipmentCellList[i], i).onSelect = OnToggleValueChange;
            UIEventListener.UIEventListener.AttachListener(m_EquipmentCellList[i], i).onEnter = OnCellEnter;
            UIEventListener.UIEventListener.AttachListener(m_EquipmentCellList[i], i).onExit = OnCellExit;
        }
        m_ItemIdDic.Add(m_ReformerCell.name, 0);
        m_ReformerCell.GetComponent<Toggle>().group = toggleGroup;
        UIEventListener.UIEventListener.AttachListener(m_ReformerCell).onClick = OnReformerClick;
        UIEventListener.UIEventListener.AttachListener(m_ReformerCell).onSelect = OnToggleValueChange;
        UIEventListener.UIEventListener.AttachListener(m_ReformerCell).onEnter = OnCellEnter;
        UIEventListener.UIEventListener.AttachListener(m_ReformerCell).onExit = OnCellExit;


        State.GetAction(UIAction.Common_Select).Callback -= OnEnterClick;
        State.GetAction(UIAction.Hangar_Appoint).Callback -= OnAppointClick;
        State.GetAction(UIAction.Assemble_WarshipChips).Callback -= OnModClick;
        State.GetAction(UIAction.Assemble_Hangar).Callback -= OnOpenShipPanel;

        State.GetAction(UIAction.Common_Select).Callback += OnEnterClick;
        State.GetAction(UIAction.Hangar_Appoint).Callback += OnAppointClick;
        State.GetAction(UIAction.Assemble_WarshipChips).Callback += OnModClick;
        State.GetAction(UIAction.Assemble_Hangar).Callback += OnOpenShipPanel;


        OnRefresh();
    }

    protected override void OnViewPartUnload()
    {
        State.GetAction(UIAction.Common_Select).Callback -= OnEnterClick;
        State.GetAction(UIAction.Hangar_Appoint).Callback -= OnAppointClick;
        State.GetAction(UIAction.Assemble_WarshipChips).Callback -= OnModClick;
        State.GetAction(UIAction.Assemble_Hangar).Callback -= OnOpenShipPanel;
    }

    /// <summary>
    /// 刷新数据
    /// </summary>
    /// <param name="msg"></param>
    public void OnRefresh()
    {
        m_CanvasGroup.interactable = true;
        if (m_CurrentShip == null)
        {
            return;
        }
        SendViewerChange();
        SetData();
        RefreshHotKey();
        if (m_BeforeToggle && m_BeforeToggle.gameObject)
        {
            OnToggleValueChange(m_BeforeToggle.gameObject);
            EventSystemUtils.SetFocus(m_BeforeToggle);
        }
        else
        {
            OnToggleValueChange(m_WeaponCellList[0].gameObject);
            EventSystemUtils.SetFocus(m_WeaponCellList[0].GetComponent<Toggle>());
        }
        MsgWarshipPanelState msg = MessageSingleton.Get<MsgWarshipPanelState>();
        msg.State = WarshipPanelState.Main;
        GameFacade.Instance.SendNotification(NotificationName.MSG_WARSHIP_PANEL_CHANGE, msg);
    }

    private void OnCellEnter(GameObject go, params object[] args)
    {
        if (InputManager.Instance.CurrentInputDevice != InputManager.GameInputDevice.KeyboardAndMouse)
        {
            //FocusTo(go);
            EventSystem.current.SetSelectedGameObject(go);
            OnToggleValueChange(go, args);
        }
    }

    private void OnCellExit(GameObject go, params object[] args)
    {
        if (InputManager.Instance.CurrentInputDevice != InputManager.GameInputDevice.KeyboardAndMouse)
        {
            GameObject focus = null;
            //FocusTo(focus);
           // EventSystem.current.SetSelectedGameObject(go);
            EventSystem.current.SetSelectedGameObject(null);
            if (m_BeforeToggle)
            {
                m_BeforeToggle.isOn = false;
                m_BeforeToggle.GetComponent<Animator>().SetBool("IsOn", false);
            }
            SetHotKeyEnabled(UIAction.Common_Select, false);
        }
    }

    private void OnToggleValueChange(GameObject go, params object[] args)
    {
        if (m_BeforeToggle)
        {
            m_BeforeToggle.isOn = false;
            m_BeforeToggle.GetComponent<Animator>().SetBool("IsOn", false);
        }
        m_BeforeToggle = go.GetComponent<Toggle>();
        m_BeforeToggle.isOn = true;
        m_BeforeToggle.GetComponent<Animator>().SetBool("IsOn", true);
        SetHotKeyEnabled(UIAction.Common_Select, true);
    }

    /// <summary>
    /// 设置热键可见性
    /// </summary>
    /// <param name="hotKeyID">hotKeyID</param>
    /// <param name="enable">可见性</param>
    public void SetHotKeyEnabled(string hotKeyID, bool enable, bool isHold = false)
    {
        State.GetAction(hotKeyID).Enabled = enable;
    }

    /// <summary>
	/// 设置模型图
	/// </summary>
	private void SendViewerChange()
    {
        if (m_CurrentShip != null)
        {
            Msg3DViewerInfo viewerInfo = MessageSingleton.Get<Msg3DViewerInfo>();
            Model model = m_CfgEternityProxy.GetModel(m_CurrentShip.GetBaseConfig().Model);
            viewerInfo.Model = model;
            viewerInfo.IsShip = true;
            viewerInfo.position = new Vector3(-271.7f, -53.3f, 0);
            viewerInfo.size = new Vector2(1775, 1209);
            GameFacade.Instance.SendNotification(NotificationName.MSG_3DVIEWER_CHANGE, viewerInfo);
        }
    }


    /// <summary>
	/// 刷新数据
	/// </summary>
	private void SetData()
    {
        SetTips();
        SetShipData();
        SetWeaponDataList();
        SetReformerData();
        SetEquipmentDataList();
    }

    #region 刷新数据
    public override NotificationName[] ListNotificationInterests()
    {
        return new NotificationName[]
        {
            NotificationName.MSG_SHIP_DATA_CHANGED,
            NotificationName.MSG_PACKAGE_ITEM_MOVE,

            NotificationName.MSG_WARSHIP_PANEL_CHANGE,
     
            NotificationName.MSG_CLOSE_MAIN_PANEL,
        
        };
    }

    public override void HandleNotification(INotification notification)
    {
        switch (notification.Name)
        {
            case NotificationName.MSG_SHIP_DATA_CHANGED:
                RefreshUsingItem(notification.Body as MsgShipDataChanged);
                break;
            case NotificationName.MSG_PACKAGE_ITEM_MOVE:
                RefreshHotKey();
                break;
            case NotificationName.MSG_WARSHIP_PANEL_CHANGE:
                PanelOnChange((notification.Body as MsgWarshipPanelState).State, notification.Body as MsgWarshipPanelState);
                break;
            case NotificationName.MSG_CLOSE_MAIN_PANEL:
                m_WarshipDialogPanel.BeforeToggle = m_BeforeToggle = null;
                break;

        }
    }

    /// <summary>
    /// 子面板切换
    /// </summary>
    /// <param name="state"></param>
    private void PanelOnChange(WarshipPanelState state, MsgWarshipPanelState data = null)
    {
        m_CurrentState = state;

        switch (state)
        {
            case WarshipPanelState.Main:
                break;
            case WarshipPanelState.ModMainShip:
            case WarshipPanelState.ModMainWeapon:
                UIManager.Instance.OpenPanel(UIPanel.WarshipModPanel, data);
                break;
            case WarshipPanelState.ListWeapon:
                UIManager.Instance.OpenPanel(UIPanel.WarshipListPanel, data);
                break;
            case WarshipPanelState.ListMod:
                UIManager.Instance.OpenPanel(UIPanel.WarshipChipPanel, data);
                break;
            case WarshipPanelState.ListReformer:
                UIManager.Instance.OpenPanel(UIPanel.WarshipReformerPanel, data);
                break;
            case WarshipPanelState.ListEquip:
                switch ((EquipmentL1)data.CurrentEquipmentData.ContainerPOS+1)
                {
                    case EquipmentL1.Processor:
                        UIManager.Instance.OpenPanel(UIPanel.WarshipProcessorPanel, data);
                        break;
                    case EquipmentL1.Armor:
                        UIManager.Instance.OpenPanel(UIPanel.WarshipArmorPanel, data);
                        break;
                    case EquipmentL1.Reactor:
                        UIManager.Instance.OpenPanel(UIPanel.WarshipReactorPanel, data);
                        break;
                    case EquipmentL1.AuxiliaryUnit:
                        UIManager.Instance.OpenPanel(UIPanel.WarshipAuxiliaryUnitPanel, data);
                        break;
                    case EquipmentL1.Nanobot:
                        UIManager.Instance.OpenPanel(UIPanel.WarshipNanobotPanel, data);
                        break;
                    case EquipmentL1.SignalGenerator:
                        UIManager.Instance.OpenPanel(UIPanel.WarshipSignalGeneratorPanel, data);
                        break;
                    default:
                        break;
                }
                    break;
        }
    }

    /// <summary>
    /// 刷新热键
    /// </summary>
    private void RefreshHotKey()
    {
        if (m_CurrentShip.GetUID() == m_ShipProxy.GetAppointWarShip().GetUID())
        {
            SetHotKeyEnabled(UIAction.Hangar_Appoint, false);
        }
        else
        {
            SetHotKeyEnabled(UIAction.Hangar_Appoint, true);
        }
    }

    /// <summary>
    /// 刷新利用道具
    /// </summary>
    /// <param name="data"></param>
    private void RefreshUsingItem(MsgShipDataChanged data)
    {
        if (data.ShipUid == m_CurrentShip.GetUID())
        {
            OnShow(null);
            SetData();
        }
    }
    #endregion

    #region SetTips
    /// <summary>
    /// 设置tips
    /// </summary>
    private void SetTips()
    {
        MsgChildShowTipsInfo msg = MessageSingleton.Get<MsgChildShowTipsInfo>();
        msg.TipData = m_CurrentShip;
        State.SetTipData(msg.TipData);
        GameFacade.Instance.SendNotification(NotificationName.MSG_CHILD_TIPS_CHANGE, msg);
    }

    #endregion

    #region SetShipData
    private void InitShipPanel()
    {
        if (m_ShipPanelInited)
        {
            return;
        }
        m_ShipPanelInited = true;
        m_ShipPanelContentRoot = TransformUtil.FindUIObject<Transform>(m_ShipCell, "Content").gameObject;
        m_ShipPanelEmptyRoot = TransformUtil.FindUIObject<Transform>(m_ShipCell, "Empty").gameObject;
        m_ShipPanelIcon = TransformUtil.FindUIObject<Image>(m_ShipCell, "Content/Image_Icon");
        m_ShipPanelNameText = TransformUtil.FindUIObject<TMP_Text>(m_ShipCell, "Content/Label_Name");
        m_ShipPanelQualityImage = TransformUtil.FindUIObject<Image>(m_ShipCell, "Content/Image_Quality");
        m_ShipPanelLvText = TransformUtil.FindUIObject<TMP_Text>(m_ShipCell, "Content/Label_Lv");

        int modCount = TransformUtil.FindUIObject<Transform>(m_ShipCell, "Content/MOD").transform.childCount;
        m_ShipMods = new GameObject[modCount];
        for (int i = 0; i < modCount; i++)
        {
            m_ShipMods[i] = TransformUtil.FindUIObject<Transform>(m_ShipCell, "Content/MOD").transform.GetChild(i).gameObject;
        }
    }
    /// <summary>
    /// 设置船具体属性数据
    /// </summary>
    private void SetShipData()
    {
        InitShipPanel();

        m_ShipPanelEmptyRoot.SetActive(false);
        m_ShipPanelContentRoot.SetActive(true);

        UIUtil.SetIconImage(m_ShipPanelIcon, TableUtil.GetItemIconBundle(m_CurrentShip.GetTID()), TableUtil.GetItemIconImage(m_CurrentShip.GetTID()));
        m_ShipPanelNameText.text = TableUtil.GetItemName((int)m_CurrentShip.GetTID());
        m_ShipPanelQualityImage.color = ColorUtil.ShipColor.GetColorByPrime(m_CurrentShip.GetConfig().IsPrime > 0);
        m_ShipPanelLvText.text = "Lv." + PadLeft(m_CurrentShip.GetLv().ToString());

        uint modCount = m_CurrentShip.GetGeneralModContainer().GetCurrentSizeMax() + m_CurrentShip.GetExclusivelyModContainer().GetCurrentSizeMax();
        for (int i = 0; i < m_ShipMods.Length; i++)
        {
            m_ShipMods[i].gameObject.SetActive(i < modCount);
            m_ShipMods[i].transform.GetChild(0).GetComponent<Image>().enabled = false;
        }

        IMod[] mods = m_CurrentShip.GetGeneralModContainer().GetMods();
        if (mods != null && mods.Length > 0)
        {
            for (int i = 0; i < mods.Length; i++)
            {
                m_ShipMods[mods[i].GetPos()].transform.GetChild(0).GetComponent<Image>().enabled = true;
                m_ShipMods[mods[i].GetPos()].transform.GetChild(0).GetComponent<Image>().color = ColorUtil.GetColorByItemQuality(mods[i].GetBaseConfig().Quality);
            }
        }
        mods = m_CurrentShip.GetExclusivelyModContainer().GetMods();
        uint GeneralModCount = m_CurrentShip.GetGeneralModContainer().GetCurrentSizeMax();
        if (mods != null && mods.Length > 0)
        {
            for (int i = 0; i < mods.Length; i++)
            {
                m_ShipMods[mods[i].GetPos() + GeneralModCount].transform.GetChild(0).GetComponent<Image>().enabled = true;
                m_ShipMods[mods[i].GetPos() + GeneralModCount].transform.GetChild(0).GetComponent<Image>().color = ColorUtil.GetColorByItemQuality(mods[i].GetBaseConfig().Quality);
            }
        }

    }
    #endregion

    #region SetWeaponData
    /// <summary>
    /// 设置各种武器数据
    /// </summary>
    private void SetWeaponDataList()
    {
        if (m_Weapons != null)
        {
            for (int i = 0; i < m_Weapons.Length; i++)
            {
                SetWeaponData(m_Weapons[i], m_WeaponCellList[i]);
            }
        }
    }

    /// <summary>
    /// 设置单个武器数据
    /// </summary>
    /// <param name="data">数据</param>
    /// <param name="cell">gameobject</param>

    private void SetWeaponData(IWeapon data, Transform cell)
    {
        TransformUtil.FindUIObject<Transform>(cell, "Content").gameObject.SetActive(data != null);
        TransformUtil.FindUIObject<Transform>(cell, "Empty").gameObject.SetActive(data == null);
        if (data != null)
        {
            //icon
            UIUtil.SetIconImage(TransformUtil.FindUIObject<Image>(cell, "Content/Image_Icon"), TableUtil.GetItemIconBundle(data.GetTID()), TableUtil.GetItemIconImage(data.GetTID()));
            //name
            TransformUtil.FindUIObject<TMP_Text>(cell, "Content/Label_Name").text = TableUtil.GetItemName((int)data.GetTID());
            //quality
            TransformUtil.FindUIObject<Image>(cell, "Content/Image_Quality").color = ColorUtil.GetColorByItemQuality(data.GetBaseConfig().Quality);
            //lv
            TransformUtil.FindUIObject<TMP_Text>(cell, "Content/Label_Lv").text = "Lv." + PadLeft(data.GetLv().ToString());
        }
    }

    #endregion

    #region SetReformerData
    /// <summary>
    /// 设置各种转化炉数据
    /// </summary>
    private void SetReformerData()
    {
        TransformUtil.FindUIObject<Transform>(m_ReformerCell, "Content").gameObject.SetActive(m_Reformer != null);
        TransformUtil.FindUIObject<Transform>(m_ReformerCell, "Empty").gameObject.SetActive(m_Reformer == null);
        if (m_Reformer != null)
        {
            //icon
            UIUtil.SetIconImage(TransformUtil.FindUIObject<Image>(m_ReformerCell, "Content/Image_Icon"), TableUtil.GetItemIconBundle(m_Reformer.GetTID()), TableUtil.GetItemIconImage(m_Reformer.GetTID()));
            //name
            TransformUtil.FindUIObject<TMP_Text>(m_ReformerCell, "Content/Label_Name").text = TableUtil.GetItemName((int)m_Reformer.GetTID());
            //quality
            TransformUtil.FindUIObject<Image>(m_ReformerCell, "Content/Image_Quality").color = ColorUtil.GetColorByItemQuality(m_Reformer.GetBaseConfig().Quality);
            //lv
            TransformUtil.FindUIObject<TMP_Text>(m_ReformerCell, "Content/Label_Lv").text = "Lv." + PadLeft(m_Reformer.GetLv().ToString());
        }
    }

    #endregion

    #region SetEquipData
    /// <summary>
    /// 设置各种装备数据
    /// </summary>
    private void SetEquipmentDataList()
    {
        for (int i = 0; i < m_Equipment.Length; i++)
        {
            SetEquipmentData(m_Equipment[i], m_EquipmentCellList[i]);
        }
    }

    /// <summary>
    /// 设置单个装备数据
    /// </summary>
    /// <param name="data"></param>
    /// <param name="cell"></param>
    private void SetEquipmentData(IEquipment data, Transform cell)
    {
        TransformUtil.FindUIObject<Transform>(cell, "Content").gameObject.SetActive(data != null);
        TransformUtil.FindUIObject<Transform>(cell, "Empty").gameObject.SetActive(data == null);
        if (data != null)
        {
            //icon
            UIUtil.SetIconImage(TransformUtil.FindUIObject<Image>(cell, "Content/Image_Icon"), TableUtil.GetItemIconBundle(data.GetTID()), TableUtil.GetItemIconImage(data.GetTID()));
            //name
            TransformUtil.FindUIObject<TMP_Text>(cell, "Content/Label_Name").text = TableUtil.GetItemName((int)data.GetTID());
            //quality
            TransformUtil.FindUIObject<Image>(cell, "Content/Image_Quality").color = ColorUtil.GetColorByItemQuality(data.GetBaseConfig().Quality);
            //lv
            TransformUtil.FindUIObject<TMP_Text>(cell, "Content/Label_Lv").text = "Lv." + PadLeft(data.GetLv().ToString());
        }
    }
    #endregion

    #region CellOnClickHandler

    /// <summary>
    /// 点击船入口
    /// </summary>
    /// <param name="sender">触发对象</param>
    /// <param name="args">船入口索引</param>
    private void OnShipClick(GameObject sender, params object[] args)
    {
        //sender.GetComponent<Animator>().SetBool("IsOn", true);
    }

    /// <summary>
    /// 某个武器入口被点击
    /// </summary>
    /// <param name="sender">触发对象</param>
    /// <param name="args">武器入口索引</param>
    private void OnWeaponClick(GameObject sender, params object[] args)
    {
        m_BeforeToggle = sender.GetComponent<Toggle>();
        //sender.GetComponent<Animator>().SetBool("IsOn", true);
        m_CanvasGroup.interactable = false;

        int id = (int)args[0];
        MsgWarshipPanelState msg = MessageSingleton.Get<MsgWarshipPanelState>();
        msg.State = WarshipPanelState.ListWeapon;
        msg.CurrentShip = m_CurrentShip;
        msg.CurrentReformerData = null;
        msg.CurrentEquipmentData = null;
        msg.CurrentModData = null;

        msg.CurrentWeaponData = new MsgWarshipPanelState.DataBase<IWeapon>(
            m_Weapons[id],
            m_CurrentShip.GetWeaponContainer().GetUID(),
            id);
     //   GameFacade.Instance.SendNotification(NotificationName.MSG_3DVIEWER_CHANGE);
        GameFacade.Instance.SendNotification(NotificationName.MSG_WARSHIP_PANEL_CHANGE, msg);

        UIManager.Instance.ClosePanel(OwnerView);
    }

    private void OnReformerClick(GameObject sender, params object[] args)
    {
        m_BeforeToggle = sender.GetComponent<Toggle>();
        //sender.GetComponent<Animator>().SetBool("IsOn", true);
        m_CanvasGroup.interactable = false;

        MsgWarshipPanelState msg = MessageSingleton.Get<MsgWarshipPanelState>();
        msg.State = WarshipPanelState.ListReformer;
        msg.CurrentShip = m_CurrentShip;

        msg.CurrentWeaponData = null;
        msg.CurrentEquipmentData = null;
        msg.CurrentModData = null;

        msg.CurrentReformerData = new MsgWarshipPanelState.DataBase<IReformer>(
            m_Reformer,
            m_CurrentShip.GetReformerContainer().GetUID(),
            0);
        //GameFacade.Instance.SendNotification(NotificationName.MSG_3DVIEWER_CHANGE);
        GameFacade.Instance.SendNotification(NotificationName.MSG_WARSHIP_PANEL_CHANGE, msg);

        UIManager.Instance.ClosePanel(OwnerView);
    }

    /// <summary>
    /// 某个装备入口被点击
    /// </summary>
    /// <param name="sender">触发对象</param>
    /// <param name="args">装备入口索引</param>
    private void OnEquipmentClick(GameObject sender, params object[] args)
    {
        m_BeforeToggle = sender.GetComponent<Toggle>();
        //sender.GetComponent<Animator>().SetBool("IsOn", true);
        m_CanvasGroup.interactable = false;

        MsgWarshipPanelState msg = MessageSingleton.Get<MsgWarshipPanelState>();
        msg.State = WarshipPanelState.ListEquip;
        msg.CurrentShip = m_CurrentShip;

        msg.CurrentWeaponData = null;
        msg.CurrentModData = null;
        msg.CurrentReformerData = null;

        int pos = (int)args[0];
        msg.CurrentEquipmentData = new MsgWarshipPanelState.DataBase<IEquipment>(
            m_Equipment[pos],
            m_CurrentShip.GetEquipmentContainer().GetUID(),
            pos);
       // GameFacade.Instance.SendNotification(NotificationName.MSG_3DVIEWER_CHANGE);
        GameFacade.Instance.SendNotification(NotificationName.MSG_WARSHIP_PANEL_CHANGE, msg);

        UIManager.Instance.ClosePanel(OwnerView);
    }
    #endregion

    #region hotkeyHandler
    /// <summary>
    /// esc
    /// </summary>
    /// <param name="callback"></param>
    private void OnEscClick(HotkeyCallback callback)
    {
        if (callback.performed)
        {
            GameFacade.Instance.SendNotification(NotificationName.MSG_CLOSE_MAIN_PANEL);
        }
    }

    /// <summary>
    /// 点击操作
    /// </summary>
    /// <param name="callback"></param>
    private void OnEnterClick(HotkeyCallback callback)
    {
        if (callback.performed)
        {
            //if (callback.isFromKeyboardMouse || callback.isFromUI)
            {
                if (m_BeforeToggle)
                {
                    m_BeforeToggle.GetComponent<UIEventListener.UIEventListener>().onClick(m_BeforeToggle.gameObject, m_ItemIdDic[m_BeforeToggle.name]);
                }
            }

        }
    }

    /// <summary>
    /// 任命出战
    /// </summary>
    private void OnAppointClick(HotkeyCallback callback)
    {
        if (callback.performed)
        {
            NetworkManager.Instance.GetPackageController().ReqestMove(m_CurrentShip.GetUID(), m_PackageProxy.GetHeroItem().UID, 1, 0);
        }
    }
    /// <summary>
    /// 进入舰船mod页面
    /// </summary>
    /// <param name="callback"></param>
    private void OnModClick(HotkeyCallback callback)
    {
        if (callback.performed)
        {
            m_CanvasGroup.interactable = false;

            MsgWarshipPanelState msg = MessageSingleton.Get<MsgWarshipPanelState>();
            msg.State = WarshipPanelState.ModMainShip;
            msg.CurrentShip = m_CurrentShip;
            msg.CurrentEquipmentData = null;
            msg.CurrentModData = null;
            msg.CurrentReformerData = null;
            msg.CurrentWeaponData = null;
           // GameFacade.Instance.SendNotification(NotificationName.MSG_3DVIEWER_CHANGE);
            GameFacade.Instance.SendNotification(NotificationName.MSG_WARSHIP_PANEL_CHANGE, msg);

            UIManager.Instance.ClosePanel(OwnerView);
        }
    }

    /// <summary>
    /// 打开船的面板
    /// </summary>
    /// <param name="callback"></param>
    private void OnOpenShipPanel(HotkeyCallback callback)
    {
        UIManager.Instance.ClosePanel(OwnerView);
        UIManager.Instance.OpenPanel(UIPanel.ShipHangarPanel, m_CurrentShip);
    }
    #endregion

    /// <summary>
	/// 字符串数字左侧补位
	/// </summary>
	/// <param name="str">字符串</param>
	/// <param name="totalWidth">总数</param>
	/// <param name="paddingChar">补0</param>
	/// <returns></returns>
	private string PadLeft(string str, int totalWidth = 3, char paddingChar = '0')
    {
        int index = 0;
        char[] charArray = str.PadLeft(3, '0').ToCharArray();
        for (int i = 0; i < charArray.Length; i++)
        {
            char cc = charArray[i];
            if (cc != '0')
            {
                index = i;
                break;
            }
        }
        string sstr = str.ToString().PadLeft(3, '0');
        if (index != 0)
        {
            sstr = sstr.Insert(index, "</color>");
            sstr = sstr.Insert(0, "<color=#808080>");
        }
        return sstr;
    }

    /// <summary>
    /// 设置当前角色模型
    /// </summary>
    /// <param name="tid">模型ID</param>
    private void ShowCharacter()
    {
        Model model = m_CfgEternityProxy.GetItemModelByKey((uint)m_ServerListProxy.GetCurrentCharacterVO().Tid);
        State.Set3DModelInfo(AssetAddressKey.PRELOADUI_UI3D_CHARACTERPANEL,
            new Effect3DViewer.ModelInfo[]
            { new Effect3DViewer.ModelInfo(){
                perfab = model.AssetName,
                position = new Vector3(model.UiPosition(0), model.UiPosition(1), model.UiPosition(2)),
                rotation = new Vector3(model.UiRotation(0), model.UiRotation(1), model.UiRotation(2)),
                scale = model.UiScale * Vector3.one
             }}, null);
    }

}
