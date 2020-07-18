using Eternity.FlatBuffer;
using FlatBuffers;
using PureMVC.Patterns.Proxy;
using UnityEngine;

public partial class CfgEternityProxy : Proxy
{
	public Weapon GetWeapon(uint itemID)
	{
#if UNITY_EDITOR
		if (!m_Config.WeaponsByKey(itemID).HasValue)
			Debug.LogErrorFormat("找不到itemID为 {0} 的武器", itemID);
#endif
		return m_Config.WeaponsByKey(itemID).Value;
	}

	/// <summary>
	/// 获取霰弹枪的武器属性
	/// </summary>
	/// <param name="weaponDataID">即 Weapon.TypeDateSheetld</param>
	/// <returns></returns>
	public WeaponShotgun GetWeaponDataOfShotgun(uint weaponDataID)
	{
#if UNITY_EDITOR
		if (!m_Config.WeaponShotgunsByKey(weaponDataID).HasValue)
			Debug.LogErrorFormat("找不到 weaponDataID 为 {0} 的武器数据", weaponDataID);
#endif
		return m_Config.WeaponShotgunsByKey(weaponDataID).Value;
	}

	/// <summary>
	/// 获取导弹的武器属性
	/// </summary>
	/// <param name="weaponDataID">即 Weapon.TypeDateSheetld</param>
	/// <returns></returns>
	public WeaponMissile GetWeaponDataOfMissile(uint weaponDataID)
	{
#if UNITY_EDITOR
		if (!m_Config.WeaponMissilesByKey(weaponDataID).HasValue)
			Debug.LogErrorFormat("找不到 weaponDataID 为 {0} 的武器数据", weaponDataID);
#endif
		return m_Config.WeaponMissilesByKey(weaponDataID).Value;
	}

	/// <summary>
	/// 获取速射枪的武器属性
	/// </summary>
	/// <param name="weaponDataID">即 Weapon.TypeDateSheetld</param>
	/// <returns></returns>
	public WeaponRapidFirer GetWeaponDataOfMachineGun(uint weaponDataID)
	{
#if UNITY_EDITOR
		if (!m_Config.WeaponRapidFirersByKey(weaponDataID).HasValue)
			Debug.LogErrorFormat("找不到 weaponDataID 为 {0} 的武器数据", weaponDataID);
#endif
		return m_Config.WeaponRapidFirersByKey(weaponDataID).Value;
	}

    /// <summary>
    /// 获取矿抢的武器属性
    /// </summary>
    /// <param name="weaponDataID">即 Weapon.TypeDateSheetld</param>
    /// <returns></returns>
    public WeaponMiningLaser GetWeaponDataOfMining(uint weaponDataID)
    {
#if UNITY_EDITOR
        if (!m_Config.WeaponMiningLasersByKey(weaponDataID).HasValue)
            Debug.LogErrorFormat("找不到 weaponDataID 为 {0} 的武器数据", weaponDataID);
#endif
        return m_Config.WeaponMiningLasersByKey(weaponDataID).Value;
    }

}