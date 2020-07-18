using Eternity.FlatBuffer;
using System.Collections.Generic;
using UnityEngine;
using static UIViewState;

public class UIHotkeyPart : BaseViewPart
{
    private const string ASSET_ADDRESS = Leyoutech.Core.Loader.Config.AssetAddressKey.PRELOADUI_COMMONHOTKEYPART;

    /// <summary>
    /// 已注册的所有热键
    /// </summary>
    private List<string> m_HotKeyIDs = new List<string>();

    /// <summary>
    /// 打开时
    /// </summary>
    /// <param name="msg"></param>
    public override void OnShow(object msg)
    {
        base.OnShow(msg);

        //ResetAllHotkeyState();

        OwnerView.State.OnPageIndexChanged -= OnPageIndexChange;
        OwnerView.State.OnPageIndexChanged += OnPageIndexChange;

        LoadViewPart(ASSET_ADDRESS, OwnerView.HotkeyBox);
    }

    public override void OnHide()
    {
        OwnerView.State.OnPageIndexChanged -= OnPageIndexChange;

        base.OnHide();
    }

    /// <summary>
    /// 获得焦点时重建所有热键
    /// </summary>
    public override void OnGotFocus()
    {
        if (OwnerView != null && GetTransform() != null)
        {
            UpdateAllHotkeyElements();
        }
    }

    /// <summary>
    /// 失去焦点时删除所有热键
    /// </summary>
    public override void OnLostFocus()
    {
        RemoveAllHotkeyElement();
        
        base.OnLostFocus();
    }

    /// <summary>
    /// 部件加载时
    /// </summary>
    protected override void OnViewPartLoaded()
    {
        OwnerView.State.OnActionBoxChanged -= OnHotkeyBoxChanged;
        OwnerView.State.OnActionVisibleChanged -= OnHotkeyVisibleChanged;
        OwnerView.State.OnActionEnableChanged -= OnHotkeyEnabledChanged;

        UpdateAllHotkeyElements();

        OwnerView.State.OnActionBoxChanged += OnHotkeyBoxChanged;
        OwnerView.State.OnActionVisibleChanged += OnHotkeyVisibleChanged;
        OwnerView.State.OnActionEnableChanged += OnHotkeyEnabledChanged;
    }

    /// <summary>
    /// 部件卸载时
    /// </summary>
    protected override void OnViewPartUnload()
    {
        OwnerView.State.OnActionBoxChanged -= OnHotkeyBoxChanged;
        OwnerView.State.OnActionVisibleChanged -= OnHotkeyVisibleChanged;
        OwnerView.State.OnActionEnableChanged -= OnHotkeyEnabledChanged;
        OwnerView.State.OnActionStateChanged -= OnHotkeyStateChanged;

        RemoveAllHotkeyElement();
    }

    /// <summary>
    /// 页码改变前
    /// </summary>
    /// <param name="oldIndex">旧索引</param>
    /// <param name="newIndex">新索引</param>
    private void OnPageIndexChange(int oldIndex, int newIndex)
    {
        //ResetAllHotkeyState();

        if (GetTransform())
            UpdateAllHotkeyElements();
    }

    /// <summary>
    /// 热键挂点改变时
    /// </summary>
    private void OnHotkeyBoxChanged()
    {
        if (OwnerView != null)
            UpdateAllHotkeyElements();
    }

    /// <summary>
    /// 热键可见性改变时
    /// </summary>
    /// <param name="id">id</param>
    /// <param name="isHold">是否为长按键</param>
    /// <param name="visible">是否可见</param>
    private void OnHotkeyVisibleChanged(string id, bool visible)
    {
        if (OwnerView != null)
            OwnerView.SetHotKeyVisible(id, visible);
    }

    /// <summary>
    /// 热键可用性改变时
    /// </summary>
    /// <param name="id">id</param>
    /// <param name="isHold">是否为长按键</param>
    /// <param name="enabled">是否可用</param>
    private void OnHotkeyEnabledChanged(string id, bool enabled)
    {
        if (OwnerView != null)
            OwnerView.SetHotKeyEnabled(id, enabled);
    }

    /// <summary>
    /// 热键状态改变时
    /// </summary>
    /// <param name="hotkeyID">热键ID</param>
    /// <param name="isHold">是否为长按</param>
    private void OnHotkeyStateChanged(string hotkeyID)
    {
        if (OwnerView != null)
            UpdateAllHotkeyElements();
    }

    #region 热键

    /// <summary>
    /// 重置所有热键状态
    /// </summary>
    private void ResetAllHotkeyState()
    {
        if (!Config.HasValue)
            return;

        int pageIndex = OwnerView.State.GetPageIndex();
        if (pageIndex < 0 || pageIndex >= Config.Value.LabelIdLength)
            return;

        CfgEternityProxy cfg = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;

        UiLabelConfig? pageCfg = cfg.GetUIPage((uint)Config.Value.LabelId(pageIndex));
        if (!pageCfg.HasValue)
            return;

        int count = pageCfg.Value.HotkeyLength;
        for (int i = 0; i < count; i++)
        {
            UiHotkeyConfig? hotkeyCfg = cfg.GetUIHotkey(pageCfg.Value.Hotkey(i));
            if (!hotkeyCfg.HasValue)
                continue;

            UIViewAction key = OwnerView.State.GetAction(hotkeyCfg.Value.Id);
            key.StateList.Clear();

            string hotkey = "";
            string text = "";
            float time = 0;
            int arg = 0;

            int stateCount = Mathf.Max(hotkeyCfg.Value.HotKeyLength, hotkeyCfg.Value.TimeLength, hotkeyCfg.Value.TextLength);
            for (int j = 0; j < stateCount; j++)
            {
                if (j < hotkeyCfg.Value.HotKeyLength)
                    hotkey = hotkeyCfg.Value.HotKey(j);
                if (j < hotkeyCfg.Value.TextLength)
                    text = GetLocalization(hotkeyCfg.Value.Text(j));
                if (j < hotkeyCfg.Value.TimeLength)
                    time = hotkeyCfg.Value.Time(j);
                if (j < hotkeyCfg.Value.ArgsLength)
                    arg = hotkeyCfg.Value.Args(j);

                key.StateList.Add(new UIViewActionState() { Hotkey = hotkey, Text = text, Time = time, Arg = arg });
            }

            key.State = hotkeyCfg.Value.NormalState;
        }
    }

    /// <summary>
    /// 删除所有热键
    /// </summary>
    private void RemoveAllHotkeyElement()
    {
        if (OwnerView != null)
        {
            foreach (string id in m_HotKeyIDs)
            {
                OwnerView.SetHotKeyEnabled(id, false);
                OwnerView.DeleteHotKey(id);
            }
        }

        m_HotKeyIDs.Clear();
    }

    /// <summary>
    /// 更新所有热键
    /// </summary>
    private void UpdateAllHotkeyElements()
    {
        RemoveAllHotkeyElement();

        if (!OwnerView.Focused)
            return;

        OwnerView.State.OnActionStateChanged -= OnHotkeyStateChanged;

        if (Config.HasValue)
        {
            int pageIndex = OwnerView.State.GetPageIndex();
            if (pageIndex < Config.Value.LabelIdLength)
            {
                CfgEternityProxy cfg = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;

                UiLabelConfig? pageCfg = cfg.GetUIPage((uint)Config.Value.LabelId(pageIndex));
                if (pageCfg.HasValue)
                {
                    int count = pageCfg.Value.HotkeyLength;
                    for (int i = 0; i < count; i++)
                    {
                        UiHotkeyConfig? hotkeyCfg = cfg.GetUIHotkey(pageCfg.Value.Hotkey(i));
                        if (!hotkeyCfg.HasValue)
                            continue;

                        UIViewAction key = OwnerView.State.GetAction(hotkeyCfg.Value.Id);
                        UpdateHotkeyElement(key);
                        m_HotKeyIDs.Add(key.ID);
                    }
                }
            }
        }

        if (m_HotKeyIDs.Count > 0)
        {
            OwnerView.State.OnActionStateChanged += OnHotkeyStateChanged;
        }
    }

    /// <summary>
    /// 更新单个热键
    /// </summary>
    /// <param name="key"></param>
    private void UpdateHotkeyElement(UIViewAction key)
    {
        if (key.State < 0 || key.State >= key.StateList.Count)
            return;

        Transform outer = OwnerView.State.GetActionBox();
        Transform parent = outer ? outer : FindComponent<Transform>("Content/HotkeyBox");

        UIViewActionState keyState = key.StateList[key.State];

        OwnerView.AddHotKey(key.ID, keyState.Hotkey, (callback) => { key.FireEvent(callback); }, keyState.Time, parent, keyState.Text);
        OwnerView.SetHotKeyVisible(key.ID, key.Visible);
        OwnerView.SetHotKeyEnabled(key.ID, key.Enabled);
        OwnerView.SetHotKeyDescription(key.ID, keyState.Text);
    }

    #endregion

    #region 热键状态


    /// <summary>
    /// 获取热键的状态列表
    /// </summary>
    /// <returns>状态列表</returns>
    private string[] GetHotkeyStates(UiHotkeyConfig hotkeyCfg)
    {
        /*
        if (hotkeyCfg.HotKey == HotKeyID.FuncL3 && hotkeyCfg.Time == 0)
        {
            //排序热键的状态
            if (Config.HasValue)
            {
                CfgEternityProxy cfg = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;

                int index = OwnerView.State.GetPageIndex();

                UiLabelConfig? pageConfig = cfg.GetUIPage((uint)Config.Value.LabelId(index));
                if (pageConfig.HasValue && pageConfig.Value.SortListLength > 0)
                {
                    string[] sortKinds = new string[pageConfig.Value.SortListLength];
                    for (int i = 0; i < sortKinds.Length; i++)
                    {
                        sortKinds[i] = GetSortText((UIViewSortKind)pageConfig.Value.SortList(i));
                    }
                    return sortKinds;
                }
                else
                {
                    //其它键的状态

                    string state1 = hotkeyCfg.HotKeyName1;
                    string state2 = hotkeyCfg.HotKeyName2;
                    string state3 = hotkeyCfg.HotKeyName3;
                    string state4 = hotkeyCfg.HotKeyName4;

                    if (!string.IsNullOrEmpty(state1))
                        state1 = GetLocalization(state1);
                    if (!string.IsNullOrEmpty(state2))
                        state2 = GetLocalization(state2);
                    if (!string.IsNullOrEmpty(state3))
                        state3 = GetLocalization(state3);
                    if (!string.IsNullOrEmpty(state4))
                        state4 = GetLocalization(state4);

                    return new string[] { state1, state2, state3, state4 };
                }
            }
        }
        else
        {
            //其它键的状态

            string state1 = hotkeyCfg.HotKeyName1;
            string state2 = hotkeyCfg.HotKeyName2;
            string state3 = hotkeyCfg.HotKeyName3;
            string state4 = hotkeyCfg.HotKeyName4;

            if (!string.IsNullOrEmpty(state1))
                state1 = GetLocalization(state1);
            if (!string.IsNullOrEmpty(state2))
                state2 = GetLocalization(state2);
            if (!string.IsNullOrEmpty(state3))
                state3 = GetLocalization(state3);
            if (!string.IsNullOrEmpty(state4))
                state4 = GetLocalization(state4);

            return new string[] { state1, state2, state3, state4 };
        }*/
        return new string[] { };
    }

    /// <summary>
    /// 取排序的多语言文字
    /// </summary>
    /// <param name="id">ID</param>
    /// <returns>文本</returns>
    private string GetSortText(UIViewSortKind id)
    {
        return GetLocalization("package_title_" + (id + 1022));
    }

    #endregion
}
