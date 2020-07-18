/*===============================
 * Author: [Allen]
 * Purpose: SkillRPCNet
 * Time: 2019/12/18 19:25:29
================================*/
using Crucis.Protocol;
using Crucis.Protocol.GameSession;
using System;
using System.Collections.Generic;


public class SkillRPCNet
{
    private CastSingSkillStream m_CastSingSkillStream;                                      //前摇技能流
    private CastAccumulationSkillStream m_CastAccumulationSkillStream;   //蓄力技能流
    private CastGuideSkillStream m_CastGuideSkillStream;                                //引导技能流


    private Action m_abortAction;

    public SkillRPCNet(Action abortAction)
    {
        m_abortAction = abortAction;
    }


    public void Close()
    {
        m_CastSingSkillStream?.Close();
        m_CastAccumulationSkillStream?.Close();
        m_CastGuideSkillStream?.Close();

        Leyoutech.Utility.DebugUtility.Log("SkillRPCNet", " Close" + this.GetHashCode());

    }


    /// <summary>
    /// 请求释放前摇技能
    /// </summary>
    /// <param name="skill_id_">技能ID</param>
    /// <param name="targetlist_">目标信息</param>
    /// <param name="quat_">摄像机朝向</param>
    /// <returns></returns>
    public async void CreatListenerCastSkill(uint entityId, uint skill_id_, List<Crucis.Protocol.TargetInfo> targetlist_, Crucis.Protocol.Vec4 quat_)
    {
        m_CastSingSkillStream = new CastSingSkillStream(entityId, skill_id_, targetlist_, quat_);
        CastSingSkillResponse response;
        while ((response = await m_CastSingSkillStream.ReadAsync()) != null)
        {
            Run_CastSingSkillResponse(entityId, skill_id_, response);
        }
        Leyoutech.Utility.DebugUtility.Log("SkillRPCNet", "关闭" + this.GetHashCode());
        m_CastSingSkillStream = null;
        m_abortAction?.Invoke();
    }

    /// <summary>
    /// 请求释放蓄力技能服务器验证
    /// </summary>
    /// <param name="request"></param>
    public async void CreatListenerCastAccumulationSkill(uint entityId, uint skill_id_)
    {
        Leyoutech.Utility.DebugUtility.Log("SkillRPCNet", " 请求释放蓄力");


        m_CastAccumulationSkillStream = new CastAccumulationSkillStream();
        CastAccumulationSkillResponse response;
        while ((response = await m_CastAccumulationSkillStream.ReadAsync()) != null)
        {
            Run_CastAccumulationSkillResponse(entityId, response);
        }
        Leyoutech.Utility.DebugUtility.Log("SkillRPCNet", "关闭" + this.GetHashCode());
        m_CastAccumulationSkillStream = null;
        m_abortAction?.Invoke();
    }


    /// <summary>
    /// 请求引导技能服务器验证
    /// </summary>
    /// <param name="request"></param>
    public async void CreatListenerCastGuideSkill(uint entityId, uint skill_id_)
    {
        m_CastGuideSkillStream = new CastGuideSkillStream();
        CastGuideSkillResponse response;
        while ((response = await m_CastGuideSkillStream.ReadAsync()) != null)
        {
            Run_CastGuideSkillResponse(entityId, response);
        }
        Leyoutech.Utility.DebugUtility.Log("SkillRPCNet", "关闭" + this.GetHashCode());
        m_CastGuideSkillStream = null;
        m_abortAction?.Invoke();
    }



    /// <summary>
    /// 蓄力技能流写入
    /// </summary>
    /// <param name="entityId"></param>
    /// <param name="skill_id_"></param>
    /// <param name="request"></param>
    public void CastAccumulationSkillWright(uint entityId, uint skill_id_, AccumulationSkill request)
    {
        Leyoutech.Utility.DebugUtility.Log("SkillRPCNet", " 蓄力技能写入" + this.GetHashCode());
        m_CastAccumulationSkillStream?.Wright(request);
    }

    /// <summary>
    /// 引导技能流写入
    /// </summary>
    /// <param name="entityId"></param>
    /// <param name="skill_id_"></param>
    /// <param name="request"></param>
    public void CastGuideSkillWright(uint entityId, uint skill_id_, GuideSkill request)
    {
        Leyoutech.Utility.DebugUtility.Log("SkillRPCNet", "引导技能流写入" + this.GetHashCode());

        m_CastGuideSkillStream?.Wright(request);
    }


    /// <summary>
    /// 前摇技能流监听相应
    /// </summary>
    /// <param name="entityId"></param>
    /// <param name="skill_id_"></param>
    /// <param name="message"></param>
    public void Run_CastSingSkillResponse(uint entityId, uint skill_id_, CastSingSkillResponse message)
    {
        CastSingSkillResponse.Types.Success success = message.Success;
        if (success != null)
        {
            CastSkillResult result = success.Success_.Result;
            EmitNode nodeinfo = success.Success_.NodeInfo;
            StopSkill stopskill = success.Success_.StopSkill;

            // 处理是否可以释放的返回结果
            if (result != null)
            {
                DisposeCastSkillResult(result);
            }

            //随机发射点
            if (nodeinfo != null)
            {
                DisposeEmitNodeResult(entityId, nodeinfo);
            }

            // 执行取消技能
            if (stopskill != null)
            {
                DisposeStopResult(stopskill);
            }
        }
    }


    /// <summary>
    /// 蓄力技能流监听响应
    /// </summary>
    /// <param name="message"></param>
    public void Run_CastAccumulationSkillResponse(uint entityId, CastAccumulationSkillResponse message)
    {
        CastAccumulationSkillResponse.Types.Success success = message.Success;
        if (success != null)
        {
            CastSkillResult result = success.Success_.Result;
            AccumulationSkillEndResult end_result = success.Success_.EndResult;
            EmitNode node_info = success.Success_.NodeInfo;
            StopSkill stopskill = success.Success_.StopSkill;

            Leyoutech.Utility.DebugUtility.Log("SkillRPCNet", " 蓄力技能流监听响应" + this.GetHashCode());


            // 处理是否可以释放的返回结果
            if (result != null)
            {
                DisposeCastSkillResult(result);
            }

            //处理蓄力按键抬起
            if (end_result != null)
            {
                DisposeAccumulationSkillEndResult(entityId, end_result);
            }

            //随机发射点
            if (node_info != null)
            {
                DisposeEmitNodeResult(entityId, node_info);
            }

            // break 阶段
            if (stopskill != null)
            {
                DisposeStopResult(stopskill);
            }
        }
    }

    /// <summary>
    /// 引导技能流监听响应
    /// </summary>
    /// <param name="message"></param>
    public void Run_CastGuideSkillResponse(uint entityId, CastGuideSkillResponse message)
    {
        CastGuideSkillResponse.Types.Success success = message.Success;
        if (success != null)
        {
            CastSkillResult result = success.Success_.Result;
            EmitNode node_info = success.Success_.NodeInfo;
            StopSkill stopskill = success.Success_.StopSkill;
            GuideSkillEndResult guideSkillEndResult = success.Success_.EndResult;
            ChangeGuideSkillTargetInfoResult changeGuideSkillTargetInfoResult = success.Success_.ChangeResult;

            Leyoutech.Utility.DebugUtility.Log("SkillRPCNet", " 引导技能流监听响应" + this.GetHashCode());


            // 处理是否可以释放的返回结果
            if (result != null)
            {
                DisposeCastSkillResult(result);
            }

            //随机发射点
            if (node_info != null)
            {
                DisposeEmitNodeResult(entityId, node_info);
            }

            // 执行取消技能
            if (stopskill != null)
            {
                DisposeStopResult(stopskill);
            }

            //引导按键抬起，end 阶段
            if (guideSkillEndResult != null)
            {
                DisposeGuideSkillEndResult(guideSkillEndResult);
            }

            //目标改变
            if (changeGuideSkillTargetInfoResult != null)
            {

            }
        }
    }





    /// <summary>
    /// 处理是否可以释放的返回结果
    /// </summary>
    private static void DisposeCastSkillResult(CastSkillResult result)
    {
        if (result == null)
        {
            Leyoutech.Utility.DebugUtility.LogError("技能释放验证","服务器下行消息  --- > CastSkillResult 为null ,服务器检查下 ");
            return;
        }

        BaseEntity entity = GameplayManager.Instance.GetEntityManager().GetEntityById<BaseEntity>(result.CasterId);
        if (entity == null)
            return;

        CaseSkillResult caseSkillResult = new CaseSkillResult();
        if (result.Code)
        {
            //验证成功
            uint skillid = result.SkillId;
            caseSkillResult.skillId = (int)skillid;
            caseSkillResult.succeed = true;
            Leyoutech.Utility.DebugUtility.Log("技能释放验证", string.Format("验证成功 SkillId = {0}", skillid));
        }
        else
        {
            //验证失败
            uint skillid = result.SkillId;
            caseSkillResult.skillId = (int)skillid;
            caseSkillResult.succeed = false;

            Leyoutech.Utility.DebugUtility.Log("技能释放验证", string.Format("验证失败 SkillId = {0}", skillid));
        }
        entity.SendEvent(ComponentEventName.CaseSkillResult, caseSkillResult);
    }

    /// <summary>
    /// 执行取消技能
    /// </summary>
    /// <param name="result"></param>
    private void DisposeStopResult(StopSkill result)
    {
        if (result == null)
        {
            Leyoutech.Utility.DebugUtility.LogError("技能停止","服务器下行消息  --- >StopSkill 为null ,服务器检查下 ");
            return;
        }

        BaseEntity entity = GameplayManager.Instance.GetEntityManager().GetEntityById<BaseEntity>(result.CasterId);
        if (entity == null)
            return;

        //结束技能
        StopSkillResult endResult = new StopSkillResult();
        uint skillid = result.SkillId;
        endResult.skillId = (int)skillid;
        entity.SendEvent(ComponentEventName.ToStopSkill, endResult);
    }

    /// <summary>
    /// 引导技能结束
    /// </summary>
    /// <param name="result"></param>
    private void DisposeGuideSkillEndResult(GuideSkillEndResult result)
    {
        if (result == null)
        {
            Leyoutech.Utility.DebugUtility.LogError("引导技能结束", "服务器下行消息  --- >GuideSkillEndResult 为null ,服务器检查下 ");
            return;
        }

        Leyoutech.Utility.DebugUtility.Log("引导技能结束", "服务器下行消息  --- >GuideSkillEndResult ");


        BaseEntity entity = GameplayManager.Instance.GetEntityManager().GetEntityById<BaseEntity>(result.CasterId);
        if (entity == null)
            return;

        EndSkillResult enResult = new EndSkillResult();
        uint skillid = result.SkillId;
        enResult.skillId = (int)skillid;
        entity.SendEvent(ComponentEventName.ToEndSkill, enResult);
    }




    /// <summary>
    /// 处理蓄力抬起
    /// </summary>
    private void DisposeAccumulationSkillEndResult(uint entityId, AccumulationSkillEndResult result)
    {
        if (result == null)
        {
            Leyoutech.Utility.DebugUtility.LogError("蓄力抬起", "服务器下行消息  --- >AccumulationSkillEndResult 为null ,服务器检查下 ");
            return;
        }

        BaseEntity entity = GameplayManager.Instance.GetEntityManager().GetEntityById<BaseEntity>(entityId);
        if (entity == null)
            return;

        if (result.Code)
        {
            //成功
            AccumulationIndexResult accResult = new AccumulationIndexResult();
            accResult.skillId = (int)result.SkillId;
            accResult.accumulationIndex = (int)result.GroupIndex;
            entity.SendEvent(ComponentEventName.AccumulationIndex, accResult);
        }
        else
        {
            //失败
            EndSkillResult endResult = new EndSkillResult();
            uint skillid = result.SkillId;
            endResult.skillId = (int)skillid;
            entity.SendEvent(ComponentEventName.ToEndSkill, endResult);
        }
    }

    /// <summary>
    /// 处理随机发射点
    /// </summary>
    private void DisposeEmitNodeResult(uint entityId, EmitNode node_info)
    {
        if (node_info == null)
        {
            Leyoutech.Utility.DebugUtility.LogError("随机发射点", "服务器下行消息  --- >EmitNode 为null ,服务器检查下 ");
            return;
        }

        BaseEntity entity = GameplayManager.Instance.GetEntityManager().GetEntityById<BaseEntity>(entityId);
        if (entity == null)
            return;

        Leyoutech.Utility.DebugUtility.LogWarning("随机发射点", string.Format("收到随机发射点entity = {0}, 发射点类型 = {1}，indexs = {2} , assign_index  = {3}" , 
            entityId,
            (int)node_info.NodeType ,
            string.Join(",",node_info.IndexList), 
            (int)node_info.AssignIndex));

        RandomEmitNodeResult nodeResult = new RandomEmitNodeResult();
        nodeResult.emitNode = node_info;
        entity.SendEvent(ComponentEventName.RandomEmitNode, nodeResult);
    }
}