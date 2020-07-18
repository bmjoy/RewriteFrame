#if UNITY_EDITOR
using UnityEngine;

namespace Interaction
{
	/// <summary>
	/// 测试用的
	/// </summary>
	public class TEST_InteractableUnit : MonoBehaviour, IInteractable
	{
		public float InteractableDistanceSquare;
		public Material Focus;

		private MeshRenderer m_Mesh;
		private Material m_OriginMaterial;
		private SphereCollider m_Collider;

		public string GetDisplay()
		{
			return gameObject.name;
		}

		public float GetInteractableDistanceSquare()
		{
			return InteractableDistanceSquare * InteractableDistanceSquare;
		}

		public float GetInteractableRadius()
		{
			return transform.localScale.x * 0.5f;
		}

		public Vector3 GetWorldPosition()
		{
			return transform.position;
		}

		public bool IsAlive()
		{
			return this != null;
		}

		public bool GetIsActive()
		{
			return this != null;
		}

		public void OnInteracted()
		{
			Debug.LogError("OnInteracted: " + gameObject.name);
		}

		public void SetFocus(bool focus)
		{
			if (focus)
			{
				m_Mesh.material = Focus;
			}
			else
			{
				m_Mesh.material = m_OriginMaterial;
			}
		}

		protected void Awake()
		{
			m_Mesh = gameObject.GetComponent<MeshRenderer>();
			m_OriginMaterial = m_Mesh.material;
			m_Collider = gameObject.GetComponent<SphereCollider>();
			InteractionManager.GetInstance().RegisterInteractable(this);
		}

		protected void OnDestroy()
		{
			InteractionManager.GetInstance().UnregisterInteractable(this);
		}
	}
}
#endif