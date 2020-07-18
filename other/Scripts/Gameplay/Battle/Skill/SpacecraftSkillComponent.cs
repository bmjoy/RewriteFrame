using Assets.Scripts.Define;
using Crucis.Protocol;
using Eternity.FlatBuffer;
using Eternity.FlatBuffer.Enums;
using Gameplay.Battle.Emit;
using Gameplay.Battle.Skill.Tokens;
using Gameplay.Battle.Timeline;
using Leyoutech.AI.FSM;
using Leyoutech.Core.Context;
using Leyoutech.Core.Timeline;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Battle.Skill
{
    public interface ISpacecraftSkillProperty : IBaseActionProperty, IBindNodeActionProperty
    {
        void SetFireCountdown(float countdown);
    }

    public class SpacecraftSkillComponent : EntityComponent<ISpacecraftSkillProperty>
    {
        /// <summary>
        /// 静态总表
        /// </summary>
        private CfgEternityProxy m_CfgEternityProxy = null;

        /// <summary>
        /// 技能运行时代理数据
        /// </summary>
        private PlayerSkillProxy m_PlayerSkillProxy;

        /// <summary>
        /// 上下文
        /// </summary>
        private DefaultContext m_Context = null;
        private ISpacecraftSkillProperty m_ISpacecraftSkillProperty;

        // 当前正在释放的技能ID
        private int m_currSkillId = -1;

        //是否处理过按键抬起了，根据不同技能类型，具体定义设置它
        private bool m_IsProcessedUp = true;

        private bool IsWeaponSkill = false;

        #region  Trigger 技能 临时变量
        private int m_TriggerSkillId = -1;
        private bool IsTrigger = false;
        private float m_TriggerLogicTime = 1f;
        private float m_TriggerStartTime = 0f;
        #endregion

        /// <summary>
        /// 当前正在释放的技能ID
        /// </summary>
        public int SkillID
        {
            get { return m_currSkillId; }
            set
            {
                m_currSkillId = value;
                if (m_ISpacecraftSkillProperty != null)
                    m_ISpacecraftSkillProperty.SetCurrSkillId(m_currSkillId);
            }
        }

        private SkillRunningData m_RunningData = null;
        private SkillStateMachine m_StateMachine = null;
        private SkillStageType m_StageType = SkillStageType.None;
        public SkillStageType StageType
        {
            get
            {
                return m_StageType;
            }

            set
            {
                if (m_StageType != value)
                {
                    m_StageType = value;
                    if (m_StateMachine.IsRunning)
                    {
                        if (m_StageType == SkillStageType.None)
                        {
                            m_StateMachine.PerformAction(SkillActionToken.FinalizeAction, m_StageType);
                        }
                        else
                        {
                            m_StateMachine.PerformAction(SkillActionToken.StageChangedAction, m_StageType);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 改变当前技能状态到对应的break
        /// </summary>
        /// <returns></returns>
        private SkillStageType ChangedSkillStageToBreak()
        {
            if (StageType == SkillStageType.Begin)
                return SkillStageType.BreakBegin;

            if (StageType == SkillStageType.Release)
                return SkillStageType.BreakRelease;

            if (StageType == SkillStageType.End)
                return SkillStageType.BreakEnd;

            return SkillStageType.None;
        }



        public override void OnInitialize(ISpacecraftSkillProperty property)
        {
            m_ISpacecraftSkillProperty = property;
            m_CfgEternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
            m_PlayerSkillProxy = GameFacade.Instance.RetrieveProxy(ProxyName.PlayerSkillProxy) as PlayerSkillProxy;

            m_Context = new DefaultContext();
            m_RunningData = new SkillRunningData();

            m_Context.AddObject(this, true);
            m_Context.AddObject(typeof(IActionFactory), TimelineActionFactory.Factory, true);
            m_Context.AddObject(m_RunningData, true);
            m_Context.AddObject(typeof(IBaseActionProperty), property, true);
            m_Context.AddObject(typeof(IBindNodeActionProperty), property, true);

            ContextAddOtherObject();

            m_StateMachine = new SkillStateMachine(m_Context);
            m_StateMachine.CompletedHandler += OnStateMachineComplete;
            m_StateMachine.ChangedHandler += OnStateMachineChanged;
        }

        /// <summary>
        /// 添加监听
        /// </summary>
        public override void OnAddListener()
        {
            AddListener(ComponentEventName.Dead, EntityDead);                                                                                                                           //单位死亡
            AddListener(ComponentEventName.Relive, EntityRelive);                                                                                                                        //单位复活
            AddListener(ComponentEventName.SkillButtonResponse, OnHotkey);                                                                                                  //热键相应
            AddListener(ComponentEventName.BroadCastSkill_ReleaseSkill, OnS2C_ReleaseSkill);                                                                     //服务器信息释放技能相应
            AddListener(ComponentEventName.CaseSkillResult, OnCaseSkillResult);                                                                                             //服务器预演释放验证结果
            AddListener(ComponentEventName.AccumulationIndex, OnAccumulationIndexResult);                                                                  //服务器蓄力索引结果
            AddListener(ComponentEventName.RandomEmitNode, OnRandomEmitNodeResult);                                                                      //随机发射点
            AddListener(ComponentEventName.ToEndSkill, OnEndSkillResult);                                                                                                        //结束技能
            AddListener(ComponentEventName.ToStopSkill, OnStopSkillResult);                                                                                                      //停止技能
            AddListener(ComponentEventName.CoerceSkillButtonUp, OnCoerceSkillButtonUp);                                                                         //强制技能按键抬起

        }

        /// <summary>
        /// 是否可以释放
        /// </summary>
        /// <param name="skillId"></param>
        /// <returns></returns>
        public bool CanReleaseSkill(int skillId)
        {
            if (m_PlayerSkillProxy.IsInCD(skillId))
            {
                return false;
            }
            return !m_StateMachine.IsRunning;
        }

        /// <summary>
        /// 释放技能
        /// </summary>
        /// <param name="skillId"></param>
        public void ReleaseSkill(int skillid)
        {
            SkillID = skillid;
            IsWeaponSkill = IsWeapon(SkillID);

            SkillData skillData = m_CfgEternityProxy.GetSkillData(SkillID);
            m_Context.AddObject<SkillData>(skillData);

            Leyoutech.Utility.DebugUtility.LogWarning("技能", string.Format("OnStateMachineComplete->执行释放技能  ------>  Entity = {0} , 主角 = {1},技能类型 = {2} ,{3}, 技能ID ={4}, IsTrigger = {5} ",
                m_ISpacecraftSkillProperty.EntityId(),
                m_ISpacecraftSkillProperty.IsMain(),
                skillData.BaseData.Value.CategoryType,
                IsWeaponSkill ? "武器技能" : "飞船技能",
                SkillID,
                skillData.BaseData.Value.IsTrigger
                ));

            PrepareRunningData(SkillID);

            if (m_ISpacecraftSkillProperty.IsMain() )
            {
                SkillRPCNet skillRPC = new SkillRPCNet(RpcCloseAction);
                m_Context.AddObject<SkillRPCNet>(skillRPC);
            }

            m_StageType = SkillStageType.Begin;
            m_StateMachine.SetInitialState(SkillStateToken.Begin);

            //进度条隐藏显示
            ShowProgressBar(true);
        }

        private void PrepareRunningData(int skillID)
        {
            m_RunningData.Reset();

            if (IsWeaponSkill)
            {
                m_RunningData.TimeScaleRate = GetWeaponFireInterval(skillID);
            }
            else
            {
                m_RunningData.TimeScaleRate = 1.0f;
            }
            SkillReleaseStageData releaseStageData = m_CfgEternityProxy.GetSkillReleaseStageData(skillID);
            m_RunningData.ReleaseLoopCount = releaseStageData.LoopCount;
        }

        public override void OnUpdate(float deltaTime)
        {
            if (m_StateMachine.IsRunning)
            {
				float fire_CountDown = m_CfgEternityProxy.GetGamingConfig(1).Value.Sound.Value.CountDown.Value.Fire;
				m_ISpacecraftSkillProperty.SetFireCountdown(fire_CountDown);

                m_StateMachine.DoUpdate(deltaTime);
            }

            //Trigger
            if (m_TriggerSkillId <= 0 || !IsTrigger || !m_ISpacecraftSkillProperty.GetSkillBttonIsDown())
                return;
            if (!CanReleaseSkill(m_TriggerSkillId))
                return;
            if (m_TriggerStartTime + m_TriggerLogicTime > Time.time)
                return;
            m_TriggerStartTime = Time.time;


            Leyoutech.Utility.DebugUtility.LogWarning("IsTrigger技能",string.Format("m_TriggerSkillId = {0}", m_TriggerSkillId));
            if (m_ISpacecraftSkillProperty.IsMain()) //主角
            {
                ReleaseSkill(m_TriggerSkillId);
                C2S_ButtonDown();
            }

        }

        private void OnStateMachineChanged(StateMachine machine, StateChangedEventArgs e)
        {
            StateToken newStateToken = e.NewState.Token;
            if (newStateToken == SkillStateToken.Release)
            {
                //进度条隐藏
                ShowProgressBar(false);
            }
        }

        private void OnStateMachineComplete(StateMachine machine)
        {
            Leyoutech.Utility.DebugUtility.LogWarning("技能", string.Format("OnStateMachineComplete->技能释放完成  ------>  Entity = {0} , 主角 = {1},技能类型 = {2} ,{3}, 技能ID ={4}",
                m_ISpacecraftSkillProperty.EntityId(),
                m_ISpacecraftSkillProperty.IsMain(),
                m_Context.GetObject<SkillData>().BaseData.Value.CategoryType,
                IsWeaponSkill?"武器技能":"飞船技能",
                SkillID
                ));




            SkillRPCNet skillRPC = m_Context.GetObject<SkillRPCNet>();
            if (skillRPC != null)
                skillRPC.Close();
            skillRPC = null;
            m_Context.DeleteObject<SkillRPCNet>();
            m_Context.Clear();
            m_RunningData.Reset();
            if (IsWeaponSkill)
                SendEvent(ComponentEventName.WeaponSkillFinish, new WeaponSkillFinish() { IsMain = m_ISpacecraftSkillProperty.IsMain(), skillId = SkillID });
            SkillID = -1;
            IsWeaponSkill = false;
            StageType = SkillStageType.None;
            m_ISpacecraftSkillProperty.GetPerceptronTarget().ClearSkilltargetEntitys();
        }

        /// <summary>
        /// 取消技能
        /// </summary>
        private void DoCancelSkill()
        {
            IsTrigger = false;
            if (m_StateMachine.IsRunning)
            {
                m_StateMachine.PerformAction(SkillActionToken.FinalizeAction);
                Leyoutech.Utility.DebugUtility.LogWarning("技能", "技能被取消");
            }
        }

        /// <summary>
        /// 恢复技能
        /// </summary>
        private void DoResumeSkill()
        {
            if (m_StateMachine.IsRunning)
            {
                m_StateMachine.PerformAction(SkillActionToken.ResumeTimeline);
            }
        }

        /// <summary>
        /// 暂停技能
        /// </summary>
        private void DoPausedSkill()
        {
            if (m_StateMachine.IsRunning)
            {
                m_StateMachine.PerformAction(SkillActionToken.PausedTimeline);
            }
        }

        /// <summary>
        /// 角色Entiy 被销毁时
        /// </summary>
        public override void OnDestroy()
        {
            base.OnDestroy();
            m_ISpacecraftSkillProperty.SetChangeGuideSkillTargetAction(null);
            m_ISpacecraftSkillProperty.SetSkillBttonIsDown(false);
            SkillRPCNet skillRPC = m_Context.GetObject<SkillRPCNet>();
            if (skillRPC != null)
                skillRPC.Close();
            skillRPC = null;

            m_Context.Clear(true);
            m_RunningData = null;
            m_CfgEternityProxy = null;
            m_Context = null;
            m_StateMachine = null;
            m_ISpacecraftSkillProperty = null;
        }

        /// <summary>
        /// 其他上下文或者模块需要携带的数据
        /// </summary>
        private void ContextAddOtherObject()
        {
            //感知目标信息
            m_Context.AddObject<PerceptronTarget>(m_ISpacecraftSkillProperty.GetPerceptronTarget(), true);

            //摄像机信息
            MainCameraComponent mainCamComponent = CameraManager.GetInstance().GetMainCamereComponent();
            m_Context.AddObject<MainCameraComponent>(mainCamComponent, true);

            if (m_ISpacecraftSkillProperty.IsMain())
            {
                //设置引导技能目标变化函数变化委托
                m_ISpacecraftSkillProperty.SetChangeGuideSkillTargetAction(ChangeGuideSkillTargetAction);
            }
        }



        #region -----------------------------------------    事件监听相应------------------------------------------------

        /// <summary>
        /// 角色死亡
        /// </summary>
        public void EntityDead(IComponentEvent obj)
        {

        }

        /// <summary>
        /// 角色复活
        /// </summary>
        public void EntityRelive(IComponentEvent obj)
        {

        }

        /// <summary>
        /// 技能按键相应
        /// </summary>
        /// <param name="obj"></param>
        private void OnHotkey(IComponentEvent obj)
        {
            //战斗状态||过载模式 才响应
            if (m_ISpacecraftSkillProperty.GetCurrentState().GetMainState() == EnumMainState.Fight
                || m_ISpacecraftSkillProperty.GetCurrentState().IsHasSubState(EnumSubState.Overload))
            {
                SkillCastEvent skillCastEvent = obj as SkillCastEvent;

                if (m_ISpacecraftSkillProperty.GetCurrentState().IsHasSubState(EnumSubState.Peerless) && !skillCastEvent.IsWeaponSkill) //转化炉模式只允许释放武器技能
                     return;

                int skillId = 0;
                if (skillCastEvent.IsWeaponSkill)
                {
                    skillId = skillCastEvent.SkillIndex; //非武器 表示第几个技能，武器则表示技能ID
                }
                else
                {
                    PlayerShipSkillVO skillVO = null;
                    skillVO = m_PlayerSkillProxy.GetShipSkillByIndex(skillCastEvent.SkillIndex); //由第几个技能，获取技能VO
                    if (skillVO == null)
                        return;

                    skillId = skillVO.GetID();
                }

                Leyoutech.Utility.DebugUtility.Log("技能按键响应", string.Format("技能 Id = {0} , 按下 = {1}", skillId, skillCastEvent.KeyPressed));

                //按下
                if (skillCastEvent.KeyPressed)
                {
                    //是否可释放
                    if (!CanReleaseSkill(skillId))
                    {
                        Leyoutech.Utility.DebugUtility.Log("技能按键响应", string.Format("技能 Id = {0} ,  不可释放", skillId));
                        return;
                    }

					float fire_CountDown = m_CfgEternityProxy.GetGamingConfig(1).Value.Sound.Value.CountDown.Value.Fire;
					m_ISpacecraftSkillProperty.SetFireCountdown(fire_CountDown);

                    ////执行预演播放，并请求服务器验证，如果失败则打断释放
                    ////执行释放
                    ReleaseSkill( skillId);

                    if (m_ISpacecraftSkillProperty.IsMain()) //主角
                    {
                        C2S_ButtonDown();
                    }

                    // Trigger 重复类技能记录
                    m_TriggerStartTime = Time.time;
                    m_TriggerSkillId = skillId;
                    IsTrigger = m_CfgEternityProxy.GetSkillData(skillId).BaseData.Value.IsTrigger;
                    m_TriggerLogicTime = m_CfgEternityProxy.GetSkillData(skillId).BaseData.Value.Triggertime;
                    m_ISpacecraftSkillProperty.SetSkillBttonIsDown(true);
                }
                else //抬起
                {
                    if (m_ISpacecraftSkillProperty.IsMain()) //主角
                    {
                        if (m_StateMachine.IsRunning && skillId == SkillID)
                            C2S_ButtonUp();
                    }

                    m_TriggerSkillId = -1;
                    IsTrigger = false;
                    m_ISpacecraftSkillProperty.SetSkillBttonIsDown(false);
                }
            }
        }


        /// <summary>
        /// 服务器告知播放技能
        /// </summary>
        public void OnS2C_ReleaseSkill(IComponentEvent Event)
        {
            BroadCastSkill_ReleaseSkill messag = Event as BroadCastSkill_ReleaseSkill;
            ReleaseSkill(messag.skillId);
        }

        /// <summary>
        /// 取消释放技能
        /// </summary>
        /// <param name="obj"></param>
        private void OnCaseSkillResult(IComponentEvent obj)
        {
            CaseSkillResult result = obj as CaseSkillResult;
            if (result.skillId != SkillID)
                return;
            if (!result.succeed)
            {
                DoCancelSkill();
            }
        }

        /// <summary>
        /// 服务器蓄力索引结果
        /// </summary>
        /// <param name="obj"></param>
        public void OnAccumulationIndexResult(IComponentEvent obj)
        {
            AccumulationIndexResult result = obj as AccumulationIndexResult;
            if (result == null)
                return;

            if (result.skillId != SkillID)
                return;

            SkillData skillData = m_Context.GetObject<SkillData>();
            string lpc = string.Empty;
            if (skillData.ReleaseStageData.Value.IsDynamicLoopCount)
            {
                m_RunningData.ReleaseLoopCount = m_ISpacecraftSkillProperty.GetPerceptronTarget().GetCurrentTargetsCount();
                lpc = string.Format("动态loopcout = {0}", m_RunningData.ReleaseLoopCount);
            }

            Leyoutech.Utility.DebugUtility.Log("蓄力", " 蓄力结果  index = " + result.accumulationIndex + lpc);

            m_RunningData.ReleaseStageIndex = result.accumulationIndex;
            StageType = SkillStageType.Release;
        }


        /// <summary>
        /// 随机发射点索引结果
        /// </summary>
        /// <param name="obj"></param>
        public void OnRandomEmitNodeResult(IComponentEvent obj)
        {
            RandomEmitNodeResult result = obj as RandomEmitNodeResult;
            if (result == null)
                return;

            EmitSelectionData emitSelectionData = m_Context.GetObject<SkillRunningData>().EmitSelectoin;//发射口选择数据

            EmitData[] emitDatas = new EmitData[result.emitNode.IndexList.Count];
            for(int i =0;i<result.emitNode.IndexList.Count;++i)
            {
                emitDatas[i] = new EmitData()
                {
                    NodeType = (BindNodeType)result.emitNode.NodeType,
                    NodeIndex = result.emitNode.IndexList[i],
                };
            }
            emitSelectionData.AddOrUpdateEmit(result.emitNode.AssignIndex, emitDatas);

            //恢复时间轴
            DoResumeSkill();
        }

        /// <summary>
        /// 结束技能，切入end 阶段
        /// </summary>
        /// <param name="obj"></param>
        private void OnEndSkillResult(IComponentEvent obj)
        {
            EndSkillResult result = obj as EndSkillResult;
            if (result == null)
                return;

            if (SkillID != result.skillId)
                return;

            Leyoutech.Utility.DebugUtility.LogWarning("技能", string.Format("停止技能，由 {0} 状态切入End ! ", StageType));
            StageType = SkillStageType.End;
        }

        /// <summary>
        /// 结束技能，切入break阶段
        /// </summary>
        /// <param name="obj"></param>
        private void OnStopSkillResult(IComponentEvent obj)
        {
            StopSkillResult result = obj as StopSkillResult;
            if (result == null)
                return;

            if (SkillID != result.skillId)
                return;

            Leyoutech.Utility.DebugUtility.LogWarning("技能", string.Format("停止技能，由 {0} 状态切入Break ! ", StageType));
            StageType = ChangedSkillStageToBreak();
        }

        /// <summary>
        /// 去通知UI，显示 进度条
        /// </summary>
        /// <param name="isShow">显示/隐藏</param>
        /// <param name="styleIndex">样式索引</param>
        /// <param name="holdtime">维持时间</param>
        private void ToNoticeShowTimesProgressBar(bool isShow, int styleIndex = 0, float holdtime = 0)
        {
            if (isShow)
            {
                MsgShowSkillTimeProgressBar msg = new MsgShowSkillTimeProgressBar();
                msg.StyleIndex = styleIndex;
                msg.Duration = holdtime;
                GameFacade.Instance.SendNotification(NotificationName.ShowSkillProgressBar, msg);
            }
            else
            {
                MsgHideSkillTimeProgressBar msg = new MsgHideSkillTimeProgressBar();
                msg.StyleIndex = styleIndex;
                GameFacade.Instance.SendNotification(NotificationName.HidSkillProgressBar, msg);
            }
        }

        /// <summary>
        /// 强制技能按键抬起
        /// </summary>
        private void OnCoerceSkillButtonUp(IComponentEvent obj)
        {
            Leyoutech.Utility.DebugUtility.Log("技能", "强制技能按键抬起 ");

            if (m_ISpacecraftSkillProperty.IsMain()) //主角
            {
                C2S_ButtonUp();
            }
        }



        #endregion -------------------------------end----------事件监听相应------------------------------------------------






        #region ------------------------------------------------------ 通信----------------------------------------------------------


        /// <summary>
        /// 客户端---> 服务器
        /// </summary>
        /// <param name="skillId"></param>
        private void C2S_ButtonDown()
        {
            SkillData skillData = m_Context.GetObject<SkillData>();
            if (skillData.ByteBuffer == null)
                return;


            if (skillData.BaseData.Value.CategoryType == Eternity.FlatBuffer.Enums.SkillCategoryType.Sing) // 吟唱，前摇---- > begin,release,end 自动切换
            {
                ToCastSingSkill(skillData);
            }
            else if (skillData.BaseData.Value.CategoryType == Eternity.FlatBuffer.Enums.SkillCategoryType.Accumulation) //蓄力---->按下begin,抬起通信release,end
            {
                m_IsProcessedUp = false;
                ToCastAccumulationSkill(skillData, true);
            }
            else if (skillData.BaseData.Value.CategoryType == Eternity.FlatBuffer.Enums.SkillCategoryType.Guide) //引导  ----->按下begin,release,抬起通信end
            {
                m_IsProcessedUp = false;
                ToCastGuideSkill(skillData, 1);
            }
        }

        /// <summary>
        /// 客户端---> 服务器
        /// </summary>
        /// <param name="skillId"></param>

        private void C2S_ButtonUp()
        {
            SkillData skillData = m_Context.GetObject<SkillData>();
            if (skillData.ByteBuffer == null)
                return;

            if (skillData.BaseData.Value.CategoryType == Eternity.FlatBuffer.Enums.SkillCategoryType.Sing) // 吟唱，前摇---- > begin,release,end 自动切换
            {

            }
            else if (skillData.BaseData.Value.CategoryType == Eternity.FlatBuffer.Enums.SkillCategoryType.Accumulation) //蓄力---->按下begin,抬起通信release,end
            {
                if (m_IsProcessedUp)        //处理过抬起了，按下，抬起需要只处理一次，如无前置条件，在释放完毕前，不接受新输入
                    return;
                m_IsProcessedUp = true;
                ToCastAccumulationSkill(skillData, false);
            }
            else if (skillData.BaseData.Value.CategoryType == Eternity.FlatBuffer.Enums.SkillCategoryType.Guide) //引导  ----->按下begin,release,抬起通信end
            {
                if (m_IsProcessedUp)
                    return;
                m_IsProcessedUp = true;
                ToCastGuideSkill(skillData, 3);
            }
        }


        /// <summary>
        ///吟唱，前摇 ，验证目标，技能是否可继续释放
        /// </summary>
        private void ToCastSingSkill(SkillData skillData)
        {
            MainCameraComponent mainCamComponent = m_Context.GetObject<MainCameraComponent>();
            float d = skillData.BaseData.Value.MaxDistance;//* SceneController.SKILL_PRECISION;

            Vector3 direction = Vector3.zero;
            List<Crucis.Protocol.TargetInfo> targetInfos = m_ISpacecraftSkillProperty.GetPerceptronTarget().GetCurrentTargetInfos(true, out direction, true, skillData.BaseData.Value.TagercalculationType);
            SkillRPCNet skillRPC = m_Context.GetObject<SkillRPCNet>();
            if (skillRPC == null)
                return;

            Crucis.Protocol.Vec4 vec4 = GetCameraQuaternion(mainCamComponent, direction, d);

            skillRPC.CreatListenerCastSkill((uint)m_ISpacecraftSkillProperty.EntityId(), (uint)skillData.Id, targetInfos, vec4);
        }


        /// <summary>
        /// 蓄力---->按下begin,抬起通信release,end
        /// </summary>
        /// <param name="skillData"></param>
        private void ToCastAccumulationSkill(SkillData skillData, bool isdown)
        {
            uint entityId = (uint)m_ISpacecraftSkillProperty.EntityId();
            uint skillId = (uint)skillData.Id;
            SkillRPCNet skillRPC = m_Context.GetObject<SkillRPCNet>();
            if (skillRPC == null)
                return;

            if (isdown) //按下
            {
                Leyoutech.Utility.DebugUtility.Log("蓄力", " 按下");

                AccumulationSkill skillA = new AccumulationSkill();
                skillA.AccumulationSkillBegin = new global::Crucis.Protocol.AccumulationSkillBegin();
                skillA.AccumulationSkillBegin.CasterId = entityId;
                skillA.AccumulationSkillBegin.SkillId = skillId;
                skillRPC.CreatListenerCastAccumulationSkill(entityId, skillId);
                skillRPC.CastAccumulationSkillWright(entityId, skillId, skillA);
            }
            else //抬起
            {
                Leyoutech.Utility.DebugUtility.Log("蓄力", " 抬起");
                Vector3 direction = Vector3.zero;
                List<Crucis.Protocol.TargetInfo> targetList = m_ISpacecraftSkillProperty.GetPerceptronTarget().GetCurrentTargetInfos(false, out direction, true, skillData.BaseData.Value.TagercalculationType);
                AccumulationSkill skillA = new AccumulationSkill();
                skillA.AccumulationSkillEnd = new global::Crucis.Protocol.SkillSelectTargetInfo();
                skillA.AccumulationSkillEnd.CasterId = (uint)m_ISpacecraftSkillProperty.EntityId();
                skillA.AccumulationSkillEnd.SkillId = (uint)skillData.Id;
                skillA.AccumulationSkillEnd.TargetList.Add(targetList);
                
                MainCameraComponent mainCamComponent = m_Context.GetObject<MainCameraComponent>();
                float d = skillData.BaseData.Value.MaxDistance; //* SceneController.SKILL_PRECISION;
                Crucis.Protocol.Vec4 vec4 = GetCameraQuaternion(mainCamComponent, direction, d);
                skillA.AccumulationSkillEnd.Quat = vec4;

                skillRPC.CastAccumulationSkillWright(entityId, skillId, skillA);

                if (skillData.BaseData.Value.IsNeedTarget && (targetList == null || targetList.Count == 0))
                {
                    StageType = SkillStageType.None;
                }
            }
        }

        /// <summary>
        /// 释放引导技能 guidState : 1 按下，2 改变 ，3 抬起
        /// </summary>
        private void ToCastGuideSkill(SkillData skillData, int guidState)
        {
            SkillRPCNet skillRPC = m_Context.GetObject<SkillRPCNet>();

            if (skillData.ByteBuffer == null || skillRPC == null)
                return;

            uint entityId = (uint)m_ISpacecraftSkillProperty.EntityId();
            uint skillId = (uint)skillData.Id;

            switch (guidState)
            {
                case 1: //按下
                    {
                        Leyoutech.Utility.DebugUtility.Log("引导技能", " 按下");


                        GuideSkill guideSkill = new GuideSkill();
                        guideSkill.GuideSkill_ = new global::Crucis.Protocol.SkillSelectTargetInfo();
                        guideSkill.GuideSkill_.CasterId = (uint)m_ISpacecraftSkillProperty.EntityId();
                        guideSkill.GuideSkill_.SkillId = (uint)skillData.Id;
                        Vector3 direction = Vector3.zero;
                        guideSkill.GuideSkill_.TargetList.Add(m_ISpacecraftSkillProperty.GetPerceptronTarget().GetCurrentTargetInfos(false, out direction, true, skillData.BaseData.Value.TagercalculationType));
                        if (guideSkill.GuideSkill_.TargetList.Count == 0)
                            Leyoutech.Utility.DebugUtility.Log(" 引导技能", "------%%%%%%%----按下 ：Null ");

                        for (int i = 0; i < guideSkill.GuideSkill_.TargetList.Count; i++)
                        {
                            Leyoutech.Utility.DebugUtility.Log(" 引导技能", "------%%%%%%%----按下 ：" + guideSkill.GuideSkill_.TargetList[i].TargetId);
                        }


                        MainCameraComponent mainCamComponent = m_Context.GetObject<MainCameraComponent>();
                        float d = skillData.BaseData.Value.MaxDistance;// * SceneController.SKILL_PRECISION;
                        Crucis.Protocol.Vec4 vec4 = GetCameraQuaternion(mainCamComponent, direction, d);
                        guideSkill.GuideSkill_.Quat = vec4;
                        skillRPC.CreatListenerCastGuideSkill(entityId, skillId);
                        skillRPC.CastGuideSkillWright(entityId, skillId, guideSkill);
                    }
                    break;
                case 2://目标改变
                    {
                        Leyoutech.Utility.DebugUtility.Log(" 引导技能", "目标改变");

                        GuideSkill guideSkill = new GuideSkill();
                        guideSkill.ChangeTarget = new global::Crucis.Protocol.ChangeGuideSkillTargetInfo();
                        guideSkill.ChangeTarget.CasterId = (uint)m_ISpacecraftSkillProperty.EntityId();
                        guideSkill.ChangeTarget.SkillId = (uint)skillData.Id;
                        Vector3 direction = Vector3.zero;
                        guideSkill.ChangeTarget.TargetList.Add(m_ISpacecraftSkillProperty.GetPerceptronTarget().GetCurrentTargetInfos(false, out direction, false));

                        if (guideSkill.ChangeTarget.TargetList.Count == 0)
                            Leyoutech.Utility.DebugUtility.Log(" 引导技能", "------%%%%%%%----目标改变 ：Null ");

                        for (int i = 0; i < guideSkill.ChangeTarget.TargetList.Count; i++)
                        {
                            Leyoutech.Utility.DebugUtility.Log(" 引导技能", "------%%%%%%%----目标改变 ：" + guideSkill.ChangeTarget.TargetList[i].TargetId);
                        }


                        MainCameraComponent mainCamComponent = m_Context.GetObject<MainCameraComponent>();
                        float d = skillData.BaseData.Value.MaxDistance;// * SceneController.SKILL_PRECISION;
                        Crucis.Protocol.Vec4 vec4 = GetCameraQuaternion(mainCamComponent, direction, d);
                        guideSkill.ChangeTarget.Quat = vec4;
                        skillRPC.CastGuideSkillWright(entityId, skillId, guideSkill);
                    }
                    break;
                case 3://抬起
                    {
                        Leyoutech.Utility.DebugUtility.Log("引导技能", " 抬起");

                        GuideSkill guideSkill = new GuideSkill();
                        guideSkill.GuideEnd = new global::Crucis.Protocol.GuideSkillEnd();
                        guideSkill.GuideEnd.CasterId = (uint)m_ISpacecraftSkillProperty.EntityId();
                        guideSkill.GuideEnd.SkillId = (uint)skillData.Id;
                        skillRPC.CastGuideSkillWright(entityId, skillId, guideSkill);
                    }
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 获取当前主摄像机注视，所选技能最远处的，四元数旋转值
        /// </summary>
        /// <param name="mainCamComponent">主摄像机组件</param>
        /// <param name="d">最远距离</param>
        /// <returns></returns>
        private Crucis.Protocol.Vec4 GetCameraQuaternion(MainCameraComponent mainCamComponent, Vector3 direction, float d)
        {
            if (mainCamComponent == null)
            {
                Leyoutech.Utility.DebugUtility.LogError("目标方向", " 上下文没有摄像机数据");
                return null;
            }

            //最远点
            Vector3 distantPoint = Vector3.zero;
            Vector3 playerPosition = m_ISpacecraftSkillProperty.GetRootTransform().position;   //玩家位置
            Vector3 CamPosition = mainCamComponent.GetPosition(); //摄像机位置

            Vector3 cameDir = direction;/*mainCamComponent.GetForward()*/   //摄像机方向向量
            Vector3 camera2Player = playerPosition - CamPosition;//摄像机到玩家的向量
            Vector3 verticalPos = CamPosition + Vector3.Dot(camera2Player, cameDir) * cameDir; //垂线坐标 = 摄像机坐标+ camera2Player在cameDir投影距离 * cameDir向量
            Vector3 Play2VerticaN = (verticalPos - playerPosition).normalized; //玩家到垂线点 的单位向量
            float Play2VerticaD = Vector3.Distance(verticalPos, playerPosition);//玩家跟垂线点的距离

            float MaxDis = d;
            if (MaxDis > Play2VerticaD)
            {
                distantPoint = Mathf.Sqrt(MaxDis * MaxDis - Play2VerticaD * Play2VerticaD) * cameDir + verticalPos; //最远点 = 三角函数求得垂线点到最远点向量+ 垂线点坐标
            }
            else
            {
                distantPoint = playerPosition + Play2VerticaN * MaxDis;//垂线上找到距离是 MaxDis 的坐标
            }


            Vector3 Spheredirection = (distantPoint - m_ISpacecraftSkillProperty.GetRootTransform().position).normalized;       //船到最远处单位方向向量
            Quaternion cameraQuaternion = Quaternion.Euler(Quaternion.FromToRotation(Vector3.forward, Spheredirection).eulerAngles); //欧拉角-->四元数


            Crucis.Protocol.Vec4 vec4 = new Crucis.Protocol.Vec4();
            vec4.X = cameraQuaternion.x;
            vec4.Y = cameraQuaternion.y;
            vec4.Z = cameraQuaternion.z;
            vec4.W = cameraQuaternion.w;

            Vector3 tt = playerPosition + cameraQuaternion * Vector3.forward * d;
            Leyoutech.Utility.DebugUtility.LogWarning("目标方向", string.Format(" 摄像机注视方向射程最远点计算-----> 玩家自己entity = {0}, 玩家坐标：{1} , 计算得到方向 欧拉角 : {2} ， 最大射程点坐标：{3}",
                m_ISpacecraftSkillProperty.EntityId(),
                playerPosition,
                cameraQuaternion,
                distantPoint));

            return vec4;
        }

        /// <summary>
        /// 引导技能目标改变通信函数委托
        /// </summary>
        private void ChangeGuideSkillTargetAction()
        {
            SkillData skillData = m_Context.GetObject<SkillData>();
            ToCastGuideSkill(skillData, 2);
        }

        /// <summary>
        /// 进度条显示与否
        /// </summary>
        private void ShowProgressBar(bool show )
        {
            SkillData skillData = m_Context.GetObject<SkillData>();
            if (!m_ISpacecraftSkillProperty.IsMain() || !skillData.BeginStageData.Value.ProgressBarData.Value.IsShow)
                return;

            int ProgressBarStyleIndex = 0;
            float ProgressBarHoldTime = 0;

             if (show)
            {
                //进度条隐藏显示
                ProgressBarStyleIndex = skillData.BeginStageData.Value.ProgressBarData.Value.StyleIndex;
                ProgressBarHoldTime = skillData.BeginStageData.Value.Group.Value.Length;
                ToNoticeShowTimesProgressBar(show, ProgressBarStyleIndex, ProgressBarHoldTime);
            }
             else
            {
                //进度条隐藏
                ProgressBarStyleIndex = skillData.BeginStageData.Value.ProgressBarData.Value.StyleIndex;
                ProgressBarHoldTime = 0;
                ToNoticeShowTimesProgressBar(show, ProgressBarStyleIndex, ProgressBarHoldTime);
            }
        }

        /// <summary>
        /// Rpc异常中断
        /// </summary>
        public void RpcCloseAction()
        {
            if (StageType != SkillStageType.None)
                StageType = SkillStageType.None;
        }



        #endregion -------------------------------end----------通信----------------------------------------------------------


        #region  --------------------------------------------------同步----------------------------------------------------------
        /// <summary>
        /// 是否是武器技能
        /// </summary>
        /// <returns></returns>
        private bool IsWeapon(int skllId)
        {
            ShipItemsProxy shipItemsProxy = GameFacade.Instance.RetrieveProxy(ProxyName.ShipItemsProxy) as ShipItemsProxy;
            IShip ship = shipItemsProxy.GetCurrentWarShip(m_ISpacecraftSkillProperty.EntityId());
            if (ship != null)
            {
                IWeapon[] weapons = ship.GetWeaponContainer().GetWeapons();
                for (int i = 0; i < weapons.Length; i++)
                {
                    if (weapons[i].GetBaseConfig().SkillId == (ulong)skllId)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 获取武器射速by SkillID
        /// </summary>
        /// <param name="skllId"></param>
        /// <returns></returns>
        private float GetWeaponFireInterval(int skllId)
        {
            if(m_ISpacecraftSkillProperty.GetHeroType() == KHeroType.htPlayer)
            {
                ShipItemsProxy shipItemsProxy = GameFacade.Instance.RetrieveProxy(ProxyName.ShipItemsProxy) as ShipItemsProxy;
                IShip ship = shipItemsProxy.GetCurrentWarShip(m_ISpacecraftSkillProperty.EntityId());
                if (ship != null)
                {
                    IWeapon[] weapons = ship.GetWeaponContainer().GetWeapons();
                    for (int i = 0; i < weapons.Length; i++)
                    {
                        if (weapons[i].GetBaseConfig().SkillId == (ulong)skllId)
                        {
                            return WeaponAndCrossSightFactory.CalculateFireInterval((SpacecraftEntity)m_ISpacecraftSkillProperty.GetOwner(),
                                weapons[i].GetConfig().TypeDateSheetld,
                                weapons[i].GetUID(),
                                (WeaponAndCrossSight.WeaponAndCrossSightTypes)weapons[i].GetConfig().Reticle);
                        }
                    }
                }
            }

            return 1.0f;
        }

        #endregion -----------------------------------end---------------同步--------------------------------------------------
    }
}

