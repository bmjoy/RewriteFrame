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
	public enum KS2C_Protocol
	{
		gs_client_connection_begin = 0,
		s2c_sync_player_base_info = 1,
		s2c_sync_player_state_info = 2,
		s2c_account_kickout = 3,
		s2c_switch_map = 4,
		s2c_hero_move = 5,
		s2c_hero_stop = 6,
		s2c_ship_move = 7,
		s2c_hero_force_move = 8,
		s2c_hero_pos_info = 9,
		s2c_battle_startedframe = 10,
		s2c_player_turn = 11,
		s2c_sync_scene_time_frame = 12,
		s2c_sync_scene_obj_end = 13,
		s2c_sync_scene_hero_data_end = 14,
		s2c_sync_new_hero = 15,
		s2c_sync_hero_data = 16,
		s2c_cast_skill = 17,
		s2c_stop_skill = 18,
		s2c_skill_effect = 19,
		s2c_heal_effect = 20,
		s2c_trigger_apply = 21,
		s2c_skill_cd_update = 22,
		s2c_add_buff_notify = 23,
		s2c_del_buff_notify = 24,
		s2c_hero_death = 25,
		s2c_remove_scene_obj = 26,
		s2c_sync_hero_hpmp_anger = 27,
		s2c_sync_move_speed = 28,
		s2c_cast_skill_fail_notify = 29,
		s2c_force_change_pos = 30,
		s2c_add_exp = 31,
		s2c_sync_new_player_info = 32,
		s2c_update_player_info = 33,
		s2c_hero_relive = 34,
		s2c_update_level = 35,
		s2c_call_script = 36,
		s2c_sync_all_attribute = 37,
		s2c_sync_one_attribute = 38,
		s2c_sync_self_attribute = 39,
		s2c_add_summon = 40,
		s2c_remove_summon = 41,
		s2c_sync_pkinfo = 42,
		s2c_sync_teaminfo = 43,
		s2c_line_info_respond = 44,
		s2c_sync_hero_ex = 45,
		s2c_add_doodad = 46,
		s2c_add_effect_obj = 47,
		s2c_sync_hero_leave = 48,
		s2c_skill_modify = 49,
		s2c_hero_action = 50,
		s2c_effect_immuno = 51,
		s2c_zs_to_player = 52,
		s2c_switch_zs_respond = 53,
		s2c_switch_gc_respond = 54,
		s2c_update_revenge_player = 55,
		s2c_syn_counterattack_player = 56,
		s2c_pve_end = 57,
		s2c_show_tips_info = 58,
		s2c_pve_mission_value_change = 59,
		s2c_pve_mission_values = 60,
		s2c_reward_list = 61,
		s2c_fight_result = 62,
		s2c_error_code = 63,
		s2c_sing_skill = 64,
		s2c_receive_task = 65,
		s2c_drop_task = 66,
		s2c_commit_task = 67,
		s2c_task_process = 68,
		s2c_send_tasklist = 69,
		s2c_send_npc_mission = 70,
		s2c_task_finishlist = 71,
		s2c_task_faillist = 72,
		s2c_task_failnotify = 73,
		s2c_task_statenotify = 74,
		s2c_notify_memberbattleinfo_list = 75,
		s2c_commit_stage = 76,
		s2c_send_taskcanlist = 77,
		s2c_send_taskzone = 78,
		s2c_team_member_list = 79,
		s2c_team_member_added = 80,
		s2c_team_member_leave = 81,
		s2c_team_member_change = 82,
		s2c_team_dissove = 83,
		s2c_team_create = 84,
		s2c_team_bechairman = 85,
		s2c_team_member_forceleave = 86,
		s2c_team_invite = 87,
		s2c_team_invite_error = 88,
		s2c_team_invite_reply = 89,
		s2c_team_invite_reply_error = 90,
		s2c_syn_fight_status = 91,
		s2c_relive_time = 92,
		s2c_ship_jump_response = 93,
		s2c_jump_map_confirm = 94,
		s2c_money_infos = 95,
		s2c_money_change = 96,
		s2c_sell_back = 97,
		s2c_job_info = 98,
		s2c_foundry_get_info_back = 99,
		s2c_foundry_build_back = 100,
		s2c_foundry_speed_back = 101,
		s2c_foundry_cancel_back = 102,
		s2c_foundry_receive_back = 103,
		s2c_foundry_add_robot_back = 104,
		s2c_add_friend_back = 105,
		s2c_del_friend_back = 106,
		s2c_add_black_back = 107,
		s2c_del_black_back = 108,
		s2c_sync_friendlist = 109,
		s2c_sync_togetherlist = 110,
		s2c_reset_friend = 111,
		s2c_sync_friend = 112,
		s2c_set_status_back = 113,
		s2c_sync_friend_invite = 114,
		s2c_add_friend_invite = 115,
		s2c_del_friend_invite = 116,
		s2c_chat_message = 117,
		s2c_popup_window = 118,
		s2c_call_skill_target_lsit = 119,
		s2c_call_skill_pos_lsit = 120,
		s2c_attr_change = 121,
		s2c_brocast_ship_state = 122,
		s2c_brocast_trigger_state = 123,
		s2c_mail_get_list = 124,
		s2c_mail_detail_info = 125,
		s2c_mail_get_accessory = 126,
		s2c_mail_delete = 127,
		s2c_mail_total_count = 128,
		s2c_mail_new = 129,
		s2c_mail_param_list = 130,
		s2c_mail_starred = 131,
		s2c_share_scene_hero = 132,
		s2c_share_move_hero = 133,
		s2c_share_remove_hero = 134,
		s2c_sync_pose = 135,
		s2c_sync_gear = 136,
		s2c_team_organize_result = 137,
		s2c_team_organize_opt = 138,
		s2c_brocast_trigger_energy = 139,
		s2c_brocast_die_type = 140,
		s2c_subtask_update = 141,
		s2c_battle_status = 142,
		s2c_temporary_chest = 143,
		s2c_chest_get_result = 144,
		s2c_weapon_value = 145,
		s2c_scene_event_state = 146,
		s2c_scene_event_mission_progress_changed = 147,
		s2c_ship_move_new = 148,
		s2c_human_move_new = 149,
		s2c_monster_road_point = 150,
		s2c_ship_force_action = 151,
		s2c_item_begin_sync = 152,
		s2c_item_end_sync = 153,
		s2c_item_sync = 154,
		s2c_container_sync = 155,
		s2c_item_attr_sync = 156,
		s2c_item_operate_list = 157,
		s2c_sync_contact_geometry = 158,
		s2c_sync_hero_state = 159,
		s2c_sync_drop_list = 160,
		s2c_notify_delete_item_log = 161,
		s2c_notify_new_item_log = 162,
		s2c_sync_item_log = 163,
		s2c_enter_battle = 164,
		s2c_leave_battle = 165,
		s2c_change_plot_time = 166,
		s2c_change_plot_state = 167,
		s2c_add_trigger_obj = 168,
		s2c_plot_monster_list = 169,
		s2c_sync_fightship_visible_item_list = 170,
		s2c_sync_playerinfo = 171,
		s2c_sync_personal_drop = 172,
		s2c_notify_personal_drop_result = 173,
		s2c_sync_detector_distance = 174,
		s2c_sync_player_discover_precious = 175,
		s2c_open_cehst_by_key_result = 176,
		s2c_sync_boss_hud = 177,
		s2c_sync_camp_changed = 178,
		s2c_sync_seal_changed = 179,
		s2c_sync_shop_info = 180,
		s2c_exchange_result = 181,
		s2c_sync_buy_back = 182,
		s2c_sync_special_buy_record = 183,
		s2c_levelup_reward_list = 184,
		gs_client_connection_end = 185,
	}

	public class S2C_HEADER: KProtoBuf
	{
		public ushort protocolID;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			protocolID = reader.ReadUInt16();
		}
	}

	public class Vectors: KProtoBuf
	{
		public float x;
		public float y;
		public float z;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			x = reader.ReadSingle();
			y = reader.ReadSingle();
			z = reader.ReadSingle();
		}
	}

	public class MsgQuaternion: KProtoBuf
	{
		public float x;
		public float y;
		public float z;
		public float w;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			x = reader.ReadSingle();
			y = reader.ReadSingle();
			z = reader.ReadSingle();
			w = reader.ReadSingle();
		}
	}

	public class S2C_MONSTER_ROAD_POINT: S2C_HEADER
	{
		public uint heroID;
		public sbyte behavior_type;
		public uint bind_hero_id;
		public Vectors bind_hero_offset = new Vectors();
		public sbyte bind_be_refresh;
		public sbyte bind_be_imme;
		public List<Vectors> monster_road_point = new List<Vectors>();

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			heroID = reader.ReadUInt32();
			behavior_type = reader.ReadSByte();
			bind_hero_id = reader.ReadUInt32();
			bind_hero_offset = KProtoBuf.CreateProtoBufAndUnPack<Vectors>(reader);
			bind_be_refresh = reader.ReadSByte();
			bind_be_imme = reader.ReadSByte();
			{
				monster_road_point.Clear();
				int __arrayLengthInBuffer = reader.ReadInt16();
				for (int jArrayIndex = 0; jArrayIndex < __arrayLengthInBuffer; jArrayIndex++)
					monster_road_point.Add(KProtoBuf.CreateProtoBufAndUnPack<Vectors>(reader));
			}
		}
	}

	public class S2C_SHIP_MOVE_NEW: S2C_HEADER
	{
		public short type;
		public uint heroID;
		public sbyte behavior_type;
		public uint bind_hero_id;
		public Vectors bind_hero_offset = new Vectors();
		public sbyte bind_be_refresh;
		public sbyte bind_be_imme;
		public ulong client_send_tick;
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
		public Vectors predict_point = new Vectors();
		public ulong state;
		public uint lowState;
		public byte hit_wall;
		public List<Vectors> monster_road_point = new List<Vectors>();

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			type = reader.ReadInt16();
			heroID = reader.ReadUInt32();
			behavior_type = reader.ReadSByte();
			bind_hero_id = reader.ReadUInt32();
			bind_hero_offset = KProtoBuf.CreateProtoBufAndUnPack<Vectors>(reader);
			bind_be_refresh = reader.ReadSByte();
			bind_be_imme = reader.ReadSByte();
			client_send_tick = reader.ReadUInt64();
			positon_x = reader.ReadSingle();
			positon_y = reader.ReadSingle();
			positon_z = reader.ReadSingle();
			rotation_x = reader.ReadSingle();
			rotation_y = reader.ReadSingle();
			rotation_z = reader.ReadSingle();
			rotation_w = reader.ReadSingle();
			line_velocity_x = reader.ReadSingle();
			line_velocity_y = reader.ReadSingle();
			line_velocity_z = reader.ReadSingle();
			angular_velocity_x = reader.ReadSingle();
			angular_velocity_y = reader.ReadSingle();
			angular_velocity_z = reader.ReadSingle();
			rotate_axis_x = reader.ReadSingle();
			rotate_axis_y = reader.ReadSingle();
			rotate_axis_z = reader.ReadSingle();
			engine_axis_x = reader.ReadSingle();
			engine_axis_y = reader.ReadSingle();
			engine_axis_z = reader.ReadSingle();
			predict_point = KProtoBuf.CreateProtoBufAndUnPack<Vectors>(reader);
			state = reader.ReadUInt64();
			lowState = reader.ReadUInt32();
			hit_wall = reader.ReadByte();
			{
				monster_road_point.Clear();
				int __arrayLengthInBuffer = reader.ReadInt16();
				for (int jArrayIndex = 0; jArrayIndex < __arrayLengthInBuffer; jArrayIndex++)
					monster_road_point.Add(KProtoBuf.CreateProtoBufAndUnPack<Vectors>(reader));
			}
		}
	}

	public class S2C_HUMAN_MOVE_NEW: S2C_HEADER
	{
		public short type;
		public sbyte run_flag;
		public uint heroID;
		public Vectors position = new Vectors();
		public MsgQuaternion rotation = new MsgQuaternion();
		public Vectors engine_axis = new Vectors();

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			type = reader.ReadInt16();
			run_flag = reader.ReadSByte();
			heroID = reader.ReadUInt32();
			position = KProtoBuf.CreateProtoBufAndUnPack<Vectors>(reader);
			rotation = KProtoBuf.CreateProtoBufAndUnPack<MsgQuaternion>(reader);
			engine_axis = KProtoBuf.CreateProtoBufAndUnPack<Vectors>(reader);
		}
	}

	public class S2C_SHIP_FORCE_ACTION: S2C_HEADER
	{
		public uint heroID;
		public short action_type;
		public sbyte with_param;
		public Vectors position = new Vectors();
		public Vectors velocity = new Vectors();
		public Vectors angularVelocity = new Vectors();
		public MsgQuaternion rotation = new MsgQuaternion();
		public Vectors rotate_axis = new Vectors();
		public Vectors engine_axis = new Vectors();
		public short motion;
		public List<Vectors> targets = new List<Vectors>();

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			heroID = reader.ReadUInt32();
			action_type = reader.ReadInt16();
			with_param = reader.ReadSByte();
			position = KProtoBuf.CreateProtoBufAndUnPack<Vectors>(reader);
			velocity = KProtoBuf.CreateProtoBufAndUnPack<Vectors>(reader);
			angularVelocity = KProtoBuf.CreateProtoBufAndUnPack<Vectors>(reader);
			rotation = KProtoBuf.CreateProtoBufAndUnPack<MsgQuaternion>(reader);
			rotate_axis = KProtoBuf.CreateProtoBufAndUnPack<Vectors>(reader);
			engine_axis = KProtoBuf.CreateProtoBufAndUnPack<Vectors>(reader);
			motion = reader.ReadInt16();
			{
				targets.Clear();
				int __arrayLengthInBuffer = reader.ReadInt16();
				for (int jArrayIndex = 0; jArrayIndex < __arrayLengthInBuffer; jArrayIndex++)
					targets.Add(KProtoBuf.CreateProtoBufAndUnPack<Vectors>(reader));
			}
		}
	}

	public class WEAPONVALUE: KProtoBuf
	{
		public ulong weapon_oid;
		public int cur_value;
		public int max_value;
		public int safty_valve;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			weapon_oid = reader.ReadUInt64();
			cur_value = reader.ReadInt32();
			max_value = reader.ReadInt32();
			safty_valve = reader.ReadInt32();
		}
	}

	public class S2C_WEAPON_VALUE: S2C_HEADER
	{
		public ulong cur_weapon_uid;
		public List<WEAPONVALUE> infos = new List<WEAPONVALUE>();

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			cur_weapon_uid = reader.ReadUInt64();
			{
				infos.Clear();
				int __arrayLengthInBuffer = reader.ReadInt16();
				for (int jArrayIndex = 0; jArrayIndex < __arrayLengthInBuffer; jArrayIndex++)
					infos.Add(KProtoBuf.CreateProtoBufAndUnPack<WEAPONVALUE>(reader));
			}
		}
	}

	public class S2C_SYNC_POSE: S2C_HEADER
	{
		public uint hero_id;
		public int pose_index;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			hero_id = reader.ReadUInt32();
			pose_index = reader.ReadInt32();
		}
	}

	public class S2C_SYNC_GEAR: S2C_HEADER
	{
		public short gear;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			gear = reader.ReadInt16();
		}
	}

	public class S2C_SYN_FIGHT_STATUS: S2C_HEADER
	{
		public uint id;
		public sbyte is_fighting;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			id = reader.ReadUInt32();
			is_fighting = reader.ReadSByte();
		}
	}

	public class S2C_SYNC_PLAYER_BASE_INFO: S2C_HEADER
	{
		public ulong uPlayerID;
		public int nLastSaveTime;
		public int nLastLoginTime;
		public int nTotalGameTime;
		public int nCreateTime;
		public sbyte byGender;
		public string szAccountName = "";
		public string szPlayerName = "";
		public int nServerTime;
		public sbyte byHeroJob;
		public int nGroupID;
		public int power;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			uPlayerID = reader.ReadUInt64();
			nLastSaveTime = reader.ReadInt32();
			nLastLoginTime = reader.ReadInt32();
			nTotalGameTime = reader.ReadInt32();
			nCreateTime = reader.ReadInt32();
			byGender = reader.ReadSByte();
			szAccountName = KProtoBuf.ReadString(reader, 64);
			szPlayerName = KProtoBuf.ReadString(reader, 64);
			nServerTime = reader.ReadInt32();
			byHeroJob = reader.ReadSByte();
			nGroupID = reader.ReadInt32();
			power = reader.ReadInt32();
		}
	}

	public class S2C_SYNC_PLAYER_STATE_INFO: S2C_HEADER
	{
		public ushort uJob;
		public byte byVipLevel;
		public uint nVIPExp;
		public uint nVIPEndTime;
		public ushort nLevel;
		public double dExp;
		public uint nVitality;
		public int power;
		public uint dan_level;
		public double dan_exp;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			uJob = reader.ReadUInt16();
			byVipLevel = reader.ReadByte();
			nVIPExp = reader.ReadUInt32();
			nVIPEndTime = reader.ReadUInt32();
			nLevel = reader.ReadUInt16();
			dExp = reader.ReadDouble();
			nVitality = reader.ReadUInt32();
			power = reader.ReadInt32();
			dan_level = reader.ReadUInt32();
			dan_exp = reader.ReadDouble();
		}
	}

	public class S2C_ACCOUNT_KICKOUT: S2C_HEADER
	{
		public int nTag;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			nTag = reader.ReadInt32();
		}
	}

	public class S2C_SWITCH_MAP: S2C_HEADER
	{
		public uint mapID;
		public int copyIndex;
		public int lineID;
		public float posX;
		public float posY;
		public float posZ;
		public ulong area_id;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			mapID = reader.ReadUInt32();
			copyIndex = reader.ReadInt32();
			lineID = reader.ReadInt32();
			posX = reader.ReadSingle();
			posY = reader.ReadSingle();
			posZ = reader.ReadSingle();
			area_id = reader.ReadUInt64();
		}
	}

	public class S2C_HERO_MOVE: S2C_HEADER
	{
		public uint heroID;
		public int posX;
		public int posY;
		public int posZ;
		public ushort frameTime;
		public byte state;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			heroID = reader.ReadUInt32();
			posX = reader.ReadInt32();
			posY = reader.ReadInt32();
			posZ = reader.ReadInt32();
			frameTime = reader.ReadUInt16();
			state = reader.ReadByte();
		}
	}

	public class S2C_PLAYER_TURN: S2C_HEADER
	{
		public uint heroID;
		public ushort turnX;
		public ushort turnY;
		public ushort turnZ;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			heroID = reader.ReadUInt32();
			turnX = reader.ReadUInt16();
			turnY = reader.ReadUInt16();
			turnZ = reader.ReadUInt16();
		}
	}

	public class S2C_HERO_STOP: S2C_HEADER
	{
		public uint heroID;
		public ushort posX;
		public ushort posY;
		public short posZ;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			heroID = reader.ReadUInt32();
			posX = reader.ReadUInt16();
			posY = reader.ReadUInt16();
			posZ = reader.ReadInt16();
		}
	}

	public class S2C_HERO_FORCE_MOVE: S2C_HEADER
	{
		public uint heroID;
		public ushort posX;
		public ushort posY;
		public short posZ;
		public byte forceMoveType;
		public ushort forceMoveFTime;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			heroID = reader.ReadUInt32();
			posX = reader.ReadUInt16();
			posY = reader.ReadUInt16();
			posZ = reader.ReadInt16();
			forceMoveType = reader.ReadByte();
			forceMoveFTime = reader.ReadUInt16();
		}
	}

	public class S2C_SHIP_MOVE: S2C_HEADER
	{
		public uint heroID;
		public ulong area_id;
		public int posX;
		public int posY;
		public int posZ;
		public int targetX;
		public int targetY;
		public int targetZ;
		public int turnX;
		public int turnY;
		public int turnZ;
		public int turnW;
		public ulong state;
		public uint lowState;
		public ulong timeStamp;
		public int moveSpeedX;
		public int moveSpeedY;
		public int moveSpeedZ;
		public int angularSpeedX;
		public int angularSpeedY;
		public int angularSpeedZ;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			heroID = reader.ReadUInt32();
			area_id = reader.ReadUInt64();
			posX = reader.ReadInt32();
			posY = reader.ReadInt32();
			posZ = reader.ReadInt32();
			targetX = reader.ReadInt32();
			targetY = reader.ReadInt32();
			targetZ = reader.ReadInt32();
			turnX = reader.ReadInt32();
			turnY = reader.ReadInt32();
			turnZ = reader.ReadInt32();
			turnW = reader.ReadInt32();
			state = reader.ReadUInt64();
			lowState = reader.ReadUInt32();
			timeStamp = reader.ReadUInt64();
			moveSpeedX = reader.ReadInt32();
			moveSpeedY = reader.ReadInt32();
			moveSpeedZ = reader.ReadInt32();
			angularSpeedX = reader.ReadInt32();
			angularSpeedY = reader.ReadInt32();
			angularSpeedZ = reader.ReadInt32();
		}
	}

	public class S2C_HERO_POS_INFO: S2C_HEADER
	{
		public uint heroID;
		public int posX;
		public int posY;
		public int posZ;
		public ushort moveToX;
		public ushort moveToY;
		public ushort moveToZ;
		public uint state;
		public ushort moveSpeed;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			heroID = reader.ReadUInt32();
			posX = reader.ReadInt32();
			posY = reader.ReadInt32();
			posZ = reader.ReadInt32();
			moveToX = reader.ReadUInt16();
			moveToY = reader.ReadUInt16();
			moveToZ = reader.ReadUInt16();
			state = reader.ReadUInt32();
			moveSpeed = reader.ReadUInt16();
		}
	}

	public class S2C_BATTLE_STARTEDFRAME: S2C_HEADER
	{
		public int startFrame;
		public int startedFrame;
		public int totalFrame;
		public int leftFrame;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			startFrame = reader.ReadInt32();
			startedFrame = reader.ReadInt32();
			totalFrame = reader.ReadInt32();
			leftFrame = reader.ReadInt32();
		}
	}

	public class S2C_SYNC_SCENE_TIME_FRAME: S2C_HEADER
	{
		public ulong timeFrame;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			timeFrame = reader.ReadUInt64();
		}
	}

	public class S2C_SYNC_SCENE_OBJ_END: S2C_HEADER
	{

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
		}
	}

	public class S2C_SYNC_SCENE_HERO_DATA_END: S2C_HEADER
	{

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
		}
	}

	public class KHeroBuffList: KProtoBuf
	{
		public uint bufferID;
		public byte overlapCount;
		public int nTime;
		public uint link_id;
		public byte is_master;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			bufferID = reader.ReadUInt32();
			overlapCount = reader.ReadByte();
			nTime = reader.ReadInt32();
			link_id = reader.ReadUInt32();
			is_master = reader.ReadByte();
		}
	}

	public class WeaponList: KProtoBuf
	{
		public int id;
		public int tabIndex;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			id = reader.ReadInt32();
			tabIndex = reader.ReadInt32();
		}
	}

	public class ATTR: KProtoBuf
	{
		public uint id;
		public double value;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			id = reader.ReadUInt32();
			value = reader.ReadDouble();
		}
	}

	public class S2C_SYNC_NEW_HERO: S2C_HEADER
	{
		public uint id;
		public uint teamID;
		public byte teamMode;
		public byte campID;
		public byte pkMode;
		public uint ownerHeroID;
		public ulong ownerPlayerID;
		public uint guildID;
		public uint massID;
		public uint uLevel;
		public float faceDirX;
		public float faceDirY;
		public float faceDirZ;
		public float faceDirW;
		public sbyte invincible;
		public sbyte visible;
		public ulong area_id;
		public float posX;
		public float posY;
		public float posZ;
		public float desX;
		public float desY;
		public float desZ;
		public uint templateID;
		public ushort moveSpeed;
		public ulong state;
		public uint moveState;
		public ulong startFrame;
		public byte bNewHero;
		public long HP;
		public long EnergyPower;
		public long SuperMagnetic;
		public long MaxHP;
		public long MaxEnergyPower;
		public long MaxSuperMagnetic;
		public long mana_shield_value;
		public long mana_shield_value_max;
		public long defense_shield_value;
		public long defense_shield_value_max;
		public long shield_value;
		public long shield_value_max;
		public uint enterPose;
		public ushort heroGroup;
		public short createIndex;
		public byte animationType;
		public uint item_tid;
		public string szPlayerName = "";
		public sbyte is_fighting;
		public int ViewSight;
		public sbyte is_refresh;
		public short gear;
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
		public ulong teleport_id_;
		public long current_peerless;
		public long max_peerless;
		public uint weakness_effect_id;
		public byte is_seal;
		public List<KHeroBuffList> bufferList = new List<KHeroBuffList>();

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			id = reader.ReadUInt32();
			teamID = reader.ReadUInt32();
			teamMode = reader.ReadByte();
			campID = reader.ReadByte();
			pkMode = reader.ReadByte();
			ownerHeroID = reader.ReadUInt32();
			ownerPlayerID = reader.ReadUInt64();
			guildID = reader.ReadUInt32();
			massID = reader.ReadUInt32();
			uLevel = reader.ReadUInt32();
			faceDirX = reader.ReadSingle();
			faceDirY = reader.ReadSingle();
			faceDirZ = reader.ReadSingle();
			faceDirW = reader.ReadSingle();
			invincible = reader.ReadSByte();
			visible = reader.ReadSByte();
			area_id = reader.ReadUInt64();
			posX = reader.ReadSingle();
			posY = reader.ReadSingle();
			posZ = reader.ReadSingle();
			desX = reader.ReadSingle();
			desY = reader.ReadSingle();
			desZ = reader.ReadSingle();
			templateID = reader.ReadUInt32();
			moveSpeed = reader.ReadUInt16();
			state = reader.ReadUInt64();
			moveState = reader.ReadUInt32();
			startFrame = reader.ReadUInt64();
			bNewHero = reader.ReadByte();
			HP = reader.ReadInt64();
			EnergyPower = reader.ReadInt64();
			SuperMagnetic = reader.ReadInt64();
			MaxHP = reader.ReadInt64();
			MaxEnergyPower = reader.ReadInt64();
			MaxSuperMagnetic = reader.ReadInt64();
			mana_shield_value = reader.ReadInt64();
			mana_shield_value_max = reader.ReadInt64();
			defense_shield_value = reader.ReadInt64();
			defense_shield_value_max = reader.ReadInt64();
			shield_value = reader.ReadInt64();
			shield_value_max = reader.ReadInt64();
			enterPose = reader.ReadUInt32();
			heroGroup = reader.ReadUInt16();
			createIndex = reader.ReadInt16();
			animationType = reader.ReadByte();
			item_tid = reader.ReadUInt32();
			szPlayerName = KProtoBuf.ReadString(reader, 64);
			is_fighting = reader.ReadSByte();
			ViewSight = reader.ReadInt32();
			is_refresh = reader.ReadSByte();
			gear = reader.ReadInt16();
			line_velocity_x = reader.ReadSingle();
			line_velocity_y = reader.ReadSingle();
			line_velocity_z = reader.ReadSingle();
			angular_velocity_x = reader.ReadSingle();
			angular_velocity_y = reader.ReadSingle();
			angular_velocity_z = reader.ReadSingle();
			rotate_axis_x = reader.ReadSingle();
			rotate_axis_y = reader.ReadSingle();
			rotate_axis_z = reader.ReadSingle();
			engine_axis_x = reader.ReadSingle();
			engine_axis_y = reader.ReadSingle();
			engine_axis_z = reader.ReadSingle();
			teleport_id_ = reader.ReadUInt64();
			current_peerless = reader.ReadInt64();
			max_peerless = reader.ReadInt64();
			weakness_effect_id = reader.ReadUInt32();
			is_seal = reader.ReadByte();
			{
				bufferList.Clear();
				int __arrayLengthInBuffer = reader.ReadInt16();
				for (int jArrayIndex = 0; jArrayIndex < __arrayLengthInBuffer; jArrayIndex++)
					bufferList.Add(KProtoBuf.CreateProtoBufAndUnPack<KHeroBuffList>(reader));
			}
		}
	}

	public class S2C_SYNC_HERO_DATA: S2C_HEADER
	{
		public ushort wJob;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			wJob = reader.ReadUInt16();
		}
	}

	public class S2C_SKILL_EFFECT: S2C_HEADER
	{
		public uint wTargetHeroID;
		public uint wCasterID;
		public uint wTriggerID;
		public uint wDamage;
		public uint PenetrationDamage;
		public short crit_type;
		public sbyte isdoge;
		public byte byAttackEvent;
		public ushort count;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			wTargetHeroID = reader.ReadUInt32();
			wCasterID = reader.ReadUInt32();
			wTriggerID = reader.ReadUInt32();
			wDamage = reader.ReadUInt32();
			PenetrationDamage = reader.ReadUInt32();
			crit_type = reader.ReadInt16();
			isdoge = reader.ReadSByte();
			byAttackEvent = reader.ReadByte();
			count = reader.ReadUInt16();
		}
	}

	public class S2C_HEAL_EFFECT: S2C_HEADER
	{
		public uint Healtype;
		public uint wTargetHeroID;
		public uint wCasterID;
		public uint wTriggerID;
		public uint wHealValue;
		public byte HealFrom;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			Healtype = reader.ReadUInt32();
			wTargetHeroID = reader.ReadUInt32();
			wCasterID = reader.ReadUInt32();
			wTriggerID = reader.ReadUInt32();
			wHealValue = reader.ReadUInt32();
			HealFrom = reader.ReadByte();
		}
	}

	public class S2C_SING_SKILL: S2C_HEADER
	{
		public uint casterID;
		public uint skillID;
		public uint target_id;
		public float x;
		public float y;
		public float z;
		public short hanging_point_id;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			casterID = reader.ReadUInt32();
			skillID = reader.ReadUInt32();
			target_id = reader.ReadUInt32();
			x = reader.ReadSingle();
			y = reader.ReadSingle();
			z = reader.ReadSingle();
			hanging_point_id = reader.ReadInt16();
		}
	}

	public class S2C_TargetInfo: KProtoBuf
	{
		public uint targetID;
		public ushort count;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			targetID = reader.ReadUInt32();
			count = reader.ReadUInt16();
		}
	}

	public class S2C_CAST_SKILL: S2C_HEADER
	{
		public uint casterID;
		public uint skillID;
		public uint modifySkillID;
		public float x;
		public float y;
		public float z;
		public short hanging_point_id;
		public List<S2C_TargetInfo> target_info_list = new List<S2C_TargetInfo>();

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			casterID = reader.ReadUInt32();
			skillID = reader.ReadUInt32();
			modifySkillID = reader.ReadUInt32();
			x = reader.ReadSingle();
			y = reader.ReadSingle();
			z = reader.ReadSingle();
			hanging_point_id = reader.ReadInt16();
			{
				target_info_list.Clear();
				int __arrayLengthInBuffer = reader.ReadInt16();
				for (int jArrayIndex = 0; jArrayIndex < __arrayLengthInBuffer; jArrayIndex++)
					target_info_list.Add(KProtoBuf.CreateProtoBufAndUnPack<S2C_TargetInfo>(reader));
			}
		}
	}

	public class POSITION: KProtoBuf
	{
		public int x;
		public int y;
		public int z;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			x = reader.ReadInt32();
			y = reader.ReadInt32();
			z = reader.ReadInt32();
		}
	}

	public class S2C_CALL_SKILL_TARGET_LSIT: S2C_HEADER
	{
		public uint caster_id;
		public uint skill_id;
		public List<uint> target_list = new List<uint>();

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			caster_id = reader.ReadUInt32();
			skill_id = reader.ReadUInt32();
			{
				target_list.Clear();
				int __arrayLengthInBuffer = reader.ReadInt16();
				for (int jArrayIndex = 0; jArrayIndex < __arrayLengthInBuffer; jArrayIndex++)
					target_list.Add(reader.ReadUInt32());
			}
		}
	}

	public class S2C_CALL_SKILL_POS_LSIT: S2C_HEADER
	{
		public uint caster_id;
		public uint skill_id;
		public List<POSITION> pos_list = new List<POSITION>();

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			caster_id = reader.ReadUInt32();
			skill_id = reader.ReadUInt32();
			{
				pos_list.Clear();
				int __arrayLengthInBuffer = reader.ReadInt16();
				for (int jArrayIndex = 0; jArrayIndex < __arrayLengthInBuffer; jArrayIndex++)
					pos_list.Add(KProtoBuf.CreateProtoBufAndUnPack<POSITION>(reader));
			}
		}
	}

	public class S2C_STOP_SKILL: S2C_HEADER
	{
		public uint casterID;
		public uint skillID;
		public uint modifySkillID;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			casterID = reader.ReadUInt32();
			skillID = reader.ReadUInt32();
			modifySkillID = reader.ReadUInt32();
		}
	}

	public class S2C_TRIGGER_APPLY: S2C_HEADER
	{
		public uint wTriggerID;
		public uint wCasterID;
		public uint wTargetID;
		public ushort posX;
		public ushort posY;
		public short posZ;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			wTriggerID = reader.ReadUInt32();
			wCasterID = reader.ReadUInt32();
			wTargetID = reader.ReadUInt32();
			posX = reader.ReadUInt16();
			posY = reader.ReadUInt16();
			posZ = reader.ReadInt16();
		}
	}

	public class S2C_ADD_BUFF_NOTIFY: S2C_HEADER
	{
		public uint herID;
		public uint wBuffID;
		public byte byOverLap;
		public int nTime;
		public uint link_id;
		public byte is_master;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			herID = reader.ReadUInt32();
			wBuffID = reader.ReadUInt32();
			byOverLap = reader.ReadByte();
			nTime = reader.ReadInt32();
			link_id = reader.ReadUInt32();
			is_master = reader.ReadByte();
		}
	}

	public class S2C_DEL_BUFF_NOTIFY: S2C_HEADER
	{
		public uint herID;
		public uint wBuffID;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			herID = reader.ReadUInt32();
			wBuffID = reader.ReadUInt32();
		}
	}

	public class S2C_RELIVE_TIME: S2C_HEADER
	{
		public int cd;
		public uint killer_id;
		public string killer_player_name = "";
		public List<short> relive_type_list = new List<short>();

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			cd = reader.ReadInt32();
			killer_id = reader.ReadUInt32();
			killer_player_name = KProtoBuf.ReadString(reader, 64);
			{
				relive_type_list.Clear();
				int __arrayLengthInBuffer = reader.ReadInt16();
				for (int jArrayIndex = 0; jArrayIndex < __arrayLengthInBuffer; jArrayIndex++)
					relive_type_list.Add(reader.ReadInt16());
			}
		}
	}

	public class DropInfo: KProtoBuf
	{
		public ulong player_uid;
		public uint chest_tid;
		public uint ship_tlv;
		public uint quality;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			player_uid = reader.ReadUInt64();
			chest_tid = reader.ReadUInt32();
			ship_tlv = reader.ReadUInt32();
			quality = reader.ReadUInt32();
		}
	}

	public class S2C_HERO_DEATH: S2C_HEADER
	{
		public uint heroID;
		public uint KillerID;
		public uint triggerID;
		public List<DropInfo> drop_list = new List<DropInfo>();

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			heroID = reader.ReadUInt32();
			KillerID = reader.ReadUInt32();
			triggerID = reader.ReadUInt32();
			{
				drop_list.Clear();
				int __arrayLengthInBuffer = reader.ReadInt16();
				for (int jArrayIndex = 0; jArrayIndex < __arrayLengthInBuffer; jArrayIndex++)
					drop_list.Add(KProtoBuf.CreateProtoBufAndUnPack<DropInfo>(reader));
			}
		}
	}

	public class S2C_REMOVE_SCENE_OBJ: S2C_HEADER
	{
		public uint objID;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			objID = reader.ReadUInt32();
		}
	}

	public class S2C_SYNC_HERO_HPMP_ANGER: S2C_HEADER
	{
		public uint heroID;
		public long hp;
		public long energyPower;
		public long superMagnetic;
		public long mana_shield_value;
		public long defense_shield_value;
		public long shield_value;
		public long current_peerless;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			heroID = reader.ReadUInt32();
			hp = reader.ReadInt64();
			energyPower = reader.ReadInt64();
			superMagnetic = reader.ReadInt64();
			mana_shield_value = reader.ReadInt64();
			defense_shield_value = reader.ReadInt64();
			shield_value = reader.ReadInt64();
			current_peerless = reader.ReadInt64();
		}
	}

	public class S2C_SYNC_MOVE_SPEED: S2C_HEADER
	{
		public uint heroID;
		public ushort moveSpeed;
		public int moveFrameTime;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			heroID = reader.ReadUInt32();
			moveSpeed = reader.ReadUInt16();
			moveFrameTime = reader.ReadInt32();
		}
	}

	public class S2C_CAST_SKILL_FAIL_NOTIFY: S2C_HEADER
	{
		public uint skillID;
		public uint code;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			skillID = reader.ReadUInt32();
			code = reader.ReadUInt32();
		}
	}

	public class S2C_FORCE_CHANGE_POS: S2C_HEADER
	{
		public byte forced;
		public uint heroID;
		public ushort moveState;
		public byte reset_all;
		public ulong area_id;
		public Vectors position = new Vectors();
		public Vectors velocity = new Vectors();
		public Vectors angularVelocity = new Vectors();
		public MsgQuaternion rotation = new MsgQuaternion();
		public Vectors engineAxis = new Vectors();
		public Vectors rotateAxis = new Vectors();

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			forced = reader.ReadByte();
			heroID = reader.ReadUInt32();
			moveState = reader.ReadUInt16();
			reset_all = reader.ReadByte();
			area_id = reader.ReadUInt64();
			position = KProtoBuf.CreateProtoBufAndUnPack<Vectors>(reader);
			velocity = KProtoBuf.CreateProtoBufAndUnPack<Vectors>(reader);
			angularVelocity = KProtoBuf.CreateProtoBufAndUnPack<Vectors>(reader);
			rotation = KProtoBuf.CreateProtoBufAndUnPack<MsgQuaternion>(reader);
			engineAxis = KProtoBuf.CreateProtoBufAndUnPack<Vectors>(reader);
			rotateAxis = KProtoBuf.CreateProtoBufAndUnPack<Vectors>(reader);
		}
	}

	public class S2C_ADD_EXP: S2C_HEADER
	{
		public ushort ntype;
		public ulong nuid;
		public ulong shipid;
		public double curExp;
		public double addExp;
		public double cur_dan_exp;
		public double add_dan_exp;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			ntype = reader.ReadUInt16();
			nuid = reader.ReadUInt64();
			shipid = reader.ReadUInt64();
			curExp = reader.ReadDouble();
			addExp = reader.ReadDouble();
			cur_dan_exp = reader.ReadDouble();
			add_dan_exp = reader.ReadDouble();
		}
	}

	public class S2C_SYNC_NEW_PLAYER_INFO: S2C_HEADER
	{
		public ulong playerID;
		public string szPlayerName = "";
		public string szGuild = "";
		public byte byGender;
		public byte[] syncPlayerInfo = null;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			playerID = reader.ReadUInt64();
			szPlayerName = KProtoBuf.ReadString(reader, 64);
			szGuild = KProtoBuf.ReadString(reader, 32);
			byGender = reader.ReadByte();
			syncPlayerInfo = KProtoBuf.ReadByteArray(reader, 0);
		}
	}

	public class S2C_UPDATE_PLAYER_INFO: S2C_HEADER
	{
		public ulong playerID;
		public string szPlayerName = "";
		public string szGuild = "";
		public byte byGender;
		public byte[] syncPlayerInfo = null;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			playerID = reader.ReadUInt64();
			szPlayerName = KProtoBuf.ReadString(reader, 64);
			szGuild = KProtoBuf.ReadString(reader, 32);
			byGender = reader.ReadByte();
			syncPlayerInfo = KProtoBuf.ReadByteArray(reader, 0);
		}
	}

	public class S2C_HERO_RELIVE: S2C_HEADER
	{
		public uint heroID;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			heroID = reader.ReadUInt32();
		}
	}

	public class S2C_UPDATE_LEVEL: S2C_HEADER
	{
		public ushort ntype;
		public ulong nuid;
		public ulong shipid;
		public ushort nlevel;
		public double dExp;
		public ushort dan_level;
		public double dan_exp;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			ntype = reader.ReadUInt16();
			nuid = reader.ReadUInt64();
			shipid = reader.ReadUInt64();
			nlevel = reader.ReadUInt16();
			dExp = reader.ReadDouble();
			dan_level = reader.ReadUInt16();
			dan_exp = reader.ReadDouble();
		}
	}

	public class S2C_CALL_SCRIPT: S2C_HEADER
	{
		public byte[] data = null;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			data = KProtoBuf.ReadByteArray(reader, 0);
		}
	}

	public class S2C_SYNC_ALL_ATTRIBUTE: S2C_HEADER
	{
		public int MaxHP;
		public int MaxMP;
		public int MaxAnger;
		public int Attack;
		public int Defence;
		public int Miss;
		public int NotMiss;
		public int Crit;
		public int NotCrit;
		public int Block;
		public int NotBlock;
		public int CritHurtPP;
		public int NotCritHurtPP;
		public int BlockMultiPP;
		public int MoveSpeed;
		public int AttackCof;
		public int DamageCof;
		public int CritHurt;
		public int NotCritHurt;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			MaxHP = reader.ReadInt32();
			MaxMP = reader.ReadInt32();
			MaxAnger = reader.ReadInt32();
			Attack = reader.ReadInt32();
			Defence = reader.ReadInt32();
			Miss = reader.ReadInt32();
			NotMiss = reader.ReadInt32();
			Crit = reader.ReadInt32();
			NotCrit = reader.ReadInt32();
			Block = reader.ReadInt32();
			NotBlock = reader.ReadInt32();
			CritHurtPP = reader.ReadInt32();
			NotCritHurtPP = reader.ReadInt32();
			BlockMultiPP = reader.ReadInt32();
			MoveSpeed = reader.ReadInt32();
			AttackCof = reader.ReadInt32();
			DamageCof = reader.ReadInt32();
			CritHurt = reader.ReadInt32();
			NotCritHurt = reader.ReadInt32();
		}
	}

	public class S2C_SYNC_ONE_ATTRIBUTE: S2C_HEADER
	{
		public uint HeroID;
		public byte AttributeType;
		public int AttributeValue;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			HeroID = reader.ReadUInt32();
			AttributeType = reader.ReadByte();
			AttributeValue = reader.ReadInt32();
		}
	}

	public class S2C_SYNC_SELF_ATTRIBUTE: S2C_HEADER
	{
		public byte AttributeType;
		public int AttributeValue;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			AttributeType = reader.ReadByte();
			AttributeValue = reader.ReadInt32();
		}
	}

	public class S2C_SKILL_CD_UPDATE: S2C_HEADER
	{
		public uint uHeroID;
		public ushort skillID;
		public ushort newCDFrameTime;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			uHeroID = reader.ReadUInt32();
			skillID = reader.ReadUInt16();
			newCDFrameTime = reader.ReadUInt16();
		}
	}

	public class S2C_ADD_SUMMON: S2C_HEADER
	{
		public uint uHeroID;
		public uint uTemplateID;
		public uint uLessTime;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			uHeroID = reader.ReadUInt32();
			uTemplateID = reader.ReadUInt32();
			uLessTime = reader.ReadUInt32();
		}
	}

	public class S2C_REMOVE_SUMMON: S2C_HEADER
	{
		public uint uHeroID;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			uHeroID = reader.ReadUInt32();
		}
	}

	public class S2C_SYNC_PKINFO: S2C_HEADER
	{
		public uint heroID;
		public byte campID;
		public byte pkMode;
		public uint guildID;
		public uint massID;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			heroID = reader.ReadUInt32();
			campID = reader.ReadByte();
			pkMode = reader.ReadByte();
			guildID = reader.ReadUInt32();
			massID = reader.ReadUInt32();
		}
	}

	public class S2C_SYNC_TEAMINFO: S2C_HEADER
	{
		public uint heroID;
		public byte teamMode;
		public uint teamID;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			heroID = reader.ReadUInt32();
			teamMode = reader.ReadByte();
			teamID = reader.ReadUInt32();
		}
	}

	public class KLineData: KProtoBuf
	{
		public int nLineID;
		public byte bFull;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			nLineID = reader.ReadInt32();
			bFull = reader.ReadByte();
		}
	}

	public class S2C_LINE_INFO_RESPOND: S2C_HEADER
	{
		public uint uMapID;
		public uint uAutoSync;
		public List<KLineData> lineDatas = new List<KLineData>();

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			uMapID = reader.ReadUInt32();
			uAutoSync = reader.ReadUInt32();
			{
				lineDatas.Clear();
				int __arrayLengthInBuffer = reader.ReadInt16();
				for (int jArrayIndex = 0; jArrayIndex < __arrayLengthInBuffer; jArrayIndex++)
					lineDatas.Add(KProtoBuf.CreateProtoBufAndUnPack<KLineData>(reader));
			}
		}
	}

	public class S2C_SYNC_HERO_EX: S2C_HEADER
	{
		public uint heroID;
		public string szPlayerName = "";

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			heroID = reader.ReadUInt32();
			szPlayerName = KProtoBuf.ReadString(reader, 64);
		}
	}

	public class S2C_ADD_DOODAD: S2C_HEADER
	{
		public uint uID;
		public byte uType;
		public int nX;
		public int nY;
		public int nLeftTime;
		public byte bOpen;
		public string szName = "";
		public List<int> nParams = new List<int>();

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			uID = reader.ReadUInt32();
			uType = reader.ReadByte();
			nX = reader.ReadInt32();
			nY = reader.ReadInt32();
			nLeftTime = reader.ReadInt32();
			bOpen = reader.ReadByte();
			szName = KProtoBuf.ReadString(reader, 64);
			{
				nParams.Clear();
				int __arrayLengthInBuffer = reader.ReadInt16();
				for (int jArrayIndex = 0; jArrayIndex < __arrayLengthInBuffer; jArrayIndex++)
					nParams.Add(reader.ReadInt32());
			}
		}
	}

	public class S2C_ADD_EFFECT_OBJ: S2C_HEADER
	{
		public uint uID;
		public byte uType;
		public int nX;
		public int nY;
		public List<int> nParams = new List<int>();

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			uID = reader.ReadUInt32();
			uType = reader.ReadByte();
			nX = reader.ReadInt32();
			nY = reader.ReadInt32();
			{
				nParams.Clear();
				int __arrayLengthInBuffer = reader.ReadInt16();
				for (int jArrayIndex = 0; jArrayIndex < __arrayLengthInBuffer; jArrayIndex++)
					nParams.Add(reader.ReadInt32());
			}
		}
	}

	public class S2C_ADD_TRIGGER_OBJ: S2C_HEADER
	{
		public uint uID;
		public byte uType;
		public int nX;
		public int nY;
		public int nZ;
		public ulong uTriggerTid;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			uID = reader.ReadUInt32();
			uType = reader.ReadByte();
			nX = reader.ReadInt32();
			nY = reader.ReadInt32();
			nZ = reader.ReadInt32();
			uTriggerTid = reader.ReadUInt64();
		}
	}

	public class S2C_SYNC_HERO_LEAVE: S2C_HEADER
	{
		public uint heroID;
		public int leaveTime;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			heroID = reader.ReadUInt32();
			leaveTime = reader.ReadInt32();
		}
	}

	public class S2C_SKILL_MODIFY: S2C_HEADER
	{
		public uint uHeroID;
		public ushort skillID;
		public ushort modifyID;
		public ushort newCDFrameTime;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			uHeroID = reader.ReadUInt32();
			skillID = reader.ReadUInt16();
			modifyID = reader.ReadUInt16();
			newCDFrameTime = reader.ReadUInt16();
		}
	}

	public class S2C_HERO_ACTION: S2C_HEADER
	{
		public uint uHeroID;
		public ushort heroGroup;
		public byte animationType;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			uHeroID = reader.ReadUInt32();
			heroGroup = reader.ReadUInt16();
			animationType = reader.ReadByte();
		}
	}

	public class S2C_EFFECT_IMMUNO: S2C_HEADER
	{
		public uint uHeroID;
		public byte uType;
		public uint uParam;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			uHeroID = reader.ReadUInt32();
			uType = reader.ReadByte();
			uParam = reader.ReadUInt32();
		}
	}

	public class S2C_ZS_TO_PLAYER: S2C_HEADER
	{
		public byte[] data = null;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			data = KProtoBuf.ReadByteArray(reader, 0);
		}
	}

	public class S2C_SWITCH_ZS_RESPOND: S2C_HEADER
	{
		public ulong roleID;
		public int nResult;
		public string szGateWayIP = "";
		public ushort uGateWayPort;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			roleID = reader.ReadUInt64();
			nResult = reader.ReadInt32();
			szGateWayIP = KProtoBuf.ReadString(reader, 64);
			uGateWayPort = reader.ReadUInt16();
		}
	}

	public class S2C_SWITCH_GC_RESPOND: S2C_HEADER
	{
		public ulong roleID;
		public int nResult;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			roleID = reader.ReadUInt64();
			nResult = reader.ReadInt32();
		}
	}

	public class S2C_UPDATE_REVENGE_PLAYER: S2C_HEADER
	{
		public ulong nRevengeID;
		public sbyte uType;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			nRevengeID = reader.ReadUInt64();
			uType = reader.ReadSByte();
		}
	}

	public class S2C_SYN_COUNTERATTACK_PLAYER: S2C_HEADER
	{
		public ulong nCounterattackID;
		public int nLeftTime;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			nCounterattackID = reader.ReadUInt64();
			nLeftTime = reader.ReadInt32();
		}
	}

	public class S2C_PVE_END: S2C_HEADER
	{
		public sbyte success;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			success = reader.ReadSByte();
		}
	}

	public class S2C_SHOW_TIPS_INFO: S2C_HEADER
	{
		public int tips_id;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			tips_id = reader.ReadInt32();
		}
	}

	public class S2C_PVE_MISSION_VALUE_CHANGE: S2C_HEADER
	{
		public uint left_milisecond;
		public int mission_id;
		public int value;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			left_milisecond = reader.ReadUInt32();
			mission_id = reader.ReadInt32();
			value = reader.ReadInt32();
		}
	}

	public class PveMissionValue: KProtoBuf
	{
		public int mission_id;
		public int value;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			mission_id = reader.ReadInt32();
			value = reader.ReadInt32();
		}
	}

	public class S2C_PVE_MISSION_VALUES: S2C_HEADER
	{
		public uint left_milisecond;
		public List<PveMissionValue> values = new List<PveMissionValue>();

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			left_milisecond = reader.ReadUInt32();
			{
				values.Clear();
				int __arrayLengthInBuffer = reader.ReadInt16();
				for (int jArrayIndex = 0; jArrayIndex < __arrayLengthInBuffer; jArrayIndex++)
					values.Add(KProtoBuf.CreateProtoBufAndUnPack<PveMissionValue>(reader));
			}
		}
	}

	public class RewardInfo: KProtoBuf
	{
		public byte type;
		public uint count;
		public ulong id;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			type = reader.ReadByte();
			count = reader.ReadUInt32();
			id = reader.ReadUInt64();
		}
	}

	public class S2C_REWARD_LIST: S2C_HEADER
	{
		public uint result;
		public List<RewardInfo> rewards = new List<RewardInfo>();

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			result = reader.ReadUInt32();
			{
				rewards.Clear();
				int __arrayLengthInBuffer = reader.ReadInt16();
				for (int jArrayIndex = 0; jArrayIndex < __arrayLengthInBuffer; jArrayIndex++)
					rewards.Add(KProtoBuf.CreateProtoBufAndUnPack<RewardInfo>(reader));
			}
		}
	}

	public class PlayerFightResult: KProtoBuf
	{
		public uint damage;
		public uint damaged;
		public ushort kill_count;
		public ushort die_count;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			damage = reader.ReadUInt32();
			damaged = reader.ReadUInt32();
			kill_count = reader.ReadUInt16();
			die_count = reader.ReadUInt16();
		}
	}

	public class S2C_FIGHT_RESULT: S2C_HEADER
	{
		public List<PlayerFightResult> player_results = new List<PlayerFightResult>();

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			{
				player_results.Clear();
				int __arrayLengthInBuffer = reader.ReadInt16();
				for (int jArrayIndex = 0; jArrayIndex < __arrayLengthInBuffer; jArrayIndex++)
					player_results.Add(KProtoBuf.CreateProtoBufAndUnPack<PlayerFightResult>(reader));
			}
		}
	}

	public class S2C_ERROR_CODE: S2C_HEADER
	{
		public ushort error;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			error = reader.ReadUInt16();
		}
	}

	public class TaskProcess: KProtoBuf
	{
		public ulong taskOID;
		public long doingValue;
		public uint subTid;
		public uint group;
		public ulong parent_id;
		public ushort finished;
		public long data1;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			taskOID = reader.ReadUInt64();
			doingValue = reader.ReadInt64();
			subTid = reader.ReadUInt32();
			group = reader.ReadUInt32();
			parent_id = reader.ReadUInt64();
			finished = reader.ReadUInt16();
			data1 = reader.ReadInt64();
		}
	}

	public class S2C_RECEIVE_TASK: S2C_HEADER
	{
		public ushort optResult;
		public ulong taskOID;
		public uint missionTID;
		public List<TaskProcess> processInfo = new List<TaskProcess>();

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			optResult = reader.ReadUInt16();
			taskOID = reader.ReadUInt64();
			missionTID = reader.ReadUInt32();
			{
				processInfo.Clear();
				int __arrayLengthInBuffer = reader.ReadInt16();
				for (int jArrayIndex = 0; jArrayIndex < __arrayLengthInBuffer; jArrayIndex++)
					processInfo.Add(KProtoBuf.CreateProtoBufAndUnPack<TaskProcess>(reader));
			}
		}
	}

	public class S2C_COMMIT_TASK: S2C_HEADER
	{
		public ushort optResult;
		public ulong taskOID;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			optResult = reader.ReadUInt16();
			taskOID = reader.ReadUInt64();
		}
	}

	public class S2C_DROP_TASK: S2C_HEADER
	{
		public ushort optResult;
		public ulong taskOID;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			optResult = reader.ReadUInt16();
			taskOID = reader.ReadUInt64();
		}
	}

	public class S2C_TASK_PROCESS: S2C_HEADER
	{
		public List<TaskProcess> processInfo = new List<TaskProcess>();

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			{
				processInfo.Clear();
				int __arrayLengthInBuffer = reader.ReadInt16();
				for (int jArrayIndex = 0; jArrayIndex < __arrayLengthInBuffer; jArrayIndex++)
					processInfo.Add(KProtoBuf.CreateProtoBufAndUnPack<TaskProcess>(reader));
			}
		}
	}

	public class KTaskStage: KProtoBuf
	{
		public ushort crrentIndex;
		public ushort crrentNum;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			crrentIndex = reader.ReadUInt16();
			crrentNum = reader.ReadUInt16();
		}
	}

	public class KTaskContinueList: KProtoBuf
	{
		public ulong taskOID;
		public uint taskID;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			taskOID = reader.ReadUInt64();
			taskID = reader.ReadUInt32();
		}
	}

	public class KSubTaskProcess: KProtoBuf
	{
		public uint tid;
		public uint param1;
		public uint param2;
		public uint param3;
		public uint param4;
		public uint param5;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			tid = reader.ReadUInt32();
			param1 = reader.ReadUInt32();
			param2 = reader.ReadUInt32();
			param3 = reader.ReadUInt32();
			param4 = reader.ReadUInt32();
			param5 = reader.ReadUInt32();
		}
	}

	public class S2C_SUBTASK_UPDATE: S2C_HEADER
	{
		public uint taskID;
		public List<KSubTaskProcess> sub_task_process = new List<KSubTaskProcess>();

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			taskID = reader.ReadUInt32();
			{
				sub_task_process.Clear();
				int __arrayLengthInBuffer = reader.ReadInt16();
				for (int jArrayIndex = 0; jArrayIndex < __arrayLengthInBuffer; jArrayIndex++)
					sub_task_process.Add(KProtoBuf.CreateProtoBufAndUnPack<KSubTaskProcess>(reader));
			}
		}
	}

	public class S2C_COMMIT_STAGE: S2C_HEADER
	{
		public uint taskID;
		public ushort complted;
		public ushort flag_type;
		public List<KTaskStage> taskStage = new List<KTaskStage>();

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			taskID = reader.ReadUInt32();
			complted = reader.ReadUInt16();
			flag_type = reader.ReadUInt16();
			{
				taskStage.Clear();
				int __arrayLengthInBuffer = reader.ReadInt16();
				for (int jArrayIndex = 0; jArrayIndex < __arrayLengthInBuffer; jArrayIndex++)
					taskStage.Add(KProtoBuf.CreateProtoBufAndUnPack<KTaskStage>(reader));
			}
		}
	}

	public class S2C_SEND_TASKlIST: S2C_HEADER
	{
		public List<KTaskContinueList> taskContinueList = new List<KTaskContinueList>();

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			{
				taskContinueList.Clear();
				int __arrayLengthInBuffer = reader.ReadInt16();
				for (int jArrayIndex = 0; jArrayIndex < __arrayLengthInBuffer; jArrayIndex++)
					taskContinueList.Add(KProtoBuf.CreateProtoBufAndUnPack<KTaskContinueList>(reader));
			}
		}
	}

	public class S2C_SEND_NPC_MISSION: S2C_HEADER
	{
		public uint npcID;
		public List<uint> missionID = new List<uint>();

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			npcID = reader.ReadUInt32();
			{
				missionID.Clear();
				int __arrayLengthInBuffer = reader.ReadInt16();
				for (int jArrayIndex = 0; jArrayIndex < __arrayLengthInBuffer; jArrayIndex++)
					missionID.Add(reader.ReadUInt32());
			}
		}
	}

	public class S2C_TASK_FINISHLIST: S2C_HEADER
	{
		public List<uint> missionID = new List<uint>();

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			{
				missionID.Clear();
				int __arrayLengthInBuffer = reader.ReadInt16();
				for (int jArrayIndex = 0; jArrayIndex < __arrayLengthInBuffer; jArrayIndex++)
					missionID.Add(reader.ReadUInt32());
			}
		}
	}

	public class S2C_TASK_FAILLIST: S2C_HEADER
	{
		public List<ulong> taskOID = new List<ulong>();

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			{
				taskOID.Clear();
				int __arrayLengthInBuffer = reader.ReadInt16();
				for (int jArrayIndex = 0; jArrayIndex < __arrayLengthInBuffer; jArrayIndex++)
					taskOID.Add(reader.ReadUInt64());
			}
		}
	}

	public class S2C_TASK_FAILNOTIFY: S2C_HEADER
	{
		public List<ulong> taskOID = new List<ulong>();

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			{
				taskOID.Clear();
				int __arrayLengthInBuffer = reader.ReadInt16();
				for (int jArrayIndex = 0; jArrayIndex < __arrayLengthInBuffer; jArrayIndex++)
					taskOID.Add(reader.ReadUInt64());
			}
		}
	}

	public class S2C_TASK_STATENOTIFY: S2C_HEADER
	{
		public ulong taskOID;
		public uint task_state;
		public uint group_id;
		public uint group_state;
		public uint node_id;
		public uint node_state;
		public ulong rootOID;
		public uint root_state;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			taskOID = reader.ReadUInt64();
			task_state = reader.ReadUInt32();
			group_id = reader.ReadUInt32();
			group_state = reader.ReadUInt32();
			node_id = reader.ReadUInt32();
			node_state = reader.ReadUInt32();
			rootOID = reader.ReadUInt64();
			root_state = reader.ReadUInt32();
		}
	}

	public class ModelOfMember: KProtoBuf
	{
		public ulong modelID;
		public int level;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			modelID = reader.ReadUInt64();
			level = reader.ReadInt32();
		}
	}

	public class ShipOfMember: KProtoBuf
	{
		public ulong keelID;
		public int level;
		public ModelOfMember[] modelData = new ModelOfMember[5]{new ModelOfMember(),new ModelOfMember(),new ModelOfMember(),new ModelOfMember(),new ModelOfMember()};

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			keelID = reader.ReadUInt64();
			level = reader.ReadInt32();
			for (int jArrayIndex = 0; jArrayIndex < modelData.Length; jArrayIndex++)
				modelData[jArrayIndex] = KProtoBuf.CreateProtoBufAndUnPack<ModelOfMember>(reader);
		}
	}

	public class TeamMember: KProtoBuf
	{
		public ulong id;
		public int tempid;
		public string name = "";
		public int dan_level;
		public short level;
		public sbyte isOnline;
		public ulong jointime;
		public ushort position;
		public ushort alive;
		public ulong mapid;
		public int areaid;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			id = reader.ReadUInt64();
			tempid = reader.ReadInt32();
			name = KProtoBuf.ReadString(reader, 64);
			dan_level = reader.ReadInt32();
			level = reader.ReadInt16();
			isOnline = reader.ReadSByte();
			jointime = reader.ReadUInt64();
			position = reader.ReadUInt16();
			alive = reader.ReadUInt16();
			mapid = reader.ReadUInt64();
			areaid = reader.ReadInt32();
		}
	}

	public class TeamMemberBattleInfo: KProtoBuf
	{
		public ulong id;
		public string name = "";
		public ulong blood;
		public ulong bloodMax;
		public ulong defense;
		public ulong defenseMax;
		public sbyte isAlive;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			id = reader.ReadUInt64();
			name = KProtoBuf.ReadString(reader, 64);
			blood = reader.ReadUInt64();
			bloodMax = reader.ReadUInt64();
			defense = reader.ReadUInt64();
			defenseMax = reader.ReadUInt64();
			isAlive = reader.ReadSByte();
		}
	}

	public class S2C_NOTIFY_MEMBERBATTLEINFO_LIST: S2C_HEADER
	{
		public TeamMemberBattleInfo members = new TeamMemberBattleInfo();

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			members = KProtoBuf.CreateProtoBufAndUnPack<TeamMemberBattleInfo>(reader);
		}
	}

	public class S2C_TEAM_MEMBER_LIST: S2C_HEADER
	{
		public List<TeamMember> members = new List<TeamMember>();

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			{
				members.Clear();
				int __arrayLengthInBuffer = reader.ReadInt16();
				for (int jArrayIndex = 0; jArrayIndex < __arrayLengthInBuffer; jArrayIndex++)
					members.Add(KProtoBuf.CreateProtoBufAndUnPack<TeamMember>(reader));
			}
		}
	}

	public class S2C_TEAM_MEMBER_ADDED: S2C_HEADER
	{
		public string teamid = "";
		public TeamMember members = new TeamMember();

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			teamid = KProtoBuf.ReadString(reader, 128);
			members = KProtoBuf.CreateProtoBufAndUnPack<TeamMember>(reader);
		}
	}

	public class S2C_TEAM_MEMBER_LEAVE: S2C_HEADER
	{
		public string teamid = "";
		public ulong id;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			teamid = KProtoBuf.ReadString(reader, 128);
			id = reader.ReadUInt64();
		}
	}

	public class S2C_TEAM_MEMBER_CHANGE: S2C_HEADER
	{
		public TeamMember members = new TeamMember();

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			members = KProtoBuf.CreateProtoBufAndUnPack<TeamMember>(reader);
		}
	}

	public class S2C_TEAM_INVITE: S2C_HEADER
	{
		public ulong id;
		public string name = "";

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			id = reader.ReadUInt64();
			name = KProtoBuf.ReadString(reader, 64);
		}
	}

	public class S2C_TEAM_INVITE_REPLY: S2C_HEADER
	{
		public ulong id;
		public string name = "";
		public sbyte accept;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			id = reader.ReadUInt64();
			name = KProtoBuf.ReadString(reader, 64);
			accept = reader.ReadSByte();
		}
	}

	public class S2C_TEAM_INVITE_ERROR: S2C_HEADER
	{
		public sbyte error;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			error = reader.ReadSByte();
		}
	}

	public class S2C_TEAM_INVITE_REPLY_ERROR: S2C_HEADER
	{
		public sbyte error;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			error = reader.ReadSByte();
		}
	}

	public class S2C_TEAM_DISSOVE: S2C_HEADER
	{
		public string teamid = "";

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			teamid = KProtoBuf.ReadString(reader, 128);
		}
	}

	public class S2C_TEAM_CREATE: S2C_HEADER
	{
		public sbyte result;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			result = reader.ReadSByte();
		}
	}

	public class S2C_TEAM_BECHAIRMAN: S2C_HEADER
	{
		public ulong chair_man_id;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			chair_man_id = reader.ReadUInt64();
		}
	}

	public class SettlementModuleInfo: KProtoBuf
	{
		public ulong id;
		public uint expAdd;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			id = reader.ReadUInt64();
			expAdd = reader.ReadUInt32();
		}
	}

	public class SettlementRewardInfo: KProtoBuf
	{
		public uint id;
		public byte type;
		public uint count;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			id = reader.ReadUInt32();
			type = reader.ReadByte();
			count = reader.ReadUInt32();
		}
	}

	public class SettlementCombatInfo: KProtoBuf
	{
		public ulong id;
		public int dps;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			id = reader.ReadUInt64();
			dps = reader.ReadInt32();
		}
	}

	public class Settlement: KProtoBuf
	{
		public int shipExpAdd;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			shipExpAdd = reader.ReadInt32();
		}
	}

	public class MasteryInfo: KProtoBuf
	{
		public int mastery_id;
		public sbyte is_lock;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			mastery_id = reader.ReadInt32();
			is_lock = reader.ReadSByte();
		}
	}

	public class S2C_JOB_INFO: S2C_HEADER
	{
		public short job;
		public short level;
		public List<MasteryInfo> master_info_list = new List<MasteryInfo>();

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			job = reader.ReadInt16();
			level = reader.ReadInt16();
			{
				master_info_list.Clear();
				int __arrayLengthInBuffer = reader.ReadInt16();
				for (int jArrayIndex = 0; jArrayIndex < __arrayLengthInBuffer; jArrayIndex++)
					master_info_list.Add(KProtoBuf.CreateProtoBufAndUnPack<MasteryInfo>(reader));
			}
		}
	}

	public class S2C_SHIP_JUMP_RESPONSE: S2C_HEADER
	{
		public uint heroID;
		public byte request_jump;
		public uint index;
		public List<Vectors> road_point_list = new List<Vectors>();

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			heroID = reader.ReadUInt32();
			request_jump = reader.ReadByte();
			index = reader.ReadUInt32();
			{
				road_point_list.Clear();
				int __arrayLengthInBuffer = reader.ReadInt16();
				for (int jArrayIndex = 0; jArrayIndex < __arrayLengthInBuffer; jArrayIndex++)
					road_point_list.Add(KProtoBuf.CreateProtoBufAndUnPack<Vectors>(reader));
			}
		}
	}

	public class S2C_JUMP_MAP_CONFIRM: S2C_HEADER
	{
		public ulong roleID;
		public uint index;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			roleID = reader.ReadUInt64();
			index = reader.ReadUInt32();
		}
	}

	public class MoneyData: KProtoBuf
	{
		public ushort money_type;
		public uint money_num;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			money_type = reader.ReadUInt16();
			money_num = reader.ReadUInt32();
		}
	}

	public class S2C_MONEY_INFOS: S2C_HEADER
	{
		public List<MoneyData> money_infos = new List<MoneyData>();

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			{
				money_infos.Clear();
				int __arrayLengthInBuffer = reader.ReadInt16();
				for (int jArrayIndex = 0; jArrayIndex < __arrayLengthInBuffer; jArrayIndex++)
					money_infos.Add(KProtoBuf.CreateProtoBufAndUnPack<MoneyData>(reader));
			}
		}
	}

	public class S2C_MONEY_CHANGE: S2C_HEADER
	{
		public MoneyData money_data = new MoneyData();

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			money_data = KProtoBuf.CreateProtoBufAndUnPack<MoneyData>(reader);
		}
	}

	public class S2C_SELL_BACK: S2C_HEADER
	{
		public ushort opt_result;
		public uint uid;
		public int left_num;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			opt_result = reader.ReadUInt16();
			uid = reader.ReadUInt32();
			left_num = reader.ReadInt32();
		}
	}

	public class FriendData: KProtoBuf
	{
		public ulong uid;
		public uint tid;
		public byte status;
		public byte flag;
		public string name = "";
		public uint level;
		public uint dan_level;
		public ulong add_time;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			uid = reader.ReadUInt64();
			tid = reader.ReadUInt32();
			status = reader.ReadByte();
			flag = reader.ReadByte();
			name = KProtoBuf.ReadString(reader, 64);
			level = reader.ReadUInt32();
			dan_level = reader.ReadUInt32();
			add_time = reader.ReadUInt64();
		}
	}

	public class S2C_RESET_FRIEND: S2C_HEADER
	{
		public sbyte status;
		public int friendMaxCount;
		public int blackMaxCount;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			status = reader.ReadSByte();
			friendMaxCount = reader.ReadInt32();
			blackMaxCount = reader.ReadInt32();
		}
	}

	public class S2C_SYNC_FRIEND: S2C_HEADER
	{
		public FriendData friend_syc = new FriendData();

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			friend_syc = KProtoBuf.CreateProtoBufAndUnPack<FriendData>(reader);
		}
	}

	public class S2C_SET_STATUS_BACK: S2C_HEADER
	{
		public sbyte code;
		public sbyte status;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			code = reader.ReadSByte();
			status = reader.ReadSByte();
		}
	}

	public class S2C_ADD_FRIEND_BACK: S2C_HEADER
	{
		public ushort code;
		public FriendData friend_add = new FriendData();

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			code = reader.ReadUInt16();
			friend_add = KProtoBuf.CreateProtoBufAndUnPack<FriendData>(reader);
		}
	}

	public class S2C_DEL_FRIEND_BACK: S2C_HEADER
	{
		public ushort code;
		public ulong uid;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			code = reader.ReadUInt16();
			uid = reader.ReadUInt64();
		}
	}

	public class S2C_ADD_BLACK_BACK: S2C_HEADER
	{
		public ushort code;
		public FriendData friend_black = new FriendData();

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			code = reader.ReadUInt16();
			friend_black = KProtoBuf.CreateProtoBufAndUnPack<FriendData>(reader);
		}
	}

	public class S2C_DEL_BLACK_BACK: S2C_HEADER
	{
		public ushort code;
		public ulong uid;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			code = reader.ReadUInt16();
			uid = reader.ReadUInt64();
		}
	}

	public class FriendReqData: KProtoBuf
	{
		public ulong id;
		public ulong uid;
		public string name = "";
		public ushort level;
		public ulong addTime;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			id = reader.ReadUInt64();
			uid = reader.ReadUInt64();
			name = KProtoBuf.ReadString(reader, 64);
			level = reader.ReadUInt16();
			addTime = reader.ReadUInt64();
		}
	}

	public class S2C_SYNC_FRIEND_INVITE: S2C_HEADER
	{
		public FriendReqData data = new FriendReqData();

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			data = KProtoBuf.CreateProtoBufAndUnPack<FriendReqData>(reader);
		}
	}

	public class S2C_ADD_FRIEND_INVITE: S2C_HEADER
	{
		public FriendReqData data = new FriendReqData();

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			data = KProtoBuf.CreateProtoBufAndUnPack<FriendReqData>(reader);
		}
	}

	public class S2C_SYNC_FRIENDLIST: S2C_HEADER
	{
		public List<FriendData> friendlist = new List<FriendData>();

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			{
				friendlist.Clear();
				int __arrayLengthInBuffer = reader.ReadInt16();
				for (int jArrayIndex = 0; jArrayIndex < __arrayLengthInBuffer; jArrayIndex++)
					friendlist.Add(KProtoBuf.CreateProtoBufAndUnPack<FriendData>(reader));
			}
		}
	}

	public class S2C_SYNC_TOGETHERLIST: S2C_HEADER
	{
		public List<FriendData> togetherlist = new List<FriendData>();

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			{
				togetherlist.Clear();
				int __arrayLengthInBuffer = reader.ReadInt16();
				for (int jArrayIndex = 0; jArrayIndex < __arrayLengthInBuffer; jArrayIndex++)
					togetherlist.Add(KProtoBuf.CreateProtoBufAndUnPack<FriendData>(reader));
			}
		}
	}

	public class FoundryMember: KProtoBuf
	{
		public int itemTid;
		public int startTime;
		public int endTime;
		public int spendTime;
		public sbyte is_finish;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			itemTid = reader.ReadInt32();
			startTime = reader.ReadInt32();
			endTime = reader.ReadInt32();
			spendTime = reader.ReadInt32();
			is_finish = reader.ReadSByte();
		}
	}

	public class S2C_FOUNDRY_GETINFO_BACK: S2C_HEADER
	{
		public List<FoundryMember> member = new List<FoundryMember>();

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			{
				member.Clear();
				int __arrayLengthInBuffer = reader.ReadInt16();
				for (int jArrayIndex = 0; jArrayIndex < __arrayLengthInBuffer; jArrayIndex++)
					member.Add(KProtoBuf.CreateProtoBufAndUnPack<FoundryMember>(reader));
			}
		}
	}

	public class S2C_FOUNDRY_BUILD_BACK: S2C_HEADER
	{
		public FoundryMember member = new FoundryMember();

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			member = KProtoBuf.CreateProtoBufAndUnPack<FoundryMember>(reader);
		}
	}

	public class S2C_FOUNDRY_SPEED_BACK: S2C_HEADER
	{
		public int itemTid;
		public sbyte code;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			itemTid = reader.ReadInt32();
			code = reader.ReadSByte();
		}
	}

	public class S2C_FOUNDRY_CANCEL_BACK: S2C_HEADER
	{
		public int itemTid;
		public sbyte code;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			itemTid = reader.ReadInt32();
			code = reader.ReadSByte();
		}
	}

	public class S2C_FOUNDRY_RECEIVE_BACK: S2C_HEADER
	{
		public int itemTid;
		public sbyte code;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			itemTid = reader.ReadInt32();
			code = reader.ReadSByte();
		}
	}

	public class S2C_FOUNDRY_ADD_ROBOT_BACK: S2C_HEADER
	{
		public int robotMaxCount;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			robotMaxCount = reader.ReadInt32();
		}
	}

	public class S2C_DEL_FRIEND_INVITE: S2C_HEADER
	{
		public ulong id;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			id = reader.ReadUInt64();
		}
	}

	public class S2C_CHAT_MESSAGE: S2C_HEADER
	{
		public ushort channel;
		public ulong fromID;
		public string fromName = "";
		public string message = "";

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			channel = reader.ReadUInt16();
			fromID = reader.ReadUInt64();
			fromName = KProtoBuf.ReadString(reader, 64);
			message = KProtoBuf.ReadString(reader, 1024);
		}
	}

	public class S2C_POPUP_WINDOW: S2C_HEADER
	{
		public uint popup_type;
		public uint target;
		public uint event_id;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			popup_type = reader.ReadUInt32();
			target = reader.ReadUInt32();
			event_id = reader.ReadUInt32();
		}
	}

	public class S2C_ATTR_CHANGE: S2C_HEADER
	{
		public uint hero_id;
		public List<ATTR> attr_list = new List<ATTR>();

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			hero_id = reader.ReadUInt32();
			{
				attr_list.Clear();
				int __arrayLengthInBuffer = reader.ReadInt16();
				for (int jArrayIndex = 0; jArrayIndex < __arrayLengthInBuffer; jArrayIndex++)
					attr_list.Add(KProtoBuf.CreateProtoBufAndUnPack<ATTR>(reader));
			}
		}
	}

	public class KMailItem: KProtoBuf
	{
		public uint id;
		public uint num;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			id = reader.ReadUInt32();
			num = reader.ReadUInt32();
		}
	}

	public class MailParam: KProtoBuf
	{
		public int type;
		public int target_type;
		public byte is_string_param;
		public ulong format_id;
		public string format_string = "";

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			type = reader.ReadInt32();
			target_type = reader.ReadInt32();
			is_string_param = reader.ReadByte();
			format_id = reader.ReadUInt64();
			format_string = KProtoBuf.ReadString(reader, 64);
		}
	}

	public class MailSimpleData: KProtoBuf
	{
		public string id = "";
		public uint tid;
		public byte index;
		public byte max_index;
		public long expireTime;
		public long recvTime;
		public byte readed;
		public byte got;
		public byte is_new;
		public byte starred;
		public byte has_accessory;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			id = KProtoBuf.ReadString(reader, 32);
			tid = reader.ReadUInt32();
			index = reader.ReadByte();
			max_index = reader.ReadByte();
			expireTime = reader.ReadInt64();
			recvTime = reader.ReadInt64();
			readed = reader.ReadByte();
			got = reader.ReadByte();
			is_new = reader.ReadByte();
			starred = reader.ReadByte();
			has_accessory = reader.ReadByte();
		}
	}

	public class S2C_MAIL_PARAM_LIST: S2C_HEADER
	{
		public string id = "";
		public List<MailParam> param_list = new List<MailParam>();

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			id = KProtoBuf.ReadString(reader, 32);
			{
				param_list.Clear();
				int __arrayLengthInBuffer = reader.ReadInt16();
				for (int jArrayIndex = 0; jArrayIndex < __arrayLengthInBuffer; jArrayIndex++)
					param_list.Add(KProtoBuf.CreateProtoBufAndUnPack<MailParam>(reader));
			}
		}
	}

	public class S2C_MAIL_GET_LIST: S2C_HEADER
	{
		public uint errCode;
		public uint totalMailsCount;
		public List<MailSimpleData> mailList = new List<MailSimpleData>();

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			errCode = reader.ReadUInt32();
			totalMailsCount = reader.ReadUInt32();
			{
				mailList.Clear();
				int __arrayLengthInBuffer = reader.ReadInt16();
				for (int jArrayIndex = 0; jArrayIndex < __arrayLengthInBuffer; jArrayIndex++)
					mailList.Add(KProtoBuf.CreateProtoBufAndUnPack<MailSimpleData>(reader));
			}
		}
	}

	public class S2C_MAIL_DETAIL_INFO: S2C_HEADER
	{
		public uint errCode;
		public string id = "";
		public List<KMailItem> items = new List<KMailItem>();

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			errCode = reader.ReadUInt32();
			id = KProtoBuf.ReadString(reader, 32);
			{
				items.Clear();
				int __arrayLengthInBuffer = reader.ReadInt16();
				for (int jArrayIndex = 0; jArrayIndex < __arrayLengthInBuffer; jArrayIndex++)
					items.Add(KProtoBuf.CreateProtoBufAndUnPack<KMailItem>(reader));
			}
		}
	}

	public class S2C_MAIL_GET_ACCESSORY: S2C_HEADER
	{
		public uint errCode;
		public string id = "";

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			errCode = reader.ReadUInt32();
			id = KProtoBuf.ReadString(reader, 32);
		}
	}

	public class MailDelRet: KProtoBuf
	{
		public uint errCode;
		public string ids = "";

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			errCode = reader.ReadUInt32();
			ids = KProtoBuf.ReadString(reader, 32);
		}
	}

	public class S2C_MAIL_DELETE: S2C_HEADER
	{
		public List<MailDelRet> mails = new List<MailDelRet>();

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			{
				mails.Clear();
				int __arrayLengthInBuffer = reader.ReadInt16();
				for (int jArrayIndex = 0; jArrayIndex < __arrayLengthInBuffer; jArrayIndex++)
					mails.Add(KProtoBuf.CreateProtoBufAndUnPack<MailDelRet>(reader));
			}
		}
	}

	public class S2C_MAIL_TOTAL_COUNT: S2C_HEADER
	{
		public uint count;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			count = reader.ReadUInt32();
		}
	}

	public class S2C_MAIL_NEW: S2C_HEADER
	{
		public MailSimpleData new_mail = new MailSimpleData();

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			new_mail = KProtoBuf.CreateProtoBufAndUnPack<MailSimpleData>(reader);
		}
	}

	public class S2C_MAIL_STARRED: S2C_HEADER
	{
		public string id = "";
		public byte starred;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			id = KProtoBuf.ReadString(reader, 32);
			starred = reader.ReadByte();
		}
	}

	public class S2C_BROCAST_SHIP_STATE: S2C_HEADER
	{
		public int state;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			state = reader.ReadInt32();
		}
	}

	public class S2C_BROCAST_TRIGGER_STATE: S2C_HEADER
	{
		public uint npcUID;
		public uint state;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			npcUID = reader.ReadUInt32();
			state = reader.ReadUInt32();
		}
	}

	public class SharedNewHero: KProtoBuf
	{
		public uint id;
		public uint teamID;
		public byte teamMode;
		public byte campID;
		public byte pkMode;
		public uint ownerHeroID;
		public ulong ownerPlayerID;
		public uint guildID;
		public uint massID;
		public uint uLevel;
		public short faceDirX;
		public short faceDirY;
		public short faceDirZ;
		public sbyte invincible;
		public sbyte visible;
		public int posX;
		public int posY;
		public int posZ;
		public int desX;
		public int desY;
		public int desZ;
		public uint templateID;
		public ushort moveSpeed;
		public int moveState;
		public ulong startFrame;
		public byte bNewHero;
		public long HP;
		public long EnergyPower;
		public long SuperMagnetic;
		public long MaxHP;
		public long MaxEnergyPower;
		public long MaxSuperMagnetic;
		public long mana_shield_value;
		public long mana_shield_value_max;
		public long defense_shield_value;
		public long defense_shield_value_max;
		public long shield_value;
		public long shield_value_max;
		public uint enterPose;
		public ushort heroGroup;
		public short createIndex;
		public byte animationType;
		public uint item_tid;
		public string szPlayerName = "";
		public sbyte is_fighting;
		public uint turning_accelerated_speed;
		public uint accelerate_speed;
		public uint accelerate_speed_fighting;
		public uint max_speed;
		public uint max_speed_fighting;
		public uint turning_max_speed;
		public uint turning_max_speed_fighting;
		public uint up_down_accelerate_speed;
		public uint up_down_accelerate_speed_fighting;
		public uint up_dowm_max_speed;
		public uint up_down_max_speed_fighting;
		public uint move_speed_ibmr;
		public uint move_speed_ibmr_fighting;
		public int ViewSight;
		public List<KHeroBuffList> bufferList = new List<KHeroBuffList>();

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			id = reader.ReadUInt32();
			teamID = reader.ReadUInt32();
			teamMode = reader.ReadByte();
			campID = reader.ReadByte();
			pkMode = reader.ReadByte();
			ownerHeroID = reader.ReadUInt32();
			ownerPlayerID = reader.ReadUInt64();
			guildID = reader.ReadUInt32();
			massID = reader.ReadUInt32();
			uLevel = reader.ReadUInt32();
			faceDirX = reader.ReadInt16();
			faceDirY = reader.ReadInt16();
			faceDirZ = reader.ReadInt16();
			invincible = reader.ReadSByte();
			visible = reader.ReadSByte();
			posX = reader.ReadInt32();
			posY = reader.ReadInt32();
			posZ = reader.ReadInt32();
			desX = reader.ReadInt32();
			desY = reader.ReadInt32();
			desZ = reader.ReadInt32();
			templateID = reader.ReadUInt32();
			moveSpeed = reader.ReadUInt16();
			moveState = reader.ReadInt32();
			startFrame = reader.ReadUInt64();
			bNewHero = reader.ReadByte();
			HP = reader.ReadInt64();
			EnergyPower = reader.ReadInt64();
			SuperMagnetic = reader.ReadInt64();
			MaxHP = reader.ReadInt64();
			MaxEnergyPower = reader.ReadInt64();
			MaxSuperMagnetic = reader.ReadInt64();
			mana_shield_value = reader.ReadInt64();
			mana_shield_value_max = reader.ReadInt64();
			defense_shield_value = reader.ReadInt64();
			defense_shield_value_max = reader.ReadInt64();
			shield_value = reader.ReadInt64();
			shield_value_max = reader.ReadInt64();
			enterPose = reader.ReadUInt32();
			heroGroup = reader.ReadUInt16();
			createIndex = reader.ReadInt16();
			animationType = reader.ReadByte();
			item_tid = reader.ReadUInt32();
			szPlayerName = KProtoBuf.ReadString(reader, 64);
			is_fighting = reader.ReadSByte();
			turning_accelerated_speed = reader.ReadUInt32();
			accelerate_speed = reader.ReadUInt32();
			accelerate_speed_fighting = reader.ReadUInt32();
			max_speed = reader.ReadUInt32();
			max_speed_fighting = reader.ReadUInt32();
			turning_max_speed = reader.ReadUInt32();
			turning_max_speed_fighting = reader.ReadUInt32();
			up_down_accelerate_speed = reader.ReadUInt32();
			up_down_accelerate_speed_fighting = reader.ReadUInt32();
			up_dowm_max_speed = reader.ReadUInt32();
			up_down_max_speed_fighting = reader.ReadUInt32();
			move_speed_ibmr = reader.ReadUInt32();
			move_speed_ibmr_fighting = reader.ReadUInt32();
			ViewSight = reader.ReadInt32();
			{
				bufferList.Clear();
				int __arrayLengthInBuffer = reader.ReadInt16();
				for (int jArrayIndex = 0; jArrayIndex < __arrayLengthInBuffer; jArrayIndex++)
					bufferList.Add(KProtoBuf.CreateProtoBufAndUnPack<KHeroBuffList>(reader));
			}
		}
	}

	public class S2C_SHARE_SCENE_HERO: S2C_HEADER
	{
		public byte shared;
		public SharedNewHero heroinfo = new SharedNewHero();

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			shared = reader.ReadByte();
			heroinfo = KProtoBuf.CreateProtoBufAndUnPack<SharedNewHero>(reader);
		}
	}

	public class S2C_SHARED_SHIP_MOVE: KProtoBuf
	{
		public uint heroID;
		public int posX;
		public int posY;
		public int posZ;
		public int targetX;
		public int targetY;
		public int targetZ;
		public ushort turnX;
		public ushort turnY;
		public ushort turnZ;
		public uint state;
		public ulong timeStamp;
		public int velocity;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			heroID = reader.ReadUInt32();
			posX = reader.ReadInt32();
			posY = reader.ReadInt32();
			posZ = reader.ReadInt32();
			targetX = reader.ReadInt32();
			targetY = reader.ReadInt32();
			targetZ = reader.ReadInt32();
			turnX = reader.ReadUInt16();
			turnY = reader.ReadUInt16();
			turnZ = reader.ReadUInt16();
			state = reader.ReadUInt32();
			timeStamp = reader.ReadUInt64();
			velocity = reader.ReadInt32();
		}
	}

	public class S2C_SHARE_MOVE_HERO: S2C_HEADER
	{
		public byte shared;
		public S2C_SHARED_SHIP_MOVE moveinfo = new S2C_SHARED_SHIP_MOVE();

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			shared = reader.ReadByte();
			moveinfo = KProtoBuf.CreateProtoBufAndUnPack<S2C_SHARED_SHIP_MOVE>(reader);
		}
	}

	public class S2C_SHARE_REMOVE_HERO: S2C_HEADER
	{
		public byte shared;
		public ulong objid;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			shared = reader.ReadByte();
			objid = reader.ReadUInt64();
		}
	}

	public class S2C_TEAM_ORGANIZE_RESULT: S2C_HEADER
	{
		public sbyte opt_result;
		public int err_code;
		public List<string> err_name = new List<string>();

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			opt_result = reader.ReadSByte();
			err_code = reader.ReadInt32();
			{
				err_name.Clear();
				int __arrayLengthInBuffer = reader.ReadInt16();
				for (int jArrayIndex = 0; jArrayIndex < __arrayLengthInBuffer; jArrayIndex++)
					err_name.Add(KProtoBuf.ReadString(reader, 128));
			}
		}
	}

	public class S2C_TEAM_ORGANIZE_OPT: S2C_HEADER
	{
		public int opt_notice;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			opt_notice = reader.ReadInt32();
		}
	}

	public class S2C_TEAM_MEMBER_FORCELEAVE: S2C_HEADER
	{
		public byte opt_result;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			opt_result = reader.ReadByte();
		}
	}

	public class S2C_BROCAST_TRIGGER_ENERGY: S2C_HEADER
	{
		public uint npcUID;
		public uint energy;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			npcUID = reader.ReadUInt32();
			energy = reader.ReadUInt32();
		}
	}

	public class S2C_BROCAST_DIE_TYPE: S2C_HEADER
	{
		public uint npcUID;
		public uint dieType;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			npcUID = reader.ReadUInt32();
			dieType = reader.ReadUInt32();
		}
	}

	public class S2C_BATTLE_STATUS_MSG: KProtoBuf
	{
		public ulong battle_id;
		public int table_id;
		public int task_id;
		public short status;
		public uint cd_time_left;
		public short out_of_range;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			battle_id = reader.ReadUInt64();
			table_id = reader.ReadInt32();
			task_id = reader.ReadInt32();
			status = reader.ReadInt16();
			cd_time_left = reader.ReadUInt32();
			out_of_range = reader.ReadInt16();
		}
	}

	public class S2C_BATTLE_STATUS: S2C_HEADER
	{
		public List<S2C_BATTLE_STATUS_MSG> status = new List<S2C_BATTLE_STATUS_MSG>();

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			{
				status.Clear();
				int __arrayLengthInBuffer = reader.ReadInt16();
				for (int jArrayIndex = 0; jArrayIndex < __arrayLengthInBuffer; jArrayIndex++)
					status.Add(KProtoBuf.CreateProtoBufAndUnPack<S2C_BATTLE_STATUS_MSG>(reader));
			}
		}
	}

	public class S2C_TEMPORARY_CHEST: S2C_HEADER
	{
		public ulong chest_uid;
		public uint chest_tid;
		public uint modelId;
		public uint quality;
		public float posX;
		public float posY;
		public float posZ;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			chest_uid = reader.ReadUInt64();
			chest_tid = reader.ReadUInt32();
			modelId = reader.ReadUInt32();
			quality = reader.ReadUInt32();
			posX = reader.ReadSingle();
			posY = reader.ReadSingle();
			posZ = reader.ReadSingle();
		}
	}

	public class S2C_CHEST_GET_RESULT: S2C_HEADER
	{
		public ulong chest_uid;
		public uint error_code;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			chest_uid = reader.ReadUInt64();
			error_code = reader.ReadUInt32();
		}
	}

	public class SceneEventItem: KProtoBuf
	{
		public uint world_task_id;
		public uint progress;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			world_task_id = reader.ReadUInt32();
			progress = reader.ReadUInt32();
		}
	}

	public class S2C_SCENE_EVENT_STATE: S2C_HEADER
	{
		public uint tid;
		public uint cur_state;
		public List<SceneEventItem> items = new List<SceneEventItem>();

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			tid = reader.ReadUInt32();
			cur_state = reader.ReadUInt32();
			{
				items.Clear();
				int __arrayLengthInBuffer = reader.ReadInt16();
				for (int jArrayIndex = 0; jArrayIndex < __arrayLengthInBuffer; jArrayIndex++)
					items.Add(KProtoBuf.CreateProtoBufAndUnPack<SceneEventItem>(reader));
			}
		}
	}

	public class S2C_SCENE_EVENT_MISSION_PROGRESS_CHANGED: S2C_HEADER
	{
		public uint tid;
		public uint world_task_id;
		public uint progress;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			tid = reader.ReadUInt32();
			world_task_id = reader.ReadUInt32();
			progress = reader.ReadUInt32();
		}
	}

	public class S2C_ITEM_BEGIN_SYNC: S2C_HEADER
	{

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
		}
	}

	public class S2C_ITEM_END_SYNC: S2C_HEADER
	{

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
		}
	}

	public class S2C_ITEM_SYNC: S2C_HEADER
	{
		public ulong uid;
		public uint tid;
		public long count;
		public ulong parent;
		public ushort pos;
		public ulong reference;
		public ulong create_time;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			uid = reader.ReadUInt64();
			tid = reader.ReadUInt32();
			count = reader.ReadInt64();
			parent = reader.ReadUInt64();
			pos = reader.ReadUInt16();
			reference = reader.ReadUInt64();
			create_time = reader.ReadUInt64();
		}
	}

	public class S2C_CONTAINER_SYNC: S2C_HEADER
	{
		public ulong uid;
		public uint cur_capacity;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			uid = reader.ReadUInt64();
			cur_capacity = reader.ReadUInt32();
		}
	}

	public class S2C_ITEM_ATTR_SYNC: S2C_HEADER
	{
		public ulong uid;
		public short lv;
		public int exp;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			uid = reader.ReadUInt64();
			lv = reader.ReadInt16();
			exp = reader.ReadInt32();
		}
	}

	public class ItemOperate: KProtoBuf
	{
		public int type;
		public ulong uid;
		public uint tid;
		public long count;
		public ulong parent;
		public ushort pos;
		public uint cur_capacity;
		public ulong reference;
		public short lv;
		public int exp;
		public ulong create_time;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			type = reader.ReadInt32();
			uid = reader.ReadUInt64();
			tid = reader.ReadUInt32();
			count = reader.ReadInt64();
			parent = reader.ReadUInt64();
			pos = reader.ReadUInt16();
			cur_capacity = reader.ReadUInt32();
			reference = reader.ReadUInt64();
			lv = reader.ReadInt16();
			exp = reader.ReadInt32();
			create_time = reader.ReadUInt64();
		}
	}

	public class S2C_ITEM_OPERATE_LIST: S2C_HEADER
	{
		public int op_type;
		public ulong mark;
		public int errcode;
		public List<ItemOperate> op_list = new List<ItemOperate>();

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			op_type = reader.ReadInt32();
			mark = reader.ReadUInt64();
			errcode = reader.ReadInt32();
			{
				op_list.Clear();
				int __arrayLengthInBuffer = reader.ReadInt16();
				for (int jArrayIndex = 0; jArrayIndex < __arrayLengthInBuffer; jArrayIndex++)
					op_list.Add(KProtoBuf.CreateProtoBufAndUnPack<ItemOperate>(reader));
			}
		}
	}

	public class GeometryInfo: KProtoBuf
	{
		public int draw_type;
		public Vectors center_pos = new Vectors();
		public Vectors box_size = new Vectors();
		public MsgQuaternion rotation = new MsgQuaternion();
		public Vectors start_pos = new Vectors();
		public MsgQuaternion ship_rotation = new MsgQuaternion();
		public Vectors move_dir = new Vectors();
		public Vectors capsule_scale = new Vectors();

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			draw_type = reader.ReadInt32();
			center_pos = KProtoBuf.CreateProtoBufAndUnPack<Vectors>(reader);
			box_size = KProtoBuf.CreateProtoBufAndUnPack<Vectors>(reader);
			rotation = KProtoBuf.CreateProtoBufAndUnPack<MsgQuaternion>(reader);
			start_pos = KProtoBuf.CreateProtoBufAndUnPack<Vectors>(reader);
			ship_rotation = KProtoBuf.CreateProtoBufAndUnPack<MsgQuaternion>(reader);
			move_dir = KProtoBuf.CreateProtoBufAndUnPack<Vectors>(reader);
			capsule_scale = KProtoBuf.CreateProtoBufAndUnPack<Vectors>(reader);
		}
	}

	public class S2C_SYNC_CONTACT_GEOMETRY: S2C_HEADER
	{
		public List<GeometryInfo> geometry_list = new List<GeometryInfo>();

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			{
				geometry_list.Clear();
				int __arrayLengthInBuffer = reader.ReadInt16();
				for (int jArrayIndex = 0; jArrayIndex < __arrayLengthInBuffer; jArrayIndex++)
					geometry_list.Add(KProtoBuf.CreateProtoBufAndUnPack<GeometryInfo>(reader));
			}
		}
	}

	public class S2C_SYNC_HERO_STATE: S2C_HEADER
	{
		public short type;
		public ulong state;
		public uint heroID;
		public uint move_state;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			type = reader.ReadInt16();
			state = reader.ReadUInt64();
			heroID = reader.ReadUInt32();
			move_state = reader.ReadUInt32();
		}
	}

	public class S2C_SYNC_DROP_LIST: S2C_HEADER
	{
		public ulong hero_uid;
		public List<DropInfo> drop_list = new List<DropInfo>();

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			hero_uid = reader.ReadUInt64();
			{
				drop_list.Clear();
				int __arrayLengthInBuffer = reader.ReadInt16();
				for (int jArrayIndex = 0; jArrayIndex < __arrayLengthInBuffer; jArrayIndex++)
					drop_list.Add(KProtoBuf.CreateProtoBufAndUnPack<DropInfo>(reader));
			}
		}
	}

	public class S2C_NOTIFY_DELETE_ITEM_LOG: S2C_HEADER
	{
		public uint error_code;
		public List<string> ids = new List<string>();

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			error_code = reader.ReadUInt32();
			{
				ids.Clear();
				int __arrayLengthInBuffer = reader.ReadInt16();
				for (int jArrayIndex = 0; jArrayIndex < __arrayLengthInBuffer; jArrayIndex++)
					ids.Add(KProtoBuf.ReadString(reader, 32));
			}
		}
	}

	public class LogDisplayInfo: KProtoBuf
	{
		public string log_id = "";
		public uint item_tid;
		public long item_count;
		public ulong receive_time;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			log_id = KProtoBuf.ReadString(reader, 32);
			item_tid = reader.ReadUInt32();
			item_count = reader.ReadInt64();
			receive_time = reader.ReadUInt64();
		}
	}

	public class S2C_NOTIFY_NEW_ITEM_LOG: S2C_HEADER
	{
		public LogDisplayInfo new_log = new LogDisplayInfo();

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			new_log = KProtoBuf.CreateProtoBufAndUnPack<LogDisplayInfo>(reader);
		}
	}

	public class S2C_SYNC_ITEM_LOG: S2C_HEADER
	{
		public uint error_code;
		public List<LogDisplayInfo> log_list = new List<LogDisplayInfo>();

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			error_code = reader.ReadUInt32();
			{
				log_list.Clear();
				int __arrayLengthInBuffer = reader.ReadInt16();
				for (int jArrayIndex = 0; jArrayIndex < __arrayLengthInBuffer; jArrayIndex++)
					log_list.Add(KProtoBuf.CreateProtoBufAndUnPack<LogDisplayInfo>(reader));
			}
		}
	}

	public class S2C_ENTER_BATTLE: S2C_HEADER
	{

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
		}
	}

	public class S2C_LEAVE_BATTLE: S2C_HEADER
	{

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
		}
	}

	public class PlotTimeInfo: KProtoBuf
	{
		public uint type;
		public uint time;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			type = reader.ReadUInt32();
			time = reader.ReadUInt32();
		}
	}

	public class S2C_CHANGE_PLOT_TIME: S2C_HEADER
	{
		public List<PlotTimeInfo> time_list = new List<PlotTimeInfo>();

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			{
				time_list.Clear();
				int __arrayLengthInBuffer = reader.ReadInt16();
				for (int jArrayIndex = 0; jArrayIndex < __arrayLengthInBuffer; jArrayIndex++)
					time_list.Add(KProtoBuf.CreateProtoBufAndUnPack<PlotTimeInfo>(reader));
			}
		}
	}

	public class S2C_SYNC_BOSS_HUD: S2C_HEADER
	{

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
		}
	}

	public class S2C_CHANGE_PLOT_STATE: S2C_HEADER
	{
		public ulong template_id;
		public ulong npc_id;
		public ushort plot_state;
		public byte in_scene;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			template_id = reader.ReadUInt64();
			npc_id = reader.ReadUInt64();
			plot_state = reader.ReadUInt16();
			in_scene = reader.ReadByte();
		}
	}

	public class S2C_PLOT_MONSTER_LIST: S2C_HEADER
	{
		public ulong npc_id;
		public List<ulong> monster_uids = new List<ulong>();

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			npc_id = reader.ReadUInt64();
			{
				monster_uids.Clear();
				int __arrayLengthInBuffer = reader.ReadInt16();
				for (int jArrayIndex = 0; jArrayIndex < __arrayLengthInBuffer; jArrayIndex++)
					monster_uids.Add(reader.ReadUInt64());
			}
		}
	}

	public class FightshipItemList: KProtoBuf
	{
		public ulong uid;
		public uint tid;
		public ulong parent;
		public ushort pos;
		public short lv;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			uid = reader.ReadUInt64();
			tid = reader.ReadUInt32();
			parent = reader.ReadUInt64();
			pos = reader.ReadUInt16();
			lv = reader.ReadInt16();
		}
	}

	public class S2C_SYNC_FIGHTSHIP_VISIBLE_ITEM_LIST: S2C_HEADER
	{
		public ulong uid;
		public List<FightshipItemList> item_list = new List<FightshipItemList>();

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			uid = reader.ReadUInt64();
			{
				item_list.Clear();
				int __arrayLengthInBuffer = reader.ReadInt16();
				for (int jArrayIndex = 0; jArrayIndex < __arrayLengthInBuffer; jArrayIndex++)
					item_list.Add(KProtoBuf.CreateProtoBufAndUnPack<FightshipItemList>(reader));
			}
		}
	}

	public class S2C_SYNC_PLAYERINFO: S2C_HEADER
	{
		public ulong uid;
		public List<FightshipItemList> item_list = new List<FightshipItemList>();

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			uid = reader.ReadUInt64();
			{
				item_list.Clear();
				int __arrayLengthInBuffer = reader.ReadInt16();
				for (int jArrayIndex = 0; jArrayIndex < __arrayLengthInBuffer; jArrayIndex++)
					item_list.Add(KProtoBuf.CreateProtoBufAndUnPack<FightshipItemList>(reader));
			}
		}
	}

	public class PersonalDrop: KProtoBuf
	{
		public uint map_id;
		public ulong area_uid;
		public ulong from_hero_uid;
		public ulong chest_npc_uid;
		public uint chest_npc_tid;
		public uint drop_item_tid;
		public uint ship_tlv;
		public float positon_x;
		public float positon_y;
		public float positon_z;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			map_id = reader.ReadUInt32();
			area_uid = reader.ReadUInt64();
			from_hero_uid = reader.ReadUInt64();
			chest_npc_uid = reader.ReadUInt64();
			chest_npc_tid = reader.ReadUInt32();
			drop_item_tid = reader.ReadUInt32();
			ship_tlv = reader.ReadUInt32();
			positon_x = reader.ReadSingle();
			positon_y = reader.ReadSingle();
			positon_z = reader.ReadSingle();
		}
	}

	public class S2C_SYNC_PERSONAL_DROP: S2C_HEADER
	{
		public byte isMapSync;
		public byte is_die_drop;
		public List<PersonalDrop> drop_list = new List<PersonalDrop>();

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			isMapSync = reader.ReadByte();
			is_die_drop = reader.ReadByte();
			{
				drop_list.Clear();
				int __arrayLengthInBuffer = reader.ReadInt16();
				for (int jArrayIndex = 0; jArrayIndex < __arrayLengthInBuffer; jArrayIndex++)
					drop_list.Add(KProtoBuf.CreateProtoBufAndUnPack<PersonalDrop>(reader));
			}
		}
	}

	public class S2C_NOTIFY_PERSONAL_DROP_RESULT: S2C_HEADER
	{
		public uint map_id;
		public ulong area_uid;
		public ulong npc_uid;
		public sbyte result;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			map_id = reader.ReadUInt32();
			area_uid = reader.ReadUInt64();
			npc_uid = reader.ReadUInt64();
			result = reader.ReadSByte();
		}
	}

	public class S2C_SYNC_DETECTOR_DISTANCE: S2C_HEADER
	{
		public ulong hero_uid;
		public byte is_active;
		public uint treasure_signal_tid;
		public float distance;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			hero_uid = reader.ReadUInt64();
			is_active = reader.ReadByte();
			treasure_signal_tid = reader.ReadUInt32();
			distance = reader.ReadSingle();
		}
	}

	public class S2C_SYNC_PLAYER_DISCOVER_PRECIOUS: S2C_HEADER
	{
		public int discover_type;
		public byte is_in;
		public double fY;
		public double fR;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			discover_type = reader.ReadInt32();
			is_in = reader.ReadByte();
			fY = reader.ReadDouble();
			fR = reader.ReadDouble();
		}
	}

	public class S2C_OPEN_CEHST_BY_KEY_RESULT: S2C_HEADER
	{
		public ulong hero_uid;
		public sbyte result;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			hero_uid = reader.ReadUInt64();
			result = reader.ReadSByte();
		}
	}

	public class S2C_SYNC_CAMP_CHANGED: S2C_HEADER
	{
		public ulong hero_uid;
		public ulong camp_id;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			hero_uid = reader.ReadUInt64();
			camp_id = reader.ReadUInt64();
		}
	}

	public class S2C_SYNC_SEAL_CHANGED: S2C_HEADER
	{
		public ulong hero_uid;
		public byte is_seal;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			hero_uid = reader.ReadUInt64();
			is_seal = reader.ReadByte();
		}
	}

	public class GoodsInfo: KProtoBuf
	{
		public ulong oid;
		public byte is_open;
		public ulong goods_id;
		public ulong item_id;
		public ulong expire_time;
		public ulong refresh_time;
		public long server_left_num;
		public uint order;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			oid = reader.ReadUInt64();
			is_open = reader.ReadByte();
			goods_id = reader.ReadUInt64();
			item_id = reader.ReadUInt64();
			expire_time = reader.ReadUInt64();
			refresh_time = reader.ReadUInt64();
			server_left_num = reader.ReadInt64();
			order = reader.ReadUInt32();
		}
	}

	public class S2C_SYNC_SHOP_INFO: S2C_HEADER
	{
		public uint shop_id;
		public byte is_open;
		public ulong refresh_time;
		public List<GoodsInfo> goods_list = new List<GoodsInfo>();

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			shop_id = reader.ReadUInt32();
			is_open = reader.ReadByte();
			refresh_time = reader.ReadUInt64();
			{
				goods_list.Clear();
				int __arrayLengthInBuffer = reader.ReadInt16();
				for (int jArrayIndex = 0; jArrayIndex < __arrayLengthInBuffer; jArrayIndex++)
					goods_list.Add(KProtoBuf.CreateProtoBufAndUnPack<GoodsInfo>(reader));
			}
		}
	}

	public class S2C_EXCHANGE_RESULT: S2C_HEADER
	{
		public short result;
		public byte op_code;
		public ulong item_id;
		public ulong uid;
		public uint num;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			result = reader.ReadInt16();
			op_code = reader.ReadByte();
			item_id = reader.ReadUInt64();
			uid = reader.ReadUInt64();
			num = reader.ReadUInt32();
		}
	}

	public class GoodsSaledInfo: KProtoBuf
	{
		public uint shop_id;
		public ulong item_id;
		public uint num;
		public ulong uid;
		public ulong expire_time;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			shop_id = reader.ReadUInt32();
			item_id = reader.ReadUInt64();
			num = reader.ReadUInt32();
			uid = reader.ReadUInt64();
			expire_time = reader.ReadUInt64();
		}
	}

	public class S2C_SYNC_BUY_BACK: S2C_HEADER
	{
		public uint shop_id;
		public List<GoodsSaledInfo> goods_saled_info = new List<GoodsSaledInfo>();

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			shop_id = reader.ReadUInt32();
			{
				goods_saled_info.Clear();
				int __arrayLengthInBuffer = reader.ReadInt16();
				for (int jArrayIndex = 0; jArrayIndex < __arrayLengthInBuffer; jArrayIndex++)
					goods_saled_info.Add(KProtoBuf.CreateProtoBufAndUnPack<GoodsSaledInfo>(reader));
			}
		}
	}

	public class GoodsBuyInfo: KProtoBuf
	{
		public ulong oid;
		public ulong item_id;
		public uint num;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			oid = reader.ReadUInt64();
			item_id = reader.ReadUInt64();
			num = reader.ReadUInt32();
		}
	}

	public class S2C_SYNC_SPECIAL_BUY_RECORD: S2C_HEADER
	{
		public List<GoodsBuyInfo> goods_buy_list = new List<GoodsBuyInfo>();

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			{
				goods_buy_list.Clear();
				int __arrayLengthInBuffer = reader.ReadInt16();
				for (int jArrayIndex = 0; jArrayIndex < __arrayLengthInBuffer; jArrayIndex++)
					goods_buy_list.Add(KProtoBuf.CreateProtoBufAndUnPack<GoodsBuyInfo>(reader));
			}
		}
	}

	public class S2C_LEVELUP_REWARD_LIST: S2C_HEADER
	{
		public uint result;
		public List<RewardInfo> rewards = new List<RewardInfo>();

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			result = reader.ReadUInt32();
			{
				rewards.Clear();
				int __arrayLengthInBuffer = reader.ReadInt16();
				for (int jArrayIndex = 0; jArrayIndex < __arrayLengthInBuffer; jArrayIndex++)
					rewards.Add(KProtoBuf.CreateProtoBufAndUnPack<RewardInfo>(reader));
			}
		}
	}

}
