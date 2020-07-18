using UnityEngine;

public interface ISpacecraftRefreshAxisProperty
{
    void SetRotateAxis(Vector3 vector);
    void SetEngineAxis(Vector3 vector);
    Rigidbody GetRigidbody();
    Transform GetRootTransform();
}

public sealed class SpacecraftRefreshAxisComponent : EntityComponent<ISpacecraftRefreshAxisProperty>
{
    private ISpacecraftRefreshAxisProperty m_SpacecraftRefreshAxisProperty;
    private Vector3 lasterAngularVelocity = new Vector3();

    public override void OnInitialize(ISpacecraftRefreshAxisProperty property)
    {
        m_SpacecraftRefreshAxisProperty = property;
    }

    public override void OnFixedUpdate()
    {
        base.OnFixedUpdate();

        RefreshRotateAxis();
        RefreshEngineAxis();
    }

    private void RefreshRotateAxis()
    {
        Vector3 rotateAxis = new Vector3();
        Rigidbody rigidbody = m_SpacecraftRefreshAxisProperty.GetRigidbody();
        if (rigidbody != null)
        {
            Vector3 newAngularVelocity = rigidbody.angularVelocity;
            if (Mathf.Abs(newAngularVelocity.y) < Mathf.Abs(lasterAngularVelocity.y))
            {
                rotateAxis = Vector3.zero;
            }
            else
            {
                rotateAxis = newAngularVelocity;
            }

            lasterAngularVelocity = newAngularVelocity;

            m_SpacecraftRefreshAxisProperty.SetRotateAxis(rotateAxis);
        }
    }

    private void RefreshEngineAxis()
    {
        Rigidbody rigidbody = m_SpacecraftRefreshAxisProperty.GetRigidbody();
        if (rigidbody != null)
        {
            Vector3 engineAxis = m_SpacecraftRefreshAxisProperty.GetRootTransform().InverseTransformDirection(rigidbody.velocity);
            m_SpacecraftRefreshAxisProperty.SetEngineAxis(engineAxis);
        }
    }
}
