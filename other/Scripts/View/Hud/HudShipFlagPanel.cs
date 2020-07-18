using Assets.Scripts.Define;
using Eternity.FlatBuffer;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HudShipFlagPanel : HudBase
{
    private const string ASSET_ADDRESS = Leyoutech.Core.Loader.Config.AssetAddressKey.PRELOADUI_HUD_SHIPFLAGPANEL;

    /// <summary>
    /// 画布相机
    /// </summary>
    private Camera m_CanvasCamera;
    /// <summary>
    /// 标记容器
    /// </summary>
    private RectTransform m_FlagBox;
    /// <summary>
    /// 标记容器
    /// </summary>
    private RectTransform m_IdleBox;
    /// <summary>
    /// 标记模板
    /// </summary>
    private RectTransform m_FlagTemplate;


    /// <summary>
    /// 任务跟踪条目表
    /// </summary>
    private Dictionary<uint, RectTransform> m_NpcID2FlagA = new Dictionary<uint, RectTransform>();
    /// <summary>
    /// 任务跟踪条目表
    /// </summary>
    private Dictionary<uint, RectTransform> m_NpcID2FlagB = new Dictionary<uint, RectTransform>();

    public HudShipFlagPanel() : base(UIPanel.HudShipFlagPanel, ASSET_ADDRESS, PanelType.Hud) { }


    public override void Initialize()
    {
        m_FlagBox = FindComponent<RectTransform>("Content/FlagBox");
        m_IdleBox = FindComponent<RectTransform>("Content/IdleBox");
        m_FlagTemplate = FindComponent<RectTransform>("Content/NameFlag");

        m_CanvasCamera = m_FlagBox.GetComponentInParent<Canvas>().worldCamera;
    }

    public override void OnShow(object msg)
    {
        base.OnShow(msg);

        StartUpdate();
    }

    public override void OnHide(object msg)
    {
        m_NpcID2FlagA.Clear();
        m_NpcID2FlagB.Clear();

        while (m_FlagBox.childCount > 0)
        {
            m_FlagBox.GetChild(0).gameObject.SetActive(false);
            m_FlagBox.GetChild(0).SetParent(m_IdleBox);
        }

        base.OnHide(msg);
    }

    /// <summary>
    /// 更新
    /// </summary>
    protected override void Update()
    {
        CfgEternityProxy eternityProxy = Facade.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
        TaskTrackingProxy taskTrackingProxy = Facade.RetrieveProxy(ProxyName.TaskTrackingProxy) as TaskTrackingProxy;

        if (!IsWatchOrUIInputMode() && !IsDead() && !IsLeaping())
        {
            GameplayProxy gamePlayProxy = Facade.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;
            BaseEntity main = gamePlayProxy.GetEntityById<BaseEntity>(gamePlayProxy.GetMainPlayerUID());

            List<SpacecraftEntity> humanList = gamePlayProxy.GetEntities<SpacecraftEntity>(KHeroType.htNpc);
            foreach (SpacecraftEntity entity in humanList)
            {
                if (entity.GetHeroType() != KHeroType.htNpc)
                    continue;

                Npc entityVO = eternityProxy.GetNpcByKey(entity.GetTemplateID());
                if (entityVO.Display == 0)
                    continue;

                Vector3 titleOffset = entityVO.HeroHeaderLength >= 3 ? new Vector3(entityVO.HeroHeader(0), entityVO.HeroHeader(1), entityVO.HeroHeader(2)) : Vector3.zero;
                Vector3 targetPosition = entity.transform.position + entity.transform.TransformDirection(titleOffset);

                //屏幕外忽略
                if (!IsInScreen(targetPosition, Camera.main))
                    continue;

                //太远忽略
                float targetDistance = Vector3.Distance(entity.transform.position, main.transform.position) * GameConstant.METRE_PER_UNIT;
                if (targetDistance >= entityVO.MissionTargetHiddenDistance * GameConstant.METRE_PER_UNIT)
                    continue;

                Vector2 iconPosition;
                Vector3 screenPosition = Camera.main.WorldToScreenPoint(targetPosition);
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(m_FlagBox, screenPosition, m_CanvasCamera, out iconPosition))
                {
                    bool isNew = false;
                    uint entityID = entity.GetUId();

                    //创建标记
                    RectTransform flag = m_NpcID2FlagA.ContainsKey(entityID) ? m_NpcID2FlagA[entityID] : null;
                    if (flag == null)
                    {
                        if (m_IdleBox.childCount > 0)
                        {
                            flag = m_IdleBox.GetChild(0).GetComponent<RectTransform>();
                            flag.gameObject.SetActive(true);
                            flag.SetParent(m_FlagBox);
                        }
                        else
                        {
                            flag = Object.Instantiate(m_FlagTemplate, m_FlagBox);
                            flag.gameObject.SetActive(true);
                        }
                        isNew = true;
                    }
                    m_NpcID2FlagA.Remove(entityID);
                    m_NpcID2FlagB.Add(entityID, flag);

                    //标记坐标
                    flag.anchoredPosition = new Vector2(iconPosition.x, iconPosition.y);

                    //名称文本
                    if (isNew)
                    {
                        flag.Find("Name/Label_NPCName").GetComponent<TMP_Text>().text = TableUtil.GetNpcName(entity.GetTemplateID());

                        Image npcIcon = flag.Find("IconBox/IconScenceNameElement/Image_NpcIcon").GetComponent<Image>();
                        npcIcon.color = entityVO.NameBoardIcon == 0 ? Color.clear : Color.white;
                        if (entityVO.NameBoardIcon != 0)
                            UIUtil.SetIconImage(npcIcon, entityVO.NameBoardIcon);
                    }

                    //任务
                    TaskTrackingProxy.TrackingInfo tracking = taskTrackingProxy.GetNpcMission(entity.GetUId(), entity.GetTemplateID());
                    MissionType missionType = tracking != null ? tracking.MissionType : MissionType.None;

                    //任务状态
                    RectTransform flagIcon = FindComponent<RectTransform>(flag, "IconBox");
                    RectTransform flagMission = FindComponent<RectTransform>(flag, "MissionBox");
                    Animator flagMissionAnimator = FindComponent<Animator>(flagMission, "IconMissionElement");
                    Image flagMissionIcon = FindComponent<Image>(flagMissionAnimator, "Icon").GetComponent<Image>();

                    UIUtil.SetIconImage(flagMissionIcon, GameConstant.FUNCTION_ICON_ATLAS_ASSETADDRESS, GetMissionIcon(missionType));

                    flagMission.gameObject.SetActive(missionType != MissionType.None);
                    flagIcon.gameObject.SetActive(!flagMission.gameObject.activeSelf);
                    taskTrackingProxy.SetAreadyAddMissionInfo(entity.GetUId(), entity.GetTemplateID());

                    flagMissionAnimator.SetBool("Finished", tracking != null && tracking.MissionState == MissionState.Finished);

                    /*
                    //图标动画
                    Animator flagIconAnimator = flag.Find("IconBox").GetComponent<Animator>();
                    if (flagIconAnimator != null && flagIconAnimator.runtimeAnimatorController != null && flagIconAnimator.isActiveAndEnabled)
                    {
                        flagIconAnimator.ResetTrigger("Open");
                        flagIconAnimator.ResetTrigger("Close");
                        flagIconAnimator.SetTrigger(targetDistance <= GameConstant.DEFAULT_HUMAN_TITLE_SHOW_RANGE * 2 ? "Open" : "Close");
                    }

                    //名称动画
                    Animator flagTextAnimator = flag.Find("Name").GetComponent<Animator>();
                    if (flagTextAnimator != null && flagTextAnimator.runtimeAnimatorController != null && flagTextAnimator.isActiveAndEnabled)
                    {
                        flagTextAnimator.ResetTrigger("Open");
                        flagTextAnimator.ResetTrigger("Close");
                        flagTextAnimator.SetTrigger(targetDistance <= GameConstant.DEFAULT_HUMAN_TITLE_SHOW_RANGE ? "Open" : "Close");
                    }
                    */
                }

            }
        }

		//回收标记
		foreach (var flag in m_NpcID2FlagA)
		{
			taskTrackingProxy.SetAreadyAddMissionInfo(flag.Key, 0, true);
			flag.Value.gameObject.SetActive(false);
			flag.Value.SetParent(m_IdleBox);
		}
		m_NpcID2FlagA.Clear();

		//交换缓存
		Dictionary<uint, RectTransform> tmp = m_NpcID2FlagA;
		m_NpcID2FlagA = m_NpcID2FlagB;
		m_NpcID2FlagB = tmp;
	}
}
