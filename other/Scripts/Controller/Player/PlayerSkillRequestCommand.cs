using PureMVC.Interfaces;
using PureMVC.Patterns.Command;
using UnityEngine;

/// <summary>
/// 向服务器请求放技能
/// </summary>
public class PlayerSkillRequestCommand : SimpleCommand
{
	public override void Execute(INotification notification)
	{
// 		MsgPlayerSkillRequest msg = notification.Body as MsgPlayerSkillRequest;
// 
// 		PlayerSkillProxy skillProxy = GameFacade.Instance.RetrieveProxy(ProxyName.PlayerSkillProxy) as PlayerSkillProxy;
// 
// 		skillProxy.GetSkill(msg.SkillID).LastReleaseTime = Time.time;
	}
}

 