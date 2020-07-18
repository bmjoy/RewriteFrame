#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Map
{
    [ExecuteInEditMode]
    public class StarMapEditorRoot : MonoBehaviour
    {

        #region 属性
        /// <summary>
        /// 星图配置数据
        /// </summary>
        private List<StarMapVO> m_StarMapVoList;
        
        /// <summary>
        /// 恒星关联的GamingMap
        /// </summary>
        private Dictionary<int, List<EditorGamingMap>> m_GamingMapDatas = new Dictionary<int, List<EditorGamingMap>>();

        public List<EditorGamingMap> m_AllGamingMaps;

        #endregion

        #region 挂载
        /// <summary>
        /// 编辑星图界面
        /// </summary>
        public StarMapMainPanel m_StarMapMainPanel;
        /// <summary>
        /// 编辑恒星界面
        /// </summary>
        public FixedStarPanel m_FixedStarPanel;
        /// <summary>
        /// 编辑行星界面
        /// </summary>
        public PlanetPanel m_PlanetPanel;

        public Transform m_RootTrans;
        
        #endregion

        #region 私有方法
        private void OnEnable()
        {
            if (Application.isPlaying)
            {
                Object.DestroyImmediate(this);
                return;
            }
            InitEditor();
        }

        private void OnDisable()
        {
            ClearEditor();
        }
        
        private void CloseAllPanel()
        {
            CloseFixedStar();
            ClosePlanet();
            CloseStarMap();
        }

        private void InitGamingMapDatas()
        {
            //if(m_AllGamingMaps == null)
            //{
            //    return;
            //}
            m_GamingMapDatas.Clear();
            if (m_StarMapVoList != null &&m_StarMapVoList.Count>0)
            {
                for(int iStar = 0;iStar<m_StarMapVoList.Count;iStar++)
                {
                    StarMapVO starVo = m_StarMapVoList[iStar];
                    if(starVo == null)
                    {
                        continue;
                    }
                    List<EditorGamingMap> editorGamings = new List<EditorGamingMap>();
                    for (int iAll = 0;iAll<m_AllGamingMaps.Count;iAll++)
                    {
                        EditorGamingMap editorGaming = m_AllGamingMaps[iAll];
                        if(editorGaming.belongFixedStar == starVo.FixedStarid)
                        {
                            editorGamings.Add(editorGaming);
                        }
                    }
                    m_GamingMapDatas.Add(starVo.FixedStarid, editorGamings);
                }
            }
        }

        public Vector3 GetRootPos()
        {
            return m_RootTrans.position;
        }
        #endregion

        #region Json相关
        public EditorStarMap m_PreviewStarMap = null;
        public void LoadStarMap()
        {
            EditorStarMap starMap = EditorGamingMapData.LoadStarMap();
            if(starMap == null)
            {
                return;
            }
            m_PreviewStarMap = starMap;
            InitPanels(true);
        }

        public Vector3 GetPreviewStarMapPos(int fixedStarId)
        {
            if (m_PreviewStarMap != null)
            {
                EditorFixedStar[] stars = m_PreviewStarMap.fixedStars;
                if (stars != null && stars.Length > 0)
                {
                    for (int iStar = 0; iStar < stars.Length; iStar++)
                    {
                        if (stars[iStar].fixedStarId == fixedStarId)
                        {
                            return stars[iStar].position.ToVector3();
                        }
                    }
                }
            }
            return Vector3.zero;
        }

        public EditorStarMapArea GetPreviewArea(EditorPlanet planet,ulong areaId)
        {
            if(planet == null)
            {
                return null;
            }
            if (m_PreviewStarMap != null)
            {
                EditorStarMapArea[] areas = planet.arealist;
                if (areas != null && areas.Length > 0)
                {
                    for (int iArea = 0; iArea < areas.Length; iArea++)
                    {
                        EditorStarMapArea starMapArea = areas[iArea];
                        if(starMapArea == null)
                        {
                            continue;
                        }
                        if(starMapArea.areaId == areaId)
                        {
                            return starMapArea;
                        }
                    }
                }
            }
            return null;
        }


        public EditorPlanet GetPreviewPlanet(uint gamingmap_id)
        {
            if (m_PreviewStarMap != null)
            {
                EditorFixedStar[] stars = m_PreviewStarMap.fixedStars;
                if (stars != null && stars.Length > 0)
                {
                    for (int iStar = 0; iStar < stars.Length; iStar++)
                    {
                       EditorPlanet[] planets = stars[iStar].planetList;
                        if(planets != null && planets.Length>0)
                        {
                            for(int iPlanet = 0;iPlanet<planets.Length;iPlanet++)
                            {
                                if(planets[iPlanet].gamingmapId == gamingmap_id)
                                {
                                    return planets[iPlanet];
                                }
                            }
                        }
                    }
                }
            }
            return null;
        }

        public void ExportStarMap()
        {
            if (m_StarMapVoList != null && m_StarMapVoList.Count > 0)
            {
                EditorStarMap starMapData = new EditorStarMap();
                List<EditorFixedStar> fixedStars = new List<EditorFixedStar>();
                List<EditorPlanet> planets = new List<EditorPlanet>();
                List<EditorStarMapArea> planetAreas = new List<EditorStarMapArea>();
                FixedStarElement element = null;
                for (int iStar = 0; iStar < m_StarMapVoList.Count; iStar++)
                {
                    StarMapVO starMapVo = m_StarMapVoList[iStar];
                    EditorFixedStar fixedStar = new EditorFixedStar();
                    fixedStars.Add(fixedStar);
                    fixedStar.fixedStarId = starMapVo.FixedStarid;
                    fixedStar.fixedStarName = starMapVo.Name;
                    fixedStar.fixedStarRes = starMapVo.AssetName;
                    fixedStar.relations = starMapVo.Relation_Id;
                    element = m_StarMapMainPanel.GetElement(starMapVo.FixedStarid);
                    if (element != null)
                    {
                        fixedStar.position = new EditorPosition2D(element.GetPosition());
                    }
                    planets.Clear();
                    SavePlanet(planets, starMapVo.FixedStarid, planetAreas,fixedStar);
                    fixedStar.planetList = planets.ToArray();
                }
                starMapData.fixedStars = fixedStars.ToArray();
                EditorGamingMapData.SaveStarMapToJson(starMapData);
            }

        }
        /// <summary>
        /// 保存行星
        /// </summary>
        /// <param name="planets"></param>
        private void SavePlanet(List<EditorPlanet> planets, int fixedStarId, List<EditorStarMapArea> areaList,EditorFixedStar fixedStar)
        {
            List<EditorGamingMap> gamingDatas = null;
            if (m_GamingMapDatas.TryGetValue(fixedStarId, out gamingDatas))
            {
                PlanetContainer panelContainer = m_FixedStarPanel.GetElement(fixedStarId);
                if (gamingDatas != null)
                {
                    for (int iGaming = 0; iGaming < gamingDatas.Count; iGaming++)
                    {
                        EditorGamingMap gamingData = gamingDatas[iGaming];
                        PlanetElement element = null; //= GetElement(gamingData.gamingmap_id);
                        if(panelContainer != null)
                        {
                            element = panelContainer.GetElement(gamingData.gamingmapId);
                        }
                        EditorPlanet editorPlanet = new EditorPlanet();
                        planets.Add(editorPlanet);
                        editorPlanet.gamingmapId = gamingData.gamingmapId;
                        editorPlanet.gamingmapName = gamingData.gamingmapName;
                        PlanetAreaContainer areaContainer = m_PlanetPanel.ExistElement(gamingData.gamingmapId);
                        if(areaContainer != null)
                        {
                            editorPlanet.minimapSize = areaContainer.m_MiniMapSize;
                            editorPlanet.bgmapRes = areaContainer.GetStarRes();
                            editorPlanet.bgmapScale = new EditorPosition2D(areaContainer.m_FixedStarScale);
                            editorPlanet.bgmapPos = new EditorPosition2D(areaContainer.m_FixedStarPos);
                        }
                        
                        if (element != null)
                        {
                            editorPlanet.gamingmapRes = element.m_Res;
                            editorPlanet.position = new EditorPosition2D(element.GetPosition());
                            editorPlanet.scale = new EditorPosition2D(element.GetScale());
                        }
                        areaList.Clear();
                        ulong areaId = 0;
                        SaveArea(gamingData, areaList,ref areaId);
                        if(areaId>0)
                        {
                            fixedStar.ttGamingMapId = gamingData.gamingmapId;
                            fixedStar.ttGamingAreaId = areaId;
                        }
                        editorPlanet.arealist = areaList.ToArray();
                    }
                }
            }
        }
       

        private void SaveArea(EditorGamingMap gamingData, List<EditorStarMapArea> areaList,ref ulong areaId)
        {
            if (gamingData == null || gamingData.areaList == null || gamingData.areaList.Length <= 0)
            {
                return;
            }
            EditorArea[] editorAreas = gamingData.areaList;
            for (int iArea = 0; iArea < editorAreas.Length; iArea++)
            {
                EditorArea editorArea = editorAreas[iArea];
                if (editorArea == null)
                {
                    continue;
                }

                EditorStarMapArea starMapArea = new EditorStarMapArea();
                areaList.Add(starMapArea);
                starMapArea.areaId = editorArea.areaId;
                starMapArea.areaType = editorArea.areaType;
                starMapArea.areaName = editorArea.areaName;
                PlanetAreaElement areaElement = m_PlanetPanel.GetElement(gamingData.gamingmapId, editorArea.areaId);
                if (areaElement != null)
                {
                    starMapArea.area_res = areaElement.m_Res;
                    starMapArea.childrenAreaList = editorArea.childrenAreaList;
                    if(editorArea.leapList != null && editorArea.leapList.Length>0)
                    {
                        starMapArea.area_leap_type = editorArea.leapList[0].leapType;
                    }
                    starMapArea.position = new EditorPosition2D(areaElement.GetPosition());
                }
                if(editorArea.areaType == (int)AreaType.Titan)
                {
                    if(areaId >0)
                    {
                        Debug.LogError(gamingData.gamingmapName+"存在多个泰坦区域:"+ editorArea.areaId);
                    }
                    else
                    {
                        areaId = editorArea.areaId;
                    }
                }
            }
        }

        #endregion

        #region 编辑操作

        public static GameObject FindResAsset(string res)
        {
            if (string.IsNullOrEmpty(res))
            {
                return null;
            }
            string[] resAssets = AssetDatabase.FindAssets(string.Format("{0} t:Prefab", res));
            if (resAssets != null && resAssets.Length > 0)
            {
                string assetPath = resAssets[0];
                for (int iRes = 0; iRes < resAssets.Length; iRes++)
                {
                    string resPath = AssetDatabase.GUIDToAssetPath(resAssets[iRes]);
                    string[] resSplit = resPath.Split('/');
                    if (resSplit != null && resSplit.Length > 0)
                    {
                        string lastName = resSplit[resSplit.Length - 1];
                        if (lastName.Equals(string.Format("{0}.prefab", res)))
                        {
                            return AssetDatabase.LoadAssetAtPath<GameObject>(resPath);
                        }
                    }
                }
            }
            return null;
        }

        private List<GUIContent> m_ContentCache = new List<GUIContent>();
        private IEnumerator m_DoUpdateEnumerator = null;
        private void InitEditor()
        {
            m_PreviewStarMap = null;
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGUI;
            EditorApplication.update += DoUpdate;
            m_DoUpdateEnumerator = DoEditorUpdate();
            m_AllGamingMaps = EditorGamingMapData.LoadAllDeapSpaceMapJson();
            EditorGamingMapData.LoadStarMapVO();
            m_StarMapVoList = ConfigVO<StarMapVO>.Instance.GetList();
            InitPanels();
        }

        private void InitPanels(bool needReset = false)
        {
            InitGamingMapDatas();
            InitStarMap(needReset);
            InitFixedStar(needReset);
            InitPlanet(needReset);
        }
        

        private void RefreshPalnetElement(PlanetElement planetElement)
        {
            m_ContentCache.Clear();
            m_ContentCache.Add(new GUIContent("刷新行星区域"));
            Vector2 mousePosition = Event.current.mousePosition;
            GameObject userData = Selection.activeGameObject;
            int selected = -1;
            EditorUtility.DisplayCustomMenu(new Rect(mousePosition.x, mousePosition.y, 0, 0), m_ContentCache.ToArray(), selected,
                delegate (object data, string[] opt, int select)
                {
                    switch (select)
                    {
                        case 0:
                            OpenPlanet(planetElement);
                            break;

                    }
                }, userData);
            Event.current.Use();
        }

        /// <summary>
        /// 屏幕坐标转为SceneView坐标
        /// </summary>
        /// <param name="sceneView"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        private Vector3 GetWorldPostion(SceneView sceneView,Transform parent)
        {
            Camera cam = sceneView.camera;
            Vector3 mousepos = Event.current.mousePosition;
            mousepos.z = -cam.worldToCameraMatrix.MultiplyPoint(parent.position).z;
            mousepos.y = cam.pixelHeight - mousepos.y;
            mousepos = sceneView.camera.ScreenToWorldPoint(mousepos);
            return mousepos;
        }

        private void DoUpdate()
        {
            if(m_DoUpdateEnumerator != null)
            {
                if(!m_DoUpdateEnumerator.MoveNext())
                {
                    m_DoUpdateEnumerator = null;
                }
            }
        }

        private IEnumerator DoEditorUpdate()
        {
            while(true)
            {
                yield return null;
                if (m_StarMapMainPanel != null)
                {
                    IEnumerator starMapMainEnumer = m_StarMapMainPanel.DoEditorUpdate(this);
                    while(starMapMainEnumer.MoveNext())
                    {
                        yield return null;
                    }
                }
                if (m_FixedStarPanel != null)
                {
                    IEnumerator fixedStarEnumer = m_FixedStarPanel.DoEditorUpdate(this);
                    while (fixedStarEnumer.MoveNext())
                    {
                        yield return null;
                    }
                }
                if (m_PlanetPanel != null)
                {
                    IEnumerator planetEnumer = m_PlanetPanel.DoEditorUpdate(this);
                    while (planetEnumer.MoveNext())
                    {
                        yield return null;
                    }
                }
                yield return null;
            }
            
         }

        private void ClearEditor()
        {
            m_DoUpdateEnumerator = null;
            EditorApplication.hierarchyWindowItemOnGUI -= OnHierarchyGUI;
            EditorApplication.update -= DoUpdate;
            //SceneView.onSceneGUIDelegate -= OnSceneGUI;
            m_StarMapVoList = null;
            m_AllGamingMaps = null;
            m_GamingMapDatas.Clear();
        }

        /// <summary>
		/// 右键响应
		/// </summary>
		/// <param name="instanceID"></param>
		/// <param name="selectionRect"></param>
		private void OnHierarchyGUI(int instanceID, Rect selectionRect)
        {
            if (Event.current != null && selectionRect.Contains(Event.current.mousePosition)
                && Event.current.button == 1 && Event.current.type <= EventType.MouseUp)
            {
                GameObject selectedGameObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
                if (selectedGameObject)
                {
                    StarMapEditorRoot starMapRoot = selectedGameObject.GetComponent<StarMapEditorRoot>();
                    if (starMapRoot != null)
                    {
                        RefreshStarMapRootUI(starMapRoot);
                    }

                    FixedStarElement starElement = selectedGameObject.GetComponent<FixedStarElement>();
                    if(starElement != null)
                    {
                        RefreshFixedStarElementUI(starElement);
                    }

                    StarMapMainPanel starMapMainPanel = selectedGameObject.GetComponent<StarMapMainPanel>();
                    if(starMapMainPanel != null)
                    {
                        RefreshStarMapMainPanel(starMapMainPanel);
                    }

                    PlanetElement planetElement = selectedGameObject.GetComponent<PlanetElement>();
                    if (planetElement != null)
                    {
                        RefreshPalnetElement(planetElement);
                    }

                    FixedStarPanel fixedStarPanel = selectedGameObject.GetComponent<FixedStarPanel>();
                    if(fixedStarPanel != null)
                    {
                        RefreshFixedStarPanel(fixedStarPanel);
                    }

                    PlanetPanel planetPanel = selectedGameObject.GetComponent<PlanetPanel>();
                    if(planetPanel != null)
                    {
                        RefreshPlanetPanel(planetPanel);
                    }

                    PlanetContainer planetContainer = selectedGameObject.GetComponent<PlanetContainer>();
                    if(planetContainer != null)
                    {
                        RefreshPlanetContainer(planetContainer);
                    }

                    PlanetAreaContainer planetAreaContainer = selectedGameObject.GetComponent<PlanetAreaContainer>();
                    if(planetAreaContainer != null)
                    {
                        RefreshPlanetAreaContainer(planetAreaContainer);
                    }
                }
            }
        }

        private void RefreshPlanetAreaContainer(PlanetAreaContainer planetAreaContainer)
        {
            if (planetAreaContainer == null)
            {
                return;
            }
            m_ContentCache.Clear();
            m_ContentCache.Add(new GUIContent("打开"));
            Vector2 mousePosition = Event.current.mousePosition;
            GameObject userData = Selection.activeGameObject;
            int selected = -1;
            EditorUtility.DisplayCustomMenu(new Rect(mousePosition.x, mousePosition.y, 0, 0), m_ContentCache.ToArray(), selected,
                delegate (object data, string[] opt, int select)
                {
                    switch (select)
                    {
                        case 0:
                            OpenPlanetAreaContainer(planetAreaContainer);
                            break;

                    }
                }, userData);
            Event.current.Use();
        }

        private void OpenPlanetAreaContainer(PlanetAreaContainer planetAreaContainer)
        {
            if (planetAreaContainer == null)
            {
                return;
            }
            CloseAllPanel();
            PlanetPanel planetPanel = planetAreaContainer.m_Panel;
            if (planetPanel != null)
            {
                planetPanel.gameObject.SetActive(true);
                planetPanel.Open(planetAreaContainer);
            }
        }

        private void RefreshPlanetContainer(PlanetContainer planetContainer)
        {
            if (planetContainer == null)
            {
                return;
            }
            m_ContentCache.Clear();
            m_ContentCache.Add(new GUIContent("打开"));
            Vector2 mousePosition = Event.current.mousePosition;
            GameObject userData = Selection.activeGameObject;
            int selected = -1;
            EditorUtility.DisplayCustomMenu(new Rect(mousePosition.x, mousePosition.y, 0, 0), m_ContentCache.ToArray(), selected,
                delegate (object data, string[] opt, int select)
                {
                    switch (select)
                    {
                        case 0:
                            OpenPlanetContainer(planetContainer);
                            break;

                    }
                }, userData);
            Event.current.Use();
        }

        private void OpenPlanetContainer(PlanetContainer planetContainer)
        {
            if(planetContainer == null)
            {
                return;
            }
            CloseAllPanel();
            FixedStarPanel fixedStarPanel = planetContainer.m_FixedStarPanel;
            if(fixedStarPanel != null)
            {
                fixedStarPanel.gameObject.SetActive(true);
                fixedStarPanel.Open(planetContainer);
            }
        }

        private void RefreshPlanetPanel(PlanetPanel planetPanel)
        {
            if (planetPanel == null)
            {
                return;
            }
            m_ContentCache.Clear();
            m_ContentCache.Add(new GUIContent("打开"));
            Vector2 mousePosition = Event.current.mousePosition;
            GameObject userData = Selection.activeGameObject;
            int selected = -1;
            EditorUtility.DisplayCustomMenu(new Rect(mousePosition.x, mousePosition.y, 0, 0), m_ContentCache.ToArray(), selected,
                delegate (object data, string[] opt, int select)
                {
                    switch (select)
                    {
                        case 0:
                            OpenPlanet(null);
                            break;

                    }
                }, userData);
            Event.current.Use();
        }

        private void RefreshFixedStarPanel(FixedStarPanel fixedStarPanel)
        {
            if (fixedStarPanel == null)
            {
                return;
            }
            m_ContentCache.Clear();
            m_ContentCache.Add(new GUIContent("打开"));
            Vector2 mousePosition = Event.current.mousePosition;
            GameObject userData = Selection.activeGameObject;
            int selected = -1;
            EditorUtility.DisplayCustomMenu(new Rect(mousePosition.x, mousePosition.y, 0, 0), m_ContentCache.ToArray(), selected,
                delegate (object data, string[] opt, int select)
                {
                    switch (select)
                    {
                        case 0:
                            OpenFixedStar(null);
                            break;

                    }
                }, userData);
            Event.current.Use();
        }

        private void RefreshStarMapMainPanel(StarMapMainPanel starMapMainPanel)
        {
            if(starMapMainPanel == null)
            {
                return;
            }
            m_ContentCache.Clear();
            m_ContentCache.Add(new GUIContent("打开"));
            Vector2 mousePosition = Event.current.mousePosition;
            GameObject userData = Selection.activeGameObject;
            int selected = -1;
            EditorUtility.DisplayCustomMenu(new Rect(mousePosition.x, mousePosition.y, 0, 0), m_ContentCache.ToArray(), selected,
                delegate (object data, string[] opt, int select)
                {
                    switch (select)
                    {
                        case 0:
                            OpenStarMap();
                            break;

                    }
                }, userData);
            Event.current.Use();
        }

        private void RefreshFixedStarElementUI(FixedStarElement starElement)
        {
            if(starElement == null)
            {
                return;
            }

            m_ContentCache.Clear();
            m_ContentCache.Add(new GUIContent("刷新行星"));
            Vector2 mousePosition = Event.current.mousePosition;
            GameObject userData = Selection.activeGameObject;
            int selected = -1;
            EditorUtility.DisplayCustomMenu(new Rect(mousePosition.x, mousePosition.y, 0, 0), m_ContentCache.ToArray(), selected,
                delegate (object data, string[] opt, int select)
                {
                    switch (select)
                    {
                        case 0:
                            this.OpenFixedStar(starElement);
                            break;

                    }
                }, userData);
            Event.current.Use();
        }

        /// <summary>
		/// 刷新npc列表显示ui
		/// </summary>
		/// <param name="contentList"></param>
		/// <param name="areaSpawner"></param>
		private void RefreshStarMapRootUI(StarMapEditorRoot starMapRoot)
        {
            if (starMapRoot == null)
            {
                return;
            }
            m_ContentCache.Clear();
            m_ContentCache.Add(new GUIContent("打开星图面板"));
            m_ContentCache.Add(new GUIContent("保存"));
            m_ContentCache.Add(new GUIContent("导出json"));
            m_ContentCache.Add(new GUIContent("导入json"));
            m_ContentCache.Add(new GUIContent("重置"));
            Vector2 mousePosition = Event.current.mousePosition;
            GameObject userData = Selection.activeGameObject;
            int selected = -1;
            EditorUtility.DisplayCustomMenu(new Rect(mousePosition.x, mousePosition.y, 0, 0), m_ContentCache.ToArray(), selected,
                delegate (object data, string[] opt, int select)
                {
                    switch (select)
                    {
                        case 0:
                            starMapRoot.OpenStarMap();
                            break;
                        case 1:
                            EditorSceneManager.SaveOpenScenes();
                            break;
                        case 2://导出json
                            starMapRoot.ExportStarMap();
                            break;
                        case 3://导入json
                            starMapRoot.LoadStarMap();
                            break;
                        case 4://初始化
                            m_PreviewStarMap = null;
                            InitPanels(true);
                            break;

                    }
                }, userData);
            Event.current.Use();
        }
        #endregion

        #region 编辑星图

        private void InitStarMap(bool needReset = false)
        {
            m_StarMapMainPanel.InitStarMap(this,needReset);
        }

        public void OpenStarMap()
        {
            CloseAllPanel();
            //if(m_StarMapPrefab == null || m_StarContainer == null)
            //{
            //    return;
            //}
            if(m_StarMapMainPanel != null)
            {
                m_StarMapMainPanel.gameObject.SetActive(true);
                Selection.activeGameObject = m_StarMapMainPanel.gameObject;
            }
            
        }

        private void CloseStarMap()
        {
            if(m_StarMapMainPanel != null)
            {
                m_StarMapMainPanel.gameObject.SetActive(false);
            }
        }
        #endregion

        #region 编辑恒星
       

        private void OpenFixedStar(FixedStarElement element)
        {
            CloseAllPanel();
            if (element == null)
            {
                m_FixedStarPanel.gameObject.SetActive(true);
                return;
            }
            if (m_FixedStarPanel != null)
            {
                m_FixedStarPanel.gameObject.SetActive(true);
                if (element != null)
                {
                    m_FixedStarPanel.ShowContainer(element.m_FixedStarid);
                }
            }
        }

        private void InitFixedStar(bool needReset = false)
        {
            m_FixedStarPanel.InitFixedStar(this,needReset);
        }

        private void CloseFixedStar()
        {
            if (m_FixedStarPanel != null)
            {
                m_FixedStarPanel.gameObject.SetActive(false);
            }
        }

        #endregion

        #region 编辑行星
        

        private void OpenPlanet(PlanetElement element)
        {
            CloseAllPanel();
            if(element == null)
            {
                m_PlanetPanel.gameObject.SetActive(true);
            }
            if(m_PlanetPanel != null)
            {
                m_PlanetPanel.gameObject.SetActive(true);
                if (element != null && element.m_StarData != null)
                {
                    m_PlanetPanel.ShowContainer(element.m_StarData.gamingmapId);
                }
            }
        }

        private void InitPlanet(bool needReset = false)
        {
            m_PlanetPanel.Init(this,needReset);
        }

        private void ClosePlanet()
        {
            if(m_PlanetPanel != null)
            {
                m_PlanetPanel.gameObject.SetActive(false);
            }
        }
        #endregion
    }
    
}
#endif