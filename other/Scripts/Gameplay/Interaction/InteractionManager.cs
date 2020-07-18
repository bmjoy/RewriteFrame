using System.Collections.Generic;
using UnityEngine;

namespace Interaction
{
	public class InteractionManager : MonoBehaviour
	{
		public const string LOG_TAG = "Interaction";
		private const float SAFE_AREA_X = 0.2f;
		private const float SAFE_AREA_Y = 0.1f;

		private static InteractionManager ms_Instance;

		private IOwnerPlayer m_OwnerPlayer;
		private BetterList<Interactable> m_Interactables;
		/// <summary>
		/// 用于避免重复添加到<see cref="m_Interactables"/>
		/// </summary>
		private HashSet<int> m_InteractableInstanceIDs;
		private IInteractable m_LastFocusInteractable;

		private CameraState m_CameraState;
#if UNITY_EDITOR
		private bool m_EnableGizmos = false;
#endif
		private bool m_DisplayDetail = false;
		private int m_PhysicsLayerMask;

		public static InteractionManager GetInstance()
		{
			if (ms_Instance == null)
			{
				GameObject go = new GameObject("InteractionManager");
				DontDestroyOnLoad(go);
				ms_Instance = go.AddComponent<InteractionManager>();
				ms_Instance.Initialize();
			}
			return ms_Instance;
		}

		public void RegisterInteractable(IInteractable interactable)
		{
			if (m_InteractableInstanceIDs.Add(interactable.GetInstanceID()))
			{
				Leyoutech.Utility.DebugUtility.Log(LOG_TAG, "Register Interactable: " + interactable.GetDisplay());
				m_Interactables.Add(new Interactable(interactable));
			}
		}

		public void UnregisterInteractable(IInteractable interactable)
		{
			if (m_InteractableInstanceIDs.Remove(interactable.GetInstanceID()))
			{
				for (int iInteractable = 0; iInteractable < m_Interactables.Count; iInteractable++)
				{
					if (m_Interactables[iInteractable].Instance == interactable)
					{
						Leyoutech.Utility.DebugUtility.Log(LOG_TAG
							, "Unregister Interactable: " + m_Interactables[iInteractable].Instance.GetDisplay());
						m_Interactables.RemoveAt(iInteractable);
						break;
					}
				}
			}
		}

		public void RegisterOwnerPlayer(IOwnerPlayer ownerPlayer)
		{
			Leyoutech.Utility.DebugUtility.Log(LOG_TAG, "Register Owner Player: " 
				+ (ownerPlayer == null
					? "None"
					: ownerPlayer.GetDisplay()));
			m_OwnerPlayer = ownerPlayer;
		}

		public void UnregisterOwnerPlayer(IOwnerPlayer ownerPlayer)
		{
			if (m_OwnerPlayer == ownerPlayer)
			{
				Leyoutech.Utility.DebugUtility.Log(LOG_TAG
					, "Unregister Owner Player: " + ownerPlayer.GetDisplay());
				m_OwnerPlayer = null;
			}
		}

		public void OnInteracted()
		{
			if (m_LastFocusInteractable != null)
			{
				Leyoutech.Utility.DebugUtility.Log(LOG_TAG, "On Interacted: " + m_LastFocusInteractable.GetDisplay());
				m_LastFocusInteractable.OnInteracted();
			}
			else
			{
				Leyoutech.Utility.DebugUtility.LogWarning(LOG_TAG, "UI回调了交互，但是InteractionManager没有关注的目标，逻辑上可能问题");
			}
		}

		protected void LateUpdate()
		{
			if (m_OwnerPlayer == null
				|| !m_OwnerPlayer.IsAlive())
			{
				if (m_OwnerPlayer != null)
				{
					Leyoutech.Utility.DebugUtility.Log(LOG_TAG
						, "Unregister Owner Player because not alive");
				}
				m_OwnerPlayer = null;
				m_LastFocusInteractable = null;
				return;
			}

			// fill camera state
			if (GameFacade._IsGaming)
			{
				MainCameraComponent cameraComponent = CameraManager.GetInstance().GetMainCamereComponent();
				m_CameraState.HalfTanFov = cameraComponent.GetHalfTanFov();
				m_CameraState.AspectRatio = CameraManager.GetInstance().GetAspectRatio();
			}
			else
			{
				m_CameraState.HalfTanFov = Leyoutech.Utility.RendererUtility.CaculateHalfTanCameraFov(m_CameraState.Camera.fieldOfView);
				m_CameraState.AspectRatio = (float)Screen.width / Screen.height;
			}
			m_CameraState.WorldPosition = m_CameraState.Camera.transform.position;

			// foreach interactable
			Interactable focusInteractable = null;
			float maxWeight = float.MinValue;
			for (int iInteractable = 0; iInteractable < m_Interactables.Count; iInteractable++)
			{
				Interactable iterInteractable = m_Interactables[iInteractable];
				IInteractable iterInteractableInstance = iterInteractable.Instance;
				Interactable.ForDebug forDebug = Interactable.EMPTY_FORDEBUG;
				if (!iterInteractableInstance.IsAlive())
				{
					Leyoutech.Utility.DebugUtility.Log(LOG_TAG
						, "Unregister Interactable because not alive");
					m_Interactables.RemoveAt(iInteractable);
					iInteractable--;
					continue;
				}

#if UNITY_EDITOR
				if (m_EnableGizmos)
				{
					EditorExtend.GizmosHelper.GetInstance().DrawSphere(iterInteractableInstance.GetWorldPosition()
						, iterInteractableInstance.GetInteractableRadius()
						, Color.gray);
				}
#endif

				// 判断距离能否交互
				float interactableToOwnerPlayerDistanceSquare = (iterInteractableInstance.GetWorldPosition() - m_OwnerPlayer.GetWorldPosition()).sqrMagnitude;
				forDebug.ToPlayerDistanceSquare = interactableToOwnerPlayerDistanceSquare;
				if (interactableToOwnerPlayerDistanceSquare > iterInteractableInstance.GetInteractableDistanceSquare())
				{
					m_Interactables[iInteractable]._ForDebug = forDebug;
					continue;
				}
				forDebug.MyAllowStep = Interactable.ForDebug.AllowStep.DistanceToPlayer;

				// 判断是否在屏幕内
				Vector3 viewportPoint = m_CameraState.Camera.WorldToViewportPoint(iterInteractableInstance.GetWorldPosition());
				forDebug.ViewportPoint = viewportPoint;
				// 到 以相机朝向为法线的平面 的距离
				float interactableToCameraPlaneDistance = viewportPoint.z;
				// 在相机后方
				if (interactableToCameraPlaneDistance < 0)
				{
					m_Interactables[iInteractable]._ForDebug = forDebug;
					continue;
				}
				forDebug.MyAllowStep = Interactable.ForDebug.AllowStep.InCameraForward;

				// 到相机的距离
				Vector3 cameraToInteractableDirection = iterInteractableInstance.GetWorldPosition() - m_CameraState.WorldPosition;
				float interactableToCameraDistance = cameraToInteractableDirection.magnitude;
				float interactableRelativeHeight = Leyoutech.Utility.RendererUtility.CaculateRelativeHeightInScreen(iterInteractableInstance.GetInteractableRadius() * 0.5f
					, interactableToCameraDistance
					, m_CameraState.HalfTanFov);
				float interactableRelativeWidth = interactableRelativeHeight * m_CameraState.AspectRatio;
				bool inScreen = viewportPoint.x > SAFE_AREA_X - interactableRelativeWidth
					&& viewportPoint.x < (1.0f + interactableRelativeWidth - SAFE_AREA_X)
					&& viewportPoint.y > SAFE_AREA_Y - interactableRelativeHeight
					&& viewportPoint.y < (1.0f + interactableRelativeHeight - SAFE_AREA_Y);
				forDebug.RelativeSize = new Vector2(interactableRelativeWidth, interactableRelativeHeight);
				if (!inScreen)
				{
					m_Interactables[iInteractable]._ForDebug = forDebug;
					continue;
				}
				forDebug.MyAllowStep = Interactable.ForDebug.AllowStep.InScreen;

				// 到屏幕中心距离
				float interactableToScreenCenterWidth = (viewportPoint.x - 0.5f) * m_CameraState.AspectRatio;
				float interactableToScreenCenterHeight = (viewportPoint.y - 0.5f);
				// 单位是屏幕高度的平方(0表示在正中心，0.25表示距离中心半个屏幕高度)
				float interactableToScreenCenterDistanceSquare = interactableToScreenCenterWidth * interactableToScreenCenterWidth
					+ interactableToScreenCenterHeight * interactableToScreenCenterHeight;
				forDebug.ToScreenCenterDistanceSquare = interactableToScreenCenterDistanceSquare;

				// 计算权重
				float weight = 1.0f
					/ interactableToScreenCenterDistanceSquare
					* interactableRelativeWidth * interactableRelativeWidth
					/ interactableToOwnerPlayerDistanceSquare * iterInteractableInstance.GetInteractableDistanceSquare();
				forDebug.Weight = weight;
				if (weight < maxWeight)
				{
					m_Interactables[iInteractable]._ForDebug = forDebug;
					continue;
				}
				forDebug.MyAllowStep = Interactable.ForDebug.AllowStep.Weight;

				// 检测目标是否被碰撞体遮挡
				bool isRaycast = Physics.Raycast(m_CameraState.WorldPosition
					, cameraToInteractableDirection
					, interactableToCameraDistance - iterInteractableInstance.GetInteractableRadius()
					, m_PhysicsLayerMask);

#if UNITY_EDITOR
				if (m_EnableGizmos)
				{
					EditorExtend.GizmosHelper.GetInstance().DrawLine(m_CameraState.WorldPosition
						, m_CameraState.WorldPosition
							+ cameraToInteractableDirection
								* (interactableToCameraDistance - iterInteractableInstance.GetInteractableRadius())
									/ interactableToCameraDistance
						, isRaycast ? Color.red : Color.green);
				}
#endif

				if (isRaycast)
				{
					m_Interactables[iInteractable]._ForDebug = forDebug;
					continue;
				}
				forDebug.MyAllowStep = Interactable.ForDebug.AllowStep.Raycast;

				maxWeight = weight;
				focusInteractable = iterInteractable;
				m_Interactables[iInteractable]._ForDebug = forDebug;

			}

			// change focus
			bool changedFocus = m_LastFocusInteractable == null
				? focusInteractable != null
				: focusInteractable == null || focusInteractable.Instance != m_LastFocusInteractable;
			if (changedFocus)
			{
				Leyoutech.Utility.DebugUtility.Log(LOG_TAG
					, string.Format("ChangedFocus from ({0}) to ({1}): "
						, m_LastFocusInteractable == null || !m_LastFocusInteractable.IsAlive()
							? "None"
							: m_LastFocusInteractable.GetDisplay()
						, focusInteractable == null || !focusInteractable.Instance.IsAlive()
							? "None"
							: focusInteractable.Instance.GetDisplay()));

				if (m_LastFocusInteractable != null
					&& m_LastFocusInteractable.IsAlive())
				{
					m_LastFocusInteractable.SetFocus(false);
				}

				if (focusInteractable == null)
				{
					m_LastFocusInteractable = null;
				}
				else if (focusInteractable.Instance.GetIsActive())
				{
					m_LastFocusInteractable = focusInteractable.Instance;
					m_LastFocusInteractable.SetFocus(true);
				}
			}

			if (m_LastFocusInteractable != null)
			{
				focusInteractable._ForDebug.MyAllowStep = Interactable.ForDebug.AllowStep.Pass;
			}
		}

		private void Initialize()
		{
			m_PhysicsLayerMask = LayerMask.GetMask("Default");

			m_Interactables = new BetterList<Interactable>();
			m_InteractableInstanceIDs = new HashSet<int>();

			m_CameraState.Camera = GameFacade._IsGaming
				? CameraManager.GetInstance().GetMainCamereComponent().GetCamera()
				: Camera.main;

			DebugPanel.DebugPanelInstance.GetInstance().RegistGUI(DebugPanel.TabName.Interactable, DoGUI, true);
		}

		private void DoGUI(DebugPanel.Config config)
		{
			if (!config.IsEditor)
			{
                config.BeginHorizontal();
				GUILayout.BeginVertical();
			}

            // Buttons
            config.BeginHorizontal();
#if UNITY_EDITOR
			m_EnableGizmos = GUILayout.Toggle(m_EnableGizmos, "Enable Gizmos", config.ToggleStyle);
#endif
			m_DisplayDetail = GUILayout.Toggle(m_DisplayDetail, "Display Detail", config.ToggleStyle);
            config.EndHorizontal();

			// Setps
			System.Text.StringBuilder stringBuilder = Leyoutech.Utility.StringUtility.AllocStringBuilderCache();
			for (int iStep = 0; iStep < (int)Interactable.ForDebug.AllowStep.Count; iStep++)
			{
				if (iStep != 0)
				{
					stringBuilder.Append(" - ");
				}
				stringBuilder.Append(((Interactable.ForDebug.AllowStep)iStep).ToString());
			}
			GUILayout.Box(Leyoutech.Utility.StringUtility.ReleaseStringBuilderCacheAndReturnString(), config.BoxStyle);

			// Player
			GUILayout.Box("Player:" +
				(m_OwnerPlayer != null && m_OwnerPlayer.IsAlive()
					? string.Format("{0} Position:{1}", m_OwnerPlayer.GetDisplay(), m_OwnerPlayer.GetWorldPosition())
					: "None")
			, config.BoxStyle);

			// Focus
			GUILayout.Box("Focus:" +
					(m_LastFocusInteractable != null && m_LastFocusInteractable.IsAlive()
						? m_LastFocusInteractable.GetDisplay()
						: "Uninteractable")
				, config.ImportantBoxStyle);

			// Interactables
			for (int iInteractable = 0; iInteractable < m_Interactables.Count; iInteractable++)
			{
				Interactable iterInteractable = m_Interactables[iInteractable];
				IInteractable iterInteractableInstance = iterInteractable.Instance;
				if (iterInteractableInstance.IsAlive())
				{
					GUILayout.Box(string.Format("Interactable:{0} - Radius:{1:F2} - Distance:{2:F2} Step:{3}"
								, iterInteractableInstance.GetDisplay()
								, iterInteractableInstance.GetInteractableRadius()
								, Mathf.Sqrt(iterInteractableInstance.GetInteractableDistanceSquare())
								, iterInteractable._ForDebug.MyAllowStep)
							+ (m_DisplayDetail
								? iterInteractable._ForDebug.ToDetailInfo()
								: "")
						, config.BoxStyle);
				}
			}

			if (!config.IsEditor)
			{
				GUILayout.FlexibleSpace();
				GUILayout.EndVertical();
				GUILayout.FlexibleSpace();
				GUILayout.BeginVertical();
				GUILayout.EndVertical();
                config.EndHorizontal();
			}
		}

		private struct CameraState
		{
			public Camera Camera;
			public Vector3 WorldPosition;
			public float HalfTanFov;
			public float AspectRatio;
		}

		private class Interactable
		{
			public readonly static ForDebug EMPTY_FORDEBUG = new ForDebug(ForDebug.AllowStep.None);

			public IInteractable Instance;
			internal ForDebug _ForDebug;

			public Interactable(IInteractable instance)
			{
				Instance = instance;
				_ForDebug = EMPTY_FORDEBUG;
			}

			public struct ForDebug
			{
				public AllowStep MyAllowStep;

				public float ToPlayerDistanceSquare;
				public Vector3 ViewportPoint;
				public Vector2 RelativeSize;
				public float ToScreenCenterDistanceSquare;
				public float Weight;

				public ForDebug(AllowStep allowStep)
				{
					MyAllowStep = allowStep;
					ToPlayerDistanceSquare = 0;
					ViewportPoint = Vector3.zero;
					RelativeSize = Vector2.zero;
					ToScreenCenterDistanceSquare = 0;
					Weight = 0;
				}

				public string ToDetailInfo()
				{
					System.Text.StringBuilder stringBuilder = Leyoutech.Utility.StringUtility.AllocStringBuilderCache();
					stringBuilder.Append(string.Format("\nTo Player Distance:{0:F2}"
						, Mathf.Sqrt(ToPlayerDistanceSquare)));

					if (MyAllowStep > AllowStep.None)
					{
						stringBuilder.Append(" - ")
							.Append("Viewport Point:")
							.Append(ViewportPoint);

						if (MyAllowStep > AllowStep.DistanceToPlayer)
						{
							stringBuilder.Append(" - ")
								.Append("Relative Size:")
								.Append(RelativeSize);

							if (MyAllowStep > AllowStep.InCameraForward)
							{
								stringBuilder.Append(string.Format("\nTo Screen Center Distance:{0:F2} - Weight:{1:F2}"
									, Mathf.Sqrt(ToScreenCenterDistanceSquare)
									, Weight));
							}
						}
					}
					return Leyoutech.Utility.StringUtility.ReleaseStringBuilderCacheAndReturnString();
				}

				public enum AllowStep
				{
					None = 0,
					/// <summary>
					/// 距离玩家的距离
					/// </summary>
					DistanceToPlayer,
					/// <summary>
					/// 在相机前方
					/// </summary>
					InCameraForward,
					/// <summary>
					/// 在屏幕内
					/// </summary>
					InScreen,
					/// <summary>
					/// 权重高
					/// </summary>
					Weight,
					/// <summary>
					/// 到玩家之间没有碰撞体
					/// </summary>
					Raycast,
					/// <summary>
					/// 通过
					/// </summary>
					Pass,
					/// <summary>
					/// 用于计数必须放最后
					/// </summary>
					Count,
				}
			}
		}
	}
}