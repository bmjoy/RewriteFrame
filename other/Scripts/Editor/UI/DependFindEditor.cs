
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace LeyoutechEditor.Core.UI
{
    public class DependsFindEditor : EditorWindow
    {
        /// <summary>
        /// 全局的设置数据
        /// </summary>
        private static MyGameObjectScript target;

        /// <summary>
        /// 当前窗口的数据
        /// </summary>
        private SerializedObject targetObject;
        /// <summary>
        /// 滚动位置
        /// </summary>
        private Vector2 scrollDelta;
        /// <summary>
        /// 滚动位置
        /// </summary>
        private Vector2 scrollDelta2;



        /// <summary>
        /// 数据对象结构
        /// </summary>
        [Serializable]
        public class MyGameObjectScript : ScriptableObject
        {
            [SerializeField]
            public DefaultAsset folder = null;
        }

        [MenuItem("Custom/Depends Find Window")]
        static void Init()
        {
            if (target == null)
                target = ScriptableObject.CreateInstance<MyGameObjectScript>();

            var window = GetWindow(typeof(DependsFindEditor)) as DependsFindEditor;
            window.titleContent = new GUIContent("Assets Dependency");
            window.targetObject = new SerializedObject(target);

            window.Show();
        }

        void OnGUI()
        {
            
            EditorGUILayout.BeginVertical();
            GUI.enabled = findState == 0;
            EditorGUILayout.PropertyField(targetObject.FindProperty("folder"), new GUIContent("  Find Folder"));
            if (findState == 0 || findState == 1)
            {
                scrollDelta = EditorGUILayout.BeginScrollView(scrollDelta);
                var all = m_SelectionList;
                if (all != null)
                {
                    foreach (var a in all)
                    {
                        string path = AssetDatabase.GetAssetPath(a);
                        if (!string.IsNullOrEmpty(path))
                        {
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField("  ○", GUILayout.Width(20));
                            EditorGUILayout.LabelField(AssetDatabase.GetAssetPath(a));
                            EditorGUILayout.EndHorizontal();
                        }
                    }
                }
                EditorGUILayout.EndScrollView();
            }
            GUI.enabled = true;

            targetObject.ApplyModifiedProperties();

            if (findState == 0)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("pick selection"))
                {
                    m_SelectionList = Selection.GetFiltered<UnityEngine.Object>(SelectionMode.DeepAssets);
                }
                GUI.enabled = m_SelectionList != null && m_SelectionList.Length > 0;
                if (GUILayout.Button("find"))
                {
                    findState = 1;

                    m_DependList.Clear();
                    foreach (var item in m_SelectionList)
                    {
                        m_DependList.Add(AssetDatabase.GetAssetPath(item), new List<string>());
                        m_DependFoldState.Add(AssetDatabase.GetAssetPath(item), false);
                    }

                    m_allAssets.Clear();
                    string folderPath = target.folder == null ? null : AssetDatabase.GetAssetPath(target.folder);
                    foreach (string asset in AssetDatabase.GetAllAssetPaths())
                    {
                        if (folderPath == null || asset.StartsWith(folderPath))
                            m_allAssets.Add(asset);
                    }
                    m_allAssetsCount = m_allAssets.Count;
                }
                GUI.enabled = true;
                EditorGUILayout.EndHorizontal();
            }
            else if (findState == 1)
            {
                if (m_allAssets.Count > 0)
                {
                    if (GUILayout.Button("cancel (" + m_allAssets.Count + ")"))
                    {
                        findState = 0;
                        m_DependList.Clear();
                        m_DependFoldState.Clear();
                        m_allAssets.Clear();
                        m_allAssetsCount = 0;
                    }
                    else
                    {
                        string currAsset = m_allAssets[0];
                        foreach (string path in AssetDatabase.GetDependencies(currAsset))
                        {
                            if (currAsset.Equals(path))
                                continue;
                            if (m_DependList.ContainsKey(path))
                                m_DependList[path].Add(currAsset);
                        }

                        m_allAssets.RemoveAt(0);
                    }
                    Repaint();
                }
                else
                {
                    findState = 2;
                    scrollDelta2 = Vector2.zero;
                }
            }
            else if (findState == 2)
            {
                scrollDelta2 = EditorGUILayout.BeginScrollView(scrollDelta2);
                foreach (string key in m_DependList.Keys.ToArray())
                {
                    Color oldColor = GUI.color;

                    GUI.color = m_DependList[key].Count == 0 ? Color.cyan : oldColor;
                    EditorGUILayout.BeginHorizontal();
                    m_DependFoldState[key] = EditorGUILayout.Foldout(m_DependFoldState[key], key + " (" + m_DependList[key].Count + ")");
                    if (GUILayout.Button("s", GUILayout.Width(20)))
                    {
                        Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(key);
                    }
                    EditorGUILayout.EndHorizontal();
                    GUI.color = oldColor;


                    if (m_DependFoldState[key])
                    {
                        if (m_DependList[key].Count > 0)
                        {
                            foreach (string item in m_DependList[key])
                            {
                                EditorGUILayout.BeginHorizontal();
                                EditorGUI.indentLevel++;
                                EditorGUILayout.LabelField(item);
                                if (GUILayout.Button("s", GUILayout.Width(20)))
                                {
                                    Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(item);
                                }
                                EditorGUI.indentLevel--;
                                EditorGUILayout.EndHorizontal();
                            }
                        }
                    }
                }
                EditorGUILayout.EndScrollView();

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("remove all death file"))
                {
                    foreach (string key in m_DependList.Keys.ToArray())
                    {
                        if (m_DependList[key].Count == 0)
                        {
                            AssetDatabase.DeleteAsset(key);
                            m_DependList.Remove(key);
                        }
                    }
                }
                if (GUILayout.Button(" ok "))
                {
                    findState = 0;
                    m_DependList.Clear();
                    m_DependFoldState.Clear();
                    m_allAssets.Clear();
                    m_allAssetsCount = 0;
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }

        private UnityEngine.Object[] m_SelectionList;
        private Dictionary<string, List<string>> m_DependList = new Dictionary<string, List<string>>();
        private Dictionary<string, bool> m_DependFoldState = new Dictionary<string, bool>();
        private List<string> m_allAssets = new List<string>();
        private int m_allAssetsCount = 0;
        private int findState = 0;
    }
}
