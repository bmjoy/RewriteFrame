using Leyoutech.Core.Effect;
using Eternity.FlatBuffer;
using Game.VFXController;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Define;

/// <summary>
/// Buff的实例
/// </summary>
public class Buff
{
	/// <summary>
	/// Buff的数据
	/// </summary>
	public BuffVO VO;

	/// <summary>
	/// Buff 的客户端逻辑效果. 
	/// 比如隐身后不可选中
	/// </summary>
	public BuffEffectBase BuffEffect;

	public IBuffProperty BuffProperty;

	// Cache
	private CfgSkillSystemProxy m_SkillProxy;
    private GameplayProxy m_GameplayProxy;

    public Buff(BuffVO vo, IBuffProperty property)
	{
		VO = vo;
		BuffProperty = property;

		// Cache
		m_SkillProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgSkillSystemProxy) as CfgSkillSystemProxy;
        m_GameplayProxy = GameFacade.Instance.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;

        Leyoutech.Utility.DebugUtility.LogWarning("Buff", string.Format("创建一个Buff ---> 归属entity = {0} , Buff ID = {1}", BuffProperty.EntityId(), VO.ID));

        SkillBuff configVO = m_SkillProxy.GetBuff((int)VO.ID);
		if (configVO.ByteBuffer != null)
		{
			BuffEffect = BuffEffectBase.GetBuffEffectByType((BuffEffectType)configVO.BuffEffectId, this);
            Transform selfTf = BuffProperty.GetRootTransform();
            SpacecraftEntity otherEntity = m_GameplayProxy.GetEntityById<SpacecraftEntity>(VO.Link_id) as SpacecraftEntity;
            Transform otherTf = null;
            if(otherEntity != null && !otherEntity.IsDead())
            {
                otherTf = otherEntity.GetRootTransform();
            }

            BuffEffect.Init(selfTf, otherTf);
        }
		else
		{
            BuffEffect = new BuffEffectBase(this);
		}
	}

	public int GetEffect()
	{
		SkillBuff configVO = m_SkillProxy.GetBuff((int)VO.ID);
		if (configVO.ByteBuffer != null)
		{
			return configVO.PropEffect;
		}
		return 0;
	}

	public void OnAddBuff()
	{
        CreateStartFXAndLoopFX();
		BuffEffect.OnBuffAdd();
	}

	public void OnRemoveBuff()
	{
        Leyoutech.Utility.DebugUtility.LogWarning("Buff", string.Format("移除Buff ---> 归属entity = {0} , Buff ID = {1}", BuffProperty.EntityId(), VO.ID));
        StopEffctVFX();
        RemoveBuffFXAndCreateEndFX();
	}

    private void StopEffctVFX()
    {
        if(BuffEffect !=null)
        {
            List<EffectController> buffEffcollerlist = BuffEffect.GetCurrBuffEffectControllers();
            for (int i = 0; i < buffEffcollerlist.Count; i++)
            {
                EffectController controller = buffEffcollerlist[i];
                if (controller != null)
                {
                    BuffProperty.RemoveVFX(controller);
                }
            }
            BuffEffect.OnBuffRemove();
        }
    }


    ~Buff()
    {
        StopEffctVFX();
        BuffEffect = null;
    }

    /// <summary>
    /// 创建开始,循环阶段特效
    /// </summary>
	private void CreateStartFXAndLoopFX()
	{
        if(VO.Link_id>0 && !VO.Is_master)
        {
            //连线 buff, 被链接方，不播特效
            return;
        }


		SkillBuff buffConfig = m_SkillProxy.GetBuff((int)VO.ID);

		CfgSkillSystemProxy skillProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgSkillSystemProxy) as CfgSkillSystemProxy;

		if (Leyoutech.Utility.EffectUtility.IsEffectNameValid(buffConfig.StartFx))
		{
			EffectController StartFX = EffectManager.GetInstance().CreateEffect(buffConfig.StartFx, EffectManager.GetEffectGroupNameInSpace(BuffProperty.IsMain()));
			//StartFX.transform.SetParent(BuffEffect.m_SelfTransform, false);
            BuffEffect.SetStartEffectController(StartFX);
            StartFX.SetCreateForMainPlayer(BuffProperty.IsMain());
		}

		if (Leyoutech.Utility.EffectUtility.IsEffectNameValid(buffConfig.LoopFx))
		{
            EffectController LoopFX = EffectManager.GetInstance().CreateEffect(buffConfig.LoopFx, EffectManager.GetEffectGroupNameInSpace(BuffProperty.IsMain()), OnCreateStartAndLoopFX,this);
            //LoopFX.transform.SetParent(BuffEffect.m_SelfTransform, false);
            BuffEffect.SetLoopEffectController(LoopFX);
            LoopFX.SetCreateForMainPlayer(BuffProperty.IsMain());
		}
	}

	private void RemoveBuffFXAndCreateEndFX()
	{
        if (VO.Link_id > 0 && !VO.Is_master)
        {
            //连线 buff, 被链接方，不播特效
            return;
        }

        // 创建结束特效
        SkillBuff buffVO = m_SkillProxy.GetBuff((int)VO.ID);

		if (Leyoutech.Utility.EffectUtility.IsEffectNameValid(buffVO.EndFX))
		{
			EffectController EndFX = EffectManager.GetInstance().CreateEffect(buffVO.EndFX, EffectManager.GetEffectGroupNameInSpace(BuffProperty.IsMain()));
			//EndFX.transform.SetParent(BuffEffect.m_SelfTransform, false);
            BuffEffect.SetEndEffectController(EndFX);
            EndFX.SetCreateForMainPlayer(BuffProperty.IsMain());
		}
	}

	private void OnCreateStartAndLoopFX(EffectController buffFX, System.Object usedata)
	{
        if(usedata != null)
        {
            Buff buff = (Buff)usedata;

            if (buff != null)
            {
               // buff.VFXList?.Add(buffFX);
                buff.BuffProperty?.AddVFX(buffFX);

                if (buff.BuffEffect != null)
                {
                    buff.BuffEffect.OnLoopFXLoaded(buffFX);
                }
            }
            else
            {
                buffFX.RecycleFX();
            }
        }
        else
        {
            buffFX.RecycleFX();
        }
    }
}
