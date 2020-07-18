
using Assets.Scripts.Define;
using Assets.Scripts.Lib.Net;
using Assets.Scripts.Proto;
using Crucis.Protocol;
using Crucis.Protocol.GameSession;
using Game.Frame.Net;
using PureMVC.Patterns.Facade;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropItemController : AbsRpcController
{
    public DropItemController() : base()
    {
        NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_sync_drop_list, OnSaveDropItemInfo, typeof(S2C_SYNC_DROP_LIST));
        NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_chest_get_result, OnPickUpResult, typeof(S2C_CHEST_GET_RESULT));
	}

    /// <summary>
    /// 掉落创建
    /// </summary>
    /// <param name="buf">协议内容</param>


    private void OnSaveDropItemInfo(KProtoBuf buf)
    {
        ///S2C_SYNC_DROP_LIST msg = buf as S2C_SYNC_DROP_LIST;
		///Debug.LogError("msg.drop_list.Count:" + msg.drop_list.Count);
		///DropItemManager.Instance.OnSaveDropItemInfo((uint)msg.hero_uid, msg.drop_list, 1);

	}

    /// <summary>
    /// 掉落拾取
    /// </summary>
    /// <param name="buf">协议内容</param>
    private void OnPickUpResult(KProtoBuf buf)
    {

        //S2C_CHEST_GET_RESULT msg = buf as S2C_CHEST_GET_RESULT;
		//Debug.LogErrorFormat("S2C_CHEST_GET_RESULT  {0}  result {1}", msg.chest_uid, msg.error_code);
		//DropItemManager.Instance.PickUpResult(msg.chest_uid, msg.error_code);
    }

    /// <summary>
    /// 发送拾取协议
    /// </summary>
    /// <param name="id">宝箱唯一id</param>
    /// <param name="type">宝箱类型</param>
    /// 
    public void OnSendPickUpProtocol(ulong id, uint type = 2)
    {

        //Debug.LogErrorFormat("OnSendPickUpProtocol  {0}", id);
        if(DropItemManager.Instance.CheckIsDropItem((uint)id))
        {
            //Debug.LogErrorFormat("OnSendPickUpProtocol 1 {0}", id);
            if (DropItemManager.Instance.CheckIsOnCreate(id))
                UIManager.Instance.StartCoroutine(DelayToSendProtocol(id));
            else
                SendPickUpProtocol(id);
        }

	}

    private IEnumerator DelayToSendProtocol(ulong key)
    {
        //Debug.LogErrorFormat("DelayToSendProtocol2  {0}", key);
        yield return new WaitForSeconds(5.0f);
        //Debug.LogErrorFormat("DelayToSendProtocol3  {0}", key);
        SendPickUpProtocol(key);
    }

	public async void SendPickUpProtocol(ulong id, uint type = 2)
	{
		//C2S_REQUEST_OPEN_CHEST msg = new C2S_REQUEST_OPEN_CHEST();
		//msg.protocolID = (ushort)KC2S_Protocol.c2s_request_open_chest;
		//msg.reward_uid = id;
		//msg.req_type = type; //
		//NetworkManager.Instance.SendToGameServer(msg);
		//Debug.Log("自动领取宝箱");

		Debug.Log("------->>>>>发送拾取宝箱信息" + id);
		c2s_RequestOpenChest msg = new c2s_RequestOpenChest();
		msg.RewardUid = id;
		msg.ReqType = type;
		AskOpenChestResponse askOpenChestResponse = await AskOpenChestRPC.AskOpenChest(msg);
		Debug.Log("------->>>>>收拾取宝箱结果信息" + id);
		if (askOpenChestResponse.Except != null)
		{
			Debug.LogError("------->>>>>>拾取宝箱失败");
		}
		else if (askOpenChestResponse.Success != null)
		{
			Debug.Log("------->>>>>>拾取宝箱成功");
			s2c_ChestGetResult result = askOpenChestResponse.Success.Success_;
			DropItemManager.Instance.PickUpResult(result.ChestUid, result.ErrorCode);
		}
	}

	public async void SendOpenChestByKey(uint keyId, ulong id)
	{
		//Debug.Log("发送Id" + keyId + "===="+ id);
		//C2S_OPEN_CHEST_BY_KEY msg = new C2S_OPEN_CHEST_BY_KEY();
		//msg.protocolID = (ushort)KC2S_Protocol.c2s_open_chest_by_key;
		//msg.hero_uid = id;
		//msg.key_tid = keyId; 
		//NetworkManager.Instance.SendToGameServer(msg);
		Debug.Log("------->>>>>发送开宝箱信息" + id);
		c2s_OpenChestByKey msg = new c2s_OpenChestByKey();
		msg.HeroUid = id;
		msg.KeyTid = keyId;
		AskOpenChestByKeyResponse askOpenChestByKeyResponse = await AskOpenChestByKeyRPC.AskOpenChestByKey(msg);
		Debug.Log("------->>>>>收开宝箱结果信息" + id);
		if (askOpenChestByKeyResponse.Except != null)
		{
			Debug.LogError("------->>>>>>开宝箱失败");
		}
		else if (askOpenChestByKeyResponse.Success != null)
		{
			Debug.Log("------->>>>>>开宝箱成功");
			///s2c_OpenChestByKeyResult result = askOpenChestByKeyResponse.Success.Success_;
			GameFacade.Instance.SendNotification(NotificationName.MSG_S2C_OPEN_CEHST_BY_KEY_RESULT);
		}
	}
}