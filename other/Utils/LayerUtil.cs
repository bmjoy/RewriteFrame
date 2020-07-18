using Assets.Scripts.Define;
using UnityEngine;

public class LayerUtil
{
	public const int MAX_LAYER_COUNT = 32;

	private static int LayersCanCollideWithSkillProjectile = 0;
	/// <summary>
	/// 获取与技能投射物相交的所有层
	/// </summary>
	/// <returns></returns>
	public static int GetLayersIntersectWithSkillProjectile(bool ignoreMainPlayer)
	{
		if (LayersCanCollideWithSkillProjectile == 0)
		{
			for (int iLayer = 0; iLayer < MAX_LAYER_COUNT; iLayer++)
			{
				string layerName = LayerMask.LayerToName(iLayer);
				if (string.IsNullOrEmpty(layerName))
					continue;

				if (!Physics.GetIgnoreLayerCollision(iLayer, GameConstant.LayerTypeID.SkillProjectile))
				{
					LayersCanCollideWithSkillProjectile |= 1 << iLayer;
				}
			}

			if (ignoreMainPlayer)
			{
				LayersCanCollideWithSkillProjectile &= ~(1 << GameConstant.LayerTypeID.MainPlayer);
			}
		}

		return LayersCanCollideWithSkillProjectile;
	}

	/// <summary>
	/// 根据Entity的 HeroType 和 是否是主角, 决定他的Layer
	/// </summary>
	/// <param name="heroType">Entity.GetHeroType()</param>
	/// <param name="isMain">是不是主角</param>
	/// <returns></returns>
	public static int GetLayerByHeroType(KHeroType heroType, bool isMain)
	{
		return heroType == KHeroType.htPlayer
				? isMain
					? GameConstant.LayerTypeID.MainPlayer
					: GameConstant.LayerTypeID.SpacecraftOtherPlayer
				: heroType == KHeroType.htReliveNpc
					? GameConstant.LayerTypeID.InvisibleFunctionalNPC
					: GameConstant.LayerTypeID.SpacecraftNPC;
	}

	/// <summary>
	/// 设置go到某layer
	/// </summary>
	/// <param name="go"></param>
	/// <param name="layerId"><seealso cref="GameConstant.LayerTypeID"/></param>
	/// <param name="changeChildren"></param>
	public static void SetGameObjectToLayer(GameObject go, int layerId, bool changeChildren)
	{
		go.layer = layerId;
		if (changeChildren)
		{
			Transform[] childList = go.GetComponentsInChildren<Transform>(true);
			for (int i = 0; i < childList.Length; i++)
			{
				childList[i].gameObject.layer = layerId;
			}
		}
	}
}

