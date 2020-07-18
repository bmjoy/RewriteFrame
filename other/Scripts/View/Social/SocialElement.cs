using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class SocialElement : MonoBehaviour
{
	/////////好友
	/// <summary>
	/// 头像图标
	/// </summary>
	private Image m_HeardImage;

	/// <summary>
	/// 在线图标
	/// </summary>
	private Image m_OnLineImage;

	/// <summary>
	/// 玩家名字
	/// </summary>
	private TextMeshProUGUI m_PlayerName;

	/// <summary>
	/// 玩家等级
	/// </summary>
	private TextMeshProUGUI m_LevelText;

	/// <summary>
	/// 段位
	/// </summary>
	private TextMeshProUGUI m_DanText;

	////////////////队伍
	
	/// <summary>
	/// 头像图标
	/// </summary>
	private Image m_TeamHeardImage;

	/// <summary>
	/// 在线图标
	/// </summary>
	private Image m_TeamOnLineImage;
	/// <summary>
	/// 队伍位置
	/// </summary>
	private int m_TeamPos;

	/// <summary>
	/// 队伍位置
	/// </summary>
	private Image m_TeamPosImage;

	/// <summary>
	/// 队长标识
	/// </summary>
	private Image m_TeamHeaderImage;

	/// <summary>
	/// 好友标识
	/// </summary>
	private Image m_TeamFriendImage;

	/// <summary>
	/// 语言标识
	/// </summary>
	private Image m_TeamVoiceImage;

	/// <summary>
	/// 队伍标识组
	/// </summary>
	private Transform m_TeamIamgeSignRoot;

	/// <summary>
	/// 内容物体
	/// </summary>
	private Transform m_Elelment;

	/// <summary>
	/// 空的内容
	/// </summary>
	private Transform m_NullElelment;

	/// <summary>
	/// 当前社交类型
	/// </summary>
	private UISocialList.SocialType m_SocialType;

	/// <summary>
	/// 好友proxy
	/// </summary>
	private FriendProxy m_FriendProxy;

	/// <summary>
	/// 队伍proxy
	/// </summary>
	private TeamProxy m_TeamProxy;

	/// <summary>
	/// EternityProxy
	/// </summary>
	private CfgEternityProxy m_CfgEternityProxy;

	/// <summary>
	/// 当前队伍数据
	/// </summary>
	private TeamMemberVO m_TeamMemberVO;

	/// <summary>
	/// 当前好友数据
	/// </summary>
	private FriendInfoVO m_FriendInfoVO;

	/// <summary>
	/// 队伍位置标识
	/// </summary>
	private int[] m_TeamPosIcons;

	public void Initialize()
	{
		m_FriendProxy = (FriendProxy)GameFacade.Instance.RetrieveProxy(ProxyName.FriendProxy);
		m_TeamProxy = (TeamProxy)GameFacade.Instance.RetrieveProxy(ProxyName.TeamProxy);
		m_CfgEternityProxy = (CfgEternityProxy)GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy);
		m_HeardImage = TransformUtil.FindUIObject<Image>(transform, "Content/Image_Head");
		m_OnLineImage = TransformUtil.FindUIObject<Image>(transform, "Content/Image_State");
		m_TeamHeardImage = TransformUtil.FindUIObject<Image>(transform, "Content/Image_Head2");
		m_TeamOnLineImage = TransformUtil.FindUIObject<Image>(transform, "Content/Image_State2");
		m_PlayerName = TransformUtil.FindUIObject<TextMeshProUGUI>(transform, "Content/Mask/Label_Name");
		m_LevelText = TransformUtil.FindUIObject<TextMeshProUGUI>(transform, "Content/Mask/Label_LV");
		m_DanText = TransformUtil.FindUIObject<TextMeshProUGUI>(transform, "Content/Mask/Label_Rank");
		m_TeamPosImage = TransformUtil.FindUIObject<Image>(transform, "Content/Image_Number");
		m_TeamHeaderImage = TransformUtil.FindUIObject<Image>(transform, "Content/Icon/Image_Mark1");
		m_TeamVoiceImage = TransformUtil.FindUIObject<Image>(transform, "Content/Icon/Image_Mark3");
		m_TeamFriendImage = TransformUtil.FindUIObject<Image>(transform, "Content/Icon/Image_Mark2");
		m_TeamIamgeSignRoot = TransformUtil.FindUIObject<Transform>(transform, "Content/Icon");
		m_NullElelment = TransformUtil.FindUIObject<Transform>(transform, "Image_Empty");
		m_Elelment = TransformUtil.FindUIObject<Transform>(transform, "Content");
		m_TeamPosIcons = new int[] {40005,40006,40007,40008,40009 };
	}

	/// <summary>
	/// 填充队伍数据
	/// </summary>
	public void SetTeamData(TeamMemberVO infoVO, UISocialList.SocialType socialType,Vector2Int vector2Int)
	{
		UIUtil.SetIconImage(m_TeamPosImage, (uint)m_TeamPosIcons[vector2Int.y]);
		if (infoVO.UID > 0)
		{
			m_NullElelment.gameObject.SetActive(false);
			m_Elelment.gameObject.SetActive(true);
			m_SocialType = socialType;
			SetContent();
			m_LevelText.text =string.Format(TableUtil.GetLanguageString("character_text_1019"), infoVO.Level.ToString()) ;
            m_DanText.text = string.Format(TableUtil.GetLanguageString("social_text_1004"), infoVO.DanLevel.ToString());
            m_PlayerName.text = infoVO.Name;
			if (infoVO.IsOnline)
			{
				m_TeamOnLineImage.gameObject.SetActive(true);
			}
			if (infoVO.TID>0)
			{
				if (m_CfgEternityProxy.GetPlayerByItemTId(infoVO.TID)!=null)
				{
					UIUtil.SetIconImage(m_TeamHeardImage, (uint)m_CfgEternityProxy.GetPlayerByItemTId(infoVO.TID).Value.HeadPortrait);
				}
			}
			if (m_FriendProxy.GetFriend(infoVO.UID) != null)//好友
			{
				m_TeamFriendImage.gameObject.SetActive(true);
			}
			else
			{
				m_TeamFriendImage.gameObject.SetActive(false);
			}
			if (m_TeamProxy.GetMember(infoVO.UID) == null)
			{
				m_TeamHeaderImage.gameObject.SetActive(false);
			}
			else
			{
				if (m_TeamProxy.GetMember(infoVO.UID).IsLeader)//队长
				{
					m_TeamHeaderImage.gameObject.SetActive(true);
				}
				else
				{
					m_TeamHeaderImage.gameObject.SetActive(false);
				}
			}
			
		
		}
		else
		{
			m_NullElelment.gameObject.SetActive(true);
			m_Elelment.gameObject.SetActive(false);
		}

	}

	/// <summary>
	/// 填充好友数据
	/// </summary>
	public void SetFriendData(FriendInfoVO infoVO, UISocialList.SocialType socialType)
	{
		if (infoVO.UID > 0)
		{
			m_NullElelment.gameObject.SetActive(false);
			m_Elelment.gameObject.SetActive(true);
			m_SocialType = socialType;
			SetContent();
			m_LevelText.text = string.Format(TableUtil.GetLanguageString("character_text_1019"), infoVO.Level.ToString());
			m_PlayerName.text = infoVO.Name;
			if (infoVO.TID>0)
			{
				if (m_CfgEternityProxy.GetPlayerByItemTId(infoVO.TID)!=null)
				{
					UIUtil.SetIconImage(m_HeardImage, (uint)m_CfgEternityProxy.GetPlayerByItemTId(infoVO.TID).Value.HeadPortrait);
				}
			}
			if (infoVO.Status == FriendInfoVO.FriendState.ONLINE)
			{
				m_OnLineImage.gameObject.SetActive(true);
			}
		}
		else
		{
			m_NullElelment.gameObject.SetActive(true);
			m_Elelment.gameObject.SetActive(false);
		}

	}
	/// <summary>
	/// 根据类型初始化面板内容
	/// </summary>
	private void SetContent()
	{
		m_TeamIamgeSignRoot.gameObject.SetActive(false);
		m_HeardImage.gameObject.SetActive(false);
		m_OnLineImage.gameObject.SetActive(false);
		m_TeamHeardImage.gameObject.SetActive(false);
		m_TeamOnLineImage.gameObject.SetActive(false);
		m_TeamPosImage.gameObject.SetActive(false);
		switch (m_SocialType)
		{
			case UISocialList.SocialType.Team:
				m_TeamPosImage.gameObject.SetActive(true);
				m_TeamIamgeSignRoot.gameObject.SetActive(true);
				m_TeamHeardImage.gameObject.SetActive(true);
				//m_TeamOnLineImage.gameObject.SetActive(true);
				break;
			case UISocialList.SocialType.Friend:
				m_HeardImage.gameObject.SetActive(true);
				break;
			case UISocialList.SocialType.Ship:
				break;
			case UISocialList.SocialType.Other:
				m_HeardImage.gameObject.SetActive(true);
				break;
			default:
				break;
		}
	}

}
