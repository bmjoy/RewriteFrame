///*===============================
// * Author: [dinghuilin]
// * Purpose: StoreHelper.cs
// * Time: 2019/03/26  11:07
//================================*/
//using FlatBuffers;
//using PureMVC.Patterns.Proxy;
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.IO;
//using UnityEngine;
//using UnityEngine.Rendering.PostProcessing;

///// <summary>
///// 设置存储帮助
///// </summary>
//public class StoreHelperProxy:Proxy
//{
//	/// <summary>
//	/// 存储路径
//	/// </summary>
//    private string m_FilePath;
//	/// <summary>
//	/// 存储字典
//	/// </summary>
//    private Dictionary<string, int> m_Items;
//	/// <summary>
//	/// 分辨率数组
//	/// </summary>
//	private Resolution[] m_Resolutions;
//	public Resolution[] GetResolutions()
//	{
//		return m_Resolutions;
//	}
//	/// <summary>
//	/// 显示模式
//	/// </summary>
//	private int m_DisplayMode;
//	public int GetDisplayMode()
//	{
//		return m_DisplayMode;
//	}
//	/// <summary>
//	/// 屏幕分辨率
//	/// </summary>
//	private int m_ScreenResolution;
//	public int GetScreenResolution()
//	{
//		return m_ScreenResolution;
//	}
//	/// <summary>
//	/// 视频质量
//	/// </summary>
//	private int m_VideoQuality;
//	public int GetVideoQuality()
//	{
//		return m_VideoQuality;
//	}
//	public void SetVideoQuality(int value)
//	{
//		m_VideoQuality = value;
//	}

//	/// <summary>
//	/// 屏幕充满类型
//	/// </summary>
//	private FullScreenMode m_Mode;
//	/// <summary>
//	/// 宇宙飞船的视线
//	/// </summary>
//	private int m_TheSpacecraftLineOfSight;
//	/// <summary>
//	/// 人的视距
//	/// </summary>
//	private int m_PeopleStadia;
//	/// <summary>
//	/// 贴图质量
//	/// </summary>
//	private int m_TextureQuality;
//	/// <summary>
//	/// 灯光质量
//	/// </summary>
//	private int m_LightQuality;
//	/// <summary>
//	/// 各向异性纹理
//	/// </summary>
//	private int m_AnisotropicTextures;
//	/// <summary>
//	/// 反锯齿
//	/// </summary>
//	private int m_AntiAliasing;
//	/// <summary>
//	/// 实时反射
//	/// </summary>
//	private int m_RealtimeReflection;
//	/// <summary>
//	/// 阴影质量
//	/// </summary>
//	private int m_ShadowsQuality;
//	/// <summary>
//	/// 同步个数
//	/// </summary>
//	private int m_SyncCount;
//	/// <summary>
//	/// HDR高动态声音渲染
//	/// </summary>
//	private int m_HDR;
//	/// <summary>
//	/// 屏幕特效
//	/// </summary>
//	private int m_ScreenEffect;
//	/// <summary>
//	/// 控制音量
//	/// </summary>
//	private int m_MasterVolume;
//	/// <summary>
//	/// 特效音量
//	/// </summary>
//	private int m_SoundEffectsVolume;
//	/// <summary>
//	/// settingProxy
//	/// </summary>
//	private CfgSettingProxy m_SettingProxy;
//	private CfgSettingProxy GetSettingProxy()
//	{
//		if (m_SettingProxy == null)
//		{
//			m_SettingProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgSettingProxy) as CfgSettingProxy; ;
//		}
//		return m_SettingProxy;
//	}

//	public StoreHelperProxy() : base(ProxyName.StoreHelperProxy)
//	{
//		InitStoreHelper();
//		ReloadData(SetType.Audio);
//		ReloadData(SetType.Video);
//	}
//	public override void InitData(ByteBuffer buffer)
//	{
		
//	}
//	/// <summary>
//	/// 初始化Store
//	/// </summary>
//	public void InitStoreHelper()
//	{
		
//		string fileName;
//#if UNITY_EDITOR
//		fileName = "setting_editor";
//#else
//        fileName = "setting";
//#endif
//		Initialize(string.Format("{0}\\{1}.ini", Application.persistentDataPath, fileName)); // UNDONE Destory storeHelper

//		m_Resolutions = Screen.resolutions;
//		Array.Sort(m_Resolutions, SortResolutions);
//	}
//	public int GetInt(string key, int defaultValue = 0)
//    {
//        int value;
//        if (!m_Items.TryGetValue(key, out value))
//        {
//            value = defaultValue;
//        }
//        return value;
//    }
//	/// <summary>
//	/// 存储值到字典
//	/// </summary>
//	/// <param name="key"></param>
//	/// <param name="value"></param>
//    public void SetInt(string key, int value)
//    {
//        if (m_Items.ContainsKey(key))
//        {
//            m_Items[key] = value;
//        }
//        else
//        {
//            m_Items.Add(key, value);
//        }
//    }
//	/// <summary>
//	/// 删除
//	/// </summary>
//	/// <param name="key"></param>
//    public void DeleteKey(string key)
//    {
//        m_Items.Remove(key);
//    }
//	/// <summary>
//	/// 保存路径
//	/// </summary>
//    public void SaveToFile()
//    {
//        try
//        {
//            string[] lines = new string[m_Items.Count];
//            int iLine = 0;
//            foreach (KeyValuePair<string, int> kv in m_Items)
//            {
//                lines[iLine++] = string.Format("{0}={1}", kv.Key, kv.Value);
//            }
//            File.WriteAllLines(m_FilePath, lines, System.Text.Encoding.UTF8);
//        }
//        catch (System.Exception e)
//        {
//            Debug.LogError("Destory StoreHelper Failed\n" + e.ToString());
//            // UNDONE Assert
//            throw e;
//        }
//    }
//	/// <summary>
//	/// 初始化
//	/// </summary>
//	public void Init()
//	{
//		ApplySetting(SetType.Audio, false);
//		ApplySetting(SetType.Video, false);
//	}
//	/// <summary>
//	/// 读取本地设置
//	/// </summary>
//	/// <param name="filePath"></param>
//	public void Initialize(string filePath)
//    {
//        this.m_FilePath = filePath;
//        Debug.Log("StoreHelper file: " + filePath);

//        m_Items = new Dictionary<string, int>();
//        try
//        {
//            if (File.Exists(filePath))
//            {
//                string[] lines = File.ReadAllLines(filePath, System.Text.Encoding.UTF8);
//                for (int iLine = 0; iLine < lines.Length; iLine++)
//                {
//                    string line = lines[iLine];
//                    int valueIndexOf = line.IndexOf("=");
//                    if (valueIndexOf <= 0)
//                    {
//                        continue;
//                    }

//                    string key = line.Substring(0, valueIndexOf);
//                    string sValue = line.Substring(valueIndexOf + 1);
//                    int iValue;
//                    if (int.TryParse(sValue, out iValue)
//                        && !m_Items.ContainsKey(key))
//                    {
//                        m_Items.Add(key, iValue);
//                    }
//                }
//            }
//        }
//        catch (System.Exception e)
//        {
//            Debug.LogError("Initialize StoreHelper Failed\n" + e.ToString());
//            // UNDONE Assert
//            throw e;
//        }
//    }
//	/// <summary>
//	/// 销毁
//	/// </summary>
//    public void Destory()
//    {
//        m_Items.Clear();
//    }

//	/// <summary>
//	///重新加载数据 
//	/// </summary>
//	/// <param name="setType">类型</param>
//	public void ReloadData(SetType setType)
//	{
//		switch (setType)
//		{
//			case SetType.Audio:
//				m_MasterVolume = ReloadIntValue(VideoSettingType.MasterVolume);
//				m_SoundEffectsVolume = ReloadIntValue(VideoSettingType.SoundEffectsVolume);
//				break;
//			case SetType.Video:
//				m_DisplayMode = ReloadIntValue(VideoSettingType.DisplayMode);
//				if (m_DisplayMode == 2) // HACK 以前displayMode有三个，现在只有两个
//				{
//					m_DisplayMode = 0;
//				}

//				int defaultResolutionIdx = 0;
//				int lastMinDiffResolution = int.MaxValue;
//				for (int iResolution = 0; iResolution < m_Resolutions.Length; iResolution++)
//				{
//					int diff = (int)Mathf.Abs(m_Resolutions[iResolution].width - 1920); // 寻找宽度最接近1920的分辨率
//					if (diff <= lastMinDiffResolution)
//					{
//						defaultResolutionIdx = iResolution;
//						lastMinDiffResolution = diff;
//					}
//				}
//				m_ScreenResolution = GetInt(VideoSettingType.ScreenResolution.ToString(), defaultResolutionIdx);
//				// Unity每次获得的分辨率列表不一定相同（例如换显示器），所以这里做个保护 [2/11/2019 huangwm]
//				if (m_ScreenResolution >= m_Resolutions.Length)
//				{
//					m_ScreenResolution = defaultResolutionIdx;
//				}
//				SetInt(VideoSettingType.ScreenResolution.ToString(), m_ScreenResolution);
//				m_VideoQuality = ReloadIntValue(VideoSettingType.VideoQuality);
//				m_TextureQuality = ReloadIntValue(VideoSettingType.TextureQuality, m_VideoQuality);
//				m_LightQuality = ReloadIntValue(VideoSettingType.LightQuality, m_VideoQuality);
//				m_AnisotropicTextures = ReloadIntValue(VideoSettingType.AnisotropicTextures, m_VideoQuality);
//				m_AntiAliasing = ReloadIntValue(VideoSettingType.AntiAliasing);
//				m_RealtimeReflection = ReloadIntValue(VideoSettingType.RealtimeReflection, m_VideoQuality);
//				m_ShadowsQuality = GetInt(VideoSettingType.ShadowsQuality.ToString()
//					, GetSettingProxy().GetSetVOByKey((int)VideoSettingType.ShadowsQuality)
//					.Defaults(GetSettingProxy().GetSetVOByKey((int)VideoSettingType.VideoQuality)
//					.Defaults(0)));
//				SetInt(VideoSettingType.ShadowsQuality.ToString(), m_ShadowsQuality);
//				m_SyncCount = ReloadIntValue(VideoSettingType.VSyncCount, m_VideoQuality);
//				m_HDR = ReloadIntValue(VideoSettingType.HDR);
//				m_ScreenEffect = ReloadIntValue(VideoSettingType.ScreenEffect);
//				m_PeopleStadia = ReloadIntValue(VideoSettingType.PeopleStadia);
//				m_TheSpacecraftLineOfSight = ReloadIntValue(VideoSettingType.TheSpacecraftLineOfSight);
//				break;
//		}
//	}
//	/// <summary>
//	/// 分辨率排序
//	/// </summary>
//	/// <param name="x">分辨率x</param>
//	/// <param name="y">分辨率y</param>
//	/// <returns></returns>
//	private int SortResolutions(Resolution x, Resolution y)
//	{
//		return x.width == y.width
//			? x.height == y.height
//				? x.refreshRate - y.refreshRate
//				: x.height - y.height
//			: x.width - y.width;
//	}
//	/// <summary>
//	/// 恢复默认设置
//	/// </summary>
//	/// <param name="settingType">设置类型</param>
//	/// <param name="defaultValue">默认值</param>
//	/// <returns></returns>
//	private int ReloadIntValue(VideoSettingType settingType, int defaultValue = 0)
//	{
//		int value = GetInt(settingType.ToString(), GetSettingProxy().GetSetVOByKey((int)settingType).Defaults(defaultValue));
//		SetInt(settingType.ToString(), value);
//		return value;
//	}
//	/// <summary>
//	/// 恢复默认设置
//	/// </summary>
//	/// <param name="setType">类型</param>
//	public void RestoreToDefault(SetType setType)
//	{
//		switch (setType)
//		{
//			case SetType.Audio:
//				DeleteKey(VideoSettingType.MasterVolume.ToString());
//				DeleteKey(VideoSettingType.SoundEffectsVolume.ToString());
//				break;
//			case SetType.Video:
//				DeleteKey(VideoSettingType.DisplayMode.ToString());
//				DeleteKey(VideoSettingType.ScreenResolution.ToString());
//				DeleteKey(VideoSettingType.VideoQuality.ToString());
//				DeleteKey(VideoSettingType.TextureQuality.ToString());
//				DeleteKey(VideoSettingType.LightQuality.ToString());
//				DeleteKey(VideoSettingType.AnisotropicTextures.ToString());
//				DeleteKey(VideoSettingType.AntiAliasing.ToString());
//				DeleteKey(VideoSettingType.RealtimeReflection.ToString());
//				DeleteKey(VideoSettingType.ShadowsQuality.ToString());
//				DeleteKey(VideoSettingType.VSyncCount.ToString());
//				DeleteKey(VideoSettingType.HDR.ToString());
//				DeleteKey(VideoSettingType.ScreenEffect.ToString());
//				DeleteKey(VideoSettingType.PeopleStadia.ToString());
//				DeleteKey(VideoSettingType.TheSpacecraftLineOfSight.ToString());
//				break;
//		}

//		ReloadData(setType);
//		ApplySetting(setType);
//	}
//	/// <summary>
//	/// 应用设置
//	/// </summary>
//	/// <param name="setType">类型</param>
//	/// <param name="udpateCamera">是否更新相机</param>
//	public void ApplySetting(SetType setType, bool udpateCamera = true)
//	{
//		switch (setType)
//		{
//			case SetType.Audio:
//				SetInt(VideoSettingType.MasterVolume.ToString(), m_MasterVolume);
//				//to do
//				//AudioManager.Instance.BgmVolume = m_MasterVolume * 0.01f;

//				SetInt(VideoSettingType.SoundEffectsVolume.ToString(), m_SoundEffectsVolume);
//				//to do
//				//AudioManager.Instance.SfxVolume = m_SoundEffectsVolume * 0.01f;
//				break;
//			case SetType.Video:
//				SetInt(VideoSettingType.DisplayMode.ToString(), m_DisplayMode);
//				switch (m_DisplayMode)
//				{
//					case 0:
//						m_Mode = FullScreenMode.FullScreenWindow;
//						break;
//					case 1:
//						m_Mode = FullScreenMode.Windowed;
//						break;
//					default:
//						m_Mode = FullScreenMode.Windowed;
//						break;
//				}

//				SetInt(VideoSettingType.ScreenResolution.ToString(), m_ScreenResolution);
//				//to do
//				//GameApplication.Instance.StartCoroutine(SetResolution_Co(m_Resolutions[m_ScreenResolution].width
//				//	, m_Resolutions[m_ScreenResolution].height
//				//	, m_Resolutions[m_ScreenResolution].refreshRate
//				//	, m_Mode));

//				SetInt(VideoSettingType.VideoQuality.ToString(), m_VideoQuality);
//				QualitySettings.SetQualityLevel(m_VideoQuality);

//				//纹理品质赋值越小，品质越高
//				int length = GetSettingProxy().GetSetVOByKey((int)VideoSettingType.TextureQuality).OptionLength - 1;
//				SetInt(VideoSettingType.TextureQuality.ToString(), m_TextureQuality);
//				QualitySettings.masterTextureLimit = Mathf.Abs(m_TextureQuality - length);

//				SetInt(VideoSettingType.LightQuality.ToString(), m_LightQuality);
//				QualitySettings.pixelLightCount = m_LightQuality;

//				SetInt(VideoSettingType.AnisotropicTextures.ToString(), m_AnisotropicTextures);
//				QualitySettings.anisotropicFiltering = (AnisotropicFiltering)m_AnisotropicTextures;

//				SetInt(VideoSettingType.AntiAliasing.ToString(), m_AntiAliasing);
//				OnChangeAntiAllasing();

//				SetInt(VideoSettingType.ShadowsQuality.ToString(), m_ShadowsQuality);
//				OnChangeShadowQuality();

//				SetInt(VideoSettingType.HDR.ToString(), m_HDR);
//				SetInt(VideoSettingType.RealtimeReflection.ToString(), m_RealtimeReflection);
//				QualitySettings.realtimeReflectionProbes = m_RealtimeReflection == 0;

//				SetInt(VideoSettingType.VSyncCount.ToString(), m_SyncCount);
//				QualitySettings.vSyncCount = m_SyncCount == 0 ? 1 : 0;

//				SetInt(VideoSettingType.ScreenEffect.ToString(), m_ScreenEffect);
//				SetInt(VideoSettingType.TheSpacecraftLineOfSight.ToString(), m_TheSpacecraftLineOfSight);

//				SetInt(VideoSettingType.PeopleStadia.ToString(), m_PeopleStadia);

//				if (udpateCamera)
//				{
//					//to do
//					//SceneProxy sceneProxy = Facade.RetrieveProxy(ProxyName.SceneProxy) as SceneProxy;
//					//InitSceneCamera(Camera.main, sceneProxy.mainHero.motionType == MotionType.Spacecraft);
//				}
//				break;
//		}

//		SaveToFile();
//	}
//	/// <summary>
//	/// 获取设置
//	/// </summary>
//	/// <param name="type">类型</param>
//	/// <returns></returns>
//	public int GetValue(int type)
//	{
//		switch (type)
//		{
//			case (int)VideoSettingType.DisplayMode:
//				return m_DisplayMode;
//			case (int)VideoSettingType.ScreenResolution:
//				return m_ScreenResolution;
//			case (int)VideoSettingType.VideoQuality:
//				return m_VideoQuality;
//			case (int)VideoSettingType.TextureQuality:
//				return m_TextureQuality;
//			case (int)VideoSettingType.LightQuality:
//				return m_LightQuality;
//			case (int)VideoSettingType.AnisotropicTextures:
//				return m_AnisotropicTextures;
//			case (int)VideoSettingType.AntiAliasing:
//				return m_AntiAliasing;
//			case (int)VideoSettingType.ShadowsQuality:
//				return m_ShadowsQuality;
//			case (int)VideoSettingType.HDR:
//				return m_HDR;
//			case (int)VideoSettingType.RealtimeReflection:
//				return m_RealtimeReflection;
//			case (int)VideoSettingType.VSyncCount:
//				return m_SyncCount;
//			case (int)VideoSettingType.ScreenEffect:
//				return m_ScreenEffect;
//			case (int)VideoSettingType.TheSpacecraftLineOfSight:
//				return m_TheSpacecraftLineOfSight;
//			case (int)VideoSettingType.PeopleStadia:
//				return m_PeopleStadia;
//			case (int)VideoSettingType.MasterVolume:
//				return m_MasterVolume;
//			case (int)VideoSettingType.SoundEffectsVolume:
//				return m_SoundEffectsVolume;
//		}
//		return 0;
//	}
//	/// <summary>
//	/// 设置
//	/// </summary>
//	/// <param name="type">类型</param>
//	/// <param name="value">参数</param>
//	public void SetValue(int type, int value)
//	{
//		switch (type)
//		{
//			case (int)VideoSettingType.DisplayMode:
//				m_DisplayMode = value;
//				break;
//			case (int)VideoSettingType.ScreenResolution:
//				m_ScreenResolution = value;
//				break;
//			case (int)VideoSettingType.VideoQuality:
//				m_VideoQuality = value;
//				OnChangeVideoQuality();
//				break;
//			case (int)VideoSettingType.TextureQuality:
//				m_TextureQuality = value;
//				break;
//			case (int)VideoSettingType.AnisotropicTextures:
//				m_AnisotropicTextures = value;
//				break;
//			case (int)VideoSettingType.AntiAliasing:
//				m_AntiAliasing = value;
//				break;
//			case (int)VideoSettingType.ShadowsQuality:
//				m_ShadowsQuality = value;
//				break;
//			case (int)VideoSettingType.LightQuality:
//				m_LightQuality = value;
//				break;
//			case (int)VideoSettingType.HDR:
//				m_HDR = value;
//				break;
//			case (int)VideoSettingType.RealtimeReflection:
//				m_RealtimeReflection = value;
//				m_VideoQuality = 6;
//				break;
//			case (int)VideoSettingType.VSyncCount:
//				m_SyncCount = value;
//				m_VideoQuality = 6;
//				break;
//			case (int)VideoSettingType.ScreenEffect:
//				m_ScreenEffect = value;
//				break;
//			case (int)VideoSettingType.TheSpacecraftLineOfSight:
//				m_TheSpacecraftLineOfSight = value;
//				break;
//			case (int)VideoSettingType.PeopleStadia:
//				m_PeopleStadia = value;
//				break;
//			case (int)VideoSettingType.MasterVolume:
//				m_MasterVolume = value;
//				break;
//			case (int)VideoSettingType.SoundEffectsVolume:
//				m_SoundEffectsVolume = value;
//				break;
//			default:
//				break;
//		}
//	}
//	/// <summary>
//	/// 初始化相机
//	/// </summary>
//	/// <param name="camera">相机</param>
//	/// <param name="isSpacecraft">是否是飞船形态</param>
//	public void InitSceneCamera(Camera camera, bool isSpacecraft)
//	{
//		//if (camera == null)
//		//{
//		//	return;
//		//}

//		//CfgGlobalPameraProxy globalProxy = Facade.RetrieveProxy(ProxyName.CfgGlobalPameraProxy) as CfgGlobalPameraProxy;
//		//string[] rangeStrings = (isSpacecraft
//		//		? globalProxy.GetStringValueByKey(Assets.Scripts.Define.KGlobalKey.SpacecraftFarClipRange)
//		//		: globalProxy.GetStringValueByKey(Assets.Scripts.Define.KGlobalKey.CharacterFarClipRange))
//		//	.Split('^');

//		//float minFarClip = float.Parse(rangeStrings[0]);
//		//float maxFarClip = float.Parse(rangeStrings[1]);
//		//camera.farClipPlane = (isSpacecraft ? m_TheSpacecraftLineOfSight : m_PeopleStadia) * 0.01f // percentage to sacle factor
//		//	* (maxFarClip - minFarClip) + minFarClip;

//		//camera.allowHDR = m_HDR == 0;
//		//camera.allowMSAA = m_AntiAliasing != 0;
//	}
//	/// <summary>
//	/// HACK分辨率
//	/// </summary>
//	public void HACK_SetResolution()
//	{
//		Debug.Log("HACK_SetResolution");
//		//to do
//		//GameApplication.Instance.StartCoroutine(SetResolution_Co(m_Resolutions[m_ScreenResolution].width
//		//			, m_Resolutions[m_ScreenResolution].height
//		//			, m_Resolutions[m_ScreenResolution].refreshRate
//		//			, m_Mode));
//	}

//	/// <summary>
//	/// 是否有修改
//	/// </summary>
//	/// <returns></returns>
//	public bool HaveChanged(SetType setType)
//	{
//		switch (setType)
//		{
//			case SetType.Audio:
//				return m_MasterVolume != GetInt(VideoSettingType.MasterVolume.ToString())
//					|| m_SoundEffectsVolume != GetInt(VideoSettingType.SoundEffectsVolume.ToString());
//			case SetType.Video:
//				return m_DisplayMode != GetInt(VideoSettingType.DisplayMode.ToString())
//					|| m_ScreenResolution != GetInt(VideoSettingType.ScreenResolution.ToString())
//					|| m_TheSpacecraftLineOfSight != GetInt(VideoSettingType.TheSpacecraftLineOfSight.ToString())
//					|| m_PeopleStadia != GetInt(VideoSettingType.PeopleStadia.ToString())
//					|| m_VideoQuality != GetInt(VideoSettingType.VideoQuality.ToString())
//					|| m_TextureQuality != GetInt(VideoSettingType.TextureQuality.ToString())
//					|| m_LightQuality != GetInt(VideoSettingType.LightQuality.ToString())
//					|| m_AnisotropicTextures != GetInt(VideoSettingType.AnisotropicTextures.ToString())
//					|| m_AntiAliasing != GetInt(VideoSettingType.AntiAliasing.ToString())
//					|| m_RealtimeReflection != GetInt(VideoSettingType.RealtimeReflection.ToString())
//					|| m_ShadowsQuality != GetInt(VideoSettingType.ShadowsQuality.ToString())
//					|| m_SyncCount != GetInt(VideoSettingType.VSyncCount.ToString())
//					|| m_HDR != GetInt(VideoSettingType.HDR.ToString())
//					|| m_ScreenEffect != GetInt(VideoSettingType.ScreenEffect.ToString());
//			default:
//				return false;
//		}
//	}
//	/// <summary>
//	///  反锯齿
//	/// </summary>
//	private void OnChangeAntiAllasing()
//	{
//		//to do
//		//PostProcessLayer ppl = GameApplication.Instance.globalPostLayer;
//		//if (ppl == null)
//		//{
//		//	return;
//		//}

//		//ppl.antialiasingMode = (PostProcessLayer.Antialiasing)m_AntiAliasing;
//	}
//	/// <summary>
//	/// 设置阴影质量
//	/// </summary>
//	private void OnChangeShadowQuality()
//	{
//		switch (m_ShadowsQuality)
//		{
//			case 0:
//				{
//					QualitySettings.shadows = ShadowQuality.Disable;
//					QualitySettings.shadowResolution = ShadowResolution.Low;
//					QualitySettings.shadowProjection = ShadowProjection.StableFit;
//					QualitySettings.shadowDistance = 15;
//					QualitySettings.shadowmaskMode = ShadowmaskMode.Shadowmask;
//					QualitySettings.shadowNearPlaneOffset = 3;
//					QualitySettings.shadowCascades = 0;
//				}
//				break;
//			case 1:
//				{
//					QualitySettings.shadows = ShadowQuality.Disable;
//					QualitySettings.shadowResolution = ShadowResolution.Low;
//					QualitySettings.shadowProjection = ShadowProjection.StableFit;
//					QualitySettings.shadowDistance = 20;
//					QualitySettings.shadowmaskMode = ShadowmaskMode.Shadowmask;
//					QualitySettings.shadowNearPlaneOffset = 3;
//					QualitySettings.shadowCascades = 0;
//				}
//				break;
//			case 2:
//				{
//					QualitySettings.shadows = ShadowQuality.HardOnly;
//					QualitySettings.shadowResolution = ShadowResolution.Low;
//					QualitySettings.shadowProjection = ShadowProjection.StableFit;
//					QualitySettings.shadowDistance = 20;
//					QualitySettings.shadowmaskMode = ShadowmaskMode.Shadowmask;
//					QualitySettings.shadowNearPlaneOffset = 3;
//					QualitySettings.shadowCascades = 0;
//				}
//				break;
//			case 3:
//				{
//					QualitySettings.shadows = ShadowQuality.All;
//					QualitySettings.shadowResolution = ShadowResolution.Medium;
//					QualitySettings.shadowProjection = ShadowProjection.StableFit;
//					QualitySettings.shadowDistance = 40;
//					QualitySettings.shadowmaskMode = ShadowmaskMode.DistanceShadowmask;
//					QualitySettings.shadowNearPlaneOffset = 3;
//					QualitySettings.shadowCascades = 2;
//				}
//				break;
//			case 4:
//				{
//					QualitySettings.shadows = ShadowQuality.All;
//					QualitySettings.shadowResolution = ShadowResolution.High;
//					QualitySettings.shadowProjection = ShadowProjection.StableFit;
//					QualitySettings.shadowDistance = 70;
//					QualitySettings.shadowmaskMode = ShadowmaskMode.DistanceShadowmask;
//					QualitySettings.shadowNearPlaneOffset = 3;
//					QualitySettings.shadowCascades = 2;
//				}
//				break;
//			case 5:
//				{
//					QualitySettings.shadows = ShadowQuality.All;
//					QualitySettings.shadowResolution = ShadowResolution.VeryHigh;
//					QualitySettings.shadowProjection = ShadowProjection.StableFit;
//					QualitySettings.shadowDistance = 150;
//					QualitySettings.shadowmaskMode = ShadowmaskMode.DistanceShadowmask;
//					QualitySettings.shadowNearPlaneOffset = 3;
//					QualitySettings.shadowCascades = 4;
//				}
//				break;
//		}
//	}
//	/// <summary>
//	/// 根据视频设置其他
//	/// </summary>
//	private void OnChangeVideoQuality()
//	{
//		if (m_VideoQuality == 6)
//		{
//			return;
//		}

//		m_TextureQuality = GetSettingProxy().GetSetVOByKey((int)VideoSettingType.TextureQuality).Defaults(m_VideoQuality);
//		m_LightQuality = GetSettingProxy().GetSetVOByKey((int)VideoSettingType.LightQuality).Defaults(m_VideoQuality);
//		m_AnisotropicTextures = GetSettingProxy().GetSetVOByKey((int)VideoSettingType.AnisotropicTextures).Defaults(m_VideoQuality);
//		m_RealtimeReflection = GetSettingProxy().GetSetVOByKey((int)VideoSettingType.RealtimeReflection).Defaults(m_VideoQuality);
//		m_ShadowsQuality = GetSettingProxy().GetSetVOByKey((int)VideoSettingType.ShadowsQuality).Defaults(m_VideoQuality);
//		m_SyncCount = GetSettingProxy().GetSetVOByKey((int)VideoSettingType.VSyncCount).Defaults(m_VideoQuality);
//	}

//	/// <summary>
//	/// 设置屏幕分辨率
//	/// </summary>
//	/// <param name="width">宽</param>
//	/// <param name="height">高</param>
//	/// <param name="refreshRate">刷新</param>
//	/// <param name="mode">屏幕模式</param>
//	/// <returns></returns>
//	private IEnumerator SetResolution_Co(int width, int height, int refreshRate, FullScreenMode mode)
//	{
//		Debug.Log("Apply Resolution:" + width + " x " + height + " " + refreshRate + "HZ " + mode.ToString());
//		yield return null;
//		Screen.fullScreenMode = mode;
//		yield return null;
//		Screen.SetResolution(width, height, mode, m_Resolutions[m_ScreenResolution].refreshRate);
//		yield return null;
//		Debug.Log("Current Resolution:"
//			+ Screen.currentResolution.width
//			+ " x "
//			+ Screen.currentResolution.height
//			+ " "
//			+ Screen.currentResolution.refreshRate
//			+ "HZ "
//			+ Screen.fullScreenMode.ToString());
//	}
//}
///// <summary>
///// 视频类型
///// </summary>
//public enum VideoSettingType
//{
//	/// <summary>
//	/// 显示模式
//	/// </summary>
//	DisplayMode = 100000,
//	/// <summary>
//	/// 分辨率
//	/// </summary>
//	ScreenResolution,
//	/// <summary>
//	/// 战舰视野
//	/// </summary>
//	TheSpacecraftLineOfSight,
//	/// <summary>
//	/// 人的视野
//	/// </summary>
//	PeopleStadia,
//	/// <summary>
//	/// 视频质量
//	/// </summary>
//	VideoQuality,
//	/// <summary>
//	/// 纹理质量
//	/// </summary>
//	TextureQuality,
//	/// <summary>
//	/// 灯光质量
//	/// </summary>
//	LightQuality,
//	/// <summary>
//	/// 各向异性的贴图（法线）
//	/// </summary>
//	AnisotropicTextures,
//	/// <summary>
//	/// 抗锯齿
//	/// </summary>
//	AntiAliasing,
//	/// <summary>
//	/// 实时反射
//	/// </summary>
//	RealtimeReflection,
//	/// <summary>
//	/// 阴影质量
//	/// </summary>
//	ShadowsQuality,
//	/// <summary>
//	/// 同步
//	/// </summary>
//	VSyncCount,
//	/// <summary>
//	/// HDR高动态声音渲染
//	/// </summary>
//	HDR,
//	/// <summary>
//	/// 屏幕特效
//	/// </summary>
//	ScreenEffect,
//	/// <summary>
//	/// 控制音量
//	/// </summary>
//	MasterVolume,
//	/// <summary>
//	///特效音量
//	/// </summary>
//	SoundEffectsVolume
//}





