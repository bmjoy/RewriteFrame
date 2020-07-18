using Eternity.FlatBuffer;
using Leyoutech.Core.Loader.Config;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EscStarMapElement : MonoBehaviour
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
        if (m_Model == null)
        {
            m_Model = transform.Find("Content/NPC").GetComponent<RawImage>();
        }
        if (m_Label == null)
        {
            m_Label = transform.Find("Content/Content/Label_Name").GetComponent<TMP_Text>();
        }
        m_Label.text = TableUtil.GetLanguageString("esc_title_1005");
        m_Model.gameObject.SetActive(true);
        ShowModel();
    }

    /// <summary>
    /// 显示模型
    /// </summary>
    public void ShowModel()
    {
        CfgEternityProxy m_CfgEternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
        Effect3DViewer m_Model3DViewer = m_Model.GetOrAddComponent<Effect3DViewer>();
        UiModel m_UiModel = m_CfgEternityProxy.GetUiModel("Esc_1004");
        if (m_Model3DViewer != null)
        {
            //m_Model3DViewer.AutoAdjustBestRotationAndDistance = true;
            m_Model3DViewer.ClearModel();
            m_Model3DViewer.LoadModel
                (m_UiModel.Light, m_UiModel.ModelName,
                m_CfgEternityProxy.GetUiModelPos(m_UiModel), m_CfgEternityProxy.GetUiModelRotation(m_UiModel), m_CfgEternityProxy.GetUiModelScale(m_UiModel));
        }
    }
}
