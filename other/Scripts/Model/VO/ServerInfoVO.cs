/************************
 *	ServerInfoVO	    *
 *	Gaoyu			    *
 *	2019-4-11		    *
 ************************/
using System.Collections.Generic;

public class ServerInfoVO
{
	/// <summary>
	/// 服务器Id
	/// </summary>
	public int Id;
	/// <summary>
	/// 服务器名字
	/// </summary>
	public string Name;
	/// <summary>
	/// 服务器GID
	/// </summary>
	public string Gid;
    /// <summary>
	/// 服务器状态
	/// </summary>
	public int State = 1;
	/// <summary>
	/// 服务器 开启时间
	/// </summary>
	public string OpenTime;
	/// <summary>
	/// 服务器Ip
	/// </summary>
	public string Ip;
	/// <summary>
	/// 服务器端口
	/// </summary>
	public int Port;
	/// <summary>
	/// 角色VO list
	/// </summary>
	public List<CharacterVO> CharacterList;

	public class CharacterVO
	{
		/// <summary>
		/// 角色uid
		/// </summary>
		public ulong UId;
		/// <summary>
		/// 姓名
		/// </summary>
		public string Name;
		/// <summary>
		/// tid
		/// </summary>
		public int Tid;
		/// <summary>
		/// lv
		/// </summary>
		public int Level;
		/// <summary>
		/// 上次登陆时间
		/// </summary>
		public int LastLoginTime;
        /// <summary>
        /// 段位
        /// </summary>
        public int DanLevel;
	}
}
