using Assets.Scripts.Define;
using PureMVC.Interfaces;
using PureMVC.Patterns.Command;

/// <summary>
/// 玩家经验发生改变
/// </summary>
public class PlayerExpChangeCommand : SimpleCommand
{
	public override void Execute(INotification notification)
	{
		MsgPlayerExpChanged message = (MsgPlayerExpChanged)notification.Body;

		CfgLanguageProxy languageProxy = Facade.RetrieveProxy(ProxyName.CfgLanguageProxy) as CfgLanguageProxy;
		ChatProxy chatProxy = Facade.RetrieveProxy(ProxyName.ChatProxy) as ChatProxy;

		chatProxy.AddMessage(ChatChannel.CombatLog, string.Format(languageProxy.GetLocalization(100051), message.m_Exp));

		if (message.m_Exp > 0)
		{
            //UNDONE 获得道具提示  chw
            /*
			MsgItemGetting msg = MessageSingleton.Get<MsgItemGetting>();
			msg.m_Name = MissionRewardIconName.GetItemName((int)PackageType.eSpecialPlayerExp, -1);
			msg.m_Count = message.m_Exp;
			msg.m_IconBundle = MissionRewardIconName.GetIconBundle((int)PackageType.eSpecialPlayerExp, -1);
			msg.m_IconName = MissionRewardIconName.GetName((int)PackageType.eSpecialPlayerExp, -1);

			SendNotification(NotificationName.ItemGetting, msg);
            */
		}
	}
}