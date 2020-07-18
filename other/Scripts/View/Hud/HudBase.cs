using Assets.Scripts.Define;
using System.Collections;
using UnityEngine;

public class HudBase : UIPanelBase
{
    /// <summary>
    /// GameLocalizationProxy
    /// </summary>
    private GameLocalizationProxy m_LocalizationProxy = null;
    /// <summary>
    /// CfgEternityProxy
    /// </summary>
    private CfgEternityProxy m_CfgEternityProxy = null;
    /// <summary>
    /// GameplayProxy
    /// </summary>
    private GameplayProxy m_GameplayProxy = null;

    public HudBase(UIPanel panelName, string assetAddress, PanelType panelType) : base(panelName, assetAddress, panelType) { }

    /// <summary>
    /// GameplayProxy
    /// </summary>
    protected GameplayProxy GamePlayProxy
    {
        get
        {
            if (m_GameplayProxy == null)
                m_GameplayProxy = Facade.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;

            return m_GameplayProxy;
        }
    }


    /// <summary>
    /// 获取本地化文本
    /// </summary>
    /// <param name="id">本地化Key</param>
    /// <returns>本地化文本</returns>
    protected string GetLocalization(string id)
    {
        if (m_LocalizationProxy == null)
            m_LocalizationProxy = Facade.RetrieveProxy(ProxyName.GameLocalizationProxy) as GameLocalizationProxy;

        return m_LocalizationProxy.GetString(id);
    }

    /// <summary>
    /// 当前否为手表或UI输入模式
    /// </summary>
    /// <returns>bool</returns>
    protected bool IsWatchOrUIInputMode()
    {
        return InputManager.IsWatchInputMode() || InputManager.IsUIInputMode();
    }

    /// <summary>
    /// 是否在屏幕内
    /// </summary>
    /// <param name="targetPosition">目标点</param>
    /// <returns>bool</returns>
    protected bool IsInScreen(Vector3 targetPosition, Camera camera)
    {
        Vector3 viewportPoint = camera.WorldToViewportPoint(targetPosition);
        return viewportPoint.x >= 0 && viewportPoint.y >= 0 && viewportPoint.x <= 1 && viewportPoint.y <= 1 && viewportPoint.z > camera.nearClipPlane;
    }

    /// <summary>
    /// 获取玩家Entity
    /// </summary>
    /// <returns></returns>
    protected SpacecraftEntity GetMainEntity()
    {
        return GamePlayProxy.GetEntityById<SpacecraftEntity>(GamePlayProxy.GetMainPlayerUID());
    }

    /// <summary>
    /// 是否在太空中
    /// </summary>
    /// <returns>bool</returns>
    protected bool IsInSpace()
    {
        if (m_CfgEternityProxy == null)
            m_CfgEternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;

        return m_CfgEternityProxy.IsSpace();
    }

    /// <summary>
    /// 是否在跃迁中
    /// </summary>
    /// <returns>bool</returns>
    protected bool IsLeaping()
    {
        SpacecraftEntity entity = GetMainEntity();
        if (entity)
        {
            return entity.IsLeap();
        }
        return false;
    }

    /// <summary>
    /// 是否在战斗状态
    /// </summary>
    /// <returns>bool</returns>
    protected bool IsBattling()
    {
        SpacecraftEntity entity = GetMainEntity();
        if (entity)
        {
            return entity.GetCurrentState().GetMainState() == EnumMainState.Fight;
        }
        else
            return false;
    }

    /// <summary>
    /// 是否在过载状态
    /// </summary>
    /// <returns></returns>
    protected bool IsOverload()
    {
        SpacecraftEntity entity = GetMainEntity();
        if (entity)
            return entity.GetCurrentState().IsHasSubState(EnumSubState.Overload);
        else
            return false;
    }

    /// <summary>
    /// 是否为转化炉状态
    /// </summary>
    /// <returns></returns>
    protected bool IsPeerless()
    {
        SpacecraftEntity entity = GetMainEntity();
        if (entity)
            return entity.GetCurrentState().IsHasSubState(EnumSubState.Peerless);
        else
            return false;
    }

    /// <summary>
    /// 是否为死亡状态
    /// </summary>
    /// <returns>bool</returns>
    protected bool IsDead()
    {
        SpacecraftEntity entity = GetMainEntity();
        if (entity)
            return entity.GetCurrentState().GetMainState() == EnumMainState.Dead;
        else
            return false;
    }

    /// <summary>
    /// 获取任务图标
    /// </summary>
    /// <param name="type">任务类型</param>
    /// <returns>任务图标</returns>
    protected string GetMissionIcon(MissionType type)
    {
        switch (type)
        {
            case MissionType.Main: return "Icon_Mission_Main";
			case MissionType.Branch: return "Icon_Mission_Side";
            case MissionType.Daily: return "Icon_Mission_Day";
            case MissionType.Dungeon: return "Icon_Mission_Instance";
            case MissionType.World: return "Icon_Mission_World";
            //case TaskType.SubTask: return "Icon_Mission_Main";
        }
        return "";
    }

    public override void OnShow(object msg)
    {
        base.OnShow(msg);

        InputManager.Instance.OnInputActionMapChanged -= OnInputMapChanged;
        InputManager.Instance.OnInputActionMapChanged += OnInputMapChanged;

        EnableInertance();
    }

    public override void OnHide(object msg)
    {
        InputManager.Instance.OnInputActionMapChanged -= OnInputMapChanged;

        DisableInertance();

        base.OnHide(msg);
    }

    /// <summary>
    /// 当前输入表改变时
    /// </summary>
    /// <param name="a">改变前的输入表</param>
    /// <param name="b">改变后的输入表</param>
    private void OnInputMapChanged(HotKeyMapID b)
    {
        OnInputMapChanged();
    }

    /// <summary>
    /// 输入表改变的处理函数
    /// </summary>
    protected virtual void OnInputMapChanged()
    {
    }


    #region HUD模拟飞船惯性

    /// <summary>
    /// 是否上升中
    /// </summary>
    private bool m_Ascend = false;
    /// <summary>
    /// 是否下降中
    /// </summary>
    private bool m_Descend = false;
    /// <summary>
    /// 惯性X方向
    /// </summary>
    private float m_XOffset = 0;
    /// <summary>
    /// 惯性Y方向
    /// </summary>
    private float m_YOffset = 0;
    /// <summary>
    /// 惯性Z方向
    /// </summary>
    private float m_ZOffset = 0;
    /// <summary>
    /// 惯性设置
    /// </summary>
    private InertiaWithHud m_InertiaSetting;
    /// <summary>
    /// 协程
    /// </summary>
    private Coroutine m_Coroutine;

    /// <summary>
    /// 启用惯性
    /// </summary>
    private void EnableInertance()
    {
        //m_InertiaSetting = GetTransform().GetComponent<InertiaWithHud>();
        //if (m_InertiaSetting != null && m_InertiaSetting.InertiaEnabled)
        //{
            HotkeyManager.Instance.Register("ship_move_" + GetTransform().GetInstanceID(), HotKeyMapID.SHIP, HotKeyID.ShipMove, OnShipMove);
            HotkeyManager.Instance.Register("ship_ascend_" + GetTransform().GetInstanceID(), HotKeyMapID.SHIP, HotKeyID.ShipAscend, OnShipAscend);
            HotkeyManager.Instance.Register("ship_descend_" + GetTransform().GetInstanceID(), HotKeyMapID.SHIP, HotKeyID.ShipDescend, OnShipDescend);

            m_Coroutine = UIManager.Instance.StartCoroutine(UpdateInertance());
        //}
    }

    /// <summary>
    /// 禁用惯性
    /// </summary>
    private void DisableInertance()
    {
        HotkeyManager.Instance.Unregister("ship_move_" + GetTransform().GetInstanceID());
        HotkeyManager.Instance.Unregister("ship_ascend_" + GetTransform().GetInstanceID());
        HotkeyManager.Instance.Unregister("ship_descend_" + GetTransform().GetInstanceID());

        if (m_Coroutine != null)
        {
            UIManager.Instance.StopCoroutine(m_Coroutine);

            m_Coroutine = null;
        }
    }

    /// <summary>
    /// 飞船移动时
    /// </summary>
    /// <param name="info">热键状态</param>
    private void OnShipMove(HotkeyCallback info)
    {
        Vector2 dir = info.ReadValue<Vector2>();
        m_XOffset = dir.x;
        m_ZOffset = dir.y;
    }
    /// <summary>
    /// 飞船上升时
    /// </summary>
    /// <param name="info">热键状态</param>
    private void OnShipAscend(HotkeyCallback info)
    {
        m_Ascend = info.performed;
    }
    /// <summary>
    /// 飞船下降时
    /// </summary>
    /// <param name="info">热键状态</param>
    private void OnShipDescend(HotkeyCallback info)
    {
        m_Descend = info.performed;
    }

    /// <summary>
    /// 更新惯性协程
    /// </summary>
    /// <returns>IEnumerator</returns>
    private IEnumerator UpdateInertance()
    {
        RectTransform rect = GetTransform().GetComponent<RectTransform>();
        while(true)
        {
            InertiaWithHud inertia = GetTransform().GetComponent<InertiaWithHud>();
            if (inertia != null && inertia.isActiveAndEnabled)
            {
                if (inertia.mode == InertiaWithHud.InertiaMode.Input)
                {
                    UpdateInertanceByInput(rect, inertia);
                }
                else if (inertia.mode == InertiaWithHud.InertiaMode.Rigbody)
                {
                    UpdateInertanceByVelocity(rect, inertia);
                }
            }

            yield return new WaitForEndOfFrame();
        }
    }
    /// <summary>
    /// 根据输入状态更新惯性
    /// </summary>
    /// <param name="rect">要微调的UI</param>
    /// <param name="inertia">惯性设置</param>
    private void UpdateInertanceByInput(RectTransform rect, InertiaWithHud inertia)
    {
        Vector3 offset = rect.anchoredPosition3D;

        m_YOffset = m_Ascend ? 1 : (m_Descend ? -1 : 0);

        float x = Mathf.Lerp(offset.x, m_XOffset * inertia.OffsetX, Time.deltaTime);
        float y = Mathf.Lerp(offset.y, m_YOffset * inertia.OffsetY, Time.deltaTime);
        float z = Mathf.Lerp(offset.z, m_ZOffset * inertia.OffsetZ, Time.deltaTime);

        rect.anchoredPosition3D = new Vector3(x, y, z);
    }

    /// <summary>
    /// 根据刚体速度更新惯性
    /// </summary>
    /// <param name="rect">要微调的UI</param>
    /// <param name="inertia">惯性设置</param>
    private void UpdateInertanceByVelocity(RectTransform rect, InertiaWithHud inertia)
    {
        SpacecraftEntity main = GetMainEntity();
        if (main == null) return;

        Transform mainShadow = main.GetSyncTarget();
        if (mainShadow == null) return;

        Rigidbody rigidbody = mainShadow.GetComponent<Rigidbody>();
        if (rigidbody == null) return;

        SpacecraftMotionComponent mainMotion = main.GetEntityComponent<SpacecraftMotionComponent>();
        if (mainMotion == null) return;

        SpacecraftMotionInfo cruiseMotion = mainMotion.GetCruiseModeMotionInfo();

        Vector3 velocity = rigidbody.transform.InverseTransformDirection(rigidbody.velocity);

        float forwardSpeed = velocity.z;
        float forwardSpeedMax = cruiseMotion.LineVelocityMax.z;
        float forwardSpeedPercent = Mathf.Clamp01(Mathf.Abs(forwardSpeed) / Mathf.Abs(forwardSpeedMax));

        float upSpeed = velocity.y;
        float upSpeedMax = cruiseMotion.LineVelocityMax.y;
        float upSpeedPercent = Mathf.Clamp01(Mathf.Abs(upSpeed) / Mathf.Abs(upSpeedMax));

        float rotationSpeed = rigidbody.angularVelocity.y * Mathf.Rad2Deg;
        float rotationSpeedMax = cruiseMotion.AngularVelocityMax.y;
        float rotationSpeedPercent = Mathf.Clamp01(Mathf.Abs(rotationSpeed) / Mathf.Abs(rotationSpeedMax));

        float forwardDir = Vector3.Dot(rigidbody.velocity.normalized, Camera.main.transform.forward);

        float x = rotationSpeedPercent * inertia.OffsetX * (rotationSpeed > 0 ? 1 : -1);
        float y = upSpeedPercent * inertia.OffsetY * (upSpeed > 0 ? 1 : -1);
        float z = forwardSpeedPercent * inertia.OffsetZ * forwardDir;

        //Debug.LogError("----> " +
        //    string.Format("{0:N2}", forwardSpeed) + "/" + forwardSpeedMax + "          " +
        //    string.Format("{0:N2}", upSpeed) + "/" + upSpeedMax + "           " +
        //    string.Format("{0:N2}", rotationSpeed) + "/" + rotationSpeedMax);

        rect.anchoredPosition3D = new Vector3(x, y, z);
    }

    #endregion
}
