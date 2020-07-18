/*===============================
 * Author: [Allen]
 * Purpose: 音效时间触发器管理
 * Time: 2019/5/9 17:31:05
================================*/
using System.Collections.Generic;
using UnityEngine;

public class WwiseTimeTriggerManager
{
    /// <summary>
    ///音效触发器
    /// </summary>
    private class Trigger 
    {
        /// <summary>
        /// 触发时间
        /// </summary>
        public int triggerTime;

        /// <summary>
        /// 结束时间
        /// </summary>
        public int overTime;

        /// <summary>
        /// 音效事件名字
        /// </summary>
        public string soundEventName;

        /// <summary>
        /// 是否已经触发
        /// </summary>
        public bool bTrigger;

        /// <summary>
        /// 是否是2D
        /// </summary>
        public bool b2D;

        /// <summary>
        /// 音效ID
        /// </summary>
        public uint playId;

        /// <summary>
        /// 音效挂点 3D 使用
        /// </summary>
        public  Transform SoundParent;

        /// <summary>
        /// 音效位置 3D 使用
        /// </summary>
        public Vector3 SoundPoint;
    }


    /// <summary>
    /// 触发器容器
    /// </summary>
    private LinkedList<Trigger> m_TriggerList = new LinkedList<Trigger>();

    /// <summary>
    /// Tick 开始时间
    /// </summary>
    private int m_nCurTime = 0;


    public void Init()
    {
        ServerTimeUtil.Instance.OnFixedUpdate += Tick;
    }


    /// <summary>
    /// 获得当前时间
    /// </summary>
    /// <returns></returns>
    private int GetNowTime()
    {
        return (int)ServerTimeUtil.Instance.GetNowTimeMSEL();
    }


    /// <summary>
    /// 添加触发器2D
    /// </summary>
    /// <param name="triggerTime">触发时间</param>
    /// <param name="sustainTime">维持时间</param>
    /// <param name="soundEventName">音效事件名字</param>
    /// <returns></returns>
    public bool AddTimeTrigger2D(  int triggerTime, int sustainTime, string soundEventName)
    {
        if (string.IsNullOrEmpty(soundEventName))
            return false;

        if (triggerTime < 0 || triggerTime < 0)
            return false;

        if (m_TriggerList.Count == 0)
            m_nCurTime = GetNowTime();

        Trigger trigger = new Trigger();
        trigger.triggerTime = GetNowTime() + triggerTime;
        trigger.overTime = GetNowTime() + triggerTime + sustainTime;
        trigger.soundEventName = soundEventName;
        trigger.bTrigger = false;
        trigger.b2D = true;

        if (!m_TriggerList.Contains(trigger))
            m_TriggerList.AddLast(trigger);
        return true;
    }

    /// <summary>
    /// 添加触发器
    /// </summary>
    /// <param name="triggerTime">触发时间</param>
    /// <param name="sustainTime">维持时间</param>
    /// <param name="soundEventName">音效事件名字</param>
    /// <param name="soundPoint3D">3D音效 生成位置</param>
    /// <returns></returns>
    public bool AddTimeTrigger3D(  int triggerTime , int sustainTime, string soundEventName,Vector3 soundPoint3D )
    {
        if (string.IsNullOrEmpty(soundEventName))
            return false;

        if (triggerTime < 0 || sustainTime < 0)
            return false;

        if(m_TriggerList.Count == 0)
            m_nCurTime = GetNowTime();

        Trigger trigger = new Trigger();
        trigger.triggerTime = GetNowTime() + triggerTime;
        trigger.overTime = GetNowTime() + triggerTime+ sustainTime;
        trigger.soundEventName = soundEventName;
        trigger.bTrigger = false;
        trigger.b2D = false;
        trigger.SoundParent = null;
        trigger.SoundPoint = soundPoint3D;


        if (!m_TriggerList.Contains(trigger))
            m_TriggerList.AddLast(trigger);
        return true;
    }
    /// <summary>
    /// 添加触发器
    /// </summary>
    /// <param name="triggerTime">触发时间</param>
    /// <param name="sustainTime">维持时间</param>
    /// <param name="soundEventName">音效事件名字</param>
    /// <param name="soundParent3D">3D 音效挂点</param>
    /// <returns></returns>
    public bool AddTimeTrigger3D(int triggerTime, int sustainTime, string soundEventName, Transform soundParent3D = null)
    {
        if (string.IsNullOrEmpty(soundEventName))
            return false;

        if (triggerTime < 0 || sustainTime < 0)
            return false;

        if (m_TriggerList.Count == 0)
            m_nCurTime = GetNowTime();

        Trigger trigger = new Trigger();
        trigger.triggerTime = GetNowTime() + triggerTime;
        trigger.overTime = GetNowTime() + triggerTime+ sustainTime;
        trigger.soundEventName = soundEventName;
        trigger.bTrigger = false;
        trigger.b2D = false;
        trigger.SoundParent = soundParent3D;
        trigger.SoundPoint = Vector3.zero;

        if (!m_TriggerList.Contains(trigger))
            m_TriggerList.AddLast(trigger);
        return true;
    }


    public void Tick()
    {
        //暂时屏蔽吧，这部分在构思构思


        //if (m_TriggerList.Count == 0)
        //    return;

        //LinkedListNode<Trigger> pTriggerVertor = m_TriggerList.First;
        //Trigger pTrigger = null;

        //int nowTime = GetNowTime();
        //while (pTriggerVertor != null)
        //{
        //    pTrigger = pTriggerVertor.Value;

        //    if(!pTrigger.bTrigger)
        //    {
        //        //没触发
        //        int nTriggerTime = pTrigger.triggerTime;
        //        if (nowTime - m_nCurTime < nTriggerTime)
        //        {
        //            pTriggerVertor = pTriggerVertor.Next;
        //            continue;
        //        }

        //        if (pTrigger.b2D)
        //        {
        //            uint playId = WwiseManager.Instance.PlaySound2D(pTrigger.soundEventName);
        //            pTrigger.playId = playId;
        //            pTrigger.bTrigger = true;
        //            pTriggerVertor.Value = pTrigger;
        //        }
        //        else
        //        {
        //            uint playId = 0;
        //            if (pTrigger.SoundParent == null)
        //            {
        //                playId = WwiseManager.Instance.PlaySound3D(pTrigger.soundEventName, pTrigger.SoundPoint);
        //            }
        //            else
        //            {
        //                playId = WwiseManager.Instance.PlaySound3D(pTrigger.soundEventName, pTrigger.SoundParent);
        //            }
        //            pTrigger.playId = playId;
        //            pTrigger.bTrigger = true;
        //            pTriggerVertor.Value = pTrigger;
        //        }
        //    }
        //   else
        //    {
        //        //触发了
        //        int nTriggerTime = pTrigger.overTime;
        //        if (nowTime - m_nCurTime < nTriggerTime)
        //        {
        //            pTriggerVertor = pTriggerVertor.Next;
        //            continue;
        //        }
        //        AkSoundEngine.StopPlayingID(pTrigger.playId);
        //        m_TriggerList.Remove(pTrigger);
        //    }

        //    pTriggerVertor = pTriggerVertor.Next;
        //}
    }

    /// <summary>
    /// 释放
    /// </summary>
    public void Release()
    {
        m_TriggerList.Clear();
    }
}