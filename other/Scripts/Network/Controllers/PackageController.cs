using Assets.Scripts.Define;
using Assets.Scripts.Lib.Net;
using Assets.Scripts.Proto;
using Eternity.Runtime.Item;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Frame.Net
{
	public class PackageController : AbsRpcController
	{
		private PackageProxy m_PackageProxy;
		private PackageProxy GetPackageProxy()
		{
			if (m_PackageProxy == null)
			{
				m_PackageProxy = GameFacade.Instance.RetrieveProxy(ProxyName.PackageProxy) as PackageProxy;
			}
			return m_PackageProxy;
		}

		/// <summary>
		/// 构造函数
		/// </summary>
		public PackageController() : base()
		{
			ListenServerMessage();
		}

		/// <summary>
		/// 绑定服务器消息
		/// </summary>
		private void ListenServerMessage()
		{
			NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_item_begin_sync, OnItemSyncBegin, typeof(S2C_ITEM_BEGIN_SYNC));
			NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_item_end_sync, OnItemSyncEnd, typeof(S2C_ITEM_END_SYNC));
			NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_item_sync, OnItemSync, typeof(S2C_ITEM_SYNC));
			NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_container_sync, OnContainerSync, typeof(S2C_CONTAINER_SYNC));
			NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_item_operate_list, OnItemChange, typeof(S2C_ITEM_OPERATE_LIST));
			NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_item_attr_sync, OnItemAttrChange, typeof(S2C_ITEM_ATTR_SYNC));
			NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_sell_back, OnItemSellBack, typeof(S2C_SELL_BACK));
		}

		#region S2C
		private bool m_needHoldEndToInit;
		/// <summary>
		/// 初始化接收数据开始
		/// </summary>
		/// <param name="buf"></param>
		private void OnItemSyncBegin(KProtoBuf buf)
		{
			GameFacade.Instance.SendNotification(NotificationName.MSG_PACKAGE_ITEMSYNC_BEGIN);
			m_needHoldEndToInit = true;
			GetPackageProxy().CleanupData();
		}

		/// <summary>
		/// 初始化接收数据结束
		/// </summary>
		/// <param name="buf"></param>
		private void OnItemSyncEnd(KProtoBuf buf)
		{
			GetPackageProxy().RelationData();
			m_needHoldEndToInit = false;
			GameFacade.Instance.SendNotification(NotificationName.MSG_PACKAGE_ITEMSYNC_END);
		}

		/// <summary>
		/// 同步item
		/// </summary>
		/// <param name="buf"></param>
		private void OnItemSync(KProtoBuf buf)
		{
			S2C_ITEM_SYNC msg = buf as S2C_ITEM_SYNC;
			GetPackageProxy().AddItem(m_needHoldEndToInit, msg.uid, msg.tid, msg.parent, msg.pos, msg.count, 0, msg.reference, msg.create_time);
		}

		/// <summary>
		/// 同步container 容量
		/// </summary>
		/// <param name="buf"></param>
		private void OnContainerSync(KProtoBuf buf)
		{
			S2C_CONTAINER_SYNC msg = buf as S2C_CONTAINER_SYNC;
			GetPackageProxy().ChangeContainerSize(msg.uid, msg.cur_capacity);
		}

		/// <summary>
		/// 同步道具属性
		/// </summary>
		/// <param name="buf"></param>
		private void OnItemAttrChange(KProtoBuf buf)
		{
			S2C_ITEM_ATTR_SYNC msg = buf as S2C_ITEM_ATTR_SYNC;
			GetPackageProxy().ChangeItemAttr(msg.uid, msg.lv, msg.exp);
		}

		/// <summary>
		/// 道具各种操作
		/// </summary>
		/// <param name="buf"></param>
		private void OnItemChange(KProtoBuf buf)
		{
			S2C_ITEM_OPERATE_LIST msg = buf as S2C_ITEM_OPERATE_LIST;
			if (msg.errcode == 0)
			{
				ItemOperateInfoTemp.OperateType weaponOperate = 0;
				Category category = 0;
				int pos = 0;
				ulong uid = 0;
                List<ulong> ships = new List<ulong>();
				for (int i = 0; i < msg.op_list.Count; i++)
				{
					ItemOperate itemMsg = msg.op_list[i];
					switch ((ItemProcessType)itemMsg.type)
					{
						case ItemProcessType.IPTAddItem:
							//操作顺序不可修改
							Category mainType = GetPackageProxy().AddItem(false, itemMsg.uid, itemMsg.tid, itemMsg.parent, itemMsg.pos, itemMsg.count, itemMsg.cur_capacity, itemMsg.reference, itemMsg.create_time);
							GetPackageProxy().ChangeItemAttr(itemMsg.uid, itemMsg.lv, itemMsg.exp);
							category = GetPackageProxy().CheckMarkItemAdd(msg.mark, itemMsg.uid);
							if (category == Category.Weapon)
							{
								if (weaponOperate == ItemOperateInfoTemp.OperateType.Remove)
								{
									weaponOperate = ItemOperateInfoTemp.OperateType.Replace;
									pos = itemMsg.pos;
									uid = itemMsg.uid;
								}
								else
								{
									weaponOperate = ItemOperateInfoTemp.OperateType.Add;
									pos = itemMsg.pos;
									uid = itemMsg.uid;
								}
							}
                            if (mainType == Category.Warship)
                            {
                                ships.Add(itemMsg.uid);
                            }
							//
							break;
						case ItemProcessType.IPTDeleteItem:
							//操作顺序不可修改
							category = GetPackageProxy().CheckMarkItemRemove(msg.mark, itemMsg.uid);
							GetPackageProxy().RemoveItem(itemMsg.uid);
							if (category == Category.Weapon)
							{
								weaponOperate = ItemOperateInfoTemp.OperateType.Remove;
								pos = itemMsg.pos;
								uid = itemMsg.uid;
							}
							//
							break;
						case ItemProcessType.IPTStackChange:
							GetPackageProxy().ChangeStackCount(itemMsg.uid, itemMsg.count);
							break;
						case ItemProcessType.IPTPositionChange:
							GetPackageProxy().ChangePosition(itemMsg.uid, itemMsg.parent, itemMsg.pos);
							break;
						case ItemProcessType.IPTCapacityChange:
							GetPackageProxy().ChangeContainerSize(itemMsg.uid, itemMsg.cur_capacity);
							break;
					}
				}
				switch ((ItemOperateType)msg.op_type)
				{
					case ItemOperateType.IOTAddItem:
						GameFacade.Instance.SendNotification(NotificationName.MSG_PACKAGE_ITEM_ADD);
						break;
					case ItemOperateType.IOTDestoryItem:
						GameFacade.Instance.SendNotification(NotificationName.MSG_PACKAGE_ITEM_DESTORY);
						break;
					case ItemOperateType.IOTConsumeItem:
						GameFacade.Instance.SendNotification(NotificationName.MSG_PACKAGE_ITEM_CONSUME);
						break;
					case ItemOperateType.IOTMoveItem:
						GameFacade.Instance.SendNotification(NotificationName.MSG_PACKAGE_ITEM_MOVE);                        
                        break;
                    case ItemOperateType.IOTSkillChanged:
                        {
                            PlayerSkillProxy skillProxy = GameFacade.Instance.RetrieveProxy(ProxyName.PlayerSkillProxy) as PlayerSkillProxy;
                            skillProxy.RefreshShipSkills();
                        }
                        break;
				}

				if (weaponOperate != 0)
				{
					ItemOperateInfoTemp info = new ItemOperateInfoTemp();
					info.Category = Category.Weapon;
					info.Type = weaponOperate;
					info.Pos = pos;
					info.UID = uid;
					GameFacade.Instance.SendNotification(NotificationName.MSG_PACKAGE_ITEM_OPERATE, info);

					ItemOperateEvent itemEvent = new ItemOperateEvent();
					info.Category = Category.Weapon;
					info.Type = weaponOperate;
					info.Pos = pos;
					info.UID = uid;
					
					GameplayProxy gameplayProxy = GameFacade.Instance.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;
					SpacecraftEntity entity = gameplayProxy.GetMainPlayer();

					entity?.SendEvent(ComponentEventName.ItemInPackageChanged, itemEvent);
				}
                if (ships.Count > 0)
                {
                    ShipProxy shipProxy = GameFacade.Instance.RetrieveProxy(ProxyName.ShipProxy) as ShipProxy;
                    foreach (ulong id in ships)
                    {
                        shipProxy.ChangeShipPackage(id);
                    }
                }                
			}
		}


		/// <summary>
		/// 售出返回
		/// </summary>
		/// <param name="buf"></param>
		private void OnItemSellBack(KProtoBuf buf)
		{
			S2C_SELL_BACK msg = (S2C_SELL_BACK)buf;
			if (msg.opt_result == 1)
			{

			}
			else
			{

			}
		}
		#endregion


		#region C2S
		/// <summary>
		/// 出售
		/// </summary>
		/// <param name="uid"></param>
		/// <param name="count"></param>
		public void RequestSell(ulong uid, ushort count)
		{
			C2S_ITEM_SELL req = SingleInstanceCache.GetInstanceByType<C2S_ITEM_SELL>();
			req.protocolID = (ushort)KC2S_Protocol.c2s_item_sell;
			req.uid = uid;
			req.count = count;
			NetworkManager.Instance.SendToGameServer(req);
		}

		/// <summary>
		/// 移动道具
		/// </summary>
		/// <param name="itemUid"></param>
		/// <param name="targetContainer"></param>
		public void ReqestMove(ulong itemUid, ulong targetContainer, ulong mark = 0)
		{
			ReqestMove(itemUid, targetContainer, ushort.MaxValue, mark);
		}

		/// <summary>
		/// 移动道具
		/// </summary>
		/// <param name="itemUid"></param>
		/// <param name="targetContainer"></param>
		/// <param name="targetPos"></param>
		public void ReqestMove(ulong itemUid, ulong targetContainer, int targetPos, ulong mark)
		{
			C2S_MOVE_ITEM req = SingleInstanceCache.GetInstanceByType<C2S_MOVE_ITEM>();
			req.protocolID = (ushort)KC2S_Protocol.c2s_move_item;
			req.mark = mark;
			req.src_item_uid = itemUid;
			req.dest_container_uid = targetContainer;
			req.dest_pos = (ushort)targetPos;
			NetworkManager.Instance.SendToGameServer(req);
		}
		#endregion

	}
}