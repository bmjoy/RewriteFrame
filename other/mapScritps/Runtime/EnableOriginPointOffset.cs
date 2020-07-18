using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Map
{
	/// <summary>
	/// 挂上这个脚本的rootGameObject会在游戏中被修改原点
	/// <see cref="MapManager.ChangeWorldOriginPositionOffset(Vector3)"/>
	/// </summary>
    public class EnableOriginPointOffset : MonoBehaviour
    {
		/// <summary>
		/// 尽在<see cref="MapController"/>中用
		/// </summary>
		internal Vector3 _RealWorldPostion;
    }
}