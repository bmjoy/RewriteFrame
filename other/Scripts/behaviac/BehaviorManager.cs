using behaviac;
using System.Collections.Generic;
using UnityEngine;

public class BehaviorTaskData
{
    public string Name;
    public EnumEventType EventType;
}

public class BehaviorManager : Singleton<BehaviorManager>
{
    private readonly Dictionary<int, BehaviorTaskData> m_TaskTable = new Dictionary<int, BehaviorTaskData>();
    private readonly Dictionary<int, behaviac.Agent> m_AgentTable = new Dictionary<int, behaviac.Agent>();
    private readonly Dictionary<int, string> m_SwitchTreeCommandTable = new Dictionary<int, string>();

#if BEHAVIAC_TEST
	private GameplayProxy m_GameplayProxy;
	private SpacecraftEntity m_Entity;
	private SpacecraftBehaviorComponent m_BehaviorComponent;
#endif

	#region UnityEngine API
	private void OnDestroy()
    {
        Destory();
    }

    private void Update()
    {
        DoUpdate();
    }

#if BEHAVIAC_TEST
	private void OnGUI()
	{
		if (GUI.Button(new Rect(0, 0, 100, 50), "获取玩家"))
		{
			m_GameplayProxy = GameFacade.Instance.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;
			m_Entity = m_GameplayProxy.GetMainPlayer();
			m_BehaviorComponent = m_Entity.GetEntityComponent<SpacecraftBehaviorComponent>();
		}

		if (GUI.Button(new Rect(0, 100, 100, 50), "战斗"))
		{
			if (m_BehaviorComponent == null)
			{
				m_GameplayProxy = GameFacade.Instance.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;
				m_Entity = m_GameplayProxy.GetMainPlayer();
				m_BehaviorComponent = m_Entity.GetEntityComponent<SpacecraftBehaviorComponent>();
			}
			else
			{
				m_BehaviorComponent.ChangeMainStateTest(EnumMainState.Fight);
			}
		}

		if (GUI.Button(new Rect(100, 0, 100, 50), "巡航"))
		{
			if (m_BehaviorComponent == null)
			{
				m_GameplayProxy = GameFacade.Instance.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;
				m_Entity = m_GameplayProxy.GetMainPlayer();
				m_BehaviorComponent = m_Entity.GetEntityComponent<SpacecraftBehaviorComponent>();
			}
			else
			{
				m_BehaviorComponent.ChangeMainStateTest(EnumMainState.Cruise);
			}
		}

		if (GUI.Button(new Rect(0, 50, 100, 50), "死亡"))
		{
			if (m_BehaviorComponent == null)
			{
				m_GameplayProxy = GameFacade.Instance.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;
				m_Entity = m_GameplayProxy.GetMainPlayer();
				m_BehaviorComponent = m_Entity.GetEntityComponent<SpacecraftBehaviorComponent>();
			}
			else
			{
				m_BehaviorComponent.ChangeMainStateTest(EnumMainState.Dead);
			}
		}
	}
#endif

	#endregion

	/// <summary>
	/// 初始化
	/// </summary>
	public void Initialize()
    {
#if UNITY_EDITOR && BEHAVIAC_LOG
        behaviac.Config.IsLogging = true;
        behaviac.Config.IsSocketing = false;
        //behaviac.Config.IsLoggingFlush = true;
#endif
        behaviac.Workspace.Instance.FilePath = FilePath;
        behaviac.Workspace.Instance.FileFormat = behaviac.Workspace.EFileFormat.EFF_cs;
        behaviac.Workspace.Instance.IsExecAgents = true;
	}

    private void Destory()
    {
        behaviac.Workspace.Instance.Cleanup();
    }

    /// <summary>
    /// 切换行为树
    /// </summary>
    /// <param name="agent"></param>
    /// <param name="relativePath"></param>
    /// <returns></returns>
    public bool SwitchTree(behaviac.Agent agent, string relativePath)
    {
        if (agent == null)
        {
            return false;
        }

        bool ret = agent.btload(relativePath);
        agent.btsetcurrent(relativePath);

        BehaviorManager.Instance.LogFormat(agent, string.Format($"switch to {relativePath}"));

        return ret;
    }

    /// <summary>
    /// 添加Agent
    /// </summary>
    /// <param name="agent"></param>
    public void AddAgent(behaviac.Agent agent)
    {
        if (agent == null)
        {
            return;
        }

        if (m_AgentTable.ContainsKey(agent.GetId()))
        {
            return;
        }

        m_AgentTable.Add(agent.GetId(), agent);
    }

    public void RemoveAgent(behaviac.Agent agent)
    {
        if (agent == null)
        {
            return;
        }

        if (m_AgentTable.ContainsKey(agent.GetId()))
        {
            m_AgentTable.Remove(agent.GetId());
        }
    }

    public void AddTask(behaviac.Agent agent, BehaviorTaskData behaviorTaskData)
    {
        if (agent == null)
        {
            return;
        }

        m_TaskTable.Add(agent.GetId(), behaviorTaskData);
    }

    public void AddSwitchCommandTable(behaviac.Agent agent, string relativePath)
    {
        if (agent == null || m_SwitchTreeCommandTable.ContainsKey(agent.GetId()))
        {
            return;
        }

        m_SwitchTreeCommandTable.Add(agent.GetId(), relativePath);

        LogFormat(agent, $"AddSwitchCommandTable relativePath:{relativePath}");
    }

    public void LogFormat(Agent pAgent, string format, params object[] args)
    {
        LogManager.Instance.Output(pAgent, "[Log] " + string.Format(format, args) + "\n");
    }

    public void LogErrorFormat(Agent pAgent, string format, params object[] args)
    {
        LogManager.Instance.Output(pAgent, "[Error] " + string.Format(format, args) + "\n");
    }

    private void DoUpdate()
    {
        behaviac.Workspace.Instance.DoubleValueSinceStartup = Time.realtimeSinceStartup * 1000f;

        behaviac.Workspace.Instance.Update();

        //DispatchTask();

        DispathcSwitchTreeCommand();
    }

    private void DispatchTask()
    {
        foreach (var item in m_TaskTable)
        {
            behaviac.Agent agent = null;
            if (m_AgentTable.TryGetValue(item.Key, out agent))
            {
                agent.FireEvent(item.Value.Name, item.Value.EventType);
            }
        }
        m_TaskTable.Clear();
    }

    private void DispathcSwitchTreeCommand()
    {
        foreach (var item in m_SwitchTreeCommandTable)
        {
            behaviac.Agent agent = null;
            if (m_AgentTable.TryGetValue(item.Key, out agent))
            {
                BehaviorManager.Instance.SwitchTree(agent, item.Value);
            }
        }
        m_SwitchTreeCommandTable.Clear();
    }

    private string FilePath
    {
        get
        {
            string relativePath = "/Scripts/behaviac/behaviac_generated/behaviors";

            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                return Application.dataPath + relativePath;
            }
            else if (Application.platform == RuntimePlatform.WindowsPlayer)
            {
                return Application.dataPath + relativePath;
            }
            else
            {
                return "Assets" + relativePath;
            }
        }
    }
}
