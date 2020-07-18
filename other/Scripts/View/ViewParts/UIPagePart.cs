using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Eternity.FlatBuffer;

public class UIPagePart : BaseViewPart
{
    private const string ASSET_ADDRESS = Leyoutech.Core.Loader.Config.AssetAddressKey.PRELOADUI_COMMONPAGEPART;

    /// <summary>
    /// 按钮容器
    /// </summary>
    private Transform ButtonBox;
    /// <summary>
    /// 动作后需要进入页码
    /// </summary>
    private int m_NextPageIndex;
    /// <summary>
    /// 忽略事件
    /// </summary>
    private bool m_SkipEventListener;

    public override void OnShow(object msg)
    {
        base.OnShow(msg);

        m_NextPageIndex = -1;

        LoadViewPart(ASSET_ADDRESS, OwnerView.PageBox);
    }

    public override void OnHide()
    {
        if (OwnerView.AnimatorController != null)
            OwnerView.AnimatorController.OnAnimationEvent -= OnAnimationEvent;
        else
            Debug.Log(OwnerView);
        base.OnHide();
    }

    /// <summary>
    /// 部件加载时
    /// </summary>
    protected override void OnViewPartLoaded()
    {
        OwnerView.State.OnPageIndexChanged -= OnPageIndexChanged;
        OwnerView.State.OnPageIndexChanged += OnPageIndexChanged;
        OwnerView.State.OnPageLabelChanged -= OnPageLabelChanged;
        OwnerView.State.OnPageLabelChanged += OnPageLabelChanged;
        OwnerView.State.OnPageVisibleChanged -= OnPageVisibleChanged;
        OwnerView.State.OnPageVisibleChanged += OnPageVisibleChanged;

        UpdateList();
        InstallHotkey();

        OnPageIndexChanged(OwnerView.State.GetPageIndex(), OwnerView.State.GetPageIndex());
    }

    /// <summary>
    /// 恢复热键
    /// </summary>
    public override void OnGotFocus()
    {
        base.OnGotFocus();

        InstallHotkey();
    }

    /// <summary>
    /// 安装热键
    /// </summary>
    public virtual void InstallHotkey()
    {
        if (!GetTransform())
            return;
        OwnerView.DeleteHotKey("NavNegative");
        OwnerView.DeleteHotKey("NavPositive");
        //Q键
        Transform hotkeyQ = FindComponent<Transform>("Content/LImage");
        if (hotkeyQ)
            OwnerView.AddHotKey("NavNegative", HotKeyID.NavNegative, OnQCallback, hotkeyQ, null, HotkeyManager.HotkeyStyle.UI_SIMPLE);

        //E键
        Transform hotkeyE = FindComponent<Transform>("Content/RImage");
        if (hotkeyE)
            OwnerView.AddHotKey("NavPositive", HotKeyID.NavPositive, OnECallback, hotkeyE, null, HotkeyManager.HotkeyStyle.UI_SIMPLE);
    }

    /// <summary>
    /// 部件卸载时
    /// </summary>
    protected override void OnViewPartUnload()
    {
        OwnerView.State.OnPageIndexChanged -= OnPageIndexChanged;
        OwnerView.State.OnPageLabelChanged -= OnPageLabelChanged;
        OwnerView.State.OnPageVisibleChanged -= OnPageVisibleChanged;

        ClearList();

        base.OnViewPartUnload();
    }

    /// <summary>
    /// 获取页面总数
    /// </summary>
    /// <returns>总数</returns>
    private int GetPageCount()
    {
        return Config.HasValue ? Config.Value.LabelIdLength : 0;
    }

    /// <summary>
    /// 页面索引改变时
    /// </summary>
    /// <param name="oldIndex">变化前的索引号</param>
    /// <param name="newIndex">变化后的索引号</param>
    private void OnPageIndexChanged(int oldIndex, int newIndex)
    {
        newIndex = Mathf.Clamp(newIndex, 0, GetPageCount() - 1);

        m_SkipEventListener = true;

        Transform ButtonBox = FindComponent<Transform>("Content/ButtonBox");
        for (int i = 0; i < ButtonBox.childCount; i++)
        {
            Transform child = ButtonBox.GetChild(i);
            if (child.gameObject.activeSelf)
            {
                Toggle button = child.GetComponent<Toggle>();
                if (button)
                    button.isOn = i == newIndex;
            }
        }

        m_SkipEventListener = false;
    }

    /// <summary>
    /// 页面标签改变时
    /// </summary>
    /// <param name="index">页索引</param>
    /// <param name="text">文本</param>
    private void OnPageLabelChanged(int index, string text)
    {
        Transform ButtonBox = FindComponent<Transform>("Content/ButtonBox");
        if (index < ButtonBox.childCount)
        {
            Transform child = ButtonBox.GetChild(index);

            //更新显示
            UIIconAndLabel buttonInfo = child.GetComponent<UIIconAndLabel>();
            if (buttonInfo && buttonInfo.Info)
                buttonInfo.Info.text = text;
        }
    }

    /// <summary>
    /// 页面可见性改变时
    /// </summary>
    /// <param name="pageIndex">页索引</param>
    /// <param name="visible">可见性</param>
    private void OnPageVisibleChanged(int pageIndex, bool visible)
    {
        int currIndex = OwnerView.State.GetPageIndex();

        if (pageIndex != currIndex)
        {
            UpdateList();

            OnPageIndexChanged(OwnerView.State.GetPageIndex(), OwnerView.State.GetPageIndex());
        }
        else
        {
            UpdateList();

            int tabCount = GetPageCount();

            //尝试向右切页
            int indexRight = currIndex;
            indexRight++;
            while (indexRight < tabCount)
            {
                if (OwnerView.State.GetPageVisible(indexRight))
                    break;

                indexRight++;
            }

            if (indexRight < tabCount)
            {
                ChangePageIndex(indexRight, true);
                return;
            }

            //尝试向左切页
            int indexLeft = currIndex;
            indexLeft--;
            while (indexLeft >= 0)
            {
                if (OwnerView.State.GetPageVisible(indexLeft))
                    break;

                indexLeft--;
            }

            if (indexLeft >= 0)
            {
                ChangePageIndex(indexLeft, true);
                return;
            }

            //找不到可用页
            ChangePageIndex(-1, true);
        }
    }

    /// <summary>
    /// 收到Q键回调
    /// </summary>
    /// <param name="callback">热键状态</param>
    public void OnQCallback(HotkeyCallback callback)
    {
        if (!callback.performed)
            return;

        if (OwnerView == null)
            return;

        if (m_NextPageIndex != -1)
            return;

        int tabIndex = OwnerView.State.GetPageIndex();

        tabIndex--;
        while (tabIndex >= 0)
        {
            if (OwnerView.State.GetPageVisible(tabIndex))
                break;

            tabIndex--;
        }

        if (tabIndex < 0)
        {
            WwiseUtil.PlaySound((int)WwiseMusic.Music_Button_SelectMove_invalid, false, null);
        }
        else
        {
            ChangePageIndex(tabIndex);
            WwiseUtil.PlaySound((int)WwiseMusic.Music_Button_SelectMove_valid, false, null);
        }
    }

    /// <summary>
    /// 收到E键回调
    /// </summary>
    /// <param name="callback">热键状态</param>
    public void OnECallback(HotkeyCallback callback)
    {
        if (!callback.performed)
            return;

        if (OwnerView == null)
            return;

        if (m_NextPageIndex != -1)
            return;

        int tabIndex = OwnerView.State.GetPageIndex();
        int tabCount = GetPageCount();
        tabIndex++;
        while (tabIndex < tabCount)
        {
            if (OwnerView.State.GetPageVisible(tabIndex))
                break;

            tabIndex++;
        }

        if (tabIndex >= tabCount)
        {
            WwiseUtil.PlaySound((int)WwiseMusic.Music_Button_SelectMove_invalid, false, null);
        }
        else
        {
            ChangePageIndex(tabIndex);
            WwiseUtil.PlaySound((int)WwiseMusic.Music_Button_SelectMove_valid, false, null);
        }
    }

    /// <summary>
    /// 清空列表
    /// </summary>
    private void ClearList()
    {
        Transform ButtonBox = FindComponent<Transform>("Content/ButtonBox");
        if (ButtonBox)
        {
            for (int i = 0; i < ButtonBox.childCount; i++)
            {
                Transform child = ButtonBox.GetChild(i);
                child.gameObject.SetActive(false);

                Toggle button = child.GetComponent<Toggle>();
                if (button)
                {
                    button.onValueChanged.RemoveAllListeners();
                    button.isOn = false;
                }
            }
        }
    }

    /// <summary>
    /// 更新列表
    /// </summary>
    private void UpdateList()
    {
        ClearList();

        Transform ButtonBox = FindComponent<Transform>("Content/ButtonBox");
        Transform template = FindComponent<Transform>("Content/ButtonBox/Button");
        if (ButtonBox && template)
        {
            CfgEternityProxy cfg = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;

            int index = 0;
            int count = GetPageCount();

            for (index = 0; index < count; index++)
            {
                //数据
                UiLabelConfig? pageCfg = cfg.GetUIPage((uint)Config.Value.LabelId(index));

                //视图
                Transform child = null;
                if (index < ButtonBox.childCount)
                    child = ButtonBox.GetChild(index);
                else
                    child = Object.Instantiate(template, ButtonBox);

                //更新显示
                child.gameObject.SetActive(OwnerView.State.GetPageVisible(index));

                UIIconAndLabel buttonInfo = child.GetComponent<UIIconAndLabel>();
                if (buttonInfo)
                {
                    if (buttonInfo.Icon)
                    {
                        if (pageCfg.HasValue && pageCfg.Value.Icon != 0)
                            UIUtil.SetIconImage(buttonInfo.Icon, (uint)pageCfg.Value.Icon);
                        else
                            buttonInfo.Icon.sprite = null;
                    }

                    if (buttonInfo.Label)
                        buttonInfo.Label.text = pageCfg.HasValue ? GetLocalization(pageCfg.Value.Name) : string.Empty;

                    if (buttonInfo.Info)
                        buttonInfo.Info.text = OwnerView.State.GetPageLabel(index);
                }

                //绑定事件
                Toggle button = child.GetComponent<Toggle>();
                if (button)
                {
                    int id = index;
                    button.navigation = new Navigation() { mode = Navigation.Mode.None };
                    button.onValueChanged.RemoveAllListeners();
                    button.onValueChanged.AddListener((selected) =>
                    {
                        if (button.animator != null)
                        {
                            button.animator.SetBool("IsOn", selected);
                            button.animator.SetTrigger("Normal");
                        }

                        if (selected && !m_SkipEventListener)
                        {
                            if (OwnerView != null)
                                ChangePageIndex(id);
                        }
                    });
                }

                //音效处理
                ClickSoundListener handler = child.GetComponent<ClickSoundListener>();
                if (!handler)
                    handler = child.GetOrAddComponent<ClickSoundListener>();
            }

            //关闭多余的按钮
            for (; index < ButtonBox.childCount; index++)
            {
                Transform child = ButtonBox.GetChild(index);
                child.gameObject.SetActive(false);

                Toggle button = child.GetComponent<Toggle>();
                if (button)
                {
                    button.onValueChanged.RemoveAllListeners();
                    button.isOn = false;
                }
            }
        }
    }

    /// <summary>
    /// 改变页面索引
    /// </summary>
    /// <param name="index">页索引</param>
    private void ChangePageIndex(int index, bool ignoreAnim = false)
    {
        int oldIndex = OwnerView.State.GetPageIndex();
        int newIndex = index;

        if (!ignoreAnim && OwnerView.AnimatorController && oldIndex != newIndex)
        {
            OwnerView.AnimatorController.Animator.SetTrigger(oldIndex < newIndex ? "Right" : "Left");
            OwnerView.AnimatorController.OnAnimationEvent -= OnAnimationEvent;
            OwnerView.AnimatorController.OnAnimationEvent += OnAnimationEvent;
            m_NextPageIndex = newIndex;
        }
        else
        {
            OwnerView.State.SetPageIndex(newIndex);
        }
    }


    /// <summary>
    /// 收到动画事件
    /// </summary>
    /// <param name="key">事件参数</param>
    private void OnAnimationEvent(string key)
    {
        if (string.Equals("UpdateList", key))
        {
            OwnerView.AnimatorController.OnAnimationEvent -= OnAnimationEvent;

            if (m_NextPageIndex != -1)
            {
                OwnerView.State.SetPageIndex(m_NextPageIndex);
                m_NextPageIndex = -1;
            }
        }
    }

    /// <summary>
    /// 点击事件处理
    /// </summary>
    private class ClickSoundListener : MonoBehaviour, IPointerUpHandler
    {
        /// <summary>
        /// 鼠标松开时
        /// </summary>
        /// <param name="eventData">事件数据</param>
        public void OnPointerUp(PointerEventData eventData)
        {
            WwiseUtil.PlaySound((int)WwiseMusic.Music_Button_Click_1, false, null);
        }
    }
}
