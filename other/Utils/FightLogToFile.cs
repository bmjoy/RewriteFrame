using System.IO;
using UnityEngine;

/// <summary>
/// 将日志输出的文件里
/// </summary>
public class FightLogToFile : Singleton<FightLogToFile>
{
    private static FightLogToFile s_Instance;
    private StreamWriter m_Wirter;

    public void Initialization()
    {
#if ENABLE_SYNCHRONOUS_SPACECRAFT_SELF_LOG
        StartWrite("Fight_Log_Spacecraft_Self");
#endif
#if ENABLE_SYNCHRONOUS_HUMAN_SELF_LOG
		StartWrite("Fight_Log_Human_Self");
#endif
#if ENABLE_SYNCHRONOUS_HUMAN_OTHER_LOG
		StartWrite("Fight_Log_Human_Other");
#endif
	}

    /// <summary>
    /// 初始化 创建文件（如果文件已存在覆盖）
    /// </summary>
    /// <param name="filePath"></param>
    public void StartWrite(string filePath)
    {
        if (m_Wirter != null)
        {
            return;
        }

        if (filePath != null)
        {
            EndWrite();
        }

        Debug.LogError("StartWrite " + filePath);

        m_Wirter = new StreamWriter(filePath, false);
    }

    /// <summary>
    /// 写入文件
    /// </summary>
    public void EndWrite()
    {
        if (m_Wirter == null)
        {
            return;
        }

        Debug.LogError("EndWrite");

        m_Wirter.Flush();
        m_Wirter.Close();
        m_Wirter.Dispose();
        m_Wirter = null;
    }

    /// <summary>
    /// 写入缓冲区
    /// </summary>
    /// <param name="content"></param>
    public void Write(string content)
    {
        if (m_Wirter == null)
        {
            return;
        }

        m_Wirter.Write(content);
    }

    /// <summary>
    /// 传为Json写入缓冲区
    /// </summary>
    /// <param name="content"></param>
    public void WriteToJson(string name, object content)
    {
        Write("[" + name + "]" + ":" + UnityEngine.JsonUtility.ToJson(content) + "\t");
    }

    /// <summary>
    /// 传为Json写入缓冲区
    /// </summary>
    /// <param name="content"></param>
    public void WriteToJsonEnter(string name, object content)
    {
        Write("[" + name + "]" + ":" + UnityEngine.JsonUtility.ToJson(content) + "\n");
    }

    private void OnApplicationQuit()
    {
#if ENABLE_SYNCHRONOUS_SPACECRAFT_SELF_LOG || ENABLE_SYNCHRONOUS_HUMAN_SELF_LOG
        EndWrite();
#endif
    }
}

