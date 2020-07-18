/*===============================
 * Author: [Allen]
 * Purpose: 卸载 加载音效组合
 * Time: 2019/6/21 11:35:59
================================*/
using PureMVC.Interfaces;
using PureMVC.Patterns.Command;

public class Sound_UnLoadComboCommand : SimpleCommand
{
    public override void Execute(INotification notification)
    {
        WwiseManager.UnLoadMusicCombo((int)notification.Body);
    }
}


