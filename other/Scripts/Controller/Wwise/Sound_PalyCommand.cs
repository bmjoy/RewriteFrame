/*===============================
 * Author: [Allen]
 * Purpose: Class1
 * Time: 2019/6/24 16:43:55
================================*/
using PureMVC.Interfaces;
using PureMVC.Patterns.Command;

public class Sound_PalyCommand : SimpleCommand
{
    public override void Execute(INotification notification)
    {
        //特殊枚举类型标记
        MsgPlaySpecialTypeMusicOrSound specialparame = notification.Body as MsgPlaySpecialTypeMusicOrSound;
        if (specialparame != null)
        {
            if (specialparame.UseSoundParent)
                WwiseManager.PlaySpecialTypeMusicOrSound(specialparame.ComboId, specialparame.type, specialparame.palce, specialparame.alreadyPrepare, specialparame.SoundParent, specialparame.endAction, specialparame.userEndData);
            else
                WwiseManager.PlaySpecialTypeMusicOrSound(specialparame.ComboId, specialparame.type, specialparame.palce, specialparame.alreadyPrepare, specialparame.point, specialparame.endAction, specialparame.userEndData);
        }

        //普通的
        MsgPlayMusicOrSound playparame = notification.Body as MsgPlayMusicOrSound;
        if (playparame != null)
        {
            if (playparame.UseSoundParent)
                WwiseManager.PlayMusicOrSound(playparame.musicId, playparame.alreadyPrepare, playparame.SoundParent, playparame.endAction, playparame.userEndData);
            else
                WwiseManager.PlayMusicOrSound(playparame.musicId, playparame.alreadyPrepare, playparame.point, playparame.endAction, playparame.userEndData);
        }

        //toggle 越界声音
        MsgPlayMusicOrSound_outLine outLineparame = notification.Body as MsgPlayMusicOrSound_outLine;
        if (outLineparame != null && outLineparame.OldSelectionObj != null && outLineparame.OldSelectionObj.activeInHierarchy)
        {
            UnityEngine.UI.ButtonWithSound buttonSoundSprite = outLineparame.OldSelectionObj.GetComponent<UnityEngine.UI.ButtonWithSound>();
            if(buttonSoundSprite != null)
            {
                buttonSoundSprite.PlayOutLineSound();
            }
        }
    }
}