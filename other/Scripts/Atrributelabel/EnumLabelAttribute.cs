/*===============================
 * Author: [Allen]
 * Purpose: 枚举标签
 * Time: 2019/10/29 17:50:29
================================*/
using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Enum | AttributeTargets.Field)]
public class EnumLabelAttribute : PropertyAttribute
{
    public string label;
    public int[] order = new int[0];
    public EnumLabelAttribute(string label)
    {
        this.label = label;
    }

    public EnumLabelAttribute(string label, params int[] order)
    {
        this.label = label;
        this.order = order;
    }
}

