public class EscRolePanel : CompositeView
{
    public EscRolePanel() : base(UIPanel.EscRolePanel, PanelType.Normal)
    {
    }

    

    public override void OnShow(object msg)
    {
        base.OnShow(msg);
        State.GetAction(UIAction.Common_Back).Callback -= OnEscCallback;
    }
}
