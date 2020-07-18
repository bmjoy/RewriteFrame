using UnityEngine;

namespace Map
{
	[ExecuteInEditMode]
	public abstract class BaseSceneVFX : MonoBehaviour, ILODItem
	{
#if UNITY_EDITOR
		[EditorExtend.Button("应用修改", "_ApplyChanged", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)]
		public bool _ApplyChangedButton;
		[EditorExtend.Button("应用LOD", "InitializeLOD", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)]
		public bool _ApplyLODButton;
		[EditorExtend.Button("移除未保存的节点", "_RemoveNotSaveChildern", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)]
		public bool _RemoveNotSaveChildernButton;

		/// <summary>
		/// 是否需要在编辑模式预览效果，最好在Disable这个Component时改变这个选项
		/// 改变后，需要重新Enable这个Component 生效
		/// </summary>
		public bool _NeedPreviewInEditMode = false;
		/// <summary>
		/// 是否需要在编辑器下预览LOD
		/// </summary>
		[Tooltip("是否需要在编辑器下预览LOD")]
		public bool _NeedPreviewLOGInEditMode = false;
		public bool _EnableGizmos = false;
		public Color _GizmosColor = Color.red;
#endif

		/// <summary>
		/// 如果这个节点是LODGroup的子节点时，当LODGroup在这个LOD以下时才会显示这个特效
		/// </summary>
		[Tooltip("如果这个节点是LODGroup的子节点时，当LODGroup在这个LOD以上时才会显示这个特效")
			, EditorExtend.ToggleInt(Constants.NOTSET_LOD_INDEX
				, Style = EditorExtend.ToggleIntAttribute.FieldStyle.IntSlider
				, MinValue = 0
				, MaxValue = 7)]
		public int MaxDisplayLODIndex = Constants.NOTSET_LOD_INDEX;

		protected LODGroup m_LODGroup;
		protected bool m_LODItemActive;

		private bool m_IsInitialized = false;

#if UNITY_EDITOR
		/// <summary>
		/// 是否需要执行
		/// </summary>
		private bool _NeedExecuteInEditMode;
#endif

		public bool IsInitialized()
		{
			return m_IsInitialized;
		}

		public void DoBeforeInitialize()
		{
			InitializeLOD();
		}

		public abstract void DoInitialize();

		public abstract void DoRelease();

		public void DoAfterRelease()
		{
			// 编辑时是通过OnEnable\OnDisable触发Initialize Release的
			if (Application.isPlaying)
			{
				LODManager.GetInstance().RemoveLODItem(this);
				m_LODGroup = null;
			}
		}

		public bool CanUpdate()
		{
			return m_LODItemActive
#if UNITY_EDITOR
				&& (_NeedExecuteInEditMode || Application.isPlaying)
#endif
				;
		}

		/// <summary>
		/// TODO 目前是每帧都会调用一次，但是将来会改成根据距离、优先级等因素，动态变更更新频率
		/// </summary>
		public abstract void DoUpdate(float time, float deltaTime);

		public abstract Bounds CaculateAABB();

		public void SetLODItemActive(bool active)
		{
#if UNITY_EDITOR
			if (!Application.isPlaying
				&& !_NeedPreviewLOGInEditMode)
			{
				return;
			}
#endif
			if (m_LODItemActive != active)
			{
				m_LODItemActive = active;
				gameObject.SetActive(active);
			}
		}

		public LODGroup GetLODGroup()
		{
			return m_LODGroup;
		}

		public int GetMaxDisplayLODIndex()
		{
			return MaxDisplayLODIndex;
		}

		public bool IsAlive()
		{
			return this != null;
		}

		public void DoUpdateLOD(int lodIndex)
		{
		}

		protected void Awake()
		{
#if UNITY_EDITOR
			if (Application.isPlaying)
#endif
			{
				DoBeforeInitialize();
				DoInitialize();
				m_IsInitialized = true;
			}
		}

		protected void OnDestroy()
		{
#if UNITY_EDITOR
			if (IsInitialized()
				&& Application.isPlaying)
#endif
            {
                m_IsInitialized = false;

				DoRelease();
				DoAfterRelease();
			}
		}

		protected void LateUpdate()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				return;
			}
#endif
			if (CanUpdate())
			{
				DoUpdate(Time.time, Time.deltaTime);
			}
		}

		protected void InitializeLOD()
		{
			m_LODItemActive = true;
			if (MaxDisplayLODIndex != Constants.NOTSET_LOD_INDEX)
			{
				m_LODGroup = transform.GetComponentInParent<LODGroup>();
				if (m_LODGroup)
				{
					LODManager.GetInstance().AddLODItem(this);
				}
			}
		}

#if UNITY_EDITOR
		protected void OnEnable()
		{
			_NeedExecuteInEditMode = !Application.isPlaying &&
				(_NeedPreviewInEditMode || _EnableForceExecuteInEditor());

			if (!Application.isPlaying)
			{
				_DoEditorInitialize();
			}

			if (_NeedExecuteInEditMode)
			{
				DoBeforeInitialize();
				DoInitialize();
				m_IsInitialized = true;
			}

			if (!Application.isPlaying)
			{
				EditorExtend.UpdateAnyway.GetInstance().GetMakeSureParticleSimulateBehaviour().AddParticle(gameObject);

				EditorExtend.UpdateAnyway.GetInstance().UpdateAction += _OnEditorUpdate;
			}
		}

		protected void OnDisable()
		{
			if (!Application.isPlaying)
			{
				EditorExtend.UpdateAnyway.GetInstance().UpdateAction -= _OnEditorUpdate;
			}

			if (IsInitialized())
			{
				m_IsInitialized = false;

				if (_NeedExecuteInEditMode)
				{
					DoRelease();
					DoAfterRelease();
				}

				if (!Application.isPlaying)
				{
					_DoEditorRelease();
				}
			}
		}

		protected void OnDrawGizmosSelected()
		{
			if (_EnableGizmos)
			{
				Gizmos.color = _GizmosColor;
				_DoDrawGizmos();
			}
		}

		protected virtual void _DoDrawGizmos()
		{
		}

		/// <summary>
		/// 有子类继承，如果return true，则相当于勾选<see cref="_NeedPreviewInEditMode"/>
		/// </summary>
		/// <returns></returns>
		protected virtual bool _EnableForceExecuteInEditor()
		{
			return false;
		}

		/// <summary>
		/// 只在Edit Mode下执行的Update，用于美术编辑VFX时，时时生效
		/// </summary>
		protected virtual void _DoEditorInitialize()
		{
		}

		/// <summary>
		/// 只在Edit Mode下执行的Update，用于美术编辑VFX时，时时生效
		/// </summary>
		protected virtual void _DoEditorRelease()
		{
		}

		/// <summary>
		/// 只在Edit Mode下执行的Update，用于美术编辑VFX时，时时生效
		/// </summary>
		protected virtual void _DoEditorUpdate(float time, float deltaTime)
		{
		}

		protected virtual void _ApplyChanged()
		{
			enabled = false;
			enabled = true;
		}

		protected virtual void _RemoveNotSaveChildern()
		{
			bool enabledBefore = enabled;
			enabled = false;
			for (int iChild = transform.childCount - 1; iChild >= 0; iChild--)
			{
				Transform iterChild = transform.GetChild(iChild);
				if (iterChild.hideFlags == HideFlags.DontSave)
				{
					DestroyImmediate(iterChild.gameObject);
				}
			}
			enabled = enabledBefore;
		}

		private void _OnEditorUpdate(float time, float delta)
		{
			if (gameObject.activeInHierarchy)
			{
				_DoEditorUpdate(time, delta);
			}

			if (CanUpdate())
			{
				DoUpdate(time, delta);
			}
		}
#endif
	}
}