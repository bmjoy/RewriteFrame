using Eternity.FlatBuffer;
using UnityEngine;
using UnityEngine.UI;

public static class UIUtil
{
	/// <summary>
	/// 设置image 
	/// </summary>
	/// <param name="image">image </param>
	/// <param name="iconBundle">iconBundle地址</param>
	/// <param name="iconName">iconName</param>
	public static void SetIconImage(Image image, string iconBundle, string iconName, bool autoSetNativeSize = false)
	{
		UIManager.Instance.GetUISprite(iconBundle, iconName, (sprite) =>
		{
            if(image !=null && sprite !=null)
            {
                image.sprite = sprite;
                if (autoSetNativeSize)
                {
                    image.SetNativeSize();
                }
            }
		});
	}

	/// <summary>
	/// 设置image 
	/// </summary>
	/// <param name="image">image</param>
	/// <param name="IconId">Icon表Tid</param>
	public static void SetIconImage(Image image, uint IconId, bool autoSetNativeSize = false)
	{
		CfgEternityProxy cfgEternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
		Icon icon = cfgEternityProxy.GetIconName(IconId);
		SetIconImage(image, icon.Atlas, icon.AssetName, autoSetNativeSize);
	}

	/// <summary>
	/// 设置Icon image方图 
	/// </summary>
	/// <param name="image">image</param>
	/// <param name="IconId">Icon表Tid</param>
	public static void SetIconImageSquare(Image image, uint IconId, bool autoSetNativeSize = false)
	{
		CfgEternityProxy cfgEternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
		Icon icon = cfgEternityProxy.GetIconName(IconId);
		SetIconImage(image, icon.Atlas, icon.SquareName, autoSetNativeSize);
	}

	/// <summary>
	/// 根据品质，替换图片的品质背景图
	/// </summary>
	public static void SetQualityBackgroundImage(Image backgroundImage, int quality, bool autoSetNativeSize = false)
	{
		string[] ImageArray = new string[] { "", "Quality_Tips1", "Quality_Tips2", "Quality_Tips3", "Quality_Tips4", "Quality_Tips5", "Quality_Tips6", "Quality_Tips7", "Quality_Tips8" };
		if (quality < 0 || quality >= ImageArray.Length)
		{
			Debug.LogError(" 品质值错误 quality =" + quality);
			return;
		}
		SetIconImage(backgroundImage, GameConstant.COMMON_ATLAS_ASSETADDRESS, ImageArray[quality], autoSetNativeSize);
	}

}