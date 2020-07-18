using Eternity.FlatBuffer;
using UnityEngine;

public interface ICampProperty
{
	bool IsMain();
	uint GetUId();
	uint GetTemplateID();
	uint GetCampID();
	void SetPrestige(uint campType, uint prestige);
}

public class CampComponent : EntityComponent<ICampProperty>
{
	private ICampProperty m_Property;

	public override void OnInitialize(ICampProperty property)
	{
		m_Property = property;
		InitPrestige();
	}

	/// <summary>
	/// FIXME: 临时函数, 现在直接使用camp_list表里的声望默认值来初始化对不同阵营的声望
	/// </summary>
	private void InitPrestige()
	{
		// camplist表中, 阵营ID从1开始
		uint camp = m_Property.GetCampID();
		if (camp < 1)
		{
			Debug.LogErrorFormat("templateID为{0}的怪阵营错误", m_Property.GetTemplateID());
		}

        CfgEternityProxy cfgEternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;

		int count = cfgEternityProxy.GetCampCount();
		for (int campIndex = 0; campIndex < count; campIndex++)
		{
			CampList campVO = cfgEternityProxy.GetCampVOByIndex(campIndex);
			CampNode? campnode = campVO.NodeCampList(campIndex);
			if (!campnode.HasValue)
			{
				Debug.LogError("CampList表错误. 不该出现的错误");
				continue;
			}

			m_Property.SetPrestige(campnode.Value.CampOther, cfgEternityProxy.GetCampRelationVO(camp, campnode.Value.CampOther).Value.PrestigeInit);
		}
	}
}
