using Assets.Scripts.Lib.Net;
using Assets.Scripts.Proto;
using Game.Frame.Net;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopController : AbsRpcController
{
    /// <summary>
    /// 商店Proxy
    /// </summary>
    private ShopProxy m_ShopProxy;
    /// <summary>
    /// 道具Proxy
    /// </summary>
    private CfgEternityProxy m_CfgEternityProxy;
    /// <summary>
    /// 背包Proxy
    /// </summary>
    private PackageProxy m_PackageProxy;
    private ShopProxy GetShopProxy()
    {
        if (m_ShopProxy == null)
        {
            m_ShopProxy = GameFacade.Instance.RetrieveProxy(ProxyName.ShopProxy) as ShopProxy;
        }
        return m_ShopProxy;
    }

    private CfgEternityProxy GetCfgEternityProxy()
    {
        if (m_CfgEternityProxy == null)
        {
            m_CfgEternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
        }
        return m_CfgEternityProxy;
    }

    private PackageProxy GetPackageProxy()
    {
        if (m_PackageProxy == null)
        {
            m_PackageProxy = GameFacade.Instance.RetrieveProxy(ProxyName.PackageProxy) as PackageProxy;
        }
        return m_PackageProxy;
    }
    public ShopController() : base()
    {
        ListenServerMessage();
    }
    private void ListenServerMessage()
    {
        NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_sync_special_buy_record, OnShopBuyRecord, typeof(S2C_SYNC_SPECIAL_BUY_RECORD));
        NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_sync_shop_info, OnShopWindowInfoSync, typeof(S2C_SYNC_SHOP_INFO));
        NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_exchange_result, OnExchangeBack, typeof(S2C_EXCHANGE_RESULT));
        NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_sync_buy_back, OnShopSellBackInfoSync, typeof(S2C_SYNC_BUY_BACK));
    }

    #region S2C
    /// <summary>
    /// 同步商品购买记录
    /// </summary>
    /// <param name="buf"></param>
    public void OnShopBuyRecord(KProtoBuf buf)
    {
        GetShopProxy().ClearBuyRecord();
        S2C_SYNC_SPECIAL_BUY_RECORD msg = (S2C_SYNC_SPECIAL_BUY_RECORD)buf;
        for (int i = 0; i < msg.goods_buy_list.Count; i++)
        {
            GoodsBuyInfo buyInfo = msg.goods_buy_list[i];
            GetShopProxy().AddBuyRecord(buyInfo.oid, buyInfo.num);
        }
        GameFacade.Instance.SendNotification(NotificationName.MSG_SHOP_CHANGE);
    }
    /// <summary>
    /// 同步商店橱窗信息
    /// </summary>
    /// <param name="buf"></param>
    public void OnShopWindowInfoSync(KProtoBuf buf)
    {
        GetShopProxy().ClearShopWindow();
        S2C_SYNC_SHOP_INFO msg = (S2C_SYNC_SHOP_INFO)buf;
        GetShopProxy().SetShopId(msg.shop_id);
        GetShopProxy().SetRefreshTime(msg.refresh_time);
        for (int i = 0; i < msg.goods_list.Count; i++)
        {
            GoodsInfo info = msg.goods_list[i];
            ShopWindowVO shopWindowVO = new ShopWindowVO();
            shopWindowVO.Oid = info.oid;
            shopWindowVO.Tid = (uint)info.item_id;            
            shopWindowVO.IsOpen = info.is_open;
            shopWindowVO.ShopItemConfig = GetCfgEternityProxy().GetShopData((uint)info.goods_id, (uint)info.item_id);
            shopWindowVO.RefreshTime = info.refresh_time;
            shopWindowVO.ServerLeftNum = info.server_left_num;
            shopWindowVO.LimitCount = shopWindowVO.ShopItemConfig.Value.IndividualLimitNum;
            shopWindowVO.Order = info.order;
            GetShopProxy().AddShopWindowData(info.oid, shopWindowVO);
        }
        GameFacade.Instance.SendNotification(NotificationName.MSG_SHOP_CHANGE);
    }
    
    /// <summary>
    /// 同步商店回购信息
    /// </summary>
    /// <param name="buf"></param>
    public void OnShopSellBackInfoSync(KProtoBuf buf)
    {
        GetShopProxy().ClearSellBack();
        S2C_SYNC_BUY_BACK msg = (S2C_SYNC_BUY_BACK)buf;
        for (int i = 0; i < msg.goods_saled_info.Count; i++)
        {
            GoodsSaledInfo info = msg.goods_saled_info[i];
            ShopSellBackVO shopSellBackVO = new ShopSellBackVO();
            shopSellBackVO.Uid = info.uid;
            shopSellBackVO.Tid = info.item_id;
            shopSellBackVO.Num = info.num;
            shopSellBackVO.ShopId = info.shop_id;
            shopSellBackVO.ExpireTime = info.expire_time;
            shopSellBackVO.ItemConfig = GetCfgEternityProxy().GetItemByKey((uint)info.item_id);
            GetShopProxy().AddSellBackData(info.uid, shopSellBackVO);
        }
        GameFacade.Instance.SendNotification(NotificationName.MSG_SHOP_CHANGE);
    }
    /// <summary>
    /// 交易返回
    /// </summary>
    /// <param name="buf"></param>
    public void OnExchangeBack(KProtoBuf buf)
    {
        S2C_EXCHANGE_RESULT msg = (S2C_EXCHANGE_RESULT)buf;
        if (msg.result == 0)
        {
            switch (msg.op_code)
            {
                case 0:
                    //GetShopProxy().ChangeSellCount(msg.uid, msg.num);
                    break;
                case 1:
                    //GetPackageProxy().ChangeStackCount(msg.uid, msg.num);
                    break;
                case 2:
                    //GetShopProxy().ChangeSellBackCount(msg.uid, msg.num);
                    break;
            }
            GameFacade.Instance.SendNotification(NotificationName.MSG_SHOP_EXCHANGE, msg.op_code);
        }
        else
        {
            Debug.LogError("交易失败");
        }
    }
    #endregion

    #region C2S
    /// <summary>
    /// 请求商店橱窗信息
    /// </summary>
    /// <param name="shopId">商店id</param>
    public void RequestShopWindowInfo(uint shopId)
    {
        C2S_SHOP_INFO req = SingleInstanceCache.GetInstanceByType<C2S_SHOP_INFO>();
        req.protocolID = (ushort)KC2S_Protocol.c2s_shop_info;
        req.shop_id = shopId;
        NetworkManager.Instance.SendToGameServer(req);
    }
    /// <summary>
    /// 请求回购信息
    /// </summary>
    /// <param name="shopId">商店id</param>
    public void RequestShopSellBackInfo(uint shopId)
    {
        C2S_BUY_BACK_INFO req = SingleInstanceCache.GetInstanceByType<C2S_BUY_BACK_INFO>();
        req.protocolID = (ushort)KC2S_Protocol.c2s_buy_back_info;
        req.shop_id = shopId;
        NetworkManager.Instance.SendToGameServer(req);
    }
    /// <summary>
    /// 请求交易
    /// </summary>
    /// <param name="type">交易类型</param>
    /// <param name="shopId">商店id</param>
    /// <param name="itemId">道具id</param>
    /// <param name="id">物品id</param>
    /// <param name="num">交易数量</param>
    public void RequestExChange(byte type, uint shopId, ulong itemId, ulong id, uint num)
    {
        C2S_REQUEST_EXCHANGE req = SingleInstanceCache.GetInstanceByType<C2S_REQUEST_EXCHANGE>();
        req.protocolID = (ushort)KC2S_Protocol.c2s_request_exchange;
        req.op_code = type;
        req.shop_id = shopId;
        req.item_id = itemId;
        req.uid = id;
        req.num = num;
        NetworkManager.Instance.SendToGameServer(req);
    }
    #endregion
}
