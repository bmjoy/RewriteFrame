using UnityEngine;

namespace Map
{
	public class BezierCurvePatrouillersSceneVFX : BaseSceneVFX
	{
		/// <summary>
		/// 巡逻者的数量
		/// </summary>
		[Tooltip("巡逻者的数量")]
		public int PatrouillerCount;
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
		/// 巡逻者相对巡逻路径的最大偏移
		/// </summary>
		[Tooltip("巡逻者相对巡逻路径的最大偏移")]
		public float PatrouillerMaxOffset;
		[Tooltip("特性")
			, EditorExtend.EnumFlags(typeof(FeatureFlag))]
		public FeatureFlag MyFeatureFlag = FeatureFlag.EnableMove | FeatureFlag.RandomInitializePosition;
		/// <summary>
		/// 巡逻者的模板
		/// </summary>
		public PatrouillerTemplate[] PatrouillerTemplates;
		/// <summary>
		/// 巡逻的路径
		/// </summary>
		[Tooltip("巡逻的路径")]
		public BezierCurve.BezierCurve PatrouillerPath;

		private Patrouiller[] Patrouillers;

		public override Bounds CaculateAABB()
		{
			throw new System.NotImplementedException();
		}

		public override void DoInitialize()
		{
			float totalWeight = 0;
			for (int iTemplate = 0; iTemplate < PatrouillerTemplates.Length; iTemplate++)
			{
				totalWeight += PatrouillerTemplates[iTemplate].Weight;
			}
			
			Patrouillers = new Patrouiller[PatrouillerCount];
			for (int iPatrouille = 0; iPatrouille < Patrouillers.Length; iPatrouille++)
			{
				float randomWeight = Random.Range(0, totalWeight);
				int templateIndex = 0;
				for (int iTemplate = 0; iTemplate < PatrouillerTemplates.Length; iTemplate++)
				{
					randomWeight -= PatrouillerTemplates[iTemplate].Weight;
					if (randomWeight < 0)
					{
						templateIndex = iTemplate;
						break;
					}
				}

				Patrouillers[iPatrouille].Transform = Instantiate(PatrouillerTemplates[templateIndex].Prefab).transform;
				Patrouillers[iPatrouille].Transform.SetParent(transform, false);
				Patrouillers[iPatrouille].Transform.localPosition = Constants.INSTANTIATE_UNIT_POSITION;

				if ((MyFeatureFlag & FeatureFlag.RandomInitializePosition) != 0)
					{
					Patrouillers[iPatrouille].TimeProgress = Random.Range(0.0f, 1.0f);
					Patrouillers[iPatrouille].Direction = CacluateDirection((MyFeatureFlag & FeatureFlag.MovePingPang) != 0
						? (Random.Range(-1.0f, 1.0f) > 0.0f
							? 1.0f
							: -1.0f)
						: 1.0f);
				}
				else
				{
					Patrouillers[iPatrouille].TimeProgress = 0.0f;
					Patrouillers[iPatrouille].Direction = CacluateDirection(1.0f);
				}

				Patrouillers[iPatrouille].PositionOffset = new Vector3(Random.Range(-PatrouillerMaxOffset, PatrouillerMaxOffset)
					, Random.Range(-PatrouillerMaxOffset, PatrouillerMaxOffset)
					, Random.Range(-PatrouillerMaxOffset, PatrouillerMaxOffset));
				Patrouillers[iPatrouille].LastPatrouillerPosition = EvaluatePatrouillerPosition(Patrouillers[iPatrouille]);
				
				if ((MyFeatureFlag & FeatureFlag.RandomInitializePosition) != 0)
				{
					Patrouillers[iPatrouille].Transform.localPosition = Patrouillers[iPatrouille].LastPatrouillerPosition;
				}

#if UNITY_EDITOR
				Patrouillers[iPatrouille].Transform.gameObject.name = "Patrouiller " + iPatrouille;
				if (!Application.isPlaying)
				{
					Patrouillers[iPatrouille].Transform.gameObject.hideFlags = HideFlags.DontSave;
				}
#endif
			}
		}

		public override void DoRelease()
		{
			if (Patrouillers != null)
			{
				for (int iPatrouille = 0; iPatrouille < Patrouillers.Length; iPatrouille++)
				{
					DestroyImmediate(Patrouillers[iPatrouille].Transform.gameObject);
				}
				Patrouillers = null;
			}
		}

		public override void DoUpdate(float time, float deltaTime)
		{
			if ((MyFeatureFlag & FeatureFlag.EnableMove) != 0)
			{
				for (int iPatrouille = 0; iPatrouille < Patrouillers.Length; iPatrouille++)
				{
					DoUpdatePatrouiller(ref Patrouillers[iPatrouille], time, deltaTime);
				}
			}
		}

#if UNITY_EDITOR
		protected override void _DoEditorInitialize()
		{
			base._DoEditorInitialize();

			if (PatrouillerPath == null)
			{
				PatrouillerPath = gameObject.GetComponent<BezierCurve.BezierCurve>();
				if (PatrouillerPath == null)
				{
					PatrouillerPath = gameObject.AddComponent<BezierCurve.BezierCurve>();
				}
			}
		}
#endif

		private void DoUpdatePatrouiller(ref Patrouiller patrouiller, float time, float deltaTime)
		{
			if (patrouiller.TimeProgress > 1)
			{
				if ((MyFeatureFlag & FeatureFlag.MovePingPang) != 0)
				{
					patrouiller.TimeProgress = 2.0f - patrouiller.TimeProgress;
					patrouiller.Direction = CacluateDirection(-1.0f);
				}
				else
				{
					patrouiller.TimeProgress = patrouiller.TimeProgress - 1.0f;
				}
			}
			else if (patrouiller.TimeProgress < 0)
			{
				if ((MyFeatureFlag & FeatureFlag.MovePingPang) != 0)
				{
					patrouiller.TimeProgress = -patrouiller.TimeProgress;
					patrouiller.Direction = CacluateDirection(1.0f);
				}
				else
				{
					patrouiller.TimeProgress = 1.0f - patrouiller.TimeProgress;
				}
			}
			else
			{
				float timeProgressDelta = deltaTime / StartToEndTime;
				patrouiller.TimeProgress += timeProgressDelta * patrouiller.Direction;
			}

			Vector3 newPatrouillerPosition = EvaluatePatrouillerPosition(patrouiller);
			patrouiller.Transform.localPosition = newPatrouillerPosition;
			patrouiller.Transform.localRotation = Quaternion.LookRotation(newPatrouillerPosition - patrouiller.LastPatrouillerPosition);
			patrouiller.LastPatrouillerPosition = newPatrouillerPosition;
		}

		private Vector3 EvaluatePatrouillerPosition(Patrouiller patrouiller)
		{
			float progress = StartToEndProgressCurve.Evaluate(patrouiller.TimeProgress);
			return PatrouillerPath.EvaluateInBezier_LocalSpace(progress) + patrouiller.PositionOffset;
		}

		private float CacluateDirection(float direction)
		{
			return (1.0f - Random.Range(0, StartToEndTimeNoise)) * direction;
		}

		private struct Patrouiller
		{
			public Transform Transform;
			/// <summary>
			/// 0     -> 1
			/// Start -> End
			/// </summary>
			public float TimeProgress;
			/// <summary>
			/// greater then 0: start to end
			/// less then 0: end to start
			/// </summary>
			public float Direction;
			/// <summary>
			/// 上一帧的坐标，用来计算朝向
			/// </summary>
			public Vector3 LastPatrouillerPosition;
			/// <summary>
			/// 和巡逻路径的偏移
			/// </summary>
			public Vector3 PositionOffset;
		}

		[System.Flags]
		public enum FeatureFlag
		{
			/// <summary>
			/// 启用移动
			/// </summary>
			EnableMove = 1 << 0,
			/// <summary>
			/// 来回移动
			/// </summary>
			MovePingPang = 1 << 1,
			/// <summary>
			/// 随机起始位置
			/// </summary>
			RandomInitializePosition = 1 << 2,
		}

		[System.Serializable]
		public struct PatrouillerTemplate
		{
			public GameObject Prefab;
			/// <summary>
			/// 这个值越高，这个物体出现的频率也就越高
			/// </summary>
			public float Weight;
		}
	}
}