using System;
using System.Collections;
using UnityEngine;

public static class TimeUtil
{
	/// <summary>
	/// 秒转换成时分秒
	/// </summary>
	/// <returns></returns>
	public static bool Time_msToMinutesAndSeconds(ulong s, ref long days, ref long hours, ref long minutes, ref long seconds)
	{
		return Time_msToMinutesAndSeconds((long)s, ref days, ref hours, ref minutes, ref seconds);
	}

	/// <summary>
	/// 秒转换成时分秒
	/// </summary>
	/// <param name="time">秒</param>
	public static string GetTimeStr(long time)
	{
		long h = Mathf.FloorToInt(time / 3600);
		long m = Mathf.FloorToInt(time / 60 - h * 60);
		long s = Mathf.FloorToInt(time - m * 60 - h * 3600);
		return h.ToString("00") + ":" + m.ToString("00") + ":" + s.ToString("00");
	}

	/// <summary>
	/// 秒转换成时分秒
	/// </summary>
	/// <returns></returns>
	public static bool Time_msToMinutesAndSeconds(long s, ref long days, ref long hours, ref long minutes, ref long seconds)
	{
		if (s < 0)
			return false;

		string timeStr = "";
		long time = s;           //秒

		seconds = time % 60;           //模秒
		minutes = time / 60 % 60;      //模分
		hours = time / 3600 % 24;      //模时
		days = time / (3600 * 24);      //除天/模时

		return true;
	}
    /// <summary>
    /// 毫秒转化成时分秒毫秒
    /// </summary>
    /// <param name="ms"></param>
    /// <param name="days"></param>
    /// <param name="hours"></param>
    /// <param name="minutes"></param>
    /// <param name="seconds"></param>
    /// <param name=""></param>
    /// <returns></returns>
    public static bool Time_msToMinutesAndMillisecond(long ms, ref long days, ref long hours, ref long minutes, ref long seconds, ref long millisecond)
    {
        if (ms < 0)
            return false;
        int ss = 1000;
        int mi = ss * 60;
        int hh = mi * 60;
        int dd = hh * 24;
        days = ms / dd;
        hours = (ms - days * dd) / hh;
        minutes = (ms - days * dd - hours * hh) / mi;
        seconds = (ms - days * dd - hours * hh - minutes * mi) / ss;
        millisecond = ms - days * dd - hours * hh - minutes * mi - seconds * ss;
        return true;
    }
    /// <summary>
    /// 转换时间格式
    /// </summary>
    /// <param name="lTime"></param>
    /// <returns></returns>
    public static DateTime GetDateTime(long lTime)
	{
		return GetDateTime((ulong)lTime);
	}

	/// <summary>
	/// 时间戳转换为格式时间：(这里以秒为单位)
	/// </summary>
	/// <param name="timeStamp"></param>
	/// <returns></returns>
	public static DateTime GetDateTime(ulong lTime)
	{
		DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));  //时间戳起始点转为目前时区
		long unixTime = (long)lTime * 10000000L;
		TimeSpan toNow = new TimeSpan(unixTime); //时间间隔
		DateTime dt = dtStart.Add(toNow);//加上时间间隔得到目标时间
										 // return dt.ToString("yyyy-MM-dd");
		return dt;
	}
	/// <summary>
	/// 时间戳转换为格式时间：(这里以秒为单位)
	/// </summary>
	/// <param name="lTime"></param>
	/// <returns></returns>
	public static string GetDateTimeToString(long lTime)
	{
		return GetDateTimeToString((ulong)lTime);
	}
	/// <summary>
	/// 时间戳转换为格式时间：(这里以秒为单位)
	/// </summary>
	/// <param name="lTime"></param>
	/// <returns></returns>
	public static string GetDateTimeToString(ulong lTime)
	{
		//日期
		DateTime dateTime = GetDateTime(lTime);

		string rtime = dateTime.ToString("r");
		string[] rtimes = rtime.Split(' ');

		//月.日.年  时:分 时区
		string sendTimestr = "{0}.{1}.{2} {3}:{4:D2} {5}";
		string month = rtimes[2];//"";

		sendTimestr = string.Format(sendTimestr, month, dateTime.Day, dateTime.Year, dateTime.Hour, dateTime.Minute, rtimes[rtimes.Length - 1]);

		return sendTimestr;
	}

}
