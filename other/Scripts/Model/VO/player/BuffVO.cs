public class BuffVO
{
	/// <summary>
	/// ID
	/// </summary>
	public uint ID;
    
    /// <summary>
    /// 堆叠数
    /// </summary>
    public int StackCount;

	/// <summary>
	/// 开始时间
	/// </summary>
	public float BeginTime;

	/// <summary>
	/// 剩余时间
	/// </summary>
	public float EndTime;

    /// <summary>
    /// A 链接B 特效buff ,的需要链接方ID
    /// </summary>
    public uint Link_id;

    /// <summary>
    /// 是否是主链接，A 链接B  中的A 
    /// </summary>
    public bool Is_master;



    public BuffVO(uint id,int stackCount, float beginTime, float lifeTime, uint link_id = 0, bool is_master = false)
    {
        this.ID = id;
        this.StackCount = stackCount;
        this.BeginTime = beginTime;
        this.EndTime = lifeTime;
        this.EndTime = lifeTime;
        this.Link_id = link_id;
        this.Is_master = is_master;
    }
}
