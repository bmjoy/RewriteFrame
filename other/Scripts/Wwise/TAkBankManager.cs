/*===============================
 * Author: [Allen]
 * Purpose: TAkBankManager
 * Time: 2019/5/27 15:51:51
================================*/

using System.Collections.Generic;
using UnityEngine;

namespace AK.Wwise
{
    public class TAkBankManager : MonoBehaviour
    {
        private List<AkBank> m_AkBankList;
        private Transform bankObj = null;
        private Transform musicObj = null;

        /// <summary>
        /// 加载计数
        /// </summary>
        private int LoadOverCout = 0;

        private void Awake()
        {
            bankObj = transform.GetChild(0);
            if (bankObj == null)
                return;
            AkBank[] akbankArray = bankObj.GetComponents<AkBank>();
            if (akbankArray == null || akbankArray.Length == 0)
                return;
            m_AkBankList = new List<AkBank>(akbankArray);

            for (int i = 0; i < m_AkBankList.Count; i++)
            {
                AkBank bank = m_AkBankList[i];
                bank.Completed = FirstBankListLoadOverDetector;
            }

            bankObj.gameObject.SetActive(true);
        }

        /// <summary>
        /// 加载检查
        /// </summary>
        private void FirstBankListLoadOverDetector()
        {
            LoadOverCout++;

            if (LoadOverCout == m_AkBankList.Count)
            {
                Debug.Log("---------------> 无代码逻辑的 bank加载完毕");

                musicObj = transform.GetChild(1);
                if (musicObj == null)
                    return;

                musicObj.gameObject.SetActive(true);
            }
        }

    }
}

