using Eternity.FlatBuffer;
using Gameplay.Battle.Flyer;
using Gameplay.Battle.Flyer.Tokens;
using Gameplay.Battle.Timeline;
using Leyoutech.AI.FSM;
using Leyoutech.Core.Context;
using Leyoutech.Core.Timeline;

namespace Gameplay.Battle.Skill
{
    public interface IFlyerProperty : IBaseActionProperty, IMoveActionProperty, IBulletTargetProperty
    {
        /// <summary>
        /// 获取飞行物数据ID
        /// </summary>
        int GetFlyerDataId();
    }

    public class FlyerActionComponent : EntityComponent<IFlyerProperty>
    {
        private CfgEternityProxy m_CfgEternityProxy = null;

        private DefaultContext m_Context = null;
        private IFlyerProperty m_FlyerProprety = null;

        private FlyerStateMachine m_StateMachine = null;
        private FlyerStageType m_StageType = FlyerStageType.None;
        public FlyerStageType StageType
        {
            get
            {
                return m_StageType;
            }
            set
            {
                if(m_StageType != value)
                {
                    m_StageType = value;
                    if (m_StateMachine.IsRunning)
                    {
                        if (m_StageType == FlyerStageType.End)
                        {
                            m_StateMachine.PerformAction(FlyerActionToken.EndAction);
                        }
                        else if (m_StageType == FlyerStageType.None)
                        {
                            m_StateMachine.PerformAction(FlyerActionToken.FinalizeAction);
                        }
                    }
                }
            }
        }

        public override void OnInitialize(IFlyerProperty property)
        {
            m_FlyerProprety = property;
            m_CfgEternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
            m_Context = new DefaultContext();

            m_Context.AddObject(this, true);
            m_Context.AddObject(typeof(IBulletTargetProperty), property, true);
            m_Context.AddObject(typeof(IActionFactory), TimelineActionFactory.Factory, true);
            m_Context.AddObject(typeof(IBaseActionProperty), property, true);
            m_Context.AddObject(typeof(IMoveActionProperty), property, true);

            ContextAddOtherObject();

            m_StateMachine = new FlyerStateMachine(m_Context);
            m_StateMachine.CompletedHandler += OnStateMachineComplete;

            m_StageType = FlyerStageType.Flying;
            m_StateMachine.SetInitialState(FlyerStateToken.Flying);
        }


        /// <summary>
        /// 添加监听
        /// </summary>
        public override void OnAddListener()
        {
            AddListener(ComponentEventName.FlyerTriggerToEnitity, DoEnd);                                                                                                                           //碰撞到单位了
        }

        /// <summary>
        /// 其他上下文需要携带的数据
        /// </summary>
        private void ContextAddOtherObject()
        {
            FlyerData flyerData = m_CfgEternityProxy.GetFlyerData(m_FlyerProprety.GetFlyerDataId());
            m_Context.AddObject<FlyerData>(flyerData);
        }

        /// <summary>
        /// 切换状态到 EndStage
        /// </summary>
        /// <param name="obj"></param>
        public void DoEnd(IComponentEvent obj)
        {
            FlyerTriggerToEnitity trigger = obj as FlyerTriggerToEnitity;
            Leyoutech.Utility.DebugUtility.LogWarning("子弹", string.Format("DoEnd --> 碰撞到单位了 who = {0}", trigger.targetEntity));
            StageType = FlyerStageType.End;
        }

        private void OnStateMachineComplete(StateMachine machine)
        {
            Leyoutech.Utility.DebugUtility.LogWarning("子弹", $"{GetType()}::OnStateMachineComplete::->子弹 销毁 EntityId = " + m_FlyerProprety.EntityId());
            m_Context.Clear(true);
            m_FlyerProprety.SetOnUpdateEnd(NoticeToDestroySelf);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            m_Context.Clear(true);

            m_CfgEternityProxy = null;
            m_Context = null;
            m_StateMachine = null;
            m_FlyerProprety = null;
        }


        public override void OnUpdate(float delta)
        {
            if(m_StateMachine.IsRunning)
            {
                m_StateMachine.DoUpdate(delta);
            }
        }

        /// <summary>
        /// 通知去干掉自己
        /// </summary>
        private void NoticeToDestroySelf()
        {
            GameplayManager.Instance.GetEntityManager().RemoveEntity(m_FlyerProprety.EntityId());
        }
    }
}

