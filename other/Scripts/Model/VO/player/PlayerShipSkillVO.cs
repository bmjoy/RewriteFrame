using Eternity.FlatBuffer;
using Eternity.FlatBuffer.Enums;
using System.Collections.Generic;

public class PlayerShipSkillVO
{
    private int m_TemplateID;
    private SkillData m_SkillData;

    public PlayerShipSkillVO(int skillID)
    {
        m_TemplateID = skillID;

        CfgEternityProxy eternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
        m_SkillData = eternityProxy.GetSkillData(m_TemplateID);
    }

    public int GetID()
    {
        return m_TemplateID;
    }

    public int GetIcon()
    {
        return m_SkillData.BaseData.Value.Icon;
    }

    public CdType[] GetCDRealseCondition()
    {
        List<CdType> cdTypes = new List<CdType>();
        SkillBaseData baseData = m_SkillData.BaseData.Value;
        for(int i =0;i<baseData.CdDatasLength;++i)
        {
            CdData cdData = baseData.CdDatas(i).Value;
            cdTypes.Add(cdData.CdType);
        }

        return cdTypes.ToArray();
    }
}
