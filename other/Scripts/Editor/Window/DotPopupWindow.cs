using LeyoutechEditor.Core.EGUI;
using UnityEditor;
using UnityEngine;

namespace LeyoutechEditor.Core.Window
{
    public abstract class DotPopupWindow : EditorWindow
    {
        protected bool AutoClose { get; set; } = false;
        protected bool AlawyFocus { get; set; } = false;

        public static T GetPopupWindow<T>() where T : DotPopupWindow
        {
            var array = Resources.FindObjectsOfTypeAll(typeof(T)) as T[];
            var t = (array.Length <= 0) ? null : array[0];

            return t ?? CreateInstance<T>();
        }

        public void Show<T>(Rect position, bool focus = true, bool autoClose = false) where T : DotPopupWindow
        {
            minSize = position.size;
            this.position = position;
            AutoClose = autoClose;
            AlawyFocus = focus;

            if (AlawyFocus) Focus();

            ShowPopup();
        }

        /// <summary>
        /// 绘制弹窗背景
        /// </summary>
        protected virtual void DrawBackground()
        {
            Rect winRect = new Rect(Vector2.zero, position.size);
            EditorGUI.DrawRect(winRect, EditorGUIUtil.BorderColor);

            Rect backgroundRect = new Rect(Vector2.one, position.size - new Vector2(2f, 2f));
            EditorGUI.DrawRect(backgroundRect, EditorGUIUtil.BackgroundColor);
        }


        protected virtual void OnGUI()
        {
            DrawBackground();
        }

        private void OnLostFocus()
        {
            if (AutoClose)
            {
                Close();
            }
            if(AlawyFocus)
            {
                Focus();
            }
        }
    }
}
