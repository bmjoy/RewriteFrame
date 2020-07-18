using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIFilterPart : BaseViewPart
{
    private const string ASSET_ADDRESS = Leyoutech.Core.Loader.Config.AssetAddressKey.PRELOADUI_COMMONSORTPART;

    /// <summary>
    /// 动作后需入页码
    /// </summary>
    private int m_NextPageIndex;
    /// <summary>
    /// 动作后需要进入分类
    /// </summary>
    private int m_NextCategoryIndex;


    public override void OnShow(object msg)
    {
        base.OnShow(msg);

        m_NextPageIndex = -1;
        m_NextCategoryIndex = -1;

        LoadViewPart(ASSET_ADDRESS, OwnerView.SortBox);
    }

    public override void OnHide()
    {
        OwnerView.AnimatorController.OnAnimationEvent -= OnAnimationEvent;

        base.OnHide();
    }

    /// <summary>
    /// 部件加载完成时
    /// </summary>
    protected override void OnViewPartLoaded()
    {
        //X键
        State.GetAction(UIAction.ProductCategory).Callback -= OnXCallback;
        State.GetAction(UIAction.ProductCategory).Callback += OnXCallback;

        State.OnPageIndexChanged -= OnPageChanged;
        State.OnPageIndexChanged += OnPageChanged;
        State.OnCategoryIndexChanged -= OnFilterIndexChanged;
        State.OnCategoryIndexChanged += OnFilterIndexChanged;

        OnPageChanged(State.GetPageIndex(), State.GetPageIndex());
    }

    /// <summary>
    /// 部件卸载时
    /// </summary>
    protected override void OnViewPartUnload()
    {
        State.GetAction(UIAction.ProductCategory).Callback -= OnXCallback;

        State.OnPageIndexChanged -= OnPageChanged;
        State.OnCategoryIndexChanged -= OnFilterIndexChanged;

        RemoveAllCategory();

        base.OnViewPartUnload();
    }


    /// <summary>
    /// 当页面索引改变时
    /// </summary>
    /// <param name="oldIndex">旧索引</param>
    /// <param name="newIndex">新索引</param>
    private void OnPageChanged(int oldIndex, int newIndex)
    {
        UpdateList();

        SelectItem(State.GetPageCategoryIndex(State.GetPageIndex()));
    }

    /// <summary>
    /// 当过滤索引改变时
    /// </summary>
    /// <param name="oldIndex">旧索引</param>
    /// <param name="newIndex">新索引</param>
    private void OnFilterIndexChanged(int pageIndex, int oldIndex, int newIndex)
    {
        if (pageIndex == State.GetPageIndex())
            SelectItem(State.GetPageCategoryIndex(State.GetPageIndex()));
    }

    /// <summary>
    /// 收到X键回调
    /// </summary>
    /// <param name="callback">热键状态</param>
    private void OnXCallback(HotkeyCallback callback)
    {
        if (!callback.performed)
            return;

        if (OwnerView == null)
            return;

        if (m_NextCategoryIndex != -1)
            return;

        int index = State.GetPageCategoryIndex(State.GetPageIndex());
        UIViewCategory[] categorys = State.GetPage().Categorys;

        if (index >= categorys.Length - 1)
            index = 0;
        else
            index++;

        ChangeFilterIndex(index);
    }

    /// <summary>
    /// 选中指定项
    /// </summary>
    /// <param name="index">索引</param>
    private void SelectItem(int index)
    {
        UIViewCategory[] categorys = State.GetPage().Categorys;

        index = Mathf.Clamp(index, 0, categorys.Length - 1);
        if (index < 0)
            index = 0;

        Transform ButtonBox = FindComponent<Transform>("Content/ButtonBox");
        if (index < ButtonBox.childCount)
        {
            Transform child = ButtonBox.GetChild(index);
            if (child.gameObject.activeSelf)
            {
                Toggle button = child.GetComponent<Toggle>();
                if (button)
                {
                    button.isOn = true;

                    if (button.animator != null)
                    {
                        button.animator.SetBool("IsOn", true);
                        button.animator.SetTrigger("Normal");
                    }
                }
            }
        }
    }

    /// <summary>
    /// 删除所有分类
    /// </summary>
    private void RemoveAllCategory()
    {
        Transform ButtonBox = FindComponent<Transform>("Content/ButtonBox");

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

    /// <summary>
    /// 更新所有分类
    /// </summary>
    private void UpdateList()
    {
        Transform ButtonBox = FindComponent<Transform>("Content/ButtonBox");
        Transform template = FindComponent<Transform>("Content/ButtonBox/Button");

        if (!ButtonBox || !template)
            return;

        UIViewCategory[] categorys = State.GetPage().Categorys;

        int index = 0;
        int count = categorys != null ? categorys.Length : 0;

        for (index = 0; index < count; index++)
        {
            Transform child = null;
            if (index < ButtonBox.childCount)
                child = ButtonBox.GetChild(index);
            else
                child = UnityEngine.Object.Instantiate(template, ButtonBox);

            child.gameObject.SetActive(true);

            //更新显示
            UIIconAndLabel buttonInfo = child.GetComponent<UIIconAndLabel>();
            if (buttonInfo)
            {
                if (buttonInfo.Label)
                    buttonInfo.Label.text = categorys[index].Label;

                if (buttonInfo.Icon)
                    buttonInfo.Icon.sprite = null;
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

                    if (selected)
                    {
                        if (OwnerView != null)
                            ChangeFilterIndex(id);
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

    /// <summary>
    /// 改变分类索引
    /// </summary>
    /// <param name="categoryIndex">新索引</param>
    private void ChangeFilterIndex(int categoryIndex)
    {
        int pageIndex = State.GetPageIndex();

        int oldIndex = State.GetPageCategoryIndex(State.GetPageIndex());
        int newIndex = categoryIndex;

        if (OwnerView.AnimatorController && oldIndex != newIndex)
        {
            OwnerView.AnimatorController.Animator.SetTrigger(oldIndex < newIndex ? "Down" : "Up");
            OwnerView.AnimatorController.OnAnimationEvent -= OnAnimationEvent;
            OwnerView.AnimatorController.OnAnimationEvent += OnAnimationEvent;

            m_NextPageIndex = pageIndex;
            m_NextCategoryIndex = newIndex;
        }
        else
        {
            State.SetPageCategoryIndex(pageIndex, newIndex);
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

            if (m_NextPageIndex != -1 && m_NextCategoryIndex != -1 && State.GetPageIndex() == m_NextPageIndex)
            {
                State.SetPageCategoryIndex(m_NextPageIndex, m_NextCategoryIndex);

                m_NextPageIndex = -1;
                m_NextCategoryIndex = -1;
            }
        }
    }

    /// <summary>
    /// 点击事件处理
    /// </summary>
    private class ClickSoundListener : MonoBehaviour, IPointerUpHandler
    {
        public void OnPointerUp(PointerEventData eventData)
        {
            WwiseUtil.PlaySound((int)WwiseMusic.Music_Button_Click_1, false, null);
        }
    }
}
