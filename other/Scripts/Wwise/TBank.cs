/*===============================
 * Author: [Allen]
 * Purpose: 音效库
 * Time: 2019/05/07  17:27
================================*/
using AK.Wwise;
using System;

namespace AK.Wwise
{

    public class TBank : Bank
    {
        /// <summary>
        /// 音效库名字
        /// </summary>
        private string TName;

        public TBank(string name)
        {
            TName = name;
        }

        /// <summary>
        /// 是否可用
        /// </summary>
        /// <returns></returns>
        public override bool IsValid()
        {
            return true;
        }

        /// <summary>
        /// 音效库名字
        /// </summary>
        public override string Name
        {
            get
            {
                if (string.IsNullOrEmpty(TName))
                    return base.Name;

                return TName;
            }
        }

        /// <summary>
        /// 加载库
        /// </summary>
        /// <param name="decodeBank">解码</param>
        /// <param name="saveDecodedBank">保存解码</param>
        /// <param name="LoadSucceedCallback">加载完毕回调</param>
        public void TLoad(bool decodeBank = false, bool saveDecodedBank = false, Action<AKRESULT> LoadSucceedCallback = null)
        {
            if (IsValid())
                AkBankManager.LoadBank(Name, decodeBank, saveDecodedBank, LoadSucceedCallback);
        }
    }
}
