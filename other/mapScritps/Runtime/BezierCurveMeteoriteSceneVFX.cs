#if UNITY_EDITOR
using Map;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class BezierCurveMeteoriteSceneVFX : MonoBehaviour, ILODItem
{
    [EditorExtend.Button("生成", "ApplyChanged", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)]
    public bool _Generate;

    public bool m_ChangePrefab = false;
    [Tooltip("曲线路径")]
    public BezierCurve.BezierCurve m_Path;

    /// <summary>
    /// 星图模板列表（用于生成星图的模板）
    /// </summary>
    [Tooltip("模板列表")]
    public List<StarTmplete> m_StarMapTempletes = new List<StarTmplete>() { new StarTmplete() };

    private UnitData[] m_Units;
    /// <summary>
    /// 单元矩形内物体的数量
    /// </summary>
    [Range(1, 10000), Tooltip("需要生成的数量")]
    public int m_TotalCount = 5;

    /// <summary>
    /// 矩形宽度
    /// </summary>
    [Range(0, 50), Tooltip("偏移")]
    public float m_OffestX = 0;

    /// <summary>
    /// 矩形高度
    /// </summary>
    [Range(0, 50), Tooltip("偏移")]
    public float m_OffestY = 0;

    /// <summary>
    /// 矩形高度
    /// </summary>
    [Range(0, 50), Tooltip("偏移")]
    public float m_OffestZ = 0;

    /// <summary>
    /// 生成
    /// </summary>
    private IEnumerator m_GenerateEnum;
    /// <summary>
    /// 预设资源路径
    /// </summary>
    private string m_AssetPath;
    /// <summary>
    /// 预设父节点
    /// </summary>
    private Transform m_Parent;

    #region LOD相关
    [EditorExtend.Button("应用LOD", "InitializeLOD", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)]
    public bool _ApplyLODButton;
    /// <summary>
    /// 是否需要在编辑器下预览LOD
    /// </summary>
    [Tooltip("是否需要在编辑器下预览LOD")]
    public bool _NeedPreviewLOGInEditMode = false;
    /// <summary>
    /// 如果这个节点是LODGroup的子节点时，当LODGroup在这个LOD以下时才会显示这个特效
    /// </summary>
    [Tooltip("如果这个节点是LODGroup的子节点时，当LODGroup在这个LOD以上时才会显示这个特效")
        , EditorExtend.ToggleInt(Constants.NOTSET_LOD_INDEX
            , Style = EditorExtend.ToggleIntAttribute.FieldStyle.IntSlider
            , MinValue = 0
            , MaxValue = 7)]
    public int MaxDisplayLODIndex = Constants.NOTSET_LOD_INDEX;
    
    protected LODGroup m_LODGroup;
    protected bool m_LODItemActive;
    
    #endregion

    private void Awake()
    {
#if UNITY_EDITOR
        if(Application.isPlaying)
#endif
        {
            InitializeLOD();
        }
    }

    private void OnEnable()
    {
        if (Application.isPlaying)
        {
            return;
        }

        EditorApplication.update += DoUpdate;

        if (m_Path == null)
        {
            m_Path = gameObject.GetComponent<BezierCurve.BezierCurve>();
            if (m_Path == null)
            {
                m_Path = gameObject.AddComponent<BezierCurve.BezierCurve>();
            }
        }

        if (string.IsNullOrEmpty(m_AssetPath))
        {
            CalcuateParent();
        }

    }

    private void CalcuateParent()
    {
        m_Parent = transform;
        while (m_Parent.parent != null)
        {
            m_Parent = m_Parent.parent;
        }

        if (PrefabUtility.IsPartOfPrefabInstance(m_Parent))
        {
            m_AssetPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(m_Parent.gameObject);
        }
    }

    private void OnDisable()
    {
        EditorApplication.update -= DoUpdate;
        if(_NeedPreviewLOGInEditMode)
        {
            RemoveLod();
        }
    }

    private void Init()
    {
        if (m_TotalCount > 0)
        {
            float totalWeight = 0;
            int[] unitCounts = new int[m_StarMapTempletes.Count];
            for (int iTemplate = 0; iTemplate < m_StarMapTempletes.Count; iTemplate++)
            {
                totalWeight += m_StarMapTempletes[iTemplate].m_Weight;
                unitCounts[iTemplate] = 0;
            }

            m_Units = new UnitData[m_TotalCount];
            for (int iTotal = 0; iTotal < m_TotalCount; iTotal++)
            {
                float randomWeight = UnityEngine.Random.Range(0, totalWeight);
                int templateIndex = 0;
                for (int iTemplate = 0; iTemplate < m_StarMapTempletes.Count; iTemplate++)
                {
                    randomWeight -= m_StarMapTempletes[iTemplate].m_Weight;
                    if (randomWeight < 0)
                    {
                        templateIndex = iTemplate;
                        break;
                    }
                }
                StarTmplete starTmplete = m_StarMapTempletes[templateIndex];
                Vector3 position = m_Path.EvaluateInBezier_LocalSpace((iTotal + 1.0f) / m_TotalCount);

                position.x += Random.Range(-m_OffestX, m_OffestX);
                position.y += Random.Range(-m_OffestY, m_OffestY);
                position.z += Random.Range(-m_OffestZ, m_OffestZ);

                Vector3 rotation = Vector3.zero;
                if (starTmplete.m_RandX)
                {
                    rotation.x = Random.Range(0f, 360f);
                }
                if (starTmplete.m_RandY)
                {
                    rotation.y = Random.Range(0f, 360f);
                }
                if (starTmplete.m_RandZ)
                {
                    rotation.z = Random.Range(0f, 360f);
                }

                UnitData data = new UnitData();
                data.m_Position = position;
                data.m_Rotation = rotation;
                data.m_StarTmplete = starTmplete;
                m_Units[iTotal] = data;
            }
        }

        m_GenerateEnum = DoGenerate();
    }


    private IEnumerator DoGenerate()
    {
        if (m_Units != null && m_Units.Length > 0)
        {
            for (int iUnit = 0; iUnit < m_Units.Length; iUnit++)
            {
                if (iUnit % 5 == 0)
                {
                    yield return null;
                }
                UnitData unitData = m_Units[iUnit];
                StarTmplete starTmplete = unitData.m_StarTmplete;
                GameObject obj = UnityEditor.PrefabUtility.InstantiatePrefab(starTmplete.m_Templete) as GameObject;
                obj.transform.SetParent(transform);
                obj.transform.localPosition = unitData.m_Position;
                obj.transform.localEulerAngles = unitData.m_Rotation;
            }
        }
        yield return null;
        if (!string.IsNullOrEmpty(m_AssetPath) && m_Parent != null)
        {
            PrefabUtility.SaveAsPrefabAssetAndConnect(m_Parent.gameObject, m_AssetPath, InteractionMode.AutomatedAction);
        }
    }

    private void DoUpdate()
    {
        if (m_GenerateEnum != null)
        {
            if (!m_GenerateEnum.MoveNext())
            {
                m_GenerateEnum = null;
            }
        }

        if (m_ChangePrefab)
        {
            CalcuateParent();
            if (PrefabUtility.IsPartOfPrefabInstance(m_Parent))
            {
                PrefabUtility.UnpackPrefabInstance(m_Parent.gameObject, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            }
            m_ChangePrefab = false;
        }
    }

    private void OnDestroy()
    {
#if UNITY_EDITOR
        if(Application.isPlaying)
#endif
        {
            RemoveLod();
        }
       
    }
    

    private void Clear()
    {
        int childCount = transform.childCount;
        if (childCount > 0)
        {
            for (int iChild = childCount - 1; iChild >= 0; iChild--)
            {
                GameObject obj = transform.GetChild(iChild).gameObject;
                BezierCurve.BezierPoint point = obj.GetComponent<BezierCurve.BezierPoint>();
                if (point == null)
                {
                    GameObject.DestroyImmediate(obj);
                }
            }
        }
    }


    public void ApplyChanged()
    {
        CalcuateParent();
        if (PrefabUtility.IsPartOfPrefabInstance(m_Parent))
        {
            PrefabUtility.UnpackPrefabInstance(m_Parent.gameObject, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        }

        Clear();
        Init();
    }

    [MenuItem("Custom/创建画线区域")]
    private static void CreateLineStarMapMenuItem()
    {
        GameObject obj = new GameObject("LineStarMap");
        BezierCurveMeteoriteSceneVFX starMapRoot = obj.AddComponent<BezierCurveMeteoriteSceneVFX>();
    }

    #region LOD相关
    public LODGroup GetLODGroup()
    {
        return m_LODGroup;
    }

    public int GetMaxDisplayLODIndex()
    {
        return MaxDisplayLODIndex;
    }

    public void SetLODItemActive(bool active)
    {
#if UNITY_EDITOR
        if (!Application.isPlaying
            && !_NeedPreviewLOGInEditMode)
        {
            return;
        }
#endif
        if (m_LODItemActive != active)
        {
            m_LODItemActive = active;
            gameObject.SetActive(active);
        }
    }

    public bool IsAlive()
    {
        return this != null;
    }

    private void InitializeLOD()
    {
        m_LODItemActive = true;
        if (MaxDisplayLODIndex != Constants.NOTSET_LOD_INDEX)
        {
            m_LODGroup = transform.GetComponentInParent<LODGroup>();
            if (m_LODGroup)
            {
                LODManager.GetInstance().AddLODItem(this);
            }
        }
    }

    private void RemoveLod()
    {
        if (Application.isPlaying)
        {
            LODManager.GetInstance().RemoveLODItem(this);
            m_LODGroup = null;
        }
    }

	public void DoUpdateLOD(int lodIndex)
	{
	}
	#endregion


	/// <summary>
	/// 生成单元数据
	/// </summary>
	public struct UnitData
    {
        public StarTmplete m_StarTmplete;
        public Vector3 m_Position;
        public Vector3 m_Rotation;
    }
}
#endif