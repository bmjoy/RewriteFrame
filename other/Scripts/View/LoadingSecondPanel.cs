using System.Collections.Generic;
using DG.Tweening;
using PureMVC.Interfaces;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.Define;

public class LoadingSecondPanel : UIPanelBase
{
    /// <summary>
    /// 索引下限
    /// </summary>
    private const int IMAGE_INDEX_MIN = 2;
    /// <summary>
    /// 索引上限
    /// </summary>
    private const int IMAGE_INDEX_MAX = 7;
    /// <summary>
    /// 面板资源路径
    /// </summary>
    private const string ASSET_ADDRESS = "Assets/Artwork/UI/Prefabs/LoadingSecondPanel.prefab";
    /// <summary>
    /// 图片资源路径
    /// </summary>
    private const string LOADING_ADDRESS = "Loading_bg_{0}";
    /// <summary>
    /// 背景图
    /// </summary>
    private Image m_Bg;
    /// <summary>
    /// 随机图片索引
    /// </summary>
    private int m_Index;
    /// <summary>
    /// 图片资源加载完成
    /// </summary>
    private bool m_AllImageLoaded = false;
    /// <summary>
    /// 打开动画播放结束
    /// </summary>
    private bool m_OpenEnd = false;
    /// <summary>
    /// 切换场景结束
    /// </summary>
    private bool m_SwitchMapEnd = false;
    /// <summary>
    /// 动画控制器
    /// </summary>
    private UIAnimationEvent m_AnimatorController;
    /// <summary>
    /// 打开参数
    /// </summary>
    private LoadingPanelParamere m_LoadingPanelParamere = null;
    /// <summary>
    /// 已加载列表
    /// </summary>
    private Dictionary<int, Sprite> m_LoadList = new Dictionary<int, Sprite>();

	public LoadingSecondPanel() : base(UIPanel.LoadingSecondPanel, ASSET_ADDRESS, PanelType.Notice)
    {
    }

    public override void Initialize()
    {
        m_Bg = FindComponent<Image>("Content/Image_Loading/Image_Bg");
        m_AnimatorController = GetTransform().GetComponent<UIAnimationEvent>();
    }

    public override void OnShow(object msg)
    {
        base.OnShow(msg);
        m_AnimatorController.OnAnimationEvent += OnAnimationEvent;
        ServerTimeUtil.Instance.OnTick += OnUpdate;
		MSAIBossProxy msab = GameFacade.Instance.RetrieveProxy(ProxyName.MSAIBossProxy) as MSAIBossProxy;
		if (msab != null && msab.GetPlayerIsInBattle())
		{
			WwiseUtil.PlaySound((int)SoundID.LoadDown, false, null);
			// TODO
			msab.SetPlayerIsInBattle(false);
		}
		else
		{
			WwiseUtil.PlaySound((int)SoundID.LoadUp, false, null);
		}
	}

    public override void OnRefresh(object msg)
    {
        InputManager.Instance.UIInputMap = HotKeyMapID.None;

        m_LoadingPanelParamere = (LoadingPanelParamere)msg;
        if (m_Index >= IMAGE_INDEX_MIN)
        {
            m_Bg.sprite = m_LoadList[m_Index];
        }        
    }

    public void OnUpdate()
    {
        if (m_LoadingPanelParamere != null)
        {           
            if (m_OpenEnd && m_SwitchMapEnd)
            {
                m_AnimatorController.Animator.SetTrigger("Exit");               
            }            
        }
    }
    /// <summary>
    /// 收到动画事件
    /// </summary>
    /// <param name="key">事件参数</param>
    private void OnAnimationEvent(string key)
    {       
        if (string.Equals("Enter", key))
        {
            m_LoadingPanelParamere.OnShown();
            UIManager.Instance.ClosePanel(UIPanel.CharacterPanel);
            m_OpenEnd = true;           
        }
        else if (string.Equals("Exit", key))
        {
            UIManager.Instance.ClosePanel(this);
        }        
    }
    public override NotificationName[] ListNotificationInterests()
    {
        return new NotificationName[]
        {
            NotificationName.MSG_SWITCH_SCENE_END
        };
    }

    public override void HandleNotification(INotification notification)
    {
        switch ((NotificationName)notification.Name)
        {
            case NotificationName.MSG_SWITCH_SCENE_END:
                m_SwitchMapEnd = true;
                break;
        }
    }
    public override void OnHide(object msg)
    {
        base.OnHide(msg);
        m_AnimatorController.OnAnimationEvent -= OnAnimationEvent;
        ServerTimeUtil.Instance.OnTick -= OnUpdate;
        m_OpenEnd = false;
        m_SwitchMapEnd = false;
        m_LoadingPanelParamere = null;
		WwiseUtil.PlaySound((int)SoundID.LoadEnd, false, null);
		//如果全加载过了
		//随机选一个 并且return
		if (m_AllImageLoaded)
        {
            m_Index = Random.Range(IMAGE_INDEX_MIN, IMAGE_INDEX_MAX);
            return;
        }
        //选一个没加载过的
        int index;
        while (true)
        {
            index = Random.Range(IMAGE_INDEX_MIN, IMAGE_INDEX_MAX);
            Sprite sprite;
            if (!m_LoadList.TryGetValue(index,out sprite))
            {
                m_Index = index;                
                break;
            }
        }
        //加载并且存一个id
        //如果数量都加载了 就标记m_AllImageLoaded 为true
        string m_BgAddress = string.Format(LOADING_ADDRESS, m_Index);
        AssetUtil.LoadAssetAsync(m_BgAddress,            (pathOrAddress, returnObject, userData) =>            {
                if (returnObject != null)
                {
                    Texture2D texture = (Texture2D)returnObject;
                    m_LoadList.Add(m_Index, Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero));
                    m_AllImageLoaded = m_LoadList.Values.Count == IMAGE_INDEX_MAX - IMAGE_INDEX_MIN;
                }
                else
                {
                    Debug.LogError(string.Format("资源加载成功，但返回null,  pathOrAddress = {0}", pathOrAddress));
                } 
            });
    }
}