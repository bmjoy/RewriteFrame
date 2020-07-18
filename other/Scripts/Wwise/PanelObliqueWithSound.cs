/*===============================
 * Author: [Allen]
 * Purpose: 面板&倾斜声效控制
 * Time: 2019/8/14 18:34:35
================================*/
using UnityEngine;

namespace UnityEngine.UI
{
    [AddComponentMenu("UI/PanelObliqueWithSound 面板切斜声效脚本")]
    [ExecuteInEditMode]
    public class PanelObliqueWithSound : MonoBehaviour
    {
        [SerializeField]
        private bool isOblique = false;                     //是否是倾斜面板

        [SerializeField]
        private RectTransform ObliqueRectTransform;  //倾斜参考计算的 RectTransform

        [SerializeField]
        private int SoundPlayID = -1;                        //Sound倾斜音效 播放 ID

        [SerializeField]
        private int SoundStopID = -1;                         //Sound倾斜音效 停止 ID

        [SerializeField]
        private int Rcpc = -1;                                        //控制RTPC 倾斜参数 ID

        [SerializeField]
        private int PanelOpenSoundID = -1;              //面板打开时 音效ID

        [SerializeField]
        private int PanelCloseSoundID = -1;              //面板关闭时 音效ID


        private float valueOff = 10.0f;                          //比例值系数


        void Awake()
        {
        }

        private void OnEnable()
        {
            if (Application.isPlaying && isOblique)
                WwiseUtil.PlaySound(SoundPlayID, false, null);
        }

        private void OnDisable()
        {
            if (Application.isPlaying && isOblique)
                WwiseUtil.PlaySound(SoundStopID, false, null);
        }

        /// <summary>
        /// 播放面板打开音效
        /// </summary>
        public void PlayPanelOpenSound()
        {
            if(PanelOpenSoundID >0)
                WwiseUtil.PlaySound(PanelOpenSoundID, false, null);
        }

        /// <summary>
        /// 播放面板关闭音效
        /// </summary>
        public void PlayPanelCloseSound()
        {
            if (PanelCloseSoundID > 0)
                WwiseUtil.PlaySound(PanelCloseSoundID, false, null);
        }

        private void Update()
        {
            if (!Application.isPlaying || !isOblique || ObliqueRectTransform == null)
                return;

            Camera canvasCamera = CameraManager.GetInstance().GetUICameraComponent().GetCamera();
            Vector2 mousePosition = InputManager.Instance.GetCurrentVirtualCursorPos();
            Vector2 mousePos_U;
            if (canvasCamera != null && RectTransformUtility.ScreenPointToLocalPointInRectangle(ObliqueRectTransform, mousePosition, canvasCamera, out mousePos_U))
            {
                float x = mousePos_U.x; // mousePosition.x - transfomPosition.x;     //鼠标点相对于 面板坐标点的 位置
                float y = mousePos_U.y; // mousePosition.y - transfomPosition.y;

                float width = ObliqueRectTransform.sizeDelta.x / 2;      //面板宽度
                float height = ObliqueRectTransform.sizeDelta.y / 2;

                float Xmin = Mathf.Min(Mathf.Abs(x), width);      //鼠标 x ，跟 w 取最小值
                float Ymin = Mathf.Min(Mathf.Abs(y), height);

                float xyDis = Vector2.Distance(new Vector2(Xmin, Ymin), Vector2.zero);          //xy 距离
                float whDis = Vector2.Distance(new Vector2(width, height), Vector2.zero);     //面板最远处距离

                float value = whDis == 0 ? 0 : xyDis / whDis;

                value = value * valueOff;

                //Debug.LogWarning("---------0-----------> mousePosition = " + mousePosition + "  mousePos_U = " + mousePos_U);
                //Debug.LogWarning("---------1-----------> x = " + x + "  y = " + y + " width=" + width + " height=" + height + " Xmin=" + Xmin + " Ymin=" + Ymin);
                //Debug.LogWarning("---------2-----------> xyDis = " + xyDis + "  whDis = " + whDis + " value=" + value);

                if (Rcpc > 0)
                    WwiseManager.SetParameter(Rcpc, value );
            }
        }
    }
}

