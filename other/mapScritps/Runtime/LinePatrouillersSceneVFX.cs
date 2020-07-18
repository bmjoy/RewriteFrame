using UnityEngine;

namespace Map
{
	public class LinePatrouillersSceneVFX : BaseSceneVFX
	{
		/// <summary>
		/// 负责巡逻的角色Prefab
		/// </summary>
		[Tooltip("负责巡逻的角色Prefab")]
		public GameObject PatrouillerPrefab;
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
		[Tooltip("特性")
			, EditorExtend.EnumFlags(typeof(FeatureFlag))]
		public FeatureFlag MyFeatureFlag = FeatureFlag.EnableMove | FeatureFlag.RandomInitializePosition;

#if UNITY_EDITOR
		/// <summary>
		/// 起始点的类型
		/// </summary>
		[Tooltip("起始点的类型")]
		public PatrouillePointStartType _MyPatrouillePointStartType;
		/// <summary>
		/// 起始点的类型
		/// </summary>
		[Tooltip("结束点的类型")]
		public PatrouillePointEndType _MyPatrouillePointEndType;
		/// <summary>
		/// 是否显示巡逻点
		/// </summary>
		[Tooltip("是否显示巡逻点")]
		public bool _DisplayPatrouillePoint;
#endif
		[Header("跟你没关")
			, Tooltip("跟你没关，别管这个")]
		public Quaternion StartToEndRotation;
		[Tooltip("跟你没关，别管这个")]
		public Quaternion EndToStartRotation;
		[Tooltip("跟你没关，别管这个")]
		public Patrouiller[] Patrouillers;

#if UNITY_EDITOR
		[Tooltip("跟你没关，别管这个")]
		public Vector3 _PatrouillePointStartPosition;
		[Tooltip("跟你没关，别管这个")]
		public Quaternion _PatrouillePointStartRotation;
		[Tooltip("跟你没关，别管这个")]
		public Vector3 _PatrouillePointStartScale = Vector3.one;
		[Tooltip("跟你没关，别管这个")]
		public Vector3 _PatrouillePointEndPosition;

		private Transform _PatrouillePointStart;
		private Transform _PatrouillePointEnd;
#endif

		public override void DoInitialize()
		{
			for (int iPatrouille = 0; iPatrouille < Patrouillers.Length; iPatrouille++)
			{
				Patrouillers[iPatrouille].Transform = Instantiate(PatrouillerPrefab).transform;
				Patrouillers[iPatrouille].Transform.SetParent(transform, false);
				Patrouillers[iPatrouille].Transform.localPosition = Constants.INSTANTIATE_UNIT_POSITION;
				Patrouillers[iPatrouille].Transform.localRotation = StartToEndRotation;

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

		public override Bounds CaculateAABB()
		{
			return new Bounds();
		}

#if UNITY_EDITOR
		protected override void _DoEditorUpdate(float time, float deltaTime)
		{
			base._DoEditorUpdate(time, deltaTime);

			_PatrouillePointStartPosition = _PatrouillePointStart.localPosition;
			_PatrouillePointStartRotation = _PatrouillePointStart.localRotation;
			_PatrouillePointStartScale = _PatrouillePointStart.localScale;

			_PatrouillePointEndPosition = _PatrouillePointEnd.localPosition;
			_PatrouillePointEnd.localRotation = _PatrouillePointStartRotation;

			_PatrouillePointStart.gameObject.SetActive(_DisplayPatrouillePoint);
			_PatrouillePointEnd.gameObject.SetActive(_DisplayPatrouillePoint);

			if (_MyPatrouillePointStartType != PatrouillePointStartType.Plane
				&& _MyPatrouillePointEndType != PatrouillePointEndType.Plane)
			{
				_PatrouillePointEnd.localScale = _PatrouillePointStartScale;
			}
		}

		protected override void _DoEditorInitialize()
		{
			base._DoEditorInitialize();

			PrimitiveType startPrimitiveType = (PrimitiveType)_MyPatrouillePointStartType;
			_PatrouillePointStart = CratePatrouillePoint("Patrouille Point Start"
				, _PatrouillePointStartPosition
				, _PatrouillePointStartRotation
				, _PatrouillePointStartScale
				, startPrimitiveType);

			PrimitiveType endPrimitiveType = _MyPatrouillePointEndType == PatrouillePointEndType.SameStart
				? (PrimitiveType)_MyPatrouillePointStartType
				: PrimitiveType.Sphere;
			_PatrouillePointEnd = CratePatrouillePoint("Patrouille Point End"
				, _PatrouillePointEndPosition
				, _PatrouillePointStartRotation
				, Vector3.one // 缩放会在DoEditorUpdate中更新，这里无所谓				
				, endPrimitiveType);

			StartToEndRotation = Quaternion.LookRotation(_PatrouillePointEndPosition - _PatrouillePointStartPosition);
			EndToStartRotation = Quaternion.LookRotation(_PatrouillePointStartPosition - _PatrouillePointEndPosition);

			Patrouillers = new Patrouiller[PatrouillerCount];
			// 不想在for循环里每次计算，所以在这里提前算好
			Plane endPatrouillePlane = new Plane(_PatrouillePointEndPosition - _PatrouillePointStartPosition, _PatrouillePointEndPosition);
			Vector3 endToStart = _PatrouillePointEndPosition - _PatrouillePointStartPosition;
			Vector3 endToStartNormalized = endToStart.normalized;
			for (int iPatrouiller = 0; iPatrouiller < Patrouillers.Length; iPatrouiller++)
			{
				Vector3 startPosition = Vector3.zero;
				switch (_MyPatrouillePointStartType)
				{
					case PatrouillePointStartType.Sphere:
						{
							Vector3 insideUnitSphere = Random.insideUnitSphere;
							Vector3 insideSphere = Leyoutech.Utility.MathUtility.EachMulti(_PatrouillePointStartScale * 0.5f, insideUnitSphere);
							Vector3 insideSphereWithRotation = _PatrouillePointStartRotation * insideSphere;
							startPosition = _PatrouillePointStartPosition + insideSphereWithRotation;
						}
						break;
					case PatrouillePointStartType.Cube:
						{
							Vector3 insideUnitCube = new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f));
							Vector3 insideCube = Leyoutech.Utility.MathUtility.EachMulti(_PatrouillePointStartScale, insideUnitCube);
							Vector3 insideCubeWithRotation = _PatrouillePointStartRotation * insideCube;
							startPosition = _PatrouillePointStartPosition + insideCubeWithRotation;
						}
						break;
					case PatrouillePointStartType.Plane:
						{
							Vector3 insideUnitPlane = new Vector3(Random.Range(-0.5f, 0.5f), 0.0f, Random.Range(-0.5f, 0.5f));
							Vector3 insidePlane = Leyoutech.Utility.MathUtility.EachMulti(_PatrouillePointStartScale * 10.0f, insideUnitPlane);
							Vector3 insidePlaneWithRotation = _PatrouillePointStartRotation * insidePlane;
							startPosition = _PatrouillePointStartPosition + insidePlaneWithRotation;
						}
						break;
					default:
						Leyoutech.Utility.DebugUtility.Assert(false
							, string.Format("not support PatrouillePointStartType({0})", _MyPatrouillePointStartType)
							, this
							, false);
						break;
				}
				Patrouillers[iPatrouiller].Start = startPosition;

				Vector3 endPosition = Vector3.zero;
				switch (_MyPatrouillePointEndType)
				{
					case PatrouillePointEndType.Plane:
						{
							float startPointToEndPlaneDistance = -endPatrouillePlane.GetDistanceToPoint(Patrouillers[iPatrouiller].Start);
							endPosition = Patrouillers[iPatrouiller].Start
								+ endToStartNormalized * startPointToEndPlaneDistance;
						}
						break;
					case PatrouillePointEndType.SameStart:
						{
							endPosition = Patrouillers[iPatrouiller].Start + endToStart;
						}
						break;
					default:
						Leyoutech.Utility.DebugUtility.Assert(false
							, string.Format("not support PatrouillePointEndType({0})", _MyPatrouillePointEndType)
							, this
							, false);
						break;
				}
				Patrouillers[iPatrouiller].End = endPosition;
			}
		}

		protected override void _DoEditorRelease()
		{
			DestroyImmediate(_PatrouillePointStart.gameObject);
			DestroyImmediate(_PatrouillePointEnd.gameObject);

			base._DoEditorRelease();
		}

		protected override void _DoDrawGizmos()
		{
			//Gizmos.DrawLine(_PatrouillePointStart.position, _PatrouillePointEnd.position);
		}

		private Transform CratePatrouillePoint(string displayName
			, Vector3 position
			, Quaternion rotation
			, Vector3 scale
			, PrimitiveType primitiveType)
		{
			GameObject patrolPointGameObject = GameObject.CreatePrimitive(primitiveType);
			patrolPointGameObject.name = displayName;
			patrolPointGameObject.hideFlags = HideFlags.DontSave;
			DestroyImmediate(patrolPointGameObject.GetComponent<Collider>());

			Transform patrolPointTransform = patrolPointGameObject.transform;
			patrolPointTransform.SetParent(transform, false);
			patrolPointTransform.localPosition = position;
			patrolPointTransform.localRotation = rotation;
			patrolPointTransform.localScale = scale;
			return patrolPointTransform;
		}
#endif

		private void DoUpdatePatrouiller(ref Patrouiller patrouiller, float time, float deltaTime)
		{
			if (patrouiller.TimeProgress > 1)
			{
				if ((MyFeatureFlag & FeatureFlag.MovePingPang) != 0)
				{
					patrouiller.TimeProgress = 1;
					patrouiller.Direction = CacluateDirection(-1.0f);
				}
				else
				{
					patrouiller.TimeProgress = 0;
				}

				patrouiller.Transform.localRotation = patrouiller.Direction > 0
					? StartToEndRotation
					: EndToStartRotation;
			}
			else if (patrouiller.TimeProgress < 0)
			{
				// 只有动态修改PingPang的情况才会进入这个分支
				patrouiller.TimeProgress = 0;
				patrouiller.Direction = CacluateDirection(1.0f);
				patrouiller.Transform.localRotation = StartToEndRotation;
			}
			else
			{
				float timeProgressDelta = deltaTime / StartToEndTime;
				patrouiller.TimeProgress += timeProgressDelta * patrouiller.Direction;
			}

			float progress = StartToEndProgressCurve.Evaluate(patrouiller.TimeProgress);
			patrouiller.Transform.localPosition = patrouiller.Start + (patrouiller.End - patrouiller.Start) * progress;
		}

		private float CacluateDirection(float direction)
		{
			return (1.0f - Random.Range(0, StartToEndTimeNoise)) * direction;
		}

		[System.Serializable]
		public struct Patrouiller
		{
			public Vector3 Start;
			public Vector3 End;

			[HideInInspector]
			public Transform Transform;
			/// <summary>
			/// 0     -> 1
			/// Start -> End
			/// </summary>
			[HideInInspector]
			public float TimeProgress;
			/// <summary>
			/// greater then 0: start to end
			/// less then 0: end to start
			/// </summary>
			[HideInInspector]
			public float Direction;
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

#if UNITY_EDITOR
		public enum PatrouillePointStartType
		{
			Plane = PrimitiveType.Plane,
			Cube = PrimitiveType.Cube,
			Sphere = PrimitiveType.Sphere,
		}

		public enum PatrouillePointEndType
		{
			Plane = PrimitiveType.Plane,
			/// <summary>
			/// 和<see cref="PatrouillePointStartType"/>相同
			/// </summary>
			SameStart = 255,
		}
#endif
	}
}