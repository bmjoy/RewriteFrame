using System;
using System.Collections;
using System.IO;
using UnityEngine.Networking;
using UnityEngine;
/// <summary>
/// 断点续传
/// </summary>
public class HttpDownLoad : MonoSingleton<HttpDownLoad>
{
    public float progress { get; private set; }

    public bool isDone { get; private set; }

    private bool isStop;

    public IEnumerator OnStart(string url, string filePath, Action callBack)
    {
        var headRequest = UnityWebRequest.Head(url);

        yield return headRequest.SendWebRequest();

        var totalLength = long.Parse(headRequest.GetResponseHeader("Content-Length"));

        var dirPath = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }

        using (var fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write))
        {
            var fileLength = fs.Length;

            if (fileLength < totalLength)
            {
                fs.Seek(fileLength, SeekOrigin.Begin);

                var request = UnityWebRequest.Get(url);
                request.SetRequestHeader("Range", "bytes=" + fileLength + "-" + totalLength);
                request.SendWebRequest();

                var index = 0;
                while (!request.isDone)
                {
                    if (isStop) break;
                    yield return null;
                    var buff = request.downloadHandler.data;
                    if (buff != null)
                    {
                        var length = buff.Length - index;
                        fs.Write(buff, index, length);
                        index += length;
                        fileLength += length;

                        if (fileLength == totalLength)
                        {
                            progress = 1f;
                        }
                        else
                        {
                            progress = fileLength / (float)totalLength;
                            Debug.LogError((((int)(request.downloadProgress * 100)) % 100) +
                            "====" + (((int)(progress * 100)) % 100));
                        }
                    }
                }
            }
            else
            {
                progress = 1f;
            }

            fs.Close();
            fs.Dispose();
        }

        if (progress >= 1f)
        {
            isDone = true;
            if (callBack != null)
            {
                callBack();
            }
        }
    }

    public void Stop()
    {
        isStop = true;
    }
}