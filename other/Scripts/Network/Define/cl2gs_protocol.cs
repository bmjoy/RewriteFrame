/*
compile by protobuf, please don't edit it manually. 
Any problem please contact tongxuehu@gmail.com, thx.
*/

using System;
using System.Collections.Generic;
using System.IO;
using Assets.Scripts.Lib.Net;

namespace Assets.Scripts.Proto
{
	public enum KProcotolVersion
	{
		eProcotolVersion = 1,
	}

	public enum KC2S_Protocol
	{
		client_gs_connection_begin = 0,
		c2s_handshake_request = 1,
		c2s_loading_complete = 2,
		c2s_apply_scene_obj = 3,
		c2s_player_logout = 4,
		c2s_hero_move = 5,
		c2s_ship_move = 6,
		c2s_player_turn = 7,
		c2s_gm_cmd = 8,
		c2s_click_hero = 9,
		c2s_cast_skill = 10,
		c2s_call_gs = 11,
		c2s_call_ls = 12,
		c2s_call_zs = 13,
		c2s_req_pos_info = 14,
		c2s_line_info_request = 15,
		c2s_req_force_move = 16,
		c2s_player_to_zs = 17,
		c2s_switch_map = 18,
		c2s_exit_pve = 19,
		c2s_player_relive = 20,
		c2s_receive_task = 21,
		c2s_commit_task = 22,
		c2s_drop_task = 23,
		c2s_npc_tasklist = 24,
		c2s_task_talkandexplore = 25,
		c2s_change_pkmode = 26,
		c2s_jump_map_request = 27,
		c2s_jump_map_confirm = 28,
		c2s_item_sell = 29,
		c2s_change_power = 30,
		c2s_foundry_get_info = 31,
		c2s_foundry_uid_info = 32,
		c2s_foundry_build = 33,
		c2s_foundry_speed = 34,
		c2s_foundry_cancel = 35,
		c2s_foundry_receive = 36,
		c2s_foundry_add_robot = 37,
		c2s_add_friend = 38,
		c2s_del_friend = 39,
		c2s_add_black = 40,
		c2s_del_black = 41,
		c2s_req_friendlist = 42,
		c2s_req_togetherlist = 43,
		c2s_handle_friend_req = 44,
		c2s_chat_message = 45,
		c2s_quit_message = 46,
		c2s_team_invite = 47,
		c2s_team_invite_reply = 48,
		c2s_team_leave = 49,
		c2s_team_leave_byleadr = 50,
		c2s_teamleader_change = 51,
		c2s_teamlist_request = 52,
		c2s_cast_skill_new = 53,
		c2s_mail_get_list = 54,
		c2s_mail_detail_info = 55,
		c2s_mail_get_accessory = 56,
		c2s_mail_delete = 57,
		c2s_mail_delete_all_read = 58,
		c2s_mail_starred = 59,
		c2s_mail_clear_new = 60,
		c2s_change_pose = 61,
		c2s_team_organize = 62,
		c2s_team_organize_opt = 63,
		c2s_team_organize_refuse = 64,
		c2s_change_gear = 65,
		c2s_cast_skill_tps = 66,
		c2s_battlezone_status = 67,
		c2s_request_open_chest = 68,
		c2s_remove_temporary_chest = 69,
		c2s_battlereward_attach = 70,
		c2s_change_magazine = 71,
		c2s_change_main_weapon = 72,
		c2s_client_frame = 73,
		c2s_ship_move_new = 74,
		c2s_human_move_new = 75,
		c2s_sync_hit_wall = 76,
		c2s_change_skill_target = 77,
		c2s_ship_jump_request = 78,
		c2s_change_ship_status = 79,
		c2s_consume_item = 80,
		c2s_destroy_item = 81,
		c2s_move_item = 82,
		c2s_open_pvd = 83,
		c2s_npc_talk = 84,
		c2s_delete_item_log = 85,
		c2s_all_delete_item_log = 86,
		c2s_request_all_item_log = 87,
		c2s_request_playerinfo = 88,
		c2s_request_personal_drop = 89,
		c2s_lock_target = 90,
		c2s_open_chest_by_key = 91,
		c2s_shop_info = 92,
		c2s_request_exchange = 93,
		c2s_buy_back_info = 94,
		c2s_special_buy_record = 95,
		c2s_request_levelup_dan = 96,
		client_gs_connection_end = 97,
	}

	public class C2S_HEADER: KProtoBuf
	{
		public ushort protocolID;
		public ulong clientTimeTick;
		public ulong gateTimeTick;

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
			writer.Write(protocolID);
			writer.Write(clientTimeTick);
			writer.Write(gateTimeTick);
		}
	}

	public class C2S_LOCK_TARGET: C2S_HEADER
	{
		public short type;

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
			writer.Write(type);
		}
	}

	public class C2S_CHANGE_SHIP_STATUS: C2S_HEADER
	{
		public short type;
		public ushort status;

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
			writer.Write(type);
			writer.Write(status);
		}
	}

	public class C2S_SHIP_MOVE_NEW: C2S_HEADER
	{
		public uint heroID;
		public float positon_x;
		public float positon_y;
		public float positon_z;
		public float rotation_x;
		public float rotation_y;
		public float rotation_z;
		public float rotation_w;
		public float line_velocity_x;
		public float line_velocity_y;
		public float line_velocity_z;
		public float angular_velocity_x;
		public float angular_velocity_y;
		public float angular_velocity_z;
		public float rotate_axis_x;
		public float rotate_axis_y;
		public float rotate_axis_z;
		public float engine_axis_x;
		public float engine_axis_y;
		public float engine_axis_z;
		public uint state;
		public uint lowState;
		public ulong client_send_tick;

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
			writer.Write(heroID);
			writer.Write(positon_x);
			writer.Write(positon_y);
			writer.Write(positon_z);
			writer.Write(rotation_x);
			writer.Write(rotation_y);
			writer.Write(rotation_z);
			writer.Write(rotation_w);
			writer.Write(line_velocity_x);
			writer.Write(line_velocity_y);
			writer.Write(line_velocity_z);
			writer.Write(angular_velocity_x);
			writer.Write(angular_velocity_y);
			writer.Write(angular_velocity_z);
			writer.Write(rotate_axis_x);
			writer.Write(rotate_axis_y);
			writer.Write(rotate_axis_z);
			writer.Write(engine_axis_x);
			writer.Write(engine_axis_y);
			writer.Write(engine_axis_z);
			writer.Write(state);
			writer.Write(lowState);
			writer.Write(client_send_tick);
		}
	}

	public class C2S_HUMAN_MOVE_NEW: C2S_HEADER
	{
		public uint heroID;
		public float positon_x;
		public float positon_y;
		public float positon_z;
		public float rotation_x;
		public float rotation_y;
		public float rotation_z;
		public float rotation_w;
		public float engine_axis_x;
		public float engine_axis_y;
		public float engine_axis_z;
		public byte run_flag;

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
			writer.Write(heroID);
			writer.Write(positon_x);
			writer.Write(positon_y);
			writer.Write(positon_z);
			writer.Write(rotation_x);
			writer.Write(rotation_y);
			writer.Write(rotation_z);
			writer.Write(rotation_w);
			writer.Write(engine_axis_x);
			writer.Write(engine_axis_y);
			writer.Write(engine_axis_z);
			writer.Write(run_flag);
		}
	}

	public class C2S_SYNC_HIT_WALL: C2S_HEADER
	{
		public byte hit_wall;

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
			writer.Write(hit_wall);
		}
	}

	public class C2S_CLIENT_FRAME: C2S_HEADER
	{
		public ulong client_frame;

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
			writer.Write(client_frame);
		}
	}

	public class C2S_CHANGE_MAGAZINE: C2S_HEADER
	{
		public ulong weapon_tid;

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
			writer.Write(weapon_tid);
		}
	}

	public class C2S_CHANGE_MAIN_WEAPON: C2S_HEADER
	{
		public ulong use_weapon_oid;

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
			writer.Write(use_weapon_oid);
		}
	}

	public class C2S_CHANGE_GEAR: C2S_HEADER
	{
		public short gear_change;

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
			writer.Write(gear_change);
		}
	}

	public class C2S_CHANGE_POSE: C2S_HEADER
	{
		public int pose_index;

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
			writer.Write(pose_index);
		}
	}

	public class C2S_CHANGE_POWER: C2S_HEADER
	{
		public int power;

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
			writer.Write(power);
		}
	}

	public class C2S_CHANGE_PKMODE: C2S_HEADER
	{
		public byte pk_mode;

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
			writer.Write(pk_mode);
		}
	}

	public class C2S_HANDSHAKE_REQUEST: C2S_HEADER
	{
		public ulong uRoleID;
		public byte procotolversion;
		public byte[] guid = null;
		public int _guidLength_ = 0;

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
			writer.Write(uRoleID);
			writer.Write(procotolversion);
			KProtoBuf.WriteByteArray(writer, guid, 16);
		}
	}

	public class C2S_LOADING_COMPLETE: C2S_HEADER
	{

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
		}
	}

	public class C2S_APPLY_SCENE_OBJ: C2S_HEADER
	{

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
		}
	}

	public class C2S_APPLY_SCENE_HERO_DATA: C2S_HEADER
	{

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
		}
	}

	public class C2S_PLAYER_LOGOUT: C2S_HEADER
	{

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
		}
	}

	public class C2S_HERO_MOVE: C2S_HEADER
	{
		public uint heroID;
		public int posX;
		public int posY;
		public int posZ;
		public short moveSpeed;
		public byte state;
		public ulong sendTime;

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
			writer.Write(heroID);
			writer.Write(posX);
			writer.Write(posY);
			writer.Write(posZ);
			writer.Write(moveSpeed);
			writer.Write(state);
			writer.Write(sendTime);
		}
	}

	public class C2S_SHIP_MOVE: C2S_HEADER
	{
		public uint heroID;
		public int posX;
		public int posY;
		public int posZ;
		public int targetX;
		public int targetY;
		public int targetZ;
		public int turnX;
		public int turnY;
		public int turnZ;
		public int trunW;
		public uint state;
		public uint lowState;
		public ulong timeStamp;
		public int moveSpeedX;
		public int moveSpeedY;
		public int moveSpeedZ;
		public int angularSpeedX;
		public int angularSpeedY;
		public int angularSpeedZ;

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
			writer.Write(heroID);
			writer.Write(posX);
			writer.Write(posY);
			writer.Write(posZ);
			writer.Write(targetX);
			writer.Write(targetY);
			writer.Write(targetZ);
			writer.Write(turnX);
			writer.Write(turnY);
			writer.Write(turnZ);
			writer.Write(trunW);
			writer.Write(state);
			writer.Write(lowState);
			writer.Write(timeStamp);
			writer.Write(moveSpeedX);
			writer.Write(moveSpeedY);
			writer.Write(moveSpeedZ);
			writer.Write(angularSpeedX);
			writer.Write(angularSpeedY);
			writer.Write(angularSpeedZ);
		}
	}

	public class C2S_PLAYER_TURN: C2S_HEADER
	{
		public uint heroID;
		public ushort turnX;
		public ushort turnY;
		public ushort turnZ;
		public ulong sendTime;

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
			writer.Write(heroID);
			writer.Write(turnX);
			writer.Write(turnY);
			writer.Write(turnZ);
			writer.Write(sendTime);
		}
	}

	public class C2S_GM_CMD: C2S_HEADER
	{
		public string command = "";

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
			KProtoBuf.WriteString(writer, command, 0);
		}
	}

	public class C2S_CLICK_HERO: C2S_HEADER
	{
		public uint heroID;

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
			writer.Write(heroID);
		}
	}

	public class WEAPONINFO: KProtoBuf
	{
		public int x;
		public int y;
		public int z;

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
			writer.Write(x);
			writer.Write(y);
			writer.Write(z);
		}
	}

	public class C2S_CHANGE_SKILL_TARGET: C2S_HEADER
	{
		public uint skill_id;
		public float x;
		public float y;
		public float z;
		public float w;
		public int target_id;

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
			writer.Write(skill_id);
			writer.Write(x);
			writer.Write(y);
			writer.Write(z);
			writer.Write(w);
			writer.Write(target_id);
		}
	}

	public class C2S_TargetInfo: KProtoBuf
	{
		public uint target_id;
		public ushort count;

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
			writer.Write(target_id);
			writer.Write(count);
		}
	}

	public class C2S_CAST_SKILL_TPS: C2S_HEADER
	{
		public short cast_type;
		public uint caster_id;
		public uint skill_id;
		public float x;
		public float y;
		public float z;
		public float w;
		public List<C2S_TargetInfo> target_info_list = new List<C2S_TargetInfo>();

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
			writer.Write(cast_type);
			writer.Write(caster_id);
			writer.Write(skill_id);
			writer.Write(x);
			writer.Write(y);
			writer.Write(z);
			writer.Write(w);
			{
				writer.Write((short)target_info_list.Count);
				List<C2S_TargetInfo>.Enumerator enumerator = target_info_list.GetEnumerator();
				while (enumerator.MoveNext())
				{
					C2S_TargetInfo itemInEnumerator = enumerator.Current;
					itemInEnumerator.Pack(writer);
				}
			}
		}
	}

	public class C2S_CAST_SKILL_NEW: C2S_HEADER
	{
		public uint caster_id;
		public uint target_id;
		public uint skill_id;
		public uint weapon_num;
		public List<WEAPONINFO> info = new List<WEAPONINFO>();

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
			writer.Write(caster_id);
			writer.Write(target_id);
			writer.Write(skill_id);
			writer.Write(weapon_num);
			{
				writer.Write((short)info.Count);
				List<WEAPONINFO>.Enumerator enumerator = info.GetEnumerator();
				while (enumerator.MoveNext())
				{
					WEAPONINFO itemInEnumerator = enumerator.Current;
					itemInEnumerator.Pack(writer);
				}
			}
		}
	}

	public class C2S_CAST_SKILL: C2S_HEADER
	{
		public uint casterID;
		public uint targetID;
		public uint skillID;
		public int x;
		public int y;
		public int z;
		public ulong sendTime;

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
			writer.Write(casterID);
			writer.Write(targetID);
			writer.Write(skillID);
			writer.Write(x);
			writer.Write(y);
			writer.Write(z);
			writer.Write(sendTime);
		}
	}

	public class C2S_CALL_SERVER: C2S_HEADER
	{
		public byte[] data = null;
		public int _dataLength_ = 0;

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
			KProtoBuf.WriteByteArray(writer, data, _dataLength_);
		}
	}

	public class C2S_REQ_POS_INFO: C2S_HEADER
	{

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
		}
	}

	public class C2S_LINE_INFO_REQUEST: C2S_HEADER
	{
		public int nGroupID;
		public int nMapID;

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
			writer.Write(nGroupID);
			writer.Write(nMapID);
		}
	}

	public class C2S_REQ_FORCE_MOVE: C2S_HEADER
	{
		public uint heroID;
		public ushort posX;
		public ushort posY;
		public short posZ;
		public byte forceMoveType;
		public int speed;
		public ulong sendTime;

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
			writer.Write(heroID);
			writer.Write(posX);
			writer.Write(posY);
			writer.Write(posZ);
			writer.Write(forceMoveType);
			writer.Write(speed);
			writer.Write(sendTime);
		}
	}

	public class C2S_PLAYER_TO_ZS: C2S_HEADER
	{
		public byte[] data = null;
		public int _dataLength_ = 0;

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
			KProtoBuf.WriteByteArray(writer, data, _dataLength_);
		}
	}

	public class C2S_SWITCH_MAP: C2S_HEADER
	{
		public uint telportID;
		public uint chanel_id;

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
			writer.Write(telportID);
			writer.Write(chanel_id);
		}
	}

	public class C2S_ExitPve: C2S_HEADER
	{

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
		}
	}

	public class C2S_PLAYER_RELIVE: C2S_HEADER
	{
		public ushort relive_type;

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
			writer.Write(relive_type);
		}
	}

	public class C2S_RECEIVE_TASK: C2S_HEADER
	{
		public uint taskID;

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
			writer.Write(taskID);
		}
	}

	public class C2S_COMMIT_TASK: C2S_HEADER
	{
		public ulong taskOID;

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
			writer.Write(taskOID);
		}
	}

	public class C2S_DROP_TASK: C2S_HEADER
	{
		public ulong taskOID;

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
			writer.Write(taskOID);
		}
	}

	public class C2S_NPC_TASKLIST: C2S_HEADER
	{
		public uint npcID;

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
			writer.Write(npcID);
		}
	}

	public class C2S_TASK_TALKANDEXPLORE: C2S_HEADER
	{
		public uint taskID;
		public uint npcID;
		public ushort flag_type;
		public ushort mission_type;

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
			writer.Write(taskID);
			writer.Write(npcID);
			writer.Write(flag_type);
			writer.Write(mission_type);
		}
	}

	public class C2S_TEAM_INVITE: C2S_HEADER
	{
		public ulong playerID;

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
			writer.Write(playerID);
		}
	}

	public class C2S_TEAM_INVITE_REPLY: C2S_HEADER
	{
		public ulong playerID;
		public sbyte accept;

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
			writer.Write(playerID);
			writer.Write(accept);
		}
	}

	public class C2S_TEAM_LEAVE: C2S_HEADER
	{

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
		}
	}

	public class C2S_TEAMLIST_REQUEST: C2S_HEADER
	{

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
		}
	}

	public class C2S_SHIP_JUMP_REQUEST: C2S_HEADER
	{
		public byte request_jump;
		public uint cur_area_id;
		public uint to_area_id;

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
			writer.Write(request_jump);
			writer.Write(cur_area_id);
			writer.Write(to_area_id);
		}
	}

	public class C2S_JUMP_MAP_CONFIRM: C2S_HEADER
	{
		public uint index;

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
			writer.Write(index);
		}
	}

	public class C2S_ITEM_SELL: C2S_HEADER
	{
		public ushort package_index;
		public ulong uid;
		public ushort count;

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
			writer.Write(package_index);
			writer.Write(uid);
			writer.Write(count);
		}
	}

	public class C2S_FOUNDRY_GET_INFO: C2S_HEADER
	{

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
		}
	}

	public class C2S_FOUNDRY_UID_INFO: C2S_HEADER
	{
		public List<int> itemTids = new List<int>();

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
			{
				writer.Write((short)itemTids.Count);
				List<int>.Enumerator enumerator = itemTids.GetEnumerator();
				while (enumerator.MoveNext())
				{
					int itemInEnumerator = enumerator.Current;
					writer.Write(itemInEnumerator);
				}
			}
		}
	}

	public class C2S_FOUNDRY_BUILD: C2S_HEADER
	{
		public int itemTid;

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
			writer.Write(itemTid);
		}
	}

	public class C2S_FOUNDRY_SPEED: C2S_HEADER
	{
		public int itemTid;

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
			writer.Write(itemTid);
		}
	}

	public class C2S_FOUNDRY_CANCEL: C2S_HEADER
	{
		public int itemTid;

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
			writer.Write(itemTid);
		}
	}

	public class C2S_FOUNDRY_RECEIVE: C2S_HEADER
	{
		public int itemTid;

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
			writer.Write(itemTid);
		}
	}

	public class C2S_FOUNDRY_ADD_ROBOT: C2S_HEADER
	{

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
		}
	}

	public class C2S_ADD_FRIEND: C2S_HEADER
	{
		public ulong uid;

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
			writer.Write(uid);
		}
	}

	public class C2S_DEL_FRIEND: C2S_HEADER
	{
		public ulong uid;

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
			writer.Write(uid);
		}
	}

	public class C2S_ADD_BLACK: C2S_HEADER
	{
		public ulong uid;

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
			writer.Write(uid);
		}
	}

	public class C2S_DEL_BLACK: C2S_HEADER
	{
		public ulong uid;

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
			writer.Write(uid);
		}
	}

	public class C2S_REQ_FRIENDLIST: C2S_HEADER
	{

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
		}
	}

	public class C2S_REQ_TOGETHERLIST: C2S_HEADER
	{

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
		}
	}

	public class C2S_HANDLE_FRIEND_REQ: C2S_HEADER
	{
		public uint id;
		public byte accept;

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
			writer.Write(id);
			writer.Write(accept);
		}
	}

	public class C2S_CHAT_MESSAGE: C2S_HEADER
	{
		public ushort channel;
		public ulong toID;
		public string message = "";

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
			writer.Write(channel);
			writer.Write(toID);
			KProtoBuf.WriteString(writer, message, 1024);
		}
	}

	public class C2S_QUIT_MESSAGE: C2S_HEADER
	{

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
		}
	}

	public class C2S_MAIL_GET_LIST: C2S_HEADER
	{

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
		}
	}

	public class C2S_MAIL_DETAIL_INFO: C2S_HEADER
	{
		public string id = "";

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
			KProtoBuf.WriteString(writer, id, 32);
		}
	}

	public class C2S_MAIL_GET_ACCESSORY: C2S_HEADER
	{
		public string id = "";
		public byte del_after_get_accessory;

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
			KProtoBuf.WriteString(writer, id, 32);
			writer.Write(del_after_get_accessory);
		}
	}

	public class C2S_MAIL_DELETE: C2S_HEADER
	{
		public string id = "";

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
			KProtoBuf.WriteString(writer, id, 32);
		}
	}

	public class C2S_MAIL_DELETE_ALL_READ: C2S_HEADER
	{

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
		}
	}

	public class C2S_MAIL_STARRED: C2S_HEADER
	{
		public string id = "";

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
			KProtoBuf.WriteString(writer, id, 32);
		}
	}

	public class C2S_MAIL_CLEAR_NEW: C2S_HEADER
	{
		public List<string> ids = new List<string>();

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
			{
				writer.Write((short)ids.Count);
				List<string>.Enumerator enumerator = ids.GetEnumerator();
				while (enumerator.MoveNext())
				{
					string itemInEnumerator = enumerator.Current;
					KProtoBuf.WriteString(writer, itemInEnumerator, 32);
				}
			}
		}
	}

	public class C2S_TEAM_ORGANIZE: C2S_HEADER
	{
		public uint maptype;
		public uint mapid;

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
			writer.Write(maptype);
			writer.Write(mapid);
		}
	}

	public class C2S_TEAM_ORGANIZE_OPT: C2S_HEADER
	{
		public int opt_notify;

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
			writer.Write(opt_notify);
		}
	}

	public class C2S_TEAM_ORGANIZE_REFUSE: C2S_HEADER
	{

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
		}
	}

	public class C2S_TEAM_LEAVE_BYLEADR: C2S_HEADER
	{
		public ulong remove_playerid;

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
			writer.Write(remove_playerid);
		}
	}

	public class C2S_TEAMLEADER_CHANGE: C2S_HEADER
	{
		public ulong change_to_playerid;

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
			writer.Write(change_to_playerid);
		}
	}

	public class C2S_DILACATED_BY_UID: C2S_HEADER
	{
		public int type;
		public ulong shipuid;
		public ulong weaponuid;

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
			writer.Write(type);
			writer.Write(shipuid);
			writer.Write(weaponuid);
		}
	}

	public class C2S_BATTLEZONE_STATUS: C2S_HEADER
	{
		public uint type;

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
			writer.Write(type);
		}
	}

	public class C2S_REQUEST_OPEN_CHEST: C2S_HEADER
	{
		public uint req_type;
		public ulong reward_uid;

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
			writer.Write(req_type);
			writer.Write(reward_uid);
		}
	}

	public class C2S_REMOVE_TEMPORARY_CHEST: C2S_HEADER
	{
		public ulong reward_uid;

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
			writer.Write(reward_uid);
		}
	}

	public class C2S_CONSUME_ITEM: C2S_HEADER
	{
		public ulong uid;
		public uint count;

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
			writer.Write(uid);
			writer.Write(count);
		}
	}

	public class C2S_DESTROY_ITEM: C2S_HEADER
	{
		public ulong uid;

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
			writer.Write(uid);
		}
	}

	public class C2S_MOVE_ITEM: C2S_HEADER
	{
		public ulong mark;
		public ulong src_item_uid;
		public ulong dest_container_uid;
		public ushort dest_pos;

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
			writer.Write(mark);
			writer.Write(src_item_uid);
			writer.Write(dest_container_uid);
			writer.Write(dest_pos);
		}
	}

	public class C2S_NPC_TALK: C2S_HEADER
	{
		public uint npcId;
		public uint dialogId;

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
			writer.Write(npcId);
			writer.Write(dialogId);
		}
	}

	public class C2S_OPEN_PVD: C2S_HEADER
	{
		public byte flag;

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
			writer.Write(flag);
		}
	}

	public class C2S_DELETE_ITEM_LOG: C2S_HEADER
	{
		public string id = "";

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
			KProtoBuf.WriteString(writer, id, 32);
		}
	}

	public class C2S_ALL_DELETE_ITEM_LOG: C2S_HEADER
	{

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
		}
	}

	public class C2S_REQUEST_ALL_ITEM_LOG: C2S_HEADER
	{

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
		}
	}

	public class C2S_REQUEST_PLAYERINFO: C2S_HEADER
	{
		public ulong id;

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
			writer.Write(id);
		}
	}

	public class C2S_REQUEST_PERSONAL_DROP: C2S_HEADER
	{
		public ulong id;

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
			writer.Write(id);
		}
	}

	public class C2S_OPEN_CHEST_BY_KEY: C2S_HEADER
	{
		public ulong hero_uid;
		public uint key_tid;

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
			writer.Write(hero_uid);
			writer.Write(key_tid);
		}
	}

	public class C2S_SHOP_INFO: C2S_HEADER
	{
		public uint shop_id;

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
			writer.Write(shop_id);
		}
	}

	public class C2S_REQUEST_EXCHANGE: C2S_HEADER
	{
		public byte op_code;
		public uint shop_id;
		public ulong item_id;
		public ulong uid;
		public uint num;

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
			writer.Write(op_code);
			writer.Write(shop_id);
			writer.Write(item_id);
			writer.Write(uid);
			writer.Write(num);
		}
	}

	public class C2S_BUY_BACK_INFO: C2S_HEADER
	{
		public uint shop_id;

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
			writer.Write(shop_id);
		}
	}

	public class C2S_SPECIAL_BUY_RECORD: C2S_HEADER
	{
		public ulong uid;

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
			writer.Write(uid);
		}
	}

	public class C2S_REQUEST_LEVELUP_DAN: C2S_HEADER
	{
		public ulong uid;

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
			writer.Write(uid);
		}
	}

}
