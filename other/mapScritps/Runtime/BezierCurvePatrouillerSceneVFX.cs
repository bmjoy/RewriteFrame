using UnityEngine;

namespace Map
{
	public class BezierCurvePatrouillerSceneVFX : BaseSceneVFX
	{
		/// <summary>
		/// 负责巡逻的角色
		/// </summary>
		[Tooltip("负责巡逻的角色")]
		public Transform Patrouiller;
		/// <summary>
		/// 往返巡逻
		/// </summary>
		[Tooltip("往返巡逻")]
		public bool MovePingPang;
		/// <summary>
		/// 从Start到End的进度曲线
		/// </summary>
		[Tooltip("从Start到End的进度曲线")]
		public AnimationCurve StartToEndProgressCurve;
		/// <summary>
		/// 从Start到End需要用的时间
		/// </summary>
		[Tooltip("从Start到End需要用的时间")]
		public float StartToEndTime;
		/// <summary>
		/// 会影响<see cref="StartToEndTime"/>
		/// 这个值越大，每次从Start到End的时间相差就越多
		/// 范围0~1
		/// </summary>
		[Tooltip("这个值越大，每次从Start到End的时间相差就越多\n范围0~1")
			, Range(0, 1)]
		public float StartToEndTimeNoise;
		/// <summary>
		/// 巡逻的路径
		/// </summary>
		[Tooltip("巡逻的路径")]
		public BezierCurve.BezierCurve PatrouillerPath;

		/// <summary>
		/// <see cref="Patrouiller"/>上一帧的坐标，用来计算朝向
		/// </summary>
		private Vector3 m_LastPatrouillerPosition;
		/// <summary>
		/// 0     -> 1
		/// Start -> End
		/// </summary>
		private float m_TimeProgress;
		/// <summary>
		/// greater then 0: start to end
		/// less then 0: end to start
		/// </summary>
		private float m_Direction;

		public override void DoInitialize()
		{
			m_TimeProgress = 0;
			m_Direction = CacluateDirection(1.0f);
			m_LastPatrouillerPosition = EvaluatePatrouillerPosition(m_TimeProgress);
		}

		public override void DoRelease()
		{
		}

		public override void DoUpdate(float time, float deltaTime)
		{
			if (m_TimeProgress > 1)
			{
				if (MovePingPang)
				{
					m_TimeProgress = 1;
					m_Direction = CacluateDirection(-1.0f);
				}
				else
				{
					m_TimeProgress = 0;
				}
			}
			else if (m_TimeProgress < 0)
			{
				// 只有动态修改PingPang的情况才会进入这个分支
				m_TimeProgress = 0;
				m_Direction = CacluateDirection(1.0f);
			}
			else
			{
				float timeProgressDelta = deltaTime / StartToEndTime;
				m_TimeProgress += timeProgressDelta * m_Direction;
			}

			Vector3 newPatrouillerPosition = EvaluatePatrouillerPosition(m_TimeProgress);
			Patrouiller.localPosition = newPatrouillerPosition;
			Patrouiller.localRotation = Quaternion.LookRotation(newPatrouillerPosition - m_LastPatrouillerPosition);
			m_LastPatrouillerPosition = newPatrouillerPosition;
		}

		private Vector3 EvaluatePatrouillerPosition(float timeProgress)
		{
			float progress = StartToEndProgressCurve.Evaluate(timeProgress);
			return PatrouillerPath.EvaluateInBezier_LocalSpace(progress);
		}

		public override Bounds CaculateAABB()
		{
			throw new System.NotImplementedException();
		}

#if UNITY_EDITOR
		protected override void _DoEditorInitialize()
		{
			base._DoEditorInitialize();

			if (PatrouillerPath == null)
			{
				PatrouillerPath = gameObject.GetComponent<BezierCurve.BezierCurve>();
				if (PatrouillerPath==null)
				{
					PatrouillerPath = gameObject.AddComponent<BezierCurve.BezierCurve>();
				}
			}
		}
#endif

		private float CacluateDirection(float direction)
		{
			return (1.0f - Random.Range(0, StartToEndTimeNoise)) * direction;
		}
	}
}