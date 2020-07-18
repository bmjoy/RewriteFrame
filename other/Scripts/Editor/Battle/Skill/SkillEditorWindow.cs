using Eternity.Share.Config;
using Eternity.Share.Config.ActionDatas;
using Eternity.Share.Config.SkillDatas;
using LeyoutechEditor.Core.EGUI;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace EternityEditor.Battle.Skill
{
    public class SkillEditorWindow : EditorWindow
    {
        [MenuItem("Custom/Battle/Skill Editor")]
        public static void ShowWin()
        {
            var win = EditorWindow.GetWindow<SkillEditorWindow>();
            win.titleContent = new GUIContent("Skill Editor");
            win.Show();
        }
        private static readonly int TOOLBAR_HEIGHT = 20;
        private static readonly int CONFIG_LIST_WIDTH = 120;
        private static readonly int TAB_PAGE_HEIGHT = 50;

        private GUIContent[] m_TabPageContents = new GUIContent[]
        {
            new GUIContent("Base"),
            new GUIContent("Begin Stage"),
            new GUIContent("Release Stage"),
            new GUIContent("End Stage"),
        };
        private IContentTabPage[] m_TabPages = new IContentTabPage[4];

        private int m_SkillID = 0;
        private int m_ActionIndex = 0;

        private string m_SelectedSkillFullPath = string.Empty;
        public string SelectedSkillFullPath
        {
            get
            {
                return m_SelectedSkillFullPath;
            }
            set
            {
                if(m_SelectedSkillFullPath!=value)
                {
                    m_SelectedSkillFullPath = value;

                    FindIDAndActionIndex();
                    if (!string.IsNullOrEmpty(m_SelectedSkillFullPath))
                    {
                        SkillData data = m_DataDic[m_SelectedSkillFullPath];

                        if(data.Id != m_SkillID)
                        {
                            data.Id = m_SkillID;
                        }

                        m_TabPages[0].SetData(data.BaseData);
                        m_TabPages[1].SetData(data.BeginStageData);
                        m_TabPages[2].SetData(data.ReleaseStageData);
                        m_TabPages[3].SetData(data.EndStageData);
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
            m_SelectedSkillFullPath = string.Empty;

            m_TabPages = new IContentTabPage[4];
            m_TabPages[0] = new SkillBaseContentTab(this);
            m_TabPages[1] = new SkillBeginStageContentTab(this);
            m_TabPages[2] = new SkillReleaseStageContentTab(this);
            m_TabPages[3] = new SkillEndStageContentTab(this);
        }

        private void OnGUI()
        {
            Rect toolbarRect = new Rect(0, 0, position.width, TOOLBAR_HEIGHT);
            DrawToolbar(toolbarRect);

            Rect configListRect = new Rect(1, TOOLBAR_HEIGHT, CONFIG_LIST_WIDTH, position.height - TOOLBAR_HEIGHT-1);
            EditorGUIUtil.DrawAreaLine(configListRect, Color.gray);

            configListRect.x += 2;
            configListRect.y += 2;
            configListRect.width -= 4;
            configListRect.height -= 4;
            DrawConfigList(configListRect);

            Rect tabPageRect = new Rect(CONFIG_LIST_WIDTH, TOOLBAR_HEIGHT, position.width - CONFIG_LIST_WIDTH, TAB_PAGE_HEIGHT);
            DrawTabPages(tabPageRect);

            Rect tabContentRect = new Rect(CONFIG_LIST_WIDTH, TOOLBAR_HEIGHT + TAB_PAGE_HEIGHT, position.width - CONFIG_LIST_WIDTH-1, position.height - TAB_PAGE_HEIGHT - TOOLBAR_HEIGHT-1);
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
                    GUILayout.Label($"  Skill ID:{m_SkillID},Next Action:{m_ActionIndex}  ",EditorStyles.toolbarTextField);

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
                EditorGUILayout.LabelField("Skill Data List", boldCenterStyle,GUILayout.Width(configListRect.width));

                m_ScrollListPos = EditorGUILayout.BeginScrollView(m_ScrollListPos,false,true, GUILayout.Width(configListRect.width));
                {
                    foreach(var kvp in m_DataDic)
                    {
                        if(!string.IsNullOrEmpty(m_SelectedSkillFullPath) && kvp.Key == m_SelectedSkillFullPath)
                        {
                            EditorGUIUtil.BeginGUIBackgroundColor(Color.cyan);
                            {
                                if (GUILayout.Button(Path.GetFileNameWithoutExtension(kvp.Key)))
                                {
                                    SelectedSkillFullPath = kvp.Key;
                                }
                            }
                            EditorGUIUtil.EndGUIBackgroundColor();
                        }else
                        {
                            if (GUILayout.Button(Path.GetFileNameWithoutExtension(kvp.Key)))
                            {
                                SelectedSkillFullPath = kvp.Key;
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
                m_TabPageSelectedIndex = GUILayout.Toolbar(m_TabPageSelectedIndex, m_TabPageContents,GUILayout.Height(tabPageRect.height));
            }
            GUILayout.EndArea();
        }

        private void DrawTabContent(Rect contentRect)
        {
            if(!string.IsNullOrEmpty(m_SelectedSkillFullPath))
            {
                m_TabPages[m_TabPageSelectedIndex].OnGUI(contentRect);
            }
        }

        private string m_DataPath = "../eternity_configurations/share/data/skill/skilldata";
        private Dictionary<string, SkillData> m_DataDic = new Dictionary<string, SkillData>();
        private void LoadData()
        {
            m_DataDic.Clear();

            DirectoryInfo dInfo = new DirectoryInfo(m_DataPath);
            if(dInfo.Exists)
            {
                FileInfo[] fileInfos = dInfo.GetFiles("*.json");
                if(fileInfos!=null && fileInfos.Length>0)
                {
                    foreach(var fi in fileInfos)
                    {
                        StreamReader sr = fi.OpenText();
                        string content = sr.ReadToEnd();
                        sr.Close();

                        SkillData skillData = JsonDataReader.ReadObjectFromJson<SkillData>(content);
                        m_DataDic.Add(fi.FullName, skillData);
                    }
                }
            }
        }

        private void FindIDAndActionIndex()
        {
            m_SkillID = -1;
            m_ActionIndex = -1;

            if (string.IsNullOrEmpty(SelectedSkillFullPath))
            {
                return;
            }

            string fileName = Path.GetFileNameWithoutExtension(SelectedSkillFullPath);
            string skillIDRegex = @"skill_(?<id>[0-9]+)";
            Match match = new Regex(skillIDRegex).Match(fileName);
            if (match.Groups["id"].Success)
            {
                m_SkillID = int.Parse(match.Groups["id"].Value);
            }
            else
            {
                Debug.LogError("Skill id is error");
                return;
            }

            SkillData data = m_DataDic[SelectedSkillFullPath];
            List<ActionData> actions = new List<ActionData>();
            foreach (var track in data.BeginStageData.Group.Tracks)
            {
                actions.AddRange(track.Actions);
            }
            foreach (var track in data.BeginStageData.BreakGroup.Tracks)
            {
                actions.AddRange(track.Actions);
            }
            foreach (var childData in data.ReleaseStageData.Childs)
            {
                foreach (var track in childData.Group.Tracks)
                {
                    actions.AddRange(track.Actions);
                }
                foreach (var track in childData.BreakGroup.Tracks)
                {
                    actions.AddRange(track.Actions);
                }
            }
            foreach (var track in data.EndStageData.Group.Tracks)
            {
                actions.AddRange(track.Actions);
            }
            foreach (var track in data.EndStageData.BreakGroup.Tracks)
            {
                actions.AddRange(track.Actions);
            }
            if(actions.Count>0)
            {
                int maxActionIndex = (from action in actions select action.Index).ToList().Max();
                string tempStr = maxActionIndex.ToString();
                maxActionIndex = int.Parse(tempStr.Substring(tempStr.Length - 3, 3));
                if (maxActionIndex + 100 > 999)
                {
                    foreach (var action in actions)
                    {
                        m_ActionIndex++;
                        action.Index = m_SkillID * 1000 + m_ActionIndex;
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
            return m_SkillID * 1000 + (++m_ActionIndex);
        }

        public bool IsShowDesc()
        {
            return m_IsShowDesc;
        }

        private void CreateData()
        {
            CreateDataPopWindow.ShowWin(new Rect(position.x + position.width * .5f - 100, position.y + position.height * .5f - 75, 200, 150), (id) =>
            {
                SkillData skillData = new SkillData();
                string fullPath = $"{m_DataPath}/skill_{id}.json";
                m_DataDic.Add(fullPath, skillData);
                SelectedSkillFullPath = fullPath;
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
            if(string.IsNullOrEmpty(SelectedSkillFullPath))
            {
                return;
            }

            if(EditorUtility.DisplayDialog("Warning",$"The config will be delete.\n{SelectedSkillFullPath}\nAre you sure?","OK","Cancel"))
            {
                m_DataDic.Remove(SelectedSkillFullPath);
                File.Delete(SelectedSkillFullPath);
                SelectedSkillFullPath = string.Empty;

                Repaint();
            }
        }

        private void SaveData()
        {
            if(string.IsNullOrEmpty(SelectedSkillFullPath))
            {
                return;
            }

            SkillData data = m_DataDic[SelectedSkillFullPath];
            foreach (var track in data.BeginStageData.Group.Tracks)
            {
                track.Actions.Sort((item1, item2) => { return item1.FireTime.CompareTo(item2.FireTime); }) ;
            }
            foreach (var track in data.BeginStageData.BreakGroup.Tracks)
            {
                track.Actions.Sort((item1, item2) => { return item1.FireTime.CompareTo(item2.FireTime); });
            }
            foreach (var childData in data.ReleaseStageData.Childs)
            {
                foreach (var track in childData.Group.Tracks)
                {
                    track.Actions.Sort((item1, item2) => { return item1.FireTime.CompareTo(item2.FireTime); });
                }
                foreach (var track in childData.BreakGroup.Tracks)
                {
                    track.Actions.Sort((item1, item2) => { return item1.FireTime.CompareTo(item2.FireTime); });
                }
            }
            foreach (var track in data.EndStageData.Group.Tracks)
            {
                track.Actions.Sort((item1, item2) => { return item1.FireTime.CompareTo(item2.FireTime); });
            }
            foreach (var track in data.EndStageData.BreakGroup.Tracks)
            {
                track.Actions.Sort((item1, item2) => { return item1.FireTime.CompareTo(item2.FireTime); });
            }

            string json = JsonDataWriter.WriteObjectToJson(data);
            File.WriteAllText(SelectedSkillFullPath, json);
        }
    }
}
