using Eternity.FlatBuffer;
using PureMVC.Interfaces;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIProducePressPart : BaseViewPart
{
    /// <summary>
    /// 资源路径
    /// </summary>
    private const string ASSET_ADDRESS = Leyoutech.Core.Loader.Config.AssetAddressKey.PRELOADUI_PRODUCEPROGRESSPANEL;
    /// <summary>
    /// 生产数据
    /// </summary>
    protected FoundryProxy m_FoundryProxy;
    /// <summary>
	/// 当前鼠标选中的蓝图ID
	/// </summary>
	private int m_SelectProduceTid = 0;
    /// <summary>
	/// 返回道具根节点
	/// </summary>
	private Transform m_ReturnItemRoot;
    /// <summary>
    /// 按键标题
    /// </summary>
    private TextMeshProUGUI m_ReturnTitle;
    /// <summary>
	/// 提示信息
	/// </summary>
	private TextMeshProUGUI m_HintLabel;
    /// <summary>
    /// 按键进度
    /// </summary>
    private Image m_DownProgressImage;
    /// <summary>
	/// 操作确认面板上的Icon集合
	/// </summary>
	private List<AbstractIconBase> m_IconList = new List<AbstractIconBase>();
    /// <summary>
    /// 生产面板
    /// </summary>
    private ProduceView m_ProduceView;
    /// <summary>
    /// 协程
    /// </summary>
    private Coroutine m_Coroutine;
    /// <summary>
    /// 内容
    /// </summary>
    private GameObject m_Content;
    /// <summary>
    /// 延迟时间
    /// </summary>
    private float m_Time = 10;

    public override void OnShow(object msg)
    {
        base.OnShow(msg);
        m_FoundryProxy = GameFacade.Instance.RetrieveProxy(ProxyName.FoundryProxy) as FoundryProxy;
        m_ProduceView = OwnerView as ProduceView;
        m_Time = 10;
        if (OwnerView.TipBox)
        {
            m_Coroutine = UIManager.Instance.StartCoroutine(SetPressRoot());
        }
           

    }

    /// <summary>
    /// 部件加载时
    /// </summary>
    protected override void OnViewPartLoaded()
    {
        m_HintLabel = FindComponent<TextMeshProUGUI>("Image_Progress/Label_str");
        m_DownProgressImage = FindComponent<Image>("Image_Progress/ImageCircle");
        m_ReturnItemRoot = FindComponent<Transform>("Image_Progress/Content");
        m_ReturnTitle = FindComponent<TextMeshProUGUI>("Image_Progress/Label");
        m_Content = FindComponent<Image>("Image_Progress").gameObject;
        m_ProduceView.SetDownProgressImage(m_DownProgressImage);
        m_Content.SetActive(false);
        GetTransform().SetParent(OwnerView.TipBox.GetChild(0).Find("Content/TipBox3"));
        GetTransform().GetComponent<RectTransform>().localPosition = Vector3.zero;
        GetTransform().GetComponent<RectTransform>().localScale = Vector3.one;
    }

   
    /// <summary>
    /// 设置父物体
    /// </summary>
    /// <returns></returns>
    public IEnumerator SetPressRoot()
    {
        while (m_Time > 0)
        {
            m_Time -= Time.deltaTime;
            if (OwnerView.TipBox.childCount > 0)
            {
                LoadViewPart(ASSET_ADDRESS, OwnerView.OtherBox);
                break;
            }
            yield return  null;
        }
    }

    /// <summary>
    /// 部件卸载时
    /// </summary>
    protected override void OnViewPartUnload()
    {
        if (m_Coroutine!=null)
            UIManager.Instance.StopCoroutine(m_Coroutine);
    }

    public override NotificationName[] ListNotificationInterests()
    {
        return new NotificationName[]
        {
            NotificationName.MSG_PRODUCE_ORDER,
            NotificationName.MSG_PRODUCE_ORDE_RRETRIEVE,
            NotificationName.MSG_PRODUCE_ORDE_SHOW,
        };
    }

    public override void HandleNotification(INotification notification)
    {
        switch ((NotificationName)notification.Name)
        {
            case NotificationName.MSG_PRODUCE_ORDER:
                OpenConfim((MsgProduceConfim)(notification.Body));
                break;
            case NotificationName.MSG_PRODUCE_ORDE_RRETRIEVE:
                RetrieveIcon();
                break;
            case NotificationName.MSG_PRODUCE_ORDE_SHOW:
                Show();
                break;
            default:
                break;
        }
    }

    #region 确认面板
    /// <summary>
    /// 打开确认面板
    /// </summary>
    /// <param name="type">操作类型</param>
    public void OpenConfim(MsgProduceConfim confim)//消耗数据
    {
        ProduceOrder type = confim.OrderType;
        m_SelectProduceTid = confim.Tid;
        switch (type)
        {
            case ProduceOrder.Produce:  //生产
                m_ReturnTitle.text = string.Format(TableUtil.GetLanguageString("production_text_1025"),
                TableUtil.GetItemName(m_FoundryProxy.GetItemByProduceKey(m_SelectProduceTid).Id));
                m_HintLabel.text = "";
                break;
            case ProduceOrder.Canel:  //取消生产
                m_ReturnTitle.text = TableUtil.GetLanguageString("production_text_1026");
                m_HintLabel.text = TableUtil.GetLanguageString("production_text_1041");
                m_IconList.Clear();
                Item[] items = m_FoundryProxy.GetEffectItem(m_SelectProduceTid);
                EffectElement?[] effects = m_FoundryProxy.GetEffectElementsByProduceTid(m_SelectProduceTid);
                for (int i = 0; i < items.Length; i++)
                {
                    IconManager.Instance.LoadItemIcon<IconCommon>(IconConstName.ICON_COMMON, m_ReturnItemRoot,
                    (icon) =>
                    {
                        m_IconList.Add(icon);
                        icon.SetData(TableUtil.GetItemIconTid(items[i].Id), items[i].Quality, (int)effects[i].Value.Value);
                    });
                }

                break;
            case ProduceOrder.Recevie:  //领取 不要图标
                m_ReturnTitle.text = TableUtil.GetLanguageString("production_text_1028");
                m_HintLabel.text = "";
                break;
            case ProduceOrder.SpeedUp:  //加速
                m_ReturnTitle.text = TableUtil.GetLanguageString("production_text_1027");
                m_HintLabel.text = TableUtil.GetLanguageString("production_text_1040");
                m_IconList.Clear();
                IconManager.Instance.LoadItemIcon<IconCommon>(IconConstName.ICON_COMMON, m_ReturnItemRoot,
                    (icon) =>
                    {
                        m_IconList.Add(icon);
                        icon.SetData(TableUtil.GetItemIconTid(GameConstant.CurrencyConst.RECHARGE_CURRENCY_ITEM_TID),
                        TableUtil.GetItemQuality(GameConstant.CurrencyConst.RECHARGE_CURRENCY_ITEM_TID), (int)confim.ExpendNum);
                    });
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 回收Icon
    /// </summary>
    private void RetrieveIcon()
    {
        for (int i = 0; i < m_IconList.Count; i++)
        {
            if (m_IconList[i] != null)
            {
                IconManager.Instance.RetrieveIcon(m_IconList[i]);
            }
        }
        m_DownProgressImage.fillAmount = 0;
        m_Content.SetActive(false);
    }

    /// <summary>
    /// 显示
    /// </summary>
    public void Show()
    {
        m_Content.SetActive(true);
    }
    #endregion


}
/// <summary>
/// 生产指令
/// </summary>
public enum ProduceOrder
{
    /// <summary>
    /// 生产
    /// </summary>
    Produce = 1,
    /// <summary>
    /// 取消
    /// </summary>
    Canel,
    /// <summary>
    /// 接收
    /// </summary>
    Recevie,
    /// <summary>
    /// 加速
    /// </summary>
    SpeedUp,
}
