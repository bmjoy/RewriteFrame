/*===============================
 * Author: [dinghuilin]
 * Purpose: FoundryController.cs
 * Time: 2019/03/28  10:02
================================*/
using Assets.Scripts.Lib.Net;
using Assets.Scripts.Proto;
using Game.Frame.Net;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 生产Controller
/// </summary>
public class FoundryController : BaseNetController
{
	/// <summary>
	/// 生产Proxy
	/// </summary>
	/// <returns></returns>
	private FoundryProxy m_FoundryProxy;

	public FoundryController()
	{
		m_FoundryProxy = (FoundryProxy)GameFacade.Instance.RetrieveProxy(ProxyName.FoundryProxy);
		NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_foundry_get_info_back, OnFoundryInfo, typeof(S2C_FOUNDRY_GETINFO_BACK));         //打开UI时，当前生产中，or 生产完成但未领取的
		NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_foundry_build_back, OnFoundryBuild, typeof(S2C_FOUNDRY_BUILD_BACK));//bulde 按钮的返回
		NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_foundry_cancel_back, OnFoundryCancel, typeof(S2C_FOUNDRY_CANCEL_BACK));//取消 budle 返回
		NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_foundry_speed_back, OnFoundrySpeed, typeof(S2C_FOUNDRY_SPEED_BACK)); // 立即完成wwwww
		NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_foundry_receive_back, OnFoundryReceive, typeof(S2C_FOUNDRY_RECEIVE_BACK));// 领取放回
	}

	#region C2S

	/// <summary>
	/// client获取生产信息
	/// </summary>
	public void SendGetFoundryInfo()
	{
		C2S_FOUNDRY_GET_INFO req = SingleInstanceCache.GetInstanceByType<C2S_FOUNDRY_GET_INFO>();
		req.protocolID = (ushort)KC2S_Protocol.c2s_foundry_get_info;
		NetworkManager.Instance.SendToGameServer(req);
	}

	/// <summary>
	/// client发给服务器激活的蓝图UID
	/// </summary>
	public void SendActiveInfo(List<int> list)
	{
		C2S_FOUNDRY_UID_INFO req = SingleInstanceCache.GetInstanceByType<C2S_FOUNDRY_UID_INFO>();
		req.protocolID = (ushort)KC2S_Protocol.c2s_foundry_uid_info;
		req.itemTids = list;
		NetworkManager.Instance.SendToGameServer(req);
	}

	/// <summary>
	///client 建造
	/// </summary>
	/// <param name="uid">实例id</param>
	public void SendBuild(int uid)
	{
		C2S_FOUNDRY_BUILD req = SingleInstanceCache.GetInstanceByType<C2S_FOUNDRY_BUILD>();
		req.protocolID = (ushort)KC2S_Protocol.c2s_foundry_build;
		req.itemTid = uid;
		NetworkManager.Instance.SendToGameServer(req);
	}

	/// <summary>
	/// client 加速生产
	/// </summary>
	/// <param name="uid">实例id</param>
	public void SendSpeed(int uid)
	{
		C2S_FOUNDRY_SPEED req = SingleInstanceCache.GetInstanceByType<C2S_FOUNDRY_SPEED>();
		req.protocolID = (ushort)KC2S_Protocol.c2s_foundry_speed;
		req.itemTid = uid;
		NetworkManager.Instance.SendToGameServer(req);
	}

	/// <summary>
	/// client 取消生产
	/// </summary>
	/// <param name="uid">实例id</param>
	public void SendCancel(int uid)
	{
		C2S_FOUNDRY_CANCEL req = SingleInstanceCache.GetInstanceByType<C2S_FOUNDRY_CANCEL>();
		req.protocolID = (ushort)KC2S_Protocol.c2s_foundry_cancel;
		req.itemTid = uid;
		NetworkManager.Instance.SendToGameServer(req);
	}

	/// <summary>
	/// client 接收
	/// </summary>
	/// <param name="uid">实例id</param>
	public void SendReceive(int uid)
	{
		C2S_FOUNDRY_RECEIVE req = SingleInstanceCache.GetInstanceByType<C2S_FOUNDRY_RECEIVE>();
		req.protocolID = (ushort)KC2S_Protocol.c2s_foundry_receive;
		req.itemTid = uid;
		NetworkManager.Instance.SendToGameServer(req);
	}

	/// <summary>
	/// client 
	/// </summary>
	/// <param name="uid">实例id</param>
	public void SendAddRobot()
	{
		C2S_FOUNDRY_ADD_ROBOT req = SingleInstanceCache.GetInstanceByType<C2S_FOUNDRY_ADD_ROBOT>();
		req.protocolID = (ushort)KC2S_Protocol.c2s_foundry_add_robot;
		NetworkManager.Instance.SendToGameServer(req);
	}

	#endregion

	#region S2C

	/// <summary>
	/// server 生产消息返回
	/// </summary>
	/// <param name="buf"></param>
	private void OnFoundryInfo(KProtoBuf buf)
	{
		S2C_FOUNDRY_GETINFO_BACK msg = buf as S2C_FOUNDRY_GETINFO_BACK;
		for (int i = 0; i < msg.member.Count; i++)
		{
			m_FoundryProxy.AddMember(
				msg.member[i].itemTid,
				(ulong)msg.member[i].startTime,
				(ulong)msg.member[i].endTime,
				ServerTimeUtil.Instance.GetNowTime() - (ulong)msg.member[i].startTime,
				msg.member[i].is_finish > 0);
		}
		MsgProduct msgProduct = MessageSingleton.Get<MsgProduct>();
		msgProduct.msg = NotificationName.MSG_FOUNDRY_GET_INFO;
		GameFacade.Instance.SendNotification(NotificationName.MSG_FOUNDRY_UPDATE, msgProduct);
	}

	/// <summary>
	/// server 生产建造返回
	/// </summary>
	/// <param name="buf"></param>
	private void OnFoundryBuild(KProtoBuf buf)
	{
		S2C_FOUNDRY_BUILD_BACK msg = buf as S2C_FOUNDRY_BUILD_BACK;
		m_FoundryProxy.AddMember(
			msg.member.itemTid,
			(ulong)msg.member.startTime,
			(ulong)msg.member.endTime,
			0,
			msg.member.is_finish > 0);
		MsgProduct msgProduct = MessageSingleton.Get<MsgProduct>();
		msgProduct.itemTID = msg.member.itemTid;
		msgProduct.msg = NotificationName.MSG_FOUNDRY_BUILD;
		GameFacade.Instance.SendNotification(NotificationName.MSG_FOUNDRY_UPDATE, msgProduct);
	}

	/// <summary>
	/// server 生产取消返回
	/// </summary>
	/// <param name="buf"></param>
	private void OnFoundryCancel(KProtoBuf buf)
	{
		S2C_FOUNDRY_CANCEL_BACK msg = buf as S2C_FOUNDRY_CANCEL_BACK;
		m_FoundryProxy.RemoveMember(msg.itemTid);
		MsgProduct msgProduct = MessageSingleton.Get<MsgProduct>();
		msgProduct.itemTID = msg.itemTid;
		msgProduct.msg = NotificationName.MSG_FOUNDRY_CANCEL;
		GameFacade.Instance.SendNotification(NotificationName.MSG_FOUNDRY_UPDATE, msgProduct);
	}

	/// <summary>
	/// server 生产加速返回
	/// </summary>
	/// <param name="buf"></param>
	private void OnFoundrySpeed(KProtoBuf buf)
	{
		S2C_FOUNDRY_SPEED_BACK msg = buf as S2C_FOUNDRY_SPEED_BACK;
		m_FoundryProxy.SetFoundryItemFinish(msg.itemTid);
		MsgProduct msgProduct = MessageSingleton.Get<MsgProduct>();
		msgProduct.itemTID = msg.itemTid;
		msgProduct.msg = NotificationName.MSG_FOUNDRY_SPEED;
		GameFacade.Instance.SendNotification(NotificationName.MSG_FOUNDRY_UPDATE, msgProduct);
	}

	/// <summary>
	/// server 生产接收返回
	/// </summary>
	/// <param name="buf"></param>
	private void OnFoundryReceive(KProtoBuf buf)
	{
		S2C_FOUNDRY_RECEIVE_BACK msg = buf as S2C_FOUNDRY_RECEIVE_BACK;
		//奖励面板
		ProduceInfoVO foundryMember = m_FoundryProxy.GetFoundryById(msg.itemTid);
		if (foundryMember == null)
		{
			return;
		}
		//var drawVO = (GameFacade.Instance.RetrieveProxy(ProxyName.CfgItemListProxy) as CfgItemListProxy).GetDrawingVOByKey((int)foundryMember.TID);
		//if (drawVO.ByteBuffer == null)
		//{
		//	return;
		//}
		//to do 奖励面板确定是否要
		m_FoundryProxy.RemoveMember(msg.itemTid);
		MsgProduct msgProduct = MessageSingleton.Get<MsgProduct>();
		msgProduct.itemTID = msg.itemTid;
		msgProduct.msg = NotificationName.MSG_FOUNDRY_RECEIVE;
		GameFacade.Instance.SendNotification(NotificationName.MSG_FOUNDRY_UPDATE, msgProduct);
	}

	#endregion
}
