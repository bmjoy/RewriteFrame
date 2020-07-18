using PureMVC.Patterns.Proxy;
using System.Collections.Generic;
using Assets.Scripts.Proto;
using UnityEngine;

public class ShipItemsProxy : Proxy
{
	private Dictionary<ulong, Dictionary<ulong, ItemContainer>> m_ShipItems = new Dictionary<ulong, Dictionary<ulong, ItemContainer>>();

	public ShipItemsProxy() : base(ProxyName.ShipItemsProxy)
	{
		Debug.Log("ShipItemsProxy!!!");
	}

	public void InitShipItemsByByRespond(S2C_SYNC_FIGHTSHIP_VISIBLE_ITEM_LIST respond)
	{
		PackageProxy packageProxy = GameFacade.Instance.RetrieveProxy(ProxyName.PackageProxy) as PackageProxy;
		ulong uid = respond.uid;

		Dictionary<ulong, ItemContainer> items = new Dictionary<ulong, ItemContainer>();
		foreach (var item in respond.item_list)
		{
			ItemContainer itemcon = packageProxy.CreateItem(item.uid, item.tid, item.parent, item.pos, 0, 0, 0, 0);
			itemcon.Lv = item.lv;
			items.Add(item.uid, itemcon);
		}
		RelationData(items);
		// 用最新的
		if (m_ShipItems.ContainsKey(uid))
			RemoveShipItems(uid);

		m_ShipItems.Add(uid, items);
	}

	/// <summary>
	/// 好友信息解析
	/// </summary>
	/// <param name="respond"></param>
	public void InitShipItemsByByRespond(S2C_SYNC_PLAYERINFO respond)
	{
		PackageProxy packageProxy = GameFacade.Instance.RetrieveProxy(ProxyName.PackageProxy) as PackageProxy;
		ulong uid = respond.uid;
		Debug.Log("收到并解析玩家数据"+uid);
		Dictionary<ulong, ItemContainer> items = new Dictionary<ulong, ItemContainer>();
		foreach (var item in respond.item_list)
		{
			ItemContainer itemcon = packageProxy.CreateItem(item.uid, item.tid, item.parent, item.pos, 0, 0, 0, 0);
			itemcon.Lv = item.lv;
			items.Add(item.uid, itemcon);
		}
		RelationData(items);
		// 用最新的
		if (m_ShipItems.ContainsKey(uid))
			RemoveShipItems(uid);

		m_ShipItems.Add(uid, items);
		GameFacade.Instance.SendNotification(NotificationName.MSG_FRIEND_SHIPDATA_CHANGE,uid);
	}

	/// <summary>
	/// 删除道具
	/// </summary>
	/// <param name="uid"></param>
	public void RemoveShipItems(ulong uid)
	{
		if (m_ShipItems.ContainsKey(uid))
		{
			m_ShipItems.Remove(uid);
			m_Ships.Remove(uid);
		}
	}

	/// <summary>
	/// 清除
	/// </summary>
	public void ClearShipItems()
	{
		m_ShipItems.Clear();
		m_Ships.Clear();
	}

	public void RelationData(Dictionary<ulong, ItemContainer> items)
	{
		foreach (var item in items)
		{
			LinkData(item.Key, item.Value.ParentUID, items);
		}
	}

	/// <summary>
	/// 关联道具数据为树形结构
	/// </summary>
	/// <param name="uid"></param>
	/// <param name="parentUid"></param>
	public void LinkData(ulong uid, ulong parentUid, Dictionary<ulong, ItemContainer> items)
	{
		if (items.TryGetValue(parentUid, out ItemContainer container))
		{
			if (container.Items == null)
			{
				container.Items = new Dictionary<ulong, ItemBase>();
			}
			if (!container.Items.ContainsKey(uid))
			{
				container.Items.Add(uid, items[uid]);
			}
		}
	}

	#region Ship

	private Dictionary<ulong, IShip> m_Ships = new Dictionary<ulong, IShip>();
	/// <summary>
	/// 获取当前出战船
	/// </summary>
	/// <returns></returns>
	public IShip GetCurrentWarShip(ulong uid)
	{
		if (m_ShipItems.Count > 0 && m_ShipItems.ContainsKey(uid))
		{
			Dictionary<ulong, ItemContainer> items = m_ShipItems[uid];
			if (m_Ships.Count > 0 && m_Ships.ContainsKey(uid))
			{
				return m_Ships[uid];
			}

			ItemWarShipVO itemWarShip = null;
			foreach (ItemBase item in items.Values)
			{
				if (item is ItemWarShipVO)
				{
					itemWarShip = item as ItemWarShipVO;
					break;
				}
			}

			if (itemWarShip != null)
			{
				IShip currentWarShip = WarShipDataFactory.BuildShipData(itemWarShip);
				m_Ships.Add(uid, currentWarShip);
				return currentWarShip;
			}
		}
		return null;
	}

	#endregion

}