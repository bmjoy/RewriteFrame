using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ServerListPanel : CompositeView
{
    /// <summary>
    /// ��ǰѡ����������ڵ�
    /// </summary>
    private Transform m_CurrentServerRoot;
    /// <summary>
	/// CellElementPath
	/// </summary>
	private const string ELEMENT_PREFAB_ASSET_ADDRESS = "Assets/Artwork/UI/Prefabs/Element/ServerListPanelElement.prefab";
    /// <summary>
	/// cellPrefab
	/// </summary>
	private GameObject m_ListCellPrefab;
    /// <summary>
	/// ��ǰѡ�������
	/// </summary>
	public Transform m_CurrentServer;
    /// <summary>
	/// ��ǰѡ�����������Text
	/// </summary>
	public TMP_Text m_CurrentServerNameText;
    /// <summary>
    /// ��ǰѡ�������״̬
    /// </summary>
    public TMP_Text m_CurrentServerStateText;
    public ServerListPanel() : base(UIPanel.ServerListPanel, PanelType.Normal)
    {

	}

    public override void Initialize()
    {
        base.Initialize();
        m_CurrentServerRoot = FindComponent<Transform>("Content/Other/CurrentSever/Current");
        if (m_ListCellPrefab == null)
        {
            UIManager.Instance.GetUIElement(ELEMENT_PREFAB_ASSET_ADDRESS, (GameObject prefab) =>
            {
                m_ListCellPrefab = prefab;
                m_ListCellPrefab.CreatePool(1, ELEMENT_PREFAB_ASSET_ADDRESS);
                m_CurrentServer = m_ListCellPrefab.Spawn(m_CurrentServerRoot).transform;
                m_CurrentServer.localScale = Vector3.one;
                m_CurrentServer.GetComponent<Toggle>().interactable = false;
                m_CurrentServerNameText = m_CurrentServer.Find("Content/ServerField").GetComponent<TMP_Text>();
                m_CurrentServerStateText = m_CurrentServer.Find("Content/StateField").GetComponent<TMP_Text>();
            });
        }       
    }
}

