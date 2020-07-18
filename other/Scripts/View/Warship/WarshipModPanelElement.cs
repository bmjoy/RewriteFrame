using Eternity.Runtime.Item;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WarshipModPanelElement : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
	/// <summary>
	/// Icon组件
	/// </summary>
	private Image m_Icon;

	/// <summary>
	/// 是否初始化
	/// </summary>
	private bool m_Inited;

	/// <summary>
	/// 点击时间
	/// </summary>
	private float m_ClickTime;

	/// <summary>
	/// 数据
	/// </summary>
	private CfgEternityProxy m_CfgEternityProxy;
	/// <summary>
	/// 装备Mod类型
	/// </summary>
	private EquipmentModL1 m_ModType1;
	/// <summary>
	/// 装备Mod类型
	/// </summary>
	private EquipmentModL2 m_ModType2;
	/// <summary>
	/// 背包UId
	/// </summary>
	private ulong m_ContainerUid;
	/// <summary>
	/// 位置标记
	/// </summary>
	private int m_Pos;
	/// <summary>
	///  单体Uid
	/// </summary>
	private ulong m_Uid;
	/// <summary>
	///  单体Tid
	/// </summary>
	private uint m_Tid;
	/// <summary>
	/// 数据
	/// </summary>
	private IMod m_Data;
	/// <summary>
	/// 初始化
	/// </summary>
	private void Initialize()
	{
		if (m_Inited)
		{
			return;
		}
		m_Inited = true;
		m_CfgEternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
		m_Icon = TransformUtil.FindUIObject<Image>(transform, "Icon");
	}

	/// <summary>
	/// 设置基础数据
	/// </summary>
	/// <param name="modType1">mod类型1</param>
	/// <param name="modType2">mod类型2</param>
	/// <param name="containerUid">背包uid</param>
	/// <param name="pos">位置标记</param>
	public void SetBaseData(EquipmentModL1 modType1, EquipmentModL2 modType2, ulong containerUid, int pos)
	{
		Initialize();
		m_ModType1 = modType1;
		m_ModType2 = modType2;
		m_ContainerUid = containerUid;
		m_Pos = pos;
	}

	/// <summary>
	/// 设置Mod数据
	/// </summary>
	/// <param name="data">数据</param>
	public void SetData(IMod data)
	{
		Initialize();
		m_Data = data;
		SetData(data.GetUID(), data.GetTID());
	}
	/// <summary>
	/// 设置Mod数据(重载)
	/// </summary>
	/// <param name="uid">uid</param>
	/// <param name="tid">tid</param>
	private void SetData(ulong uid, uint tid)
	{
		m_Uid = uid;
		m_Tid = tid;

		m_Icon.color = ColorUtil.GetColorByItemQuality(m_CfgEternityProxy.GetItemByKey(tid).Quality);
		m_Icon.enabled = true;
	}
	/// <summary>
	/// 清理
	/// </summary>
	public void Cleanup()
	{
		Initialize();
		m_Icon.enabled = false;
		m_Data = null;
		m_ModType1 = 0;
		m_ModType2 = 0;
		m_ContainerUid = 0;
		m_Pos = 0;
		m_Uid = 0;
		m_Tid = 0;
	}
	/// <summary>
	/// 获取装备枚举类型1
	/// </summary>
	/// <returns></returns>
	public EquipmentModL1 GetEquipmentModL1()
	{
		return m_ModType1;
	}
	/// <summary>
	/// 获取装备枚举类型2
	/// </summary>
	/// <returns></returns>
	public EquipmentModL2 GetEquipmentModL2()
	{
		return m_ModType2;
	}
	/// <summary>
	/// 是否有数据
	/// </summary>
	/// <returns></returns>
	public bool HasData()
	{
		return m_Data != null;
	}
	/// <summary>
	/// 获取modUid
	/// </summary>
	/// <returns></returns>
	public ulong GetModUid()
	{
		return m_Uid;
	}
	/// <summary>
	/// 获取modTid
	/// </summary>
	/// <returns></returns>
	public uint GetModTid()
	{
		return m_Tid;
	}
	/// <summary>
	/// 获取包的UID
	/// </summary>
	/// <returns></returns>
	public ulong GetContainerUID()
	{
		return m_ContainerUid;
	}
	/// <summary>
	/// 获取包的位置
	/// </summary>
	/// <returns></returns>
	public int GetContainerPOS()
	{
		return m_Pos;
	}
	/// <summary>
	/// 获取等级
	/// </summary>
	/// <returns></returns>
	public int GetLv()
	{
		return m_Data?.GetLv() ?? 0;
	}

	/// <summary>
	/// 获取数据
	/// </summary>
	/// <returns></returns>
	public IMod GetData()
	{
		return m_Data;
	}

	/// <summary>
	/// 选择(焦点)
	/// </summary>
	/// <param name="eventData"></param>
	public void OnSelect(BaseEventData eventData)
	{
		gameObject.GetComponent<Toggle>().isOn = true;
		OnSelected?.Invoke(this);
	}

	/// <summary>
	/// 取消选中(焦点)
	/// </summary>
	/// <param name="eventData"></param>
	public void OnDeselect(BaseEventData eventData)
	{
		gameObject.GetComponent<Toggle>().isOn = false;
		OnDeselected?.Invoke(this);
	}


	/// <summary>
	/// 点击（焦点）
	/// </summary>
	/// <param name="eventData"></param>
	public void OnPointerClick(PointerEventData eventData)
	{
		if (eventData.clickTime - m_ClickTime <= 0.5)
		{
			OnDoubleClicked?.Invoke(this);
		}
		m_ClickTime = eventData.clickTime;
	}

	private void OnDestroy()
	{
		Cleanup();
		OnSelected = null;
		OnDeselected = null;
		OnDoubleClicked = null;
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (InputManager.Instance.CurrentInputDevice != InputManager.GameInputDevice.KeyboardAndMouse)
		{
			gameObject.GetComponent<Toggle>().isOn = true;
			OnSelected?.Invoke(this);
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (InputManager.Instance.CurrentInputDevice != InputManager.GameInputDevice.KeyboardAndMouse)
		{
			OnDeselected?.Invoke(this);
		}
	}


	/// <summary>
	/// 选中事件
	/// </summary>
	public UnityAction<WarshipModPanelElement> OnSelected;
	/// <summary>
	/// 离开事件
	/// </summary>
	public UnityAction<WarshipModPanelElement> OnDeselected;
	/// <summary>
	/// 点击事件
	/// </summary>
	public UnityAction<WarshipModPanelElement> OnDoubleClicked;
}