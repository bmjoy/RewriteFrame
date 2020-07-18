using Eternity.Runtime.Item;
using PureMVC.Interfaces;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HudGetItemPanel : UIPanelBase
{
    private const string ASSET_ADDRESS = Leyoutech.Core.Loader.Config.AssetAddressKey.PRELOADUI_HUD_GETITEMPANEL;

    /// <summary>
    /// 条目的创建间隔
    /// </summary>
    private const float ITEM_CREATE_INTERVAL = 0.5f;

    /// <summary>
    /// 条目的存活时间
    /// </summary>
    private const float ITEM_LIFE_TIME = 5.0f;

    /// <summary>
    /// 条目的最多数量
    /// </summary>
    private const int ITEM_MAX_COUNT = 3;

    /// <summary>
    /// 每个条目的高度
    /// </summary>
    private const float ITEM_HEIGHT = 70.0f;


    /// <summary>
    /// 条目容器
    /// </summary>
    private RectTransform m_Root;

    /// <summary>
    /// 条目模板
    /// </summary>
    private RectTransform m_ItemTemplate;

    /// <summary>
    /// 协程
    /// </summary>
    private Coroutine m_Coroutine;


    /// <summary>
    /// 正在等待的条目队列
    /// </summary>
    private List<ItemData> m_WaitingQueue = new List<ItemData>();

    /// <summary>
    /// 正在展示的条目队列
    /// </summary>
    private List<ItemData> m_ShowingQueue = new List<ItemData>();

    /// <summary>
    /// 暂停
    /// </summary>
    private bool m_Paused = false;

    /// <summary>
    /// 上一次创建条目的时间
    /// </summary>
    private float m_LastCreateTime = 0.0f;


    public HudGetItemPanel() : base(UIPanel.HudGetItemPanel, ASSET_ADDRESS, PanelType.Hud) { }

    public override void Initialize()
    {
        m_Root = FindComponent<RectTransform>("Content");
        m_ItemTemplate = FindComponent<RectTransform>("Content/Item");
    }

    public override void OnShow(object msg)
    {
        base.OnShow(msg);

        m_ItemTemplate.gameObject.SetActive(false);

        m_Coroutine = UIManager.Instance.StartCoroutine(UpdateCoroutine());
    }

    public override void OnHide(object msg)
    {
        if (m_Coroutine != null)
            UIManager.Instance.StopCoroutine(m_Coroutine);

        for (int i = 0; i < m_ShowingQueue.Count; i++)
        {
            m_ShowingQueue[i].Transform.gameObject.SetActive(false);
        }

        m_WaitingQueue.Clear();
        m_ShowingQueue.Clear();

        base.OnHide(msg);
    }

    public override NotificationName[] ListNotificationInterests()
    {
        return new NotificationName[]
        {
            NotificationName.MSG_PACKAGE_ITEM_CHANGE,
			//NotificationName.MSG_BOTTOMFLOATING_PAUSEANDRECOVER,
		};
    }

    public override void HandleNotification(INotification notification)
    {
        switch ((NotificationName)notification.Name)
        {
            case NotificationName.MSG_PACKAGE_ITEM_CHANGE:
                OnGettingItem(notification.Body as ItemChangeInfo);
                break;
                //case NotificationName.MSG_BOTTOMFLOATING_PAUSEANDRECOVER:
                //	OnPuaseOrRecover();
                //	break;
        }
    }

    /// <summary>
    /// 添加条目
    /// </summary>
    /// <param name="itemGetting">条目数据</param>
    private void OnGettingItem(ItemChangeInfo itemGetting)
    {
        if (itemGetting.CountChangeDelta > 0 && itemGetting.TID > 0)
        {
            if (itemGetting.Category != 0 && itemGetting.Category != Category.Package)
            {
                ItemData data = new ItemData();
                data.Name = TableUtil.GetItemName(itemGetting.TID);
                data.Quality = TableUtil.GetItemQuality(itemGetting.TID);
                data.Count = itemGetting.CountChangeDelta;
                data.IconBundle = TableUtil.GetItemIconBundle(itemGetting.TID);
                data.IconName = TableUtil.GetItemSquareIconImage(itemGetting.TID);

                m_WaitingQueue.Add(data);
            }
        }
    }

    /// <summary>
    /// 暂停/恢复
    /// </summary>
    private void OnPuaseOrRecover()
    {
        m_Paused = !m_Paused;
    }

    /// <summary>
    /// 协程函数
    /// </summary>
    /// <returns></returns>
    private IEnumerator UpdateCoroutine()
    {
        yield return new WaitForSeconds(5.0f);

        while (true)
        {
            UpdateItems();

            yield return new WaitForEndOfFrame();
        }
    }

    /// <summary>
    /// 更新所有条目
    /// </summary>
    private void UpdateItems()
    {
        float now = Time.time;
        if (!m_Paused && m_WaitingQueue.Count > 0)
        {
            if (now >= m_LastCreateTime + ITEM_CREATE_INTERVAL)
            {
                m_LastCreateTime = now;

                CfgLanguageProxy languageProxy = Facade.RetrieveProxy(ProxyName.CfgLanguageProxy) as CfgLanguageProxy;

                ItemData data = m_WaitingQueue[0];
                m_WaitingQueue.RemoveAt(0);

                Transform item = null;

                for (int i = 0; i < m_Root.childCount; i++)
                {
                    if (!m_Root.GetChild(i).gameObject.activeSelf)
                    {
                        item = m_Root.GetChild(i);
                        break;
                    }
                }

                if (item == null)
                {
                    item = GameObject.Instantiate(m_ItemTemplate, m_Root);
                }

                Image quality1 = item.Find("Quality/Quality_1").GetComponent<Image>();
                Image quality2 = item.Find("Quality_2").GetComponent<Image>();
                Image quality3 = item.Find("Quality_3").GetComponent<Image>();
                Image quality4 = item.Find("Quality_4").GetComponent<Image>();
                Image quality5 = item.Find("Quality_5").GetComponent<Image>();
                Image icon = item.Find("Image_Icon").GetComponent<Image>();
                TMP_Text text = item.Find("Label_IconName").GetComponent<TMP_Text>();
                Animator animator = item.GetComponent<Animator>();

                item.SetAsLastSibling();
                item.gameObject.SetActive(true);
                Color color = ColorUtil.GetColorByItemQuality(data.Quality);
                quality1.color = quality2.color = quality3.color = quality4.color = quality5.color = color;
                string name = ColorUtil.AddColor(data.Name, color);
                text.text = string.Format(TableUtil.GetLanguageString("hud_text_id_1018"), name, data.Count.ToString());

                UIUtil.SetIconImage(icon, data.IconBundle, data.IconName);

                data.CreateTime = now;
                data.Transform = item;
                data.IsNew = true;
                m_ShowingQueue.Add(data);
            }
        }

        while (m_ShowingQueue.Count > ITEM_MAX_COUNT)
        {
            ItemData first = m_ShowingQueue[0];
            first.Transform.SetAsLastSibling();
            first.Transform.gameObject.SetActive(false);
            first.Transform = null;

            m_ShowingQueue.RemoveAt(0);
        }

        float y = 0;
        for (int i = m_ShowingQueue.Count - 1; i >= 0; i--)
        {
            ItemData data = m_ShowingQueue[i];
            if (data.CreateTime + ITEM_LIFE_TIME < now)
            {
                data.Transform.SetAsLastSibling();
                data.Transform.gameObject.SetActive(false);
                data.Transform = null;

                m_ShowingQueue.RemoveAt(i);
            }
            else
            {
                Vector3 position = data.Transform.localPosition;
                if (data.IsNew)
                {
                    data.Transform.localPosition = new Vector3(position.x, y, position.z);
                    data.IsNew = false;
                }
                else
                {
                    data.Transform.localPosition = new Vector3(position.x, Mathf.Lerp(position.y, y, 0.5f), position.z);
                }

                y += ITEM_HEIGHT;
                if (m_ShowingQueue.Count - 1 - i == 1)
                {
                    data.Transform.GetComponent<CanvasGroup>().alpha = 0.4f;
                }
                else if (m_ShowingQueue.Count - 1 - i == 0)
                {
                    data.Transform.GetComponent<CanvasGroup>().alpha = 1f;
                }
                else if (m_ShowingQueue.Count - 1 - i == 2)
                {
                    data.Transform.GetComponent<CanvasGroup>().alpha = 0.2f;
                }
            }
        }
    }

    private class ItemData
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name;

        /// <summary>
        ///品质
        /// </summary>
        public int Quality;

        /// <summary>
        /// 数量
        /// </summary>
        public long Count;

        /// <summary>
        /// 图标Bundle
        /// </summary>
        public string IconBundle;

        /// <summary>
        /// 图标Name
        /// </summary>
        public string IconName;

        /// <summary>
        /// 创建时间
        /// </summary>
        public float CreateTime;

        /// <summary>
        /// 显示对象
        /// </summary>
        public Transform Transform;

        /// <summary>
        /// 是否是一个新的
        /// </summary>
        public bool IsNew;
    }
}
