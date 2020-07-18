using Assets.Scripts.Core.Timeline;
using Eternity.FlatBuffer;
using Leyoutech.Core.Context;
using System.Collections.Generic;

namespace Leyoutech.Core.Timeline
{
    /// <summary>
    /// 时间轴状态
    /// </summary>
    public enum TimeLineState
    {
        /// <summary>
        /// 无效
        /// </summary>
        None=0,

        /// <summary>
        /// 运行中
        /// </summary>
        Playing,

        /// <summary>
        /// 暂停
        /// </summary>
        Paused,

        /// <summary>
        /// 完成
        /// </summary>
        Finished,
    }

    /// <summary>
    /// 完成回调
    /// </summary>
    /// <param name="trackGroup"></param>
    public delegate void OnTrackGroupFinished();



    public class TimelineTrackGroup : ATimelineEnv
    {
        /// <summary>
        /// 存放所有时间轴的容器
        /// </summary>
        private List<TimelineActionTrack> m_ActionTracks = new List<TimelineActionTrack>();

        /// <summary>
        /// 时间轴状态
        /// </summary>
        public TimeLineState State { get; private set; } = TimeLineState.None;

        private float m_Length = 0.0f;

        public OnTrackGroupFinished FinishCallback;

        public TimelineTrackGroup(IContext context)
        {
            SetEnv(context);
        }

        public void SetTrackGroup(TrackGroup trackGroup,float timeScale = 1.0f)
        {
            DoReset();

            m_Length = trackGroup.Length * timeScale;
            for (int i = 0; i < trackGroup.TracksLength; i++)
            {
                ActionTrack actionTrack = trackGroup.Tracks(i).Value;
                TimelineActionTrack tlaTrack = new TimelineActionTrack(m_Context, actionTrack,timeScale);
                m_ActionTracks.Add(tlaTrack);
            }
        }

        public void DoReset()
        {
            State = TimeLineState.None;
            foreach (var track in m_ActionTracks)
            {
                track.DoDestroy();
            }
            m_ActionTracks.Clear();
            m_Length = 0.0f;
            FinishCallback = null;
        }

        /// <summary>
        /// 销毁
        /// </summary>
        public override void DoDestroy()
        {
            DoReset();

            base.DoDestroy();
        }

        /// <summary>
        /// 运行
        /// </summary>
        public void Play()
        {
            State = TimeLineState.Playing;
        }

        /// <summary>
        /// 暂停
        /// </summary>
        public void Pause()
        {
            if(State == TimeLineState.Playing || State == TimeLineState.None)
            {
                State = TimeLineState.Paused;
                foreach(var track in m_ActionTracks)
                {
                    track.DoPause();
                }
            }
        }

        /// <summary>
        /// 恢复
        /// </summary>
        public void Resume()
        {
            if(State == TimeLineState.Paused)
            {
                State = TimeLineState.Playing;
                foreach(var track in m_ActionTracks)
                {
                    track.DoResume();
                }
            }
        }

        /// <summary>
        /// 停止
        /// </summary>
        public void Stop()
        {
            if(State == TimeLineState.Playing || State == TimeLineState.Paused)
            {
                State = TimeLineState.Finished;

                DoReset();
            }
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="deltaTime"></param>
        public override void DoUpdate(float deltaTime)
        {
            if(State != TimeLineState.Playing)
            {
                return;  //暂停更新
            }
            foreach (var track in m_ActionTracks)
            {
                track.DoUpdate(deltaTime);
            }

            m_Length -= deltaTime;
            if(m_Length<=0)
            {
                State = TimeLineState.Finished;
                FinishCallback.Invoke();
            }
        }
    }
}
