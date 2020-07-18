using Eternity.FlatBuffer;
using Eternity.Runtime.Item;
using System.Collections.Generic;

public static class WarShipDataFactory
{
	/// <summary>
	/// 通过船道具实例构建一艘船的数据
	/// </summary>
	/// <param name="itemShip"></param>
	/// <returns></returns>
	public static IShip BuildShipData(ItemWarShipVO itemShip)
	{
		return new Ship(itemShip);
	}

	/// <summary>
	/// 通过道具数据构建一艘船
	/// </summary>
	/// <param name="itemData"></param>
	/// <returns></returns>
	public static IShip BuildShipData(Item itemData)
	{
		if (ItemTypeUtil.GetItemType(itemData.Type).MainType == Category.Warship)
		{
			return new Ship(itemData);
		}
		return null;
	}

	/// <summary>
	/// 加装备
	/// </summary>
	/// <param name="targetShip"></param>
	/// <param name="item"></param>
	/// <returns></returns>
	public static IShip AddItemToShip(IShip targetShip, ItemContainer item)
	{
		(targetShip as Ship).AddItem(item);
		return targetShip;
	}

	/// <summary>
	/// 减装备
	/// </summary>
	/// <param name="targetShip"></param>
	/// <param name="uid"></param>
	/// <returns></returns>
	public static IShip RemoveItemToShip(IShip targetShip, ulong uid, ulong parentUid)
	{
		(targetShip as Ship).RemoveItem(uid, parentUid);
		return targetShip;
	}

	#region Class Ship
	private class Ship : ShipItem, IShip
	{
		private Dictionary<ulong, ShipItem> m_AllItem;

		public Ship(Item itemData)
		{
			SetTID(itemData.Id);
			SetBaseConfig(itemData);
		}

		public Ship(ItemWarShipVO itemShip)
		{
			m_AllItem = new Dictionary<ulong, ShipItem>();
			SetUID(itemShip.UID);
			SetTID(itemShip.TID);
			SetLv(itemShip.Lv);
			SetExp(itemShip.Exp);
			SetCraetTime(itemShip.CreateTime);
			SetWarShipType(itemShip.WarshipL1);
			SetReference(itemShip.Reference);
			SetBaseConfig(itemShip.ItemConfig);
			Convert(this, itemShip.Items);
		}

		public void AddItem(ItemContainer item)
		{
			ShipItem shipItem = new ShipItem();
			shipItem.SetUID(item.UID);
			shipItem.SetTID(item.TID);
			shipItem.SetReference(item.Reference);
			switch (item)
			{
				case ItemWeaponVO val:
					shipItem.SetLv(val.Lv);
					shipItem.SetItemPos(val.Position);
					m_AllItem[item.ParentUID].AddItem(shipItem);
					break;
				case ItemReformerVO val:
					shipItem.SetLv(val.Lv);
					m_AllItem[item.ParentUID].AddItem(shipItem);
					break;
				case ItemEquipmentVO val:
					shipItem.SetLv(val.Lv);
					shipItem.SetItemPos(val.Position);
					m_AllItem[item.ParentUID].AddItem(shipItem);
					break;
				case ItemModVO val:
					shipItem.SetLv(val.Lv);
					shipItem.SetItemPos(val.Position);
					m_AllItem[item.ParentUID].AddItem(shipItem);
					break;
                case ItemSkillVO val:
                    shipItem.SetItemPos(val.Position);
                    m_AllItem[item.ParentUID].AddItem(shipItem);
                    break;
				case ItemContainer val:
					shipItem.SetCurrentSizeMax(val.CurrentSizeMax);
					m_AllItem[item.ParentUID].AddContainer(val.ContainerType, shipItem);
					break;
			}
			shipItem.SetBaseConfig(item.ItemConfig);
			Convert(shipItem, item.Items);

			if (m_AllItem.ContainsKey(shipItem.GetUID()))
			{
				m_AllItem.Remove(shipItem.GetUID());
			}
			m_AllItem.Add(shipItem.GetUID(), shipItem);
		}

		public void RemoveItem(ulong uid, ulong parentUid)
		{
			m_AllItem[parentUid].RemoveItem(m_AllItem[uid]);
			m_AllItem.Remove(uid);
		}

		#region 物船转换
		private void Convert(ShipItem shipContainer, Dictionary<ulong, ItemBase> items)
		{
			if (items != null && items.Count != 0)
			{
				foreach (var item in items)
				{
					ShipItem shipItem = new ShipItem();
					shipItem.SetUID(item.Value.UID);
					shipItem.SetTID(item.Value.TID);
					shipItem.SetReference(item.Value.Reference);
					shipItem.SetBaseConfig(item.Value.ItemConfig);
					switch (item.Value.MainType)
					{
						case Category.Package:
							shipItem.SetCurrentSizeMax((item.Value as ItemContainer).CurrentSizeMax);
							shipContainer.AddContainer((item.Value as ItemContainer).ContainerType, shipItem);
							Convert(shipItem, (item.Value as ItemContainer).Items);
							break;
						case Category.Weapon:
							shipItem.SetLv((item.Value as ItemWeaponVO).Lv);
							shipItem.SetItemPos(item.Value.Position);
							shipContainer.AddItem(shipItem);
							Convert(shipItem, (item.Value as ItemContainer).Items);
							break;
						case Category.EquipmentMod:
							shipItem.SetLv((item.Value as ItemModVO).Lv);
							shipItem.SetItemPos(item.Value.Position);
							shipContainer.AddItem(shipItem);
							break;
						case Category.Equipment:
							shipItem.SetLv((item.Value as ItemEquipmentVO).Lv);
							shipItem.SetItemPos(item.Value.Position);
							shipContainer.AddItem(shipItem);
							break;
						case Category.Reformer:
							shipItem.SetLv((item.Value as ItemReformerVO).Lv);
							shipContainer.AddItem(shipItem);
							break;
						default:
							shipContainer.AddItem(shipItem);
							break;
					}

					if (m_AllItem.ContainsKey(shipItem.GetUID()))
					{
						m_AllItem.Remove(shipItem.GetUID());
					}
					m_AllItem.Add(shipItem.GetUID(), shipItem);
				}
			}
		}
		#endregion

		#region IShip
		public IShipItemBase GetItem(ulong uid)
		{
			if (m_AllItem.TryGetValue(uid, out ShipItem value))
			{
				return value;
			}
			return null;
		}

		public IWeaponContainer GetWeaponContainer()
		{
			IShipContainerBase shipContainerBase = GetPackage(Category.Weapon.ToString());
			if (shipContainerBase != null)
				return shipContainerBase as IWeaponContainer;
			return null;
		}

		public IReformerContainer GetReformerContainer()
		{
			return GetPackage(Category.Reformer.ToString()) as IReformerContainer;
		}

		public IEquipmentContainer GetEquipmentContainer()
		{
			IShipContainerBase shipContainerBase = GetPackage(Category.Equipment.ToString());
			if (shipContainerBase != null)
				return shipContainerBase as IEquipmentContainer;
			return null;
		}

		public ISkillContainer GetSkillContainer()
		{
			return GetPackage(Category.Skill.ToString()) as ISkillContainer;
		}

		public Warship GetConfig()
		{
			return GetBaseConfig().ItemUnion<Warship>().Value;
		}

		IModContainer IShip.GetGeneralModContainer()
		{
			return GetPackage(Category.EquipmentMod.ToString(),
					EquipmentModL1.WarshipMod.ToString(),
					EquipmentModL2.General.ToString()) as IModContainer;
		}

		IModContainer IShip.GetExclusivelyModContainer()
		{
			return GetPackage(Category.EquipmentMod.ToString(),
					EquipmentModL1.WarshipMod.ToString(),
					EquipmentModL2.Exclusively.ToString()) as IModContainer;
		}
		#endregion
	}
	#endregion
}