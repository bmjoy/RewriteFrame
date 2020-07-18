using Eternity.FlatBuffer;
using Leyoutech.AI.FSM;
using Leyoutech.Core.Timeline;

namespace Gameplay.Battle.Timeline.States
{
    public abstract class AActionState : StateBase
    {
        protected TimelineTrackGroup m_TrackGroup;

        protected AActionState(StateToken token) : base(token)
        {
        }

        public override void OnEnter(StateEnterEventArgs e)
        {
            InitTrackGroup();
            base.OnEnter(e);
        }

        public override void OnExit(StateExitEventArgs e)
        {
            base.OnExit(e);
            DisposeTrackGroup();
        }

        public override void DoUpdate(float deltaTime)
        {
            m_TrackGroup?.DoUpdate(deltaTime);
        }

        protected void InitTrackGroup()
        {
            if (m_TrackGroup == null)
            {
                m_TrackGroup = new TimelineTrackGroup(Context);
                
                Context.AddObject(m_TrackGroup);
            }
            float timeScale = 1.0f;
            TrackGroup group = GetTrackGroup(out timeScale);

            m_TrackGroup.SetTrackGroup(group,timeScale);
            m_TrackGroup.FinishCallback = OnTrackGroupComplete;

            m_TrackGroup.Play();
        }

        protected void DisposeTrackGroup()
        {
            Context.DeleteObject<TimelineTrackGroup>();
            if (m_TrackGroup != null)
            {
                m_TrackGroup.FinishCallback = null;
                m_TrackGroup.DoDestroy();
                m_TrackGroup = null;
            }
        }

        protected abstract TrackGroup GetTrackGroup(out float timeScale);
        protected abstract void OnTrackGroupComplete();
    }
}
