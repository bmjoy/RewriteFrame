using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
/// <summary>
/// 下载测试
/// 
/// </summary>
public class WebRequestTest : MonoBehaviour
{

    private void Start()
    {
        // StartCoroutine(DoLoadFile());
        //StartCoroutine(DoLoadTexture());
        StartCoroutine(DoLoadAssetBundle());
    }

    // 下载文本或二进制文件
    private IEnumerator DoLoadFile()
    {
        string url = Application.streamingAssetsPath + "/" + "test.txt";
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();
            if (request.isHttpError || request.isNetworkError)
            {
                // 下载出错
                Debug.LogError(request.error);
            }
            else
            {
                // 下载完成
                string text = request.downloadHandler.text;
                byte[] bytes = request.downloadHandler.data;
                // 优先释放request 会降低内存峰值
                request.Dispose();
            }
        }
    }

    // 下载图片
    private IEnumerator DoLoadTexture()
    {
        string url = Application.streamingAssetsPath + "/" + "test.png";
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
        {
            yield return request.SendWebRequest();
            if (request.isHttpError || request.isNetworkError)
            {
                // 下载出错
                Debug.LogError(request.error);
            }
            else
            {
                // 下载完成
                Texture2D texture = (request.downloadHandler as DownloadHandlerTexture).texture;
                // 优先释放request 会降低内存峰值
                request.Dispose();
            }
        }
    }
    Dictionary<string, GameObject> dic = new Dictionary<string, GameObject>();




    // 下载AssetBundle
    private IEnumerator DoLoadAssetBundle()
    {
        string url = Application.streamingAssetsPath + "/" + "AssetBundles/ui/prefabs/view/" + "ssss_prefab.assetbundle";
        Debug.LogError(url);
        using (UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(url))
        {
            yield return request.SendWebRequest();
            if (request.isHttpError || request.isNetworkError)
            {
                // 下载出错
                Debug.LogError(request.error);
            }
            else
            {
                string assetName = "Assets/AssetsPackage/UI/Prefabs/View/ssss.prefab";
                // 下载完成
                AssetBundle assetBundle = (request.downloadHandler as DownloadHandlerAssetBundle).assetBundle;
                GameObject go = assetBundle.LoadAsset<GameObject>(assetName);
                Debug.LogError(go);
                dic.Add(assetName, go);
                assetBundle.Unload(false);
                // 优先释放request 会降低内存峰值
                request.Dispose();
                //GameObject.Instantiate(dic[assetName]);
                Debug.LogError("ssssssss");
                finsh = true;
            }
        }
    }
    bool finsh = false;
    private void Update()
    {
        if (finsh)
        {
            finsh = false;

            GameObject.Instantiate(dic["Assets/AssetsPackage/UI/Prefabs/View/ssss.prefab"]);
        }
    }

}