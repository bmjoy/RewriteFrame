using Assets.Scripts.Define;
using PureMVC.Interfaces;
using UnityEngine;
using System.Collections;

public class HudHitNumber : HudBase
{
    private const string ASSET_ADDRESS = Leyoutech.Core.Loader.Config.AssetAddressKey.PRELOADUI_HUD_HITNUMBER;

    /// <summary>
    /// 主Canvas
    /// </summary>
    private Canvas m_Canvas;
    /// <summary>
    /// 模板
    /// </summary>
    private GameObject m_Template;
    /// <summary>
    /// 模板实例的挂点
    /// </summary>
    private RectTransform m_Content;
    /// <summary>
    /// 动画设置
    /// </summary>
    private HitCharAnimComponent m_Settings;

    public HudHitNumber() : base(UIPanel.HudHitNumber, ASSET_ADDRESS, PanelType.Hud) { }

    public override void Initialize()
    {
        m_Canvas = GetTransform().GetComponentInParent<Canvas>();

        m_Template = FindComponent<Transform>("Templates/Number").gameObject;
        m_Template.CreatePool(0,string.Empty);

        m_Content = FindComponent<RectTransform>("Content");

        m_Settings = GetTransform().GetComponent<HitCharAnimComponent>();
    }

    public override void OnShow(object msg)
    {
        base.OnShow(msg);

        //Test();
    }

    public override void OnHide(object msg)
    {
        m_Template.RecycleAll();

        base.OnHide(msg);
    }

    public override NotificationName[] ListNotificationInterests()
    {
        return new NotificationName[]
        {
            NotificationName.SkillHurt,
            NotificationName.BuffHurt,
            NotificationName.HurtImmuno
        };
    }

    public override void HandleNotification(INotification notification)
    {
        switch ((NotificationName)notification.Name)
        {
            case NotificationName.SkillHurt:
                OnSkillHurt(notification.Body as SkillHurtInfo);
                break;
            case NotificationName.BuffHurt:
                OnBuffHurt(notification.Body as BuffHurtInfo);
                break;
            case NotificationName.HurtImmuno:
                OnHurtImmuno(notification.Body as HurtImmuno);
                break;
        }
    }

    /// <summary>
    /// 处理技能伤害
    /// </summary>
    /// <param name="info">SkillHurtInfo</param>
    private void OnSkillHurt(SkillHurtInfo info)
    {
        if (info == null) { return; }
		//弱点攻击
		if (info.IsWeak && info.Damage != 0 && !info.IsCrit)
		{
			AddNumber(info.TargetID, string.Format(TableUtil.GetLanguageString("damage_number_id_1009"), info.Damage), "hurtweak");
		}
        //普通伤害
        if (info.Damage != 0 && !info.IsCrit && !info.IsWeak)
        {
            AddNumber(info.TargetID, string.Format(TableUtil.GetLanguageString("damage_number_id_1001"), info.Damage), "normal");
        }
        //暴击伤害
        if (info.Damage != 0 && info.IsCrit)
        {
            AddNumber(info.TargetID, string.Format(TableUtil.GetLanguageString("damage_number_id_1002"), info.Damage), "crit");
        }
        //穿透伤害
        if (info.PenetrationDamage != 0)
        {
            AddNumber(info.TargetID, string.Format(TableUtil.GetLanguageString("damage_number_id_1003"), info.PenetrationDamage), "penetration");
        }
        //闪避
        if (info.IsDodge)
        {
            AddNumber(info.TargetID, string.Format(TableUtil.GetLanguageString("damage_number_id_1004")), "miss");
        }
        //效果
        if (info.EffectID != 0)
        {
            //AddNumber(info.TargetID, string.Format(TableUtil.GetLanguageString("damage_number_id_1005"), info.EffectID), "effect");
        }
    }

    /// <summary>
    /// BUFF伤害
    /// </summary>
    /// <param name="info">BuffHurtInfo</param>
    private void OnBuffHurt(BuffHurtInfo info)
    {
        switch(info.type)
        {
            case EffectType.RecoverHP:
                AddNumber(info.targetID, string.Format(TableUtil.GetLanguageString("damage_number_id_1006"), info.value), "recoverHP");
                break;
            case EffectType.RecoverShield:
			case EffectType.RecoverPower:
				AddNumber(info.targetID, string.Format(TableUtil.GetLanguageString("damage_number_id_1007"), info.value), "recoverMP");
                break;
            case EffectType.DotDamage:
                AddNumber(info.targetID, string.Format(TableUtil.GetLanguageString("damage_number_id_1001"), info.value), "buffDotDamage");
                break;
            case EffectType.Damage:
                AddNumber(info.targetID, string.Format(TableUtil.GetLanguageString("damage_number_id_1001"), info.value), "buffDamage");
                break;
        }
    }

    /// <summary>
    /// 伤害免疫
    /// </summary>
    /// <param name="info">HurtImmuno</param>
    public void OnHurtImmuno(HurtImmuno info)
    {
        if (info.value != 0)
        {
            //var proxy = DataManager.GetProxy<CfgSkillSystemProxy>();
            //var buff = proxy.GetBuff(value);
            //var name = proxy.GetLocalization(SystemLanguage.English, buff.NameId);

            //AddNumber(info.targetID, string.Format(TableUtil.GetLanguageString("damage_number_id_1008"), name), "hurtImmuno");
        }
    }

    /// <summary>
    /// 添加一相打击数字
    /// </summary>
    /// <param name="targetID">目标ID</param>
    /// <param name="value">数字内容</param>
    /// <param name="settingName">动画设置</param>
    private void AddNumber(uint targetID, string value, string settingName)
    {
        if (m_Settings == null) { return; }
        if (string.IsNullOrEmpty(settingName)) { return; }

        GameplayProxy proxy = Facade.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;
        SpacecraftEntity target = proxy.GetEntityById<SpacecraftEntity>(targetID);

        if (target == null)
        {
            target = proxy.GetEntityById<SpacecraftEntity>(proxy.GetMainPlayerUID());
        }

        if (target != null)
        {
            HitCharAnimComponent.HitCharAnim curves = m_Settings.GetSetting(settingName);

            if (curves == null) { return; }
            if (curves.offsetX.length == 0 && curves.offsetY.length == 0 && curves.alpha.length == 0 && curves.scale.length == 0) { return; }

            float time = 0.0f;
            if (curves.offsetX.length > 0) { foreach (Keyframe key in curves.offsetX.keys) { time = Mathf.Max(time, key.time); }; }
            if (curves.offsetY.length > 0) { foreach (Keyframe key in curves.offsetY.keys) { time = Mathf.Max(time, key.time); }; }
            if (curves.alpha.length > 0) { foreach (Keyframe key in curves.alpha.keys) { time = Mathf.Max(time, key.time); }; }
            if (curves.scale.length > 0) { foreach (Keyframe key in curves.scale.keys) { time = Mathf.Max(time, key.time); }; }

            if (time <= 0) { return; }

            //计算飞船在屏幕上的点
            Vector3 viewportPoint = Camera.main.WorldToViewportPoint(target.transform.position);
            bool isFront = viewportPoint.z >= Camera.main.nearClipPlane;
            bool inScreen = viewportPoint.x >= 0 && viewportPoint.y >= 0 && viewportPoint.x <= 1 && viewportPoint.y <= 1 && isFront;

            if (inScreen && !IsWatchOrUIInputMode() && !IsDead() && !IsLeaping())
            {
                Vector2 shipPosition = Vector2.zero;
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(m_Content, Camera.main.WorldToScreenPoint(target.transform.position), m_Canvas.worldCamera, out shipPosition))
                {
                    Anim anim = m_Template.Spawn(m_Content).GetOrAddComponent<Anim>();
                    anim.Curves = curves;
                    anim.Text = value.ToString();
                    anim.Run(shipPosition);
                }
            }
        }
    }

    #region 测试

    /// <summary>
    /// 测试
    /// </summary>
    private void Test()
    {
        UIManager.Instance.StartCoroutine(TestCoroutine());
    }

    /// <summary>
    /// 测试协程
    /// </summary>
    /// <returns>IEnumerator</returns>
    protected IEnumerator TestCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(1.0f);
            switch (Random.Range(9, 11))
            {
                case 0:
                    Facade.SendNotification(NotificationName.SkillHurt, new SkillHurtInfo() { Damage = Random.Range(1, 100000) });
                    break;
                case 1:
                    Facade.SendNotification(NotificationName.SkillHurt, new SkillHurtInfo() { Damage = Random.Range(1, 100000), IsCrit = true });
                    break;
                case 2:
                    Facade.SendNotification(NotificationName.SkillHurt, new SkillHurtInfo() { PenetrationDamage = Random.Range(1, 100000) });
                    break;
                case 3:
                    Facade.SendNotification(NotificationName.SkillHurt, new SkillHurtInfo() { IsDodge = true });
                    break;
                case 4:
                    Facade.SendNotification(NotificationName.SkillHurt, new SkillHurtInfo() { EffectID = Random.Range(1, 9) });
                    break;
                case 5:
                    Facade.SendNotification(NotificationName.SkillHurt, new BuffHurtInfo() { type = EffectType.RecoverHP, value = Random.Range(1, 100000) });
                    break;
                case 6:
                    Facade.SendNotification(NotificationName.SkillHurt, new BuffHurtInfo() { type = EffectType.RecoverShield, value = Random.Range(1, 100000) });
                    break;
                case 7:
                    Facade.SendNotification(NotificationName.SkillHurt, new BuffHurtInfo() { type = EffectType.DotDamage, value = Random.Range(1, 100000) });
                    break;
                case 8:
                    Facade.SendNotification(NotificationName.SkillHurt, new BuffHurtInfo() { type = EffectType.Damage, value = Random.Range(1, 100000) });
                    break;
				case 10:
					Facade.SendNotification(NotificationName.SkillHurt, new SkillHurtInfo() { Damage = Random.Range(1, 100000),IsWeak = true });
					break;
			}
        }
    }

    #endregion

    public class Anim : MonoBehaviour
    {
        /// <summary>
        /// 根节
        /// </summary>
        private RectTransform m_RectTransform;
        /// <summary>
        /// 根节点
        /// </summary>
        private CanvasGroup m_CanvasGroup;
        /// <summary>
        /// 文本框
        /// </summary>
        private TMPro.TMP_Text m_Field;

        /// <summary>
        /// 开始时间
        /// </summary>
        private float m_BeginTime;
        /// <summary>
        /// 目标位置
        /// </summary>
        private Vector2 m_TargetPosition;
        /// <summary>
        /// 持续时间
        /// </summary>
        private float m_Duration;
        /// <summary>
        /// 角度
        /// </summary>
        private float m_Angle = 0;
        /// <summary>
        /// 偏移
        /// </summary>
        private float m_Offset = 0;

        /// <summary>
        /// 文字内容
        /// </summary>
        public string Text = "";
        /// <summary>
        /// 动画配置
        /// </summary>
        public HitCharAnimComponent.HitCharAnim Curves;

        /// <summary>
        /// 启动
        /// </summary>
        /// <param name="targetPosition">目标位置</param>
        public void Run(Vector2 targetPosition)
        {
            m_RectTransform = GetComponent<RectTransform>();
            m_CanvasGroup = GetComponent<CanvasGroup>();
            m_Field = transform.Find("Field").GetComponent<TMPro.TMP_Text>();

            m_RectTransform.anchoredPosition = Vector2.zero;
            m_RectTransform.localScale = Vector3.one;
            m_CanvasGroup.alpha = 1;
            m_Field.text = Text;
            m_BeginTime = Time.realtimeSinceStartup;
            m_TargetPosition = targetPosition;
            m_Offset = Random.Range(Curves.birthOffsetBegin, Curves.birthOffsetEnd);
            m_Angle = Random.Range(Curves.birthAngleBegin, Curves.birthAngleEnd);
            m_Duration = 0;

			foreach (Keyframe key in Curves.offsetX.keys) { m_Duration = Mathf.Max(m_Duration, key.time); }
            foreach (Keyframe key in Curves.offsetY.keys) { m_Duration = Mathf.Max(m_Duration, key.time); }
            foreach (Keyframe key in Curves.alpha.keys) { m_Duration = Mathf.Max(m_Duration, key.time); }
            foreach (Keyframe key in Curves.scale.keys) { m_Duration = Mathf.Max(m_Duration, key.time); }

            Update();
        }

        /// <summary>
        /// 更新
        /// </summary>
        private void Update()
        {
            float time = Time.realtimeSinceStartup - this.m_BeginTime;
            if (time < m_Duration)
            {
                if (Curves.alpha.length > 0)
                {
                    m_CanvasGroup.alpha = Curves.alpha.Evaluate(time);
                }
                if (Curves.scale.length > 0)
                {
                    float scale = Curves.scale.Evaluate(time);
                    m_RectTransform.localScale = new Vector3(scale, scale, 1);
                }

                if (Curves.offsetX.length > 0 || Curves.offsetY.length > 0)
                {
                    float x = Curves.offsetX.length > 0 ? Curves.offsetX.Evaluate(time) : 0;
                    float y = Curves.offsetY.length > 0 ? Curves.offsetY.Evaluate(time) : 0;

                    Quaternion rotation = Quaternion.Euler(0, 0, m_Angle);
                    Vector3 postion = rotation * new Vector3(x + 0, y + m_Offset, 0);

                    m_RectTransform.anchoredPosition = m_TargetPosition + new Vector2(postion.x, postion.y);
                }
                else
                {
                    Quaternion rotation = Quaternion.Euler(0, 0, m_Angle);
                    Vector3 postion = rotation * new Vector3(0, m_Offset, 0);

                    m_RectTransform.anchoredPosition = m_TargetPosition + new Vector2(postion.x, postion.y);
                }
            }
            else
            {
                gameObject.Recycle();
            }
        }
    }
}
