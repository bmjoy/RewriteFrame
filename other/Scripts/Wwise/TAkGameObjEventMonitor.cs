/*===============================
 * Author: [Allen]
 * Purpose: 音效播放监视器
 * Time: 2019/5/28 16:11:07
================================*/
using System;
using System.Collections.Generic;
using UnityEngine;
using SystemObject = System.Object;

namespace AK.Wwise
{
    public class TAkGameObjEventMonitor : MonoBehaviour
    {
        [SerializeField]
        public List<string> NowPlayingMuisc = new List<string>();           //当前正在播放中的音效Event 

        private List<uint> NowPlayingMuiscPlayId = new List<uint>();     //当前正在播放中的音效PlayID 

        private Dictionary<uint, Action<SystemObject>> NowPlayingMuiscEndAction = new Dictionary<uint, Action<SystemObject>>(); //当前正在播放中的音效PlayID ,完毕时回调
        private Dictionary<uint, SystemObject> NowPlayingMuiscEndActionData = new Dictionary<uint, SystemObject>(); //当前正在播放中的音效PlayID ,完毕时回调参数


        public List<uint> GetNowPlayingMuiscPlayId()
        {
            return NowPlayingMuiscPlayId;
        }

        /// <summary>
        /// 获得当前数量
        /// </summary>
        /// <returns></returns>
        public int GetCurrentCout()
        {
            return NowPlayingMuisc.Count;
        }


        private void OnEnable()
        {
            NowPlayingMuisc.Clear();
            NowPlayingMuiscPlayId.Clear();
            NowPlayingMuiscEndAction.Clear();
            NowPlayingMuiscEndActionData.Clear();
        }

        private void OnDestroy()
        {
           // Debug.LogError("-------->物体 OnDestroy 销毁 ！gameObjID = " + AkSoundEngine.GetAkGameObjectID(gameObject) + "    name: " + gameObject.name);

            NowPlayingMuisc.Clear();
            StopAllPlayingEvent();
            NowPlayingMuiscPlayId.Clear();
            NowPlayingMuiscEndAction.Clear();
            NowPlayingMuiscEndActionData.Clear();
        }

        /// <summary>
        /// add 增加一个播放中的信息
        /// </summary>
        /// <param name="soundEvenName"></param>
        /// <param name="playId"></param>
        public void AddPlaying(string soundEvenName, uint playId , Action<SystemObject> endAction, SystemObject userEndData)
        {
            if (playId <= 0)
                return;

            NowPlayingMuisc.Add(soundEvenName);
            NowPlayingMuiscPlayId.Add(playId);
            NowPlayingMuiscEndAction.Add(playId, endAction);
            NowPlayingMuiscEndActionData.Add(playId, userEndData);

            //Debug.Log("----TAkGameObjEventMonitor --AddPlaying--> playId =" + playId);
        }

        /// <summary>
        /// 删除一个信息
        /// </summary>
        /// <param name="playId"></param>
        public void DeleOneEvent(uint playId)
        {
            if (!NowPlayingMuiscPlayId.Contains(playId))
                return;

            int index = NowPlayingMuiscPlayId.FindIndex(item => item.Equals(playId));
            NowPlayingMuisc.RemoveAt(index);
            NowPlayingMuiscPlayId.RemoveAt(index);

            if (NowPlayingMuiscEndAction.TryGetValue(playId, out Action<SystemObject> endAction))
            {    

                NowPlayingMuiscEndActionData.TryGetValue(playId, out SystemObject userEndData);
                endAction?.Invoke(userEndData);
                NowPlayingMuiscEndAction.Remove(playId);
                NowPlayingMuiscEndActionData.Remove(playId);
            }

            //Debug.Log("----TAkGameObjEventMonitor --DeleOneEvent--> playId = "+ playId);
        }

        //停止所有音效
        private void StopAllPlayingEvent()
        {
            for (int i = 0 ; i < NowPlayingMuiscPlayId.Count; i++)
            {
                uint playId = NowPlayingMuiscPlayId[i];
                AkSoundEngine.StopPlayingID(playId);

                if (NowPlayingMuiscEndAction.ContainsKey(playId))
                {
                    NowPlayingMuiscEndActionData.TryGetValue(playId, out SystemObject userEndData);
                    NowPlayingMuiscEndAction[playId]?.Invoke(userEndData);
                }
            }
        }
    }
}