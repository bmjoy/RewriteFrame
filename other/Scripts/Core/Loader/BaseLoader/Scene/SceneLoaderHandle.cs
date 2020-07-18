using UnityEngine;
using UnityEngine.SceneManagement;

namespace Leyoutech.Core.Loader
{
    /// <summary>
    ///加载场景的Handle
    /// </summary>
    public sealed class SceneLoaderHandle
    {
        public string PathOrAddress { get; set; }
        public string AssetPath { get; set; }
        public string SceneName { get; set; }
        public float Progress { get; set; } = 0.0f;

        private Scene m_Scene;
        private bool m_IsActive = true;
        public bool IsActive
        {
            get
            {
                return m_IsActive;
            }
            set
            {
                if(m_IsActive!=value)
                {
                    m_IsActive = value;
                    if(m_Scene.isLoaded)
                    {
                        SetSceneActive(m_IsActive);
                    }
                }
            }
        }

        internal void SetScene(Scene scene)
        {
            m_Scene = scene;
            if(!m_IsActive)
            {
                SetSceneActive(m_IsActive);
            }
        }
        /// <summary>
        /// 激活场景
        /// </summary>
        /// <param name="isActive"></param>
        private void SetSceneActive(bool isActive)
        {
            GameObject[] gObjs = m_Scene.GetRootGameObjects();
            foreach (var go in gObjs)
            {
                go.SetActive(isActive);
            }
        }
    }
}

