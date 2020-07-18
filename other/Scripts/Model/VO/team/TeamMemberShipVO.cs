using System.Collections.Generic;

public class TeamMemberShipVO
{
    /// <summary>
    /// ID
    /// </summary>
    public int ID;

    /// <summary>
    /// 等级
    /// </summary>
    public int Level;

	/// <summary>
	/// 飞船模组列表
	/// </summary>
    public List<TeamMemberShipModVO> Modules = new List<TeamMemberShipModVO>();
}