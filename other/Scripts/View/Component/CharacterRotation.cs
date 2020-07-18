using UnityEngine;
using UnityEngine.EventSystems;

public class CharacterRotation : MonoBehaviour,IDragHandler
{
    [Header("Model")]
    public Transform target;
    public float normalAngle = 0;

    [Header("Mouse")]
    public float mouseDragPositionMulti = -330f;
    public float mouseDragDeltaLimit = 50.0f;

    [Header("Joystic")]
    public float joysticDragMulti = -3.0f;

    private int m_Mode = 0;
    private Vector2 m_JoysticDirection = Vector2.zero;

    /// <summary>
    /// 是否可以旋转
    /// </summary>
    private bool m_IsRotate;
    public bool IsRotate { get => m_IsRotate; set => m_IsRotate = value; }

    private void OnEnable()
    {
        IsRotate = true;
        m_JoysticDirection = Vector2.zero;
        HotkeyManager.Instance.Register("rotate_" + GetInstanceID(), HotKeyMapID.UI, HotKeyID.UGUI_Stick2, OnCallback);
    }

    private void OnDisable()
    {
        m_JoysticDirection = Vector2.zero;
		if (HotkeyManager.Instance!=null)
		{
			HotkeyManager.Instance.Unregister("rotate_" + GetInstanceID());
		}
	}

    public void ResetAngle()
    {
        if(target)
        {
            target.localEulerAngles = new Vector3(0, normalAngle, 0);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!IsRotate)
            return;
        if(InputManager.Instance.CurrentInputDevice != InputManager.GameInputDevice.KeyboardAndMouse)
            return;

        if (!target)
            return;

        m_Mode = 1;

        float delta = Mathf.Min(Mathf.Abs(eventData.delta.x), mouseDragDeltaLimit) * (eventData.delta.x > 0 ? 1 : -1);

        target.Rotate(Vector3.up, delta / Screen.width * mouseDragPositionMulti);
    }

    private void OnCallback(HotkeyCallback callback)
    {
        m_Mode = 2;
        m_JoysticDirection = callback.ReadValue<Vector2>();
    }

    private void Update()
    {
        if (!IsRotate)
            return;
        if (!target)
            return;

        if (m_Mode == 2)
        {
            target.Rotate(Vector3.up, m_JoysticDirection.x * joysticDragMulti);
        }
    }
}
