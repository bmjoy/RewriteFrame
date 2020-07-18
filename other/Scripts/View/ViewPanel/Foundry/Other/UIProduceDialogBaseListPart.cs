using Eternity.FlatBuffer;
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

public class UIProduceDialogBaseListPart : UIListPart
{
    /// <summary>
    /// 单条资源路径
    /// </summary>
    private const string ELEMENT_ADDRESS = AssetAddressKey.PRELOADUIELEMENT_PRODUCEDIALOGELEMENT;
    /// <summary>
    /// list预设地址
    /// </summary>
    private const string ASSET_ADDRESS = AssetAddressKey.PRELOADUI_COMMONLISTPART_PRODUCEDIALOG;
    /// <summary>
    /// 创角Proxy
    /// </summary>
    private ServerListProxy m_ServerListProxy; 

    /// <summary>
    /// 角色Proxy
    /// </summary>
    private CfgEternityProxy m_CfgEternityProxy;
    /// <summary>
	/// 面板上toggles 组
	/// </summary>
	private Toggle[] m_Toggles;
    /// <summary>
	/// 聚合界面入口类型名字
	/// </summary>
	protected string[] m_TypeNames;

    /// <summary>
    /// 聚合界面入口描述文字
    /// </summary>
    protected string[] m_TitleDescribes;

    /// <summary>
    /// 聚合界面图标
    /// </summary>
    protected int[] m_ImageIcons;

    /// <summary>
    /// 聚合界面图标背景
    /// </summary>
    protected int[] m_ImageBaseIcons;
    /// <summary>
    /// 聚合界面入口toggle个数
    /// </summary>
    protected int m_ToggleCount;

    /// <summary>
    /// 当前生产类型
    /// </summary>
    protected ProduceType m_CurrentProduceType;
    /// <summary>
	/// 标题图标
	/// </summary>
	protected Image m_TitleImage;
    /// <summary>
    /// 按钮列表
    /// </summary>
    private Transform m_ToggleRoot;
    /// <summary>
    /// 描述文本
    /// </summary>
    private TextMeshProUGUI m_TextDescribe;
    /// <summary>
    /// icon 图标
    /// </summary>
    private Image m_ImageIcon;
    /// <summary>
    /// icon 图标
    /// </summary>
    private Image m_ImageBsseIcon;
    /// <summary>
    /// 类型名字
    /// </summary>
    private TextMeshProUGUI m_TypeName;
    /// <summary>
    /// 数据表
    /// </summary>
    private List<ProduceDialogVO> m_ProduceDialogVoList = new List<ProduceDialogVO> ();
    /// <summary>
	/// 按下按钮索引
	/// </summary>
	private int m_LastSelectedIndex;
    /// <summary>
    /// 当前选中的按钮数据
    /// </summary>
    private ProduceDialogVO m_CurrentProduceDialogVO;

    private UIScrollRect m_UIScrollRect;

    public override void OnShow(object msg)
    {
        base.OnShow(msg);
        LoadViewPart(ASSET_ADDRESS, OwnerView.ListBox);
        m_CfgEternityProxy = (CfgEternityProxy)GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy);
        m_ServerListProxy = (ServerListProxy)GameFacade.Instance.RetrieveProxy(ProxyName.ServerListProxy);
        SetData();
        CreateButtons();
        State.OnSelectionChanged -= OnSelectionDataChanged;
        State.OnSelectionChanged += OnSelectionDataChanged;
        State.GetAction(UIAction.Common_Select).Callback += OnClick;

    }

    public override void OnHide()
    {
        base.OnHide();
        State.OnSelectionChanged -= OnSelectionDataChanged;
        State.GetAction(UIAction.Common_Select).Callback -= OnClick;
    }
    protected override string GetCellPlaceholderTemplate()
    {
        return null;
    }
    protected override void OnViewPartLoaded()
    {
        base.OnViewPartLoaded();
        ShowCharacter();
        OwnerView.PageBox.gameObject.SetActive(false);

        m_UIScrollRect = FindComponent<UIScrollRect>("Content/Scroller");
        ProduceDialogBasePanel produceDialogBasePanel = OwnerView as ProduceDialogBasePanel;
        if (produceDialogBasePanel.CloseByEsc)
        {
            //State.GetPage(0).ListSelection = new Vector2Int(0, 0);
            m_UIScrollRect.SetSelection(new Vector2Int(0, 0));
            produceDialogBasePanel.CloseByEsc = false;
        }
        switch (produceDialogBasePanel.CurrentProduceType)
        {
            case ProduceType.HeavyWeapon:
            case ProduceType.Reformer:
            case ProduceType.Device:
            case ProduceType.Ship:
                m_UIScrollRect.CellListPadding = new RectOffset(0,
                    m_UIScrollRect.CellListPadding.right,
                    m_UIScrollRect.CellListPadding.top,
                    m_UIScrollRect.CellListPadding.bottom);
                break;
            case ProduceType.Chip:
                m_UIScrollRect.CellListPadding = new RectOffset(170, 
                    m_UIScrollRect.CellListPadding.right,
                    m_UIScrollRect.CellListPadding.top,
                    m_UIScrollRect.CellListPadding.bottom);
                break;
          
            default:
                break;
        }

    }


    protected override void OnViewPartUnload()
    {
        base.OnViewPartUnload();
       
    }

    protected override string GetCellTemplate()
    {
        return ELEMENT_ADDRESS;
    }
    private void OnSelectionDataChanged(object obj)
    {
        if (obj is ProduceDialogVO)
        {
            m_CurrentProduceDialogVO = obj as ProduceDialogVO;
            State.GetAction(UIAction.Common_Select).Enabled = true;
        }
        else
        {
            State.GetAction(UIAction.Common_Select).Enabled = false;
        }
    }

    protected override void OnCellRenderer(int groupIndex, int cellIndex, object cellData, RectTransform cellView, bool selected)
    {
        Animator animator = cellView.GetComponent<Animator>();
        if (animator)
            animator.SetBool("IsOn", selected);

        ProduceDialogVO infoVO = (ProduceDialogVO)cellData;
        OnClickTool onClickTool = cellView.GetOrAddComponent<OnClickTool>();
        onClickTool.OnClickHandler = OnClick;
        onClickTool.OnEnterHandler = OnEnter;
        onClickTool.OnExitHandler = OnExit;
        Transform childTf = cellView.transform;
        m_TextDescribe = childTf.Find("Move/Label_Des").GetComponent<TextMeshProUGUI>();
        m_TextDescribe.SetText(TableUtil.GetLanguageString(infoVO.TextDescribe));
        m_TypeName = childTf.Find("Move/Label_Name").GetComponent<TextMeshProUGUI>();
        m_TypeName.SetText(TableUtil.GetLanguageString(infoVO.TypeName));
        m_ImageIcon = childTf.Find("Image_Icon").GetComponent<Image>();
        m_ImageBsseIcon = childTf.Find("ImageBase").GetComponent<Image>();
        UIUtil.SetIconImage(m_ImageIcon, (uint)infoVO.ImageIcon);
        UIUtil.SetIconImage(m_ImageBsseIcon, (uint)infoVO.ImageBaseIcon);

    }

    /// <summary>
    /// 页签变化时
    /// </summary>
    /// <param name="oldIndex">老页签</param>
    /// <param name="newIndex">新页签</param>
    protected override void OnPageIndexChanged(int oldIndex, int newIndex)
    {
        FillPanel();
    }

    /// <summary>
    /// 点击事件
    /// </summary>
    /// <param name="arg0"></param>
    /// <param name="arg1"></param>
    private void OnClick(bool arg0, GameObject arg1)
    {
        UIManager.Instance.StartCoroutine(Excute(Time.deltaTime,()=>{
            SelectOpen(m_CurrentProduceDialogVO.Index + 1);
        }));
    }

    /// <summary>
    /// 离开事件
    /// </summary>
    /// <param name="arg0"></param>
    /// <param name="arg1"></param>
    private void OnExit(bool arg0, GameObject arg1)
    {
        //if (InputManager.Instance.CurrentInputDevice != InputManager.GameInputDevice.KeyboardAndMouse)
        //{
        //    State.GetAction(UIAction.Common_Select).Enabled = false;
        //}
    }

    /// <summary>
    /// 进入事件
    /// </summary>
    /// <param name="arg0"></param>
    /// <param name="arg1"></param>
    private void OnEnter(bool arg0, GameObject arg1)
    {
      //  if (InputManager.Instance.CurrentInputDevice != InputManager.GameInputDevice.KeyboardAndMouse)
        //{
        //    State.GetAction(UIAction.Common_Select).Enabled = true;
        //}
    }

    /// <summary>
    /// 按下enter
    /// </summary>
    /// <param name="callbackContext"></param>
    public void OnClick(HotkeyCallback callbackContext)
    {
        if (callbackContext.performed)
        {
            if (callbackContext.isFromKeyboardMouse || callbackContext.isFromUI||callbackContext.isNavigateMode)
            {
                SelectOpen(m_CurrentProduceDialogVO.Index + 1);
               
            }
        }
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
    /// 创建button
    /// </summary>
    public void CreateButtons()
    {
        m_ProduceDialogVoList.Clear();
        for (int i = 0; i < m_ToggleCount; i++)
        {
            ProduceDialogVO produceDialogVO = new ProduceDialogVO();
            produceDialogVO.Index = i;
            produceDialogVO.ImageBaseIcon = m_ImageBaseIcons[i];
            produceDialogVO.ImageIcon = m_ImageIcons[i];
            produceDialogVO.TypeName = m_TypeNames[i];
            produceDialogVO.TextDescribe = m_TitleDescribes[i];
            m_ProduceDialogVoList.Add(produceDialogVO);
        }

    }

    /// <summary>
    /// 填充数据（标题和类型）
    /// </summary>
    public void SetData()
    {
        ProduceDialogBasePanel produceDialogBasePanel = OwnerView as ProduceDialogBasePanel;
        if (produceDialogBasePanel != null)
        {
            switch (produceDialogBasePanel.CurrentProduceType)
            {
                case ProduceType.HeavyWeapon:
                    m_ToggleCount = 4;
                    m_TitleDescribes = new string[] { "production_text_1001", "production_text_1002", "production_text_1003", "production_text_1015" };
                    m_TypeNames = new string[] { "production_title_1006", "production_title_1007", "production_title_1008", "production_title_1020" };
                    m_ImageIcons = new int[] { 40106, 40107, 40108, 40104 };
                    m_ImageBaseIcons = new int[] { 40109, 40109, 40109, 40105 };
                    m_CurrentProduceType = ProduceType.HeavyWeapon;
                    break;
                case ProduceType.Reformer:
                    m_ToggleCount = 4;
                    m_TitleDescribes = new string[] { "production_text_1010", "production_text_1011", "production_text_1012", "production_text_1015" };
                    m_TypeNames = new string[] { "production_title_1015", "production_title_1016", "production_title_1017", "production_title_1020" };
                    m_ImageIcons = new int[] { 40100, 40101, 40102, 40104 };
                    m_ImageBaseIcons = new int[] { 40103, 40103, 40103, 40105 };
                    m_CurrentProduceType = ProduceType.Reformer;
                    break;
                case ProduceType.Chip:
                    m_ToggleCount = 3;
                    m_TitleDescribes = new string[] { "production_text_1013", "production_text_1014", "production_text_1015" };
                    m_TypeNames = new string[] { "production_title_1018", "production_title_1019", "production_title_1020" };
                    m_ImageIcons = new int[] { 40110, 40111, 40104 };
                    m_ImageBaseIcons = new int[] { 40113, 40113, 40105 };
                    m_CurrentProduceType = ProduceType.Chip;
                    break;
                case ProduceType.Device:
                    m_ToggleCount = 4;
                    m_TitleDescribes = new string[] { "production_text_1007", "production_text_1008", "production_text_1009", "production_text_1015" };
                    m_TypeNames = new string[] { "production_title_1012", "production_title_1013", "production_title_1014", "production_title_1020" };
                    m_ImageIcons = new int[] { 40114, 40115, 40116, 40104 };
                    m_ImageBaseIcons = new int[] { 40117, 40117, 40117, 40105 };
                    m_CurrentProduceType = ProduceType.Device;
                    break;
                case ProduceType.Ship:
                    m_ToggleCount = 4;
                    m_TitleDescribes = new string[] { "production_text_1042", "production_text_1043", "production_text_1044", "production_text_1015" };
                    m_TypeNames = new string[] { "production_title_1062", "production_title_1063", "production_title_1064", "production_title_1020" };
                    m_ImageIcons = new int[] { 40118, 40119, 40120, 40104 };
                    m_ImageBaseIcons = new int[] { 40121, 40121, 40121, 40105 };
                    m_CurrentProduceType = ProduceType.Ship;
                    break;
                default:
                    break;
            }
        }
        
    }

    /// <summary>
    /// 填充面板数据
    /// </summary>
    public void FillPanel()
    {
        AddDatas(null, m_ProduceDialogVoList.ToArray());
    }

    /// <summary>
    /// 选择打开子面板
    /// </summary>
    /// <param name="typeName">面板名字</param>
    public virtual void SelectOpen(int index)
    {
        ProduceDialogType type = (ProduceDialogType)index;
        UIManager.Instance.ClosePanel(OwnerView);

        UIManager.Instance.StartCoroutine(Excute(0, () =>
        {
            MsgOpenProduce msgOpenProduce = new MsgOpenProduce();
            msgOpenProduce.MProduceDialogType = type;
            msgOpenProduce.CurrentProduceType = m_CurrentProduceType;
            switch (m_CurrentProduceType)
            {
                case ProduceType.HeavyWeapon:
                    UIManager.Instance.OpenPanel(UIPanel.ProduceWeaponPanel, msgOpenProduce);
                    break;
                case ProduceType.Reformer:
                    UIManager.Instance.OpenPanel(UIPanel.ProduceReformerPanel, msgOpenProduce);
                    break;
                case ProduceType.Chip:
                    UIManager.Instance.OpenPanel(UIPanel.ProduceChipPanel, msgOpenProduce);
                    break;
                case ProduceType.Device:
                    UIManager.Instance.OpenPanel(UIPanel.ProduceDevicePanel, msgOpenProduce);
                    break;
                case ProduceType.Ship:
                    UIManager.Instance.OpenPanel(UIPanel.ProduceShipPanel, msgOpenProduce);
                    break;
                default:
                    break;
            }
        }
        ));
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

    private class OnClickTool : MonoBehaviour, IPointerDownHandler,IPointerExitHandler,IPointerEnterHandler
    {
        /// <summary>
        /// 点击事件
        /// </summary>
        public UnityAction<bool, GameObject> OnClickHandler;
        /// <summary>
        /// 进入事件
        /// </summary>
        public UnityAction<bool, GameObject> OnEnterHandler;
        /// <summary>
        /// 离开事件
        /// </summary>
        public UnityAction<bool, GameObject> OnExitHandler;

        public void OnPointerDown(PointerEventData eventData)
        {
            OnClickHandler?.Invoke(true, gameObject);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            OnEnterHandler?.Invoke(true, gameObject);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            OnExitHandler?.Invoke(true, gameObject);
        }
    }
}
