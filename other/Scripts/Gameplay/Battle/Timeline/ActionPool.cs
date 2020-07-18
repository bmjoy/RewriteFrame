
/*
This file was create by tool.
Please don't change it by your self.
*/

using Leyoutech.Core.Pool;
using Leyoutech.Core.Timeline;
using Eternity.FlatBuffer.Enums;
using Gameplay.Battle.Timeline.Actions.UI;
using Battle.Timeline.Actions.States;
using Gameplay.Battle.Timeline.Actions.Speeds;
using Battle.Timeline.Actions.Sounds;
using Gameplay.Battle.Timeline.Actions.Sounds;
using Gameplay.Battle.Timeline.Actions.Others;
using Gameplay.Battle.Timeline.Actions.Flyer;
using Gameplay.Battle.Timeline.Actions.FlyerPaths;
using Gameplay.Battle.Timeline.Actions.Emit;
using Gameplay.Battle.Timeline.Actions.Effects;
using Gameplay.Battle.Timeline.Actions.Directions;
using Gameplay.Battle.Timeline.Actions.CDs;
using Gameplay.Battle.Timeline.Actions.Accelerates;

namespace Gameplay.Battle.Timeline
{
    public static partial class ActionPool
    {
        static ActionPool()
        {
            Creater = GetActionPool;
        }

        public static IActionPool GetActionPool(ActionID actionID)
        {
            switch(actionID)
            {

                case ActionID.UI_CHANGE_CROSSSIGHT_SIZE:
                    return new ChangeCrossSightSizeActionPool();

                case ActionID.PAUSE_TIMELINE_EVENT_ACTION:
                    return new PauseActionPool();

                case ActionID.SET_MAX_SPEED_EVENT_ACTION:
                    return new SetMaxSpeedActionPool();

                case ActionID.CHANGE_SPEED_EVENT_ACTION:
                    return new ChangeSpeedActionPool();

                case ActionID.PLAY_POINT_SOUND_EVENT_ACTION:
                    return new PlayPointSoundActionPool();

                case ActionID.PLAY_ENTITY_POSITION_SOUND_EVENT_ACTION:
                    return new PlayEntityPositionSoundActionPool();

                case ActionID.PLAY_ENTITY_BIND_SOUND_EVENT_ACTION:
                    return new PlayEntityBindSoundActionPool();

                case ActionID.PLAY_EMIT_SOUND_EVENT_ACTION:
                    return new PlayEmitSoundActionPool();

                case ActionID.ACCUMULATION_FORCE_RELEASE_EVENT_ACTION:
                    return new AccumulationForceReleaseActionPool();

                case ActionID.FLYER_EMIT_EVENT_ACTION:
                    return new FlyerEmitActionPool();

                case ActionID.FLYER_TRACE_PATH_DURATION_ACTION:
                    return new FlyerTracePathActionPool();

                case ActionID.FLYER_ANGULAR_TRACE_PATH_DURATION_ACTION:
                    return new FlyerAngularTracePathActionPool();

                case ActionID.TARGETNEAR_EMIT_NODE_EVENT_ACTION:
                    return new TargetNearEmitNodeActionPool();

                case ActionID.RANGE_EMIT_NODE_EVENT_ACTION:
                    return new RangeEmitNodeActionPool();

                case ActionID.NEXT_LOOP_EMIT_NODE_EVENT_ACTION:
                    return new NextLoopEmitNodeActionPool();

                case ActionID.FIXED_EMIT_NODE_EVENT_ACTION:
                    return new FixedEmitNodeActionPool();

                case ActionID.AUTO_RANGE_EMIT_NODE_EVENT_ACTION:
                    return new AutoRangeEmitNodeActionPool();

                case ActionID.AOE_TARGET_EFFECT_EVENT_ACTION:
                    return new AoeTargetEffectActionPool();

                case ActionID.ADD_ENTITY_POSITION_EFFECT_EVENT_ACTION:
                    return new AddEntityPositionEffectActionPool();

                case ActionID.ADD_ENTITY_BIND_TO_TARGET_EFFECT_EVENT_ACTION:
                    return new AddEntityBindToTargetEffectActionPool();

                case ActionID.ADD_EMIT_LINK_TARGET_EFFECT_EVENT_ACTION:
                    return new AddEmitLinkTargetEffectActionPool();

                case ActionID.ADD_EMIT_LINK_TARGET__EFFECT_DURATION_ACTION:
                    return new AddEmitLinkTargetDurationEffectActionPool();

                case ActionID.ADD_EMIT_EFFECT_EVENT_ACTION:
                    return new AddEmitEffectActionPool();

                case ActionID.ADD_EMIT_EFFECT_DURATION_ACTION:
                    return new AddEmitDurationEffectActionPool();

                case ActionID.ADD_BIND_LINK_TARGET_EFFECT_EVENT_ACTION:
                    return new AddBindLinkTargetEffectActionPool();

                case ActionID.SET_DIRECTION_EVENT_ACTION:
                    return new SetDirectionActionPool();

                case ActionID.AFFECT_CD_EVENT_ACTION:
                    return new AffectCDActionPool();

                case ActionID.CHANGE_ACCELERATE_DURATION_ACTION:
                    return new ChangeAccelerateActionPool();

            }

            return null;
        }
    }


    public class ChangeCrossSightSizeActionPool : IActionPool
    {
        private ObjectPool<ChangeCrossSightSizeAction> m_Pool = null;

        public ChangeCrossSightSizeActionPool()
        {
            m_Pool = new ObjectPool<ChangeCrossSightSizeAction>(5);
        }

        public AActionItem GetAction()
        {
            return m_Pool.Get();
        }

        public void ReleaseAction(AActionItem actionItem)
        {
            m_Pool.Release((ChangeCrossSightSizeAction)actionItem);
        }

        public void Clear()
        {
            m_Pool.Clear();
            m_Pool = null;
        }
    }

    public class PauseActionPool : IActionPool
    {
        private ObjectPool<PauseAction> m_Pool = null;

        public PauseActionPool()
        {
            m_Pool = new ObjectPool<PauseAction>(5);
        }

        public AActionItem GetAction()
        {
            return m_Pool.Get();
        }

        public void ReleaseAction(AActionItem actionItem)
        {
            m_Pool.Release((PauseAction)actionItem);
        }

        public void Clear()
        {
            m_Pool.Clear();
            m_Pool = null;
        }
    }

    public class SetMaxSpeedActionPool : IActionPool
    {
        private ObjectPool<SetMaxSpeedAction> m_Pool = null;

        public SetMaxSpeedActionPool()
        {
            m_Pool = new ObjectPool<SetMaxSpeedAction>(5);
        }

        public AActionItem GetAction()
        {
            return m_Pool.Get();
        }

        public void ReleaseAction(AActionItem actionItem)
        {
            m_Pool.Release((SetMaxSpeedAction)actionItem);
        }

        public void Clear()
        {
            m_Pool.Clear();
            m_Pool = null;
        }
    }

    public class ChangeSpeedActionPool : IActionPool
    {
        private ObjectPool<ChangeSpeedAction> m_Pool = null;

        public ChangeSpeedActionPool()
        {
            m_Pool = new ObjectPool<ChangeSpeedAction>(5);
        }

        public AActionItem GetAction()
        {
            return m_Pool.Get();
        }

        public void ReleaseAction(AActionItem actionItem)
        {
            m_Pool.Release((ChangeSpeedAction)actionItem);
        }

        public void Clear()
        {
            m_Pool.Clear();
            m_Pool = null;
        }
    }

    public class PlayPointSoundActionPool : IActionPool
    {
        private ObjectPool<PlayPointSoundAction> m_Pool = null;

        public PlayPointSoundActionPool()
        {
            m_Pool = new ObjectPool<PlayPointSoundAction>(5);
        }

        public AActionItem GetAction()
        {
            return m_Pool.Get();
        }

        public void ReleaseAction(AActionItem actionItem)
        {
            m_Pool.Release((PlayPointSoundAction)actionItem);
        }

        public void Clear()
        {
            m_Pool.Clear();
            m_Pool = null;
        }
    }

    public class PlayEntityPositionSoundActionPool : IActionPool
    {
        private ObjectPool<PlayEntityPositionSoundAction> m_Pool = null;

        public PlayEntityPositionSoundActionPool()
        {
            m_Pool = new ObjectPool<PlayEntityPositionSoundAction>(5);
        }

        public AActionItem GetAction()
        {
            return m_Pool.Get();
        }

        public void ReleaseAction(AActionItem actionItem)
        {
            m_Pool.Release((PlayEntityPositionSoundAction)actionItem);
        }

        public void Clear()
        {
            m_Pool.Clear();
            m_Pool = null;
        }
    }

    public class PlayEntityBindSoundActionPool : IActionPool
    {
        private ObjectPool<PlayEntityBindSoundAction> m_Pool = null;

        public PlayEntityBindSoundActionPool()
        {
            m_Pool = new ObjectPool<PlayEntityBindSoundAction>(5);
        }

        public AActionItem GetAction()
        {
            return m_Pool.Get();
        }

        public void ReleaseAction(AActionItem actionItem)
        {
            m_Pool.Release((PlayEntityBindSoundAction)actionItem);
        }

        public void Clear()
        {
            m_Pool.Clear();
            m_Pool = null;
        }
    }

    public class PlayEmitSoundActionPool : IActionPool
    {
        private ObjectPool<PlayEmitSoundAction> m_Pool = null;

        public PlayEmitSoundActionPool()
        {
            m_Pool = new ObjectPool<PlayEmitSoundAction>(5);
        }

        public AActionItem GetAction()
        {
            return m_Pool.Get();
        }

        public void ReleaseAction(AActionItem actionItem)
        {
            m_Pool.Release((PlayEmitSoundAction)actionItem);
        }

        public void Clear()
        {
            m_Pool.Clear();
            m_Pool = null;
        }
    }

    public class AccumulationForceReleaseActionPool : IActionPool
    {
        private ObjectPool<AccumulationForceReleaseAction> m_Pool = null;

        public AccumulationForceReleaseActionPool()
        {
            m_Pool = new ObjectPool<AccumulationForceReleaseAction>(5);
        }

        public AActionItem GetAction()
        {
            return m_Pool.Get();
        }

        public void ReleaseAction(AActionItem actionItem)
        {
            m_Pool.Release((AccumulationForceReleaseAction)actionItem);
        }

        public void Clear()
        {
            m_Pool.Clear();
            m_Pool = null;
        }
    }

    public class FlyerEmitActionPool : IActionPool
    {
        private ObjectPool<FlyerEmitAction> m_Pool = null;

        public FlyerEmitActionPool()
        {
            m_Pool = new ObjectPool<FlyerEmitAction>(5);
        }

        public AActionItem GetAction()
        {
            return m_Pool.Get();
        }

        public void ReleaseAction(AActionItem actionItem)
        {
            m_Pool.Release((FlyerEmitAction)actionItem);
        }

        public void Clear()
        {
            m_Pool.Clear();
            m_Pool = null;
        }
    }

    public class FlyerTracePathActionPool : IActionPool
    {
        private ObjectPool<FlyerTracePathAction> m_Pool = null;

        public FlyerTracePathActionPool()
        {
            m_Pool = new ObjectPool<FlyerTracePathAction>(5);
        }

        public AActionItem GetAction()
        {
            return m_Pool.Get();
        }

        public void ReleaseAction(AActionItem actionItem)
        {
            m_Pool.Release((FlyerTracePathAction)actionItem);
        }

        public void Clear()
        {
            m_Pool.Clear();
            m_Pool = null;
        }
    }

    public class FlyerAngularTracePathActionPool : IActionPool
    {
        private ObjectPool<FlyerAngularTracePathAction> m_Pool = null;

        public FlyerAngularTracePathActionPool()
        {
            m_Pool = new ObjectPool<FlyerAngularTracePathAction>(5);
        }

        public AActionItem GetAction()
        {
            return m_Pool.Get();
        }

        public void ReleaseAction(AActionItem actionItem)
        {
            m_Pool.Release((FlyerAngularTracePathAction)actionItem);
        }

        public void Clear()
        {
            m_Pool.Clear();
            m_Pool = null;
        }
    }

    public class TargetNearEmitNodeActionPool : IActionPool
    {
        private ObjectPool<TargetNearEmitNodeAction> m_Pool = null;

        public TargetNearEmitNodeActionPool()
        {
            m_Pool = new ObjectPool<TargetNearEmitNodeAction>(5);
        }

        public AActionItem GetAction()
        {
            return m_Pool.Get();
        }

        public void ReleaseAction(AActionItem actionItem)
        {
            m_Pool.Release((TargetNearEmitNodeAction)actionItem);
        }

        public void Clear()
        {
            m_Pool.Clear();
            m_Pool = null;
        }
    }

    public class RangeEmitNodeActionPool : IActionPool
    {
        private ObjectPool<RangeEmitNodeAction> m_Pool = null;

        public RangeEmitNodeActionPool()
        {
            m_Pool = new ObjectPool<RangeEmitNodeAction>(5);
        }

        public AActionItem GetAction()
        {
            return m_Pool.Get();
        }

        public void ReleaseAction(AActionItem actionItem)
        {
            m_Pool.Release((RangeEmitNodeAction)actionItem);
        }

        public void Clear()
        {
            m_Pool.Clear();
            m_Pool = null;
        }
    }

    public class NextLoopEmitNodeActionPool : IActionPool
    {
        private ObjectPool<NextLoopEmitNodeAction> m_Pool = null;

        public NextLoopEmitNodeActionPool()
        {
            m_Pool = new ObjectPool<NextLoopEmitNodeAction>(5);
        }

        public AActionItem GetAction()
        {
            return m_Pool.Get();
        }

        public void ReleaseAction(AActionItem actionItem)
        {
            m_Pool.Release((NextLoopEmitNodeAction)actionItem);
        }

        public void Clear()
        {
            m_Pool.Clear();
            m_Pool = null;
        }
    }

    public class FixedEmitNodeActionPool : IActionPool
    {
        private ObjectPool<FixedEmitNodeAction> m_Pool = null;

        public FixedEmitNodeActionPool()
        {
            m_Pool = new ObjectPool<FixedEmitNodeAction>(5);
        }

        public AActionItem GetAction()
        {
            return m_Pool.Get();
        }

        public void ReleaseAction(AActionItem actionItem)
        {
            m_Pool.Release((FixedEmitNodeAction)actionItem);
        }

        public void Clear()
        {
            m_Pool.Clear();
            m_Pool = null;
        }
    }

    public class AutoRangeEmitNodeActionPool : IActionPool
    {
        private ObjectPool<AutoRangeEmitNodeAction> m_Pool = null;

        public AutoRangeEmitNodeActionPool()
        {
            m_Pool = new ObjectPool<AutoRangeEmitNodeAction>(5);
        }

        public AActionItem GetAction()
        {
            return m_Pool.Get();
        }

        public void ReleaseAction(AActionItem actionItem)
        {
            m_Pool.Release((AutoRangeEmitNodeAction)actionItem);
        }

        public void Clear()
        {
            m_Pool.Clear();
            m_Pool = null;
        }
    }

    public class AoeTargetEffectActionPool : IActionPool
    {
        private ObjectPool<AoeTargetEffectAction> m_Pool = null;

        public AoeTargetEffectActionPool()
        {
            m_Pool = new ObjectPool<AoeTargetEffectAction>(5);
        }

        public AActionItem GetAction()
        {
            return m_Pool.Get();
        }

        public void ReleaseAction(AActionItem actionItem)
        {
            m_Pool.Release((AoeTargetEffectAction)actionItem);
        }

        public void Clear()
        {
            m_Pool.Clear();
            m_Pool = null;
        }
    }

    public class AddEntityPositionEffectActionPool : IActionPool
    {
        private ObjectPool<AddEntityPositionEffectAction> m_Pool = null;

        public AddEntityPositionEffectActionPool()
        {
            m_Pool = new ObjectPool<AddEntityPositionEffectAction>(5);
        }

        public AActionItem GetAction()
        {
            return m_Pool.Get();
        }

        public void ReleaseAction(AActionItem actionItem)
        {
            m_Pool.Release((AddEntityPositionEffectAction)actionItem);
        }

        public void Clear()
        {
            m_Pool.Clear();
            m_Pool = null;
        }
    }

    public class AddEntityBindToTargetEffectActionPool : IActionPool
    {
        private ObjectPool<AddEntityBindToTargetEffectAction> m_Pool = null;

        public AddEntityBindToTargetEffectActionPool()
        {
            m_Pool = new ObjectPool<AddEntityBindToTargetEffectAction>(5);
        }

        public AActionItem GetAction()
        {
            return m_Pool.Get();
        }

        public void ReleaseAction(AActionItem actionItem)
        {
            m_Pool.Release((AddEntityBindToTargetEffectAction)actionItem);
        }

        public void Clear()
        {
            m_Pool.Clear();
            m_Pool = null;
        }
    }

    public class AddEmitLinkTargetEffectActionPool : IActionPool
    {
        private ObjectPool<AddEmitLinkTargetEffectAction> m_Pool = null;

        public AddEmitLinkTargetEffectActionPool()
        {
            m_Pool = new ObjectPool<AddEmitLinkTargetEffectAction>(5);
        }

        public AActionItem GetAction()
        {
            return m_Pool.Get();
        }

        public void ReleaseAction(AActionItem actionItem)
        {
            m_Pool.Release((AddEmitLinkTargetEffectAction)actionItem);
        }

        public void Clear()
        {
            m_Pool.Clear();
            m_Pool = null;
        }
    }

    public class AddEmitLinkTargetDurationEffectActionPool : IActionPool
    {
        private ObjectPool<AddEmitLinkTargetDurationEffectAction> m_Pool = null;

        public AddEmitLinkTargetDurationEffectActionPool()
        {
            m_Pool = new ObjectPool<AddEmitLinkTargetDurationEffectAction>(5);
        }

        public AActionItem GetAction()
        {
            return m_Pool.Get();
        }

        public void ReleaseAction(AActionItem actionItem)
        {
            m_Pool.Release((AddEmitLinkTargetDurationEffectAction)actionItem);
        }

        public void Clear()
        {
            m_Pool.Clear();
            m_Pool = null;
        }
    }

    public class AddEmitEffectActionPool : IActionPool
    {
        private ObjectPool<AddEmitEffectAction> m_Pool = null;

        public AddEmitEffectActionPool()
        {
            m_Pool = new ObjectPool<AddEmitEffectAction>(5);
        }

        public AActionItem GetAction()
        {
            return m_Pool.Get();
        }

        public void ReleaseAction(AActionItem actionItem)
        {
            m_Pool.Release((AddEmitEffectAction)actionItem);
        }

        public void Clear()
        {
            m_Pool.Clear();
            m_Pool = null;
        }
    }

    public class AddEmitDurationEffectActionPool : IActionPool
    {
        private ObjectPool<AddEmitDurationEffectAction> m_Pool = null;

        public AddEmitDurationEffectActionPool()
        {
            m_Pool = new ObjectPool<AddEmitDurationEffectAction>(5);
        }

        public AActionItem GetAction()
        {
            return m_Pool.Get();
        }

        public void ReleaseAction(AActionItem actionItem)
        {
            m_Pool.Release((AddEmitDurationEffectAction)actionItem);
        }

        public void Clear()
        {
            m_Pool.Clear();
            m_Pool = null;
        }
    }

    public class AddBindLinkTargetEffectActionPool : IActionPool
    {
        private ObjectPool<AddBindLinkTargetEffectAction> m_Pool = null;

        public AddBindLinkTargetEffectActionPool()
        {
            m_Pool = new ObjectPool<AddBindLinkTargetEffectAction>(5);
        }

        public AActionItem GetAction()
        {
            return m_Pool.Get();
        }

        public void ReleaseAction(AActionItem actionItem)
        {
            m_Pool.Release((AddBindLinkTargetEffectAction)actionItem);
        }

        public void Clear()
        {
            m_Pool.Clear();
            m_Pool = null;
        }
    }

    public class SetDirectionActionPool : IActionPool
    {
        private ObjectPool<SetDirectionAction> m_Pool = null;

        public SetDirectionActionPool()
        {
            m_Pool = new ObjectPool<SetDirectionAction>(5);
        }

        public AActionItem GetAction()
        {
            return m_Pool.Get();
        }

        public void ReleaseAction(AActionItem actionItem)
        {
            m_Pool.Release((SetDirectionAction)actionItem);
        }

        public void Clear()
        {
            m_Pool.Clear();
            m_Pool = null;
        }
    }

    public class AffectCDActionPool : IActionPool
    {
        private ObjectPool<AffectCDAction> m_Pool = null;

        public AffectCDActionPool()
        {
            m_Pool = new ObjectPool<AffectCDAction>(5);
        }

        public AActionItem GetAction()
        {
            return m_Pool.Get();
        }

        public void ReleaseAction(AActionItem actionItem)
        {
            m_Pool.Release((AffectCDAction)actionItem);
        }

        public void Clear()
        {
            m_Pool.Clear();
            m_Pool = null;
        }
    }

    public class ChangeAccelerateActionPool : IActionPool
    {
        private ObjectPool<ChangeAccelerateAction> m_Pool = null;

        public ChangeAccelerateActionPool()
        {
            m_Pool = new ObjectPool<ChangeAccelerateAction>(5);
        }

        public AActionItem GetAction()
        {
            return m_Pool.Get();
        }

        public void ReleaseAction(AActionItem actionItem)
        {
            m_Pool.Release((ChangeAccelerateAction)actionItem);
        }

        public void Clear()
        {
            m_Pool.Clear();
            m_Pool = null;
        }
    }


}
