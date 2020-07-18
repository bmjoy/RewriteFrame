using UnityEngine;

namespace Interaction
{
	public interface IOwnerPlayer
	{
		/// <summary>
		/// 用于避免内存泄漏
		/// </summary>
		bool IsAlive();
		Vector3 GetWorldPosition();
		/// <summary>
		/// 调试用的
		/// </summary>
		string GetDisplay();
	}
}