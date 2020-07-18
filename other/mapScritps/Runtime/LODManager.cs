#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;
using UnityEngine;

namespace Map
{
	[ExecuteInEditMode]
	public class LODManager : MonoBehaviour
	{
		private static LODManager ms_Instance;

		private HashSet<int> m_LODItemInstanceIDs = new HashSet<int>();
		private List<LODBehaviour> m_LODs = new List<LODBehaviour>();

		public static LODManager GetInstance()
		{
			if (ms_Instance == null)
			{
				ms_Instance = FindObjectOfType<LODManager>();
				if (ms_Instance == null)
				{
					ms_Instance = new GameObject().AddComponent<LODManager>();
				}
			}
			return ms_Instance;
		}

		public void AddLODItem(ILODItem lodItem)
		{
			if (m_LODItemInstanceIDs.Add(lodItem.GetInstanceID()))
			{
				m_LODs.Add(new LODBehaviour(lodItem));
			}
		}

		public void RemoveLODItem(ILODItem lodItem)
		{
			int instanceID = lodItem.GetInstanceID();
			if (m_LODItemInstanceIDs.Remove(instanceID))
			{
				for (int iLOD = 0; iLOD < m_LODs.Count; iLOD++)
				{
					if (m_LODs[iLOD].Instance.GetInstanceID() == instanceID)
					{
						m_LODs.RemoveAt(iLOD);
						break;
					}
				}
			}
		}

		protected void OnEnable()
		{
			gameObject.hideFlags = HideFlags.HideInHierarchy
				| HideFlags.DontSaveInBuild;
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				EditorExtend.UpdateAnyway.GetInstance().UpdateAction += OnEditorUpdate;
			}
#endif
		}

		protected void OnDisable()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				EditorExtend.UpdateAnyway.GetInstance().UpdateAction -= OnEditorUpdate;
			}
#endif
		}

		protected void LateUpdate()
		{
#if UNITY_EDITOR
			if (Application.isPlaying)
			{
				if (EditorWindow.focusedWindow == null
					|| EditorWindow.focusedWindow.GetType() == Leyoutech.Utility.UnityEditorReflectionUtility.GetGameWindowType())
				{
					DoUpdate(Camera.main, Time.time, Time.deltaTime);
				}
				else
				{
					OnEditorUpdate(Time.time, Time.deltaTime);
				}
			}
#else
			DoUpdate(CameraManager.GetInstance().GetMainCamereComponent().GetCamera(), Time.time, Time.deltaTime);
#endif
		}

		private void DoUpdate(Camera camera, float time, float deltaTime)
		{
			if (camera == null)
			{
				return;
			}

			Vector3 cameraPosition = camera.transform.position;
			float halfTanFov = Leyoutech.Utility.RendererUtility.CaculateHalfTanCameraFov(camera.fieldOfView);

			for (int iLOD = 0; iLOD < m_LODs.Count; iLOD++)
			{
				LODBehaviour iterLOD = m_LODs[iLOD];
				if (iterLOD.Instance.IsAlive())
				{
					iterLOD.DoUpdate(time, deltaTime, cameraPosition, halfTanFov);

					// LODItem is struct, so need set return to m_LODItems
					m_LODs[iLOD] = iterLOD;
				}
				else
				{
					m_LODs.RemoveAt(iLOD);
					iLOD--;
				}
			}
		}

#if UNITY_EDITOR
		private void OnEditorUpdate(float time, float deltaTime)
		{
			if (ms_Instance != null
				&& ms_Instance != this)
			{
				DestroyImmediate(gameObject);
				return;
			}

			if (SceneView.lastActiveSceneView)
			{
				DoUpdate(SceneView.lastActiveSceneView.camera, time, deltaTime);
			}
		}
#endif

		private struct LODBehaviour
		{
			public readonly ILODItem Instance;
			private float m_LastUpdateTime;

			public LODBehaviour(ILODItem instance)
			{
				Instance = instance;
				m_LastUpdateTime = 0;
			}

			public void DoUpdate(float time
				, float deltaTime
				, Vector3 cameraPosition
				, float halfTanFov)
			{
				if (time - m_LastUpdateTime > Constants.LOD_UPDATE_TIME_INTERVAL)
				{
					m_LastUpdateTime = time;
					int lodIndex = Leyoutech.Utility.LODUtility.CaculateCurrentLOGIndex(Instance.GetLODGroup(), cameraPosition, halfTanFov);
					bool lodItemActive = lodIndex <= Instance.GetMaxDisplayLODIndex();
					Instance.SetLODItemActive(lodItemActive);
					Instance.DoUpdateLOD(lodIndex);
				}
			}
		}
	}
}