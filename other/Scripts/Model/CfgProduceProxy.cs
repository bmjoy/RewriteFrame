using Eternity.FlatBuffer;
using PureMVC.Patterns.Proxy;
using UnityEngine.Assertions;

public partial class CfgEternityProxy : Proxy
{

    /// <summary>
    /// 根据tid 获取生产基础数据
    /// </summary>
    /// <param name="tid"></param>
    /// <returns></returns>
    public Produce GetProduceByKey(uint tid)
    {
        Produce? produceVO = m_Config.ProducesByKey(tid);
        Assert.IsTrue(produceVO.HasValue, "CfgEternityProxy => GetProduceByKey not exist tid " + tid);
        return produceVO.Value;
    }

    /// <summary>
    /// 获取生产总数据长度
    /// </summary>
    /// <returns></returns>
    public int GetProduceDataLength()
    {
        return m_Config.ProducesLength;
    }

    /// <summary>
    /// 根据数据表index获取蓝图信息
    /// </summary>
    /// <param name="index">index</param>
    /// <returns></returns>
    public Produce? GetProducesByIndex(int index)
    {
        return m_Config.Produces(index);
    }

}