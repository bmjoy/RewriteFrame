using Eternity.FlatBuffer;
using Eternity.Runtime.Item;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Leyoutech.Core.Loader.Config;
using PureMVC.Interfaces;
using System;

public class UIWarshipModListPart : BaseViewPart
{
    /// <summary>
    /// list预设地址
    /// </summary>
    private const string ASSET_ADDRESS = AssetAddressKey.PRELOADUI_WARSHIPMODPANEL;

    /// <summary>
	/// 数据
	/// </summary>
	private CfgEternityProxy m_CfgEternityProxy;
    /// <summary>
    /// 战舰装备面板上一次类型
    /// </summary>
    private WarshipPanelState m_BeforeState;
    /// <summary>
    /// 战舰装备面板当前类型
    /// </summary>
    private WarshipPanelState m_CurrentState;
    /// <summary>
    /// modItem预设体
    /// </summary>
    private GameObject m_ModItemPrefab;
    /// <summary>
    /// 内容
    /// </summary>
    private Transform m_Container;
    /// <summary>
    /// mod Tips
    /// </summary>
    private GameObject m_ModTips;
    /// <summary>
    /// 当前船
    /// </summary>
    private IShip m_CurrentShip;
    /// <summary>
    /// 当前武器
    /// </summary>
    private IWeapon m_CurrentWeapon;
    /// <summary>
    /// 武器背包UID
    /// </summary>
    private ulong m_WeaponContainerUID;
    /// <summary>
    /// 武器背包位置标记
    /// </summary>
    private int m_WeaponContainerPOS;
    /// <summary>
    /// modList
    /// </summary>
    private List<WarshipModPanelElement> m_ModCellList;
    /// <summary>
    /// 当前选择的个体
    /// </summary>
    private WarshipModPanelElement m_CurrentSelectedCell;
    /// <summary>
    /// 消息
    /// </summary>
    private object m_Msg;
    public override void OnShow(object msg)
    {
        base.OnShow(msg);
        LoadViewPart(ASSET_ADDRESS, OwnerView.ListBox);
        m_Msg = msg;
        m_CfgEternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;

    }

    public override void OnHide()
    {
        m_CurrentSelectedCell = null;
        
        base.OnHide();
    }
    protected override void OnViewPartLoaded()
    {
        m_Container = FindComponent<Transform>("MODList");
        m_ModCellList = new List<WarshipModPanelElement>();

        UIManager.Instance.GetUIElement("Assets/Artwork/UI/Prefabs/Element/WarShipMessageElement.prefab",
            (cell) =>
            {
                m_ModTips = cell.Spawn(m_Container);
                m_ModTips.SetActive(false);
                m_ModTips.transform.localScale = Vector3.one;
            }
        );

        UIManager.Instance.GetUIElement("Assets/Artwork/UI/Prefabs/Element/WarShipModElement.prefab",
            (cell) =>
            {
                m_ModItemPrefab = cell;
            }
        );

        m_CurrentSelectedCell = null;
        OnRefresh(m_Msg);
        WarshipModPanel warshipModPanel = OwnerView as WarshipModPanel;
        warshipModPanel.OnEscClick = OnEscClick;
        State.GetAction(UIAction.Common_Select).Callback += OnModSelected;
        State.GetAction(UIAction.Common_Select).Enabled = false;
        ShowCharacter();
    }

    protected override void OnViewPartUnload()
    {
        WarshipModPanel warshipModPanel = OwnerView as WarshipModPanel;
        warshipModPanel.OnEscClick = null;
        State.GetAction(UIAction.Common_Select).Callback -= OnModSelected;
        for (int i = 0; i < m_ModCellList.Count; i++)
        {
            m_ModCellList[i].gameObject.Recycle();
        }
        m_ModTips.Recycle();
    }
    public void OnRefresh(object msg)
    {
        MsgWarshipPanelState data = msg as MsgWarshipPanelState;
        m_BeforeState = data.BeforeState;
        m_CurrentState = data.State;
        m_CurrentShip = data.CurrentShip;
        uint count = 0;
        uint modPosId = 0;
        switch (m_CurrentState)
        {
            case WarshipPanelState.ModMainShip:
                count = m_CurrentShip.GetGeneralModContainer().GetCurrentSizeMax()
                      + m_CurrentShip.GetExclusivelyModContainer().GetCurrentSizeMax();
                modPosId = (uint)m_CurrentShip.GetConfig().ModPosition;
                break;
            case WarshipPanelState.ModMainWeapon:
                m_CurrentWeapon = data.CurrentWeaponData.Data;
                m_WeaponContainerUID = data.CurrentWeaponData.ContainerUID;
                m_WeaponContainerPOS = data.CurrentWeaponData.ContainerPOS;
                count = data.CurrentWeaponData.Data.GetGeneralModContainer().GetCurrentSizeMax()
                      + data.CurrentWeaponData.Data.GetExclusivelyModContainer().GetCurrentSizeMax();
                modPosId = (uint)data.CurrentWeaponData.Data.GetConfig().ModPosition;
                break;
        }
        SendViewerChange();
     //   SetTips();
        CheckModBtnCount(count, modPosId);
        if (data.CurrentModData != null)
        {
            //SelectMod(data.CurrentModData.ContainerUID, data.CurrentModData.ContainerPOS);
        }
        else
        {
            //SelectMod(m_ModCellList[0]);
        }
        State.SetTipData(null);
        EventSystem.current.SetSelectedGameObject(null);
    }

    /// <summary>
    /// 设置模型图
    /// </summary>
    private void SendViewerChange()
    {
        Msg3DViewerInfo viewerInfo = MessageSingleton.Get<Msg3DViewerInfo>();
        Model model;
        if (m_CurrentState == WarshipPanelState.ModMainShip)
        {
            model = m_CfgEternityProxy.GetModel(m_CurrentShip.GetBaseConfig().Model);
            viewerInfo.IsShip = true;
            viewerInfo.position = new Vector3(-49.8f, -89.7f, 0);
            viewerInfo.size = new Vector2(1987, 1416);
        }
        else
        {
            //暂时没有武器
            model = m_CfgEternityProxy.GetModel(m_CurrentWeapon.GetBaseConfig().Model);
            viewerInfo.IsShip = false;
            viewerInfo.position = new Vector3(-49.8f, -89.7f, 0);
            viewerInfo.size = new Vector2(1987, 1416);
        }
        viewerInfo.Model = model;
       
        GameFacade.Instance.SendNotification(NotificationName.MSG_3DVIEWER_CHANGE, viewerInfo);
    }

    /// <summary>
    /// 设置tips
    /// </summary>
    private void SetTips()
    {
        MsgChildShowTipsInfo msg = MessageSingleton.Get<MsgChildShowTipsInfo>();
        if (m_CurrentState == WarshipPanelState.ModMainShip)
            msg.TipData = m_CurrentShip;
        else
            msg.TipData = m_CurrentWeapon;
        State.SetTipData(msg.TipData);
        GameFacade.Instance.SendNotification(NotificationName.MSG_CHILD_TIPS_CHANGE, msg);
    }

    /// <summary>
    /// 检查mod按钮数量
    /// </summary>
    /// <param name="count">数量</param>
    /// <param name="modPosId">mod位置标记</param>
    private void CheckModBtnCount(uint count, uint modPosId)
    {
        if (m_ModCellList.Count == count)
        {
            SetModBtnListData();
        }
        else if (m_ModCellList.Count < count)
        {
            while (m_ModCellList.Count < count)
            {
                m_ModCellList.Add(GetCell());
            }
        }
        else
        {
            while (m_ModCellList.Count > count)
            {
                WarshipModPanelElement item = m_ModCellList[0];
                m_ModCellList.RemoveAt(0);
                item.gameObject.Recycle();
            }
        }

        ModPosition modPosCfg = m_CfgEternityProxy.GetModPosition(modPosId);
        ModPositionAttr modPosAttr;
        for (int i = 0; i < m_ModCellList.Count; i++)
        {
            modPosAttr = modPosCfg.Positions(i).Value;
            m_ModCellList[i].transform.localPosition = new Vector3(modPosAttr.X, modPosAttr.Y, 0);
            m_ModCellList[i].GetComponent<Toggle>().isOn = false;

            m_ModCellList[i].name = i.ToString();
        }

        SetModBtnListData();
    }

    /// <summary>
    /// 获取单个物体
    /// </summary>
    /// <returns></returns>
    private WarshipModPanelElement GetCell()
    {
        WarshipModPanelElement cell = m_ModItemPrefab.Spawn(m_Container).GetOrAddComponent<WarshipModPanelElement>();
        cell.transform.localScale = Vector3.one;
        cell.OnSelected = ModCellOnSelected;
        cell.OnDeselected = ModCellOnDeselected;
        cell.OnDoubleClicked = ModCellOnDoubleClick;
        return cell;
    }

    /// <summary>
    /// 设置mod按钮list数据
    /// </summary>
    private void SetModBtnListData()
    {
        ModCellOnDeselected(null);
        switch (m_CurrentState)
        {
            case WarshipPanelState.ModMainShip:
                SetShipModOrShipEquipModData(EquipmentModL1.WarshipMod, m_CurrentShip.GetGeneralModContainer(), m_CurrentShip.GetExclusivelyModContainer());
                break;
            case WarshipPanelState.ModMainWeapon:
                SetShipModOrShipEquipModData(EquipmentModL1.WeaponMod, m_CurrentWeapon.GetGeneralModContainer(), m_CurrentWeapon.GetExclusivelyModContainer());
                break;
        }
    }
    /// <summary>
    /// 设置船mod 或是船装备mod数据
    /// </summary>
    /// <param name="equipmentModL1"></param>
    /// <param name="GeneralModContainer"></param>
    /// <param name="ExclusivelyModContainer"></param>
    private void SetShipModOrShipEquipModData(EquipmentModL1 equipmentModL1, IModContainer GeneralModContainer, IModContainer ExclusivelyModContainer)
    {
        for (int i = 0; i < m_ModCellList.Count; i++)
        {
            m_ModCellList[i].Cleanup();
            if (i < GeneralModContainer.GetCurrentSizeMax())
            {
                m_ModCellList[i].SetBaseData(equipmentModL1, EquipmentModL2.General, GeneralModContainer.GetUID(), i);
            }
            else
            {
                m_ModCellList[i].SetBaseData(equipmentModL1, EquipmentModL2.Exclusively, ExclusivelyModContainer.GetUID(), i - (int)GeneralModContainer.GetCurrentSizeMax());
            }
        }

        IMod[] mods = GeneralModContainer.GetMods();
        if (mods != null && mods.Length > 0)
        {
            for (int i = 0; i < mods.Length; i++)
            {
                m_ModCellList[mods[i].GetPos()].SetData(mods[i]);
            }
        }
        mods = ExclusivelyModContainer.GetMods();
        int GeneralModCount = (int)GeneralModContainer.GetCurrentSizeMax();
        if (mods != null && mods.Length > 0)
        {
            for (int i = 0; i < mods.Length; i++)
            {
                m_ModCellList[mods[i].GetPos() + GeneralModCount].SetData(mods[i]);
            }
        }
    }
    /// <summary>
    /// 选择Mod 重载方法
    /// </summary>
    /// <param name="containerUID">背包UID</param>
    /// <param name="containerPOS"背包位置标记></param>
    private void SelectMod(ulong containerUID, int containerPOS)
    {
        for (int i = 0; i < m_ModCellList.Count; i++)
        {
            if (m_ModCellList[i].GetContainerUID() == containerUID && m_ModCellList[i].GetContainerPOS() == containerPOS)
            {
                //SelectMod(m_ModCellList[i]);
                break;
            }
        }
    }

    /// <summary>
    /// 选择Mod 重载方法
    /// </summary>
    /// <param name="cell">内容Element</param>
    private void SelectMod(WarshipModPanelElement cell)
    {
        m_CurrentSelectedCell = null;
        cell.GetComponent<Toggle>().isOn = true;
        ModCellOnSelected(cell);
    }

    /// <summary>
    /// mod选中
    /// </summary>
    /// <param name="cell"></param>
    private void ModCellOnSelected(WarshipModPanelElement cell)
    {
        
        if (cell == m_CurrentSelectedCell)
        {
            return;
        }
        if (!EventSystem.current.alreadySelecting)
        {
            //FocusTo(cell.gameObject);
            EventSystem.current.SetSelectedGameObject(cell.gameObject);
        }
        m_CurrentSelectedCell = cell;
        State.GetAction(UIAction.Common_Select).Enabled = true;
        ShowModMessageTips(cell);
        State.SetTipData(m_CurrentSelectedCell.GetData());
    }

    /// <summary>
    /// mod取消选中
    /// </summary>
    /// <param name="cell"></param>
    private void ModCellOnDeselected(WarshipModPanelElement cell)
    {
        if (cell)
        {
            cell.GetComponent<Toggle>().isOn = false;
            if (!EventSystem.current.alreadySelecting)
            {
               // FocusTo((GameObject)null);
                EventSystem.current.SetSelectedGameObject(null);
            }
        }
        if (m_CurrentSelectedCell == cell)
        {
            m_CurrentSelectedCell = null;
            State.GetAction(UIAction.Common_Select).Enabled = false;
            m_ModTips?.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// mod被双击
    /// </summary>
    /// <param name="cell"></param>
    private void ModCellOnDoubleClick(WarshipModPanelElement cell)
    {
        m_CurrentSelectedCell = cell;
        OpenModList();
    }

    #region mod标签提示
    private void ShowModMessageTips(WarshipModPanelElement cell)
    {
        m_ModTips.transform.localPosition = cell.transform.localPosition;
        m_ModTips.gameObject.SetActive(true);

        TransformUtil.FindUIObject<Transform>(m_ModTips.transform, "Content").gameObject.SetActive(cell.HasData());
        TransformUtil.FindUIObject<Transform>(m_ModTips.transform, "Empty").gameObject.SetActive(!cell.HasData());

        if (cell.HasData())
        {
            UIUtil.SetIconImage(
                TransformUtil.FindUIObject<Image>(m_ModTips.transform, "Content/Image_Icon"),
                TableUtil.GetItemIconBundle(cell.GetModTid()),
                TableUtil.GetItemIconImage(cell.GetModTid()),
                autoSetNativeSize: true);

            TransformUtil.FindUIObject<Image>(m_ModTips.transform, "Content/Image_Quality").color = ColorUtil.GetColorByItemQuality(m_CfgEternityProxy.GetItemByKey(cell.GetModTid()).Quality);

            //数字不足3位，补0 ，并将补充的颜色置灰
            int lv = cell.GetLv();
            int index = 0;
            char[] charArray = lv.ToString().PadLeft(3, '0').ToCharArray();
            for (int i = 0; i < charArray.Length; i++)
            {
                char cc = charArray[i];
                if (cc != '0')
                {
                    index = i;
                    break;
                }
            }
            string sstr = lv.ToString().PadLeft(3, '0');
            if (index != 0)
            {
                sstr = sstr.Insert(index, "</color>");
                sstr = sstr.Insert(0, "<color=#808080>");
            }
            TransformUtil.FindUIObject<TMP_Text>(m_ModTips.transform, "Content/Label_Lv").text = sstr;

            TransformUtil.FindUIObject<TMP_Text>(m_ModTips.transform, "Content/Label_Name").text = TableUtil.GetItemName(cell.GetModTid());
        }
    }
    #endregion

    #region OpenModList
    /// <summary>
    /// 打开mod List
    /// </summary>
    private void OpenModList()
    {
        if (m_CurrentSelectedCell == null)
        {
            return;
        }

        MsgWarshipPanelState msg = MessageSingleton.Get<MsgWarshipPanelState>();
        msg.BeforeState = m_CurrentState;
        msg.State = WarshipPanelState.ListMod;

        msg.CurrentWeaponData = new MsgWarshipPanelState.DataBase<IWeapon>(
            m_CurrentWeapon,
            m_WeaponContainerUID,
            m_WeaponContainerPOS);

        msg.CurrentModData = new MsgWarshipPanelState.ModData(
            m_CurrentSelectedCell.GetData(),
            m_CurrentSelectedCell.GetContainerUID(),
            m_CurrentSelectedCell.GetContainerPOS());
        msg.CurrentModData.ModType1 = m_CurrentSelectedCell.GetEquipmentModL1();
        msg.CurrentModData.ModType2 = m_CurrentSelectedCell.GetEquipmentModL2();

        UIManager.Instance.ClosePanel(OwnerView);
       // GameFacade.Instance.SendNotification(NotificationName.MSG_3DVIEWER_CHANGE);
        GameFacade.Instance.SendNotification(NotificationName.MSG_WARSHIP_PANEL_CHANGE, msg);
        UIManager.Instance.OpenPanel(UIPanel.WarshipChipPanel, msg);
    }
    #endregion

    #region hotkeyHandler
    /// <summary>
    /// 选择mod 操作
    /// </summary>
    /// <param name="callback"></param>
    private void OnModSelected(HotkeyCallback callback)
    {
        if (callback.performed)
        {
            OpenModList();
        }
    }

    /// <summary>
    /// esc操作
    /// </summary>
    /// <param name="callback"></param>
    private void OnEscClick(HotkeyCallback callback)
    {
        if (callback.performed)
        {
            GameFacade.Instance.SendNotification(NotificationName.MSG_3DVIEWER_CHANGE);
            UIManager.Instance.ClosePanel(OwnerView);
            MsgWarshipPanelState msg = MessageSingleton.Get<MsgWarshipPanelState>();
            msg.CurrentWeaponData = null;
            if (m_CurrentState == WarshipPanelState.ModMainShip)
            {
                msg.State = WarshipPanelState.Main;
            }
            else
            {
                msg.State = WarshipPanelState.ListWeapon;
                msg.CurrentWeaponData = new MsgWarshipPanelState.DataBase<IWeapon>(
                    m_CurrentWeapon,
                    m_WeaponContainerUID,
                    m_WeaponContainerPOS);
            }
            UIManager.Instance.OpenPanel(UIPanel.WarshipDialogPanel,m_CurrentShip);
            GameFacade.Instance.SendNotification(NotificationName.MSG_WARSHIP_PANEL_CHANGE, msg);
        }
    }
    #endregion

    /// <summary>
    /// 设置当前角色模型
    /// </summary>
    /// <param name="tid">模型ID</param>
    private void ShowCharacter()
    {
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
}
