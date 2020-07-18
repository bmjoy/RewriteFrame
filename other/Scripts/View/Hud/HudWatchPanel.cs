using Cinemachine;
#if !UNITY_2019_1_OR_NEWER
using Cinemachine.Timeline;
#endif
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.UI;

public class HudWatchPanel : HudBase
{
	private const string ASSET_ADDRESS = Leyoutech.Core.Loader.Config.AssetAddressKey.PRELOADUI_WATCHPANEL;

    /// <summary>
    /// 相机挂点名
    /// </summary>
    private const string CAMERA_POINT_NAME = "*Camera";
    /// <summary>
    /// 相机注视点名
    /// </summary>
    private const string CAMERA_LOOK_AT_POINT_NAME = "*CameraLookAt";
    /// <summary>
    /// UI挂点名
    /// </summary>
    private const string WATCH_UI_NAME = "*hand_UI";

    /// <summary>
    /// 人形态渲染层
    /// </summary>
    private const string WATCH_2D_LAYER = "UI";
    /// <summary>
    /// 船形态渲染层
    /// </summary>
    private const string WATCH_3D_LAYER = "UICreatRole";

    /// <summary>
    /// 机位1名称
    /// </summary>
    private const string VIRTUAL_CAMERA_NAME_1 = "CM_CharacterWatchA";
    /// <summary>
    /// 机位2名称
    /// </summary>
    private const string VIRTUAL_CAMERA_NAME_2 = "CM_CharacterWatchB";

    /// <summary>
    /// 按钮图标的名称
    /// </summary>
	private string[] m_ButtonIconNames = new string[] { "Watch_Icon_package", "Watch_Icon_Social", "Watch_Icon_Message" };//"Watch_icon_role","Watch_icon_mission", "Watch_icon_legion"
    /// <summary>
    /// 按钮图标的名称
    /// </summary>
	private string[] m_ButtonNameIds = new string[] { "watch_title_1002", "watch_title_1003", "watch_title_1004" }; 
    /// <summary>
    /// 按钮图标的描述
    /// </summary>
	private string[] m_ButtonDescIds = new string[] { "watch_text_1001", "watch_text_1002", "watch_text_1003" }; 

    /// <summary>
    /// 手表面板
    /// </summary>
	private Transform m_WatchPanel;
    /// <summary>
    /// 手表挂点
    /// </summary>
    private Transform m_WatchPoint;
    /// <summary>
    /// 打开效果的Timeline
    /// </summary>
    private PlayableDirector m_OpenTimeline;
    /// <summary>
    /// 关闭效果的Timeline
    /// </summary>
    private PlayableDirector m_CloseTimeline;
    /// <summary>
    /// 场景层的后期效果
    /// </summary>
    private GameObject m_PostEffectScene;
    /// <summary>
    /// 场景层后期效果的层
    /// </summary>
    private int m_PostEffectSceneLayer;
    /// <summary>
    /// UI层的后期效果
    /// </summary>
    private GameObject m_PostEffectUI;
    /// <summary>
    /// UI层后期效果的层
    /// </summary>
    private int m_PostEffectUILayer;
    /// <summary>
    /// 当前动画阶段
    /// </summary>
    private TimeLineState m_LookAtState;
    /// <summary>
    /// 手表中当前选中项
    /// </summary>
	private int m_SelectedIndex = -1;
    /// <summary>
    /// 协程
    /// </summary>
    private Coroutine m_Coroutine;
    /// <summary>
    /// 手表输入键
    /// </summary>
    private bool m_WatchKeyHolding;
    /// <summary>
    /// 手表输入轴
    /// <summary>
    private Vector2 m_WatchMouseAxis;
    /// <summary>
    /// 手表按键容器
    /// </summary>
    private Transform m_ButtonBox;

    private enum TimeLineState { NONE, OPENING, OPENED, CLOSEING }

    public HudWatchPanel() : base(UIPanel.HudWatchPanel, ASSET_ADDRESS, PanelType.Hud) { }

	public override void Initialize()
	{
        m_WatchPanel = FindComponent<Transform>("Content");
		m_WatchPanel.gameObject.SetActive(false);

        m_OpenTimeline = FindComponent<PlayableDirector>("Timeline/Open");
        m_OpenTimeline.gameObject.SetActive(false);

        m_CloseTimeline = FindComponent<PlayableDirector>("Timeline/Close");
        m_CloseTimeline.gameObject.SetActive(false);

        m_ButtonBox = FindComponent<Transform>("Content/ButtonBox");

        m_PostEffectScene = FindComponent<RectTransform>("PostEffectMain").gameObject;
        m_PostEffectSceneLayer = m_PostEffectScene.layer;
        m_PostEffectUI = FindComponent<RectTransform>("PostEffectUI").gameObject;
        m_PostEffectUILayer = m_PostEffectUI.layer;
    }

	public override void OnShow(object msg)
	{
		base.OnShow(msg);

        SetIconImage();
        UpdateCenterInfo();

        m_OpenTimeline.played -= OnPlayableDirectorStarted;
        m_OpenTimeline.stopped -= OnPlayableDirectorStoped;
        m_CloseTimeline.played -= OnPlayableDirectorStarted;
        m_CloseTimeline.stopped -= OnPlayableDirectorStoped;

        m_OpenTimeline.played += OnPlayableDirectorStarted;
        m_OpenTimeline.stopped += OnPlayableDirectorStoped;
        m_CloseTimeline.played += OnPlayableDirectorStarted;
        m_CloseTimeline.stopped += OnPlayableDirectorStoped;

        m_PostEffectScene.gameObject.SetActive(false);
        m_PostEffectUI.gameObject.SetActive(false);

        ListenHotkeys(true, false);

        m_Coroutine = UIManager.Instance.StartCoroutine(UpdateWatch());      
    }

    public override void OnHide(object msg)
    {
        m_OpenTimeline.played -= OnPlayableDirectorStarted;
        m_OpenTimeline.stopped -= OnPlayableDirectorStoped;
        m_CloseTimeline.played -= OnPlayableDirectorStarted;
        m_CloseTimeline.stopped -= OnPlayableDirectorStoped;

        UIManager.Instance.StopCoroutine(m_Coroutine);

        ForceCloseWatch(false);

        ListenHotkeys(false, false);

        base.OnHide(msg);
    }
    
    /// <summary>
    /// 监视热键
    /// </summary>
    /// <param name="enabled1">开启键是否启用</param>
    /// <param name="enabled2">选择和关闭键是否启用</param>
    private void ListenHotkeys(bool enabled1, bool enabled2)
    {
        if (enabled1)
        {
            AddHotKey("humanOpenKey", HotKeyMapID.HUMAN, HotKeyID.WatchOpen, OnWatchKeyStateChangedFromShip);
            AddHotKey("shipOpenKey", HotKeyMapID.SHIP, HotKeyID.WatchOpen, OnWatchKeyStateChangedFromShip);
        }
        else
        {
            DeleteHotKey("humanOpenKey");
            DeleteHotKey("shipOpenKey");
        }

        if (enabled2)
        {
            AddHotKey("close", HotKeyMapID.Watch, HotKeyID.WatchClose, OnItemKeyStateChangedFromUI);
            AddHotKey("axis", HotKeyMapID.Watch, HotKeyID.WatchAxis, OnWatchAxisStateChanged);
            AddHotKey("direction", HotKeyMapID.Watch, HotKeyID.WatchDirection, OnWatchDirectionStateChanged);           
        }
        else
        {
            DeleteHotKey("close");
            DeleteHotKey("axis");
            DeleteHotKey("direction");
        }
    }

    /// <summary>
    /// 设置手表按键图片
    /// </summary>
    private void SetIconImage()
    {
        for (int i = 0; i < m_ButtonBox.childCount; i++)
        {
            Image icon = m_ButtonBox.GetChild(i).Find("Button/Content/Image_Icon").GetComponent<Image>();
            if (icon != null)
            {
                if (i < m_ButtonIconNames.Length)
                {
                    UIUtil.SetIconImage(icon, GameConstant.FUNCTION_ICON_ATLAS_ASSETADDRESS, m_ButtonIconNames[i]);
                }

                icon.transform.localEulerAngles = new Vector3(0, 0, -360 / m_ButtonBox.childCount * i);
            }
        }
    }

    /// <summary>
    /// 手表键按下时
    /// </summary>
    /// <param name="callback">热键状态</param>
    private void OnWatchKeyStateChangedFromShip(HotkeyCallback callback)
    {
        if (callback.started)
        {
            if (m_LookAtState == TimeLineState.NONE)
                m_WatchKeyHolding = true;
        }
    }

    /// <summary>
    /// 手表键松开时
    /// </summary>
    /// <param name="callback">热键状态</param>
    private void OnItemKeyStateChangedFromUI(HotkeyCallback callback)
    {
        if (!callback.started)
            m_WatchKeyHolding = false;
    }

    /// <summary>
    /// 手表输入轴状态改变时
    /// </summary>
    /// <param name="callback">热键状态</param>
    private void OnWatchAxisStateChanged(HotkeyCallback callback)
    {
        if (m_WatchKeyHolding && m_LookAtState == TimeLineState.OPENED)
        {
            if (InputManager.Instance.CurrentInputDevice != InputManager.GameInputDevice.KeyboardAndMouse)
            {
                Vector2 v2 = callback.ReadValue<Vector2>();
                if (v2.x != 0 && v2.y != 0)
                    SetSelection(v2);
            }
            else
            {
                m_WatchMouseAxis = m_WatchMouseAxis + callback.ReadValue<Vector2>();
                if (m_WatchMouseAxis.magnitude > 10)
                    SetSelection(m_WatchMouseAxis);
            }
        }
    }

    /// <summary>
    /// 手表输入方向改变时
    /// </summary>
    /// <param name="callback">热键状态</param>
    private void OnWatchDirectionStateChanged(HotkeyCallback callback)
    {
        Vector2 dir = callback.ReadValue<Vector2>();
        if (dir.x != 0 || dir.y != 0)
            SetSelection(dir);
    }

    /// <summary>
    /// 播放动画
    /// </summary>
    /// <param name="director">播放器</param>
    private void PlayTimeLine(PlayableDirector director)
    {
        CinemachineBrain mainCameraBrain = null;
        CinemachineVirtualCamera camera1 = null;
        CinemachineVirtualCamera camera2 = null;

        GameplayProxy gameplayProxy = GameFacade.Instance.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;
        Transform hero = IsInSpace() ? null : gameplayProxy.GetMainPlayerSkinTransform();
        if (hero != null)
        {
            MainCameraComponent mainCamera = CameraManager.GetInstance().GetMainCamereComponent();
            mainCameraBrain = mainCamera.GetCamera().GetComponent<CinemachineBrain>();
            camera1 = GetOrCreateVirtualCamera(VIRTUAL_CAMERA_NAME_1);
            camera2 = GetOrCreateVirtualCamera(VIRTUAL_CAMERA_NAME_2);
            
            if (director == m_OpenTimeline)
            {
                //机位1 ( 打开手表时相机所在机位 )
                camera1.Follow = GetOrCreateVirtualCameraProxy("WATCH_BACK_POINT");
                camera1.Follow.position = mainCamera.transform.position;
                camera1.Follow.rotation = mainCamera.transform.rotation;

                //机位2 ( 注视手表的机位 )
                camera2.Follow = FindTransformBy(hero, CAMERA_POINT_NAME);

                //UI挂点
                m_WatchPoint = FindTransformBy(hero, WATCH_UI_NAME);
                Canvas.worldCamera = CameraManager.GetInstance().GetUI3DCameraComponent().GetCamera();
                Canvas.worldCamera.cullingMask = 1 << LayerMask.NameToLayer(WATCH_3D_LAYER);
                Canvas.worldCamera.gameObject.SetActive(true);
                Canvas.scaleFactor = 0.001f;
                Canvas.transform.localScale = Vector3.one * 0.0001f;
                Canvas.GetComponent<RectTransform>().sizeDelta = UIManager.ReferenceResolution;

                LayerUtil.SetGameObjectToLayer(Canvas.gameObject, LayerMask.NameToLayer(WATCH_3D_LAYER), true);
                LayerUtil.SetGameObjectToLayer(m_PostEffectScene, m_PostEffectSceneLayer, false);
                LayerUtil.SetGameObjectToLayer(m_PostEffectUI, m_PostEffectUILayer, false);

                camera1.enabled = true;
                camera2.enabled = true;
            }
        }

        m_WatchPanel.gameObject.SetActive(true);

        if (director == m_OpenTimeline)
        {
            m_CloseTimeline.gameObject.SetActive(false);

            SetCinemachineTrack(m_OpenTimeline, mainCameraBrain, camera1, camera2);

            m_OpenTimeline.extrapolationMode = DirectorWrapMode.Hold;
            m_OpenTimeline.gameObject.SetActive(true);
            m_OpenTimeline.time = 0.0f;
            m_OpenTimeline.Play();

            m_PostEffectScene.gameObject.SetActive(true);
            m_PostEffectUI.gameObject.SetActive(true);
        }
        else if (director == m_CloseTimeline)
        {
            m_OpenTimeline.gameObject.SetActive(false);

            SetCinemachineTrack(director, mainCameraBrain, camera2, camera1);

            m_CloseTimeline.extrapolationMode = DirectorWrapMode.None;
            m_CloseTimeline.gameObject.SetActive(true);
            m_CloseTimeline.time = 0.0f;
            m_CloseTimeline.Play();

            m_PostEffectScene.gameObject.SetActive(true);
            m_PostEffectUI.gameObject.SetActive(true);

            m_SelectedIndex = -1;
            UpdateCenterInfo();
        }

        //人形动画
        PlayHumanAnimator(director == m_OpenTimeline);
    }

    /// <summary>
    /// 播放人形态动画
    /// </summary>
    /// <param name="open">打开还是关闭</param>
    private void PlayHumanAnimator(bool open)
    {
        GameplayProxy gameplayProxy = GameFacade.Instance.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;
        Transform hero = IsInSpace() ? null : gameplayProxy.GetMainPlayerSkinTransform();

        Animator humanAnimator = hero != null ? hero.GetComponentInChildren<Animator>() : null;
        if (humanAnimator)
        {
            humanAnimator.ResetTrigger("lookAtWatchOpen");
            humanAnimator.ResetTrigger("lookAtWatchClose");

            if (open)
                humanAnimator.SetTrigger("lookAtWatchOpen");
            else
                humanAnimator.SetTrigger("lookAtWatchClose");
        }
    }

    /// <summary>
    /// 打开动画播放开始时
    /// </summary>
    /// <param name="director">对应的PlayableDirector</param>
    private void OnPlayableDirectorStarted(PlayableDirector director)
    {
        if (director == m_OpenTimeline)
        {
            ListenHotkeys(false, true);

            InputManager.Instance.HudInputMap = HotKeyMapID.Watch;

            //抬手音效
            WwiseUtil.PlaySound((int)WwiseMusic.Music_Panel_Watch_Open, false, null);
        }
        else if (director == m_CloseTimeline)
        {
            SetCinemachineTrack(m_OpenTimeline, null, null, null);

            //放手音效
            WwiseUtil.PlaySound((int)WwiseMusic.Music_Panel_Watch_Close, false, null);
        }
    }

    /// <summary>
    /// 打开动画播放完成时
    /// </summary>
    /// <param name="director">对应的PlayableDirector</param>
    private void OnPlayableDirectorStoped(PlayableDirector director)
    {
        if (director == m_OpenTimeline)
        {
            m_WatchMouseAxis = Vector2.zero;
            m_LookAtState = TimeLineState.OPENED;
            m_SelectedIndex = -1;
            UpdateCenterInfo();
        }
        else
        {
            ForceCloseWatch(true);
        }
    }

    /// <summary>
    /// 强制关闭手表
    /// </summary>
    /// <param name="dealyResetCamera">是否需要延迟关闭相机</param>
    private void ForceCloseWatch(bool dealyResetCamera)
    {
        m_LookAtState = TimeLineState.NONE;
        m_SelectedIndex = -1;
        m_WatchKeyHolding = false;

        ListenHotkeys(true, false);

        InputManager.Instance.HudInputMap = HotKeyMapID.None;

        m_WatchPoint = null;

        m_WatchPanel.gameObject.SetActive(false);

        SetCinemachineTrack(m_OpenTimeline, null, null, null);
        SetCinemachineTrack(m_CloseTimeline, null, null, null);
        
        m_OpenTimeline.gameObject.SetActive(false);
        m_CloseTimeline.gameObject.SetActive(false);

        m_PostEffectScene.gameObject.SetActive(false);
        m_PostEffectUI.gameObject.SetActive(false);

        Canvas.worldCamera.gameObject.SetActive(false);
        Canvas.worldCamera = CameraManager.GetInstance().GetUICameraComponent().GetCamera();
        Canvas.worldCamera.cullingMask = 1 << LayerMask.NameToLayer(WATCH_2D_LAYER);
        Canvas.worldCamera.gameObject.SetActive(true);

        LayerUtil.SetGameObjectToLayer(Canvas.gameObject, LayerMask.NameToLayer(WATCH_2D_LAYER), true);
        LayerUtil.SetGameObjectToLayer(m_PostEffectScene, m_PostEffectSceneLayer, false);
        LayerUtil.SetGameObjectToLayer(m_PostEffectUI, m_PostEffectUILayer, false);

        UIManager.Instance.ResetPanelFit(this);

        CinemachineVirtualCamera camera1 = GetOrCreateVirtualCamera(VIRTUAL_CAMERA_NAME_1);
        CinemachineVirtualCamera camera2 = GetOrCreateVirtualCamera(VIRTUAL_CAMERA_NAME_2);

        if (dealyResetCamera)
        {
            camera2.Follow = camera1.Follow;
            camera1.enabled = camera2.enabled = false;

            UIManager.Instance.StartCoroutine(ResetCameraBrainBlend());
        }
        else
        {
            camera2.Follow = camera1.Follow = null;
            camera1.enabled = camera2.enabled = false;
        }

        PlayHumanAnimator(false);
    }

    /// <summary>
    /// 重置相机混合
    /// </summary>
    /// <returns>IEnumerator</returns>
    private IEnumerator ResetCameraBrainBlend()
    {
        CinemachineBrain cameraBrain = CameraManager.GetInstance().GetMainCamereComponent().GetCamera().GetComponent<CinemachineBrain>();

        CinemachineBlendDefinition oldSetting = cameraBrain.m_DefaultBlend;

        CinemachineBlendDefinition newSetting = cameraBrain.m_DefaultBlend;
        newSetting.m_Time = 0.0f;
        cameraBrain.m_DefaultBlend = newSetting;

        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        cameraBrain.m_DefaultBlend = oldSetting;

        CinemachineVirtualCamera camera1 = GetOrCreateVirtualCamera(VIRTUAL_CAMERA_NAME_1);
        CinemachineVirtualCamera camera2 = GetOrCreateVirtualCamera(VIRTUAL_CAMERA_NAME_2);
        camera2.Follow = camera1.Follow = null;
        camera1.enabled = camera2.enabled = false;
    }


    /// <summary>
    /// 获取或创建机位
    /// </summary>
    /// <param name="name">名称</param>
    /// <returns>CinemachineVirtualCamera</returns>
    private CinemachineVirtualCamera GetOrCreateVirtualCamera(string name)
    {
        MainCameraComponent mainCamera = CameraManager.GetInstance().GetMainCamereComponent();

        Transform virtualCameraTranform = mainCamera.transform.parent.Find(name);
        if (!virtualCameraTranform)
        {
            virtualCameraTranform = new GameObject(name).transform;
            virtualCameraTranform.SetParent(mainCamera.transform.parent);
        }

        CinemachineVirtualCamera virtualCamera = virtualCameraTranform.gameObject.GetComponent<CinemachineVirtualCamera>();
        if (!virtualCamera)
        {
            virtualCamera = virtualCameraTranform.gameObject.AddComponent<CinemachineVirtualCamera>();
            virtualCamera.m_StandbyUpdate = CinemachineVirtualCameraBase.StandbyUpdateMode.Always;
            virtualCamera.m_Lens.FieldOfView = mainCamera.GetCamera().fieldOfView;

            CinemachineTransposer transposer = virtualCamera.AddCinemachineComponent<CinemachineTransposer>();
            transposer.m_BindingMode = CinemachineTransposer.BindingMode.WorldSpace;
            transposer.m_FollowOffset = Vector3.zero;

            virtualCamera.AddCinemachineComponent<CinemachineSameAsFollowTarget>();
        }

        return virtualCamera;
    }

    /// <summary>
    /// 获取或创建机位
    /// </summary>
    /// <param name="name">名称</param>
    /// <returns>Transform</returns>
    private Transform GetOrCreateVirtualCameraProxy(string name)
    {
        Transform mainCamera = CameraManager.GetInstance().transform;// GetMainCamereComponent();

        Transform virtualCameraTranform = mainCamera.transform.Find(name);
        if (!virtualCameraTranform)
        {
            virtualCameraTranform = new GameObject(name).transform;
            virtualCameraTranform.SetParent(mainCamera.transform);
        }
        return virtualCameraTranform;
    }

    /// <summary>
    /// 设置人物动画轨道,相机轨道
    /// </summary>
    /// <param name="director">播放器</param>
    /// <param name="animator">人物动画</param>
    /// <param name="brain">主相机</param>
    /// <param name="cameras">机位列表</param>
    private void SetCinemachineTrack(PlayableDirector director, CinemachineBrain brain, params CinemachineVirtualCameraBase[] cameras)
    {
        bool cinemachineFinded = false;
        bool animatorFinded = false;

        foreach (PlayableBinding binding in director.playableAsset.outputs)
        {
            if (binding.streamName.Equals("CinemachineTrack"))
            {
                director.SetGenericBinding(binding.sourceObject, brain);

                int index = 0;

                CinemachineTrack track = (CinemachineTrack)binding.sourceObject;
                foreach (TimelineClip clip in track.GetClips())
                {
                    CinemachineShot cameraShot = clip.asset as CinemachineShot;

                    director.SetReferenceValue(cameraShot.VirtualCamera.exposedName, cameras != null && index < cameras.Length ? cameras[index] : null);

                    index++;
                }
                cinemachineFinded = true;
            }

            if (cinemachineFinded && animatorFinded)
            {
                break;
            }
        }
    }

    /// <summary>
    /// 按名称查找挂点
    /// </summary>
    /// <param name="root">根节点</param>
    /// <param name="name">名称</param>
    /// <returns>挂点</returns>
    private Transform FindTransformBy(Transform root, string name)
    {
        for (int i = 0; i < root.childCount; i++)
        {
            Transform child = root.GetChild(i);
            if (child.name.Equals(name))
            {
                return child;
            }
            else
            {
                Transform result = FindTransformBy(child, name);
                if (result != null)
                {
                    return result;
                }
            }
        }
        return null;
    }

    /// <summary>
    /// 更新中心显示
    /// </summary>
    private void UpdateCenterInfo()
    {
        int selectedIndex = m_SelectedIndex;
        Image funcIcon = FindComponent<Image>("Content/Content/Quan/Tiao_Image/Image_Icon");
        TMP_Text funcName = FindComponent<TMP_Text>("Content/Content/Quan/Tiao_Image/Label_Name");
        TMP_Text funcDesc = FindComponent<TMP_Text>("Content/Content/Quan/Tiao_Image/Label_Describe");

        if (selectedIndex >= 0)
        {            
            funcIcon.gameObject.SetActive(selectedIndex < m_ButtonIconNames.Length);
            funcName.text = selectedIndex < m_ButtonNameIds.Length ? TableUtil.GetLanguageString(m_ButtonNameIds[selectedIndex]) : "";
            funcDesc.text = selectedIndex < m_ButtonDescIds.Length ? TableUtil.GetLanguageString(m_ButtonDescIds[selectedIndex]) : "";

            if (funcIcon.gameObject.activeSelf)
            {
                funcIcon.color = Color.white;              
                UIUtil.SetIconImage(funcIcon, GameConstant.FUNCTION_ICON_ATLAS_ASSETADDRESS, m_ButtonIconNames[selectedIndex]);
            }

            //按钮选中/切换音效
            WwiseUtil.PlaySound((int)WwiseMusic.Music_Panel_Watch_ButtonChange, false, null);
        }
        else
        {
            funcIcon.color = Color.clear;
            funcName.text = string.Empty;
            funcDesc.text = string.Empty;           
        }

        for (int i = 0; i < m_ButtonBox.childCount; i++)
        {
            Animator animator = m_ButtonBox.GetChild(i).Find("Button").GetComponent<Animator>();
            if (animator != null && animator.isActiveAndEnabled)
            {
                animator.SetBool("IsOn", i == selectedIndex);
            }           
        }
    }

    /// <summary>
    /// 协程
    /// </summary>
    /// <returns>IEnumerator</returns>
    private IEnumerator UpdateWatch()
    {
        while(true)
        {
            BindWatchPointPosition();

            //如果打开动画已经到了最后一帧，手动调用播放结束回调
            if (m_LookAtState ==  TimeLineState.OPENING && m_OpenTimeline != null && m_OpenTimeline.isActiveAndEnabled && m_OpenTimeline.state == PlayState.Playing && m_OpenTimeline.time >= m_OpenTimeline.duration)
            {
                OnPlayableDirectorStoped(m_OpenTimeline);
            }

            //分状态处理
            if (m_LookAtState == TimeLineState.NONE)
            {
                if (m_WatchKeyHolding)
                {
                    //启动打开动画
                    PlayTimeLine(m_OpenTimeline);
                    m_LookAtState = TimeLineState.OPENING;
                }
            }
            else if(m_LookAtState== TimeLineState.OPENING)
            {
                //取消打开动画
                if (!InputManager.IsWatchInputMode())
                    ForceCloseWatch(false);
            }
            else if (m_LookAtState == TimeLineState.OPENED)
            {
                if(!InputManager.IsWatchInputMode())
                {
                    ForceCloseWatch(false);
                }
                else if (!m_WatchKeyHolding)
                {
                    if (m_SelectedIndex >= 0)
                    {
                        ListenHotkeys(true, false);

                        // 延迟一帧开子面板,避免子面板出错，导致自已也出错。
                        UIManager.Instance.StartCoroutine(OpenPanel(m_SelectedIndex));

                        //等待窗口打开
                        while (InputManager.IsWatchInputMode())
                        {
                            BindWatchPointPosition();
                            yield return new WaitForEndOfFrame();
                        }

                        m_WatchPanel.gameObject.SetActive(false);

                        //等待窗口关闭
                        while (!InputManager.IsWatchInputMode())
                        {
                            BindWatchPointPosition();
                            yield return new WaitForEndOfFrame();
                        }

                        m_WatchPanel.gameObject.SetActive(true);
                    }

                    //启动关闭动画
                    PlayTimeLine(m_CloseTimeline);
                    m_LookAtState = TimeLineState.CLOSEING;
                    m_WatchKeyHolding = false;
                }
            }

            yield return new WaitForEndOfFrame();
        }
    }

    /// <summary>
    /// 绑定3DCanvas坐标到挂点
    /// </summary>
    private void BindWatchPointPosition()
    {
        if (m_LookAtState != TimeLineState.NONE && m_WatchPoint != null)
        {
            /*
            UIManager.Instance.WorldSpaceCanvas.transform.position = m_WatchPoint.transform.position;
            UIManager.Instance.WorldSpaceCanvas.transform.rotation = m_WatchPoint.rotation;

            Camera cinemaMain = CameraManager.GetInstance().GetMainCamereComponent().GetCamera();
            if (cinemaMain)
            {
                UIManager.Instance.WorldSpaceCanvas.Canvas.worldCamera.transform.position = cinemaMain.transform.position;
                UIManager.Instance.WorldSpaceCanvas.Canvas.worldCamera.transform.rotation = cinemaMain.transform.rotation;
            }*/

            Canvas.transform.position = m_WatchPoint.transform.position;
            Canvas.transform.rotation = m_WatchPoint.rotation;

            Camera cinemaMain = CameraManager.GetInstance().GetMainCamereComponent().GetCamera();
            if (cinemaMain)
            {
                Canvas.worldCamera.transform.position = cinemaMain.transform.position;
                Canvas.worldCamera.transform.rotation = cinemaMain.transform.rotation;
            }
        }
    }

    /// <summary>
    /// 根据角度突出显示对应的选中项
    /// </summary>
    /// <param name="direction">方向矢量</param>
    private void SetSelection(Vector2 direction)
    {
        direction = direction.normalized * new Vector2(-1, -1);

        float angleStep = 360.0f / m_ButtonBox.childCount;

        float max = float.MinValue;
        int index = -1;

        if (direction.magnitude > 0)
        {
            Vector3 vector = Quaternion.Euler(0, 0, 180.0f) * Vector3.up;
            Quaternion rotation = Quaternion.Euler(0, 0, angleStep);
            for (int i = 0; i < m_ButtonBox.childCount; i++)
            {
                float value = Vector2.Dot(direction, vector);
                if (value > 0)
                {
                    if (value > max)
                    {
                        max = value;
                        index = i;
                    }
                }
                vector = rotation * vector;
            }
        }

        if (index != -1 && m_SelectedIndex != index)
        {
            m_SelectedIndex = index;

            UpdateCenterInfo();
        }
    }

    /// <summary>
    /// 打开面板
    /// </summary>
    /// <param name="SelectedIndex">索引ID</param>
    /// <returns>IEnumerator</returns>
    private IEnumerator OpenPanel(int SelectedIndex)
    {
        yield return new WaitForEndOfFrame();

        switch (SelectedIndex)
        {
            case 0:
                UIManager.Instance.OpenPanel(UIPanel.PackagePanel/*, UIManager.Instance.Get3DUICanvas().transform*/);
                break;
            case 1:               
                UIManager.Instance.OpenPanel(UIPanel.SocialPanel/*, UIManager.Instance.Get3DUICanvas().transform*/);
                break;
            case 2:
				UIManager.Instance.OpenPanel(UIPanel.MailPanel/*, UIManager.Instance.Get3DUICanvas().transform*/);
                //UIManager.Instance.OpenPanel(UIPanel.ProduceDialogPanel/*, UIManager.Instance.Get3DUICanvas().transform*/);
                break;
        }
    }
}
