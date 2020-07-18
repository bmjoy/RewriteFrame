using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class LineImage : MonoBehaviour
{
    /// <summary>
    /// 图片
    /// </summary>
    private Image m_Image;

    public void Awake()
    {
        m_Image = GetComponent<Image>();
    }

    /// <summary>
    /// 划线
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    public void DrawLine(Vector2 start, Vector2 end,int id)
    {
        gameObject.name = id.ToString();
        Vector2 dir = end - start;
        float length = dir.magnitude;
        float angle = Vector2.Angle(dir,Vector2.right);
       
        float dot = Vector2.Dot(dir, Vector2.up);
        if (angle != 90)
        {
            if (dot<0)
            {
                angle += 90;
            }
        }
        m_Image.gameObject.transform.localEulerAngles = new Vector3(0,0,angle);
        float y = m_Image.GetComponent<RectTransform>().sizeDelta.y;
        m_Image.GetComponent<RectTransform>().sizeDelta = new Vector2( length,y);
        m_Image.gameObject.transform.localPosition = new Vector3((start.x + end.x) * 0.5f, (start.y+end.y)*0.5f,0);
    }

    public void DrawColor(Color color)
    {
        m_Image.color = color;
    }
}
