using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NpcShopSellBackElementGrid : MonoBehaviour
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
    private TMP_Text m_Num;
    /// <summary>
    /// 货币类型
    /// </summary>
    private Image m_MoneyIcon;
    /// <summary>
    /// 商品售价
    /// </summary>
    private TMP_Text m_Price;
    /// <summary>
    /// 当前回购数据
    /// </summary>
    private ShopSellBackVO m_SellBackVO;
    /// <summary>
    /// 初始化控件
    /// </summary>
    public void Init()
    {
        m_Quality = TransformUtil.FindUIObject<Image>(transform, "Content/Image_Quality");
        m_Icon = TransformUtil.FindUIObject<Image>(transform, "Content/Image_Icon");
        m_Name = TransformUtil.FindUIObject<TMP_Text>(transform, "Content/Mask/Label_Name");
        m_Level = TransformUtil.FindUIObject<TMP_Text>(transform, "Content/Mask/Label_Lv2");
        m_Icon2 = TransformUtil.FindUIObject<Image>(transform, "Content/Image_Icon2");
        m_Num = TransformUtil.FindUIObject<TMP_Text>(transform, "Content/Label_Num");
        m_MoneyIcon = TransformUtil.FindUIObject<Image>(transform, "Content/ShopMessage/Money/Icon_Money");
        m_Price = TransformUtil.FindUIObject<TMP_Text>(transform, "Content/ShopMessage/Money/Label_Money_Sell");
    }
    /// <summary>
    /// 设置回购数据
    /// </summary>
    /// <param name="sellBackVO"></param>
    /// <param name="isList">是否列表模式</param>
    public void SetData(ShopSellBackVO sellBackVO, bool isList)
    {
        Init();
        m_SellBackVO = sellBackVO;
        m_Quality.color = ColorUtil.GetColorByItemQuality(sellBackVO.ItemConfig.Quality);
        if (isList)
        {
            UIUtil.SetIconImage(m_Icon, TableUtil.GetItemIconBundle((uint)m_SellBackVO.Tid), TableUtil.GetItemIconImage((uint)m_SellBackVO.Tid));
            UIUtil.SetIconImage(m_Icon2, TableUtil.GetItemIconBundle((uint)m_SellBackVO.Tid), TableUtil.GetItemIconImage((uint)m_SellBackVO.Tid));
        }
        else
        {
            UIUtil.SetIconImage(m_Icon, TableUtil.GetItemIconBundle((uint)m_SellBackVO.Tid), TableUtil.GetItemSquareIconImage((uint)m_SellBackVO.Tid));
            UIUtil.SetIconImage(m_Icon2, TableUtil.GetItemIconBundle((uint)m_SellBackVO.Tid), TableUtil.GetItemSquareIconImage((uint)m_SellBackVO.Tid));
        }
        m_Num.text = sellBackVO.Num.ToString();
        UIUtil.SetIconImage(m_MoneyIcon, TableUtil.GetItemIconBundle((uint)sellBackVO.ItemConfig.SellCurrency), TableUtil.GetItemIconImage((uint)sellBackVO.ItemConfig.SellCurrency));
        m_Price.text = sellBackVO.ItemConfig.BuybackPrice.ToString();
        m_Name.text = TableUtil.GetItemName((uint)m_SellBackVO.Tid);
        m_Level.text = TableUtil.ShowLevel(1);


        if (MoneyeEnough())
        {
            m_Price.color = new Color(30f / 255f, 170f / 255f, 33f / 255f, 1);
        }
        else
        {
            m_Price.color = Color.red;
        }
    }

    /// <summary>
    /// 货币是否充足
    /// </summary>
    /// <returns></returns>
    public bool MoneyeEnough()
    {
        long m_OwnMoney = 0;
        if (m_SellBackVO.ItemConfig.SellCurrency == 1100004)
        {
            m_OwnMoney = CurrencyUtil.GetGameCurrencyCount();
        }
        else if (m_SellBackVO.ItemConfig.SellCurrency == 1000001)
        {
            m_OwnMoney = CurrencyUtil.GetRechargeCurrencyCount();
        }
        if ( m_SellBackVO.ItemConfig.BuybackPrice <= m_OwnMoney)
        {
            return true;
        }
        return false;
    }
}
