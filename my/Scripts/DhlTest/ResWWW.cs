using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Text;
public class ResWWW : MonoBehaviour
{
    private string url = "https://abserver.oss-cn-beijing.aliyuncs.com/test10.apk";
    void Start()
    {
        StartCoroutine(DownloadAndSave(url, "test.apk", (b, s) => { Debug.LogError(b + "=====" + s); }));
    }

    void Update()
    {

    }
    /// <summary>
    /// 下载并保存资源到本地
    /// </summary>
    /// <param name="url"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static IEnumerator DownloadAndSave(string url, string name, Action<bool, string> Finish = null)
    {
        string Loading = string.Empty;
        bool b = false;
        WWW www = new WWW(url);
        if (www.error != null)
        {
            print("error:" + www.error);
        }
        while (!www.isDone)
        {
            Loading = (((int)(www.progress * 100)) % 100) + "%";
            if (Finish != null)
            {
                Finish(b, Loading);
            }
            yield return 1;
        }
        if (www.isDone)
        {
            Loading = "100%";
            byte[] bytes = www.bytes;
            b = SaveAssets(Application.streamingAssetsPath, name, bytes);
            if (Finish != null)
            {
                Finish(b, Loading);
            }
        }
    }
    /// <summary>
    /// 保存资源到本地
    /// </summary>
    /// <param name="path"></param>
    /// <param name="name"></param>
    /// <param name="info"></param>
    /// <param name="length"></param>
    public static bool SaveAssets(string path, string name, byte[] bytes)
    {
        Stream sw;
        FileInfo t = new FileInfo(path + "//" + name);
        if (!t.Exists)
        {
            try
            {
                sw = t.Create();
                sw.Write(bytes, 0, bytes.Length);
                sw.Close();
                sw.Dispose();
                return true;
            }
            catch
            {
                return false;
            }
        }
        else
        {
            return true;
        }
    }

}
