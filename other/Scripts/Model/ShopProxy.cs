using Eternity.FlatBuffer;
using PureMVC.Patterns.Proxy;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;

public class ShopProxy : Proxy
{
    /// <summary>
    /// 打开的商店id
    /// </summary>
    private uint m_ShopId;
    /// <summary>
    /// 商店刷新时间
    /// </summary>
    private ulong m_RefreshTime;
    /// <summary>
    /// CfgEternityProxy
    /// </summary>
    private CfgEternityProxy m_CfgEternityProxy;

    private CfgEternityProxy GetCfgEternityProxy()
    {
        if (m_CfgEternityProxy == null)
        {
            m_CfgEternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
        }
        return m_CfgEternityProxy;
    }
    /// <summary>
    /// 背包Proxy
    /// </summary>
    private PackageProxy m_PackageProxy;
    private PackageProxy GetPackageProxy()
    {
        if (m_PackageProxy == null)
        {
            m_PackageProxy = GameFacade.Instance.RetrieveProxy(ProxyName.PackageProxy) as PackageProxy;
        }
        return m_PackageProxy;
    }
    /// <summary>
    /// 购买数据
    /// </summary>
    private Dictionary<ulong, ShopWindowVO> m_ShopWindowData = new Dictionary<ulong, ShopWindowVO>();

    /// <summary>
    /// 回购数据
    /// </summary>
    private Dictionary<ulong, ShopSellBackVO> m_SellBackData = new Dictionary<ulong, ShopSellBackVO>();

    /// <summary>
    /// 商店购买记录
    /// </summary>
    private Dictionary<ulong, uint> m_BuyRecordData = new Dictionary<ulong, uint>();

    public ShopProxy() : base(ProxyName.ShopProxy)
    {

    }
    /// <summary>
    ///添加商品橱窗数据及购买剩余量
    /// </summary>
    /// <param name="oid"></param>
    /// <param name="shopWindowVO"></param>
    public void AddShopWindowData(ulong oid, ShopWindowVO shopWindowVO)
    {
        m_ShopWindowData.Add(oid, shopWindowVO);
        uint num;
        if (m_BuyRecordData.TryGetValue(oid, out num))
        {
            if (shopWindowVO.ShopItemConfig.Value.IndividualLimitNum >= 0)
            {
                shopWindowVO.LimitCount = shopWindowVO.ShopItemConfig.Value.IndividualLimitNum - num;
            }
            if (shopWindowVO.LimitCount == 0)
            {
                shopWindowVO.IsOpen = 0;
            }
        }
    }
    /// <summary>
    /// 添加购买记录
    /// </summary>
    /// <param name="oid"></param>
    /// <param name="num"></param>
    public void AddBuyRecord(ulong oid, uint num)
    {
        m_BuyRecordData.Add(oid, num);
        ShopWindowVO shopWindowVO;
        if (m_ShopWindowData.TryGetValue(oid, out shopWindowVO))
        {
            if (shopWindowVO.ShopItemConfig.Value.IndividualLimitNum >= 0)
            {
                shopWindowVO.LimitCount = shopWindowVO.ShopItemConfig.Value.IndividualLimitNum - num;
            }
            if (shopWindowVO.LimitCount == 0)
            {
                shopWindowVO.IsOpen = 0;
            }
        }
    }
    /// <summary>
    /// 添加回购数据
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="sellBackVO"></param>
    public void AddSellBackData(ulong uid, ShopSellBackVO sellBackVO)
    {
        m_SellBackData.Add(uid, sellBackVO);
    }
    /// <summary>
    /// 清除商店橱窗
    /// </summary>
    public void ClearShopWindow()
    {
        m_ShopWindowData.Clear();
    }
    /// <summary>
    /// 清除回购数据
    /// </summary>
    public void ClearSellBack()
    {
        m_SellBackData.Clear();
    }
    /// <summary>
    /// 清除购买记录
    /// </summary>
    public void ClearBuyRecord()
    {
        m_BuyRecordData.Clear();
    }
    /// <summary>
    /// 修改售出道具数量
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="count"></param>
    public void ChangeSellCount(ulong uid, long count)
    {
        Assert.IsTrue(m_ShopWindowData.TryGetValue(uid, out ShopWindowVO shopWindowVO), "ShopProxy => ChangeSellCount not exist id " + uid);
        if (m_ShopWindowData[uid].LimitCount > 0)
        {
            m_ShopWindowData[uid].LimitCount = m_ShopWindowData[uid].LimitCount - count;
        }
        if (m_ShopWindowData[uid].LimitCount == 0)
        {
            m_ShopWindowData[uid].IsOpen = 0;
        }
    }

    /// <summary>
    /// 售卖数据集合
    /// </summary>
    /// <returns></returns>
    public ShopWindowVO[] GetSellData()
    {
        return m_ShopWindowData.Values.ToArray();
    }
    /// <summary>
    /// 回购数据集合
    /// </summary>
    /// <returns></returns>
    public ShopSellBackVO[] GetSellBackData()
    {
        return m_SellBackData.Values.ToArray();
    }   
    /// <summary>
    /// 设置商店Id
    /// </summary>
    /// <param name="shopId"></param>
    public void SetShopId(uint shopId)
    {
        m_ShopId = shopId;
    }
    /// <summary>
    /// 获取商店Id
    /// </summary>
    /// <returns></returns>
    public uint GetShopId()
    {
        return m_ShopId;
    }
    /// <summary>
    /// 设置商店刷新时间
    /// </summary>
    /// <param name="refreshTime"></param>
    public void SetRefreshTime(ulong refreshTime)
    {
        m_RefreshTime = refreshTime;
    }
    /// <summary>
    /// 获取商店刷新时间
    /// </summary>
    /// <returns></returns>
    public ulong GetRefreshTime()
    {
        return m_RefreshTime;
    }
}
