using Crucis.Protocol.GameSession;
using UnityEngine;

namespace Crucis.Protocol
{
    /// <summary>
    /// 玩家运动协议
    /// </summary>
    public static class HeroMoveHandler
    {
        public static HeroMoveStream hm_stream;
        
        public static async void StartHeroMoveStream()
        {
            hm_stream?.Close();
            hm_stream = new HeroMoveStream();
            HeroMoveResponse response;
            while ((response = await hm_stream.ReadAsync()) != null)
            {
                HeroMoveResponse.Types.Success success = response.Success;
                if (success != null)
                {
                    G2C_HeroMove g2C_HeroMove = new G2C_HeroMove() { Respond = success.Success_ };
                    GameplayManager.Instance.GetEntityManager().SendEventToEntity((uint)success.Success_.HeroId, ComponentEventName.G2C_HeroMove, g2C_HeroMove);
                }
            }
        }

        public static void SyncMMoMove(Vector3 engine_force, Vector3 orientations, ulong currentstate
            , ulong areaid, Vector3 position, Quaternion rotation, Vector3 line_velocity, Vector3 angular_velocity)
        {
            HeroMoveUnit heroMoveUnit = new HeroMoveUnit();
            MMOMove mmoMove = new MMOMove();

            mmoMove.EngineAxis = NetHelper.PxVec3ToVec3(engine_force);
            mmoMove.RotateAxis = NetHelper.PxVec3ToVec3(orientations);
            heroMoveUnit.MmoMove = mmoMove;

            hm_stream?.Wright(heroMoveUnit, currentstate, areaid,
                NetHelper.PxVec3ToVec3(position), NetHelper.PxQuatToVec4(rotation),
                NetHelper.PxVec3ToVec3(line_velocity), NetHelper.PxVec3ToVec3(angular_velocity));
        }

        public static void SyncDof4Move(Vector3 engine_force, Vector3 orientations, ulong currentstate
            , ulong areaid, Vector3 position, Quaternion rotation, Vector3 line_velocity, Vector3 angular_velocity)
        {
            HeroMoveUnit heroMoveUnit = new HeroMoveUnit();
            Dof4Move dof4Move = new Dof4Move();
            
            dof4Move.EngineForce = NetHelper.PxVec3ToVec3(engine_force);
            dof4Move.Orientations = NetHelper.PxVec3ToVec3(orientations);
            heroMoveUnit.Dof4Move = dof4Move;

            hm_stream?.Wright(heroMoveUnit, currentstate, areaid,
                NetHelper.PxVec3ToVec3(position), NetHelper.PxQuatToVec4(rotation), 
                NetHelper.PxVec3ToVec3(line_velocity), NetHelper.PxVec3ToVec3(angular_velocity));
        }

        public static void SyncDof6Move(Vector3 engine_force, Vector3 orientations, ulong currentstate
            , ulong areaid, Vector3 position, Quaternion rotation, Vector3 line_velocity, Vector3 angular_velocity)
        {
            HeroMoveUnit heroMoveUnit = new HeroMoveUnit();
            Dof6Move dof6Move = new Dof6Move();

            dof6Move.EngineForce = NetHelper.PxVec3ToVec3(engine_force);
            dof6Move.EngineTorque = NetHelper.PxVec3ToVec3(orientations);
            heroMoveUnit.Dof6Move = dof6Move;

            hm_stream?.Wright(heroMoveUnit, currentstate, areaid,
                NetHelper.PxVec3ToVec3(position), NetHelper.PxQuatToVec4(rotation),
                NetHelper.PxVec3ToVec3(line_velocity), NetHelper.PxVec3ToVec3(angular_velocity));
        }

        public static void SyncHitWall(bool isCollision, ulong currentstate
            , ulong areaid, Vector3 position, Quaternion rotation, Vector3 line_velocity, Vector3 angular_velocity)
        {
            HeroMoveUnit heroMoveUnit = new HeroMoveUnit();
            HitWall hitWall = new HitWall();

            hitWall.IsHitWall = isCollision;
            heroMoveUnit.HitWall = hitWall;

            hm_stream?.Wright(heroMoveUnit, currentstate, areaid,
                NetHelper.PxVec3ToVec3(position), NetHelper.PxQuatToVec4(rotation),
                NetHelper.PxVec3ToVec3(line_velocity), NetHelper.PxVec3ToVec3(angular_velocity));
        }

        public static void SyncLeap(ulong toAreaId, ulong currentstate
            , ulong areaid, Vector3 position, Quaternion rotation, Vector3 line_velocity, Vector3 angular_velocity)
        {
            HeroMoveUnit heroMoveUnit = new HeroMoveUnit();
            Leap leap = new Leap();

            leap.ToAreaId = toAreaId;
            heroMoveUnit.Leap = leap;

            hm_stream?.Wright(heroMoveUnit, currentstate, areaid,
                NetHelper.PxVec3ToVec3(position), NetHelper.PxQuatToVec4(rotation),
                NetHelper.PxVec3ToVec3(line_velocity), NetHelper.PxVec3ToVec3(angular_velocity));
        }

        public static void SyncLeapCancel(ulong currentstate
          , ulong areaid, Vector3 position, Quaternion rotation, Vector3 line_velocity, Vector3 angular_velocity)
        {
            HeroMoveUnit heroMoveUnit = new HeroMoveUnit();
            heroMoveUnit.LeapCancel = new LeapCancel();

            hm_stream?.Wright(heroMoveUnit, currentstate, areaid,
                NetHelper.PxVec3ToVec3(position), NetHelper.PxQuatToVec4(rotation),
                NetHelper.PxVec3ToVec3(line_velocity), NetHelper.PxVec3ToVec3(angular_velocity));
        }
    }
}
