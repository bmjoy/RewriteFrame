using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Esc3DViewer : Common3DViewer
{
    protected override string GetLightPrefabPath()
    {
        return "Assets/Artwork/UI/Prefabs/UI3D_EscCharacterPanel.prefab";
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
        return new Vector3(0, 180, 0);
    }
}
