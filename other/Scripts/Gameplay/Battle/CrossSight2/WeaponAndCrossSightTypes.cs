/*===============================
 * Author: [Allen]
 * Purpose: WeaponAndCrossSightFactory
 * Time: 2020/2/28 3:24:20
================================*/
using Target = CCrossSightLoic.Target;
using Eternity.FlatBuffer;
using UnityEngine;
using System.Collections.Generic;
using FlatBuffers;

public static class WeaponAndCrossSightFactory
{
    /// <summary>
    /// 创建武器
    /// </summary>
    /// <param name="crosssightinfo"></param>
    public static WeaponAndCrossSight CreatTWeapon(ulong uId, uint tId, ulong skillId ,WeaponAndCrossSight.WeaponAndCrossSightTypes type)
    {
        WeaponAndCrossSight result = null;
        if (type == WeaponAndCrossSight.WeaponAndCrossSightTypes.MachineGun) //速射抢
        {
            result = new WeaponAndCrossSight_MachineGun( uId,  tId, skillId);
            result.Init();
        }

        if (type == WeaponAndCrossSight.WeaponAndCrossSightTypes.Missile)//导弹
        {
            result = new WeaponAndCrossSight_Missile(uId, tId, skillId);
            result.Init();
        }

        if (type == WeaponAndCrossSight.WeaponAndCrossSightTypes.ShotGun)//导弹
        {
            result = new WeaponAndCrossSight_ShotGun(uId, tId, skillId);
            result.Init();
        }

        if (type == WeaponAndCrossSight.WeaponAndCrossSightTypes.Reformer)//转化炉
        {
            result = new WeaponAndCrossSight_Reformer(uId, tId, skillId);
            result.Init();
        }

        if (type == WeaponAndCrossSight.WeaponAndCrossSightTypes.Mining)//矿枪, 采用持续引导激光类型
        {
            result = new WeaponAndCrossSight_Mining(uId, tId, skillId);
            result.Init();
        }

        return result;
    }

    /// <summary>
    /// 获得射击间隔
    /// </summary>
    /// <returns></returns>
    public static float CalculateFireInterval(SpacecraftEntity entity,uint TypeDateSheetld, ulong weaponUid ,WeaponAndCrossSight.WeaponAndCrossSightTypes type)
    {
        if (entity == null)
            return 0;

        int minuteBulletNumber = 0;
        float fir = 0f;

        CfgEternityProxy  cfgEternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;

        switch (type)
        {
            case WeaponAndCrossSight.WeaponAndCrossSightTypes.MachineGun:
                {
                WeaponRapidFirer WData = cfgEternityProxy.GetWeaponDataOfMachineGun(TypeDateSheetld);
                minuteBulletNumber = WData.MinuteBulletNumber;
                }
                break;
            case WeaponAndCrossSight.WeaponAndCrossSightTypes.Missile:
                {
                    WeaponMissile WData = cfgEternityProxy.GetWeaponDataOfMissile(TypeDateSheetld);
                    minuteBulletNumber = WData.MinuteBulletNumber;
                }
                break;
            case WeaponAndCrossSight.WeaponAndCrossSightTypes.ShotGun:
                {
                    WeaponShotgun WData = cfgEternityProxy.GetWeaponDataOfShotgun(TypeDateSheetld);
                    minuteBulletNumber = WData.MinuteBulletNumber;
                }
                break;
            case WeaponAndCrossSight.WeaponAndCrossSightTypes.Mining:
                {
                    WeaponMiningLaser WData = cfgEternityProxy.GetWeaponDataOfMining(TypeDateSheetld);
                    minuteBulletNumber = WData.MinuteBulletNumber;
                }
                break;
            case WeaponAndCrossSight.WeaponAndCrossSightTypes.Null:
            default:
                return 0f;
        }



        // 当武器射速属性 >= 0，使用1号武器射击间隔公式
        // 当武器射速属性 < 0，使用2号武器射击间隔公式

        // 1号武器射击间隔公式
        // （ 60 / A） /（ 1 + B / 100）*1000

        // 2号武器射击间隔公式
        // ABS（（ 60 / A） *（ 1 + B / 100 - 2）*1000）

        // A：1分钟子弹数
        // B：武器射速

        // 备注：		
        // 	武器射击间隔公式最终数值为整数四舍五入
        // 	最终单位数值为毫秒

        float weaponFireSpeed = entity.GetWeaponAttribute(weaponUid, crucis.attributepipeline.attribenum.AttributeName.kLightHeatCastSpeed);
        if (weaponFireSpeed >= 0)
        {
            fir = 60f / minuteBulletNumber / (1 + weaponFireSpeed / 100f);
        }
        else
        {
            fir = Mathf.Abs(60f / minuteBulletNumber * (1 + weaponFireSpeed / 100f - 2f));
        }

        return fir;
    }
}


/// <summary>
/// 武器基类
/// </summary>
public class WeaponAndCrossSight
{
    /// <summary>
    /// 武器类型
    /// </summary>
    public enum WeaponAndCrossSightTypes
    {
        /// <summary>
        /// 无
        /// </summary>
        Null,

        /// <summary>
        /// 机关枪
        /// </summary>
        MachineGun = 1,

        /// <summary>
        /// 导弹
        /// </summary>
        Missile = 2,

        /// <summary>
        /// 散弹枪
        /// </summary>
        ShotGun = 3,

        /// <summary>
        /// 转化炉
        /// </summary>
        Reformer = 4,

        /// <summary>
        /// 矿枪, 采用持续引导激光类型
        /// </summary>
        Mining = 7,
    }

    /// <summary>
    /// 开火状态
    /// </summary>
    protected enum GunState
    {
        Fire,
        Stop,
    }


    protected CfgEternityProxy m_CfgEternityProxy;
    protected SpacecraftEntity m_MainPlayer;
    protected PlayerSkillProxy m_PlayerSkillProxy;
    protected GameplayProxy m_GameplayProxy;


    //武器表配置
    protected Weapon m_WeaponTable;

    //武器UID
    protected ulong m_UId;

    //准星检测
    protected CrossSightInfo m_CrossSightInfo;
    // 准星逻辑
    protected CCrossSightLoic m_CrossSightLoic;


    /// <summary>
    /// 热键按下---开始开火的时间
    /// </summary>
    protected float m_TimeOfStartFire = 0;

    /// <summary>
    ///热键抬起-- 停止射击的时间. Time.time
    /// </summary>
    protected float m_TimeOfStopFire = 0;

    /// <summary>
    /// 开火状态
    /// </summary>
    protected GunState m_State = GunState.Stop;


    /// <summary>
    /// 准星目标列表
    /// </summary>
    protected List<Target> m_Targets = new List<Target>();

    /// <summary>
    /// 准星目标列表
    /// </summary>
    private List<ulong> m_OldTargets = new List<ulong>();

    protected ulong m_SkillID;

    /// <summary>
    /// 技能可以释放的最大距离
    /// </summary>
    protected float m_SkillMaxDistance = 0;

    /// <summary>
    /// 发射间隔
    /// </summary>
    protected float m_FireInterval = 0;


    public WeaponAndCrossSight(ulong uId, uint tId, ulong skillId)
    {
        m_CfgEternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
        m_PlayerSkillProxy = GameFacade.Instance.RetrieveProxy(ProxyName.PlayerSkillProxy) as PlayerSkillProxy;
        m_GameplayProxy = GameFacade.Instance.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;

        m_UId = uId;
        m_SkillID = skillId;
        m_WeaponTable = m_CfgEternityProxy.GetWeapon(tId);
    }

    /// <summary>
    /// 初始化
    /// </summary>
    public virtual void Init()
    {
        m_MainPlayer = m_GameplayProxy.GetMainPlayer();
        SkillData skillData = m_CfgEternityProxy.GetSkillData((int)m_SkillID);
        m_SkillMaxDistance = skillData.BaseData.Value.MaxDistance;//* SceneController.SPACE_ACCURACY_TO_CLIENT;
    }

    public ulong GetUId()
    {
        return m_UId;
    }

    /// <summary>
    /// 是否是普通武器 or 转化炉武器
    /// </summary>
    public virtual bool IsOrdinary()
    {
        return true;
    }

    /// <summary>
    /// 是否满足释放条件
    /// </summary>
    /// <returns></returns>
    public virtual bool IsCanRelease()
    {
        return m_PlayerSkillProxy.CanWeaponRelease(m_UId);
    }

    /// <summary>
    /// 武器响应按键
    /// </summary>
    /// <param name="skillHotkey"></param>
    public  void OnHotKey(SkillHotkey skillHotkey)
    {
       // Leyoutech.Utility.DebugUtility.Log("武器", "准星---OnHotKey--> 状态" + skillHotkey.ActionPhase);

        bool press = skillHotkey.ActionPhase == HotkeyPhase.Started;
        if (press && IsCanRelease())
            OnHotKeyDown((int)m_SkillID);
        else
            OnHotKeyUp((int)m_SkillID);
    }

    /// <summary>
    /// 热键按下
    /// </summary>
    public virtual void OnHotKeyDown(int skillindex)
    {
        m_TimeOfStartFire = Time.time;
        m_State = GunState.Fire;
        m_MainPlayer.SetCanToggleWeapon(false);//不能切换武器
    }

    /// <summary>
    /// 热键抬起
    /// </summary>
    public virtual void OnHotKeyUp(int skillindex)
    {
        m_TimeOfStopFire = Time.time;
        m_State = GunState.Stop;
    }

    /// <summary>
    /// 通知武器准星Size变化
    /// </summary>
    public virtual void ChangeCrossSightSize()
    {
    }


    /// <summary>
    /// 整体逻辑刷新
    /// </summary>
    /// <param name="delta"></param>
    public  void OnUpdate(float delta)
    {
        //武器逻辑刷新
        OnUpdate_First( delta);

        //Hud 准星框大小变化
        MessageToTheChangeOfCrosShairSize( delta);

        //准星范围检测逻辑刷新
        CCrossSightLoicUpdate( delta);

        //筛选目标列表
        FiltrateTheTargets(delta);
    }

    /// <summary>
    /// 首先被刷新的函数
    /// </summary>
    /// <param name="delta"></param>
    protected virtual void OnUpdate_First(float delta)
    {
    }

    /// <summary>
    /// 通知Hud准星大小变化情况
    /// </summary>
    /// <param name="delta"></param>
    protected virtual void MessageToTheChangeOfCrosShairSize(float delta)
    {
        Camera cam = CameraManager.GetInstance()?.GetMainCamereComponent()?.GetCamera();
        if (cam != null)
        {
            MsgPlayerWeaponCrosshair crosshairOffset = GetRelativeHeightOfReticle();
            GameFacade.Instance.SendNotification(NotificationName.PlayerWeaponCrosshairScale, crosshairOffset);
        }
    }

    /// <summary>
    /// 得到当前准星，水平/垂直相扩散对比例信息
    /// </summary>
    protected virtual MsgPlayerWeaponCrosshair GetRelativeHeightOfReticle()
    {
        MsgPlayerWeaponCrosshair crosshair = new MsgPlayerWeaponCrosshair();
        return crosshair;
    }

    /// <summary>
    /// 准星目标检测
    /// </summary>
    /// <param name="delta"></param>
    private void CCrossSightLoicUpdate(float delta)
    {
        if (m_CrossSightLoic == null)
            return;

        m_Targets.Clear();
        m_Targets.AddRange(m_CrossSightLoic.GetTarget());


        //对比刷新
        List<ulong> OldTargetstf = new List<ulong>();
        foreach (var item in m_Targets)
        {
            if (m_OldTargets.Contains(item.target_entityId))
            {
                m_OldTargets.Remove(item.target_entityId);
            }
            else
            {
                //临时
                //                 item.target_tf.GetChild(0).gameObject.SetActive(true);      //红色
                //                 item.target_tf.GetChild(1).gameObject.SetActive(false);
            }
            OldTargetstf.Add(item.target_entityId);
        }

        foreach (var item1 in m_OldTargets)
        {
            //临时
            //             item1.GetChild(0).gameObject.SetActive(false);      //绿色
            //             item1.GetChild(1).gameObject.SetActive(true);
        }
        m_OldTargets.Clear();
        m_OldTargets.AddRange(OldTargetstf);
    }

    /// <summary>
    /// 筛选目标列表
    /// </summary>
    protected virtual void FiltrateTheTargets(float delta)
    {
    }

    /// <summary>
    ///  获取弹道上的目标，并返回弹道方向--用于计算偏移后的弹道
    /// </summary>
    public void GetBallisticTargetAndOutDirection(out List<Target> targets, out Vector3 direction, params object[] args)
    {
        targets = null;
        direction = CameraManager.GetInstance().GetMainCamereComponent().GetForward();
        if (m_CrossSightLoic != null)
        {
            m_CrossSightLoic.GetBallisticTargetAndOutDirection(out targets, out direction, args);
        }
    }

    /// <summary>
    /// 获取当前准星内的目标列表
    /// </summary>
    /// <returns></returns>
    public virtual List<Target> GetTargets()
    {
        return m_Targets;
    }


    /// <summary>
    /// 通知此把武器的技能释放结束，清除临时变量
    /// </summary>
    /// <param name="skillId"></param>
    public virtual void WeaponSkillFinish()
    {
        ClearVariate();
        m_MainPlayer.SetCanToggleWeapon(true);//能切换武器
    }

    /// <summary>
    /// 释放
    /// </summary>
    public virtual void OnRelease()
    {
        m_CrossSightInfo = null;
        ClearVariate();
        if (m_CrossSightLoic != null)
            m_CrossSightLoic.Release();
    }


    /// <summary>
    /// 清除变量
    /// </summary>
    private void ClearVariate()
    {
        m_TimeOfStartFire = 0;
        m_TimeOfStopFire = 0;
        m_State = GunState.Stop;
        m_Targets.Clear();
        m_OldTargets.Clear();
    }
}