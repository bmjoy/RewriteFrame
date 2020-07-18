using Assets.Scripts.Define;
using Eternity.FlatBuffer;
using UnityEngine;

public interface IHumanAnimatorProperty
{
    bool IsMain();
    uint GetItemID();
    KHeroType GetHeroType();
}

public class HumanAnimatorComponent : EntityComponent<IHumanAnimatorProperty>
{
    /// <summary>
    /// 动画参数
    /// </summary>
    private readonly AnimatorParameterInfo Velocity = new AnimatorParameterInfo() { Name = "velocity", Type = AnimatorControllerParameterType.Float };
    /// <summary>
    /// 动画参数
    /// </summary>
    private readonly AnimatorParameterInfo Turn = new AnimatorParameterInfo() { Name = "turn", Type = AnimatorControllerParameterType.Float };
    /// <summary>
    /// Lerp速度
    /// </summary>
    private float m_LerpSpeed = 8f;
    private Animator m_Animator;

    /// <summary>
    /// 目标杆量
    /// </summary>
    private Vector3 m_TargetEnginAxis;
    /// <summary>
    /// 当前杆量
    /// </summary>
    private Vector3 m_EnginAxis;
    /// <summary>
    /// 是否自己
    /// </summary>
    private bool m_IsMain;

    private int m_musicComboID = -1;

    /// <summary>
    /// 角色类型
    /// </summary>
    private KHeroType m_KHeroType;

    private AK.Wwise.EntityAnimatorOfWwise entityAnimatorOfWwise;

    public override void OnInitialize(IHumanAnimatorProperty property)
    {
        m_IsMain = property.IsMain();
        m_KHeroType = property.GetHeroType();
        CfgEternityProxy cfgEternityProxy = (CfgEternityProxy)GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy);
		if (m_KHeroType == KHeroType.htPlayer)
		{
			Player? player = cfgEternityProxy.GetPlayerByItemTId((int)property.GetItemID());
			if (player.HasValue)
			{
				m_musicComboID = player.Value.MusicComboID;
			}
		}
	}

    public override void OnAddListener()
    {
        AddListener(ComponentEventName.AvatarLoadFinish, OnHumanAvatarLoadFinish);
        AddListener(ComponentEventName.HumanAnimationChangeState, OnHumanAnimationChangeState);
    }

    public override void OnUpdate(float delta)
    {
        if (m_Animator == null)
        {
            return;
        }

        if (m_EnginAxis.x != m_TargetEnginAxis.x)
        {
            m_EnginAxis.x = Mathf.Lerp(m_EnginAxis.x, m_TargetEnginAxis.x, m_LerpSpeed * delta);
            m_Animator.SetFloat(Turn.Name, m_EnginAxis.x);
        }

        if (m_EnginAxis.z != m_TargetEnginAxis.z)
        {
            m_EnginAxis.z = Mathf.Lerp(m_EnginAxis.z, m_TargetEnginAxis.z, m_LerpSpeed * delta);
            m_Animator.SetFloat(Velocity.Name, m_EnginAxis.z);
        }

        entityAnimatorOfWwise.SetVelocityAndTurn(m_EnginAxis.z , m_EnginAxis.x);
    }

    /// <summary>
    /// 改变输入状态回调
    /// </summary>
    /// <param name="componentEvent"></param>
    private void OnHumanAnimationChangeState(IComponentEvent componentEvent)
    {
        HumanAnimationChangeStateEvent inputStateChangeEvent = componentEvent as HumanAnimationChangeStateEvent;

        m_TargetEnginAxis = inputStateChangeEvent.EngineAxis;
    }

    /// <summary>
    /// 皮肤加载完成回调
    /// </summary>
    /// <param name="componentEvent"></param>
    private void OnHumanAvatarLoadFinish(IComponentEvent componentEvent)
    {
        AvatarLoadFinishEvent humanAvatarLoadFinishEvent = componentEvent as AvatarLoadFinishEvent;

        if (!HasParameter(humanAvatarLoadFinishEvent.Animator, Turn))         
        {
            return;
        }

        if(!HasParameter(humanAvatarLoadFinishEvent.Animator, Velocity))
        {
            return;
        }

        m_Animator = humanAvatarLoadFinishEvent.Animator;

        //add 动作音效脚本
        entityAnimatorOfWwise = m_Animator.gameObject.GetOrAddComponent<AK.Wwise.EntityAnimatorOfWwise>();
        entityAnimatorOfWwise.SetIsProtagonist(m_IsMain, m_KHeroType);
        entityAnimatorOfWwise.SetComboID(m_musicComboID);
    }

    /// <summary>
    /// 检查变量是否存在
    /// </summary>
    /// <param name="animator"></param>
    /// <param name="animatorParameterInfo"></param>
    /// <returns></returns>
    private bool HasParameter(Animator animator, AnimatorParameterInfo animatorParameterInfo)
    {
        foreach (var parameter in animator.parameters)
        {
            if (parameter.name == animatorParameterInfo.Name && parameter.type == animatorParameterInfo.Type)
            {
                return true;
            }
        }

        return false;
    }
}
