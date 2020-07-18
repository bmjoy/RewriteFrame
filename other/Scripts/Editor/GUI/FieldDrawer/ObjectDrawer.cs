using LeyoutechEditor.Core.EGUI;
using UnityEditor;
using SystemObject = System.Object;

namespace LeyoutechEditor.EGUI.FieldDrawer
{
    public class ObjectDrawer
    {
        private string m_Title = string.Empty;
        private SystemObject m_Data = null;
        private FieldData[] m_FieldDatas = null;

        public ObjectDrawer(string title,SystemObject data)
        {
            this.m_Title = title;
            this.m_Data = data;
            if(data!=null)
            {
                m_FieldDatas = FieldDrawerUtil.GetTypeFieldDrawer(data.GetType());
                foreach(var fd in m_FieldDatas)
                {
                    if(fd.Drawer!=null)
                    {
                        fd.Drawer.SetData(data);
                    }
                }
            }
        }
        
        public void OnGUI(bool isShowDesc = false)
        {
            bool isNeedIndent = false;
            if(!string.IsNullOrEmpty(m_Title))
            {
                EditorGUILayout.LabelField(m_Title);
                isNeedIndent = true;
            }

            if(isNeedIndent)
            {
                EditorGUIUtil.BeginIndent();
            }

            if (m_Data == null)
            {
                EditorGUILayout.HelpBox("Data is null", MessageType.Error);
            }
            else
            {
                foreach (var fieldData in m_FieldDatas)
                {
                    if (fieldData.Drawer == null)
                    {
                        EditorGUILayout.LabelField(fieldData.Name, "Drawer is null");
                    }
                    else
                    {
                        fieldData.Drawer.DrawField(isShowDesc);
                    }
                }
            }

            if (isNeedIndent)
            {
                EditorGUIUtil.EndIndent();
            }
        }
    }
}
