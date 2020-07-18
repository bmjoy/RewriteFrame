using System;
using UnityEngine;
using static Cinemachine.CinemachineVirtualCameraBase;

public class MainCameraTransitionBlenderSettings : ScriptableObject
{
	public const string ANY_CAMERA_LABEL = "**ANY CAMERA**";
	public readonly CustomBlend NOTSET_CUSTOM_BLEND = new CustomBlend(BlendHint.None, false);

	[SerializeField]
	private CustomBlend[] m_CustomBlends = null;

	public TransitionBlendDefinition FindBlend(string fromCameraName, string toCameraName)
	{
		bool gotAnyToAny = false;
		CustomBlend anyToAny = NOTSET_CUSTOM_BLEND;

		bool gotAnyToMe = false;
		CustomBlend anyToMe = NOTSET_CUSTOM_BLEND;

		bool gotMeToAny = false;
		CustomBlend meToAny = NOTSET_CUSTOM_BLEND;

		for (int iCustomBlend = 0; iCustomBlend < m_CustomBlends.Length; ++iCustomBlend)
		{
			// Attempt to find direct name first
			CustomBlend iterCustomBlend = m_CustomBlends[iCustomBlend];
			if ((iterCustomBlend.From == fromCameraName)
				&& (iterCustomBlend.To == toCameraName))
			{
				return new TransitionBlendDefinition(iterCustomBlend);
			}

			// If we come across applicable wildcards, remember them
			if (iterCustomBlend.From == ANY_CAMERA_LABEL)
			{
				if (iterCustomBlend.To == toCameraName)
				{
					anyToMe = iterCustomBlend;
					gotAnyToMe = true;
				}
				else if (iterCustomBlend.To == ANY_CAMERA_LABEL)
				{
					anyToAny = iterCustomBlend;
					gotAnyToAny = true;
				}
			}
			else if (iterCustomBlend.To == ANY_CAMERA_LABEL
				&& iterCustomBlend.From == fromCameraName)
			{
				meToAny = iterCustomBlend;
				gotMeToAny = true;
			}
		}

		// If nothing is found try to find wild card blends from any
		// camera to our new one
		if (gotAnyToMe)
		{
			Leyoutech.Utility.DebugUtility.Log(CameraManager.LOG_TAG
				, string.Format("Not found transition blend from ({0}) to ({1}), used any to ({1})"
					, fromCameraName
					, toCameraName));
			return new TransitionBlendDefinition(anyToMe);
		}

		// Still have nothing? Try from our camera to any camera
		else if (gotMeToAny)
		{
			Leyoutech.Utility.DebugUtility.Log(CameraManager.LOG_TAG
				, string.Format("Not found transition blend from ({0}) to ({1}), used ({0}) to any"
					, fromCameraName
					, toCameraName));
			return new TransitionBlendDefinition(meToAny);
		}

		else if (gotAnyToAny)
		{
			Leyoutech.Utility.DebugUtility.Log(CameraManager.LOG_TAG
				, string.Format("Not found transition blend from ({0}) to ({1}), used any to any"
					, fromCameraName
					, toCameraName));
			return new TransitionBlendDefinition(anyToAny);
		}
		else
		{
			Leyoutech.Utility.DebugUtility.LogWarning(CameraManager.LOG_TAG
				, string.Format("Not found transition blend from ({0}) to ({1}), used notset"
					, fromCameraName
					, toCameraName));
			return new TransitionBlendDefinition(NOTSET_CUSTOM_BLEND);
		}
	}

	[Serializable]
	public struct CustomBlend
	{
		public string From;
		public string To;
		public BlendHint BlendHint;
		public bool InheritPosition;

		public CustomBlend(BlendHint blendHint, bool inheritPosition)
		{
			From = ANY_CAMERA_LABEL;
			To = ANY_CAMERA_LABEL;
			BlendHint = blendHint;
			InheritPosition = inheritPosition;
		}
	}
}