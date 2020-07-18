using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 计算方程式的值或者解
/// </summary>
public class EquationUtility : MonoBehaviour
{
	public enum EquationType
	{
		/// <summary>
		/// 一元三次
		/// coef3 * x^3 + coef2 * x^2 + coef1 * x + coef0
		/// </summary>
		Cubical = 0,
		/// <summary>
		/// 一元二次
		/// coef2 * x^2 + coef1 * x + coef0
		/// </summary>
		Quadratic = 1,
		/// <summary>
		/// 线性
		/// coef1 * x + coef0
		/// </summary>
		Linear = 2,
		/// <summary>
		/// 幂函数
		/// coef1 * (x ^ coef0)
		/// </summary>
		Power = 3,
	}

	/// <summary>
	/// 计算方程式的值
	/// 
	/// 这个函数是希望外部调用的接口
	/// 因为现在最多支持到一元三次函数, 所以只需要4个参数
	/// 不同的函数类型, 参数有不同的解释
	/// <see cref="EquationType"/>
	/// </summary>
	/// <param name="type"></param>
	/// <param name="x"></param>
	/// <param name="param3"></param>
	/// <param name="param2"></param>
	/// <param name="param1"></param>
	/// <param name="param0"></param>
	/// <returns></returns>
	public static float CalculateEquation(EquationType type, float x, float param3, float param2, float param1, float param0)
	{
		switch (type)
		{
			case EquationType.Cubical:
				return CubicalEquation(x, param3, param2, param1, param0);
			case EquationType.Quadratic:
				return QuadraticEquation(x, param2, param1, param0);
			case EquationType.Linear:
				return LinearEquation(x, param1, param0);
			case EquationType.Power:
				return PowerEquation(x, param1, param0);
			default:
				return 0;
		}
	}

	/// <summary>
	/// 计算方程式的解
	/// 
	/// 这个函数是希望外部调用的接口
	/// 因为现在最多支持到一元三次函数, 所以只需要4个参数
	/// 不同的函数类型, 参数有不同的解释
	/// <see cref="EquationType"/>
	/// </summary>
	/// <param name="type"></param>
	/// <param name="x"></param>
	/// <param name="param3"></param>
	/// <param name="param2"></param>
	/// <param name="param1"></param>
	/// <param name="param0"></param>
	/// <returns></returns>
	public static float CalculateInverseEquation(EquationType type, float x, float param3, float param2, float param1, float param0)
	{
		switch (type)
		{
			case EquationType.Cubical:
				return RootOfCubicalEquation(x, param3, param2, param1, param0);
			case EquationType.Quadratic:
				return RootOfQuadraticEquation(x, param2, param1, param0);
			case EquationType.Linear:
				return RootOfLinearEquation(x, param1, param0);
			case EquationType.Power:
				return RootOfPowerEquation(x, param1, param0);
			default:
				return 0;
		}
	}

	/// <summary>
	/// y = linearCoef * x + constCoef
	/// </summary>
	/// <param name="x"></param>
	/// <param name="linearCoef"></param>
	/// <returns></returns>
	public static float LinearEquation(float x, float linearCoef, float constCoef)
	{
		return x * linearCoef + constCoef;
	}

	/// <summary>
	/// x = (y - constCoef) / linearCoef
	/// </summary>
	/// <param name="y"></param>
	/// <param name="linearCoef"></param>
	/// <returns></returns>
	public static float RootOfLinearEquation(float y, float linearCoef, float constCoef)
	{
		if (linearCoef == 0)
		{
			Debug.LogError("除0错误!! 大概率是表里的公式类型或者参数配错了");
			return 0;
		}
		return (y - constCoef) / linearCoef;
	}

	/// <summary>
	/// y = ax^2 + bx + c
	/// 求y
	/// </summary>
	/// <param name="x"></param>
	/// <param name="a"></param>
	/// <param name="b"></param>
	/// <returns></returns>
	public static float QuadraticEquation(float x, float a, float b, float c)
	{
		return a * x * x + b * x + c;
	}

	/// <summary>
	/// y = ax^2 + bx + c
	/// 求x
	/// </summary>
	/// <param name="y"></param>
	/// <param name="a"></param>
	/// <param name="b"></param>
	/// <param name="c"></param>
	/// <returns></returns>
	public static float RootOfQuadraticEquation(float y, float a, float b, float c)
	{
		c = c - y;
		
		// x * (ax + b) = 0
		if (Mathf.Abs(c) < 1e-15)
		{
			return -b / a;
		}
		else
		{
			b /= a;
			c /= a;
			float delta = b * b - 4 * c;
			if (delta > 0)
			{
				float temp = Mathf.Sqrt(delta);
				float x1 = (-b + temp) / 2;
				float x2 = (-b - temp) / 2;
				return x1 > 0 ? x1 : x2;
			}
			else if (delta == 0)
			{
				return -b / 2;
			}
			else
			{
				return 0;
			}
		}
	}

	/// <summary>
	/// ax^3 + bx^2 + cx + d
	/// </summary>
	/// <param name="x"></param>
	/// <param name="a"></param>
	/// <param name="b"></param>
	/// <param name="c"></param>
	/// <param name="d"></param>
	/// <returns></returns>
	public static float CubicalEquation(float x, float a, float b, float c, float d)
	{
		return a * x * x * x + b * x * x + c * x + d;
	}

	/// <summary>
	/// 返回ax^3 + bx^2 + cx + d = y 的一个实数根
	/// </summary>
	/// <param name="y"></param>
	/// <param name="a"></param>
	/// <param name="b"></param>
	/// <param name="c"></param>
	/// <param name="d"></param>
	/// <returns></returns>
	public static float RootOfCubicalEquation(float y, float a, float b, float c, float d)
	{
		d = d - y;

		float p, q, r, x, theta, delta;
		float temp = a;
		a = b / temp;
		b = c / temp;
		c = d / temp;
		p = b - a * a / 3;
		q = c + a * (2 * a * a - 9 * b) / 27;
		p /= -3;
		q /= -2;
		delta = q * q - p * p * p;

		if (delta < 0)
		{
			r = p * Mathf.Sqrt(p);
			theta = Mathf.Acos(q / r) / 3;
			x = 2 * Mathf.Pow(r, 1.0f / 3.0f) * Mathf.Cos(theta) - a / 3;
		}
		else if (delta == 0)
		{
			x = -a / 3 + 2 * Mathf.Pow(q, 1.0f / 3.0f);
		}
		else
		{
			temp = Mathf.Sqrt(delta);
			//卡尔达诺公式
			x = -a / 3 + Mathf.Pow(q + temp, 1.0f / 3.0f) + Mathf.Pow(q - temp, 1.0f / 3.0f);
		}

		return x;
	}

	/// <summary>
	/// y = a * x^b
	/// 求y
	/// </summary>
	/// <param name="x"></param>
	/// <param name="coef"></param>
	/// <param name="power"></param>
	/// <returns></returns>
	public static float PowerEquation(float x, float a, float b)
	{
		return a * Mathf.Pow(x, b);
	}

	/// <summary>
	/// y = a * x^b
	/// 求x
	/// </summary>
	/// <param name="y"></param>
	/// <param name="a"></param>
	/// <param name="b"></param>
	/// <returns></returns>
	public static float RootOfPowerEquation(float y, float a, float b)
	{
		if (a == 0 || b == 0)
			return 0;

		return Mathf.Pow(y / a, 1 / b);
	}
}
