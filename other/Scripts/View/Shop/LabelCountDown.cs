using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CountDownCompent : MonoBehaviour
{
    public static List<CountDownCompent> m_CountDowns = new List<CountDownCompent>();
    /// <summary>
    /// 倒计时文本框
    /// </summary>
    public TMP_Text m_CountDownLabel;
    /// <summary>
    /// 倒计时
    /// </summary>
    public ulong m_Time;
    private void OnEnable()
    {
        m_CountDowns.Add(this);
    }

    private void OnDisable()
    {
        m_CountDowns.Remove(this);
    }
    public void SetTime(ulong refreshTime)
    {
        if (m_CountDownLabel == null)
        {
            m_CountDownLabel = TransformUtil.FindUIObject<TMP_Text>(transform, "Content/Label_Time");
        }
        m_Time = refreshTime;
    }
}
