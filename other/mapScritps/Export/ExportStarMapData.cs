#if UNITY_EDITOR
using Map;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 导出星图数据
/// </summary>
public class ExportStarMapData : ExportBase
{
    private string m_StarMapPath;
    public override ExporterHandle BeginExport(params object[] exportPara)
    {
        string[] starMapScenePath = AssetDatabase.FindAssets("StarMap t:Scene");
        if(starMapScenePath != null && starMapScenePath.Length == 1)
        {
            m_StarMapPath = AssetDatabase.GUIDToAssetPath(starMapScenePath[0]);
        }
        else
        {
            Debug.LogError("未找到星图scene");
        }
        return base.BeginExport(exportPara);
    }

    protected override void DoEnd()
    {
        base.DoEnd();
        m_StarMapPath = null;
    }

    protected override IEnumerator DoExport()
    {
        if(!string.IsNullOrEmpty(m_StarMapPath))
        {
            Scene starMapScene = EditorSceneManager.OpenScene(m_StarMapPath, OpenSceneMode.Single);
            while(!starMapScene.isLoaded)
            {
                yield return null;
            }
            StarMapEditorRoot root = Object.FindObjectOfType<StarMapEditorRoot>();
            if(root != null)
            {
                root.ExportStarMap();
            }
        }
        yield return null;
    }
}
#endif