/*===============================
 * Author: [Allen]
 * Purpose: 十字线，
 * Time: 2020/3/18 17:45:12
================================*/

public class WeaponAndCrossSight_Line : WeaponAndCrossSight
{

    public WeaponAndCrossSight_Line(ulong uid, uint tId, ulong skillId) : base(uid, tId, skillId)
    {
    }

    public override void Init()
    {
        base.Init();
        m_CrossSightInfo = new CrossSightInfo();
        m_CrossSightInfo.m_MaxRayDistance = m_SkillMaxDistance;
        m_CrossSightInfo.m_CrossSightShape = CrossSightShape.CrossLine;
        m_CrossSightLoic = new CCrossSightLoic();
        m_CrossSightLoic = new CrossSightLoic_Improve_line(m_CrossSightLoic);
        m_CrossSightLoic.SetCrossSightInfo(m_CrossSightInfo);
    }
}

