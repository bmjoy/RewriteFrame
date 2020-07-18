using System;
using UnityEditor;
using UnityEngine;
using AssetBundles;
namespace EditorExtend
{
    [InitializeOnLoad]
    public static class CustomToolbar
    {
        private static GUIContent ms_ExportDataContent;
        private static GUIContent ms_LogConsoleContent;
        private static GUIContent ms_AddressableGenerateContent;
        private static GUIContent ms_StartServerContent;
        private static GUIContent ms_StartGameContent;



        static CustomToolbar()
        {
            ToolbarExtend.OnLeftToolbarGUI += OnLeftGUI;
            ToolbarExtend.OnRightToolbarGUI += OnRightGUI;
        }

        private static void Initialie()
        {
            if (ms_StartGameContent == null)
            {

                // ms_StartServerContent = new GUIContent(""
                //      , AssetBundleManager.Instance.LoadAssetAsync("UI/Atlas/Role/ui_role_62.png", typeof(UnityEngine.Object)).asset as Texture2D
                //      , "Start eternity server");

                // ms_StartGameContent = new GUIContent(""
                //     , AssetBundleManager.Instance.LoadAssetAsync("UI/Atlas/Role/ui_role_62.png", typeof(UnityEngine.Object)).asset as Texture2D
                //     , "Start eternity game");

                ms_StartGameContent = new GUIContent("G", "Start server");
                ms_StartServerContent = new GUIContent("P", "ProtoToGame");
            }
        }

        private static void OnLeftGUI()
        {
            Initialie();

            GUILayout.FlexibleSpace();
            if (!Application.isPlaying
                && GUILayout.Button(ms_StartServerContent, ToolbarExtend.GetCommandButtonStyle()))
            {
                StartServer();
            }
            if (!Application.isPlaying
                && GUILayout.Button(ms_StartGameContent, ToolbarExtend.GetCommandButtonStyle()))
            {
                EditorPrefs.SetBool(URLSetting.START_IS_GAME, true);
                EditorApplication.ExecuteMenuItem("Edit/Play");
            }
        }

        private static void OnRightGUI()
        {
            Initialie();
        }


        private static void StartServer()
        {
            string batchDirectory = Application.dataPath + "/../Tools/";  //   "/../../product/server/bin/" ..表示上一级
            string batchFileName = "start.bat";

            if (System.IO.File.Exists(batchDirectory + batchFileName))
            {
                ExecuteBatchFile(batchDirectory, batchFileName, false);
            }
            else
            {
                UnityEngine.Debug.LogError("Start Server" + "Start Server: " + batchDirectory + batchFileName);
            }
        }

        private static void ExecuteBatchFile(string directory, string fileName, bool waitForExit = true)
        {
            System.Diagnostics.ProcessStartInfo processInfo = new System.Diagnostics.ProcessStartInfo("cmd.exe"
                , string.Format("/c \"{0}{1}\"", directory, fileName));

            processInfo.CreateNoWindow = false;
            processInfo.UseShellExecute = true;
            processInfo.RedirectStandardError = false;
            processInfo.RedirectStandardOutput = false;
            processInfo.WorkingDirectory = directory;
            processInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;

            System.Diagnostics.Process process = System.Diagnostics.Process.Start(processInfo);
            if (waitForExit)
            {
                process.WaitForExit();
            }
        }
    }
}