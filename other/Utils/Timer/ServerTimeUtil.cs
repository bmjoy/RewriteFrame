using System;
using UnityEngine;
using UnityEngine.Events;
/// <summary>
/// 同步服务器时间   原来的脚本 ServerTimeProxy
/// </summary>
public class ServerTimeUtil : Singleton<ServerTimeUtil>
{
	/// <summary>
	/// 帧时间
	/// </summary>
	private float m_FixedTime;

	private void FixedUpdate()
	{
		m_FixedTime += Time.deltaTime;
		if (m_FixedTime >= 1)
		{
			m_FixedTime--;
			OnTick?.Invoke();
		}

        OnFixedUpdate?.Invoke();
    }
	/// <summary>
	/// 当前客户端服务器时间
	/// (秒值)
	/// </summary>
	public ulong GetNowTime()
	{
		//todo 场景时间未设置
		return NetworkManager.Instance.GetLoginController().TimeStamp() / 1000;
	}

    /// <summary>
    /// 当前客户端服务器时间
    /// (毫秒秒)
    /// </summary>
    public ulong GetNowTimeMSEL()
    {
        //todo 场景时间未设置
        return NetworkManager.Instance.GetLoginController().TimeStamp() ;
    }


    /// <summary>
    /// 获取当前时间
    /// </summary>
    /// <returns></returns>
    public DateTime GetDateTime()
	{
		return TimeUtil.GetDateTime(GetNowTime());
	}
	/// <summary>
	/// tick
	/// </summary>
	public event UnityAction OnTick;

    public event UnityAction OnFixedUpdate;
}
