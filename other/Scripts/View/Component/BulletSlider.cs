using UnityEngine;

[ExecuteAlways]
public class BulletSlider : MonoBehaviour
{
    public RectTransform bullet;
    public RectTransform bulletFlag;

    [SerializeField]
    [Range(1,100)]
    private int m_BulletCount = 0;
    [SerializeField]
    [Range(0.0f, 1.0f)]
    private float m_Progress = 0;

    public int BulletCount
    {
        get { return Mathf.Max(1, m_BulletCount); }
        set
        {
            int newValue = value < 1 ? 1 : value;
            if(newValue!=m_BulletCount)
            {
                m_BulletCount = newValue;
                SetDirty();
            }
        }
    }
    public float FillAmount
    {
        get { return m_Progress; }
        set
        {
            float newValue = Mathf.Clamp01(float.IsNaN(value) ? 0 : value);
            if (newValue != m_Progress)
            {
                m_Progress = newValue;
                SetDirty();
            }
        }
    }

    private void SetDirty()
    {
        UpdateFillAmount();
    }

    protected void UpdateFillAmount()
    {
        if (bullet)
        {
            string bulletFlagPath = "";
            Transform curr = bulletFlag;
            while(curr)
            {
                bulletFlagPath = curr.name + (string.IsNullOrEmpty(bulletFlagPath) ? "" : "/" + bulletFlagPath);

                curr = curr.parent != bullet.transform ? curr.parent : null;
            }

            Transform bulletBox = bullet.transform.parent;

            int index = 0;
            int selectedCount = Mathf.FloorToInt(FillAmount * (float)BulletCount);
            for (int i = 0; i < BulletCount; i++)
            {
                Transform child = i < bulletBox.childCount ? bulletBox.GetChild(i) : Object.Instantiate(bullet, bulletBox);
                child.gameObject.SetActive(true);

                Transform flag = child.Find(bulletFlagPath);
                if (flag)
                    flag.gameObject.SetActive(i < selectedCount);

                index++;
            }

            for (int i = bulletBox.childCount - 1; i >= index && i > 0; i--)
            {
                bulletBox.GetChild(i).gameObject.SetActive(false);

                //if (Application.isEditor)
                //    Object.DestroyImmediate(bulletBox.GetChild(i).gameObject);
                //else
                //    Object.Destroy(bulletBox.GetChild(i).gameObject);
            }
        }
    }

    private void OnValidate()
    {
        SetDirty();
    }
}