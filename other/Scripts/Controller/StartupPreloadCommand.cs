using Crucis.Protocol;
using PureMVC.Interfaces;
using PureMVC.Patterns.Command;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartupPreloadCommand : SimpleCommand
{
    public override void Execute(INotification notification)
    {
        DataManager.Instance.Preload(OnPreloadDataComplete);
    }

    /// <summary>
    /// 预加载数据完毕回调
    /// </summary>
    private void OnPreloadDataComplete()
    {
        SendNotification(NotificationName.StartupInitialize);

		NetWork.InitNetWorkStstem(typeof(GameFacade).Assembly);
        NetworkManager.Instance.Initialize();

		CameraManager.GetInstance().Initialize().Complete += OnCameraManagerInitialized;
    }

    /// <summary>
    /// 相机初始化完成
    /// </summary>
    private void OnCameraManagerInitialized()
    {
        WwiseManager.Instance.Initialize();

        //主摄像机增加监听器
        WwiseManager.Instance.AddAudiolisendner(CameraManager.GetInstance().GetMainCamereComponent().gameObject);

		// 添加手柄震动

		AkSoundEngine.AddOutput(new AkOutputSettings("Default_Motion_Device", 0), out ulong out_pDeviceID, null, 0);

		UIManager.Instance.Initialize(OnUIManagerInitalized);
	}

    /// <summary>
    /// UI管理器初始化完成
    /// </summary>
    private void OnUIManagerInitalized()
    {
        InputManager.Instance.Initialize(OnInputManagerInitalized);
    }

    /// <summary>
    /// 输入管理器初始化完成
    /// </summary>
    private void OnInputManagerInitalized()
    {
        HotkeyManager.Instance.Initialize(OnHotkeyManagernitalized);
    }

    /// <summary>
    /// 热键管理器初始化完成
    /// </summary>
    private void OnHotkeyManagernitalized()
    {
        UIManager.Instance.PreloadUIPanel(OnPreloadUIElementComplete);
	}

    /// <summary>
    /// UI预载完成
    /// </summary>
    private void OnPreloadUIElementComplete()
    {
        NetWork.InitNetWorkStstem(typeof(GameFacade).Assembly);

        NetworkManager.Instance.Initialize();

        SendNotification(NotificationName.StartupPreloadComplete);

        IconManager.Instance.Initialize(null);

        ServerTimeUtil.Instance.OnTick += OnUpdate;
    }

    private void OnUpdate()
    {

        //删除游戏场景中的启动Loading
        foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            if (root.GetComponent<Canvas>())
            {
                Animator m_Animator = root.transform.GetChild(0).GetComponent<Animator>();
                AnimatorStateInfo m_Info = m_Animator.GetCurrentAnimatorStateInfo(0);
                if (m_Info.normalizedTime > 1.0f)
                {
                    UIManager.Instance.OpenPanel(UIPanel.LoginPanel);
					UIManager.Instance.OpenPanel(UIPanel.DialoguePanel);
					UnityEngine.Object.Destroy(root);
                    ServerTimeUtil.Instance.OnTick -= OnUpdate;
                    break;
                }
               
            }
        }
    }
}