
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Diagnostics;

public class LogLocation
{
    [UnityEditor.Callbacks.OnOpenAssetAttribute(0)]
    static bool OnOpenAsset(int instanceID, int line)
    {
        string stackTrace = GetStackTrace();
        /*这里与LuaException里一样，也需要根据自己的log格式去修改解析的正则
         * 我这里是按照下面的异常格式解析
        LuaException: X:/ tolua - master / Assets / ToLua / Examples / 02_ScriptsFromFile / ScriptsFromFile.lua:22: attempt to perform arithmetic on local 'luaClass1'(a table value)
            stack traceback:
	            ScriptsFromFile.lua:22: in main chunk

LUA: dddddd
stack traceback:
             GameMain.lua:38: in function 'GameMain.Start'


             xlua.access, no field __Hotfix0_TestHotfix
stack traceback:
	[C]: in field 'access'
	[string "Init"]:101: in field 'hotfix'
	XLua/Hotfix/HotfixTest.lua:25: in function 'XLua.Hotfix.HotfixTest.Register'
        */

        //Match match = Regex.Match(stackTrace, "LuaException: (.*?.lua:\\d+)");
        Match match = Regex.Match(stackTrace, "stack traceback:\n(.*?.lua:\\d+)");

        // Match match = Regex.Match(match1.ToString().Replace("stack traceback:\n\t", ""), ".*");
        // UnityEngine.Debug.LogError("===================: " +match.ToString());
        if (OpenLuaLocation(match))
        {
            return true;
        }
        match = Regex.Match(stackTrace, @"file:line:column (.+):0", RegexOptions.IgnoreCase);
        if (FindLocation(match, true))
        {
            return true;
        }

        match = Regex.Match(stackTrace, @"\(at (.+)\)", RegexOptions.IgnoreCase);
        if (FindLocation(match, false))
        {
            return true;
        }

        return false;
    }

    static bool OpenLuaLocation(Match match)
    {
        if (!match.Success)
        {
            return false;
        }
        int luaIDETypeIndex = EditorPrefs.GetInt("LUA_IDE_TYPE");
        LuaIDEType luaIDEType = (LuaIDEType)luaIDETypeIndex;

        string idePath = string.Empty;
        if (luaIDEType == LuaIDEType.IDEA)
        {
            idePath = EditorPrefs.GetString("IDEA_IDE_Path");
        }
        else if (luaIDEType == LuaIDEType.VSCode)
        {
            idePath = EditorPrefs.GetString("VSCode_IDE_Path");
        }


        if (string.IsNullOrEmpty(idePath) || !System.IO.File.Exists(idePath))
        {
            return false;
        }


        while (match.Success)
        {
            string pathLine = match.Groups[1].Value;
            if (!pathLine.Contains("LogSubsystem.cs"))//一直为true
            {
                int spliteIndex = pathLine.LastIndexOf(':');
                string path = pathLine.Substring(0, spliteIndex);
                int line = System.Convert.ToInt32(pathLine.Substring(spliteIndex + 1));
                string s = path.Replace("\t", "");
                for (int i = 0; i < XLuaManager.m_path.Count; i++)
                {
                    if (XLuaManager.m_path[i].Contains(s))//xlua 里所有lua 文件地址合集
                    {
                        path = XLuaManager.m_path[i];
                        break;
                    }
                }
                string args = string.Empty;
                if (luaIDEType == LuaIDEType.IDEA)
                {
                    args = string.Format("{0}:{1}", path.Replace("\\", "/"), line);
                }
                else if (luaIDEType == LuaIDEType.VSCode)
                {
                    args = string.Format("-g {0}:{1}", path.Replace("\\", "/"), line);
                }

                Process process = new Process();
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = idePath;
                startInfo.Arguments = args;
                startInfo.UseShellExecute = false;
                startInfo.CreateNoWindow = false;
                startInfo.RedirectStandardOutput = false;
                process.StartInfo = startInfo;
                process.Start();
                return true;
            }
            match = match.NextMatch();
        }
        return false;
    }

    static bool FindLocation(Match match, bool bFullPath)
    {
        while (match.Success)
        {
            string pathLine = match.Groups[1].Value;
            if (!pathLine.Contains("LogSubsystem.cs"))
            {
                int spliteIndex = pathLine.LastIndexOf(':');
                string path = pathLine.Substring(0, spliteIndex);
                int Line = System.Convert.ToInt32(pathLine.Substring(spliteIndex + 1));
                if (!bFullPath)
                {
                    path = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf("Assets")) + path;
                }
                UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(path, Line);
                return true;
            }
            match = match.NextMatch();
        }
        return false;
    }

    static string GetStackTrace()
    {
        System.Type consoleWindowType = typeof(EditorWindow).Assembly.GetType("UnityEditor.ConsoleWindow");
        FieldInfo ms_ConsoleWindow_fieldInfo = consoleWindowType.GetField("ms_ConsoleWindow", BindingFlags.Static | BindingFlags.NonPublic);
        EditorWindow consoleWindowInstance = ms_ConsoleWindow_fieldInfo.GetValue(null) as EditorWindow;
        if (null != consoleWindowInstance)
        {
            if (EditorWindow.focusedWindow == consoleWindowInstance)
            {
                FieldInfo m_ActiveText_fieldInfo = consoleWindowType.GetField("m_ActiveText", BindingFlags.Instance | BindingFlags.NonPublic);
                return m_ActiveText_fieldInfo.GetValue(consoleWindowInstance).ToString();
            }
        }
        return string.Empty;
    }
}
