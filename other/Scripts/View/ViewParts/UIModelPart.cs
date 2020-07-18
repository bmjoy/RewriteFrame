using UnityEngine.UI;

public class UIModelPart : BaseViewPart
{
    private const string ASSET_ADDRESS = Leyoutech.Core.Loader.Config.AssetAddressKey.PRELOADUI_COMMONMODELPART;

    /// <summary>
    /// 3D视图
    /// </summary>
    private Effect3DViewer m_3DViewer;

    public override void OnShow(object msg)
    {
        base.OnShow(msg);

        if(OwnerView.ModelBox)
            LoadViewPart(ASSET_ADDRESS, OwnerView.ModelBox);
    }

    /// <summary>
    /// 部件加载时
    /// </summary>
    protected override void OnViewPartLoaded()
    {
        RawImage rawImage = FindComponent<RawImage>("Content/Viewer");
        if(rawImage)
        {
            m_3DViewer = rawImage.GetComponent<Effect3DViewer>();
            if (!m_3DViewer)
            {
                m_3DViewer = rawImage.gameObject.AddComponent<Effect3DViewer>();
                m_3DViewer.AutoAdjustBestRotationAndDistance = false;
            }
        }

        OwnerView.State.OnModelInfoChanged -= OnModelInfoChanged;
        OwnerView.State.OnModelInfoChanged += OnModelInfoChanged;

        OnModelInfoChanged();
    }

    /// <summary>
    /// 部件卸载时
    /// </summary>
    protected override void OnViewPartUnload()
    {
        OwnerView.State.OnModelInfoChanged -= OnModelInfoChanged;

        if (m_3DViewer)
        {
            m_3DViewer.ClearModel();
            m_3DViewer = null;
        }
    }

    /// <summary>
    /// 模型信息改变时
    /// </summary>
    private void OnModelInfoChanged()
    {
        string environtment;
        Effect3DViewer.ModelInfo[] models;
        string effect;

        OwnerView.State.Get3DModelInfo(out environtment, out models, out effect);

        if(m_3DViewer)
        {
            m_3DViewer.LoadModel(environtment, models, effect);
        }
    }
}
