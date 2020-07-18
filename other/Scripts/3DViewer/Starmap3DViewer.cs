using UnityEngine;

public class Starmap3DViewer : Common3DViewer
{

	protected override string GetLightPrefabPath()
	{
		return "Assets/Artwork/UI/Prefabs/UI3D_StarmapPanel.prefab";
	}

	protected override int GetTextureSize()
	{
		return 2048;
	}

	protected override Vector3 GetBasePosition()
	{
		return Vector3.zero;
	}

	protected override Vector3 GetBaseRotation()
	{
		return Vector3.zero;
	}
}
