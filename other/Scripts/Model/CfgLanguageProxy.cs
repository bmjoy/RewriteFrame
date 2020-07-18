using FlatBuffers;
using PureMVC.Patterns.Proxy;
using UnityEngine;

public class CfgLanguageProxy : Proxy
{
	private const string ASSET_ADDRESS = "Assets/Config/data/language.bytes";

	public CfgLanguageProxy() : base(ProxyName.CfgLanguageProxy)
	{

	}

	public override void InitData(ByteBuffer buffer)
	{

	}

	/// <summary>
	/// 获取多语言表内容
	/// </summary>
	/// <param name="id">language 表内 index</param>
	/// <param name="language">语言类型</param>
	/// <returns></returns>
	public string GetLocalization(int id, SystemLanguage language = SystemLanguage.English)
	{
		//Debug.LogError("fuck " + id);
		return $"通过 int 获取多语言文字的方法已经过时啦~~~  {id}";
	}
}