using Eternity.FlatBuffer;
using Eternity.Runtime.Item;
using Leyoutech.Core.Loader.Config;
using PureMVC.Interfaces;
using PureMVC.Patterns.Facade;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class UIProducePartList : UIListPart
{
    /// <summary>
    /// 单条资源路径
    /// </summary>
    private const string ELEMENT_ADDRESS = AssetAddressKey.PRELOADUIELEMENT_PRODUCEELELMENT_LIST;
    /// <summary>
    /// 标题栏资源路径
    /// </summary>
    private const string TITLE_ADDRESS = AssetAddressKey.PRELOADUIELEMENT_PACKAGETITLEELEMENT;

    /// <summary>
    /// 按住触发事件的时长 改为可变值
    /// </summary>
    private float HOLD_TIME;

    /// <summary>
    /// 每60秒消耗货币
    /// </summary>
    private const float SECOND = 60;

    /// <summary>
    /// 每60秒消耗几个货币
    /// </summary>
    private float m_ExpendNumber = 1;

    /// <summary>
	/// 生产数据
	/// </summary>
	protected FoundryProxy m_FoundryProxy;

    /// <summary>
    /// 背包数据
    /// </summary>
    private PackageProxy m_PackageProxy;

    /// <summary>
    /// 船数据
    /// </summary>
    private ShipProxy m_ShipProxy;

    /// <summary>
    /// 创角Proxy
    /// </summary>
    private ServerListProxy m_ServerListProxy;

    /// <summary>
    /// Proxy
    /// </summary>
    private CfgEternityProxy m_CfgEternityProxy;

    /// <summary>
	/// 当前鼠标选中的蓝图ID
	/// </summary>
	private int m_SelectProduceTid = 0;

    /// <summary>
	/// 处理过的状态和属性的格子数据
	/// </summary>
	protected List<ProduceInfoVO> m_GridList = new List<ProduceInfoVO>();

    /// <summary>
	/// 当前大类型的数据
	/// </summary>
	protected List<Produce> m_CurrentMainTypeList = new List<Produce>();

    /// <summary>
    /// 零件数据
    /// </summary>
    protected List<Produce> m_PartProduceList = new List<Produce>();

    /// <summary>
	/// 当前顶部一级页签类型
	/// </summary>
	protected ProduceRightType m_CurrentRightTabType;

    /// <summary>
    ///  当前级别
    /// </summary>
    protected int m_Grad;

    /// <summary>
	///  保存当前级别
	/// </summary>
	protected int m_GradOld;

    /// <summary>
	/// 当前生产排序类型
	/// </summary>
	protected SortProduceType m_CurrentSortProduceType;

    ///// <summary>
    ///// 当前视图
    ///// </summary>
    protected ProduceView m_ProduceView;

    /// <summary>
	/// 当前生产类型
	/// </summary>
	protected ProduceType m_CurrentType;

    /// <summary>
    /// 上次选择的ProduceElelment蓝图ID
    /// </summary>
    private int m_OldProduceTid;
    /// <summary>
    /// 当前生产的蓝图Id
    /// </summary>
    protected int m_ProducingTid = 0;

    /// <summary>
	/// 是否对比
	/// </summary>
	protected bool isCompare = false;

    /// <summary>
    /// 废弃生产
    /// </summary>
    private bool m_Discard = false;

    /// <summary>
	/// 是否长按下生产键
	/// </summary>
	private bool m_HoldProduceDown;

    /// <summary>
    /// 是否长按下取消键
    /// </summary>
    protected bool m_HoldCancelDown;

    /// <summary>
	/// 音效进度变化参数，比例系数
	/// </summary>
	private float m_SoundRtpcOffet = 10.0f;

    /// <summary>
	/// 当前对比武器index
	/// </summary>
	protected int m_ComperWeaponIndex;

    /// <summary>
    /// 视图打开时调用
    /// </summary>
    /// <param name="owner">父视图</param>
    public override void OnShow(object msg)
    {
        base.OnShow(msg);
        m_FoundryProxy = GameFacade.Instance.RetrieveProxy(ProxyName.FoundryProxy) as FoundryProxy;
        m_PackageProxy = GameFacade.Instance.RetrieveProxy(ProxyName.PackageProxy) as PackageProxy;
        m_CfgEternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
        m_ServerListProxy = GameFacade.Instance.RetrieveProxy(ProxyName.ServerListProxy) as ServerListProxy;
        m_ShipProxy = GameFacade.Instance.RetrieveProxy(ProxyName.ShipProxy) as ShipProxy;
        HOLD_TIME = State.GetAction(UIAction.Product_Accelerate).StateList[0].Time;
        m_ExpendNumber = m_CfgEternityProxy.GetGamingConfig(1).Value.Produce.Value.FinishExpenseRate;
        m_ProduceView = OwnerView as ProduceView;
        m_FoundryProxy.GetAllDataByTable();
        m_PartProduceList = m_FoundryProxy.GetDataByMainType(BlueprintL1.Material);//零件
        NetworkManager.Instance.GetFoundryController().SendGetFoundryInfo();
        MsgOpenProduce msgOpenProduce=(MsgOpenProduce)msg;
        m_CurrentType = msgOpenProduce.CurrentProduceType;
        m_Grad = (int)msgOpenProduce.MProduceDialogType;
        m_GradOld = m_Grad;

        State.OnSelectionChanged -= OnSelectionDataChanged;
        State.OnSelectionChanged += OnSelectionDataChanged;
        State.GetAction(UIAction.Product_Accelerate).Callback += OnProduce;
        State.GetAction(UIAction.Product_Cancel).Callback += OnCanelProduce;
        State.GetAction(UIAction.Product_ProduceAndCollect).Callback += OnRevecie;
        State.GetAction(UIAction.Product_Accelerate).Visible = false;
        switch (m_CurrentType)
        {
            case ProduceType.HeavyWeapon:
                m_CurrentBlueprintType = BlueprintL1.Weapon;
                break;
            case ProduceType.Reformer:
                m_CurrentBlueprintType = BlueprintL1.Reformer;
                break;
            case ProduceType.Chip:
                m_CurrentBlueprintType = BlueprintL1.EquipmentMod;
                break;
            case ProduceType.Device:
                m_CurrentBlueprintType = BlueprintL1.Equipment;
                break;
            case ProduceType.Ship:
                m_CurrentBlueprintType = BlueprintL1.Warship;
                m_FoundryProxy.InitShipPackage();
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 视图关闭时调用
    /// </summary>
    /// <param name="owner">父视图</param>
    public override void OnHide()
    {
        base.OnHide();
        State.GetAction(UIAction.Product_Accelerate).Callback -= OnProduce;
        State.GetAction(UIAction.Product_Cancel).Callback -= OnCanelProduce;
        State.OnSelectionChanged -= OnSelectionDataChanged;
        State.GetAction(UIAction.Product_ProduceAndCollect).Callback -= OnRevecie;
    }

    protected override void OnViewPartLoaded()
    {
        base.OnViewPartLoaded();
        State.SetPageIndex(m_GradOld + 1);
        ShowCharacter();

    }
    protected override void OnViewPartUnload()
    {
        base.OnViewPartUnload();
    }

    protected override string GetHeadTemplate()
    {
        return TITLE_ADDRESS;
    }

    public override NotificationName[] ListNotificationInterests()
    {
        return new NotificationName[]
        {
            NotificationName.MSG_FOUNDRY_UPDATE,
            NotificationName.MSG_PRODUT_UPDATE,
            NotificationName.MSG_CURRENCY_CHANGED,
        };
    }

    public override void HandleNotification(INotification notification)
    {
        switch ((NotificationName)notification.Name)
        {
            case NotificationName.MSG_FOUNDRY_UPDATE:
                UpdateDataByServer(notification.Body);
                break;
            case NotificationName.MSG_PRODUT_UPDATE:
                RefreshHotKey((ProduceState)notification.Body);
                break;
            case NotificationName.MSG_CURRENCY_CHANGED:
                break;
            default:
                break;
        }
    }

    protected override string GetCellTemplate()
    {
        int pageIndex = State.GetPageIndex();
        UIViewListLayout style = State.GetPageLayoutStyle(State.GetPageIndex());

        if (style == UIViewListLayout.Row)
        {
            return AssetAddressKey.PRELOADUIELEMENT_PRODUCEELELMENT_LIST;
        }
        else if (style == UIViewListLayout.Grid)
        {
            return AssetAddressKey.PRELOADUIELEMENT_PRODUCEELELMENT_GRID;
        }
        return null;
    }
    protected override void OnCellPlaceholderRenderer(int groupIndex, int cellIndex, RectTransform cellView, bool selected)
    {
        Animator animator = cellView.GetComponent<Animator>();
        if (animator)
            animator.SetBool("IsOn", selected);
        if (selected)
        {
            m_SelectProduceTid = 0;
            RefreshHotKey(ProduceState.CanNotProduce);
        }
    }

    protected override void OnCellRenderer(int groupIndex, int cellIndex, object cellData, RectTransform cellView, bool selected)
    {
        UIViewListLayout style = State.GetPageLayoutStyle(State.GetPageIndex());

        Animator animator = cellView.GetComponent<Animator>();
        RectTransform content = FindComponent<RectTransform>(cellView, "Content");
        if (animator)
            animator.SetBool("IsOn", selected);

        int page = groupIndex;
        int index = cellIndex;
        ProduceInfoVO infoVO = (ProduceInfoVO)cellData;
        cellView.name = infoVO.TID.ToString();
        ProduceElelment m_ProduceElelmentNew = cellView.GetComponent<ProduceElelment>();
        if (m_ProduceElelmentNew == null)
        {
            m_ProduceElelmentNew = cellView.gameObject.AddComponent<ProduceElelment>();
            m_ProduceElelmentNew.Initialize();
        }
        m_ProduceElelmentNew.SetData(infoVO, m_CurrentType,style);
        infoVO.Elelment = m_ProduceElelmentNew;
       
    }

    private void OnSelectionDataChanged(object obj)
    {
        if (!m_Discard)
            SetHotKeyEnabled(UIAction.Product_ProduceAndCollect, false, 0,false);

        if (obj is ProduceInfoVO)
        {
            ProduceInfoVO item = obj as ProduceInfoVO;
            m_SelectProduceTid = item.TID;
            if (m_OldProduceTid != m_SelectProduceTid)
            {
                m_OldProduceTid = m_SelectProduceTid;
            }
            if (m_FoundryProxy.GetBluePrintDic().TryGetValue(m_SelectProduceTid, out ProduceInfoVO girdInfo))
            {
                RefreshHotKey(girdInfo.BluePrintState);
            }
        }
        else
        {
            m_SelectProduceTid = 0;
            if (m_OldProduceTid != m_SelectProduceTid)
            {
                m_OldProduceTid = m_SelectProduceTid;
            }
            SetHotKeyEnabled(UIAction.Product_ProduceAndCollect, false, 0,false);
            SetHotKeyEnabled(UIAction.Product_Cancel, false, 0, false);
        }
    }

    /// <summary>
    /// 设置一级页签类型 
    /// </summary>
    public virtual void SetPageLabel()
    {
        int pageIndex = State.GetPageIndex();
        switch (pageIndex)
        {
            case 0:
            case 1:
                m_Grad = m_GradOld;
                m_CurrentRightTabType = (ProduceRightType)pageIndex;
                break;
            case 2:
            case 3:
                m_CurrentRightTabType = (ProduceRightType)pageIndex;
                m_Grad = pageIndex - 1;
                break;
            case 4:
                if (m_CurrentType == ProduceType.Chip)
                {
                    m_CurrentRightTabType = ProduceRightType.Part;//零件
                }
                else
                {
                    m_CurrentRightTabType = (ProduceRightType)pageIndex;
                    m_Grad = pageIndex - 1;
                }
                break;
            case 5:
                m_CurrentRightTabType = (ProduceRightType)pageIndex;
                break;
            default:
                break;
        }


    }

    /// <summary>
    /// 页签变化时
    /// </summary>
    /// <param name="oldIndex">老页签</param>
    /// <param name="newIndex">新页签</param>
    protected override void OnPageIndexChanged(int oldIndex, int newIndex)
    {
        FiltrateByType();
    }

    /// <summary>
    /// 过滤索引改变时
    /// </summary>
    /// <param name="oldIndex"></param>
    /// <param name="newIndex"></param>
    protected override void OnFilterIndexChanged(int oldIndex, int newIndex)
    {
        FiltrateByType();
    }

    #region 临时缓存
    /// <summary>
    /// 临时缓存本界面不包括零件的数据
    /// </summary>
    protected List<Produce> m_ListTempA = new List<Produce>();
    /// <summary>
    ///  临时缓存本界面包括零件的数据
    /// </summary>
    protected List<Produce> m_ListTempB = new List<Produce>();
    /// <summary>
    /// 临时缓存本界面生产中和完成的数据
    /// </summary>
    protected List<ProduceInfoVO> m_ListTemp = new List<ProduceInfoVO>();

    #endregion

    /// <summary>
    /// 填充面板数据
    /// </summary>
    public virtual void FiltrateByType()
    {
        SetPageLabel();
        m_ListTempA.Clear();
        m_ListTempB.Clear();
        m_ListTemp.Clear();
        m_GridList.Clear();
        ClearData();
        switch (m_CurrentRightTabType)
        {
            case ProduceRightType.Producing:
                AddProduceingData(false);
                break;
            case ProduceRightType.CanProduce:
                AddProduceingData(true);
                break;
            case ProduceRightType.T1:
            case ProduceRightType.T2:
            case ProduceRightType.T3:
                if (m_CurrentBlueprintType != BlueprintL1.EquipmentMod)
                    GetDataByTable();
                else
                    GetDataByTable(false);
                break;
            case ProduceRightType.Part:
                AddPartData();
                break;
            default:
                break;
        }

        ShowToggleProduce();
        m_ListTempA.Clear();
        m_ListTempB.Clear();
        m_ListTemp.Clear();
    }

    /// <summary>
	/// 设置本页面的所有蓝图生产状态(左侧all标签)
	/// </summary> 
    /// <param name="listB">列表数据</param>
	public void SetAllProduce(List<Produce> listB)
    {
        m_FoundryProxy.GetBluePrintDic().Clear();
        for (int i = 0; i < listB.Count; i++)
        {
            m_FoundryProxy.SetProduceState((int)listB[i].Id);
        }
    }

    /// <summary>
    /// 封装添加数据
    /// </summary>
    /// <param name="title">标题</param>
    /// <param name="list">列表数据</param>
    public void AddDataToView(string title, List<ProduceInfoVO> list)
    {
        AddDatas(TableUtil.GetLanguageString(title), list.ToArray());
    }

    /// <summary>
    /// 封装添加数据
    /// </summary>
    /// <param name="title">标题</param>
    /// <param name="list">列表数据</param>
    public void AddDatas(object title, List<ProduceInfoVO> list)
    {
        AddDatas(title, list.ToArray());
    }

    /// <summary>
    /// 添加生产中，生产完成或可生产 数据
    /// </summary>
    /// <param name="canProduce">是否可生产</param>
    protected void AddProduceingData(bool canProduce)
    {
        m_ListTempA = m_FoundryProxy.GetDataByMainType(m_CurrentBlueprintType);
        m_ListTempB = m_ListTempA.Concat(m_PartProduceList).ToList<Produce>();
        SetAllProduce(m_ListTempB);
        m_ListTemp = m_FoundryProxy.GetBluePrintDic().Values.ToList();
        m_ListTemp = SortCurrentData(m_ListTemp);
        if (!canProduce)
        {
            for (int i = 0; i < m_ListTemp.Count; i++)
            {
                if (m_ListTemp[i].BluePrintState == ProduceState.Producing || m_ListTemp[i].BluePrintState == ProduceState.Finsh)
                {
                    m_GridList.Add(m_ListTemp[i]);
                }
            }
            AddDataToView("production_title_1027", m_GridList);
        }
        else
        {
            for (int i = 0; i < m_ListTemp.Count; i++)
            {
                if (m_ListTemp[i].BluePrintState == ProduceState.CanProduce)
                {
                    m_GridList.Add(m_ListTemp[i]);
                }
            }
            AddDataToView("production_title_1028", m_GridList);
        }
        
    }

    /// <summary>
    /// 添加零件数据
    /// </summary>
    protected void AddPartData()
    {
        m_ListTempB.AddRange(m_PartProduceList);
        SetAllProduce(m_ListTempB);
        m_ListTemp = m_FoundryProxy.GetBluePrintDic().Values.ToList();
        m_ListTemp = SortCurrentData(m_ListTemp);
        m_GridList = m_ListTemp;
        AddDataToView("production_title_1056", m_GridList);
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

    /// <summary>
    /// 通过表格获取数据
    /// </summary>
    protected void GetDataByTable(bool includeT = true)
    {
        UIViewCategory category = State.GetPageCategoryData();
       
        if (category.ItemType != null)
        {
            if (category.IsAll)
            {
                UIViewPage page = State.GetPage();

                for (int i = 0; i < page.Categorys.Length; i++)
                {
                    m_FoundryProxy.GetBluePrintDic().Clear();
                    if (page.Categorys[i].IsAll)
                        continue;

                    category = page.Categorys[i];

                    for (int j = 0; j < category.ItemType.Length; j++)
                    {
                        ItemType itemType = category.ItemType[j];
                        if (itemType == null)
                            continue;

                        if (includeT)
                            m_FoundryProxy.FindItemArrayByItemType(itemType, true, m_Grad);
                        else
                            m_FoundryProxy.FindItemArrayByItemType(itemType, true, m_Grad, false);
                    }
                    m_ListTemp = m_FoundryProxy.GetBluePrintDic().Values.ToList();
                    m_GridList = m_ListTemp;
                    AddDatas(category.Label, m_GridList);
                }
            }
            else
            {
                m_FoundryProxy.GetBluePrintDic().Clear();
                for (int i = 0; i < category.ItemType.Length; i++)
                {
                    ItemType itemType = category.ItemType[i];
                    if (itemType == null)
                        continue;
                    if (includeT)
                        m_FoundryProxy.FindItemArrayByItemType(itemType, true, m_Grad);
                    else
                        m_FoundryProxy.FindItemArrayByItemType(itemType, true, m_Grad,false);
                }
                m_ListTemp = m_FoundryProxy.GetBluePrintDic().Values.ToList();
                m_GridList = m_ListTemp;
                AddDatas(category.Label, m_GridList);
            }
        }
    }

    #region 排序
    /// <summary>
    /// 按下热键排序
    /// </summary>
    /// <param name="list">list</param>
    public List<ProduceInfoVO> SortCurrentData(List<ProduceInfoVO> list)
    {
        switch (m_CurrentSortProduceType)
        {
            case SortProduceType.Quality:
                return SortQuality(list);
            case SortProduceType.T:
                return SortTGarder(list);
            case SortProduceType.LimitLevel:
                return SortLevel(list);
            case SortProduceType.Name:
                return SortName(list);
            case SortProduceType.Time:
                return SortTime(list);
            default:
                return SortQuality(list);
        }
    }

    /// <summary>
    /// 按品质排序
    /// </summary>
    /// <param name="list">list</param>
    public List<ProduceInfoVO> SortQuality(List<ProduceInfoVO> list)
    {
        return list = list.OrderBy(o => o.MItem.Quality).ThenBy(o => o.MProduce.Order).ThenBy(o => o.MProduce.Id).ToList();
    }

    /// <summary>
    /// 按T等级排序
    /// </summary>
    /// <param name="list">list</param>
    public List<ProduceInfoVO> SortTGarder(List<ProduceInfoVO> list)
    {
        return list = list.OrderBy(o => o.MItem.Grade).ThenBy(o => o.MProduce.Order).ThenBy(o => o.MProduce.Id).ToList();
    }

    /// <summary>
    /// 按使用登记排序
    /// </summary>
    /// <param name="list">list</param>
    public List<ProduceInfoVO> SortLevel(List<ProduceInfoVO> list)
    {
        return list = list.OrderBy(o => o.MProduce.PlayerLvLimit).ThenBy(o => o.MProduce.Order).ThenBy(o => o.MProduce.Id).ToList();
    }

    /// <summary>
    /// 按名字az排序
    /// </summary>
    /// <param name="list">list</param>
    public List<ProduceInfoVO> SortName(List<ProduceInfoVO> list)
    {
        return list = list.OrderBy(o => TableUtil.GetItemName(o.MProduce.Id)).ThenBy(o => o.MProduce.Order).ThenBy(o => o.MProduce.Id).ToList();
    }

    /// <summary>
    /// 按获取时间排序
    /// </summary>
    /// <param name="list">list</param>
    public List<ProduceInfoVO> SortTime(List<ProduceInfoVO> list)
    {
        return list = list.OrderBy(o => o.MProduce.Quality).ThenBy(o => o.MProduce.Order).ThenBy(o => o.MProduce.Id).ToList();
    }

    #endregion

    #region 按键

    /// <summary> 
    /// 空格键按下  生产 加速 领取
    /// </summary>
    /// <param name="callbackContext">参数</param>
    public void OnProduce(HotkeyCallback callbackContext)
    {
        if (callbackContext.started)
        {
            if(!m_HoldProduceDown)
            {
                m_HoldProduceDown = true;
                if (m_SelectProduceTid > 0)
                {
                    m_ProducingTid = m_SelectProduceTid;
                    if (m_FoundryProxy.GetBluePrintDic().TryGetValue(m_SelectProduceTid, out ProduceInfoVO girdInfo))
                    {
                        //if (girdInfo.BluePrintState == ProduceState.CanProduce)
                        //{
                        //    MsgProduceConfim msgProduceConfim = new MsgProduceConfim();
                        //    msgProduceConfim.OrderType = ProduceOrder.Produce;
                        //    msgProduceConfim.Tid = m_SelectProduceTid;
                        //    GameFacade.Instance.SendNotification(NotificationName.MSG_PRODUCE_ORDER, msgProduceConfim);
                        //}
                       
                        if (girdInfo.BluePrintState == ProduceState.Producing)
                        {
                            ProduceInfoVO foundryMember = m_FoundryProxy.GetFoundryMemberByTID(girdInfo.TID);
                            if (foundryMember != null)
                            {
                                MsgProduceConfim msgProduceConfim = new MsgProduceConfim();
                                msgProduceConfim.OrderType = ProduceOrder.SpeedUp;
                                msgProduceConfim.Tid = m_SelectProduceTid;
                                msgProduceConfim.ExpendNum = GetNeedGlod(m_SelectProduceTid);
                                GameFacade.Instance.SendNotification(NotificationName.MSG_PRODUCE_ORDER, msgProduceConfim);
                            }
                            else
                            {
                                Debug.Log("fuwuqi不包含");
                            }
                        }
                    }
                    else
                    {
                        Debug.Log("不包含");
                    }
                }
                WwiseUtil.PlaySound((int)WwiseMusic.Music_Production_began, false, null);
            }
            if (m_ProducingTid == m_SelectProduceTid && !m_Discard)
            {
                GameFacade.Instance.SendNotification(NotificationName.MSG_PRODUCE_ORDE_SHOW);
                float progress = (float)((callbackContext.time - callbackContext.startTime) / callbackContext.duration);
                if (m_ProduceView.GetDownProgressImage() != null)
                    m_ProduceView.GetDownProgressImage().fillAmount = progress;

                float repc = progress * m_SoundRtpcOffet;
                WwiseManager.SetParameter(WwiseRtpc.Rtpc_UI_Hotkey, repc);
            }
            else
            {
                GameFacade.Instance.SendNotification(NotificationName.MSG_PRODUCE_ORDE_RRETRIEVE);
                m_Discard = true;
                WwiseUtil.PlaySound((int)WwiseMusic.Music_Production_ToCancel, false, null);
            }

        }
        if (callbackContext.performed && m_ProducingTid == m_SelectProduceTid && !m_Discard)
        {
            GameFacade.Instance.SendNotification(NotificationName.MSG_PRODUCE_ORDE_RRETRIEVE);
            ProduceCallBack();
            m_HoldProduceDown = false;
            WwiseUtil.PlaySound((int)WwiseMusic.Music_Production_end, false, null);
        }
        if (callbackContext.cancelled)
        {
            GameFacade.Instance.SendNotification(NotificationName.MSG_PRODUCE_ORDE_RRETRIEVE);
            m_Discard = false;
            m_HoldProduceDown = false;
            WwiseUtil.PlaySound((int)WwiseMusic.Music_Production_ToCancel, false, null);
        }
    }

    /// <summary>
    /// 空格键按下 领取
    /// </summary>
    public void OnRevecie(HotkeyCallback callbackContext)
    {
        if (callbackContext.started)
        {
            if (m_SelectProduceTid > 0)
            {
                m_ProducingTid = m_SelectProduceTid;
                if (m_FoundryProxy.GetBluePrintDic().TryGetValue(m_SelectProduceTid, out ProduceInfoVO girdInfo))
                {
                    if (girdInfo.BluePrintState == ProduceState.Finsh)
                    {
                        ProduceInfoVO foundryMember = m_FoundryProxy.GetFoundryMemberByTID(girdInfo.TID);
                        if (foundryMember != null)
                        {
                            Receive(foundryMember.TID);
                        }
                    }
                    if (girdInfo.BluePrintState == ProduceState.CanProduce)
                    {
                        Make(girdInfo.TID);
                    }
                }
            }
        }
      
    }
    /// <summary>
    /// v 键按下 取消
    /// </summary>
    /// <param name="callbackContext"></param>
    public void OnCanelProduce(HotkeyCallback callbackContext)
    {
        if (callbackContext.started)
         {
            if(!m_HoldCancelDown)
            {
                m_ProducingTid = m_SelectProduceTid;
                m_HoldCancelDown = true;
                //if (Mathf.Approximately(m_ProduceView.GetDownProgressImage().fillAmount, 0.0f))
                {
                    MsgProduceConfim msgProduceConfim = new MsgProduceConfim();
                    msgProduceConfim.OrderType = ProduceOrder.Canel;
                    msgProduceConfim.Tid = m_SelectProduceTid;
                    GameFacade.Instance.SendNotification(NotificationName.MSG_PRODUCE_ORDER, msgProduceConfim);
                }
            }
            if (m_ProducingTid == m_SelectProduceTid && !m_Discard)
            {
                GameFacade.Instance.SendNotification(NotificationName.MSG_PRODUCE_ORDE_SHOW);
                float progress = (float)((callbackContext.time - callbackContext.startTime) / callbackContext.duration);
                if (m_ProduceView.GetDownProgressImage() != null)
                     m_ProduceView.GetDownProgressImage().fillAmount = progress;
            }
            else
            {
                GameFacade.Instance.SendNotification(NotificationName.MSG_PRODUCE_ORDE_RRETRIEVE);
                m_Discard = true;
                WwiseUtil.PlaySound((int)WwiseMusic.Music_Production_ToCancel, false, null);
            }
        }
        if (callbackContext.performed && m_ProducingTid == m_SelectProduceTid && !m_Discard)
        {
            WwiseUtil.PlaySound((int)WwiseMusic.Music_Resolve_Over, false, null);
            CanelProduceCallBack();
            GameFacade.Instance.SendNotification(NotificationName.MSG_PRODUCE_ORDE_RRETRIEVE);
            WwiseUtil.PlaySound((int)WwiseMusic.Music_Production_end, false, null);
            m_HoldCancelDown = false;
        }
        if (callbackContext.cancelled)
        {
            GameFacade.Instance.SendNotification(NotificationName.MSG_PRODUCE_ORDE_RRETRIEVE);
            m_Discard = false;
            m_HoldCancelDown = false;
            WwiseUtil.PlaySound((int)WwiseMusic.Music_Production_ToCancel, false, null);
        }
    }

    #endregion

    #region 生产具体操作
    /// <summary>
    /// 生产按键回调
    /// </summary>
    private void ProduceCallBack()
    {
        if (m_SelectProduceTid > 0)
        {
            ProduceInfoVO girdInfo = null;

            if (m_FoundryProxy.GetBluePrintDic().TryGetValue(m_SelectProduceTid, out girdInfo))
            {
                if (girdInfo.BluePrintState == ProduceState.CanProduce)// 蓝图生产状态    0,不可生产，1 生产中，2可生产 3 生产完成
                {
                   // Make(girdInfo.TID);
                }
                if (girdInfo.BluePrintState == ProduceState.Finsh)
                {
                    ProduceInfoVO foundryMember = m_FoundryProxy.GetFoundryMemberByTID(girdInfo.TID);
                    if (foundryMember != null)
                    {
                        //Receive(foundryMember.TID);
                    }
                }
                if (girdInfo.BluePrintState == ProduceState.Producing)
                {
                    ProduceInfoVO foundryMember = m_FoundryProxy.GetFoundryMemberByTID(girdInfo.TID);
                    if (foundryMember != null)
                    {
                        if (BeSpeedUpByMoney(foundryMember.TID))
                        {
                            ImmediatelyFinish(foundryMember.TID);
                        }
                        else
                        {
                            Debug.Log("加速货币不够");
                        }
                    }

                }
            }
            else
            {
                Debug.LogWarning("操作违法" + m_SelectProduceTid);
            }
        }
    }

    /// <summary>
    /// 取消生产按键回调
    /// </summary>
    private void CanelProduceCallBack()
    {
        ProduceInfoVO girdInfo = null;
        if (m_FoundryProxy.GetBluePrintDic().TryGetValue(m_SelectProduceTid, out girdInfo))
        {
            if (girdInfo.BluePrintState == ProduceState.Producing)
            {
                ProduceInfoVO foundryMember = m_FoundryProxy.GetFoundryMemberByTID(girdInfo.TID);
                if (foundryMember != null)
                {
                    Cancel(foundryMember.TID);
                }
            }
        }
    }
    #endregion

    /// <summary>
    /// 通过金币判断是否可以加速
    /// </summary>
    /// <param name="id">蓝图ID</param>
    /// <returns></returns>
    public bool BeSpeedUpByMoney(int id)
    {
        long needCout = GetNeedGlod(id);
        long gold = CurrencyUtil.GetRechargeCurrencyCount();
        if (gold < needCout)
        {
            return false;
        }
        return true;
    }

    /// <summary>
    /// 获取加速需要多少钱
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public long GetNeedGlod(int id)
    {
        long needCout = 0;
        ProduceInfoVO foundryMember = m_FoundryProxy.GetFoundryMemberByTID(id);
        float factorValue = m_CfgEternityProxy.GetGamingConfig(1).Value.Produce.Value.FinishExpenseRate;
        float maxTime = foundryMember.EndTime - foundryMember.StartTime;
        float needtime = foundryMember.EndTime - foundryMember.StartTime - foundryMember.SpendTime;
        if ((needtime - HOLD_TIME) > SECOND)
        {
            needCout = (needtime - HOLD_TIME) % SECOND == 0 ? (long)((needtime - HOLD_TIME) / SECOND) : (long)((needtime - HOLD_TIME) / SECOND + 1);
            needCout = (long)(needCout * m_ExpendNumber);
        }
        else
        {
            needCout = (long)m_ExpendNumber;
        }
        return needCout;
    }

    /// <summary>
	/// 热键显示更新
	/// </summary>
	/// <param name="state">状态</param>
	public void RefreshHotKey(ProduceState state)
    {
        // 根据策划需求刷新
        //蓝图生产状态    0,不可生产，1 生产中，2可生产 3 生产完成
        State.GetAction(UIAction.Product_ProduceAndCollect).Visible = true;
        State.GetAction(UIAction.Product_Accelerate).Visible = false;

        switch (state)
        {
            case ProduceState.CanNotProduce:
                SetHotKeyEnabled(UIAction.Product_Cancel, false);
                SetHotKeyEnabled(UIAction.Product_ProduceAndCollect, false,0,false);
               
                break;
            case ProduceState.Producing:
                SetHotKeyEnabled(UIAction.Product_Cancel, true);
                SetHotKeyEnabled(UIAction.Product_Accelerate, true, 0);
                State.GetAction(UIAction.Product_ProduceAndCollect).Visible = false;
                State.GetAction(UIAction.Product_Accelerate).Visible = true;
                break;
            case ProduceState.CanProduce:
                SetHotKeyEnabled(UIAction.Product_Cancel, false);
                SetHotKeyEnabled(UIAction.Product_ProduceAndCollect, true, 0,false);
                break;
            case ProduceState.Finsh:
                SetHotKeyEnabled(UIAction.Product_Cancel, false);
                SetHotKeyEnabled(UIAction.Product_ProduceAndCollect, true, 1, false);
                break;
            case ProduceState.Have:
                SetHotKeyEnabled(UIAction.Product_Accelerate, false, 0);
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 设置热键可见性
    /// </summary>
    /// <param name="hotKeyID">hotKeyID</param>
    /// <param name="enable">可见性</param>
    /// <param name="style">风格样式</param>
    public void SetHotKeyEnabled(string hotKeyID, bool enable, int style = 0, bool hold = true)
    {
        State.GetAction(hotKeyID).Enabled = enable;
        State.GetAction(hotKeyID).State = style;
    }

    #region 发送消息
    /// <summary>
    /// 生产
    /// </summary>
    /// <param name="TID">实例ID</param>
    private void Make(int TID)
    {
        Debug.Log("制作   = " + TID);
        NetworkManager.Instance.GetFoundryController().SendBuild(TID);
    }
    /// <summary>
    /// 立刻完成
    /// </summary>
    /// <param name="TID">实例ID</param>
    private void ImmediatelyFinish(int TID)   //生产线ID，非 背包Uid
    {
        Debug.Log("立刻完成   = " + TID);
        NetworkManager.Instance.GetFoundryController().SendSpeed(TID);
    }
    /// <summary>
    /// //取消生产
    /// </summary>
    /// <param name="TID">实例ID</param>
    private void Cancel(int TID)//生产线ID，非 背包Uid
    {
        Debug.Log("取消   = " + TID);
        NetworkManager.Instance.GetFoundryController().SendCancel(TID);
    }
    /// <summary>
    /// 领取生产
    /// </summary>
    /// <param name="TID">实例ID</param>
    private void Receive(int TID)
    {
        Debug.Log("领取   = " + TID);
        NetworkManager.Instance.GetFoundryController().SendReceive(TID);
    }
    #endregion

    #region 接收消息回调

    /// <summary>
    /// 根据服务器回调更新数据
    /// </summary>
    /// <param name="objs">消息体参数</param>
    protected void UpdateDataByServer(object obj)
    {
        MsgProduct msg = obj as MsgProduct;
        ShowToggleProduce();
        switch (msg.msg)
        {
            case NotificationName.MSG_FOUNDRY_GET_INFO:

                break;
            case NotificationName.MSG_FOUNDRY_BUILD:
                OnBuildBack(msg.itemTID);
                FiltrateByType();
                if (m_GridList.Count <= 0)
                    State.SetPageIndex(0);
                break;
            case NotificationName.MSG_FOUNDRY_SPEED:
                OnSpeedBack(msg.itemTID);
                FiltrateByType();
                State.SetPageIndex(0);
                break;
            case NotificationName.MSG_FOUNDRY_CANCEL:
                OnCancelBack(msg.itemTID);
                FiltrateByType();
                if (m_GridList.Count <= 0)
                {
                    if (m_FaultageReady)
                        State.SetPageIndex(1);
                    else
                        State.SetPageIndex(2);
                }
                
                break;
            case NotificationName.MSG_FOUNDRY_RECEIVE:
                OnReceiveBack(msg.itemTID);
                FiltrateByType();
                if (m_GridList.Count <= 0)
                {
                    if (m_FaultageReady)
                        State.SetPageIndex(1);
                    else
                        State.SetPageIndex(2);
                }
                   
                break;
        }

    }
    /// <summary>
    /// 延迟调用
    /// </summary>
    /// <param name="seconds">秒数</param>
    /// <param name="callBack">回调函数</param>
    /// <returns></returns>
    public static IEnumerator Excute(float seconds, Action callBack)
    {
        yield return new WaitForSeconds(seconds);
        callBack();
    }

    /// <summary>
    /// //制作返回
    /// </summary>
    /// <param name="TID">实例ID</param>
    private void OnBuildBack(int TID)
    {
        Debug.Log("制作返回   = " + TID);
        ProduceInfoVO foundryMember = m_FoundryProxy.GetFoundryById(TID);
        if (foundryMember == null)
        {
            return;
        }
        bool finshed = foundryMember.StartTime + foundryMember.SpendTime >= foundryMember.EndTime;
        ProduceState state = finshed ? ProduceState.Finsh : ProduceState.Producing;
        float progress = 0;
        if (foundryMember.EndTime > foundryMember.StartTime && foundryMember.SpendTime >= 0)
        {
            progress = (float)foundryMember.SpendTime / (foundryMember.EndTime - foundryMember.StartTime);
        }
        m_FoundryProxy.SetGirdDataInfo(foundryMember.TID, state, progress);      //蓝图生产状态    0,不可生产，1 生产中，2可生产 3 生产完成
        if (m_FoundryProxy.GetBluePrintDic().TryGetValue(foundryMember.TID, out ProduceInfoVO girdInfo))
        {
            if (girdInfo.Elelment)
            {
                girdInfo.Elelment.RefreshData(girdInfo.Progress, girdInfo.BluePrintState);
            }
            RefreshHotKey(girdInfo.BluePrintState);
            //UpdateTips(TID);
        }
    }
    /// <summary>
    /// //取消返回
    /// </summary>
    /// <param name="TID">实例ID</param>
    private void OnCancelBack(int TID)
    {
        Debug.Log("取消返回   = " + TID);
        m_FoundryProxy.BeCanProduce(TID);
        ProduceInfoVO girdInfo = null;
        if (m_FoundryProxy.GetBluePrintDic().TryGetValue(TID, out girdInfo))
        {
            if (girdInfo.Elelment)
            {
                girdInfo.Elelment.RefreshData(girdInfo.Progress, girdInfo.BluePrintState);
            }
            RefreshHotKey(girdInfo.BluePrintState);
            //UpdateTips(TID);
        }
    }
    /// <summary>
    /// //立即完成
    /// </summary>
    /// <param name="TID">TID</param>
    private void OnSpeedBack(int TID)
    {
        Debug.Log("立即完成 返回" + TID);

        ProduceInfoVO foundryMember = m_FoundryProxy.GetFoundryById(TID);
        if (foundryMember == null)
        {
            return;
        }
        float progress = 0;
        if (foundryMember.EndTime > foundryMember.StartTime && foundryMember.SpendTime >= 0)
        {
            progress = (float)foundryMember.SpendTime / (foundryMember.EndTime - foundryMember.StartTime);
        }
        m_FoundryProxy.SetGirdDataInfo(foundryMember.TID, ProduceState.Finsh, progress); //蓝图生产状态  0,不可生产，1 生产中，2可生产 3 生产完成
        ProduceInfoVO girdInfo = null;
        if (m_FoundryProxy.GetBluePrintDic().TryGetValue(foundryMember.TID, out girdInfo))
        {
            if (girdInfo.Elelment)
            {
                girdInfo.Elelment.RefreshData(girdInfo.Progress, girdInfo.BluePrintState);
            }
            RefreshHotKey(girdInfo.BluePrintState);
            //UpdateTips(TID);
        }
    }

    /// <summary>
    /// 接收产品返回
    /// </summary>
    /// <param name="TID"></param>
    private void OnReceiveBack(int TID)
    {
        Debug.Log("领取返回   = " + TID);
        m_FoundryProxy.BeCanProduce(TID);
        ProduceInfoVO girdInfo = null;
        if (m_FoundryProxy.GetBluePrintDic().TryGetValue(TID, out girdInfo))
        {
            if (girdInfo.Elelment)
            {
                girdInfo.Elelment.RefreshData(girdInfo.Progress, girdInfo.BluePrintState);
            }
            RefreshHotKey(girdInfo.BluePrintState);
            //UpdateTips(TID);
        }

    }
    #endregion

    /// <summary>
	/// 当前蓝图类型
	/// </summary>
	protected BlueprintL1 m_CurrentBlueprintType;
    /// <summary>
	/// 是否断层 （有没有材料够按钮）
	/// </summary>
	protected bool m_FaultageReady;

    /// <summary>
    /// 是否断层 （有没有生产中按钮）
    /// </summary>
    protected bool m_FaultageProduce;
    /// <summary>
	/// 当前最小个数
	/// </summary>
	protected int m_CurrentMinCount;

    #region 是否显示可生产按钮

    /// <summary>
    /// 是否显示可生产按钮
    /// </summary>
    public void ShowToggleProduce()
    {
        List<Produce> listA = m_FoundryProxy.GetDataByMainType(m_CurrentBlueprintType);
        List<Produce> listB = listA.Concat(m_PartProduceList).ToList<Produce>();
        SetAllProduce(listB);
        List<ProduceInfoVO> list = m_FoundryProxy.GetBluePrintDic().Values.ToList();
        m_FaultageReady = false;
        m_FaultageProduce = false;
        m_CurrentMinCount = 2;
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].BluePrintState == ProduceState.CanProduce)
            {
              
                State.SetPageVisible(1,true);
                m_FaultageReady = true;
            }
            if (list[i].BluePrintState == ProduceState.Producing || list[i].BluePrintState == ProduceState.Finsh)
            {
                State.SetPageVisible(0, true);

                m_FaultageProduce = true;
            }
        }
        if (m_FaultageReady && !m_FaultageProduce)
        {
            m_CurrentMinCount = 1;
            State.SetPageVisible(0, false);
            State.SetPageVisible(1, true);

        }
        else if (m_FaultageProduce && m_FaultageReady)
        {
            m_CurrentMinCount = 0;
            State.SetPageVisible(0, true);
            State.SetPageVisible(1, true);
        }
        else if (!m_FaultageReady && m_FaultageProduce)
        {
            m_CurrentMinCount = 0;
            State.SetPageVisible(0, true);
            State.SetPageVisible(1, false);

        }
        else if (!m_FaultageReady && !m_FaultageProduce)
        {
            State.SetPageVisible(0, false);
            State.SetPageVisible(1, false);

        }
    }

    #endregion
}
#region 生产状态枚举
public enum ProduceState
{
    /// <summary>
    /// 不可生产
    /// </summary>
    CanNotProduce = 0,
    /// <summary>
    /// 生产中
    /// </summary>
    Producing,
    /// <summary>
    /// 可生产
    /// </summary>
    CanProduce,
    /// <summary>
    /// 生产完成
    /// </summary>
    Finsh,
    /// <summary>
    /// 已拥有
    /// </summary>
    Have,
    /// <summary>
    /// none
    /// </summary>
    None,
}
#endregion

#region 生产标签 
/// <summary>
/// 一级标签
/// </summary>
public enum ProduceRightType
{
    /// <summary>
    /// 完成
    /// </summary>
    Finsh = -1,
    /// <summary>
    /// 生产中
    /// </summary>
    Producing = 0,
    /// 材料充足
    /// </summary>
    CanProduce,
    /// <summary>
    /// T1标签   ===ShipChip
    /// </summary>
    T1,
    /// <summary>
    /// T2标签  ===WeaponChip
    /// </summary>
    T2,
    /// <summary>
    /// T3标签
    /// </summary>
    T3,
    /// <summary>
    /// 零件
    /// </summary>
    Part,
}
/// <summary>
/// 生产入口标签武器装备
/// </summary>
public enum ProduceDialogType
{
    /// <summary>
    /// T1标签
    /// </summary>
    T1 = 1,
    /// <summary>
    /// T2标签
    /// </summary>
    T2,
    /// <summary>
    /// T3标签
    /// </summary>
    T3,
    /// <summary>
    /// 零件
    /// </summary>
    Part,
    /// <summary>
    /// 战舰芯片
    /// </summary>
    ShipChip,
    /// <summary>
    /// 武器芯片
    /// </summary>
    WeaponChip,
}

/// <summary>
/// 排序规则
/// </summary>
public enum SortProduceType
{
    /// <summary>
    /// 品质
    /// </summary>
    Quality = 0,
    /// <summary>
    /// T 等级
    /// </summary>
    T,
    /// <summary>
    /// 使用等级
    /// </summary>
    LimitLevel,
    /// <summary>
    /// 名字
    /// </summary>
    Name,
    /// <summary>
    /// 获得时间
    /// </summary>
    Time,
    /// <summary>
    /// 总数
    /// </summary>
    Total

}
#endregion