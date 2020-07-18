using PureMVC.Interfaces;
using PureMVC.Patterns.Command;

public class PlayerPoseChangeCommand : SimpleCommand
{
	public override void Execute(INotification notification)
	{
		(GameFacade.Instance.RetrieveProxy(ProxyName.PlayerSkillProxy) as PlayerSkillProxy).RebuildSkillList();
	}
}
