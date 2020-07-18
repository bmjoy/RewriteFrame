using UnityEditor;

namespace LeyoutechEditor.Core.Util
{
    public static class PlayerSettingsUtil
    {
        /// <summary>
        /// 判断是否定义了指定的宏
        /// </summary>
        /// <param name="symbol">宏</param>
        /// <returns></returns>
        public static bool HasScriptingDefineSymbol(string symbol)
        {
            string symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(ActiveBuildTargetGroup);
            return symbols.IndexOf(symbol) >= 0;
        }
        /// <summary>
        /// 添加宏定义
        /// </summary>
        /// <param name="symbol"></param>
        public static void AddScriptingDefineSymbol(string symbol)
        {
            BuildTargetGroup btGroup = ActiveBuildTargetGroup;
            string symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(btGroup);
            int symbolIndex = symbols.IndexOf(symbol);
            if(symbolIndex>=0)
            {
                return;
            }

            if (symbols.Length > 0 && symbols[symbols.Length - 1] != ';')
            {
                symbols += ";";
            }
            symbols += symbol;

            PlayerSettings.SetScriptingDefineSymbolsForGroup(btGroup, symbols);
        }

        /// <summary>
        /// 删除宏定义
        /// </summary>
        /// <param name="symbol"></param>
        public static void RemoveScriptingDefineSymbol(string symbol)
        {
            BuildTargetGroup btGroup = ActiveBuildTargetGroup;
            string symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(btGroup);
            int symbolIndex = symbols.IndexOf(symbol);
            if (symbolIndex < 0)
            {
                return;
            }

            if (symbols.Length > symbol.Length && symbols[symbolIndex + 1] == ';')
            {
                symbols = symbols.Replace(symbol + ";", "");
            }
            else
            {
                symbols = symbols.Replace(symbol, "");
            }

            PlayerSettings.SetScriptingDefineSymbolsForGroup(btGroup, symbols);
        }
        /// <summary>
        /// 根据当前平台查找BuildTargetGroup
        /// </summary>
        private static BuildTargetGroup ActiveBuildTargetGroup
        {
            get
            {
                BuildTargetGroup btGroup = BuildTargetGroup.Standalone;
                BuildTarget buildTarget = EditorUserBuildSettings.activeBuildTarget;
                if (buildTarget == BuildTarget.StandaloneWindows64)
                {
                    btGroup = BuildTargetGroup.Standalone;
                }
                else if (buildTarget == BuildTarget.XboxOne)
                {
                    btGroup = BuildTargetGroup.XboxOne;
                }
                else if (buildTarget == BuildTarget.PS4)
                {
                    btGroup = BuildTargetGroup.PS4;
                }else if(buildTarget == BuildTarget.Android)
                {
                    btGroup = BuildTargetGroup.Android;
                }else if(buildTarget == BuildTarget.iOS)
                {
                    btGroup = BuildTargetGroup.iOS;
                }

                return btGroup;
            }
        }
    }
}
