using PureMVC.Interfaces;
using PureMVC.Patterns.Command;

public class InitializeControllerCommand : SimpleCommand
{

    public override void Execute(INotification notification)
    {
        Facade.RegisterCommand(NotificationName.PlayerExpChanged, () => new PlayerExpChangeCommand());
        Facade.RegisterCommand(NotificationName.PlayerLevelChanged, () => new PlayerLevelChangeCommand());
        Facade.RegisterCommand(NotificationName.PlayerShipExpChanged, () => new PlayerShipExpChangeCommand());
        Facade.RegisterCommand(NotificationName.PlayerShipLevelChanged, () => new PlayerShipLevelChangeCommand());
        Facade.RegisterCommand(NotificationName.PlayerDanExpChanged, () => new PlayerDanExpChangeCommand());
        Facade.RegisterCommand(NotificationName.TeamErrorInfo, () => new TeamErrorInfoCommand());

        Facade.RegisterCommand(NotificationName.PlayerWeaponPowerChanged, () => new PlayerWeaponPowerChangeCommand());
        Facade.RegisterCommand(NotificationName.PlayerWeaponToggleBegin, () => new PlayerWeaponToggleBeginCommand());

        Facade.RegisterCommand(NotificationName.PlayerSkillRequest, () => new PlayerSkillRequestCommand());
        Facade.RegisterCommand(NotificationName.PlayerSkillReleased, () => new PlayerSkillReleasedCommand());
        
        Facade.RegisterCommand(NotificationName.MSG_SWITCH_SCENE_START, () => new SceneSwitchBeginCommand());
        Facade.RegisterCommand(NotificationName.MSG_SWITCH_SCENE_END, () => new SceneSwitchEndCommand());
        
		Facade.RegisterCommand(NotificationName.MSG_QUIT, () => new ExitGameCommand());

        Facade.RegisterCommand(NotificationName.MSG_SOUND_SET_LISTENER_TARGET, () => new Sound_SetListenerTargetCommand());
        Facade.RegisterCommand(NotificationName.MSG_SOUND_LOAD_COMBO, () => new Sound_LoadComboCommand());
        Facade.RegisterCommand(NotificationName.MSG_SOUND_UNLOAD_COMBO, () => new Sound_UnLoadComboCommand());
        Facade.RegisterCommand(NotificationName.MSG_SOUND_PLAY, () => new Sound_PalyCommand());
    }
}