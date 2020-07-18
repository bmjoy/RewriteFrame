#if UNITY_EDITOR
using UnityEngine;

namespace Map
{
    public class MapEditorSetting : ScriptableObject
	{
		/// <summary>
		/// 资源导出的路径
		/// </summary>
		public string AssetExportDirectory;
		/// <summary>
		/// 预览用的Scene的路径
		/// </summary>
		public string PreviewScenePath;
		/// <summary>
		/// 导出所有场景时使用这个设置
		/// </summary>
		public ExportSetting ExportAllMapSetting;
        
        /// <summary>
        /// 模板路径
        /// </summary>
        public string m_MapTempletePath;
		/// <summary>
		/// 地图保存路径
		/// </summary>
		public string m_MapSavePath;

		/// <summary>
		/// AreaSpawner模板路径
		/// </summary>
		public string m_AreaSpawnerTempletePath;

        /// <summary>
        /// 碰撞导出路径
        /// </summary>
        public string m_ColliderSavePath;

        /// <summary>
        /// 碰撞导出备份路径
        /// </summary>
        public string m_ColliderSavePathCopy;

        /// <summary>
        /// 寻宝预设保存路径
        /// </summary>
        public string m_TreasurePrefabSavePath;

        /// <summary>
        /// 挖矿预设保存路径
        /// </summary>
        public string m_MineralPrefabSavePath;
	}
}
#endif