/*===============================
 * Author: [Allen]
 * Purpose: BuffEffectLink
 * Time: 2019/11/20 16:57:56
================================*/
/*
 *链接型特效，A-->B， 被标记Is_master, 为主链接 。类似LOL 复仇之矛
 */

using Assets.Scripts.Define;
using Leyoutech.Core.Effect;
using UnityEngine;

public class BuffEffectLink : BuffEffectBase
{   

    public BuffEffectLink(Buff buff) : base(buff)
    {

    }

    public override void OnBuffAdd()
    {
    }


    /// <summary>
    /// 设置开始特效
    /// </summary>
    /// <param name="starteffct"></param>
    public override void SetStartEffectController(EffectController starteffct)
    {
        base.SetStartEffectController(starteffct);

        //TODO  下方待补充        
        //补充链接另一边吧


    }

    /// <summary>
    /// 设置循环特效
    /// </summary>
    /// <param name="loopeffct"></param>
    public override void SetLoopEffectController(EffectController loopeffct)
    {
        base.SetLoopEffectController(loopeffct);

        //TODO  下方待补充        
        //补充链接另一边吧
    }

    /// <summary>
    /// 设置结束特效
    /// </summary>
    /// <param name="endeffct"></param>
    public override void SetEndEffectController(EffectController endeffct)
    {
        base.SetEndEffectController(endeffct);

        //TODO  下方待补充        
        //补充链接另一边吧
    }


    public override void OnLoopFXLoaded(EffectController vfx)
    {
        base.OnLoopFXLoaded(vfx);

        if (m_SelfTransform == null || (m_SelfTransform != null && m_SelfTransform.GetComponent<SpacecraftEntity>() != null && m_SelfTransform.GetComponent<SpacecraftEntity>().IsDead()))
        {
            //Debug.LogWarning("连线特效异步加载特效回来前，自己挂了，特效实例化完成后无用了，干掉");
            vfx.RecycleFX();
            return;
        }


        vfx.transform.SetParent(m_SelfTransform, false);
        if (m_OtherTransform != null && m_OtherTransform.GetComponent<SpacecraftEntity>() != null && !m_OtherTransform.GetComponent<SpacecraftEntity>().IsDead())
        {
            //Debug.LogWarning("连线特效 OK");
            vfx.SetBeamTarget(m_SelfTransform, m_OtherTransform, Vector3.zero);
        }
        else
        {
            //Debug.LogWarning("连线特效异步加载特效回来前，对方目标挂了，特效实例化完成后无用了，干掉");
            vfx.RecycleFX();
        }
    }
}

