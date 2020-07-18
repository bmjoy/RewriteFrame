using DG.Tweening;
using Eternity.FlatBuffer;
using PureMVC.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Material = UnityEngine.Material;
using SystemObject = System.Object;


public class HudVoicePanel : HudBase
{
    /// <summary>
    /// 资源地址
    /// </summary>
    private const string ASSET_ADDRESS = Leyoutech.Core.Loader.Config.AssetAddressKey.PRELOADUI_HUD_VOICEASSISTANTPANEL;
    /// <summary>
    /// 根节点
    /// </summary>
    private RectTransform m_Root;
    /// <summary>
    /// 动画
    /// </summary>
    private DOTweenAnimation m_Tween;
    /// <summary>
    /// 名字
    /// </summary>
    private TMP_Text m_Name;
    /// <summary>
    /// 头像
    /// </summary>
    private Image m_Icon;
    /// <summary>
    /// 音效
    /// </summary>
    private Image m_Audio;
    /// <summary>
    /// 叠加音效
    /// </summary>
    private Image m_RepeatAudio;
    /// <summary>
    /// 组id
    /// </summary>
    private int m_GroupId;
    /// <summary>
    /// 组优先级
    /// </summary>
    private int m_GroupPriority = 0;
    /// <summary>
    /// 通讯信息列表
    /// </summary>
    private List<VideoPhone?> m_VideoPhones;
    /// <summary>
    /// 音效动画起始位置
    /// </summary>
    private const int m_StartValue = 400;
    /// <summary>
    /// CfgEternityProxy
    /// </summary>
    private CfgEternityProxy m_CfgEternityProxy;
    /// <summary>
    /// 音效节点
    /// </summary>
    public GameObject m_SoundPlayObj = null;
    /// <summary>
    /// 播放参数
    /// </summary>
    private PlayParameter m_PlayParameter;
    public HudVoicePanel() : base(UIPanel.HudVoicePanel, ASSET_ADDRESS, PanelType.Hud) { }

    public override void Initialize()
    {
        m_Root = FindComponent<RectTransform>("Content/Sounds");
        m_Tween = FindComponent<DOTweenAnimation>("Content/Sounds");
        m_Name = FindComponent<TMP_Text>("Content/Sounds/Sound/Name/Label_Name");
        m_Icon = FindComponent<Image>("Content/Sounds/Sound/Icon");
        m_Audio = FindComponent<Image>("Content/Sounds/Sound/SoundWaves/Lines");
        m_RepeatAudio = FindComponent<Image>("Content/Sounds/Sound/SoundWaves/Lines");
        m_CfgEternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
        m_VideoPhones = new List<VideoPhone?>();

        if (m_SoundPlayObj == null)
        {
            m_SoundPlayObj = new GameObject("myWwiseSound");
            m_SoundPlayObj.transform.SetParent(m_Root, false);
        }
    }

    public override void OnShow(object msg)
    {
        base.OnShow(msg);
        m_Root.gameObject.SetActive(false);
    }

    public override void OnHide(object msg)
    {
        base.OnHide(msg);
    }

    public override NotificationName[] ListNotificationInterests()
    {
        return new NotificationName[]
        {
            NotificationName.VideoPhoneChange,
        };
    }

    public override void HandleNotification(INotification notification)
    {
        switch (notification.Name)
        {
            case NotificationName.VideoPhoneChange:
                ChangeVideoList((PlayParameter)notification.Body);
                break;
        }
    }
    /// <summary>
    /// 修改通讯列表
    /// </summary>
    /// <param name="groupId">组id</param>
    private void ChangeVideoList(PlayParameter parameter)
    {
        List<VideoPhone?> videoPhones = m_CfgEternityProxy.GetVideoPhoneList(parameter.groupId);
        if (videoPhones.Count == 0 || videoPhones == null)
        {
            return;
        }
        int GroupPriority = videoPhones[0].Value.GroupPriority;

        if (GroupPriority >= m_GroupPriority)
        {
            if (m_VideoPhones.Count > 0)
            {
                m_PlayParameter.action?.Invoke();
                if (m_Root.anchoredPosition.x == m_StartValue)
                {
                    m_Root.gameObject.SetActive(false);
                    m_VideoPhones = videoPhones;
                    m_GroupPriority = GroupPriority;
                    m_PlayParameter = parameter;
                    ShowVideoPhoneList(0);
                }
                else
                {
                    m_Tween.DOPlayBackwards();
                    m_Tween.onRewind.AddListener(() =>
                    {
                        m_Root.gameObject.SetActive(false);
                        m_VideoPhones = videoPhones;
                        m_GroupPriority = GroupPriority;
                        m_PlayParameter = parameter;
                        ShowVideoPhoneList(0);
                        m_Tween.onRewind.RemoveAllListeners();
                    });
                }
            }
            else
            {
                m_VideoPhones = videoPhones;
                m_GroupPriority = GroupPriority;
                m_PlayParameter = parameter;
                ShowVideoPhoneList(0);
            }
        }
        else
        {
            parameter.action?.Invoke();
        }
    }
    /// <summary>
    /// 显示通讯列表
    /// </summary>
    /// <param name="groupId"></param>
    private void ShowVideoPhoneList(int index)
    {
        if (!m_Root.gameObject.activeSelf)
        {
            m_Root.gameObject.SetActive(true);
        }
        if (m_PlayParameter.npcId > 0)
        {
            m_Name.text = m_CfgEternityProxy.GetNpcByKey(m_PlayParameter.npcId).Name;
        }
        else
        {
            m_Name.text = TableUtil.GetLanguageString(string.Format("video_phone_name_{0}", m_VideoPhones[index].Value.Id));
        }        
        UIUtil.SetIconImage(m_Icon, (uint)m_VideoPhones[index].Value.Icon);

        if (m_SoundPlayObj != null )
        {
            GameObject.DestroyImmediate(m_SoundPlayObj.GetComponent<AK.Wwise.TAkGameObjEventMonitor>());
			WwiseUtil.PlaySound(m_VideoPhones[index].Value.Voice, false, m_SoundPlayObj.transform, OnPlayEnd, new UserEndData(m_VideoPhones.GetHashCode(), index));
		}

		// m_Coroutine = UIManager.Instance.StartCoroutine(Excute(2f, (int tindex) =>
		//{
		//    Debug.LogError("---------------> play  hasdId = " + m_VideoPhones.GetHashCode() + " tindex = " + tindex);
		//     //m_Index = tindex;
		//     WwiseUtil.PlaySound(11, false, m_SoundPlayObj.transform, OnPlayEnd, new UserEndData(m_VideoPhones.GetHashCode(), tindex));
		//}, index));

		AssetUtil.LoadAssetAsync(m_VideoPhones[index].Value.VoiceEffect,
            (pathOrAddress, returnObject, userData) =>
            {
                if (returnObject != null)
                {
                    Material audio = (Material)returnObject;
                    m_Audio.material = audio;
                    m_RepeatAudio.material = audio;
                }
                else
                {
                    Debug.LogError(string.Format("资源加载成功，但返回null,  pathOrAddress = {0}", pathOrAddress));
                }
            });
    }

    /// <summary>
    /// 播放结束时
    /// </summary>
    private void OnPlayEnd(SystemObject userEndData)
    {
        int hashId = ((UserEndData)userEndData).hashId;
        int tIndex = ((UserEndData)userEndData).tIndex;
        if (m_VideoPhones.GetHashCode() != hashId)
        {            
            return;
        }

        m_Tween.DOPlayBackwards();
        m_Tween.onRewind.AddListener(() =>
        {
            m_Root.gameObject.SetActive(false);
            if (tIndex < m_VideoPhones.Count - 1)
            {
                int index = tIndex + 1;
                ShowVideoPhoneList(index);
            }
            else
            {
                m_GroupPriority = 0;
                m_VideoPhones.Clear();
                m_PlayParameter.action?.Invoke();
            }
            m_Tween.onRewind.RemoveAllListeners();
        });
    }

    public class UserEndData
    {
        public int hashId;
        public int tIndex;
        public UserEndData (int hashid, int id)
        {
            hashId = hashid;
            tIndex = id;
        }
    }   
}
