using Leyoutech.Core.Effect;
using Game.VFXController;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// buff的功能函数的基类
/// 比如, 隐身
/// </summary>
public  class BuffEffectBase
{
	protected Buff m_Buff;

	//private static BuffEffectNull m_EffectNull;
	private static object m_Lock = new object();


    /// <summary>
    /// 特效挂点
    /// </summary>
    public Transform m_SelfTransform;

    /// <summary>
    ///连线型,link 方的挂点
    /// </summary>
    public Transform m_OtherTransform;


    /// <summary>
    /// Start特效
    /// </summary>
    protected EffectController m_StartEffectController;

    /// <summary>
    /// Loop特效
    /// </summary>
    protected EffectController m_LoopEffectController;


    /// <summary>
    /// End特效
    /// </summary>
    protected EffectController m_EndEffectController;


//     /// <summary>
//     /// 没有任何特殊操作的Buff
//     /// </summary>
//     public static BuffEffectNull EffectNull
// 	{
// 		get
// 		{
// 			lock (m_Lock)
// 			{
// 				if (m_EffectNull == null)
// 				{
// 					m_EffectNull = new BuffEffectNull();
// 				}
// 
// 				return m_EffectNull;
// 			}
// 		}
// 	}


    /// <summary>
    /// 初始化
    /// </summary>
    public void Init(Transform selfTransform , Transform otherTransform)
    {
        m_SelfTransform = selfTransform;
        m_OtherTransform = otherTransform;
    }



	/// <summary>
	/// 根据Buff效果的类型获取Buff的效果函数
	/// </summary>
	/// <param name="buffType"></param>
	/// <returns></returns>
	public static BuffEffectBase GetBuffEffectByType(BuffEffectType buffType, Buff buff)
	{
		switch (buffType)
		{
			case BuffEffectType.Invisible:
				return new BuffEffectInvisible(buff);
			case BuffEffectType.Shield:
				return new BuffEffectShield(buff);
            case BuffEffectType.Link_1:                                             //线1，线2 表现一致
            case BuffEffectType.Link_2:
                return new BuffEffectLink(buff);
            default:
                return new BuffEffectBase(buff); 
        }
	}

	public BuffEffectBase(Buff buff)
	{
		m_Buff = buff;
    }

    /// <summary>
    /// Buff添加上时执行的操作
    /// 比如隐身后, 单独给他设一个Layer, 以防被选中
    /// </summary>
    public virtual void OnBuffAdd()
    {

    }

    /// <summary>
    /// Buff移除时执行的操作
    /// 比如隐身取消后, 把他的Layer设回正常值
    /// </summary>
    public virtual void OnBuffRemove()
    {
        StopEffectVFX();
        m_SelfTransform = null;
        m_OtherTransform = null;
        m_StartEffectController = null;
        m_LoopEffectController = null;
        m_EndEffectController = null;
    }

    /// <summary>
    /// 获得当前buff 包含的所有特效实体
    /// </summary>
    /// <returns></returns>
    public List<EffectController> GetCurrBuffEffectControllers()
    {
        List<EffectController> list = new List<EffectController>();
        list.Add(m_StartEffectController);
        list.Add(m_LoopEffectController);
        list.Add(m_EndEffectController);

        return list;
    }

    /// <summary>
    /// 停止特效
    /// </summary>
    private void StopEffectVFX()
    {
        if(m_StartEffectController != null)
        {
            m_StartEffectController.StopAndRecycleFX();
            m_StartEffectController.RecycleFX();
        }

        if (m_LoopEffectController != null)
        {
            m_LoopEffectController.StopAndRecycleFX();
            m_LoopEffectController.RecycleFX();
        }

        if (m_EndEffectController != null)
        {
            m_EndEffectController.StopAndRecycleFX();
            m_EndEffectController.RecycleFX();
        }
    }

	/// <summary>
	/// 循环特效读取完以后的操作
	/// TODO. 这里写的有点难受. 本来 BuffEffectBase 应该是只处理逻辑相关的东西的.
	/// </summary>
	/// <param name="vfx"></param>
	public virtual void OnLoopFXLoaded(EffectController vfx)
	{
	}
	
	public virtual void OnEvent(ComponentEventName eventName, IComponentEvent eventParam)
	{
	}

    /// <summary>
    /// 设置开始特效
    /// </summary>
    /// <param name="starteffct"></param>
    public virtual void SetStartEffectController(EffectController starteffct)
    {
        m_StartEffectController = starteffct;
        m_StartEffectController.transform.SetParent(m_SelfTransform, false);
    }

    /// <summary>
    /// 设置循环特效
    /// </summary>
    /// <param name="loopeffct"></param>
    public virtual void SetLoopEffectController(EffectController loopeffct)
    {
        m_LoopEffectController = loopeffct;
        m_LoopEffectController.transform.SetParent(m_SelfTransform, false);
    }

    /// <summary>
    /// 设置结束特效
    /// </summary>
    /// <param name="endeffct"></param>
    public virtual void SetEndEffectController(EffectController endeffct)
    {
        m_EndEffectController = endeffct;
        m_EndEffectController.transform.SetParent(m_SelfTransform, false);
    }

}

public class BuffEffectNull : BuffEffectBase
{
	public BuffEffectNull() : base(null)
	{
	}

	public override void OnBuffAdd()
	{

	}
}
