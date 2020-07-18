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
	public enum KG2C_Protocol
	{
		g2c_protocol_begin = 1000,
		g2c_ping_respond = 1001,
		g2c_handshake_respond = 1002,
		g2c_account_locked_notify = 1003,
		g2c_account_verify_respond = 1004,
		g2c_kick_account_notify = 1005,
		g2c_sync_role_info = 1006,
		g2c_role_name_respond = 1007,
		g2c_create_role_respond = 1008,
		g2c_delete_role_respond = 1009,
		g2c_undo_del_role_respond = 1010,
		g2c_sync_login_key = 1011,
		g2c_query_role_id_list_respond = 1012,
		g2c_error_info_notify = 1013,
		g2c_gs_to_player = 1014,
		g2c_player_swtich_gs = 1015,
		g2c_exit_game_respond = 1016,
		g2c_timeout_check = 1017,
		g2c_protocol_total = 1018,
	}

	public class KG2C_PROTOCOL_HEADER: KProtoBuf
	{
		public ushort byProtocol;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			byProtocol = reader.ReadUInt16();
		}
	}

	public class KG2C_PingRespond: KG2C_PROTOCOL_HEADER
	{
		public uint time;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			time = reader.ReadUInt32();
		}
	}

	public class KG2C_AccountLockedNotify: KG2C_PROTOCOL_HEADER
	{
		public byte nothing;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			nothing = reader.ReadByte();
		}
	}

	public class KG2C_AccountVerifyRespond: KG2C_PROTOCOL_HEADER
	{
		public int code;
		public int loginTime;
		public int lastLoginTime;
		public uint lastLoginIP;
		public int zoneID;
		public uint limitPlayTimeFlag;
		public uint limitPlayOnlineSeconds;
		public uint limitPlayOfflineSeconds;
		public byte limitPlayEnable;
		public uint limitOnlineSetting;
		public uint limitOfflineSetting;
		public int nTag;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			code = reader.ReadInt32();
			loginTime = reader.ReadInt32();
			lastLoginTime = reader.ReadInt32();
			lastLoginIP = reader.ReadUInt32();
			zoneID = reader.ReadInt32();
			limitPlayTimeFlag = reader.ReadUInt32();
			limitPlayOnlineSeconds = reader.ReadUInt32();
			limitPlayOfflineSeconds = reader.ReadUInt32();
			limitPlayEnable = reader.ReadByte();
			limitOnlineSetting = reader.ReadUInt32();
			limitOfflineSetting = reader.ReadUInt32();
			nTag = reader.ReadInt32();
		}
	}

	public class KG2C_SyncLoginKey: KG2C_PROTOCOL_HEADER
	{
		public byte code;
		public ulong uRoleID;
		public byte[] guid = null;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			code = reader.ReadByte();
			uRoleID = reader.ReadUInt64();
			guid = KProtoBuf.ReadByteArray(reader, 16);
		}
	}

	public class KG2C_KickAccountNotify: KG2C_PROTOCOL_HEADER
	{
		public byte nothing;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			nothing = reader.ReadByte();
		}
	}

	public class KG2C_RoleNameRespond: KG2C_PROTOCOL_HEADER
	{
		public string szRoleName = "";

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			szRoleName = KProtoBuf.ReadString(reader, 64);
		}
	}

	public class KG2C_CreateRoleRespond: KG2C_PROTOCOL_HEADER
	{
		public byte code;
		public ulong uRoleID;
		public string szRoleName = "";
		public int nMainHeroTemplateID;
		public ushort uLevel;
		public int nDelCountDownTime;
		public int nLastLoginTime;
		public byte[] exData = null;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			code = reader.ReadByte();
			uRoleID = reader.ReadUInt64();
			szRoleName = KProtoBuf.ReadString(reader, 64);
			nMainHeroTemplateID = reader.ReadInt32();
			uLevel = reader.ReadUInt16();
			nDelCountDownTime = reader.ReadInt32();
			nLastLoginTime = reader.ReadInt32();
			exData = KProtoBuf.ReadByteArray(reader, 0);
		}
	}

	public class KG2C_DeleteRoleRespond: KG2C_PROTOCOL_HEADER
	{
		public byte code;
		public ulong uRoleID;
		public int nDelCountDownTime;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			code = reader.ReadByte();
			uRoleID = reader.ReadUInt64();
			nDelCountDownTime = reader.ReadInt32();
		}
	}

	public class KG2C_UndoDelRoleRespond: KG2C_PROTOCOL_HEADER
	{
		public byte code;
		public ulong uRoleID;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			code = reader.ReadByte();
			uRoleID = reader.ReadUInt64();
		}
	}

	public class KG2C_HandshakeRespond: KG2C_PROTOCOL_HEADER
	{
		public byte code;
		public byte gameEdition;
		public string szZoneName = "";
		public string szGroupName = "";
		public uint uPlayerIndex;
		public byte bReconnect;
		public int nTag;
		public string szClientIP = "";

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			code = reader.ReadByte();
			gameEdition = reader.ReadByte();
			szZoneName = KProtoBuf.ReadString(reader, 48);
			szGroupName = KProtoBuf.ReadString(reader, 64);
			uPlayerIndex = reader.ReadUInt32();
			bReconnect = reader.ReadByte();
			nTag = reader.ReadInt32();
			szClientIP = KProtoBuf.ReadString(reader, 64);
		}
	}

	public class KG2C_SyncRoleInfo: KG2C_PROTOCOL_HEADER
	{
		public ulong uRoleID;
		public string szRoleName = "";
		public int nMainHeroTemplateID;
		public ushort uLevel;
		public int nDelCountDownTime;
		public int nLastLoginTime;
		public byte[] exData = null;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			uRoleID = reader.ReadUInt64();
			szRoleName = KProtoBuf.ReadString(reader, 64);
			nMainHeroTemplateID = reader.ReadInt32();
			uLevel = reader.ReadUInt16();
			nDelCountDownTime = reader.ReadInt32();
			nLastLoginTime = reader.ReadInt32();
			exData = KProtoBuf.ReadByteArray(reader, 0);
		}
	}

	public class KG2C_QueryIDListRespond: KG2C_PROTOCOL_HEADER
	{
		public List<ulong> roleIdList = new List<ulong>();

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			{
				roleIdList.Clear();
				int __arrayLengthInBuffer = reader.ReadInt16();
				for (int jArrayIndex = 0; jArrayIndex < __arrayLengthInBuffer; jArrayIndex++)
					roleIdList.Add(reader.ReadUInt64());
			}
		}
	}

	public class KG2C_Error_Info_Notify: KG2C_PROTOCOL_HEADER
	{
		public byte code;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			code = reader.ReadByte();
		}
	}

	public class KG2C_GSToPlayer: KG2C_PROTOCOL_HEADER
	{
		public byte[] data = null;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			data = KProtoBuf.ReadByteArray(reader, 0);
		}
	}

	public class KG2C_PlayerSwitchGS: KG2C_PROTOCOL_HEADER
	{
		public ulong uPlayerID;
		public byte[] guid = new byte[16];
		public int fromGSIndex;
		public int toGSIndex;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			uPlayerID = reader.ReadUInt64();
			for (int jArrayIndex = 0; jArrayIndex < guid.Length; jArrayIndex++)
				guid[jArrayIndex] = reader.ReadByte();
			fromGSIndex = reader.ReadInt32();
			toGSIndex = reader.ReadInt32();
		}
	}

	public class KG2C_EXIT_GAME_RESPOND: KG2C_PROTOCOL_HEADER
	{
		public int exitCode;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			exitCode = reader.ReadInt32();
		}
	}

	public class KG2C_TIMEOUT_CHECK: KG2C_PROTOCOL_HEADER
	{
		public byte sessionId;
		public ulong timeStamp;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			sessionId = reader.ReadByte();
			timeStamp = reader.ReadUInt64();
		}
	}

}
