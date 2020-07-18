using Leyoutech.Core.Loader;
using PureMVC.Interfaces;
using PureMVC.Patterns.Facade;
using UnityEngine;
using Utils.Timer;

/// <summary>
/// 见父类说明
/// </summary>
public class GameFacade : Facade
{
#if UNITY_EDITOR
	public static PrefsValue<int> s_AssetLoaderMode = new PrefsValue<int>("GameFacades_AssetLoaderMode", (int)AssetLoaderMode.AssetDatabase);
	public static PrefsValue<string> s_AssetBundlePath = new PrefsValue<string>("GameFacades_AssetBundlePath", "");
	public static PrefsValue<bool> s_StartIsGameMode = new PrefsValue<bool>("GameFacades_StartIsGameMode", false);
#endif

	/// <summary>
	/// 当前运行的是否是游戏
	///		当前可能运行的是一些测试场景，例如美术的场景漫游
	/// </summary>
	internal static bool _IsGaming = false;

	public static IFacade Instance
    {
        get
        {
            return instance;
        }
    }

    /// <summary>
    /// <see cref="Facade.InitializeModel()"/>
    /// </summary>
    protected override void InitializeModel()
    {
        base.InitializeModel();
    }

    protected override void InitializeView()
    {
        base.InitializeView();
    }

    protected override void InitializeController()
    {
        base.InitializeController();

        RegisterCommand(NotificationName.StartupPreload, () => new StartupPreloadCommand());
        RegisterCommand(NotificationName.StartupInitialize, () => new StartupInitializeCommand());
    }

	/// <summary>
	/// 游戏启动时调用（仅只一次）
	/// </summary>
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	static void Startup()
	{
#if UNITY_EDITOR
		if (!s_StartIsGameMode.Get())
		{
			return; 
		}
		s_StartIsGameMode.Set(false);
#endif

		_IsGaming = true;

        Leyoutech.GameController.StartUp();
        
        GetInstance(() => new GameFacade());

		ClockUtil.Instance().Start();
		FightLogToFile.Instance.Initialization();
        BehaviorManager.Instance.Initialize();
        Cursor.lockState = CursorLockMode.Confined;
		Cursor.visible = false;

		AssetLoaderMode loaderMode;
		string assetBundlePath;
		int maxLoadingCount;
#if UNITY_EDITOR
		loaderMode = (AssetLoaderMode)s_AssetLoaderMode.Get();
		assetBundlePath = loaderMode == AssetLoaderMode.AssetBundle
			? s_AssetBundlePath.Get() + "/" + UnityEditor.EditorUserBuildSettings.activeBuildTarget + "/assetbundles"
			: string.Empty;
		maxLoadingCount = 30;
#else
		loaderMode = AssetLoaderMode.AssetBundle;
		assetBundlePath = Application.streamingAssetsPath+"/assetbundles";
		maxLoadingCount = 10;
#endif

		AssetBundle.UnloadAllAssetBundles(true);

		AssetManager.GetInstance().InitLoader(loaderMode, AssetPathMode.Address, maxLoadingCount, assetBundlePath,
			(isSuccess) =>
			{
				if (isSuccess)
				{
					CRenderer.Shaders.LoadInstanceFromAssetBundle(() =>
					{
						AssetManager.GetInstance().LoadSceneAsync("Assets/Scenes/GameMain.unity", (address, scene, userData) =>
						{
							instance.SendNotification(NotificationName.StartupPreload);
						}, null);
					});
				}
			});
	}
}
