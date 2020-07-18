using crucis.attributepipeline.attribenum;
using Crucis.Protocol;
using Crucis.Protocol.GameSession;
using Eternity.FlatBuffer;
using PureMVC.Patterns.Proxy;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using pbc = global::Google.Protobuf.Collections;
public class TalentProxy : Proxy
{
    /// <summary>
    /// EternityProxy
    /// </summary>
    private CfgEternityProxy m_CfgEternityProxy;

    /// <summary>
    /// 天赋点容器
    /// </summary>
    private Dictionary<uint, TalentVO> m_TalentVODic = new Dictionary<uint, TalentVO>();

    /// <summary>
    /// 天赋树容器
    /// </summary>
    private Dictionary<int, TalentTypeVO> m_TalentRootVODic = new Dictionary<int, TalentTypeVO>();

    /// <summary>
    /// 服务器返回数据
    /// </summary>
    private pbc::RepeatedField<TalentNode> m_TalentNodes = new pbc.RepeatedField<TalentNode>();

    /// <summary>
    /// 天赋点容器
    /// </summary>
    /// <returns></returns>
    public Dictionary<uint, TalentVO> GetTalentVODic()
    {
        return m_TalentVODic;
    }

    /// <summary>
    /// 天赋树容器
    /// </summary>
    /// <returns></returns>
    public Dictionary<int, TalentTypeVO> GetTalentRootVODic()
    {
        return m_TalentRootVODic;
    }

    public TalentProxy() : base(ProxyName.TalentProxy)
    {
        m_CfgEternityProxy = Facade.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
    }

    /// <summary>
    /// 添加数据
    /// </summary>
    /// <param name="id"></param>
    public void AddTalent(uint id,TalentVO talentVO)
    {
        if (m_TalentVODic.ContainsKey(id))
        {
            m_TalentVODic[id] = talentVO;
        }
        else
        {
            m_TalentVODic.Add(id,talentVO);
        }
    }

    /// <summary>
    /// 添加数据
    /// </summary>
    /// <param name="id"></param>
    public void AddData(uint id)
    {
        int m_TalentLength = m_CfgEternityProxy.GetTalentNodeLength(id);
        for (int i = 0; i < m_TalentLength; i++)
        {
            TalentSubNode? talentSubNode = m_CfgEternityProxy.GetTalentSubNodeByIndex(id, i);
            TalentVO talentVO = new TalentVO();
            if (talentSubNode.HasValue)
            {
                if (m_TalentVODic.TryGetValue(talentSubNode.Value.Id, out talentVO))
                {
                    SetTalentState(talentVO);
                }
                else
                {
                    talentVO = new TalentVO();
                    talentVO.TalentRootId = id;
                    talentVO.MTalentSubNode = talentSubNode;
                    talentVO.Id = (int)talentSubNode.Value.Id;
                    talentVO.UnLockId = (int)talentSubNode.Value.UnlockCondition;
                    talentVO.IconId = talentSubNode.Value.Icon;
                    talentVO.BackNodeId = (int)talentSubNode.Value.PreNode;
                    talentVO.MaxLevel = m_CfgEternityProxy.GetTalentMaxLevel((uint)talentVO.Id);
                    talentVO.EffectNum = m_CfgEternityProxy.GetUpLevelCost((uint)talentVO.Id,talentVO.Level);
                    SetTalentState(talentVO);
                    AddTalent(talentSubNode.Value.Id, talentVO);

                }
                //talentVO.Level = 0;

            }
        }
    }

    /// <summary>
    /// 重置等级数据
    /// </summary>
    /// <param name="id"></param>
    public void ResetLevelData(uint id)
    {
        foreach (var item in m_TalentVODic.Values)
        {
            item.Level = 0;
        }
      
    }


    #region  服务器通信
    /// <summary>
    /// 获取天赋信息
    /// </summary>
    /// <param name="id">tid</param>
    public async void GetTalentInfos(ulong id)
    {
        Debug.Log("------->>>>>发送获取天赋信息"+id);
        GetTalentInfosResponse getTalentInfosResponse = await GetTalentInfosRPC.GetTalentInfos(id);
        Debug.Log("------->>>>>接收天赋信息" + getTalentInfosResponse.Success);
        if (getTalentInfosResponse.Success != null)
        {
            m_TalentNodes.Clear();
            m_TalentNodes = getTalentInfosResponse.Success.Success_.Nodes;
            for (int i = 0; i < m_TalentNodes.Count; i++)
            {
                if (m_TalentVODic.TryGetValue(m_TalentNodes[i].Tid, out TalentVO talentVO))
                {
                    talentVO.Level = (int)m_TalentNodes[i].Level;
                    talentVO.Type = (int)m_TalentNodes[i].Type;
                    talentVO.MaxLevel = (int)m_TalentNodes[i].Maxlevel;
                    SetTalentState(talentVO);

                }
                else
                {
                    Debug.Log("本地没有这个数据");
                }
            }

        }
        else
        {
            Debug.Log("没有数据");
        }
        GameFacade.Instance.SendNotification(NotificationName.MSG_TALENT_CHANGEINFOS);

    }

    /// <summary>
    /// 发送操作指令
    /// </summary>
    /// <param name="code">指令</param>
    /// <param name="tid">tid</param>
    public async void GetTalentOperation(TalentCode code, ulong tid)
    {
        Debug.Log(tid + "------->>>>>发送操作天赋指令"+code);
        TalentOperationResponse getTalentInfosResponse = await TalentOperationRPC.TalentOperation((uint)code, tid);
        Debug.Log("------->>>>>接收操作天赋" + getTalentInfosResponse.Success);
        if (code == TalentCode.Reset || code == TalentCode.StopUse)
        {
            ResetLevelData((uint)tid);
        }
        if (getTalentInfosResponse.Success != null && getTalentInfosResponse.Success.Success_.ErrorCode == 0)
        {
            m_TalentNodes.Clear();
            m_TalentNodes = getTalentInfosResponse.Success.Success_.Nodes;
            int errorCode= getTalentInfosResponse.Success.Success_.ErrorCode;
            DebugErrorCode(errorCode);
            for (int i = 0; i < m_TalentNodes.Count; i++)
            {
                TalentVO talentVO = null;
                if (m_TalentVODic.TryGetValue(m_TalentNodes[i].Tid, out talentVO))
                {
                    talentVO.Level = (int)m_TalentNodes[i].Level;
                    talentVO.Type = (int)m_TalentNodes[i].Type;
                    talentVO.MaxLevel = (int)m_TalentNodes[i].Maxlevel;
                    SetTalentState(talentVO);
                }
                else
                {

                    Debug.Log("本地没有这个数据");
                }
            }
        }
        else
        {
            Debug.Log("没有数据");
        }
        MsgTalentOperation msgTalentOperation = new MsgTalentOperation();
        msgTalentOperation.M_TalentCode = code;
        msgTalentOperation.Tid = (uint)tid;
        if (getTalentInfosResponse.Success.Success_.ErrorCode == 0)
        {
            GameFacade.Instance.SendNotification(NotificationName.MSG_TALENT_OPERATION, msgTalentOperation);
        }
    }
    #endregion
    /// <summary>
    /// 报错返回
    /// </summary>
    public void DebugErrorCode(int error)
    {
        Debug.Log((TalentError)error);
    }

    /// <summary>
    /// 解锁条件字符
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public string GetUnLockLabel(uint id)
    {

        StringBuilder str=new StringBuilder();
        if (id<=0)
        {
            str.Append("Not");
        }
        if (id > 0)
        {
            EffectElement?[] effectElements = m_CfgEternityProxy.GetEffectElementsByKey(id);
            for (int i = 0; i < effectElements.Length; i++)
            {
                OperationType fun = (OperationType)effectElements[i].Value.Function;
                switch (fun)
                {
                    case OperationType.kGreaterAndEqual:
                        if (effectElements[i].Value.Attribute == (int)AttributeName.kDuanLevel)
                        {
                            str.Append(
                                TableUtil.GetLanguageString(AttributeName.kDuanLevel)+
                                TableUtil.GetLanguageString(OperationType.kGreaterAndEqual)+
                                effectElements[i].Value.Value.ToString()+"\n");
                           
                        }
                        if (effectElements[i].Value.Attribute == (int)AttributeName.kRoleLevel)
                        {
                            str.Append(
                                  TableUtil.GetLanguageString(AttributeName.kRoleLevel) +
                                  TableUtil.GetLanguageString(OperationType.kGreaterAndEqual) +
                                  effectElements[i].Value.Value.ToString() + "\n");
                        }
                        break;
                    case OperationType.kGreater:
                        if (effectElements[i].Value.Attribute == (int)AttributeName.kDuanLevel)
                        {
                            str.Append(
                                 TableUtil.GetLanguageString(AttributeName.kDuanLevel) +
                                 TableUtil.GetLanguageString(OperationType.kGreaterAndEqual) +
                                 effectElements[i].Value.Value.ToString() + "\n");
                        }
                        if (effectElements[i].Value.Attribute == (int)AttributeName.kRoleLevel)
                        {
                            str.Append(
                                 TableUtil.GetLanguageString(AttributeName.kRoleLevel) +
                                 TableUtil.GetLanguageString(OperationType.kGreater) +
                                 effectElements[i].Value.Value.ToString() + "\n");
                        }
                        break;

                    default:
                        break;
                }
            }
        }
        return str.ToString();
    }

    /// <summary>
    /// 设置天赋状态
    /// </summary>
    /// <param name="talentVO"></param>
    public void SetTalentState(TalentVO talentVO)
    {
        if (talentVO==null)
            return;
        if (talentVO.Level == 0)
        {
            talentVO.State = TalentState.NoActivate;
        }
        bool talentrootMeet = true;
        bool talentMeet = true;
        PlayerInfoVo playerInfoVo = NetworkManager.Instance.GetPlayerController().GetPlayerInfo();
        #region 判读激活
        // Debug.Log(talentVO.Id+"--------------" + talentVO.UnLockId);
        if (talentVO.UnLockId>0)
        {
            EffectElement?[] effectElements = m_CfgEternityProxy.GetEffectElementsByKey((uint)talentVO.UnLockId);
            for (int i = 0; i < effectElements.Length; i++)
            {
                OperationType fun = (OperationType)effectElements[i].Value.Function;
                switch (fun)
                {
                    case OperationType.kGreaterAndEqual:
                        if (effectElements[i].Value.Attribute == (int)AttributeName.kDuanLevel)
                        {
                            if (playerInfoVo.Exp < effectElements[i].Value.Value)
                            {
                                talentMeet = false;
                            }
                        }
                        if (effectElements[i].Value.Attribute == (int)AttributeName.kRoleLevel)
                        {
                            if (playerInfoVo.Level < effectElements[i].Value.Value)
                            {
                                talentMeet = false;
                            }
                        }
                        break;
                    case OperationType.kGreater:
                        if (effectElements[i].Value.Attribute == (int)AttributeName.kDuanLevel)
                        {
                            if (playerInfoVo.Exp <= effectElements[i].Value.Value)
                            {
                                talentMeet = false;
                            }
                        }
                        if (effectElements[i].Value.Attribute == (int)AttributeName.kRoleLevel)
                        {
                            if (playerInfoVo.Level <= effectElements[i].Value.Value)
                            {
                                talentMeet = false;
                            }
                        }
                        break;

                    default:
                        break;
                }
            }
        }

        // EffectElement?[] effectElementRoots = m_CfgEternityProxy.GetEffectElementsByKey((uint)talentVO.MTalentTypeVO.UnLockId);
        //for (int i = 0; i < effectElementRoots.Length; i++)
        //{
        //    OperationType fun = (OperationType)effectElementRoots[i].Value.Function;
        //    switch (fun)
        //    {
        //        case OperationType.kGreaterAndEqual://反向判段
        //            if (effectElementRoots[i].Value.Attribute == (int)AttributeName.kDuanLevel)
        //            {
        //                if (playerInfoVo.Exp < effectElementRoots[i].Value.Value)
        //                {
        //                    talentrootMeet = false;
        //                }
        //            }
        //            if (effectElementRoots[i].Value.Attribute == (int)AttributeName.kRoleLevel)
        //            {
        //                if (playerInfoVo.Level < effectElementRoots[i].Value.Value)
        //                {
        //                    talentrootMeet = false;
        //                }
        //            }
        //            break;
        //        case OperationType.kGreater:
        //            if (effectElementRoots[i].Value.Attribute == (int)AttributeName.kDuanLevel)
        //            {
        //                if (playerInfoVo.Exp <= effectElementRoots[i].Value.Value)
        //                {
        //                    talentrootMeet = false;
        //                }
        //            }
        //            if (effectElementRoots[i].Value.Attribute == (int)AttributeName.kRoleLevel)
        //            {
        //                if (playerInfoVo.Level <= effectElementRoots[i].Value.Value)
        //                {
        //                    talentrootMeet = false;
        //                }
        //            }
        //            break;

        //        default:
        //            break;
        //    }
        //}

        #endregion
        if (m_TalentVODic.TryGetValue((uint)talentVO.BackNodeId, out TalentVO vO))
        {
            if (vO.Level >= 1 && talentrootMeet && talentMeet)//前置等于1
            {
                talentVO.State = TalentState.CanActivate;
            }
        }
        if ((uint)talentVO.BackNodeId == 0)
        {
            talentVO.State = TalentState.CanActivate;
        }
        if (talentVO.Level>=1)
        {
            talentVO.State = TalentState.Activate;
        }

        if (talentVO.Level == talentVO.MaxLevel)
        {
            talentVO.State = TalentState.FullLevel;
        }

    }

    /// <summary>
    /// 天赋指令返回错误码
    /// </summary>
    enum TalentError
    {
        OK = 0,
        ALREADY_ACTIVITY = 1,               //已经激活
        UNLOCK = 2,                         //未解锁
        UNACTIVITY = 3,                     //未激活
        NOT_FIND_TALENT_CONFIG = 4,         // 未找到天赋配置
        NOT_HAVE_ENOUGH_TALENT_VALUE = 5,   //天赋点不够
        ALREAY_MAX_LEVEL = 6,               //已经到最大等级
        INVALID_PLAYER = 7,                 //角色不存在
        NOT_AT_GROUND = 8,                  //不在人形态
    }
}
