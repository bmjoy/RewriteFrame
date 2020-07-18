using System;
using UnityEngine;
using UnityEngine.Assertions;

public interface ISpacecraftCheckOffsetProperty
{
    Rigidbody GetRigidbody();
    Transform GetRootTransform();
}

/// <summary>
/// 船形态 拖拽主玩家逻辑
/// </summary>
public class SpacecraftCheckeOffsetComponent : EntityComponent<ISpacecraftCheckOffsetProperty>
{
    private ISpacecraftCheckOffsetProperty m_SpacecraftCheckOffsetProperty;
    private GameplayProxy m_GameplayProxy;

    /// <summary>
    /// 是否应用拖拽
    /// </summary>
    bool m_IsAvailableOffset = true;

    public override void OnInitialize(ISpacecraftCheckOffsetProperty property)
    {
        m_SpacecraftCheckOffsetProperty = property;

        m_GameplayProxy = GameFacade.Instance.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;
    }

    public override void OnAddListener()
    {
        Map.MapManager.GetInstance()._OnChangedArea += OnChangeArea;
        AddListener(ComponentEventName.SpacecraftLeapEnd, OnSpacecraftLeapEnd);
    }

    public override void OnDestroy()
    {
        Map.MapManager.GetInstance()._OnChangedArea -= OnChangeArea;
    }

    private void OnChangeArea(ulong areaId)
    {
        if (areaId != m_GameplayProxy.GetLeapTargetAreaUid())
        {
            return;
        }

        if (!m_IsAvailableOffset)
        {
            return;
        }

        /// 玩家世界坐标
        Vector3 playerRealWorldPosition;
        m_GameplayProxy.ClientPositionToWorldPosition(out playerRealWorldPosition, m_SpacecraftCheckOffsetProperty.GetRigidbody().position);
        /// 跃迁剩余向量
        Vector3 remainingDistance = m_GameplayProxy.GetLeapEndAreaOffsetPosition() - m_GameplayProxy.ClientToServerAreaOffset(m_SpacecraftCheckOffsetProperty.GetRigidbody().position);
        /// 跃迁终点位置
        Vector3 targetPoint = m_GameplayProxy.GetLeapTargetAreaOffset() - remainingDistance;
        /// 拖拽向量
        Vector3 offset = targetPoint - m_SpacecraftCheckOffsetProperty.GetRigidbody().position;
        /// 通知运动模型拖拽
        SendEvent(ComponentEventName.SetOffset, new SetOffsetEvent() { Offset = offset });
        /// 更新当前所在区域
        m_GameplayProxy.SetCurrentAreaUid(m_GameplayProxy.GetLeapTargetAreaUid());
        /// 清除拖拽偏移
        m_GameplayProxy.ClearTotalPositionOffset();

        m_IsAvailableOffset = false;
    }

    private void OnSpacecraftLeapEnd(IComponentEvent componentEvent)
    {
        m_IsAvailableOffset = true;
    }

    public override void OnFixedUpdate()
    {
        if (!m_IsAvailableOffset)
        {
            return;
        }

        Rigidbody rigidbody = m_SpacecraftCheckOffsetProperty.GetRigidbody();

        Vector3 playerClientPosition = rigidbody.position;
        Vector3 totalPositionOffset = Vector3.zero;

        if (m_GameplayProxy.IsNeedOffset(ref playerClientPosition, ref totalPositionOffset))
        {
            Vector3 offsetDifference = playerClientPosition - rigidbody.position;

            rigidbody.position = playerClientPosition;

            m_GameplayProxy.SetPositionOffset(offsetDifference);

            SendEvent(ComponentEventName.SetOffset, new SetOffsetEvent() { Offset = offsetDifference });

            CameraManager.GetInstance().GetMainCamereComponent().TargetObjectWarpedAllCM(offsetDifference);
        }
    }
}
