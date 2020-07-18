using Leyoutech.Core.Context;
using System;
using System.Collections.Generic;

namespace Leyoutech.AI.FSM
{
    public delegate void StateChangedHandler(StateMachine machine, StateChangedEventArgs e);
    public delegate void MachineCompletedHandler(StateMachine machine);

    public class StateMachine
    {
        public IContext Context { get; }

        public StateBase CurrentState { get; private set; }
        public StateToken CurrentStateToken => CurrentState?.Token;

        public bool IsRunning => CurrentState != null;

        public event StateChangedHandler ChangedHandler;
        public event MachineCompletedHandler CompletedHandler;

        private Dictionary<StateToken, StateBase> stateDic = new Dictionary<StateToken, StateBase>();

        public StateMachine():this(null)
        {
        }

        public StateMachine(IContext context)
        {
            Context = context;
        }

        public void RegisterState(StateBase state)
        {
            if(state == null)
            {
                throw new ArgumentNullException("StateMachine::RegisterState->state is null");
            }

            if(stateDic.ContainsKey(state.Token))
            {
                throw new InvalidOperationException($"StateMachine::RegisterState->{state.Token} already registered.");
            }
            
            stateDic.Add(state.Token, state);
            state.Initialize(this);
        }

        public void SetInitialState(StateToken initialState)
        {
            SetInitialState(initialState, null);
        }

        public void SetInitialState(StateToken initialState,object data)
        {
            PerformTransitionTo(initialState, data);
        }

        public void PerformTransitionTo(StateToken stateToken, object data)
        {
            if(stateToken == null)
            {
                if (IsRunning)
                {
                    CurrentState.OnExit(new StateExitEventArgs(null, data));
                    CurrentState = null;
                }
                OnComplete();
            }
            else
            {
                if(stateDic.TryGetValue(stateToken,out StateBase state))
                {
                    if(!IsRunning)
                    {
                        CurrentState = state;
                        CurrentState.OnEnter(new StateEnterEventArgs(null, data));
                    }
                    else
                    {
                        if(CurrentStateToken != stateToken)
                        {
                            StateBase oldState = CurrentState;

                            CurrentState.OnExit(new StateExitEventArgs(stateToken, data));
                            CurrentState = state;
                            CurrentState.OnEnter(new StateEnterEventArgs(oldState.Token, data));

                            OnStateChanged(new StateChangedEventArgs(oldState, state));
                        }
                    }
                }else
                {
                    throw new InvalidOperationException($"StateMachine::PerformTransitionTo->state is not found.toke  = {stateToken}");
                }
            }
        }

        protected virtual void OnStateChanged(StateChangedEventArgs e)
        {
            ChangedHandler?.Invoke(this, e);
        }

        protected virtual void OnComplete()
        {
            CompletedHandler?.Invoke(this);
        }

        public virtual void DoUpdate(float deltaTime)
        {
            CurrentState?.DoUpdate(deltaTime);
        }

        public void PerformAction(ActionToken action)
        {
            PerformAction(action, null);
        }

        public void PerformAction(ActionToken action,object data)
        {
            if(action == null)
            {
                throw new ArgumentNullException("StateMachine::PreformAction->actoin is null");
            }

            if(CurrentState == null)
            {
                throw new InvalidOperationException("StateMachine::PreformAction->CurrentState is null");
            }

            CurrentState.ExecuteAction(action, data);
        }
    }
}
