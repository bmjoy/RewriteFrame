using Assets.Scripts.Define;
using UnityEngine;

public interface ISetOffsetComponentProperty
{
    Rigidbody GetRigidbody();
    Transform GetRootTransform();
    KHeroType GetHeroType();
}

public class SetOffsetComponent : EntityComponent<ISetOffsetComponentProperty>
{
    private ISetOffsetComponentProperty m_SetOffsetComponentProperty;

    public override void OnInitialize(ISetOffsetComponentProperty property)
    {
        m_SetOffsetComponentProperty = property;
    }

    public override void OnAddListener()
    {
        AddListener(ComponentEventName.SetOffset, OnSetOffset);
    }

    private void OnSetOffset(IComponentEvent componentEvent)
    {
        SetOffsetEvent setOffsetEvent = componentEvent as SetOffsetEvent;

        Transform transform = m_SetOffsetComponentProperty.GetRootTransform();
        transform.localPosition += setOffsetEvent.Offset;

        Rigidbody rigidbody = m_SetOffsetComponentProperty.GetRigidbody();
        if (rigidbody != null)
        {
            rigidbody.position += setOffsetEvent.Offset;
        }
    }
}