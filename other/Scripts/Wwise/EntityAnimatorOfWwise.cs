/*===============================
 * Author: [Allen]
 * Purpose: 角色动作帧音效
 * Time: 2019/5/30 19:43:18
================================*/
using Assets.Scripts.Define;
using UnityEngine;

namespace AK.Wwise
{
    public class EntityAnimatorOfWwise : MonoBehaviour
    {
        /// <summary>
        /// 是否是主角
        /// </summary>
        private bool IsProtagonist = false;

        /// <summary>
        /// 速度
        /// </summary>
        private float Velocity = 0;

        /// <summary>
        /// 转向
        /// </summary>
        private float Turn = 0;

        /// <summary>
        /// 类型
        /// </summary>
        private KHeroType m_KHeroType;

        /// <summary>
        /// 组合ID
        /// </summary>
        private int m_ComboID = -1;


        /// <summary>
        /// 设置组合ID
        /// </summary>
        /// <param name="ComboID"></param>
        public void SetComboID(int ComboID)
        {
            m_ComboID = ComboID;
        }

        /// <summary>
        /// 设置主角
        /// </summary>
        /// <param name="value"></param>
        public void SetIsProtagonist(bool value, KHeroType KHeroType)
        {
            IsProtagonist = value;
            m_KHeroType = KHeroType;
        }

        /// <summary>
        /// 设置转向，速度
        /// </summary>
        /// <param name="velocity"></param>
        /// <param name="turn"></param>
        public void SetVelocityAndTurn(float velocity , float turn )
        {
            Velocity = velocity;
            Turn = turn;
        }

        /// <summary>
        /// 人形态跑步
        /// </summary>
        /// <param name="str"></param>
        public void HumRun_Foot(string str)
        {
            if (m_KHeroType != KHeroType.htPlayer)
            {
                return;
            };

            switch (str)
            {
                case "Run":
                    {
                        if ( Mathf.Abs(Velocity) <0.1f && Mathf.Abs(Turn) <0.1f)
                        {
                        }
                        else
                        {
                            if (m_ComboID <= 0)
                                break;
                            WwiseUtil.PlaySound(m_ComboID, WwiseMusicSpecialType.SpecialType_RoleFootMaterial_4,
                                WwiseMusicPalce.Palce_1st,
                                //IsProtagonist? WwiseMusicPalce.Palce_1st : WwiseMusicPalce.Palce_3st,
                                false, transform);
                        }
                    }
                    break;
                case "Stop":
                    {
                        if(m_ComboID <=0)
                            break; 
                        WwiseUtil.PlaySound(m_ComboID, WwiseMusicSpecialType.SpecialType_RoleFootStop,
                            WwiseMusicPalce.Palce_1st,
                            //IsProtagonist? WwiseMusicPalce.Palce_1st : WwiseMusicPalce.Palce_3st,
                            false, transform);
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
