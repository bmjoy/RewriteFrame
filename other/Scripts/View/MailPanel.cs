using System.Collections;
using System.Collections.Generic;
using PureMVC.Interfaces;
using UnityEngine;

public class MailPanel : CompositeView
{
    public MailPanel() : base(UIPanel.MailPanel, PanelType.Normal)
    {

    }
    public override void Initialize()
    {
        base.Initialize();
        NetworkManager.Instance.GetMailController().C_to_S_GetEmailList();
        NetworkManager.Instance.GetLogController().C_to_S_GetLogList();
    }
    public override void OnShow(object msg)
    {
        base.OnShow(msg);
    }                      
      
    public override void OnHide(object msg)
    {
        base.OnHide(msg);
    }
}
