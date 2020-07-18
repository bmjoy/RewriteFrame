using UnityEngine;

namespace Map
{
	/// <summary>
	/// 挂上这个脚本的gameObject会保持和玩家的距离
	/// </summary>
	public class ForcedLockingDistanceToPlayer : MonoBehaviour
	{
		/// <summary>
		/// 尽在<see cref="MapController"/>中用
		/// </summary>
		internal Vector3 _DistanceToPlayer;
	}
}

