public class EscPermitPanel : CompositeView
{
    public EscPermitPanel() : base(UIPanel.EscPermitPanel,PanelType.Normal)
    {

    }

    public override void OnShow(object msg)
    {
        base.OnShow(msg);
        State.GetAction(UIAction.Common_Back).Callback -= OnEscCallback;
    }
}
