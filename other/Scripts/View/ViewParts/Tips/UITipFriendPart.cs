using PureMVC.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class UITipFriendPart : UITipItemPart
{
    private const string TIP_PREFAB = Leyoutech.Core.Loader.Config.AssetAddressKey.PRELOADUI_TIPSFRIENDSPANEL;

    /// <summary>
    /// 玩家名字
    /// </summary>
    private TextMeshProUGUI m_PlayerName;
    /// <summary>
    /// 玩家等级
    /// </summary>
    private TextMeshProUGUI m_Level;
    /// <summary>
    /// 玩家排行榜
    /// </summary>
    private TextMeshProUGUI m_Rank;
    /// <summary>
    /// 玩家名字
    /// </summary>
    private TextMeshProUGUI m_ShipLevel;
    /// <summary>
    /// 玩家战舰等级
    /// </summary>
    private TextMeshProUGUI m_GuildName;
    /// <summary>
    /// 玩家区域
    /// </summary>
    private TextMeshProUGUI m_Position;
    /// <summary>
    /// 玩家战舰
    /// </summary>
    private TextMeshProUGUI m_Wraship;
    /// <summary>
    /// 玩家武器A
    /// </summary>
    private TextMeshProUGUI m_WeaponA;
    /// <summary>
    /// 玩家武器B
    /// </summary>
    private TextMeshProUGUI m_WeaponB;
    /// <summary>
    /// 玩家转化炉
    /// </summary>
    private TextMeshProUGUI m_Converter;
    /// <summary>
    /// 玩家装置处理器
    /// </summary>
    private TextMeshProUGUI m_Processor;
    /// <summary>
    /// 玩家装置机器人
    /// </summary>
    private TextMeshProUGUI m_Nanobot;
    /// <summary>
    /// 玩家装置装甲涂层
    /// </summary>
    private TextMeshProUGUI m_ArmorCoating;
    /// <summary>
    /// 玩家装置辅助单元
    /// </summary>
    private TextMeshProUGUI m_AuxiliaryUnit;
    /// <summary>
    ///玩家装置反应堆
    /// </summary>
    private TextMeshProUGUI m_Reactor;
    /// <summary>
    /// 玩家装置放大器
    /// </summary>
    private TextMeshProUGUI m_Amplifier;
    /// <summary>
    /// 武器集合
    /// </summary>
    private TextMeshProUGUI[] m_Weapons;
    /// <summary>
    /// 装置集合
    /// </summary>
    private TextMeshProUGUI[] m_EquipMents;
    /// <summary>
    /// 船的信息 
    /// </summary>
    private IShip m_Ship;

    /// <summary>
    /// TIP的Prefab
    /// </summary>
    private GameObject m_TipPrefab;
    /// <summary>
    /// Tip的Prefab是否在加载中
    /// </summary>
    private bool m_TipPrefabLoading;

    /// <summary>
    /// Tip1
    /// </summary>
    private GameObject m_TipInstance1;
    /// <summary>
    /// 空tips
    /// </summary>
    private GameObject m_EmptyTipsObj;

    public override void OnShow(object msg)
    {
        base.OnShow(msg);

        m_TipPrefab = null;
        m_TipPrefabLoading = false;
    }

    public override void OnHide()
    {
        m_TipPrefab = null;
        m_TipPrefabLoading = false;
        m_EmptyTipsObj = null;
        base.OnHide();
    }
    public void Initialize()
    {
        m_PlayerName = FindComponent<TextMeshProUGUI>("TipsScrollView/Viewport/Content/Content/NameType/Label_Name");
        m_Level = FindComponent<TextMeshProUGUI>("TipsScrollView/Viewport/Content/Content/NameType/Label_Lv");
        m_Rank = FindComponent<TextMeshProUGUI>("TipsScrollView/Viewport/Content/Content/NameType/Label_Rank");
        m_ShipLevel = FindComponent<TextMeshProUGUI>("TipsScrollView/Viewport/Content/Content/NameType/Label_Name");
        m_GuildName = FindComponent<TextMeshProUGUI>("TipsScrollView/Viewport/Content/Content/NameType/Type/Label_Type");
        m_Position = FindComponent<TextMeshProUGUI>("TipsScrollView/Viewport/Content/Content/Position/Label_Describe");
        m_Wraship = FindComponent<TextMeshProUGUI>("TipsScrollView/Viewport/Content/Content/Warship/Label_Name");
        m_WeaponA = FindComponent<TextMeshProUGUI>("TipsScrollView/Viewport/Content/Content/Weapon/Label_Weapon1");
        m_WeaponB = FindComponent<TextMeshProUGUI>("TipsScrollView/Viewport/Content/Content/Weapon/Label_Weapon2");
        m_Converter = FindComponent<TextMeshProUGUI>("TipsScrollView/Viewport/Content/Content/Converter/Label_Name");
        m_Processor = FindComponent<TextMeshProUGUI>("TipsScrollView/Viewport/Content/Content/Processor/Label_Name");
        m_Nanobot = FindComponent<TextMeshProUGUI>("TipsScrollView/Viewport/Content/Content/Nanobot/Label_Name");
        m_ArmorCoating = FindComponent<TextMeshProUGUI>("TipsScrollView/Viewport/Content/Content/ArmorCoating/Label_Name");
        m_AuxiliaryUnit = FindComponent<TextMeshProUGUI>("TipsScrollView/Viewport/Content/Content/AuxiliaryUnit/Label_Name");
        m_Reactor = FindComponent<TextMeshProUGUI>("TipsScrollView/Viewport/Content/Content/Reactor/Label_Name");
        m_Amplifier = FindComponent<TextMeshProUGUI>("TipsScrollView/Viewport/Content/Content/Amplifier/Label_Name");
        m_Weapons = new TextMeshProUGUI[] { m_WeaponA, m_WeaponB };
        m_EquipMents = new TextMeshProUGUI[] { m_Processor, m_Nanobot, m_ArmorCoating, m_AuxiliaryUnit, m_Reactor, m_Amplifier };
    }

    /// <summary>
    /// 清除视图
    /// </summary>
    protected override void CloseTipView()
    {
        if (m_TipInstance1)
        {
            m_TipInstance1.Recycle();
            m_TipInstance1 = null;
        }
      
        base.CloseTipView();
       
    }

    public override NotificationName[] ListNotificationInterests()
    {
        return new NotificationName[]
        {
            NotificationName.MSG_FRIEND_SHIPDATA_CHANGE,
        };
    }

    /// <summary>
    /// <see cref="UIPanelBase.HandleNotification(INotification)"/>
    /// </summary>
    public override void HandleNotification(INotification notification)
    {
        switch ((NotificationName)notification.Name)
        {
            case NotificationName.MSG_FRIEND_SHIPDATA_CHANGE:
                GetShipData((ulong)notification.Body);
                break;
            default:
                break;
        }
    }


    /// <summary>
    /// 更新Tip视图
    /// </summary>
    /// <param name="data">数据</param>
    protected override void OpenTipView()
    {
        if (TipData is FriendInfoVO)
            OpenTip(TipData as FriendInfoVO);
        else if (TipData is TeamMemberVO)
            OpenTip(TipData as TeamMemberVO);
        else 
            base.OpenTipView();
    }

    /// <summary>
    /// 打开Tip
    /// </summary>
    private void OpenTip(FriendInfoVO data)
    {
        if (m_TipPrefab)
        {
            if (!m_TipInstance1)
            {
                m_TipInstance1 = m_TipPrefab.Spawn(TipBoxLeft);
                m_TipInstance1.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            }

            LayoutFriendTip(m_TipInstance1, data);
        }
        else if (!m_TipPrefabLoading)
        {
            m_TipPrefabLoading = true;

            LoadPrefabFromPool(TIP_PREFAB, (prefab) =>
            {
                if (Opened)
                {
                    m_TipPrefab = prefab;

                    OpenTipView();
                }
            });
        }
    }

    /// <summary>
    /// 打开Tip
    /// </summary>
    private void OpenTip(TeamMemberVO data)
    {
        if (m_TipPrefab)
        {
            if (!m_TipInstance1)
            {
                m_TipInstance1 = m_TipPrefab.Spawn(TipBoxLeft);
                m_TipInstance1.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            }

            LayoutFriendTip(m_TipInstance1, data);
        }
        else if (!m_TipPrefabLoading)
        {
            m_TipPrefabLoading = true;

            LoadPrefabFromPool(TIP_PREFAB, (prefab) =>
            {
                if (Opened)
                {
                    m_TipPrefab = prefab;
                    OpenTipView();
                }
            });
        }

    }

    /// <summary>
    /// 加载好友数据
    /// </summary>
    /// <param name="tip">tip物体</param>
    /// <param name="data">数据</param>
    private void LayoutFriendTip(GameObject tip, FriendInfoVO data)
    {
        if (data.UID > 0)
        {
            SetEmptyRootActive(false);
            Initialize();
            m_PlayerName.text = data.Name;
            m_Level.text = string.Format(TableUtil.GetLanguageString("character_text_1019"), data.Level.ToString());
        }
        else
        {
            //SetEmptyRootActive(true);
        }
    }

    /// <summary>
    /// 加载队伍数据
    /// </summary>
    /// <param name="tip">tip物体</param>
    /// <param name="data">数据</param>
    private void LayoutFriendTip(GameObject tip, TeamMemberVO data)
    {
        if (data.UID > 0)
        {
            SetEmptyRootActive(false);
            Initialize();
            m_PlayerName.text = data.Name;
            m_Level.text = string.Format(TableUtil.GetLanguageString("character_text_1019"), data.Level.ToString());
            m_Rank.text = string.Format(TableUtil.GetLanguageString("social_text_1004"), data.DanLevel.ToString());
        }
        else
        {
            SetEmptyRootActive(true);
        }
       
    }

    /// <summary>
	/// 获取其他玩家的信息
	/// </summary>
	/// <param name="id"></param>
	private void GetShipData(ulong id)
    {
        //Debug.LogError("收到玩家信息"+id);
        ShipItemsProxy shipItemsProxy = GameFacade.Instance.RetrieveProxy(ProxyName.ShipItemsProxy) as ShipItemsProxy;
        // 飞船的
        IShip ship = shipItemsProxy.GetCurrentWarShip(id);

        if (ship != null)
        {
            IWeaponContainer weaponContainer = ship.GetWeaponContainer();
            if (weaponContainer != null)
            {
                IWeapon[] weapons = weaponContainer.GetWeapons();
                //Debug.LogError("weapons==="+weapons.Length);
                if (weapons != null)
                {
                    for (int i = 0; i < weapons.Length; i++)
                    {
                        IWeapon weapon = weapons[i];
                        m_Weapons[i].text = TableUtil.GetItemName((int)weapon.GetBaseConfig().Id);
                    }
                }
            }

            IEquipmentContainer equipmentContainer = ship.GetEquipmentContainer();
            if (equipmentContainer != null)
            {
                IEquipment[] equipments = ship.GetEquipmentContainer().GetEquipments();
                for (int i = 0; i < equipments.Length; i++)
                {
                    IEquipment equipment = equipments[i];
                    m_EquipMents[i].text = TableUtil.GetItemName((int)equipment.GetBaseConfig().Id);
                }
            }
            IReformer ireformer = ship.GetReformerContainer().GetReformer();
            if (ireformer != null)
            {
                m_Converter.text = TableUtil.GetItemName((int)ireformer.GetBaseConfig().Id);
            }
        }
        else
        {
            Debug.LogError("船的信息为空");
        }
    }

    /// <summary>
    /// 设置社交空tips显示
    /// </summary>
    public void SetEmptyRootActive(bool isActive)
    {
        if (isActive)
        {
            m_TipInstance1.transform.GetChild(0).gameObject.SetActive(false);
            if (m_EmptyTipsObj == null)
            {
                m_EmptyTipsObj = m_TipInstance1.transform.GetChild(1).gameObject;
            }
            m_EmptyTipsObj.SetActive(true);
        }
        else
        {
            m_TipInstance1.transform.GetChild(0).gameObject.SetActive(true);
            if (m_EmptyTipsObj != null)
            {
                m_EmptyTipsObj.SetActive(false);
            }
            // UIManager.Instance.ClosePanel(PanelName.TipsEmptyPanel);
        }

    }

    /// <summary>
    /// 查找指定节点下的组件
    /// </summary>
    /// <param name="path">相对节点的相对路径</param>
    /// <returns>对应组件</returns>
    protected new T FindComponent<T>(string path)
    {
        Transform result = m_TipInstance1.transform.Find(path);
        if (result)
        {
            return result.GetComponent<T>();
        }
        return default;
    }
}