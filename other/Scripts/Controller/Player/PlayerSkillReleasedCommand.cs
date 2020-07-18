using PureMVC.Interfaces;
using PureMVC.Patterns.Command;

/// <summary>
/// 服务器确认可以放技能
/// </summary>
public class PlayerSkillReleasedCommand : SimpleCommand
{
	public override void Execute(INotification notification)
	{
// 		MsgPlayerSkillCooldown msg = notification.Body as MsgPlayerSkillCooldown;
// 
// 		PlayerSkillProxy skillProxy = GameFacade.Instance.RetrieveProxy(ProxyName.PlayerSkillProxy) as PlayerSkillProxy;
// 
// 		PlayerSkillVO skill = skillProxy.GetSkill(msg.SkillID);
// 
// 		//重置CD
// 		skill.LastReleaseTime = 0;
// 		skillProxy.BeginCooldown(skill);
// 
// 		//重置公共CD
// 		foreach (PlayerSkillVO curr in skillProxy.GetSkills())
// 		{
// 			if (curr.ID != msg.SkillID && curr.SkillSystemGrow.WeaponType == skill.SkillSystemGrow.WeaponType)
// 			{
// 				skillProxy.BeginPubCooldown(curr, skill.SkillSystemGrow.TriggerCD);
// 			}
// 		}
	}
}