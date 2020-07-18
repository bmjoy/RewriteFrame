using Eternity.FlatBuffer.Enums;
using UnityEngine;

public class PlayerSkillCDVO
{
    private CdType m_CdType = CdType.Skill;
    private int m_SkillID = -1;

    private float m_StartTime = 0.0f;
    private float m_EndTime = 0.0f;

    public PlayerSkillCDVO(CdType cdType)
    {
        m_CdType = cdType;

        m_StartTime = m_EndTime = Time.unscaledTime;
    }

    public PlayerSkillCDVO(int skillID,CdType cdType):this(cdType)
    {
        this.m_SkillID = skillID;
    }

    public float GetProgress()
    {
        if(IsInCD())
        {
            return (Time.unscaledTime - m_StartTime) / (m_EndTime - m_StartTime);
        }
        return 0.0f;
    }

    public float GetRemainingTime()
    {
        float remainingTime = m_EndTime - Time.unscaledTime;
        if(remainingTime>=0)
        {
            return remainingTime;
        }
        return 0.0f;
    }

    public void SetCD(float cdTime)
    {
        m_StartTime = Time.unscaledTime;
        m_EndTime = m_StartTime + cdTime;
    }

    public bool IsInCD()
    {
        return Time.unscaledTime < m_EndTime;
    }
}