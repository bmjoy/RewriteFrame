using Leyoutech.Core.Effect;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class Effect3DViewer : Base3DViewer
{
	/// <summary>
	/// RawImage
	/// </summary>
	private RawImage m_RawImageComponent;
	/// <summary>
	/// 环境预置件路径
	/// </summary>
	private string m_EnvironmentPath;
	/// <summary>
	/// 子模型
	/// </summary>
	protected ModelInfo[] m_ModelInfoArray;
	/// <summary>
	/// 效果预置件路径
	/// </summary>
	private string m_EffectPath;
	/// <summary>
	/// 渲染纹理
	/// </summary>
	private RenderTexture m_RenderTexture;
	/// <summary>
	/// 渲染纹理大小
	/// </summary>
	private int m_RenderTextureSize = 2048;

	/// <summary>
	/// 是否已无效
	/// </summary>
	private bool m_Dirty = false;
	/// <summary>
	/// 重载ID
	/// </summary>
	private uint m_ReloadID = 0;

	/// <summary>
	/// 已加载的环境
	/// </summary>
	private GameObject m_LightEnvirontment;
	/// <summary>
	/// 相机的初始坐标
	/// </summary>
	private Vector3 m_CameraLocalPosition;
	/// <summary>
	/// 相机的初始旋转
	/// </summary>
	private Vector3 m_CameraLocalRotatiton;

	/// <summary>
	/// 避免GC
	/// </summary>
	private List<Renderer> m_MeshListCache = new List<Renderer>();

    /// <summary>
    /// 模型信息
    /// </summary>
    public struct ModelInfo
    {
        /// <summary>
        /// 预置件
        /// </summary>
        public string perfab;
        /// <summary>
        /// 位置
        /// </summary>
        public Vector3 position;
        /// <summary>
        /// 旋转
        /// </summary>
        public Vector3 rotation;
        /// <summary>
        /// 缩放
        /// </summary>
        public Vector3 scale;
    }

    /// <summary>
    /// RawImage
    /// </summary>
    protected RawImage RawImageComponent
	{
		get
		{
			if (!m_RawImageComponent)
				m_RawImageComponent = GetComponent<RawImage>();
			return m_RawImageComponent;
		}
	}

	/// <summary>
	/// 环境Prefab路径
	/// </summary>
	public string EnvirontmentPerfab
	{
		get { return m_EnvironmentPath; }
		set
		{
			if (SetProperty(ref m_EnvironmentPath, value))
				SetDirty();
		}
	}

	/// <summary>
	/// 模型信息数组
	/// </summary>
	public ModelInfo[] ModelInfoArray
	{
		get { return m_ModelInfoArray; }
		set
		{
			m_ModelInfoArray = value;
			SetDirty();
		}
	}
	/// <summary>
	/// 效果Prefab路径
	/// </summary>
	public string EffectPerfab
	{
		get { return m_EffectPath; }
		set { SetProperty(ref m_EffectPath, value); }
	}

	/// <summary>
	/// 相机
	/// </summary>
	public Camera Camera { get; private set; }
	/// <summary>
	/// 模型容器
	/// </summary>
	public Transform ModelBox { get; private set; }
	/// <summary>
	/// 模型实例集合
	/// </summary>
	public GameObject[] ModelArray { get; private set; }
	/// <summary>
	/// 效果实例列表
	/// </summary>
	public EffectController[] EffectArray { get; private set; }
	/// <summary>
	/// 模型包围盒
	/// </summary>
	public Bounds ModelBounds { get; private set; }
	/// <summary>
	/// 模型包围盒对象线长度
	/// </summary>
	public float ModelBoundsDiagonalLength { get; private set; }

	/// <summary>
	/// 模型是否旋转
	/// </summary>
	public bool ModelRotate { get; set; }
	/// <summary>
	/// 自动调整相机旋转和距离
	/// </summary>
	public bool AutoAdjustBestRotationAndDistance { get; set; } = false;

	/// <summary>
	/// 纹理大小
	/// </summary>
	public int TextureSize
	{
		get { return m_RenderTextureSize; }
		set
		{
			if (SetProperty(ref m_RenderTextureSize, Mathf.Max(2, value)))
			{
				if (m_RenderTexture && m_LightEnvirontment)
				{
                    RenderTexture.ReleaseTemporary(m_RenderTexture);

                    m_RenderTexture = RenderTexture.GetTemporary(TextureSize, TextureSize, 8, RenderTextureFormat.BGRA32);

					RawImageComponent.texture = m_RenderTexture;

					if (Camera != null)
					{
						Camera.targetTexture = m_RenderTexture;
						OnRectTransformDimensionsChange();
					}
				}
			}
		}
	}

	private void OnDestroy()
	{
		Unload();
	}

	/// <summary>
	/// 设置属性值
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="currentValue"></param>
	/// <param name="newValue"></param>
	private bool SetProperty<T>(ref T currentValue, T newValue)
	{
		if ((currentValue != null || newValue != null) && (currentValue == null || !currentValue.Equals(newValue)))
		{
			currentValue = newValue;
			return true;
		}
		return false;
	}


	/// <summary>
	/// 加载模型
	/// </summary>
	/// <param name="environmentPerfab">光照环境</param>
	/// <param name="models">模型列表</param>
	/// <param name="effectPerfab">效果</param>
	public void LoadModel(string environmentPerfab, ModelInfo[] models, string effectPerfab)
	{
		EnvirontmentPerfab = environmentPerfab;
		ModelInfoArray = models;
		EffectPerfab = effectPerfab;
	}
    /// <summary>
	/// 加载模型
    /// </summary>
	/// <param name="environmentPerfab">光照环境</param>
	/// <param name="models">模型列表</param>
	public void LoadModel(string environmentPerfab, ModelInfo[] models)
	{
		LoadModel(environmentPerfab, models, null);
	}
    /// <summary>
	/// 加载模型
    /// </summary>
	/// <param name="environmentPerfab">光照环境</param>
    /// <param name="modelPerfab">模型</param>
    /// <param name="effectPerfab">特效</param>
    /// <param name="position">坐标</param>
    /// <param name="rotation">旋转</param>
    /// <param name="scale">缩放</param>
	public void LoadModel(string environmentPerfab, string modelPerfab, string effectPerfab, Vector3 position, Vector3 rotation, Vector3 scale)
	{
		LoadModel(environmentPerfab, new ModelInfo[] { new ModelInfo() { perfab = modelPerfab, position = position, rotation = rotation, scale = scale } }, effectPerfab);
	}
    /// <summary>
	/// 加载模型
    /// </summary>
	/// <param name="environmentPerfab">光照环境</param>
    /// <param name="modelPerfab">模型</param>
    /// <param name="position">坐标</param>
    /// <param name="rotation">旋转</param>
    /// <param name="scale">缩放</param>
	public void LoadModel(string environmentPerfab, string modelPerfab, Vector3 position, Vector3 rotation, Vector3 scale)
	{
		LoadModel(environmentPerfab, modelPerfab, null, position, rotation, scale);
	}
    /// <summary>
	/// 加载模型
    /// </summary>
	/// <param name="environmentPerfab">光照环境</param>
    /// <param name="modelPerfab">模型</param>
    /// <param name="effectPerfab">特效</param>
	public void LoadModel(string environmentPerfab, string modelPerfab, string effectPerfab)
	{
		LoadModel(environmentPerfab, modelPerfab, effectPerfab, Vector3.zero, Vector3.zero, Vector3.one);
	}
    /// <summary>
	/// 加载模型
    /// </summary>
	/// <param name="environmentPerfab">光照环境</param>
    /// <param name="modelPerfab">模型</param>
	public void LoadModel(string environmentPerfab, string modelPerfab)
	{
		LoadModel(environmentPerfab, modelPerfab, null, Vector3.zero, Vector3.zero, Vector3.one);
	}

	/// <summary>
	/// 清空模型
	/// </summary>
	public void ClearModel()
	{
		EnvirontmentPerfab = null;
		ModelInfoArray = null;
		EffectPerfab = null;
	}

	/// <summary>
	/// 标记模型无效
	/// </summary>
	private void SetDirty()
	{
		if (!m_Dirty)
		{
			m_Dirty = true;

			Unload();
            if(gameObject.activeInHierarchy&&gameObject.activeSelf)
			StartCoroutine(WaitOneFrameAndLoad());
		}
	}

	/// <summary>
	/// 等待一帧并加载
	/// </summary>
	/// <returns></returns>
	private IEnumerator WaitOneFrameAndLoad()
	{
		yield return new WaitForEndOfFrame();

		if (m_Dirty)
		{
			m_Dirty = false;
			m_ReloadID++;

			if (!string.IsNullOrEmpty(m_EnvironmentPath) && m_ModelInfoArray != null && m_ModelInfoArray.Length > 0)
				LoadEnvirontment();
		}
	}

	/// <summary>
	/// 卸载
	/// </summary>
	private void Unload()
	{
		RawImageComponent.texture = null;
		RawImageComponent.enabled = false;

		ModelBox = null;
		ModelBounds = new Bounds();
		ModelBoundsDiagonalLength = 0;

		if (EffectArray != null)
		{
			foreach (EffectController effect in EffectArray)
			{
				if (effect != null)
					effect.RecycleFX();
			}
			EffectArray = null;
		}

		if (ModelArray != null && ModelArray.Length > 0)
		{
			foreach (GameObject child in ModelArray)
			{
				if (child)
				{
					SetToLayer(child, false);
					ResetModel(child);
					child.SetActive(false);
					child.Recycle();
				}
			}
			ModelArray = null;
		}

		if (Camera != null)
		{
			Camera.transform.localPosition = m_CameraLocalPosition;
			Camera.transform.localEulerAngles = m_CameraLocalRotatiton;
			Camera.targetTexture = null;
			Camera.ResetAspect();
			Camera = null;
		}

		if (m_LightEnvirontment)
		{
			SetToLayer(m_LightEnvirontment, false);
			m_LightEnvirontment.SetActive(false);
			m_LightEnvirontment.Recycle();
			m_LightEnvirontment = null;
		}

		ResetDragable();
		RebuildModelBounds();
	}

    /// <summary>
    /// 重置模型
    /// </summary>
    /// <param name="model">模型</param>
	private void ResetModel(GameObject model)
	{
		foreach (Renderer renderer in model.GetComponentsInChildren<Renderer>(true))
		{
			renderer.enabled = true;
		}
		foreach (Transform transform in model.GetComponentsInChildren<Transform>(true))
		{
			transform.gameObject.SetActive(true);
		}
	}

	/// <summary>
	/// 加载环境
	/// </summary>
	private void LoadEnvirontment()
	{
		uint reloadID = m_ReloadID;
		string path = m_EnvironmentPath;
		AssetUtil.LoadAssetAsync(path,
			(pathOrAddress, returnObject, userData) =>
			{
				if (returnObject != null)
				{
					GameObject prefab = (GameObject)returnObject;
					if (reloadID == m_ReloadID && path.Equals(m_EnvironmentPath) && m_ModelInfoArray != null && m_ModelInfoArray.Length > 0)
					{
						prefab.CreatePool(1, path);
						m_LightEnvirontment = prefab.Spawn();
						m_LightEnvirontment.SetActive(false);

						ModelBox = m_LightEnvirontment.transform.Find("UI3DBox");
						ModelBox.localEulerAngles = Vector3.zero;
						ModelBox.localScale = Vector3.one;

						LoadModel(0);
					}
				}
				else
				{
					Debug.LogError(string.Format("资源加载成功，但返回null,  pathOrAddress = {0}", pathOrAddress));
				}
			});
	}

    /// <summary>
    /// 加载模型
    /// </summary>
    /// <param name="i">索引</param>
	private void LoadModel(int i)
	{
		if (m_ModelInfoArray != null && i < m_ModelInfoArray.Length)
		{
			uint reloadID = m_ReloadID;

			int index = i;
			string path = m_ModelInfoArray[i].perfab;

			if (!string.IsNullOrEmpty(path))
			{
				AssetUtil.LoadAssetAsync(path,
					(pathOrAddress, returnObject, userData) =>
					{
						if (reloadID != m_ReloadID) return;
						if (m_LightEnvirontment == null) return;
						if (m_ModelInfoArray == null) return;
						if (index >= m_ModelInfoArray.Length) return;
						if (!path.Equals(m_ModelInfoArray[index].perfab)) return;

						WarShipRotate autoMoveAndRotate = ModelBox.GetOrAddComponent<WarShipRotate>();
						if (autoMoveAndRotate != null)
						{
							autoMoveAndRotate.IsRotate = ModelRotate;
						}

						GameObject prefab = returnObject as GameObject;
						prefab.CreatePool(pathOrAddress);

						if (ModelArray == null)
							ModelArray = new GameObject[m_ModelInfoArray.Length];

						ModelArray[index] = prefab.Spawn(ModelBox);
                        ModelArray[index].SetActive(true);
                        ModelArray[index].transform.localEulerAngles = m_ModelInfoArray[index].rotation;
                        ModelArray[index].transform.localPosition = m_ModelInfoArray[index].position;
                        ModelArray[index].transform.localScale = m_ModelInfoArray[index].scale;

						if (index < ModelArray.Length - 1)
						{
							LoadModel(index + 1);
						}
						else
						{
							RebuildModelBounds();

							if (!string.IsNullOrEmpty(EffectPerfab))
							{
								LoadEffect();
							}
							else
							{
								InstallModelAndEffect();
							}
						}
					});
			}
		}
	}

	/// <summary>
	/// 加载特效Prefab
	/// </summary>
	private void LoadEffect()
	{
		uint reloadID = m_ReloadID;
		string path = m_EffectPath;

		EffectArray = new EffectController[ModelArray.Length];
		for (int i = 0; i < ModelArray.Length; i++)
		{
			GameObject model = ModelArray[i];
			if (model)
			{
				EffectController effectController = EffectManager.GetInstance().CreateEffect(m_EffectPath, EffectManager.GetEffectGroupNameInSpace(true),
					(effect,usedata) =>
					{
                        Effect3DViewer viewer = (Effect3DViewer)usedata;
                        if(viewer != null)
                        {
                            if (reloadID != viewer.m_ReloadID) return;
                            if (viewer.m_LightEnvirontment == null) return;
                            if (viewer.ModelArray == null) return;
                            if (!path.Equals(viewer.m_EffectPath)) return;

                            viewer. InstallModelAndEffect();
                            effect.PlayFX();//加载完特效再显示
                        }
					}
                    ,this);

				if (ModelArray != null)
				{
					effectController.transform.SetParent(model.transform, false);
					effectController.SetCreateForMainPlayer(true);
					effectController.StopFX(true);//特效未加载完不显示
				}
				else
				{
					effectController.StopFX(true);
				}

				EffectArray[i] = effectController;
			}
			else
			{
				EffectArray[i] = null;
			}
		}
	}

	/// <summary>
	/// 安装模型与效果
	/// </summary>
	private void InstallModelAndEffect()
	{
		SetToLayer(m_LightEnvirontment, true);

		if (m_RenderTexture == null)
            m_RenderTexture = RenderTexture.GetTemporary(TextureSize, TextureSize, 8, RenderTextureFormat.BGRA32);

		Camera = m_LightEnvirontment.GetComponentInChildren<Camera>();
		if (Camera != null)
		{
			Camera.targetTexture = m_RenderTexture;
			OnRectTransformDimensionsChange();
		}

		m_LightEnvirontment.SetActive(true);

		ResetDragable();

		m_CameraLocalPosition = Camera.transform.localPosition;
		m_CameraLocalRotatiton = Camera.transform.localEulerAngles;

		if (AutoAdjustBestRotationAndDistance)
			AdjustCameraToBestDistance(true);

		StartCoroutine(DelayOneFrameShow());

		///
		if (ModelArray != null && ModelArray.Length > 0)
		{
			Animator animator = ModelArray[0].GetComponent<Animator>();
			if (animator)
			{
				animator.SetTrigger("Work");
			}
		}

		if (m_Lookat)
			SetTarget();
	}

	/// <summary>
	/// 调整相机到一个最优的距离
	/// </summary>
	public void AdjustCameraToBestDistance(bool lookat)
	{
		if (Camera && ModelBox)
		{
            float bestDistance = ModelBoundsDiagonalLength / 2.0f / Mathf.Tan(Camera.fieldOfView * Mathf.Deg2Rad / 2.0f);
			Vector3 direction = (Camera.transform.position - ModelBox.transform.position).normalized;
			Camera.transform.position = ModelBox.position + direction * bestDistance;
			if (lookat)
				Camera.transform.LookAt(ModelBox);
		}
	}

	/// <summary>
	/// 延迟一帧显示
	/// </summary>
	/// <returns>IEnumerator</returns>
	private IEnumerator DelayOneFrameShow()
	{
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();

		RawImageComponent.texture = m_RenderTexture;
		RawImageComponent.enabled = true;
	}

	/// <summary>
	/// 尺寸发生变化时,重设相机宽高比.
	/// </summary>
	private void OnRectTransformDimensionsChange()
	{
		if (Camera != null)
		{
			RectTransform rect = GetComponent<RectTransform>();
			Camera.aspect = rect.rect.width / rect.rect.height;
		}
	}

	/// <summary>
	/// 启用时
	/// </summary>
	private void OnEnable()
	{
		RawImageComponent.texture = null;
		RawImageComponent.enabled = false;

		m_Dirty = false;

		SetDirty();
	}

	/// <summary>
	/// 禁用时
	/// </summary>
	private void OnDisable()
	{
		if (m_RenderTexture != null)
		{
            RenderTexture.ReleaseTemporary(m_RenderTexture);
			m_RenderTexture = null;
		}

		Unload();
	}

	/// <summary>
	/// 重置拖动信息
	/// </summary>
	private void ResetDragable()
	{
		CharacterRotation component = GetComponent<CharacterRotation>();
		if (component)
		{
			component.target = ModelArray != null ? ModelBox : null;
			component.normalAngle = 0;
			component.ResetAngle();
		}
	}

	/// <summary>
	/// 重建模型包围盒
	/// </summary>
	public void RebuildModelBounds()
	{
		int skinCount = 0;
		float minX = float.MaxValue;
		float minY = float.MaxValue;
		float minZ = float.MaxValue;
		float maxX = float.MinValue;
		float maxY = float.MinValue;
		float maxZ = float.MinValue;

		if (ModelArray != null)
		{
			foreach (GameObject model in ModelArray)
            {
                m_MeshListCache.Clear();
                model.GetComponentsInChildren(true, m_MeshListCache);
				foreach (Renderer mesh in m_MeshListCache)
				{
					if (mesh is MeshRenderer || mesh is SkinnedMeshRenderer)
					{
						Bounds bounds = mesh.bounds;
                        minX = Mathf.Min(minX, bounds.min.x);
						minY = Mathf.Min(minY, bounds.min.y);
						minZ = Mathf.Min(minZ, bounds.min.z);
						maxX = Mathf.Max(maxX, bounds.max.x);
						maxY = Mathf.Max(maxY, bounds.max.y);
						maxZ = Mathf.Max(maxZ, bounds.max.z);
						skinCount++;
					}
				}
			}
		}
		m_MeshListCache.Clear();

		Bounds newBounds = new Bounds();
		if (skinCount > 0)
		{
			newBounds.min = new Vector3(minX, minY, minZ);
			newBounds.max = new Vector3(maxX, maxY, maxZ);
		}

		ModelBounds = newBounds;
		ModelBoundsDiagonalLength = skinCount > 0 ? ModelBounds.size.magnitude : 0;
    }

	/// <summary>
	/// 绘制边界盒
	/// </summary>
	private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
		Gizmos.DrawWireCube(ModelBounds.center, ModelBounds.size);
	}






	/// <summary>
	/// 是否拖拽
	/// </summary>
	private bool m_Dragable;

	/// <summary>
	/// 旋转速度
	/// </summary>
	private float m_RotateSpeed;

	/// <summary>
	/// 是否LOOKAT
	/// </summary>
	private bool m_Lookat;

	/// <summary>
	/// 设置渲染目标
	/// </summary>
	/// <param name="target"></param>
	private void SetTarget()
	{
		ModelMouseHandler handler = RawImageComponent.gameObject.GetOrAddComponent<ModelMouseHandler>();
		RectTransform rectTransform = RawImageComponent.GetComponent<RectTransform>();
		HumanoidComponent ui3dHumanoid = ModelArray != null && ModelArray.Length > 0 ? ModelArray[0].GetComponentInChildren<HumanoidComponent>() : null;
		Camera camera = m_LightEnvirontment.GetComponentInChildren<Camera>();
		handler.SetData(ModelBox, m_Dragable, m_RotateSpeed, m_Lookat, camera, rectTransform, ui3dHumanoid);
	}
	public void SetLookAtData(bool dragable = true, float rotateSpeed = 0, bool lookat = true)
	{
		m_Dragable = dragable;
		m_RotateSpeed = rotateSpeed;
		m_Lookat = lookat;
	}
}
