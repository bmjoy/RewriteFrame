using EditorExtend;
using Eternity.FlatBuffer;
using Leyoutech.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 霰弹枪武器
/// </summary>
public class BattleWeapon_Shotgun : BattleWeaponBase
{
	public class AimArea
	{
		public float SpreadAngle;
		public float RollAngle;
		/// <summary>
		/// 射线分散可覆盖的角度
		/// </summary>
		public float CoverAngle;
		/// <summary>
		/// 射线数量
		/// </summary>
		public float RayCount;

		private Vector3 m_BaseRay;

		public AimArea()
		{
			m_RayListCache = new List<Vector3>();
			m_RaySpreadAngleListCache = new List<float>();
			m_RayRollAngleListCache = new List<float>();
		}

		public void UpdateBaseRay(Vector3 baseRay)
		{
			m_BaseRay = MathUtility.PitchAndRoll(baseRay, SpreadAngle / 2, RollAngle);
		}

		public Vector3 GetBaseRay()
		{
			return m_BaseRay;
		}

		private List<Vector3> m_RayListCache;
		private List<float> m_RaySpreadAngleListCache;
		private List<float> m_RayRollAngleListCache;

		/// <summary>
		/// 区域内的所有射线随机朝向
		/// 以区域中心射线为基准
		/// 每条射线的pitch角为最大spread角的 1/4 - 3/4
		/// 每条射线的roll角为 随机偏移角度 + 360 / 射线数量 * 当前射线索引
		/// </summary>
		/// <param name="rayList"></param>
		public void UpdateRayList(bool rerandomRayDir)
		{
			m_RayListCache.Clear();
			if (rerandomRayDir)
			{
				m_RaySpreadAngleListCache.Clear();
				m_RayRollAngleListCache.Clear();
			}

			float rayRollAngleOffset = Random.Range(0, 360);
			for (int iRay = 0; iRay < RayCount; iRay++)
			{
				if (rerandomRayDir || m_RaySpreadAngleListCache.Count < RayCount || m_RayRollAngleListCache.Count < RayCount)
				{
					m_RaySpreadAngleListCache.Add(Random.Range(0.25f, 0.75f) * CoverAngle / 2);
					m_RayRollAngleListCache.Add((360 / RayCount * iRay + rayRollAngleOffset) % 360);
				}
				float spreadAngle = m_RaySpreadAngleListCache[iRay];
				float rollAngle = m_RayRollAngleListCache[iRay];

				m_RayListCache.Add(MathUtility.PitchAndRoll(m_BaseRay, spreadAngle, rollAngle));
			}
		}

		public void GetRayList(ref List<Vector3> rayList)
		{
			rayList.Clear();
			rayList.AddRange(m_RayListCache);
		}
	}

	private AimArea m_MainAimArea;
	private List<AimArea> m_SubAimAreas;

	/// <summary>
	/// 上次开火的时间
	/// </summary>
	private float m_LastFireTime;

	/// <summary>
	/// 武器特有属性
	/// </summary>
	private WeaponShotgun m_WeaponFireData;

	// 参数
	// 覆盖角度: 每个区域在世界空间中对应的圆锥体的顶角的角度
	// 扩散角: 子区域在开火后扩散到周围区域, 这是扩散后其中心射线与视线的夹角

	/// <summary>
	/// 子区域数量
	/// </summary>
	private int m_SubAimAreaCount;
	/// <summary>
	/// 子区域扩散角度
	/// </summary>
	private float m_AimAreaMaxSpreadAngle;
	/// <summary>
	/// 扩散恢复时间. 秒
	/// </summary>
	private float m_RestoreDuration;
	/// <summary>
	/// 中心区域覆盖角度
	/// </summary>
	private float m_MainAreaCoverAngle;
	/// <summary>
	/// 中心区域射线数量
	/// </summary>
	private int m_MainAreaRayCount;
	/// <summary>
	/// 子区域覆盖角度
	/// </summary>
	private float m_SubAimAreaCoverAngle;
	/// <summary>
	/// 子区域射线数量
	/// </summary>
	private int m_SubAimAreaRayCount;


	public BattleWeapon_Shotgun(IWeapon weapon) : base(weapon)
	{
		m_MainAimArea = new AimArea();

		m_WeaponFireData = m_CfgEternityProxy.GetWeaponDataOfShotgun(m_WeaponConfig.TypeDateSheetld);

		TranslateUniformParamToWeaponParam();
	}

    public override void OnRelease()
    {
        base.OnRelease();
    }

    public override bool CanWeaponFire()
	{
		return Time.time - m_LastFireTime > CalculateFireInterval();
	}

	public override float CalculateFireInterval()
	{
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

		float weaponFireSpeed = m_MainPlayer.GetWeaponAttribute(m_WeaponItem.GetUID(), crucis.attributepipeline.attribenum.AttributeName.kLightHeatCastSpeed);
		if (weaponFireSpeed >= 0)
		{
			m_FireInterval = 60f / m_WeaponFireData.MinuteBulletNumber / (1 + weaponFireSpeed / 100f);
		}
		else
		{
			m_FireInterval = Mathf.Abs(60f / m_WeaponFireData.MinuteBulletNumber * (1 + weaponFireSpeed / 100f - 2f));
		}

		return m_FireInterval;
	}

	List<SkillTarget> m_TargetsCache = new List<SkillTarget>();
	List<Vector3> m_AimDirectionsCache = new List<Vector3>();
	public override void CalculateNextTargets(ref List<SkillTarget> targetList)
	{
		targetList.Clear();

		// 根据当前瞄准方向更新主区域的目标信息
		m_MainAimArea.UpdateBaseRay(GetViewRay().direction);
		m_MainAimArea.UpdateRayList(true);
		m_MainAimArea.GetRayList(ref m_AimDirectionsCache);
		for (int iRay = 0; iRay < m_AimDirectionsCache.Count; iRay++)
		{
			targetList.Add(CalulateTargetDataByShootingDirection(m_AimDirectionsCache[iRay]));
		}

		// 根据当前瞄准方向更新主区域的目标信息
		for (int iAim = 0; iAim < m_SubAimAreas.Count; iAim++)
		{
			m_SubAimAreas[iAim].UpdateBaseRay(GetViewRay().direction);
			m_SubAimAreas[iAim].UpdateRayList(true);
			m_SubAimAreas[iAim].GetRayList(ref m_AimDirectionsCache);
			for (int iRay = 0; iRay < m_AimDirectionsCache.Count; iRay++)
			{
				targetList.Add(CalulateTargetDataByShootingDirection(m_AimDirectionsCache[iRay]));
			}
		}
	}

	public override void OnWeaponSkillFinished(bool success)
	{
		if (success)
		{
			// 每次武器射击以后: 
			// Spread角扩散到最大
			// roll角加一个固定偏移
			float rollAngleOffset = Random.Range(0, 360);
			for (int iAim = 0; iAim < m_SubAimAreas.Count; iAim++)
			{
				AimArea aimArea = m_SubAimAreas[iAim];
				aimArea.SpreadAngle = m_AimAreaMaxSpreadAngle;
				aimArea.RollAngle = (360 / m_SubAimAreas.Count * iAim + rollAngleOffset) % 360;
			}

			m_LastFireTime = Time.time;

			NotifyHUDCrosshair();
		}
	}
	
	public override MsgPlayerWeaponCrosshair GetRelativeHeightOfReticle()
	{
		MsgPlayerWeaponCrosshair crosshair = new MsgPlayerWeaponCrosshair();
		Camera cam = CameraManager.GetInstance()?.GetMainCamereComponent()?.GetCamera();
		float horFOV = cam.fieldOfView * cam.aspect;
		float verFOV = cam.fieldOfView;
		if (cam != null)
		{
			//crosshair.HorizontalRelativeHeight = m_MainAreaCoverAngle / horFOV;
			//crosshair.VerticalRelativeHeight = m_MainAreaCoverAngle / verFOV;

			//crosshair.SubAimAreaRelativeHeight.Clear();
			//crosshair.SubAimAreaScreenPosition.Clear();

			//for (int iArea = 0; iArea < m_SubAimAreas.Count; iArea++)
			//{
			//	m_SubAimAreas[iArea].UpdateBaseRay(cam.transform.forward);
			//	crosshair.SubAimAreaRelativeHeight.Add(new Vector2(m_SubAimAreas[iArea].CoverAngle / horFOV, m_SubAimAreas[iArea].CoverAngle / verFOV));
			//	crosshair.SubAimAreaScreenPosition.Add(cam.WorldToScreenPoint(cam.transform.position + m_SubAimAreas[iArea].GetBaseRay()));
			//}

			//crosshair.RemainingRestoreDuration = Mathf.Clamp(m_LastFireTime + m_RestoreDuration - Time.time, 0, float.MaxValue);
			return crosshair;
		}
		else
		{
			return base.GetRelativeHeightOfReticle();
		}
	}

	public override void ResetWeaponRuntimeData()
	{
		for (int iAim = 0; iAim < m_SubAimAreas.Count; iAim++)
		{
			AimArea aimArea = m_SubAimAreas[iAim];
			aimArea.SpreadAngle = 0;
			aimArea.RollAngle = 360 / m_SubAimAreas.Count * iAim;
		}
	}

	/// <summary>
	/// 随时间恢复子区域的扩散位置
	/// </summary>
	/// <param name="delta"></param>
	public override void OnUpdate(float delta)
	{
		base.OnUpdate(delta);

		TranslateUniformParamToWeaponParam();

		float spreadRestoreSpeed = m_AimAreaMaxSpreadAngle / m_RestoreDuration;
		for (int iAim = 0; iAim < m_SubAimAreas.Count; iAim++)
		{
			AimArea aimArea = m_SubAimAreas[iAim];
			aimArea.SpreadAngle = Mathf.Clamp(aimArea.SpreadAngle - spreadRestoreSpeed * delta, 0, float.MaxValue);
		}

#if UNITY_EDITOR
		DrawGizmo();
		DrawBoxOnGUI();
#endif
	}

#if UNITY_EDITOR
	private void DrawGizmo()
	{
		MainCameraComponent mainCam = CameraManager.GetInstance().GetMainCamereComponent();
		if (mainCam == null)
			return;

		// 根据当前瞄准方向更新主区域的目标信息
		m_MainAimArea.UpdateBaseRay(GetViewRay().direction);
		Vector3 baseRayPos = mainCam.GetPosition() + m_MainAimArea.GetBaseRay();
		GizmosHelper.GetInstance().DrawCircle(baseRayPos, 0.05f);

		m_MainAimArea.UpdateRayList(false);
		m_MainAimArea.GetRayList(ref m_AimDirectionsCache);
		for (int iRay = 0; iRay < m_AimDirectionsCache.Count; iRay++)
		{
			Vector3 pos = mainCam.GetPosition() + m_AimDirectionsCache[iRay];
			GizmosHelper.GetInstance().DrawCircle(pos, 0.005f);
			GizmosHelper.GetInstance().DrawLine(mainCam.GetPosition(), mainCam.GetPosition() + m_AimDirectionsCache[iRay] * 5);
		}

		// 根据当前瞄准方向更新主区域的目标信息
		for (int iAim = 0; iAim < m_SubAimAreas.Count; iAim++)
		{
			m_SubAimAreas[iAim].UpdateBaseRay(GetViewRay().direction);
			baseRayPos = mainCam.GetPosition() + m_SubAimAreas[iAim].GetBaseRay();
			GizmosHelper.GetInstance().DrawCircle(baseRayPos, 0.05f);

			m_SubAimAreas[iAim].UpdateRayList(false);
			m_SubAimAreas[iAim].GetRayList(ref m_AimDirectionsCache);
			for (int iRay = 0; iRay < m_AimDirectionsCache.Count; iRay++)
			{
				Vector3 pos = mainCam.GetPosition() + m_AimDirectionsCache[iRay];
				GizmosHelper.GetInstance().DrawCircle(pos, 0.005f);
				GizmosHelper.GetInstance().DrawLine(mainCam.GetPosition(), mainCam.GetPosition() + m_AimDirectionsCache[iRay] * 5);
			}
		}
	}

	private void DrawBoxOnGUI()
	{
		MainCameraComponent mainCam = CameraManager.GetInstance().GetMainCamereComponent();
		if (mainCam == null)
			return;

		for (int iAim = 0; iAim < m_SubAimAreas.Count; iAim++)
		{
			AimArea aimArea = m_SubAimAreas[iAim];
			m_SubAimAreas[iAim].UpdateBaseRay(GetViewRay().direction);
			Vector3 aimDir = m_SubAimAreas[iAim].GetBaseRay();
			Vector3 pos = mainCam.GetPosition() + aimDir;
			Vector3 screenPos = mainCam.GetCamera().WorldToScreenPoint(pos);

			GizmosHelper.GetInstance().DrawGUIBox(screenPos, 100);
		}
	}
#endif


	private Ray GetViewRay()
	{
		MainCameraComponent mainCam = CameraManager.GetInstance().GetMainCamereComponent();
		if (mainCam != null)
		{
			return new Ray(mainCam.GetPosition(), mainCam.GetForward());
		}
		else
		{
			return new Ray();
		}
	}

	private void TranslateUniformParamToWeaponParam()
	{
		float accuracy = m_MainPlayer.GetWeaponAttribute(m_WeaponItem.GetUID(), crucis.attributepipeline.attribenum.AttributeName.kWeaponAccuracy);
		float stability = m_MainPlayer.GetWeaponAttribute(m_WeaponItem.GetUID(), crucis.attributepipeline.attribenum.AttributeName.kWeaponStability);

		// 影响武器运作的每一条属性, 都有一个计算公式. 计算公式中使用的变量的值来自于 weapon_xxx表 
		// 公式来自于: 王洪波
		// 霰弹枪武器属性表: weapon_shotgun
		m_AimAreaMaxSpreadAngle = m_WeaponFireData.SonDiffusionAngle - accuracy * m_WeaponFireData.SonDiffusionAngleCoefficient;
		m_MainAreaCoverAngle = m_WeaponFireData.CoreDiameterAngle - accuracy * m_WeaponFireData.CoreDiameterAngleCoefficient;
		m_SubAimAreaCoverAngle = m_WeaponFireData.SonDiameterAngle - accuracy * m_WeaponFireData.SonDiameterAngleCoefficient;
		// 表里的恢复时间的单位是毫秒
		m_RestoreDuration = (m_WeaponFireData.RecoveryTime - stability * m_WeaponFireData.RecoveryTimeCoefficient) / 1000f;

		m_SubAimAreaCount = (int)m_WeaponFireData.SonNumber;
		m_MainAreaRayCount = (int)m_WeaponFireData.CoreRadialNumber;
		m_SubAimAreaRayCount = (int)m_WeaponFireData.SonRadialNumber;

		m_MainAimArea.RollAngle = 0f;
		m_MainAimArea.SpreadAngle = 0f;
		m_MainAimArea.RayCount = m_MainAreaRayCount;
		m_MainAimArea.CoverAngle = m_MainAreaRayCount;

		// 根据子区域数量重新计算当前每个子区域的偏移位置
		if (m_SubAimAreas == null)
		{
			m_SubAimAreas = new List<AimArea>();
		}

		if (m_SubAimAreas.Count != m_SubAimAreaCount)
		{
			m_SubAimAreas.Clear();
			for (int iArea = 0; iArea < m_SubAimAreaCount; iArea++)
			{
				m_SubAimAreas.Add(new AimArea());
			}

			for (int iAim = 0; iAim < m_SubAimAreas.Count; iAim++)
			{
				AimArea aimArea = m_SubAimAreas[iAim];
				aimArea.SpreadAngle = 0;
				aimArea.RollAngle = 360 / m_SubAimAreas.Count * iAim;
				aimArea.RayCount = m_SubAimAreaRayCount;
				aimArea.CoverAngle = m_SubAimAreaCoverAngle;
			}
		}

		for (int iAim = 0; iAim < m_SubAimAreas.Count; iAim++)
		{
			AimArea aimArea = m_SubAimAreas[iAim];
			aimArea.RayCount = m_SubAimAreaRayCount;
			aimArea.CoverAngle = m_SubAimAreaCoverAngle;
		}
	}
}
