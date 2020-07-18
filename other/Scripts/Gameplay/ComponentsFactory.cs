
public class ComponentsFactory 
{
    public ComponentType CreateComponent<ComponentType, PropertyType>(PropertyType property, ComponentEventDispatcher entityEventDispatcher, ISendEventToEntity sendEventToEntity) where ComponentType : EntityComponent<PropertyType>, new()
    {
        ComponentType component = new ComponentType();
        component.SetEntityEventDispatcher(entityEventDispatcher);
        component.SetHashCode<ComponentType>();
        component.OnInitialize(property);
        component.OnAddListener();

        return component;
    }
}
