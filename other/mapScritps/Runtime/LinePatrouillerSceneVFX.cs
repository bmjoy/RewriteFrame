using UnityEngine;

namespace Map
{
	public class LinePatrouillerSceneVFX : BaseSceneVFX
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

		[Header("跟你没关")
			, Tooltip("跟你没关，别管这个")]
		public Vector3 PatrouillePointStartPosition;
		[Tooltip("跟你没关，别管这个")]
		public Vector3 PatrouillePointEndPosition;

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

#if UNITY_EDITOR
		private Transform _PatrouillePointStart;
		private Transform _PatrouillePointEnd;
#endif

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

				Patrouiller.localRotation = m_Direction > 0
					? Quaternion.LookRotation(PatrouillePointEndPosition - PatrouillePointStartPosition)
					: Quaternion.LookRotation(PatrouillePointStartPosition - PatrouillePointEndPosition);
			}
			else if (m_TimeProgress < 0)
			{
				// 只有动态修改PingPang的情况才会进入这个分支
				m_TimeProgress = 0;
				m_Direction = CacluateDirection(1.0f);
				Patrouiller.localRotation = Quaternion.LookRotation(PatrouillePointEndPosition - PatrouillePointStartPosition);
			}
			else
			{
				float timeProgressDelta = deltaTime / StartToEndTime;
				m_TimeProgress += timeProgressDelta * m_Direction;
			}

			float progress = StartToEndProgressCurve.Evaluate(m_TimeProgress);
			Patrouiller.localPosition = PatrouillePointStartPosition + (PatrouillePointEndPosition - PatrouillePointStartPosition) * progress;
		}

		public override Bounds CaculateAABB()
		{
			return new Bounds(transform.position + PatrouillePointStartPosition + (PatrouillePointEndPosition - PatrouillePointStartPosition) * 0.5f
				, Vector3.one * (PatrouillePointEndPosition - PatrouillePointStartPosition).magnitude);
		}

		public override void DoInitialize()
		{
			m_TimeProgress = 0;
			m_Direction = CacluateDirection(1.0f);
		}

		public override void DoRelease()
		{
		}

#if UNITY_EDITOR
		protected override void _DoEditorUpdate(float time, float deltaTime)
		{
			base._DoEditorUpdate(time, deltaTime);

			PatrouillePointStartPosition = _PatrouillePointStart.localPosition;
			PatrouillePointEndPosition = _PatrouillePointEnd.localPosition;
		}

		protected override void _DoEditorInitialize()
		{
			base._DoEditorInitialize();

			_PatrouillePointStart = CratePatrouillePoint("Patrouille Point Start", PatrouillePointStartPosition);
			_PatrouillePointEnd = CratePatrouillePoint("Patrouille Point End", PatrouillePointEndPosition);
		}

		protected override void _DoEditorRelease()
		{
			DestroyImmediate(_PatrouillePointStart.gameObject);
			DestroyImmediate(_PatrouillePointEnd.gameObject);

			base._DoEditorRelease();
		}

		protected override void _DoDrawGizmos()
		{
			Gizmos.DrawLine(_PatrouillePointStart.position, _PatrouillePointEnd.position);
		}

		private Transform CratePatrouillePoint(string displayName, Vector3 position)
		{
			GameObject patrolPointGameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			patrolPointGameObject.name = displayName;
			patrolPointGameObject.hideFlags = HideFlags.DontSave;
			DestroyImmediate(patrolPointGameObject.GetComponent<Collider>());

			Transform patrolPointTransform = patrolPointGameObject.transform;
			patrolPointTransform.SetParent(transform, false);
			patrolPointTransform.localPosition = position;
			return patrolPointTransform;
		}
#endif

		private float CacluateDirection(float direction)
		{
			return (1.0f - Random.Range(0, StartToEndTimeNoise)) * direction;
		}
	}
}