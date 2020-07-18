using Cinemachine;
using System;
using UnityEngine;
using static Cinemachine.CinemachineCore;

public interface IHumanInputProperty
{

}

/// <summary>
/// 人形态输入逻辑
/// </summary>
public class HumanInputComponent : EntityComponent<IHumanInputProperty>
{
    /// <summary>
    /// 输入模式
    /// </summary>
    private InputMode m_InputMode;
    /// <summary>
    /// 移动杆量
    /// </summary>
    private Vector3 m_EngineAxis;
    /// <summary>
    /// 摄像机方向
    /// </summary>
    private Vector2 m_CameraVector;

    public override void OnInitialize(IHumanInputProperty property)
    {
    }

    public override void OnAddListener()
    {
        AddListener(ComponentEventName.InputSample, OnInputSample);

        //HotkeyManager.Instance.Register("HumanInputComponent_main_stick", HotKeyMapID.HUMAN, HotKeyID.HumanMoveStick, JoyStick);
        HotkeyManager.Instance.Register("HumanInputComponent_move_keyboard", HotKeyMapID.HUMAN, HotKeyID.HumanMoveAxis, JoyStick);
        HotkeyManager.Instance.Register("HumanInputComponent_OnCameraStick", HotKeyMapID.HUMAN, HotKeyID.HumanCamera, OnCameraStick);

        CinemachineCore.GetInputAxis += InputAxisOverride;

        InputManager.Instance.OnInputActionMapChanged += OnInputMapChanged;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        CinemachineCore.GetInputAxis -= InputAxisOverride;

        //HotkeyManager.Instance.Unregister("HumanInputComponent_main_stick");
        HotkeyManager.Instance.Unregister("HumanInputComponent_move_keyboard");
        HotkeyManager.Instance.Unregister("HumanInputComponent_OnCameraStick");
        
        InputManager.Instance.OnInputActionMapChanged -= OnInputMapChanged;
    }

    private void OnInputMapChanged(HotKeyMapID arg2)
    {
        m_EngineAxis = Vector3.zero;
        m_CameraVector = Vector3.zero;
    }

    public float InputAxisOverride(string axisName)
    {
        if (axisName == "Mouse X")
            return m_CameraVector.x;
        else if (axisName == "Mouse Y")
            return m_CameraVector.y;

        return 0;
    }

    private void OnCameraStick(HotkeyCallback callbackContext)
    {
        m_CameraVector = callbackContext.ReadValue<Vector2>();
        if (callbackContext.control.name == "delta")
        {
            m_CameraVector *= 0.1f;
        }
        m_CameraVector.y *= -1;
    }

    private void JoyStick(HotkeyCallback obj)
    {
        var vector = obj.ReadValue<Vector2>();
        if (Mathf.Abs(vector.x) < 0.1f)
        {
            vector.x = 0f;
        }
        if (Mathf.Abs(vector.y) < 0.1f)
        {
            vector.y = 0f;
        }

        m_EngineAxis.x = vector.x;
        m_EngineAxis.z = vector.y;

        //Debug.LogErrorFormat($"JoyStick vectorX:{vector.x} vectorY:{vector.y}");
    }

    /// <summary>
    /// 输入采样事件
    /// </summary>
    /// <param name="componentEvent"></param>
    private void OnInputSample(IComponentEvent componentEvent)
    {
        SendEvent(ComponentEventName.ChangeHumanInputState, new ChangeHumanInputStateEvent() { EngineAxis = m_EngineAxis });
    }
}

