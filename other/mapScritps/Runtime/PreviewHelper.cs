#if UNITY_EDITOR
using UnityEngine;

namespace Map
{
	public class PreviewHelper : MonoBehaviour
	{
		private static PreviewHelper ms_Instance;

		public float PositionLimit = 1000.0f;

		private Vector3 m_Offset;

		public static PreviewHelper GetInstance()
		{
			return ms_Instance;
		}

		public void SetRealPosition(Vector3 position)
		{
			transform.position = position + m_Offset;
		}

		protected void Start()
		{
			ms_Instance = this;

			Application.targetFrameRate = 60;
			Application.backgroundLoadingPriority = ThreadPriority.Low;
			MapManager.GetInstance();
		}

		protected void LateUpdate()
		{
			if (Mathf.Abs(transform.position.x) > PositionLimit
				|| Mathf.Abs(transform.position.y) > PositionLimit
				|| Mathf.Abs(transform.position.z) > PositionLimit)
			{
				m_Offset -= transform.position;
				transform.position = Vector3.zero;
			}
			MapManager.GetInstance().SetPlayerPosition(transform.position - m_Offset, transform.position);
		}
	}
}
#endif