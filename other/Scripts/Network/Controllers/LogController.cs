using Assets.Scripts.Lib.Net;
using Assets.Scripts.Proto;
using Game.Frame.Net;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogController : BaseNetController
{
    #region proxy
    private LogProxy GetLogProxy()
    {
        return GameFacade.Instance.RetrieveProxy(ProxyName.LogProxy) as LogProxy;
    }
    #endregion

    public LogController()
    {
        NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_sync_item_log, GetLogList, typeof(S2C_SYNC_ITEM_LOG));
        NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_notify_new_item_log, LogNew, typeof(S2C_NOTIFY_NEW_ITEM_LOG));
        NetworkManager.Instance.ListenGameServer(KS2C_Protocol.s2c_notify_delete_item_log, LogDelete, typeof(S2C_NOTIFY_DELETE_ITEM_LOG));
    }
    #region S2C
    /// <summary>
    /// 获取日志列表
    /// </summary>
    /// <param name="buf"></param>
    private void GetLogList(KProtoBuf buf)
    {
        GetLogProxy().ClearLog();
        S2C_SYNC_ITEM_LOG msg = buf as S2C_SYNC_ITEM_LOG;
        for (int i = 0; i < msg.log_list.Count; i++)
        {            
            CreatLog(msg.log_list[i]);          
        }
        GameFacade.Instance.SendNotification(NotificationName.MSG_LOG_RENDERLIST);
    }

    /// <summary>
    /// 添加新日志
    /// </summary>
    /// <param name="buf"></param>
    private void LogNew(KProtoBuf buf)
    {
        S2C_NOTIFY_NEW_ITEM_LOG msg = buf as S2C_NOTIFY_NEW_ITEM_LOG;
        CreatLog(msg.new_log);
        GameFacade.Instance.SendNotification(NotificationName.MSG_LOG_RENDERLIST);
    }

    /// <summary>
    /// 删除日志
    /// </summary>
    /// <param name="buf"></param>
    private void LogDelete(KProtoBuf buf)
    {
        S2C_NOTIFY_DELETE_ITEM_LOG msg = buf as S2C_NOTIFY_DELETE_ITEM_LOG;
        for (int i = 0; i < msg.ids.Count; i++)
        {
            GetLogProxy().DeleteLog(msg.ids[i]);
        }
        GameFacade.Instance.SendNotification(NotificationName.MSG_LOG_RENDERLIST);
    }

    /// <summary>
    /// 创建一条日志数据
    /// </summary>
    /// <param name="info"></param>
    private void CreatLog(LogDisplayInfo info)
    {
        LogDataVO m_LogDataVO = new LogDataVO();
        m_LogDataVO.Id = info.log_id;
        m_LogDataVO.Tid = info.item_tid;
        m_LogDataVO.Num = info.item_count;
        m_LogDataVO.ReveiveTime = info.receive_time;
        GetLogProxy().AddLog(m_LogDataVO.Id, m_LogDataVO);
    }
    #endregion

    #region  C2S
    /// <summary>
    /// 请求获取日志列表
    /// </summary>
    public void C_to_S_GetLogList()
    {
        C2S_REQUEST_ALL_ITEM_LOG msg = new C2S_REQUEST_ALL_ITEM_LOG();
        msg.protocolID = (ushort)KC2S_Protocol.c2s_request_all_item_log;
        NetworkManager.Instance.SendToGameServer(msg);
    }
    
    /// <summary>
    /// 请求删除日志
    /// </summary>
    /// <param name="id"></param>
    public void C_to_S_GetLogDelete(string id)
    {        
        if (GetLogProxy().GetLogs().TryGetValue(id, out LogDataVO logDataVO))
        {
            C2S_DELETE_ITEM_LOG msg = new C2S_DELETE_ITEM_LOG();
            msg.protocolID = (ushort)KC2S_Protocol.c2s_delete_item_log;
            msg.id = id;
            NetworkManager.Instance.SendToGameServer(msg);
        }
    }

    /// <summary>
    /// 请求删除所有日志
    /// </summary>
    public void C_to_S_DeleAllLog()
    {
        C2S_ALL_DELETE_ITEM_LOG msg = new C2S_ALL_DELETE_ITEM_LOG();
        msg.protocolID = (ushort)KC2S_Protocol.c2s_all_delete_item_log;
        NetworkManager.Instance.SendToGameServer(msg);
    }
    #endregion
}
