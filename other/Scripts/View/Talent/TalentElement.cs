using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class TalentElement : MonoBehaviour
{
    /// <summary>
    /// 等级数
    /// </summary>
    private TextMeshProUGUI m_NumberLabel;
    /// <summary>
    /// 图标Icon
    /// </summary>
    private Image m_IconImage;
    /// <summary>
    /// 未解锁
    /// </summary>
    private GameObject m_LockObj;
    /// <summary>
    /// 未激活
    /// </summary>
    private GameObject m_NotUpGrade;
    /// <summary>
    /// 对应的线
    /// </summary>
    private LineRenderer m_LineRenderer;
    /// <summary>
    /// 对应的线
    /// </summary>
    private LineImage m_LineImage;
    /// <summary>
    /// 天赋状态
    /// </summary>
    private TalentState m_TalentState;
    /// <summary>
    /// 天赋上一次状态
    /// </summary>
    private TalentState m_OldTalentState;
    /// <summary>
    /// 天赋上一次等级
    /// </summary>
    private int m_OldLevel;
    /// <summary>
    /// 动画
    /// </summary>
    private Animator m_Animator;
    /// <summary>
    /// 天赋
    /// </summary>
    private TalentVO m_TalentVO;
    /// <summary>
    /// 激活动画附着的物体
    /// </summary>
    private GameObject m_ActiveGameObject;
    /// <summary>
    /// 颜色
    /// </summary>
    private Color m_Color;
    private Color COLORH = new Color(1,1,1,10/255f);
    private Color COLOR = new Color(1, 1, 1,1);
    /// <summary>
    /// 获取Vo
    /// </summary>
    /// <returns></returns>
    public TalentVO GetTalentVO()
    {
        return m_TalentVO;
    }

    /// <summary>
    /// 初始化
    /// </summary>
    public void Initialize()
    {
        //m_CfgEternityProxy = (CfgEternityProxy)GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy);
        m_NumberLabel = TransformUtil.FindUIObject<TextMeshProUGUI>(transform, "ImagePoint/Label_Talent");
        m_IconImage = TransformUtil.FindUIObject<Image>(transform, "ImagePoint/ImageIcon");
        m_Animator = TransformUtil.FindUIObject<Animator>(transform, "ImagePoint");
        m_ActiveGameObject = TransformUtil.FindUIObject<Transform>(transform, "ImagePoint/Show").gameObject;
    }

    /// <summary>
    ///加载数据
    /// </summary>
    public void SetContent(LineImage lineImage,LineRenderer lineRenderer,TalentVO talentVO)
    {
        m_TalentVO = talentVO;
        Vector3 vector3 = new Vector3((float)talentVO.MTalentSubNode.Value.Position.Value.X, (float)talentVO.MTalentSubNode.Value.Position.Value.Y, 0);
        SetPos(vector3);
        m_LineRenderer = lineRenderer;
        m_LineImage = lineImage;
        m_Color = COLORH;
        talentVO.MTalentElement = this;
        m_OldTalentState = m_TalentState;
        m_TalentState = talentVO.State;
        m_NumberLabel.text = string.Format(TableUtil.GetLanguageString("shiphangar_text_1009"),talentVO.Level,talentVO.MaxLevel);
        UpdateState();
        List<Vector3> pos = new List<Vector3>();
        Vector3 start = new Vector3((float)talentVO.MTalentSubNode.Value.LinkPoints.Value.NodeX, (float)talentVO.MTalentSubNode.Value.LinkPoints.Value.NodeY, 0);
        Vector3 end = new Vector3((float)talentVO.MTalentSubNode.Value.LinkPoints.Value.PreNodeX, (float)talentVO.MTalentSubNode.Value.LinkPoints.Value.PreNodeY, 0);
        DrawLinrRenderer(lineRenderer, new Vector3[] { start ,end});
        DrawLineImageRenderer(m_LineImage, new Vector3[] { start, end });
    }

    /// <summary>
    /// 设置坐标
    /// </summary>
    private void SetPos(Vector3 pos)
    {
        transform.localPosition = pos;
    }

    /// <summary>
    /// 划线
    /// </summary>
    public void DrawLinrRenderer(LineRenderer lineRenderer, Vector3 [] pos)
    {

        for (int i = 0; i < pos.Length; i++)
        {
            lineRenderer.SetPosition(i, pos[i]);
        }
        lineRenderer.enabled = false;
    }

    /// <summary>
    /// 划线
    /// </summary>
    public void DrawLineImageRenderer(LineImage lineImage, Vector3[] pos)
    {
        lineImage.DrawLine(pos[0],pos[1],m_TalentVO.Id);
    }

    /// <summary>
    /// 更新状态
    /// </summary>
    public void UpdateState()
    {
        //Debug.Log("更新"+m_TalentState);
        m_Animator.SetInteger("State", (int)m_TalentState);
        if (m_TalentState != TalentState.NoActivate)
        {
            m_LineRenderer.startColor = Color.white;
            m_LineRenderer.endColor = Color.white;
            m_LineImage.DrawColor(COLOR);
        }
        else
        {
            m_LineRenderer.startColor = m_Color;
            m_LineRenderer.endColor = m_Color;
            m_LineImage.DrawColor(m_Color);
        }
    }

    /// <summary>
    /// 升级动画  激活（另行）
    /// </summary>
    public void PlayUpLevel()
    {
        Debug.Log("升级动画");
        m_Animator.SetTrigger("UpGradeMove");
    }

    /// <summary>
    /// 激活动画
    /// </summary>
    public void ShowUnLock(bool s)
    {
        if (s != m_ActiveGameObject.activeSelf)
        {
            m_ActiveGameObject.SetActive(s);
        }
    }

    public void OnDisable()
    {
        m_LineRenderer.startColor = m_Color;
        m_LineRenderer.endColor = m_Color;
        m_LineImage.DrawColor(m_Color);
    }
}
