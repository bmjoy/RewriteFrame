using PureMVC.Interfaces;
using UnityEngine;
using UnityEngine.UI;

public class HudWeaponPanel : UIPanelBase
{
    private const string ASSET_ADDRESS = Leyoutech.Core.Loader.Config.AssetAddressKey.PRELOADUI_HUD_WEAPONPANEL;

    /// <summary>
    /// 根节点
    /// </summary>
    private CanvasGroup m_Root;
    /// <summary>
    /// 根节点动画
    /// </summary>
    private Animator m_RootAnimator;
    /// <summary>
    /// 主武器
    /// </summary>
    private Animator m_MainWeapon;
    /// <summary>
    /// 主武器图标
    /// </summary>
    private Image m_MainWeaponIcon;
    /// <summary>
    /// 副武器
    /// </summary>
    private Animator m_SecondaryWeapon;
    /// <summary>
    /// 副武器图标
    /// </summary>
    private Image m_SecondaryWeaponIcon;


    public HudWeaponPanel() : base(UIPanel.HudWeaponPanel, ASSET_ADDRESS, PanelType.Hud) { }

    public override void Initialize()
    {
        m_Root = FindComponent<CanvasGroup>("Content");
        m_RootAnimator = FindComponent<Animator>("Content");

        m_MainWeapon = m_Root.transform.GetChild(0).GetComponent<Animator>();
        m_MainWeaponIcon = m_MainWeapon.transform.Find("Content/Image_WeaponIcon").GetComponent<Image>();
        m_SecondaryWeapon = m_Root.transform.GetChild(1).GetComponent<Animator>();
        m_SecondaryWeaponIcon = m_SecondaryWeapon.transform.Find("Content/Image_WeaponIcon").GetComponent<Image>();
    }

    public override void OnShow(object msg)
    {
        base.OnShow(msg);

        OnWeaponToggleEnd(true);
    }

    public override NotificationName[] ListNotificationInterests()
    {
        return new NotificationName[]
        {
            NotificationName.PlayerWeaponToggleEnd,
			NotificationName.MSG_PACKAGE_ITEM_OPERATE,
			NotificationName.MSG_HUMAN_ENTITY_ON_ADDED
		};
    }

    public override void HandleNotification(INotification notification)
    {
        switch (notification.Name)
        {
            case NotificationName.PlayerWeaponToggleEnd:
			case NotificationName.MSG_PACKAGE_ITEM_OPERATE:
				OnWeaponToggleEnd(false);
                break;
			case NotificationName.MSG_HUMAN_ENTITY_ON_ADDED:
				{
					MsgEntityInfo entityInfo = (MsgEntityInfo)notification.Body;
					if (entityInfo.IsMain)
                        OnWeaponToggleEnd(true);
                }
				break;
		}
    }

    /// <summary>
    /// 武器切换时
    /// </summary>
    /// <param name="hiddenRoot">是否隐藏根节点</param>
    private void OnWeaponToggleEnd(bool hiddenRoot)
    {
        m_Root.alpha = hiddenRoot ? 0 : 1;

        PlayerSkillProxy skillProxy = Facade.RetrieveProxy(ProxyName.PlayerSkillProxy) as PlayerSkillProxy;

		IWeapon currWeapon = skillProxy.GetCurrentWeapon();
        if (currWeapon != null)
        {
            IWeapon mainWeapon = skillProxy.GetWeaponByIndex(0);
			IWeapon secondaryWeapon = skillProxy.GetWeaponByIndex(1);

            m_MainWeapon.gameObject.SetActive(true);
            m_SecondaryWeapon.gameObject.SetActive(true);

            m_MainWeapon.ResetTrigger("Big");
            m_MainWeapon.ResetTrigger("Small");

            m_SecondaryWeapon.ResetTrigger("Big");
            m_SecondaryWeapon.ResetTrigger("Small");

            m_RootAnimator.ResetTrigger("Show");
            m_RootAnimator.SetTrigger("Show");

            if (skillProxy.UsingReformer())
            {
                if (currWeapon.GetPos() == 0)
                {
                    m_MainWeapon.SetTrigger("Big");
                    m_SecondaryWeapon.SetTrigger("Small");
                    m_SecondaryWeapon.gameObject.SetActive(secondaryWeapon != null);
                }
                else if (currWeapon.GetPos() == 1)
                {
                    m_MainWeapon.SetTrigger("Small");
                    m_SecondaryWeapon.SetTrigger("Big");
                    m_MainWeapon.gameObject.SetActive(mainWeapon != null);
                }
            }
            else
            {
                if (currWeapon.GetPos() == 0)
                {
                    m_MainWeapon.SetTrigger("Big");
                    m_SecondaryWeapon.SetTrigger("Small");
                    m_SecondaryWeapon.gameObject.SetActive(secondaryWeapon != null);
                }
                else if (currWeapon.GetPos() == 1)
                {
                    m_MainWeapon.SetTrigger("Small");
                    m_SecondaryWeapon.SetTrigger("Big");
                    m_MainWeapon.gameObject.SetActive(mainWeapon != null);
                }
            }

            m_MainWeapon.gameObject.SetActive(mainWeapon != null);
            if (mainWeapon != null)
                UIUtil.SetIconImage(m_MainWeaponIcon, mainWeapon.GetBaseConfig().Icon);

            m_SecondaryWeaponIcon.gameObject.SetActive(secondaryWeapon != null);
            if (secondaryWeapon != null)
                UIUtil.SetIconImage(m_SecondaryWeaponIcon, secondaryWeapon.GetBaseConfig().Icon);
        }
        else
        {
            m_MainWeapon.gameObject.SetActive(false);
            m_SecondaryWeapon.gameObject.SetActive(false);
        }
    }
}
