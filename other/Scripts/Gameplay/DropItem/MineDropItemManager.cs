using Assets.Scripts.Proto;
using Leyoutech.Core.Loader;
using Eternity.FlatBuffer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Crucis.Protocol;
using Crucis.Protocol.GameSession;

public class MineDropItemInfo
{
	public uint mapid;
	public ulong areaid;
	public ulong uid;
	public ulong parentid;
	public uint tid;
	public uint ship_tlv;
	public uint item_tid;
	public float positon_x;
	public float positon_y;
	public float positon_z;
	///entity
	public GameObject obj;
}

public class MineDropItemManager : Singleton<MineDropItemManager>
{
	private CfgEternityProxy m_CfgEternityProxy;

	private GameplayProxy m_GameplayProxy;

	/// <summary>
	/// 掉落缓存
	/// </summary>
	private Dictionary<string, MineDropItemInfo> m_GatherDropItemInfos = new Dictionary<string, MineDropItemInfo>();

	public MineDropItemManager()
	{
		m_CfgEternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
		m_GameplayProxy = GameFacade.Instance.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;
	}

	/// <summary>
	/// 创建掉落物协议
	/// </summary>
	/// <param name="respond"></param>
	public void CreateDropItemByRespond(s2cSyncPersonalDrop respond)
	{
		if (respond.DropList.Count <= 0)
		{
			/// Debug.Log("CreateDropItemByRespond:the drop_list.Count <= 0");
			return;
		}

		if (respond.IsMapSync == false)
		{
			int index = Random.Range(0, 2);
			WwiseUtil.PlaySound(WwiseManager.voiceComboID, index == 0 ? WwiseMusicSpecialType.SpecialType_Voice_minera_event3 : WwiseMusicSpecialType.SpecialType_Voice_minera_event4, WwiseMusicPalce.Palce_1st, false, null);
		}

		ulong parentid = 0;
		foreach (var item in respond.DropList)
		{
			string key = item.MapId.ToString() + item.AreaUid.ToString() + item.ChestNpcUid.ToString();
			if (!m_GatherDropItemInfos.ContainsKey(key))
			{
				MineDropItemInfo mineDropItemInfo = new MineDropItemInfo();
				mineDropItemInfo.mapid = item.MapId;
				mineDropItemInfo.areaid = item.AreaUid;
				mineDropItemInfo.uid = item.ChestNpcUid;
				mineDropItemInfo.tid = item.ChestNpcTid;
				mineDropItemInfo.item_tid = item.DropItemTid;
				mineDropItemInfo.ship_tlv = item.ShipTlv;
				mineDropItemInfo.parentid = item.FromHeroUid;
				mineDropItemInfo.positon_x = item.PositonX;
				mineDropItemInfo.positon_y = item.PositonY;
				mineDropItemInfo.positon_z = item.PositonZ;
				if (parentid == 0)
				{
					parentid = mineDropItemInfo.parentid;
				}

				uint mapId = m_CfgEternityProxy.GetCurrentMapData().GamingmapId;
				ulong aredId = m_GameplayProxy.GetCurrentAreaUid();

				if (mapId == item.MapId && aredId == item.AreaUid)
				{
					CreateDropItem(key, mineDropItemInfo, respond.IsMapSync == false);
				}
			}
		}

		/// 新掉落播音效
		if (respond.IsDieDrop == false && respond.IsMapSync == false && parentid != 0)
		{
			SpacecraftEntity spacecraftEntity = m_GameplayProxy.GetEntityById<SpacecraftEntity>((uint)parentid);
			spacecraftEntity.SendEvent(ComponentEventName.PlayDropSound, null);
		}
	}

	/// <summary>
	/// 创建矿石时
	/// </summary>
	/// <param name="uid"></param>
	public void CheckSyncDropItem()
	{
		if (m_GatherDropItemInfos.Count <= 0)
		{
			return;
		}
		foreach (var item in m_GatherDropItemInfos.Values)
		{
			uint mapId = m_CfgEternityProxy.GetCurrentMapData().GamingmapId;
			ulong aredId = Map.MapManager.GetInstance().GetCurrentAreaUid();

			if (mapId == item.mapid && aredId == item.areaid)
			{
				string key = mapId.ToString() + aredId.ToString() + item.uid.ToString();
				CreateDropItem(key, item);
			}
		}
	}

	private void CreateDropItem(string key, MineDropItemInfo mineDropItemInfo, bool needFly = false)
	{
		NpcCombat? nc = m_CfgEternityProxy.GetNpcCombatByKey(mineDropItemInfo.tid);
		if (!nc.HasValue)
		{
			return;
		}

		PackageBoxAttr? pb = m_CfgEternityProxy.GetPackageBoxAttrByModelIdAndGrade((uint)nc.Value.BoxModel, mineDropItemInfo.ship_tlv);
		if (!pb.HasValue)
		{
			return;
		}

		int modelid = pb.Value.BoxModel;
		Model? mdata = m_CfgEternityProxy.GetModel(modelid);
		if (mdata.Value.AssetName.Equals(string.Empty))
		{
			return;
		}

		Vector3 curPos = Vector3.zero;
		SpacecraftEntity spe = m_GameplayProxy.GetEntityById<SpacecraftEntity>((uint)mineDropItemInfo.parentid) as SpacecraftEntity;
		if (spe)
		{
			curPos = spe.GetRootTransform().position;
		}

        AssetUtil.InstanceAssetAsync(mdata.Value.AssetName, (address, uObj, userData) =>
        {
            mineDropItemInfo.obj = uObj as GameObject;
            MineDropComponent mineDropComponent = mineDropItemInfo.obj.GetComponent<MineDropComponent>();
            if (!mineDropComponent)
            {
                mineDropComponent = mineDropItemInfo.obj.AddComponent<MineDropComponent>();
            }

            SpacecraftEntity mainPlayer = m_GameplayProxy.GetMainPlayer();
            mineDropComponent.Initialize(mainPlayer, mineDropItemInfo.uid, mineDropItemInfo.tid, mineDropItemInfo.item_tid, pb);
            if (needFly)
            {
                mineDropItemInfo.obj.transform.position = curPos;
                mineDropComponent.AddFlyEffect(mineDropItemInfo.positon_x, mineDropItemInfo.positon_y, mineDropItemInfo.positon_z);
            }
            else
            {
                Vector3 endPos = new Vector3(mineDropItemInfo.positon_x, mineDropItemInfo.positon_y, mineDropItemInfo.positon_z);
                Vector3 clientPosition = m_GameplayProxy.ServerAreaOffsetToClientPosition(endPos);
                mineDropItemInfo.obj.transform.position = clientPosition;
                mineDropComponent.SetDropItemState(DropItemState.Stay);
                mineDropComponent.AddEffect(true);
            }

            /// 跃迁客户端不清数据
            if (m_GatherDropItemInfos.ContainsKey(key))
            {
                m_GatherDropItemInfos.Remove(key);
            }
            m_GatherDropItemInfos.Add(key, mineDropItemInfo);
        });
	}

	/// <summary>
	/// 拾取
	/// </summary>
	/// <param name="uid"></param>
	public void AutoPickUp(ulong uid)
	{
		SendPickUpDropItem(uid);
	}

	private async void SendPickUpDropItem(ulong id)
	{
		//C2S_REQUEST_PERSONAL_DROP msg = new C2S_REQUEST_PERSONAL_DROP();
		//msg.protocolID = (ushort)KC2S_Protocol.c2s_request_personal_drop;
		//msg.id = id;
		//NetworkManager.Instance.SendToGameServer(msg);


		Debug.Log("------->>>>>发送拾取矿石信息" + id);
		c2s_RequestPersonalDrop msg = new c2s_RequestPersonalDrop();
		msg.Id = id;
		AskPersonalDropResponse askPersonalDropResponse = await AskPersonalDropRPC.AskPersonalDrop(msg);
		Debug.Log("------->>>>>收拾取矿石结果信息" + id);
		if (askPersonalDropResponse.Except != null)
		{
			Debug.LogError("------->>>>>拾取矿石失败");
		}
		else if (askPersonalDropResponse.Success != null)
		{
			s2c_NotifyPersonalDropResult result = askPersonalDropResponse.Success.Success_;
			Debug.Log("------->>>>>>收取拾取矿石成功_Result:" + result.Result);
			if (result.Result)
			{
				string key = result.MapId.ToString() + result.AreaUid.ToString() + result.NpcUid.ToString();
				NotifyDropResult(key);
			}
		}
	}

	public void NotifyDropResult(string key)
	{
		if (m_GatherDropItemInfos.ContainsKey(key))
		{
			GameObject obj = m_GatherDropItemInfos[key].obj;
			if (obj)
			{
				MineDropComponent mineDropComponent = obj.GetComponent<MineDropComponent>();
				if (mineDropComponent)
				{
					UIManager.Instance.StartCoroutine(PickUpEffect(key, mineDropComponent));
				}
			}
			else
			{
				DestoryDropInfo(key);
			}
		}
	}

	/// <summary>
	/// 拾取结果协议处理
	/// </summary>
	/// <param name="respond"></param>
	public void NotifyDropItemResult(S2C_NOTIFY_PERSONAL_DROP_RESULT respond)
	{
		//if (respond.result == 0)
		//{
		//	string key = respond.map_id.ToString() + respond.area_uid.ToString() + respond.npc_uid.ToString();
		//	if (m_GatherDropItemInfos.ContainsKey(key))
		//	{
		//		GameObject obj = m_GatherDropItemInfos[key].obj;
		//		if (obj)
		//		{
		//			MineDropComponent mineDropComponent = obj.GetComponent<MineDropComponent>();
		//			if (mineDropComponent)
		//			{
		//				UIManager.Instance.StartCoroutine(PickUpEffect(key, mineDropComponent));
		//			}
		//		}
		//		else
		//		{
		//			DestoryDropInfo(key);
		//		}
		//	}
		//}
	}

	IEnumerator PickUpEffect(string key, MineDropComponent mineDropComponent)
	{
		mineDropComponent.AddEffect();

		MineDropItemInfo gatherDropItemInfo = m_GatherDropItemInfos[key];
		if (gatherDropItemInfo.obj != null)
		{
			gatherDropItemInfo.obj.SetActive(false);
		}

		yield return new WaitForSeconds(3.0f);
		DestoryDropInfo(key);
	}

	private void DestoryDropInfo(string key, bool clearInfo = true)
	{
		MineDropItemInfo gatherDropItemInfo = m_GatherDropItemInfos[key];
		if (gatherDropItemInfo.obj != null)
		{
			DestroyGameObject(ref gatherDropItemInfo.obj);
		}

		if (clearInfo)
		{
			m_GatherDropItemInfos.Remove(key);
		}
	}

	/// <summary>
	/// 跃迁清gameObject
	/// </summary>
	public void DestoryAllDropGameObject()
	{
		if (m_GatherDropItemInfos.Count <= 0)
		{
			return;
		}

		foreach (var info in m_GatherDropItemInfos.Values)
		{
			DestroyGameObject(ref info.obj);
		}
	}

	/// <summary>
	/// 清缓存
	/// </summary>
	public void ClearAllInfo()
	{
		if (m_GatherDropItemInfos.Count <= 0)
		{
			return;
		}

		foreach (var info in m_GatherDropItemInfos.Values)
		{
			DestroyGameObject(ref info.obj);
		}
		m_GatherDropItemInfos.Clear();
	}

	private void DestroyGameObject(ref GameObject obj)
	{
		if (obj)
		{
			GameObject.Destroy(obj);
			obj = null;
		}
	}

	public void OnDestroy()
	{
		ClearAllInfo();
	}
}
