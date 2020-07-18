using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leyoutech.Utility
{
	public static class PlayerPrefsUtility
	{
		public static bool Exist(string key)
		{
			return PlayerPrefs.HasKey(key);
		}

		public static int GetInt(string key)
		{
			return PlayerPrefs.GetInt(key);
		}

		public static string GetString(string key)
		{
			return PlayerPrefs.GetString(key);
		}

		public static void SetInt(string key,int value = -1)
		{
			PlayerPrefs.SetInt(key, value);
		}

		public static void SetString(string key,string value = "")
		{
			PlayerPrefs.SetString(key, value);
		}
	}
}

