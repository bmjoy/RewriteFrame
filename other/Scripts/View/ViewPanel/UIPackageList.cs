using Eternity.Runtime.Item;
using Leyoutech.Core.Loader.Config;
using PureMVC.Interfaces;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIPackageList : UIListPart
{
    /// <summary>
    /// 要出售的道具ID
    /// </summary>
    private ulong m_SellItemId;

    /// <summary>
    /// 最后一次选中的道具ID
    /// </summary>
    private ulong m_LastSelectUid;

    /// <summary>
    /// 道具缓存列表
    /// </summary>
    private List<ItemBase> m_ItemBaseCache = new List<ItemBase>();

    /// <summary>
    /// 打开时
    /// </summary>
    /// <param name="msg">消息</param>
    public override void OnShow(object msg)
    {
        base.OnShow(msg);

        State.OnSelectionChanged -= OnSelectionDataChanged;
        State.OnSelectionChanged += OnSelectionDataChanged;

        State.GetAction(UIAction.Common_Deconstruct).Callback -= OnDeleteCallback;
        State.GetAction(UIAction.Common_Deconstruct).Callback += OnDeleteCallback;
    }

    /// <summary>
    /// 关闭时
    /// </summary>
    public override void OnHide()
    {
        State.OnSelectionChanged -= OnSelectionDataChanged;

        State.GetAction(UIAction.Common_Deconstruct).Callback -= OnDeleteCallback;

        base.OnHide();
    }

    /// <summary>
    /// 注册消息
    /// </summary>
    /// <returns>消息列表</returns>
    public override NotificationName[] ListNotificationInterests()
    {
        return new NotificationName[]{NotificationName.MSG_PACKAGE_ITEM_CHANGE};
    }

    /// <summary>
    /// 处理消息
    /// </summary>
    /// <param name="notification">消息内容</param>
    public override void HandleNotification(INotification notification)
    {
        switch (notification.Name)
        {
            case NotificationName.MSG_PACKAGE_ITEM_CHANGE:
                UpdateData();
                break;
        }
    }

    /// <summary>
    /// 页面切换时
    /// </summary>
    /// <param name="oldIndex">旧索引</param>
    /// <param name="newIndex">新索引</param>
    protected override void OnPageIndexChanged(int oldIndex, int newIndex)
    {
        UpdateData();
    }

    /// <summary>
    /// 过滤方式切换时
    /// </summary>
    /// <param name="oldIndex">旧索引</param>
    /// <param name="newIndex">新索引</param>
    protected override void OnFilterIndexChanged(int oldIndex, int newIndex)
    {
        UpdateData();
    }

    /// <summary>
    /// 当前选中数据改变时
    /// </summary>
    /// <param name="obj">数据</param>
    private void OnSelectionDataChanged(object obj)
    {
        if (m_SellItemId != 0)
            State.GetAction(UIAction.Common_Deconstruct).Enabled = false;

        if(obj is ItemBase)
        {
            ItemBase item = obj as ItemBase;

            if (item.Replicas != null && item.Replicas.Count > 0)
                State.GetAction(UIAction.Common_Deconstruct).Enabled = false;
            else if(item.ItemConfig.DeconstructPackage == 0)
                State.GetAction(UIAction.Common_Deconstruct).Enabled = false;                    
            else
                State.GetAction(UIAction.Common_Deconstruct).Enabled = true;

            m_LastSelectUid = item.UID;
        }
        else
        {
            m_LastSelectUid = 0;
            State.GetAction(UIAction.Common_Deconstruct).Enabled = false;
        }
    }

    /// <summary>
    /// 删除回调
    /// </summary>
    /// <param name="callback">热键数据</param>
    private void OnDeleteCallback(HotkeyCallback callback)
    {
        if (callback.started)
        {
            if (m_SellItemId == 0)
            {
                m_SellItemId = m_LastSelectUid;
            }
        }
        if (callback.performed)
        {
            if (m_LastSelectUid == m_SellItemId)
            {
                NetworkManager.Instance.GetPackageController().RequestSell((ulong)m_LastSelectUid, 1);
                WwiseUtil.PlaySound((int)WwiseMusic.Music_Resolve_Over, false, null);
            }
            m_SellItemId = 0;
        }
        if (callback.cancelled)
        {
            m_SellItemId = 0;
        }
    }

    /// <summary>
    /// 确定单元格的模板
    /// </summary>
    /// <returns></returns>
    protected override string GetCellTemplate()
    {
        UIViewListLayout style = State.GetPageLayoutStyle(State.GetPageIndex());

        if (style == UIViewListLayout.Row)
            return AssetAddressKey.PRELOADUIELEMENT_PACKAGEELEMENT_LIST;
        else if (style == UIViewListLayout.Grid)
            return AssetAddressKey.PRELOADUIELEMENT_PACKAGEELEMENT_GRID;

        return null;
    }

    /// <summary>
    /// 渲染单元格
    /// </summary>
    /// <param name="groupIndex">组索引</param>
    /// <param name="cellIndex">列表索引</param>
    /// <param name="cellData">数据</param>
    /// <param name="cellView">视图</param>
    /// <param name="selected">是否选中</param>
    protected override void OnCellRenderer(int groupIndex, int cellIndex, object cellData, RectTransform cellView, bool selected)
    {
        ItemBase itemVO = cellData as ItemBase;

        Animator animator = cellView.GetComponent<Animator>();
        Image icon1 = FindComponent<Image>(cellView, "Content/Image_Icon");
        Image icon2 = FindComponent<Image>(cellView, "Content/Image_Icon2");
        TMP_Text nameField = FindComponent<TMP_Text>(cellView, "Content/Mask/Label_Name");
        TMP_Text countField = FindComponent<TMP_Text>(cellView, "Content/Label_Num");
        Image qualityImage = FindComponent<Image>(cellView, "Content/Image_Quality");
        TMP_Text typeField = FindComponent<TMP_Text>(cellView, "Content/Mask/Label_WeaponLabel");
        Transform modContainer = FindComponent<Transform>(cellView, "Content/MOD");
        Transform useFlag = FindComponent<Transform>(cellView, "Content/Image_Used");
        TMP_Text levelLabel = FindComponent<TMP_Text>(cellView, "Content/Label_Lv");
        TMP_Text levelField = FindComponent<TMP_Text>(cellView, "Content/Label_Lv2");

        //动画
        animator.SetBool("IsOn", selected);
        //名称
        nameField.text = TableUtil.GetItemName((int)itemVO.TID);        
        //品质
        qualityImage.color = ColorUtil.GetColorByItemQuality(itemVO.ItemConfig.Quality);
        //数量
        countField.text = itemVO.Count.ToString();
        //图标
        if (State.GetPageLayoutStyle(State.GetPageIndex()) == UIViewListLayout.Row)
        {
            UIUtil.SetIconImage(icon1, itemVO.ItemConfig.Icon);
            UIUtil.SetIconImage(icon2, itemVO.ItemConfig.Icon);
        }
        else
        {
            UIUtil.SetIconImageSquare(icon1, itemVO.ItemConfig.Icon);
            UIUtil.SetIconImageSquare(icon2, itemVO.ItemConfig.Icon);
        }
        //使用标记
        useFlag.gameObject.SetActive(itemVO.Replicas != null && itemVO.Replicas.Count > 0);
        //等级
        levelLabel.gameObject.SetActive(itemVO.MainType == Category.Weapon || itemVO.MainType == Category.Reformer || itemVO.MainType == Category.Equipment || itemVO.MainType == Category.EquipmentMod);
        levelField.text = levelLabel.gameObject.activeSelf ? FormatLevel(itemVO.Lv) : string.Empty;
        //类型
        switch(itemVO.MainType)
        {
            case Category.Weapon:
                typeField.text = TableUtil.GetLanguageString((cellData as ItemWeaponVO).WeaponType2);
                break;
            case Category.Reformer:
                typeField.text = TableUtil.GetLanguageString(itemVO.MainType);
                break;
            case Category.Equipment:
                typeField.text = TableUtil.GetLanguageString((cellData as ItemEquipmentVO).EquipmentType);
                break;
            case Category.EquipmentMod:
                typeField.text = TableUtil.GetLanguageString((cellData as ItemModVO).ModType1);
                break;
            default:
                typeField.text = TableUtil.GetLanguageString(itemVO.MainType);
                break;
        }
        //模组数量
        UpdateModList(modContainer, itemVO.MainType == Category.Weapon ? GetWeaponModCount(cellData as ItemWeaponVO) : 0);
    }
    /// <summary>
    /// 渲染空白单元格
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
            State.GetAction(UIAction.Common_Deconstruct).Enabled = false;
        }
    }
    /// <summary>
    /// 格式化等级
    /// </summary>
    /// <param name="level">等级</param>
    /// <returns>文本</returns>
    private string FormatLevel(int level)
    {
        string lvString = level.ToString();

        if (lvString.Length == 1)
            return "<color=#808080>00</color>" + lvString;
        else if (lvString.Length == 2)
            return "<color=#808080>0</color>" + lvString;
        else
            return lvString;
    }

    /// <summary>
    /// 更新模组列表
    /// </summary>
    /// <param name="modBox">mod容器</param>
    /// <param name="count">mod数量</param>
    public void UpdateModList(Transform modBox, uint count)
    {
        int index = 0;

        for(;index<count;index++)
        {
            Transform icon = index < modBox.childCount ? modBox.GetChild(index) : Object.Instantiate(modBox.GetChild(0), modBox);
            icon.gameObject.SetActive(true);
        }

        for(;index<modBox.childCount;index++)
        {
            modBox.GetChild(index).gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 获取武器的模组数量
    /// </summary>
    /// <param name="itemContainer"></param>
    /// <returns>模组数量</returns>
    private uint GetWeaponModCount(ItemContainer itemContainer)
    {
        uint containerSizeA = 0;
        uint containerSizeB = 0;

        if (itemContainer != null && itemContainer.Items != null)
        {
            string containerKeyA = Category.EquipmentMod.ToString() + EquipmentModL1.WeaponMod.ToString() + EquipmentModL2.General.ToString();
            string containerKeyB = Category.EquipmentMod.ToString() + EquipmentModL1.WeaponMod.ToString() + EquipmentModL2.Exclusively.ToString();

            foreach (ItemBase item in itemContainer.Items.Values)
            {
                if (item is ItemContainer)
                {
                    ItemContainer container = item as ItemContainer;
                    if (container.ContainerType.Equals(containerKeyA))
                        containerSizeA = container.CurrentSizeMax;
                    else if (container.ContainerType.Equals(containerKeyB))
                        containerSizeB = container.CurrentSizeMax;
                }
            }
        }

        return containerSizeA + containerSizeB;
    }



    /// <summary>
    /// 更新数据
    /// </summary>
    private void UpdateData()
    {
        PackageProxy packageProxy = GameFacade.Instance.RetrieveProxy(ProxyName.PackageProxy) as PackageProxy;

        ClearData();

        UIViewCategory category = State.GetPageCategoryData();
        if (category.ItemType != null)
        {
            if(category.IsAll)
            {
                UIViewPage page = State.GetPage();

                for(int i=0;i<page.Categorys.Length;i++)
                {
                    if (page.Categorys[i].IsAll)
                        continue;

                    category = page.Categorys[i];

                    m_ItemBaseCache.Clear();
                    for (int j = 0; j < category.ItemType.Length; j++)
                    {
                        ItemType itemType = category.ItemType[j];
                        if (itemType == null)
                            continue;

                        m_ItemBaseCache.AddRange(packageProxy.FindItemArrayByItemType(itemType, true));
                    }

                    AddDatas(category.Label, m_ItemBaseCache.ToArray());

                    m_ItemBaseCache.Clear();
                }
            }
            else
            {
                m_ItemBaseCache.Clear();

                for (int i = 0; i < category.ItemType.Length; i++)
                {
                    ItemType itemType = category.ItemType[i];
                    if (itemType == null)
                        continue;

                    m_ItemBaseCache.AddRange(packageProxy.FindItemArrayByItemType(itemType, true));
                }

                AddDatas(category.Label, m_ItemBaseCache.ToArray());

                m_ItemBaseCache.Clear();
            }
        }

        UpdatePackSize();
    }

    /// <summary>
    /// 更新背包大小
    /// </summary>
    private void UpdatePackSize()
    {
        int pageIndex = State.GetPageIndex();

        PackageProxy pack = GameFacade.Instance.RetrieveProxy(ProxyName.PackageProxy) as PackageProxy;

        ItemContainer m_Container = null;

        PackagePageType packType = (PackagePageType)pageIndex;
        switch (packType)
        {
            case PackagePageType.Consumables:
                m_Container = pack.GetPackage(Category.Expendable);
                break;
            case PackagePageType.Material:
                m_Container = pack.GetPackage(Category.Material);
                break;
            case PackagePageType.Weapons:
                m_Container = pack.GetPackage(Category.Weapon);
                break;
            case PackagePageType.Converters:
                m_Container = pack.GetPackage(Category.Reformer);
                break;
            case PackagePageType.Devices:
                m_Container = pack.GetPackage(Category.Equipment);
                break;
            case PackagePageType.Chips:
                m_Container = pack.GetPackage(Category.EquipmentMod);
                break;
        }

        int m_Count = m_Container != null && m_Container.Items != null ? m_Container.Items.Count : 0;
        int m_MaxCount = m_Container != null ? (int)m_Container.CurrentSizeMax : 100;

        State.SetPageLabel(State.GetPageIndex(), string.Format(GetLocalization("package_title_1008"), m_Count, m_MaxCount));
    }
}
