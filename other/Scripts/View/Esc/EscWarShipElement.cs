using Eternity.FlatBuffer;
using Leyoutech.Core.Loader.Config;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EscWarShipElement : MonoBehaviour
{
    /// <summary>
    /// 模型
    /// </summary>
    private RawImage m_Model;
    /// <summary>
    /// 选项文字
    /// </summary>
    private TMP_Text m_Label;
    public void Init()
    {
        if (m_Label == null)
        {
            m_Label = transform.Find("Content/Content/Label_Name").GetComponent<TMP_Text>();
        }
        if (m_Model == null)
        {
            m_Model = transform.Find("Content/NPC").GetComponent<RawImage>();
        }
        m_Label.text = TableUtil.GetLanguageString("esc_title_1003");
        //m_Model.gameObject.SetActive(true);
        //ShowModel();
    }

    /// <summary>
    /// 显示模型
    /// </summary>
    public void ShowModel()
    {
        CfgEternityProxy m_CfgEternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
        ShipProxy m_ShipProxy = GameFacade.Instance.RetrieveProxy(ProxyName.ShipProxy) as ShipProxy;
        IShip m_Ship = m_ShipProxy.GetAppointWarShip();
        Effect3DViewer m_Model3DViewer = m_Model.GetOrAddComponent<Effect3DViewer>();
        Model m_UiModel = m_CfgEternityProxy.GetModel((int)m_Ship.GetBaseConfig().Model);
        if (m_Model3DViewer != null)
        {
            m_Model3DViewer.AutoAdjustBestRotationAndDistance = true;
            m_Model3DViewer.ClearModel();
            m_Model3DViewer.LoadModel
                (AssetAddressKey.PRELOADUI_UI3DSHIP, m_UiModel.AssetName);
        }
    }
}
