/*===============================
 * Author: [Allen]
 * Purpose: 矿抢
 * Time: 2020/3/18 12:08:48
================================*/
using Eternity.FlatBuffer;

public class WeaponAndCrossSight_Mining : WeaponAndCrossSight
{
    /// <summary>
    ///矿抢武器专有属性
    /// </summary>
    private WeaponMiningLaser m_WeaponFireData;


    /// <summary>
    /// 这次按键的按下, 已经释放过技能了
    /// </summary>
    private bool m_FiredDuringThisPress = false;


    public WeaponAndCrossSight_Mining(ulong uid, uint tId, ulong skillId) : base(uid, tId, skillId)
    {
    }

    public override void Init()
    {
        base.Init();
        m_WeaponFireData = m_CfgEternityProxy.GetWeaponDataOfMining(m_WeaponTable.TypeDateSheetld);

        m_CrossSightInfo = new CrossSightInfo();
        m_CrossSightInfo.m_MaxRayDistance = m_SkillMaxDistance;
        m_CrossSightInfo.m_CrossSightShape = CrossSightShape.CrossLine;
        m_CrossSightInfo.m_LineInfo.detectionMode = DetectionMode.All;
        m_CrossSightLoic = new CCrossSightLoic();
        m_CrossSightLoic = new CrossSightLoic_Improve_line(m_CrossSightLoic);
        m_CrossSightLoic.SetCrossSightInfo(m_CrossSightInfo);

        m_FireInterval = WeaponAndCrossSightFactory.CalculateFireInterval(m_MainPlayer, m_WeaponTable.TypeDateSheetld, m_UId, WeaponAndCrossSight.WeaponAndCrossSightTypes.Mining);
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

