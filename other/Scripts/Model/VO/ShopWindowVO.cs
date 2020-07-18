using Eternity.FlatBuffer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 商店出售橱窗
/// </summary>
public class ShopWindowVO 
{
    /// <summary>
    /// 商品id
    /// </summary>
    public ulong Oid;
    /// <summary>
    /// 是否开启
    /// </summary>
    public byte IsOpen;    
    /// <summary>
	/// 商品config
	/// </summary>
	public ShopItemData? ShopItemConfig;
    /// <summary>
    /// 刷新时间
    /// </summary>
    public ulong RefreshTime;
    /// <summary>
    /// 全服剩余量
    /// </summary>
    public long ServerLeftNum;
    /// <summary>
    /// 个人限量剩余
    /// </summary>
    public long LimitCount;
    /// <summary>
    /// 道具Tid
    /// </summary>
    public uint Tid;
    /// <summary>
    /// 商品橱窗order
    /// </summary>
    public uint Order;
}
