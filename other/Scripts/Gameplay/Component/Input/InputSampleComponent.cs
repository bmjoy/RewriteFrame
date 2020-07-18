using Utils.Timer;

public interface IInputSampleProperty
{

}

/// <summary>
/// 输入采样时间控制逻辑
/// </summary>
public class InputSampleComponent : EntityComponent<IInputSampleProperty>
{
    private uint m_TimerId = 0;

    public override void OnInitialize(IInputSampleProperty property)
    {
        m_TimerId = RealTimerUtil.Instance.Register(GamePlayGlobalDefine.PlayInputSampleInterval, RealTimerUtil.EVERLASTING, OnInputSample);
    }

    public override void OnDestroy()
    {
        RealTimerUtil.Instance.Unregister(m_TimerId);
    }

    private void OnInputSample(int times)
    {
        SendEvent(ComponentEventName.InputSample, null);
    }
}

