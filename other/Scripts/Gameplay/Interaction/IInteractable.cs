using UnityEngine;

namespace Interaction
{
	/// <summary>
	/// 可交互的
	/// </summary>
	public interface IInteractable
	{
		/// <summary>
		/// 可交互的半径
		/// </summary>
		float GetInteractableRadius();
		/// <summary>
		/// 可交互的距离的平方
		/// </summary>
		float GetInteractableDistanceSquare();
		/// <summary>
		/// <see cref="UnityEngine.Object.GetInstanceID"/>
		/// </summary>
		/// <returns></returns>
		int GetInstanceID();
		/// <summary>
		/// 用于避免内存泄漏
		/// </summary>
		bool IsAlive();
		/// <summary>
		/// 获取世界坐标的坐标
		/// </summary>
		Vector3 GetWorldPosition();
		/// <summary>
		/// 设置当前是否是玩家的焦点
		/// </summary>
		void SetFocus(bool focus);
		/// <summary>
		/// 是否可交互
		/// </summary>
		bool GetIsActive();
		/// <summary>
		/// 玩家触发了交互
		/// </summary>
		void OnInteracted();
		/// <summary>
		/// 调试用的
		/// </summary>
		string GetDisplay();
	}
}