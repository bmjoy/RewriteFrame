using PureMVC.Interfaces;
using PureMVC.Patterns.Command;

public class TeamErrorInfoCommand : SimpleCommand
{
	public override void Execute(INotification notification)
	{
		MsgTeamErrorInfo message = (MsgTeamErrorInfo)notification.Body;
		ChatProxy chatProxy = Facade.RetrieveProxy(ProxyName.ChatProxy) as ChatProxy;

		chatProxy.AddMessage(chatProxy.CurrentChannel, message.m_Error);

        SendNotification(NotificationName.MSG_TEAM_ERROR_INFO, message.m_Error);
	}
}