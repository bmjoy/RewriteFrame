using Leyoutech.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;
using static BatchRendering.RandomDisperseMesh;

namespace Map
{
    [ExecuteInEditMode]
    public class RandomDisperseMulMesh : MonoBehaviour
    {

        #region 属性
        /// <summary>
        /// 随机分布信息
        /// </summary>
        [Serializable]
        public class DisperseInfo
        {
            public Mesh m_Mesh;

            public Material m_Material;
            /// <summary>
            /// 计算Mesh顶点的MVP矩阵的Shader
            /// </summary>
            public ComputeShader ComputeShader;

            [System.NonSerialized]
            public Material m_UseMaterial;

            [System.NonSerialized]
            public int m_DisperseCount;
            /// <summary>
            /// 角度是否开启随机
            /// </summary>
            public bool m_RandX = true;
            public bool m_RandY = true;
            public bool m_RandZ = true;
            /// <summary>
            /// 权重
            /// </summary>
            [Range(0, 100)]
            public int m_Weight;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct GlobalState
        {
            public Matrix4x4 MatM;
            public Matrix4x4 MatMVP;
            public Vector3 CameraLocalPosition;
            public Vector3 CameraLocalForward;
        }

        /// <summary>
		/// Mesh的Transform信息
		/// 不命名为MeshTransform是因为Mesh以后可能会运动，那这里就需要存速度、角速度等信息
		/// </summary>
		[StructLayout(LayoutKind.Sequential)]
        private struct MeshState
        {
            public Vector3 LocalPosition;
            public Vector3 LocalRotation;
            public Vector3 LocalScale;
            public Matrix4x4 Dummy1;
            public Matrix4x4 Dummy2;
            public int Dummy3;
        }


        private const string CS_MAIN_KERNEL_NAME = "CSMain";
        private const string CS_PARAM1_NAME = "_Param1";
        private const string CS_GLOBAL_STATE_NAME = "_GlobalState";
        private const string CS_MESH_STATES_NAME = "_MeshStates";


        

        /// <summary>
        /// 散布模板
        /// </summary>
        public List<DisperseInfo> m_DisperseTempletes = new List<DisperseInfo>() { new DisperseInfo() };

        /// <summary>
        /// 散布物体总数量
        /// </summary>
        public int m_DispersCount;

        /// <summary>
        /// X轴方向的比例
        /// </summary>
        [Range(0.0f, 10.0f)]
        public float m_ScaleX = 1;
        /// <summary>
        /// Y轴方向的比例
        /// </summary>
        [Range(0.0f, 10.0f)]
        public float m_ScaleY = 1;

        /// <summary>
        /// z轴方向的比例
        /// </summary>
        [Range(0.0f, 10.0f)]
        public float m_ScaleZ = 1;

        /// <summary>
        /// 半径
        /// </summary>
        public float m_Radius;
        /// <summary>
        /// 方向的偏移
        /// </summary>
        [Range(0.0f, 5.0f)]
        public float m_Offest;
        /// <summary>
		/// Mesh到相机的最小距离，相机接近时Mesh会被推开
		/// </summary>
		public float MinDisplayToCamera;
        /// <summary>
        /// 会显示Mesh的最大距离，超过这个距离，Mesh会被放在一个不可能看到的位置
        /// </summary>
        public float MaxDisplayDistanceToCamera;
        /// <summary>
		/// Mesh的最小缩放
		/// </summary>
		public Vector3 MinScale;
        /// <summary>
        /// Mesh的最大缩放
        /// </summary>
        public Vector3 MaxScale;
        /// <summary>
        /// 缩放的xyz轴之间的最大Offset，如果为0那么就是uniform scale
        /// </summary>
        public float ScaleMaxOffset;
        private Camera m_Camera;
        /// <summary>
		/// Mesh在世界空间的AABB
		/// </summary>
		private Bounds m_LimitBounds;

        private int[] m_CS_MainKernel;

        private GlobalState[] m_GlobalState;

        private ComputeBuffer m_CB_GlobalState;
        private List<ComputeBuffer> m_CB_BufferArgs = new List<ComputeBuffer>();
        private List<ComputeBuffer> m_CB_MeshStates = new List<ComputeBuffer>();

#if UNITY_EDITOR
        /// <summary>
        /// Scene、Game窗口的MVP矩阵公用一块显存，而计算Scene在Game之后
        /// 会导致两个窗口都渲染时，Game窗口用的MVP矩阵和Scene窗口相同
        /// 解决方法：
        ///		A：分配两块显存分别用于Scene、Game
        ///			缺点：我能力有限，这样做可能会影响游戏Build后的运行性能
        ///		B：只在一个窗口Rendering
        ///			缺点：只能在一个窗口预览效果
        /// </summary>
        public RendererIn MyRendererIn;
#endif
        #endregion

        #region 私有方法

        private void OnEnable()
        {
            StartRendering(Camera.main);
#if UNITY_EDITOR
            UnityEditor.SceneView.onSceneGUIDelegate += OnSceneGUI;
#endif
        }

        private void OnDisable()
        {
            StopRendering();
#if UNITY_EDITOR
            UnityEditor.SceneView.onSceneGUIDelegate -= OnSceneGUI;
#endif
        }

        public Bounds CaculateLimitBounds()
        {
            return new Bounds(transform.position, Vector3.one * m_Radius * 2* (m_Offest>0?m_Offest:1));
        }

        private void DoUpdate(Camera camera = null)
        {
            if(camera != null)
            {
                m_Camera = camera;
            }
            if (m_Camera == null)
            {
                m_Camera = Camera.main;
            }
            if(m_Camera == null)
            {
                return;
            }
            try
            {
                m_LimitBounds.center = transform.position;

                Matrix4x4 mat_M = transform.localToWorldMatrix;
                Matrix4x4 mat_V = m_Camera.worldToCameraMatrix;
                Matrix4x4 mat_P = GL.GetGPUProjectionMatrix(m_Camera.projectionMatrix, true);
                m_GlobalState[0].MatM = mat_M;
                m_GlobalState[0].MatMVP = mat_P * mat_V * mat_M;
                m_GlobalState[0].CameraLocalPosition = transform.InverseTransformPoint(m_Camera.transform.position);
                m_GlobalState[0].CameraLocalForward = transform.InverseTransformDirection(m_Camera.transform.forward);
                m_CB_GlobalState.SetData(m_GlobalState);
                
                if(m_DisperseTempletes != null && m_DisperseTempletes.Count>0)
                {
                    for(int iDisperse = 0;iDisperse<m_DisperseTempletes.Count;iDisperse++)
                    {
                        DisperseInfo disperseInfo = m_DisperseTempletes[iDisperse];
                        if(disperseInfo == null || disperseInfo.m_Material == null|| disperseInfo.m_Mesh == null || disperseInfo.m_DisperseCount<=0)
                        {
                            continue;
                        }
                        
                        disperseInfo.ComputeShader.Dispatch(m_CS_MainKernel[iDisperse], disperseInfo.m_DisperseCount, 1, 1);
                        Graphics.DrawMeshInstancedIndirect(disperseInfo.m_Mesh
                            , 0
                            , disperseInfo.m_UseMaterial
                            , m_LimitBounds
                            , m_CB_BufferArgs[iDisperse]
                            , 0
                            , null
                            , ShadowCastingMode.Off
                            , false
                            , gameObject.layer
                            , m_Camera
                            , LightProbeUsage.Off);
                            }
                }
                
            }
            catch (Exception e)
            {

            }
        }

        private void LateUpdate()
        {
#if UNITY_EDITOR
            if (MyRendererIn == RendererIn.Game)
            {
                DoUpdate();
            }
#else
            DoUpdate(FindObjectOfType<Camera>());
#endif

        }
        /// <summary>
		/// 开始渲染，流程：
		///		随机分布Mesh
		///		分配显存
		///		把Mesh的Transform信息发送给显存
		/// </summary>
		public void StartRendering(Camera camera)
        {
            try
            {
                m_Camera = camera;
                
                m_GlobalState = new GlobalState[1];
                m_CB_GlobalState = new ComputeBuffer(1, Marshal.SizeOf(typeof(GlobalState)));
                int[] disperseMeshs = new int[m_DisperseTempletes.Count];
                if(m_DispersCount>0)
                {
                    float totalWeight = 0;
                    int[] unitCounts = new int[m_DisperseTempletes.Count];
                    for (int iTemplate = 0; iTemplate < m_DisperseTempletes.Count; iTemplate++)
                    {
                        totalWeight += m_DisperseTempletes[iTemplate].m_Weight;
                        unitCounts[iTemplate] = 0;
                    }

                    for(int iDisperse = 0;iDisperse<m_DispersCount;iDisperse++)
                    {
                        float randomWeight = UnityEngine.Random.Range(0, totalWeight);
                        int templateIndex = 0;
                        for (int iTemplate = 0; iTemplate < m_DisperseTempletes.Count; iTemplate++)
                        {
                            randomWeight -= m_DisperseTempletes[iTemplate].m_Weight;
                            if (randomWeight < 0)
                            {
                                templateIndex = iTemplate;
                                break;
                            }
                        }
                        
                        disperseMeshs[templateIndex]++;
                    }
                }
                if(disperseMeshs != null && disperseMeshs.Length>0)
                {
                    m_CS_MainKernel = new int[disperseMeshs.Length];
                    for (int iDisperse = 0; iDisperse < disperseMeshs.Length; iDisperse++)
                    {
                        DisperseInfo disperseInfo = m_DisperseTempletes[iDisperse];
                        int disperseCount = disperseMeshs[iDisperse];
                        MeshState[] meshStates = new MeshState[disperseCount];
                        for(int iMesh = 0;iMesh<disperseCount;iMesh++)
                        {
                            Vector3 position = UnityEngine.Random.insideUnitSphere;
                            float magnitude = m_Offest;
                            magnitude += position.magnitude * (1 - m_Offest);
                            position.x *= m_ScaleX;
                            position.y *= m_ScaleY;
                            position.z *= m_ScaleZ;
                            Vector3 rotation = Vector3.zero;
                            if (disperseInfo.m_RandX)
                            {
                                rotation.x = UnityEngine.Random.Range(0f, 360f);
                            }
                            if (disperseInfo.m_RandY)
                            {
                                rotation.y = UnityEngine.Random.Range(0f, 360f);
                            }
                            if (disperseInfo.m_RandZ)
                            {
                                rotation.z = UnityEngine.Random.Range(0f, 360f);
                            }
                            position = position.normalized * magnitude * m_Radius;
                            meshStates[iMesh].LocalPosition = position;
                            meshStates[iMesh].LocalRotation = rotation;
                            meshStates[iMesh].LocalScale = RandomUtility.RandomScale(MinScale, MaxScale, ScaleMaxOffset);
                        }
                        
                        ComputeBuffer m_CB_MeshState = new ComputeBuffer(disperseCount, Marshal.SizeOf(typeof(MeshState)));
                        m_CB_MeshState.SetData(meshStates);
                        m_CB_MeshStates.Add(m_CB_MeshState);
                        uint[] m_BufferArgs = new uint[5] { disperseInfo.m_Mesh.GetIndexCount(0),(uint)disperseCount,0,0,0 };
                        ComputeBuffer bufferArgs = new ComputeBuffer(1, (m_BufferArgs.Length * Marshal.SizeOf(typeof(uint)))
                         , ComputeBufferType.IndirectArguments);
                        bufferArgs.SetData(m_BufferArgs);

                        m_CB_BufferArgs.Add(bufferArgs);
                        m_CS_MainKernel[iDisperse] = disperseInfo.ComputeShader.FindKernel(CS_MAIN_KERNEL_NAME);
                        disperseInfo.ComputeShader.SetBuffer(m_CS_MainKernel[iDisperse], CS_GLOBAL_STATE_NAME, m_CB_GlobalState);
                        disperseInfo.ComputeShader.SetBuffer(m_CS_MainKernel[iDisperse], CS_MESH_STATES_NAME, m_CB_MeshState);
                        disperseInfo.ComputeShader.SetVector(CS_PARAM1_NAME, new Vector4(MinDisplayToCamera, MaxDisplayDistanceToCamera, 0, 0));
                        if (disperseInfo.m_UseMaterial == null)
                        {
                            disperseInfo.m_UseMaterial = new Material(disperseInfo.m_Material);
                        }
                        disperseInfo.m_UseMaterial.SetBuffer(CS_MESH_STATES_NAME, m_CB_MeshState);
                        disperseInfo.m_DisperseCount = disperseCount;
                    }
                }
                
                m_LimitBounds = CaculateLimitBounds();
            }
            catch (Exception e)
            {

            }

        }
#if UNITY_EDITOR
        /// <summary>
        /// 用于在Scene窗口Renderer
        /// </summary>
        protected void OnSceneGUI(UnityEditor.SceneView sceneView)
        {
            if (MyRendererIn == RendererIn.Scene)
            {
                DoUpdate(sceneView.camera);
            }
           
        }
#endif
        /// <summary>
        /// 停止渲染，工作流程：
        ///		释放显存
        /// </summary>
        public void StopRendering()
        {
            try
            {
                m_CB_GlobalState.Release();
                for(int iMesh = 0;iMesh< m_CB_MeshStates.Count;iMesh++)
                {
                    m_CB_MeshStates[iMesh].Release();
                }
                for(int iBufferArg = 0;iBufferArg<m_CB_BufferArgs.Count;iBufferArg++)
                {
                    m_CB_BufferArgs[iBufferArg].Release();
                }
                m_CB_MeshStates.Clear();
                m_CB_BufferArgs.Clear();
            }
            catch (Exception e)
            {

            }
        }


#endregion

    }
}

