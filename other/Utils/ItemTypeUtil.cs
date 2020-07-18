using Eternity.FlatBuffer;
using Eternity.Runtime.Item;
using ProtoSharp;
using System;
using System.Collections.Generic;
using System.Reflection;



public static class ItemTypeUtil
{
	private static Dictionary<uint, ItemType> m_ItemTypeDic = new Dictionary<uint, ItemType>();

	/// <summary>
	/// 获取一个道具的类型结构
	/// </summary>
	/// <param name="type">Item.Type</param>
	/// <returns></returns>
	public static ItemType GetItemType(uint type)
	{
		m_ItemTypeDic.TryGetValue(type, out ItemType value);
		if (value == null)
		{
			value = CreateItemType(type);
			m_ItemTypeDic.Add(type, value);
		}
		return value;
	}

	/// <summary>
	/// 设置子类型数据
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="value"></param>
	/// <param name="itemType"></param>
	public static void SetSubType<T>(ref T value, ItemType itemType) where T : Enum
	{
		for (int i = 0; i < itemType.EnumList.Length; i++)
		{
			if (itemType.EnumList[i] != null)
			{
				if (itemType.EnumList[i] is T)
				{
					value = (T)itemType.EnumList[i];
					return;
				}
			}
		}
	}

	/// <summary>
	/// 获取包的具体类型
	/// </summary>
	/// <param name="item"></param>
	/// <returns></returns>
	public static string GetContainerType(Item item)
	{
		string s = "";
		if (item.ContainerData.HasValue)
		{
			Eternity.FlatBuffer.Container c = item.ContainerData.Value;
			for (int i = 0; i < c.PositionsLength; i++)
			{
				for (int j = 0; j < c.Positions(i).Value.BagTypesLength; j++)
				{
					if (GetItemType(c.Positions(i).Value.BagTypes(j)).MainType == Category.Equipment)
					{
						s += GetItemType(c.Positions(i).Value.BagTypes(j)).MainType.ToString();
						return s;
					}
					else if (GetItemType(c.Positions(i).Value.BagTypes(j)).MainType == Category.Weapon)
					{
						s += GetItemType(c.Positions(i).Value.BagTypes(j)).MainType.ToString();
						return s;
					}
					else if (GetItemType(c.Positions(i).Value.BagTypes(j)).MainType == Category.EquipmentMod)
					{
						for (int k = 1; k < GetItemType(c.Positions(i).Value.BagTypes(j)).EnumList.Length; k++)
						{
							if (GetItemType(c.Positions(i).Value.BagTypes(j)).EnumList[k] != null)
							{
								s += GetItemType(c.Positions(i).Value.BagTypes(j)).EnumList[k];
							}
						}
					}
					else
					{
						s += GetItemType(c.Positions(i).Value.BagTypes(j)).MainType.ToString();
					}
				}
			}
		}
		return s.Replace("0", "");
	}

	/// <summary>
	/// 获取武器类型. 导弹, 激光等
	/// </summary>
	/// <returns>小于0是非法值. 可用的值可以转换为 WeaponL1 类型</returns>
	public static int GetWeaponType(IWeapon weapon)
	{
		ItemType itemType = ItemTypeUtil.GetItemType(weapon.GetBaseConfig().Type);
		if (itemType.MainType == Category.Weapon)
		{
			WeaponL1 weaponL1 = (WeaponL1)0;
			SetSubType(ref weaponL1, itemType);
			return (int)weaponL1;
		}
		else
		{
			return -1;
		}
	}

	#region 构建ItemType数据
	/// <summary>
	/// 解析type数据
	/// </summary>
	/// <param name="type"></param>
	/// <returns></returns>
	private static ItemType CreateItemType(uint type)
	{
		ItemType itemType = new ItemType();
        itemType.NativeType = type;

		uint[] intType = new uint[5]
		{
			type << 7 >> 27,
			type << (7 + 5) >> 27,
			type << (7 + 5 + 5) >> 27,
			type << (7 + 5 + 5 + 5) >> 27,
			type << (7 + 5 + 5 + 5 + 5) >> 27
		};

		itemType.RootType = (RootCategory)intType[0];
		itemType.MainType = (Category)intType[1];

        int count = 0;
        for (int i = intType.Length - 1; i >= 0; i--)
        {
            if (intType[i] != 0)
            {
                count = i + 1;
                break;
            }
        }

		itemType.EnumList = new object[count];


		Type rootType = typeof(RootCategory);
		for (int i = 0; i < count; i++)
		{
            itemType.EnumList[i] = Enum.ToObject(rootType, intType[i]);

			Type t = GetMatchField(intType[i], rootType);
			if (t == null)
			{
				t = GetMatchEnum(rootType);
			}
			if (t == null)
			{
				break;
			}
			rootType = t;
		}

		return itemType;
	}

	/// <summary>
	/// 反射获取子类型
	/// </summary>
	/// <param name="value"></param>
	/// <param name="type2find"></param>
	/// <returns></returns>
	private static Type GetMatchField(uint value, Type type2find)
	{
		Type t = null;
		ItemTypeRelationshipAttribute attrib;
		FieldInfo[] fields = type2find.GetFields();

		for (int i = 0; i < fields.Length; i++)
		{
			FieldInfo info = fields[i];
			attrib = fields[i].GetCustomAttribute<ItemTypeRelationshipAttribute>();
			if (attrib != null)
			{
				if ((uint)info.GetValue(info) == value)
				{
					t = attrib.relationship_type;
					break;
				}
			}
		}
		return t;
	}

	/// <summary>
	/// 反射获取子类型
	/// </summary>
	/// <param name="type"></param>
	/// <returns></returns>
	private static Type GetMatchEnum(Type type)
	{
		return type.GetCustomAttribute<ItemTypeRelationshipAttribute>()?.relationship_type;
	}
	#endregion
}

/// <summary>
/// 道具类型结构化数据
/// </summary>
public class ItemType
{
    /// <summary>
    /// 原始类型
    /// </summary>
    public uint NativeType;
	/// <summary>
	/// 道具基础类型
	/// </summary>
	public RootCategory RootType;
	/// <summary>
	/// 道具主类型
	/// </summary>
	public Category MainType;
	/// <summary>
	/// 5层类型数据枚举值
	/// 包含 RootCategory ， Category
	/// </summary>
	public object[] EnumList;
}