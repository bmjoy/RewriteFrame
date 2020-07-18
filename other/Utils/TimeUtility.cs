using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leyoutech.Utility
{
	public static class TimeUtility
	{
		/// <summary>
		/// 获取当前时间戳
		/// </summary>
		/// <param name="bflag"></param>
		/// <returns></returns>
		public static ulong GetTimeStamp(bool bflag = true)
		{
			TimeSpan ts = DateTime.UtcNow - new DateTime(2019, 5, 1, 0, 0, 0, 0);
			ulong ret;
			if (bflag)
				ret = Convert.ToUInt64(ts.TotalSeconds);
			else
				ret = Convert.ToUInt64(ts.TotalMilliseconds);
			return ret;
		}

	}
}

