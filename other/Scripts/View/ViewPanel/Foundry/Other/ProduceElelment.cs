using Eternity.FlatBuffer;
using Eternity.Runtime.Item;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 生产中的Elelment，里面显示的内容总体一样，只是有的有品质，有的没有品质等情况
/// </summary>
public class ProduceElelment :MonoBehaviour
{
	#region 变量

	/// <summary>
	/// 实例ID
	/// </summary>
	private ulong m_UID;

	/// <summary>
	/// 数据表的ID
	/// </summary>
	private int m_TID;

	/// <summary>
	/// 生产服务器数据
	/// </summary>
	private FoundryProxy m_FoundryProxy;

	/// <summary>
	/// 当前生产类型
	/// </summary>
	private ProduceType m_ProduceType;

	/// <summary>
	/// 当前生产状态
	/// </summary>
	private int m_ProduceState;

	/// <summary>
	/// 是否更新 倒计时
	/// </summary>
	private bool IsUpdateTime;

	/// <summary>
	/// 生产还需要时间
	/// </summary>
	private ulong m_Needtime = 0;

	/// <summary>
	/// 最大生产时间
	/// </summary>
	private ulong m_MaxTime = 0;

	/// <summary>
	/// 生产状态进度
	/// </summary>
	private float m_Progress;

	/// <summary>
	/// 生产状态进度条根节点
	/// </summary>
	private Image m_ProgressRootImage;

	/// <summary>
	/// 蓝图图标
	/// </summary>
	private Image m_IconImage;

	/// <summary>
	/// 第一次获取标记
	/// </summary>
	private Image m_FirstSignImage;

	/// <summary>
	/// 蓝图名字
	/// </summary>
	private TextMeshProUGUI m_BlueprintNameLabel;

	/// <summary>
	/// 蓝图产出数量
	/// </summary>
	private TextMeshProUGUI m_BlueprintNumberLabel;

	/// <summary>
	/// 蓝图生产状态
	/// </summary>
	private TextMeshProUGUI m_BlueprintStateLabel;

	/// <summary>
	/// 蓝图生产状态
	/// </summary>
	private Image m_BlueprintQuality;

    /// <summary>
    /// 成品类型
    /// </summary>
    private Image m_TypeImage;

	/// <summary>
	/// Prime属性文字
	/// </summary>
	private TextMeshProUGUI m_Prime;

	/// <summary>
	/// 产品道具
	/// </summary>
	private Item m_CurrentItem;

	/// <summary>
	/// 蓝图数据
	/// </summary>
	private Produce m_CurrentProduce;

	/// <summary>
	/// 当前数据
	/// </summary>
	private ProduceInfoVO m_ProduceInfoVO;

	/// <summary>
	/// 叠加icon图片
	/// </summary>
	private Image m_OverlyingIcon;

    /// <summary>
    /// 面板类型（格子，条目）
    /// </summary>
    private UIViewListLayout m_Style;

    #endregion

    public void Initialize()
	{
	    m_FoundryProxy = (FoundryProxy)GameFacade.Instance.RetrieveProxy(ProxyName.FoundryProxy);
		m_ProgressRootImage = TransformUtil.FindUIObject<Image>(transform, "Content/Time");
		m_IconImage = TransformUtil.FindUIObject<Image>(transform, "Content/Image_Icon");
		m_OverlyingIcon = TransformUtil.FindUIObject<Image>(transform, "Content/Image_Icon2");
		m_BlueprintNameLabel = TransformUtil.FindUIObject<TextMeshProUGUI>(transform, "Content/Mask/Label_Name");
		m_BlueprintStateLabel = TransformUtil.FindUIObject<TextMeshProUGUI>(transform, "Content/Label_State");
		m_BlueprintQuality = TransformUtil.FindUIObject<Image>(transform, "Content/Image_Quality");
		m_TypeImage = TransformUtil.FindUIObject<Image>(transform, "Content/Mask/Type/Image_type");
		m_Prime = TransformUtil.FindUIObject<TextMeshProUGUI>(transform, "Content/Mask/Type/Label_Prime");
        m_BlueprintNumberLabel = TransformUtil.FindUIObject<TextMeshProUGUI>(transform, "Content/Label_Num");


    }

	/// <summary>
	/// 填充数据
	/// </summary>
	public void SetData(ProduceInfoVO infoVO, ProduceType produceType, UIViewListLayout style = UIViewListLayout.Row)
	{
		if (infoVO.TID > 0)
		{
            m_Style = style;
            m_TID = infoVO.TID;
			m_ProduceType = produceType;
			m_CurrentItem = m_FoundryProxy.GetItemByProduceKey(m_TID);
			m_CurrentProduce = m_FoundryProxy.GetProduceByKey(m_TID);
			m_ProduceInfoVO = infoVO;
			RefreshData(infoVO.Progress, infoVO.BluePrintState);
			SetContent();
		}
		
	}

	private void OnEnable()
	{
		ServerTimeUtil.Instance.OnTick += UpdateTime;
	}

	/// <summary>
	/// 刷新数据
	/// </summary>
	public void RefreshData(float progress, ProduceState State)
	{
		progress = Mathf.Clamp01(progress);
		m_Progress = progress == 0 ? 0f : progress;
		m_ProgressRootImage.fillAmount = m_Progress;
		GetProduceTime(State);
		UpdateTime();
	}

	/// <summary>
	/// 获取蓝图ID
	/// </summary>
	/// <returns></returns>
	public int GetTID()
	{
		return m_TID;
	}

	/// <summary>
	/// 获取当前生产数据
	/// </summary>
	/// <returns></returns>
	public Produce GetProduce()
	{
		return m_CurrentProduce;
	}

	/// <summary>
	/// 根据类型初始化面板内容
	/// </summary>
	private void SetContent()
	{
		m_Prime.gameObject.SetActive(false);
		m_BlueprintNameLabel.text = TableUtil.GetItemName(m_CurrentProduce.Id);
		m_BlueprintQuality.color =ColorUtil.GetColorByItemQuality(TableUtil.GetItemQuality(m_CurrentItem.Id));
        m_BlueprintNumberLabel.text = m_CurrentProduce.ProductNum.ToString();
        string iconName="";
        if (m_Style== UIViewListLayout.Row)
            iconName = TableUtil.GetItemIconImage(m_CurrentItem.Id);
        else
		    iconName = TableUtil.GetItemSquareIconImage(m_CurrentItem.Id);

		UIUtil.SetIconImage(m_IconImage, TableUtil.GetItemIconBundle(m_CurrentItem.Id), iconName);
		m_OverlyingIcon.sprite = m_IconImage.sprite;
		switch (m_ProduceType)
		{
			case ProduceType.HeavyWeapon:
			case ProduceType.LightWeapon:
				WeaponL2 weaponL2 = 0;
				ItemTypeUtil.SetSubType(ref weaponL2, ItemTypeUtil.GetItemType(m_CurrentItem.Type));
				m_Prime.text = TableUtil.GetLanguageString(weaponL2);
				m_Prime.gameObject.SetActive(true);
				break;
		}
	}

	/// <summary>
	/// 获取生产时间
	/// </summary>
	/// <param name="finish"> 生产状态是否为完成</param>
	private void GetProduceTime(ProduceState state)
	{
		if (state == ProduceState.Producing)
		{
			ProduceInfoVO foundryMember = m_FoundryProxy.GetFoundryMemberByTID(m_TID);
			if (foundryMember == null)
			{
				IsUpdateTime = false;
				return;
			}
			m_MaxTime = foundryMember.EndTime - foundryMember.StartTime;
			m_Needtime = foundryMember.EndTime - foundryMember.StartTime - foundryMember.SpendTime;
			if (m_Needtime <= 0 || m_MaxTime <= 0)
			{
				return;
			}
			IsUpdateTime = true;
		}
		else
		{
			IsUpdateTime = false;
			if (state == ProduceState.CanNotProduce)
			{
				m_BlueprintStateLabel.text = TableUtil.GetLanguageString("production_title_1060");
			}
			else if (state == ProduceState.CanProduce)
			{
				m_BlueprintStateLabel.text = TableUtil.GetLanguageString("production_title_1061");
			}
			else if (state == ProduceState.Finsh)
			{
				m_BlueprintStateLabel.text = TableUtil.GetLanguageString("production_title_1059");
			}
            else if (state == ProduceState.Have)
            {
                m_BlueprintStateLabel.text = TableUtil.GetLanguageString("production_text_1045");
            }
        }
	}

	/// <summary>
	/// 进度条刷新
	/// </summary>
	private void UpdateTime()
	{
		if (IsUpdateTime)
		{
			if (m_Needtime <= 0)
			{
				IsUpdateTime = false;
			}
			IsUpdateTime = true;
			m_Needtime--;
			float prog = m_MaxTime <= 0 ? 0 : (m_MaxTime - m_Needtime) / (float)m_MaxTime;
			if (prog >= 1)
			{
				m_ProgressRootImage.fillAmount = 0;
			}
			m_Progress = prog;
			string str = TimeUtil.GetTimeStr((long)m_Needtime);
			m_BlueprintStateLabel.text =string.Format(TableUtil.GetLanguageString("production_title_1058"), str);
			m_ProgressRootImage.fillAmount = m_Progress;
			ProduceInfoVO gird = null;
			if (m_FoundryProxy.GetBluePrintDic().TryGetValue(m_TID, out gird))
			{
				gird.BluePrintState = prog >= 1 ? ProduceState.Finsh : gird.BluePrintState;
				gird.Progress = prog;

				if (gird.BluePrintState == ProduceState.Finsh)
				{
					RefreshData(1f, ProduceState.Finsh);
					GameFacade.Instance.SendNotification(NotificationName.MSG_PRODUT_UPDATE, gird.BluePrintState);
				}
			}
		}

	}

	/// <summary>
	/// 关闭面板调用
	/// </summary>
	private void OnDisable()
	{
		if (ServerTimeUtil.Instance)
		{
			ServerTimeUtil.Instance.OnTick -= UpdateTime;
		}
	}
}
