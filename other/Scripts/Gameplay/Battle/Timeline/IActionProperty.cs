using Assets.Scripts.Define;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Gameplay.Battle.Timeline
{
    public interface IBaseActionProperty
    {
        /// <summary>
        /// 获取的是自己，表示的是Component 所属于哪个entity
        /// </summary>
        /// <returns></returns>
        BaseEntity GetOwner();

        /// <summary>
        ///  enity 的所属于哪个父enity 的ID
        /// </summary>
        /// <returns></returns>
        uint GetEntityFatherOwnerID();


        bool IsMain();
        Transform GetRootTransform();
        PerceptronTarget GetPerceptronTarget();

        ulong EntityId();
         uint GetItemID();

        KHeroType GetHeroType();

        void SetOnUpdateEnd(Action action);

        void SetCurrSkillId(int currSkillId);

        void SetSkillBttonIsDown(bool isdown);
         bool GetSkillBttonIsDown();

        void SetChangeGuideSkillTargetAction(Action action);
        void SendEvent(ComponentEventName eventName, IComponentEvent entityEvent);

        HeroState GetCurrentState();
    }

    public interface IMoveActionProperty
    {
        void SetMaxSpeed(float maxSpeed);
        void SetSpeed(float speed);

        float GetSpeed();
        void SetDirection(Vector3 direction);
    }

    public interface IBindNodeActionProperty
    {
        SpacecraftPresentation GetPresentation();
    }
}
