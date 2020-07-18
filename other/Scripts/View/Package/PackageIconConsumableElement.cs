using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PackageIconConsumableElement : MonoBehaviour
{
    /// <summary>
    /// 空物品
    /// </summary>
    private Transform m_Empty;

    /// <summary>
    /// 内容
    /// </summary>
    private Transform m_Content;

    /// <summary>
    /// 图标
    /// </summary>
    private Image m_IconImage;

    /// <summary>
    /// 叠加icon图片
    /// </summary>
    private Image m_OverlyingIcon;

    /// <summary>
    /// 物品名称
    /// </summary>
    private TMP_Text m_NameLabel;

    /// <summary>
    /// 数量
    /// </summary>
    private TMP_Text m_NumLabel;

    /// <summary>
    /// 品质
    /// </summary>
    private Image m_Quality;

    /// <summary>
    /// 类型
    /// </summary>
    private TMP_Text m_TypeLabel;
    /// <summary>
    /// Mod容器
    /// </summary>
    private Transform m_ModContainer;

    /// <summary>
    /// 初始化标记
    /// </summary>
    private bool m_Inited;


    private void Initialize()
    {
        if (m_Inited)
        {
            return;
        }
        m_Inited = true;

        m_Empty = TransformUtil.FindUIObject<Transform>(transform, "Image_Empty");
        m_Content = TransformUtil.FindUIObject<Transform>(transform, "Content");
        m_IconImage = TransformUtil.FindUIObject<Image>(transform, "Content/Image_Icon");
        m_OverlyingIcon = TransformUtil.FindUIObject<Image>(transform, "Content/Image_Icon2");
        m_NameLabel = TransformUtil.FindUIObject<TMP_Text>(transform, "Content/Mask/Label_Name");
        m_NumLabel = TransformUtil.FindUIObject<TMP_Text>(transform, "Content/Label_Num");
        m_Quality = TransformUtil.FindUIObject<Image>(transform, "Content/Image_Quality");
        m_TypeLabel = TransformUtil.FindUIObject<TMP_Text>(transform, "Content/Mask/Label_WeaponLabel");
        m_ModContainer = TransformUtil.FindUIObject<Transform>(transform, "Content/MOD");
    }

    public void SetData(ItemBase Vo)
    {
        Initialize();
        m_Empty.gameObject.SetActive(Vo.TID == 0);
        m_Content.gameObject.SetActive(Vo.TID != 0);
        if (Vo.TID == 0)
        {
            return;
        }
        UIUtil.SetIconImage(m_IconImage, TableUtil.GetItemIconBundle(Vo.TID), TableUtil.GetItemIconImage(Vo.TID));
        m_OverlyingIcon.sprite = m_IconImage.sprite;
        m_NameLabel.text = TableUtil.GetItemName((int)Vo.TID);
        m_Quality.color = ColorUtil.GetColorByItemQuality(Vo.ItemConfig.Quality);
        m_NumLabel.text = Vo.Count.ToString();
        m_TypeLabel.text = TableUtil.GetLanguageString(Vo.MainType);
    }
}
