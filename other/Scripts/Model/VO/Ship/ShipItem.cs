using Eternity.FlatBuffer;
using Eternity.Runtime.Item;
using System.Collections.Generic;

public class ShipItem : ShipContainer, IWeapon, IReformer, IEquipment, IMod, ISkill
{
	private int m_pos;
	private Item m_BaseConfig;
	private int m_Lv;
	private int m_Exp;
    private ulong m_CraetTime;
    private WarshipL1 m_WarshipL1;
    protected Dictionary<string, ShipContainer> m_Containers = new Dictionary<string, ShipContainer>();

	public void AddContainer(string type, ShipContainer value)
	{
		m_Containers.Add(type, value);
	}

	public void SetBaseConfig(Item value)
	{
		m_BaseConfig = value;
	}

	public void SetItemPos(int value)
	{
		m_pos = value;
	}

	public void SetLv(int value)
	{
		m_Lv = value;
	}

	public void SetExp(int value)
	{
		m_Exp = value;
	}
    public void SetCraetTime(ulong value)
    {
        m_CraetTime = value;
    }
    public void SetWarShipType(WarshipL1 value)
    {
        m_WarshipL1 = value;
    }
    public Item GetBaseConfig()
	{
		return m_BaseConfig;
	}

	public int GetPos()
	{
		return m_pos;
	}

	public int GetLv()
	{
		return m_Lv;
	}

	public int GetExp()
	{
		return m_Exp;
	}
    public ulong GetCreatTime()
    {
        return m_CraetTime;
    }
    public WarshipL1 GetWarShipType()
    {
        return m_WarshipL1;
    }
    protected IShipContainerBase GetPackage(params string[] category)
	{
		string s = "";
		for (int i = 0; i < category.Length; i++)
		{
			s += category[i];
		}
		if (m_Containers.ContainsKey(s))
			return m_Containers[s];
		return null;
	}

	IModContainer IWeapon.GetGeneralModContainer()
	{
		return GetPackage(Category.EquipmentMod.ToString(),
							EquipmentModL1.WeaponMod.ToString(),
							EquipmentModL2.General.ToString()) as IModContainer;
	}

	IModContainer IWeapon.GetExclusivelyModContainer()
	{
		return GetPackage(Category.EquipmentMod.ToString(),
					EquipmentModL1.WeaponMod.ToString(),
					EquipmentModL2.Exclusively.ToString()) as IModContainer;
	}

	Weapon IWeapon.GetConfig()
	{
		return GetBaseConfig().ItemUnion<Weapon>().Value;
	}

	Weapon IReformer.GetConfig()
	{
		return GetBaseConfig().ItemUnion<Weapon>().Value;
	}

	Equip IEquipment.GetConfig()
	{
		return GetBaseConfig().ItemUnion<Equip>().Value;
	}

	Mod IMod.GetConfig()
	{
		return GetBaseConfig().ItemUnion<Mod>().Value;
	}
}