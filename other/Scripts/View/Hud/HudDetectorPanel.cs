using PureMVC.Interfaces;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
public class HudDetectorPanel : HudBase
{
    private const string ASSET_ADDRESS = Leyoutech.Core.Loader.Config.AssetAddressKey.PRELOADUI_HUD_DETECTORPANEL;
    public HudDetectorPanel() : base(UIPanel.HudDetectorPanel, ASSET_ADDRESS, PanelType.Hud) { }

    /// <summary>
    /// 宝藏的高度
    /// </summary>
    private float m_HeightTreasure;

    /// <summary>
    /// 高度差
    /// </summary>
    private float m_DifferenceTreasure;

    /// <summary>
    /// 最大高度差
    /// </summary>
    private float m_DifferenceMax;

    /// <summary>
    /// 内容
    /// </summary>
    private GameObject m_Content;

    /// <summary>
	/// 玩家主角
	/// </summary>
	private SpacecraftEntity m_MainEntity;

    /// <summary>
	/// GameplayProxy
	/// </summary>
	private GameplayProxy m_GameplayProxy;

    /// <summary>
    /// 标记尺度的Image
    /// </summary>
    private RectTransform m_ImageRect;

    /// <summary>
    /// y 轴比例
    /// </summary>
    private float m_RatioY;
    /// <summary>
    /// 偏移量
    /// </summary>
    private float m_OffsetY;
    /// <summary>
    /// 图片半径
    /// </summary>
    private float m_Radius_Image;
    /// <summary>
    /// 缩放比例
    /// </summary>
    private float m_Scale;
    /// <summary>
    /// 是否显示
    /// </summary>
    private bool m_Show;

    /// <summary>
    /// 高度图标
    /// </summary>
    private RectTransform m_HeightRect;
    /// <summary>
    /// 圆形图标
    /// </summary>
    private RectTransform m_CircularRect;
    /// <summary>
    /// 初始X值
    /// </summary>
    private float m_InitialValueX;
    /// <summary>
    /// 区域ID开启时
    /// </summary>
    private ulong m_AredIdOpen;
    /// <summary>
    /// 区域ID关闭时
    /// </summary>
    private ulong m_AredIdClose;
    /// <summary>
    /// DOTween动画组件
    /// </summary>
    private DOTweenAnimation m_DoTweenAnimation;

    public override void Initialize()
    {
        m_Content = FindComponent<Transform>("Content").gameObject;
        m_HeightRect = FindComponent<RectTransform>("Content/Image_Base/MoveLine/Image");
        m_CircularRect = FindComponent<RectTransform>("Content/Image_Base/MoveLine/Image_MoveCircle");
        m_ImageRect = FindComponent<RectTransform>("Content/Image_Circle");
        m_GameplayProxy = Facade.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;
        m_InitialValueX= m_HeightRect.localPosition.x;
        DOTweenAnimation[] doTweens = m_Content.GetComponents<DOTweenAnimation>();
        for (int i = 0; i < doTweens.Length; i++)
        {
            Debug.Log(doTweens[i].id.Equals("close"));
            if (doTweens[i].id.Equals("close"))
            {
                m_DoTweenAnimation = doTweens[i];
            }
        }
        m_Content.gameObject.SetActive(false);

    }

    public override void OnShow(object msg)
    {
        base.OnShow(msg);

        StartUpdate();
    }

    public override void OnHide(object msg)
    {
        base.OnHide(msg);
    }

    public override NotificationName[] ListNotificationInterests()
    {
        return new NotificationName[]
        {
            NotificationName.MSG_DETECTOR_SHOW,
        };
    }

    public override void HandleNotification(INotification notification)
    {
        switch (notification.Name)
        {
            case NotificationName.MSG_DETECTOR_SHOW:
                SetHeightTreasure(notification.Body);
                break;
        }
    }

    /// <summary>
    /// 更新
    /// </summary>
    protected override void Update()
    {
        if (m_MainEntity == null)
        {
            m_MainEntity = m_GameplayProxy.GetEntityById<SpacecraftEntity>(m_GameplayProxy.GetMainPlayerUID());
        }
        if (!IsWatchOrUIInputMode()  && !IsLeaping())
        {
            if (Time.frameCount % 5 == 0 && m_Show)
            {
                if (!m_Content.gameObject.activeSelf)
                {
                    WwiseUtil.PlaySound((int)WwiseMusic.Music_Hud_Resource_Open, false, null);
                    m_Content.gameObject.SetActive(true);
                }
                CalculateHeight();
            }
        }
        else
        {
            if (m_Content.gameObject.activeSelf)
            {
                WwiseUtil.PlaySound((int)WwiseMusic.Music_Hud_Resource_Close, false, null);
                m_Content.gameObject.SetActive(false);
            }
        }
        if (IsDead())
        {
            m_Show = false;
            if (m_Content.gameObject.activeSelf)
            {
                WwiseUtil.PlaySound((int)WwiseMusic.Music_Hud_Resource_Close, false, null);
                m_Content.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// 存储数据
    /// </summary>
    /// <param name="height">高度</param>
    /// <param name="maxHeight">最大高度差</param>
    private void SetHeightTreasure(object obj)
    {
        MsgDetectorShow msgDetectorShow = (MsgDetectorShow)obj;
        m_HeightTreasure = msgDetectorShow.Height;
        m_DifferenceMax = msgDetectorShow.MaxHeight;
        if (m_DifferenceMax <= 0)
            m_DifferenceMax = 20;
        ShowContent(msgDetectorShow.Show);
        
    }

    /// <summary>
    /// 计算高度
    /// </summary>
    private void CalculateHeight()
    {
        if (m_MainEntity == null || m_MainEntity.GetRootTransform() == null)
        {
            return;
        }
        m_DifferenceTreasure = m_MainEntity.GetRootTransform().position.y - m_HeightTreasure;
        m_Scale = m_DifferenceTreasure / (m_DifferenceMax*0.5f);
        m_Scale = Mathf.Sqrt(1- m_Scale * m_Scale);
        m_Scale = Mathf.Clamp01(m_Scale);
        m_RatioY = m_DifferenceTreasure / m_DifferenceMax;
        m_OffsetY = Mathf.Abs(m_RatioY * m_ImageRect.sizeDelta.y);
        m_Radius_Image = m_ImageRect.sizeDelta.y * 0.5f;
        float a = m_Radius_Image - Mathf.Sqrt(m_Radius_Image * m_Radius_Image - m_OffsetY * m_OffsetY);
        if (m_RatioY > 0)//高
        {
            m_HeightRect.localPosition = new Vector3(m_InitialValueX + a, m_OffsetY, m_HeightRect.localPosition.z);
            m_CircularRect.localPosition = new Vector3(m_CircularRect.localPosition.x, m_OffsetY, m_CircularRect.localPosition.z);
        }
        else//低
        {
            m_HeightRect.localPosition = new Vector3(m_InitialValueX + a, -m_OffsetY, m_HeightRect.localPosition.z);
            m_CircularRect.localPosition = new Vector3(m_CircularRect.localPosition.x, -m_OffsetY, m_CircularRect.localPosition.z);
        }
        m_CircularRect.localScale = m_Scale * Vector3.one;
    }
  
    /// <summary>
    /// 显示内容
    /// </summary>
    /// <param name="show">是否显示</param>
    public void ShowContent(bool show)
    {
        m_Show = show;
        m_Content.gameObject.SetActive(show);
        if (show)
        {
            m_AredIdOpen = Map.MapManager.GetInstance().GetCurrentAreaUid();
            WwiseUtil.PlaySound((int)WwiseMusic.Music_Hud_Resource_Open, false, null);
        }
        else
        {
            m_AredIdClose = Map.MapManager.GetInstance().GetCurrentAreaUid();
            if(m_AredIdOpen==m_AredIdClose)
                WwiseUtil.PlaySound((int)WwiseMusic.Music_Hud_Resource_Close, false, null);
        }
    }
 
}
public enum DetectorColor
{
    Red,//红色
    Yellow,//黄色
    Green,//绿色
}

