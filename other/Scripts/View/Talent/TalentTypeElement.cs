using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TalentTypeElement : MonoBehaviour
{
    /// <summary>
    /// 等级
    /// </summary>
    private TextMeshProUGUI m_LevelLabel;
    /// <summary>
    /// 名字
    /// </summary>
    private TextMeshProUGUI m_NameLabel;
    /// <summary>
    /// 图标Icon
    /// </summary>
    private Image m_IconImage;
    /// <summary>
    /// 消耗图标Icon
    /// </summary>
    private Image m_IconCostImage;
    /// <summary>
    /// 消耗数量
    /// </summary>
    private TextMeshProUGUI m_CoinLabel;

    /// <summary>
    /// 初始化
    /// </summary>
    public void Initialize()
    {
        //m_CfgEternityProxy = (CfgEternityProxy)GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy);
        m_LevelLabel = TransformUtil.FindUIObject<TextMeshProUGUI>(transform, "ImagePoint/Label_TalentLv");
        m_NameLabel = TransformUtil.FindUIObject<TextMeshProUGUI>(transform, "ImagePoint/Label_TalentName");
        m_IconImage = TransformUtil.FindUIObject<Image>(transform, "ImagePoint/Image_BackIcon"); 
        m_IconCostImage = TransformUtil.FindUIObject<Image>(transform, "ImagePoint/Content/Image_Icon"); 
        m_CoinLabel = TransformUtil.FindUIObject<TextMeshProUGUI>(transform, "ImagePoint/Content/Label_Coin");
    }

    /// <summary>
    ///加载数据
    /// </summary>
    public void SetContent(TalentTypeVO talentTypeVO)
    {
        PlayerInfoVo playerInfoVo = NetworkManager.Instance.GetPlayerController().GetPlayerInfo();
        m_NameLabel.text = TableUtil.GetTalentName(talentTypeVO.Id);
        //UIUtil.SetIconImageSquare(m_IconImage, talentTypeVO.IconId);
        m_CoinLabel.text =  CurrencyUtil.GetTalentCurrencyCount().ToString();
        m_LevelLabel.text = string.Format(TableUtil.GetLanguageString("character_text_1019"), playerInfoVo.WatchLv);
        talentTypeVO.Level = (int)playerInfoVo.WatchLv;
        Vector3 vector3 = new Vector3((float)talentTypeVO.MTalent.Value.Position.Value.X, (float)talentTypeVO.MTalent.Value.Position.Value.Y, 0);
        SetPos(vector3);
    }
    /// <summary>
    /// 设置坐标
    /// </summary>
    private void SetPos(Vector3 pos)
    {
        transform.localPosition = pos;
    }
}
