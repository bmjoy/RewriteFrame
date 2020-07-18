using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections;

public static class MaterialPropertyUtility
{
	public static Dictionary<string, MethodInfo> methods = new Dictionary<string, MethodInfo>();

	static MaterialPropertyUtility()
	{
		var asm = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetTypes().Any(t => t.Name == "ShaderUtil"));
		if (asm != null)
		{
			var tp = asm.GetTypes().FirstOrDefault(t => t.Name == "ShaderUtil");
			foreach (var method in tp.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
			{
				methods[method.Name] = method;
			}
		}
	}

	public static List<Texture> GetTextures(this Material shader)
	{
		var list = new List<Texture>();
		var count = shader.GetPropertyCount();
		for (var i = 0; i < count; i++)
		{
			if (shader.GetPropertyType(i) == 4)
			{
				list.Add((Texture)shader.GetDefaultProperty(i));
			}
		}
		return list;
	}

	public static int GetPropertyCount(this Material shader)
	{
		return Call<int>("GetPropertyCount", shader.shader);
	}

	public static int GetPropertyType(this Material shader, int index)
	{
		return Call<int>("GetPropertyType", shader.shader, index);
	}

	public static string GetPropertyName(this Material shader, int index)
	{
		return Call<string>("GetPropertyName", shader.shader, index);
	}

	public static void SetDefaultProperty(this Material material, int index, object value)
	{
		var name = material.GetPropertyName(index);
		var type = material.GetPropertyType(index);
		switch (type)
		{
			case 0:
				material.SetColor(name, (Color)value);
				break;
			case 1:
				material.SetVector(name, (Vector4)value);
				break;
			case 2:
				material.SetFloat(name, (float)value);
				break;
			case 3:
				material.SetFloat(name, (float)value);
				break;
			case 4:
				material.SetTexture(name, (Texture)value);
				break;
		}
	}


	public static void SetRuntimeProperty(this Renderer render, int materialIndex, int propertyIndex, object value)
	{
		MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
		render.GetPropertyBlock(propertyBlock);

		Material material = render.materials[materialIndex];
		string name = material.GetPropertyName(propertyIndex);
		int type = material.GetPropertyType(propertyIndex);

		switch (type)
		{
			case 0:
				propertyBlock.SetColor(name, (Color)value);
				break;
			case 1:
				propertyBlock.SetVector(name, (Vector4)value);
				break;
			case 2:
				propertyBlock.SetFloat(name, (float)value);
				break;
			case 3:
				propertyBlock.SetFloat(name, (float)value);
				break;
			case 4:
				// TODO: 设置texture会报错, 原因未知. 没空查了, 记录一下
				if (value != null)
					propertyBlock.SetTexture(name, (Texture)value);
				break;
		}

		render.SetPropertyBlock(propertyBlock);
	}

	public static object GetRuntimeProperty(this Renderer render, int materialIndex, int propertyIndex)
	{
		MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
		render.GetPropertyBlock(propertyBlock);

		Material material = render.materials[materialIndex];
		string name = material.GetPropertyName(propertyIndex);
		int type = material.GetPropertyType(propertyIndex);
		switch (type)
		{
			case 0:
				return propertyBlock.GetColor(name);

			case 1:
				return propertyBlock.GetVector(name);


			case 2:
			case 3:
				return propertyBlock.GetFloat(name);

			case 4:
				return propertyBlock.GetTexture(name);

		}
		return null;
	}

	public static object GetDefaultProperty(this Material material, int index)
	{
		var name = material.GetPropertyName(index);
		var type = material.GetPropertyType(index);
		switch (type)
		{
			case 0:
				return material.GetColor(name);

			case 1:
				return material.GetVector(name);


			case 2:
			case 3:
				return material.GetFloat(name);

			case 4:
				return material.GetTexture(name);

		}
		return null;
	}

	public static T Call<T>(string name, params object[] parameters)
	{
		return (T)methods[name].Invoke(null, parameters);
	}

}