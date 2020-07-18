using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 基础行为控制器，建议移动，动画，特效均由其执行
// 基础行为原子操作，本质是对属性和动画的控制更改
// 不应包含任何逻辑操作，且其行为应该是可逆可控的
public class BaseActionExecuter {
    private List<BaseAction> m_actionlist = new List<BaseAction>();

    public void AddAction(BaseAction action)
    {
        m_actionlist.Add(action);
    }

    public void Execute()
    {
        foreach (var action in m_actionlist)
        {
            action.Execute();
        }
        m_actionlist.Clear();
    }
}
