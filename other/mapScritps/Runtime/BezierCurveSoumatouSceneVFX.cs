using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Map
{
	public class BezierCurveSoumatouSceneVFX : BaseSoumatouSceneVFX
	{
		/// <summary>
		/// 往返巡逻
		/// </summary>
		[Tooltip("往返巡逻")]
		public bool MovePingPang;

		[Header("跟你没关")
			, Tooltip("跟你没关，别管这个")]
		public Vector3[] FragmentPosition;
		[Tooltip("跟你没关，别管这个")]
		public Quaternion[] FragmentRotation;

#if UNITY_EDITOR
		[Tooltip("跟你没关，别管这个")]
		public FragmentPathData _FragmentPathData;
#endif

#if UNITY_EDITOR
		private BezierCurve.BezierCurve _FragmentPath;
#endif

		public override Bounds CaculateAABB()
		{
			throw new System.NotImplementedException();
		}

		/// <summary>
		/// <see cref="MovePingPang"/>为true时生效
		/// 值为1; -1
		/// </summary>
		private int m_NextFocusDirection;

		public override void DoInitialize()
		{
			base.DoInitialize();

			m_NextFocusDirection = ChangeFocusIntervalFragmentCount;

			for (int iFragment = 0; iFragment < FragmentPosition.Length; iFragment++)
			{
				InstantiateFragment(iFragment, FragmentPosition[iFragment], FragmentRotation[iFragment]);
			}
		}

		protected override void MoveToNextFocusFragment()
		{
			DefaultLineMoveToNextFocusFragment(ref m_NextFocusDirection, MovePingPang);
		}

#if UNITY_EDITOR
		protected override void _DoEditorUpdate(float time, float deltaTime)
		{
			base._DoEditorUpdate(time, deltaTime);
		}

		protected override void _DoEditorInitialize()
		{
			base._DoEditorInitialize();

			_FragmentPath = new GameObject("Fragment Path").AddComponent<BezierCurve.BezierCurve>();
			_FragmentPath.gameObject.hideFlags = HideFlags.DontSave;
			_FragmentPath.transform.SetParent(transform, false);
			_FragmentPath.SetCloseCurve(_FragmentPathData.Close);
			_FragmentPath.SetResolution(_FragmentPathData.Resolution);
			for (int iPoint = 0; iPoint < _FragmentPathData.Points.Length; iPoint++)
			{
				_FragmentPath.AddPoint_LocalSpace(_FragmentPathData.Points[iPoint].HandleStyle
					, _FragmentPathData.Points[iPoint].PointPosition
					, _FragmentPathData.Points[iPoint].Handle1Position
					, _FragmentPathData.Points[iPoint].Handle2Position);
			}
		}

		protected override void _DoEditorRelease()
		{
			DestroyImmediate(_FragmentPath.gameObject);

			base._DoEditorRelease();
		}

		protected override void _ApplyChanged()
		{
			_FragmentPathData.Points = new FragmentPathData.Point[_FragmentPath.Points.Count];
			_FragmentPathData.Close = _FragmentPath.IsCloseCurve();
			_FragmentPathData.Resolution = _FragmentPath.GetResolution();
			for (int iPoint = 0; iPoint < _FragmentPath.Points.Count; iPoint++)
			{
				_FragmentPathData.Points[iPoint].HandleStyle = _FragmentPath.Points[iPoint].MyHandleStyle;
				_FragmentPathData.Points[iPoint].PointPosition = _FragmentPath.Points[iPoint].GetPosition_LocalSpace();
				_FragmentPathData.Points[iPoint].Handle1Position = _FragmentPath.Points[iPoint].GetHandle1Position_LocalSpace();
				_FragmentPathData.Points[iPoint].Handle2Position = _FragmentPath.Points[iPoint].GetHandle2Position_LocalSpace();
			}

			FragmentPosition = new Vector3[FragmentCount];
			FragmentRotation = new Quaternion[FragmentCount];
			float delta = 1.0f / FragmentCount;
			for (int iFragment = 0; iFragment < FragmentCount; iFragment++)
			{
				FragmentPosition[iFragment] = _FragmentPath.EvaluateInBezier_LocalSpace(delta * iFragment);
			}

			for (int iFragment = 1; iFragment < FragmentCount; iFragment++)
			{
				FragmentRotation[iFragment] = Quaternion.LookRotation(FragmentPosition[iFragment] - FragmentPosition[iFragment - 1]);
			}
			FragmentRotation[0] = _FragmentPath.IsCloseCurve()
				? Quaternion.LookRotation(FragmentPosition[0] - FragmentPosition[FragmentCount - 1])
				: FragmentRotation[1];

			base._ApplyChanged();
		}
#endif

#if UNITY_EDITOR
		[System.Serializable]
		public struct FragmentPathData
		{
			public bool Close;
			public float Resolution;
			public Point[] Points;

		[System.Serializable]
			public struct Point
			{
				public BezierCurve.BezierPoint.HandleStyle HandleStyle;
				public Vector3 PointPosition;
				public Vector3 Handle1Position;
				public Vector3 Handle2Position;
			}
		}
#endif
	}
}