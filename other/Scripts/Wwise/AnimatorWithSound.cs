/*===============================
 * Author: [Allen]
 * Purpose: 普通动画挂接脚本执行帧事件声音 
 * Time: 2019/7/16 14:59:02
================================*/
//用于普通UI 动画，休闲NPC 动画
//无额外程序逻辑的

using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.UI
{
    [AddComponentMenu("UI/AnimatorWithSound 动画帧事件音效挂载脚本")]
    [ExecuteInEditMode]
    public class AnimatorWithSound : MonoBehaviour
    {
        private bool isMain = false; //主角

        private List<int> m_SoundIdList;

        [SerializeField]
        public List<SoundIdItem> SoundIditemList = new List<SoundIdItem>();

        [SerializeField]
        private bool isEntity = false;

        private bool isFind = false;


        [System.Serializable]
        public class SoundIdItem
        {
            /// <summary>
            /// 音效ID
            /// </summary>
            public int SoundID; 
            /// <summary>
            /// 音效描述
            /// </summary>
            public string SoundDis;
        }

        private List<int>  GetSoundList()
        {
            if (Application.isPlaying)
            {
                if (m_SoundIdList == null)
                {
                    m_SoundIdList = new List<int>();
                    for (int i = 0; i < SoundIditemList.Count; i++)
                    {
                        SoundIdItem item = SoundIditemList[i];
                        m_SoundIdList.Add(item.SoundID);
                    }
                }
                return m_SoundIdList;
            }
            return null;
        }


        public void PlaySound(int soundId)
        {
            if (GetSoundList()!=null && GetSoundList().Contains(soundId))
            {
                //Debug.LogError("-------PlaySound----->id = " + soundId);
                WwiseUtil.PlaySound(soundId, false, transform);
            }
        }


        /// <summary>
        /// 模型角色使用的
        /// 参数举例   100_101,第一人称播放100，第三人称播放 101
        /// </summary>
        /// <param name="Id_1st_or_Id_3st">第一人称_第三人称</param>
        public void PlayEntitySound(string Id_1st_or_Id_3st)
        {
            if (!isEntity || string.IsNullOrEmpty(Id_1st_or_Id_3st))
                return;

            ToFind();

            string[] strList = Id_1st_or_Id_3st.Split('_');
            if (strList.Length != 2)
                return;

            int Id = -1;
            if (int.TryParse(isMain ? strList[0] : strList[1], out Id))
            {
                if (Id > 0)
                    PlaySound(Id);
            }
        }

        private void OnEnable()
        {
            if (!isEntity)
                return;
            isFind = false;
        }

        private  void ToFind()
        {
            if ( isFind)
                return;

            BaseEntity entity = transform.GetComponentInParent<BaseEntity>();
            //if (entity != null)
            //    Debug.Log("  找到了 BaseEntity :  " + entity.name);

            isMain = entity != null ? entity.IsMain() : false;

            isFind = true;
        }

        private void OnDisable()
        {
        }
    }
}
