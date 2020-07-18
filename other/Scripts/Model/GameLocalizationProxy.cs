using Eternity.FlatBuffer;
using I2.Loc;
using PureMVC.Patterns.Proxy;
using UnityEngine;
using UnityEngine.Assertions;

public class GameLocalizationProxy : Proxy, ILocalizationParamsManager
{
    #region 全局多语言参数处理

    /// <summary>
    /// 构造函数
    /// </summary>
    public GameLocalizationProxy() : base(ProxyName.GameLocalizationProxy)
    {
        if (!LocalizationManager.ParamManagers.Contains(this))
        {
            LocalizationManager.ParamManagers.Add(this);
            LocalizationManager.LocalizeAll(true);
        }
    }

    /// <summary>
    /// 处理多语言里的参数
    /// </summary>
    /// <param name="Param">参数ID</param>
    /// <returns>字符串</returns>
    public string GetParameterValue(string Param)
    {
        return "??";
    }

    #endregion


    #region 获取本地化语言

    /// <summary>
    /// 获取本地化字符
    /// </summary>
    /// <param name="Term">id</param>
    /// <param name="applyParameters">是否要解析参数</param>
    /// <param name="localParametersRoot">局部参数根节点</param>
    /// <param name="FixForRTL">是否修正R2L的布局</param>
    /// <param name="maxLineLengthForRTL">R2L单行的字数</param>
    /// <param name="ignoreRTLnumbers">忽略R2L中的数字</param>
    /// <param name="overrideLanguage"></param>
    /// <returns>本地化后的文字</returns>
    public string GetString(string Term, bool applyParameters = false, GameObject localParametersRoot = null,bool FixForRTL = true, int maxLineLengthForRTL = 0, bool ignoreRTLnumbers = true,  string overrideLanguage = null)
    {
        //从UI的语言表中取
        string text = GetStringFromUI(Term, applyParameters, localParametersRoot, FixForRTL, maxLineLengthForRTL, ignoreRTLnumbers, overrideLanguage);

        //从CFG的语言表中取
        if (text == null)
            text = GetStringFromCfg(Term, applyParameters, localParametersRoot, FixForRTL, maxLineLengthForRTL, ignoreRTLnumbers, overrideLanguage);

        //从Map配置语言表取
        if(text==null)
            text = GetStringFromMapCfg(Term, applyParameters, localParametersRoot, FixForRTL, maxLineLengthForRTL, ignoreRTLnumbers, overrideLanguage);

        return text;
    }

    /// <summary>
    /// 从UI的语言表中取
    /// </summary>
    /// <param name="Term">id</param>
    /// <param name="applyParameters">是否要解析参数</param>
    /// <param name="localParametersRoot">局部参数根节点</param>
    /// <param name="FixForRTL">是否修正R2L的布局</param>
    /// <param name="maxLineLengthForRTL">R2L单行的字数</param>
    /// <param name="ignoreRTLnumbers">忽略R2L中的数字</param>
    /// <param name="overrideLanguage"></param>
    /// <returns>本地化后的文字</returns>
    public string GetStringFromUI(string Term, bool applyParameters = false, GameObject localParametersRoot = null, bool FixForRTL = true, int maxLineLengthForRTL = 0, bool ignoreRTLnumbers = true, string overrideLanguage = null)
    {
        return LocalizationManager.GetTranslation(Term, FixForRTL, maxLineLengthForRTL, ignoreRTLnumbers, applyParameters, localParametersRoot, overrideLanguage);
    }

    /// <summary>
    /// 从配置表中获取字符串
    /// </summary>
    /// <param name="Term">id</param>
    /// <param name="applyParameters">是否要解析参数</param>
    /// <param name="localParametersRoot">局部参数根节点</param>
    /// <param name="FixForRTL">是否修正R2L的布局</param>
    /// <param name="maxLineLengthForRTL">R2L单行的字数</param>
    /// <param name="ignoreRTLnumbers">忽略R2L中的数字</param>
    /// <param name="overrideLanguage"></param>
    /// <returns>本地化后的文字</returns>
    public string GetStringFromCfg(string Term, bool applyParameters = false, GameObject localParametersRoot = null, bool FixForRTL = true, int maxLineLengthForRTL = 0, bool ignoreRTLnumbers = true, string overrideLanguage = null)
    {
        CfgEternityProxy proxy = Facade.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;

        Language? language = proxy.GetLanguage(Term);

        string text = null;
        if (language.HasValue)
        {
            string currentLanguage = string.IsNullOrEmpty(overrideLanguage) ? LocalizationManager.CurrentLanguage : overrideLanguage;
            switch (currentLanguage)
            {
                case "Chinese": text = language.Value.Chs;break;
                case "English": text = language.Value.EnUs;break;
                default: text = language.Value.EnUs;break;
            }

            if (text != null)
            {
                if (applyParameters)
                {
                    LocalizationManager.ApplyLocalizationParams(ref text, localParametersRoot);
                }

                if (LocalizationManager.IsRight2Left && FixForRTL)
                {
                    text = LocalizationManager.ApplyRTLfix(text, maxLineLengthForRTL, ignoreRTLnumbers);
                }
            }
        }

        return text;
    }
    /// <summary>
    /// 从配置表中获取字符串
    /// </summary>
    /// <param name="Term">id</param>
    /// <param name="applyParameters">是否要解析参数</param>
    /// <param name="localParametersRoot">局部参数根节点</param>
    /// <param name="FixForRTL">是否修正R2L的布局</param>
    /// <param name="maxLineLengthForRTL">R2L单行的字数</param>
    /// <param name="ignoreRTLnumbers">忽略R2L中的数字</param>
    /// <param name="overrideLanguage"></param>
    /// <returns>本地化后的文字</returns>
    public string GetStringFromMapCfg(string Term, bool applyParameters = false, GameObject localParametersRoot = null, bool FixForRTL = true, int maxLineLengthForRTL = 0, bool ignoreRTLnumbers = true, string overrideLanguage = null)
    {
        CfgEternityProxy proxy = Facade.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;

        LanguageMapEditor? language = proxy.GetLanguageByMap(Term);

        string text = null;
        if (language.HasValue)
        {
            string currentLanguage = string.IsNullOrEmpty(overrideLanguage) ? LocalizationManager.CurrentLanguage : overrideLanguage;
            switch (currentLanguage)
            {
                case "Chinese": text = language.Value.Chs; break;
                case "English": text = language.Value.EnUs; break;
                default: text = language.Value.EnUs; break;
            }

            if (text != null)
            {
                if (applyParameters)
                {
                    LocalizationManager.ApplyLocalizationParams(ref text, localParametersRoot);
                }

                if (LocalizationManager.IsRight2Left && FixForRTL)
                {
                    text = LocalizationManager.ApplyRTLfix(text, maxLineLengthForRTL, ignoreRTLnumbers);
                }
            }
        }

        return text;
    }

    #endregion
}