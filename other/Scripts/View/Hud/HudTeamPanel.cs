using PureMVC.Interfaces;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class HudTeamPanel : HudBase
{
	private const string ASSET_ADDRESS = "Assets/Artwork/UI/Prefabs/Hud/HUD_TeamPanel.prefab";

	/// <summary>
	/// 队伍Proxy
	/// </summary>
	private TeamProxy m_TeamProxy;

	/// <summary>
	/// 好友proxy
	/// </summary>
	private FriendProxy m_FriendProxy;

	/// <summary>
	/// 服务器proxy
	/// </summary>
	private ServerListProxy m_ServerListProxy;

	/// <summary>
	/// 队伍的信息挂点
	/// </summary>
	private Transform m_TeamContentRoot;

	/// <summary>
	/// 队伍的信息
	/// </summary>
	private Transform[] m_TeamContents;

	/// <summary>
	/// 队伍的信息CanvasGroup
	/// </summary>
	private CanvasGroup[] m_TeamCanvasGroup;

	/// <summary>
	/// 队伍的信息名字Text
	/// </summary>
	private TextMeshProUGUI[] m_TeamNameLabel;

	/// <summary>
	/// 队伍的信息护盾Slider
	/// </summary>
	private Slider[] m_TeamDefenseSlider;

	/// <summary>
	/// 队伍的信息血量Slider
	/// </summary>
	private Slider[] m_TeamHPSlider;

	/// <summary>
	/// 是否在线标志
	/// </summary>
	private GameObject[] m_TypeOnLine;

	/// <summary>
	/// 是否存活
	/// </summary>
	private GameObject []m_TypeIsAlive;

    /// <summary>
    /// 透明度
    /// </summary>
    private float m_Alpha = 0.2f;


    public HudTeamPanel() : base(UIPanel.HudTeamPanel, ASSET_ADDRESS, PanelType.Hud)
	{
	}
	
	public override void Initialize()
	{
		base.Initialize();

		m_TeamProxy = (TeamProxy)Facade.RetrieveProxy(ProxyName.TeamProxy);
		m_FriendProxy = (FriendProxy)Facade.RetrieveProxy(ProxyName.FriendProxy);
		m_ServerListProxy = (ServerListProxy)Facade.RetrieveProxy(ProxyName.ServerListProxy);
		m_TeamContentRoot = FindComponent<Transform>("Content");
		m_TypeIsAlive = new GameObject[m_TeamProxy.MEMBERCOUNTLIMIT];
		m_TypeOnLine = new GameObject[m_TeamProxy.MEMBERCOUNTLIMIT];
		m_TeamContents = new Transform[m_TeamProxy.MEMBERCOUNTLIMIT];
		m_TeamCanvasGroup = new CanvasGroup[m_TeamProxy.MEMBERCOUNTLIMIT];
		m_TeamNameLabel = new TextMeshProUGUI[m_TeamProxy.MEMBERCOUNTLIMIT];
		m_TeamDefenseSlider = new Slider[m_TeamProxy.MEMBERCOUNTLIMIT];
		m_TeamHPSlider = new Slider[m_TeamProxy.MEMBERCOUNTLIMIT];
		for (int i = 0; i < m_TeamContentRoot.childCount; i++)
		{
			m_TeamContents[i] = m_TeamContentRoot.GetChild(i).GetChild(0);
			m_TeamCanvasGroup[i] = m_TeamContentRoot.GetChild(i).GetOrAddComponent<CanvasGroup>();
			m_TeamNameLabel[i] = m_TeamContentRoot.GetChild(i).Find("Normal/Name/Name").GetComponent<TextMeshProUGUI>();
			m_TeamDefenseSlider[i] = m_TeamContentRoot.GetChild(i).Find("Normal/Slider/Slider_MP").GetComponent<Slider>();
			m_TeamHPSlider[i] = m_TeamContentRoot.GetChild(i).Find("Normal/Slider/Slider_Hp").GetComponent<Slider>();
			m_TypeIsAlive[i] = m_TeamContentRoot.GetChild(i).Find("Normal/Type1").gameObject;
			m_TypeOnLine[i] = m_TeamContentRoot.GetChild(i).Find("Normal/Type2").gameObject;
			Debug.Log(m_TeamDefenseSlider[i]);
		}

	}
	public override void OnRefresh(object msg)
	{
		base.OnRefresh(msg);
		NetworkManager.Instance.GetTeamController().GetTeamList();//获取组队列表
		m_FriendProxy.GetNearbyList();
		FillPanel();
	}

	public override void OnShow(object objs)
	{
		base.OnShow(objs);
	}

	public override void OnHide(object objs)
	{
		base.OnHide(objs);
	}
	public override NotificationName[] ListNotificationInterests()
	{
		return new NotificationName[]
		{
			NotificationName.MSG_TEAM_BATTLE_UPDATE,
			NotificationName.MSG_TEAM_MEMBER_UPDATE
		};
	}

	public override void HandleNotification(INotification notification)
	{
		switch ((NotificationName)notification.Name)
		{
			case NotificationName.MSG_TEAM_BATTLE_UPDATE:
				ulong id = (ulong)notification.Body;
				if (m_TeamProxy.GetMember(id) !=null)
				{
					FillPanel();
				}
				break;
			case NotificationName.MSG_TEAM_MEMBER_UPDATE://好友列表更新
				FillPanel();
				break;
			default:
				break;
		}
	}

	/// <summary>
	/// 打开界面时屏蔽
	/// </summary>
	protected override void OnInputMapChanged()
	{
		this.GetGameObject().SetActive(!IsWatchOrUIInputMode());
	}

	/// <summary>
	/// 填充面板
	/// </summary>
	private void FillPanel()
	{
		int count;
		//Debug.LogError("刷新hud ");
		if (m_TeamProxy.GetMembersList()!=null)
		{
			List<TeamMemberVO> datas = m_TeamProxy.GetMembersList();
			m_TeamProxy.SortMembers();
			count = datas.Count;
			for (int i = 0; i < count; i++)
			{
				m_TeamContents[i].gameObject.SetActive(true);
				m_TypeIsAlive[i].SetActive(false);
				m_TypeOnLine[i].SetActive(false);
				m_TeamNameLabel[i].text = datas[i].Name;
				if (datas[i].MaxHP == 0)
				{
					datas[i].MaxHP = 1;
				}
				if (datas[i].MaxDefense == 0)
				{
					datas[i].MaxDefense = 1;
				}
				//if (datas[i].HP<=0)
				//{
				//	datas[i].IsDead = true;
				//}
				Debug.Log("HP------" + datas[i].HP + datas[i].Name);
				m_TeamHPSlider[i].value=datas[i].HP * 1.0f / datas[i].MaxHP;
				m_TeamDefenseSlider[i].value = datas[i].Defense * 1.0f / datas[i].MaxDefense;
				m_TeamCanvasGroup[i].alpha = 1f;
				if (datas[i].IsDead)
				{
					//Debug.Log("dead------" + datas[i].Name);
					m_TeamCanvasGroup[i].alpha = m_Alpha;
					m_TeamHPSlider[i].value = 1;
					m_TeamDefenseSlider[i].value = 1;
					m_TypeIsAlive[i].SetActive(true);
					m_TypeOnLine[i].SetActive(false);
				}
				else
				{
					//Debug.LogError(datas[i].HP+"huozhe------" + datas[i].Name);
				}
				if (!m_FriendProxy.NearbyList(datas[i].UID) && datas[i].UID != m_ServerListProxy.GetCurrentCharacterVO().UId)
				{
					//Debug.Log("yuanli------" + datas[i].Name);
					m_TeamCanvasGroup[i].alpha = m_Alpha;
					m_TeamHPSlider[i].value = 1;
					m_TeamDefenseSlider[i].value = 1;
					m_TypeIsAlive[i].SetActive(false);
					m_TypeOnLine[i].SetActive(false);
				}
				if (!datas[i].IsOnline) //todo 离线
				{
					//Debug.Log("xiaxian------" + datas[i].Name);
					m_TeamCanvasGroup[i].alpha = m_Alpha;
					m_TypeIsAlive[i].SetActive(false);
					m_TypeOnLine[i].SetActive(true);
				}
				if (datas[i].UID == m_ServerListProxy.GetCurrentCharacterVO().UId)
				{
					CfgEternityProxy cfgEternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
					if (!cfgEternityProxy.IsSpace())
					{
						datas[i].IsDead = false;
						m_TeamCanvasGroup[i].alpha = 1f;
						m_TeamHPSlider[i].value = 1;
						m_TeamDefenseSlider[i].value = 1;
						m_TypeIsAlive[i].SetActive(false);
						m_TypeOnLine[i].SetActive(false);
					}
				}
				else
				{
					CfgEternityProxy cfgEternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
					if (!cfgEternityProxy.IsSpace())
					{
						m_FriendProxy.GetNearbyList();
						if (m_FriendProxy.GetNearby(datas[i].UID)!=null)
						{
							datas[i].IsDead = false;
							m_TeamCanvasGroup[i].alpha = 1f;
							m_TeamHPSlider[i].value = 1;
							m_TeamDefenseSlider[i].value = 1;
							m_TypeIsAlive[i].SetActive(false);
							m_TypeOnLine[i].SetActive(false);
						}
					}
				}
				
			}
			
			for (int i = count; i < m_TeamProxy.MEMBERCOUNTLIMIT; i++)
			{
				m_TeamContents[i].gameObject.SetActive(false);
			}
		}
		else
		{
			for (int i = 0; i < m_TeamProxy.MEMBERCOUNTLIMIT; i++)
			{
				m_TeamContents[i].gameObject.SetActive(false);
			}
		}
	}
}
