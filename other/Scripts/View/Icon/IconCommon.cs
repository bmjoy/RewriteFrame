using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IconCommon : AbstractIconBase
{
	/// <summary>
	/// Icon资源地址
	/// </summary>
	private uint m_IconNameTid;

	/// <summary>
	/// Icon数量
	/// </summary>
	private TextMeshProUGUI m_IconNumber;

	/// <summary>
	/// Icon数量
	/// </summary>
	private int m_IconQuality;

	public override void Initialize()
	{
		m_Icon = TransformUtil.FindUIObject<Image>(transform, "Icon");
		m_Quality = TransformUtil.FindUIObject<Image>(transform, "Quality");
		m_BlackImage = TransformUtil.FindUIObject<Image>(transform, "Black");
		m_IconNumber = TransformUtil.FindUIObject<TextMeshProUGUI>(transform, "label_Num");
		SetUseState(true);
	}

	/// <summary>
	/// 设置Icon  跟 品质色
	/// </summary>
	private void SetIconAndQuality()
	{
		UIUtil.SetIconImageSquare(m_Icon, m_IconNameTid);
		m_Quality.color = ColorUtil.GetColorByItemQuality(m_IconQuality);
	}

	public override void Release()
	{
	}

	/// <summary>
	/// 设置数据
	/// </summary>
	/// <param name="iconName">Icon 地址</param>
	/// <param name="quality">品质</param>
	/// <param name="num">数量</param>
	public void SetData(uint iconName,int quality, int num)
	{
		m_IconNumber.text = num.ToString();
		m_IconNameTid = iconName;
		m_IconQuality = quality;
		SetIconAndQuality();
	}

}
