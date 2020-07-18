using FlatBuffers;
using PureMVC.Patterns.Proxy;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

// reviewed by william 2019.4.30

public class CfgSettingProxy : Proxy
{
    private const string ASSET_ADDRESS = "Assets/Config/data/setting.bytes";

    //private Setting m_Config;

    public CfgSettingProxy() : base(ProxyName.CfgSettingProxy)
    {
        DataManager.Instance.LoadDataToProxy(ASSET_ADDRESS, this);
    }


    public override void InitData(ByteBuffer buffer)
    {
        //m_Config = Setting.GetRootAsSetting(buffer);
    }
	/*
	
    public SetVO GetSetVOByKey(int tid)
    {
        SetVO? setVO = m_Config.Set.Value.DataByKey(tid);
		Assert.IsTrue(setVO.HasValue, "CfgSettingProxy => GetSetVOByKey not exist tid " + tid);
        return setVO.Value;
    }
	
    /// <summary>/*
    /// 根据Order获取不同面板的内容
    /// </summary>
    /// <param name="type">类型</param>
    /// <returns></returns>
    public List<SetVO> GetOrderSetVOList(SetType type)
    {
        List<SetVO> settings = new List<SetVO>();
		int count = m_Config.Set.Value.DataLength;
        for (int i = 0; i < count; i++)
        {
            SetVO setVO = m_Config.Set.Value.Data(i).GetValueOrDefault();
            if (setVO.Type == (int)type)
            {
                settings.Add(setVO);
            }
        }
        settings.Sort(SetingVOSortByOrder);
        return settings;
    }

    /// <summary>
    /// 排序通过Order
    /// </summary>
    /// <param name="x">变量x</param>
    /// <param name="y">变量y</param>
    /// <returns></returns>
    private int SetingVOSortByOrder(SetVO x, SetVO y)
    {
        return x.Order - y.Order;
    }

    /// <summary>
    /// 获取名称
    /// </summary>
    /// <param name="language">系统语言</param>
    /// <param name="key">语言ID</param>
    /// <returns></returns>
    public string GetLocalization(SystemLanguage language, int key)
    {
        LocalizationVO? value = m_Config.Localization.Value.DataByKey(key);
        if (value.HasValue)
        {
            if (language == SystemLanguage.English)
            {
                return value.Value.EnUs;
            }
            else
            {
                return value.Value.ZhCn;
            }
        }
        return "";
    }
	*/
}