using UnityEngine;

public class ItemAutoCard : MonoBehaviour
{
	/// <summary>
	/// 转角系数
	/// </summary>
	[SerializeField, Range(0, 20)]
	private float m_AngleCoefficient = 4f;

	/// <summary>
	/// 转角时间
	/// </summary>
	[SerializeField, Range(0, 1f)]
	private float m_Time = 0.1f;

	/// <summary>
	/// 矩阵
	/// </summary>
	private RectTransform m_RectTransform;

	void Awake()
	{
		m_RectTransform = this.GetComponent<RectTransform>();
	}

    private void Update()
    {
        Vector2 mousePosition = InputManager.Instance.GetCurrentVirtualCursorPos();
        Vector3 transfomPosition = CameraManager.GetInstance().GetUICameraComponent().GetCamera().WorldToScreenPoint(m_RectTransform.position);

        float x = mousePosition.x - transfomPosition.x;
        float y = mousePosition.y - transfomPosition.y;

        float width = m_RectTransform.sizeDelta.x / 2;
        float height = m_RectTransform.sizeDelta.y / 2;
        float angle = m_AngleCoefficient * (width / height);

        //if (Mathf.Abs(x) > m_Width || Mathf.Abs(y) > m_Height)
        //{
        //	m_RectTransform.rotation = Quaternion.Euler(0, 0, 0);
        //}

        x = Mathf.Clamp(x, -width, width);
        y = Mathf.Clamp(y, -height, height);

        float ax = x != 0 ? -x / width * m_AngleCoefficient : 0;
        float ay = y != 0 ? y / height * angle : 0;

		m_RectTransform.rotation = Quaternion.Lerp(m_RectTransform.rotation, Quaternion.Euler(ay, ax, 0), m_Time);
	}

}
