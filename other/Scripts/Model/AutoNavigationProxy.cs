using Map;
using PureMVC.Patterns.Proxy;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TaskTrackingProxy;
using Leap = TaskTrackingProxy.Leap;

public class AutoNavigationProxy : Proxy
{
	public AutoNavigationProxy() : base(ProxyName.AutoNavigationProxy) { }

	private TaskTrackingProxy m_TaskTrackingProxy;

	private LeapPath m_LeapPath;
	private Stack<Leap> m_LeapList;

	private TaskTrackingProxy GetTaskTrackingProxy()
	{
		if (m_TaskTrackingProxy == null)
		{
			m_TaskTrackingProxy = GameFacade.Instance.RetrieveProxy(ProxyName.TaskTrackingProxy) as TaskTrackingProxy;
		}
		return m_TaskTrackingProxy;
	}

	public void GoTo(uint fromSceneID, ulong fromArea, uint toSceneID, ulong toArea)
	{
		if (fromArea == Constants.NOTSET_AREA_UID)
		{
            GameplayProxy gameplayProxy = GameFacade.Instance.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;
            if (gameplayProxy.IsHasLeapBreak())
            {
                return;
            }
            NetworkManager.Instance.GetSceneController().RequestStartLeap(fromArea, toArea);
			return;
		}
		m_LeapPath = null;
		m_LeapList = new Stack<Leap>();

		if (fromSceneID == toSceneID)
		{
			m_LeapPath = GetTaskTrackingProxy().FindLeapPath(toSceneID, fromArea, toArea);
		}

		if (m_LeapPath != null)
		{
			while (m_LeapPath != null)
			{
				m_LeapList.Push(m_LeapPath.leap);
				m_LeapPath = m_LeapPath.prev;
			}
			m_LeapPath = null;
			if (m_LeapList.Count > 0)
			{
				SendLeap();
			}
		}
	}

	private void SendLeap()
	{
        if (m_LeapList != null && m_LeapList.Count > 0)
        {
            Leap data = m_LeapList.Pop();
            NetworkManager.Instance.GetSceneController().RequestStartLeap(data.fromLeap, data.toLeap);
        }
	}

	public void CheckNextAutoLeap()
	{
		UIManager.Instance.StartCoroutine(DelayLeap());
	}
	IEnumerator DelayLeap()
	{
		yield return new WaitForSeconds(0.5f);
		SendLeap();
	}

	public void StopAutoLeap()
	{
		if (m_LeapList != null)
		{
			m_LeapList.Clear();
			m_LeapList = null;
		}
	}

}