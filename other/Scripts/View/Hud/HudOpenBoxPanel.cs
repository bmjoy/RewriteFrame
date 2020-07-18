using Assets.Scripts.Define;
using Eternity.FlatBuffer;
using PureMVC.Interfaces;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static InputManager;

public class HudOpenBoxPanel : HudBase
{
	private const string ASSET_ADDRESS = Leyoutech.Core.Loader.Config.AssetAddressKey.PRELOADUI_HUD_OPENBOXPANEL;

	private const string ASSET_UI3DShip = Leyoutech.Core.Loader.Config.AssetAddressKey.PRELOADUI_UI3D_MINERALPANEL;

	/// <summary>
	/// 四方向Key
	/// </summary>
	private uint ITEM_L = 1190001;
	private uint ITEM_B = 1190002;
	private uint ITEM_R = 1190003;
	private uint ITEM_T = 1190004;

	/// <summary>
	/// 根节点
	/// </summary>
	private RectTransform m_Root;
	/// <summary>
	/// 根节点动画
	/// </summary>
	private Animator m_RootAnimator;
	/// <summary>
	/// 内容
	/// </summary>
	private RectTransform m_Content;
	/// <summary>
	/// 相机
	/// </summary>
	private Camera m_Camera;

	/// <summary>
	/// 四方向按钮
	/// </summary>
	private Animator m_ButtonL;
	private Animator m_ButtonT;
	private Animator m_ButtonR;
	private Animator m_ButtonB;
	/// <summary>
	/// 四方向图标
	/// </summary>
	private Image m_IconL;
	private Image m_IconT;
	private Image m_IconR;
	private Image m_IconB;
	/// <summary>
	/// 四方向文本
	/// </summary>
	private TMP_Text m_LabelL;
	private TMP_Text m_LabelT;
	private TMP_Text m_LabelR;
	private TMP_Text m_LabelB;
	/// <summary>
	/// 四方向热键
	/// </summary>
	private RectTransform m_HotkeyL;
	private RectTransform m_HotkeyT;
	private RectTransform m_HotkeyR;
	private RectTransform m_HotkeyB;

	/// <summary>
	/// 当前选中按钮
	/// </summary>
	private Animator m_CurrentButton;

	/// <summary>
	/// 当前目标NPC的ID
	/// </summary>
	private uint m_TargetNpcID;

	/// <summary>
	/// 当前目标NPC的UID
	/// </summary>
	private uint m_TargetNpcUID;

	/// <summary>
	/// 射线数据
	/// </summary>
	private RaycastProxy m_RaycastProxy;

	/// <summary>
	/// 数据
	/// </summary>
	private CfgEternityProxy m_CfgEternityProxy;

	/// <summary>
	/// 主相机
	/// </summary>
	private Camera m_MainCamera;

	/// <summary>
	/// 玩家主角
	/// </summary>
	private SpacecraftEntity m_MainEntity;

	/// <summary>
	/// GameplayProxy
	/// </summary>
	private GameplayProxy m_GameplayProxy;

	/// <summary>
	/// 是否播放音效
	/// </summary>
	private bool m_IsPlaySound;

	/// <summary>
	/// 是否显示
	/// </summary>
	private bool m_IsShowChest;
	/// <summary>
	/// 是否注册热键
	/// </summary>
	private bool m_PrevIsShow = false;

	/// <summary>
	/// 是否隐藏
	/// </summary>
	private bool m_IsHideChest;
	public HudOpenBoxPanel() : base(UIPanel.HudOpenBoxPanel, ASSET_ADDRESS, PanelType.Hud) { }

	public override void Initialize()
	{
		m_Root = GetTransform() as RectTransform;
		m_RootAnimator = GetTransform().GetComponent<Animator>();
		m_Content = FindComponent<RectTransform>("Back");
		m_Camera = m_RootAnimator.GetComponentInParent<Canvas>().worldCamera;
		m_RaycastProxy = Facade.RetrieveProxy(ProxyName.RaycastProxy) as RaycastProxy;
		m_CfgEternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;

		m_MainCamera = Camera.main;
		m_GameplayProxy = Facade.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;
		m_ButtonL = FindComponent<Animator>("Back/Panel/Button2");
		m_ButtonT = FindComponent<Animator>("Back/Panel/Button");
		m_ButtonR = FindComponent<Animator>("Back/Panel/Button4");
		m_ButtonB = FindComponent<Animator>("Back/Panel/Button3");

		ITEM_L = m_CfgEternityProxy.TreasureKeyByIndex(0).Id;
		ITEM_B = m_CfgEternityProxy.TreasureKeyByIndex(1).Id;
		ITEM_R = m_CfgEternityProxy.TreasureKeyByIndex(2).Id;
		ITEM_T = m_CfgEternityProxy.TreasureKeyByIndex(3).Id;

		m_IconL = FindComponent<Image>(m_ButtonL.transform, "Image_Icon");
		m_IconT = FindComponent<Image>(m_ButtonT.transform, "Image_Icon");
		m_IconR = FindComponent<Image>(m_ButtonR.transform, "Image_Icon");
		m_IconB = FindComponent<Image>(m_ButtonB.transform, "Image_Icon");

		m_LabelL = FindComponent<TMP_Text>(m_ButtonL.transform, "Label_Number");
		m_LabelT = FindComponent<TMP_Text>(m_ButtonT.transform, "Label_Number");
		m_LabelR = FindComponent<TMP_Text>(m_ButtonR.transform, "Label_Number");
		m_LabelB = FindComponent<TMP_Text>(m_ButtonB.transform, "Label_Number");

		m_HotkeyL = FindComponent<RectTransform>(m_ButtonL.transform, "Hotkey");
		m_HotkeyT = FindComponent<RectTransform>(m_ButtonT.transform, "Hotkey");
		m_HotkeyR = FindComponent<RectTransform>(m_ButtonR.transform, "Hotkey");
		m_HotkeyB = FindComponent<RectTransform>(m_ButtonB.transform, "Hotkey");
	}

	public override void OnShow(object msg)
	{
		base.OnShow(msg);

		CfgEternityProxy proxy = Facade.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
		UIUtil.SetIconImageSquare(m_IconL, proxy.GetItemByKey(ITEM_L).Icon);
		UIUtil.SetIconImageSquare(m_IconB, proxy.GetItemByKey(ITEM_B).Icon);
		UIUtil.SetIconImageSquare(m_IconR, proxy.GetItemByKey(ITEM_R).Icon);
		UIUtil.SetIconImageSquare(m_IconT, proxy.GetItemByKey(ITEM_T).Icon);

		InputManager.Instance.OnInputDeviceChanged += ResetHotkeys;

		ResetAnimators(null, HotkeyPhase.Disabled, true);
		StartUpdate();
	}

	public override void OnHide(object msg)
	{
		InputManager.Instance.OnInputDeviceChanged -= ResetHotkeys;

		base.OnHide(msg);
	}

	public override NotificationName[] ListNotificationInterests()
	{
		return new NotificationName[]
		{
			NotificationName.MSG_S2C_OPEN_CEHST_BY_KEY_RESULT,
            NotificationName.MSG_PACKAGE_ITEM_CHANGE,
		};
	}

	public override void HandleNotification(INotification notification)
	{
		switch (notification.Name)
		{
            case NotificationName.MSG_PACKAGE_ITEM_CHANGE:
            case NotificationName.MSG_S2C_OPEN_CEHST_BY_KEY_RESULT:
				ResetAnimators(null, HotkeyPhase.Disabled, true);
                SetHotKey();
                break;
		}
	}

	/// <summary>
	/// 更新
	/// </summary>
	protected override void Update()
	{
		if (m_MainEntity == null)
		{
			m_MainEntity = m_GameplayProxy.GetEntityById<SpacecraftEntity>(m_GameplayProxy.GetMainPlayerUID());
		}
		ShowChestHint();
		ShowOpenChest();

		bool isShow = m_Content && m_Content.gameObject.activeSelf;
		if (isShow != m_PrevIsShow)
		{
			ResetHotkeys(InputManager.Instance.CurrentInputDevice);
			m_PrevIsShow = isShow;
		}
	}

	/// <summary>
	/// 显示开宝箱
	/// </summary>
	public void ShowOpenChest()
	{
		if (!IsDead() && !IsBattling() && !IsLeaping() && !IsWatchOrUIInputMode())
		{
			SpacecraftEntity entity = m_RaycastProxy.Raycast();
			if (entity && (entity.GetCurrentState().GetMainState() != EnumMainState.Dead) && entity.GetHeroType() == KHeroType.htLockChest)
			{
				Vector3 titleOffset = Vector3.zero;
				m_TargetNpcUID = entity.GetUId();
				//头顶偏移

				Npc entityVO = m_CfgEternityProxy.GetNpcByKey(entity.GetTemplateID());
				if (Vector3.Distance(entity.transform.position, m_MainEntity.transform.position) > entityVO.TriggerRange)
				{
					return;
				}
				if (entityVO.HeroHeaderLength >= 3)
				{
					titleOffset = new Vector3(entityVO.HeroHeader(0), entityVO.HeroHeader(1), entityVO.HeroHeader(2));

				}

				//忽略屏幕外的
				Vector3 screenPosition = m_MainCamera.WorldToScreenPoint(entity.transform.TransformPoint(titleOffset));
				if (screenPosition.z > m_MainCamera.nearClipPlane && screenPosition.x >= 0 && screenPosition.x <= m_MainCamera.pixelWidth && screenPosition.y > 0 && screenPosition.y <= Camera.main.pixelHeight)
				{
					Vector2 anchoredPosition;
					if (RectTransformUtility.ScreenPointToLocalPointInRectangle(m_Root, screenPosition, m_Camera, out anchoredPosition))
					{
						m_Content.anchorMin = m_Content.anchorMax = Vector2.one * 0.5f;
						m_Content.anchoredPosition = anchoredPosition;
					}

					if (!m_Content.gameObject.activeSelf && entity.GetIsActive())
					{
						m_Content.gameObject.SetActive(true);
                        ResetAnimators(null, HotkeyPhase.Disabled, true);
                    }
				}
				else
				{
					m_Content.gameObject.SetActive(false);
				}
				return;
			}
			else
			{
				m_Content.gameObject.SetActive(false);
			}
		}
		else
		{
			m_Content.gameObject.SetActive(false);
		}
	}

	/// <summary>
	/// 显示开锁宝箱提示
	/// </summary>
	public void ShowChestHint()
	{
		if (!IsDead() && !IsLeaping() && !IsWatchOrUIInputMode())
		{
			List<SpacecraftEntity> lockChestList = m_GameplayProxy.GetEntities<SpacecraftEntity>(KHeroType.htLockChest);
            if (lockChestList.Count <= 0)
            {
                if (!m_IsHideChest)
                {
                    Facade.SendNotification(NotificationName.MSG_INTERACTIVE_HIDETIP, HudNpcInteractiveFlagPanel.InteractiveTipType.LockChest);
                }
                m_IsShowChest = false;
                m_IsHideChest = true;
            }
            for (int i = 0; i < lockChestList.Count; i++)
			{
                Npc lockChest = m_CfgEternityProxy.GetNpcByKey(lockChestList[i].GetTemplateID());
				if (Vector3.Distance(lockChestList[i].transform.position, m_MainEntity.transform.position) > lockChest.TriggerRange)
				{
					m_IsPlaySound = false;
					m_IsShowChest = false;
					Facade.SendNotification(NotificationName.MSG_INTERACTIVE_HIDETIP, HudNpcInteractiveFlagPanel.InteractiveTipType.LockChest);
				}
				else if (lockChestList[i].GetIsActive())
				{
					if (!m_IsPlaySound)
					{
						if (IsBattling())
						{
							m_IsPlaySound = true;
							WwiseUtil.PlaySound(WwiseManager.voiceComboID, WwiseMusicSpecialType.SpecialType_Voice_treasure_event4, WwiseMusicPalce.Palce_1st, false, null);
						}
					}
					if (IsBattling())
					{
						if (!m_IsShowChest)
						{
							Facade.SendNotification(NotificationName.MSG_INTERACTIVE_SHOWTIP, HudNpcInteractiveFlagPanel.InteractiveTipType.LockChest);
						}
						m_IsShowChest = true;
						m_IsHideChest = false;
					}
					else
					{
						if (!m_IsHideChest)
						{
							Facade.SendNotification(NotificationName.MSG_INTERACTIVE_HIDETIP, HudNpcInteractiveFlagPanel.InteractiveTipType.LockChest);
						}
						m_IsShowChest = false;
						m_IsHideChest = true;
					}
				}
			}
		}
	}

	/// <summary>
	/// 重设热键
	/// </summary>
	/// <param name="device"></param>
	private void ResetHotkeys(GameInputDevice device)
    {
        DeleteAllHotKey();
        DeleteAllBlockKey();
		if (m_Content && m_Content.gameObject.activeSelf)
		{
            bool isFromKeyboardMouse = device == GameInputDevice.KeyboardAndMouse;

			if (isFromKeyboardMouse)
			{
				AddBlockKey(HotKeyMapID.SHIP, HotKeyID.WeaponToggleLeft);
				AddBlockKey(HotKeyMapID.SHIP, HotKeyID.WeaponToggleRight);
			}
			else
			{
				AddBlockKey(HotKeyMapID.SHIP, HotKeyID.WeaponToggleLeft);
				AddBlockKey(HotKeyMapID.SHIP, HotKeyID.WeaponToggleRight);
				AddBlockKey(HotKeyMapID.SHIP, HotKeyID.WatchOpen);
				AddBlockKey(HotKeyMapID.SHIP, HotKeyID.ShipSkill4);
                AddBlockKey(HotKeyMapID.SHIP, HotKeyID.ItemOpen);
			}

            AddHotKey("l", HotKeyMapID.SHIP, HotKeyID.OpenBoxLeft, OnLeftCallback, 1.0f, m_HotkeyL, null, HotkeyManager.HotkeyStyle.UI);
            AddHotKey("r", HotKeyMapID.SHIP, HotKeyID.OpenBoxRight, OnRightCallback, 1.0f, m_HotkeyR, null, HotkeyManager.HotkeyStyle.UI);
			AddHotKey("t", HotKeyMapID.SHIP, HotKeyID.OpenBoxUp, OnUpCallback, 1.0f, m_HotkeyT, null, HotkeyManager.HotkeyStyle.UI);
            AddHotKey("b", HotKeyMapID.SHIP, HotKeyID.OpenBoxDown, OnDownCallback, 1.0f, m_HotkeyB, null, HotkeyManager.HotkeyStyle.UI);

            SetHotKey();

        }
    }

    /// <summary>
    /// 设置热键刷新
    /// </summary>
    public void SetHotKey()
    {
        PackageProxy proxy = Facade.RetrieveProxy(ProxyName.PackageProxy) as PackageProxy;
        SetHotKeyEnabled("l", proxy.GetItemCountByTID(ITEM_L) > 0);
        SetHotKeyEnabled("b", proxy.GetItemCountByTID(ITEM_B) > 0);
        SetHotKeyEnabled("r", proxy.GetItemCountByTID(ITEM_R) > 0);
        SetHotKeyEnabled("t", proxy.GetItemCountByTID(ITEM_T) > 0);
    }

    /// <summary>
    /// 上键按下时
    /// </summary>
    /// <param name="context"></param>
    private void OnUpCallback(HotkeyCallback context)
    {
        ResetAnimators(context.started ? m_ButtonT : null, context.phase);
		if (context.performed)
		{
			NetworkManager.Instance.GetDropItemController().SendOpenChestByKey(ITEM_T, m_TargetNpcUID);
		}
	}

    /// <summary>
    /// 下键按下时
    /// </summary>
    /// <param name="context"></param>
    private void OnDownCallback(HotkeyCallback context)
    {
        ResetAnimators(context.started ? m_ButtonB : null, context.phase);
		if (context.performed)
		{
			NetworkManager.Instance.GetDropItemController().SendOpenChestByKey(ITEM_B, m_TargetNpcUID);
		}
	}

    /// <summary>
    /// 左键按下时
    /// </summary>
    /// <param name="context"></param>
    private void OnLeftCallback(HotkeyCallback context)
    {
        ResetAnimators(context.started ? m_ButtonL : null, context.phase);
		if (context.performed)
		{
			NetworkManager.Instance.GetDropItemController().SendOpenChestByKey(ITEM_L, m_TargetNpcUID);
		}
	}

    /// <summary>
    /// 右键按下时
    /// </summary>
    /// <param name="context"></param>
    private void OnRightCallback(HotkeyCallback context)
    {
        ResetAnimators(context.started ? m_ButtonR : null, context.phase);
		if (context.performed)
		{
			NetworkManager.Instance.GetDropItemController().SendOpenChestByKey(ITEM_R, m_TargetNpcUID);
		}
	}

    /// <summary>
    /// 重置所有动画状态
    /// </summary>
    private void ResetAnimators(Animator selected, HotkeyPhase phase, bool forceUpdate = false)
    {
        if (forceUpdate || m_CurrentButton != selected)
        {
            m_CurrentButton = selected;

            m_ButtonL.ResetTrigger("Normal");
            m_ButtonL.ResetTrigger("IsOn");
            m_ButtonL.ResetTrigger("Disabled");

            m_ButtonB.ResetTrigger("Normal");
            m_ButtonB.ResetTrigger("IsOn");
            m_ButtonB.ResetTrigger("Disabled");

            m_ButtonR.ResetTrigger("Normal");
            m_ButtonR.ResetTrigger("IsOn");
            m_ButtonR.ResetTrigger("Disabled");

            m_ButtonT.ResetTrigger("Normal");
            m_ButtonT.ResetTrigger("IsOn");
            m_ButtonT.ResetTrigger("Disabled");

            PackageProxy proxy = Facade.RetrieveProxy(ProxyName.PackageProxy) as PackageProxy;

            long countL = proxy.GetItemCountByTID(ITEM_L);
            long countB = proxy.GetItemCountByTID(ITEM_B);
            long countR = proxy.GetItemCountByTID(ITEM_R);
            long countT = proxy.GetItemCountByTID(ITEM_T);

            if (m_CurrentButton == m_ButtonL && countL <= 0) return;
            if (m_CurrentButton == m_ButtonT && countT <= 0) return;
            if (m_CurrentButton == m_ButtonR && countR <= 0) return;
            if (m_CurrentButton == m_ButtonB && countB <= 0) return;

            m_ButtonL.SetTrigger(m_CurrentButton == m_ButtonL && countL > 0 ? "IsOn" : (m_CurrentButton == null && countL > 0 ? "Normal" : "Disabled"));
            m_ButtonT.SetTrigger(m_CurrentButton == m_ButtonT && countT > 0 ? "IsOn" : (m_CurrentButton == null && countT > 0 ? "Normal" : "Disabled"));
            m_ButtonR.SetTrigger(m_CurrentButton == m_ButtonR && countR > 0 ? "IsOn" : (m_CurrentButton == null && countR > 0 ? "Normal" : "Disabled"));
            m_ButtonB.SetTrigger(m_CurrentButton == m_ButtonB && countB > 0 ? "IsOn" : (m_CurrentButton == null && countB > 0 ? "Normal" : "Disabled"));

            m_LabelL.text = countL.ToString();
            m_LabelB.text = countB.ToString();
            m_LabelR.text = countR.ToString();
            m_LabelT.text = countT.ToString();

            if (phase == HotkeyPhase.Performed)
            {
                //AnimationRoot("FinishAndClose");
            }
        }
    }

	/// <summary>
	/// 动画触发事件
	/// </summary>
	/// <param name="trigger"></param>
	/// <param name="close"></param>
    private void AnimationRoot(string trigger, bool close = false)
    {
        m_RootAnimator.SetTrigger(trigger);

        if(close)
            UIManager.Instance.StartCoroutine(DelayClose());
    }

    private IEnumerator DelayClose()
    {
        yield return new WaitForSeconds(0.5f);

        UIManager.Instance.ClosePanel(this);
    }
}
