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
	public enum KC2ZS_Protocol
	{
		cl2zs_protocol_begin = 0,
		cl2zs_test = 1,
		cl2zs_protocol_end = 2,
	}

	public class KCL2ZS_PROTOCOL_HEADER: KProtoBuf
	{
		public ushort protocolID;

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
			writer.Write(protocolID);
		}
	}

	public class KCL2ZS_TEST: KCL2ZS_PROTOCOL_HEADER
	{
		public ulong uRoleID;

		public override void Pack(BinaryWriter writer)
		{
			base.Pack(writer);
			writer.Write(uRoleID);
		}
	}

}
