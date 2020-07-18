using Leyoutech.Core.Timer;
using Leyoutech.Core.Util;
using UnityEngine;

namespace Leyoutech
{
    /// <summary>
    /// 初始化入口，用于控制Update的执行
    /// </summary>
    public class GameController : MonoBehaviour
    {
        private static GameController sm_GameController = null;
        public static void StartUp()
        {
            if(sm_GameController == null)
            {
                DontDestroyHandler.CreateComponent<GameController>();
            }
        }

        public static GameController GetController()
        {
            return sm_GameController;
        }

        private TimerManager timerMgr = null;
        private void Awake()
        {
            if(sm_GameController!=null)
            {
                Destroy(this);
                return;
            }
            sm_GameController = this;

            timerMgr = TimerManager.GetInstance();
            
            CRenderer.RendererManager.GetInstance();
		}

		private void Update()
		{
			float deltaTime = Time.deltaTime;
			// for performance
			float invertDeltaTime = 1.0f / deltaTime;

			timerMgr.DoUpdate(deltaTime);

            //GameFPSControl.GetInstance().OnUpdate(deltaTime);

            Leyoutech.Core.Loader.AssetManager.GetInstance().DoUpdate(deltaTime);

            CRenderer.RendererManager.GetInstance().DoUpdate(deltaTime, invertDeltaTime);
		}
	}
}
