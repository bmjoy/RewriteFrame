namespace Leyoutech.AI.FSM
{
    public abstract class StateEventArgs
    {
        public object Data { get; } = null;

        protected StateEventArgs(object data)
        {
            Data = data;
        }
    }

    public class StateEnterEventArgs : StateEventArgs
    {
        public StateToken From { get; }
        
        public StateEnterEventArgs(StateToken from):this(from,null)
        {
        }

        public StateEnterEventArgs(StateToken from,object data) : base(data)
        {
            From = from;
        }
    }

    public class StateExitEventArgs : StateEventArgs
    {
        public StateToken To { get; }

        public StateExitEventArgs(StateToken to) : this(to,null)
        {
        }

        public StateExitEventArgs(StateToken to,object data) : base(data)
        {
            To = to;
        }
    }

    public class StateChangedEventArgs : StateEventArgs
    {
        public StateBase OldState { get; }
        public StateBase NewState { get; }

        public StateChangedEventArgs(StateBase oldState,StateBase newState) : base(null)
        {
            OldState = oldState;
            NewState = newState;
        }
    }
}
