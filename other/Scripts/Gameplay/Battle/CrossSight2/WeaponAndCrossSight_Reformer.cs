/*===============================
 * Author: [Allen]
 * Purpose: 转化炉
 * Time: 2020/3/21 19:20:46
================================*/

using Eternity.FlatBuffer;

public class WeaponAndCrossSight_Reformer : WeaponAndCrossSight
{
    /// <summary>
    /// 这次按键的按下, 已经释放过技能了
    /// </summary>
    private bool m_FiredDuringThisPress = false;


    public WeaponAndCrossSight_Reformer(ulong uid, uint tId, ulong skillId) : base(uid, tId, skillId)
    {
    }

    public override void Init()
    {
        base.Init();

        m_CrossSightInfo = new CrossSightInfo();
        m_CrossSightInfo.m_MaxRayDistance = m_SkillMaxDistance;
        m_CrossSightInfo.m_CrossSightShape = CrossSightShape.CrossLine;
        m_CrossSightInfo.m_LineInfo.detectionMode = DetectionMode.All;
        m_CrossSightLoic = new CCrossSightLoic();
        m_CrossSightLoic = new CrossSightLoic_Improve_line(m_CrossSightLoic);
        m_CrossSightLoic.SetCrossSightInfo(m_CrossSightInfo);
    }

    /// <summary>
    /// 是否是普通武器 or 转化炉武器
    /// </summary>
    public override bool IsOrdinary()
    {
        return false;
    }

    /// <summary>
    /// 是否满足释放条件
    /// </summary>
    public override bool IsCanRelease()
    {
        return m_MainPlayer.GetCurrentState().IsHasSubState(EnumSubState.Peerless);
      //  return base.IsCanRelease();
    }


    public override void OnHotKeyDown(int skillID)
    {
        base.OnHotKeyDown(skillID);

        if (!m_FiredDuringThisPress && m_PlayerSkillProxy.CanCurrentWeaponRelease())
        {
            //Leyoutech.Utility.DebugUtility.Log("武器", "热键按下，请求释放技能");

            //执行释放
            SkillCastEvent castSkillEvent = new SkillCastEvent();
            castSkillEvent.IsWeaponSkill = true;
            castSkillEvent.SkillIndex = skillID;
            castSkillEvent.KeyPressed = true;
            m_MainPlayer.SendEvent(ComponentEventName.SkillButtonResponse, castSkillEvent);
            m_FiredDuringThisPress = true;
        }
        else if (!m_FiredDuringThisPress && !m_PlayerSkillProxy.CanCurrentWeaponRelease())
        {
            //开火失败
            GameFacade.Instance.SendNotification(NotificationName.PlayerWeaponFireFail);
        }
        else
        {
            // 已经放过技能了, 不再发送释放技能消息. 解决武器过热以后还会继续射击的问题
        }
    }

    /// <summary>
    /// 热键抬起
    /// </summary>
    /// <param name="skillindex"></param>
    public override void OnHotKeyUp(int skillID)
    {
        base.OnHotKeyUp(skillID);
        //Leyoutech.Utility.DebugUtility.Log("武器", "热键抬起，请求释放技能结束");

        SkillCastEvent castSkillEvent = new SkillCastEvent();
        castSkillEvent.IsWeaponSkill = true;
        castSkillEvent.SkillIndex = skillID;
        castSkillEvent.KeyPressed = false;

        m_MainPlayer.SendEvent(ComponentEventName.SkillButtonResponse, castSkillEvent);
        m_FiredDuringThisPress = false;
    }
}