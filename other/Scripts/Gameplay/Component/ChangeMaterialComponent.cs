using Eternity.FlatBuffer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IChangeMaterialProperty
{
	uint GetItemID();
	uint GetUId();
	Npc GetNPCTemplateVO();
	Transform GetRootTransform();
}

public class ChangeMaterialComponent : EntityComponent<IChangeMaterialProperty>
{
	/// <summary>
	/// 所处区域
	/// </summary>
	private enum DistanceType
	{
		none,		/// 默认无
		weak,		/// 弱信号
		medium,     ///	中信号
		strong,     /// 强信号
		discovery   /// 可发现信号
	}
	private DistanceType m_StartType = DistanceType.none;

	/// <summary>
	/// 变换方向+or-
	/// </summary>
	private bool m_IsAdd = false;

	/// <summary>
	/// 用来解决异步标记
	/// </summary>
	private bool m_OnLoaded = false;
	private bool m_OnGet = false;
	private bool m_IsActive = false;
	private float m_Distance;
	private uint m_SignalTid;

	/// <summary>
	/// 变换频率
	/// </summary>
	private float m_Rate = 0;

	/// <summary>
	/// 频率特效alpha
	/// </summary>
	private float m_Alpha;

	/// <summary>
	/// 材质
	/// </summary>
	private readonly string MATERIAL_NAME = "caikuang_Test";

	/// <summary>
	/// 缓存玩家
	/// </summary>
	private SpacecraftEntity m_MainEntity;

	private WaitForSeconds m_WaitForSeconds;

	private MeshRenderer m_MeshRenderer;

	/// <summary>
	/// 需要改变颜色的部分
	/// </summary>
	private Transform m_Transform;

	/// <summary>
	/// 缓存需要改变颜色的特效
	/// </summary>
	private ParticleSystem[] m_ParticleSystems;

	private GameplayProxy m_GameplayProxy;

	private CfgEternityProxy m_CfgEternityProxy;

	private TreasureHuntProxy m_TreasureHuntProxy;

	private Dictionary<DistanceType, uint> m_Colors = new Dictionary<DistanceType, uint>();

	private Dictionary<DistanceType, float> m_Frequencys = new Dictionary<DistanceType, float>();

	private Dictionary<DistanceType, int> m_Sounds = new Dictionary<DistanceType, int>();

	private IChangeMaterialProperty m_Property;

	public override void OnInitialize(IChangeMaterialProperty property)
	{
		m_Property = property;
		Npc npcVO = m_Property.GetNPCTemplateVO();
		/// npcVO.TriggerRange
		m_CfgEternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
		m_TreasureHuntProxy = GameFacade.Instance.RetrieveProxy(ProxyName.TreasureHuntProxy) as TreasureHuntProxy;
		m_GameplayProxy = GameFacade.Instance.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;
		m_MainEntity = m_GameplayProxy.GetMainPlayer();

		GamingConfigTreasure gamingConfigTreasure = m_CfgEternityProxy.GetGamingConfig(1).Value.Treasure.Value;
		GamingConfigTreasureColor colour = gamingConfigTreasure.Colour.Value;
		m_Colors[DistanceType.discovery] = colour.DiscoveryDistance;
		m_Colors[DistanceType.strong] = colour.StrongSignalDistance;
		m_Colors[DistanceType.medium] = colour.MediumSignalDistance;
		m_Colors[DistanceType.weak] = colour.WeakSignalDistance;

		GamingConfigTreasureFrequency frequency = gamingConfigTreasure.Frequency.Value;
		m_Frequencys[DistanceType.discovery] = frequency.DiscoveryDistance;
		m_Frequencys[DistanceType.strong] = frequency.StrongSignalDistance;
		m_Frequencys[DistanceType.medium] = frequency.MediumSignalDistance;
		m_Frequencys[DistanceType.weak] = frequency.WeakSignalDistance;

		GamingConfigTreasureSound sound = gamingConfigTreasure.Sound.Value;
		m_Sounds[DistanceType.none] = (int)sound.NoSignalDetector;
		m_Sounds[DistanceType.strong] = (int)sound.StrongSignalDetector;
		m_Sounds[DistanceType.medium] = (int)sound.MediumSignalDetector;
		m_Sounds[DistanceType.weak] = (int)sound.WeakSignalDetector;
	}

	public override void OnAddListener()
	{
		AddListener(ComponentEventName.OnGetMeshRenderer, OnGetMeshRenderer);
	}

	private void OnGetMeshRenderer(IComponentEvent componentEvent)
	{
		GetMeshRendererEvent getMeshRendererEvent = componentEvent as GetMeshRendererEvent;
		m_MeshRenderer = getMeshRendererEvent.MeshRenderer;
		m_Transform = getMeshRendererEvent.Transform;
		m_OnLoaded = true;
		if (m_OnGet)
		{
			ChangeEffect(m_Distance, m_SignalTid, m_IsActive);
		}
	}

	public void ChangeEffect(float distance, uint signalTid, bool isActive = false)
	{
		/// 解决异步加载问题
		m_OnGet = true;
		if (!m_OnLoaded)
		{
			m_Distance = distance;
			m_SignalTid = signalTid;
			m_IsActive = isActive;
			return;
		}
		m_OnGet = false;

		/// 未激活时特效为白色
		if (!m_IsActive)
		{
			ChangeParticleColor(Color.white);
			return;
		}

		TreasureSignal signalVO = m_CfgEternityProxy.TreasureSignalsByKey(signalTid);
		float wDis = signalVO.WeakSignalDistance;
		float mDis = signalVO.MediumSignalDistance;
		float sDis = signalVO.StrongSignalDistance;
		DistanceType endType = DistanceType.none;
		float far = 0;
		float near = 0;
		/// 判断从小圈到大圈
		if (distance < sDis)
		{
			far = sDis;
			near = signalVO.DiscoveryDistance;
			m_StartType = DistanceType.strong;
			endType = DistanceType.discovery;
		}
		else if (distance < mDis)
		{
			far = mDis;
			near = sDis;
			m_StartType = DistanceType.medium;
			endType = DistanceType.strong;
        }
		else if (distance < wDis)
		{
			far = wDis;
			near = mDis;
			m_StartType = DistanceType.weak;
			endType = DistanceType.medium;
        }

		/// 探测器可能在圈外
		if (m_StartType == DistanceType.none)
		{
			ChangeParticleColor(Color.white);
			return;
		}

		float lerp = (far - distance) / (far - near);
		Color startColor = m_CfgEternityProxy.GetGlobalColor((int)m_Colors[m_StartType]);
		Color endColor =  m_CfgEternityProxy.GetGlobalColor((int)m_Colors[endType]);
		Color curColor = Color.Lerp(startColor, endColor, lerp);
		float curRate = m_Frequencys[m_StartType] + (m_Frequencys[endType] - m_Frequencys[m_StartType]) * lerp;
		///ChangeMaterial(MATERIAL_NAME, curColor, curRate);
		ChangeParticleColor(curColor, curRate);

		/// 音效，先停再播最新的
		if (m_TreasureHuntProxy.GetCurDetectorTransfrom())
		{
			WwiseUtil.PlaySound(m_Sounds[DistanceType.none], false, m_TreasureHuntProxy.GetCurDetectorTransfrom());
		}
		WwiseUtil.PlaySound(m_Sounds[m_StartType], false, m_Property.GetRootTransform());
		m_TreasureHuntProxy.SetCurDetectorSoundInfo(m_Property.GetUId(), m_Property.GetRootTransform());
	}

	private void ChangeParticleColor(Color color, float rate = 0)
	{
		if (m_Transform)
		{
			m_ParticleSystems = m_Transform.GetComponentsInChildren<ParticleSystem>();
			for (int i = 0; i < m_ParticleSystems.Length; i++)
			{
				ParticleSystem particleSystem = m_ParticleSystems[i];
				ParticleSystem.MainModule main = particleSystem.main;
				main.startColor = color;
				/// 只改这个特效的alpha
				if (particleSystem.name == "Sphere1")
				{
					Renderer renderer = particleSystem.GetComponent<Renderer>();
					/// 设置初始值
					if (renderer)
					{
						renderer.material.SetFloat("_Main_intensity", 10);
						renderer.material.SetFloat("_Fre_intensity", 5);
						renderer.material.SetFloat("_fre_power", 0.5f);
					}
				}
			}

			if (rate > 0)
			{
				m_Rate = rate;
			}
			m_Alpha = 5;
		}
	}

	private void ChangeParticleAlpha(float rate)
	{
		for (int i = 0; i < m_ParticleSystems.Length; i++)
		{
			ParticleSystem particleSystem = m_ParticleSystems[i];
			/// 只改这个特效的alpha
			if (particleSystem.name == "Sphere1")
			{
				Renderer renderer = particleSystem.GetComponent<Renderer>();
				if (renderer)
				{
					///renderer.material.SetFloat("_Main_intensity", rate);
					renderer.material.SetFloat("_Fre_intensity", rate);
				}
			}
		}
	}

	public void ChangeMaterial(string name, Color color = default(Color), float rate = 0)
	{
		AssetUtil.LoadAssetAsync(name,
			(pathOrAddress, uObj, userData) =>
			{
				UnityEngine.Material cMaterial = uObj as UnityEngine.Material;
				cMaterial.SetColor("_Frenel", color);
				/// 变化范围1-0-1-0-1...
				cMaterial.SetFloat("_Fre", 1);
				cMaterial.SetFloat("_Vertex", 0);
				if (m_MeshRenderer)
				{
					m_MeshRenderer.material = cMaterial;
					if (rate > 0)
					{
						m_Rate = rate;
					}
				}
			}
		);
	}

	public override void OnUpdate(float delta)
	{
		if (m_Rate > 0)
		{
			base.OnUpdate(delta);
			UpdateRate(delta);
		}
	}

	public override void OnLateUpdate()
	{
		if (m_StartType == DistanceType.none)
		{
			return;
		}

		/// 检测音效范围
		Npc npcVO = m_Property.GetNPCTemplateVO();
		float range = npcVO.TriggerRange;
		float dis = Vector3.Distance(m_MainEntity.GetRootTransform().position, m_Property.GetRootTransform().position);
		uint curUID = m_TreasureHuntProxy.GetCurDetectorUID();
		uint uid = m_Property.GetUId();
		if (dis > range)
		{
			if (uid == curUID)
			{
				WwiseUtil.PlaySound(m_Sounds[DistanceType.none], false, m_TreasureHuntProxy.GetCurDetectorTransfrom());
				m_TreasureHuntProxy.SetCurDetectorSoundInfo(0, null);
			}
		}
		else
		{
			if (curUID == 0)
			{
				WwiseUtil.PlaySound(m_Sounds[m_StartType], false, m_Property.GetRootTransform());
				m_TreasureHuntProxy.SetCurDetectorSoundInfo(uid, m_Property.GetRootTransform());
			}
			else if (uid != curUID)
			{
				SpacecraftEntity entity = m_GameplayProxy.GetEntityById<SpacecraftEntity>(curUID);
				/// 距离当前正在播的距离
				float curDis = Vector3.Distance(m_MainEntity.GetRootTransform().position, entity.GetRootTransform().position);
				if (dis < curDis)
				{
					WwiseUtil.PlaySound(m_Sounds[DistanceType.none], false, m_TreasureHuntProxy.GetCurDetectorTransfrom());
					WwiseUtil.PlaySound(m_Sounds[m_StartType], false, m_Property.GetRootTransform());
					m_TreasureHuntProxy.SetCurDetectorSoundInfo(uid, m_Property.GetRootTransform());
				}
			}

		}
	}

	private void UpdateRate(float delta)
	{
		///float fre = m_MeshRenderer.material.GetFloat("_Fre");
		if (!m_IsAdd)
		{
			m_Alpha -= m_Rate * delta;
		}
		else
		{
			m_Alpha += m_Rate * delta;
		}

		if (m_Alpha <= 0)
		{
			m_IsAdd = true;
			m_Alpha = 0;
		}
		else if (m_Alpha >= 5)
		{
			m_IsAdd = false;
			m_Alpha = 5;
		}

		ChangeParticleAlpha(m_Alpha);
		///m_MeshRenderer.material.SetFloat("_Fre", fre);
	}

	public override void OnDestroy()
	{
		if (m_Property.GetUId() == m_TreasureHuntProxy.GetCurDetectorUID())
		{
			m_TreasureHuntProxy.SetCurDetectorSoundInfo(0, null);
		}

		base.OnDestroy();
	}
}