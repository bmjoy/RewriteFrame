using PureMVC.Interfaces;
using PureMVC.Patterns.Command;

/// <summary>
/// 玩家等级发生改变
/// </summary>
public class PlayerLevelChangeCommand : SimpleCommand
{

	public override void Execute(INotification notification)
	{
		MsgPlayerLevelChanged message = notification.Body as MsgPlayerLevelChanged;

		CfgLanguageProxy languageProxy = Facade.RetrieveProxy(ProxyName.CfgLanguageProxy) as CfgLanguageProxy;
		ChatProxy chatProxy = Facade.RetrieveProxy(ProxyName.ChatProxy) as ChatProxy;

		if (!message.m_IsSelf)
		{
			for (uint i = message.m_OldLevel + 1; i <= message.m_NewLevel; i++)
			{
				chatProxy.AddMessage(chatProxy.CurrentChannel, string.Format(languageProxy.GetLocalization(100055), message.m_PlayerName, i));
			}
		}
		else
		{
			for (uint i = message.m_OldLevel + 1; i <= message.m_NewLevel; i++)
			{
				chatProxy.AddMessage(chatProxy.CurrentChannel, string.Format(languageProxy.GetLocalization(100053), i));

				//UNDONE 调升级UI chw
				//EventDispatcher.Global.DispatchEvent(Notifications.MSG_CONFIRM_OPEN, NotifactionTypeEnum.NotifactionTypeEnum_UpgradePanel, true, true, i);
			}
		}
	}
}