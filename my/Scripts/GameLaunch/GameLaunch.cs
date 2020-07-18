using UnityEngine;
using System.Collections;
using AssetBundles;
using GameChannel;
using System;
using XLua;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
[Hotfix]
[LuaCallCSharp]
public class GameLaunch : MonoBehaviour
{
    const string launchPrefabPath = "UI/Prefabs/View/UILaunch.prefab";
    const string noticeTipPrefabPath = "UI/Prefabs/Common/UINoticeTip.prefab";

    GameObject launchPrefab;
    GameObject noticeTipPrefab;
    AssetbundleUpdater updater;


    /// <summary>
    /// 游戏启动时调用（仅只一次）
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void OnStartGame()
    {
#if UNITY_EDITOR
        if (UnityEditor.EditorPrefs.GetBool(URLSetting.START_IS_GAME))
        {
            SceneManager.LoadScene("LaunchScene");
            UnityEditor.EditorPrefs.SetBool(URLSetting.START_IS_GAME, false);
        }
#endif


    }

    private void OnEnable()
    {
        // 启动ugui图集管理器
        var start = DateTime.Now;
        AtlasLoader.Instance.Startup();
        Debug.Log(string.Format("SpriteAtlasManager Init use {0}ms", (DateTime.Now - start).Milliseconds));
    }
    IEnumerator Start()
    {
        LoggerHelper.Instance.Startup();
        //注释掉IOS的推送服务
        //#if UNITY_IPHONE
        //        UnityEngine.iOS.NotificationServices.RegisterForNotifications(UnityEngine.iOS.NotificationType.Alert | UnityEngine.iOS.NotificationType.Badge | UnityEngine.iOS.NotificationType.Sound);
        //        UnityEngine.iOS.Device.SetNoBackupFlag(Application.persistentDataPath);
        //#endif

        // 初始化App版本
        var start = DateTime.Now;
        yield return InitAppVersion();
        Logger.Log(string.Format("InitAppVersion use {0}ms", (DateTime.Now - start).Milliseconds));

        // 初始化渠道
        start = DateTime.Now;
        yield return InitChannel();
        Logger.Log(string.Format("InitChannel use {0}ms", (DateTime.Now - start).Milliseconds));

        // 启动资源管理模块
        start = DateTime.Now;
        yield return AssetBundleManager.Instance.Initialize();
        Logger.Log(string.Format("AssetBundleManager Initialize use {0}ms", (DateTime.Now - start).Milliseconds));

        // 启动xlua热修复模块
        start = DateTime.Now;
        XLuaManager.Instance.Startup();
        string luaAssetbundleName = XLuaManager.Instance.AssetbundleName;
        AssetBundleManager.Instance.SetAssetBundleResident(luaAssetbundleName, true);
        var abloader = AssetBundleManager.Instance.LoadAssetBundleAsync(luaAssetbundleName);
        yield return abloader;
        abloader.Dispose();
        XLuaManager.Instance.OnInit();
        XLuaManager.Instance.StartHotfix();
        Logger.Log(string.Format("XLuaManager StartHotfix use {0}ms", (DateTime.Now - start).Milliseconds));

        // 初始化UI界面
        yield return InitLaunchPrefab();
        yield return null;
        yield return InitNoticeTipPrefab();


        // 开始更新
        if (updater != null)
        {
            updater.StartCheckUpdate();
        }
        yield return TestLoad();
        yield break;

    }

    IEnumerator InitAppVersion()
    {
        var appVersionRequest = AssetBundleManager.Instance.RequestAssetFileAsync(BuildUtils.AppVersionFileName);
        yield return appVersionRequest;
        var streamingAppVersion = appVersionRequest.text;
        appVersionRequest.Dispose();

        var appVersionPath = AssetBundleUtility.GetPersistentDataPath(BuildUtils.AppVersionFileName);
        var persistentAppVersion = GameUtility.SafeReadAllText(appVersionPath);
        Logger.Log(string.Format("streamingAppVersion = {0}, persistentAppVersion = {1}", streamingAppVersion, persistentAppVersion));

        // 如果persistent目录版本比streamingAssets目录app版本低，说明是大版本覆盖安装，清理过时的缓存
        if (!string.IsNullOrEmpty(persistentAppVersion) && BuildUtils.CheckIsNewVersion(persistentAppVersion, streamingAppVersion))
        {
            var path = AssetBundleUtility.GetPersistentDataPath();
            GameUtility.SafeDeleteDir(path);
        }
        GameUtility.SafeWriteAllText(appVersionPath, streamingAppVersion);
        ChannelManager.instance.appVersion = streamingAppVersion;
        yield break;
    }

    IEnumerator InitChannel()
    {
#if UNITY_EDITOR
        if (AssetBundleConfig.IsEditorMode)
        {
            yield break;
        }
#endif
        var channelNameRequest = AssetBundleManager.Instance.RequestAssetFileAsync(BuildUtils.ChannelNameFileName);
        yield return channelNameRequest;
        var channelName = channelNameRequest.text;
        channelNameRequest.Dispose();
        ChannelManager.instance.Init(channelName);
        Logger.Log(string.Format("channelName = {0}", channelName));
        yield break;
    }

    GameObject InstantiateGameObject(GameObject prefab)
    {
        var start = DateTime.Now;
        GameObject go = GameObject.Instantiate(prefab);
        Logger.Log(string.Format("Instantiate use {0}ms", (DateTime.Now - start).Milliseconds));
        Debug.Log(go + "==============");
        var luanchLayer = GameObject.Find("UIRoot/LuanchLayer");
        go.transform.SetParent(luanchLayer.transform);
        var rectTransform = go.GetComponent<RectTransform>();
        rectTransform.offsetMax = Vector2.zero;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.localScale = Vector3.one;
        rectTransform.localPosition = Vector3.zero;

        return go;

    }

    IEnumerator InitNoticeTipPrefab()
    {
        var start = DateTime.Now;
        var loader = AssetBundleManager.Instance.LoadAssetAsync(noticeTipPrefabPath, typeof(GameObject));
        yield return loader;
        noticeTipPrefab = loader.asset as GameObject;
        Logger.Log(string.Format("Load noticeTipPrefab use {0}ms", (DateTime.Now - start).Milliseconds));
        loader.Dispose();
        if (noticeTipPrefab == null)
        {
            Logger.LogError("LoadAssetAsync noticeTipPrefab err : " + noticeTipPrefabPath);
            yield break;
        }
        var go = InstantiateGameObject(noticeTipPrefab);
        UINoticeTip.Instance.UIGameObject = go;
        yield break;
    }

    IEnumerator InitLaunchPrefab()
    {
        var start = DateTime.Now;
        var loader = AssetBundleManager.Instance.LoadAssetAsync(launchPrefabPath, typeof(GameObject));
        yield return loader;
        launchPrefab = loader.asset as GameObject;
        Logger.Log(string.Format("Load launchPrefab use {0}ms", (DateTime.Now - start).Milliseconds));
        loader.Dispose();
        if (launchPrefab == null)
        {
            Logger.LogError("LoadAssetAsync launchPrefab err : " + launchPrefabPath);
            yield break;
        }
        var go = InstantiateGameObject(launchPrefab);
        updater = go.AddComponent<AssetbundleUpdater>();
        yield break;
    }
    const string producePrefabPath = "UI/SpriteAtlas/Role.spriteatlas";
    IEnumerator TestLoad()
    {
        Debug.Log(Time.time + "-==111==kaishi");
        var loader = AssetBundleManager.Instance.LoadAssetAsync(producePrefabPath, typeof(UnityEngine.U2D.SpriteAtlas), (objd) =>
        {
            UnityEngine.U2D.SpriteAtlas producePrefab1 = objd as UnityEngine.U2D.SpriteAtlas;
            Debug.Log(producePrefab1.name + Time.time + "==1111==huidiao");
        });
        yield return loader;
        Debug.Log(Time.time + "-====kaishi");

        UnityEngine.U2D.SpriteAtlas producePrefab = loader.asset as UnityEngine.U2D.SpriteAtlas;
        string ip = GetCurrentMachineLocalIP();

        loader.Dispose();

        //     var abloader = AssetBundleManager.Instance.LoadAssetBundleAsync("testab/uiapk_lua_bytes.assetbundle", (objd) =>
        //    {
        //        Debug.LogError(objd);
        //        Debug.LogError(Time.time + "==huidiao");
        //    });
        //     yield return abloader;
        //     abloader.Dispose();
        //     Debug.LogError(Time.time + "-=-=-=-=jieshu");

        yield break;
    }


    public static string GetCurrentMachineLocalIP()
    {
        try
        {
            // 注意：这里获取所有内网地址后选择一个最小的，因为可能存在虚拟机网卡
            var ips = new List<string>();
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    ips.Add(ip.ToString());
                }
            }
            ips.Sort();
            if (ips.Count <= 0)
            {
                Logger.LogError("Get inter network ip failed!");
            }
            else
            {
                return ips[0];
            }
        }
        catch (System.Exception ex)
        {
            Logger.LogError("Get inter network ip failed with err : " + ex.Message);
            Logger.LogError("Go Tools/Package to specify any machine as local server!!!");
        }
        return string.Empty;
    }
}
