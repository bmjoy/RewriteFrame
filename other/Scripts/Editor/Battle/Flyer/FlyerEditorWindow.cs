using Eternity.Share.Config;
using Eternity.Share.Config.ActionDatas;
using Eternity.Share.Config.FlyerDatas;
using LeyoutechEditor.Core.EGUI;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace EternityEditor.Battle.Flyer
{
    public class FlyerEditorWindow : EditorWindow
    {
        [MenuItem("Custom/Battle/Flyer Editor")]
        public static void ShowWin()
        {
            var win = EditorWindow.GetWindow<FlyerEditorWindow>();
            win.titleContent = new GUIContent("Flyer Editor");
            win.Show();
        }

        private static readonly int TOOLBAR_HEIGHT = 20;
        private static readonly int CONFIG_LIST_WIDTH = 120;
        private static readonly int TAB_PAGE_HEIGHT = 50;

        private GUIContent[] m_TabPageContents = new GUIContent[]
        {
            new GUIContent("Base"),
            new GUIContent("Flying Stage"),
            new GUIContent("End Stage"),
        };
        private IContentTabPage[] m_TabPages = new IContentTabPage[3];

        private int m_FlyerID = 0;
        private int m_ActionIndex = 0;

        private string m_SelectedDataFullPath = string.Empty;
        public string SelectedDataFullPath
        {
            get
            {
                return m_SelectedDataFullPath;
            }
            set
            {
                if (m_SelectedDataFullPath != value)
                {
                    m_SelectedDataFullPath = value;

                    FindIDAndIndex();
                    if (!string.IsNullOrEmpty(m_SelectedDataFullPath))
                    {
                        FlyerData data = m_DataDic[m_SelectedDataFullPath];
                        if(data.Id!=m_FlyerID)
                        {
                            data.Id = m_FlyerID;
                        }
                        m_TabPages[0].SetData(data.BaseData);
                        m_TabPages[1].SetData(data.FlyingData);
                        m_TabPages[2].SetData(data.EndData);
                    }

                }
            }
        }

        private GUIStyle boldCenterStyle = null;
        private void OnEnable()
        {
            if (boldCenterStyle == null)
            {
                boldCenterStyle = new GUIStyle(EditorStyles.boldLabel);
                boldCenterStyle.fontSize = 15;
                boldCenterStyle.alignment = TextAnchor.MiddleCenter;
            }

            LoadData();
            m_SelectedDataFullPath = string.Empty;

            m_TabPages = new IContentTabPage[3];
            m_TabPages[0] = new FlyerBaseContentTab(this);
            m_TabPages[1] = new FlyerFlyingStageContentTab(this);
            m_TabPages[2] = new FlyerEndStageContentTab(this);
        }

        private void OnGUI()
        {
            Rect toolbarRect = new Rect(0, 0, position.width, TOOLBAR_HEIGHT);
            DrawToolbar(toolbarRect);

            Rect configListRect = new Rect(1, TOOLBAR_HEIGHT, CONFIG_LIST_WIDTH, position.height - TOOLBAR_HEIGHT - 1);
            EditorGUIUtil.DrawAreaLine(configListRect, Color.gray);

            configListRect.x += 2;
            configListRect.y += 2;
            configListRect.width -= 4;
            configListRect.height -= 4;
            DrawConfigList(configListRect);

            Rect tabPageRect = new Rect(CONFIG_LIST_WIDTH, TOOLBAR_HEIGHT, position.width - CONFIG_LIST_WIDTH, TAB_PAGE_HEIGHT);
            DrawTabPages(tabPageRect);

            Rect tabContentRect = new Rect(CONFIG_LIST_WIDTH, TOOLBAR_HEIGHT + TAB_PAGE_HEIGHT, position.width - CONFIG_LIST_WIDTH - 1, position.height - TAB_PAGE_HEIGHT - TOOLBAR_HEIGHT - 1);
            EditorGUIUtil.DrawAreaLine(tabContentRect, Color.yellow);

            tabContentRect.x += 1;
            tabContentRect.y += 1;
            tabContentRect.width -= 2;
            tabContentRect.height -= 2;
            DrawTabContent(tabContentRect);
        }

        private bool m_IsShowDesc = false;
        private void DrawToolbar(Rect toolbarRect)
        {
            GUILayout.BeginArea(toolbarRect, EditorStyles.toolbar);
            {
                EditorGUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("Create", "toolbarbutton", GUILayout.Width(60)))
                    {
                        CreateData();
                    }
                    if (GUILayout.Button("Delete", "toolbarbutton", GUILayout.Width(60)))
                    {
                        DeleteData();
                    }

                    if (GUILayout.Button("Save", "toolbarbutton", GUILayout.Width(60)))
                    {
                        SaveData();
                    }
                    GUILayout.FlexibleSpace();

                    m_IsShowDesc = EditorGUILayout.ToggleLeft("Show Desc", m_IsShowDesc);

                    GUILayout.Label($"  Flyer ID:{m_FlyerID},Next Action:{m_ActionIndex}  ", EditorStyles.toolbarTextField);

                    if (GUILayout.Button("Refresh", "toolbarbutton", GUILayout.Width(60)))
                    {
                        OnEnable();
                        Repaint();
                    }

                }
                EditorGUILayout.EndHorizontal();
            }
            GUILayout.EndArea();
        }

        private Vector2 m_ScrollListPos = Vector2.zero;
        private void DrawConfigList(Rect configListRect)
        {
            GUILayout.BeginArea(configListRect);
            {
                EditorGUILayout.LabelField("Flyer Data List", boldCenterStyle, GUILayout.Width(configListRect.width));

                m_ScrollListPos = EditorGUILayout.BeginScrollView(m_ScrollListPos, false, true, GUILayout.Width(configListRect.width));
                {
                    foreach (var kvp in m_DataDic)
                    {
                        if (!string.IsNullOrEmpty(m_SelectedDataFullPath) && kvp.Key == m_SelectedDataFullPath)
                        {
                            EditorGUIUtil.BeginGUIBackgroundColor(Color.cyan);
                            {
                                if (GUILayout.Button(Path.GetFileNameWithoutExtension(kvp.Key)))
                                {
                                    SelectedDataFullPath = kvp.Key;
                                }
                            }
                            EditorGUIUtil.EndGUIBackgroundColor();
                        }
                        else
                        {
                            if (GUILayout.Button(Path.GetFileNameWithoutExtension(kvp.Key)))
                            {
                                SelectedDataFullPath = kvp.Key;
                            }
                        }
                    }
                }
                EditorGUILayout.EndScrollView();
            }
            GUILayout.EndArea();
        }

        private int m_TabPageSelectedIndex = 0;
        private void DrawTabPages(Rect tabPageRect)
        {
            GUILayout.BeginArea(tabPageRect);
            {
                m_TabPageSelectedIndex = GUILayout.Toolbar(m_TabPageSelectedIndex, m_TabPageContents, GUILayout.Height(tabPageRect.height));
            }
            GUILayout.EndArea();
        }

        private void DrawTabContent(Rect contentRect)
        {
            if (!string.IsNullOrEmpty(m_SelectedDataFullPath))
            {
                m_TabPages[m_TabPageSelectedIndex].OnGUI(contentRect);
            }
        }

        private string m_DataPath = "../eternity_configurations/share/data/skill/flyerdata";
        private Dictionary<string, FlyerData> m_DataDic = new Dictionary<string, FlyerData>();
        private void LoadData()
        {
            m_DataDic.Clear();

            DirectoryInfo dInfo = new DirectoryInfo(m_DataPath);
            if (dInfo.Exists)
            {
                FileInfo[] fileInfos = dInfo.GetFiles("*.json");
                if (fileInfos != null && fileInfos.Length > 0)
                {
                    foreach (var fi in fileInfos)
                    {
                        StreamReader sr = fi.OpenText();
                        string content = sr.ReadToEnd();
                        sr.Close();

                        FlyerData flyerData = JsonDataReader.ReadObjectFromJson<FlyerData>(content);
                        m_DataDic.Add(fi.FullName, flyerData);
                    }
                }
            }
        }

        private void FindIDAndIndex()
        {
            m_FlyerID = -1;
            m_ActionIndex = -1;

            if (string.IsNullOrEmpty(SelectedDataFullPath))
            {
                return;
            }

            string fileName = Path.GetFileNameWithoutExtension(SelectedDataFullPath);
            string skillIDRegex = @"flyer_(?<id>[0-9]+)";
            Match match = new Regex(skillIDRegex).Match(fileName);
            if (match.Groups["id"].Success)
            {
                m_FlyerID = int.Parse(match.Groups["id"].Value);
            }
            else
            {
                Debug.LogError("Flyer id is error");
                return;
            }

            FlyerData data = m_DataDic[SelectedDataFullPath];
            List<ActionData> actions = new List<ActionData>();
            foreach (var track in data.FlyingData.Group.Tracks)
            {
                actions.AddRange(track.Actions);
            }
            foreach (var track in data.EndData.Group.Tracks)
            {
                actions.AddRange(track.Actions);
            }
            if (actions.Count > 0)
            {
                int maxActionIndex = (from action in actions select action.Index).ToList().Max();
                string tempStr = maxActionIndex.ToString();
                maxActionIndex = int.Parse(tempStr.Substring(tempStr.Length - 3, 3));
                if (maxActionIndex + 100 > 999)
                {
                    foreach (var action in actions)
                    {
                        m_ActionIndex++;
                        action.Index = m_FlyerID * 1000 + m_ActionIndex;
                    }
                }
                else
                {
                    m_ActionIndex = maxActionIndex + 1;
                }
            }
        }

        public int GetActionIndex()
        {
            return m_FlyerID * 1000 + (++m_ActionIndex);
        }

        public bool IsShowDesc()
        {
            return m_IsShowDesc;
        }

        private void CreateData()
        {
            CreateDataPopWindow.ShowWin(new Rect(position.x + position.width * .5f - 100, position.y + position.height * .5f - 75, 200, 150), (id) =>
            {
                FlyerData skillData = new FlyerData();
                string fullPath = $"{m_DataPath}/flyer_{id}.json";
                m_DataDic.Add(fullPath, skillData);
                SelectedDataFullPath = fullPath;
            }, (id) =>
            {
                if (id <= 10000)
                {
                    return false;
                }

                string fullPath = $"{m_DataPath}/skill_{id}.json";
                foreach (var kvp in m_DataDic)
                {
                    if (Path.GetFileNameWithoutExtension(kvp.Key) == fullPath)
                    {
                        return false;
                    }
                }
                return true;
            });
        }

        private void DeleteData()
        {
            if (string.IsNullOrEmpty(SelectedDataFullPath))
            {
                return;
            }

            if (EditorUtility.DisplayDialog("Warning", $"The config will be delete.\n{SelectedDataFullPath}\nAre you sure?", "OK", "Cancel"))
            {
                m_DataDic.Remove(SelectedDataFullPath);
                File.Delete(SelectedDataFullPath);
                SelectedDataFullPath = string.Empty;

                Repaint();
            }
        }

        private void SaveData()
        {
            if (string.IsNullOrEmpty(SelectedDataFullPath))
            {
                return;
            }

            FlyerData data = m_DataDic[SelectedDataFullPath];
            foreach (var track in data.FlyingData.Group.Tracks)
            {
                track.Actions.Sort((item1, item2) => { return item1.FireTime.CompareTo(item2.FireTime); });
            }
            foreach (var track in data.EndData.Group.Tracks)
            {
                track.Actions.Sort((item1, item2) => { return item1.FireTime.CompareTo(item2.FireTime); });
            }
            
            string json = JsonDataWriter.WriteObjectToJson(data);
            File.WriteAllText(SelectedDataFullPath, json);
        }
    }
}
