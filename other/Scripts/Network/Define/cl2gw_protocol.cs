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
	public enum KC2G_Protocol
	{
		c2g_protocol_begin = 1,
		c2g_ping_signal = 2,
		c2g_handshake_request = 3,
		c2g_account_verify_request = 4,
		c2g_role_name_request = 5,
		c2g_create_role_request = 6,
		c2g_delete_role_request = 7,
		c2g_undo_del_role_request = 8,
		c2g_role_login_request = 9,
		c2g_player_to_gs = 10,
		c2g_exit_game = 11,
		c2g_enter_zs = 12,
		c2g_timeout_check = 13,
		c2g_protocol_total = 14,
	}

	public class KC2G_PROTOCOL_HEADER: KProtoBuf
	{
		public ushort byProtocol;
		public ulong gateTimeTick;

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
			writer.Write(byProtocol);
			writer.Write(gateTimeTick);
		}
	}

	public enum KG_NET_TYPE
	{
		NET_TYPE_DEFAULT = 0,
		NET_TYPE_TELECOM = 1,
		NET_TYPE_NETCOM = 2,
		NET_TYPE_UNICOM = 3,
		NET_TYPE_MOBILE = 4,
		NET_TYPE_CRC = 5,
		NET_TYPE_SATCOM = 6,
	}

	public class KC2G_HandshakeRequest: KC2G_PROTOCOL_HEADER
	{
		public int nProtocolVersion;
		public byte uNetType;
		public string szAccount = "";
		public string szToken = "";
		public uint uOldPlayerIndex;
		public ulong uRoleID;

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
			writer.Write(nProtocolVersion);
			writer.Write(uNetType);
			KProtoBuf.WriteString(writer, szAccount, 128);
			KProtoBuf.WriteString(writer, szToken, 128);
			writer.Write(uOldPlayerIndex);
			writer.Write(uRoleID);
		}
	}

	public class KC2G_PingSignal: KC2G_PROTOCOL_HEADER
	{
		public uint dwTime;

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
			writer.Write(dwTime);
		}
	}

	public class KC2G_AccountVerifyRequest: KC2G_PROTOCOL_HEADER
	{
		public int nGroupID;
		public string account = "";
		public string password = "";
		public string pf = "";
		public int nTag;
		public List<ulong> roleIdList = new List<ulong>();

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
			writer.Write(nGroupID);
			KProtoBuf.WriteString(writer, account, 128);
			KProtoBuf.WriteString(writer, password, 128);
			KProtoBuf.WriteString(writer, pf, 32);
			writer.Write(nTag);
			{
				writer.Write((short)roleIdList.Count);
				List<ulong>.Enumerator enumerator = roleIdList.GetEnumerator();
				while (enumerator.MoveNext())
				{
					ulong itemInEnumerator = enumerator.Current;
					writer.Write(itemInEnumerator);
				}
			}
		}
	}

	public class KC2G_Role_Name_Request: KC2G_PROTOCOL_HEADER
	{
		public byte eGender;

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
			writer.Write(eGender);
		}
	}

	public class KC2G_CreateRoleRequest: KC2G_PROTOCOL_HEADER
	{
		public int nMainHeroTemplateID;
		public string szRoleName = "";

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
			writer.Write(nMainHeroTemplateID);
			KProtoBuf.WriteString(writer, szRoleName, 64);
		}
	}

	public class KC2G_DeleteRoleRequest: KC2G_PROTOCOL_HEADER
	{
		public ulong uRoleID;

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
			writer.Write(uRoleID);
		}
	}

	public class KC2G_UndoDelRoleRequest: KC2G_PROTOCOL_HEADER
	{
		public ulong uRoleID;

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
			writer.Write(uRoleID);
		}
	}

	public class KC2G_RoleLoginRequest: KC2G_PROTOCOL_HEADER
	{
		public ulong uRoleID;

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
			writer.Write(uRoleID);
		}
	}

	public class KC2G_PlayerToGS: KC2G_PROTOCOL_HEADER
	{
		public uint uSign;
		public byte[] data = null;
		public int _dataLength_ = 0;

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
			writer.Write(uSign);
			KProtoBuf.WriteByteArray(writer, data, _dataLength_);
		}
	}

	public class KC2G_EXIT_GAME: KC2G_PROTOCOL_HEADER
	{
		public int nExitCode;

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
			writer.Write(nExitCode);
		}
	}

	public class KC2G_ENTER_ZS: KC2G_PROTOCOL_HEADER
	{
		public ulong uRoleID;

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
			writer.Write(uRoleID);
		}
	}

	public class KC2G_TIMEOUT_CHECK: KC2G_PROTOCOL_HEADER
	{
		public byte sessionId;
		public ulong timeStamp;

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
			writer.Write(sessionId);
			writer.Write(timeStamp);
		}
	}

}
