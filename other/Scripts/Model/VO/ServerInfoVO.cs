/************************
 *	ServerInfoVO	    *
 *	Gaoyu			    *
 *	2019-4-11		    *
 ************************/
using System.Collections.Generic;

public class ServerInfoVO
{
	/// <summary>
	/// ������Id
	/// </summary>
	public int Id;
	/// <summary>
	/// ����������
	/// </summary>
	public string Name;
	/// <summary>
	/// ������GID
	/// </summary>
	public string Gid;
    /// <summary>
	/// ������״̬
	/// </summary>
	public int State = 1;
	/// <summary>
	/// ������ ����ʱ��
	/// </summary>
	public string OpenTime;
	/// <summary>
	/// ������Ip
	/// </summary>
	public string Ip;
	/// <summary>
	/// �������˿�
	/// </summary>
	public int Port;
	/// <summary>
	/// ��ɫVO list
	/// </summary>
	public List<CharacterVO> CharacterList;

	public class CharacterVO
	{
		/// <summary>
		/// ��ɫuid
		/// </summary>
		public ulong UId;
		/// <summary>
		/// ����
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
		/// �ϴε�½ʱ��
		/// </summary>
		public int LastLoginTime;
        /// <summary>
        /// ��λ
        /// </summary>
        public int DanLevel;
	}
}
