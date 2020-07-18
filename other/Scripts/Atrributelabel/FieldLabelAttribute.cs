/*===============================
 * Author: [Allen]
 * Purpose: FieldLabelAttribute
 * Time: 2019/10/29 17:37:23
================================*/
using System;
using UnityEngine;

/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class FieldLabelAttribute : PropertyAttribute
{
    public string label;//要显示的字符
    public FieldLabelAttribute(string label)
    {
        this.label = label;
        //获取你想要绘制的字段（比如"技能"）
    }
}
