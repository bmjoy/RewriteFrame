using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 挂在灯光上的脚本, 随着玩家角色到灯光的距离, 修改灯光的强度
/// </summary>
public class LightTrigger : MonoBehaviour
{
	[Tooltip("灯光强度渐变的范围, 与PostProcessVolume的BlendDistance意义一致")]
	public float BlendDistance = 0;
	
	private Light m_Light;
	private SphereCollider m_Collider;

	/// <summary>
	/// 开始时灯光强度. 初始化时记录一下, 省的来回调给弄乱了
	/// </summary>
	private float m_InitialIntensity;
	/// <summary>
	/// 灯光强度百分比
	/// </summary>
	private float m_IntensityPercent;

	/// <summary>
	/// 主角玩家已经进入了灯光控制区域, 进入后开始调节灯光强度
	/// </summary>
	private bool m_MainPlayerEntered;

	/// <summary>
	/// 主角玩家的Transform
	/// </summary>
	private Transform m_MainPlayerTransform;

	private bool m_Valid;

	
    void Awake()
    {
		m_Light = GetComponent<Light>();
		m_Valid = true;

		if (m_Light == null)
		{
			Debug.LogErrorFormat("灯光控制器 {0} 没有设置Light", name);
			m_Valid = false;
		}
		else
		{
			m_InitialIntensity = m_Light.intensity;
		}

		m_Collider = GetComponent<SphereCollider>();
		if (m_Collider == null)
		{
			Debug.LogErrorFormat("灯光控制器 {0} 没有设置球形碰撞区域", name);
			m_Valid = false;
		}
		else
		{
			Vector3 extent = m_Collider.bounds.extents;
			float minExtent = Mathf.Min(Mathf.Min(extent.x, extent.y), extent.z);
			if (minExtent < BlendDistance)
			{
				Debug.LogErrorFormat("灯光控制器 {0} 的BlendDistance 好像设置的有点太大了. 请检查一下", name);
			}
		}

		Reset();
	}

	//private void OnTriggerEnter(SphereCollider other)
	//{
	//	BaseEntity entity = other.attachedRigidbody?.GetComponent<BaseEntity>();
	//	if (entity != null && entity.IsMain())
	//	{
	//		m_MainPlayerEntered = true;
	//		m_MainPlayerTransform = entity.transform;
	//	}
	//}

	//private void OnTriggerExit(Collider other)
	//{
	//	if (m_MainPlayerEntered && m_MainPlayerTransform == other.transform)
	//	{
	//		Reset();
	//	}
	//}

	private void Reset()
	{
		m_MainPlayerEntered = false;
		m_MainPlayerTransform = null;
		m_IntensityPercent = 0;
		m_Light.intensity = m_InitialIntensity * m_IntensityPercent;
	}

	void Update()
    {
		if (!m_Valid)
			return;

		// 找到主角
		if (m_MainPlayerTransform == null)
		{
			GameplayProxy gameplayProxy = GameFacade.Instance.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;
			uint uid = gameplayProxy.GetMainPlayerUID();
			BaseEntity mainPlayer = gameplayProxy.GetEntityById<BaseEntity>(uid);
			if (mainPlayer != null)
				m_MainPlayerTransform = mainPlayer.transform;
		}

		if (m_MainPlayerTransform != null)
		{
			// 判断主角是不是进入了光照触发区域
			float distanceFromCenter = (m_MainPlayerTransform.position - transform.position).magnitude;
			m_MainPlayerEntered = distanceFromCenter < m_Collider.radius;

			// 主角进入光照区域, 调节光强
			if (m_MainPlayerEntered)
			{
				float entryDistance = m_Collider.radius - distanceFromCenter;
				m_IntensityPercent = 0f;
				if (entryDistance > 0)
					m_IntensityPercent = Mathf.Clamp01(entryDistance / BlendDistance);

				m_Light.intensity = m_InitialIntensity * m_IntensityPercent;
			}
			else
			{
				Reset();
			}
		}
	}
}
