using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Leyoutech.Utility
{
	public static class EffectUtility
	{
		public static bool IsEffectNameValid(string effectName)
		{
			return !string.IsNullOrEmpty(effectName) && effectName != "None";
		}
	}
}
