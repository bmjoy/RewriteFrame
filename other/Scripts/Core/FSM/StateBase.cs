using Leyoutech.Core.Context;
using System;
using System.Collections.Generic;

namespace Leyoutech.AI.FSM
{
    public delegate void StateInitializedHandler(StateBase state);
    public delegate void StateEnterHandler(StateBase state,StateEnterEventArgs e);
    public delegate void StateExitHandler(StateBase state,StateExitEventArgs e);

    public delegate void StateActionHandler(ActionToken action, object data);

    public abstract class StateBase
    {
        public StateToken Token { get; }

        public StateMachine Machine { get; private set; }
        
        public IContext Context
        {
            get
            {
                if(Machine!=null)
                {
                    return Machine.Context;
                }
                return null;
            }
        }

        public event StateInitializedHandler InitializedHandler;
        public event StateEnterHandler EnterHandler;
        public event StateExitHandler ExitHandler;

        private Dictionary<ActionToken, StateActionHandler> actionHandlerDic = new Dictionary<ActionToken, StateActionHandler>();

        protected StateBase(StateToken token)
        {
            if(token == null)
            {
                throw new ArgumentNullException("StateBase::StateBase->token is null");
            }

            Token = token;
        }

        public T GetContext<T>() where T:IContext
        {
            if(Context == null)
            {
                return default(T);
            }

            if(Context is T )
            {
                return (T)Context;
            }

            throw new InvalidCastException($"StateBase::GetContext->Context is of type '{Context.GetType().FullName}',impossible to cast to '{typeof(T).FullName}'.");
        }

        internal void Initialize(StateMachine machine)
        {
            Machine = machine;

            OnInitialized();
        }

        protected virtual void OnInitialized()
        {
            InitializedHandler?.Invoke(this);
        }

        public virtual void OnEnter(StateEnterEventArgs e)
        {
            EnterHandler?.Invoke(this, e);
        }

        public virtual void OnExit(StateExitEventArgs e)
        {
            ExitHandler?.Invoke(this, e);
        }

        protected void ChangeState(StateToken stateToken)
        {
            ChangeState(stateToken, null);
        }

        protected void ChangeState(StateToken stateToken, object data)
        {
            if (stateToken != Token)
            {
                Machine?.PerformTransitionTo(stateToken, data);
            }
        }

        public virtual void DoUpdate(float deltaTime)
        {
        }

        protected void RegisterAction(ActionToken action,StateActionHandler actionHandler)
        {
            if(action == null)
            {
                throw new ArgumentNullException("StateBase::RegisterAction->action is null");
            }

            if(actionHandler == null)
            {
                throw new ArgumentNullException("StateBase::RegisterAction->actionHandler is null");
            }

            if(actionHandlerDic.ContainsKey(action))
            {
                throw new InvalidOperationException($"Action has been registered.action = {action}.");
            }
            actionHandlerDic.Add(action, actionHandler);
        }

        protected void UnregisterAction(ActionToken action)
        {
            if (action == null)
            {
                throw new ArgumentNullException("StateBase::UnregisterAction->action is null");
            }
            if(actionHandlerDic.ContainsKey(action))
            {
                actionHandlerDic.Remove(action);
            }
        }

        internal void ExecuteAction(ActionToken action,object data)
        {
            if(actionHandlerDic.TryGetValue(action,out StateActionHandler handler))
            {
                handler(action, data);
            }
        }

        public override string ToString()
        {
            return Token?.ToString() ?? "(Null Token)";
        }
    }
}
