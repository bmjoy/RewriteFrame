using Assets.Scripts.Define;
using PureMVC.Interfaces;
using UnityEngine;
using UnityEngine.InputSystem;

public class HudItemPanel : HudBase
{
    private const string ASSET_ADDRESS = Leyoutech.Core.Loader.Config.AssetAddressKey.PRELOADUI_HUD_ITEMPANEL;

    /// <summary>
    /// 根动画
    /// </summary>
    private Animator m_Root;
    /// <summary>
    /// 圆盘动画
    /// </summary>
    private Animator m_Circle;
    /// <summary>
    /// 列表容器
    /// </summary>
    private RectTransform m_ToggleBox;
    /// <summary>
    /// 选择箭头
    /// </summary>
    private RectTransform m_SelectArrow;
    /// <summary>
    /// 热键容器
    /// </summary>
    private RectTransform m_HotkeyBox;
    /// <summary>
    /// 鼠标矢量
    /// </summary>
    private Vector2 m_MouseVector;
    /// <summary>
    /// 当前选择索引
    /// </summary>
    private int m_SelectionIndex;

    public HudItemPanel() : base(UIPanel.HudItemPanel, ASSET_ADDRESS, PanelType.Hud) { }

    public override void Initialize()
    {
        m_Root = GetTransform().GetComponent<Animator>();
        m_Circle = FindComponent<Animator>("Open");
        m_ToggleBox = FindComponent<RectTransform>("Open/Content/ItemBox");
        m_SelectArrow = FindComponent<RectTransform>("Open/Cursor");
        m_HotkeyBox = FindComponent<RectTransform>("Item/HotKey");
    }

    public override void OnShow(object msg)
    {
        base.OnShow(msg);

        AddHotKey(HotKeyMapID.SHIP, HotKeyID.ItemOpen, OnItemKeyStateChangedFromShip, m_HotkeyBox);
        AddHotKey(HotKeyMapID.Item, HotKeyID.ItemClose, OnItemKeyStateChangedFromUI);
        AddHotKey(HotKeyMapID.Item, HotKeyID.ItemAxis, OnItemAxisStateChanged);
    }

    public override NotificationName[] ListNotificationInterests()
    {
        return new NotificationName[]
        {
            NotificationName.MSG_CHANGE_BATTLE_STATE
        };
    }

    public override void HandleNotification(INotification notification)
    {
        switch ((NotificationName)notification.Name)
        {
            case NotificationName.MSG_CHANGE_BATTLE_STATE:
                OnBattleStateChanged();
                break;
        }
    }

    /// <summary>
    /// 战斗状态改变时
    /// </summary>
    private void OnBattleStateChanged()
    {
        if (m_Root)
        {
            if (GetMainEntity()!=null)
            {
                m_Root.ResetTrigger("Cruise");
                m_Root.ResetTrigger("Battle");
                m_Root.SetTrigger(IsBattling() ? "Battle" : "Cruise");
            }
        }
    }

    /// <summary>
    /// 飞船模式下按下道具盘开启键时
    /// </summary>
    /// <param name="callback">热键参数</param>
    private void OnItemKeyStateChangedFromShip(HotkeyCallback callback)
    {
        if(callback.performed)
        {
            m_Circle.ResetTrigger("Open");
            m_Circle.ResetTrigger("Closed");

            if (callback.performed)
            {
                m_MouseVector = Vector2.zero;
                m_Circle.gameObject.SetActive(true);
                m_Circle.SetTrigger("Open");

                InputManager.Instance.HudInputMap = HotKeyMapID.Item;
            }
        }
    }

    /// <summary>
    /// UI模式下松开道具盘开启键时
    /// </summary>
    /// <param name="callback">热键参数</param>
    private void OnItemKeyStateChangedFromUI(HotkeyCallback callback)
    {
        if (!callback.performed)
        {
            m_Circle.ResetTrigger("Open");
            m_Circle.ResetTrigger("Closed");

            m_MouseVector = Vector2.zero;
            m_Circle.gameObject.SetActive(true);
            m_Circle.SetTrigger("Closed");

            InputManager.Instance.HudInputMap = HotKeyMapID.None;
        }
    }

    /// <summary>
    /// 处理道具选择
    /// </summary>
    /// <param name="callback">热键参数</param>
    private void OnItemAxisStateChanged(HotkeyCallback callback)
    {
        if (InputManager.Instance.CurrentInputDevice != InputManager.GameInputDevice.KeyboardAndMouse)
        {
            SetSelection(callback.ReadValue<Vector2>());
        }
        else
        {
            m_MouseVector = m_MouseVector + callback.ReadValue<Vector2>();
            SetSelection(m_MouseVector);
        }
    }

    /// <summary>
    /// 根据角度突出显示对应的选中项
    /// </summary>
    /// <param name="direction">方向矢量</param>
	private void SetSelection(Vector2 direction)
    {
        direction = direction.normalized * new Vector2(1, -1);

        float angleStep = 360.0f / m_ToggleBox.childCount;

        float max = float.MinValue;
        int index = -1;

        if (direction.magnitude > 0)
        {
            Vector3 vector = Quaternion.Euler(0, 0, 180.0f) * Vector3.up;
            Quaternion rotation = Quaternion.Euler(0, 0, angleStep);
            for (int i = 0; i < m_ToggleBox.childCount; i++)
            {
                float value = Vector2.Dot(direction, vector);
                if (value > 0)
                {
                    if (value > max)
                    {
                        max = value;
                        index = i;
                    }
                }
                vector = rotation * vector;
            }
        }

        for (int i = 0; i < m_ToggleBox.childCount; i++)
        {
            Animator animator = m_ToggleBox.GetChild(i).GetComponent<Animator>();
            if (animator)
            {
                animator.SetBool("IsOn", i == index);
            }
        }

        m_SelectArrow.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y * -1, direction.x) * Mathf.Rad2Deg - 90);

        m_SelectionIndex = index;
    }
}
