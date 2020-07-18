#if UNITY_EDITOR
using UnityEngine;

namespace Interaction
{
	public class TEST_OwnerPlayer : MonoBehaviour, IOwnerPlayer
	{
		protected void OnEnable()
		{
			InteractionManager.GetInstance().RegisterOwnerPlayer(this);
		}

		protected void OnDisable()
		{
			InteractionManager.GetInstance().UnregisterOwnerPlayer(this);
		}

		public Vector3 GetWorldPosition()
		{
			return transform.position;
		}

		public bool IsAlive()
		{
			return this != null;
		}

		public string GetDisplay()
		{
			return gameObject.name;
		}
	}
}
#endif