using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanoidComponent : MonoBehaviour
{
	/// <summary>
	/// 右胳膊
	/// </summary>
	private Transform m_RightHandObj = null;
	/// <summary>
	/// 左胳膊
	/// </summary>
	private Transform m_LeftHandObj = null;
	/// <summary>
	/// 右腿
	/// </summary>
	private Transform m_RightFootObj = null;
	/// <summary>
	/// 左腿
	/// </summary>
	private Transform m_LeftFootObj = null;
	/// <summary>
	/// 目标物体
	/// </summary>
	private Transform m_LookObj = null;
	/// <summary>
	/// 
	/// </summary>
	private Vector3 m_LookOffset = Vector3.zero;
	/// <summary>
	/// 目标坐标
	/// </summary>
	private Vector3 m_LookPos = Vector3.zero;
	public void SetLookPos(Vector3 value)
	{
		m_LookPos = value;
	}
	/// <summary>
	/// 是否设置IK
	/// </summary>
	private bool m_IkActive = false;
	public void SetIkActive(bool value)
	{
		m_IkActive = value;
	}

	/// <summary>
	/// LookAt坐标
	/// </summary>
	private Vector3 m_LookAtTarget = new Vector3(0.0f, 0.0f, 0.0f);
	/// <summary>
	/// 动画组件
	/// </summary>
	private  Animator m_Animator;

	#region 由美术在面板调节调节
	/// <summary>
	/// LookAt权重
	/// </summary>
	[SerializeField, Range(0, 1)]
	private float m_LookAtWeight = 0f;
	/// <summary>
	/// LookAt灵敏度
	/// </summary>
	[SerializeField, Range(1, 10)]
	private int m_lLookAtSensitivity = 5;
	/// <summary>
	/// 身体权重
	/// </summary>
	[SerializeField, Range(0, 1)]
	private float m_BodyWeight = 0.25f;
	/// <summary>
	/// 头权重
	/// </summary>
	[SerializeField, Range(0, 1)]
	private float m_HeadWeight = 0.5f;
	/// <summary>
	/// 眼睛权重
	/// </summary>
	[SerializeField, Range(0, 1)]
	private float m_EyesWeight = 1f;
	/// <summary>
	/// 权重系数限制
	/// </summary>
	[SerializeField, Range(0, 1)]
	private float m_ClampWeight = 0.95f;
	#endregion

	private void Awake()
	{
		m_LookAtWeight = 0;
		m_Animator = GetComponent<Animator>();
	}

	public void Release()
	{
		m_IkActive = false;
		m_LookObj = null;
		m_LookPos = Vector3.zero;
		m_LookAtWeight = 0;

		//base.Release();
	}

	void OnAnimatorIK(int layerIndex)
	{
		if (m_Animator)
		{
			// 假如被指定，设置注视目标的位置
			if (m_LookObj != null)
			{
				m_LookPos = m_LookObj.position + m_LookOffset;
			}

			m_LookAtTarget = Vector3.Lerp(m_LookAtTarget, m_LookPos, Time.deltaTime * m_lLookAtSensitivity);

			//假如IK被激活，设置位置及旋转方向到目标。
			if (m_IkActive)
			{

				m_LookAtWeight = Mathf.Lerp(m_LookAtWeight, 1f, Time.deltaTime * m_lLookAtSensitivity);
				m_Animator.SetLookAtWeight(m_LookAtWeight, m_BodyWeight, m_HeadWeight, m_EyesWeight, m_ClampWeight);
				m_Animator.SetLookAtPosition(m_LookAtTarget);

				// 被指定后，设置手脚的旋转和位置
				if (m_RightHandObj != null)
				{
					m_Animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
					m_Animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);
					m_Animator.SetIKPosition(AvatarIKGoal.RightHand, m_RightHandObj.position);
					m_Animator.SetIKRotation(AvatarIKGoal.RightHand, m_RightHandObj.rotation);
				}
				if (m_LeftHandObj != null)
				{
					m_Animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
					m_Animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
					m_Animator.SetIKPosition(AvatarIKGoal.LeftHand, m_LeftHandObj.position);
					m_Animator.SetIKRotation(AvatarIKGoal.LeftHand, m_LeftHandObj.rotation);
				}
				if (m_RightFootObj != null)
				{
					m_Animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1);
					m_Animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 1);
					m_Animator.SetIKPosition(AvatarIKGoal.RightFoot, m_RightFootObj.position);
					m_Animator.SetIKRotation(AvatarIKGoal.RightFoot, m_RightFootObj.rotation);
				}
				if (m_LeftFootObj != null)
				{
					m_Animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1);
					m_Animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1);
					m_Animator.SetIKPosition(AvatarIKGoal.LeftFoot, m_LeftFootObj.position);
					m_Animator.SetIKRotation(AvatarIKGoal.LeftFoot, m_LeftFootObj.rotation);
				}
			}

			//如果IK没有激活，重置头部、手、脚的旋转和位置
			else
			{
				m_LookAtWeight = Mathf.Lerp(m_LookAtWeight, 0f, Time.deltaTime * m_lLookAtSensitivity);
				m_Animator.SetLookAtWeight(m_LookAtWeight, m_BodyWeight, m_HeadWeight, m_EyesWeight, m_ClampWeight);
				m_Animator.SetLookAtPosition(m_LookAtTarget);

				m_Animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
				m_Animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0);
				m_Animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0);
				m_Animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0);
				m_Animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 0);
				m_Animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 0);
				m_Animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 0);
				m_Animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 0);
			}
		}
	}
}
