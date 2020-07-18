using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class RadialSlider : MonoBehaviour
{
    public Image mask;

    public Transform flag;

    [SerializeField]
    private float m_FillFrom = 0;
    [SerializeField]
    private float m_FillTo = 0;
    [SerializeField]
    private float m_AngleFrom = 0;
    [SerializeField]
    private float m_AngleTo = 0;
    [SerializeField]
    [Range(0.0f,1.0f)]
    private float m_Progress = 0;

    public float FillFrom
    {
        get { return m_FillFrom; }
        set
        {
            m_FillFrom = float.IsNaN(value) ? 0 : value;
            UpdateFillAmount();
        }
    }

    public float FillTo
    {
        get { return m_FillTo; }
        set
        {
            m_FillTo = float.IsNaN(value) ? 0 : value;
            UpdateFillAmount();
        }
    }
    public float AngleFrom
    {
        get { return m_AngleFrom; }
        set
        {
            m_AngleFrom = float.IsNaN(value) ? 0 : value;
            UpdateFillAmount();
        }
    }

    public float AngleTo
    {
        get { return m_AngleTo; }
        set
        {
            m_AngleTo = float.IsNaN(value) ? 0 : value;
            UpdateFillAmount();
        }
    }

    public float FillAmount
    {
        get { return m_Progress; }
        set
        {
            m_Progress = Mathf.Clamp01(float.IsNaN(value) ? 0 : value);
            UpdateFillAmount();
        }
    }

    public void UpdateFillAmount()
    {
        if (mask)
            mask.fillAmount = Mathf.Lerp(FillFrom, FillTo, m_Progress);
        if (flag)
            flag.localEulerAngles = Vector3.forward * Mathf.Lerp(AngleFrom, AngleTo, m_Progress);
    }

    private void OnValidate()
    {
        UpdateFillAmount();
    }
}
