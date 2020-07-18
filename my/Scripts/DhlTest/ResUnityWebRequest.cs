using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class ResUnityWebRequest : MonoBehaviour
{
    UnityWebRequest webRequest;
    private string downloadUrl = "";
    private string savePath = "";//如"E://"
    private string downloadFileName = "";

    // Use this for initialization
    void Start()
    {
        downloadUrl = "http://10.53.2.86:9090/MyFrame/test11.apk"; ;//下载链接
        savePath = Application.streamingAssetsPath + "/";
        downloadFileName = "test11.apk";
        savePath = savePath + downloadFileName;
        StartCoroutine(Down(downloadUrl));
        // StartCoroutine(HttpDownLoad.Instance.OnStart(downloadUrl, savePath, () => { Debug.LogError("finsh"); }));
    }

    // Update is called once per frame
    void Update()
    {
        // if (!webRequest.isDone)
        //     Debug.Log("下载进度：" + GetProcess());
    }
    /// <summary>
    /// 根据URL下载文件
    /// </summary>
    /// <param name="downloadingUrl"></param>
    /// <returns></returns>
    IEnumerator Down(string downloadingUrl)

    {
        //发送请求
        webRequest = UnityWebRequest.Get(downloadingUrl);
        webRequest.timeout = 30;//设置超时，若webRequest.SendWebRequest()连接超时会返回，且isNetworkError为true
        yield return webRequest.SendWebRequest();
        Debug.LogError("sss");
        if (webRequest.isNetworkError)
        {
            Debug.Log("Download Error:" + webRequest.error);
        }
        else
        {
            //获取二进制数据

            var File = webRequest.downloadHandler.data;

            //创建文件写入对象

            FileStream nFile = new FileStream(savePath, FileMode.Create);

            //写入数据

            nFile.Write(File, 0, File.Length);

            nFile.Close();
        }



    }

    /// <summary>
    /// 获取下载进度
    /// </summary>
    /// <returns></returns>
    public float GetProcess()
    {
        if (webRequest != null)
        {
            return (((int)(webRequest.downloadProgress * 100)) % 100);
        }
        return 0;
    }
    /// <summary>
    /// 获取当前下载内容长度
    /// </summary>
    /// <returns></returns>
    public long GetCurrentLength()
    {
        if (webRequest != null)
        {
            return (long)webRequest.downloadedBytes;
        }
        return 0;
    }

}
