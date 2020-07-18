using Game.VFXController;
using Eternity.FlatBuffer;
using DG.Tweening;
using UnityEngine;
using System.Collections;
using Leyoutech.Core.Effect;

public class MineDropComponent : MonoBehaviour
{
	private ulong m_UID;

	private uint m_ItemTid;

	/// <summary>
	/// 缓存NpcVO
	/// </summary>
	private Npc m_NpcVO;

	/// <summary>
	/// 停留特效实例
	/// </summary>
	private EffectController m_StayFxInstance = null;

	/// <summary>
	/// 拾取特效实例
	/// </summary>
	private EffectController m_GatherFxInstance = null;

	/// <summary>
	/// 获得者
	/// </summary>
	private SpacecraftEntity m_SpacecraftEntity;

	/// <summary>
	/// 缓存
	/// </summary>
	private PackageBoxAttr? m_PackageBoxAttr;

	/// <summary>
	/// 掉落物状态
	/// </summary>
	private DropItemState m_DropItemState;

	private void Awake()
	{
		m_DropItemState = DropItemState.None;
	}

	private void OnDestroy()
	{
		if (m_StayFxInstance != null)
		{
			m_StayFxInstance.RecycleFX();
			m_StayFxInstance = null;
		}
		if (m_GatherFxInstance != null)
		{
			m_GatherFxInstance.RecycleFX();
			m_GatherFxInstance = null;
		}
	}

	public void Initialize(SpacecraftEntity mainPlayer, ulong uid, uint tid, uint item_tid, PackageBoxAttr? pb)
	{
		m_SpacecraftEntity = mainPlayer;
		m_UID = uid;
		m_ItemTid = item_tid;
		m_PackageBoxAttr = pb;
		m_DropItemState = DropItemState.Born;

		m_NpcVO = (GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy).GetNpcByKey(tid);
	}

	public void AddFlyEffect(float posX, float posY, float posZ)
	{
		CfgEternityProxy cfgEternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
		PackageItem packageItem = cfgEternityProxy.PackageItemsByKey(m_ItemTid);
		GameplayProxy gameplayProxy = GameFacade.Instance.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;
		Vector3 endPos = new Vector3(posX, posY, posZ);
		Vector3 clientPosition = gameplayProxy.ServerAreaOffsetToClientPosition(endPos);
		AddEffect();

		float flyTime = Random.Range(packageItem.FlyTimeMin, packageItem.FlyTimeMax);
		gameObject.transform.DOMove(clientPosition, flyTime).SetEase((Ease)packageItem.FlySpeed).OnComplete(
			() =>
			{
				m_DropItemState = DropItemState.Stay;
				AddEffect();
			}
		);
	}

	public void AddEffect(bool isStay = false)
	{
		if (!m_PackageBoxAttr.HasValue || m_DropItemState == DropItemState.None)
		{
			return;
		}

		if (m_DropItemState == DropItemState.Born)
		{
			EffectController bornFxInstance = EffectManager.GetInstance().CreateEffect(m_PackageBoxAttr.Value.RefreshGfx, EffectManager.GetEffectGroupNameInSpace(false));
			bornFxInstance.transform.position = gameObject.transform.position;
			WwiseUtil.PlaySound((int)WwiseMusic.DropItem_Bron, false, bornFxInstance.transform.position);
			bornFxInstance.SetCreateForMainPlayer(false);

			m_StayFxInstance = EffectManager.GetInstance().CreateEffect(m_PackageBoxAttr.Value.ContinuousGfx, EffectManager.GetEffectGroupNameInSpace(false));
			m_StayFxInstance.transform.SetParent(gameObject.transform, false);
			m_StayFxInstance.SetCreateForMainPlayer(false);
		}
		else if (m_DropItemState == DropItemState.Stay && isStay)
		{
			m_StayFxInstance = EffectManager.GetInstance().CreateEffect(m_PackageBoxAttr.Value.ContinuousGfx, EffectManager.GetEffectGroupNameInSpace(false));
			m_StayFxInstance.transform.SetParent(gameObject.transform, false);
			m_StayFxInstance.SetCreateForMainPlayer(false);
		}
		else if (m_DropItemState == DropItemState.Gather)
		{
			RemoveStayFx();

			m_GatherFxInstance = EffectManager.GetInstance().CreateEffect(m_PackageBoxAttr.Value.PickUpGfx, EffectManager.GetEffectGroupNameInSpace(false));
			m_GatherFxInstance.transform.position = gameObject.transform.position;
			m_GatherFxInstance.SetCreateForMainPlayer(false);
		}
	}

	public void SetDropItemState(DropItemState state)
	{
		m_DropItemState = state;
	}

	public DropItemState GetDropItemState()
	{
		return m_DropItemState;
	}

	private void RemoveBornFx()
	{
	}

	private void RemoveStayFx()
	{
		if (m_StayFxInstance != null)
		{
			m_StayFxInstance.StopFX();
			m_StayFxInstance = null;
		}
	}

	private void RemoveGatherFx()
	{
		if (m_StayFxInstance != null)
		{
			m_StayFxInstance.StopFX();
			m_StayFxInstance = null;
		}
	}

	private float GetInteractableDistanceSquare()
	{
		float triggerRange = m_NpcVO.TriggerRange;
		return triggerRange * triggerRange;
	}

	private void LateUpdate()
	{
		if (m_SpacecraftEntity && m_DropItemState == DropItemState.Stay)
		{
			Vector3 playerPos = m_SpacecraftEntity.GetRootTransform().position;
			Vector3 currentPos = gameObject.transform.position;
			float square = GetInteractableDistanceSquare();
			float dis = (playerPos - currentPos).sqrMagnitude;
			if (dis <= square)
			{
				/// 拾取
				m_DropItemState = DropItemState.Gather;
				MineDropItemManager.Instance.AutoPickUp(m_UID);
			}
		}
	}

}

