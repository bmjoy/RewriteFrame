using PureMVC.Interfaces;
using PureMVC.Patterns.Command;

class PlayerDanExpChangeCommand : SimpleCommand
{
	public override void Execute(INotification notification)
	{
		MsgPlayerDanExpChanged message = (MsgPlayerDanExpChanged)notification.Body;

		ChatProxy chatProxy = Facade.RetrieveProxy(ProxyName.ChatProxy) as ChatProxy;

		chatProxy.AddMessage(ChatChannel.CombatLog, "Add dan exp " + message.m_Exp);
	}
}