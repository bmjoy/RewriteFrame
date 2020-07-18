using Eternity.FlatBuffer;
using Leyoutech.Core.Loader.Config;
using PureMVC.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UITalentListPart : BaseViewPart
{
    /// <summary>
    /// list预设地址
    /// </summary>
    private const string ASSET_ADDRESS = AssetAddressKey.PRELOADUI_COMMONLISTPART_TALENT;
    /// <summary>
    /// 节点预设
    /// </summary>
    private const string ASSET_ADDRESS_TALENTELEMENT = AssetAddressKey.PRELOADUIELEMENT_TALENTELEMENT;
    /// <summary>
    /// 根节点预设
    /// </summary>
    private const string ASSET_ADDRESS_TALENTTYPEELEMENT = AssetAddressKey.PRELOADUIELEMENT_TALENTTYPEELEMENT;
    /// <summary>
    /// 划线预设
    /// </summary>
    private const string ASSET_ADDRESS_TALENTLINEELMENT = AssetAddressKey.PRELOADUIELEMENT_TALENTLINEELMENT;

    /// <summary>
    /// 数据
    /// </summary>
    private CfgEternityProxy m_CfgEternityProxy;
    /// <summary>
    /// 天赋数据
    /// </summary>
    private TalentProxy m_TalentProxy;
    /// <summary>
    /// 当前天赋树ID
    /// </summary>
    private uint m_CurrentTalentRootId;
    /// <summary>
    /// 当前天赋ID
    /// </summary>
    private int m_CurrentTalentId;
    /// <summary>
    /// 记录上次点击的天赋ID
    /// </summary>
    private int m_OldTalentId;
    /// <summary>
    /// 天赋节点
    /// </summary>
    private Transform m_TalentRoot;
    /// <summary>
    /// 天赋树节点
    /// </summary>
    private Transform m_TalentTypeRoot;
    /// <summary>
    /// 天赋线节点
    /// </summary>
    private Transform m_TalentLineRoot;
    /// <summary>
    /// 天赋Element
    /// </summary>
    private GameObject m_TalentElement;
    /// <summary>
    /// 天赋树Element
    /// </summary>
    private GameObject m_TalentTypeElement;
    /// <summary>
    /// 天赋线Element
    /// </summary>
    private GameObject m_TalentLineElement;
    /// <summary>
    /// 天赋树物体
    /// </summary>
    private Transform m_TalentTypeTrans;
    /// <summary>
    /// 天赋的toggle组
    /// </summary>
    private ToggleGroup m_ToggleGroup;
    /// <summary>
    /// 当前天赋树数据
    /// </summary>
    private TalentTypeVO m_CurrentTalentTypeVO;
    /// <summary>
    /// 点前天赋数据
    /// </summary>
    private TalentVO m_CurrentTalentVO;
    /// <summary>
    /// 缩放最小值
    /// </summary>
    private float m_ZoomMin = 0;
    /// <summary>
    /// 缩放最大值
    /// </summary>
    private float m_ZoomMax = 2000f;
    /// <summary>
    /// 缩放变化值
    /// </summary>
    private float m_ZoomValue;
    /// <summary>
    /// 偏移量
    /// </summary>
    private float m_OffSet = 50;
    /// <summary>
    /// 顺滑时间
    /// </summary>
    private float m_Time = 5f;
    /// <summary>
    /// 顺滑时间
    /// </summary>
    private float m_MaxTime = 5f;
    /// <summary>
    ///是否开启 顺滑
    /// </summary>
    private bool m_Down;
    /// <summary>
    /// 协程
    /// </summary>
    private Coroutine m_Coroutine;
    /// <summary>
    /// 记录上次的坐标值
    /// </summary>
    private Vector3 m_OldPosition;
    /// <summary>
    /// 内容物体
    /// </summary>
    private Transform m_ConcetTransform;
    /// <summary>
    /// 滑动组件
    /// </summary>
    private ScrollRect m_ScrollRect;
    /// <summary>
    /// 天赋树天赋点的长度
    /// </summary>
    private int m_TalentLength;
    public override void OnShow(object msg)
    {
        base.OnShow(msg);
        LoadViewPart(ASSET_ADDRESS, OwnerView.ListBox);
        m_CfgEternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
        m_TalentProxy = GameFacade.Instance.RetrieveProxy(ProxyName.TalentProxy) as TalentProxy;
        UIManager.Instance.GetUIElement(ASSET_ADDRESS_TALENTELEMENT, (GameObject prefab) =>
        {
            m_TalentElement = prefab;
        });
        UIManager.Instance.GetUIElement(ASSET_ADDRESS_TALENTTYPEELEMENT, (GameObject prefab) =>
        {
            m_TalentTypeElement = prefab;
        });
        UIManager.Instance.GetUIElement(ASSET_ADDRESS_TALENTLINEELMENT, (GameObject prefab) =>
        {
            m_TalentLineElement = prefab;
        });
    }
    public override void OnHide()
    {


        base.OnHide();
    }
    protected override void OnViewPartLoaded()
    {
        m_OldPosition = new Vector3(578, -358, 0);
        m_TalentRoot = FindComponent<Transform>("Content/Scroller/Viewport/Content/ContentTalent");
        m_TalentTypeRoot = FindComponent<Transform>("Content/Scroller/Viewport/Content/ContentTypeTalent");
        m_TalentLineRoot = FindComponent<Transform>("Content/Scroller/Viewport/Content/ContentTalentLine");
        m_ToggleGroup = FindComponent<ToggleGroup>("Content/Scroller/Viewport/Content");
        m_ConcetTransform = FindComponent<Transform>("Content/Scroller/Viewport/Content");
        m_ScrollRect = FindComponent<ScrollRect>("Content/Scroller");
        m_TalentLineRoot.localPosition = new Vector3(0,0,-20);
        m_CurrentTalentVO = new TalentVO();
        UIManager.Instance.StartCoroutine(Excute(Time.deltaTime,()=> {
            m_ConcetTransform.transform.localPosition = m_OldPosition;
        }));
        State.SetActionCompareEnabled(false);
        State.OnPageIndexChanged -= OnPageChanged;
        State.OnPageIndexChanged += OnPageChanged;

        State.GetAction(UIAction.Talent_Active_Upgrade).Callback += OnActivate;
        State.GetAction(UIAction.Talent_BlockUp).Callback += OnStopUse;
        State.GetAction(UIAction.Talent_Reset).Callback += OnReset;
        State.GetAction(UIAction.Talent_ZoomIn).Callback += OnShrink;
        State.GetAction(UIAction.Talent_ZoomOut).Callback += OnMagnify;
        OnPageIndexChanged(State.GetPageIndex(), State.GetPageIndex());
    }

    protected override void OnViewPartUnload()
    {
        for (int i = m_TalentRoot.childCount -1; i >=0 ; i--)
        {
            ItemHandler handler = m_TalentRoot.GetChild(i).gameObject.GetComponent<ItemHandler>();
            if (handler)
            {
                handler.OnPointerOverHandler = null;
                handler.OnPointerPressHandler = null;
                handler.OnSelectHandler = null;
                handler.OnSubmitHandler = null;
            }

            //m_TalentRoot[i].isOn = false;
            m_TalentRoot.GetChild(i).GetComponent<Toggle>().onValueChanged.RemoveAllListeners();
            m_TalentRoot.GetChild(i).Recycle();
        }
        for (int i = m_TalentTypeRoot.childCount - 1; i >= 0; i--)
        {
            m_TalentTypeRoot.GetChild(i).GetComponent<Toggle>().onValueChanged.RemoveAllListeners();
            m_TalentTypeRoot.GetChild(i).Recycle();
        }
        for (int i = m_TalentLineRoot.childCount - 1; i >= 0; i--)
        {
            m_TalentLineRoot.GetChild(i).Recycle();
        }

        State.OnPageIndexChanged -= OnPageChanged;

        State.GetAction(UIAction.Talent_Active_Upgrade).Callback -= OnActivate;
        State.GetAction(UIAction.Talent_BlockUp).Callback -= OnStopUse;
        State.GetAction(UIAction.Talent_Reset).Callback -= OnReset;
        State.GetAction(UIAction.Talent_ZoomIn).Callback -= OnShrink;
        State.GetAction(UIAction.Talent_ZoomOut).Callback -= OnMagnify;
        m_TalentProxy.GetTalentVODic().Clear();
        m_ConcetTransform.transform.localPosition = m_OldPosition;
       
    }

    public override NotificationName[] ListNotificationInterests()
    {
        return new NotificationName[]
        {
            NotificationName.MSG_TALENT_CHANGEINFOS,
            NotificationName.MSG_TALENT_OPERATION,
        };
    }

    public override void HandleNotification(INotification notification)
    {
        switch ((NotificationName)notification.Name)
        {
            case NotificationName.MSG_TALENT_CHANGEINFOS://天赋信息改变
                FillPanel();
                RefreshHotKey();
                break;
            case NotificationName.MSG_TALENT_OPERATION://天赋操作
                FillPanel();
                OperationBack((MsgTalentOperation)notification.Body);
                RefreshHotKey();
                State.SetTipData(m_CurrentTalentVO);
                break;
            default:
                break;
        }
    }


    /// <summary>
    /// 获取天赋树
    /// </summary>
    /// <returns>总数</returns>
    private uint GetTalentIdByLabel(int i)
    {
        UiLabelConfig? pageCfg = m_CfgEternityProxy.GetUIPage((uint)(Config.HasValue ? Config.Value.LabelId(i) : 0));
        return pageCfg.Value.Id;
    }
    /// <summary>
    /// 当前页面改变时
    /// </summary>
    /// <param name="oldIndex">变化前的索引号</param>
    /// <param name="newIndex">变化后的索引号</param>
    private void OnPageChanged(int oldIndex, int newIndex)
    {
        OnPageIndexChanged(oldIndex, newIndex);
    }

    /// <summary>
    /// 分页索引改变时
    /// </summary>
    /// <param name="oldIndex">变化前的索引号</param>
    /// <param name="newIndex">变化后的索引号</param>
    protected virtual void OnPageIndexChanged(int oldIndex, int newIndex)
    {
        LoadTalentRoot();
        m_ConcetTransform.transform.localPosition = m_OldPosition;
        m_TalentProxy.GetTalentVODic().Clear();
        FillPanel();
        int pageIndex = State.GetPageIndex();
        OnFirstToggle();
        m_TalentProxy.GetTalentInfos(m_CurrentTalentRootId);

    }
    
    /// <summary>
    /// 填充面板
    /// </summary>
    public void FillPanel()
    {
        m_CurrentTalentTypeVO = new TalentTypeVO();
        int pageIndex = State.GetPageIndex();
        m_CurrentTalentRootId = GetTalentIdByLabel(pageIndex);
        m_CurrentTalentTypeVO.Id = (int)m_CurrentTalentRootId;
        m_TalentLength = m_CfgEternityProxy.GetTalentNodeLength(m_CurrentTalentRootId);
        Debug.Log("shuxinjiemian");
        Talent? talent  = m_CfgEternityProxy.GetTalentByKey(m_CurrentTalentRootId);
        m_TalentProxy.AddData(m_CurrentTalentRootId);

        if (talent.HasValue)
        {
            m_CurrentTalentTypeVO.MTalent = talent;
            m_CurrentTalentTypeVO.IconId = talent.Value.Icon;
            m_CurrentTalentTypeVO.UnLockId = talent.Value.UnlockCondition;
            m_CurrentTalentTypeVO.Type =(int)talent.Value.Type;
          
            if (!m_TalentProxy.GetTalentRootVODic().ContainsKey(m_CurrentTalentTypeVO.Id))
            {
                m_TalentProxy.GetTalentRootVODic().Add(m_CurrentTalentTypeVO.Id,m_CurrentTalentTypeVO);
            }
        }
        OnRendererTalentRoot();

        while (m_TalentRoot.childCount < m_TalentLength)
        {
            GameObject obj = ObjectPool.Spawn(m_TalentElement, m_TalentRoot);
            GameObject objLine = ObjectPool.Spawn(m_TalentLineElement, m_TalentLineRoot);
            Toggle toggle = obj.GetComponent<Toggle>();
            ItemHandler handler = toggle.gameObject.GetComponent<ItemHandler>() ?? toggle.gameObject.AddComponent<ItemHandler>();
            handler.OnPointerOverHandler = (pressed, go) => { OnPointerOverHandler(pressed, toggle, pageIndex); };
            handler.OnPointerPressHandler = (pressed, go) => { OnPointerPressHandler(pressed, toggle, pageIndex); };
            handler.OnSelectHandler = (selected, go) => { OnSelectionHandler(selected, toggle, pageIndex); };

            toggle.onValueChanged.RemoveAllListeners();
            toggle.onValueChanged.AddListener((bool select)=>{ OnCellSelect(select, obj); });
            toggle.group = m_ToggleGroup;
           
        }
        for (int i = m_TalentLength; i < m_TalentRoot.childCount; i++)
        {
            if (m_TalentRoot.GetChild(i).gameObject.activeSelf)
            {
                m_TalentRoot.GetChild(i).gameObject.SetActive(false);
                m_TalentLineRoot.GetChild(i).gameObject.SetActive(false);
                m_TalentLineRoot.GetChild(i).localPosition = Vector3.zero;
                m_TalentLineRoot.GetChild(i).localScale = Vector3.one;
            }

        }
        UpdateData();
    }

    /// <summary>
    /// 操作返回
    /// </summary>
    public void OperationBack(MsgTalentOperation msg)
    {
        if (msg.M_TalentCode == TalentCode.UpLevel)
        {
            Debug.Log("dddddddd"+msg.Tid);
            if (m_TalentProxy.GetTalentVODic().TryGetValue(msg.Tid,out TalentVO talentVO))
            {
                talentVO.MTalentElement.PlayUpLevel();
            } 
        }
    }
    /// <summary>
    /// 刷新数据
    /// </summary>
    public void UpdateData()
    {
        for (int i = 0; i < m_TalentLength; i++)
        {
            RectTransform rectTra = m_TalentRoot.GetChild(i).GetComponent<RectTransform>();
            if (!rectTra.gameObject.activeSelf)
            {
                rectTra.gameObject.SetActive(true);
                m_TalentLineRoot.GetChild(i).gameObject.SetActive(true);

            }
            TalentSubNode? talentSubNode= m_CfgEternityProxy.GetTalentSubNodeByIndex(m_CurrentTalentRootId,i);
            TalentVO talentVO = new TalentVO();
            if (talentSubNode.HasValue)
            {
                if (m_TalentProxy.GetTalentVODic().TryGetValue(talentSubNode.Value.Id, out talentVO))
                {
                    talentVO.MTalentSubNode = talentSubNode;
                    talentVO.Id =(int) talentSubNode.Value.Id;
                }
            }
            LineRenderer lineRenderer = m_TalentLineRoot.GetChild(i).GetComponent<LineRenderer>();
            LineImage lineImage = m_TalentLineRoot.GetChild(i).GetChild(0).GetComponent<LineImage>();
            OnRendererCell(rectTra.gameObject, lineRenderer, talentVO, lineImage);
            // DrawLinrRenderer(lineRenderer,PosLineList);
        }

    }

    /// <summary>
    /// 渲染单个格子
    /// </summary>
    public void OnRendererCell(GameObject go, LineRenderer lineRenderer, TalentVO data,LineImage lineImage)
    {
        TalentElement talentElement = go.GetComponent<TalentElement>();
        if (talentElement == null)
        {
            talentElement = go.AddComponent<TalentElement>();
            talentElement.Initialize();
        }
        talentElement.SetContent(lineImage, lineRenderer, data);
    }

    /// <summary>
    /// 加载天赋树
    /// </summary>
    public void LoadTalentRoot()
    {
        if (m_TalentTypeRoot.childCount < 1)
        {
            int pageIndex = 0;
            GameObject obj = ObjectPool.Spawn(m_TalentTypeElement, m_TalentTypeRoot);
            Toggle toggle = obj.GetComponent<Toggle>();
            ItemHandler handler = toggle.gameObject.GetComponent<ItemHandler>() ?? toggle.gameObject.AddComponent<ItemHandler>();
            handler.OnPointerOverHandler = (pressed, go) => { OnPointerOverHandler(pressed, toggle, pageIndex); };
            handler.OnPointerPressHandler = (pressed, go) => { OnPointerPressHandler(pressed, toggle, pageIndex); };
            handler.OnSelectHandler = (selected, go) => { OnSelectionHandler(selected, toggle, pageIndex); };

            toggle.onValueChanged.RemoveAllListeners();
            toggle.onValueChanged.AddListener((bool select) => { OnCellSelectTalentRoot(select, obj); });
            toggle.group = m_ToggleGroup;
            m_TalentTypeTrans = obj.transform;
        }
        if (m_TalentTypeTrans == null)
        {
            m_TalentTypeTrans = m_TalentTypeRoot.GetChild(0);
        }

    }

    /// <summary>
    /// 第一个toggle 选中
    /// </summary>
    public void OnFirstToggle()
    {
        m_ToggleGroup.allowSwitchOff = true;
        m_TalentTypeTrans.GetComponent<Toggle>().isOn = false;
        m_TalentTypeTrans.GetComponent<Toggle>().isOn = true;
        //m_ToggleGroup.allowSwitchOff = false;
    }

    /// <summary>
    /// 渲染单个天赋树数据(只有一个)
    /// </summary>
    public void OnRendererTalentRoot()
    {
        TalentTypeElement talentTypeElement = m_TalentTypeTrans.GetComponent<TalentTypeElement>();
        if (talentTypeElement == null && m_TalentTypeTrans != null)
        {
            talentTypeElement = m_TalentTypeTrans.gameObject.AddComponent<TalentTypeElement>();
            talentTypeElement.Initialize();
        }
        talentTypeElement.SetContent(m_CurrentTalentTypeVO);
    }

   

    /// <summary>
    /// 选择单个格子
    /// </summary>
    public void OnCellSelect(bool select,GameObject obj)
    {
        obj.GetComponent<Animator>().SetBool("IsOn", select);
        if (!select)
            obj.GetComponent<Animator>().SetTrigger("Normal");
        int id = obj.GetComponent<TalentElement>().GetTalentVO().Id;
        if (select)
        {
            m_TalentProxy.GetTalentVODic().TryGetValue((uint)id,out  m_CurrentTalentVO);
            State.SetTipData(m_CurrentTalentVO);
            RefreshHotKey();
        }
       
    }

    /// <summary>
    /// 选择天赋树
    /// </summary>
    public void OnCellSelectTalentRoot(bool select, GameObject obj)
    {
        obj.GetComponent<Animator>().SetBool("IsOn", select);
        if (!select)
            obj.GetComponent<Animator>().SetTrigger("Normal");

        if (select)
        {
            State.SetTipData(m_CurrentTalentTypeVO);
            SetHotKeyEnabled(UIAction.Talent_Active_Upgrade, false);
            SetHotKeyEnabled(UIAction.Talent_BlockUp, false);
            SetHotKeyEnabled(UIAction.Talent_Reset, false);
            SetHotKeyDescription(UIAction.Talent_Active_Upgrade, 0);
        }
    }


    #region 按键
    /// <summary>
    /// 激活->>>>>升级
    /// </summary>
    /// <param name="callbackContext"></param>
    public void OnActivate(HotkeyCallback callbackContext)
    {
        if (callbackContext.started)
        {
            if (m_CurrentTalentVO.State == TalentState.CanActivate)
            {
                m_CurrentTalentVO.MTalentElement.ShowUnLock(true);
            }
        }
        if (callbackContext.performed)
        {
            m_CurrentTalentVO.MTalentElement.ShowUnLock(false);
            if (m_CurrentTalentVO.State==TalentState.CanActivate)
            {
                m_TalentProxy.GetTalentOperation(TalentCode.Active, (ulong)m_CurrentTalentVO.Id);
            }
            else if(m_CurrentTalentVO.State == TalentState.Activate)
            {
                m_TalentProxy.GetTalentOperation(TalentCode.UpLevel, (ulong)m_CurrentTalentVO.Id);
            }
        }
        if (callbackContext.cancelled)
        {
            m_CurrentTalentVO.MTalentElement.ShowUnLock(false);
        }

    }

    /// <summary>
    /// 停用
    /// </summary>
    /// <param name="callbackContext"></param>
    public void OnStopUse(HotkeyCallback callbackContext)
    {
        if (callbackContext.performed)
        {
            m_TalentProxy.GetTalentOperation(TalentCode.StopUse, (ulong)m_CurrentTalentVO.Id);
        }
    }

    /// <summary>
    /// 重置操作
    /// </summary>
    /// <param name="callbackContext"></param>
    public void OnReset(HotkeyCallback callbackContext)
    {
        if (callbackContext.performed)
        {
            m_TalentProxy.GetTalentOperation(TalentCode.Reset, (ulong)m_CurrentTalentRootId);
        }
    }


    /// <summary>
    /// 缩小
    /// </summary>
    /// <param name="callbackContext"></param>
    public void OnShrink(HotkeyCallback callbackContext)
    {
        if (callbackContext.started)
        {
            m_Down = true;
            m_Coroutine = UIManager.Instance.StartCoroutine(OpUpdate(() =>
            {
                m_ZoomValue -= m_OffSet;
                if (m_ZoomValue < m_ZoomMin)
                {
                    m_ZoomValue = m_ZoomMin;
                }
                Vector3 targetPos = new Vector3(m_ToggleGroup.transform.localPosition.x,
                    m_ToggleGroup.transform.localPosition.y, m_ZoomValue);
                m_ToggleGroup.transform.localPosition = Vector3.Lerp(m_ToggleGroup.transform.localPosition, targetPos, m_Time);
            }));
        }
        if (callbackContext.performed)
        {
            m_Down = false;

            if (m_Coroutine != null)
            {
                UIManager.Instance.StopCoroutine(m_Coroutine);
                m_Time = m_MaxTime;
            }

        }
    }

    /// <summary>
    /// 放大
    /// </summary>
    /// <param name="callbackContext"></param>
    public void OnMagnify(HotkeyCallback callbackContext)
    {
        if (callbackContext.started)
        {
            m_Down = true;
            m_Coroutine = UIManager.Instance.StartCoroutine(OpUpdate(() =>
            {
                m_ZoomValue += m_OffSet;
                if (m_ZoomValue > m_ZoomMax)
                {
                    m_ZoomValue = m_ZoomMax;
                }
                Vector3 targetPos = new Vector3(m_ToggleGroup.transform.localPosition.x,
                    m_ToggleGroup.transform.localPosition.y, m_ZoomValue);
                m_ToggleGroup.transform.localPosition = Vector3.Lerp(m_ToggleGroup.transform.localPosition, targetPos,m_Time);
            }));
        }
        if (callbackContext.performed)
        {
            m_Down = false;

            if (m_Coroutine != null)
            {
                UIManager.Instance.StopCoroutine(m_Coroutine);
                m_Time = m_MaxTime;
            }

        }
    }

   
    /// <summary>
    /// 协程
    /// </summary>
    /// <param name="callBack"></param>
    /// <returns></returns>
    public IEnumerator OpUpdate(Action callBack)
    {
        while (m_Down)
        {
            m_Time -= Time.deltaTime;
            if (m_Time < 0)
            {
                m_Time = m_MaxTime;
            }
            else
            {
                callBack();
            }
            yield return null;
        }
    }

    /// <summary>
    /// 协程
    /// </summary>
    /// <param name="callBack"></param>
    /// <returns></returns>
    public IEnumerator Excute(float time, Action callBack)
    {
        yield return new WaitForSeconds(time);
        callBack();
    }
    #endregion

    /// <summary>
    /// 设置热键可见性
    /// </summary>
    /// <param name="hotKeyID">hotKeyID</param>
    /// <param name="enable">可见性</param>
    public void SetHotKeyEnabled(string hotKeyID, bool enable, bool isHold = true)
    {
        State.GetAction(hotKeyID).Enabled = enable;
    }

    /// <summary>
    /// 设置热键可见性
    /// </summary>
    /// <param name="hotKeyID">hotKeyID</param>
    /// <param name="style">风格样式</param>
    public void SetHotKeyDescription(string hotKeyID, int style = 0, bool isHold = true)
    {
        State.GetAction(hotKeyID).State = style;
    }

    /// <summary>
    /// 刷新热键
    /// </summary>
    public void RefreshHotKey()
    {
        SetHotKeyEnabled(UIAction.Talent_Active_Upgrade, false);
        SetHotKeyEnabled(UIAction.Talent_BlockUp, false);
        SetHotKeyEnabled(UIAction.Talent_Reset, false);
        SetHotKeyDescription(UIAction.Talent_Active_Upgrade, 1);
        switch (m_CurrentTalentVO.State)
        {
            case TalentState.NoActivate:
                SetHotKeyDescription(UIAction.Talent_Active_Upgrade, 0);
                break;
            case TalentState.CanActivate:
                SetHotKeyEnabled(UIAction.Talent_Active_Upgrade, true);
                SetHotKeyDescription(UIAction.Talent_Active_Upgrade, 0);
                break;
            case TalentState.Activate:
                SetHotKeyEnabled(UIAction.Talent_Active_Upgrade, true);
                SetHotKeyEnabled(UIAction.Talent_BlockUp, true);
                break;
            case TalentState.FullLevel:
                SetHotKeyEnabled(UIAction.Talent_BlockUp, true);
                break;
            default:
                break;
        
        }
        SetHotKeyEnabled(UIAction.Talent_Reset, true);
    }


    /// <summary>
    /// 鼠标进出时
    /// </summary>
    /// <param name="pressed">是否重叠</param>
    /// <param name="toggle">当前Toggle</param>
    /// <param name="index">索引号</param>
    private void OnPointerOverHandler(bool over, Toggle toggle, int index)
    {
        if (!InputManager.Instance.GetNavigateMode())
        {
            toggle.isOn = over;
            toggle.animator.SetBool("IsOn", over);

            EventSystem.current.SetSelectedGameObject(over ? toggle.gameObject : null);
        }
    }

    /// <summary>
    /// 鼠标点击时
    /// </summary>
    /// <param name="pressed">是否按下</param>
    /// <param name="toggle">当前Toggle</param>
    /// <param name="index">索引号</param>
    private void OnPointerPressHandler(bool pressed, Toggle toggle, int index)
    {
        if (!InputManager.Instance.GetNavigateMode())
        {
            if (!pressed)
            {
               // SelectOpen((ProduceDialogType)(index + 1));
               // m_LastSelectedIndex = index;
            }
        }
    }

    /// <summary>
    /// 焦点变化时
    /// </summary>
    /// <param name="selected">是否有焦点</param>
    /// <param name="toggle">当前Toggle</param>
    /// <param name="index">索引号</param>
    private void OnSelectionHandler(bool selected, Toggle toggle, int index)
    {
        if (InputManager.Instance.GetNavigateMode())
        {
            toggle.isOn = selected;
            toggle.animator.SetBool("IsOn", selected);
        }
        else
        {
            if (!selected)
                toggle.animator.SetBool("IsOn", selected);
        }
    }
    /// <summary>
    /// 导航模式下确认时
    /// </summary>
    /// <param name="toggle">当前Toggle</param>
    /// <param name="index">索引号</param>
    private void OnSubmitHandler(Toggle toggle, int index)
    {
        //SelectOpen((ProduceDialogType)(index + 1));
       // m_LastSelectedIndex = index;
    }

    private class ItemHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, ISelectHandler, IDeselectHandler, ISubmitHandler
    {
        public UnityAction<bool, GameObject> OnPointerPressHandler;
        public UnityAction<bool, GameObject> OnPointerOverHandler;
        public UnityAction<bool, GameObject> OnSelectHandler;
        public UnityAction<GameObject> OnSubmitHandler;

        public void OnPointerDown(PointerEventData eventData)
        {
            OnPointerPressHandler?.Invoke(true, gameObject);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            OnPointerPressHandler?.Invoke(false, gameObject);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            OnPointerOverHandler?.Invoke(true, gameObject);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            OnPointerOverHandler?.Invoke(false, gameObject);
        }

        public void OnSelect(BaseEventData eventData)
        {
            OnSelectHandler?.Invoke(true, gameObject);
        }

        public void OnDeselect(BaseEventData eventData)
        {
            OnSelectHandler?.Invoke(false, gameObject);
        }

        public void OnSubmit(BaseEventData eventData)
        {
            OnSubmitHandler?.Invoke(gameObject);
        }
    }
}

/// <summary>
/// 天赋状态
/// </summary>
public enum TalentState
{
    /// <summary>
    /// 未激活 锁住
    /// </summary>
    NoActivate = 0,
    /// <summary>
    /// 可激活 解锁了
    /// </summary>
    CanActivate,
    /// <summary>
    /// 激活了升级中
    /// </summary>
    Activate,
    /// <summary>
    /// 满级
    /// </summary>
    FullLevel,
   
}
/// <summary>
/// 天赋指令
/// </summary>
public enum TalentCode
{
    Active=0,//激活
    UpLevel=1,//升级
    Reset=2,//重置
    StopUse =3,//停用
}