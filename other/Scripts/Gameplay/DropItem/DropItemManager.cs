using Assets.Scripts.Define;
using Crucis.Protocol;
//using Assets.Scripts.Proto;
using Leyoutech.Core.Effect;
using Eternity.FlatBuffer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public struct DropItemInfo
{
	public ulong uid;
	public uint tid;
	public uint qualityid;
	public uint shipgrade;
}

public class DropItemManager : Singleton<DropItemManager>
{
	private CfgEternityProxy m_CfgEternityProxy;
    /// <summary>
    /// 宝箱信息  key 为怪物uid 
    /// </summary>
    private Dictionary<ulong, DropItemInfo> m_ChestInfoDic = new Dictionary<ulong, DropItemInfo>();
    /// <summary>
    /// 预存的怪物SpacecraftEntity信息  key 为怪物uid 
    /// </summary>
    private Dictionary<ulong, SpacecraftEntity> m_SpacecraftEntityDic = new Dictionary<ulong, SpacecraftEntity>(4);
    /// <summary>
    /// 宝箱的GameObject信息  key 为怪物uid 
    /// </summary>
    private Dictionary<ulong, GameObject> m_ChestGosDic = new Dictionary<ulong, GameObject>();
    /// <summary>
    /// 宝箱的品质特效GameObject信息  key 为怪物uid 
    /// </summary>
	private Dictionary<ulong, GameObject> m_EffectGosDic = new Dictionary<ulong, GameObject>(4);
    /// <summary>
    /// 宝箱的出生特效GameObject信息  key 为怪物uid 
    /// </summary>
	private Dictionary<ulong, GameObject> m_BrothEffectGosDic = new Dictionary<ulong, GameObject>(4);
    /// <summary>
    /// 宝箱的拾取第一阶段拖尾特效GameObject信息  key 为怪物uid 
    /// </summary>
	private Dictionary<ulong, GameObject> m_PickupEffectGosDic = new Dictionary<ulong, GameObject>(4);
    /// <summary>
    /// 宝箱的拾取第二阶段爆炸特效GameObject信息  key 为怪物uid 
    /// </summary>
	private Dictionary<ulong, GameObject> m_PickupNextEffectGosDic = new Dictionary<ulong, GameObject>(4);
    /// <summary>
    /// 宝箱的拾取成功信息  key 为怪物uid value 为 服务器消息S2C_CHEST_GET_RESULT errorcode 1
    /// </summary>
    private Dictionary<ulong, uint> m_PickupSuccessDic = new Dictionary<ulong, uint>(4);
    /// <summary>
    /// 宝箱的拾取第一阶段拖尾特效的tweener
    /// </summary>
    private Dictionary<ulong, DG.Tweening.Tweener> m_TweenDic = new Dictionary<ulong, DG.Tweening.Tweener>();
	private int m_NeedAddUpdate = 0;

	private static float ms_DelayRemoveTime = 1.0f; //给宝箱拾取特效预留的时间  延迟删除宝箱
	private Transform m_MainPlayerTransform;        //主角的transform

	public DropItemManager()
	{
		m_CfgEternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
	}

	/// <summary>
	/// 保存掉落信息
	/// </summary>
	/// <param name="hero_uid">怪物唯一id</param>
	/// <param name="drop_list">所有玩家的掉落list 注意是所有玩家的 --我觉得是隐患</param>
	internal void OnSaveDropItemInfo(uint hero_uid, List<DropInfo> drop_list, int type)
	{
		int length = drop_list.Count;
		if (length <= 0)
		{
			return;
		}

		ServerListProxy serverListProxy = GameFacade.Instance.RetrieveProxy(ProxyName.ServerListProxy) as ServerListProxy;
		ulong MainPlayerId = serverListProxy.GetCurrentCharacterVO().UId;
		for (int i = 0; i < length; ++i)
		{
			if (drop_list[i].PlayerUid == MainPlayerId)
			{
				DropItemInfo dropItemInfo = new DropItemInfo();
				dropItemInfo.uid = hero_uid;
				dropItemInfo.tid = drop_list[i].ChestTid;
				dropItemInfo.qualityid = drop_list[i].Quality;
				dropItemInfo.shipgrade = drop_list[i].ShipTlv;
				m_ChestInfoDic.Add(hero_uid, dropItemInfo);
                break;
			}
		}
	}

    /// <summary>
    /// 检查是否有宝箱掉落信息
    /// </summary>
    public bool CheckIsDropItem(uint uid)
	{
		if (m_ChestInfoDic.ContainsKey(uid))
		{
			return true;
		}

		return false;
	}

	/// <summary>
	/// TODO.
	/// 切图清缓存,切图会重新同步
	/// </summary>
	public void ClearInfo()
	{
		m_ChestInfoDic.Clear();
		m_ChestGosDic.Clear();
		m_SpacecraftEntityDic.Clear();
	}

	/// <summary>
	/// 清缓存
	/// </summary>
	public void RemoveInfo(ulong key)
	{
		if (m_ChestInfoDic.ContainsKey(key))
		{
			m_ChestInfoDic.Remove(key);
		}
		if (m_ChestGosDic.ContainsKey(key))
		{
			m_ChestGosDic.Remove(key);
		}
		if (m_SpacecraftEntityDic.ContainsKey(key))
		{
			Debug.Log("RemoveEntity RemoveInfo key:" + key);
			m_SpacecraftEntityDic.Remove(key);
		}
	}

	/// <summary>
	/// 创建掉落物
	/// </summary>
	/// <param name="uid">怪物唯一id</param>
	/// <param name="pos">怪物位置</param>
	/// <param name="type">1为new消息， 2为死亡消息</param>
	public void CreateDropItem(uint uid, SpacecraftEntity spe, int type)
	{
		ulong key = uid;
		if (!spe)
		{
			if (m_ChestInfoDic.ContainsKey(key))
			{
				m_ChestInfoDic.Remove(key);
			}
			if (m_SpacecraftEntityDic.ContainsKey(key))
			{
				Debug.Log("CreateDropItem !spe m_SpacecraftEntityDic remove key:" + key);
				m_SpacecraftEntityDic.Remove(key);
			}
			return;
		}
		if (type == 1 && !m_SpacecraftEntityDic.ContainsKey(key))
		{
			m_SpacecraftEntityDic.Add(key, spe);
		}
		if (!m_ChestInfoDic.ContainsKey(uid))
		{
			return;
		}

		if (m_ChestGosDic.ContainsKey(uid))
		{
			return;
		}

		CfgEternityProxy cfgEternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
		PackageBoxAttr? pb = cfgEternityProxy.GetPackageBoxAttrByModelIdAndGrade(m_ChestInfoDic[uid].tid, m_ChestInfoDic[uid].shipgrade);
		if (!pb.HasValue)
		{
			return;
		}

		int modelid = pb.Value.BoxModel;
		Model? mdata = m_CfgEternityProxy.GetModel(modelid);
		if (string.IsNullOrEmpty(mdata.Value.AssetName))/// (mdata.Value.AssetName.Equals(string.Empty))
		{
			Debug.LogErrorFormat("DropItem model assetName is empty  modelid {0}", modelid);
			return;
		}

        AssetUtil.InstanceAssetAsync(mdata.Value.AssetName,
        (pathOrAddress, returnObject, userData) =>
        {
            if (returnObject != null)
            {
				if (!spe)
				{
					Debug.Log("InstanceAssetAsync CreateDropItem !spe m_SpacecraftEntityDic remove key:" + key);
					GameObject.Destroy(returnObject);
					m_SpacecraftEntityDic.Remove(key);
					m_ChestInfoDic.Remove(key);
					return;
				}
				GameObject go = (GameObject)returnObject;
                go.transform.SetParent(spe.GetRootTransform(), false);

                if (go.transform.parent == null)
                {
                    m_ChestGosDic.Remove(uid);
                    return;
                }

                //美术不去 这玩意没有加collide需求为啥不给去？ 吐个槽 xswl
                BoxCollider box = go.GetComponentInChildren<BoxCollider>();
				if (box)
				{
					box.enabled = false;
				}
				else
				{
					Debug.LogWarning("找不到没必要的boxcollider 美术去了还是换了别的碰撞");
				}

                spe.GetSkinRootTransform().gameObject.SetActive(false);
				m_ChestGosDic.Add(key, go);
				AddBrothOrPickEffect(key, 1);
            }
            else
            {
                Debug.LogError(string.Format("资源加载成功，但返回null,  pathOrAddress = {0}", pathOrAddress));
            }
        });

    }

    /// <summary>
    /// 设置掉落无信息
    /// </summary>
    public void SetDropItemInfoByDeath(uint heroID, List<DropInfo> drop_list)
	{
		OnSaveDropItemInfo(heroID, drop_list, 2);
		bool needAddIntera = CheckIsDropItem(heroID);
		GameplayProxy gameplayProxy = GameFacade.Instance.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;
		SpacecraftEntity spe = gameplayProxy.GetEntityById<SpacecraftEntity>(heroID) as SpacecraftEntity;
		if (needAddIntera)
		{
			spe.SetDropItemUId(heroID);
			DropItemManager.Instance.CreateDropItemByDeath(heroID, spe);
		}
	}

	public void AddDestroyEffect(ulong key)
	{
		if (!m_ChestGosDic.ContainsKey(key) || m_ChestGosDic[key] == null)
		{
			return;
		}

		GameObject dropItem = null;
		if (!m_ChestGosDic.TryGetValue(key, out dropItem))
		{
			return;
		}
		
		EffectController dropItemFX = EffectManager.GetInstance().CreateEffect(
									"Z_Death_BOX"
									, EffectManager.GetEffectGroupNameInSpace(false));
		dropItemFX.transform.SetParent(null, false);
		dropItemFX.transform.localPosition = dropItem.transform.localPosition;
		dropItemFX.transform.localRotation = dropItem.transform.localRotation;
	}
	
	/// <summary>
	/// 添加出生或刷新特效
	/// </summary>
	/// <param name="key">宝箱唯一id</param>
	/// <param name="type">1为刷新特效， 2为拾取特效 3为拾取到主角身上的特效</param>
	private void AddBrothOrPickEffect(ulong key, int type)
	{
		if (!m_ChestGosDic.ContainsKey(key) || m_ChestGosDic[key] == null)
		{
			return;
		}

		if (m_BrothEffectGosDic.ContainsKey(key) && type == 1)
		{
			return;
		}

		if (m_PickupEffectGosDic.ContainsKey(key) && type == 2)
		{
			return;
		}

		if (m_PickupNextEffectGosDic.ContainsKey(key) && type == 3)
		{
			return;
		}

		GameObject dropItem = null; // m_ChestGosDic[key]这样不行吗???
		if (!m_ChestGosDic.TryGetValue(key, out dropItem))
		{
			return;
		}

		CfgEternityProxy cfgEternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
		PackageBoxAttr? pb = cfgEternityProxy.GetPackageBoxAttrByModelIdAndGrade(m_ChestInfoDic[key].tid, m_ChestInfoDic[key].shipgrade);
		if (pb == null)
		{
			return;
		}

		if (m_MainPlayerTransform == null)
		{
			GameplayProxy gameplayProxy = GameFacade.Instance.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;
			SpacecraftEntity mainPlayer = gameplayProxy.GetMainPlayer();
			m_MainPlayerTransform = (mainPlayer != null) ? mainPlayer.GetRootTransform() : null;
		}

		GameObject pgo = null;
		if (!m_ChestGosDic.TryGetValue(key, out pgo))
		{
			return;
		}

		string effName = string.Empty;
		Transform parent = null;
		if (type == 1)
		{
			//effName = pb.Value.RefreshGfx;
			//parent = dropItem.transform;
			if (!string.IsNullOrEmpty(pb.Value.RefreshGfx))
			{
				EffectController fxInstance = EffectManager.GetInstance().CreateEffect(pb.Value.RefreshGfx, EffectManager.GetEffectGroupNameInSpace(false),
					(EffectController effect,System.Object usedata) =>
					{
						/// m_BrothEffectGosDic[key] = effect.gameObject;
						///if (!string.IsNullOrEmpty(pb.Value.ContinuousGfx))
						///{
						///	EffectController gfxInstance = EffectManager.GetInstance().CreateEffect(pb.Value.ContinuousGfx, EffectManager.GetEffectGroupNameInSpace(false));
						///	gfxInstance.transform.SetParent(dropItem.transform, false);
						///	gfxInstance.SetCreateForMainPlayer(false);
						///}
					});
				fxInstance.transform.SetParent(dropItem.transform, false);
				fxInstance.SetCreateForMainPlayer(false);
				UIManager.Instance.StartCoroutine(DelayToAddEffect(key));
			}
			return;
		}
		else if (type == 2)
		{
			/// effName = pb.Value.PickUpGfx;
			effName = "Effect_skill_nengliang_path";
		}
		else if (type == 3)
		{
			/// effName = pb.Value.PickUpHfx;
			effName = "Effect_skill_nengliang_hit";
			parent = m_MainPlayerTransform;
		}

		if (string.IsNullOrEmpty(effName))
		{
			return;
		}

        AssetUtil.InstanceAssetAsync(effName,
            (pathOrAddress, returnObject, userData) =>
            {
                if (returnObject != null)
                {
                    GameObject go = (GameObject)returnObject;
                    go.transform.SetParent(parent, false);

                    if (dropItem != null)
					{
                        /// 飞
						if (type == 2)
						{
							pgo.SetActive(false);
							m_PickupEffectGosDic[key] = go;
							go.transform.position = pgo.transform.position;

							float t = Time.time;
							m_TweenDic[key] = go.transform.DOMove(m_MainPlayerTransform.position, 1.0f).OnComplete(
								() =>
								{
									go.SetActive(false);
									m_TweenDic.Remove(key);
									pgo.transform.position = go.transform.position;
									AddBrothOrPickEffect(key, 3);
								}
							).OnUpdate(
								() =>
								{
									float a = m_TweenDic[key].Duration() - (Time.time - t);
									t = Time.time;
									if (a > 0)
									{
										m_TweenDic[key].ChangeEndValue(m_MainPlayerTransform.position, a, true);
									}
								}
								).SetAutoKill(true);

							m_NeedAddUpdate += 1;
							//UIManager.Instance.StartCoroutine(DelayToDestory(key));
							//if (m_SpacecraftEntityDic[key])
							//{
							//	m_SpacecraftEntityDic[key].SetFocus(false);
							//	Interaction.InteractionManager.GetInstance().UnregisterInteractable(m_SpacecraftEntityDic[key]);
							//}
						}
						else if (type == 3)
						{
							pgo.SetActive(false);
							m_PickupNextEffectGosDic[key] = go;
							//go.transform.position = m_MainPlayerTransform.position;
							m_NeedAddUpdate -= 1;
							UIManager.Instance.StartCoroutine(DelayToDestory(key));

						}


						/// 不飞
						/// if (type == 2)
                        /// {
                        ///     m_PickupEffectGosDic[key] = go;
                        ///     go.transform.position = dropItem.transform.position;
                        ///     if (m_BrothEffectGosDic.ContainsKey(key))
                        ///     {
                        ///         GameObject.DestroyImmediate(m_BrothEffectGosDic[key]);
                        ///         m_BrothEffectGosDic.Remove(key);
                        ///     }
                        ///     dropItem.SetActive(false);
                        ///     ParticleSystem p = go.GetComponentInChildren<ParticleSystem>();
                        ///     float duration = 1.0f;
                        ///     if (p != null)
                        ///     {
                        ///         duration = p.main.duration;
                        ///     }
                        ///     UIManager.Instance.StartCoroutine(DelayToPickNext(key, duration));
						/// 
						/// 	/// 果果牛掰的移除缓存的逻辑，不敢动
						/// 	UIManager.Instance.StartCoroutine(DelayToDestory(key));
						/// }
                    }
                    else
                    {
                        GameObject.DestroyImmediate(go);
                    }
                }
                else
                {
                    Debug.LogError(string.Format("资源加载成功，但返回null,  pathOrAddress = {0}", pathOrAddress));
                }
            });
	}

    /// <summary>
    /// 根据死亡延时创建宝箱
    /// </summary>
    public void CreateDropItemByDeath(ulong key, SpacecraftEntity spe)
	{
		Debug.Log("CreateDropItemByDeath key:" + key + " SpacecraftEntity:" + spe);
		m_SpacecraftEntityDic.Add(key, spe);

		Npc npcVO = spe.GetNPCTemplateVO();
		CfgEternityProxy cfgEternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
		Model model = cfgEternityProxy.GetModel(npcVO.Model);
		int ruinTime = model.RuinTime;
		if (ruinTime > 0)
		{
			UIManager.Instance.StartCoroutine(DelayToCreateDropItemByDeath(key, spe, ruinTime / 1000.0f));
		}
		else
		{
			CreateDropItem((uint)key, spe, 2);
		}
		/// TODO.宝藏特殊处理
		/// 服务器创建后立马死亡，没有死亡特效
		///if (spe.GetHeroType() == KHeroType.htPreicous)
		///{
		///	CreateDropItem((uint)key, spe, 2);
		///}
		///else
		///{
		///	UIManager.Instance.StartCoroutine(DelayToCreateDropItemByDeath(key, spe));
		///}
		///CreateDropItem((uint)key, spe, 2);
	}

    /// <summary>
    /// 延时删除宝箱
    /// </summary>
    private IEnumerator DelayToDestory(ulong key)
	{

		yield return new WaitForSeconds(ms_DelayRemoveTime);
		//Debug.LogErrorFormat("DestoryChestGameObject  {0}  ", key);
		m_PickupSuccessDic.Remove(key);
		if (m_PickupNextEffectGosDic.ContainsKey(key))
		{
			GameObject.Destroy(m_PickupNextEffectGosDic[key]);
			m_PickupNextEffectGosDic.Remove(key);
		}
		DestoryChestGameObject(key);
	}

    /// <summary>
    /// 延时加特效 加这个0.5s是为了让能瞅一眼宝箱
    /// </summary>
    private IEnumerator DelayToAddEffect(ulong key)
    {
        yield return new WaitForSeconds(0.5f);

        AddEffect(key);
    }

	private IEnumerator DelayToPickNext(ulong key, float duration)
	{
		yield return new WaitForSeconds(duration);
		if (m_PickupEffectGosDic.ContainsKey(key))
		{
			GameObject.Destroy(m_PickupEffectGosDic[key]);
			m_PickupEffectGosDic.Remove(key);
		}
	}

	/// <summary>
	/// 延时创建宝箱 延时的时间为怪物死亡播放的所有特效和动作时间的总和
	/// </summary>
	private IEnumerator DelayToCreateDropItemByDeath(ulong key, SpacecraftEntity spe, float delayTime)
	{
		/// 特效（蒋小京需求）宝箱出现延迟3秒
		yield return new WaitForSeconds(delayTime);
		//Debug.LogErrorFormat("CreateDropItem  {0}  ", key);
		CreateDropItem((uint)key, spe, 2);
	}

	/// <summary>
	/// 添加品质特效
	/// </summary>
	/// <param name="key">宝箱唯一id</param>
	/// 
	private void AddEffect(ulong key)
	{
		if (!m_ChestGosDic.ContainsKey(key) || m_ChestGosDic[key] == null)
		{
			return;
		}

		if (m_EffectGosDic.ContainsKey(key))
		{
			return;
		}

		GameObject pgo = null;
		if (!m_ChestGosDic.TryGetValue(key, out pgo))
		{
			return;
		}

		//if (m_BrothEffectGosDic.ContainsKey(key))
		//{
		//	GameObject.DestroyImmediate(m_BrothEffectGosDic[key]);
		//	m_BrothEffectGosDic.Remove(key);
		//}

		if (m_SpacecraftEntityDic.ContainsKey(key) && m_SpacecraftEntityDic[key])
		{
			Interaction.InteractionManager.GetInstance().RegisterInteractable(m_SpacecraftEntityDic[key]);
		}

		CfgEternityProxy cfgEternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
		PackageBoxQuality? pb = cfgEternityProxy.GetPackageBoxQualityByKey(m_ChestInfoDic[key].qualityid);
		if (string.IsNullOrEmpty(pb.Value.BoxModel))
		{
			return;
		}

        AssetUtil.InstanceAssetAsync(pb.Value.BoxModel,
            (pathOrAddress, returnObject, userData) =>
            {
                if (returnObject != null)
                {
                    GameObject go = (GameObject)returnObject;
                    go.transform.SetParent(pgo.transform, false);

					if (m_ChestGosDic.TryGetValue(key, out pgo))
					{
						m_EffectGosDic[key] = go;

						// 不懂为什么写这里，没有BoxModel就不能拾取？？？
						//if (m_SpacecraftEntityDic[key])
						//    Interaction.InteractionManager.GetInstance().RegisterInteractable(m_SpacecraftEntityDic[key]);
					}
					else
					{
						GameObject.DestroyImmediate(go);
					}
                }
                else
                {
                    Debug.LogError(string.Format("资源加载成功，但返回null,  pathOrAddress = {0}", pathOrAddress));
                }
            });
	}

	/// <summary>
	/// 检查是否正在创建宝箱阶段
	/// </summary>
	/// <param name="key">宝箱唯一id</param>
	/// 
	public bool CheckIsOnCreate(ulong uid)
	{
		if (m_ChestInfoDic.ContainsKey(uid) && !m_ChestGosDic.ContainsKey(uid))
		{
			return true;
		}

		return false;
	}

    /// <summary>
    /// 自动拾取
    /// </summary>
    public void AutoPickUp(uint uid)
	{
		if (CheckIsDropItem(uid))
		{
			NetworkManager.Instance.GetDropItemController().OnSendPickUpProtocol(uid);
		}
	}

    /// <summary>
    /// 拾取成功
    /// </summary>
    public void PickUpResult(ulong uid, uint code)
	{
		if (code == (uint)ChestResultErrorCode.ChestResultSuccess)
		{
			if (!m_SpacecraftEntityDic.ContainsKey(uid))
			{
				Debug.LogError("PickUp false, m_SpacecraftEntityDic not have key:" + uid);
				return;
			}
			if (m_SpacecraftEntityDic[uid] 
				&& (m_SpacecraftEntityDic[uid].GetHeroType() == KHeroType.htPreicous
				|| m_SpacecraftEntityDic[uid].GetHeroType() == KHeroType.htNormalChest
				|| m_SpacecraftEntityDic[uid].GetHeroType() == KHeroType.htLockChest))
			{
				m_SpacecraftEntityDic[uid].SendEvent(ComponentEventName.PlaySystemSound, new PlaySystemSound()
				{
					SoundID = WwiseMusicSpecialType.SpecialType_Voice_treasure_event3
				});
			}
			AddBrothOrPickEffect(uid, 2);
			if (m_SpacecraftEntityDic[uid])
			{
				m_SpacecraftEntityDic[uid].SetFocus(false);
				Interaction.InteractionManager.GetInstance().UnregisterInteractable(m_SpacecraftEntityDic[uid]);
				m_SpacecraftEntityDic.Remove(uid);
			}
			m_PickupSuccessDic.Add(uid, code);
		}
	}

    /// <summary>
    /// 检查是否拾取成功 成功返回true
    /// </summary>
    public bool CheckDropItemPickUpSuccess(ulong uid)
	{
		if (m_PickupSuccessDic.ContainsKey(uid)) //让拾取特效播出来
		{
			return true;
		}

		return false;
	}

    /// <summary>
    /// 删除宝箱一系列的gameObject 和信息
    /// </summary>
    public void DestoryChestGameObject(ulong uid)
	{
		if (m_PickupSuccessDic.ContainsKey(uid))
		{
			return;
		}

        if (m_EffectGosDic.ContainsKey(uid))
		{
			GameObject.Destroy(m_EffectGosDic[uid]);
			m_EffectGosDic.Remove(uid);
		}
		if (m_BrothEffectGosDic.ContainsKey(uid))
		{
			GameObject.Destroy(m_BrothEffectGosDic[uid]);
			m_BrothEffectGosDic.Remove(uid);
		}
		if (m_ChestGosDic.ContainsKey(uid))
		{
			GameObject.Destroy(m_ChestGosDic[uid]);
			m_ChestGosDic.Remove(uid);
		}
		if (m_ChestInfoDic.ContainsKey(uid))
		{
			m_ChestInfoDic.Remove(uid);
		}
	}

	public void Destory()
	{
		int length = m_ChestGosDic.Count;
		foreach (GameObject go in m_ChestGosDic.Values)
		{
			GameObject.Destroy(go);
		}

		foreach (GameObject go in m_EffectGosDic.Values)
		{
			GameObject.Destroy(go);
		}
		m_ChestGosDic.Clear();
		m_EffectGosDic.Clear();
	}
}
