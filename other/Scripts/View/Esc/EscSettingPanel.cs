
public class EscSettingPanel : CompositeView
{
    public EscSettingPanel() : base(UIPanel.EscSettingPanel,PanelType.Normal)
    {

    }

    public override void OnShow(object msg)
    {
        base.OnShow(msg);
        State.GetAction(UIAction.Common_Back).Callback -= OnEscCallback;
    }
}
