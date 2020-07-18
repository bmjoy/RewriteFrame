using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UITipBasePart : BaseViewPart
{
    private const string ASSET_ADDRESS = Leyoutech.Core.Loader.Config.AssetAddressKey.PRELOADUI_COMMONTIPPART;

    /// <summary>
    /// 下一个实例的ID
    /// </summary>
    private static int NEXT_INSTANCE_ID = 1;


    /// <summary>
    /// 切换动画
    /// </summary>
    private Animator m_Animator;
    /// <summary>
    /// 空盒子
    /// </summary>
    private RectTransform m_EmptyBox;
    /// <summary>
    /// 空盒子（比较）
    /// </summary>
    private RectTransform m_EmptyBox2;
    /// <summary>
    /// 左边的Tip容器
    /// </summary>
    protected RectTransform TipBoxLeft { get; private set; }
    /// <summary>
    /// 右边的Tip容器
    /// </summary>
    protected RectTransform TipBoxRight { get; private set; }

    /// <summary>
    /// Tip数据
    /// </summary>
    protected object TipData { get; private set; }
    /// <summary>
    /// 原始Tip数据
    /// </summary>
    protected object NativeTipData { get; private set; }
    /// <summary>
    /// 当前是否为比较模式
    /// </summary>
    protected bool CompareMode { get; private set; }
    /// <summary>
    /// 当前比较索引号
    /// </summary>
    protected int CompareIndex { get; private set; }
    /// <summary>
    /// Tip比较数据
    /// </summary>
    protected object CompareTipData
    {
        get
        {
            if (CompareIndex < 0)
                return null;
            if (m_CompareDataList.Count == 0)
                return null;

            return m_CompareDataList[CompareIndex % m_CompareDataList.Count];
        }
    }

    /// <summary>
    /// 当前Tip的可以比较的所有数据
    /// </summary>
    private List<object> m_CompareDataList = new List<object>();

    /// <summary>
    /// 滚动控制器1
    /// </summary>
    private List<ScrollController> m_ScrollControllers = new List<ScrollController>();

    /// <summary>
    /// 上一次的页面索引
    /// </summary>
    private int m_PageIndexPrev = -1;

    /// <summary>
    /// 自已的实例ID
    /// </summary>
    private int m_InstanceID = 0;

    /// <summary>
    /// 状态数据
    /// </summary>
    public virtual UIViewState State
    {
        get
        {
            return OwnerView.State;
        }
    }

    /// <summary>
    /// Tip挂点
    /// </summary>
    protected virtual Transform TipBox
    {
        get { return OwnerView.TipBox; }
    }

    public override void OnShow(object msg)
    {
        base.OnShow(msg);

        if(m_InstanceID==0)
        {
            m_InstanceID = NEXT_INSTANCE_ID;
            NEXT_INSTANCE_ID++;
        }

        TipData = null;
        NativeTipData = null;
        CompareMode = false;
        CompareIndex = -1;
        //CompareTipData = null;

        m_PageIndexPrev = -1;
        m_CompareDataList.Clear();

        State.ActionCompareEnableChanged -= OnTipComapreEnableChanged;
        State.ActionCompareEnableChanged += OnTipComapreEnableChanged;
        State.OnPageIndexChanged -= OnPageIndexChanged;
        State.OnPageIndexChanged += OnPageIndexChanged;
        State.OnCategoryIndexChanged -= OnFilterIndexChanged;
        State.OnCategoryIndexChanged += OnFilterIndexChanged;

        InputManager.Instance.OnInputDeviceChanged -= OnInputDeviceChanged;
        InputManager.Instance.OnInputDeviceChanged += OnInputDeviceChanged;

        HotkeyManager.Instance.Unregister("right_stick_scrollTip_" + m_InstanceID);
        HotkeyManager.Instance.Register("right_stick_scrollTip_" + m_InstanceID, HotKeyMapID.UI, HotKeyID.UGUI_Stick2, OnRightStickChanged);

        LoadViewPart(ASSET_ADDRESS, TipBox);
    }

    public override void OnHide()
    {
        State.ActionCompareEnableChanged -= OnTipComapreEnableChanged;
        
        State.OnPageIndexChanged -= OnPageIndexChanged;
        State.OnCategoryIndexChanged -= OnFilterIndexChanged;

        InputManager.Instance.OnInputDeviceChanged -= OnInputDeviceChanged;

        HotkeyManager.Instance.Unregister("right_stick_scrollTip_" + m_InstanceID);

        TipData = null;
        NativeTipData = null;
        CompareMode = false;
        CompareIndex = -1;
        //CompareTipData = null;

        m_PageIndexPrev = -1;
        m_CompareDataList.Clear();

        base.OnHide();
    }

    /// <summary>
    /// 获得焦点时
    /// </summary>
    public override void OnGotFocus()
    {
        base.OnGotFocus();
    }

    /// <summary>
    /// 部件加载时
    /// </summary>
    protected override void OnViewPartLoaded()
    {
        m_Animator = FindComponent<Animator>("Content");
        m_EmptyBox = FindComponent<RectTransform>("Content/TipBox1/Empty");
        m_EmptyBox2 = FindComponent<RectTransform>("Content/TipBox2/Empty");
        TipBoxLeft = FindComponent<RectTransform>("Content/TipBox1");
        TipBoxRight = FindComponent<RectTransform>("Content/TipBox2");

        if (TipBoxLeft)
        {
            State.OnSelectionChanged -= OnTipDataChanged;
            State.OnSelectionChanged += OnTipDataChanged;

            InstallCompareHotkey();

            InitializePage();
        }
    }

    /// <summary>
    /// 部件卸载时
    /// </summary>
    protected override void OnViewPartUnload()
    {
        State.OnSelectionChanged -= OnTipDataChanged;

        State.GetAction(UIAction.Common_Compare_ExitCompare).Callback -= OnToggleCompareMode;
        State.GetAction(UIAction.Common_SwitchWeapon).Callback -= OnToggleCompareIndex;

        CloseTipView();
        CloseCompareView();

        m_Animator = null;
        m_EmptyBox = null;
        m_EmptyBox2 = null;
        TipBoxLeft = null;
        TipBoxRight = null;

        m_ScrollControllers.Clear();
    }

    private void OnTipComapreEnableChanged()
    {
        InstallCompareHotkey();
    }

    private void OnPageIndexChanged(int oldIndex,int newIndex)
    {
        UpdateHotkeyState();
    }

    private void OnFilterIndexChanged(int pageIndex,int oldIndex, int newIndex)
    {
        SetCompareMode(false);
        SetCompareIndex(-1);
        UpdateHotkeyState();
    }

    private void InstallCompareHotkey()
    {
        State.GetAction(UIAction.Common_Compare_ExitCompare).Callback -= OnToggleCompareMode;
        State.GetAction(UIAction.Common_SwitchWeapon).Callback -= OnToggleCompareIndex;

        if (State.GetActionCompareEnabled())
        {
            State.GetAction(UIAction.Common_Compare_ExitCompare).Callback += OnToggleCompareMode;
            State.GetAction(UIAction.Common_SwitchWeapon).Callback += OnToggleCompareIndex;
        }
    }

    /// <summary>
    /// Tip数据改变时
    /// </summary>
    /// <param name="data">TipData</param>
    private void InitializePage()
    {
        bool allowCompare = State.GetActionCompareEnabled();

        m_PageIndexPrev = State.GetPageIndex();

        SetCompareMode(State.IsCompareMode(), false);
        SetCompareIndex(-1);

        NativeTipData = State.GetTipData();
        TipData = AdapterTipData(NativeTipData);

        CloseTipView();
        OpenTipView();

        m_CompareDataList.Clear();
        FindCompareableData(TipData, m_CompareDataList);

        if (allowCompare && m_CompareDataList.Count > 0)
        {
            SetCompareIndex(State.GetCompareIndex(), false);
        }
        else
        {
            SetCompareMode(false);
            SetCompareIndex(-1);
        }

        m_EmptyBox.gameObject.SetActive(TipData == null);

        UpdateHotkeyState();

        CloseCompareView();
        if (CompareMode)
            OpenCompareView();
    }

    /// <summary>
    /// Tip数据改变时
    /// </summary>
    /// <param name="data">TipData</param>
    private void OnTipDataChanged(object data)
    {
        bool allowCompare = State.GetActionCompareEnabled();

        int pageIndex = State.GetPageIndex();
        if (pageIndex != m_PageIndexPrev)
        {
            m_PageIndexPrev = pageIndex;

            SetCompareMode(false);
            SetCompareIndex(-1);

            m_Animator.ResetTrigger("Move");
            m_Animator.ResetTrigger("Compare");
            m_Animator.ResetTrigger("Normal");

            m_Animator.SetTrigger("Move");
        }

        TipData = null;
        TipData = AdapterTipData(data);

        NativeTipData = data;

        CloseTipView();
        OpenTipView();

        m_CompareDataList.Clear();
        FindCompareableData(TipData, m_CompareDataList);

        if (allowCompare/* && m_CompareDataList.Count > 0*/)
        {
            //if (CompareTipData != null)
            //{
            //    int index = m_CompareDataList.IndexOf(CompareTipData);
            //    if (index != -1)
            //       SetCompareIndex(index);
            //    else
            //       SetCompareIndex(0);
            //}
            //else
            //    SetCompareIndex(0);

            if (m_CompareDataList.Count > 0 && CompareIndex == -1)
                SetCompareIndex(0);
        }
        else
        {
            SetCompareMode(false);
            SetCompareIndex(-1);
        }

        m_EmptyBox.gameObject.SetActive(TipData == null);

        UpdateHotkeyState();

        CloseCompareView();
        if(CompareMode)
            OpenCompareView();

        m_EmptyBox2.gameObject.SetActive(CompareMode && CompareTipData == null);
    }

    /// <summary>
    /// 切换比较模式
    /// </summary>
    /// <param name="data">热键状态</param>
    private void OnToggleCompareMode(HotkeyCallback callback)
    {
        if (!callback.performed)
            return;

        //if (m_CompareDataList.Count > 0)
        //{
            SetCompareMode(!CompareMode);
            SetCompareIndex(CompareMode ? 0 : -1);

            if (CompareMode)
                PlaySound(48);
            else
                PlaySound(49);
        //}
        //else
        //{
        //    SetCompareMode(false);
        //    SetCompareIndex(-1);
        //}

        UpdateHotkeyState();

        CloseCompareView();
        if (CompareMode)
            OpenCompareView();

        m_EmptyBox.gameObject.SetActive(TipData == null);
        m_EmptyBox2.gameObject.SetActive(CompareMode && CompareTipData == null);
    }

    /// <summary>
    /// 切换比较索引
    /// </summary>
    /// <param name="callback"></param>
    private void OnToggleCompareIndex(HotkeyCallback callback)
    {
        if (!callback.performed)
            return;

        if(CompareMode)
        {
            SetCompareIndex(CompareIndex + 1);

            CloseCompareView();
            OpenCompareView();
        }
        else
        {
            CompareIndex = -1;
            CloseCompareView();
        }

        m_EmptyBox.gameObject.SetActive(TipData == null);
        m_EmptyBox2.gameObject.SetActive(CompareMode && CompareTipData == null);
    }

    /// <summary>
    /// 更新热键状态
    /// </summary>
    private void UpdateHotkeyState()
    {
        if (State.GetActionCompareEnabled())
        {
            State.GetAction(UIAction.Common_Compare_ExitCompare).Enabled = true;//TipData != null; && m_CompareDataList.Count > 0;
            State.GetAction(UIAction.Common_Compare_ExitCompare).State = CompareMode ? 1 : 0;

            State.GetAction(UIAction.Common_SwitchWeapon).Enabled = TipData != null && CompareMode && m_CompareDataList.Count > 1;
        }
    }

    /// <summary>
    /// 设置比较模式
    /// </summary>
    /// <param name="mode">比较模式</param>
    private void SetCompareMode(bool mode, bool needEvent = true)
    {
        //if (CompareMode != mode)
        //{
            CompareMode = mode;

            m_Animator.ResetTrigger("Move");
            m_Animator.ResetTrigger("Compare");
            m_Animator.ResetTrigger("Normal");

            if(CompareMode)
                m_Animator.SetTrigger("Compare");
            else
                m_Animator.SetTrigger("Normal");

            if(needEvent)
                State.SetCompareMode(CompareMode);
        //}
    }

    /// <summary>
    /// 设置比较索引
    /// </summary>
    /// <param name="compareIndex">比较索引</param>
    private void SetCompareIndex(int index, bool needEvent = true)
    {
        if (CompareMode)
        {
            CompareIndex = Mathf.Max(0, index);

            if (m_CompareDataList.Count > 0)
            {
                CompareIndex = CompareIndex % m_CompareDataList.Count;
                //CompareTipData = m_CompareDataList[CompareIndex];

                if(needEvent)
                    State.SetCompareIndex(CompareIndex);
                return;
            }
        }

        CompareIndex = -1;
        //CompareTipData = null;

        if(needEvent)
            State.SetCompareIndex(-1);
    }

    /// <summary>
    /// 适配Tip数据
    /// </summary>
    /// <param name="data">原始数据</param>
    /// <returns>适配后的数据</returns>
    protected virtual object AdapterTipData(object data)
    {
        return data;
    }

    /// <summary>
    /// 关闭Tip内容
    /// </summary>
    protected virtual void CloseTipView()
    {

    }

    /// <summary>
    /// 打开Tip视图
    /// </summary>
    protected virtual void OpenTipView()
    {

    }

    /// <summary>
    /// 查找可以比较的数据
    /// </summary>
    /// <param name="data">当前数据</param>
    /// <param name="compareableDatas">可参与比较的数据列表</param>
    protected virtual void FindCompareableData(object data, List<object> compareableDatas)
    {

    }

    /// <summary>
    /// 关闭比较视图
    /// </summary>
    protected virtual void CloseCompareView()
    {

    }

    /// <summary>
    /// 打开比较视图
    /// </summary>
    protected virtual void OpenCompareView()
    {

    }



    /// <summary>
    /// 加载部件资源
    /// </summary>
    /// <param name="path">部件资源地址</param>
    /// <param name="callback">加载回调</param>
    protected void LoadPrefabFromPool(string assetAddress, UnityAction<GameObject> callback)
    {
        UnityAction<GameObject> callbackFun = callback;

        AssetUtil.LoadAssetAsync(assetAddress,
            (pathOrAddress, returnObject, userData) =>
            {
                //忽略关闭之后的加载回调
                if (!Opened) return;

                if (returnObject != null)
                {
                    GameObject prefab = (GameObject)returnObject;

                    prefab.CreatePool(pathOrAddress);

                    callbackFun?.Invoke(prefab);

                    InstallScrollController();
                }
                else
                {
                    Debug.LogError(string.Format("资源加载成功，但返回null,  pathOrAddress = {0}", pathOrAddress));
                }
            });
    }

    /// <summary>
    /// 当装滚动控制器
    /// </summary>
    protected void InstallScrollController()
    {
        m_ScrollControllers.Clear();

        ScrollRect[] scrollers = GetTransform().gameObject.GetComponentsInChildren<ScrollRect>();
        foreach(ScrollRect scroller in scrollers)
        {
            ScrollController controller = scroller.gameObject.GetComponent<ScrollController>();
            if (!controller)
                controller = scroller.gameObject.AddComponent<ScrollController>();

            m_ScrollControllers.Add(controller);
        }

        OnInputDeviceChanged(InputManager.Instance.CurrentInputDevice);
    }

    /// <summary>
    /// 输入硬件改变时
    /// </summary>
    private void OnInputDeviceChanged(InputManager.GameInputDevice device)
    {
        if (m_ScrollControllers.Count > 0)
        {
            bool enabled = InputManager.Instance.CurrentInputDevice != InputManager.GameInputDevice.KeyboardAndMouse;
            foreach (ScrollController controller in m_ScrollControllers)
            {
                controller.enabled = enabled;
            }
        }
    }

    /// <summary>
    /// 右摇杆输入时
    /// </summary>
    /// <param name="callback"></param>
    private void OnRightStickChanged(HotkeyCallback callback)
    {
        if (m_ScrollControllers.Count > 0)
        {
            Vector2 axis = callback.ReadValue<Vector2>();
            foreach (ScrollController controller in m_ScrollControllers)
            {
                controller.Target.verticalNormalizedPosition += axis.y * controller.ScrollerMulti;
            }
        }
    }

    [RequireComponent(typeof(ScrollRect))]
    private class ScrollController : MonoBehaviour
    {
        public ScrollRect Target;

        public float ScrollerMulti = 0.1f;

        private void Awake()
        {
            Target = GetComponent<ScrollRect>();
        }
    }
}
