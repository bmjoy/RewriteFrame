using Eternity.FlatBuffer;
using Eternity.Runtime.Item;
using System.Collections.Generic;

/// <summary>
/// 道具对象基类
/// </summary>
public class ItemObject
{
	/// <summary>
	/// 创建时间
	/// </summary>
	public ulong CreateTime;
	/// <summary>
	/// 第一层枚举
	/// </summary>
	public RootCategory RootType;
	/// <summary>
	/// 第二层枚举
	/// </summary>
	public Category MainType;
	/// <summary>
	/// 基础config
	/// </summary>
	public Item ItemConfig;
	/// <summary>
	/// 实例ID
	/// </summary>
	public ulong UID;
	/// <summary>
	/// 表ID
	/// </summary>
	public uint TID;
	/// <summary>
	/// 父节点UID
	/// </summary>
	public ulong ParentUID;
	/// <summary>
	/// 数量
	/// </summary>
	public long Count;
	/// <summary>
	/// 位置
	/// </summary>
	public int Position;
	/// <summary>
	/// 引用目标uid
	/// </summary>
	public ulong Reference;
	/// <summary>
	/// 副本uid
	/// </summary>
	public List<ulong> Replicas = new List<ulong>();
	/// <summary>
	/// 强化等级
	/// </summary>
	public int Lv;
}

/// <summary>
/// 道具基类
/// </summary>
public class ItemBase : ItemObject
{

}

/// <summary>
/// 容器
/// </summary>
public class ItemContainer : ItemBase
{
	public string ContainerType;
	/// <summary>
	/// 当前容量上限
	/// </summary>
	public uint CurrentSizeMax;
	/// <summary>
	/// 道具(含包)的列表
	/// </summary>
	public Dictionary<ulong, ItemBase> Items;
}