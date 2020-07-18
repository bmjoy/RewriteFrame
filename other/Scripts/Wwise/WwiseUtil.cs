/*===============================
 * Author: [Allen]
 * Purpose: 音效 公共函数
 * Time: 2019/5/28 18:30:21
================================*/
using SystemObject = System.Object;
using System;
using UnityEngine;

public static class WwiseUtil
{
    /// <summary>
    /// 播放音效
    /// </summary>
    /// <param name="musicId">音效ID</param>
    /// <param name="alreadyPrepare">是否已经 Prepare 加载了 一般给 false</param>
    /// <param name="SoundParent">挂点</param>
    /// <param name="playEndAction"> 非循环音效，播放完毕回调（需要的赋值就可以了）</param>
    /// <param name="userEndData"> 回调参数</param>
    public static void PlaySound(int musicId, bool alreadyPrepare, Transform SoundParent, Action<SystemObject> playEndAction = null, SystemObject userEndData = null)
    {
		//Debug.LogError("PlaySound1->musicId:" + musicId);
		//string traceStr = new System.Diagnostics.StackTrace().ToString();
		//Debug.LogError("StackTrace info:" + traceStr);
		MsgPlayMusicOrSound parame = new MsgPlayMusicOrSound();
        parame.musicId = musicId;
        parame.alreadyPrepare = alreadyPrepare;
        parame.SoundParent = SoundParent;
        parame.UseSoundParent = true;
        parame.endAction = playEndAction;
        parame.userEndData = userEndData;
        GameFacade.Instance.SendNotification(NotificationName.MSG_SOUND_PLAY, parame);
    }

    /// <summary>
    /// 播放音效
    /// </summary>
    /// <param name="musicId">音效ID</param>
    /// <param name="alreadyPrepare">是否已经 Prepare 加载了 一般给 false</param>
    /// <param name="point">位置</param>
    /// <param name="playEndAction"> //非循环音效，播放完毕回调（需要的赋值就可以了）</param>
    /// <param name="userEndData"> 回调参数</param>
    public static void PlaySound(int musicId, bool alreadyPrepare, Vector3 point, Action<SystemObject> playEndAction = null, SystemObject userEndData = null)
	{
		//Debug.LogError("PlaySound2->musicId:" + musicId);
		//string traceStr = new System.Diagnostics.StackTrace().ToString();
		//Debug.LogError("StackTrace info:" + traceStr);
		MsgPlayMusicOrSound parame = new MsgPlayMusicOrSound();
        parame.musicId = musicId;
        parame.alreadyPrepare = alreadyPrepare;
        parame.point = point;
        parame.UseSoundParent = false;
        parame.endAction = playEndAction;
        parame.userEndData = userEndData;
        GameFacade.Instance.SendNotification(NotificationName.MSG_SOUND_PLAY, parame);
    }


    /// <summary>
    /// 播放特殊枚举标注的音效
    /// </summary>
    /// <param name="ComboId">组合ID</param>
    /// <param name="type">类型</param>
    /// <param name="palce">第几人称</param>
    /// <param name="alreadyPrepare">是否已经 Prepare 加载了 一般给 false</param>
    /// <param name="SoundParent">挂点</param>
    public static void PlaySound(int ComboId, WwiseMusicSpecialType type, WwiseMusicPalce palce, bool alreadyPrepare, Transform SoundParent, Action<SystemObject> playEndAction = null,SystemObject userEndData = null)
    {
		MsgPlaySpecialTypeMusicOrSound parame = new MsgPlaySpecialTypeMusicOrSound();
        parame.ComboId = ComboId;
        parame.type = type;
        parame.palce = palce;
        parame.alreadyPrepare = alreadyPrepare;
        parame.SoundParent = SoundParent;
        parame.UseSoundParent = true;
        parame.endAction = playEndAction;
        parame.userEndData = userEndData;
        GameFacade.Instance.SendNotification(NotificationName.MSG_SOUND_PLAY, parame);
    }

    /// <summary>
    /// 播放特殊枚举标注的音效
    /// </summary>
    /// <param name="ComboId">组合ID</param>
    /// <param name="type">类型</param>
    /// <param name="palce">第几人称</param>
    /// <param name="alreadyPrepare">是否已经 Prepare 加载了 一般给 false</param>
    /// <param name="point">位置</param>
    public static void PlaySound(int ComboId, WwiseMusicSpecialType type, WwiseMusicPalce palce, bool alreadyPrepare, Vector3 point, Action<SystemObject> playEndAction = null, SystemObject userEndData = null)
    {
		MsgPlaySpecialTypeMusicOrSound parame = new MsgPlaySpecialTypeMusicOrSound();
        parame.ComboId = ComboId;
        parame.type = type;
        parame.palce = palce;
        parame.alreadyPrepare = alreadyPrepare;
        parame.point = point;
        parame.UseSoundParent = false;
        parame.endAction = playEndAction;
        parame.userEndData = userEndData;
        GameFacade.Instance.SendNotification(NotificationName.MSG_SOUND_PLAY, parame);
    }


    /// <summary>
    /// 加载组合
    /// </summary>
    /// <param name="ComboId">组合ID</param>
    /// <param name="todoPrepareEvent">是否对于标注可执行Prepare 的去 执行 PrepareEvent</param>
    public static void LoadSoundCombo(int ComboId , bool todoPrepareEvent = false)
    {
        MsgLoadSoundCombo parame = new MsgLoadSoundCombo();
        parame.SoundComboId = ComboId;
        parame.todoPrepareEvent = todoPrepareEvent;
        GameFacade.Instance.SendNotification(NotificationName.MSG_SOUND_LOAD_COMBO, parame);
    }

    /// <summary>
    /// 卸载组合
    /// </summary>
    /// <param name="ComboId">组合ID</param>
    public static void UnLoadSoundCombo(int ComboId)
    {
        GameFacade.Instance.SendNotification(NotificationName.MSG_SOUND_UNLOAD_COMBO, ComboId);
    }

    /// <summary>
    /// 设置Listener 的目标
    /// </summary>
    /// <param name="target">目标</param>
    public static void SetListenerTarget(Transform target)
    {
        GameFacade.Instance.SendNotification(NotificationName.MSG_SOUND_SET_LISTENER_TARGET, target);
    }
}

