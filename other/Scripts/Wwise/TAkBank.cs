/*===============================
 * Author: [Allen]
 * Purpose: 音效库
 * Time: 2019/05/05  19:21
================================*/
using UnityEngine;

namespace AK.Wwise
{
    /// <summary>
    /// 注意在 Awake 之前执行
    /// </summary>
    public class TAkBank : AkBank
    {
        /// <summary>
        /// 库名字
        /// </summary>
        private string tBankName = string.Empty;

        /// <summary>
        /// 设置 Bank 库名字
        /// </summary>
        /// <param name="_bankName"></param>
        public void SetBankName(string _bankName)
        {
            tBankName = _bankName;
        }

        protected override void Awake()
        {
            triggerList = new System.Collections.Generic.List<int> { START_TRIGGER_ID };
            unloadTriggerList = new System.Collections.Generic.List<int> { DESTROY_TRIGGER_ID };

            if (string.IsNullOrEmpty(tBankName))
            {
                Debug.LogWarning("bankName 为空，请注意在Awake 执行前设置！！");
            }
            else
                data = new TBank(tBankName);
            base.Awake();
        }

        public override void HandleEvent(GameObject in_gameObject)
        {
            if (!loadAsynchronous)
            {
                TBank tdate = data as TBank;
                tdate.TLoad(decodeBank, saveDecodedBank, LoadOverDetector);
                // FormAddressablesToLoadBank();
            }
            else
                data.LoadAsync();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            tBankName = string.Empty;
        }
    }
}