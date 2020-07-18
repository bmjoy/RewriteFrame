using Eternity.FlatBuffer;
using PureMVC.Interfaces;
using UnityEngine;
using UnityEngine.UI;

public class UIWarshipModelPart : BaseViewPart
{
    /// <summary>
    /// 面板资源地址
    /// </summary>
    private const string ASSET_ADDRESS = Leyoutech.Core.Loader.Config.AssetAddressKey.PRELOADUI_WARSHIPMODELPANEL;

    /// <summary>
    /// UI3D船模型
    /// </summary>
    private const string ASSET_UI3DSHIP = Leyoutech.Core.Loader.Config.AssetAddressKey.PRELOADUI_UI3DSHIP;

    /// <summary>
    ///  UI3D船特效
    /// </summary>
    private const string ASSET_UI3DSHIPEFFECT = Leyoutech.Core.Loader.Config.AssetAddressKey.PRELOADUI_UI_SHIP_SWITCH;

    /// <summary>
    /// UI武器全息特效地址
    /// </summary>
    private const string ASSET_UI3DWEAPONEFFECT = Leyoutech.Core.Loader.Config.AssetAddressKey.PRELOADUI_UI_WEAPONS_HOLOGRAPHIC;

    /// <summary>
    /// 船或者武器的模型
    /// </summary>
    private Effect3DViewer m_ShipViewer;

    /// <summary>
    /// 显示图Transform
    /// </summary>
    private RectTransform m_ShipTransform;

    /// <summary>
    /// 是否有模型数据
    /// </summary>
    private bool m_HasData = false;
    /// <summary>
    /// 模型
    /// </summary>
    private Model m_Model;
    /// <summary>
    /// 是否为飞船 
    /// </summary>
    private bool m_IsShip;
    /// <summary>
    /// 坐标
    /// </summary>
    private Vector3 m_Position;
    /// <summary>
    /// 大小
    /// </summary>
    private Vector2 m_Size;

    public override void OnShow(object msg)
    {
        base.OnShow(msg);

        LoadViewPart(ASSET_ADDRESS, OwnerView.OtherBox);
    }

    protected override void OnViewPartLoaded()
    {
        Animator shipAnimator = GetTransform().GetComponent<Animator>();
        shipAnimator.enabled = false;

        RawImage rawImage = FindComponent<RawImage>("Model");
        rawImage.raycastTarget = false;

        m_ShipViewer = rawImage.GetComponent<Effect3DViewer>() ?? rawImage.gameObject.AddComponent<Effect3DViewer>();
        m_ShipTransform = rawImage.GetComponent<RectTransform>();

        UpdateModelViewer();
    }

    protected override void OnViewPartUnload()
    {
        m_ShipViewer.ClearModel();
    }

    public override NotificationName[] ListNotificationInterests()
    {
        return new NotificationName[]
        {
             NotificationName.MSG_3DVIEWER_CHANGE
        };
    }

    public override void HandleNotification(INotification notification)
    {
        switch (notification.Name)
        {
            case NotificationName.MSG_3DVIEWER_CHANGE:
                UpdateModelData(notification.Body as Msg3DViewerInfo);
                break;
        }
    }

    /// <summary>
    /// 更新模型
    /// </summary>
    /// <param name="viewerInfo">模型数据</param>
    private void UpdateModelData(Msg3DViewerInfo viewerInfo)
    {
        if (viewerInfo != null)
        {
            m_HasData = true;
            m_Model = viewerInfo.Model;
            m_IsShip = viewerInfo.IsShip;
            m_Position = viewerInfo.position;
            m_Size = viewerInfo.size;
        }
        else
        {
            m_HasData = false;
        }

        UpdateModelViewer();
    }

    /// <summary>
    /// 更新模型
    /// </summary>
    private void UpdateModelViewer()
    {
        if (!m_ShipViewer)
            return;

        if (m_HasData)
        {
            Vector3 modelPosition = Vector3.zero;
            modelPosition.x = m_Model.UiPositionLength > 0 ? m_Model.UiPosition(0) : 0;
            modelPosition.y = m_Model.UiPositionLength > 1 ? m_Model.UiPosition(1) : 0;
            modelPosition.z = m_Model.UiPositionLength > 2 ? m_Model.UiPosition(2) : 0;

            Vector3 modelRotation = Vector3.zero;
            modelRotation.x = m_Model.UiRotationLength > 0 ? m_Model.UiRotation(0) : 0;
            modelRotation.y = m_Model.UiRotationLength > 1 ? m_Model.UiRotation(1) : 0;
            modelRotation.z = m_Model.UiRotationLength > 2 ? m_Model.UiRotation(2) : 0;

            float modelScale = m_Model.UiScale == 0 ? 1 : m_Model.UiScale;

            string effectPath = m_IsShip ? ASSET_UI3DSHIPEFFECT : ASSET_UI3DWEAPONEFFECT;

            m_ShipViewer.ModelRotate = !m_IsShip;
            m_ShipViewer.LoadModel(ASSET_UI3DSHIP, m_Model.AssetName, effectPath, modelPosition, modelRotation, modelScale * Vector3.one);

            m_ShipTransform.localPosition = m_Position;
            m_ShipTransform.sizeDelta = m_Size;
        }
        else
        {
            if (m_ShipViewer)
                m_ShipViewer.ClearModel();
        }
    }
}
