/*===============================
 * Author: [Allen]
 * Purpose: Icon基类
 * Time: 2019/4/13 17:42:57
================================*/

using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class AbstractIconBase : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IMoveHandler, IPointerDownHandler, IPointerUpHandler
{
    #region  鼠标事件
    /// <summary>
    /// 左键点击
    /// </summary>
    public Action<AbstractIconBase> OnClick;

    /// <summary>
    /// 右键点击
    /// </summary>
    public Action<AbstractIconBase> OnRightClick;

    /// <summary>
    /// 鼠标进入
    /// </summary>
    public Action<AbstractIconBase> OnEnter;

    /// <summary>
    /// 鼠标移出
    /// </summary>
    public Action<AbstractIconBase> OnExit;

    /// <summary>
    /// 鼠标移动
    /// </summary>
    public Action<AbstractIconBase> OnMove;

    /// <summary>
    /// 左键按下
    /// </summary>
    public Action<AbstractIconBase> OnLeftDown;

    /// <summary>
    /// 右键按下
    /// </summary>
    public Action<AbstractIconBase> OnRightDown;

    /// <summary>
    /// 左键抬起
    /// </summary>
    public Action<AbstractIconBase> OnLeftUp;

    /// <summary>
    /// 右键抬起
    /// </summary>
    public Action<AbstractIconBase> OnRightUp;
	#endregion

    #region 变量
	/// <summary>
	/// Icon 图片
	/// </summary>
	protected Image m_Icon;

	/// <summary>
	/// 品质图片
	/// </summary>
	protected Image m_Quality;

	/// <summary>
	/// 不可用背景Image
	/// </summary>
	protected Image m_BlackImage;

	/// <summary>
	/// Icon 预制体名字
	/// </summary>
	private string m_AssetIconName = "";
	#endregion

	#region 使用方法
	//IconManager.Instance.LoadItemIcon<IconBluePrint>(IconName.ICON_BLUEPRINT, parent,
	//				(icon) =>
	//				{
	//					icon.SetData(tid, num);
	//				});
	#endregion

	/// <summary>
	/// 获取Icon预制体名字
	/// </summary>
	/// <returns>预制体名字</returns>
	public string GetIconName()
    {
        return m_AssetIconName;
    }

    /// <summary>
    /// 设置预制体名字，并初始化组件
    /// </summary>
    /// <param name="iconName"></param>
    public void SetIconNameAndInitialize(string iconName)
    {
        m_AssetIconName = iconName;
        Initialize();
    }

    /// <summary>
    /// 初始化，通常初始化相关组件
    /// </summary>
    public abstract void Initialize();

    /// <summary>
    /// 回收，释放
    /// </summary>
    public abstract void Release();

	/// <summary>
	/// 设置是否可用
	/// </summary>
	/// <param name="isUse">是否可用</param>
	public virtual void SetUseState(bool isUse)
	{
		m_BlackImage.gameObject.SetActive(!isUse);
	}

	/// <summary>
	/// 字符串首字母大写（图集是大写的 表里的是小写的）
	/// </summary>
	/// <param name="str">更改的字符串</param>
	/// <returns></returns>
	public string FirstCharToUpper(string str)
	{
		if (String.IsNullOrEmpty(str))
			throw new ArgumentException("NULL");
		return str.Substring(0, 1).ToUpper() + str.Substring(1); 
	}

	#region ---------------------------------------event-------------------------------------------------------------
	void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
            OnClick?.Invoke(this);
        if (eventData.button == PointerEventData.InputButton.Right)
            OnRightClick?.Invoke(this);
    }

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        OnEnter?.Invoke(this);
    }
    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        OnExit?.Invoke(this);
    }

    void IMoveHandler.OnMove(AxisEventData eventData)
    {
        OnMove?.Invoke(this);
    }

    void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
            OnLeftDown?.Invoke(this);
        if (eventData.button == PointerEventData.InputButton.Right)
            OnRightDown?.Invoke(this);
    }

    void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
            OnLeftUp?.Invoke(this);
        if (eventData.button == PointerEventData.InputButton.Right)
            OnRightUp?.Invoke(this);
    }
    #endregion ----------------------------------end-----event-------------------------------------------------------------
}

