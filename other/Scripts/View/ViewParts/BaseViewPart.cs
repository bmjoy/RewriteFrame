using Eternity.FlatBuffer;
using PureMVC.Interfaces;
using PureMVC.Patterns.Facade;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static UIViewState;

public class BaseViewPart
{
    /// <summary>
    /// 最后一次加载的路径
    /// </summary>
    private string m_AssetPath = null;
    /// <summary>
    /// 已加载的资源对象
    /// </summary>
    private Transform m_Transform;

    /// <summary>
    /// 是否已打开
    /// </summary>
    public bool Opened;
    /// <summary>
    /// 父视图
    /// </summary>
    public CompositeView OwnerView;
    /// <summary>
    /// 获取配置
    /// </summary>
    public virtual UiConfig? Config { get { return OwnerView.State.UIConfig; } }
    /// <summary>
    /// 获取状态
    /// </summary>
    public virtual UIViewState State { get { return OwnerView.State; } }

    /// <summary>
    /// 获取本地化文本
    /// </summary>
    /// <param name="id">本地化Key</param>
    /// <returns>本地化文本</returns>
    protected string GetLocalization(string id)
    {
        if (string.IsNullOrEmpty(id))
            return string.Empty;
        
        return (GameFacade.Instance.RetrieveProxy(ProxyName.GameLocalizationProxy) as GameLocalizationProxy).GetString(id);
    }

    /// <summary>
    /// 播放音效
    /// </summary>
    /// <param name="id">音效ID</param>
    protected void PlaySound(int id)
    {
        WwiseUtil.PlaySound((int)id, false, null);
    }

    /// <summary>
    /// 视图打开时调用
    /// </summary>
    /// <param name="owner">父视图</param>
    public virtual void OnShow(object msg)
    {
    }

    /// <summary>
    /// 视图关闭时调用
    /// </summary>
    /// <param name="owner">父视图</param>
    public virtual void OnHide()
    {
        m_AssetPath = null;

        UnloadViewPart();
    }


    /// <summary>
    /// 部件资源加载完成时调用
    /// (在此函数中初始化资源引用，资源状态）
    /// </summary>
    /// <param name="root">资源节点</param>
    protected virtual void OnViewPartLoaded()
    {

    }

    /// <summary>
    /// 部件资源即将卸载之时调用
    /// （在此函数重置资源状态，清理资源引用）
    /// </summary>
    /// <param name="root">资源节点</param>
    protected virtual void OnViewPartUnload()
    {

    }

    /// <summary>
    /// 需要处理有消息
    /// </summary>
    /// <returns></returns>
    public virtual NotificationName[] ListNotificationInterests()
    {
        return null;
    }

    /// <summary>
    /// 处理消息
    /// </summary>
    /// <param name="notification">消息</param>
    public virtual void HandleNotification(INotification notification)
    {
    }

    /// <summary>
    /// 获得焦点时
    /// </summary>
    public virtual void OnGotFocus()
    {
    }

    /// <summary>
    /// 丢失焦点时
    /// </summary>
    public virtual void OnLostFocus()
    {
    }

    /// <summary>
    /// 延迟调用
    /// </summary>
    /// <param name="time">延迟时间</param>
    /// <param name="callback">回调</param>
    protected void SetTimeout(float time, UnityAction callback)
    {
        UIManager.Instance.StartCoroutine(DelayCoroutine(time, callback));
    }

    /// <summary>
    /// 延迟协程
    /// </summary>
    /// <param name="time">延迟时间</param>
    /// <param name="callback">回调</param>
    /// <returns>IEnumerator</returns>
    private System.Collections.IEnumerator DelayCoroutine(float time, UnityAction callback)
    {
        yield return new WaitForSeconds(time);
        callback?.Invoke();
    }

    /// <summary>
    /// 延迟调用
    /// </summary>
    /// <param name="frame">延迟帧数</param>
    /// <param name="callback">回调</param>
    protected void SetFrameEnd(UnityAction callback)
    {
        UIManager.Instance.StartCoroutine(DelayFrameCoroutine(callback));
    }

    /// <summary>
    /// 延迟协程
    /// </summary>
    /// <param name="time">延迟时间</param>
    /// <param name="callback">回调</param>
    /// <returns>IEnumerator</returns>
    private System.Collections.IEnumerator DelayFrameCoroutine(UnityAction callback)
    {
        yield return new WaitForEndOfFrame();
        callback?.Invoke();
    }

    /// <summary>
    /// 加载部件资源
    /// </summary>
    /// <param name="path">部件资源地址</param>
    /// <param name="parent">部件资源挂点</param>
    protected void LoadViewPart(string assetAddress, Transform parent)
    {
        UnloadViewPart();

        m_AssetPath = assetAddress;

        string path = assetAddress;
        AssetUtil.LoadAssetAsync(assetAddress,
            (pathOrAddress, returnObject, userData) =>
            {
                //忽略关闭之后的加载回调
                if (!Opened) return;

                //忽略已经失效的加载回调
                if (!string.Equals(m_AssetPath, path)) return;

                if (returnObject != null)
                {
                    GameObject prefab = (GameObject)returnObject;

                    prefab.CreatePool(pathOrAddress);

                    m_Transform = prefab.Spawn(parent).transform;

                    OnViewPartLoaded();
                }
                else
                {
                    m_Transform = null;

                    Debug.LogError(string.Format("资源加载成功，但返回null,  pathOrAddress = {0}", pathOrAddress));
                }
            });
    }

    /// <summary>
    /// 卸载部件资源
    /// </summary>
    protected void UnloadViewPart()
    {
        if (m_Transform)
        {
            Transform recycleObject = m_Transform;

            OnViewPartUnload();
            m_Transform = null;

            SetFrameEnd(() => { recycleObject.Recycle(); });
        }

        m_AssetPath = null;
    }

    /// <summary>
    /// 获取面板Transform
    /// </summary>
    public Transform GetTransform()
    {
        return m_Transform;
    }

    /// <summary>
    /// 查找面板内组件
    /// </summary>
    /// <param name="path">面板内相对路径</param>
    /// <returns>对应组件</returns>
    protected T FindComponent<T>(string path) where T : Component
    {
        Transform result = m_Transform.Find(path);
        if (result == null)
        {
            return null;
        }
        return result.GetComponent<T>();
    }

    /// <summary>
    /// 查找指定节点下的组件
    /// </summary>
    /// <param name="node">节点</param>
    /// <param name="path">相对节点的相对路径</param>
    /// <returns>对应组件</returns>
    protected T FindComponent<T>(Transform node, string path)
    {
        Transform result = node.Find(path);
        if (result)
        {
            return result.GetComponent<T>();
        }
        return default;
    }
}
