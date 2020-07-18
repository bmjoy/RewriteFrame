using Eternity.FlatBuffer;
using PureMVC.Patterns.Proxy;
using UnityEngine.Assertions;

public partial class CfgEternityProxy : Proxy
{

    /// <summary>
    /// 获取属性数据
    /// </summary>
    /// <param name="tid">tid</param>
    /// <returns></returns>
    public UserAttribute? GetUserAttribute(int tid)
    {
        return m_Config.UserAttributesByKey((uint)tid);
    }

    /// <summary>
    /// 获取武器属性
    /// </summary>
    /// <param name="tid">tid</param>
    /// <returns></returns>
    public WeaponAttrData?[] GetWeaponAttrDatasByKey(uint tid)
    {
        WeaponAttr? weaponAttrVO = m_Config.WeaponAttrsByKey(tid).Value;
        Assert.IsTrue(weaponAttrVO.HasValue, "CfgEternityProxy => WeaponAttrsByKey not exist tid " + tid);
        WeaponAttrData?[] weaponAttrData = new WeaponAttrData?[weaponAttrVO.Value.AttrsLength];
        for (int i = 0; i < weaponAttrVO.Value.AttrsLength; i++)
        {
            weaponAttrData[i] = weaponAttrVO.Value.Attrs(i);
        }
        return weaponAttrData;
    }

    /// <summary>
    /// 获取此装备的属性
    /// </summary>
    /// <param name="tid"></param>
    /// <returns></returns>
    public EquipmentAttrData?[] GetEquipmentAttrDataByKey(uint tid)
    {
        Equip? equip = m_Config.EquipsByKey(tid);
        Assert.IsTrue(equip.HasValue, "CfgEternityProxy => GetEquipByKey not exist tid " + tid);
        EquipmentAttrData?[] Arraydate = new EquipmentAttrData?[equip.Value.Attr.Value.AttrsLength];

        for (int i = 0; i < equip.Value.Attr.Value.AttrsLength; i++)
        {
            Arraydate[i] = equip.Value.Attr.Value.Attrs(i);
        }
        return Arraydate;
    }

    /// <summary>
    /// 获取船属性
    /// </summary>
    /// <param name="tid">tid</param>
    /// <returns></returns>
    public WarshipAttrData?[] GetWarshipAttrDatasByKey(uint tid)
    {
        Warship? warship = m_Config.WarshipsByKey(tid);
        Assert.IsTrue(warship.HasValue, "CfgEternityProxy => WarshipsByKey not exist tid " + tid);
        WarshipAttrData?[] warshipAttrDatas = new WarshipAttrData?[warship.Value.WarshipAttrData.Value.AttrsLength];
        for (int i = 0; i < warshipAttrDatas.Length; i++)
        {
            warshipAttrDatas[i] = warship.Value.WarshipAttrData.Value.Attrs(i);
        }
        return warshipAttrDatas;
    }

}