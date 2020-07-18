using System.Collections.Generic;

public class ShipContainer : IWeaponContainer, IReformerContainer, IEquipmentContainer, IModContainer, ISkillContainer
{
	private ulong m_Reference;
	private ulong m_Uid;
	private uint m_Tid;
	private uint m_CurrentSizeMax;

	private List<ShipItem> m_Items = new List<ShipItem>();

	#region Set
	public void SetUID(ulong value)
	{
		m_Uid = value;
	}

	public void SetTID(uint value)
	{
		m_Tid = value;
	}

	public void SetCurrentSizeMax(uint value)
	{
		m_CurrentSizeMax = value;
	}

	public void SetReference(ulong value)
	{
		m_Reference = value;
	}

	public ulong GetReference()
	{
		return m_Reference;
	}

	public void AddItem(ShipItem value)
	{
		m_Items.Add(value);
	}

	public void RemoveItem(ShipItem item)
	{
		m_Items.Remove(item);
	}
	#endregion

	#region BaseInterface
	public uint GetTID()
	{
		return m_Tid;
	}

	public ulong GetUID()
	{
		return m_Uid;
	}

	public uint GetCurrentSizeMax()
	{
		return m_CurrentSizeMax;
	}
	#endregion

	IWeapon[] IWeaponContainer.GetWeapons()
	{
		if (m_Items != null)
		{
			return m_Items.ToArray() as IWeapon[];
		}
		return null;
	}

	IReformer IReformerContainer.GetReformer()
	{
		if (m_Items != null && m_Items.Count > 0)
		{
			return m_Items[0] as IReformer;
		}
		return null;
	}

	IEquipment[] IEquipmentContainer.GetEquipments()
	{
		if (m_Items != null)
		{
			return m_Items.ToArray() as IEquipment[];
		}
		return null;
	}

	IMod[] IModContainer.GetMods()
	{
		if (m_Items != null)
		{
			return m_Items.ToArray() as IMod[];
		}
		return null;
	}

	ISkill[] ISkillContainer.GetSkills()
	{
		if (m_Items != null)
		{
			return m_Items.ToArray() as ISkill[];
		}
		return null;
	}
}