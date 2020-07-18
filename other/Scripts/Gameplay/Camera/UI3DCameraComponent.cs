using DebugPanel;

public class UI3DCameraComponent : BaseCameraComponent
{
    public override string GetCameraName()
    {
        return "UI3DCamera";
    }

    protected override void DoGUIOverride(Config config)
    {
    }

    protected override void DoLateUpdate(float deltaTime)
    {
        if (UIManager.Instance.Aspect > 0)
        {
            SetCameraAspectAndRect(UIManager.Instance.Aspect, UIManager.Instance.ViewportRect);
        }
    }
}