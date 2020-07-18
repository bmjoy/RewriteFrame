using Eternity.FlatBuffer;
using Leyoutech.Core.Loader.Config;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EscRoleElement : MonoBehaviour
{
    /// <summary>
    /// 角色模型
    /// </summary>
    private RawImage m_CharacterModel;
    /// <summary>
    /// serverProxy
    /// </summary>
    private ServerListProxy m_ServerListProxy;
    /// <summary>
    /// EternityProxy  
    /// </summary>
    public CfgEternityProxy m_CfgEternityProxy;
    /// <summary>
    /// 手表升级提示
    /// </summary>
    private Transform m_UpaTips;
    /// <summary>
    /// 名字
    /// </summary>
    private TMP_Text m_Label;
    public void Init()
    {
        if (m_CharacterModel == null)
        {
            m_CharacterModel = transform.Find("Content/NPC").GetComponent<RawImage>();
        }
        if (m_ServerListProxy == null)
        {
            m_ServerListProxy = (ServerListProxy)GameFacade.Instance.RetrieveProxy(ProxyName.ServerListProxy);
        }
        if (m_CfgEternityProxy == null)
        {
            m_CfgEternityProxy = (CfgEternityProxy)GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy);
        }
        if (m_UpaTips == null)
        {
            m_UpaTips = transform.Find("Content/Content/Message");
        }
        if (m_Label == null)
        {
            m_Label = transform.Find("Content/Content/Label_Name").GetComponent<TMP_Text>();
        }
        m_CharacterModel.gameObject.SetActive(true);
        m_Label.text = TableUtil.GetLanguageString("esc_title_1002");
        ShowUpaTips();
        ShowCharacter();
    }
    /// <summary>
    /// 显示角色模型
    /// </summary>
    public void ShowCharacter()
    {
        int tid = m_ServerListProxy.GetCurrentCharacterVO().Tid;
        if (m_CfgEternityProxy.GetPlayerByItemTId(tid) != null)
        {
            Model m_Model = m_CfgEternityProxy.GetItemModelByKey((uint)tid);
            Effect3DViewer m_npc3DViewer = m_CharacterModel.GetOrAddComponent<Effect3DViewer>();
            if (m_npc3DViewer != null)
            {             
                m_npc3DViewer.ClearModel();
                m_npc3DViewer.LoadModel
                    (AssetAddressKey.PRELOADUI_UI3D_ESCCHARACTERPANEL,
                    m_Model.AssetName,
                    AssetAddressKey.FX_UI_CHARACTER_SCANNING,
                    GetNpcPos(m_Model),
                    GetNpcRotation(m_Model),
                    GetNpcScale(m_Model));
            }
           
        }
        else
        {
            Debug.Log("tid错误" + tid);
        }
    }
    /// <summary>
    /// 显示手表升级提示
    /// </summary>
    private void ShowUpaTips()
    {
        PlayerInfoVo player = NetworkManager.Instance.GetPlayerController().GetPlayerInfo();
        double exp = m_CfgEternityProxy.GetPlayerUpa(player.WatchLv).Exp;
        if (player.WatchExp >= exp && exp > 0)
        {
            m_UpaTips.gameObject.SetActive(true);
        }
        else
        {
            m_UpaTips.gameObject.SetActive(false);
        }
    }
    /// <summary>
    /// 获取模型位置
    /// </summary>
    /// <param name="NpcTemplateVO"></param>
    /// <returns></returns>
    public Vector3 GetNpcPos(Model NpcTemplateVO)
    {
        if (NpcTemplateVO.EscPositionLength == 3)
        {
            return new Vector3(NpcTemplateVO.EscPosition(0), NpcTemplateVO.EscPosition(1), NpcTemplateVO.EscPosition(2));
        }
        return Vector3.zero;
    }
    /// <summary>
    /// 获取模型旋转角度
    /// </summary>
    /// <param name="NpcTemplateVO"></param>
    /// <returns></returns>
    public Vector3 GetNpcRotation(Model NpcTemplateVO)
    {
        if (NpcTemplateVO.EscRotationLength == 3)
        {
            return new Vector3(NpcTemplateVO.EscRotation(0), NpcTemplateVO.EscRotation(1), NpcTemplateVO.EscRotation(2));
        }
        return Vector3.zero;
    }
    /// <summary>
    /// 获取模型缩放大小
    /// </summary>
    /// <param name="NpcTemplateVO"></param>
    /// <returns></returns>
    public Vector3 GetNpcScale(Model NpcTemplateVO)
    {
        if (NpcTemplateVO.EscScale > 0)
        {
            return NpcTemplateVO.EscScale * Vector3.one;
        }
        return Vector3.one;
    }   
}
