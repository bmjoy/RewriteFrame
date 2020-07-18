using Eternity.FlatBuffer;
using UnityEngine;
/// <summary>
/// 蓝图生产界面上的信息  (包括接收服务器的蓝图信息)
/// </summary>
public class ProduceInfoVO
{
	/// <summary>
	/// 实例ID
	/// </summary>
	public ulong UID;
	/// <summary>
	///  蓝图ID
	/// </summary>
	public int TID;

	/// <summary>
	/// 产品的Item
	/// </summary>
	public Item MItem;

	/// <summary>
	/// 蓝图的Item
	/// </summary>
	public Item MProduce;

	/// <summary>
	/// 名字
	/// </summary>
	public string Name;

	/// <summary>
	/// 蓝图生产状态    0,不可生产，1 生产中，2可生产 3 生产完成
	/// </summary>
	public ProduceState BluePrintState;

	/// <summary>
	/// 当前生产进度
	/// </summary>
	public float Progress = 0;

	/// <summary>
	/// Icon上挂的脚本
	/// </summary>
	public ProduceElelment Elelment = null;

	/// <summary>
	/// 类型
	/// </summary>
	public int RelatedType;

	/// <summary>
	/// 开始时间
	/// </summary>
	public ulong StartTime;

	/// <summary>
	/// 结束时间
	/// </summary>
	public ulong EndTime;

	/// <summary>
	/// 持续时间
	/// </summary>
	public ulong SpendTime;

	/// <summary>
	/// 是否激活
	/// </summary>
	public bool Active;

	/// <summary>
	/// 页面上所在的位置
	/// </summary>
	public Vector2Int PosIndex;

	public bool Finished {
		get
		{
			return StartTime + SpendTime >= EndTime;
		}
	}

}
