using Assets.Scripts.Define;
using Eternity.FlatBuffer;
using Eternity.Runtime.Item;
using Leyoutech.Core.Loader.Config;
using PureMVC.Interfaces;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmShopPanel : UIPanelBase
{
    private const string ASSET_ADDRESS = AssetAddressKey.PRELOADUI_CONFIRMSHOPPANEL;

    /// <summary>
    /// 最小值文本框
    /// </summary>
    private TMP_Text m_MinNum;
    /// <summary>
    /// 最大值文本框
    /// </summary>
    private TMP_Text m_MaxNum;
    /// <summary>
    /// 提示文字
    /// </summary>
    private TMP_Text m_Tips;
    /// <summary>
    /// 商品名称
    /// </summary>
    private TMP_Text m_GoodName;
    /// <summary>
    /// 输入框
    /// </summary>
    private TMP_InputField m_InputBox;
    /// <summary>
    /// 货币容器
    /// </summary>
    private Transform m_MoneyRoot;
    /// <summary>
    /// 货币图片
    /// </summary>
    private Image m_MoneyIcon;
    /// <summary>
    /// 售出总价
    /// </summary>
    private TMP_Text m_TotalPrice;
    /// <summary>
    /// 递减按键
    /// </summary>
    private Transform m_BtnLeft;
    /// <summary>
    /// 递增按键
    /// </summary>
    private Transform m_BtnRight;
    /// <summary>
    /// 选择按键挂点
    /// </summary>
    private Transform m_HotKeyRoot;
    /// <summary>
    /// 最小值
    /// </summary>
    private long m_Min = 1;
    /// <summary>
    /// 最大值
    /// </summary>
    private long m_Max = 0;
    /// <summary>
    /// 单位数量
    /// </summary>
    private int m_Bounds = 0;
    /// <summary>
    /// 当前输入框数量
    /// </summary>
    private long m_CurrentNum = 0;
    /// <summary>
    /// 拥有货币
    /// </summary>
    private int m_Money;
    /// <summary>
    /// 打开二次确认参数
    /// </summary>
    private OpenShopParameter m_ShopParameter;
    /// <summary>
    /// 背包Proxy
    /// </summary>
    private PackageProxy m_PackageProxy;
    /// <summary>
    /// CfgEternityProxy
    /// </summary>
    private CfgEternityProxy m_CfgEternityProxy;
    /// <summary>
    /// ShipProxy
    /// </summary>
    private ShipProxy m_ShipProxy;
    /// <summary>
    /// 商店Proxy
    /// </summary>
    private ShopProxy m_ShopProxy;
    /// <summary>
    /// 物品数据
    /// </summary>
    private Item m_ItemConfig;
    /// <summary>
    /// 协程
    /// </summary>
    private Coroutine m_Coroutine;
    public ConfirmShopPanel() : base(UIPanel.ConfirmShopPanel, ASSET_ADDRESS, PanelType.Notice)
    {

    }

    public override void Initialize()
    {
        m_MinNum = FindComponent<TMP_Text>("Content/Label_Min");
        m_MaxNum = FindComponent<TMP_Text>("Content/Label_Max");
        m_Tips = FindComponent<TMP_Text>("Content/Label_Tips");
        m_GoodName = FindComponent<TMP_Text>("Content/Label_Item_Name");
        m_InputBox = FindComponent<TMP_InputField>("Content/Input");
        m_TotalPrice = FindComponent<TMP_Text>("Content/Money/Coin1/Label_Coin");
        m_MoneyRoot = FindComponent<Transform>("Content/Money");
        m_MoneyIcon = FindComponent<Image>("Content/Money/Coin1/Image_Icon");
        m_BtnLeft = FindComponent<Transform>("Content/Btn_Left");
        m_BtnRight = FindComponent<Transform>("Content/Btn_Right");
        m_HotKeyRoot = FindComponent<Transform>("Control/Footer/ContentHotKey");
        m_PackageProxy = GameFacade.Instance.RetrieveProxy(ProxyName.PackageProxy) as PackageProxy;
        m_CfgEternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
        m_ShopProxy = GameFacade.Instance.RetrieveProxy(ProxyName.ShopProxy) as ShopProxy;
        m_ShipProxy = GameFacade.Instance.RetrieveProxy(ProxyName.ShipProxy) as ShipProxy;
    }

    public override void OnShow(object msg)
    {
        base.OnShow(msg);
        if (msg != null)
        {
            m_ShopParameter = (OpenShopParameter)msg;
        }
        m_ItemConfig = m_CfgEternityProxy.GetItemByKey(m_ShopParameter.Tid);
        m_Bounds = m_ShopParameter.Bounds;
        if (m_ShopParameter.MoneyType == 1)
        {
            m_Money = (int)CurrencyUtil.GetGameCurrencyCount();
        }
        else
        {
            m_Money = (int)CurrencyUtil.GetRechargeCurrencyCount();
        }
        if (m_ShopParameter.OperateType == 1)
        {
            m_Max = m_ShopParameter.LimitCount;
        }
        else
        {
            GetGoodMax();
            if (m_Max == 0)
            {
                m_Min = 0;
            }
        }
        m_GoodName.text = TableUtil.GetItemName(m_ShopParameter.Tid);
        m_GoodName.color = ColorUtil.GetColorByItemQuality(m_ItemConfig.Quality);
        UIUtil.SetIconImage(m_MoneyIcon, TableUtil.GetItemIconBundle((KNumMoneyType)m_ShopParameter.MoneyType), TableUtil.GetItemIconImage((KNumMoneyType)m_ShopParameter.MoneyType));
        m_InputBox.onValueChanged.RemoveAllListeners();
        m_CurrentNum = m_Bounds;
        m_InputBox.text = m_Bounds.ToString();
        m_TotalPrice.text = Mathf.CeilToInt(m_CurrentNum * (m_ShopParameter.Price / m_ShopParameter.Bounds)).ToString();

        m_InputBox.onValueChanged.AddListener((str) =>
        {
            if (str.Length > 0)
            {
                m_CurrentNum = int.Parse(str);
                if (m_CurrentNum < m_Min)
                {
                    m_CurrentNum = m_Min;
                }
                else if (m_CurrentNum > m_Max)
                {
                    m_CurrentNum = m_Max;
                }
                m_InputBox.text = m_CurrentNum.ToString();
                m_BtnLeft.GetComponent<Button>().interactable = m_CurrentNum > m_Bounds;
                m_BtnRight.GetComponent<Button>().interactable = m_CurrentNum < m_Max;
                SetHotKeyEnabled("left", m_CurrentNum > m_Bounds);
                SetHotKeyEnabled("Right", m_CurrentNum < m_Max);
                int m_Price = Mathf.CeilToInt(m_CurrentNum * (m_ShopParameter.Price / m_ShopParameter.Bounds));
                m_TotalPrice.text = m_Price.ToString();
                if (m_ShopParameter.OperateType != 1)
                {
                    SetHotKeyEnabled("confirm", m_Money >= m_Price && m_CurrentNum > 0);
                }
                else
                {
                    SetHotKeyEnabled("confirm", true);
                }
            }
            else
            {
                m_CurrentNum = 0;
                int m_Price = 0;
                m_TotalPrice.text = m_Price.ToString();
                SetHotKeyEnabled("confirm", false);
            }

        });
        UIEventListener.UIEventListener.AttachListener(m_BtnLeft).onDown = OnLeftDonw;
        UIEventListener.UIEventListener.AttachListener(m_BtnLeft).onUp = OnUp;
        UIEventListener.UIEventListener.AttachListener(m_BtnRight).onDown = OnRightDown;
        UIEventListener.UIEventListener.AttachListener(m_BtnRight).onUp = OnUp;
    }
    public override void OnGotFocus()
    {
        AddHotKey("left", HotKeyID.NavLeft, Decrease);
        AddHotKey("Right", HotKeyID.NavRight, Add);
        AddHotKey("confirm", HotKeyID.FuncA, Confirm, m_HotKeyRoot, TableUtil.GetLanguageString("common_hotkey_id_1001"));
        AddHotKey(HotKeyID.FuncB, Close, m_HotKeyRoot, TableUtil.GetLanguageString("common_hotkey_id_1002"));
    }
    public override void OnRefresh(object msg)
    {
        m_BtnLeft.GetComponent<Button>().interactable = m_CurrentNum > m_Bounds;
        m_BtnRight.GetComponent<Button>().interactable = m_CurrentNum < m_Max;
        m_BtnLeft.GetComponent<ButtonWithSound>().enabled = m_CurrentNum > m_Bounds;
        m_BtnRight.GetComponent<ButtonWithSound>().enabled = m_CurrentNum < m_Max;
        SetHotKeyEnabled("left", m_CurrentNum > m_Bounds);
        SetHotKeyEnabled("Right", m_CurrentNum < m_Max);
        SetHotKeyEnabled("confirm", m_CurrentNum <= m_Max);
        m_MoneyRoot.gameObject.SetActive(m_CurrentNum <= m_Max);
        if (m_CurrentNum > m_Max)
        {
            m_Tips.gameObject.SetActive(true);
            m_Tips.text = TableUtil.GetLanguageString("shop_text_1022");
        }
        else
        {
            m_Tips.gameObject.SetActive(false);
        }

    }
    public override NotificationName[] ListNotificationInterests()
    {
        return new NotificationName[]
       {
            NotificationName.MSG_SHOP_EXCHANGE
       };
    }
    public override void HandleNotification(INotification notification)
    {
        switch (notification.Name)
        {
            case NotificationName.MSG_SHOP_EXCHANGE:
                FinishExchange((byte)notification.Body);
                break;
        }
    }
    /// <summary>
    /// 左方向键按下
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void OnLeftDonw(GameObject sender, params object[] args)
    {
        m_BtnLeft.GetComponent<ButtonWithSound>().enabled = m_CurrentNum > m_Bounds;
        ChangeNum(false);
        m_Coroutine = UIManager.Instance.StartCoroutine(GetEnumerator(false));
    }
    /// <summary>
    /// 右方向键按下
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void OnRightDown(GameObject sender, params object[] args)
    {
        m_BtnRight.GetComponent<ButtonWithSound>().enabled = m_CurrentNum < m_Max;
        ChangeNum(true);
        m_Coroutine = UIManager.Instance.StartCoroutine(GetEnumerator(true));
    }
    /// <summary>
    /// 按键抬起
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void OnUp(GameObject sender, params object[] args)
    {
        UIManager.Instance.StopCoroutine(m_Coroutine);
    }
    /// <summary>
    /// 长按数量变化协程
    /// </summary>
    /// <param name="isAdd"></param>
    /// <returns></returns>
    private IEnumerator GetEnumerator(bool isAdd)
    {
        yield return new WaitForSeconds(0.2f);
        while (true)
        {
            ChangeNum(isAdd);
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
        }
    }
    /// <summary>
    /// 修改数量
    /// </summary>
    /// <param name="isAdd">是否增加</param>
    private void ChangeNum(bool isAdd)
    {
        if (isAdd)
        {
            if (m_CurrentNum < m_Max)
            {
                m_CurrentNum = m_CurrentNum + m_Bounds;
                if (m_CurrentNum >= m_Max)
                {
                    m_CurrentNum = m_Max;
                }
            }
        }
        else
        {
            if (m_CurrentNum > m_Bounds)
            {
                m_CurrentNum = m_CurrentNum - m_Bounds;
                if (m_CurrentNum <= m_Bounds)
                {
                    m_CurrentNum = m_Bounds;
                }
            }
        }

        if (m_CurrentNum == m_Bounds || m_CurrentNum == m_Max)
        {
            if (m_Coroutine != null)
            {
                UIManager.Instance.StopCoroutine(m_Coroutine);
            }
        }
        m_InputBox.text = m_CurrentNum.ToString();
    }
    /// <summary>
    /// 热键数量减少
    /// </summary>
    private void Decrease(HotkeyCallback callback)
    {
        if (callback.started)
        {
            ChangeNum(false);
            m_Coroutine = UIManager.Instance.StartCoroutine(GetEnumerator(false));
        }
        if (callback.performed)
        {
            UIManager.Instance.StopCoroutine(m_Coroutine);
        }
    }
    /// <summary>
    /// 热键数量增加
    /// </summary>
    /// <param name="callback"></param>
    private void Add(HotkeyCallback callback)
    {
        if (callback.started)
        {
            ChangeNum(true);
            m_Coroutine = UIManager.Instance.StartCoroutine(GetEnumerator(true));
        }
        if (callback.performed)
        {
            UIManager.Instance.StopCoroutine(m_Coroutine);
        }
    }
    /// <summary>
    /// 物品最大数量
    /// </summary>
    private void GetGoodMax()
    {
        //玩家钱包容量
        int m_MoneyNum = Mathf.FloorToInt(m_Money * 1.0f / (m_ShopParameter.Price / m_ShopParameter.Bounds));
        //库存
        int m_Stock = m_ShopParameter.Stock;
        //背包空间
        ItemContainer m_Container = m_PackageProxy.GetPackage(m_ShopParameter.Category);       
        long m_PackageNum = 0;
        //船包
        if (m_Container == null)
        {
            Item item = m_CfgEternityProxy.GetItemByKey(m_ShopParameter.Tid);           
            WarshipL1 warshipL1 = 0;
            ItemTypeUtil.SetSubType(ref warshipL1, ItemTypeUtil.GetItemType(item.Type));            
            m_PackageNum = m_ShipProxy.GetShipContainer(warshipL1).CurrentSizeMax - m_ShipProxy.GetShipByType(warshipL1).Length;
        }
        //普通包
        else 
        {
            //拥有数量
            int m_LeftNum;
            //剩余堆
            int m_Stack = 0;
            if (m_Container.Items == null)
            {
                m_LeftNum = 0;
            }
            else
            {
                m_LeftNum = m_Container.Items.Count;
            }
            int m_Empty = (int)m_Container.CurrentSizeMax - m_LeftNum;


            if ((m_PackageProxy.GetItemCountByTID(m_ShopParameter.Tid) % m_ItemConfig.StackingLimit) > 0)
            {
                m_Stack = (int)(m_ItemConfig.StackingLimit - (m_PackageProxy.GetItemCountByTID(m_ShopParameter.Tid) % m_ItemConfig.StackingLimit));
            }
            else
            {
                m_Stack = 0;
            }
            //无限堆包内堆已满
            if (m_ItemConfig.Heaps == 0 && m_Stack == 0)
            {
                m_PackageNum = m_ItemConfig.StackingLimit * m_Empty;
            }
            //无限堆包内堆未满
            else if (m_ItemConfig.Heaps == 0 && m_Stack > 0)
            {
                m_PackageNum = m_Stack + m_Empty * m_ItemConfig.StackingLimit;
            }
            //单堆
            else
            {
                m_PackageNum = m_ItemConfig.StackingLimit - m_PackageProxy.GetItemCountByTID(m_ShopParameter.Tid);
            }
        }
       
        List<long> m_Capacity = new List<long>() { m_MoneyNum, m_PackageNum };
        if (m_Stock != -1)
        {
            m_Capacity.Add(m_Stock);
        }
        if (m_ShopParameter.LimitCount != -1)
        {
            m_Capacity.Add(m_ShopParameter.LimitCount);
        }
        m_Max = m_Capacity.Min();
    }
    /// <summary>
    /// 延迟关闭界面
    /// </summary>
    /// <returns></returns>
    private IEnumerator ClosePanel()
    {
        yield return new WaitForSeconds(1f);
        UIManager.Instance.ClosePanel(UIPanel.ConfirmShopPanel);
        GameFacade.Instance.SendNotification(NotificationName.MSG_SHOP_CHANGE);
    }
    /// <summary>
    /// 交易完成
    /// </summary>
    private void FinishExchange(byte result)
    {
        SetHotKeyEnabled("confirm", false);
        m_Tips.gameObject.SetActive(true);
        m_MoneyRoot.gameObject.SetActive(false);
        if (result == 1)
        {
            m_Tips.text = TableUtil.GetLanguageString("shop_text_1020");
        }
        else
        {
            m_Tips.text = TableUtil.GetLanguageString("shop_text_1019");
        }
        UIManager.Instance.StartCoroutine(ClosePanel());
    }
    /// <summary>
    /// 确认操作
    /// </summary>
    private void Confirm(HotkeyCallback callback)
    {
        if (callback.performed)
        {
            NetworkManager.Instance.GetShopController().RequestExChange
            (
                (byte)m_ShopParameter.OperateType,
                m_ShopProxy.GetShopId(),
                m_ShopParameter.Tid,
                m_ShopParameter.Id,
                (uint)m_CurrentNum
            );
        }
    }
    /// <summary>
    /// 关闭弹窗
    /// </summary>
    /// <param name="callback"></param>
    private void Close(HotkeyCallback callback)
    {
        if (callback.performed)
        {
            UIManager.Instance.ClosePanel(this);
        }
    }
    public override void OnHide(object msg)
    {
        base.OnHide(msg);
    }
    public struct OpenShopParameter
    {
        /// <summary>
        /// 操作类型
        /// </summary>
        public int OperateType;
        /// <summary>
        /// 商品Tid
        /// </summary>
        public uint Tid;
        /// <summary>
        /// 商品Id
        /// </summary>
        public ulong Id;
        /// <summary>
        /// 单位限量
        /// </summary>
        public int LimitCount;
        /// <summary>
        /// 单位数量
        /// </summary>
        public int Bounds;
        /// <summary>
        /// 库存
        /// </summary>
        public int Stock;
        /// <summary>
        /// 价格
        /// </summary>
        public float Price;
        /// <summary>
        /// 货币类型
        /// </summary>
        public int MoneyType;
        /// <summary>
        /// 商品类型
        /// </summary>
        public Category Category;
    }

}
