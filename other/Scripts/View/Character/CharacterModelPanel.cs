using Eternity.FlatBuffer.Enums;
using PureMVC.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;
using static ServerListProxy;
/// <summary>
/// 创角选角 模型窗口
/// </summary>
public class CharacterModelPanel : UIPanelBase
{
	private const string LOG_TAG = "Login";
	private const string ASSET_ADDRESS = "Assets/Artwork/UI/Prefabs/CharacterModelPanel.prefab";

    /// <summary>
    /// 当前角色voTid
    /// </summary>
    private int m_CurrentTid;
	/// <summary>
	/// 角色模型父节点
	/// </summary>
	private Transform m_CharacterRoot;

	/// <summary>
	///场景父节点
	/// </summary>
	private Transform m_SceneRoot;

	/// <summary>
	/// 模型图片
	/// </summary>
	private RawImage m_ModelImage;

	/// <summary>
	/// 创角Proxy
	/// </summary>
	private ServerListProxy m_ServerListProxy;

	/// <summary>
	/// 角色Proxy
	/// </summary>
	private CfgEternityProxy m_CfgEternityProxy;

	/// <summary>
	/// 创角动画
	/// </summary>
	private Animator m_Animator;
    /// <summary>
    /// 创角相机
    /// </summary>
    private Camera m_Camera;
    /// <summary>
    /// 角色旋转控制器
    /// </summary>
    private CharacterRotation m_RotationContoller;

	/// <summary>
	/// 当前加载角色名字
	/// </summary>
	private string m_CurrentModelName = "";

	/// <summary>
	/// 镜头后处理 
	/// </summary>
	private PostProcessVolume m_PostProcessVolume;

	/// <summary>
	/// 角色状态
	/// </summary>
	private CharacterPanelState m_Status;

	/// <summary>
	/// 角色集合
	/// </summary>
	private Dictionary<string, CharacterState> m_Characters;
	/// <summary>
	/// 当前选择状态
	/// </summary>
	private CharacterState m_CurrentState;

	public CharacterModelPanel() : base(UIPanel.CharacterModelPanel, ASSET_ADDRESS, PanelType.Normal)
	{

	}

	public override void Initialize()
	{
		m_CfgEternityProxy = (CfgEternityProxy)Facade.RetrieveProxy(ProxyName.CfgEternityProxy);
		m_ServerListProxy = (ServerListProxy)Facade.RetrieveProxy(ProxyName.ServerListProxy);
		m_ModelImage = FindComponent<RawImage>("Back/Model_Character");
		m_Animator = FindComponent<Animator>("Character_Layerbenfen/Camera");
        m_Camera = FindComponent<Camera>("Character_Layerbenfen/Camera");
        m_CharacterRoot = FindComponent<Transform>("Character_Layerbenfen/CharacterRoot/Character");
        m_RotationContoller = FindComponent<CharacterRotation>("DragableBack");
        m_RotationContoller.normalAngle = m_CharacterRoot.localEulerAngles.y;
        m_SceneRoot = FindComponent<Transform>("Character_Layerbenfen");
		m_PostProcessVolume = FindComponent<PostProcessVolume>("Character_Layerbenfen/Post");
		CanReceiveFocus = false;
		
	}
	public override void OnShow(object msg)
	{
		m_Characters = new Dictionary<string, CharacterState>();

		m_SceneRoot.SetParent(null);
		m_SceneRoot.transform.localScale = Vector3.one;

        StartUpdate();

		base.OnShow(msg);
	}

	public override void OnHide(object msg)
	{
		m_SceneRoot.SetParent(GetTransform());
		base.OnHide(msg);

		foreach (KeyValuePair<string, CharacterState> kv in m_Characters)
		{
			kv.Value.SetNeed(false);
		}
		m_Characters.Clear();
		m_Characters = null;
	}

	public override void OnRefresh(object msg)
	{
		m_ModelImage.gameObject.SetActive(false);
		m_CurrentTid = 0;
	}

    protected override void Update()
    {
        if(m_Camera)
        {
            m_Camera.aspect = UIManager.Instance.Aspect;
            m_Camera.rect = UIManager.Instance.ViewportRect;
        }
    }

    public override NotificationName[] ListNotificationInterests()
	{
		return new NotificationName[]
		{
			NotificationName.MSG_CHARACTER_CREATE_CURRENT_CHARACTERVO_CHANGE,
			NotificationName.MSG_CHARACTER_CREATE_STATE_CHANGE,
            NotificationName.MSG_CHARACTER_MODEL_ROTATE,
        };
	}

	public override void HandleNotification(INotification notification)
	{
		switch (notification.Name)
		{
			case NotificationName.MSG_CHARACTER_CREATE_CURRENT_CHARACTERVO_CHANGE:
				ShowCharacter(m_ServerListProxy.GetCurrentCharacterVO()?.Tid ?? 0);
				break;
			case NotificationName.MSG_CHARACTER_CREATE_STATE_CHANGE:
				ChangeState((notification.Body as MsgCharacterPanelState).State);
				break;
            case NotificationName.MSG_CHARACTER_MODEL_ROTATE:
                SetModelRotate(notification.Body);
                break;
            default:
				break;
		}
		
	}

	/// <summary>
	/// 设置当前角色模型
	/// </summary>
	/// <param name="tid">模型ID</param>
	private void ShowCharacter(int tid)
	{
		if (tid != m_CurrentTid && tid != 0)
		{
			m_CurrentTid = tid;            
			string prefabName = m_CfgEternityProxy.GetItemModelAssetNameByKey((uint)m_ServerListProxy.GetCurrentCharacterVO().Tid);
            if (m_Characters.TryGetValue(m_CurrentModelName, out CharacterState lastCharacterState))
			{
				if (lastCharacterState.m_Character != null)
				{
					lastCharacterState.m_Character.SetActive(false);
				}
				lastCharacterState.SetNeed(false);
			}

			m_CurrentModelName = prefabName;

			if (!m_Characters.TryGetValue(m_CurrentModelName, out CharacterState currentCharacterState))
			{
				currentCharacterState = new CharacterState(m_CurrentModelName, m_CharacterRoot);
				m_Characters.Add(m_CurrentModelName, currentCharacterState);
			}
			m_CurrentState = currentCharacterState;
			UIManager.Instance.StartCoroutine(Excute(Time.deltaTime,()=> {
                currentCharacterState.SetNeed(true);                
                currentCharacterState.m_IsShow = true;
				if (m_Status == CharacterPanelState.RoleList)
				{
					m_CurrentState.m_IsShow = true;
				}
				if (m_Status == CharacterPanelState.CreatRole)
				{
					if (m_CurrentState.m_IsShow)
					{
						if (m_CfgEternityProxy.GetGenderByTid(m_ServerListProxy.GetCurrentCharacterVO().Tid) == (uint)Gender.Male)//男
						{
							ManChangeAnimator();
						}
						else
						{
							WoManChangeAnimator();
						}
					}
				}
			}));
		}
	}

    /// <summary>
    /// 创角选角状态切换时动画改变
    /// </summary>
    /// <param name="state">操作状态</param>
    private void ChangeState(CharacterPanelState state)
	{
		m_Status = state;
		switch (state)
		{
			case CharacterPanelState.RoleList:
				m_Animator.SetTrigger("Normal");
                m_RotationContoller.ResetAngle();
                m_PostProcessVolume.isGlobal = false;
				m_CurrentTid = 0;
				m_ServerListProxy.SetSkinIndex(0);
				m_CurrentState.m_IsShow = true;
				break;
			case CharacterPanelState.CreatRole:
				UIManager.Instance.StartCoroutine(Excute());
				m_CurrentTid = 0;
				break;
		}
	}
	
	/// <summary>
	/// 延迟调用
	/// </summary>
	/// <returns></returns>
	public IEnumerator Excute()
	{
		ServerInfoVO serverInfo = m_ServerListProxy.GetLastLoginServer();
		if (m_CurrentState != null)
		{
			m_CurrentState.m_IsShow = false;
		}
		yield return new WaitForSeconds(0.2f);
		m_Animator.SetTrigger("Near");
        m_RotationContoller.ResetAngle();
        m_PostProcessVolume.isGlobal = true;
		if (m_CurrentState.m_Character!=null)
		{
			m_CurrentState.m_Character.SetActive(true);
		}
		m_CurrentState.m_IsShow = true;
	}

	/// <summary>
	/// 延迟调用
	/// </summary>
	/// <param name="seconds">秒数</param>
	/// <param name="callBack">回调函数</param>
	/// <returns></returns>
	public static IEnumerator Excute(float seconds, Action callBack)
	{
		yield return new WaitForSeconds(seconds);
		callBack();
	}

	/// <summary>
	/// 女动画
	/// </summary>
	private void ManChangeAnimator()
	{
		m_Animator.SetTrigger("Near");
        m_RotationContoller.ResetAngle();
    }

	/// <summary>
	/// 男动画
	/// </summary>
	private void WoManChangeAnimator()
	{
		m_Animator.SetTrigger("Near_woman");
        m_RotationContoller.ResetAngle();
    }

    /// <summary>
    /// 设置模型是否旋转
    /// </summary>
    /// <param name="obj">数据</param>
    public void SetModelRotate(object obj)
    {
        m_RotationContoller.IsRotate = (bool)obj;
    }
	private class CharacterState
	{
		/// <summary>
		/// 模型名字
		/// </summary>
		public readonly string ModelName;
		/// <summary>
		/// 父节点
		/// </summary>
		public readonly Transform Parent;
		/// <summary>
		/// 当前模型的资源状态
		/// </summary>
		private AssetState m_AssetState;
		/// <summary>
		/// 是否显示
		/// </summary>
		public bool m_IsShow;
		/// <summary>
		/// 这个角色当前是否需要被显示
		/// 用于在Load和Unload的回调中判断
		///		如果为true <see cref="Unload"/>结束后会触发<see cref="LoadAsync"/>
		///		如果为false <see cref="LoadAsync"/>结束后会触发<see cref="Unload"/>
		/// </summary>
		private bool m_Need;
		/// <summary>
		/// 当前模型，只有<see cref="m_AssetState"/>为<see cref="AssetState.Loaded"/>时这个值不为null
		/// </summary>
		public GameObject m_Character;
		/// <summary>
		/// <see cref="m_Character"/>是否在显示
		/// </summary>
		private bool m_Displying;

		public CharacterState(string modelName, Transform parent)
		{
			ModelName = modelName;
			Parent = parent;
			m_AssetState = AssetState.Unload;
			m_Need = false;
			m_Character = null;
			m_Displying = false;
			m_IsShow = true;
		}

		/// <summary>
		/// 设置当前模型是否需要显示
		/// 设置后会根据状态调用<see cref="LoadAsync"/>或<see cref="Unload"/>
		/// </summary>
		/// <param name="need"></param>
		public void SetNeed(bool need)
		{
			Leyoutech.Utility.DebugUtility.Log(LOG_TAG
				, string.Format("Model ({0}) SetNeed {1}"
					, ModelName , need));
			m_Need = need;
			UpdateByNeed();
		}

		/// <summary>
		/// 详见<see cref="m_Need"/>
		/// </summary>
		private void UpdateByNeed()
		{
			switch(m_AssetState)
			{
				case AssetState.Loading:
					// not need handle
					break;
				case AssetState.Loaded:
					if (!m_Need)
					{
						Unload();
					}
					break;
				case AssetState.Unload:
					if (m_Need)
					{
						LoadAsync();
					}
					break;
				default:
					Leyoutech.Utility.DebugUtility.Assert(false, LOG_TAG, "Not support AssetState: " + m_AssetState);
					break;
			}

			if (m_Character)
			{
				if (m_Displying != m_Need)
				{
					m_Character.gameObject.SetActive(m_Need);
					m_Displying = m_Need;
				}
			}
		}

		/// <summary>
		/// 异步加载模型
		/// </summary>
		private void LoadAsync()
		{
			Leyoutech.Utility.DebugUtility.Assert(m_AssetState == AssetState.Unload, LOG_TAG, "m_AssetState == AssetState.Unload");
			m_AssetState = AssetState.Loading;

            AssetUtil.InstanceAssetAsync(ModelName,
            (pathOrAddress, returnObject, userData) =>
            {
                if (returnObject != null)
                {
                    GameObject obj = (GameObject)returnObject;
                    obj.transform.SetParent(Parent, false);
                    OnLoaded(obj);
                }
                else
                {
                    Debug.LogError(string.Format("资源加载成功，但返回null,  pathOrAddress = {0}", pathOrAddress));
                }
            });
        }

		/// <summary>
		/// 加载完成的回调
		/// </summary>
		private void OnLoaded(GameObject obj)
		{

			//Leyoutech.Utility.DebugUtility.Assert<GameObject>(obj, LOG_TAG, "Load Character");
			m_Character = obj;
			Debug.Log(m_IsShow);
			if (!m_IsShow)
			{
				m_Character.SetActive(false);
			}
			m_Character.name = ModelName;
			m_Character.transform.localPosition = Vector3.zero;
			m_Character.transform.localRotation = Quaternion.identity;
			SetLayer(m_Character, "UICreatRole");
			m_AssetState = AssetState.Loaded;
			m_Displying = true;
			UpdateByNeed();
		}

		/// <summary>
		/// 卸载
		/// </summary>
		private void Unload()
		{
			m_Character.gameObject.SetActive(false);
            UnityEngine.Object.Destroy(m_Character);
			m_Character = null;
			m_AssetState = AssetState.Unload;
			m_Displying = false;
			//UpdateByNeed();
		}

		/// <summary>
		/// 设置层级
		/// </summary>
		/// <param name="obj">物体</param>
		/// <param name="layerName">层级名字</param>
		private void SetLayer(GameObject obj, string layerName)
		{
			obj.layer = LayerMask.NameToLayer(layerName);
			for (int i = 0; i < obj.transform.childCount; i++)
			{
				SetLayer(obj.transform.GetChild(i).gameObject, layerName);
			}
		}

		/// <summary>
		/// 初始 <see cref="AssetState.Unload"/>
		/// 玩家选中这个角色 <see cref="AssetState.Loading"/>
		/// 加载完成后 <see cref="AssetState.Loaded"/>
		/// 切换到其他角色 <see cref="AssetState.Unload"/>
		/// </summary>
		public enum AssetState
		{
			Unload,
			Loading,
			Loaded,
		}
	}
}
