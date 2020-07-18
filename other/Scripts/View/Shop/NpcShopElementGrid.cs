using Assets.Scripts.Define;
using Eternity.FlatBuffer;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NpcShopElementGrid : MonoBehaviour
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
    /// 单位数量
    /// </summary>
    private TMP_Text m_Bounds;
    /// <summary>
    /// 库存
    /// </summary>
    private TMP_Text m_Stock;
    /// <summary>
    /// 个人限量
    /// </summary>
    private TMP_Text m_Available;
    /// <summary>
    /// 货币类型
    /// </summary>
    private Image m_MoneyIcon;
    /// <summary>
    /// 打折价格
    /// </summary>
    private TMP_Text m_Discount;
    /// <summary>
    /// 商品售价
    /// </summary>
    private TMP_Text m_Price;
    /// <summary>
    /// 未开启变灰显示图片
    /// </summary>
    private Image m_Black;
    /// <summary>
    /// 价格
    /// </summary>
    private int m_GoodPrice;
    /// <summary>
    /// 商品数据
    /// </summary>
    private ShopItemData m_ShopItemData;
    /// <summary>
    /// 初始化控件
    /// </summary>
    public void Init()
    {
        m_Quality = TransformUtil.FindUIObject<Image>(transform, "Content/Image_Quality");
        m_Icon = TransformUtil.FindUIObject<Image>(transform, "Content/Image_Icon");
        m_Icon2 = TransformUtil.FindUIObject<Image>(transform, "Content/Image_Icon2");
        m_Name = TransformUtil.FindUIObject<TMP_Text>(transform, "Content/Mask/Label_Name");
        m_Level = TransformUtil.FindUIObject<TMP_Text>(transform, "Content/Mask/Label_Lv2");
        m_Bounds = TransformUtil.FindUIObject<TMP_Text>(transform, "Content/Label_Num");
        m_Stock = TransformUtil.FindUIObject<TMP_Text>(transform, "Content/ShopMessage/Label_Stork ");
        m_Available = TransformUtil.FindUIObject<TMP_Text>(transform, "Content/ShopMessage/Label_Available");
        m_MoneyIcon = TransformUtil.FindUIObject<Image>(transform, "Content/ShopMessage/Money/Icon_Money");
        m_Discount = TransformUtil.FindUIObject<TMP_Text>(transform, "Content/ShopMessage/Money/Label_Money_Discount");
        m_Price = TransformUtil.FindUIObject<TMP_Text>(transform, "Content/ShopMessage/Money/Label_Money_Sell");
        m_Black = TransformUtil.FindUIObject<Image>(transform, "Content/Image_Black");
    }

    /// <summary>
    ///  设置商品数据
    /// </summary>
    /// <param name="shopWindowVO"></param>
    /// <param name="isSelect"></param>
    /// <param name="isList"></param>
    public void SetData(ShopWindowVO shopWindowVO, bool isSelect, bool isList = false)
    {
        Init();
        Item m_Item = shopWindowVO.ShopItemConfig.Value.ItemGood.Value;
        m_ShopItemData = shopWindowVO.ShopItemConfig.Value;
        m_Quality.color = ColorUtil.GetColorByItemQuality(m_Item.Quality);
        if (isList)
        {
            UIUtil.SetIconImage(m_Icon, TableUtil.GetItemIconBundle(m_Item.Id), TableUtil.GetItemIconImage(m_Item.Id));
            UIUtil.SetIconImage(m_Icon2, TableUtil.GetItemIconBundle(m_Item.Id), TableUtil.GetItemIconImage(m_Item.Id));
        }
        else
        {
            UIUtil.SetIconImage(m_Icon, TableUtil.GetItemIconBundle(m_Item.Id), TableUtil.GetItemSquareIconImage(m_Item.Id));
            UIUtil.SetIconImage(m_Icon2, TableUtil.GetItemIconBundle(m_Item.Id), TableUtil.GetItemSquareIconImage(m_Item.Id));
        }
        m_Name.text = TableUtil.GetItemName(m_Item.Id);
        m_Level.text = TableUtil.ShowLevel(1);
        m_Bounds.text = m_ShopItemData.Bounds.ToString();
        if (shopWindowVO.LimitCount == -1)
        {
            m_Available.gameObject.SetActive(false);
        }
        else
        {
            m_Available.gameObject.SetActive(true);
            m_Available.text = string.Format(TableUtil.GetLanguageString("shop_text_1012"), shopWindowVO.LimitCount);
        }
        if (shopWindowVO.LimitCount < m_ShopItemData.Bounds)
        {
            m_Available.color =  Color.red;
        }
        else
        {
            m_Available.color = isSelect ? new Color(41f / 255f, 41f / 255f, 41f / 255f, 1) : Color.white;
        }

        UIUtil.SetIconImage(m_MoneyIcon, TableUtil.GetItemIconBundle((KNumMoneyType)m_ShopItemData.MoneyType), TableUtil.GetItemIconImage((KNumMoneyType)m_ShopItemData.MoneyType));
        if (m_ShopItemData.DisCount == 1)
        {
            m_Discount.gameObject.SetActive(false);
            m_GoodPrice = m_ShopItemData.BuyCost;
        }
        else
        {
            m_Discount.gameObject.SetActive(true);
            m_Discount.text = m_ShopItemData.BuyCost.ToString();
            m_GoodPrice = Mathf.CeilToInt(m_ShopItemData.BuyCost * m_ShopItemData.DisCount);
        }
        m_Price.text = m_GoodPrice.ToString();

        if (MoneyeEnough())
        {
            m_Price.color = new Color(30f / 255f, 170f / 255f, 33f / 255f, 1);
        }
        else
        {
            m_Price.color = Color.red;
        }
        if (shopWindowVO.ServerLeftNum == -1)
        {
            m_Stock.text = TableUtil.GetLanguageString("shop_text_1010");
        }
        else if (shopWindowVO.ServerLeftNum == 0)
        {
            m_Stock.text = TableUtil.GetLanguageString("shop_text_1013");
        }
        else
        {
            m_Stock.text = string.Format(TableUtil.GetLanguageString("shop_text_1011"), shopWindowVO.ServerLeftNum);
        }
        m_Stock.color = isSelect ? new Color(41f / 255f, 41f / 255f, 41f / 255f, 1) : Color.white;
        m_Black.gameObject.SetActive(shopWindowVO.IsOpen == 0);
    }
    /// <summary>
    /// 货币是否充足
    /// </summary>
    /// <param name="MoneyType"></param>
    /// <returns></returns>
    public bool MoneyeEnough()
    {
        long m_OwnMoney = 0;
        if (m_ShopItemData.MoneyType == 1)
        {
            m_OwnMoney = CurrencyUtil.GetGameCurrencyCount();
        }
        else if (m_ShopItemData.MoneyType == 2)
        {
            m_OwnMoney = CurrencyUtil.GetRechargeCurrencyCount();
        }
        if (m_GoodPrice <= m_OwnMoney)
        {
            return true;
        }
        return false;
    }
}
