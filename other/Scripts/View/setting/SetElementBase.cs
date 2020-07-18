using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// setElement基类
/// </summary>
public class SetElementBase : MonoBehaviour
{
	/// <summary>
	/// 设置类型
	/// </summary>
	private OptionType m_OptionType;
	public void SetOptionType(OptionType value)
	{
		m_OptionType = value;
	}
	public OptionType GetOptionType()
	{
		return m_OptionType;
	}
	/// <summary>
	/// 是否是DropDown
	/// </summary>
	private bool m_IsDropDown = false;
	public void SetIsDropDown(bool value)
	{
		m_IsDropDown = value;
	}

	/// <summary>
	/// GroupScrollerView
	/// </summary>
	public GroupScrollerView m_GroupScrollerView;

	/// <summary>
	/// 左按钮点击
	/// </summary>
	public virtual void LeftButtonClick()
	{

	}
	/// <summary>
	/// 右按钮点击
	/// </summary>
	public virtual void RightButtonClick()
	{

	}

	/// <summary>
	/// 点击事件
	/// </summary>
	/// <param name="go">物体</param>
	/// <param name="objs">参数</param>
	protected virtual void OnClick(GameObject go, object[] objs)
	{
		Vector2Int vector = (Vector2Int)objs[0];
		m_GroupScrollerView.SetSelection(vector);
		m_GroupScrollerView.ScrollToSelection();
	}
}
