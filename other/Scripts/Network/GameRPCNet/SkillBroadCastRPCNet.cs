/*===============================
 * Author: [Allen]
 * Purpose: 技能广播网络
 * Time: 2020/3/12 10:27:16
================================*/
using Assets.Scripts.Proto;
using Crucis.Protocol.GameSession;
using Eternity.FlatBuffer.Enums;
using UnityEngine;

namespace Crucis.Protocol
{
    public static class SkillBroadCastRPCNet
    {
        private static BroadSkillInfoStream m_Stream;

        public static async void Handle()
        {
            m_Stream?.Close();
            m_Stream = new BroadSkillInfoStream();
            BroadSkillInfoResponse response;
            while ((response = await m_Stream.ReadAsync()) != null)
            {
                SkillBroadCastRPCNet.Run(response);
            }
        }

        private static void Run(GameSession.BroadSkillInfoResponse message)
        {
            GameSession.BroadSkillInfoResponse.Types.Success success = message.Success;
            if (success != null)
            {
                SkillSelectTargetInfoBroadcast target_info = success.Success_.TargetInfo;//吟唱和引导技能开始
                SkillEmitNodeBroadcast node_info = success.Success_.NodeInfo ;//随机挂点同步
                StopSkill stop_skill = success.Success_.StopSkill;//技能break
                GuideSkillEnd guide_end = success.Success_.GuideEnd ;//引导技能正常结束
                SkillChangeTargetBroadcast change_result = success.Success_.ChangeResult;//引导技能切换目标
                AccumulationSkillEndBroadcast accumalation_end = success.Success_.AccumalationEnd;//蓄力技能蓄力结束（抬起按键）
                AccumulationSkillBegin accumulation_skill_begin = success.Success_.AccumulationSkillBegin;//蓄力技能蓄力开始(按下按键)
                AddBuf add_buf = success.Success_.AddBuf; //添加buf
                DeleteBuf delete_buf = success.Success_.DeleteBuf; //删除buf
               SyncClipInfo clip_info = success.Success_.ClipInfo; // 弹夹信息


                // 处理吟唱和引导技能开始
                if (target_info != null)
                {
                    DisposeSkillSelectTargetInfoBroadcast(target_info);
                }

                // 随机挂点同步
                if (node_info != null)
                {
                    DisposeSkillEmitNodeBroadcast(node_info);
                }

                // 技能break
                if (stop_skill != null)
                {
                    DisposeStopSkill(stop_skill);
                }

                // 引导技能正常结束
                if (guide_end != null)
                {
                    DisposeGuideSkillEnd(guide_end);
                }

                // 引导技能切换目标
                if (change_result != null)
                {
                    DisposeSkillChangeTargetBroadcast( change_result);
                }

                // 蓄力技能蓄力结束（抬起按键）
                if (accumalation_end != null)
                {
                    DisposeAccumulationSkillEndBroadcast(accumalation_end);
                }

                // 蓄力技能蓄力开始(按下按键)
                if (accumulation_skill_begin != null)
                {
                    DisposeAccumulationSkillBegin(  accumulation_skill_begin);
                }

                //添加buf
                if (add_buf != null)
                {
                    DisposeAddBuf(add_buf);
                }

                //删除buf
                if (delete_buf != null)
                {
                    DisposeDeleteBuf(delete_buf);
                }

                //弹夹信息  
                if (clip_info !=null)
                {
                    DisposeSyncClipInfo(clip_info);
                }
            }
        }

        /// <summary>
        /// 处理吟唱和引导技能开始
        /// </summary>
        private static void DisposeSkillSelectTargetInfoBroadcast(SkillSelectTargetInfoBroadcast result)
        {
            BaseEntity entity = GameplayManager.Instance.GetEntityManager().GetEntityById<BaseEntity>(result.CasterId);
            if (entity == null)
                return;
            BroadCastSkill_BeginTargets bctarges =  new BroadCastSkill_BeginTargets();
            uint skillid = result.SkillId;
            bctarges.skillId = (int)skillid;
            bctarges.targets = result.TargetList;
            bctarges.direction = result.TargetPosition;

            Leyoutech.Utility.DebugUtility.LogWarning("广播技能", string.Format("吟唱和引导技能开始 SkillId = {0}", skillid));

            entity.SendEvent(ComponentEventName.BroadCastSkill_BeginTargets, bctarges);
        }

        /// <summary>
        /// 处理随机挂点同步
        /// </summary>
        private static void DisposeSkillEmitNodeBroadcast(SkillEmitNodeBroadcast result)
        {
            BaseEntity entity = GameplayManager.Instance.GetEntityManager().GetEntityById<BaseEntity>(result.CasterId);
            if (entity == null)
                return;

            if (entity.GetCurrSkillId() <0 || entity.GetCurrSkillId() != (int)result.SkillId) //小于0 则不再释放中
                return;

            Leyoutech.Utility.DebugUtility.LogWarning("随机发射点", string.Format("收到随机发射点entity = {0},  SkillId = {1},发射点类型 = {2}，index = {3} , assign_index  = {4}",
                entity.UId(),
                (int)result.SkillId,
                result.NodeInfo.NodeType,
                string.Join(",", result.NodeInfo.IndexList),
                result.NodeInfo.AssignIndex));

            RandomEmitNodeResult nodeResult = new RandomEmitNodeResult();
            nodeResult.emitNode = result.NodeInfo;
            entity.SendEvent(ComponentEventName.RandomEmitNode, nodeResult);
        }

        /// <summary>
        /// 处理技能break
        /// </summary>
        private static void DisposeStopSkill(StopSkill result)
        {
            BaseEntity entity = GameplayManager.Instance.GetEntityManager().GetEntityById<BaseEntity>(result.CasterId);
            if (entity == null)
                return;

            if (entity.GetCurrSkillId() < 0 || entity.GetCurrSkillId() != (int)result.SkillId) //小于0 则不再释放中
                return;

            Leyoutech.Utility.DebugUtility.LogWarning("广播技能", string.Format("处理技能break， SkillId = {0}", result.SkillId));


            //结束技能
            StopSkillResult endResult = new StopSkillResult();
            uint skillid = result.SkillId;
            endResult.skillId = (int)skillid;
            entity.SendEvent(ComponentEventName.ToStopSkill, endResult);
        }



        /// <summary>
        /// 处理蓄力技能蓄力开始(按下按键)
        /// </summary>
        private static void DisposeAccumulationSkillBegin(AccumulationSkillBegin  result)
        {
            BaseEntity entity = GameplayManager.Instance.GetEntityManager().GetEntityById<BaseEntity>(result.CasterId);
            if (entity == null)
                return;

            Leyoutech.Utility.DebugUtility.LogWarning("广播技能", string.Format("蓄力开始(按下按键) SkillId = {0}", result.SkillId));

            //释放技能
            BroadCastSkill_ReleaseSkill releaseSkill = new BroadCastSkill_ReleaseSkill();
            releaseSkill.skillId =(int) result.SkillId;
            entity. SendEvent(ComponentEventName.BroadCastSkill_ReleaseSkill, releaseSkill);
        }

        /// <summary>
        /// 处理蓄力技能蓄力结束（抬起按键）
        /// </summary>
        private static void DisposeAccumulationSkillEndBroadcast(AccumulationSkillEndBroadcast result)
        {
            BaseEntity entity = GameplayManager.Instance.GetEntityManager().GetEntityById<BaseEntity>(result.CasterId);
            if (entity == null)
                return;

            if (entity.GetCurrSkillId() < 0 || entity.GetCurrSkillId() != (int)result.SkillId) //小于0 则不再释放中
                return;


            BroadCastSkill_Accumulation bctarges = new BroadCastSkill_Accumulation();
            uint skillid = result.SkillId;
            bctarges.skillId = (int)skillid;
            bctarges.targets = result.TargetList;
            bctarges.direction = result.TargetPosition;
            bctarges.groupIndex = (int)result.GroupIndex;

            Leyoutech.Utility.DebugUtility.LogWarning("广播技能", string.Format("蓄力结束（抬起按键） SkillId = {0}", skillid));

            entity.SendEvent(ComponentEventName.BroadCastSkill_Accumulation, bctarges);
        }

        /// <summary>
        /// 引导技能正常结束
        /// </summary>
        private static void DisposeGuideSkillEnd(GuideSkillEnd result)
        {
            BaseEntity entity = GameplayManager.Instance.GetEntityManager().GetEntityById<BaseEntity>(result.CasterId);
            if (entity == null)
                return;

            if (entity.GetCurrSkillId() < 0 || entity.GetCurrSkillId() != (int)result.SkillId) //小于0 则不再释放中
                return;


            EndSkillResult enResult = new EndSkillResult();
            uint skillid = result.SkillId;
            enResult.skillId = (int)skillid;

            Leyoutech.Utility.DebugUtility.LogWarning("广播技能", string.Format("引导技能正常结束  SkillId = {0}", skillid));

            entity.SendEvent(ComponentEventName.ToEndSkill, enResult);
        }


        /// <summary>
        /// 引导技能切换目标
        /// </summary>
        private static void DisposeSkillChangeTargetBroadcast(SkillChangeTargetBroadcast result)
        {
            BaseEntity entity = GameplayManager.Instance.GetEntityManager().GetEntityById<BaseEntity>(result.CasterId);
            if (entity == null)
                return;

            if (entity.GetCurrSkillId() < 0 || entity.GetCurrSkillId() != (int)result.SkillId) //小于0 则不再释放中
                return;

            BroadCastSkill_ChangeTargets bctarges = new BroadCastSkill_ChangeTargets();
            uint skillid = result.SkillId;
            bctarges.skillId = (int)skillid;
            bctarges.targets = result.TargetList;
            bctarges.direction = result.TargetPosition;

            Leyoutech.Utility.DebugUtility.LogWarning("广播技能", string.Format("引导技能切换目标 SkillId = {0}", skillid));

            entity.SendEvent(ComponentEventName.BroadCastSkill_ChangeTargets, bctarges);
        }

        /// <summary>
        /// 添加buf
        /// </summary>
        private static void DisposeAddBuf(AddBuf result)
        {
            BaseEntity entity = GameplayManager.Instance.GetEntityManager().GetEntityById<BaseEntity>(result.HeroId);
            if (entity == null)
                return;

            Leyoutech.Utility.DebugUtility.LogWarning("广播", string.Format("添加buf  EntitId = {0} , BuffId = {1}", result.HeroId , result.BufId));


            AddBuffEvent buffEvent = new AddBuffEvent();
            buffEvent.buff = new BuffVO(result.BufId, (int)result.Overlap, Time.time, result.Time / 1000.0f, result.LinkId, result.IsMaster);
            entity.SendEvent(ComponentEventName.BuffAdd, buffEvent);

            MSAIBossProxy mSAI = GameFacade.Instance.RetrieveProxy(ProxyName.MSAIBossProxy) as MSAIBossProxy;
            mSAI.CheckBuffPlaySound(result.BufId, true);
        }

        /// <summary>
        /// 删除buf
        /// </summary>
        private static void DisposeDeleteBuf(DeleteBuf result)
        {
            BaseEntity entity = GameplayManager.Instance.GetEntityManager().GetEntityById<BaseEntity>(result.HeroId);
            if (entity == null)
                return;

            Leyoutech.Utility.DebugUtility.LogWarning("广播", string.Format("删除buf  EntitId = {0}, BuffId = {1}", result.HeroId, result.BufId));


            RemoveBuffEvent buffEvent = new RemoveBuffEvent();
            buffEvent.buffID = result.BufId;
            entity.SendEvent(ComponentEventName.BuffRemove, buffEvent);

            MSAIBossProxy mSAI = GameFacade.Instance.RetrieveProxy(ProxyName.MSAIBossProxy) as MSAIBossProxy;
            mSAI.CheckBuffPlaySound(result.BufId, false);
        }


        /// <summary>
        /// 弹夹信息,只有主角才有
        /// </summary>
        /// <param name="result"></param>
        private static void DisposeSyncClipInfo(SyncClipInfo result)
        {
            //Leyoutech.Utility.DebugUtility.LogWarning("广播", string.Format("弹夹信息, 武器ID = {0}", result.CurWeaponUid));
            GameplayProxy m_GameplayProxy = GameFacade.Instance.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;
            SpacecraftEntity entity = m_GameplayProxy.GetEntityById<SpacecraftEntity>(m_GameplayProxy.GetMainPlayerUID());
            if (entity == null)
                return;

            PlayerSkillProxy m_PlayerSkillProxy = GameFacade.Instance.RetrieveProxy(ProxyName.PlayerSkillProxy) as PlayerSkillProxy;
            m_PlayerSkillProxy.ChangeCurrentWeaponByServer(result.CurWeaponUid);

            foreach (WeaponValue info in result.Infos)
            {
                ulong weaponUID = info.WeaponUid;

                WeaponPowerVO power = entity.GetWeaponPower(weaponUID);
                if (power == null)
                {
                    entity.SetWeaponPower(weaponUID, new WeaponPowerVO());
                    power = entity.GetWeaponPower(weaponUID);
                }

                power.WeaponUID = info.WeaponUid;
                power.CurrentValue = info.CurValue;
                power.MaxValue = info.MaxValue;
                power.SafeValue = info.SaftyValue;

                IWeapon weapon = m_PlayerSkillProxy.GetWeaponByUID(power.WeaponUID);
                if (weapon != null && weapon.GetConfig().ClipType != (int)WeaponL1.Treasure)
                {
                    if (power.CurrentValue <= 0)
                        power.ForceCooldown = true;
                    else if (power.CurrentValue >= power.SafeValue)
                        power.ForceCooldown = false;
                }
                else
                {
                    if (power.CurrentValue >= power.MaxValue)
                        power.ForceCooldown = true;
                    else if (power.CurrentValue <= power.SafeValue)
                        power.ForceCooldown = false;
                }
            }

            GameFacade.Instance.SendNotification(NotificationName.MSG_CHARACTER_WEAPON_POWER_CHANGED);
            entity.SendEvent(ComponentEventName.WeaponPowerChanged, null);
        }
    }
}