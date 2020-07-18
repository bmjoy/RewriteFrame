using UnityEngine;
using UnityEngine.Events;
using System;

[RequireComponent(typeof(Animator))]
public class UIAnimationEvent : MonoBehaviour
{
    public UnityAction<string> OnAnimationEvent;

    [NonSerialized]
    public Animator Animator;

    /// <summary>
    /// 启动时
    /// </summary>
    private void Awake()
    {
        Animator = GetComponent<Animator>();
    }

    /// <summary>
    /// 收到动画事件
    /// </summary>
    /// <param name="key"></param>
    public void FirAnimationEvent(string key)
    {
        OnAnimationEvent?.Invoke(key);
    }
}
