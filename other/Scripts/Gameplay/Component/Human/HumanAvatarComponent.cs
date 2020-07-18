using Assets.Scripts.Define;
using UnityEngine;
using UnityEngine.Assertions;

public interface IHumanAvatarProperty
{
	bool IsMain();

	string GetModelName();

	Transform GetModleParent();

	void SetSkinTransform(Transform transform);

	Transform GetSkinTransform();

	Quaternion GetBornServerRotation();

	KHeroType GetHeroType();
}

public class HumanAvatarComponent : EntityComponent<IHumanAvatarProperty>
{
    private IHumanAvatarProperty m_HumanAvatarProperty;

    public override void OnInitialize(IHumanAvatarProperty property)
    {
        m_HumanAvatarProperty = property;

        if (m_HumanAvatarProperty.GetModelName() != "None")
        {
            AssetUtil.InstanceAssetAsync(property.GetModelName(),
                (pathOrAddress, returnObject, userData) =>
                {
                    if (returnObject != null)
                    {
                        GameObject pObj = (GameObject)returnObject;
                        pObj.transform.SetParent(property.GetModleParent(), false);
                        OnLoadModel(pObj);
                    }
                    else
                    {
                        Debug.LogError(string.Format("资源加载成功，但返回null,  pathOrAddress = {0}", pathOrAddress));
                    }
                });
        }
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        if (m_HumanAvatarProperty.GetModelName() != "None")
        {
			if (m_HumanAvatarProperty.GetSkinTransform() != null)
			{
				//AssetManager.ReleaseInstance(m_HumanAvatarProperty.GetSkinTransform().gameObject);
			}
        }
    }

    private void OnLoadModel(GameObject model)
    {
       // GameObject model = asyncOperation.Result;

        Assert.IsNotNull(model, "model is null");

        LayerUtil.SetGameObjectToLayer(model, model.transform.parent.gameObject.layer, true);

        m_HumanAvatarProperty.SetSkinTransform(model.transform);
        model.transform.rotation = m_HumanAvatarProperty.GetBornServerRotation();

        Animator animator = model.transform.GetComponentInChildren<Animator>();
        if (animator != null)
        {
            SendEvent(ComponentEventName.AvatarLoadFinish,
                new AvatarLoadFinishEvent() { Animator = animator }
            );
		}

		int humanLayer = m_HumanAvatarProperty.GetHeroType() == KHeroType.htPlayer
						? m_HumanAvatarProperty.IsMain()
							? GameConstant.LayerTypeID.MainPlayer
							: GameConstant.LayerTypeID.HumanOtherPlayer
						: GameConstant.LayerTypeID.HumanNPC;
		LayerUtil.SetGameObjectToLayer(model, humanLayer, true);

        SetSoundListenerTarget();
    }

    /// <summary>
    /// 设置音效ListenerTarget
    /// </summary>
    private void SetSoundListenerTarget()
    {
        if (m_HumanAvatarProperty.IsMain())
        {
            WwiseUtil.SetListenerTarget(m_HumanAvatarProperty.GetModleParent());
        }
    }
}
