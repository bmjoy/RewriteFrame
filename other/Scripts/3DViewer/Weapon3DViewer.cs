using UnityEngine;

public class Weapon3DViewer : Common3DViewer
{
    protected override string GetLightPrefabPath()
    {
        return "Assets/Artwork/UI/Prefabs/Old/UI3DWeapons.prefab";
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
