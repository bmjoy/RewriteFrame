using Assets.Scripts.Lib.Net;
using System;
using System.Collections.Generic;

public abstract class GameEntity<RespondType> : BaseEntity where RespondType : KProtoBuf
{
	/// <summary>
	/// 初始化
	/// </summary>
	/// <param name="respond">服务器返回协议类型</param>
	public virtual void InitializeByRespond(RespondType respond)
	{

	}

	public virtual void Initialize()
	{

	}
}
