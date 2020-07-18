using PureMVC.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UITipTalentPart : UITipMissionPart
{
    private const string TIP_PREFAB = Leyoutech.Core.Loader.Config.AssetAddressKey.PRELOADUI_TIPSTALENTPANEL;
    /// <summary>
    /// 数据
    /// </summary>
    private TalentProxy m_TalentProxy;
    /// <summary>
    /// 数据
    /// </summary>
    private CfgEternityProxy m_CfgEternityProxy;
    /// <summary>
    /// 天赋名字
    /// </summary>
    private TextMeshProUGUI m_TalentName;
    /// <summary>
    /// 天赋类型
    /// </summary>
    private TextMeshProUGUI m_TypeLabel;
    /// <summary>
    /// 天赋等级
    /// </summary>
    private TextMeshProUGUI m_LevelLabel;
    /// <summary>
    /// 天赋描述
    /// </summary>
    private TextMeshProUGUI m_DescribeLabel;
    /// <summary>
    /// 解锁条件
    /// </summary>
    private TextMeshProUGUI m_UnLockLabel;
    /// <summary>
    /// 消耗道具数量
    /// </summary>
    private TextMeshProUGUI m_CostNum;
    /// <summary>
    /// 消耗道具名称
    /// </summary>
    private TextMeshProUGUI m_CostItemName;
    /// <summary>
    /// 消耗道具图标
    /// </summary>
    private Image m_CostImage;
    /// <summary>
    /// 天赋道具图标
    /// </summary>
    private Image m_TalentImage;
    /// <summary>
    /// 天赋品质
    /// </summary>
    private Image m_QualityImage;
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
    /// <summary>
    /// 空tips
    /// </summary>
    private GameObject m_EmptyTipsObj;
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
        m_EmptyTipsObj = null;
        base.OnHide();
    }
    public void InitSetData()
    {
        if (m_TalentName == null)
        {
            m_TalentName = FindComponent<TextMeshProUGUI>("TipsScrollView/Viewport/Content/Content/NameType/Label_Name");
            m_TypeLabel = FindComponent<TextMeshProUGUI>("TipsScrollView/Viewport/Content/Content/NameType/Label_Type");
            m_LevelLabel = FindComponent<TextMeshProUGUI>("TipsScrollView/Viewport/Content/Content/NameType/Label_Lv");
            m_DescribeLabel = FindComponent<TextMeshProUGUI>("TipsScrollView/Viewport/Content/Content/Describe/Label_Describe");
            m_UnLockLabel = FindComponent<TextMeshProUGUI>("TipsScrollView/Viewport/Content/Content/Locked/Label_Conditions");
            m_CostNum = FindComponent<TextMeshProUGUI>("TipsScrollView/Viewport/Content/Content/Cost/Resources/Content/Label_Num");
            m_CostItemName = FindComponent<TextMeshProUGUI>("TipsScrollView/Viewport/Content/Content/Cost/Resources/Content/Label_Name");
            m_CostImage = FindComponent<Image>("TipsScrollView/Viewport/Content/Content/Cost/Resources/Content/Icon/Icon");
            m_TalentImage = FindComponent<Image>("TipsScrollView/Viewport/Content/Content/NameType/Image_Icon");
            m_QualityImage = FindComponent<Image>("TipsScrollView/Viewport/Content/Back/Image_Quality");
        }
        m_TalentProxy = GameFacade.Instance.RetrieveProxy(ProxyName.TalentProxy) as TalentProxy;
        m_CfgEternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
    }

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
        if (TipData is TalentTypeVO|| TipData is TalentVO)
            OpenTip(TipData);
        else
            base.OpenTipView();
    }
    /// <summary>
    /// 打开Tip
    /// </summary>
    private void OpenTip(object data)
    {
        if (m_TipPrefab)
        {
            if (!m_TipInstance1)
            {
                m_TipInstance1 = m_TipPrefab.Spawn(TipBoxLeft);
                m_TipInstance1.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            }

            LayoutTip(m_TipInstance1, data);
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
    /// 加载数据
    /// </summary>
    /// <param name="tip">tip物体</param>
    /// <param name="data">数据</param>
    private void LayoutTip(GameObject tip, object data)
    {
        TalentVO talentVO = data as TalentVO;
        TalentTypeVO talentTypeVO = data as TalentTypeVO;
        if (talentVO != null)
        {
            if (talentVO.Id > 0)
            {
                InitSetData();
                if (talentVO.Level>0)
                {
                    m_DescribeLabel.text = TableUtil.GetTalentDescribe(talentVO.Id, talentVO.Level);
                }
                else
                {
                    m_DescribeLabel.text = TableUtil.GetTalentDescribe(talentVO.Id, 1);
                }
                m_TalentName.text = TableUtil.GetTalentNodeName(talentVO.Id);
                if (m_TalentProxy.GetTalentRootVODic().TryGetValue((int)talentVO.TalentRootId, out TalentTypeVO vO))
                {
                    m_TypeLabel.text = "type";
                }
                m_TypeLabel.text = "type";
                //m_PlayerName.text = data.Name;
                m_UnLockLabel.text = m_TalentProxy.GetUnLockLabel((uint)talentVO.UnLockId);
                m_CostNum.text = m_CfgEternityProxy.GetUpLevelCost((uint)talentVO.Id, talentVO.Level).ToString();
                m_LevelLabel.text = string.Format(TableUtil.GetLanguageString("shiphangar_text_1009"), talentVO.Level, talentVO.MaxLevel);
            }

        }
        else if (talentTypeVO != null)
        {
            if (talentTypeVO.Id > 0)
            {
                InitSetData();
                m_DescribeLabel.text = TableUtil.GetTalentDescribe(talentTypeVO.Id,talentTypeVO.Level);
                m_TalentName.text = TableUtil.GetTalentName(talentTypeVO.Id);
                m_TypeLabel.text = "type";
                m_LevelLabel.text = talentTypeVO.Level.ToString();
                m_CostNum.text = "";
                m_UnLockLabel.text = m_TalentProxy.GetUnLockLabel(talentTypeVO.UnLockId);
                //m_PlayerName.text = data.Name;
                //m_Level.text = string.Format(TableUtil.GetLanguageString("character_text_1019"), data.Level.ToString());
            }
        }
       
    }

    /// <summary>
    /// 查找指定节点下的组件
    /// </summary>
    /// <param name="path">相对节点的相对路径</param>
    /// <returns>对应组件</returns>
    protected new T FindComponent<T>(string path)
    {
        Transform result = m_TipInstance1.transform.Find(path);
        if (result)
        {
            return result.GetComponent<T>();
        }
        return default;
    }
}
