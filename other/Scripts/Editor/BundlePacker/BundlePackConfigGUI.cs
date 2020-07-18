using LeyoutechEditor.Core.EGUI;
using UnityEditor;
using UnityEngine;

namespace LeyoutechEditor.Core.Packer
{
    /// <summary>
    /// 绘制打包AB时，可设置的参数
    /// </summary>
    internal class BundlePackConfigGUI
    {
        //AB的压缩方式
        GUIContent[] m_CompressionContents =
        {
            new GUIContent("No Compression"),
            new GUIContent("Standard Compression (LZMA)"),
            new GUIContent("Chunk Based Compression (LZ4)")
        };
        int[] m_CompressionValues = { 0, 1, 2 };

        BundlePackConfig m_PackConfig = null;

        GUIContent m_TargetContent;
        GUIContent m_CompressionContent;
        GUIContent m_ForceRebuildContent;
        GUIContent m_AppendHashContent;
        GUIContent m_CleanBeforeBuildContent;

        private bool m_AdvancedSettings;
        private bool m_IsForceRebuild = false;
        private bool m_IsAppendHash = false;

        private Vector2 m_ScrollPos = Vector2.zero;

        internal BundlePackConfigGUI()
        {
            m_TargetContent = new GUIContent("Build Target", "Choose target platform to build for."); 
            m_CompressionContent = new GUIContent("Compression", "Choose no compress, standard (LZMA), or chunk based (LZ4)");
            m_ForceRebuildContent = new GUIContent("Force Rebuild", "Force rebuild the asset bundles");
            m_AppendHashContent = new GUIContent("Append Hash", "Append the hash to the assetBundle name.");
            m_CleanBeforeBuildContent = new GUIContent("Clean Output Dir", "Delete the output dir before build");

            m_PackConfig = Util.FileUtil.ReadFromBinary<BundlePackConfig>(BundlePackUtil.GetPackConfigPath());
            m_IsForceRebuild = m_PackConfig.BundleOptions.HasFlag(BuildAssetBundleOptions.ForceRebuildAssetBundle);
            m_IsAppendHash = m_PackConfig.BundleOptions.HasFlag(BuildAssetBundleOptions.AppendHashToAssetBundleName);
        }
        /// <summary>
        /// 查找默认的输出目录
        /// </summary>
        /// <returns></returns>
        private string GetDefaultOutputDir()
        {
            string outputABPath = new System.IO.DirectoryInfo(".").Parent.FullName.Replace('\\', '/');
            return $"{outputABPath}/eternity_assetbunles";
        }

        internal void LayoutGUI()
        {
            var centeredStyle = new GUIStyle(GUI.skin.GetStyle("Label"));
            centeredStyle.alignment = TextAnchor.UpperCenter;
            GUILayout.Label(new GUIContent("Bundle Pack Config"), centeredStyle);

            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical();
            {
                m_PackConfig.OutputDirPath = EditorGUILayoutUtil.DrawDiskFolderSelection("Bundle Output", m_PackConfig.OutputDirPath);
                if(string.IsNullOrEmpty(m_PackConfig.OutputDirPath))
                {
                    m_PackConfig.OutputDirPath = GetDefaultOutputDir();
                }
                m_PackConfig.BuildTarget = (ValidBuildTarget)EditorGUILayout.EnumPopup(m_TargetContent, m_PackConfig.BuildTarget);

                m_AdvancedSettings = EditorGUILayout.Foldout(m_AdvancedSettings, "Advanced Settings");
                if (m_AdvancedSettings)
                {
                    EditorGUIUtil.BeginIndent();
                    {
                        m_PackConfig.CleanupBeforeBuild = EditorGUILayout.Toggle(m_CleanBeforeBuildContent, m_PackConfig.CleanupBeforeBuild);
                        m_PackConfig.Compression = (CompressOptions)EditorGUILayout.IntPopup(m_CompressionContent, (int)m_PackConfig.Compression, m_CompressionContents, m_CompressionValues);

                        EditorGUILayout.Space();

                        EditorGUI.BeginChangeCheck();
                        {
                            m_IsForceRebuild = EditorGUILayout.Toggle(m_ForceRebuildContent, m_IsForceRebuild);
                            m_IsAppendHash = EditorGUILayout.Toggle(m_AppendHashContent, m_IsAppendHash);
                        }
                        if (EditorGUI.EndChangeCheck())
                        {
                            if (m_IsForceRebuild)
                            {
                                m_PackConfig.BundleOptions |= BuildAssetBundleOptions.ForceRebuildAssetBundle;
                            }
                            else
                            {
                                m_PackConfig.BundleOptions &= ~BuildAssetBundleOptions.ForceRebuildAssetBundle;
                            }
                            if (m_IsAppendHash)
                            {
                                m_PackConfig.BundleOptions |= BuildAssetBundleOptions.AppendHashToAssetBundleName;
                            }
                            else
                            {
                                m_PackConfig.BundleOptions &= ~BuildAssetBundleOptions.AppendHashToAssetBundleName;
                            }
                        }
                    }
                    EditorGUIUtil.EndIndent();
                }

                EditorGUILayout.Space();
                if (GUILayout.Button("Pack Bundle"))
                {
                    EditorApplication.delayCall += () =>
                    {
                        BundlePackUtil.PackAssetBundle(m_PackConfig);
                    };
                }
            }
            EditorGUILayout.EndVertical();


            if (GUI.changed)
            {
                Util.FileUtil.SaveToBinary<BundlePackConfig>(BundlePackUtil.GetPackConfigPath(), m_PackConfig);
            }
        }
    }
}
