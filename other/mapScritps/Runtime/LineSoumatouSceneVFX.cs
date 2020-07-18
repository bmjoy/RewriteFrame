using UnityEngine;

namespace Map
{
	public class LineSoumatouSceneVFX : BaseSoumatouSceneVFX
	{
        /// <summary>
        /// 两个单位之间的距离
        /// </summary>
        [Tooltip("两个单位之间的距离")]
        public float DistanceBetweenFragment;
        /// <summary>
        /// 往返巡逻
        /// </summary>
        [Tooltip("往返巡逻")]
        public bool MovePingPang;

		/// <summary>
		/// <see cref="MovePingPang"/>为true时生效
		/// 值为1; -1
		/// </summary>
		private int m_NextFocusDirection;

		public override Bounds CaculateAABB()
		{
			return new Bounds(transform.position - Vector3.left * FragmentCount * 0.5f * DistanceBetweenFragment
				, Vector3.one * DistanceBetweenFragment + Vector3.right * (FragmentCount - 1) * DistanceBetweenFragment);
		}

		public override void DoInitialize()
		{
			base.DoInitialize();

			m_NextFocusDirection = ChangeFocusIntervalFragmentCount;

			for (int iFragment = 0; iFragment < FragmentCount; iFragment++)
			{
				InstantiateFragment(iFragment, new Vector3(iFragment * DistanceBetweenFragment, 0, 0), Quaternion.identity);
			}
		}

		protected override void MoveToNextFocusFragment()
		{
			DefaultLineMoveToNextFocusFragment(ref m_NextFocusDirection, MovePingPang);
		}

#if UNITY_EDITOR
		protected override void _DoDrawGizmos()
		{
			base._DoDrawGizmos();
			Gizmos.DrawLine(transform.position
				, transform.TransformPoint(new Vector3((FragmentCount - 1) * DistanceBetweenFragment, 0, 0)));
		}
#endif
	}
}