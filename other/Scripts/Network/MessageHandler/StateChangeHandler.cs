
using Crucis.Protocol.GameSession;
using UnityEngine;

namespace Crucis.Protocol
{
    public static class StateChangeHandler
    {
        private static ChangeStateStream cs_stream;
        
        private static void Run(GameSession.ChangeStateResponse message)
        {
            GameSession.ChangeStateResponse.Types.Success success = message.Success;
            if (success != null)
            {
                ulong heroId = success.Success_.HeroId;
                ulong previousState = success.Success_.PreviousState;
                ulong currentState = success.Success_.CurrentState;

                Debug.Log($"ChangeStateResponse: heroid = {heroId}, prestate = {previousState}, curstate = {currentState}");

                if (success.Success_.ExMessage != null && success.Success_.ExMessage.BehaviorMessage != null)
                {
                    ChangeProperty changeProperty = new ChangeProperty();
                    changeProperty.SpacecraftMotionInfo = new SpacecraftMotionInfo()
                    {
                        LineAcceleration = success.Success_.ExMessage.BehaviorMessage.Force,
                        ReverseLineAcceleration = success.Success_.ExMessage.BehaviorMessage.Invforce,
                        ResistanceLineAcceleration = success.Success_.ExMessage.BehaviorMessage.Stopforce,
                        LineVelocityMax = success.Success_.ExMessage.BehaviorMessage.Maxsp,
                        ReverseLineVelocityMax = success.Success_.ExMessage.BehaviorMessage.Invmaxsp,
                        AngularAcceleration = success.Success_.ExMessage.BehaviorMessage.Torque,
                        ResistanceAngularAcceleration = success.Success_.ExMessage.BehaviorMessage.Stoptourque,
                        AngularVelocityMax = success.Success_.ExMessage.BehaviorMessage.Maxanglespeed,
                        CruiseLeapAcceleration = success.Success_.ExMessage.BehaviorMessage.Leapforce,
                        CruiseLeapReverseAcceleration = success.Success_.ExMessage.BehaviorMessage.Leapstopforce
                    };

                    //Debug.LogError("OnChangeProperty heroId:" + heroId + " "  + JsonUtility.ToJson(changeProperty));

                    GameplayManager.Instance.GetEntityManager().SendEventToEntity((uint)heroId, ComponentEventName.ChangeProperty, changeProperty);
                    return;
                }

                GameplayManager.Instance.GetEntityManager().SendEventToEntity((uint)heroId, ComponentEventName.G2C_ChangeState, new G2C_ChangeState()
                {
                    EntityId = heroId,
                    PreviousState = previousState,
                    CurrentState = currentState,
                    ExData = success.Success_.ExMessage
                });
            }
        }

        private static int getHeadState(ulong curstate)
        {
            int res = (int)(curstate >> 60);
            return res;
        }

        private static bool isHaveComState(ulong curstate, int cs)
        {
            if ((curstate & (ulong)(1 << cs)) > 0)
                return true;
            else
                return false;
        }

        private static void debugStateLog(ulong heroid, ulong prestate, ulong curstate)
        {
            string logstring = ($"************* HeroID: {heroid}, State:");
            int headstate = getHeadState(curstate);
            if (headstate == 0)
                logstring = logstring + "Born-";
            else if (headstate == 1)
                logstring = logstring + "Cruise-";
            else if (headstate == 2)
                logstring = logstring + "Fight-";
            else if (headstate == 3)
                logstring = logstring + "Dead-";
            else if (headstate == 11)
                logstring = logstring + "Stand-";
            else if (headstate == 12)
                logstring = logstring + "Move-";

            if (isHaveComState(curstate, 1))
                logstring = logstring + " | Login";
            if (isHaveComState(curstate, 2))
                logstring = logstring + " | Relive";
            if (isHaveComState(curstate, 3))
                logstring = logstring + " | PeerLess";
            if (isHaveComState(curstate, 4))
                logstring = logstring + " | OverLoad";
            if (isHaveComState(curstate, 54))
                logstring = logstring + " | BackToAnchor";
            if (isHaveComState(curstate, 55))
                logstring = logstring + " | LeapPrepare";
            if (isHaveComState(curstate, 56))
                logstring = logstring + " | LeapCancel";
            if (isHaveComState(curstate, 57))
                logstring = logstring + " | LeapPreheat";
            if (isHaveComState(curstate, 58))
                logstring = logstring + " | Leaping";
            if (isHaveComState(curstate, 59))
                logstring = logstring + " | LeapRemoveNotify";

            Debug.Log(logstring);
        }

        public static async void HandleChangeState()
        {
            cs_stream?.Close();
            cs_stream = new ChangeStateStream();
            ChangeStateResponse response;
            while ((response = await cs_stream.ReadAsync()) != null)
            {
                StateChangeHandler.Run(response);
            }
        }
    }
}
