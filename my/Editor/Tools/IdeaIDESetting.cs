using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public enum LuaIDEType
{
    IDEA = 0,
    VSCode
}

public class LuaIDESetting : EditorWindow
{
    [MenuItem("Lua/LuaIDE Path")]
    public static void SetIdeaIDEPath()
    {
        LuaIDESetting editor = CreateInstance<LuaIDESetting>();
        editor.Show();
    }

    public string[] luaIDETypes = new string[] { "IDEA", "VSCode"};

    void OnGUI()
    {
        int luaIDETypeIndex = EditorPrefs.GetInt("LUA_IDE_TYPE");
        luaIDETypeIndex = EditorGUILayout.Popup(luaIDETypeIndex, luaIDETypes);
        EditorPrefs.SetInt("LUA_IDE_TYPE", luaIDETypeIndex);

        LuaIDEType luaIDEType = (LuaIDEType)luaIDETypeIndex;

        if (luaIDEType == LuaIDEType.IDEA)
        {

            GUILayout.Label("IdeaIDE路径:");
            string ideaPath = EditorPrefs.GetString("IDEA_IDE_Path");
            GUILayout.TextField(ideaPath);
            if (GUILayout.Button("Browse"))
            {
                ideaPath = UnityEditor.EditorUtility.OpenFilePanel("选择路径", "", "exe");
                EditorPrefs.SetString("IDEA_IDE_Path", ideaPath);
            }
        }
        else if (luaIDEType == LuaIDEType.VSCode)
        {
            GUILayout.Label("VSCode IDE路径:");
            string vscodePath = EditorPrefs.GetString("VSCode_IDE_Path");
            GUILayout.TextField(vscodePath);
            if (GUILayout.Button("Browse"))
            {
                vscodePath = UnityEditor.EditorUtility.OpenFilePanel("选择路径", "", "exe");
                EditorPrefs.SetString("VSCode_IDE_Path", vscodePath);
            }
        }
    }
}
