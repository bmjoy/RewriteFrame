using System;

public interface ISpacecraftReliveProperty
{
    bool IsMain();
	float GetUnderAttackWarningToneCountdown();
	void SetUnderAttackWarningToneCountdown(float countdown);
}

public sealed class SpacecraftReliveComponent : EntityComponent<ISpacecraftReliveProperty>
{
    private ISpacecraftReliveProperty m_SpacecraftReliveProperty;

    public override void OnInitialize(ISpacecraftReliveProperty property)
    {
        m_SpacecraftReliveProperty = property;
    }

    public override void OnAddListener()
    {
        AddListener(ComponentEventName.Relive, OnRelive);
    }

    private void OnRelive(IComponentEvent obj)
    {
        SendEvent(ComponentEventName.ShowReliveFx, null);
        if (m_SpacecraftReliveProperty.IsMain())
        {
            GameFacade.Instance.SendNotification(NotificationName.MainHeroRevive);
			m_SpacecraftReliveProperty.SetUnderAttackWarningToneCountdown(0);
		}
    }
}