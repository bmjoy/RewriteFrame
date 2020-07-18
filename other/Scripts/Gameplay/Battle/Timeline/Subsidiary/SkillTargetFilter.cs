/*===============================
 * Author: [Allen]
 * Purpose: 技能目标筛选
 * Time: 2020/1/20 18:12:28
================================*/

using Eternity.FlatBuffer.Enums;
using System;
using System.Collections.Generic;
using UnityEngine;
using Leyoutech.Core.Timeline;

namespace Gameplay.Battle.Timeline
{

    /// <summary>
    /// 技能目标筛选器
    /// </summary>
    public abstract  class TargetFilter
    {
        /// <summary>
        /// 筛选器自己所属的单位
        /// </summary>
        protected BaseEntity m_OwerEntity = null;

        /// <summary>
        /// 准备执行筛选的单位列表
        /// </summary>
        protected LinkedList<BaseEntity> m_returnEntitys;


        /// <summary>
        /// 获取满足条件的单位列表
        /// </summary>
        /// <returns></returns>
        public abstract LinkedList<BaseEntity> GetTargetEntitys();


        public void SetTargetEntitys(BaseEntity ower,LinkedList<BaseEntity> linkedList)
        {
            m_returnEntitys = linkedList;
            m_OwerEntity = ower;
        }
    }


    public class SkillTargetFilter : TargetFilter
    {
        public override LinkedList<BaseEntity> GetTargetEntitys()
        {
            return m_returnEntitys;
        }
    }





    /// <summary>
    /// 装饰者
    /// </summary>
    public abstract class SkillTargetFilterDecorator : SkillTargetFilter
    {
        protected SkillTargetFilter m_fatherFilter;


        public SkillTargetFilterDecorator(SkillTargetFilter filter)
        {
            this.m_fatherFilter = filter;
        }

        /// <summary>
        /// 执行过滤
        /// </summary>
        protected abstract void TodoFilter();


        public override LinkedList<BaseEntity> GetTargetEntitys()
        {
            if (m_fatherFilter != null)
            {
                TodoFilter();
                return m_fatherFilter.GetTargetEntitys();
            }
            return null;
        }
    }

    /// <summary>
    /// 阵营筛选器
    /// </summary>
    public class FactionTargetFilter : SkillTargetFilterDecorator
    {
        private GameplayProxy gameplayProxy;
        /// <summary>
        /// 筛选的阵营
        /// </summary>
        private FactionType m_factionType = FactionType.All;


        public FactionTargetFilter(SkillTargetFilter filtrate, FactionType factionType ) : base(filtrate)
        {
            gameplayProxy = GameFacade.Instance.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;
            m_factionType = factionType;
        }

        protected override void TodoFilter()
        {
            m_returnEntitys = m_returnEntitys.RemoveAll(entity => gameplayProxy.IsBelongToThisFactionType(m_factionType, m_OwerEntity, entity) == false); //IsBelongToThisFactionType是否属于某个阵营关系
        }
    }


}
