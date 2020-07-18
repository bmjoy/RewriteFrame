using Leyoutech.Utility;
using UnityEngine;

namespace Map
{
	/// <summary>
	/// <see cref="https://ja.wikipedia.org/wiki/%E8%B5%B0%E9%A6%AC%E7%81%AF"/>
	/// </summary>
	public abstract class BaseSoumatouSceneVFX : BaseSceneVFX
	{
        /// <summary>
        /// 关注点的Prefab
        /// </summary>
        [Tooltip("关注点的Prefab")]
        public GameObject FocusFragmentPrefab;
        /// <summary>
        /// 单位的Prefab
        /// </summary>
        [Tooltip("单位的数量")]
        public GameObject FragmentPrefab;
        /// <summary>
        /// 单位的数量
        /// </summary>
        [Tooltip("单位的数量")]
        public int FragmentCount;
        /// <summary>
        /// 切换关注点时，间隔的Fragment数量
        /// </summary>
        [Tooltip("切换关注点时，间隔的Fragment数量")]
        public int ChangeFocusIntervalFragmentCount = 1;
		/// <summary>
		/// 切换Foces的时间间隔，这个数值越小，走马灯的速度越快
		/// </summary>
        [Tooltip("切换Foces的时间间隔，这个数值越小，走马灯的速度越快")]
		public float ChangeTime;
        public FocusMode MyFocusMode;

		protected Vector3[] m_FragmentPositions;
		protected Quaternion[] m_FragmentRotations;
		protected Transform[] m_FragmentTransforms;
		protected Transform m_FocusFragment;
		protected float m_LastChangeFocusTime;
		protected int m_FocusFragmentIndex;
		protected int m_LastFocusFragmentIndex;

		public override void DoInitialize()
		{
			if (FocusFragmentPrefab == null)
			{
				FocusFragmentPrefab = FragmentPrefab;
			}

			m_FragmentPositions = new Vector3[FragmentCount];
			m_FragmentRotations = new Quaternion[FragmentCount];
			m_FragmentTransforms = new Transform[FragmentCount];

			m_FocusFragment = Instantiate(FocusFragmentPrefab, transform).transform;
#if UNITY_EDITOR
			m_FocusFragment.gameObject.hideFlags = HideFlags.DontSave;
			m_FocusFragment.name = "Fragment Focus";
#endif
			// -1: next number is 0
			m_FocusFragmentIndex = -1;
			m_LastChangeFocusTime = -ChangeTime;
			m_LastFocusFragmentIndex = 0;
		}

		public override void DoRelease()
		{
			if (m_FocusFragment)
			{
				DestroyImmediate(m_FocusFragment.gameObject);
				m_FocusFragment = null;
			}

			if (m_FragmentTransforms != null)
			{
				for (int iFragment = 0; iFragment < m_FragmentTransforms.Length; iFragment++)
				{
					DestroyImmediate(m_FragmentTransforms[iFragment].gameObject);
				}
				m_FragmentTransforms = null;
			}


			m_FragmentPositions = null;
			m_FragmentRotations = null;
		}

		public override void DoUpdate(float time, float deltaTime)
		{
			if (time - m_LastChangeFocusTime > ChangeTime)
			{
				m_LastChangeFocusTime = time;
				MoveToNextFocusFragment();
				m_FocusFragment.localPosition = m_FragmentPositions[m_FocusFragmentIndex];
				m_FocusFragment.localRotation = m_FragmentRotations[m_FocusFragmentIndex];

				switch (MyFocusMode)
				{
					case FocusMode.Additive:
						// dont need handle
						break;
					case FocusMode.Replace:
						m_FragmentTransforms[m_LastFocusFragmentIndex].gameObject.SetActive(true);
                        m_FragmentTransforms[m_FocusFragmentIndex].gameObject.SetActive(false);
						break;
					default:
						throw new System.Exception(string.Format("Not support FocusMode({0})", MyFocusMode));
				}

				m_LastFocusFragmentIndex = m_FocusFragmentIndex;
			}
		}

		protected void InstantiateFragment(int index, Vector3 position, Quaternion rotation)
		{
			GameObject iterFragment = Instantiate(FragmentPrefab, transform);
#if UNITY_EDITOR
			iterFragment.hideFlags = HideFlags.DontSave;
			iterFragment.name = "Fragment " + index;
#endif
			m_FragmentTransforms[index] = iterFragment.transform;

			m_FragmentPositions[index] = position;
			m_FragmentTransforms[index].localPosition = position;

			m_FragmentRotations[index] = rotation;
			m_FragmentTransforms[index].localRotation = rotation;
		}

		protected abstract void MoveToNextFocusFragment();

		protected void DefaultLineMoveToNextFocusFragment(ref int direction, bool pingPang)
		{
			if (pingPang)
			{
				m_FocusFragmentIndex += direction;
				if (direction > 0
					&& m_FocusFragmentIndex >= FragmentCount)
				{
					direction = -direction;
					//m_FocusFragmentIndex = FragmentCount - (m_FocusFragmentIndex - FragmentCount + 1) - 1;
					m_FocusFragmentIndex = FragmentCount * 2 - m_FocusFragmentIndex - 2;
				}
				if (direction < 0
					&& m_FocusFragmentIndex < 0)
				{
					direction = -direction;
					m_FocusFragmentIndex = -m_FocusFragmentIndex;
				}
			}
			else
			{
				m_FocusFragmentIndex += direction;
				if (m_FocusFragmentIndex >= FragmentCount)
				{
					m_FocusFragmentIndex -= FragmentCount;
				}
				else if (m_FocusFragmentIndex < 0)
				{
					m_FocusFragmentIndex += FragmentCount;
				}
			}
		}

		public enum FocusMode
		{
			Replace,
			Additive,
		}
	}
}