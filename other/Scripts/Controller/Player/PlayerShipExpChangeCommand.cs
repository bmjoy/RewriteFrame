using Assets.Scripts.Define;
using PureMVC.Interfaces;
using PureMVC.Patterns.Command;


public class PlayerShipExpChangeCommand : SimpleCommand
{
	public override void Execute(INotification notification)
	{
		MsgPlayerShipExpChanged message = (MsgPlayerShipExpChanged)notification.Body;

		CfgLanguageProxy languageProxy = Facade.RetrieveProxy(ProxyName.CfgLanguageProxy) as CfgLanguageProxy;
		ChatProxy chatProxy = Facade.RetrieveProxy(ProxyName.ChatProxy) as ChatProxy;

		chatProxy.AddMessage(ChatChannel.CombatLog, string.Format(languageProxy.GetLocalization(100050), message.m_Exp));

		if (message.m_Exp > 0)
        {
            //UNDONE 获得道具提示  chw
            /*
			MsgItemGetting msg = MessageSingleton.Get<MsgItemGetting>();
			msg.m_IconName = MissionRewardIconName.GetItemName((int)PackageType.eSpecialAirExp, -1);
			msg.m_IconBundle = MissionRewardIconName.GetIconBundle((int)PackageType.eSpecialAirExp, -1);
			msg.m_IconName = MissionRewardIconName.GetName((int)PackageType.eSpecialAirExp, -1);
			msg.m_Count = message.m_Exp;

			SendNotification(NotificationName.ItemGetting, msg);
            */
		}
	}
}
 