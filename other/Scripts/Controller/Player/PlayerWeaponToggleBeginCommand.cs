using PureMVC.Interfaces;
using PureMVC.Patterns.Command;

/// <summary>
/// 响应武器切换请求
/// </summary>
public	class PlayerWeaponToggleBeginCommand : SimpleCommand
{
	public override void Execute(INotification notification)
	{
		PlayerSkillProxy skillProxy = GameFacade.Instance.RetrieveProxy(ProxyName.PlayerSkillProxy) as PlayerSkillProxy;

		skillProxy.ToggleCurrentWeapon();
	}
}