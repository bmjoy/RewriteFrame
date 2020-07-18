/*===============================
 * Author: [Allen]
 * Purpose: ButtonWithSound1
 * Time: 2019/7/11 10:20:52
================================*/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UnityEngine.UI
{
    [AddComponentMenu("UI/ButtonWithSound 音效按钮挂载脚本")]
    [ExecuteInEditMode]
    public class ButtonWithSound : MonoBehaviour, IPointerClickHandler ,IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, ISelectHandler
    {
        public enum UISoudEnum
        {
            [Description("无效声")]
            None = 0,
            [Description("通用点击 ID = 16(页签or 按钮)")]
            button_1 = WwiseMusic.Music_Button_Click_1,
            [Description("通用点击 ID = 17(列表Item)")]
            button_2 = WwiseMusic.Music_Button_Click_2,
            [Description("通用点击 ID = 18 (方向键选中)")]
            button_3 = WwiseMusic.Music_Button_SelectMove_valid,
            [Description("通用点击 ID = 19 (方向键越界)")]
            button_4 = WwiseMusic.Music_Button_SelectMove_invalid,
            [Description("通用点击 ID = 30")]
            button_5 = WwiseMusic.Music_WeaponParts_Setup,
            [Description("通用点击 ID = 31")]
            button_6 = WwiseMusic.Music_WeaponParts_Disboard,
            [Description("通用点击 ID = 32")]
            button_7 = WwiseMusic.Music_reborner_Setup,
            [Description("通用点击 ID = 33")]
            button_8 = WwiseMusic.Music_reborner_Disboard,
            [Description("通用点击 ID = 34")]
            button_9 = WwiseMusic.Music_reactor_Setup,
            [Description("通用点击 ID = 35")]
            button_10 = WwiseMusic.Music_reactor_Disboard,
            [Description("通用点击 ID = 36")]
            button_11 = WwiseMusic.Music_robot_Setup,
            [Description("通用点击 ID = 37")]
            button_12 = WwiseMusic.Music_robot_Disboard,
            [Description("通用点击 ID = 38")]
            button_13 = WwiseMusic.Music_ArmorCoating_Setup,
            [Description("通用点击 ID = 39")]
            button_14 = WwiseMusic.Music_ArmorCoating_Disboard,
            [Description("通用点击 ID = 40")]
            button_15 = WwiseMusic.Music_auxiliary_Setup,
            [Description("通用点击 ID = 41")]
            button_16 = WwiseMusic.Music_auxiliary_Disboard,
            [Description("通用点击 ID = 42")]
            button_17 = WwiseMusic.Music_processor_Setup,
            [Description("通用点击 ID = 43")]
            button_18 = WwiseMusic.Music_processor_Disboard,
            [Description("通用点击 ID = 44")]
            button_19 = WwiseMusic.Music_amplifier_Setup,
            [Description("通用点击 ID = 45")]
            button_20 = WwiseMusic.Music_amplifier_Disboard,
            [Description("通用点击 ID = 46")]
            button_21 = WwiseMusic.Music_chip_Setup,
            [Description("通用点击 ID = 47")]
            button_22 = WwiseMusic.Music_chip_Disboard,
            [Description("通用点击 ID = 48")]
            button_23 = WwiseMusic.Music_Compare_Open,
            [Description("通用点击 ID = 49")]
            button_24 = WwiseMusic.Music_Compare_Close,
            [Description("通用点击 ID = 55")]
            button_25 = WwiseMusic.Music_Ship_rebirth_button,
            [Description("通用点击 ID = 56")]
            button_26 = WwiseMusic.Music_human_rebirth_button,
        }

        [SerializeField]
        public List<UISoudEnum> soundIdList = new List<UISoudEnum>();

        private Toggle toggle;

        //特殊索引个数
        private int nSpecial = 2;        // +1 越界声音 +2方向键移动时选中

        public ButtonWithSound()
        {
            for (int i = 0; i <= (int)EventTriggerType.Cancel + nSpecial; i++)
            {
                if (i == (int)EventTriggerType.PointerClick)
                    soundIdList.Add(UISoudEnum.button_1);
                else
                    soundIdList.Add(UISoudEnum.None);
            }
        }

        void Start()
        {
            toggle = gameObject.GetComponent<Toggle>();
        }

        public int GetSpecialNum()
        {
            return nSpecial;
        }

        /// <summary>
        /// 修改声音 ID
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="soundId"></param>
        public UISoudEnum GetSoundID(EventTriggerType eventType)
        {
            return soundIdList[(int)eventType];
        }

        /// <summary>
        /// 修改声音 ID
        /// </summary>
        public void ChangeSoundID(int index, UISoudEnum soundId)
        {
            if (index < 0 || index >= soundIdList.Count)
                return;
            soundIdList[index] = soundId;
        }

        /// <summary>
        /// 修改声音 ID
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="soundId"></param>
        public void ChangeSoundID(EventTriggerType eventType, UISoudEnum soundId)
        {
            soundIdList[(int)eventType] = soundId;
        }

        private void PlaySound(int index)
        {
            if (index < 0 || index >= soundIdList.Count)
                return;

            int soundId = (int)soundIdList[index];
            if (soundId > 0)
            {
                //Debug.LogError("--------------------> soundId = " + soundId + "  eventType = " + (index > (int)EventTriggerType.Cancel ? index.ToString() :  ((EventTriggerType)index).ToString()) + " gameName =" + gameObject.name);
                WwiseUtil.PlaySound(soundId, false, null);
            }
        }

        public  void OnPointerClick(PointerEventData eventData)
        {
            PlaySound((int)EventTriggerType.PointerClick ); 
        }

        public  void OnPointerDown(PointerEventData eventData)
        {
            PlaySound((int)EventTriggerType.PointerDown );
        }

        public  void OnPointerEnter(PointerEventData eventData)
        {
            PlaySound((int)EventTriggerType.PointerEnter );
        }

        public  void OnPointerExit(PointerEventData eventData)
        {
            PlaySound((int)EventTriggerType.PointerExit );
        }

        public  void OnPointerUp(PointerEventData eventData)
        {
            PlaySound((int)EventTriggerType.PointerUp );
        }

        public  void OnSelect(BaseEventData eventData)
        {
//             if (/*(toggle != null && toggle.isOn)||*/ !InputManager.Instance.GetNavigateMode())
//             {
//                 return;
//             }

            //导航模式下，左右键选中
            PlaySound((int)EventTriggerType.Cancel +2);
        }

        /// <summary>
        /// 越界声音
        /// </summary>
        public void PlayOutLineSound()
        {
            PlaySound((int)EventTriggerType.Cancel +1);
        }

        /// <summary>
        /// 越界声音 消息
        /// </summary>
        /// <param name="obj"></param>
        public static void Msg_PlayOutLineSound(GameObject obj)
        {
            MsgPlayMusicOrSound_outLine parame = new MsgPlayMusicOrSound_outLine();
            parame.OldSelectionObj = obj;
            GameFacade.Instance.SendNotification(NotificationName.MSG_SOUND_PLAY, parame);
        }
    }
}