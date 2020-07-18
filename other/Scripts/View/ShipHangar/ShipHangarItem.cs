using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ShipHangarItem : MonoBehaviour, IPointerEnterHandler
{
    /// <summary>
    /// 初始化标记
    /// </summary>
    private bool m_Inited;
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
    /// 等级
    /// </summary>
    private TMP_Text m_LvLabel;
    /// <summary>
    /// 任命船标记
    /// </summary>
    private Transform m_Appoint;
    /// <summary>
    /// 新船标记
    /// </summary>
    private Transform m_New;
    /// <summary>
    /// 船数据
    /// </summary>
    private IShip m_Ship;
    /// <summary>
    /// 船Proxy
    /// </summary>
    private ShipProxy m_ShipProxy;

    /// <summary>
    /// 初始化控件
    /// </summary>
    public void Initialize()
    {
        if (m_Inited)
        {
            return;
        }
        m_Inited = true;
        m_IconImage = TransformUtil.FindUIObject<Image>(transform, "Content/Image_Icon");
        m_OverlyingIcon = TransformUtil.FindUIObject<Image>(transform, "Content/Image_Icon2");
        m_NameLabel = TransformUtil.FindUIObject<TMP_Text>(transform, "Content/Mask/Label_Name");
        m_LvLabel = TransformUtil.FindUIObject<TMP_Text>(transform, "Content/Mask/Label_Lv2");
        m_Appoint = TransformUtil.FindUIObject<Transform>(transform, "Content/Image_Used");
        m_New = TransformUtil.FindUIObject<Transform>(transform, "Content/Image_New");
        m_ShipProxy = GameFacade.Instance.RetrieveProxy(ProxyName.ShipProxy) as ShipProxy;
    }

    /// <summary>
    /// 设置船内容显示
    /// </summary>
    /// <param name="ship"></param>
    public void SetData(IShip ship,bool isList)
    {
        Initialize();  
        m_Ship = ship;
        if (isList)
        {
            UIUtil.SetIconImage(m_IconImage, TableUtil.GetItemIconBundle(ship.GetTID()), TableUtil.GetItemIconImage(ship.GetTID()));
        }
        else
        {
            UIUtil.SetIconImage(m_IconImage, TableUtil.GetItemIconBundle(ship.GetTID()), TableUtil.GetItemSquareIconImage(ship.GetTID()));                    
        }
        m_OverlyingIcon.sprite = m_IconImage.sprite;
        m_NameLabel.text = TableUtil.GetItemName((int)ship.GetTID());
        m_LvLabel.text = TableUtil.ShowLevel(ship.GetLv());
        m_Appoint.gameObject.SetActive(ship.GetUID() == m_ShipProxy.GetAppointWarShip().GetUID());
        m_New.gameObject.SetActive(!m_ShipProxy.MarkNew(ship));
    }

    /// <summary>
    /// 记录新船
    /// </summary>
    public void RecordItemNew()
    {
        if (m_Ship == null || !m_New.gameObject.activeSelf)
        {
            return;
        }
        m_New.gameObject.SetActive(false);
        m_ShipProxy.SetNew(m_Ship);
    }
    /// <summary>
    /// 划过
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        RecordItemNew();
    }   
}
