using Eternity.FlatBuffer;
using Eternity.Runtime.Item;
using PureMVC.Patterns.Proxy;
using System.Collections.Generic;

public class PackageProxy : Proxy
{

    private CfgEternityProxy m_CfgEternityProxy;
    private ShipProxy m_ShipProxy;
    /// <summary>
    /// 构造函数
    /// </summary>
    public PackageProxy() : base(ProxyName.PackageProxy)
    {
        m_CfgEternityProxy = Facade.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
        m_ShipProxy = Facade.RetrieveProxy(ProxyName.ShipProxy) as ShipProxy;
    }

    /// <summary>
    /// 各种包的父节点
    /// </summary>
    private ItemContainer m_PlayerItem;
    /// <summary>
    /// 人形态和出战船的父节点
    /// </summary>
    private ItemContainer m_HeroItem;
    public ItemContainer GetHeroItem()
    {
        return m_HeroItem;
    }
    /// <summary>
    /// 空间站各种包的父节点
    /// </summary>
    private ItemContainer m_SpaceStation;
    /// <summary>
    /// 所有包
    /// </summary>
    private Dictionary<string, ItemContainer> m_AllPackage = new Dictionary<string, ItemContainer>();
    /// <summary>
    /// 所有道具列表
    /// </summary>
    private Dictionary<ulong, ItemContainer> m_Items = new Dictionary<ulong, ItemContainer>();
    /// <summary>
    /// 道具查找缓存
    /// </summary>
    private List<ItemBase> m_ItemBaseCache = new List<ItemBase>();

    public void CleanupData()
    {
        m_AllPackage = new Dictionary<string, ItemContainer>();
        m_Items = new Dictionary<ulong, ItemContainer>();
    }

    /// <summary>
    /// 添加道具
    /// </summary>
    /// <param name="holdInit"></param>
    /// <param name="uid"></param>
    /// <param name="tid"></param>
    /// <param name="parentUid"></param>
    /// <param name="pos"></param>
    /// <param name="count"></param>
    /// <param name="capacity"></param>
    public Category AddItem(bool holdInit, ulong uid, uint tid, ulong parentUid, int pos, long count, uint capacity, ulong reference, ulong createTime)
    {
        ItemContainer item = CreateItem(uid, tid, parentUid, pos, count, capacity, reference, createTime);
        m_Items.Add(uid, item);
        if (m_PlayerItem == null && item.MainType == Category.Player)
        {
            m_PlayerItem = item;
            return Category.Player;
        }
        else if (m_HeroItem == null && item.MainType == Category.Hero)
        {
            m_HeroItem = item;
            return Category.Hero;
        }
        else if (m_SpaceStation == null && item.MainType == Category.WarehousePackage)
        {
            m_SpaceStation = item;
        }

        if (!holdInit)
        {
            LinkData(uid, parentUid);
        }
        if (item.MainType != Category.Skill)
        {
            ItemChangeInfo info = new ItemChangeInfo();
            info.ChangeType = ItemChangeInfo.Type.Add;
            info.Category = item.MainType;
            info.UID = uid;
            info.TID = item.TID;
            info.ParentUID = parentUid;
            info.ItemPos = pos;
            info.CountChangeDelta = count;
            SendNotification(NotificationName.MSG_PACKAGE_ITEM_CHANGE, info);

        }
        return item.MainType;
    }

    /// <summary>
    /// 删除道具
    /// </summary>
    /// <param name="uid"></param>
    public void RemoveItem(ulong uid)
    {
        if (m_Items.TryGetValue(uid, out ItemContainer item))
        {
            ItemChangeInfo info = new ItemChangeInfo();
            info.ChangeType = ItemChangeInfo.Type.Del;
            info.Category = item.MainType;
            info.UID = uid;
            info.ParentUID = item.ParentUID;
            info.ItemPos = item.Position;
            info.TID = item.TID;
            if (item.Reference != 0 && m_Items.TryGetValue(item.Reference, out ItemContainer original))
            {
                original.Replicas.Remove(uid);
            }
            DeLinkData(uid);
            m_Items.Remove(uid);

            if (item.MainType != Category.Skill)
            {
                SendNotification(NotificationName.MSG_PACKAGE_ITEM_CHANGE, info);
            }
        }
    }

    /// <summary>
    /// 修改包可用上限
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="size"></param>
    public void ChangeContainerSize(ulong uid, uint size)
    {
        if (m_Items.TryGetValue(uid, out ItemContainer item))
        {
            item.CurrentSizeMax = size;
        }
    }

    /// <summary>
    /// 修改道具关联和或位置
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="parentUid"></param>
    /// <param name="position"></param>
    public void ChangePosition(ulong uid, ulong parentUid, int position)
    {
        if (m_Items.TryGetValue(uid, out ItemContainer item))
        {
            if (item.ParentUID != parentUid)
            {
                ChangeRelation(uid, parentUid);
            }
            item.Position = position;
        }
    }

    /// <summary>
    /// 修改道具数量
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="count"></param>
    public void ChangeStackCount(ulong uid, long count)
    {
        if (m_Items.TryGetValue(uid, out ItemContainer item))
        {
            long oldCount = item.Count;

            item.Count = count;
            if (item.MainType == Category.Currency)
            {
                SendNotification(NotificationName.MSG_CURRENCY_CHANGED);
            }
            if (oldCount != count)
            {
                ItemChangeInfo info = new ItemChangeInfo()
                {
                    Category = item.MainType,
                    UID = item.UID,
                    TID = item.TID,
                    ItemPos = item.Position,
                    ParentUID = item.ParentUID,
                    ChangeType = ItemChangeInfo.Type.CountChange,
                    CountChangeDelta = count - oldCount
                };

                SendNotification(NotificationName.MSG_PACKAGE_ITEM_CHANGE, info);
            }
        }
    }

    /// <summary>
    /// 同步道具的lv和exp属性
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="lv"></param>
    /// <param name="exp"></param>
    public void ChangeItemAttr(ulong uid, int lv, int exp)
    {
        if (m_Items.TryGetValue(uid, out ItemContainer item))
        {
            switch (item)
            {
                case ItemWarShipVO val:
                    {
                        if (exp > 0)
                        {
                            val.Exp = exp;
                        }
                        if (item.Lv > 0 && item.Lv != lv)
                        {
                            MsgShipLevelUp msg = new MsgShipLevelUp();
                            msg.m_level = lv;
                            msg.m_Tid = item.TID;
                            Facade.SendNotification(NotificationName.MSG_PLAYER_SHIP_LEVEL_UP, msg);
                        }                                                    
                    }
                    break;               
            }
            item.Lv = lv;
        }
    }

    /// <summary>
    /// 初次关联道具数据
    /// </summary>
    public void RelationData()
    {
        foreach (var item in m_Items)
        {
            LinkData(item.Key, item.Value.ParentUID);
        }
    }

    /// <summary>
    /// 调整关联
    /// 道具的跨包移动
    /// 装备的穿脱
    /// </summary>
    /// <param name="uid">道具实例id</param>
    /// <param name="parentUid">目标父节点实例id</param>
    public void ChangeRelation(ulong uid, ulong parentUid)
    {
        DeLinkData(uid, parentUid);
        LinkData(uid, parentUid);
        if (GetItem(uid).MainType == Category.Warship)
        {
            m_ShipProxy.GetAppointWarShip();
        }
    }

    /// <summary>
    /// 关联道具数据为树形结构
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="parentUid"></param>
    public void LinkData(ulong uid, ulong parentUid)
    {
        if (m_Items.TryGetValue(parentUid, out ItemContainer container))
        {
            if (container.Items == null)
            {
                container.Items = new Dictionary<ulong, ItemBase>();
            }
            if (!container.Items.ContainsKey(uid))
            {
                container.Items.Add(uid, m_Items[uid]);
            }
            if (parentUid == m_PlayerItem.UID)//标准包
            {
                if (!m_AllPackage.ContainsKey(m_Items[uid].ContainerType))
                {
                    m_AllPackage.Add(m_Items[uid].ContainerType, m_Items[uid]);
                }
            }
            else if (parentUid == m_SpaceStation.UID)//空间站包
            {
                if (!m_AllPackage.ContainsKey(m_SpaceStation.MainType.ToString() + m_Items[uid].ContainerType))
                {
                    m_AllPackage.Add(m_SpaceStation.MainType.ToString() + m_Items[uid].ContainerType, m_Items[uid]);
                }
            }
        }
    }

    /// <summary>
    /// 解除关联
    /// </summary>
    /// <param name="uid"></param>
    public void DeLinkData(ulong uid, ulong newParentUid = 0)
    {
        if (m_Items.TryGetValue(uid, out ItemContainer item))
        {
            if (m_Items.TryGetValue(item.ParentUID, out ItemContainer container))
            {
                if (container.Items != null && container.Items.ContainsKey(uid))
                {
                    container.Items.Remove(uid);
                }
            }

            item.ParentUID = newParentUid;
        }
    }

    /// <summary>
    /// 按包类型查找包
    /// </summary>
    /// <param name="packType">包类型</param>
    /// <returns>包数据</returns>
    public ItemContainer GetPackage(params Category[] packType)
    {
        string s = string.Join(string.Empty, packType);
        if (!m_AllPackage.ContainsKey(s))
        {
            return null;
        }
        return m_AllPackage[s];
    }

    /// <summary>
    /// 在所有包里查找道具并转型(不含任务包)
    /// </summary>
    /// <param name="id">实例ID</param>
    /// <returns>找到的道具</returns>
    public T GetItem<T>(ulong id) where T : ItemContainer
    {
        return GetItem(id) as T;
    }

    /// <summary>
    /// 在所有包里查找道具(不含任务包)
    /// </summary>
    /// <param name="id">实例ID</param>
    /// <returns>找到的道具</returns>
    private ItemContainer GetItem(ulong id)
    {
        m_Items.TryGetValue(id, out ItemContainer item);
        return item;
    }

    /// <summary>
    /// 获取所有物品列表，并转型
    /// </summary>
    /// <typeparam name="T">道具子类型</typeparam>
    /// <param name="packID">包ID</param>
    /// <returns>道具数组</returns>
    /// <summary>
    public T[] GetItemList<T>(params Category[] packID) where T : ItemBase
    {
        List<T> list = new List<T>();
        ItemContainer pack = GetPackage(packID);

        if (pack == null || pack.Items == null || pack.Items.Count == 0)
        {
            return list.ToArray();
        }

        foreach (ItemBase item in pack.Items.Values)
        {
            if (item is T)
            {
                list.Add(item as T);
            }
        }
        return list.ToArray();
    }

    /// <summary>
    /// 按ItemType查找道具
    /// </summary>
    /// <param name="itemType">道具类型</param>
    /// <param name="prefixMode">是否使用前缀模式比较</param>
    /// <returns>道具列表</returns>
    public ItemBase[] FindItemArrayByItemType(ItemType itemType, bool prefixMode)
    {
        m_ItemBaseCache.Clear();

        ItemContainer pack = GetPackage(itemType.MainType);

        if (pack != null && pack.Items != null)
        {
            foreach (ItemBase item in pack.Items.Values)
            {
                if (item.ItemConfig.Type == itemType.NativeType)
                {
                    m_ItemBaseCache.Add(item);
                    continue;
                }

                if (prefixMode)
                {
                    ItemType currType = ItemTypeUtil.GetItemType(item.ItemConfig.Type);
                    bool equal = true;
                    for (int i = 0; i < itemType.EnumList.Length; i++)
                    {
                        if (!Equals(itemType.EnumList[i], currType.EnumList[i]))
                        {
                            equal = false;
                            break;
                        }
                    }

                    if (equal)
                        m_ItemBaseCache.Add(item);
                }
            }
        }

        ItemBase[] result = m_ItemBaseCache.ToArray();

        m_ItemBaseCache.Clear();

        return result;
    }

    /// <summary>
    /// 按配置ID统计所有包中指定物品的数量
    /// </summary>
    /// <param name="itemID">配置表ID</param>
    /// <returns>总数</returns>
    public long GetItemCountByTID(uint itemID)
    {
        long count = 0;
        foreach (var item in m_Items)
        {
            if (item.Value.TID == itemID)
            {
                count += item.Value.Count;
            }
        }
        return count;
    }

    #region mark处理
    public Category CheckMarkItemAdd(ulong markUid, ulong itemUid)
    {
        if (markUid == 0)
        {
            return 0;
        }
        if (m_Items.TryGetValue(markUid, out ItemContainer item))
        {
            switch (item)
            {
                case ItemWarShipVO val:
                    if (m_ShipProxy.AddItemToShip(val.UID, m_Items[itemUid]))
                    {
                        MsgShipDataChanged info = MessageSingleton.Get<MsgShipDataChanged>();
                        info.ShipUid = markUid;
                        info.ChangeType = MsgShipDataChanged.Type.Add;
                        info.ItemType = m_Items[itemUid].MainType;
                        info.ContainerUid = m_Items[itemUid].ParentUID;
                        info.ItemUid = itemUid;
                        Facade.SendNotification(NotificationName.MSG_SHIP_DATA_CHANGED, info);
                        return info.ItemType;
                    }
                    break;
            }
        }
        return 0;
    }

    public Category CheckMarkItemRemove(ulong markUid, ulong itemUid)
    {
        if (markUid == 0)
        {
            return 0;
        }
        if (m_Items.TryGetValue(markUid, out ItemContainer item))
        {
            switch (item)
            {
                case ItemWarShipVO val:
                    if (m_ShipProxy.RemoveItemToShip(val.UID, itemUid, m_Items[itemUid].ParentUID))
                    {
                        MsgShipDataChanged info = MessageSingleton.Get<MsgShipDataChanged>();
                        info.ShipUid = markUid;
                        info.ChangeType = MsgShipDataChanged.Type.Remove;
                        info.ItemType = m_Items[itemUid].MainType;
                        info.ContainerUid = m_Items[itemUid].ParentUID;
                        info.ItemUid = itemUid;
                        Facade.SendNotification(NotificationName.MSG_SHIP_DATA_CHANGED, info);
                        return info.ItemType;
                    }
                    break;
            }
        }
        return 0;
    }
    #endregion

    #region CreateNewItem
    public ItemContainer CreateItem(ulong uid, uint tid, ulong parentUid, int pos, long count, uint capacity, ulong reference, ulong createTime)
    {
        ItemContainer item;
        Item itemCfg = m_CfgEternityProxy.GetItemByKey(tid);
        ItemType itemType = ItemTypeUtil.GetItemType(itemCfg.Type);

        switch (itemType.MainType)
        {
            case Category.Blueprint://1 蓝图
                item = new ItemDrawingVO();
                ItemTypeUtil.SetSubType(ref (item as ItemDrawingVO).DrawingType, itemType);
                break;
            case Category.Weapon://2 武器
                item = new ItemWeaponVO();
                (item as ItemWeaponVO).Config = itemCfg.ItemUnion<Weapon>().Value;
                ItemTypeUtil.SetSubType(ref (item as ItemWeaponVO).WeaponType1, itemType);
                ItemTypeUtil.SetSubType(ref (item as ItemWeaponVO).WeaponType2, itemType);
                break;
            case Category.Reformer://3 转化炉
                item = new ItemReformerVO();
                break;
            case Category.Equipment://4 装备
                item = new ItemEquipmentVO();
                (item as ItemEquipmentVO).Config = itemCfg.ItemUnion<Equip>().Value;
                ItemTypeUtil.SetSubType(ref (item as ItemEquipmentVO).EquipmentType, itemType);
                break;
            case Category.EquipmentMod://5 mod
                item = new ItemModVO();
                (item as ItemModVO).Config = itemCfg.ItemUnion<Mod>().Value;
                ItemTypeUtil.SetSubType(ref (item as ItemModVO).ModType1, itemType);
                ItemTypeUtil.SetSubType(ref (item as ItemModVO).ModType2, itemType);
                break;
            case Category.Material://6 材料
                item = new ItemMaterialVO();
                ItemTypeUtil.SetSubType(ref (item as ItemMaterialVO).MaterialType, itemType);
                break;
            case Category.Expendable://消耗品
                item = new ItemExpendableVO();
                ItemTypeUtil.SetSubType(ref (item as ItemExpendableVO).ExpendableType, itemType);
                break;
            //case Category.TaskItem://7 任务道具
            //	item = new ItemMissionVO();
            //	break;
            case Category.Warship://8 船
                item = new ItemWarShipVO();
                (item as ItemWarShipVO).Config = itemCfg.ItemUnion<Warship>().Value;
                ItemTypeUtil.SetSubType(ref (item as ItemWarShipVO).WarshipL1, itemType);
                break;
            case Category.Skill://技能
                item = new ItemSkillVO();
                break;
            case Category.Package://包
                item = new ItemContainer();
                (item as ItemContainer).ContainerType = ItemTypeUtil.GetContainerType(itemCfg);
                (item as ItemContainer).CurrentSizeMax = capacity;
                break;
            case Category.Currency://货币
                item = new ItemCurrency();
                break;
            case Category.Hero://人形态和出战船的父节点
            case Category.Player://玩家各种包的父节点
            case Category.WarehousePackage://空间站仓库
            default:
                item = new ItemContainer();
                break;
        }


        item.RootType = itemType.RootType;
        item.MainType = itemType.MainType;
        item.ItemConfig = itemCfg;
        item.UID = uid;
        item.TID = tid;
        item.ParentUID = parentUid;
        item.Position = pos;
        item.Count = count;
        item.Reference = reference;
        item.CreateTime = createTime;

        if (reference != 0)
        {
            if (m_Items.TryGetValue(reference, out ItemContainer value))
            {
                value.Replicas.Add(uid);
            }
        }

        return item;
    }
    #endregion
}