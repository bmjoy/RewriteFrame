using System;
using System.Collections.Generic;
using System.IO;

public class CustomLog
{
    private const string logDirectory = "./behaviac_log";
    private static CustomLog m_Instance;
    private Dictionary<string, StreamWriter> m_logs = new Dictionary<string, StreamWriter>();

    public static CustomLog Instance
    {
        get
        {
            if (m_Instance == null)
            {
                m_Instance = new CustomLog();
            }

            return m_Instance;
        }
    }

    public void Output(string name, string msg)
    {
        StreamWriter sw = GetWriter(name);
        msg = DateTime.Now.ToString() + " " + msg + "\n";
        if (sw == null)
        {
            return;
        }

        lock (sw)
        {
            sw.Write(msg);
            sw.Flush();
        }
    }

    public void Close()
    {
        try
        {
            var e = m_logs.Values.GetEnumerator();
            while (e.MoveNext())
            {
                e.Current.Flush();
                e.Current.Close();
            }

            m_logs.Clear();
        }
        catch
        {            
        }
    }

    private StreamWriter GetWriter(string name)
    {
        StreamWriter sw = null;
        if (name == null)
        {
            return null;
        }

        if (!m_logs.TryGetValue(name, out sw))
        {
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            sw = new StreamWriter(logDirectory + "/" + name);
            m_logs.Add(name, sw);
        }

        return sw;
    }
}
