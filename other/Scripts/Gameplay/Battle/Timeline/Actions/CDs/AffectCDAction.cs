using Eternity.FlatBuffer;
using Leyoutech.Core.Timeline;

namespace Gameplay.Battle.Timeline.Actions.CDs
{
    public class AffectCDAction : AEventActionItem
    {
        public override void Trigger()
        {
            IBaseActionProperty property = m_Context.GetObject<IBaseActionProperty>();
            if (!property.IsMain())
            {
                return;
            }

            AffectCDData cdData = GetData<AffectCDData>();
            SkillData skillData = m_Context.GetObject<SkillData>();

            PlayerSkillProxy skillProxy = GameFacade.Instance.RetrieveProxy(ProxyName.PlayerSkillProxy) as PlayerSkillProxy;
            for(int i = 0;i<cdData.CdDatasLength;++i)
            {
                CdData data = cdData.CdDatas(i).Value;
                skillProxy.AddCD(skillData.Id, data.CdType, data.CdTime);
            }
        }
    }
}
