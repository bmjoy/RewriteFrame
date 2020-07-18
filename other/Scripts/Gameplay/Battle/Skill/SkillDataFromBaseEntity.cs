/*===============================
 * Author: [Allen]
 * Purpose: 需要存放在baseEntity 内的 技能数据
 * 只是都存放到baseEntity 内比较杂乱，放出来一些
 * Time: 2020/1/10 11:08:06
================================*/
using System;
using UnityEngine;

public class SkillDataFromBaseEntity : MonoBehaviour
{
    /// <summary>
    /// 当前正在释放的技能ID
    /// </summary>
    private int m_CurrSkillId = -1;

    /// <summary>
    /// 当前技能键是否被按下
    /// </summary>
    private bool m_SkillBttonIsDown = false;


    /// <summary>
    /// 引导技能目标变化，函数委托
    /// </summary>
    private Action m_ChangeGuideSkillTargetAction;


    /// <summary>
    /// 当前正在释放的技能ID
    /// </summary>
    /// <param name="currSkillId"></param>
    public void SetCurrSkillId(int currSkillId)
    {
        m_CurrSkillId = currSkillId;
    }


    /// <summary>
    /// 当前正在释放的技能ID
    /// </summary>
    public int GetCurrSkillId()
    {
        return m_CurrSkillId;
    }

    /// <summary>
    /// 当前技能键是否被按下
    /// </summary>
    /// <param name="isdown"></param>
    public void SetSkillBttonIsDown(bool isdown)
    {
        m_SkillBttonIsDown = isdown;
    }

    /// <summary>
    /// 当前技能键是否被按下
    /// </summary>
    /// <returns></returns>
    public bool GetSkillBttonIsDown()
    {
        return m_SkillBttonIsDown;
    }

    /// <summary>
    /// 引导技能目标变化函数变化委托
    /// </summary>
    public void SetChangeGuideSkillTargetAction(Action action)
    {
        m_ChangeGuideSkillTargetAction = action;
    }

    /// <summary>
    /// 引导技能目标变化函数变化委托
    /// </summary>
    public void ToDoChangeGuideSkillTargetAction()
    {
        m_ChangeGuideSkillTargetAction?.Invoke();
    }

}

