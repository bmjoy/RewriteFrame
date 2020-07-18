using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Map
{
	public interface ILODItem
	{
		/// <summary>
		/// 用于避免重复添加到<see cref="LODManager"/>
		/// </summary>
		int GetInstanceID();
		LODGroup GetLODGroup();
		/// <summary>
		/// LODIndex小于等于这个时会触发<see cref="SetLODItemActive(true)"/>
		/// </summary>
		/// <returns></returns>
		int GetMaxDisplayLODIndex();
		void SetLODItemActive(bool active);
		/// <summary>
		/// 用于判断内存泄漏
		/// </summary>
		bool IsAlive();
		void DoUpdateLOD(int lodIndex);
	}
}