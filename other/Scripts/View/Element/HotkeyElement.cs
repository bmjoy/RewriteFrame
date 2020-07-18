using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class HotkeyElement : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
	/// <summary>
	/// 动画
	/// </summary>
	private Animator m_Animator;
	/// <summary>
	/// 按钮
	/// </summary>
	private Button m_Button;
    /// <summary>
    /// 按钮屏蔽
    /// </summary>
    private CanvasGroup m_ButtonCanvasGroup;
	/// <summary>
	/// 图标
	/// </summary>
	private Image m_Icon;
	/// <summary>
	/// 文本内容
	/// </summary>
	private TextMeshProUGUI m_Text;
	/// <summary>
	/// 进度条
	/// </summary>
	private Image m_Progress;
	/// <summary>
	/// 进度条容器
	/// </summary>
	private RectTransform m_ProgressBox;

	/// <summary>
	/// 状态数据
	/// </summary>
	private HotkeyManager.HotkeyState m_State;

	/// <summary>
	/// 按下时间
	/// </summary>
	private float m_PressTime;

    /// <summary>
    /// 点按模式已经按过的次数
    /// </summary>
    private int m_MultipleClickCount;
    /// <summary>
    /// 点按模式下最后一次松开键的时间
    /// </summary>
    private float m_MultipleClickLastReleaseTime;



	private void Awake()
	{
		m_Animator = transform.GetComponent<Animator>();
		m_Button = transform.GetComponent<Button>();
        m_ButtonCanvasGroup = transform.GetComponent<CanvasGroup>();
        m_Icon = FindComponent<Image>("Icon");
		m_Progress = FindComponent<Image>("Progress/ProgressImage");
		m_ProgressBox = FindComponent<RectTransform>("Progress");
		m_Text = FindComponent<TextMeshProUGUI>("Text");

        if (m_ButtonCanvasGroup == null)
            m_ButtonCanvasGroup = transform.gameObject.AddComponent<CanvasGroup>();

		if (m_Text == null)
		{
			RectTransform textBox = FindComponent<RectTransform>("Text");
			if (textBox != null)
			{
				m_Text = textBox.GetComponentInChildren<TextMeshProUGUI>();
			}
		}

		//
		if (!m_Icon)
			m_Icon = FindComponent<Image>("HotKey/Icon");
		if (!m_Progress)
			m_Progress = FindComponent<Image>("HotKey/Progress/ProgressImage");
		if (!m_ProgressBox)
			m_ProgressBox = FindComponent<RectTransform>("HotKey/Progress");
	}

	/// <summary>
	/// 查找组件
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="path"></param>
	/// <returns></returns>
	private T FindComponent<T>(string path) where T : Component
	{
		if (transform)
		{
			Transform t = transform.Find(path);
			if (t)
			{
				return t.GetComponent<T>();
			}
		}
		return null;
	}

	public void ReSetAnimatorBySetActive(bool active)
	{
		if (active)
			PlayStartAnim();
	}

	/// <summary>
	/// 播放开始动画
	/// </summary>
	public void PlayStartAnim()
	{
		PlayAnim("Start");
	}

	/// <summary>
	/// 播放完成动画
	/// </summary>
	public void PlayFinishAnim()
	{
        if(m_State.Mode== HotkeyManager.HotkeyMode.Hold)
		    PlayAnim("FinishByHold");
        else
            PlayAnim("FinishByPress");
    }

	/// <summary>
	/// 播放指定动画
	/// </summary>
	/// <param name="name"></param>
	private void PlayAnim(string name)
	{
		if (m_Animator)
		{
			foreach (AnimatorControllerParameter arg in m_Animator.parameters)
			{
				if (arg.type == AnimatorControllerParameterType.Trigger && arg.name.Equals(name))
				{
					m_Animator.SetTrigger(name);
					break;
				}
			}
		}
	}

	/// <summary>
	/// 初始化
	/// </summary>
	/// <param name="hotkeyState">状态</param>
	public void Reset(HotkeyManager.HotkeyState hotkeyState)
	{
		m_PressTime = 0;
        m_MultipleClickCount = 0;
        m_MultipleClickLastReleaseTime = 0;

        m_State = hotkeyState;
		if (m_Progress)
			m_Progress.gameObject.SetActive(m_State.Mode == HotkeyManager.HotkeyMode.Hold);
		if (m_ProgressBox)
			m_ProgressBox.gameObject.SetActive(m_State.Mode == HotkeyManager.HotkeyMode.Hold);

		Text = m_State.Description;
		Visible = m_State.Visible;
		Enabled = m_State.Enabled;
        Interactable = m_State.Interactable;
		Progress = 0;
	}

	/// <summary>
	/// 清理
	/// </summary>
	public void Clear()
	{
		m_PressTime = 0;
        m_MultipleClickCount = 0;
        m_MultipleClickLastReleaseTime = 0;

        m_State = null;
	}

	/// <summary>
	/// 图标
	/// </summary>
	public Sprite Icon
	{
		get { return m_Icon ? m_Icon.sprite : null; }
		set { if (m_Icon) { m_Icon.sprite = value; } }
	}

	/// <summary>
	/// 文字
	/// </summary>
	public string Text
	{
		get { return m_Text ? m_Text.text : string.Empty; }
		set { if (m_Text) { m_Text.text = value; } }
	}

	/// <summary>
	/// 进度
	/// </summary>
	public float Progress
	{
		get { return m_Progress ? m_Progress.fillAmount : 0; }
		set
		{
			if (m_Progress)
			{
				m_Progress.fillAmount = Mathf.Clamp01(value);
			}
		}
	}

	/// <summary>
	/// 可见性
	/// </summary>
	public bool Visible
	{
		get { return gameObject.activeSelf; }
		set
		{
			gameObject.SetActive(value);
			if (m_Progress && value == false)
				m_Progress.fillAmount = 0;
            if (!value)
            {
                m_PressTime = 0;
                m_MultipleClickCount = 0;
                m_MultipleClickLastReleaseTime = 0;
            }
		}
	}

	/// <summary>
	/// 可用性
	/// </summary>
	public bool Enabled
	{
		get { return m_Button.interactable; }
		set
		{
			if (m_Button)
				m_Button.interactable = value;
			if (m_Progress && value == false)
				m_Progress.fillAmount = 0;
            if (!value)
            {
                m_PressTime = 0;
                m_MultipleClickCount = 0;
                m_MultipleClickLastReleaseTime = 0;
            }

            if (m_Animator)
            {
                m_Animator.Update(1.0f);
                m_Animator.SetTrigger(value ? "Normal":"Disabled");
                m_Animator.Update(1.0f);
            }
		}
	}

    /// <summary>
    /// 是否可以交互
    /// </summary>
    public bool Interactable
    {
        get { return m_ButtonCanvasGroup.blocksRaycasts; }
        set { m_ButtonCanvasGroup.blocksRaycasts = value; }
    }

	/// <summary>
	/// 按下时
	/// </summary>
	/// <param name="eventData">指针事件</param>
	public void OnPointerDown(PointerEventData eventData)
	{
		if (m_Button && m_Button.isActiveAndEnabled && m_Button.interactable && eventData.button == PointerEventData.InputButton.Left)
		{
			m_PressTime = Time.time;

			if (m_State.Mode == HotkeyManager.HotkeyMode.Press)
			{
				HotkeyManager.Instance.PlayHotKeySound(m_State, InputActionPhase.Started);

				if (m_State.Callback != null)
					m_State.Callback.Invoke(HotkeyCallback.CreateFrom(HotkeyPhase.Started, Time.time, m_PressTime, Time.time - m_PressTime, true));
			}
			else if (m_State.Mode == HotkeyManager.HotkeyMode.Hold)
			{
				if (!m_State.Holding)
				{
					PlayStartAnim();

					HotkeyManager.Instance.PlayHotKeySound(m_State, InputActionPhase.Started);

					if (m_State.Callback != null)
						m_State.Callback.Invoke(HotkeyCallback.CreateFrom(HotkeyPhase.Started, Time.time, m_PressTime, m_State.HoldDuration, true));

					Progress = 0;
				}
			}
		}
	}

	/// <summary>
	/// 松开时
	/// </summary>
	/// <param name="eventData">指针事件</param>
	public void OnPointerUp(PointerEventData eventData)
	{
		if (m_Button && m_Button.isActiveAndEnabled && m_Button.interactable && eventData.button == PointerEventData.InputButton.Left)
		{
			if (m_State.Mode == HotkeyManager.HotkeyMode.Press)
			{
				if (m_PressTime > 0)
				{
					HotkeyManager.Instance.PlayHotKeySound(m_State, InputActionPhase.Performed);

					if (m_State.Callback != null)
						m_State.Callback.Invoke(HotkeyCallback.CreateFrom(HotkeyPhase.Performed, Time.time, m_PressTime, Time.time - m_PressTime, true));
				}
            }
            else if (m_State.Mode == HotkeyManager.HotkeyMode.Hold)
			{
				if (!m_State.Holding)
				{
					float progress = Mathf.Clamp01((Time.time - m_PressTime) / m_State.HoldDuration);
					if (progress < 1)
					{
						HotkeyManager.Instance.PlayHotKeySound(m_State, InputActionPhase.Canceled);

						if (m_State.Callback != null)
							m_State.Callback.Invoke(HotkeyCallback.CreateFrom(HotkeyPhase.Canceled, Time.time, m_PressTime, m_State.HoldDuration, true));

						Progress = 0;
					}
				}
			}

			m_PressTime = 0;
		}
	}

	/// <summary>
	/// 更新
	/// </summary>
	private void Update()
	{
		if (!gameObject.activeInHierarchy) { return; }
		if (!m_Button || !m_Button.isActiveAndEnabled || !m_Button.interactable) { return; }

		if (m_State.Mode != HotkeyManager.HotkeyMode.Hold) { return; }
		if (m_State.Holding) { return; }

		if (m_PressTime > 0)
		{
			float progress = Mathf.Clamp01((Time.time - m_PressTime) / m_State.HoldDuration);
			if (progress < 1)
			{
				if (m_State.Callback != null)
					m_State.Callback.Invoke(HotkeyCallback.CreateFrom(HotkeyPhase.Started, Time.time, m_PressTime, m_State.HoldDuration, true));

				Progress = progress;

				float repc = progress * 10.0f;
				WwiseManager.SetParameter(WwiseRtpc.Rtpc_UI_Hotkey, repc);
			}
			else
			{
				PlayFinishAnim();

				HotkeyManager.Instance.PlayHotKeySound(m_State, InputActionPhase.Performed);

				if (m_State.Callback != null)
					m_State.Callback.Invoke(HotkeyCallback.CreateFrom(HotkeyPhase.Performed, Time.time, m_PressTime, m_State.HoldDuration, true));

				Progress = 0;

				m_PressTime = 0;
			}
		}
	}
}