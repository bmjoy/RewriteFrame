/*===============================
 * Author: [Allen]
 * Purpose: CrossSightInfo
 * Time: 2019/11/26 18:07:09
================================*/
using System;
using UnityEngine;


public class CrossSightInfo
{
    /// <summary>
    /// 射线最远投射长度
    /// </summary>
    public float m_MaxRayDistance = 0;

    /// <summary>
    /// 准星形状
    /// </summary>
    public CrossSightShape m_CrossSightShape = CrossSightShape.CrossLine;

    /// <summary>
    /// 假如是十字线，十字线参数
    /// </summary>
    public CrossSightShape_LineInfo m_LineInfo= new CrossSightShape_LineInfo();

    /// <summary>
    /// 假如是圆形，圆形参数
    /// </summary>
    public CrossSightShape_RoundInfo m_RoundInfo = new CrossSightShape_RoundInfo();

    /// <summary>
    /// 假如是方形，方形参数
    /// </summary>
    public CrossSightShape_Square m_SquareInfo = new CrossSightShape_Square();

    /// <summary>
    /// 假如是圆锥形，圆锥参数
    /// </summary>
    public CrossSightShape_Cone m_ConeInfo = new CrossSightShape_Cone();

    /// <summary>
    /// 假如是方锥形，方锥参
    /// </summary>
    public CrossSightShape_ConeSquare m_ConeSquareInfo = new CrossSightShape_ConeSquare();
}

