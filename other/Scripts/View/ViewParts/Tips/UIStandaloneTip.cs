using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class UIStandaloneTip : MonoBehaviour
{
    /// <summary>
    /// Tip
    /// </summary>
    private StandaloneTip m_Tip;

    /// <summary>
    /// Tip数据
    /// </summary>
    private object m_TipData;

    private void OnEnable()
    {
        if (m_Tip == null)
            m_Tip = new StandaloneTip(GetComponent<RectTransform>());

        m_Tip.Opened = true;
        m_Tip.OnShow(null);
        m_Tip.SetTipData(m_TipData);
    }

    private void OnDisable()
    {
        if (m_Tip != null)
        {
            m_Tip.SetTipData(null);
            m_Tip.OnHide();
            m_Tip.Opened = false;
        }
    }

    /// <summary>
    /// 设置Tip数据
    /// </summary>
    public object TipData
    {
        get
        {
            return m_TipData;
        }
        set
        {
            m_TipData = value;
            m_Tip.SetTipData(m_TipData);
        }
    }


    private class StandaloneTip : UITipPart
    {
        /// <summary>
        /// 模拟视图状态
        /// </summary>
        private UIViewState m_State;

        /// <summary>
        /// Tip视图容器
        /// </summary>
        private RectTransform m_TipBox;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="tipBox"></param>
        public StandaloneTip(RectTransform tipBox)
        {
            m_TipBox = tipBox;
            m_State = new UIViewState();
        }

        /// <summary>
        /// 获取视图状态
        /// </summary>
        public override UIViewState State
        {
            get { return m_State; }
        }

        /// <summary>
        /// 获取视图容器
        /// </summary>
        protected override Transform TipBox
        {
            get { return m_TipBox; }
        }

        /// <summary>
        /// 设置Tip数据
        /// </summary>
        /// <param name="data">数据</param>
        public void SetTipData(object data)
        {
            m_State.SetTipData(data);
        }
    }
}