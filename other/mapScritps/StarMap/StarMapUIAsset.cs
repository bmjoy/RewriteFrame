#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Map
{
    /// <summary>
    /// hierarchy面板显示（为了让策划区分）
    /// </summary>
    [ExecuteInEditMode]
    public class StarMapUIAsset : MonoBehaviour
    {
        private void OnEnable()
        {
            EditorApplication.hierarchyWindowItemOnGUI += HierarchyItemCB;
        }

        private void HierarchyItemCB(int instanceid,Rect selectionrect)
        {
            GameObject obj = EditorUtility.InstanceIDToObject(instanceid) as GameObject;
            if(obj == null)
            {
                return;
            }
            bool isActive = obj.activeSelf;
            Transform trans = obj.transform;
            StarMapEditorRoot root = obj.GetComponent<StarMapEditorRoot>();
            if(root != null)
            {
                DrawUI(trans, selectionrect, "编辑", Color.red);
            }

            StarMapMainPanel starMapMainPanel = obj.GetComponent<StarMapMainPanel>();
            if(starMapMainPanel != null)
            {
                DrawUI(trans,selectionrect, "编辑星图", isActive ? Color.yellow : Color.red);
            }

            FixedStarPanel fixedStarPanel = obj.GetComponent<FixedStarPanel>();
            if(fixedStarPanel != null)
            {
                DrawUI(trans,selectionrect, "编辑恒星", isActive ? Color.yellow : Color.red);
            }

            PlanetPanel planetPanel = obj.GetComponent<PlanetPanel>();
            if(planetPanel != null)
            {
                DrawUI(trans,selectionrect, "编辑行星", isActive ? Color.yellow : Color.red);
            }

            FixedStarElement fixedStarElement = obj.GetComponent<FixedStarElement>();
            if(fixedStarElement != null)
            {
                DrawUI(trans, selectionrect, "恒星", Color.red);
            }

            PlanetContainer planetContainer = obj.GetComponent<PlanetContainer>();
            if (planetContainer != null)
            {
                DrawUI(trans, selectionrect, "行星组", isActive ? Color.yellow : Color.red);
            }

            PlanetElement planetElement = obj.GetComponent<PlanetElement>();
            if (planetElement != null)
            {
                DrawUI(trans, selectionrect, "行星", Color.red);
            }

            PlanetAreaContainer planetAreaContainer = obj.GetComponent<PlanetAreaContainer>();
            if (planetAreaContainer != null)
            {
                DrawUI(trans, selectionrect, "区域组", isActive ? Color.yellow : Color.red);
            }

            PlanetAreaElement planetAreaElement = obj.GetComponent<PlanetAreaElement>();
            if (planetAreaElement != null)
            {
                DrawUI(trans, selectionrect, "区域", Color.blue);
            }
        }


        private void DrawUI(Transform trans,Rect selectionRect,string name,Color color)
        {
            Rect rect = new Rect(selectionRect);
            rect.xMin = 0;
            rect.x = rect.width - 10;
            rect.width = 10;
            GUIStyle style = new GUIStyle();
            style.normal.textColor = color;
            style.alignment = TextAnchor.MiddleRight;
            //style.fontStyle = FontStyle.Bold;
            //style.fontSize = 15;
            GUI.Label(rect, string.Format("【{0}】", name), style);
        }

        private void OnDisable()
        {
            EditorApplication.hierarchyWindowItemOnGUI -= HierarchyItemCB;
        }
    }
}
#endif
