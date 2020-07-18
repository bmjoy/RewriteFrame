using Assets.Scripts.Define;
using System.Collections.Generic;
using UnityEngine;

public class SceneEventInfoVO
{
	/// <summary>
	/// 场景事件ID
	/// </summary>
	public uint SceneEventTID;
	/// <summary>
	/// 场景事件状态
	/// </summary>
	public WorldEventState SceneEventStatus;
	/// <summary>
	/// 场景事件链表
	/// </summary>
	public List<SceneEventItemClientVO> SceneEventList = new List<SceneEventItemClientVO>();

	
}
