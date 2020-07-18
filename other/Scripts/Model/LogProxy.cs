using PureMVC.Patterns.Proxy;
using System.Collections.Generic;
using UnityEngine.Assertions;

public class LogProxy : Proxy
{
    /// <summary>
    /// 日志存储数据
    /// </summary>
    private Dictionary<string, LogDataVO> m_Logs = new Dictionary<string, LogDataVO>();

    public LogProxy() : base(ProxyName.LogProxy)
    {

    }

    /// <summary>
    /// 添加日志
    /// </summary>
    /// <param name="id"></param>
    /// <param name="logDataVO"></param>
    public void AddLog(string id,LogDataVO logDataVO)
    {
        if (m_Logs.TryGetValue(id, out LogDataVO data))
        {
            return;
        }
        m_Logs[id] = logDataVO;
    }

    /// <summary>
    /// 删除日志
    /// </summary>
    /// <param name="id"></param>
    public void DeleteLog(string id)
    {
        Assert.IsTrue(m_Logs.TryGetValue(id, out LogDataVO logDataVO), "LogProxy => DeleteLog not exist id " + id);
        m_Logs.Remove(id);
    }

    /// <summary>
    /// 清空日志
    /// </summary>
    public void ClearLog()
    {
        m_Logs.Clear();
    }

    /// <summary>
    /// 获取所有日志
    /// </summary>
    /// <returns></returns>
    public Dictionary<string, LogDataVO> GetLogs()
    {
        return m_Logs;
    }
}
