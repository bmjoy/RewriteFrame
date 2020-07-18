using System.Collections.Generic;
using UnityEditor;
using UnityObject = UnityEngine.Object;

namespace LeyoutechEditor.Core.Util
{
    public static class SelectionUtil
    {
        /// <summary>
        /// 设置在Project中选中的资源
        /// </summary>
        /// <param name="uObj"></param>
        public static void ActiveObject(UnityObject uObj)
        {
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = uObj;
        }

        public static void ActiveObject(string assetPath)
        {
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<UnityObject>(assetPath);
        }

        /// <summary>
        /// 设置在Project中选中的资源
        /// </summary>
        /// <param name="uObjs"></param>
        public static void ActiveObjects(UnityObject[] uObjs)
        {
            EditorUtility.FocusProjectWindow();
            Selection.objects = uObjs;
        }
    }
}
