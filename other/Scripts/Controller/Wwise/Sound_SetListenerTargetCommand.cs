/*===============================
 * Author: [Allen]
 * Purpose: 设置Listener 的目标
 * Time: 2019/6/21 12:03:18
================================*/
using PureMVC.Interfaces;
using PureMVC.Patterns.Command;
using UnityEngine;

public class Sound_SetListenerTargetCommand : SimpleCommand
{
    public override void Execute(INotification notification)
    {
        Transform transform = (Transform)notification.Body;
        if(transform != null)
        {
            WwiseManager.Instance.SetAudioListenerTarget(transform);
        }
    }
}

