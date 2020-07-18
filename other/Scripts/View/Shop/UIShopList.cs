using Eternity.Runtime.Item;
using Leyoutech.Core.Loader.Config;
using PureMVC.Interfaces;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static ConfirmShopPanel;

public class UIShopList : UIListPart
{
    /// <summary>
    /// 打开确认面板参数
    /// </summary>
    private OpenShopParameter m_OpenShopParameter;
    /// <summary>
    /// 商店Proxy
    /// </summary>
    private ShopProxy m_ShopProxy;
    /// <summary>
    /// 背包Proxy
    /// </summary>
    private PackageProxy m_PackageProxy;
    /// <summary>
    /// 商店id
    /// </summary>
    private int m_ShopId;
    /// <summary>
    /// 选中页签
    /// </summary>
    private int m_LastSelectPage = 0;
    public override void OnShow(object msg)
    {
        base.OnShow(msg);
        m_ShopId = (int)msg;
        if (m_ShopProxy == null)
        {
            m_ShopProxy = GameFacade.Instance.RetrieveProxy(ProxyName.ShopProxy) as ShopProxy;
        }
        if (m_PackageProxy == null)
        {
            m_PackageProxy = GameFacade.Instance.RetrieveProxy(ProxyName.PackageProxy) as PackageProxy;
        }
        
        State.GetAction(UIAction.Shop_Buy).Callback -= OnHotKeyCallBack;
        State.GetAction(UIAction.Shop_Buy).Callback += OnHotKeyCallBack;

        State.GetAction(UIAction.Shop_Sell).Callback -= OnHotKeyCallBack;
        State.GetAction(UIAction.Shop_Sell).Callback += OnHotKeyCallBack;

        NetworkManager.Instance.GetShopController().RequestShopWindowInfo((uint)m_ShopId);
        NetworkManager.Instance.GetShopController().RequestShopSellBackInfo((uint)m_ShopId);
        State.OnSelectionChanged += OnSelectionChanged;
    }
    protected override void OnViewPartLoaded()
    {
        base.OnViewPartLoaded();
        UpdateList();
        ServerTimeUtil.Instance.OnTick += UpdateData;
        UIManager.Instance.OnUpdate += UpdateTime;
    }
    protected override void OnViewPartUnload()
    {
        LeftLabel.gameObject.SetActive(false);
        base.OnViewPartUnload();
        ServerTimeUtil.Instance.OnTick -= UpdateData;
        UIManager.Instance.OnUpdate -= UpdateTime;

    }
    protected override string GetHeadTemplate()
    {
        return AssetAddressKey.PRELOADUIELEMENT_PACKAGETITLEELEMENT;
    }

    protected override string GetCellTemplate()
    {
        UIViewListLayout style = State.GetPageLayoutStyle(State.GetPageIndex());

        if (style == UIViewListLayout.Row)
        {
            if (State.GetPageIndex() == 1)
            {
                return AssetAddressKey.PRELOADUIELEMENT_NPCSHOPSELLELEMENT_LIST;
            }
            else if (State.GetPageIndex() == 0)
            {
                return AssetAddressKey.PRELOADUIELEMENT_NPCSHOPBUYELEMENT_LIST;
            }
            else
            {
                return AssetAddressKey.PRELOADUIELEMENT_NPCSHOPBUYBACKELEMENT_LIST;
            }
        }
        else if (style == UIViewListLayout.Grid)
        {
            if (State.GetPageIndex() == 1)
            {
                return AssetAddressKey.PRELOADUIELEMENT_NPCSHOPSELLELEMENT_GRID;
            }
            else if (State.GetPageIndex() == 0)
            {
                return AssetAddressKey.PRELOADUIELEMENT_NPCSHOPBUYELEMENT_GRID;
            }
            else
            {
                return AssetAddressKey.PRELOADUIELEMENT_NPCSHOPBUYBACKELEMENT_GRID;
            }
        }
        return null;
    }
    protected override void OnPageIndexChanged(int oldIndex, int newIndex)
    {
        m_LastSelectPage = newIndex;
        UpdateList();
    }
    private void OnSelectionChanged(object obj)
    {
        if (State.GetTipData() == null)
        {
            if (State.GetPageIndex() == 0 || State.GetPageIndex() == 2)
            {
                State.GetAction(UIAction.Shop_Buy).Enabled = false;
            }
            else
            {
                State.GetAction(UIAction.Shop_Sell).Enabled = false;
            }
        }

    }
    protected override void OnCellRenderer(int groupIndex, int cellIndex, object cellData, RectTransform cellView, bool selected)
    {
        Animator m_Animator = cellView.GetComponent<Animator>();
        if (m_Animator)
        {
            m_Animator.SetBool("IsOn", selected);
        }
        UIViewListLayout style = State.GetPageLayoutStyle(State.GetPageIndex());
        if (State.GetPageIndex() == 0)
        {
            ShopWindowVO m_ShopWindowVO = (ShopWindowVO)cellData;
            NpcShopElementGrid m_ShopElementGrid = cellView.GetOrAddComponent<NpcShopElementGrid>();
            if (style == UIViewListLayout.Grid)
            {
                m_ShopElementGrid.SetData(m_ShopWindowVO, selected, false);
            }
            else
            {
                m_ShopElementGrid.SetData(m_ShopWindowVO, selected, true);
            }
            CountDownCompent m_CountDownCompent = cellView.GetOrAddComponent<CountDownCompent>();
            m_CountDownCompent.SetTime(m_ShopWindowVO.RefreshTime);
            if (selected)
            {
                if (m_ShopWindowVO.IsOpen == 0 
                    || m_ShopWindowVO.ServerLeftNum == 0 
                    || !m_ShopElementGrid.MoneyeEnough() 
                    || m_ShopWindowVO.LimitCount == 0 
                    || m_ShopWindowVO.LimitCount < m_ShopWindowVO.ShopItemConfig.Value.Bounds && m_ShopWindowVO.LimitCount > 0)
                {
                    State.GetAction(UIAction.Shop_Buy).Enabled = false;
                }
                else
                {
                    State.GetAction(UIAction.Shop_Buy).Enabled = true;
                }
                m_OpenShopParameter = new OpenShopParameter();
                m_OpenShopParameter.OperateType = 0;
                m_OpenShopParameter.Tid = m_ShopWindowVO.Tid;
                m_OpenShopParameter.Id = m_ShopWindowVO.Oid;
                m_OpenShopParameter.MoneyType = m_ShopWindowVO.ShopItemConfig.Value.MoneyType;
                m_OpenShopParameter.Price = m_ShopWindowVO.ShopItemConfig.Value.BuyCost * m_ShopWindowVO.ShopItemConfig.Value.DisCount;
                m_OpenShopParameter.LimitCount = (int)m_ShopWindowVO.LimitCount;
                m_OpenShopParameter.Stock = (int)m_ShopWindowVO.ServerLeftNum;
                m_OpenShopParameter.Bounds = m_ShopWindowVO.ShopItemConfig.Value.Bounds;
                m_OpenShopParameter.Category = ItemTypeUtil.GetItemType(m_ShopWindowVO.ShopItemConfig.Value.ItemGood.Value.Type).MainType;
            }
        }
        else if (State.GetPageIndex() == 1)
        {
            ItemBase m_PackageItem = (ItemBase)cellData;
            NpcShopSellElementGrid m_SellElementGrid = cellView.GetOrAddComponent<NpcShopSellElementGrid>();
            if (style == UIViewListLayout.Grid)
            {

                m_SellElementGrid.SetData(m_PackageItem, selected, false);
            }
            else
            {
                m_SellElementGrid.SetData(m_PackageItem, selected, true);
            }
            if (selected)
            {
                if (m_PackageItem.Replicas != null && m_PackageItem.Replicas.Count > 0)
                {
                    State.GetAction(UIAction.Shop_Sell).Enabled = false;
                }
                else if (m_PackageItem.MainType == Category.Expendable)
                {
                    State.GetAction(UIAction.Shop_Sell).Enabled = false;
                }
                else
                {
                    State.GetAction(UIAction.Shop_Sell).Enabled = true;
                }
                m_OpenShopParameter = new OpenShopParameter();
                m_OpenShopParameter.OperateType = 1;
                m_OpenShopParameter.Tid = m_PackageItem.TID;
                m_OpenShopParameter.Id = m_PackageItem.UID;
                if (m_PackageItem.ItemConfig.SellCurrency == 1100004)
                {
                    m_OpenShopParameter.MoneyType = 1;
                }
                else
                {
                    m_OpenShopParameter.MoneyType = 2;
                }
                m_OpenShopParameter.Price = (int)m_PackageItem.ItemConfig.MoneyPrice;
                m_OpenShopParameter.LimitCount = (int)m_PackageItem.Count;
                m_OpenShopParameter.Stock = (int)m_PackageItem.Count;
                m_OpenShopParameter.Bounds = 1;
                m_OpenShopParameter.Category = m_PackageItem.MainType;
            }
        }
        else
        {
            ShopSellBackVO m_SellBack = (ShopSellBackVO)cellData;
            CountDownCompent m_CountDownCompent = cellView.GetOrAddComponent<CountDownCompent>();
            m_CountDownCompent.SetTime(m_SellBack.ExpireTime);
            NpcShopSellBackElementGrid m_SellElementGrid = cellView.GetOrAddComponent<NpcShopSellBackElementGrid>();
            if (style == UIViewListLayout.Grid)
            {
                m_SellElementGrid.SetData(m_SellBack, false);
            }
            else
            {
                m_SellElementGrid.SetData(m_SellBack, true);
            }
            if (selected)
            {
                if (!m_SellElementGrid.MoneyeEnough())
                {
                    State.GetAction(UIAction.Shop_Buy).Enabled = false;
                }
                else
                {
                    State.GetAction(UIAction.Shop_Buy).Enabled = true;
                }
                m_OpenShopParameter = new OpenShopParameter();
                m_OpenShopParameter.OperateType = 2;
                m_OpenShopParameter.Tid = (uint)m_SellBack.Tid;
                m_OpenShopParameter.Id = m_SellBack.Uid;
                if (m_SellBack.ItemConfig.SellCurrency == 1100004)
                {
                    m_OpenShopParameter.MoneyType = 1;
                }
                else
                {
                    m_OpenShopParameter.MoneyType = 2;
                }
                m_OpenShopParameter.Price = m_SellBack.ItemConfig.BuybackPrice;
                m_OpenShopParameter.LimitCount = (int)m_SellBack.Num;
                m_OpenShopParameter.Stock = (int)m_SellBack.Num;
                m_OpenShopParameter.Bounds = 1;
                m_OpenShopParameter.Category = ItemTypeUtil.GetItemType(m_SellBack.ItemConfig.Type).MainType;
            }
        }
    }
    protected override void OnCellPlaceholderRenderer(int groupIndex, int cellIndex, RectTransform cellView, bool selected)
    {
        Animator m_Animator = cellView.GetComponent<Animator>();
        if (m_Animator)
            m_Animator.SetBool("IsOn", selected);
        if (selected)
        {
            State.GetAction(UIAction.Shop_Buy).Enabled = false;
            State.GetAction(UIAction.Shop_Sell).Enabled = false;
        }
    }

    public override NotificationName[] ListNotificationInterests()
    {
        return new NotificationName[]
        {
            NotificationName.MSG_SHOP_CHANGE,
        };
    }

    public override void HandleNotification(INotification notification)
    {
        switch (notification.Name)
        {
            case NotificationName.MSG_SHOP_CHANGE:
                ServerTimeUtil.Instance.OnTick -= UpdateData;
                ServerTimeUtil.Instance.OnTick += UpdateData;
                UpdateList();
                break;
        }
    }
    public override void OnHide()
    {
        m_LastSelectPage = 0;
        State.GetAction(UIAction.Shop_Buy).Callback -= OnHotKeyCallBack;
        State.GetAction(UIAction.Shop_Sell).Callback -= OnHotKeyCallBack;
        State.OnSelectionChanged -= OnSelectionChanged;
        base.OnHide();       
    }
    /// <summary>
    /// 商店倒计时结束数据刷新
    /// </summary>
    private void UpdateData()
    {
        bool m_IsRefresh = false;

        for (int i = 0; i < CountDownCompent.m_CountDowns.Count; i++)
        {
            CountDownCompent countDown = CountDownCompent.m_CountDowns[i];
            if (countDown.m_Time <= ServerTimeUtil.Instance.GetNowTime())
            {
                if (countDown.m_Time > 0)
                {
                    m_IsRefresh = true;
                }
            }
        }
        if (m_ShopProxy.GetRefreshTime() <= ServerTimeUtil.Instance.GetNowTime())
        {
            if (m_ShopProxy.GetRefreshTime() > 0)
            {
                m_IsRefresh = true;
            }
        }
        if (m_IsRefresh)
        {
            UIManager.Instance.StartCoroutine(GetEnumerator());
        }
    }
    /// <summary>
    /// 延迟发送倒计时刷新消息
    /// </summary>
    /// <returns></returns>
    private IEnumerator GetEnumerator()
    {
        yield return new WaitForSeconds(0.2f);
        ServerTimeUtil.Instance.OnTick -= UpdateData;
        if (m_LastSelectPage == 0)
        {
            NetworkManager.Instance.GetShopController().RequestShopWindowInfo((uint)m_ShopId);
        }
        else
        {
            NetworkManager.Instance.GetShopController().RequestShopSellBackInfo((uint)m_ShopId);
        }
    }
    /// <summary>
    /// 倒计时时间刷新
    /// </summary>
    private void UpdateTime()
    {
        for (int i = 0; i < CountDownCompent.m_CountDowns.Count; i++)
        {
            CountDownCompent countDown = CountDownCompent.m_CountDowns[i];
            if (countDown.m_Time <= ServerTimeUtil.Instance.GetNowTime())
            {
                countDown.m_CountDownLabel.gameObject.SetActive(false);
            }
            else
            {
                long leftTime = (long)countDown.m_Time - (long)ServerTimeUtil.Instance.GetNowTime();
                countDown.m_CountDownLabel.gameObject.SetActive(true);
                SetTime(leftTime, countDown.m_CountDownLabel, "shop_text_1005");
            }
        }
        if (m_ShopProxy.GetRefreshTime() <= ServerTimeUtil.Instance.GetNowTime())
        {
            LeftLabel.gameObject.SetActive(false);
        }
        else
        {
            if (State.GetPageIndex() == 0)
            {
                long refreshTime = (long)m_ShopProxy.GetRefreshTime() - (long)ServerTimeUtil.Instance.GetNowTime();
                LeftLabel.gameObject.SetActive(true);
                SetTime(refreshTime, LeftLabel, "shop_text_1004");
            }
            else
            {
                LeftLabel.gameObject.SetActive(false);
            }
        }
    }
    /// <summary>
    /// 商品排序
    /// </summary>
    private ShopWindowVO left;
    private ShopWindowVO right;
    protected override int Compare(UIViewSortKind kind, object a, object b)
    {
        int result;
        if (State.GetPageIndex() == 0)
        {
            left = a as ShopWindowVO;
            right = b as ShopWindowVO;
            return result = (int)(left.Order - right.Order);
        }
        return 0;
    }
    /// <summary>
    /// 列表数据刷新
    /// </summary>
    /// <param name="index"></param>
    private void UpdateList()
    {
        if (GetTransform() == null)
        {
            return;
        }
        ClearData();
        int index = State.GetPageIndex();
        if (index == 0)
        {
            SetSortEnabled(true);
            AddDatas(null, m_ShopProxy.GetSellData());
        }
        else if (index == 1)
        {
            SetSortEnabled(false);
            AddDatas(null, GetPackageData());
        }
        else
        {
            SetSortEnabled(false);
            AddDatas(null, m_ShopProxy.GetSellBackData());
        }
    }
    /// <summary>
    /// 开启二次弹窗
    /// </summary>
    /// <param name="callback"></param>
    private void OnHotKeyCallBack(HotkeyCallback callback)
    {
        if (OwnerView.State.GetTipData() == null)
            return;

        if (callback.performed)
        {
            UIManager.Instance.OpenPanel(UIPanel.ConfirmShopPanel, m_OpenShopParameter);
        }
    }
    #region 工具函数
    /// <summary>
    /// 设置倒计时时间
    /// </summary>
    /// <param name="time"></param>
    private void SetTime(long time, TMP_Text label, string key = "shop_text_1004")
    {
        //剩余时间
        long days = 0;
        long hours = 0;
        long minutes = 0;
        long seconds = 0;
        TimeUtil.Time_msToMinutesAndSeconds(time, ref days, ref hours, ref minutes, ref seconds);
        if (days > 0)
        {
            hours = hours + days * 24;
        }
        label.text = string.Format(TableUtil.GetLanguageString(key), hours.ToString("00"), minutes.ToString("00"), seconds.ToString("00"));
    }
    /// <summary>
    /// 出售背包数据集合
    /// </summary>
    /// <returns></returns>
    public ItemBase[] GetPackageData()
    {
        List<ItemBase> m_PackageList = new List<ItemBase>();
        m_PackageList.AddRange(m_PackageProxy.GetItemList<ItemBase>(Category.Expendable));
        m_PackageList.AddRange(m_PackageProxy.GetItemList<ItemBase>(Category.Material));
        m_PackageList.AddRange(m_PackageProxy.GetItemList<ItemBase>(Category.Weapon));
        m_PackageList.AddRange(m_PackageProxy.GetItemList<ItemBase>(Category.Reformer));
        m_PackageList.AddRange(m_PackageProxy.GetItemList<ItemBase>(Category.Equipment));
        m_PackageList.AddRange(m_PackageProxy.GetItemList<ItemBase>(Category.EquipmentMod));
        return m_PackageList.ToArray();
    }

    #endregion

}