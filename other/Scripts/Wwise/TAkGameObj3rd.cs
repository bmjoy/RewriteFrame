/*===============================
 * Author: [Allen]
 * Purpose: 第三人称摄像机专用 声音物体类，
 * 跟AkAudioListener  组合使用
 * Time: 2019/5/8 12:02:56
================================*/

#if !(UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.
using UnityEngine;

namespace AK.Wwise
{
    [AddComponentMenu("Wwise/TAkGameOb3rd 第三人称摄像机专用声音物体")]
    [ExecuteInEditMode] // 需要通过 ExecuteInEditMode 来正确维护 isStaticObject 的状态。
    public class TAkGameObj3rd : AkGameObj
    {
        /// <summary>
        /// 目标（主角）
        /// 指定此镜头所要跟随的位置。用户可在 Inspector 中将其指定为玩家角色的 Unity gameObject
        /// </summary>
        private Transform target = null;

        /// <summary>
        /// 是否作用到 目标上
        /// </summary>
        private bool useToTarget = false;

        /// <summary>
        /// 目标高度
        /// </summary>
        private Vector3 m_targetHeightV3 = Vector3.zero;
        /// <summary>
        /// 获取目标
        /// </summary>
        /// <param name="target"></param>
        public Transform GetTarget()
        {
            return target;
        }

        /// <summary>
        /// 设置目标
        /// </summary>
        /// <param name="target"></param>
        public void SetTarget(Transform target)
        {
            m_targetHeightV3 = Vector3.zero;
            this.target = target;
            CapsuleCollider collider = target.GetComponentInChildren<CapsuleCollider>();
            if(collider != null)
            {
               float targetHeight = target.localScale.y * collider.height;
                m_targetHeightV3 = new Vector3(0, targetHeight, 0);
            }
        }

        /// <summary>
        /// 获取目标是否作为耳朵有效
        /// </summary>
        public bool GetUseToTarget()
        {
            return useToTarget;
        }

        /// <summary>
        /// 设置目标是否作为耳朵有效
        /// </summary>
        /// <param name="use"></param>
        public void BeUseToTarget(bool use)
        {
            useToTarget = use;
        }


        /// <summary>
        /// 将镜头位置设为玩家位置，据此处理距离衰减
        /// </summary>
        /// <returns></returns>
        public override Vector3 GetPosition()
        {
            if (target == null || !useToTarget)
            {
                return base.GetPosition();
            }
            return target.position + m_targetHeightV3;
        }

//         public override Vector3 GetForward()
//         {
//             if (target == null || !useToTarget)
//             {
//                 return base.GetForward();
//             }
//             return target.forward;
//         }
// 
//         public override Vector3 GetUpward()
//         {
//             if (target == null || !useToTarget)
//             {
//                 return base.GetUpward();
//             }
//             return target.up;
//         }
    }
}
#endif // #if !(UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.