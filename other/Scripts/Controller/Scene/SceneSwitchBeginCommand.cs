using PureMVC.Interfaces;
using PureMVC.Patterns.Command;

/// <summary>
/// 场景切换开始时执行
/// </summary>
class SceneSwitchBeginCommand : SimpleCommand
{
    public override void Execute(INotification notification)
    {
        UIManager.Instance.CloseAllHud();
        GameFacade.Instance.SendNotification(NotificationName.MSG_DIALOGUE_HIDE);
    }
}
