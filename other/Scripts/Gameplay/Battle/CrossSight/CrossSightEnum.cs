



using System;
using UnityEngine.Serialization;
/// <summary>
/// 准星形状
/// </summary>
public enum CrossSightShape
{
    /// <summary>
    /// 十字直线
    /// </summary>
    CrossLine,

    /// <summary>
    /// 圆的
    /// </summary>
    Round,

    /// <summary>
    /// 方形
    /// </summary>
    Square,

    /// <summary>
    /// 圆形椎体
    /// </summary>
    Cone,

    /// <summary>
    /// 方形椎体
    /// </summary>
    ConeSquare,
}


/// <summary>
/// 检测方式
/// </summary>
public enum DetectionMode
{
    /// <summary>
    /// 射线路径第一个
    /// </summary>
    One,

    /// <summary>
    /// 射线路径第一个
    /// </summary>
    All,
}

/// <summary>
///十字线参数
/// </summary>
public class CrossSightShape_LineInfo
{
    /// <summary>
    ///检测方式
    /// </summary>
    public DetectionMode detectionMode = DetectionMode.One;
}


/// <summary>
/// 圆形参数
/// </summary>
[System.Serializable]
public class CrossSightShape_RoundInfo
{
    /// <summary>
    ///检测方式
    /// </summary>
    public DetectionMode detectionMode = DetectionMode.One;

    /// <summary>
    ///半径
    /// </summary>
    public float radius = 0;
}


/// <summary>
///方形参数
/// </summary>
public class CrossSightShape_Square
{
    /// <summary>
    ///检测方式
    /// </summary>
    public DetectionMode detectionMode = DetectionMode.One;

    /// <summary>
    ///盒子形状 x
    /// </summary>
    public float boxX= 0;

    /// <summary>
    ///盒子形状 y
    /// </summary>
    public float boxY = 0;

    /// <summary>
    ///盒子形状 z
    /// </summary>
    public float boxZ = 0;
}


/// <summary>
///圆锥参数
/// </summary>
public class CrossSightShape_Cone
{
    /// <summary>
    ///检测方式
    /// </summary>
    public DetectionMode detectionMode = DetectionMode.One;

    /// <summary>
    ///圆锥检测角度
    /// </summary>
    public float angle = 0;
}

/// <summary>
///方锥参数
/// </summary>
public class CrossSightShape_ConeSquare
{
    /// <summary>
    ///检测方式
    /// </summary>
    public DetectionMode detectionMode = DetectionMode.One;

    /// <summary>
    ///视野度数
    /// </summary>
    public float fieldOfView = 60;

    /// <summary>
    ///长宽比
    /// </summary>
    public float aspect = 1;
}