using Assets.Scripts.Define;
using Eternity.FlatBuffer;
using PureMVC.Interfaces;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 任务引导标记
/// </summary>
public class HudMissionFlagPanel : HudBase
{
	private const string ASSET_ADDRESS = Leyoutech.Core.Loader.Config.AssetAddressKey.PRELOADUI_HUD_MISSIONFLAGPANEL;

    /// <summary>
    /// 主相机
    /// </summary>
    private Camera m_MainCamera;

    /// <summary>
    /// UI相机
    /// </summary>
    private Camera m_UICamera;
	/// <summary>
	/// 根结点
	/// </summary>
	private RectTransform m_Root;
	/// <summary>
	/// 缩放容器
	/// </summary>
	private RectTransform m_ScaleBox;
	/// <summary>
	/// 标记容器
	/// </summary>
	private RectTransform m_FlagBox;
	/// <summary>
	/// 标记容器
	/// </summary>
	private RectTransform m_IdleBox;
	/// <summary>
	/// 标记模板
	/// </summary>
	private RectTransform m_FlagTemplate;

	/// <summary>
	/// UID缓存
	/// </summary>
	private HashSet<uint> m_TargetUIDs = new HashSet<uint>();
    /// <summary>
    /// 边缘实体列表
    /// </summary>
    private EntityList m_EdgeEntitys = new EntityList();
    /// <summary>
    /// 布局参数表
    /// </summary>
    private Dictionary<float, EntityLayoutParams> m_LayoutParams = new Dictionary<float, EntityLayoutParams>();
    /// <summary>
    /// 更新缓存表1
    /// </summary>
    private Dictionary<TaskTrackingProxy.TrackingInfo, Entity> m_Task2Entity1 = new Dictionary<TaskTrackingProxy.TrackingInfo, Entity>();
    /// <summary>
    /// 更新缓存表1
    /// </summary>
    private Dictionary<TaskTrackingProxy.TrackingInfo, Entity> m_Task2Entity2 = new Dictionary<TaskTrackingProxy.TrackingInfo, Entity>();
    /// <summary>
    /// 分组的实体列表
    /// </summary>
    private List<EntityList> m_GroupedEntitys = new List<EntityList>();

    /// <summary>
    /// GameplayProxy
    /// </summary>
    private GameplayProxy m_GamePlayProxy;
    /// <summary>
    /// CfgEternityProxy
    /// </summary>
    private CfgEternityProxy m_EternityProxy;
    /// <summary>
    /// TaskTrackingProxy
    /// </summary>
    private TaskTrackingProxy m_TrakingProxy;


    public HudMissionFlagPanel() : base(UIPanel.HudMissionFlagPanel, ASSET_ADDRESS, PanelType.Hud) { }

	public override void Initialize()
	{
		m_Root = FindComponent<RectTransform>("Content");
		m_ScaleBox = FindComponent<RectTransform>("Content/ScaleBox");

		m_FlagTemplate = FindComponent<RectTransform>("FlagTemp");
		m_IdleBox = FindComponent<RectTransform>("Content/IdleFlags");
		m_FlagBox = FindComponent<RectTransform>("Content/TargetFlags");

        m_MainCamera = Camera.main;
		m_UICamera = m_Root.GetComponentInParent<Canvas>().worldCamera;
    }

	public override void OnShow(object msg)
	{
		base.OnShow(msg);

        m_GamePlayProxy = Facade.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;
        m_EternityProxy = Facade.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
        m_TrakingProxy = Facade.RetrieveProxy(ProxyName.TaskTrackingProxy) as TaskTrackingProxy;

        StartUpdate();
	}

	public override void OnHide(object msg)
	{
        m_TargetUIDs.Clear();
        m_EdgeEntitys.Clear();
        m_LayoutParams.Clear();

        foreach (Entity entity in m_Task2Entity1.Values) { entity.View = null; }
        m_Task2Entity1.Clear();

        foreach(Entity entity in m_Task2Entity2.Values) { entity.View = null; }
        m_Task2Entity2.Clear();

        foreach(EntityList list in m_GroupedEntitys) { EntityList.Recycle(list); }
        m_GroupedEntitys.Clear();

		while (m_FlagBox.childCount > 0)
		{
			m_FlagBox.GetChild(0).gameObject.SetActive(false);
			m_FlagBox.GetChild(0).SetParent(m_IdleBox);
		}

		base.OnHide(msg);
	}

	public override NotificationName[] ListNotificationInterests()
	{
		return new NotificationName[]
		{
			NotificationName.MSG_LEAP_COMPLETED
		};
	}

	public override void HandleNotification(INotification notification)
	{
		switch (notification.Name)
		{
			case NotificationName.MSG_LEAP_COMPLETED:
				{
					TaskTrackingProxy taskTrackingProxy = Facade.RetrieveProxy(ProxyName.TaskTrackingProxy) as TaskTrackingProxy;
					taskTrackingProxy.RebuildTracking();
				}
				break;
		}
	}

	/// <summary>
	/// 格式化单位米
	/// </summary>
	/// <param name="metre">米数</param>
	/// <returns>text</returns>
	private string FormatMetre(float metre)
	{
		return metre < 1000 ? string.Format("[{0:N1}m]", metre) : string.Format("[{0:N1}km]", metre / 1000.0f);
	}

    /// <summary>
    /// 更新所有任务标记
    /// </summary>
    protected override void Update()
    {
        if (!IsWatchOrUIInputMode() && !IsLeaping() && !IsDead())
        {
            float w = m_Root.rect.width;
            float h = m_Root.rect.height;

            m_ScaleBox.localScale = w > h ? new Vector3(1, h / w, 1) : new Vector3(w / h, 1, 1);
            m_TargetUIDs.Clear();

            BaseEntity main = m_GamePlayProxy.GetEntityById<BaseEntity>(m_GamePlayProxy.GetMainPlayerUID());

            bool isInSpace = IsInSpace();

            m_EdgeEntitys.Clear();

            List<TaskTrackingProxy.TrackingInfo> trackingList = m_TrakingProxy.GetAllTrackings();
            for (int i = 0; i < trackingList.Count; i++)
            {
                TaskTrackingProxy.TrackingInfo target = trackingList[i];

                //忽略重复的引导
                if (target.NpcUID != 0 && m_TargetUIDs.Contains(target.NpcUID))
                    continue;

                //目标位置信息
                Vector3 originPosition = Vector3.zero;
                Vector3 offsetPosition = Vector3.zero;
                float hiddentDistance = 0;
                if (!GetTaskPositionInfo(target, out originPosition, out offsetPosition, out hiddentDistance))
                    continue;

                hiddentDistance *= (isInSpace ? GameConstant.METRE_PER_UNIT : 1);

                //原点距离
                float originDistance = Vector3.Distance(originPosition, main.transform.position) * (isInSpace ? GameConstant.METRE_PER_UNIT : 1);

                //目标位置
                Vector3 targetPosition = originPosition + offsetPosition;

                //过远提示
                if (target.FarNotice)
                {
                    if (originDistance > target.FarNoticeDistance)
                        GameFacade.Instance.SendNotification(NotificationName.MSG_INTERACTIVE_MISSIONESCORT, true);
                    else
                        GameFacade.Instance.SendNotification(NotificationName.MSG_INTERACTIVE_MISSIONESCORT, false);
                }

                //是否在屏幕内
                bool isInScreen = IsInScreen(targetPosition, m_MainCamera);

                //屏幕内跃迁点的任务引导由跃迁点自已显示
                if (isInScreen && target.Mode == TaskTrackingProxy.TrackingMode.LeapPoint)
                    continue;

                if (target.Mode == TaskTrackingProxy.TrackingMode.NpcAndPoint && originDistance < hiddentDistance)
                {
                    Npc entityVO = m_EternityProxy.GetNpcByKey((uint)target.NpcTID);
                    if (entityVO.Display == 0)
                        continue;
                    else if (isInScreen)
                        continue;
                }

                //取出标记视图
                Entity flag = m_Task2Entity1.ContainsKey(target) ? m_Task2Entity1[target] : null;
                if (flag == null)
                {
                    flag = new Entity();
                    if (m_IdleBox.childCount > 0)
                    {
                        flag.View = m_IdleBox.GetChild(0).GetComponent<RectTransform>();
                        flag.View.SetParent(m_FlagBox);
                        flag.View.localScale = Vector3.one;
                        flag.View.gameObject.SetActive(true);
                    }
                    else
                    {
                        flag.View = Object.Instantiate(m_FlagTemplate, m_FlagBox);
                        flag.View.localScale = Vector3.one;
                        flag.View.gameObject.SetActive(true);
                    }

                    flag.MissionNode = FindComponent<Animator>(flag.View, "IconMissionElement");
                    flag.Icon = FindComponent<Image>(flag.MissionNode,"Icon");
                    flag.Arrow = FindComponent<RectTransform>(flag.View, "Arrow");
                    flag.Label = FindComponent<TMP_Text>(flag.View, "Label_DistanceText");

                    flag.MissionNode.SetBool("Finished", target.MissionState == MissionState.Finished);

                    //图标显示
                    MissionType missionType = target.MissionType;
                    if (target.Mode == TaskTrackingProxy.TrackingMode.NpcAndPoint)
                        missionType = m_TrakingProxy.GetNpcMissionType(target.NpcUID, (uint)target.NpcTID);

                    UIUtil.SetIconImage(flag.Icon, GameConstant.FUNCTION_ICON_ATLAS_ASSETADDRESS, GetMissionIcon(missionType));

                }

                m_Task2Entity1.Remove(target);
                m_Task2Entity2.Add(target, flag);

                //屏幕内
                if (isInScreen)
                {
                    Vector3 screenPoint = m_MainCamera.WorldToScreenPoint(targetPosition);
                    Vector2 localPoint = Vector2.zero;
                    if (RectTransformUtility.ScreenPointToLocalPointInRectangle(m_FlagBox, screenPoint, m_UICamera, out localPoint))
                        flag.View.anchoredPosition = localPoint;
                }

                //方向箭头
                flag.Arrow.gameObject.SetActive(!isInScreen);
                if (!isInScreen)
                {
                    Vector3 inCameraPosition = m_MainCamera.transform.InverseTransformPoint(targetPosition);
                    flag.Arrow.transform.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(inCameraPosition.y, inCameraPosition.x) * Mathf.Rad2Deg - 90);
                }

                //距离显示
                flag.Label.gameObject.SetActive(true);
                if (flag.Label.gameObject.activeSelf)
                    flag.Label.text = FormatMetre(originDistance);

                //目标位置
                flag.TargetPosition = targetPosition;

                //放入边缘列表
                if(!isInScreen)
                    m_EdgeEntitys.Add(flag);


                if (target.NpcUID != 0)
                    m_TargetUIDs.Add(target.NpcUID);
            }
        }

        //回收标记
        foreach (Entity flag in m_Task2Entity1.Values)
        {
            flag.View.gameObject.SetActive(false);
            flag.View.SetParent(m_IdleBox);
        }
        m_Task2Entity1.Clear();

        //交换缓存
        Dictionary<TaskTrackingProxy.TrackingInfo, Entity> tmp = m_Task2Entity1;
        m_Task2Entity1 = m_Task2Entity2;
        m_Task2Entity2 = tmp;

        //布局边缘图标
        LayoutEntityList(m_EdgeEntitys);
        m_EdgeEntitys.Clear();
    }

    /// <summary>
    /// 获取目标位置
    /// </summary>
    /// <param name="target">目标</param>
    /// <param name="targetPosition">目标位置</param>
    /// <param name="offsetPosition">偏移量</param>
    /// <param name="hiddenDistance">任务标记的隐藏距离</param>
    /// <returns>是否有效</returns>
    private bool GetTaskPositionInfo(TaskTrackingProxy.TrackingInfo target, out Vector3 targetPosition, out Vector3 offsetPosition, out float hiddenDistance)
    {
        offsetPosition = Vector3.zero;

        if (target.Mode == TaskTrackingProxy.TrackingMode.NpcAndPoint)
        {
            bool isInSpace = IsInSpace();
            Npc entityVO = m_EternityProxy.GetNpcByKey((uint)target.NpcTID);
            Vector3 offset = entityVO.HeroHeaderLength >= 3 ? new Vector3(entityVO.HeroHeader(0), entityVO.HeroHeader(1), entityVO.HeroHeader(2)) : Vector3.zero;

            BaseEntity entity = null;
            if (target.NpcUID != 0)
            {
                if (isInSpace)
                    entity = m_GamePlayProxy.GetEntityById<SpacecraftEntity>(target.NpcUID);
                else
                    entity = m_GamePlayProxy.GetEntityById<HumanEntity>(target.NpcUID);
            }

            if (entity != null)
            {
                hiddenDistance = entityVO.MissionTargetHiddenDistance;
                targetPosition = entity.transform.position;

                if (isInSpace && entity.GetHeroType() != KHeroType.htMonster)
                    offsetPosition = entity.transform.TransformDirection(offset);
                else if (!isInSpace)
                    offsetPosition = entity.transform.TransformDirection(offset);

                return true;
            }
            else
            {
                hiddenDistance = entityVO.MissionTargetHiddenDistance;
                targetPosition = m_GamePlayProxy.WorldPositionToClientPosition(target.Position);
                return true;
            }
        }
        else if (target.Mode == TaskTrackingProxy.TrackingMode.LeapPoint)
        {
            hiddenDistance = 0;
            targetPosition = m_GamePlayProxy.WorldPositionToClientPosition(target.Position);
            return true;
        }

        hiddenDistance = 0;
        targetPosition = Vector3.zero;
        return false;
    }


    #region 指引定位

    /// <summary>
    /// 布局标记列表
    /// </summary>
    /// <param name="entitys">标记列表</param>
    private void LayoutEntityList(EntityList entitys)
    {
        //确定初始属性
        foreach (Entity entity in entitys)
        {
            EntityLayoutParams layout = GetLayoutParams(entity.ViewSize);

            Vector2 position = m_MainCamera.transform.InverseTransformPoint(entity.TargetPosition).normalized * layout.RingRadius;
            entity.Angle = Mathf.Atan2(position.y, position.x) * Mathf.Rad2Deg;
            entity.Radius = layout.GetRadius(entity.Angle);
        }

        //按角度排序
        entitys.SortByAngle();

        //按角度分组
        for (int i = 0; i < entitys.Count; i++)
        {
            if (i == 0 || entitys[i].Angle != entitys[i - 1].Angle)
                m_GroupedEntitys.Add(EntityList.Create());

            m_GroupedEntitys[m_GroupedEntitys.Count - 1].Add(entitys[i]);
        }

        //分组展开
        for (int i = 0; i < m_GroupedEntitys.Count; i++)
        {
            LayoutEntityGroup(m_GroupedEntitys[i]);
        }

        //多层次组合,再展开
        while (m_GroupedEntitys.Count > 1)
        {
            bool changed = false;

            //相邻组合并
            for (int i = 0; i < m_GroupedEntitys.Count - 1; i++)
            {
                EntityList group = m_GroupedEntitys[i];
                EntityList nextGroup = m_GroupedEntitys[i + 1];

                if (group.MaxAngle >= nextGroup.MinAngle)
                {
                    group.AddRange(nextGroup);
                    group.MinAngle = Mathf.Min(group.MinAngle, nextGroup.MinAngle);
                    group.MaxAngle = Mathf.Max(group.MaxAngle, nextGroup.MaxAngle);

                    m_GroupedEntitys.Remove(nextGroup);
                    EntityList.Recycle(nextGroup);
                    changed = true;
                    i--;
                }
            }
            //首尾合并
            if (m_GroupedEntitys.Count > 1)
            {
                EntityList lastGroup = m_GroupedEntitys[m_GroupedEntitys.Count - 1];

                EntityList firstGroup = m_GroupedEntitys[0];
                float firstMinAngle = firstGroup.MinAngle + 360.0f;
                float firstMaxAngle = firstGroup.MaxAngle + 360.0f;

                if (lastGroup.MaxAngle >= firstMaxAngle)
                {
                    lastGroup.MinAngle = Mathf.Min(lastGroup.MinAngle, firstMinAngle);
                    lastGroup.MaxAngle = Mathf.Max(lastGroup.MaxAngle, firstMaxAngle);

                    for(int j=0;j<firstGroup.Count;j++)
                    {
                        Entity entity = firstGroup[j];
                        entity.Angle += 360.0f;
                        lastGroup.Add(entity);
                    }

                    m_GroupedEntitys.Remove(firstGroup);
                    EntityList.Recycle(firstGroup);
                    changed = true;
                }
            }

            //
            if(changed)
            {
                for (int i = 0; i < m_GroupedEntitys.Count; i++)
                {
                    EntityList group = m_GroupedEntitys[i];
                    group.SortByAngle();
                    LayoutEntityGroup(group);
                }
            }
            else
            {
                break;
            }
        }

        //回收组列表
        for (int i = 0; i < m_GroupedEntitys.Count; i++)
        {
            EntityList.Recycle(m_GroupedEntitys[i]);
        }
        m_GroupedEntitys.Clear();

        //应用坐标到视图
        foreach (Entity entity in m_EdgeEntitys)
        {
            EntityLayoutParams layout = GetLayoutParams(entity.ViewSize);

            //Debug.LogWarning(entity.Angle);
            Vector3 pos = Quaternion.Euler(0, 0, entity.Angle) * (Vector3.right * layout.RingRadius);

            Vector2 arrowPosition = m_ScaleBox.InverseTransformPoint(m_Root.TransformPoint(pos));
            arrowPosition = arrowPosition.normalized * (Mathf.Max(m_Root.rect.width, m_Root.rect.height) / 2);
            arrowPosition = m_Root.InverseTransformPoint(m_ScaleBox.TransformPoint(arrowPosition));

            entity.View.anchoredPosition = arrowPosition;
        }
    }

    /// <summary>
    /// 布局标记组
    /// </summary>
    /// <param name="entitys">标记组</param>
    private void LayoutEntityGroup(EntityList entitys)
    {
        Entity first = entitys[0];
        Entity last = entitys[entitys.Count - 1];

        int centerIndex = Mathf.FloorToInt(entitys.Count / 2.0f);
        int leftIndex = centerIndex + (entitys.Count % 2); 
        int rightIndex = centerIndex - 1;

        float centerAngle = first.Angle + (last.Angle - first.Angle) / 2;
        float leftAngle = centerAngle;
        float rightAngle = centerAngle;

        if (entitys.Count % 2 == 1)
        {
            Entity entity = entitys[centerIndex];
            EntityLayoutParams layout = GetLayoutParams(entity.ViewSize);
            float offsetAngle = layout.GetAngleOffsetByRadius(entity.Radius);

            entity.Angle = centerAngle;
            entity.Radius = layout.GetRadius(centerAngle);

            leftAngle += offsetAngle;
            rightAngle -= offsetAngle;
        }

        while (rightIndex >= 0)
        {
            Entity left = entitys[leftIndex];
            EntityLayoutParams leftLayout = GetLayoutParams(left.ViewSize);
            float leftRadius = leftLayout.GetLeftRadius(leftAngle);
            float leftOffsetAngle = leftLayout.GetAngleOffsetByRadius(leftRadius);

            left.Radius = leftRadius;
            left.Angle = leftAngle + leftOffsetAngle;

            Entity right = entitys[rightIndex];
            EntityLayoutParams rightLayout = GetLayoutParams(right.ViewSize);
            float rightRadius = rightLayout.GetRightRadius(rightAngle);
            float rightOffsetAngle = rightLayout.GetAngleOffsetByRadius(rightRadius);

            right.Radius = rightRadius;
            right.Angle = rightAngle - rightOffsetAngle;

            leftAngle += leftOffsetAngle * 2.0f;
            rightAngle -= rightOffsetAngle * 2.0f;

            leftIndex++;
            rightIndex--;
        }

        entitys.MaxAngle = leftAngle;
        entitys.MinAngle = rightAngle;
    }

    /// <summary>
    /// 获取布局参数
    /// </summary>
    /// <param name="size">标记大小</param>
    /// <returns>布局参数</returns>
    private EntityLayoutParams GetLayoutParams(float size)
    {
        if (!m_LayoutParams.ContainsKey(size))
        {
            EntityLayoutParams layoutParams = new EntityLayoutParams();

            layoutParams.RingRadius = Mathf.Max(m_Root.rect.width, m_Root.rect.height) / 2;

            Vector3 upInChild = m_ScaleBox.InverseTransformPoint(m_Root.TransformPoint(Vector3.up)).normalized * layoutParams.RingRadius;
            Vector3 upInRoot = m_Root.InverseTransformPoint(m_ScaleBox.TransformPoint(upInChild));

            layoutParams.MinRadius = size / 2.0f;
            layoutParams.MaxRadius = layoutParams.MinRadius / upInRoot.magnitude * layoutParams.RingRadius;

            layoutParams.MinRadiusRingAngle = Mathf.Asin(layoutParams.MinRadius / 2.0f / layoutParams.RingRadius) * Mathf.Rad2Deg * 2.0f;
            layoutParams.MaxRadiusRingAngle = Mathf.Asin(layoutParams.MaxRadius / 2.0f / layoutParams.RingRadius) * Mathf.Rad2Deg * 2.0f;

            layoutParams.LeftAngleParts = new float[] { 0.0f - layoutParams.MinRadiusRingAngle, 90.0f - layoutParams.MaxRadiusRingAngle, 180.0f - layoutParams.MinRadiusRingAngle, 270.0f - layoutParams.MaxRadiusRingAngle, 360 - layoutParams.MinRadiusRingAngle };
            layoutParams.RightAngleParts = new float[] { 0.0f + layoutParams.MinRadiusRingAngle, 90.0f + layoutParams.MaxRadiusRingAngle, 180.0f + layoutParams.MinRadiusRingAngle, 270.0f + layoutParams.MaxRadiusRingAngle, 360 + layoutParams.MinRadiusRingAngle };

            m_LayoutParams.Add(size, layoutParams);
        }
        return m_LayoutParams[size];
    }

    /// <summary>
    /// 标记
    /// </summary>
    private class Entity
    {
        /// <summary>
        /// 视图对象
        /// </summary>
        public RectTransform View;
        /// <summary>
        /// 任务节点
        /// </summary>
        public Animator MissionNode;
        /// <summary>
        /// 图标
        /// </summary>
        public Image Icon;
        /// <summary>
        /// 箭头
        /// </summary>
        public RectTransform Arrow;
        /// <summary>
        /// 标签
        /// </summary>
        public TMP_Text Label;
        /// <summary>
        /// 大小
        /// </summary>
        public float ViewSize = 64.0f;
        /// <summary>
        /// 目标的世界坐标
        /// </summary>
        public Vector3 TargetPosition;
        /// <summary>
        /// 角度
        /// </summary>
        public float Angle;
        /// <summary>
        /// 半径
        /// </summary>
        public float Radius;
    }
    /// <summary>
    /// 标记列表
    /// </summary>
    private class EntityList : List<Entity>
    {
        /// <summary>
        /// 实例池
        /// </summary>
        private static List<EntityList> s_InstancePool = new List<EntityList>();
        /// <summary>
        /// 列表中的最小角度
        /// </summary>
        public float MinAngle;
        /// <summary>
        /// 列表中的最大角度
        /// </summary>
        public float MaxAngle;
        /// <summary>
        /// 按角度排序
        /// </summary>
        public void SortByAngle()
        {
            this.Sort((a, b) =>
            {
                if (a.Angle < b.Angle)
                    return -1;
                else if (a.Angle > b.Angle)
                    return 1;
                else
                    return 0;
            });
        }
        /// <summary>
        /// 创建实例
        /// </summary>
        /// <returns>EntityList</returns>
        public static EntityList Create()
        {
            EntityList instance = null;
            if (s_InstancePool.Count > 0)
            {
                instance = s_InstancePool[0];
                s_InstancePool.RemoveAt(0);
            }
            if (instance == null)
                instance = new EntityList();
            return instance;
        }
        /// <summary>
        /// 回收实例
        /// </summary>
        /// <param name="instance">实例</param>
        public static void Recycle(EntityList instance)
        {
            instance.Clear();
            s_InstancePool.Add(instance);
        }
    }
    /// <summary>
    /// 标记布局参数
    /// </summary>
    private class EntityLayoutParams
    {
        /// <summary>
        /// 布局环的半径
        /// </summary>
        public float RingRadius;
        /// <summary>
        /// 元素的最小半径
        /// </summary>
        public float MinRadius;
        /// <summary>
        /// 元素的最大半径
        /// </summary>
        public float MaxRadius;
        /// <summary>
        /// 最小半径在布局环上所占角度
        /// </summary>
        public float MinRadiusRingAngle;
        /// <summary>
        /// 最大半径在布局环上所占的角度
        /// </summary>
        public float MaxRadiusRingAngle;
        /// <summary>
        /// 在环上求左切圆的参数段
        /// </summary>
        public float[] LeftAngleParts;
        /// <summary>
        /// 在环上求右切圆的参数段
        /// </summary>
        public float[] RightAngleParts;

        /// <summary>
        /// 根椐角度获取半径
        /// </summary>
        /// <param name="angle">角度</param>
        /// <returns>半径</returns>
        public float GetRadius(float angle)
        {
            angle = ClampAngle(angle);
            if (angle >= 0.0f && angle <= 90.0f)
                return Mathf.Lerp(MinRadius, MaxRadius, (angle - 0.0f) / 90.0f);
            else if (angle > 90.0f && angle <= 180.0f)
                return Mathf.Lerp(MaxRadius, MinRadius, (angle - 90.0f) / 90.0f);
            else if (angle > 180.0f && angle <= 270.0f)
                return Mathf.Lerp(MinRadius, MaxRadius, (angle - 180.0f) / 90.0f);
            else if (angle > 270.0f && angle <= 360.0f)
                return Mathf.Lerp(MaxRadius, MinRadius, (angle - 270.0f) / 90.0f);
            else
                return MinRadius;
        }
        /// <summary>
        /// 获取逆时针方向相切圆的半径
        /// </summary>
        /// <param name="angle">角度</param>
        /// <returns>半径</returns>
        public float GetLeftRadius(float angle)
        {
            angle = ClampAngle(angle);

            if (angle > (360.0f - MinRadiusRingAngle))
                angle -= 360.0f;

            float leftRadius = MinRadius;
            if (angle >= LeftAngleParts[0] && angle <= LeftAngleParts[1])
                return Mathf.Lerp(MinRadius, MaxRadius, (angle - LeftAngleParts[0]) / (LeftAngleParts[1] - LeftAngleParts[0]));
            else if (angle >= LeftAngleParts[1] && angle <= LeftAngleParts[2])
                return Mathf.Lerp(MaxRadius, MinRadius, (angle - LeftAngleParts[1]) / (LeftAngleParts[2] - LeftAngleParts[1]));
            else if (angle >= LeftAngleParts[2] && angle <= LeftAngleParts[3])
                return Mathf.Lerp(MinRadius, MaxRadius, (angle - LeftAngleParts[2]) / (LeftAngleParts[3] - LeftAngleParts[2]));
            else if (angle >= LeftAngleParts[3] && angle <= LeftAngleParts[4])
                return Mathf.Lerp(MaxRadius, MinRadius, (angle - LeftAngleParts[3]) / (LeftAngleParts[4] - LeftAngleParts[3]));
            else
                return MinRadius;
        }
        /// <summary>
        /// 获取顺时针方向相切圆的半径
        /// </summary>
        /// <param name="angle">角度</param>
        /// <returns>半径</returns>
        public float GetRightRadius(float angle)
        {
            angle = ClampAngle(angle);

            if (angle < MinRadiusRingAngle)
                angle += 360.0f;

            if (angle >= RightAngleParts[0] && angle <= RightAngleParts[1])
            {
                return Mathf.Lerp(MinRadius, MaxRadius, (angle - RightAngleParts[0]) / (RightAngleParts[1] - RightAngleParts[0]));
            }
            else if (angle >= RightAngleParts[1] && angle <= RightAngleParts[2])
            {
                return Mathf.Lerp(MaxRadius, MinRadius, (angle - RightAngleParts[1]) / (RightAngleParts[2] - RightAngleParts[1]));
            }
            else if (angle >= RightAngleParts[2] && angle <= RightAngleParts[3])
            {
                return Mathf.Lerp(MinRadius, MaxRadius, (angle - RightAngleParts[2]) / (RightAngleParts[3] - RightAngleParts[2]));
            }
            else if (angle >= RightAngleParts[3] && angle <= RightAngleParts[4])
            {
                return Mathf.Lerp(MaxRadius, MinRadius, (angle - RightAngleParts[3]) / (RightAngleParts[4] - RightAngleParts[3]));
            }
            else
                return MinRadius;
        }
        /// <summary>
        /// 根据半径获取角度偏移
        /// </summary>
        /// <param name="radius">半径</param>
        /// <returns>角度偏移</returns>
        public float GetAngleOffsetByRadius(float radius)
        {
            return Mathf.Lerp(MinRadiusRingAngle, MaxRadiusRingAngle, (radius - MinRadius) / (MaxRadius - MinRadius));
        }
        /// <summary>
        /// 限制角度
        /// </summary>
        /// <param name="angle">角度</param>
        /// <returns>约束后的角度</returns>
        public float ClampAngle(float angle)
        {
            while (angle < 0.0f)
                angle += 360.0f;
            while (angle > 360.0f)
                angle -= 360.0f;
            return angle;
        }
    }

    #endregion
}
