using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UITipMailPart : UITipFriendPart
{
    private const string TIP_PREFAB = Leyoutech.Core.Loader.Config.AssetAddressKey.PRELOADUI_TIPSFUNCTIONMESSAGEPANEL;

    /// <summary>
    /// TIP的Prefab
    /// </summary>
    private GameObject m_TipPrefab;
    /// <summary>
    /// Tip的Prefab是否在加载中
    /// </summary>
    private bool m_TipPrefabLoading;

    /// <summary>
    /// Tip1
    /// </summary>
    private GameObject m_TipInstance1;
    public override void OnShow(object msg)
    {
        base.OnShow(msg);

        m_TipPrefab = null;
        m_TipPrefabLoading = false;
    }

    public override void OnHide()
    {
        m_TipPrefab = null;
        m_TipPrefabLoading = false;

        base.OnHide();
    }

    #region 主Tip

    /// <summary>
    /// 清除视图
    /// </summary>
    protected override void CloseTipView()
    {
        if (m_TipInstance1)
        {
            m_TipInstance1.Recycle();
            m_TipInstance1 = null;
        }

        base.CloseTipView();
    }

    /// <summary>
    /// 更新Tip视图
    /// </summary>
    /// <param name="data">数据</param>
    protected override void OpenTipView()
    {
        if (TipData is MailDataVO)
            OpenTip(TipData as MailDataVO);
        else
            base.OpenTipView();
    }

    /// <summary>
    /// 打开Tip
    /// </summary>
    private void OpenTip(MailDataVO data)
    {
        if (m_TipPrefab)
        {
            if (!m_TipInstance1)
            {
                m_TipInstance1 = m_TipPrefab.Spawn(TipBoxLeft);
                m_TipInstance1.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            }

            LayoutMailTip(m_TipInstance1, data);
        }
        else if (!m_TipPrefabLoading)
        {
            m_TipPrefabLoading = true;

            LoadPrefabFromPool(TIP_PREFAB, (prefab) =>
            {
                if (Opened)
                {
                    m_TipPrefab = prefab;

                    OpenTipView();
                }
            });
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="view"></param>
    /// <param name="data"></param>
    private void LayoutMailTip(GameObject view, MailDataVO data)
    {
        int index = 0;
        Transform m_TipsTransform = FindComponent<Transform>(view.transform,"TipsScrollView/ALL");
        TextMeshProUGUI m_Title = FindComponent<TextMeshProUGUI>(view.transform,"TipsScrollView/ALL/Content/NameType/Label_Name");
        TextMeshProUGUI m_Sender = FindComponent<TextMeshProUGUI>(view.transform, "TipsScrollView/ALL/Content/NameType/Type/Label_Type");
        TextMeshProUGUI m_Describe = FindComponent<TextMeshProUGUI>(view.transform,"TipsScrollView/ALL/Content/Describe/Label_Describe");

        Transform itemRootTf = FindComponent<Transform>(view.transform, "TipsScrollView/ALL/Content/Materials");   
        m_Title.text = data.Title;
        m_Sender.text = data.Sender;
        m_Describe.text = data.Content;
        m_Describe.text = m_Describe.text.Replace("\\n", "\n");
        if (data.Items == null)
        {
            return;
        }
        for (; index <data.Items.Count; index++)
        {           
            RewardDateVO mailReward = data.Items[index];
            Transform node = index < itemRootTf.childCount ? itemRootTf.GetChild(index) : Object.Instantiate(itemRootTf.GetChild(0), itemRootTf);
            Image icon = FindComponent<Image>(node, "Content/Icon/Icon");
            Image quality = FindComponent<Image>(node, "Content/Icon/Quality");
            TMP_Text name = FindComponent<TMP_Text>(node, "Content/Label_Name");
            TMP_Text num = FindComponent<TMP_Text>(node, "Content/Label_Num");
            name.text = TableUtil.GetItemName((int)mailReward.Id);
            num.text = mailReward.Num.ToString();
            quality.color = ColorUtil.GetColorByItemQuality(mailReward.Quality);
            string iconName = TableUtil.GetItemSquareIconImage(mailReward.Id);
            UIUtil.SetIconImage(icon, TableUtil.GetItemIconBundle(mailReward.Id), iconName);
            if (data.Got == 1)
            {
                icon.color = Color.gray;
            }
            else
            {
                icon.color = Color.white;
            }
            node.gameObject.SetActive(true);
        }
        for (; index < itemRootTf.childCount; index++)
        {
            itemRootTf.GetChild(index).gameObject.SetActive(false);
        }       
    }        

    #endregion


}
