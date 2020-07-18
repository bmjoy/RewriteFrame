using Eternity.FlatBuffer;
using PureMVC.Patterns.Proxy;

public partial class CfgEternityProxy : Proxy
{
    /// <summary>
    /// 获取交互键设置
    /// </summary>
    /// <returns></returns>
    public InterAction? GetInteractiveKey()
    {
        return m_Config.InterActionsByKey("npcInteractive");
    }

    /// <summary>
    /// 获取UI配置
    /// </summary>
    /// <param name="id">面板ID</param>
    /// <returns>UI配置</returns>

    public UiConfig? GetUIConfig(uint id)
    {
        return m_Config.UiConfigsByKey(id);
    }

    /// <summary>
    /// UI页面配置
    /// </summary>
    /// <param name="id">页面ID</param>
    /// <returns>页面配置</returns>
    public UiLabelConfig? GetUIPage(uint id)
    {
        return m_Config.UiLabelConfigsByKey(id);
    }

    /// <summary>
    /// 获取子分类配置
    /// </summary>
    /// <param name="id">分类ID</param>
    /// <returns>分类配置</returns>
    public UiCategoryConfig? GetUICategory(uint id)
    {
        return m_Config.UiCategoryConfigsByKey(id);
    }

    /// <summary>
    /// UI热键配置
    /// </summary>
    /// <param name="id">热键ID</param>
    /// <returns>热键配置</returns>
    public UiHotkeyConfig? GetUIHotkey(string id)
    {
        return m_Config.UiHotkeyConfigsByKey(id);
    }
}
