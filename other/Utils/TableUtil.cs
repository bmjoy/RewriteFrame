/*===============================
 * Purpose: 表格相关，公共函数
 * Time: 2019/4/9 17:20:13
================================*/
using Assets.Scripts.Define;
using PureMVC.Patterns.Facade;
using UnityEngine;
using Eternity.Runtime.Item;
using System;
using System.Linq;
using System.Collections.Generic;
using static GameConstant.ItemConst;
using static GameConstant;

public static class TableUtil
{

    public static string GetLanguageString<T>(T e) where T : Enum
    {
        CfgEternityProxy proxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
        return proxy.GetLanguageStringByEnum(e);
    }

    /// <summary>
    /// 获取多语言表内容
    /// </summary>
    /// <param name="key"></param>
    /// <param name="applyParameters">是否要解析参数</param>
    /// <returns></returns>
    public static string GetLanguageString(string key, bool applyParameters = false)
    {
        GameLocalizationProxy proxy = GameFacade.Instance.RetrieveProxy(ProxyName.GameLocalizationProxy) as GameLocalizationProxy;
        string s = proxy.GetString(key, applyParameters);
        if (string.IsNullOrEmpty(s))
        {
            return key;
        }
        return s;
    }

    /// <summary>
    /// 获取多语言表内容
    /// </summary>
    /// <param name="id">language 表内 index</param>
    /// <param name="language">语言类型</param>

    public static string GetLanguageString(int id, SystemLanguage language = SystemLanguage.English)
    {
        CfgLanguageProxy itemListProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgLanguageProxy) as CfgLanguageProxy;
        return itemListProxy.GetLocalization(id, language);
    }


    public static string GetItemName(uint itemID, SystemLanguage language = SystemLanguage.English)
    {
        return GetItemName((int)itemID, language);
    }

    /// <summary>
    /// 获取物品名称
    /// </summary>
    /// <param name="itemType">物品类型</param>
    /// <param name="itemID">配置表ID</param>
    /// <returns></returns>
    public static string GetItemName(int itemID, SystemLanguage language = SystemLanguage.English)
    {
        string key = "item_name_{0}";
        GameLocalizationProxy gameLocalizationProxy = GameFacade.Instance.RetrieveProxy(ProxyName.GameLocalizationProxy) as GameLocalizationProxy;
        if (language == SystemLanguage.English)
        {
            return gameLocalizationProxy.GetStringFromCfg(string.Format(key, itemID));
        }
        else
        {
            return gameLocalizationProxy.GetStringFromCfg(string.Format(key, itemID));
        }
    }

    /// <summary>
    /// 获取物品名称
    /// </summary>
    /// <param name="_key">表名_name_{0}</param>
    /// <param name="itemType">物品类型</param>
    /// <param name="itemID">配置表ID</param>
    /// <returns></returns>
    public static string GetItemName(string _key, int itemID, SystemLanguage language = SystemLanguage.English)
    {
        string key = _key;
        GameLocalizationProxy gameLocalizationProxy = GameFacade.Instance.RetrieveProxy(ProxyName.GameLocalizationProxy) as GameLocalizationProxy;
        if (language == SystemLanguage.English)
        {
            return gameLocalizationProxy.GetStringFromCfg(string.Format(key, itemID));
        }
        else
        {
            return gameLocalizationProxy.GetStringFromCfg(string.Format(key, itemID));
        }
    }

    /// <summary>
    /// 获取NPC名称
    /// </summary>
    /// <param name="npcID">npc id</param>
    /// <returns>文本</returns>
    public static string GetNpcName(uint npcID)
    {
        return (GameFacade.Instance.RetrieveProxy(ProxyName.GameLocalizationProxy) as GameLocalizationProxy).GetStringFromCfg("npc_name_" + npcID);
    }
    /// <summary>
    /// 获取NPC描述
    /// </summary>
    /// <param name="npcID">npc id</param>
    /// <returns>文本</returns>
    public static string GetNpcDesc(uint npcID)
    {
        return (GameFacade.Instance.RetrieveProxy(ProxyName.GameLocalizationProxy) as GameLocalizationProxy).GetStringFromCfg("npc_npc_intro_" + npcID);
    }
    /// <summary>
    /// 获取任务目标描述
    /// </summary>
    /// <param name="id">id</param>
    /// <returns>文本</returns>
    public static string GetMissionTargetDesc(uint id)
    {
        return (GameFacade.Instance.RetrieveProxy(ProxyName.GameLocalizationProxy) as GameLocalizationProxy).GetStringFromCfg("mission_target_desc_" + id);
    }

    /// <summary>
    /// 获取物品名称（次函数只针对钱币）
    /// </summary>
    /// <param name="itemType">物品类型</param>
    /// <param name="itemID">配置表ID</param>
    /// <returns></returns>
    public static string GetItemName(KNumMoneyType moneyType)
    {
        switch (moneyType)
        {
            case KNumMoneyType.money1:
                return GetItemName(1100004);
            case KNumMoneyType.money2:
                return GetItemName(1000001);
            default:
                Debug.LogError("货币 映射无效");
                return "";
        }
    }

    /// <summary>
    /// 获取物品Icon  长图
    /// </summary>
    /// <param name="itemType">物品类型</param>
    /// <param name="itemID">配置表ID</param>
    /// <returns></returns>
    public static string GetItemIconImage(uint itemID)
    {
        CfgEternityProxy cfgEternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
        uint iconId = cfgEternityProxy.GetItemByKey(itemID).Icon;
        return cfgEternityProxy.GetIconName(iconId).AssetName;
    }
    /// <summary>
	/// 获取物品Icon  方图
	/// </summary>
	/// <param name="itemType">物品类型</param>
	/// <param name="itemID">配置表ID</param>
	/// <returns></returns>
	public static string GetItemSquareIconImage(uint itemID)
    {
        CfgEternityProxy cfgEternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
        uint iconId = cfgEternityProxy.GetItemByKey(itemID).Icon;
        return cfgEternityProxy.GetIconName(iconId).SquareName;
    }
    /// <summary>
    /// 获取物品Icon Tid
    /// </summary>
    /// <param name="itemID">配置表ID</param>
    /// <returns></returns>
    public static uint GetItemIconTid(uint itemID)
    {
        CfgEternityProxy cfgEternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
        uint iconId = cfgEternityProxy.GetItemByKey(itemID).Icon;
        return iconId;
    }

    /// <summary>
    /// 获取物品Icon  图片（次函数只针对钱币）
    /// </summary>
    /// <param name="itemType">物品类型</param>
    /// <param name="itemID">配置表ID</param>
    /// <returns></returns>
    public static string GetItemIconImage(KNumMoneyType moneyType)
    {
        switch (moneyType)
        {
            case KNumMoneyType.money1:
                return GetItemIconImage(1100004);
            case KNumMoneyType.money2:
                return GetItemIconImage(1000001);
            default:
                Debug.LogError("货币 映射无效");
                return "";
        }
    }

    /// <summary>
    /// 通过IconID 获取Icon的图集名称 
    /// </summary>
    /// <param name="iconID"></param>
    /// <returns></returns>
    public static string GetIconBundle(uint iconID)
    {
        CfgEternityProxy cfgEternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
        return cfgEternityProxy.GetIconName(iconID).Atlas;
    }

    /// <summary>
    /// 通过IconID 获取Icon的资源名称 
    /// </summary>
    /// <param name="iconID"></param>
    /// <returns></returns>
    public static string GetIconAsset(uint iconID)
    {
        CfgEternityProxy cfgEternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
        return cfgEternityProxy.GetIconName(iconID).AssetName;
    }

    /// <summary>
    /// 获取物品Bundle
    /// </summary>
    /// <param name="itemType">物品类型</param>
    /// <param name="itemID">配置表ID</param>
    /// <returns></returns>
    public static string GetItemIconBundle(uint itemID)
    {
        CfgEternityProxy cfgEternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
        uint iconId = cfgEternityProxy.GetItemByKey(itemID).Icon;
        return cfgEternityProxy.GetIconName(iconId).Atlas;
    }

    /// <summary>
    /// 获取天赋的名字
    /// </summary>
    /// <param name="key">key</param>
    /// <param name="enUs">是否是英文</param>
    /// <returns></returns>
    public static string GetTalentName(string key, bool enUs = true)
    {
        CfgEternityProxy cfgEternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
        return cfgEternityProxy.GetTalentName(key);
    }

    /// <summary>
    /// 获取物品Bundle（次函数只针对钱币）
    /// </summary>
    /// <param name="itemType">物品类型</param>
    /// <param name="itemID">配置表ID</param>
    /// <returns></returns>
    public static string GetItemIconBundle(KNumMoneyType moneyType)
    {

        switch (moneyType)
        {
            case KNumMoneyType.money1:
                return GetItemIconBundle(1100004);
            case KNumMoneyType.money2:
                return GetItemIconBundle(1000001);
            default:
                Debug.LogError("货币 映射无效");
                return "";
        }
    }

    public static string GetItemDescribe(uint itemID, SystemLanguage language = SystemLanguage.English)
    {
        return GetItemDescribe((int)itemID, language);
    }

    /// <summary>
    /// 获取物品描述
    /// </summary>
    /// <param name="itemType">物品类型</param>
    /// <param name="itemID">配置表ID</param>
    /// <returns></returns>
    public static string GetItemDescribe(int itemID, SystemLanguage language = SystemLanguage.English)
    {
        string key = "item_desc_{0}";
        GameLocalizationProxy gameLocalizationProxy = GameFacade.Instance.RetrieveProxy(ProxyName.GameLocalizationProxy) as GameLocalizationProxy;
        if (language == SystemLanguage.English)
        {
            return gameLocalizationProxy.GetStringFromCfg(string.Format(key, itemID));
        }
        else
        {
            return gameLocalizationProxy.GetStringFromCfg(string.Format(key, itemID));
        }
    }

    /// <summary>
    /// 获取天赋描述
    /// </summary>
    /// <param name="itemType">物品类型</param>
    /// <param name="itemID">配置表ID</param>
    /// <returns></returns>
    public static string GetTalentDescribe(int itemID, int level,SystemLanguage language = SystemLanguage.English)
    {
        string key = "talent_node_fun_desc_{0}";
        GameLocalizationProxy gameLocalizationProxy = GameFacade.Instance.RetrieveProxy(ProxyName.GameLocalizationProxy) as GameLocalizationProxy;
        if (language == SystemLanguage.English)
        {
            return gameLocalizationProxy.GetStringFromCfg(string.Format(key, itemID)+"_"+level.ToString());
        }
        else
        {
            return gameLocalizationProxy.GetStringFromCfg(string.Format(key, itemID)+"_" + level.ToString());
        }
    }

    /// <summary>
    /// 获取天赋的名字
    /// </summary>
    /// <param name="tid">天赋tid</param>
    /// <returns></returns>
    public static string GetTalentName(int tid)
    {
        return GetTalentName("talent_name_" + tid);
    }

    /// <summary>
    /// 获取天赋的名字
    /// </summary>
    /// <param name="tid">天赋tid</param>
    /// <returns></returns>
    public static string GetTalentNodeName(int tid)
    {
        return GetTalentName("talent_node_name_" + tid);
    }
    /// <summary>
    /// 获取物品描述（次函数只针对钱币）
    /// </summary>
    /// <param name="itemType">物品类型</param>
    /// <param name="itemID">配置表ID</param>
    /// <returns></returns>
    public static string GetItemDescribe(KNumMoneyType moneyType)
    {
        switch (moneyType)
        {
            case KNumMoneyType.money1:
                return GetItemDescribe(1100004);
            case KNumMoneyType.money2:
                return GetItemDescribe(1000001);
            default:
                return "";
        }
    }

    public static int GetItemQuality(uint itemID)
    {
        return GetItemQuality((int)itemID);
    }

    /// <summary>
    /// 获取物品品质
    /// </summary>
    /// <param name="itemID">配置表ID</param>
    /// <returns></returns>
    public static int GetItemQuality(int itemID)
    {
        CfgEternityProxy cfgEternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
        return cfgEternityProxy.GetItemByKey((uint)itemID).Quality;
    }

    /// <summary>
    /// 获取物品使用等级
    /// </summary>
    /// <param name="itemID">配置表ID</param>
    /// <returns></returns>
    public static int GetItemRoleLevel(uint itemID)
    {
        CfgEternityProxy cfgEternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
        return cfgEternityProxy.GetItemByKey(itemID).PlayerLvLimit;
    }

    /// <summary>
    /// 获取物品售卖价格
    /// </summary>
    /// <param name="itemID"></param>
    /// <returns></returns>
    public static ulong GetItemSellPrice(uint itemID)
    {
        CfgEternityProxy cfgEternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
        return cfgEternityProxy.GetItemByKey(itemID).MoneyPrice;
    }
    /// <summary>
    /// 获取物品Order
    /// </summary>
    /// <param name="itemID"></param>
    /// <returns></returns>
    public static int GetItemOrder(uint itemID)
    {
        CfgEternityProxy cfgEternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
        return cfgEternityProxy.GetItemByKey(itemID).Order;
    }

    /// <summary>
    /// 获取物品T级
    /// </summary>
    /// <param name="itemID"></param>
    /// <returns></returns>
    public static int GetItemGrade(uint itemID)
    {
        CfgEternityProxy cfgEternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
        return cfgEternityProxy.GetItemByKey(itemID).Grade;
    }
    /// <summary>
    /// 获取物品名称（次函数只针对钱币）
    /// </summary>
    /// <param name="itemType">物品类型</param>
    /// <param name="itemID">配置表ID</param>
    /// <returns></returns>
    public static int GetItemQuality(KNumMoneyType moneyType)
    {
        switch (moneyType)
        {
            case KNumMoneyType.money1:
                return GetItemQuality(1100004);
            case KNumMoneyType.money2:
                return GetItemQuality(1000001);
            default:
                return 1;
        }
    }
    /// <summary>
    /// 显示物品等级
    /// </summary>
    /// <param name="level"></param>
    /// <returns></returns>
    public static string ShowLevel(int level)
    {
        int index = 0;
        char[] charArray = level.ToString().PadLeft(3, '0').ToCharArray();
        for (int i = 0; i < charArray.Length; i++)
        {
            char cc = charArray[i];
            if (cc != '0')
            {
                index = i;
                break;
            }
        }
        string sstr = level.ToString().PadLeft(3, '0');
        if (index != 0)
        {
            sstr = sstr.Insert(index, "</color>");
            sstr = sstr.Insert(0, "<color=#808080>");
        }
        return sstr;
    }

    #region ItemSort
    /// <summary>
    /// 道具排序
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items">道具列表</param>
    /// <param name="sortType">排序主要类型</param>
    /// <returns></returns>
    public static T[] SortItem<T>(T[] items, ItemSortType sortType) where T : ItemBase
    {
        return SortItemByType(items, sortType).ToArray();
    }

    /// <summary>
    /// 道具排序
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items">道具列表</param>
    /// <param name="sortType">排序主要类型</param>
    /// <returns></returns>
    public static List<T> SortItem<T>(List<T> items, ItemSortType sortType) where T : ItemBase
    {
        return SortItemByType(items, sortType).ToList();
    }

    #region 排序操作
    private static IOrderedEnumerable<T> SortItemByType<T>(IEnumerable<T> items, ItemSortType sortType) where T : ItemBase
    {
        List<ItemSortType> sortList = ItemDefSort.ToList();
        sortList.Remove(sortType);
        sortList.Insert(0, sortType);
        return SortByEnum(items, sortList.ToArray());
    }

    private static IOrderedEnumerable<T> SortByEnum<T>(IEnumerable<T> items, params ItemSortType[] types) where T : ItemBase
    {
        IOrderedEnumerable<T> order = items.OrderBy((x) => 0);
        for (int i = 0; i < types.Length; i++)
        {
            switch (types[i])
            {
                case ItemSortType.Default://降 品质
                    order = order.ThenByDescending((x) => x.ItemConfig.Quality);
                    break;
                case ItemSortType.Tonnage://升 T级
                    order = order.ThenBy((x) => x.ItemConfig.Grade);
                    break;
                case ItemSortType.Enchant://降 强化等级
                    order = order.ThenByDescending((x) => x.Lv);
                    break;
                case ItemSortType.Role://降 玩家使用等级
                    order = order.ThenByDescending((x) => x.ItemConfig.PlayerLvLimit);
                    break;
                case ItemSortType.Alphabetical://升 道具名
                    order = order.ThenBy((x) => GetItemName(x.TID));
                    break;
                case ItemSortType.Selling://降 售卖价格
                    order = order.ThenByDescending((x) => x.ItemConfig.MoneyPrice);
                    break;
                case ItemSortType.Newest://降 道具生成时间
                    order = order.ThenByDescending((x) => x.CreateTime);
                    break;
                case ItemSortType.Order://降 Order
                    order = order.ThenByDescending((x) => x.ItemConfig.Order);
                    break;
                case ItemSortType.Tid://升 表TID
                    order = order.ThenBy((x) => x.TID);
                    break;
            }
        }
        return order;
    }
    #endregion
    #endregion

    #region ShipSort
    /// <summary>
    /// 战舰排序
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items">道具列表</param>
    /// <param name="sortType">排序主要类型</param>
    /// <returns></returns>
    public static T[] SortShip<T>(T[] items, ItemSortType sortType) where T : IShip
    {
        return SortShipByType(items, sortType).ToArray();
    }

    /// <summary>
    /// 战舰排序
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items">道具列表</param>
    /// <param name="sortType">排序主要类型</param>
    /// <returns></returns>
    public static List<T> SortShip<T>(List<T> items, ItemSortType sortType) where T : IShip
    {
        return SortShipByType(items, sortType).ToList();
    }

    private static IOrderedEnumerable<T> SortShipByType<T>(IEnumerable<T> items, ItemSortType sortType) where T : IShip
    {
        List<ItemSortType> sortList = ShipDefSort.ToList();
        sortList.Remove(sortType);
        sortList.Insert(0, sortType);
        return SortShipByEnum(items, sortList.ToArray());
    }

    private static IOrderedEnumerable<T> SortShipByEnum<T>(IEnumerable<T> items, params ItemSortType[] types) where T : IShip
    {
        IOrderedEnumerable<T> order = items.OrderBy((x) => 0);
        for (int i = 0; i < types.Length; i++)
        {
            switch (types[i])
            {
                case ItemSortType.Tonnage://升 T级
                    order = order.ThenBy((x) => x.GetBaseConfig().Grade);
                    break;
                case ItemSortType.Enchant://降 等级
                    order = order.ThenByDescending((x) => x.GetLv());
                    break;
                case ItemSortType.Alphabetical://升 道具名
                    order = order.ThenBy((x) => GetItemName(x.GetTID()));
                    break;
                case ItemSortType.Newest://降 道具生成时间
                    order = order.ThenByDescending((x) => x.GetCreatTime());
                    break;
                case ItemSortType.Order:  //降Order
                    order = order.ThenByDescending((x) => x.GetBaseConfig().Order);
                    break;
                case ItemSortType.Tid://升 表TID
                    order = order.ThenBy((x) => x.GetTID());
                    break;
            }
        }
        return order;
    }
    #endregion
}

