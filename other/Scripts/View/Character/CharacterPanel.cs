using PureMVC.Interfaces;
using TMPro;
using UnityEngine;
using Assets.Scripts.Define;
using static ServerListProxy;
/// <summary>
/// 创角选角基础窗口
/// </summary>
public class CharacterPanel : UIPanelBase
{
	private const string ASSET_ADDRESS = "Assets/Artwork/UI/Prefabs/CharacterPanel.prefab";

	/// <summary>
	/// 子窗口父节点
	/// </summary>
	private Transform m_PanelRoot;

	/// <summary>
	/// 创建角色Proxy
	/// </summary>
	private ServerListProxy m_ServerListProxy;

	/// <summary>
	/// CfgPlayerProxy
	/// </summary>
	private CfgEternityProxy m_CfgEternityProxy;

	/// <summary>
	/// 角色标题
	/// </summary>
	private TMP_Text m_Title;

	public CharacterPanel() : base(UIPanel.CharacterPanel, ASSET_ADDRESS, PanelType.Normal)
	{

	}

	public override void Initialize()
	{
		m_ServerListProxy = (ServerListProxy)Facade.RetrieveProxy(ProxyName.ServerListProxy);
		m_CfgEternityProxy = (CfgEternityProxy)Facade.RetrieveProxy(ProxyName.CfgEternityProxy);
		m_PanelRoot = FindComponent<Transform>("Content");
		m_Title = FindComponent<TMP_Text>("Content/Title/Icon/Label");
		m_CfgEternityProxy.CheckData();
		CanReceiveFocus = false;
	}

	public override void OnShow(object msg)
	{
		base.OnShow(msg);

		//背景音乐切换
		WwiseUtil.PlaySound((int)WwiseMusic.Music_BGM_LoginGame, false, null);
    }

    public override void OnHide(object msg)
    {
        UIManager.Instance.ClosePanel(UIPanel.CharacterModelPanel);
        UIManager.Instance.ClosePanel(UIPanel.CharacterRolePanel);
        UIManager.Instance.ClosePanel(UIPanel.CreateRolePanel);

        base.OnHide(msg);
		WwiseUtil.PlaySound((int)SoundID.LoadEnd, false, null);
		//背景音乐切换
		WwiseUtil.PlaySound((int)WwiseMusic.Music_BGM_ToEnterGame, false, null);
    }

    public override NotificationName[] ListNotificationInterests()
	{
		return new NotificationName[]
		{
			NotificationName.MSG_CHARACTER_CREATE_STATE_CHANGE,
			NotificationName.MSG_CHILDPANEL_HOTKEY,
			NotificationName.MSG_SWITCH_SCENE_START,
			NotificationName.MSG_CHARACTER_CREATE_SUCCESS,
		};
	}

	public override void HandleNotification(INotification notification)
	{
		switch (notification.Name)
		{
			case NotificationName.MSG_CHARACTER_CREATE_STATE_CHANGE:
				ChangeState((notification.Body as MsgCharacterPanelState).State);
				break;
			case NotificationName.MSG_CHILDPANEL_HOTKEY:
				break;
			case NotificationName.MSG_CHARACTER_CREATE_SUCCESS:
				NetworkManager.Instance.GetLoginController().CharacterLogin((ulong)notification.Body);
				break;
			case NotificationName.MSG_SWITCH_SCENE_START:
                break;
		}
	}
	public override void OnRefresh(object msg)
	{
		UIManager.Instance.OpenPanel(UIPanel.CharacterModelPanel, m_PanelRoot);
		ServerInfoVO serverInfo = m_ServerListProxy.GetLastLoginServer();

		if (serverInfo.CharacterList?.Count > 0)
		{
			m_ServerListProxy.SetCurrentState(CharacterPanelState.RoleList);
		}
		else
		{
			m_ServerListProxy.SetCurrentState(CharacterPanelState.CreatRole);
		}
	}

	/// <summary>
	/// 创角选角状态切换
	/// </summary>
	/// <param name="state">操作状态</param>
	private void ChangeState(CharacterPanelState state)
	{
		switch (state)
		{
			case CharacterPanelState.RoleList:
				UIManager.Instance.OpenPanel(UIPanel.CharacterRolePanel, m_PanelRoot);
				m_Title.text = TableUtil.GetLanguageString("character_title_1001");
				break;
			case CharacterPanelState.CreatRole:
				UIManager.Instance.OpenPanel(UIPanel.CreateRolePanel, m_PanelRoot);
				m_Title.text = TableUtil.GetLanguageString("character_title_1002");
				break;
		
		}
	}
	
}