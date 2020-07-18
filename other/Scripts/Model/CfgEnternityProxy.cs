using Eternity.FlatBuffer;
using FlatBuffers;
using PureMVC.Patterns.Proxy;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public partial class CfgEternityProxy : Proxy
{

    private const string ASSET_ADDRESS = "Assets/Config/data/eternity.bytes";

    private Global m_Config;

    public CfgEternityProxy() : base(ProxyName.CfgEternityProxy)
    {
        DataManager.Instance.LoadDataToProxy(ASSET_ADDRESS, this);
    }

    public override void InitData(ByteBuffer buffer)
    {
        m_Config = Global.GetRootAsGlobal(buffer);
        effect_data.EffectData.getEffect = GetEffect;
    }

    public List<effect_data.Effect> GetEffect(uint eid)
    {
        EffectElement?[] effectElements = GetEffectElementsByKey(eid);
        var eff = new List<effect_data.Effect>();

        if (effectElements.Length > 0)
        {
            for (int i = 0; i < effectElements.Length; i++)
            {
                EffectElement? item = effectElements[i];
                EffectElement effectElement = item.Value;
                eff.Add(new effect_data.Effect((int)effectElement.Function, (int)effectElement.Attribute, effectElement.Value, effectElement.PipeLv));
            }
        }

        return eff;
    }

    /// <summary>
    /// 根据itemId获取NPC combat基础数据
    /// </summary>
    /// <param name="tid"></param>
    /// <returns></returns>
    public NpcCombat GetNpcCombatByKey(uint tid)
    {
        NpcCombat? npcVO = m_Config.NpcCombatsByKey(tid);
        Assert.IsTrue(npcVO.HasValue, "CfgEternityProxy => GetNpcCombatByKey not exist tid " + tid);
        return npcVO.Value;
    }

    /// <summary>
    /// 战舰装配用的 mod 在ui上的位置数据
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public ModPosition GetModPosition(uint id)
    {
        ModPosition? modPosition = m_Config.ModPositionsByKey(id);
        Assert.IsTrue(modPosition.HasValue, "CfgEternityProxy => GetModPosition not exist tid " + id);
        return modPosition.Value;
    }

    /// <summary>
    /// 通过id获取图标数据
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public Icon GetIconName(uint id)
    {
        Icon? iconVO = m_Config.IconsByKey(id);
        Assert.IsTrue(iconVO.HasValue, "CfgEternityProxy => IconsByKey not exist tid " + id);
        return iconVO.Value;
    }

    public Location? GetLocationByKey(ulong tid)
    {
        Location? location = m_Config.LocationsByKey(tid);
        //Assert.IsTrue(location.HasValue, "CfgEternityProxy => GetLocationByKey not exist tid: " + tid);
        return location;
    }

    /// <summary>
    /// 获取多语言
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public Language? GetLanguage(string key)
    {
        Language? language = m_Config.LanguagesByKey(key);
        return language;
    }

    /// <summary>
    /// 地图多语言
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public LanguageMapEditor? GetLanguageByMap(string key)
    {
        LanguageMapEditor? language = m_Config.LanguageMapEditorsByKey(key);
        return language;
    }

    /// <summary>
    /// 获取船
    /// </summary>
    /// <param name="tid"></param>
    /// <returns></returns>
    public Warship GetWarshipByKey(uint tid)
    {
        Warship? warship = m_Config.WarshipsByKey(tid);
        Assert.IsTrue(warship.HasValue, "CfgEternityProxy => GetWarship not exist tid: " + tid);
        return warship.Value;
    }

    /// <summary>
    /// 获取全局数据
    /// </summary>
    /// <param name="tid"></param>
    /// <returns></returns>
    public GamingConfig? GetGamingConfig(uint tid)
    {
        GamingConfig? gamingConfig = m_Config.GamingConfigsByKey(tid);
        Assert.IsTrue(gamingConfig.HasValue, "CfgEternityProxy => GetGamingConfig not exist tid: " + tid);
        return gamingConfig;
    }

    /// <summary>
    /// 获取所有材料数据（原来旧的现在没用上）
    /// </summary>
    /// <returns></returns>
    public Eternity.FlatBuffer.Material?[] GetMatericals()
    {
        List<Eternity.FlatBuffer.Material?> list = new List<Eternity.FlatBuffer.Material?>();
        for (int i = 0; i < m_Config.MaterialsLength; i++)
        {
            Eternity.FlatBuffer.Material? material = m_Config.Materials(i);
            list.Add(material);
        }
        return list.ToArray();
    }

    public bool IsSpace()
    {
        return m_CurrentMapData.PathType != 1;
    }

    /// <summary>
    /// 获取消耗item
    /// </summary>
    /// <param name="tid">tid</param>
    /// <returns></returns>
    public EffectElement?[] GetEffectElementsByKey(uint tid)
    {
        Effect? effectVO = m_Config.EffectsByKey(tid);
        Assert.IsTrue(effectVO.HasValue, "CfgEternityProxy => EffectsByKey not exist tid " + tid);
        EffectElement?[] effectElements = new EffectElement?[effectVO.Value.ElementsLength];
        for (int i = 0; i < effectVO.Value.ElementsLength; i++)
        {
            effectElements[i] = effectVO.Value.Elements(i);
        }
        return effectElements;
    }

    /// <summary>
    /// 根据id和战舰级别获取唯一一列数据
    /// </summary>
    /// <param name="modelId"> tid</param>
    /// <param name="gradeId"> 战舰级别</param>
    /// <returns></returns>
    public PackageBoxAttr? GetPackageBoxAttrByModelIdAndGrade(uint modelId, uint gradeId)
    {
        PackageBox? packagebox = m_Config.PackageBoxsByKey(modelId);
        Assert.IsTrue(packagebox.HasValue, "CfgEternityProxy => PackageBox not exist tid " + modelId);
        int n = packagebox.Value.AttrsLength;
        for (int i = 0; i < n; ++i)
        {
            if (packagebox.Value.Attrs(i).Value.NpcGrade == gradeId)
                return packagebox.Value.Attrs(i).Value;
        }

        return null;
    }
    /// <summary>
    /// 获取品质data
    /// </summary>
    /// <param name="tid"></param>
    /// <returns></returns>
    public PackageBoxQuality? GetPackageBoxQualityByKey(uint tid)
    {
        PackageBoxQuality? pkbq = m_Config.PackageBoxQualitysByKey(tid);
        Assert.IsTrue(pkbq.HasValue, "CfgEternityProxy => PackageBoxQuality not exist tid " + tid);
        return pkbq;
    }

    public string GetLanguageStringByEnum<T>(T e, SystemLanguage language = SystemLanguage.English)
    {
        LanguageEnum? data = m_Config.LanguageEnumsByKey(e.GetType().FullName + "." + e);
        if (data.HasValue)
        {
            switch (language)
            {
                case SystemLanguage.English:
                    return data.Value.EnUs;
                case SystemLanguage.Chinese:
                    return data.Value.Chs;
            }
        }
        return e.ToString();
    }

	public uint GetIconIDByEnum<T>(T e)
	{
		LanguageEnum? data = m_Config.LanguageEnumsByKey(e.GetType().FullName + "." + e);
		return data.HasValue ? data.Value.Icon : 0;
	}

	/// <summary>
	/// 获取对话
	/// </summary>
	/// <param name="tid"></param>
	/// <returns></returns>
	public Dialogue? GetDialogueByKey(uint tid)
    {
        Dialogue? data = m_Config.DialoguesByKey(tid);
        Assert.IsTrue(data.HasValue, "CfgEternityProxy => GetDialogueByKey not exist tid " + tid);
        return data;
    }

	/// <summary>
	/// 获取相机参数
	/// </summary>
	/// <param name="id"></param>
	/// <returns></returns>
	public WarshipCamera? GetWarshipCamera(uint id)
    {
        return m_Config.WarshipCamerasByKey(id).Value;
    }

	public WarshipCameraDofFour GetWarshipCameraDof4(uint id)
	{
		WarshipCameraDofFour? warshipCameraDofFour = m_Config.WarshipCameraDofFoursByKey(id);
		Assert.IsTrue(warshipCameraDofFour.HasValue, "CfgEternityProxy => WarshipCameraDofFoursByKey not exist tid " + id);
		return warshipCameraDofFour.Value;
	}

	public WarshipCameraDofSix GetWarshipCameraDof6(uint id)
	{
		WarshipCameraDofSix? warshipCameraDofSix = m_Config.WarshipCameraDofSixsByKey(id);
		Assert.IsTrue(warshipCameraDofSix.HasValue, "CfgEternityProxy => WarshipCameraDofSixsByKey not exist tid " + id);
		return warshipCameraDofSix.Value;
	}

	/// <summary>
	/// 获取商品橱窗数据
	/// </summary>
	/// <param name="tid"></param>
	/// <returns></returns>
	public ShopWindow? GetShopWindowByKey(uint tid)
    {
        ShopWindow? shopWindow = m_Config.ShopWindowsByKey(tid).Value;
        Assert.IsTrue(shopWindow.HasValue, "CfgEternityProxy => GetShopWindowByKey not exist tid " + tid);
        return shopWindow;
    }
    /// <summary>
    /// 获取商品道具数据
    /// </summary>
    /// <param name="id"></param>
    /// <param name="itemId"></param>
    /// <returns></returns>
    public ShopItemData? GetShopData(uint id, uint itemId)
    {
        ShopItem? shopItem = m_Config.ShopItemsByKey(id).Value;
        for (int i = 0; i < shopItem.Value.ItemGoodLength; i++)
        {
            ShopItemData? shopItemData = shopItem.Value.ItemGood(i);
            if (shopItemData.Value.ItemGood.Value.Id == itemId)
            {
                return shopItemData;
            }
        }
        return null;
    }
    public Shop? GetShopById(uint shopId)
    {
        return m_Config.ShopsByKey(shopId);
    }
    /// <summary>
    /// 获取组id相同的通讯列表
    /// </summary>
    /// <param name="groupId">组id</param>
    /// <returns></returns>
    public List<VideoPhone?> GetVideoPhoneList(int groupId)
    {
        List<VideoPhone?> videoElements = new List<VideoPhone?>();
        for (int i = 0; i < m_Config.VideoPhonesLength; i++)
        {
            VideoPhone? videoPhones = m_Config.VideoPhones(i);
            if (videoPhones.Value.Group == groupId)
            {
                videoElements.Add(videoPhones);
            }
        }
        return videoElements;
    }    

    public PlotBehavior PlotBehaviorsByKey(uint id)
    {
        PlotBehavior? plot = m_Config.PlotBehaviorsByKey(id);
        Assert.IsTrue(plot.HasValue, "CfgEternityProxy => PlotBehaviorsByKey not exist tid " + id);
        return plot.Value;
    }
    /// <summary>
    /// 手表升级数据
    /// </summary>
    /// <param name="id">等级</param>
    /// <returns></returns>
    public LevelUpa GetPlayerUpa(uint id)
    {
        LevelUpa? levelUpa = m_Config.LevelUpasByKey(id);
        Assert.IsTrue(levelUpa.HasValue, "CfgEternityProxy => GetPlayerExp not exist tid " + levelUpa);
        return levelUpa.Value;
    }    
    /// <summary>
    /// Debug用的接口
    /// </summary>
    internal Global _GetGlobal()
    {
        return m_Config;
    }
}