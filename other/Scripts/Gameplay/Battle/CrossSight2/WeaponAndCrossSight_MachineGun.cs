/*===============================
 * Author: [Allen]
 * Purpose: 机关枪
 * Time: 2020/2/28 3:41:56
================================*/
using Eternity.FlatBuffer;
using UnityEngine;

public class WeaponAndCrossSight_MachineGun : WeaponAndCrossSight
{
    /// <summary>
    /// 速射枪武器专有属性
    /// </summary>
    private WeaponRapidFirer m_WeaponFireData;

    /// <summary>
    /// 虚拟的"子弹数"来控制准星大小
    /// </summary>
    private float m_CurrentBulletCount = 0;

    /// <summary>
    /// 当前的水平/垂直扩散角度
    /// </summary>
    private float m_CurrentHorizontalAngle;
    private float m_CurrentVerticalAngle;
    //长宽比
    private float m_Aspect;

    /// <summary>
    /// 武器精准度
    /// </summary>
    private float m_Accuracy;
    /// <summary>
    /// 武器稳定性
    /// </summary>
    private float m_Stability;

    /// <summary>
    /// 上一次减少虚拟子弹数的时间
    /// </summary>
    private float m_TimeOfDecreaseLastBullet =0;

    /// <summary>
    /// 实际发射上一颗子弹的时间
    /// </summary>
    float m_TimeOfFiredLastBullet = 0;

    /// <summary>
    /// 这次按键的按下, 已经释放过技能了
    /// </summary>
    private bool m_FiredDuringThisPress = false;



    public WeaponAndCrossSight_MachineGun(ulong uid, uint tId, ulong skillId) : base(uid,tId, skillId)
    {
    }

    public override void Init()
    {
        base.Init();
        m_WeaponFireData = m_CfgEternityProxy.GetWeaponDataOfMachineGun(m_WeaponTable.TypeDateSheetld);
        m_CrossSightInfo = new CrossSightInfo();
        m_CrossSightInfo.m_MaxRayDistance = m_SkillMaxDistance;
        m_CrossSightInfo.m_CrossSightShape = CrossSightShape.ConeSquare;
        m_CrossSightInfo.m_ConeSquareInfo.detectionMode = DetectionMode.All;
        m_CrossSightLoic = new CCrossSightLoic();
        m_CrossSightLoic = new CrossSightLoic_Improve_ConeSquare(m_CrossSightLoic);
        m_CrossSightLoic.SetCrossSightInfo(m_CrossSightInfo);
        m_CrossSightLoic.SetBallisticOffsetFunc(BallisticOffsetFunc, null);


        m_CurrentBulletCount = 0;//子弹数
        m_Accuracy = m_MainPlayer.GetWeaponAttribute(m_UId, crucis.attributepipeline.attribenum.AttributeName.kWeaponAccuracy);//精准度
        m_Stability = m_MainPlayer.GetWeaponAttribute(m_UId, crucis.attributepipeline.attribenum.AttributeName.kWeaponStability);//稳定性
        m_Aspect = m_WeaponFireData.AngleRatio;


        m_CurrentHorizontalAngle = GetHorizontalAngle_Before();
        m_CurrentVerticalAngle = m_Aspect == 0 ? m_CurrentHorizontalAngle : m_CurrentHorizontalAngle/ m_Aspect;
        m_CrossSightLoic.ChangeVirtualCameraAttribute(m_CurrentHorizontalAngle, m_Aspect);

        m_FireInterval = WeaponAndCrossSightFactory.CalculateFireInterval(m_MainPlayer, m_WeaponTable.TypeDateSheetld, m_UId, WeaponAndCrossSight.WeaponAndCrossSightTypes.MachineGun);

    }


    /// <summary>
    /// 射击前瞄准框大小公式：
    /// </summary>
    /// <returns></returns>
    private float GetHorizontalAngle_Before()
    {
        if (m_CurrentBulletCount == 0)
        {
            // A - B * C
            // A:	机枪默认瞄准框
            // B:	精准度100 % 瞄准框系数
            // C:	精准度
            return m_WeaponFireData.InitialDiffusionAngle - m_WeaponFireData.AccurateAimCoefficient * m_Accuracy;
        }
        else
        {
            // MIN（ A* B, C -D * E)	
            // MIN（射击后瞄准框大小公式* 缩框极限值系数 ，机枪默认瞄准框 - 精准度100 % 瞄准框系数 * 精准度 ）

            // A:	射击后瞄准框大小公式
            // B:	缩框极限值系数
            // C:	机枪默认瞄准框
            // D:	精准度100 % 瞄准框系数
            // E:	精准度

            float post = GetHorizontalAngle_After();
            return Mathf.Min(post * m_WeaponFireData.ShrinkLimitCoefficient
                , m_WeaponFireData.InitialDiffusionAngle - m_WeaponFireData.AccurateAimCoefficient * m_Accuracy);
        }
    }

    /// <summary>
    /// 射击后瞄准框大小公式：
    /// </summary>
    /// <returns></returns>
    private float GetHorizontalAngle_After()
    {
        // MAX（A - （ B* C ）- （（ D* E + D ^ 3 * F ) *(1 + G * C)）, H - （ I * J ））
        // A: 机枪默认瞄准框
        // B: 精准度100 % 瞄准框系数
        // C: 精准度
        // D: 子弹计数
        // E: 缩框固定值系数A
        // F: 缩框系数B
        // G: 精准度缩框加速比例
        // H: 机枪默认缩圈瞄准框
        // I: 稳定度100 % 缩圈瞄准框系数
        // J: 稳定度

        return Mathf.Max(m_WeaponFireData.InitialDiffusionAngle
                    - m_WeaponFireData.AccurateAimCoefficient * m_Accuracy
                    - (m_CurrentBulletCount * m_WeaponFireData.ShrinkFixedCoefficient
                            + Mathf.Pow(m_CurrentBulletCount, 3) * m_WeaponFireData.ShrinkCoefficient)
                        * (1 + m_WeaponFireData.AccurateShrinkAccelerateCoefficient * m_Accuracy)
                , m_WeaponFireData.ShrinkDiffusionAngle - m_WeaponFireData.StableAimCoefficient * m_Stability);
    }


    /// <summary>
    /// 子弹计数增加
    /// </summary>
    private void BulletCountAdd()
    {
        // IF（A <= （ B - C * D )  ， E , E + 1）
        // A: 射击后瞄准框大小计算公式
        // B: 机枪默认缩圈瞄准框
        // C: 稳定度100 % 缩圈瞄准框系数
        // D: 稳定度
        // E: 当前子弹计数

        if (m_CurrentHorizontalAngle > m_WeaponFireData.ShrinkDiffusionAngle - m_Stability * m_WeaponFireData.StableAimCoefficient + 0.0001f)
        {
            m_CurrentBulletCount += 1;
        }
    }

    /// <summary>
    /// 子弹计数减少
    /// </summary>
    private void BulletCountMinus()
    {
        // 当玩家停止射击后X(武器射速间隔时长) + Y(缩框回复时间间隔系数，玩家停止射击多少秒后开始缩框回复)后，
        // 每隔Z(子弹计数减少刷新间隔，决定每隔多少时间客户端计算1次子弹计数减少公式)秒后，降低一次子弹计数数量

        // 当玩家停止射击后（X + Y + A * B ） 后，每隔Z秒后，降低一次子弹计数数量

        //  子弹计数减少时间：	
        // 	X + Y + A * B

        // X：	武器射速间隔时长
        // Y：	缩框回复时间间隔系数，玩家停止射击多少秒后开始缩框回复
        // Z：	子弹计数减少刷新间隔，决定每隔多少时间客户端计算1次子弹计数减少公式
        // A：	稳定度
        // B：	稳定度影响停止射击缩框时间间隔

        // 从停火开始计时, 经过这么长时间, 才开始减少子弹数(减少子弹数 = 缩框)
        //子弹计数减少时间：
        float startDecreateBulletSinceStopFire = m_FireInterval + m_WeaponFireData.ShrinkReturnTimeCoefficient / 1000f
                                            + m_Stability * m_WeaponFireData.Ceasefirestableshrinktime / 1000f;

        if (Time.time - m_TimeOfStopFire > startDecreateBulletSinceStopFire) //停火间隔 > 到开始子弹减少执行的间隔时长
        {
            if (Time.time - m_TimeOfDecreaseLastBullet > m_WeaponFireData.BulletReduceTime / 1000f)
            {
                if (m_TimeOfDecreaseLastBullet == 0)
                    m_TimeOfDecreaseLastBullet = m_TimeOfStopFire;
                else
                    m_TimeOfDecreaseLastBullet += m_WeaponFireData.BulletReduceTime / 1000f;

                // 减少子弹数量
                // A -  （（B* C +B ^ 3 * D) -((B - 1) * C + (B - 1) ^ 3 * D))) *(1 - E * F)

                // A:  缩框开始的一瞬间 的子弹数量. 而不是当前的子弹数量
                // B:  缩框次数
                // C:  时间扩框回复固定值系数A
                // D:  时间扩框回复系数B
                // E:  稳定度
                // F:  稳定度100 % 延迟缩框比例

                // 缩框次数 = (当前时间 - 缩框开始时间) / 子弹减少间隔
                float shrinkCount = (Time.time - (m_TimeOfStopFire + startDecreateBulletSinceStopFire))
                                    / (m_WeaponFireData.BulletReduceTime / 1000f);


                if (m_CurrentBulletCount < 0)
                {
                    m_CurrentBulletCount = 0;
                }
                else
                {

                    float decreaseCount = (shrinkCount * m_WeaponFireData.TimeexpandfixedCoefficientA
                                            + Mathf.Pow(shrinkCount, 3) * m_WeaponFireData.TimeexpandCoefficientB
                                            - ((shrinkCount - 1) * m_WeaponFireData.TimeexpandfixedCoefficientA
                                                    + Mathf.Pow(shrinkCount - 1, 3) * m_WeaponFireData.TimeexpandCoefficientB))
                                            * (1 - m_Stability * m_WeaponFireData.StableDelayProportion);
                    m_CurrentBulletCount -= decreaseCount;

                    if (m_CurrentBulletCount < 0)
                    {
                        m_CurrentBulletCount = 0;
                    }
                }
            }
        }
    }

    /// <summary>
    /// 通知武器准星Size变化
    /// </summary>
    public override void ChangeCrossSightSize()
    {
        m_TimeOfFiredLastBullet = Time.time;
        m_CurrentHorizontalAngle = GetHorizontalAngle_After();
        m_CurrentVerticalAngle = m_CurrentHorizontalAngle / m_Aspect;
        BulletCountAdd();

       //Leyoutech.Utility.DebugUtility.Log("武器", "准星---ChangeCrossSightSize-->  m_CurrentBulletCount : " + m_CurrentBulletCount );
    }


    /// <summary>
    /// 热键按下
    /// </summary>
    public override void OnHotKeyDown(int skillID)
    {
        base.OnHotKeyDown(skillID);

        if (!m_FiredDuringThisPress && m_PlayerSkillProxy.CanCurrentWeaponRelease())
        {
            //Leyoutech.Utility.DebugUtility.Log("武器", "热键按下，请求释放技能");

            //执行释放
            SkillCastEvent castSkillEvent = new SkillCastEvent();
            castSkillEvent.IsWeaponSkill = true;
            castSkillEvent.SkillIndex = skillID;
            castSkillEvent.KeyPressed = true;
            m_MainPlayer.SendEvent(ComponentEventName.SkillButtonResponse, castSkillEvent);
            m_FiredDuringThisPress = true;
        }
        else if (!m_FiredDuringThisPress && !m_PlayerSkillProxy.CanCurrentWeaponRelease())
        {
            //开火失败
            GameFacade.Instance.SendNotification(NotificationName.PlayerWeaponFireFail);
        }
        else
        {
            // 已经放过技能了, 不再发送释放技能消息. 解决武器过热以后还会继续射击的问题
        }
    }

    /// <summary>
    /// 热键抬起
    /// </summary>
    /// <param name="skillindex"></param>
    public override void OnHotKeyUp(int skillID)
    {
        base.OnHotKeyUp(skillID);
        //Leyoutech.Utility.DebugUtility.Log("武器", "热键抬起，请求释放技能结束");

        SkillCastEvent castSkillEvent = new SkillCastEvent();
        castSkillEvent.IsWeaponSkill = true;
        castSkillEvent.SkillIndex = skillID;
        castSkillEvent.KeyPressed = false;

        m_MainPlayer.SendEvent(ComponentEventName.SkillButtonResponse, castSkillEvent);
        m_FiredDuringThisPress = false;
    }

    protected override void OnUpdate_First(float delta)
    {
        if (m_State == GunState.Stop)
        {
            float timeSinceStop = Time.time - m_TimeOfStopFire;

            BulletCountMinus();

            if (timeSinceStop < m_FireInterval)
            {
                //不够一次虚拟子弹发射间隔
                float t = timeSinceStop / m_FireInterval;
                float pre = GetHorizontalAngle_Before();
                float post = GetHorizontalAngle_After();
                m_CurrentHorizontalAngle = Mathf.Lerp(post, pre, t);
            }
            else
            {
                //够了一次虚拟子弹发射间隔

                // 优化. 缓存PreFireAngleWhenBullet=0
                float pre = GetHorizontalAngle_Before();
                m_CurrentHorizontalAngle = pre;
            }
        }
        else if (m_State == GunState.Fire)
        {
            float timeSinceLastFiredBullet = Time.time - m_TimeOfFiredLastBullet; //真正的子弹时间间隔
            float t = timeSinceLastFiredBullet > m_FireInterval ? 1 : timeSinceLastFiredBullet / m_FireInterval;
            float pre = GetHorizontalAngle_Before();
            float post = GetHorizontalAngle_After();
            m_CurrentHorizontalAngle = Mathf.Lerp(post, pre, t);

//             Leyoutech.Utility.DebugUtility.Log("武器", "准星---OnUpdate_First-->  射击前 : " + pre + " 射击后 : " + post 
//                 + "  间隔："+ timeSinceLastFiredBullet
//                 +"  t ：" + t
//                 + "m_CurrentBulletCount =" + m_CurrentBulletCount);
        }
    }


    /// <summary>
    /// 得到当前准星，水平/垂直相扩散对比例信息
    /// </summary>
    protected override MsgPlayerWeaponCrosshair GetRelativeHeightOfReticle()
    {
        m_CrossSightLoic.ChangeVirtualCameraAttribute(m_CurrentHorizontalAngle, m_Aspect);

        MsgPlayerWeaponCrosshair crosshair = new MsgPlayerWeaponCrosshair();
        Camera cam = CameraManager.GetInstance()?.GetMainCamereComponent()?.GetCamera();
        if (cam != null)
        {
            crosshair.HorizontalRelativeHeight = m_CurrentHorizontalAngle * 2 / (cam.fieldOfView * cam.aspect);
            crosshair.VerticalRelativeHeight = m_CurrentHorizontalAngle / m_Aspect * 2 / cam.fieldOfView;

            return crosshair;
        }
        else
        {
            return base.GetRelativeHeightOfReticle();
        }
    }

    /// <summary>
    /// 弹道偏移函数委托
    /// </summary>
    /// <param name="offx">计算后x 偏移</param>
    /// <param name="offy">计算后y 偏移</param>
    /// <param name="args">参数</param>
    private void BallisticOffsetFunc(out float offx, out float offy, params object[] args)
    {
        offx = Random.Range(0f, 1.0f);
        offy = Random.Range(0f, 1.0f);
    }


    /// <summary>
    /// 释放
    /// </summary>
    public override void OnRelease()
    {
        base.OnRelease();
        m_CurrentBulletCount = 0;
        m_FireInterval = 0;
        m_TimeOfDecreaseLastBullet = 0;
        m_TimeOfFiredLastBullet = 0;
    }
}

