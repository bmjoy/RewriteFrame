#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Map
{
	public class GamingMapEditorSetting : ScriptableObject
	{
		/// <summary>
		/// GamingMap存储路径
		/// </summary>
		public string m_GamingMapPath;
		/// <summary>
		/// GamingMap模板
		/// </summary>
		public string m_GamingMapTemplete;
		/// <summary>
		/// GamingMapArea模板
		/// </summary>
		public string m_GamingAreaTemplete;
		/// <summary>
		/// 配置表路径
		/// </summary>
		public string m_ConfigPath;
		/// <summary>
		/// json保存路径
		/// </summary>
		public string m_JsonPath;

		/// <summary>
		/// Creature模板路径
		/// </summary>
		public string m_CreatureTempletePath;
		/// <summary>
		/// Location模板路径
		/// </summary>
		public string m_LocationTempletePath;

        /// <summary>
        /// 跃迁模板路径
        /// </summary>
        public string m_LeapTempletePath;

        /// <summary>
        /// 触发器模板路径
        /// </summary>
        public string m_TriggerTempletePath;
        /// <summary>
        /// 最大战舰路径
        /// </summary>
        public string m_MaxShipPath;

        /// <summary>
        /// 星图存储路径
        /// </summary>
        public string m_StarMapPath;

        /// <summary>
        /// 战舰属性存放路径
        /// </summary>
        public string m_SpacecraftPropertyPath;
    }
}
#endif