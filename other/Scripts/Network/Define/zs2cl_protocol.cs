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
	public enum KZS2CL_Protocol
	{
		zs2cl_protocol_begin = 0,
		zs2cl_test_respone = 1,
		zs2cl_protocol_end = 2,
	}

	public class KZS2CL_PROTOCOL_HEADER: KProtoBuf
	{
		public ushort protocolID;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			protocolID = reader.ReadUInt16();
		}
	}

	public class KZS2CL_TEST_RESPONE: KZS2CL_PROTOCOL_HEADER
	{
		public ulong uRoleID;

		public override void UnPack(BinaryReader reader)
		{
			base.UnPack(reader);
			uRoleID = reader.ReadUInt64();
		}
	}

}
