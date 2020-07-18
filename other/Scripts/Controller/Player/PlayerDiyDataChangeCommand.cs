using PureMVC.Interfaces;
using PureMVC.Patterns.Command;

public class PlayerDiyDataChangeCommand : SimpleCommand
{
	public override void Execute(INotification notification)
	{
		(GameFacade.Instance.RetrieveProxy(ProxyName.PlayerSkillProxy) as PlayerSkillProxy).RebuildSkillList();
	}
}
