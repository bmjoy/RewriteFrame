using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public abstract class Common3DViewer : Base3DViewer
{
	/// <summary>
	/// 光照、相机
	/// </summary>
	protected GameObject m_Lighting;

	/// <summary>
	/// 模型
	/// </summary>
	protected GameObject m_Model;

	/// <summary>
	/// 模型的父节点
	/// </summary>
	protected Transform m_ModelRoot;

	/// <summary>
	/// 模型相机
	/// </summary>
	protected Camera m_ModelCamera;

	/// <summary>
	/// RawImage
	/// </summary>
	protected RawImage m_RawImage;

	/// <summary>
	/// 渲染纹理
	/// </summary>
	protected RenderTexture m_RenderTexture;

	/// <summary>
	/// 当前模型地址
	/// </summary>
	protected string m_ModelPath;

	/// <summary>
	/// 下一个模型地址
	/// </summary>
	protected string m_ModelPathNext;

	/// <summary>
	/// 最后一次加载的模型地址
	/// </summary>
	protected string m_ModelPathLast;

	/// <summary>
	/// 初始坐标
	/// </summary>
	protected Vector3 m_Position;

	/// <summary>
	/// 初始旋转
	/// </summary>
	protected Vector3 m_Rotation;

	/// <summary>
	/// 初始缩放
	/// </summary>
	protected Vector3 m_Scale;

	/// <summary>
	/// 加载中
	/// </summary>
	private bool m_LightingLoading = false;

	/// <summary>
	/// 获取贴图大小
	/// </summary>
	/// <returns>uint</returns>
	protected abstract int GetTextureSize();

	/// <summary>
	/// 获取光照预置件路径
	/// </summary>
	/// <returns>路径</returns>
	protected abstract string GetLightPrefabPath();

	/// <summary>
	/// 获取基础旋转
	/// </summary>
	/// <returns>euler角</returns>
	protected abstract Vector3 GetBaseRotation();

	/// <summary>
	/// 获取基础偏移
	/// </summary>
	/// <returns>本地坐杯</returns>
	protected abstract Vector3 GetBasePosition();

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
	/// 设置模型
	/// </summary>
	/// <param name="path">资源地址</param>
	public void SetModel(string path)
	{
		SetModel(path, Vector3.zero, Vector3.zero, Vector3.zero);
	}

	/// <summary>
	/// 设置模型
	/// </summary>
	/// <param name="path">资源地址</param>
	/// <param name="position">初始坐标</param>
	/// <param name="eulerRotation">初始旋转</param>
	/// <param name="scale">初始缩放</param>
	public void SetModel(string path, Vector3 position, Vector3 eulerRotation, Vector3 scale)
	{
		if (!string.IsNullOrEmpty(path))
		{
			m_Position = position;
			m_Rotation = eulerRotation;
			m_Scale = scale;

			if (!path.Equals(m_ModelPath))
			{
				m_ModelPathNext = path;

				LoadPrefab();
			}
			else
			{
				if (m_Model != null)
				{
					m_Model.transform.rotation = Quaternion.Euler(GetBaseRotation() + m_Rotation);
					m_Model.transform.localPosition = GetBasePosition() + m_Position;
					m_Model.transform.localScale = m_Scale;

                    CheckCharacterRotationComponent();

                }
			}
		}
		else
		{
			Unload();
		}
	}

	/// <summary>
	/// 启用时
	/// </summary>
	private void OnEnable()
	{
		//SetModel("Human_F_01_skin");

		if (!string.IsNullOrEmpty(m_ModelPathLast))
		{
			SetModel(m_ModelPathLast);
		}
	}

	/// <summary>
	/// 禁用时
	/// </summary>
	private void OnDisable()
	{
		m_ModelPathLast = string.IsNullOrEmpty(m_ModelPathNext) ? m_ModelPathNext : m_ModelPath;

		Unload();
	}

	/// <summary>
	/// 尺寸发生变化时,重设相机宽高比.
	/// </summary>
	private void OnRectTransformDimensionsChange()
	{
		if (m_ModelCamera != null)
		{
			RectTransform rect = m_RawImage.GetComponent<RectTransform>();
			m_ModelCamera.aspect = rect.rect.width / rect.rect.height;
		}
	}

	/// <summary>
	/// 加载光照组
	/// </summary>
	private void LoadPrefab()
	{
		if (m_Lighting != null)
		{
			LoadModel();
		}
		else if (!m_LightingLoading)
		{
			m_LightingLoading = true;

            string path = GetLightPrefabPath();

            AssetUtil.LoadAssetAsync(path,
                (pathOrAddress, returnObject, userData) =>
                {
                    if (returnObject != null)
                    {
                        if (pathOrAddress.Equals(path))
                        {
                            GameObject prefab = (GameObject)returnObject;
                            prefab.CreatePool(1, pathOrAddress);
                            m_Lighting = prefab.Spawn();
                            m_Lighting.gameObject.SetActive(false);
                            m_LightingLoading = false;
                            m_ModelRoot = m_Lighting.transform.Find("UI3DBox");

                            LoadModel();
                        }
                    }
                    else
                    {
                        Debug.LogError(string.Format("资源加载成功，但返回null,  pathOrAddress = {0}", pathOrAddress));
                    }
                });
		}
	}

	/// <summary>
	/// 加载模型
	/// </summary>
	private void LoadModel()
	{
		if (m_Model != null)
		{
			m_Model.Recycle();
			m_Model = null;
			m_ModelPath = null;
		}

		string path = m_ModelPathNext;

        if (string.IsNullOrEmpty(path))
            return;

        AssetUtil.LoadAssetAsync(path,
            (pathOrAddress, returnObject, userData) =>
            {
                if (returnObject != null)
                {
                    GameObject prefab = (GameObject)returnObject;

                    if (path.Equals(m_ModelPathNext))
                    {
                        m_ModelPath = m_ModelPathNext;
                        m_ModelPathNext = null;

                        prefab.CreatePool(1, path);
                        m_Model = prefab.Spawn(m_ModelRoot);
                        m_Model.transform.rotation = Quaternion.Euler(GetBaseRotation() + m_Rotation);
                        m_Model.transform.localPosition = GetBasePosition() + m_Position;
                        m_Model.transform.localScale = m_Scale;

                        m_RenderTexture = m_RenderTexture != null ? m_RenderTexture : new RenderTexture(GetTextureSize(), GetTextureSize(), 24, RenderTextureFormat.ARGB32);

                        m_ModelCamera = m_Lighting.GetComponentInChildren<Camera>();
                        if (m_ModelCamera != null)
                        {
                            m_ModelCamera.targetTexture = m_RenderTexture;
                        }

                        m_RawImage = GetComponent<RawImage>();
                        if (m_RawImage != null)
                        {
                            m_RawImage.texture = m_RenderTexture;
                            m_RawImage.gameObject.SetActive(true);
                            if (m_ModelCamera != null)
                            {
                                RectTransform rect = m_RawImage.GetComponent<RectTransform>();
                                m_ModelCamera.aspect = rect.rect.width / rect.rect.height;
                            }
                        }

                        m_Lighting.gameObject.SetActive(true);

                        SetToLayer(m_Lighting, true);
                        CheckCharacterRotationComponent();
                        if (m_Lookat)
                        {
                            SetTarget();
                        }
                    }
                }
                else
                {
                    Debug.LogError(string.Format("资源加载成功，但返回null,  pathOrAddress = {0}", pathOrAddress));
                }
            });
	}

	/// <summary>
	/// 卸载
	/// </summary>
	private void Unload()
	{
		SetToLayer(m_Lighting, false);

		if (m_ModelCamera != null)
		{
			m_ModelCamera.ResetAspect();
			m_ModelCamera.targetTexture = null;
			m_ModelCamera = null;
		}


		if (m_RawImage != null)
		{
			m_RawImage.texture = null;
			m_RawImage.gameObject.SetActive(false);
			m_RawImage = null;
		}

		if (m_RenderTexture != null)
		{
			Object.Destroy(m_RenderTexture);
			m_RenderTexture = null;
		}

		if (m_Model)
		{
			m_Model.Recycle();
			m_Model = null;
		}
		if (m_Lighting)
		{
			m_Lighting.Recycle();
            m_Lighting.gameObject.SetActive(false);
            m_Lighting = null;
		}
		m_LightingLoading = false;

		m_ModelPath = null;
		m_ModelPathNext = null;

        CheckCharacterRotationComponent();
    }

    /// <summary>
    /// 检查旋转组件
    /// </summary>
    private void CheckCharacterRotationComponent()
    {
        CharacterRotation component = GetComponent<CharacterRotation>();
        if (component != null)
        {
            component.target = m_ModelRoot != null ? m_ModelRoot.transform : null;
            component.normalAngle = GetBaseRotation().y;
            component.ResetAngle();
        }
    }

    /// <summary>
    /// 设置渲染目标
    /// </summary>
    private void SetTarget()
	{
		ModelMouseHandler handler = m_RawImage.gameObject.GetOrAddComponent<ModelMouseHandler>();
		RectTransform rectTransform = m_RawImage.GetComponent<RectTransform>();
		HumanoidComponent ui3dHumanoid = m_Model.GetComponentInChildren<HumanoidComponent>();
		handler.SetData(m_ModelRoot, m_Dragable, m_RotateSpeed, m_Lookat, m_ModelCamera, rectTransform, ui3dHumanoid);
	}

    /// <summary>
    /// 设置锁定目标
    /// </summary>
    /// <param name="dragable">是否可拖动</param>
    /// <param name="rotateSpeed">旋转速度</param>
    /// <param name="lookat">是否锁定</param>
	public void SetLookAtData(bool dragable = true, float rotateSpeed = 0, bool lookat = true)
	{
		m_Dragable = dragable;
		m_RotateSpeed = rotateSpeed;
		m_Lookat = lookat;
	}
}
