using UnityEngine;

public class Ship3DViewer : Common3DViewer
{
    protected override string GetLightPrefabPath()
    {
        return "Assets/Artwork/UI/Prefabs/UI3DShip.prefab";
    }

    protected override int GetTextureSize()
    {
        return 4096;
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
