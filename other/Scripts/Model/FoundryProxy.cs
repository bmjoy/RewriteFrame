/*===============================
 * Author: [dinghuilin]
 * Purpose: FoundryProxy.cs
 * Time: 2019/03/21  16:01
================================*/
using Eternity.FlatBuffer;
using Eternity.Runtime.Item;
using PureMVC.Patterns.Proxy;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 生产服务器数据
/// </summary>
public class FoundryProxy : Proxy
{
	//蓝图有两个存储容器，一个是服务器上生产的，一个是全部的   
	/// <summary>
	/// EternityProxy
	/// </summary>
	private CfgEternityProxy m_CfgEternityProxy;

    /// <summary>
    /// 背包数据
    /// </summary>
    private PackageProxy m_PackageProxy;

    /// <summary>
    /// 战舰包数据
    /// </summary>
    private ShipProxy m_ShipProxy;

    /// <summary>
    /// 外部访问蓝图容器  (包括处理过生产状态，此类型所有的蓝图)
    /// </summary>
    private Dictionary<int, ProduceInfoVO> m_BluePrintDic = new Dictionary<int, ProduceInfoVO>();
	
	/// <summary>
	/// 服务器生产数据字典  (只包括生产中或生产完成未领取的蓝图)
	/// </summary>
	private Dictionary<int, ProduceInfoVO> m_ServerBluePrintDic = new Dictionary<int, ProduceInfoVO>();

	/// <summary>
	/// 是否添加监听
	/// </summary>
	private bool addedListener;

	/// <summary>
	/// 配置表里所有的蓝图
	/// </summary>
	private List<Produce> AllProduceList = new List<Produce>();

	public FoundryProxy() : base(ProxyName.FoundryProxy)
	{
		m_CfgEternityProxy = Facade.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
	}

	/// <summary>
	/// 外部访问蓝图容器(包括所有的蓝图)
	/// </summary>
	/// <returns></returns>
	public Dictionary<int, ProduceInfoVO> GetBluePrintDic()
	{
		return m_BluePrintDic;
	}

	/// <summary>
	/// 外部访问蓝图容器(包括服务器蓝图)
	/// </summary>
	/// <returns></returns>
	public Dictionary<int, ProduceInfoVO> GetServerBluePrintDic()
	{
		return m_ServerBluePrintDic;
	}

	/// <summary>
	/// 获取蓝图信息
	/// </summary>
	/// <param name="tid">tid</param>
	/// <returns></returns>
	public Produce GetProduceByKey(int tid)
	{
		return m_CfgEternityProxy.GetProduceByKey((uint)tid);
	}

	/// <summary>
	/// 根据蓝图Tid获取产品Item
	/// </summary>
	/// <param name="tid">tid</param>
	/// <returns></returns>
	public Item GetItemByProduceKey(int tid)
	{
		return GetItemByKey((uint)GetProduceByKey(tid).ProductId);
	}

    /// <summary>
    /// 根据蓝图Tid获取产品ItemGrad
    /// </summary>
    /// <param name="tid">蓝图tid</param>
    /// <returns></returns>
    public int GetItemGradByProduceKey(int tid)
	{
		return GetItemByProduceKey(tid).Grade;
	}

	/// <summary>
	/// 获取消耗数据根据EffectTid
	/// </summary>
	/// <param name="tid">Effect Tid</param>
	/// <returns></returns>
	public EffectElement?[] GetEffectElementsByKey(int tid)
	{
		return m_CfgEternityProxy.GetEffectElementsByKey((uint)tid);
	}

	/// <summary>
	/// 获取消耗数据根据蓝图Tid
	/// </summary>
	/// <param name="tid">蓝图Tid</param>
	/// <returns></returns>
	public EffectElement?[] GetEffectElementsByProduceTid(int tid)
	{
		return m_CfgEternityProxy.GetEffectElementsByKey((uint)GetProduceByKey(tid).Cost);
	}

	/// <summary>
	/// 根据生产tid 获取消耗道具
	/// </summary>
	/// <param name="tid">ProduceTid</param>
	/// <returns></returns>
	public Item[] GetEffectItem(int tid)
	{
		EffectElement?[] effects = GetEffectElementsByProduceTid(tid);
		Item[] items = new Item[effects.Length];
		for (int i = 0; i < effects.Length; i++)
		{
			items[i] = GetItemByKey((uint)effects[i].Value.ItemId);
		}
		return items;
	}

	/// <summary>
	/// 获取生产总数据长度
	/// </summary>
	/// <returns></returns>
	public int GetProduceDataLength()
	{
		return m_CfgEternityProxy.GetProduceDataLength();
	}
	
	/// <summary>
	/// 通过配置表获取所有的蓝图信息
	/// </summary>
	public void GetAllDataByTable()
	{
		for (int i = 0; i < GetProduceDataLength(); i++)
		{
			if (m_CfgEternityProxy.GetProducesByIndex(i) !=null )
			{
				if (!AllProduceList.Contains(m_CfgEternityProxy.GetProducesByIndex(i).Value))
				{
					AllProduceList.Add(m_CfgEternityProxy.GetProducesByIndex(i).Value);
				}
			}
		}
	}

	/// <summary>
	/// 根据蓝图类型获取蓝图数据
	/// </summary>
	/// <param name="type">类型</param>
	/// <returns></returns>
	public List<Produce> GetDataByMainType(BlueprintL1 type)
	{
		BlueprintL1 blueprintL1 = new BlueprintL1();
		List<Produce> list = new List<Produce>();
		for (int i = 0; i < AllProduceList.Count; i++)
		{
			Item item = GetItemByKey(AllProduceList[i].Id);
			ItemType itemMainType = ItemTypeUtil.GetItemType(item.Type);//主类型
			ItemTypeUtil.SetSubType(ref blueprintL1, itemMainType);//次类型
			if (blueprintL1 == type)
			{
				if (!list.Contains(AllProduceList[i]))
				{
                    if (GetItemGradByProduceKey((int)AllProduceList[i].Id) < 4)//临时约束战舰T等级小于4
                    {
                        list.Add(AllProduceList[i]);
                    }
                }
			}
		}
		return list;
	}


    /// <summary>
    /// 按ItemType查找道具
    /// </summary>
    /// <param name="itemType">道具类型</param>
    /// <param name="prefixMode">是否使用前缀模式比较</param>
    /// <returns>道具列表</returns>
    public void FindItemArrayByItemType(ItemType itemType, bool prefixMode = true, int grad = 0, bool includeT = true)
    {

        for (int i = 0; i < AllProduceList.Count; i++)
        {
            Item itemProduce = GetItemByProduceKey((int)AllProduceList[i].Id);//产品
            Item item = GetItemByKey(AllProduceList[i].Id);//蓝图
            if (itemProduce.Type == itemType.NativeType)
            {
                if (!includeT)
                {
                    SetProduceState((int)item.Id);
                }
                else
                {
                    if (itemProduce.Grade == grad)
                    {
                        SetProduceState((int)item.Id);
                    }
                }

                continue;
            }

            if (prefixMode)
            {
                ItemType currType = ItemTypeUtil.GetItemType(itemProduce.Type);
                bool equal = true;
                for (int j = 0; j < itemType.EnumList.Length; j++)
                {
                    if (!includeT)
                    {
                        if (!Equals(itemType.EnumList[j], currType.EnumList[j]))
                        {
                            equal = false;
                            break;
                        }
                    }
                    else
                    {
                        if (!Equals(itemType.EnumList[j], currType.EnumList[j]))
                        {
                            equal = false;
                            break;
                        }
                        else
                        {
                            if (itemProduce.Grade != grad)
                            {
                                equal = false;
                                break;
                            }
                        }
                    }

                }

                if (equal)
                {
                    SetProduceState((int)item.Id);
                }
            }
        }

    }

    /// <summary>
    /// 战舰初始化
    /// </summary>
    public void InitShipPackage()
    {
        if (m_ShipProxy == null)
        {
            m_ShipProxy = GameFacade.Instance.RetrieveProxy(ProxyName.ShipProxy) as ShipProxy;
        }
        m_ShipProxy.InitShipPackage();
    }

    /// <summary>
    /// 根据TID获取船包里是否存在
    /// </summary>
    /// <param tid="Tid"></param>
    /// <returns></returns>
    private bool IsHaveShipByTid(uint tid)
    {
        if (m_ShipProxy==null)
        {
            m_ShipProxy = GameFacade.Instance.RetrieveProxy(ProxyName.ShipProxy) as ShipProxy;
        }
        Dictionary<ulong, IShip>  shipDic= m_ShipProxy.GetShipPackage();
        bool isHave = false;
        if (shipDic != null && shipDic.Count > 0)
        {
            foreach (var item in shipDic.Values)
            {
                if (item.GetTID()==tid)
                {
                    isHave = true;
                    break;
                }
            }
        }
        return isHave;
    }

    /// <summary>
    /// 获取道具信息
    /// </summary>
    /// <param name="tid">tid</param>
    /// <returns></returns>
    public Item GetItemByKey(uint tid)
	{
		return m_CfgEternityProxy.GetItemByKey(tid);
	}

	/// <summary>
	/// 添加服务器生产中或生产完成的数据
	/// </summary>
	/// <param name="tid">蓝图id</param>
	/// <param name="startTime">开始时间</param>
	/// <param name="endTime">结束时间</param>
	/// <param name="spendTime">花费时间</param>
	/// <param name="isFinish">是否完成</param>
	public void AddMember(int tid, ulong startTime, ulong endTime, ulong spendTime, bool isFinish)
	{
		ProduceInfoVO item = new ProduceInfoVO();
		item.TID = tid;
		item.StartTime = startTime;
		item.EndTime = endTime;
		item.SpendTime = spendTime;
		item.MItem = GetItemByProduceKey(tid);
		item.MProduce = GetItemByKey((uint)tid);
		//item.RelatedType = GetCfgItemListProxy().GetDrawingVOByKey(tid).RelatedType;

		if (!m_ServerBluePrintDic.ContainsKey(tid))
		{
			m_ServerBluePrintDic.Add(tid, item);
		}
		if (isFinish)
		{
			item.SpendTime = item.EndTime - item.StartTime;
		}
		AddListener();
	}

	/// <summary>
	/// 通过蓝图ID获取蓝图数据
	/// </summary>
	/// <param name="tid">蓝图ID</param>
	/// <returns></returns>
	public ProduceInfoVO GetFoundryMemberByTID(int tid)
	{
		if (m_ServerBluePrintDic.ContainsKey(tid))
		{
			return m_ServerBluePrintDic[tid];
		}
		else
		{
		}
		return null;
	}

	/// <summary>
	/// 移除成员
	/// </summary>
	/// <param name="tid">tid</param>
	public void RemoveMember(int tid)
	{
		if (m_ServerBluePrintDic.ContainsKey(tid))
		{
			 m_ServerBluePrintDic.Remove(tid);
		}
		if (m_ServerBluePrintDic.Count == 0)
		{
			RemoveAllListeners();
		}
	}

	/// <summary>
	/// 设置蓝图完成
	/// </summary>
	/// <param name="id">tid</param>
	public void SetFoundryItemFinish(int id)
	{
		if (m_ServerBluePrintDic != null)
		{
			if (m_ServerBluePrintDic.ContainsKey(id))
			{
				m_ServerBluePrintDic[id].SpendTime = m_ServerBluePrintDic[id].EndTime - m_ServerBluePrintDic[id].StartTime;
			}
		}
	}

	/// <summary>
	/// 通过实例id获取蓝图成员
	/// </summary>
	/// <param name="id">UID</param>
	/// <returns></returns>
	public ProduceInfoVO GetFoundryById(int id) 
	{
		if (m_ServerBluePrintDic != null && m_ServerBluePrintDic.ContainsKey(id))
		{
			return m_ServerBluePrintDic[id];
		}
		return null;
	}

	/// <summary>
	/// 添加生产时间监听
	/// </summary>
	private void AddListener()
	{
		if (!addedListener)
		{
			addedListener = true;
			ServerTimeUtil.Instance.OnTick += ServerTimeOnTick;
		} 
	}
	/// <summary>
	/// 移除生产时间监听
	/// </summary>
	private void RemoveAllListeners()
	{
		if (addedListener)
		{
			addedListener = false;
			ServerTimeUtil.Instance.OnTick -= ServerTimeOnTick;
		} 
	}

	/// <summary>
	/// 蓝图生产的时间变化
	/// </summary>
	private void ServerTimeOnTick()
	{
		if ( m_ServerBluePrintDic.Count == 0)
		{
			RemoveAllListeners();
			return;
		}
		foreach (ProduceInfoVO item in m_ServerBluePrintDic.Values)
		{
			item.SpendTime++;
		}
	}

    /// <summary>
	/// 设置蓝图状态信息
	/// </summary>
	/// <param name="uId">实例id</param>
	/// <param name="tId">蓝图id</param>
	/// <param name="num">蓝图数量</param>
	/// <param name="state">蓝图状态</param>
	/// <param name="progress">蓝图进度</param>
	/// <param name="active">是否激活</param>
	public void SetGirdDataInfo(int tId, ProduceState state, float progress = 0, bool active = true)
    {
        if (!GetBluePrintDic().ContainsKey(tId))
        {
            ProduceInfoVO girdInfo = new ProduceInfoVO();
            girdInfo.TID = tId;
            girdInfo.MItem = GetItemByProduceKey(tId);
            girdInfo.MProduce = GetItemByKey((uint)tId);
            girdInfo.BluePrintState = state;
            girdInfo.Progress = progress;
            GetBluePrintDic().Add(tId, girdInfo);
            girdInfo.Active = active;
        }
        else
        {
            ProduceInfoVO girdInfo = GetBluePrintDic()[tId];
            girdInfo.Progress = progress;
            girdInfo.BluePrintState = state;
            girdInfo.Active = active;
        }
    }


    #region 判断蓝图是否可以生产

    //根据身上是否有，材料金钱，等级   蓝图不放在背包里
    /// <summary>
    /// 判断是否正在生产
    /// </summary>
    /// <param name="tid">tid</param>
    /// <returns></returns>
    private bool BeProducingOrFinsh(int tid)
    {
        if (GetServerBluePrintDic().ContainsKey(tid))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 是否材料和钱充足可以
    /// </summary>
    /// <param name="tid"></param>
    /// <returns></returns>
    public bool BeCanProduce(int tid)
    {
        Produce produce = GetProduceByKey(tid);
        EffectElement?[] cost = GetEffectElementsByKey(produce.Cost);
        uint itmeId;
        if (m_PackageProxy==null)
        {
            m_PackageProxy = GameFacade.Instance.RetrieveProxy(ProxyName.PackageProxy) as PackageProxy;
        }
        //消耗物品
        for (int i = 0; i < cost.Length; i++)
        {
            itmeId = (uint)cost[i].Value.ItemId;
            long hasCout = m_PackageProxy.GetItemCountByTID(itmeId);
            long needCout = (long)cost[i].Value.Value;
            if (hasCout < needCout)
            {
                SetGirdDataInfo(tid, ProduceState.CanNotProduce, 0);//默认true
                return false;
            }
        }
        SetGirdDataInfo(tid, ProduceState.CanProduce, 0);
        return true;
    }

    /// <summary>
    /// 判断是否激活
    /// </summary>
    /// <param name="tid">tid</param>
    /// <returns></returns>
    private bool BeActive(int tid)
    {
        return true;
    }

    /// <summary>
    /// 是否生产完成
    /// </summary>
    /// <param name="tid">蓝图tID</param>
    /// <returns></returns>
    private bool BeFinshed(int tid)
    {
        bool finshed = false;
        if (GetServerBluePrintDic().ContainsKey(tid))
        {
            if (GetServerBluePrintDic().TryGetValue(tid, out ProduceInfoVO produceInfoVO))
            {
                float progress = 0;

                if (produceInfoVO.EndTime > produceInfoVO.StartTime && produceInfoVO.SpendTime >= 0)
                {
                    progress = (float)produceInfoVO.SpendTime / (produceInfoVO.EndTime - produceInfoVO.StartTime);
                }
                finshed = produceInfoVO.Finished;
                ProduceState state = produceInfoVO.Finished ? ProduceState.Finsh : ProduceState.Producing;
                SetGirdDataInfo(produceInfoVO.TID, state, progress);
            }
        }

        return finshed;
    }

    /// <summary>
    /// 设置蓝图状态
    /// </summary>
    /// <param name="tid">蓝图tid</param>
    public void SetProduceState(int tid)
    {
        if (BeActive(tid))//1，判断是否激活
        {
            if (BeCanProduce(tid))//2,判断是否可以生产
            {
                if (BeProducingOrFinsh(tid))//3,判断是否正在生产中
                {
                    //4,判断是否生产完成
                    BeFinshed(tid);
                    //Debug.Log("生产中");
                }
                else
                {
                    //	Debug.Log("材料足");
                }
            }
            else
            {
                //4,判断是否生产完成
                BeFinshed(tid);
                //Debug.Log("材料不足");
            }
        }
        else
        {
            //Debug.Log("未激活");
            SetGirdDataInfo(tid, ProduceState.CanNotProduce, 0);
        }

        BlueprintL1 blueprintL1 = 0;
        Item item = GetItemByKey((uint)tid);
        ItemType itemMainType = ItemTypeUtil.GetItemType(item.Type);//主类型
        ItemTypeUtil.SetSubType(ref blueprintL1, itemMainType);//次类型
        if (blueprintL1 == BlueprintL1.Warship && IsHaveShipByTid(GetItemByProduceKey(tid).Id))
        {
            SetGirdDataInfo(tid, ProduceState.Have, 0);
        }
    }



    #endregion
}
