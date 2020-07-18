using PureMVC.Interfaces;
using PureMVC.Patterns.Command;


public class PlayerWeaponPowerChangeCommand : SimpleCommand
{
	public override void Execute(INotification notification)
	{
		MsgPlayerWeaponPowerChanged msg = notification as MsgPlayerWeaponPowerChanged;

		if (msg.OldValue > 0 && msg.NewValue == 0)
		{
            SendNotification(NotificationName.MSG_SKILL_RESET_CARTRIDGE_BOX);
		}
	}
}
