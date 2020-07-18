using Assets.Scripts.Define;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NpcShopSellElementGrid : MonoBehaviour
{
    /// <summary>
    /// 商品品质
    /// </summary>
    protected Image m_Quality;
    /// <summary>
    /// 商品图标
    /// </summary>
    protected Image m_Icon;
    /// <summary>
    /// 商品重叠图标
    /// </summary>
    private Image m_Icon2;
    /// <summary>
    /// 商品名称
    /// </summary>
    private TMP_Text m_Name;
    /// <summary>
    /// 商品等级
    /// </summary>
    private TMP_Text m_Level;
    /// <summary>
    /// 数量
    /// </summary>
    private TMP_Text m_Count;
    /// <summary>
    /// 货币类型
    /// </summary>
    private Image m_MoneyIcon;
    /// <summary>
    /// 商品售价
    /// </summary>
    private TMP_Text m_Price;
    /// <summary>
    /// 是否被装备
    /// </summary>
    private Image m_Used;
    public void Init()
    {
        m_Quality = TransformUtil.FindUIObject<Image>(transform, "Content/Image_Quality");
        m_Icon = TransformUtil.FindUIObject<Image>(transform, "Content/Image_Icon");
        m_Icon2 = TransformUtil.FindUIObject<Image>(transform, "Content/Image_Icon2");
        m_Name = TransformUtil.FindUIObject<TMP_Text>(transform, "Content/Mask/Label_Name");
        m_Level = TransformUtil.FindUIObject<TMP_Text>(transform, "Content/Mask/Label_Lv2");
        m_Count = TransformUtil.FindUIObject<TMP_Text>(transform, "Content/Label_Num");
        m_MoneyIcon = TransformUtil.FindUIObject<Image>(transform, "Content/ShopMessage/Money/Icon_Money");
        m_Price = TransformUtil.FindUIObject<TMP_Text>(transform, "Content/ShopMessage/Money/Label_Money_Sell");
        m_Used = TransformUtil.FindUIObject<Image>(transform, "Content/Image_Used");
    }

    public void SetData(ItemBase item,bool isSelect, bool isList = false)
    {
        Init();
        m_Quality.color = ColorUtil.GetColorByItemQuality(item.ItemConfig.Quality);
        if (isList)
        {
            UIUtil.SetIconImage(m_Icon, TableUtil.GetItemIconBundle(item.TID), TableUtil.GetItemIconImage(item.TID));
            UIUtil.SetIconImage(m_Icon2, TableUtil.GetItemIconBundle(item.TID), TableUtil.GetItemIconImage(item.TID));
        }
        else
        {
            UIUtil.SetIconImage(m_Icon, TableUtil.GetItemIconBundle(item.TID), TableUtil.GetItemSquareIconImage(item.TID));
            UIUtil.SetIconImage(m_Icon2, TableUtil.GetItemIconBundle(item.TID), TableUtil.GetItemSquareIconImage(item.TID));
        }
        m_Count.text = item.Count.ToString();
        m_Used.gameObject.SetActive(item.Replicas != null && item.Replicas.Count > 0);
        m_Price.text = item.ItemConfig.MoneyPrice.ToString();
        if (isSelect)
        {
            m_Price.color = new Color(41f / 255f, 41f / 255f, 41f/255f, 1);
        }
        else
        {
            m_Price.color = Color.white;
        }
        m_Name.text = TableUtil.GetItemName(item.TID);
        m_Level.text = TableUtil.ShowLevel(1);
        UIUtil.SetIconImage(m_MoneyIcon, TableUtil.GetItemIconBundle((uint)item.ItemConfig.SellCurrency), TableUtil.GetItemIconImage((uint)item.ItemConfig.SellCurrency));
    }
}
