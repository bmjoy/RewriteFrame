using Eternity.FlatBuffer;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UITipMissionPart : UITipMailPart
{
    private const string TIP_PREFAB = Leyoutech.Core.Loader.Config.AssetAddressKey.PRELOADUI_TIPSMISSIONPANEL;

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
    /// 奖励ID列表
    /// </summary>
    private List<int> m_RewardIDs = new List<int>();
    /// <summary>
    /// 奖励数量列表
    /// </summary>
    private List<int> m_RewardCounts = new List<int>();
    /// <summary>
    /// 时间文本字段
    /// </summary>
    private Dictionary<TMP_Text, CountdownTime> m_TimeTextDic = new Dictionary<TMP_Text, CountdownTime>();
    /// <summary>
    /// 时间文本框缓存
    /// </summary>
    private List<KeyValuePair<TMP_Text, CountdownTime>> m_TimeTextCache;

    private struct CountdownTime
    {
        public string text;
        public ulong deadline;
    }


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
        if (TipData is MissionVO)
            OpenTip(TipData as MissionVO);
        else
            base.OpenTipView();
    }

    /// <summary>
    /// 打开Tip
    /// </summary>
    private void OpenTip(MissionVO data)
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
    /// 布局Tip
    /// </summary>
    /// <param name="view"></param>
    /// <param name="data"></param>
    private void LayoutTip(GameObject view, MissionVO mission)
    {
        m_TimeTextDic.Clear();

        CfgEternityProxy configs = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;

        Transform root = FindComponent<Transform>(view.transform, "TipsScrollView/Viewport/Content/Content");

        Image martkImage = FindComponent<Image>(root, "NameType/Image_Mark");

        TMP_Text nameField = FindComponent<TMP_Text>(root, "NameType/Label_Name");
        Image typeImage = FindComponent<Image>(root, "NameType/Image_IconType");
        TMP_Text typeField = FindComponent<TMP_Text>(root, "NameType/Label_Type");

        TMP_Text limitField = FindComponent<TMP_Text>(root, "Limit/Label");
        TMP_Text descField = FindComponent<TMP_Text>(root, "Describe/Label_Describe");

        MissionProxy missionProxy = GameFacade.Instance.RetrieveProxy(ProxyName.MissionProxy) as MissionProxy;

        martkImage.gameObject.SetActive(missionProxy.GetMissionTrack().IndexOf(mission.Tid) != -1);

        nameField.text = TableUtil.GetLanguageString("mission_name_" + mission.Tid);
        typeField.text = TableUtil.GetLanguageString("tips_text_id_" + (1084 + (int)mission.MissionType));

        UIUtil.SetIconImageSquare(typeImage, (uint)configs.GetMissionIconIdBy(mission.MissionType));

        limitField.text = string.Format(TableUtil.GetLanguageString("mission_title_1011"), mission.MissionConfig.LvLimit);
        descField.text = TableUtil.GetLanguageString("mission_main_detailedDesc_" + mission.Tid);

        //目标列表
        UpdateMissionList(mission, FindComponent<Transform>(root, "TargetList/Targets"));

        //奖励列表
        UpdateRewardList(mission, FindComponent<Transform>(root, "Materials/Resources"));

        //检查时间
        CheckTime();
    }

    #region 任务奖励列表

    /// <summary>
    /// 更新奖励列表
    /// </summary>
    /// <param name="mission">任务数据</param>
    /// <param name="rewardBox">奖励容器</param>
    private void UpdateRewardList(MissionVO mission, Transform rewardBox)
    {
        CfgEternityProxy configs = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;

        m_RewardIDs.Clear();
        m_RewardCounts.Clear();

        MissionMain cfg = mission.MissionMainConfig.Value;
        if (cfg.ItemId1 > 0)
        {
            m_RewardIDs.Add(cfg.ItemId1);
            m_RewardCounts.Add(cfg.ItemNum1);
        }
        if (cfg.ItemId2 > 0)
        {
            m_RewardIDs.Add(cfg.ItemId2);
            m_RewardCounts.Add(cfg.ItemNum2);
        }
        if (cfg.ItemId3 > 0)
        {
            m_RewardIDs.Add(cfg.ItemId3);
            m_RewardCounts.Add(cfg.ItemNum3);
        }
        if (cfg.ItemId4 > 0)
        {
            m_RewardIDs.Add(cfg.ItemId4);
            m_RewardCounts.Add(cfg.ItemNum4);
        }
        if (cfg.ItemId5 > 0)
        {
            m_RewardIDs.Add(cfg.ItemId5);
            m_RewardCounts.Add(cfg.ItemNum5);
        }

        int index = 0;
        for (; index < m_RewardIDs.Count; index++)
        {
            int itemID = m_RewardIDs[index];
            int itemCount = m_RewardCounts[index];
            Item item = configs.GetItemByKey((uint)m_RewardIDs[index]);

            Transform node = index < rewardBox.childCount ? rewardBox.GetChild(index) : Object.Instantiate(rewardBox.GetChild(0), rewardBox);
            Image icon = FindComponent<Image>(node, "Icon/Icon");
            Image quality = FindComponent<Image>(node, "Icon/Quality");
            TMP_Text name = FindComponent<TMP_Text>(node, "Label_Name");
            TMP_Text count = FindComponent<TMP_Text>(node, "Label_Num");

            node.gameObject.SetActive(true);
            UIUtil.SetIconImageSquare(icon, item.Icon);
            quality.color = ColorUtil.GetColorByItemQuality(item.Quality);
            name.text = TableUtil.GetItemName(itemID);
            count.text = itemCount.ToString();
        }
        for (; index < rewardBox.childCount; index++)
        {
            rewardBox.GetChild(index).gameObject.SetActive(false);
        }
    }
    #endregion

    #region 任务目标列表

    /// <summary>
    /// 更新任务目标
    /// </summary>
    /// <param name="mission">任务数据</param>
    /// <param name="rewardBox">目标容器</param>
    private void UpdateMissionList(MissionVO mission, Transform rowBox)
    {
        RectTransform rowTemplate = rowBox.GetChild(0).GetComponent<RectTransform>();

        CfgEternityProxy cfgEternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;

        bool finised = true;
        int index = 0;
        if (mission.MissionState == MissionState.CanAccept)
        {
            string str = string.Empty;
            if (GetRoleLevel() < mission.MissionConfig.LvLimit)
            {
                str = string.Format(TableUtil.GetLanguageString("mission_title_1011"), mission.MissionConfig.LvLimit);
            }
            else if (mission.MissionMainConfig.Value.AcceptNpcId > 0)
            {
                str = TableUtil.GetNpcName((uint)mission.MissionMainConfig.Value.AcceptNpcId);
                str = string.Format(TableUtil.GetLanguageString("mission_title_1012"), str);
            }
            RectTransform row = index < rowBox.childCount ? rowBox.GetChild(index).GetComponent<RectTransform>() : Object.Instantiate(rowTemplate, rowBox);
            row.gameObject.SetActive(true);
            UpdateMissionRow(str, string.Empty, row, true, false, false);
            finised = false;
            index++;
        }
        else
        {
            foreach (KeyValuePair<uint, SortedDictionary<uint, MissionTargetVO>> group in mission.MissionGroupTargetList)
            {
                RectTransform row = index < rowBox.childCount ? rowBox.GetChild(index).GetComponent<RectTransform>() : Object.Instantiate(rowTemplate, rowBox);
                if (index != 0 && !finised)
                {
                    row.gameObject.SetActive(true);
                    UpdateMissionRow(string.Empty, string.Empty, row, false, true, false);
                    index++;
                    row = index < rowBox.childCount ? rowBox.GetChild(index).GetComponent<RectTransform>() : Object.Instantiate(rowTemplate, rowBox);
                }
                bool frist = true;
                foreach (KeyValuePair<uint, MissionTargetVO> target in group.Value)
                {
                    if (!target.Value.DoneToFinish)
                    {
                        continue;
                    }
                    row = index < rowBox.childCount ? rowBox.GetChild(index).GetComponent<RectTransform>() : Object.Instantiate(rowTemplate, rowBox);
                    string str = string.Empty;
                    if (target.Value.DoneToFinish)//正常条件
                    {
                        switch (target.Value.MissionTargetType)
                        {
                            case MissionTargetType.Kill:
                            case MissionTargetType.CollectItem:
                            case MissionTargetType.HaveItem:
                                str = $"[{target.Value.Value}/{target.Value.MissionTargetConfig.Arg2}]";
                                break;
                            case MissionTargetType.Escort:
                            default:
                                str = target.Value.TargetState == MissionState.Finished ? "[1/1]" : "[0/1]";
                                break;
                        }
                        if (target.Value.TargetState == MissionState.Failed)
                        {
                            str = TableUtil.GetLanguageString("mission_title_1013");
                        }
                    }

                    row.gameObject.SetActive(true);
                    row.name = target.Value.Tid + "  " + TableUtil.GetMissionTargetDesc(target.Value.Tid);
                    if (target.Value.MissionTargetConfig.Arg4 > 0 && target.Value.Relation != null)
                    {
                        if (target.Value.Relation.MissionTargetType == MissionTargetType.TimeOut)
                        {
                            UpdateMissionRow(TableUtil.GetMissionTargetDesc(target.Value.Tid), str, row, false, false, frist, target.Value.TargetState, (ulong)target.Value.Relation.Value);
                        }
                    }
                    else
                    {
                        UpdateMissionRow(TableUtil.GetMissionTargetDesc(target.Value.Tid), str, row, false, false, frist, target.Value.TargetState);
                    }
                    finised = target.Value.TargetState == MissionState.Finished;
                    index++;
                    frist = false;
                }
            }
        }

        for (int i = index; i < rowBox.childCount; i++)
        {
            rowBox.GetChild(i).gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 任务行（target）数据
    /// </summary>
    /// <param name="targetFieldText"></param>
    /// <param name="numberFieldText"></param>
    /// <param name="view"></param>
    /// <param name="isCan"></param>
    /// <param name="isShowOr"></param>
    /// <param name="isFirstInGroup"></param>
    /// <param name="missionState"></param>
    private void UpdateMissionRow(string targetFieldText, string numberFieldText, RectTransform view, bool isCan, bool isShowOr, bool isFirstInGroup, MissionState missionState = MissionState.None, ulong deadline = 0)
    {
        //正常进行
        Transform normalBox = view.Find("TargetNormal");
        TMP_Text targetFieldNormal = view.Find("TargetNormal/Left/Label_Target").GetComponent<TMP_Text>();
        TMP_Text numberFieldNormal = view.Find("TargetNormal/Label_Num").GetComponent<TMP_Text>();
        //TMP_Text timeFieldNormal = view.Find("TargetNormal/Label_time").GetComponent<TMP_Text>();
        Transform orBox = view.Find("TargetNormal/Label_or");
        Transform pointBox = view.Find("TargetNormal/Left/Image_point/Image");
        //灰色 已经完成
        Transform grayBox = view.Find("TargetGray");
        TMP_Text targetFieldGray = view.Find("TargetGray/Left/Label_Target").GetComponent<TMP_Text>();
        Transform pointBoxGray = view.Find("TargetGray/Left/Image_point/Image");
        TMP_Text numberFieldGray = view.Find("TargetGray/Label_Num").GetComponent<TMP_Text>();
        //失败了的
        Transform failBox = view.Find("TargetFail");
        TMP_Text targetFieldFail = view.Find("TargetFail/Left/Label_Target").GetComponent<TMP_Text>();
        TMP_Text numberFieldFail = view.Find("TargetFail/Label_Num").GetComponent<TMP_Text>();
        Transform pointBoxFail = view.Find("TargetFail/Left/Image_point/Image");


        orBox.gameObject.SetActive(isShowOr);
        pointBox.gameObject.SetActive(!isShowOr && isFirstInGroup);
        pointBoxGray.gameObject.SetActive(!isShowOr && isFirstInGroup);
        pointBoxFail.gameObject.SetActive(!isShowOr && isFirstInGroup);
        targetFieldNormal.gameObject.SetActive(!isShowOr);
        numberFieldNormal.gameObject.SetActive(!isShowOr);
        if (isShowOr)
        {
            normalBox.gameObject.SetActive(isShowOr);
            grayBox.gameObject.SetActive(!isShowOr);
            failBox.gameObject.SetActive(!isShowOr);
            //timeFieldNormal.gameObject.SetActive(false);
            return;
        }
        if (isCan)
        {
            normalBox.gameObject.SetActive(true);
            grayBox.gameObject.SetActive(false);
            failBox.gameObject.SetActive(false);
            targetFieldNormal.gameObject.SetActive(true);
            numberFieldNormal.gameObject.SetActive(false);
            //timeFieldNormal.gameObject.SetActive(false);
            targetFieldNormal.text = targetFieldGray.text = targetFieldText;
        }
        else
        {
            targetFieldFail.text = targetFieldNormal.text = targetFieldGray.text = targetFieldText;
            numberFieldFail.text = numberFieldNormal.text = numberFieldText;
            normalBox.gameObject.SetActive(missionState != MissionState.Finished && missionState != MissionState.Failed);
            grayBox.gameObject.SetActive(missionState == MissionState.Finished);
            failBox.gameObject.SetActive(missionState == MissionState.Failed);

            if (missionState == MissionState.Going && deadline > ServerTimeUtil.Instance.GetNowTime())
            {
                m_TimeTextDic.Add(targetFieldNormal, new CountdownTime() { text = targetFieldText, deadline = deadline });
                //timeFieldNormal.gameObject.SetActive(true);
                CheckShowTime(targetFieldNormal, m_TimeTextDic[targetFieldNormal]);
            }
            else
            {
                //timeFieldNormal.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// 检查时间
    /// </summary>
    private void CheckTime()
    {
        ServerTimeUtil.Instance.OnTick -= Tick;

        if (m_TimeTextDic.Count > 0)
        {
            ServerTimeUtil.Instance.OnTick += Tick;
        }
    }

    /// <summary>
    /// 每秒检查
    /// </summary>
    private void Tick()
    {
        if (m_TipInstance1 != null && m_TimeTextDic.Count > 0)
        {
            m_TimeTextCache = m_TimeTextDic.ToList();
            foreach (var item in m_TimeTextCache)
            {
                CheckShowTime(item.Key, item.Value);
                m_TimeTextCache = m_TimeTextDic.ToList();
            }
        }
        else
        {
            ServerTimeUtil.Instance.OnTick -= Tick;
        }
    }

    /// <summary>
    /// 检查显示时间
    /// </summary>
    /// <param name="text">文本字段</param>
    /// <param name="deadline"></param>
    private void CheckShowTime(TMP_Text text, CountdownTime info)
    {
        if (ServerTimeUtil.Instance.GetNowTime() < info.deadline)
        {
            text.text = info.text + " " + $"[{TimeUtil.GetTimeStr((long)info.deadline - (long)ServerTimeUtil.Instance.GetNowTime())}]";
            //text.gameObject.SetActive(true);
        }
        else
        {
            //text.gameObject.SetActive(false);
            m_TimeTextDic.Remove(text);
        }
    }

    /// <summary>
    /// 获取角色等级
    /// </summary>
    /// <returns>等级</returns>
    private uint GetRoleLevel()
    {
        GameplayProxy gameplayProxy = GameFacade.Instance.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;
        BaseEntity mainPlayer = gameplayProxy.GetEntityById<BaseEntity>(gameplayProxy.GetMainPlayerUID());
        return mainPlayer?.GetLevel() ?? 1;
    }

    #endregion
}
