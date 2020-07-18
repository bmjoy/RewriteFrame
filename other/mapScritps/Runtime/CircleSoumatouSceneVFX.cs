using UnityEngine;

namespace Map
{
	public class CircleSoumatouSceneVFX : BaseSoumatouSceneVFX
	{
        /// <summary>
        /// 走马灯的半径
        /// </summary>
        [Tooltip("走马灯的半径")]
		public float SoumatouRadius;

        public override Bounds CaculateAABB()
		{
			return new Bounds(transform.position
				, Vector3.one * SoumatouRadius);
		}

		public override void DoInitialize()
		{
			base.DoInitialize();

			Vector3 startPosition = new Vector3(SoumatouRadius, 0, 0);
			float deltaRotation = 360.0f / FragmentCount;
			for (int iFragment = 0; iFragment < FragmentCount; iFragment++)
			{
				Quaternion iterRotation = Quaternion.Euler(new Vector3(0, deltaRotation * iFragment, 0));
				InstantiateFragment(iFragment, iterRotation * startPosition, iterRotation);
			}
		}

		protected override void MoveToNextFocusFragment()
		{
			m_FocusFragmentIndex = m_FocusFragmentIndex + ChangeFocusIntervalFragmentCount >= FragmentCount 
                ? m_FocusFragmentIndex + ChangeFocusIntervalFragmentCount - FragmentCount
                : m_FocusFragmentIndex + ChangeFocusIntervalFragmentCount;
		}

#if UNITY_EDITOR
		protected override void _DoDrawGizmos()
		{
			base._DoDrawGizmos();
			Vector3 startPosition = new Vector3(SoumatouRadius, 0, 0);
			float deltaRotation = 360.0f / FragmentCount;
			Vector3 lastPosition = startPosition;
			Vector3 iterPosition = lastPosition;
			for (int iFragment = 0; iFragment < FragmentCount; iFragment++)
			{
				Quaternion iterRotation = Quaternion.Euler(new Vector3(0, deltaRotation * iFragment, 0));
				iterPosition = iterRotation * startPosition;
				Gizmos.DrawLine(transform.TransformPoint(lastPosition), transform.TransformPoint(iterPosition));
				lastPosition = iterPosition;
			}
			Gizmos.DrawLine(transform.TransformPoint(iterPosition), transform.TransformPoint(startPosition));
		}
#endif
	}
}