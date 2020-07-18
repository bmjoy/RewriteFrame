using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarShipRotate : MonoBehaviour
{
	public Vector3andSpace moveUnitsPerSecond;
	public Vector3andSpace rotateDegreesPerSecond;
	public bool ignoreTimescale;
	private float m_LastRealTime;
	[HideInInspector]
	public bool IsRotate;
	Vector3 initialLocalPosition;

	private void Start()
	{
		m_LastRealTime = Time.realtimeSinceStartup;
		initialLocalPosition = transform.localPosition;
	}


	// Update is called once per frame
	private void Update()
	{
		if (!IsRotate)
			return;
		float deltaTime = Time.deltaTime;
		if (ignoreTimescale)
		{
			deltaTime = (Time.realtimeSinceStartup - m_LastRealTime);
			m_LastRealTime = Time.realtimeSinceStartup;
		}
		transform.Translate(moveUnitsPerSecond.value * deltaTime, moveUnitsPerSecond.space);
		transform.Rotate(rotateDegreesPerSecond.value * deltaTime, moveUnitsPerSecond.space);

		// 飞船部件使用这个脚本自动旋转的时候, 如果飞船本身也在转动, 挂载这个脚本的部件就会移位
		transform.localPosition = initialLocalPosition;
	}


	[Serializable]
	public class Vector3andSpace
	{
		public Vector3 value;
		public Space space = Space.Self;
	}
}
