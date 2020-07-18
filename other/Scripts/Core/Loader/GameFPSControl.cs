/*===============================
 * Author: [Allen]
 * Purpose: 资源加载Fps 自动控制脚本
 * Time: 2019/11/4 14:33:22
================================*/
using Leyoutech.Core.Loader;
using UnityEngine;

public class GameFPSControl : Leyoutech.Core.Util.Singleton<GameFPSControl>
    {
        /// <summary>
        /// 测量增量时间
        /// </summary>
        private float fpsMeasuringDeltaTime = 2.0f;


        /// <summary>
        /// 测量增量帧个数
        /// </summary>
        private int fpsMeasuringDeltaCout = 5;

        /// <summary>
        /// 经过时间
        /// </summary>
        private float timePassed = 0.0f;

        /// <summary>
        /// 帧数
        /// </summary>
        private int m_FrameCount = 0;

        /// <summary>
        ///当前fps
        /// </summary>
        private float m_currFPS = 0.0f;


        #region Loader

        /// <summary>
        /// 是否有基础帧率了
        /// </summary>
        private bool m_HaveBase = false;

        /// <summary>
        /// 标准fps
        /// </summary>
        private float m_baseFps = 30f;

        private int m_Scout = 0;
        /// <summary>
        /// 取样个数
        /// </summary>
        private int  m_SampleCout = 10;

        /// <summary>
        /// 取样总和
        /// </summary>
        private float m_SampleFpsTotal = 0f;


        #endregion


        public void OnUpdate(float deltaTime)
        {
            m_FrameCount = m_FrameCount + 1;
            timePassed = timePassed + deltaTime;

            //算出基本平衡帧率
            if(timePassed > fpsMeasuringDeltaTime  && !m_HaveBase )
            {
                m_currFPS = m_FrameCount / timePassed;

                timePassed = 0.0f;
                m_FrameCount = 0;

                if (m_Scout < m_SampleCout)
                {
                    m_Scout++;
                    m_SampleFpsTotal += m_currFPS;
                }
                else
                {
                    m_HaveBase = true;
                    m_baseFps = m_SampleFpsTotal / m_SampleCout;
                }
            }


            //5帧一次计算加载数量
            if (m_FrameCount > fpsMeasuringDeltaCout && m_HaveBase)
            {
                m_currFPS = m_FrameCount / timePassed;

                timePassed = 0.0f;
                m_FrameCount = 0;
                AutoGetToLoaderMaxCout();
            }
        }

        //public  void OnGUI()
        //{
        //    GUIStyle bb = new GUIStyle();
        //    bb.normal.background = null;    //这是设置背景填充的
        //    bb.normal.textColor = new Color(1.0f, 0.5f, 0.0f);   //设置字体颜色的
        //    bb.fontSize = 20;       //当然，这是字体大小

        //    //居中显示FPS
        //    GUI.Label(new Rect((Screen.width / 2) -300, 0, 200, 200), "m_baseFps:"+ m_baseFps+" FPS: " + m_currFPS + " MaxCout :" + AssetManager.GetInstance().MaxLoadingCount, bb);
        //}


        /// <summary>
        /// 自动获取当前适合的最大 加载数量
        /// </summary>
        private void AutoGetToLoaderMaxCout()
        {
            int AssetloadMaxCout = AssetManager.GetInstance().MaxLoadingCount;

            if(m_currFPS > m_baseFps) //比如 大于 30 ++
            {
                //  m_currFPS > 30  
                //加加
                AssetloadMaxCout++;
                AssetloadMaxCout = Mathf.Min(AssetloadMaxCout, 50);
                AssetManager.GetInstance().MaxLoadingCount = AssetloadMaxCout;
            }
            else if(m_currFPS > m_baseFps *0.8f)
            {
                // 30 *0.8f <m_currFPS <30
                //不 增加
            }
            else if(m_currFPS< m_baseFps *0.6f)
            {
                // m_currFPS <30 *0.6f   
                //减减
                AssetloadMaxCout --;
                AssetloadMaxCout = Mathf.Max(AssetloadMaxCout, 5);
                AssetManager.GetInstance().MaxLoadingCount = AssetloadMaxCout;
            }
        }
    }

