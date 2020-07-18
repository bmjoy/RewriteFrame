using System.Collections;
using System.Collections.Generic;
using Eternity.FlatBuffer;
using Eternity.Runtime.Item;
using Leyoutech.Core.Loader.Config;
using PureMVC.Interfaces;
using UnityEngine;

public class UIShipHangarList : UIListPart
{
    /// <summary>
    /// 船Proxy
    /// </summary>
    private ShipProxy m_ShipProxy;
    /// <summary>
    /// 当前选中船
    /// </summary>
    private IShip m_Ship;
    /// <summary>
    /// 装配界面选择的船
    /// </summary>
    private IShip m_LastPanelShip;
    /// <summary>
    /// 背包Proxy
    /// </summary>
    private PackageProxy m_PackageProxy;
    /// <summary>
    /// ShipHangarPanel
    /// </summary>
    private ShipHangarPanel m_Parent => OwnerView as ShipHangarPanel;
    public override void OnShow(object msg)
    {
        base.OnShow(msg);
        if (m_ShipProxy == null)
        {
            m_ShipProxy = GameFacade.Instance.RetrieveProxy(ProxyName.ShipProxy) as ShipProxy;
        }
        if (m_PackageProxy == null)
        {
            m_PackageProxy = GameFacade.Instance.RetrieveProxy(ProxyName.PackageProxy) as PackageProxy;
        }
        if (msg != null)
        {
            m_LastPanelShip = (IShip)msg;
        }        
        m_Parent.OnEscClick = OnEscClick;
        
        State.GetAction(UIAction.Hangar_Assemble).Callback -= OnAssembleShip;
        State.GetAction(UIAction.Hangar_Assemble).Callback += OnAssembleShip;
        State.GetAction(UIAction.Hangar_Appoint).Callback -= OnAppointShip;
        State.GetAction(UIAction.Hangar_Appoint).Callback += OnAppointShip;
       
    }
    protected override void OnViewPartLoaded()
    {
        base.OnViewPartLoaded();
        SetSelectPage();
        ShowCharacter();
    }
    protected override string GetCellTemplate()
    {
        UIViewListLayout style = State.GetPageLayoutStyle(State.GetPageIndex());

        if (style == UIViewListLayout.Row)
        {
            return AssetAddressKey.PRELOADUIELEMENT_SHIPHANGARELEMENT_LIST;
        }
        else if (style == UIViewListLayout.Grid)
        {
            return AssetAddressKey.PRELOADUIELEMENT_SHIPHANGARELEMENT_GRID;
        }
        return null;
    }
    /// <summary>
    /// 船坞item选中
    /// </summary>
    /// <param name="groupIndex"></param>
    /// <param name="cellIndex"></param>
    /// <param name="cellData"></param>
    /// <param name="cellView"></param>
    /// <param name="selected"></param>
    protected override void OnCellRenderer(int groupIndex, int cellIndex, object cellData, RectTransform cellView, bool selected)
    {
        IShip ship = (IShip)cellData;
        Animator m_Animator = cellView.GetComponent<Animator>();
        if (m_Animator)
        {
            m_Animator.SetBool("IsOn", selected);
        }
        ShipHangarItem m_ShipHangarItem = cellView.GetOrAddComponent<ShipHangarItem>();
        UIViewListLayout style = State.GetPageLayoutStyle(State.GetPageIndex());

        if (style == UIViewListLayout.Row)
        {
            m_ShipHangarItem.SetData(ship, true);
        }
        else
        {
            m_ShipHangarItem.SetData(ship, false);
        }
        if (selected)
        {
            m_Ship = ship;
            State.SetTipData(null);
            State.SetTipData(cellData);
            State.GetAction(UIAction.Hangar_Assemble).Enabled = true;
            State.GetAction(UIAction.Hangar_Appoint).Enabled = ship.GetUID() != m_ShipProxy.GetAppointWarShip().GetUID();
            m_ShipHangarItem.RecordItemNew();
        }
    }
    /// <summary>
    /// 空状态选中
    /// </summary>
    /// <param name="groupIndex"></param>
    /// <param name="cellIndex"></param>
    /// <param name="cellView"></param>
    /// <param name="selected"></param>
    protected override void OnCellPlaceholderRenderer(int groupIndex, int cellIndex, RectTransform cellView, bool selected)
    {
        base.OnCellPlaceholderRenderer(groupIndex, cellIndex, cellView, selected);
        if (selected)
        {
            State.GetAction(UIAction.Hangar_Assemble).Enabled = false;
            State.GetAction(UIAction.Hangar_Appoint).Enabled = false;
        }
    }
    /// <summary>
    /// 设置当前角色模型
    /// </summary>
    /// <param name="tid">模型ID</param>
    private void ShowCharacter()
    {
        CfgEternityProxy m_CfgEternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
        ServerListProxy m_ServerListProxy = GameFacade.Instance.RetrieveProxy(ProxyName.ServerListProxy) as ServerListProxy;
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
    /// <summary>
    /// 切换页签
    /// </summary>
    /// <param name="oldIndex"></param>
    /// <param name="newIndex"></param>
    protected override void OnPageIndexChanged(int oldIndex, int newIndex)
    {
        if (GetTransform() == null)
        {
            return;
        }
        UpdateShipData();
    }
    /// <summary>
    /// 船坞排序
    /// </summary>
    private IShip left;
    private IShip right;
    protected override int Compare(UIViewSortKind kind, object a, object b)
    {
        int result = 0;
        left = a as IShip;
        right = b as IShip;
        switch (kind)
        {
            case UIViewSortKind.GetTime:
                result = (int)(right.GetCreatTime() - left.GetCreatTime());
                break;
            default:
                return base.Compare(kind, left.GetBaseConfig(), right.GetBaseConfig());
        }
        if (result == 0)
            result = right.GetBaseConfig().Order - left.GetBaseConfig().Order;
        if (result == 0)
            result = (int)(right.GetBaseConfig().Id - left.GetBaseConfig().Id);
        return result;
    }
    public override NotificationName[] ListNotificationInterests()
    {
        return new NotificationName[]
        {
            NotificationName.MSG_PACKAGE_ITEM_MOVE,
            NotificationName.MSG_PACKAGE_ITEM_ADD,
            NotificationName.MSG_PACKAGE_ITEM_DESTORY
        };
    }
    public override void HandleNotification(INotification notification)
    {
        switch (notification.Name)
        {
            case NotificationName.MSG_PACKAGE_ITEM_MOVE:
            case NotificationName.MSG_PACKAGE_ITEM_ADD:
            case NotificationName.MSG_PACKAGE_ITEM_DESTORY:
                UpdateShipData();
                break;
        }
    }
    /// <summary>
    /// 更新战舰数据
    /// </summary>
    private void UpdateShipData()
    {
        ClearData();
        int pageIndex = State.GetPageIndex();
        if (pageIndex == 0)
        {
            AddDatas("", m_ShipProxy.GetShipByType(WarshipL1.MiningShip));
        }
        else if (pageIndex == 1)
        {
            AddDatas("", m_ShipProxy.GetShipByType(WarshipL1.FightWarship));
        }
        else
        {
            AddDatas("", m_ShipProxy.GetShipByType(WarshipL1.SurveillanceShip));
        }
        UpdatePackSize();
    }
    /// <summary>
    /// 更新船坞容量大小
    /// </summary>
    private void UpdatePackSize()
    {
        int m_Count = 0;
        ItemContainer m_Container = null;
        WarshipL1 warshipL1 = 0;
        switch (State.GetPageIndex())
        {
            case 0:
                warshipL1 = WarshipL1.MiningShip;               
                break;
            case 1:
                warshipL1 = WarshipL1.FightWarship;               
                break;
            case 2:
                warshipL1 = WarshipL1.SurveillanceShip;               
                break;
        }
        m_Container = m_ShipProxy.GetShipContainer(warshipL1);
        m_Count = m_ShipProxy.GetShipByType(warshipL1).Length;
        int m_MaxCount = m_Container != null ? (int)m_Container.CurrentSizeMax : 0;
        State.SetPageLabel(State.GetPageIndex(), string.Format(GetLocalization("shiphangar_text_1009"), m_Count, m_MaxCount));
    }
    /// <summary>
    /// 装配船
    /// </summary>
    /// <param name="callback"></param>
    private void OnAssembleShip(HotkeyCallback callback)
    {
        if (OwnerView.State.GetTipData() == null)
            return;

        if (callback.performed)
        {
            UIManager.Instance.ClosePanel(UIPanel.ShipHangarPanel);
            UIManager.Instance.OpenPanel(UIPanel.WarshipDialogPanel, m_Ship);
        }
    }
    /// <summary>
    /// 任命船
    /// </summary>
    /// <param name="callback"></param>
    private void OnAppointShip(HotkeyCallback callback)
    {
        if (callback.performed)
        {
            NetworkManager.Instance.GetPackageController().ReqestMove(
               m_Ship.GetUID(),
               m_PackageProxy.GetHeroItem().UID, 1, 0);
        }
    }
    /// <summary>
    /// esc关闭界面
    /// </summary>
    /// <param name="callback"></param>
    private void OnEscClick(HotkeyCallback callback)
    {
        if (callback.performed)
        {
            UIManager.Instance.ClosePanel(UIPanel.ShipHangarPanel);
            UIManager.Instance.OpenPanel(UIPanel.WarshipDialogPanel);
        }
    }
    /// <summary>
    /// 设置装配界面船坞选中项
    /// </summary>
    private void SetSelectPage()
    {
        if (m_LastPanelShip == null)
        {
            return;
        }
        if (m_LastPanelShip.GetWarShipType() == WarshipL1.MiningShip)
        {
            SetPageAndSelection(0, m_LastPanelShip);
        }
        else if (m_LastPanelShip.GetWarShipType() == WarshipL1.FightWarship)
        {
            SetPageAndSelection(1, m_LastPanelShip);
        }
        else
        {
            SetPageAndSelection(2, m_LastPanelShip);
        }
    }
    
    public override void OnHide()
    {
        base.OnHide();
        State.GetAction(UIAction.Hangar_Assemble).Callback -= OnAssembleShip;
        State.GetAction(UIAction.Hangar_Appoint).Callback -= OnAppointShip;
    }
}
