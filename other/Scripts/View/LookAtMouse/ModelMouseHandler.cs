using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 鼠标拖动响应类
/// </summary>
public class ModelMouseHandler : MonoBehaviour, IDragHandler
{
	/// <summary>
	/// 旋转目标模型
	/// </summary>
	private Transform m_TargetModel;


	/// <summary>
	/// 是否拖拽
	/// </summary>
	private bool m_Dragable;


	/// <summary>
	/// 旋转速度
	/// </summary>
	private float m_RotateSpeed;


	/// <summary>
	/// 是否LOOKAT
	/// </summary>
	private bool m_Lookat;


	/// <summary>
	/// 模型相机
	/// </summary>
	private Camera m_ModelCamera;


	/// <summary>
	/// 3转2 RawImage
	/// </summary>
	private RectTransform m_RawImageTransform;


	/// <summary>
	/// 模型动作控制脚本
	/// </summary>
	private HumanoidComponent m_Ui3dHumanoid;


	/// <summary>
	/// 前方
	/// </summary>
	private Vector3 m_Forward;


	/// <summary>
	/// 人的方向向量
	/// </summary>
	private Vector3 m_HumanoidDIR;


	/// <summary>
	/// 屏幕坐标
	/// </summary>
	private Vector2 m_LocalPoint;


	/// <summary>
	/// rawImage x方向偏离
	/// </summary>
	private float m_RawImageOffsetX;


	/// <summary>
	/// rawImage y方向偏离
	/// </summary>
	private float m_RawImageOffsetY;


	/// <summary>
	/// 相机 x方向偏离
	/// </summary>
	private float m_ModelCameraX;


	/// <summary>
	/// 相机 y方向偏离
	/// </summary>
	private float m_ModelCameraY;

	/// <summary>
	/// 填充数据
	/// </summary>
	/// <param name="targetModel">旋转目标模型</param>
	/// <param name="dragable">是否拖拽</param>
	/// <param name="rotateSpeed">旋转速度</param>
	/// <param name="lookat">是否LOOKAT</param>
	/// <param name="modelCamera">模型相机</param>
	/// <param name="rawImageTransform">RawImage</param>
	/// <param name="ui3dHumanoid">模型动作控制脚本</param>
	public void SetData(Transform targetModel, bool dragable, float rotateSpeed, bool lookat, Camera modelCamera,
		RectTransform rawImageTransform, HumanoidComponent ui3dHumanoid)
	{
		m_TargetModel = targetModel;
		m_Dragable = dragable;
		m_RotateSpeed = rotateSpeed;
		m_Lookat = lookat;
		m_ModelCamera = modelCamera;
		m_RawImageTransform = rawImageTransform;
		m_Ui3dHumanoid = ui3dHumanoid;
	}

	public void OnDrag(PointerEventData eventData)
	{
		if (m_Dragable)
		{
			m_TargetModel.Rotate(0, -eventData.delta.x, 0);
		}
	}

	void OnEnable()
	{
		//ServerTimeUtil.Instance.OnTick += OnUpdate;
	}

	void OnDisable()
	{
		//ServerTimeUtil.Instance.OnTick -= OnUpdate;
	}

	private void Update()
	{
		if (m_TargetModel && !Mathf.Approximately(m_RotateSpeed, 0))
		{
			m_TargetModel.Rotate(0, m_RotateSpeed, 0);
		}

		if (m_TargetModel)
		{
			if (!Mathf.Approximately(m_RotateSpeed, 0))
			{
				m_TargetModel.Rotate(0, m_RotateSpeed, 0);
			}
			if (m_Lookat && m_Ui3dHumanoid)
			{
				m_Forward = m_ModelCamera.transform.position - m_Ui3dHumanoid.transform.position;
				m_HumanoidDIR = m_Ui3dHumanoid.transform.TransformDirection(Vector3.forward);
				if (Mathf.Abs(Vector3.Angle(m_Forward, m_HumanoidDIR)) < 180)
				{
					if (RectTransformUtility.ScreenPointToLocalPointInRectangle(m_RawImageTransform, InputManager.Instance.GetCurrentVirtualCursorPos(), m_ModelCamera, out m_LocalPoint))
					{
						m_RawImageOffsetX = m_LocalPoint.x - m_RawImageTransform.rect.xMin - m_RawImageTransform.rect.width / 2;
						m_RawImageOffsetY = m_LocalPoint.y - m_RawImageTransform.rect.yMin - m_RawImageTransform.rect.height / 2;

						m_ModelCameraX = Screen.width / 2 + m_RawImageOffsetX;
						m_ModelCameraY = Screen.height / 2 + m_RawImageOffsetY;

						m_Ui3dHumanoid.SetIkActive(true);
						m_Ui3dHumanoid.SetLookPos(m_ModelCamera.ScreenToWorldPoint(new Vector3(m_ModelCameraX, m_ModelCameraY, Mathf.Abs(m_Forward.z / 2))));
					}
				}
				else
				{
					m_Ui3dHumanoid.SetIkActive(false);
					m_Ui3dHumanoid.SetLookPos(Vector3.zero);
				}
			}
		}
	}
}

