using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 星域item
/// </summary>
public class StarmapPointElementBase : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler
{
	private static Vector3 ICON_SCALE_S = new Vector3(0.6f, 0.6f, 1);
	private static Vector3 ICON_SCALE_L = new Vector3(0.8f, 0.8f, 1);
	/// <summary>
	/// 游戏数据proxy
	/// </summary>
	private CfgEternityProxy m_CfgEternityProxy;
	/// <summary>
	/// 显示内容根节点
	/// </summary>
	private RectTransform m_AutoSizeBox;
	/// <summary>
	/// 图标parent
	/// </summary>
	private GameObject m_IconGo;
	/// <summary>
	/// 图标
	/// </summary>
	private Image m_Icon;
	/// <summary>
	/// 下方图标根节点
	/// </summary>
	private GameObject m_BottomIconGo;
	/// <summary>
	/// 下方图标parent
	/// </summary>
	private RectTransform m_BottomIconParent;
	/// <summary>
	/// 模型图容器
	/// </summary>
	private GameObject m_RawGo;
	/// <summary>
	/// 模型图显示图片
	/// </summary>
	private RawImage m_RawImage;
	/// <summary>
	/// 名称
	/// </summary>
	private TMP_Text m_NameLabel;
	/// <summary>
	/// 当前所在区域点标志
	/// </summary>
	private GameObject m_LocationPoint;
	/// <summary>
	/// 是否已经初始化
	/// </summary>
	private bool m_Inited;

	/// <summary>
	/// 初始化
	/// </summary>
	protected virtual void Initialize()
	{
		if (m_Inited)
		{
			return;
		}
		m_Inited = true;

		m_CfgEternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;

		m_AutoSizeBox = TransformUtil.FindUIObject<RectTransform>(transform, "ImagePoint/Box");
		m_IconGo = TransformUtil.FindUIObject<Transform>(transform, "ImagePoint/Box/Icon").gameObject;
		m_Icon = TransformUtil.FindUIObject<Image>(transform, "ImagePoint/Box/Icon/Image_Icon");
		m_RawGo = TransformUtil.FindUIObject<Transform>(transform, "ImagePoint/Box/Raw").gameObject;
		m_RawImage = TransformUtil.FindUIObject<RawImage>(transform, "ImagePoint/Box/Raw/back");
		m_NameLabel = TransformUtil.FindUIObject<TMP_Text>(transform, "ImagePoint/Box/Name/TextBox/Name");
		m_LocationPoint = TransformUtil.FindUIObject<Transform>(transform, "ImagePoint/Box/Location").gameObject;
		m_BottomIconGo = TransformUtil.FindUIObject<Transform>(transform, "ImagePoint/Box/Image").gameObject;
		m_BottomIconParent = TransformUtil.FindUIObject<RectTransform>(transform, "ImagePoint/Box/Image/Icon/Type1");


		m_BottomIconGo.SetActive(false);
		m_IconGo.SetActive(false);
		m_RawGo.SetActive(false);
		m_LocationPoint.SetActive(false);

		UIEventListener.UIEventListener.AttachListener(gameObject).onSelect = OnToggleChanged;
	}

	/// <summary>
	/// 设置icon图片（icon和raw一个item只显示其中一个）
	/// </summary>
	protected void SetToIcon()
	{
		m_IconGo.SetActive(true);
		m_RawGo.SetActive(false);
		m_AutoSizeBox.sizeDelta = m_IconGo.GetComponent<RectTransform>().sizeDelta;
	}

	/// <summary>
	/// 设置模型图
	/// </summary>
	/// <param name="size"></param>
	protected void SetToRaw(Vector2 size)
	{
		m_IconGo.SetActive(false);
		m_RawGo.SetActive(true);
		m_RawImage.gameObject.SetActive(false);
		m_RawImage.rectTransform.sizeDelta = size;
		m_AutoSizeBox.sizeDelta = size;
	}

	/// <summary>
	/// 设置名称
	/// </summary>
	/// <param name="name"></param>
	protected void SetName(string name)
	{
		m_NameLabel.text = TableUtil.GetLanguageString(name);

		if (string.IsNullOrEmpty(m_NameLabel.text))
		{
			m_NameLabel.text = name;
		}
	}

	/// <summary>
	/// 设置区域点显示隐藏
	/// </summary>
	/// <param name="visible">当前item是否被选中</param>
	protected void SetLocationVisible(bool visible)
	{
		m_LocationPoint.SetActive(visible);
	}

	/// <summary>
	/// 设置当前item选中
	/// </summary>
	/// <param name="isOn"></param>
	protected void SetSelectState(bool isOn)
	{
		if (isOn)
		{
			SetToggleIsOn(isOn);
		}
	}

	/// <summary>
	/// 选中状态修改
	/// </summary>
	/// <param name="isOn"></param>
	public void SetToggleIsOn(bool isOn)
	{
		GetComponent<Toggle>().isOn = isOn;
		GetComponent<Animator>()?.SetBool("IsOn", isOn);
		GetComponent<Animator>()?.SetTrigger("Normal");
	}

	/// <summary>
	/// 设置上方主图标
	/// </summary>
	/// <param name="icon"></param>
	/// <param name="isBig"></param>
	public void SetTopIcon(uint icon, bool isBig)
	{
		m_IconGo.GetComponent<RectTransform>().localScale = isBig ? ICON_SCALE_L : ICON_SCALE_S;
		UIUtil.SetIconImage(m_Icon, icon);
	}

	/// <summary>
	/// 设置下方图标
	/// </summary>
	/// <param name="icons"></param>
	public void SetBottomIcon(params uint[] icons)
	{
		Image[] images = m_BottomIconParent.GetComponentsInChildren<Image>();
		for (int i = 0; i < images.Length; i++)
		{
			images[i].sprite = null;
			images[i].gameObject.SetActive(false);
		}
		for (int i = 0; i < icons?.Length; i++)
		{
			Image image;
			if (images.Length <= i)
			{
				image = Instantiate(images[0], m_BottomIconParent);
				image.sprite = null;
				image.name = icons[i].ToString();
			}
			else
			{
				image = images[i];
			}
			UIUtil.SetIconImage(image, icons[i], true);
			image.gameObject.SetActive(true);
		}

		m_BottomIconGo.SetActive(icons?.Length > 0);
	}

	/// <summary>
	/// 修改选中状态回调
	/// </summary>
	/// <param name="go"></param>
	/// <param name="args"></param>
	protected void OnToggleChanged(GameObject go, params object[] args)
	{
		OnSelected?.Invoke(this);
	}

	/// <summary>
	/// 点击回调
	/// </summary>
	/// <param name="eventData"></param>
	public void OnPointerClick(PointerEventData eventData)
	{
		OnClick?.Invoke(this);
	}

	public virtual void Destroy()
	{
		UIEventListener.UIEventListener.AttachListener(gameObject).onSelect = null;
		GetComponent<Toggle>().onValueChanged.RemoveAllListeners();
		OnSelected = null;
		OnClick = null;
		OnEnter = null;
		Destroy(this);
	}

	/// <summary>
	/// 获取模型图显示图片
	/// </summary>
	/// <returns></returns>
	protected RawImage GetRawImage()
	{
		return m_RawImage;
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (InputManager.Instance.CurrentInputDevice != InputManager.GameInputDevice.KeyboardAndMouse)
		{
			OnEnter?.Invoke(this);
		}
	}

	/// <summary>
	/// 选中事件
	/// </summary>
	public UnityAction<StarmapPointElementBase> OnSelected { get; set; }

	/// <summary>
	/// 点击事件
	/// </summary>
	public UnityAction<StarmapPointElementBase> OnClick { get; set; }

	/// <summary>
	/// 移入
	/// </summary>
	public UnityAction<StarmapPointElementBase> OnEnter { get; set; }

}