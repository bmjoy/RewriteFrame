/*===============================
 * Author: [Allen]
 * Purpose: 导弹
 * Time: 2020/3/4 12:11:12
================================*/
using System.Collections.Generic;
using Eternity.FlatBuffer;
using UnityEngine;
using Target = CCrossSightLoic.Target;

public class WeaponAndCrossSight_Missile : WeaponAndCrossSight
{
    /// <summary>
    /// 目标锁定状态
    /// </summary>
    public enum TargetLockState
    {
        /// <summary>
        /// 正在锁定. 转圈
        /// </summary>
        Locking,
        /// <summary>
        /// 已经锁定了
        /// </summary>
        Locked
    }

    /// <summary>
    /// 目标锁定信息
    /// </summary>
    public class TargetLockInfo
    {
        /// <summary>
        /// 锁定状态. 正在锁定, 已经锁定
        /// </summary>
        public TargetLockState LockState;
        /// <summary>
        /// 锁定层数. 每有一层对目标发射一次导弹
        /// </summary>
        public int LockTimes;
        /// <summary>
        /// 剩余锁定时间
        /// </summary>
        public float LockTimeRemaining;

        /// <summary>
        /// UI用
        /// </summary>
        public float LockTimePercent;
    }


    /// <summary>
    /// 导弹武器专有属性
    /// </summary>
    private WeaponMissile m_WeaponFireData;

    /// <summary>
    /// 锁定一个目标需要的时间. 秒
    /// </summary>
    private float m_LockonTimeParam;

    /// <summary>
    /// 单次攻击最大导弹数量
    /// </summary>
    private int m_MaxMissileCountInOneShotParam;

    /// <summary>
    ///  当前虚拟摄像机的水平扩散角度
    /// </summary>
    private float m_ReticleHorizontalFOVParam;

    /// <summary>
    /// 长宽比
    /// </summary>
    private float m_ReticleAspect;

    /// <summary>
    /// 目标锁定情况容器
    /// </summary>
    private Dictionary<ulong, TargetLockInfo> m_LockTargeMap = new Dictionary<ulong, TargetLockInfo>();

    /// <summary>
    /// 技能需要的结构,锁定目标列表
    /// </summary>
    protected List<Target> m_LockedTargetList = new List<Target>();


    public WeaponAndCrossSight_Missile(ulong uid, uint tId, ulong skillId) : base(uid, tId, skillId)
    {
    }

    public override void Init()
    {
        base.Init();
        m_WeaponFireData = m_CfgEternityProxy.GetWeaponDataOfMissile(m_WeaponTable.TypeDateSheetld);

        m_CrossSightInfo = new CrossSightInfo();
        m_CrossSightInfo.m_MaxRayDistance = m_SkillMaxDistance ;
        m_CrossSightInfo.m_CrossSightShape = CrossSightShape.ConeSquare;
        m_CrossSightLoic = new CCrossSightLoic();
        m_CrossSightLoic = new CrossSightLoic_Improve_ConeSquare(m_CrossSightLoic);
        m_CrossSightLoic.SetCrossSightInfo(m_CrossSightInfo);

        float accuracy = m_MainPlayer.GetWeaponAttribute(m_UId, crucis.attributepipeline.attribenum.AttributeName.kWeaponAccuracy);//精准度
        float stability = m_MainPlayer.GetWeaponAttribute(m_UId, crucis.attributepipeline.attribenum.AttributeName.kWeaponStability);//稳定性
        // 表里配置的时间是毫秒
        m_LockonTimeParam = (m_WeaponFireData.LockTime - accuracy * m_WeaponFireData.TimeCoefficient) / 1000f;
        m_MaxMissileCountInOneShotParam = (int)(m_WeaponFireData.MagazineNumber + stability * m_WeaponFireData.MagazineQuantityCoefficient);
        m_ReticleHorizontalFOVParam = m_WeaponFireData.AimingSize;
        m_ReticleAspect = m_WeaponFireData.AngleRange;
        if (m_ReticleAspect == 0)
            Debug.LogWarning(string.Format("--------------------> 导弹类武器表 ID= {0}，AngleRange = 0，策划确认下对吗？ ", m_WeaponTable.TypeDateSheetld));
        float fieldOfView = m_ReticleAspect == 0 ? 60 : m_ReticleHorizontalFOVParam / m_ReticleAspect;
        m_CrossSightLoic.ChangeVirtualCameraAttribute(fieldOfView, m_ReticleAspect);


        m_FireInterval = WeaponAndCrossSightFactory.CalculateFireInterval(m_MainPlayer, m_WeaponTable.TypeDateSheetld, m_UId, WeaponAndCrossSight.WeaponAndCrossSightTypes.Missile);

    }

    /// <summary>
    /// 热键按下
    /// </summary>
    public override void OnHotKeyDown(int skillID)
    {
        if (!m_PlayerSkillProxy.CanCurrentWeaponRelease())
        {
            //开火失败
            GameFacade.Instance.SendNotification(NotificationName.PlayerWeaponFireFail);
            return;
        }

        base.OnHotKeyDown(skillID);
        m_LockTargeMap.Clear();
        NetworkManager.Instance.GetSkillController().SendMissileLockTarget(true);

        SkillCastEvent castSkillEvent = new SkillCastEvent();
        castSkillEvent.IsWeaponSkill = true;
        castSkillEvent.SkillIndex = skillID;
        castSkillEvent.KeyPressed = true;
        m_MainPlayer.SendEvent(ComponentEventName.SkillButtonResponse, castSkillEvent);
    }

    public override void OnRelease()
    {
        base.OnRelease();
        m_LockTargeMap.Clear();
        m_LockedTargetList.Clear();
    }

    public override void OnHotKeyUp(int skillID)
    {
        base.OnHotKeyUp(skillID);
        //if (ExistingLockedTarget())
        //{            
            SkillCastEvent castSkillEvent = new SkillCastEvent();
            castSkillEvent.IsWeaponSkill = true;
            castSkillEvent.SkillIndex = skillID;
            castSkillEvent.KeyPressed = false;
            m_MainPlayer.SendEvent(ComponentEventName.SkillButtonResponse, castSkillEvent);
        //}
        //else if (ExistingLockedTarget() && !m_PlayerSkillProxy.CanCurrentWeaponRelease())
        //{
        //    GameFacade.Instance.SendNotification(NotificationName.PlayerWeaponFireFail);
        //}
        //else
        //{
        //    //没有锁定目标, 结束这次攻击流程
        //    WeaponSkillFinish();
        //}
        NetworkManager.Instance.GetSkillController().SendMissileLockTarget(false);
    }

    /// <summary>
    /// 武器技能释放结束
    /// </summary>
    public override void WeaponSkillFinish()
    {
        base.WeaponSkillFinish();
        m_LockTargeMap.Clear();

        // 通知UI当前锁定的所有目标
        PlayerMissileWeaponTargetSelectionInfo targetInfoNotify = MessageSingleton.Get<PlayerMissileWeaponTargetSelectionInfo>();
        targetInfoNotify.TargeList = m_LockTargeMap;
        GameFacade.Instance.SendNotification(NotificationName.PlayerMissileWeaponTargetSelection, targetInfoNotify);
    }


    /// <summary>
    /// 得到当前准星，水平/垂直相扩散对比例信息
    /// </summary>
    protected override MsgPlayerWeaponCrosshair GetRelativeHeightOfReticle()
    {
        MsgPlayerWeaponCrosshair crosshair = new MsgPlayerWeaponCrosshair();
        Camera cam = CameraManager.GetInstance()?.GetMainCamereComponent()?.GetCamera();
        if (cam != null)
        {
            crosshair.HorizontalRelativeHeight = m_ReticleHorizontalFOVParam / (cam.fieldOfView * cam.aspect);
            crosshair.VerticalRelativeHeight = m_ReticleHorizontalFOVParam / m_ReticleAspect / cam.fieldOfView;
            crosshair.MissileCountInOneShot = m_MaxMissileCountInOneShotParam;
            crosshair.MissileLockTime = m_LockonTimeParam;
            return crosshair;
        }
        else
        {
            return base.GetRelativeHeightOfReticle();
        }
    }

    /// <summary>
    /// 筛选目标列表
    /// </summary>
    protected override void FiltrateTheTargets(float delta)
    {
        m_LockedTargetList.Clear();
        if (m_State == GunState.Fire)
        {
            BaseEntity fentiy = GetForwardEntity();//屏幕中心单位
            if (IsThereSurplusMissile()&& fentiy!= null )
            {
                TryToLockThisTarget(fentiy.EntityId());
            }
            //清除不在虚拟相机内的单位
            ClearAlreadyNoInVirtualCameraTargets();

            //更新锁定中的单位状态
            UpdateTheLocking(delta);

            //设置技能需要的目标列表结构数据
            SetSkillTargets();
        }
        // 通知UI当前锁定的所有目标
        PlayerMissileWeaponTargetSelectionInfo targetInfoNotify = MessageSingleton.Get<PlayerMissileWeaponTargetSelectionInfo>();
        targetInfoNotify.TargeList = m_LockTargeMap;
        GameFacade.Instance.SendNotification(NotificationName.PlayerMissileWeaponTargetSelection, targetInfoNotify);
    }

    /// <summary>
    /// 获取锁定的目标列表
    /// </summary>
    /// <returns></returns>
    public override List<Target> GetTargets()
    {
        return m_LockedTargetList;
    }


    /// <summary>
    /// 正前方中心点射线选中的目标
    /// </summary>
    /// <returns></returns>
    private BaseEntity GetForwardEntity()
    {
        MainCameraComponent mainCam = CameraManager.GetInstance().GetMainCamereComponent();
        Ray ray = new Ray(mainCam.GetPosition(), mainCam.GetForward());
        RaycastHit hitInfo = new RaycastHit();
        if (Physics.Raycast(ray, out hitInfo, m_SkillMaxDistance, LayerUtil.GetLayersIntersectWithSkillProjectile(true)))
        {
            return hitInfo.collider.attachedRigidbody?.GetComponent<BaseEntity>();
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// 本此攻击流程中是否还有富余的弹药以供锁定更多的目标
    /// </summary>
    /// <returns></returns>
    private bool IsThereSurplusMissile()
    {
        int lockCount = 0;

        foreach (var pair in m_LockTargeMap)
        {
            TargetLockInfo lockInfo = pair.Value;
            lockCount += lockInfo.LockTimes;
        }

        return m_MaxMissileCountInOneShotParam > lockCount;
    }


    /// <summary>
    /// 是否存在已经锁定的目标
    /// </summary>
    /// <returns></returns>
    private bool ExistingLockedTarget()
    {
        // 剔除所有正在锁定的目标. 只留下已经锁定的
        int count = 0;
        foreach (var pair in m_LockTargeMap)
        {
            TargetLockInfo lockInfo = pair.Value;
            if (lockInfo.LockState == TargetLockState.Locked || lockInfo.LockTimes >= 1)
                count++;
        }
        return count > 0;
    }

    /// <summary>
    /// 试图保存目标
    /// </summary>
    /// <param name="newTarget"></param>
    private void TryToLockThisTarget(ulong newTargetId)
    {
        BaseEntity newTarget = GameplayManager.Instance.GetEntityManager().GetEntityById<BaseEntity>((uint)newTargetId);

        if (newTarget == null || !m_GameplayProxy.CanAttackToTarget(m_MainPlayer, (SpacecraftEntity)newTarget))
        {
            return;
        }

        // 新的目标有两种可能. 1. 不在当前目标列表里面 且 不为空, 2. 在目标列表里, 但是此目标已经锁定完成, 可以进行下一轮锁定
        if (!m_LockTargeMap.ContainsKey(newTargetId))
        {
            TargetLockInfo targetLockInfo = new TargetLockInfo();
            targetLockInfo.LockState = TargetLockState.Locking;
            targetLockInfo.LockTimeRemaining = m_LockonTimeParam;
            targetLockInfo.LockTimes = 0;
            m_LockTargeMap.Add(newTargetId, targetLockInfo);

            Leyoutech.Utility.DebugUtility.LogWarning("武器", string.Format("锁定新目标 {0}",newTarget.name));
        }
        else if (IsLockedEntity(newTargetId))
        {
            m_LockTargeMap[newTargetId].LockTimeRemaining = m_LockonTimeParam;
            m_LockTargeMap[newTargetId].LockState = TargetLockState.Locking;

            Leyoutech.Utility.DebugUtility.LogWarning("武器", string.Format("锁定老目标，为其增加一层锁定层数. {0}", newTarget.name));
        }
        else
        {
            //SLog("目标正在锁定中, 却作为新目标被选中了. {0}", newTarget.name);
        }
    }

    /// <summary>
    /// 这个目标是否已经锁定完成一层
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    private bool IsLockedEntity(ulong entityId)
    {
        if (m_LockTargeMap.ContainsKey(entityId))
        {
            return m_LockTargeMap[entityId].LockState == TargetLockState.Locked;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 清除已经不再虚拟相机里的单位
    /// </summary>
    private void ClearAlreadyNoInVirtualCameraTargets()
    {
        List<ulong> currTargetList = new List<ulong>();
        for (int i = 0; i < m_Targets.Count; i++)
        {
            Target target = m_Targets[i];
            currTargetList.Add(target.target_entityId);
        }

        List<ulong> cancelLockList = new List<ulong>();
        foreach (var pair in m_LockTargeMap)
        {
            ulong lockTarget = pair.Key;
            TargetLockInfo lockInfo = pair.Value;
            // 正在锁定的目标如果已经不在框中, 就取消锁定
            if (lockInfo.LockState == TargetLockState.Locking && !currTargetList.Contains(lockTarget))
            {
                cancelLockList.Add(lockTarget);
            }
        }

        for (int iEntity = 0; iEntity < cancelLockList.Count; iEntity++)
        {
            ulong entity = cancelLockList[iEntity];
            if (m_LockTargeMap[entity].LockTimes == 0)
            {
                m_LockTargeMap.Remove(entity);
                Leyoutech.Utility.DebugUtility.LogWarning("武器", string.Format("目标在锁定完成前出框了, 失去锁定. entityID = {0}", entity));
            }
            else
            {
                m_LockTargeMap[entity].LockState = TargetLockState.Locked;
                m_LockTargeMap[entity].LockTimeRemaining = 0;
            }
        }
    }

    /// <summary>
    /// 更新锁定中，转圈单位
    /// </summary>
    private void UpdateTheLocking(float delta)
    {
        List<ulong> cancelLockList = new List<ulong>();
        foreach (var pair in m_LockTargeMap)
        {
            BaseEntity targetEntity = GameplayManager.Instance.GetEntityManager().GetEntityById<BaseEntity>((uint)pair.Key);

            // UNDONE, 检查所有目标是不是已经超出了射程或者已经死了. 如果是的话, 就解除锁定
            if (targetEntity == null || targetEntity.IsDead())
            {
                cancelLockList.Add(pair.Key);
                continue;
            }

            TargetLockInfo targetLockInfo = pair.Value;
            if (targetLockInfo.LockState == TargetLockState.Locking && IsThereSurplusMissile())
            {
                if (IsThereSurplusMissile()) //还不够
                {
                    if (targetLockInfo.LockTimeRemaining <= 0f)
                    {
                        targetLockInfo.LockState = TargetLockState.Locked;
                        targetLockInfo.LockTimeRemaining = 0f;
                        targetLockInfo.LockTimes++;

                        Leyoutech.Utility.DebugUtility.LogWarning("武器", string.Format("目标锁定完成. EntityId = {0}", pair.Key));
                    }
                    else
                    {
                        targetLockInfo.LockTimeRemaining -= delta;
                    }
                }
                else //够了,将未完成转圈的
                {
                    if (targetLockInfo.LockTimes == 0)
                    {
                        cancelLockList.Add(pair.Key);
                    }
                    else
                    {
                        targetLockInfo.LockState = TargetLockState.Locked;
                        targetLockInfo.LockTimeRemaining = 0;
                    }
                }
            }
        }

        // 对于那些已经不可用的目标解除锁定
        for (int iEntity = 0; iEntity < cancelLockList.Count; iEntity++)
        {
            m_LockTargeMap.Remove(cancelLockList[iEntity]);
            Leyoutech.Utility.DebugUtility.LogWarning("武器", string.Format("目标因为死亡或者超出视野而失去锁定. {0}", iEntity));
        }
    }

    /// <summary>
    /// 设置技能需要的目标列表结构数据
    /// </summary>
    private void SetSkillTargets()
    {
        foreach (var item in m_LockTargeMap)
        {
            TargetLockInfo lockInfo = item.Value;
            BaseEntity entity = GameplayManager.Instance.GetEntityManager().GetEntityById<BaseEntity>((uint)item.Key);
            if (entity == null || entity.IsDead())
            {
                continue;
            }

            for (int i = 0; i < lockInfo.LockTimes; i++)
            {
                Target locktaget = new Target();
                locktaget.target_entityId = item.Key;
                locktaget.target_pos = entity.GetRootTransform().position;
                m_LockedTargetList.Add(locktaget);
            }
        }
    }
}