/*===============================
 * Author: [Allen]
 * Purpose: 散弹枪
 * Time: 2020/3/6 12:13:25
================================*/
using Eternity.FlatBuffer;
using System.Collections.Generic;
using UnityEngine;

public partial class WeaponAndCrossSight_ShotGun : WeaponAndCrossSight
{
    /// <summary>
    /// 武器特有属性
    /// </summary>
    private WeaponShotgun m_WeaponFireData;

    /// <summary>
    /// 区域列表
    /// </summary>
    private List<AimArea> m_AimAreaList = new List<AimArea>();

    /// <summary>
    /// 扩散恢复时间. 秒
    /// </summary>
    private float m_RestoreDuration;


    public WeaponAndCrossSight_ShotGun(ulong uid, uint tId, ulong skillId) : base(uid, tId, skillId)
    {
    }

    public override void Init()
    {
        base.Init();

        m_WeaponFireData = m_CfgEternityProxy.GetWeaponDataOfShotgun(m_WeaponTable.TypeDateSheetld);
        float accuracy = m_MainPlayer.GetWeaponAttribute(m_UId, crucis.attributepipeline.attribenum.AttributeName.kWeaponAccuracy);//精准度
        float stability = m_MainPlayer.GetWeaponAttribute(m_UId, crucis.attributepipeline.attribenum.AttributeName.kWeaponStability);//稳定性
        float m_SonMaxSpreadAngle = m_WeaponFireData.SonDiffusionAngle - accuracy * m_WeaponFireData.SonDiffusionAngleCoefficient;//子区域偏移最大角度
        m_RestoreDuration = (m_WeaponFireData.RecoveryTime - stability * m_WeaponFireData.RecoveryTimeCoefficient) / 1000f;
        int sonCount = (int)m_WeaponFireData.SonNumber;


        AimArea mainAimArea = new AimArea();
        // 中心区域覆盖角度
        float MainAreaCoverAngle = m_WeaponFireData.CoreDiameterAngle - accuracy * m_WeaponFireData.CoreDiameterAngleCoefficient;
        int mainRayCount = m_WeaponFireData.CoreRadialNumber;
        mainAimArea.Init(false, 0, MainAreaCoverAngle, mainRayCount, m_SkillMaxDistance, sonCount);

        m_AimAreaList.Add(mainAimArea);

        //子区域
        for (int i = 0; i < sonCount; i++)
        {
            AimArea sonAimArea = new AimArea();

            // 子区域覆盖角度
            float SubAimAreaCoverAngle = m_WeaponFireData.SonDiameterAngle - accuracy * m_WeaponFireData.SonDiameterAngleCoefficient;
            int sonRayCount = m_WeaponFireData.SonRadialNumber;
            sonAimArea.Init(true, m_SonMaxSpreadAngle, SubAimAreaCoverAngle, sonRayCount, m_SkillMaxDistance, sonCount);
            m_AimAreaList.Add(sonAimArea);
        }

        m_FireInterval = WeaponAndCrossSightFactory. CalculateFireInterval(m_MainPlayer, m_WeaponTable.TypeDateSheetld, m_UId, WeaponAndCrossSight.WeaponAndCrossSightTypes.ShotGun);
    } 

    public override void OnRelease()
    {
        base.OnRelease();

        for (int i = 0; i < m_AimAreaList.Count; i++)
        {
            AimArea aimArea = m_AimAreaList[i];
            aimArea.OnRelease();
        }
        m_AimAreaList.Clear();
    }


    public override void OnHotKeyDown(int skillID)
    {
        if (!m_PlayerSkillProxy.CanCurrentWeaponRelease())
        {
            //开火失败
            GameFacade.Instance.SendNotification(NotificationName.PlayerWeaponFireFail);
            return;
        }

        base.OnHotKeyDown(skillID);
        SetAreas();

        SkillCastEvent castSkillEvent = new SkillCastEvent();
        castSkillEvent.IsWeaponSkill = true;
        castSkillEvent.SkillIndex = skillID;
        castSkillEvent.KeyPressed = true;
        m_MainPlayer.SendEvent(ComponentEventName.SkillButtonResponse, castSkillEvent);

        MessageToHudCrosShairSize();
    }


    public override void OnHotKeyUp(int skillID)
    {
        base.OnHotKeyUp(skillID);

        SkillCastEvent castSkillEvent = new SkillCastEvent();
        castSkillEvent.IsWeaponSkill = true;
        castSkillEvent.SkillIndex = skillID;
        castSkillEvent.KeyPressed = false;
        m_MainPlayer.SendEvent(ComponentEventName.SkillButtonResponse, castSkillEvent);
    }


    /// <summary>
    /// 设置各个区域
    /// </summary>
    private void SetAreas()
    {
        Vector3 direction =CameraManager.GetInstance().GetMainCamereComponent().GetForward();

        float rollAngleOffset = Random.Range(0, 360);
        for (int i = 0; i < m_AimAreaList.Count; i++)
        {
            AimArea aimArea = m_AimAreaList[i];
            aimArea.CalculateTheCentralRay(direction, i, rollAngleOffset);
        }
    }

    public override List<CCrossSightLoic.Target> GetTargets()
    {
        m_Targets.Clear();
        for (int i = 0; i < m_AimAreaList.Count; i++)
        {
            AimArea aimArea = m_AimAreaList[i];
            m_Targets.AddRange(aimArea.GetTargets());
        }
        return m_Targets;
    }

    //protected override void MessageToTheChangeOfCrosShairSize(float delta)
    //{
    //    //空继承，防止 父类 update ，给覆盖
    //    //改为鼠标按下时，通知hud ,此时各个区域情况，由Hud 自己循环区域动画
    //}

    protected override MsgPlayerWeaponCrosshair GetRelativeHeightOfReticle()
    {
        MsgPlayerWeaponCrosshair crosshair = new MsgPlayerWeaponCrosshair();
        Camera cam = CameraManager.GetInstance()?.GetMainCamereComponent()?.GetCamera();
        if (cam != null)
        {
            float horFOV = cam.fieldOfView * cam.aspect;
            float verFOV = cam.fieldOfView;

            for (int i = 0; i < m_AimAreaList.Count; i++)
            {
                AimArea aimArea = m_AimAreaList[i];
                if (!aimArea.IsSon)
                {
                    //主区域
                    crosshair.HorizontalRelativeHeight = aimArea.GetCoverAngle() / horFOV;
                    crosshair.VerticalRelativeHeight = aimArea.GetCoverAngle() / verFOV;
                }
            }
            return crosshair;
        }
        else
        {
            return base.GetRelativeHeightOfReticle();
        }
    }


    /// <summary>
    /// 通知Hud
    /// </summary>
    private void MessageToHudCrosShairSize()
    {
        Camera cam = CameraManager.GetInstance()?.GetMainCamereComponent()?.GetCamera();
        if (cam != null)
        {
            MsgPlayerWeaponCrosshair_ShotGun crosshair = new MsgPlayerWeaponCrosshair_ShotGun();
            float horFOV = cam.fieldOfView * cam.aspect;
            float verFOV = cam.fieldOfView;

            crosshair.SubAimAreaRelativeHeight.Clear();
            crosshair.SubAimAreaScreenPosition.Clear();

            for (int i = 0; i < m_AimAreaList.Count; i++)
            {
                AimArea aimArea = m_AimAreaList[i];
                if (aimArea.IsSon)
                {
                    //子区域
                    crosshair.SubAimAreaRelativeHeight.Add(new Vector2( aimArea.GetCoverAngle() / horFOV, aimArea.GetCoverAngle() / verFOV));
                    crosshair.SubAimAreaScreenPosition.Add(cam.WorldToScreenPoint(cam.transform.position + aimArea.GetCentralRay()));
                }
            }

            crosshair.RemainingRestoreDuration = Mathf.Clamp(m_TimeOfStartFire + m_RestoreDuration - Time.time, 0, float.MaxValue);
            GameFacade.Instance.SendNotification(NotificationName.PlayerWeaponCrosshairScale_ShotGun, crosshair);
        }
    }

    protected override void OnUpdate_First(float delta)
    {
#if UNITY_EDITOR
        for (int i = 0; i < m_AimAreaList.Count; i++)
        {
            AimArea aimArea = m_AimAreaList[i];
            aimArea.DrawRay();

        }
#endif
    }
}