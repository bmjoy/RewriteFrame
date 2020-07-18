using Eternity.Runtime.Item;
using PureMVC.Patterns.Proxy;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ShipProxy : Proxy
{
    /// <summary>
    /// 人形态和出战船的父节点
    /// </summary>
    private ItemContainer m_HeroItem;
    /// <summary>
    /// 当前出战船
    /// </summary>
    private IShip m_CurrentWarShip;
    /// <summary>
    /// 船包
    /// </summary>
    private Dictionary<ulong, IShip> m_ShipPackage;

    public ShipProxy() : base(ProxyName.ShipProxy)
    {
        m_ShipPackage = new Dictionary<ulong, IShip>();
    }

    /// <summary>
    /// 外部访问船包
    /// </summary>
    /// <returns></returns>
    public Dictionary<ulong, IShip> GetShipPackage()
    {
        return m_ShipPackage;
    }
    /// <summary>
    /// 获取当前任命出战船
    /// </summary>
    /// <returns></returns>
    public IShip GetAppointWarShip()
    {
        PackageProxy m_PackageProxy = Facade.RetrieveProxy(ProxyName.PackageProxy) as PackageProxy;
        m_HeroItem = m_PackageProxy.GetHeroItem();
        if (m_HeroItem != null && m_HeroItem.Items != null && m_HeroItem.Items.Count > 0)
        {
            ItemWarShipVO itemWarShip = null;
            foreach (ItemBase item in m_HeroItem.Items.Values)
            {
                if (item is ItemWarShipVO)
                {
                    if (m_CurrentWarShip == null || m_CurrentWarShip.GetUID() != item.UID)
                    {
                        if (!m_ShipPackage.TryGetValue(item.UID, out m_CurrentWarShip))
                        {
                            itemWarShip = item as ItemWarShipVO;
                            m_CurrentWarShip = WarShipDataFactory.BuildShipData(itemWarShip);
                            m_ShipPackage[m_CurrentWarShip.GetUID()] = m_CurrentWarShip;
                        }

                    }
                    return m_CurrentWarShip;
                }
            }
        }
        m_CurrentWarShip = null;
        return null;
    }
    /// <summary>
    /// 初始化所有船数据
    /// </summary>
    /// <returns></returns>
    public void InitShipPackage()
    {
        PackageProxy m_PackageProxy = Facade.RetrieveProxy(ProxyName.PackageProxy) as PackageProxy;
        ItemContainer[] m_ShipByType = m_PackageProxy.GetItemList<ItemContainer>(Category.WarehousePackage, Category.Package);
        m_ShipPackage.Clear();
        if (m_ShipByType != null)
        {
            foreach (ItemContainer package in m_ShipByType)
            {
                if (package.Items != null)
                {
                    foreach (var item in package.Items.Values)
                    {
                        IShip m_WarShip = WarShipDataFactory.BuildShipData((ItemWarShipVO)item);
                        m_ShipPackage.Add(m_WarShip.GetUID(), m_WarShip);
                    }
                }
            }
        }
        IShip m_CurWarShip = GetAppointWarShip();
        if (!m_ShipPackage.ContainsKey(m_CurWarShip.GetUID()))
        {
            m_ShipPackage.Add(m_CurWarShip.GetUID(), m_CurWarShip);
        }
    }
    /// <summary>
    /// 修改船数据包
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public void ChangeShipPackage(ulong id)
    {
        PackageProxy m_PackageProxy = Facade.RetrieveProxy(ProxyName.PackageProxy) as PackageProxy;
        ItemWarShipVO itemWarShipVO = m_PackageProxy.GetItem<ItemWarShipVO>(id);
        IShip m_Ship = null;
        m_Ship = WarShipDataFactory.BuildShipData(itemWarShipVO);
        m_ShipPackage.Add(m_Ship.GetUID(), m_Ship);
        IShip m_CurWarShip = GetAppointWarShip();
        if (!m_ShipPackage.ContainsKey(m_CurWarShip.GetUID()))
        {
            m_ShipPackage.Add(m_CurWarShip.GetUID(), m_CurWarShip);
        }
    }
    /// <summary>
    /// 根据类型获取船
    /// </summary>
    /// <param name="warshipL1"></param>
    /// <returns></returns>
    public IShip[] GetShipByType(WarshipL1 warshipL1)
    {
        IShip[] m_Ships = GetShips();
        List<IShip> m_ShipsByType = new List<IShip>();
        if (m_Ships != null && m_Ships.Length > 0)
        {
            foreach (IShip item in m_Ships)
            {
                if (item.GetWarShipType() == warshipL1)
                {
                    m_ShipsByType.Add(item);
                }
            }
        }
        return m_ShipsByType.ToArray();
    }
    /// <summary>
    /// 获取船类型包
    /// </summary>
    /// <param name="warshipL1"></param>
    /// <returns></returns>
    public ItemContainer GetShipContainer(WarshipL1 warshipL1)
    {
        PackageProxy m_PackageProxy = Facade.RetrieveProxy(ProxyName.PackageProxy) as PackageProxy;
        switch (warshipL1)
        {
            case WarshipL1.MiningShip:
                return m_PackageProxy.GetItemList<ItemContainer>(Category.WarehousePackage, Category.Package)[2];
            case WarshipL1.FightWarship:
                return m_PackageProxy.GetItemList<ItemContainer>(Category.WarehousePackage, Category.Package)[1];
            case WarshipL1.SurveillanceShip:
                return m_PackageProxy.GetItemList<ItemContainer>(Category.WarehousePackage, Category.Package)[0];
        }
        return null;
    }
    /// <summary>
    /// 获取当前船数据
    /// </summary>
    /// <returns></returns>
    public IShip[] GetShips()
    {
        return m_ShipPackage.Values.ToArray();
    }

    /// <summary>
    /// 加装备
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="item"></param>
    /// <returns></returns>
    public bool AddItemToShip(ulong uid, ItemContainer item)
    {
        IShip ship = null;
        m_ShipPackage.TryGetValue(uid, out ship);
        if (ship != null)
        {
            m_CurrentWarShip = WarShipDataFactory.AddItemToShip(ship, item);
            m_ShipPackage[uid] = m_CurrentWarShip;
            return true;
        }
        return false;
    }
    /// <summary>
    /// 减装备
    /// </summary>
    /// <param name="ship"></param>
    /// <param name="uid"></param>
    /// <param name="parentUid"></param>
    /// <returns></returns>
    public bool RemoveItemToShip(ulong uid, ulong itemId, ulong parentUid)
    {
        IShip ship = null;
        m_ShipPackage.TryGetValue(uid, out ship);
        if (ship != null)
        {
            m_CurrentWarShip = WarShipDataFactory.RemoveItemToShip(ship, itemId, parentUid);
            m_ShipPackage[uid] = m_CurrentWarShip;
            return true;
        }
        return false;
    }

    /// <summary>
    /// 当前船是否为新船
    /// </summary>
    /// <returns></returns>
    public bool MarkNew(IShip ship)
    {
        ServerListProxy m_ServerListProxy = GameFacade.Instance.RetrieveProxy(ProxyName.ServerListProxy) as ServerListProxy;
        string key = m_ServerListProxy.GetCurrentCharacterVO().UId.ToString();
        HistoryShip m_HistoryShip = new HistoryShip();
        if (PlayerPrefs.HasKey(key))
        {
            m_HistoryShip = JsonUtility.FromJson<HistoryShip>(PlayerPrefs.GetString(key));
            if (m_HistoryShip.m_ShipList.Contains(ship.GetUID()))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        return false;
    }
    /// <summary>
    /// 存储新船
    /// </summary>
    public void SetNew(IShip ship)
    {
        if (!MarkNew(ship))
        {
            ServerListProxy m_ServerListProxy = GameFacade.Instance.RetrieveProxy(ProxyName.ServerListProxy) as ServerListProxy;
            string key = m_ServerListProxy.GetCurrentCharacterVO().UId.ToString();
            HistoryShip m_HistoryShip = new HistoryShip();
            if (PlayerPrefs.HasKey(key))
            {
                m_HistoryShip = JsonUtility.FromJson<HistoryShip>(PlayerPrefs.GetString(key));
                m_HistoryShip.m_ShipList.Add(ship.GetUID());
                PlayerPrefs.SetString(key, JsonUtility.ToJson(m_HistoryShip));
            }
            else
            {
                m_HistoryShip.m_ShipList.Add(ship.GetUID());
                PlayerPrefs.SetString(key, JsonUtility.ToJson(m_HistoryShip));
            }
        }
    }
}

public class HistoryShip
{
    /// <summary>
    /// 新船列表
    /// </summary>
    public List<ulong> m_ShipList = new List<ulong>();
}


