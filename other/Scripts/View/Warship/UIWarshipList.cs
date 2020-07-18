using Eternity.FlatBuffer;
using Eternity.Runtime.Item;
using Leyoutech.Core.Loader.Config;
using PureMVC.Interfaces;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIWarshipList : UIListPart
{
	/// <summary>
	/// WarshipListPanel
	/// </summary>
	private WarshipListPanel Parent => OwnerView as WarshipListPanel;
	/// <summary>
	/// 背包Proxy
	/// </summary>
	private PackageProxy m_packageProxy;
	/// <summary>
	/// 配置
	/// </summary>
	private CfgEternityProxy m_cfgEternityProxy;
	/// <summary>
	/// 容器种类
	/// </summary>
	private Category m_Category;
	/// <summary>
	/// 当前的武器数据
	/// </summary>
	private IWeapon m_CurrentWeapon;
	/// <summary>
	/// 容器UID
	/// </summary>
	private ulong m_ContainerUID;
	/// <summary>
	/// 容器的位置
	/// </summary>
	private int m_ContainerPOS;
	/// <summary>
	/// 当前的武器容器UID
	/// </summary>
	private ulong m_CurrentWeaponContainerUID;
	/// <summary>
	/// 当前的武器容器位置
	/// </summary>
	private int m_CurrentWeaponContainerPOS;
	/// <summary>
	/// 当前装备中的道具数据
	/// </summary>
	private IShipItemBase m_CurrentItemData;
	/// <summary>
	/// 当前选中的数据
	/// </summary>
	private ItemBase m_CurrentSelectedItemData;
	/// <summary>
	/// 筛前道具数据
	/// </summary>
	private Dictionary<ulong, ItemBase> m_ItemsDic;
	/// <summary>
	/// 筛后道具数据
	/// </summary>
	private List<ItemBase> m_Items;
	/// <summary>
	/// 文字
	/// </summary>
	private StringBuilder sb = new StringBuilder();
    /// <summary>
    /// 标题图标
    /// </summary>
    private UIIconAndLabel m_IconAndLabel;
    /// <summary>
    /// UIScrollRect
    /// </summary>
    private UIScrollRect m_UIScrollRect;
    #region Proxy Getter
    private PackageProxy PackageProxy
	{
		get
		{
			if (m_packageProxy == null)
			{
				m_packageProxy = GameFacade.Instance.RetrieveProxy(ProxyName.PackageProxy) as PackageProxy;
			}
			return m_packageProxy;
		}
	}

	private CfgEternityProxy CfgEternityProxy
	{
		get
		{
			if (m_cfgEternityProxy == null)
			{
				m_cfgEternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
			}
			return m_cfgEternityProxy;
		}
	}
	#endregion

	#region Cell样式
	protected override string GetCellTemplate()
	{
		UIViewListLayout style = Parent.State.GetPageLayoutStyle(State.GetPageIndex());
		if (style == UIViewListLayout.Row)
		{
			return AssetAddressKey.PRELOADUIELEMENT_WARSHIPELEMENT_LIST;
		}
		else if (style == UIViewListLayout.Grid)
		{
			return AssetAddressKey.PRELOADUIELEMENT_WARSHIPELEMENT_GRID;
		}
		return null;
	}

	protected override string GetCellPlaceholderTemplate()
	{
		return AssetAddressKey.PRELOADUIELEMENT_PACKAGEELEMENT_EMPTY;
	}
	#endregion

	#region Cell渲染
	/// <summary>
	/// 普通单元格渲染时
	/// </summary>
	/// <param name="groupIndex"></param>
	/// <param name="cellIndex"></param>
	/// <param name="cellData"></param>
	/// <param name="cellView"></param>
	/// <param name="selected"></param>
	protected override void OnCellRenderer(int groupIndex, int cellIndex, object cellData, RectTransform cellView, bool selected)
	{
		Animator animator = cellView.GetComponent<Animator>();
		Image quality = cellView.Find("Content/Image_Quality").GetComponent<Image>();
		Image icon1 = cellView.Find("Content/Image_Icon").GetComponent<Image>();
		Image icon2 = cellView.Find("Content/Image_Icon2").GetComponent<Image>();
		TMP_Text nameLabel = cellView.Find("Content/Mask/Label_Name").GetComponent<TMP_Text>();
		TMP_Text typeLabel = cellView.Find("Content/Mask/Label_WeaponLabel").GetComponent<TMP_Text>();
		Image currentUsedIcon = cellView.Find("Content/Image_Icon_E").GetComponent<Image>();
		Image otherUsedIcon = cellView.Find("Content/Image_Icon_E1").GetComponent<Image>();
		TMP_Text lv = cellView.Find("Content/Level/Label_Lv2").GetComponent<TMP_Text>();

		ItemBase item = cellData as ItemBase;

		quality.color = ColorUtil.GetColorByItemQuality(item.ItemConfig.Quality);

		UIViewListLayout style = Parent.State.GetPageLayoutStyle(State.GetPageIndex());
		if (style == UIViewListLayout.Grid)
		{
			UIUtil.SetIconImageSquare(icon1, item.ItemConfig.Icon);
			UIUtil.SetIconImageSquare(icon2, item.ItemConfig.Icon);
		}
		else
		{
			UIUtil.SetIconImage(icon1, item.ItemConfig.Icon);
			UIUtil.SetIconImage(icon2, item.ItemConfig.Icon);
		}
		sb.Clear();
		nameLabel.text = TableUtil.GetItemName(item.TID);
		int zeroCount = 3 - item.Lv.ToString().Length;
		if (zeroCount > 0)
		{
			sb.Append("<color=#808080>");
			do
			{
				sb.Append(0);
			} while (--zeroCount > 0);
			sb.Append("</color>");
		}
		sb.Append(item.Lv);
		lv.text = sb.ToString();

		switch (item)
		{
			case ItemWeaponVO val:
				typeLabel.text = TableUtil.GetLanguageString(val.WeaponType2);
				break;
			case ItemReformerVO val:
				typeLabel.text = TableUtil.GetLanguageString(val.MainType);
				break;
			case ItemEquipmentVO val:
				typeLabel.text = TableUtil.GetLanguageString(val.EquipmentType);
				break;
			case ItemModVO val:
				typeLabel.text = TableUtil.GetLanguageString(val.ModType1) + " " + TableUtil.GetLanguageString(val.ModType2);
				break;
		}

		currentUsedIcon.gameObject.SetActive(m_CurrentItemData?.GetReference() == item.UID);
		otherUsedIcon.gameObject.SetActive(false);
		if (!currentUsedIcon.gameObject.activeSelf)
		{
			if (item.Replicas.Count > 0)
			{
				for (int i = 0; i < item.Replicas.Count; i++)
				{
					if (Parent.Data.CurrentShip.GetItem(item.Replicas[i]) != null)
					{
						otherUsedIcon.gameObject.SetActive(true);
						break;
					}
				}
			}
		}

		animator?.SetBool("IsOn", selected);
		if (selected)
		{
			OnItemSelected(item);
			m_CurrentSelectedItemData = item;
		}
	}

	/// <summary>
	/// 占位符单元格渲染时时
	/// </summary>
	/// <param name="groupIndex"></param>
	/// <param name="cellIndex"></param>
	/// <param name="cellView"></param>
	/// <param name="selected"></param>
	protected override void OnCellPlaceholderRenderer(int groupIndex, int cellIndex, RectTransform cellView, bool selected)
	{
		Animator animator = cellView.GetComponent<Animator>();
		animator?.SetBool("IsOn", selected);
		if (selected)
		{
			OnItemSelected(null);
			m_CurrentSelectedItemData = null;
		}
       // Parent.SendNotification(NotificationName.MSG_3DVIEWER_CHANGE);
    }
    #endregion

    #region 加载完毕 准备数据

    protected override void OnViewPartLoaded()
	{
		base.OnViewPartLoaded();
        m_UIScrollRect = FindComponent<UIScrollRect>("Content/Scroller");
        m_UIScrollRect.SetSelection(new Vector2Int(0, 0));
        ClearData();
		m_Items = new List<ItemBase>();
		switch (Parent.Data.State)
		{
			case WarshipPanelState.ListWeapon:
				m_Category = Category.Weapon;
				m_CurrentItemData = Parent.Data.CurrentWeaponData.Data;
				m_ContainerUID = Parent.Data.CurrentWeaponData.ContainerUID;
				m_ContainerPOS = Parent.Data.CurrentWeaponData.ContainerPOS;
				break;
			case WarshipPanelState.ListReformer:
				m_Category = Category.Reformer;
				m_CurrentItemData = Parent.Data.CurrentReformerData.Data;
				m_ContainerUID = Parent.Data.CurrentReformerData.ContainerUID;
				m_ContainerPOS = Parent.Data.CurrentReformerData.ContainerPOS;
				break;
			case WarshipPanelState.ListEquip:
				m_Category = Category.Equipment;
				m_CurrentItemData = Parent.Data.CurrentEquipmentData.Data;
				m_ContainerUID = Parent.Data.CurrentEquipmentData.ContainerUID;
				m_ContainerPOS = Parent.Data.CurrentEquipmentData.ContainerPOS;
				break;
			case WarshipPanelState.ListMod:
				m_Category = Category.EquipmentMod;
				m_CurrentWeapon = Parent.Data.CurrentWeaponData.Data;
				m_CurrentWeaponContainerUID = Parent.Data.CurrentWeaponData.ContainerUID;
				m_CurrentWeaponContainerPOS = Parent.Data.CurrentWeaponData.ContainerPOS;
				m_CurrentItemData = Parent.Data.CurrentModData.Data;
				m_ContainerUID = Parent.Data.CurrentModData.ContainerUID;
				m_ContainerPOS = Parent.Data.CurrentModData.ContainerPOS;
				if (Parent.Data.BeforeState == WarshipPanelState.ModMainShip)
				{
					SendViewerChange(Parent.Data.CurrentShip.GetTID(), true);
				}
				else
				{
					SendViewerChange(m_CurrentWeapon.GetTID(), false);
				}
				break;
		}

		m_ItemsDic = PackageProxy.GetPackage(m_Category).Items;
		if (m_ItemsDic != null && m_ItemsDic.Count > 0)
		{
			foreach (var item in m_ItemsDic)
			{
				switch (m_Category)
				{
					case Category.Weapon when m_ContainerPOS == 0 && Parent.Data.CurrentShip.GetWarShipType() == WarshipL1.FightWarship && (item.Value as ItemWeaponVO).WeaponType1 == WeaponL1.Fighting:
					case Category.Weapon when m_ContainerPOS == 0 && Parent.Data.CurrentShip.GetWarShipType() == WarshipL1.MiningShip && (item.Value as ItemWeaponVO).WeaponType1 == WeaponL1.Mining:
					case Category.Weapon when m_ContainerPOS == 0 && Parent.Data.CurrentShip.GetWarShipType() == WarshipL1.SurveillanceShip && (item.Value as ItemWeaponVO).WeaponType1 == WeaponL1.Treasure:
					case Category.Weapon when m_ContainerPOS != 0 && (item.Value as ItemWeaponVO).WeaponType1 == WeaponL1.Fighting:
					case Category.Reformer:
					case Category.Equipment when (item.Value as ItemEquipmentVO).EquipmentType == (EquipmentL1)(m_ContainerPOS + 1):
					case Category.EquipmentMod when (item.Value as ItemModVO).ModType1 == Parent.Data.CurrentModData.ModType1 && (item.Value as ItemModVO).ModType2 == Parent.Data.CurrentModData.ModType2:
						m_Items.Add(item.Value);
						break;
				}
			}
		}
		AddDatas(null, m_Items.ToArray());
        ShowCharacter();

    }
	#endregion

	#region 管理热键

	public override void OnShow(object msg)
	{
		base.OnShow(msg);
		Parent.OnEscClick = OnEscClick;
		Parent.State.GetAction(UIAction.Common_Compare_ExitCompare).Enabled = Parent.Data.State != WarshipPanelState.ListMod;
		Parent.State.GetAction(UIAction.Assemble_Install_Unload).Callback += OnItemUse;
		Parent.State.GetAction(UIAction.Assemble_SubChips).Callback += OnModClick;

		switch (Parent.Data.State)
		{
			case WarshipPanelState.ListReformer:
			case WarshipPanelState.ListEquip:
				Parent.State.GetAction(UIAction.Assemble_SubChips).Visible = false;
				Parent.State.GetAction(UIAction.Common_SwitchWeapon).Visible = false;
				break;
			case WarshipPanelState.ListMod:
				Parent.State.GetAction(UIAction.Assemble_SubChips).Visible = false;
				Parent.State.GetAction(UIAction.Common_Compare_ExitCompare).Visible = false;
				Parent.State.GetAction(UIAction.Common_SwitchWeapon).Visible = false;
				break;
			default:
				Parent.State.GetAction(UIAction.Assemble_SubChips).Visible = true;
				Parent.State.GetAction(UIAction.Common_Compare_ExitCompare).Visible = true;
				Parent.State.GetAction(UIAction.Common_SwitchWeapon).Visible = true;
				break;
		}
	}


	public override void OnHide()
	{
		Parent.OnEscClick = null;
		Parent.State.GetAction(UIAction.Assemble_Install_Unload).Callback -= OnItemUse;
		Parent.State.GetAction(UIAction.Assemble_SubChips).Callback -= OnModClick;
		base.OnHide();
	}

	private void OnItemSelected(ItemBase item)
	{
		if (item == null)
		{
			Parent.SendNotification(NotificationName.MSG_3DVIEWER_CHANGE);
			Parent.State.GetAction(UIAction.Assemble_Install_Unload).Enabled = false;
			Parent.State.GetAction(UIAction.Assemble_SubChips).Enabled = false;
			return;
		}
		SendViewerChange(item.TID);

		if (m_CurrentItemData?.GetReference() == item.UID)
		{
			Parent.State.GetAction(UIAction.Assemble_Install_Unload).Enabled = true;
			Parent.State.GetAction(UIAction.Assemble_Install_Unload).State = 1;
			Parent.State.GetAction(UIAction.Assemble_SubChips).Enabled = true;
		}
		else
		{
			Parent.State.GetAction(UIAction.Assemble_SubChips).Enabled = false;
			if (item.Replicas.Count > 0)
			{
				for (int i = 0; i < item.Replicas.Count; i++)
				{
					if (Parent.Data.CurrentShip.GetItem(item.Replicas[i]) != null)
					{
						Parent.State.GetAction(UIAction.Assemble_Install_Unload).Enabled = false;
						Parent.State.GetAction(UIAction.Assemble_Install_Unload).State = 0;
						return;
					}
				}
			}

			Parent.State.GetAction(UIAction.Assemble_Install_Unload).Enabled = true;
			Parent.State.GetAction(UIAction.Assemble_Install_Unload).State = 0;
		}
	}
	#endregion

	#region HotkeyCallbackHandler
	private void OnItemUse(HotkeyCallback callback)
	{
		if (callback.performed)
		{
			if (OwnerView.State.GetTipData() == null)
			{
				return;
			}
			if (m_CurrentSelectedItemData != null)
			{
				if (m_CurrentItemData?.GetReference() == m_CurrentSelectedItemData.UID)//拆
				{

					NetworkManager.Instance.GetPackageController().ReqestMove(
						m_CurrentItemData.GetUID(),
						PackageProxy.GetPackage(m_Category).UID,
						Parent.Data.CurrentShip.GetUID());

					PlayerSound(isAdd: false, m_Category == Category.Equipment ? (m_CurrentSelectedItemData as ItemEquipmentVO).EquipmentType : 0);
				}
				else//装
				{
					NetworkManager.Instance.GetPackageController().ReqestMove(
						m_CurrentSelectedItemData.UID,
						m_ContainerUID,
						m_ContainerPOS,
						Parent.Data.CurrentShip.GetUID());

					PlayerSound(isAdd: true, m_Category == Category.Equipment ? (m_CurrentSelectedItemData as ItemEquipmentVO).EquipmentType : 0);
				}
			}
		}
	}

	private void OnModClick(HotkeyCallback callback)
	{
		if (callback.performed)
		{
			UIManager.Instance.ClosePanel(Parent);
			MsgWarshipPanelState msg = MessageSingleton.Get<MsgWarshipPanelState>();
			msg.BeforeState = Parent.Data.State;
			msg.State = WarshipPanelState.ModMainWeapon;
			msg.CurrentShip = Parent.Data.CurrentShip;
			msg.CurrentModData = null;
			msg.CurrentWeaponData = new MsgWarshipPanelState.DataBase<IWeapon>(
				m_CurrentItemData as IWeapon,
				m_ContainerUID,
				m_ContainerPOS);
			Parent.SendNotification(NotificationName.MSG_WARSHIP_PANEL_CHANGE, msg);
            UIManager.Instance.OpenPanel(UIPanel.WarshipModPanel, msg);

        }
	}

	/// <summary>
	/// esc
	/// </summary>
	/// <param name="callback"></param>
	private void OnEscClick(HotkeyCallback callback)
	{
		if (callback.performed)
		{
			MsgWarshipPanelState data = MessageSingleton.Get<MsgWarshipPanelState>();

            data.CurrentShip = Parent.Data.CurrentShip;
			if (Parent.Data.State == WarshipPanelState.ListMod)
			{
                data.State = Parent.Data.BeforeState;
                data.CurrentWeaponData = new MsgWarshipPanelState.DataBase<IWeapon>(
					m_CurrentWeapon,
					m_CurrentWeaponContainerUID,
					m_CurrentWeaponContainerPOS);
                data.CurrentModData = new MsgWarshipPanelState.ModData(
					m_CurrentItemData as IMod,
					m_ContainerUID,
					m_ContainerPOS);
               

                switch (data.State)
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
                        switch ((EquipmentL1)data.CurrentEquipmentData.ContainerPOS + 1)
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
                Parent.SendNotification(NotificationName.MSG_WARSHIP_PANEL_CHANGE, data);

            }
            else
			{
                data.State = WarshipPanelState.Main;
                data.CurrentEquipmentData = null;
                data.CurrentModData = null;
                data.CurrentReformerData = null;
                data.CurrentWeaponData = null;
                data.CurrentShip = Parent.Data.CurrentShip;

                UIManager.Instance.OpenPanel(UIPanel.WarshipDialogPanel, Parent.Data.CurrentShip);
            }

            UIManager.Instance.ClosePanel(Parent);
        }
	}
	#endregion

	#region HandleNotification
	public override NotificationName[] ListNotificationInterests()
	{
		return new NotificationName[]
		{
			NotificationName.MSG_SHIP_DATA_CHANGED,
		};
	}

	public override void HandleNotification(INotification notification)
	{
		switch (notification.Name)
		{
			case NotificationName.MSG_SHIP_DATA_CHANGED:
				RefreshUsingItem(notification.Body as MsgShipDataChanged);
				break;
		}
	}

	/// <summary>
	/// 刷新item数据
	/// </summary>
	/// <param name="data">数据</param>
	private void RefreshUsingItem(MsgShipDataChanged data)
	{
		if (m_Category == data.ItemType && m_ContainerUID == data.ContainerUid)
		{
			if (data.ChangeType == MsgShipDataChanged.Type.Add)
			{
				m_CurrentItemData = Parent.Data.CurrentShip.GetItem(data.ItemUid);
			}
			else if (data.ChangeType == MsgShipDataChanged.Type.Remove)
			{
				m_CurrentItemData = null;
			}
			m_CurrentSelectedItemData = null;
			RefreshCurrentAllCells();
		}
	}
	#endregion

	#region 模型图切换
	/// <summary>
	/// 设置模型图
	/// </summary>
	/// <param name="itemTid">itemTid</param>
	private void SendViewerChange(uint itemTid)
	{
		if (m_Category == Category.Weapon)
		{
			SendViewerChange(itemTid, false);
		}
	}
	/// <summary>
	/// 设置模型图
	/// </summary>
	/// <param name="itemTid">itemTid</param>
	/// <param name="isShip">是否是船</param>
	private void SendViewerChange(uint itemTid, bool isShip)
	{
		if (itemTid != 0)
		{
			Msg3DViewerInfo viewerInfo = MessageSingleton.Get<Msg3DViewerInfo>();
			Model model = CfgEternityProxy.GetItemModelByKey(itemTid);
			viewerInfo.Model = model;
			viewerInfo.IsShip = isShip;
            if (isShip)
            {
                viewerInfo.position = new Vector3(-271.7f, -53.3f, 0);
                viewerInfo.size = new Vector2(1775, 1209);
            }
            else
            {
                viewerInfo.position = new Vector3(-332, -220, 0);
                viewerInfo.size = new Vector2(724, 693);
            }
           
            Parent.SendNotification(NotificationName.MSG_3DVIEWER_CHANGE, viewerInfo);
		}
		else
		{
			Parent.SendNotification(NotificationName.MSG_3DVIEWER_CHANGE);
		}
	}

	#endregion

	#region 重写排序逻辑
	private ItemBase left;
	private ItemBase right;
	protected override int Compare(UIViewSortKind kind, object a, object b)
	{
		left = a as ItemBase;
		right = b as ItemBase;
		if (m_CurrentItemData?.GetReference() == left.UID)
		{
			return -1;
		}
		else if (m_CurrentItemData?.GetReference() == right.UID)
		{
			return 1;
		}
		else
		{
			if (left.Replicas.Count > 0)
			{
				for (int i = 0; i < left.Replicas.Count; i++)
				{
					if (Parent.Data.CurrentShip.GetItem(left.Replicas[i]) != null)
						return -1;
				}
			}

			if (right.Replicas.Count > 0)
			{
				for (int i = 0; i < right.Replicas.Count; i++)
				{
					if (Parent.Data.CurrentShip.GetItem(right.Replicas[i]) != null)
						return 1;
				}
			}
		}

		return base.Compare(kind, a, b);
	}
	#endregion

	#region 音乐
	/// <summary>
	/// 播放音乐
	/// </summary>
	/// <param name="type">背包类型</param>
	/// <param name="subType">背包子类型</param>
	/// <param name="isAdd">是否添加</param>
	private void PlayerSound(bool isAdd, EquipmentL1 subType)
	{
		WwiseMusic WwiseMusic = 0;
		switch (m_Category)
		{
			case Category.Weapon:
				WwiseMusic = isAdd ? WwiseMusic.Music_WeaponParts_Setup : WwiseMusic.Music_WeaponParts_Disboard;
				break;
			case Category.Reformer:
				WwiseMusic = isAdd ? WwiseMusic.Music_reborner_Setup : WwiseMusic.Music_reborner_Disboard;
				break;
			case Category.Equipment:
				switch (subType)
				{
					case EquipmentL1.Processor:
						WwiseMusic = isAdd ? WwiseMusic.Music_processor_Setup : WwiseMusic.Music_processor_Disboard;
						break;
					case EquipmentL1.Armor:
						WwiseMusic = isAdd ? WwiseMusic.Music_ArmorCoating_Setup : WwiseMusic.Music_ArmorCoating_Disboard;
						break;
					case EquipmentL1.Reactor:
						WwiseMusic = isAdd ? WwiseMusic.Music_reactor_Setup : WwiseMusic.Music_reactor_Disboard;
						break;
					case EquipmentL1.AuxiliaryUnit:
						WwiseMusic = isAdd ? WwiseMusic.Music_auxiliary_Setup : WwiseMusic.Music_auxiliary_Disboard;
						break;
					case EquipmentL1.Nanobot:
						WwiseMusic = isAdd ? WwiseMusic.Music_robot_Setup : WwiseMusic.Music_robot_Disboard;
						break;
					case EquipmentL1.SignalGenerator:
						WwiseMusic = isAdd ? WwiseMusic.Music_amplifier_Setup : WwiseMusic.Music_amplifier_Disboard;
						break;
				}
				break;
			case Category.EquipmentMod:
				WwiseMusic = isAdd ? WwiseMusic.Music_chip_Setup : WwiseMusic.Music_chip_Disboard;
				break;
		}
		if (WwiseMusic != 0)
		{
			WwiseUtil.PlaySound((int)WwiseMusic, false, null);
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
        Model model = CfgEternityProxy.GetItemModelByKey((uint)m_ServerListProxy.GetCurrentCharacterVO().Tid);
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
