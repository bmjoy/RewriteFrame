/*===============================
 * Author: [Allen]
 * Purpose: 音效加载组合
 * Time: 2019/6/21 10:34:28
================================*/
using PureMVC.Interfaces;
using PureMVC.Patterns.Command;

public class Sound_LoadComboCommand : SimpleCommand
{
    public override void Execute(INotification notification)
    {
        MsgLoadSoundCombo parame = notification.Body as MsgLoadSoundCombo;
        if(parame != null)
        {
            WwiseManager.LoadMusicCombo(parame.SoundComboId, parame.todoPrepareEvent);
        }
    }
}

