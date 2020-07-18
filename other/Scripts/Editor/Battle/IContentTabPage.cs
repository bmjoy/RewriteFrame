using UnityEngine;

namespace EternityEditor.Battle
{
    public interface IContentTabPage
    { 
        void SetData(object data);
        void OnGUI(Rect rect);
        int GetActionIndex();
        void OnRepaint();
    }
}
