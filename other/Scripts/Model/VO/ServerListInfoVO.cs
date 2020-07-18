using System;
/****************************
 *	JsonUtility.FromJson	*
 *	变量名就不改了			*
 ****************************/
using System.Collections.Generic;

[Serializable]
public class ServerListInfoVO
{
	public List<RoleInfoVO> roleList;
	public List<ServerInfoVO> serverList;
}
