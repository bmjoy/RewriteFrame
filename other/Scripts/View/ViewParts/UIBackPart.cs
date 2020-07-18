public class UIBackPart : BaseViewPart
{
    public override void OnShow(object msg)
    {
        base.OnShow(msg);

        if (Config.HasValue)
            LoadViewPart(Config.Value.BgPic, OwnerView.BackBox);
    }
}
