using Assets.Scripts.Lib.Net;
using System;
using System.Collections.Generic;

public abstract class GameEntity<RespondType> : BaseEntity where RespondType : KProtoBuf
{
	/// <summary>
	/// ��ʼ��
	/// </summary>
	/// <param name="respond">����������Э������</param>
	public virtual void InitializeByRespond(RespondType respond)
	{

	}

	public virtual void Initialize()
	{

	}
}
