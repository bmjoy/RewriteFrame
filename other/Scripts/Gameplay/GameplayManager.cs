using UnityEngine.SceneManagement;

public class GameplayManager : Singleton<GameplayManager>
{
    private ComponentsFactory m_ComponentsFactory;
    private EntityManager m_EntityManager;
    private ProtocolLogic m_ProtocolLogic;
    private bool m_PvdSwitch = false;

    public void Initialize()
    {
        m_ProtocolLogic = new ProtocolLogic();
        m_ComponentsFactory = new ComponentsFactory();
        m_EntityManager = new EntityManager(m_ComponentsFactory);

        GameplayProxy gameplayProxy = GameFacade.Instance.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;
        gameplayProxy.Initialize(m_EntityManager);

        m_ProtocolLogic.OnInitialize(m_EntityManager);
    }

    public void SwitchMap(Scene scene)
    {
        GameplayProxy gameplayProxy = GameFacade.Instance.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;
        gameplayProxy.Clear();
		gameplayProxy.GameMainScene = scene;
        Clear();

        m_EntityManager.SwitchMap(scene);
        m_ProtocolLogic.SendGsAppleScene();
    }

    public void Clear()
    {
        if(m_EntityManager != null)
        {
            m_EntityManager.RemoveAllEntity();
        }
    }

    public void SwitchPvd()
    {
        m_PvdSwitch = !m_PvdSwitch;
        m_ProtocolLogic.SendOpenPvd(m_PvdSwitch);
    }

    public EntityManager GetEntityManager()
    {
        return m_EntityManager;
    }
}