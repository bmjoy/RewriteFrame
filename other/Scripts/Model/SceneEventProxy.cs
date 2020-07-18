using Assets.Scripts.Define;
using PureMVC.Patterns.Proxy;
using System.Collections.Generic;
using UnityEngine;

public class SceneEventProxy : Proxy
{
	/// <summary>
	/// 当前场景事件
	/// </summary>
	public SceneEventInfoVO CurrentSceneEvent = new SceneEventInfoVO();

	public SceneEventProxy() : base(ProxyName.SceneEventProxy) { }

	/// <summary>
	/// 设置场景事件
	/// </summary>
	/// <param name="tid">场景事件ID</param>
	/// <param name="status">场景事件状态</param>
	/// <param name="items">场景事件列表</param>
	public void SetSceneEvent(uint tid, WorldEventState status, List<SceneEventItemClientVO> items)
	{
		//CurrentSceneEvent.SceneEventTID = tid;
		//CurrentSceneEvent.SceneEventStatus = status;
		//CurrentSceneEvent.SceneEventList = items;

		//SendNotification(NotificationName.MSG_SCENE_EVENT_CHANGED);

		//PlayerProxy playerProxy = Facade.RetrieveProxy(ProxyName.PlayerProxy) as PlayerProxy;
		//CfgSceneEventProxy sceneEventDataProxy = Facade.RetrieveProxy(ProxyName.CfgSceneEventProxy) as CfgSceneEventProxy;

		//string sceneEventTimeline = string.Format("SceneEvent_Timeline_{0}", CurrentSceneEvent.SceneEventTID);

		//// 只有第一次完成场景事件才播放timeline, 以后就不再播放. 使用配置文件记录这个事 
		//if (status == WorldEventState.WorldEventTriggerTime && playerProxy.GetUserdata_Int(sceneEventTimeline) != 1)
		//{
		//	SceneventVO? sceneventVO = sceneEventDataProxy.GetSceneventVOByKey(CurrentSceneEvent.SceneEventTID);
		//	if (sceneventVO != null)
		//	{
  //              //undone gaoyu ServerTimeProxy
  //              //TimelineManager.Instance.Play(sceneventVO.Value.FinishTimeline);
  //              playerProxy.SetUserdata_Int(sceneEventTimeline, 1);
		//	}
		//}
	}

	/// <summary>
	/// 设置场景事件项状态
	/// </summary>
	/// <param name="sceneEventTID">场景事件ID</param>
	/// <param name="sceneEventItemTID">世界任务ID</param>
	/// <param name="progress">进度</param>
	public void SetSceneEventItemStatus(uint sceneEventTID, uint sceneEventItemTID, uint progress)
	{
		if (CurrentSceneEvent.SceneEventTID != sceneEventTID)
		{
			Debug.LogErrorFormat("场景事件的TID与服务器发的不一致. 本地TID: {0}, 服务器发的TID: {1}", CurrentSceneEvent.SceneEventTID, sceneEventTID);
			return;
		}

		bool found = false;
		for (int i = 0; i < CurrentSceneEvent.SceneEventList.Count; i++)
		{
			if (CurrentSceneEvent.SceneEventList[i].WorldMissionTID == sceneEventItemTID)
			{
				CurrentSceneEvent.SceneEventList[i].Progress = progress;
				found = true;
				break;
			}
		}

		if (!found)
		{
			SceneEventItemClientVO sceneEventItem_client = new SceneEventItemClientVO();
			sceneEventItem_client.WorldMissionTID = sceneEventItemTID;
			sceneEventItem_client.Progress = progress;
			CurrentSceneEvent.SceneEventList.Add(sceneEventItem_client);
		
		}

		SendNotification(NotificationName.MSG_SCENE_EVENT_CHANGED);
		//EventDispatcher.Global.DispatchEvent(Notifications.MSG_SCENE_EVENT_ITEM_CHANGED, sceneEventItemTID, progress);
	}
}
