using Eternity.FlatBuffer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 机关枪武器
/// 元属性: 初始水平扩散角, 最大水平扩散角, 水平/垂直 角度比例, 射击角度扩散函数, 每次射击对射击角度扩散的归一化影响因子, 射击扩散角度随时间收缩的函数
/// 运行时属性: 归一化的角度扩散因子
/// 
/// TODO: 这里临时把最后一个参数定义为 归一化影响因子随时间变化的函数 , 而不是 射击扩散角度随时间变化的函数. 
///		是因为如果使用后者, 每次收缩以后, 下次开枪时需要通过当前的 扩散角度 去求 归一化影响因子, 太费劲了. 先不做. 以后直接让策划写函数
/// </summary>
public class BattleWeapon_MachineGun : BattleWeaponBase
{
	/// <summary>
	/// 当前的水平扩散角度
	/// </summary>
	private float m_CurrentHorizontalAngle;
	private float m_CurrentVerticalAngle;

	/// <summary>
	/// 归一化的角度扩散因子
	/// 扩散和收缩时直接影响的都是这个属性. 通过这个属性再去计算当前扩散角度
	/// </summary>
	private float m_SpreadTimeFactor;

	/// <summary>
	/// 速射枪武器专有属性
	/// </summary>
	private WeaponRapidFirer m_WeaponFireData;

	// 武器弹道扩散的静态数据
	///// <summary>
	///// 初始横向扩散角
	///// </summary>
	//private float m_InitialHorizontalSpreadAngle;
	///// <summary>
	///// 最大横向扩散角
	///// </summary>
	//private float m_MaxHorizontalSpreadAngle;
	/// <summary>
	/// 横向扩散角 / 纵向扩散角
	/// </summary>
	private float m_Aspect;
	///// <summary>
	///// 射击导致散布范围扩张时, 用射击时间计算散布角度的公式
	///// </summary>
	//private EquationUtility.EquationType m_SpreadEquationType;
	///// <summary>
	///// 每一次射击会增加多少 "单位化的射击时间" (用来计算散布角度)
	///// </summary>
	//private float m_SpreadFactorChangePerShot;
	///// <summary>
	///// 停止射击导致散布范围收缩时, 用停止射击的时间计算收缩角度的公式
	///// </summary>
	//private EquationUtility.EquationType m_ShrinkEquationType;
	///// <summary>
	///// 准星收缩过程的时间
	///// </summary>
	//private float m_ShrinkTime;

	///// <summary>
	///// 表示收缩了多久了的时间因子
	///// </summary>
	//private float m_ShrinkedTimeFactor;
	///// <summary>
	///// 归一化的收缩的扩散角度. 值域: (-1, 1)
	///// </summary>
	//private float m_ShrinkedFactor;
	
	private PlayerSkillVO m_SkillVO;
	
	// 速射枪逻辑重构的运行时参数
	/// <summary>
	/// 重构以后按一个虚拟的"子弹数"来控制准星大小
	/// </summary>
	private float m_CurrentBulletCount;

	/// <summary>
	/// 停止射击的时间. Time.time
	/// </summary>
	private float m_TimeOfStopFire;
	/// <summary>
	/// 停火的那一瞬间的子弹数
	/// </summary>
	private float m_BulletCountWhenStopFire;
	/// <summary>
	/// 开始开火的时间
	/// </summary>
	float m_TimeOfStartFire;
	/// <summary>
	/// 发射上一颗子弹的时间
	/// </summary>
	float m_TimeOfFiredLastBullet;
	
	/// <summary>
	/// 这次按键的按下, 已经释放过技能了
	/// </summary>
	private bool m_FiredDuringThisPress;

	private enum GunState
	{
		Fire,
		Stop,
	}

	GunState m_State;

	/// <summary>
	/// 上一次减少子弹数的时间
	/// </summary>
	private float m_TimeOfDecreaseLastBullet;

	/// <summary>
	/// 武器精准度
	/// </summary>
	private float m_Accuracy;
	/// <summary>
	/// 武器稳定性
	/// </summary>
	private float m_Stability;

	/// <summary>
	/// 射击后累加子弹数
	/// </summary>
	private void IncreaseBulletCountOnPostFire()
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

    public override void OnRelease()
    {
        base.OnRelease();
    }

    /// <summary>
    /// 随着时间流逝恢复子弹数
    /// </summary>
    private void DecreaseBulletCountWithTime()
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
		float startDecreateBulletSinceStopFire = m_FireInterval + m_WeaponFireData.ShrinkReturnTimeCoefficient / 1000f
											+ m_Stability * m_WeaponFireData.Ceasefirestableshrinktime / 1000f;

		if (Time.time - m_TimeOfStopFire > startDecreateBulletSinceStopFire)
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
	/// 计算射击前水平散布角
	/// </summary>
	/// <returns></returns>
	private float CalculateHorizontalAnglePreFire()
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

			float post = CalculateHorizontalAnglePostFire();
			return Mathf.Min(post * m_WeaponFireData.ShrinkLimitCoefficient
				, m_WeaponFireData.InitialDiffusionAngle - m_WeaponFireData.AccurateAimCoefficient * m_Accuracy);
		}
	}

	/// <summary>
	/// 计算射击后水平散布角
	/// </summary>
	/// <returns></returns>
	private float CalculateHorizontalAnglePostFire()
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

	public BattleWeapon_MachineGun(IWeapon weapon) : base(weapon)
	{
		m_WeaponFireData = m_CfgEternityProxy.GetWeaponDataOfMachineGun(m_WeaponConfig.TypeDateSheetld);
		m_SkillVO = m_PlayerSkillProxy.GetSkillByID((int)m_WeaponItem.GetBaseConfig().SkillId);

		m_CurrentBulletCount = 0;
		m_State = GunState.Stop;

		// TEST
		//SetupStaticWeaponParam_Debug();
		TranslateUniformParamToWeaponParam();
		// TEST
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

	public override void StopFire()
	{
		m_FiredDuringThisPress = false;
		m_State = GunState.Stop;
		m_TimeOfStopFire = Time.time;
		m_BulletCountWhenStopFire = m_CurrentBulletCount;

		m_TimeOfStartFire = 0;
		m_TimeOfFiredLastBullet = 0;
		m_TimeOfDecreaseLastBullet = 0;
	}

	/// <summary>
	/// 机枪武器对应的技能是一个持续引导技能, 按键按下后, 在松开之前, 只记做一次技能发射
	/// </summary>
	/// <param name="hotkey"></param>
	public override void WeaponOperationImplementation(SkillHotkey skillHotkey)
	{
		bool press = skillHotkey.ActionPhase == HotkeyPhase.Started;
		if (press)
		{
			if (!m_FiredDuringThisPress && m_PlayerSkillProxy.CanCurrentWeaponRelease())
			{
				SkillCastEvent castSkillEvent = new SkillCastEvent();
				castSkillEvent.IsWeaponSkill = true;
				castSkillEvent.SkillIndex = skillHotkey.SkillIndex;
				castSkillEvent.KeyPressed = true;

// #if NewSkill
// #else
                m_MainPlayer.SendEvent(ComponentEventName.CastSkill, castSkillEvent);
//#endif

				m_FiredDuringThisPress = true;
				m_State = GunState.Fire;
				m_TimeOfStartFire = Time.time;
			}
			else if (!m_FiredDuringThisPress && !m_PlayerSkillProxy.CanCurrentWeaponRelease())
			{
				GameFacade.Instance.SendNotification(NotificationName.PlayerWeaponFireFail);
			}
			else
			{
				// 已经放过技能了, 不再发送释放技能消息. 解决武器过热以后还会继续射击的问题
			}
		}
		else
		{
			if (m_State == GunState.Fire)
			{
				SkillCastEvent castSkillEvent = new SkillCastEvent();
				castSkillEvent.IsWeaponSkill = true;
				castSkillEvent.SkillIndex = skillHotkey.SkillIndex;
				castSkillEvent.KeyPressed = false;

// #if NewSkill
// #else
                m_MainPlayer.SendEvent(ComponentEventName.CastSkill, castSkillEvent);
//#endif

                m_FiredDuringThisPress = false;
				m_State = GunState.Stop;
				m_TimeOfStopFire = Time.time;
				m_BulletCountWhenStopFire = m_CurrentBulletCount;
			}
		}
	}

	/// <summary>
	/// 武器开火
	/// </summary>
	public override void PostWeaponFire()
	{
		//// 重置武器弹道扩散的收缩信息
		//FinishShrink();
		//SpreadAfterFire();
		//StartShrink();

		m_TimeOfFiredLastBullet = Time.time;
		m_CurrentHorizontalAngle = CalculateHorizontalAnglePostFire();
		m_CurrentVerticalAngle = m_CurrentHorizontalAngle / m_Aspect;
		IncreaseBulletCountOnPostFire();
	}

	/// <summary>
	/// 计算武器下一次射击的目标. (可能是空点, 可能是Entity)
	/// </summary>
	/// <returns></returns>
	public override SkillTarget CalculateNextTarget()
	{
		// 根据当前扩散角度随机方向
		Vector3 shootingDir = RandomizeShootingDirection(CameraManager.GetInstance().GetMainCamereComponent().GetForward());

		SendScreenPositionOfShotToUI(shootingDir);

		return CalulateTargetDataByShootingDirection(shootingDir);
	}

	public override void ResetWeaponRuntimeData()
	{
		// 重置运行时扩散信息
		m_CurrentHorizontalAngle = 0;
		m_CurrentVerticalAngle = 0;
		m_CurrentBulletCount = 0;
		

		//m_SpreadTimeFactor = 0;

		//FinishShrink();
	}

	public override void OnUpdate(float delta)
	{
		base.OnUpdate(delta);

		if (m_State == GunState.Stop)
		{
			float timeSinceStop = Time.time - m_TimeOfStopFire;

			DecreaseBulletCountWithTime();

			if (timeSinceStop < m_FireInterval)
			{
				float t = timeSinceStop / m_FireInterval;
				float pre = CalculateHorizontalAnglePreFire();
				float post = CalculateHorizontalAnglePostFire();
				m_CurrentHorizontalAngle = Mathf.Lerp(post, pre, t);
			}
			else
			{
				// 优化. 缓存PreFireAngleWhenBullet=0
				float pre = CalculateHorizontalAnglePreFire();
				m_CurrentHorizontalAngle = pre;
			}
		} 
		else if (m_State == GunState.Fire)
		{
			float timeSinceLastFiredBullet = Time.time - m_TimeOfFiredLastBullet;
			float t = timeSinceLastFiredBullet / m_FireInterval;
			float pre = CalculateHorizontalAnglePreFire();
			float post = CalculateHorizontalAnglePostFire();
			m_CurrentHorizontalAngle = Mathf.Lerp(post, pre, t);
		}

		//Debug.LogFormat("Angle: {0}, SpreadFactor: {1}, ShrinkFactor: {2}"
		//	, m_CurrentHorizontalAngle, m_SpreadTimeFactor
		//	, m_ShrinkedFactor);

		//ShrinkWithTime(delta);
		
		NotifyHUDCrosshair();

		//SetupStaticWeaponParam_Debug();
		TranslateUniformParamToWeaponParam();
	}

	public override MsgPlayerWeaponCrosshair GetRelativeHeightOfReticle()
	{
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
	/// 把这一次射击弹道对应的屏幕坐标传给UI
	/// </summary>
	private void SendScreenPositionOfShotToUI(Vector3 shootingDirection)
	{
		if (m_WeaponItem != m_PlayerSkillProxy.GetCurrentWeapon())
			return;

		Camera cam = CameraManager.GetInstance()?.GetMainCamereComponent()?.GetCamera();
		if (cam != null)
		{
			Vector3 screenPos = cam.WorldToScreenPoint(cam.transform.position + shootingDirection);
			MsgPlayerWeaponShot weaponShot = new MsgPlayerWeaponShot();
			weaponShot.screenPoint = screenPos;
			GameFacade.Instance.SendNotification(NotificationName.PlayerWeaponShot, weaponShot);
		}
	}

	///// <summary>
	///// 每次武器射击后的弹道扩散
	///// </summary>
	//private void SpreadAfterFire()
	//{
	//	// 每次射击累加 归一化的影响因子
	//	m_SpreadTimeFactor = Mathf.Clamp01(m_SpreadTimeFactor + m_SpreadFactorChangePerShot);
	//	ChangeSpreadAngleByFactor();
	//}

	//private void ChangeSpreadAngleByFactor()
	//{
	//	// 计算 射击角度扩散函数, 传入 归一化的影响因子, 计算得到下式中的 归一化扩散角度
	//	float SpreadAngleFactor = Mathf.Clamp01(TimeFactorToSpreadAngleFactor(m_SpreadTimeFactor));
	//	// 使用 归一化扩散角度, 计算真正的扩散角度
	//	m_CurrentHorizontalAngle = Mathf.Lerp(m_InitialHorizontalSpreadAngle, m_MaxHorizontalSpreadAngle, SpreadAngleFactor);
	//	m_CurrentVerticalAngle = m_CurrentHorizontalAngle / m_Aspect;
	//}

	/// <summary>
	/// 随机下一次射击的射击方向
	/// </summary>
	/// <param name="originalCamDir"></param>
	/// <param name="channellingTime"></param>
	/// <returns></returns>
	private Vector3 RandomizeShootingDirection(Vector3 originalCamDir)
	{
		float verticalDeviateAngle = Random.Range(-m_CurrentVerticalAngle, m_CurrentVerticalAngle);
		float horizontalDeviateAngle = Random.Range(-m_CurrentHorizontalAngle, m_CurrentHorizontalAngle);

		Vector3 camUp = m_MainPlayer.GetRootTransform().up;
		Vector3 camRight = Vector3.Cross(m_MainPlayer.GetRootTransform().up, originalCamDir);

		// 竖直偏移
		Vector3 deviatedDir = Quaternion.AngleAxis(verticalDeviateAngle, camRight) * originalCamDir;
		// 水平偏移
		deviatedDir = Quaternion.AngleAxis(horizontalDeviateAngle, camUp) * deviatedDir;

		return deviatedDir.normalized;
	}

	///// <summary>
	///// 结束当前的弹道收缩行为
	///// </summary>
	//private void FinishShrink()
	//{
	//	m_ShrinkedTimeFactor = 0;
	//	m_ShrinkedFactor = 0;
	//}

	///// <summary>
	///// 开始弹道收缩
	///// </summary>
	//private void StartShrink()
	//{
	//	m_ShrinkedTimeFactor = 0;
	//	m_ShrinkedFactor = 0;
	//}

	///// <summary>
	///// 随着时间流逝, 弹道扩散范围收缩
	///// </summary>
	//private void ShrinkWithTime(float deltaTime)
	//{
	//	// 到这一帧为止, 已经收缩多久了, 且用归一化的时间因子表示收缩的时间
	//	float shrinkTimeFactor = deltaTime / m_ShrinkTime;
	//	m_ShrinkedTimeFactor = Mathf.Clamp(m_ShrinkedTimeFactor + shrinkTimeFactor, 0, float.MaxValue);

	//	// 求出应该总共收缩的角度
	//	float newShrinkAngleFactor = TimeFactorToShrinkAngleFactor(m_ShrinkedTimeFactor);
	//	// 约束到 (-1, 1) 的值域范围内
	//	newShrinkAngleFactor = Mathf.Sign(newShrinkAngleFactor) * Mathf.Clamp01(Mathf.Abs(newShrinkAngleFactor));

	//	// 求出这一帧应该收缩的角度
	//	float shrinkAngleFactorOffset = newShrinkAngleFactor - m_ShrinkedFactor;
	//	m_ShrinkedFactor = newShrinkAngleFactor;

	//	// 求出当前扩散的角度, 收缩上面计算出的收缩角度, 并用当前的扩散角度反求扩散的时间因子, 以便开火时累加扩散的时间因子
	//	float spreadAngleFactor = Mathf.Clamp01(TimeFactorToSpreadAngleFactor(m_SpreadTimeFactor));
	//	spreadAngleFactor = Mathf.Clamp01(spreadAngleFactor + shrinkAngleFactorOffset);
	//	m_SpreadTimeFactor = SpreadAngleFactorToTimeFactor(spreadAngleFactor);
		
	//	// 重新计算当前的扩散角度
	//	ChangeSpreadAngleByFactor();
	//}

	///// <summary>
	///// 在没有收缩的情况下这把武器连续射击多长时间, 就可以让散布范围达到最大
	///// 单位: 秒
	///// </summary>
	//private float GetMaxSpreadTime()
	//{
	//	// 子弹发射间隔 / 每发子弹对散布的影响因子
	//	return (SkillBase.s_CfgSkillProxy.GetSkillReleaseInterval(m_SkillVO.GetID()) / 1000f) / m_SpreadFactorChangePerShot;
	//}

	/// <summary>
	/// 把统一的参数翻译成武器的弹道扩散参数
	/// </summary>
	private void TranslateUniformParamToWeaponParam()
	{
		m_Accuracy = m_MainPlayer.GetWeaponAttribute(m_WeaponItem.GetUID(), crucis.attributepipeline.attribenum.AttributeName.kWeaponAccuracy);
		m_Stability = m_MainPlayer.GetWeaponAttribute(m_WeaponItem.GetUID(), crucis.attributepipeline.attribenum.AttributeName.kWeaponStability);
		CalculateFireInterval();

		m_Aspect = m_WeaponFireData.AngleRatio;

		//m_InitialHorizontalSpreadAngle = Mathf.Clamp(3.2f - m_Accuracy * 0.01f, 0, float.MaxValue);
		//m_MaxHorizontalSpreadAngle = Mathf.Clamp(2 - m_Stability * 0.01f, 0, float.MaxValue);

		//m_SpreadEquationType = (EquationUtility.EquationType)(int)m_WeaponSpreadParam.param4;
		//m_SpreadFactorChangePerShot = m_WeaponSpreadParam.param5;
		//m_ShrinkEquationType = (EquationUtility.EquationType)(int)m_WeaponSpreadParam.param6;
		//m_ShrinkTime = m_WeaponSpreadParam.param7;
	}

#region 武器散布范围扩和收缩张的函数

	///// <summary>
	///// 时间因子转化为散布角度因子
	///// </summary>
	///// <param name="spreadTimeFactor"></param>
	///// <returns></returns>
	//private float TimeFactorToSpreadAngleFactor(float spreadTimeFactor)
	//{
	//	return EquationUtility.CalculateEquation(m_SpreadEquationType, spreadTimeFactor
	//		, m_DebugInfo.SpreadCoef3, m_DebugInfo.SpreadCoef2, m_DebugInfo.SpreadCoef1, m_DebugInfo.SpreadCoef0);
	//}

	///// <summary>
	///// 散布角度因子转化为时间因子
	///// </summary>
	///// <param name="angleFactor"></param>
	///// <returns></returns>
	//private float SpreadAngleFactorToTimeFactor(float angleFactor)
	//{
	//	return EquationUtility.CalculateInverseEquation(m_SpreadEquationType, angleFactor
	//		, m_DebugInfo.SpreadCoef3, m_DebugInfo.SpreadCoef2, m_DebugInfo.SpreadCoef1, m_DebugInfo.SpreadCoef0);
	//}

	///// <summary>
	///// 时间因子转化为收缩角度因子
	///// </summary>
	///// <param name="shrinkTimeFactor"></param>
	///// <returns></returns>
	//private float TimeFactorToShrinkAngleFactor(float shrinkTimeFactor)
	//{
	//	return EquationUtility.CalculateEquation(m_ShrinkEquationType, shrinkTimeFactor
	//		, m_DebugInfo.ShrinkCoef3, m_DebugInfo.ShrinkCoef2, m_DebugInfo.ShrinkCoef1, m_DebugInfo.ShrinkCoef0);
	//}

#endregion
}
