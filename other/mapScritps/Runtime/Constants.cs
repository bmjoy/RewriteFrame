using System.Collections.Generic;
using UnityEngine;

namespace Map
{
	public static class Constants
	{
		/// <summary>
		/// Log的Tag
		/// </summary>
		public const string LOG_TAG = "Map";
		/// <summary>
		/// Lod的Log的Tag
		/// </summary>
		public const string LOD_LOG_TAG = "LOD";
		/// <summary>
		/// Map的Addressable Group Name
		/// </summary>
		public const string ADDRESSABLE_MAP_GROUP_NAME = "Map";
		/// <summary>
		/// MapInfo的<see cref=" UnityEditor.AddressableAssets.Settings.AddressableAssetEntry.address"/>的前缀
		/// </summary>
		public const string MAP_INFO_ADDRESSKEY_STARTWITHS = "MapInfo_";
		/// <summary>
		/// 保存<see cref="AreaInfo"/>时的文件名Format
		/// 同时也是AreaDetailInfo的<see cref=" UnityEditor.AddressableAssets.Settings.AddressableAssetEntry.address"/>的Format
		/// </summary>
		public const string AREA_DETAIL_INFO_FILENAME_FORMAT = "AreaDetailInfo_{0}_{1}";
		/// <summary>
		/// Area的GameObject的name前缀
		/// </summary>
		public const string AREA_GAMEOBJECT_NAME_STARTWITHS = "Area_";
		/// <summary>
		/// <see cref="EditorPrefs"/>的Key，存的是<see cref="MapEditorSetting"/>的所在路径
		/// </summary>
		public const string MAP_EDITOR_SETTING_PATH_PREFSKEY = "MAP_EDITOR_SETTING_PATH_PREFSKEY";
		/// <summary>
		/// 创建<see cref="MapEditorSetting"/>时的默认文件名
		/// </summary>
		public const string MAP_EDITOR_SETTING_DEFAULT_FILENAME = "MapEditorSetting";
		/// <summary>
		/// 保存<see cref="MapInfo"/>时的文件名前缀
		/// </summary>
		public const string MAP_INFO_FILENAME_STARTWITHS = "MapInfo_";
		/// <summary>
		/// 导出的Scene的文件名前缀
		/// </summary>
		public const string SCENE_EXPORT_FILENAMESTARTWITHS = "MapScene_";
		/// <summary>
		/// 导出的Unit Prefab的目录和文件名前缀
		/// </summary>
		public const string UNIT_PREFAB_EXPORT_DIRECTORY_AND_FILENAMESTARTWITHS = UNIT_PREFAB_EXPORT_DIRECTORY + "Unit_";
		/// <summary>
		/// 导出的Unit Prefab的目录
		/// </summary>
		public const string UNIT_PREFAB_EXPORT_DIRECTORY = "Units/";
		/// <summary>
		/// Scene的<see cref=" UnityEditor.AddressableAssets.Settings.AddressableAssetEntry.address"/>的前缀
		/// </summary>
		public const string SCENE_ADDRESSKEY_STARTWITHS = "MapScene_";
		/// <summary>
		/// Map的GameObject的name前缀
		/// </summary>
		public const string MAP_GAMEOBJECT_NAME_STARTWITHS = "Map_";
		/// <summary>
		/// Unit Prefab的<see cref=" UnityEditor.AddressableAssets.Settings.AddressableAssetEntry.address"/>的前缀
		/// </summary>
		public const string UNIT_PREFAB_ADDRESSKEY_FORMAT = "Unit_{0}";
		/// <summary>
		/// <see cref="Map.Uid"/>
		/// </summary>
		public const uint MAP_UID_MIN = 0;
		/// <summary>
		/// <see cref="Map.Uid"/>
		/// </summary>
		public const uint MAP_UID_MAX = 0x7FFF;
		/// <summary>
		/// <see cref="Area.Uid"/>
		/// </summary>
		public const uint AREA_UID_MIN = 0;
		/// <summary>
		/// <see cref="Area.Uid"/>
		/// </summary>
		public const uint AREA_UID_MAX = 0x7FFF;
		/// <summary>
		/// <see cref="Map.VoxelSize"/>的最小值
		/// </summary>
		public const int VOXEL_SIZE_MIN = 1;
		/// <summary>
		/// <see cref="Map.VoxelSize"/>的最大值
		/// </summary>
		public const int VOXEL_SIZE_MAX = 10000;
        /// <summary>
        /// 导出MapInfo时,Unit在屏幕空间的大小超过这个值时会显示
        /// 参考的全境封锁2
        /// </summary>
        //public const float UNIT_MIN_DISPLAY_RELATIVE_SIZE = 0.03f;

        public const float UNIT_MIN_DISPLAY_RELATIVE_SIZE = 0.03f;
        /// <summary>
        /// 导出MapInfo时,Area在屏幕空间的大小超过这个值时会显示
        /// </summary>
        public const float AREA_MIN_DISPLAY_RELATIVE_SIZE = 0.2f;
		/// <summary>
		/// 每帧更新多少个Unit
		/// </summary>
		public const int UPDATE_UNITCOUNT_EVERFRAME = 1000;
		/// <summary>
		/// <see cref="MapPreview"/>的GameObject的name的前缀
		/// </summary>
		public const string MAP_PREVIEW_GAMEOBJECT_NAME_STARTWITHS = "Map_Preview_";
		/// <summary>
		/// <see cref="AreaPreview"/>的GameObject的name的前缀
		/// </summary>
		public const string AREA_PREVIEW_GAMEOBJECT_NAME_STARTWITHS = "Area_Preview_";
		/// <summary>
		/// <see cref="VoxelPreview"/>的GameObject的name的前缀
		/// </summary>
		public const string VOXEL_PREVIEW_GAMEOBJECT_NAME_FORMAT = "Voxel_Preview_{0}";
		/// <summary>
		/// 自动分配的Area的Uid的起始值
		/// </summary>
		public const uint AUTO_ALLOCATE_AREA_UID_BEGIN = 0;
		/// <summary>
		/// Notset的AreaIndex
		/// </summary>
		public const int NOTSET_AREA_INDEX = -1;
        /// <summary>
        /// 不限制加载的AREA
        /// </summary>
        public const ulong NOT_LIMIT_AREA_UID = ulong.MaxValue - 1;
		/// <summary>
		/// <see cref="MapVoxelGridPreview"/>的GameObject的name的前缀
		/// </summary>
		public const string MAP_VOXELGRID_PREVIEW_GAMEOBJECT_NAME_STARTWITHS = "Map_VoxelGrid_Preview";

		/// <summary>
		/// Notset的MapUid
		/// </summary>
		public const uint NOTSET_MAP_UID = 0xFFFF;
		/// <summary>
		/// Notset的AreaUid
		/// </summary>
		public const uint NOTSET_AREA_UID = 0xFFFF;
		/// <summary>
		/// Notset的VoxelIndex
		/// </summary>
		public const int NOTSET_VOXEL_INDEX = -1;
		/// <summary>
		/// 允许正在运行的UnitCommand数量
		/// </summary>
		public const int MAX_RUNING_UNITCOMMAND_COUNT = 128;
		/// <summary>
		/// 实例化Unit时默认用的坐标
		/// </summary>
		public static readonly Vector3 INSTANTIATE_UNIT_POSITION = Vector3.one * 100000;
		/// <summary>
		/// Release Area时每帧最多Release Unit的数量
		/// </summary>
		public const int MAX_RELEASE_UNIT_COUNT_EACHFRAME_WHEN_RELEASEAREA = 256;
		/// <summary>
		/// 导出时<see cref="ParticleSystem.Simulate(float)"/>
		/// </summary>
		public const float PARTICLE_SIMULATE_TIME_WHEN_EXPOTEING = 3.0f;
		/// <summary>
		/// Map下的VoxelGrid最多Voxel数量
		/// 拍脑门写的，并没有什么意义
		/// </summary>
		public const int MAX_MAP_VOXELGRID_AUTO_VOXEL_COUNT = 100000;
		/// <summary>
		/// 自动计算Map的VoxelSize时，计算结果会乘以这个系数
		/// 拍脑门写的，并没有什么意义
		/// </summary>
		public const float AUTO_MAP_VOXELSIZE_COEFFICIENT = 2.4f;
		/// <summary>
		/// 导出时Map的文件夹名前缀
		/// </summary>
		public const string EXPORT_MAP_FOLDER_NAME_STARTWITHS = "Map_";
		/// <summary>
		/// <see cref="Leyoutech.Utility.LODUtility.CaculateCurrentLOGIndex"/>
		/// </summary>
		public const int NOTSET_LOD_INDEX = int.MaxValue;
		public const float LOD_UPDATE_TIME_INTERVAL = 1.2f;

		/// <summary>
		/// GamingMapEditor配置路径
		/// </summary>
		public const string GAMINGMAP_EDITOR_SETTING_PATH_PREFSKEY = "GAMINGMAP_EDITOR_SETTING_PATH_PREFSKEY";

		/// <summary>
		/// 创建<see cref="GamingMapEditorSetting"/>时的默认文件名
		/// </summary>
		public const string GAMINGMAP_EDITOR_SETTING_DEFAULT_FILENAME = "GamingMapEditorSetting";
#if UNITY_EDITOR
		public readonly static string[] EXPORT_MAPASSET_LABELS = new string[] { "MapExport" };
		/// <summary>
		/// 判断Prefab是否有Override时忽略的<see cref="UnityEditor.PropertyModification.propertyPath"/>
		/// Prefab根节点的GameObject
		/// </summary>
		public static readonly HashSet<string> PREFAB_ROOT_GAMEOBJECT_IGNORE_MODIFICATION_PROPERTY_PATHS = new HashSet<string>(new string[] { "m_Name" });
		/// <summary>
		/// <see cref="PREFAB_ROOT_GAMEOBJECT_IGNORE_MODIFICATION_PROPERTY_PATHS"/>
		/// Prefab根节点的Transform
		/// </summary>
		public static readonly HashSet<string> PREFAB_ROOT_TRANSFORM_IGNORE_MODIFICATION_PROPERTY_PATHS = new HashSet<string>(new string[] { "m_LocalPosition.x"
			,"m_LocalPosition.y"
			,"m_LocalPosition.z"
			,"m_LocalRotation.x"
			,"m_LocalRotation.y"
			,"m_LocalRotation.z"
			,"m_LocalRotation.w"
			,"m_RootOrder"
			,"m_LocalEulerAnglesHint.x"
			,"m_LocalEulerAnglesHint.y"
			,"m_LocalEulerAnglesHint.z"
			,"m_LocalScale.x"
			,"m_LocalScale.y"
			,"m_LocalScale.z"
			,"m_LocalPosition.x"
			,"m_LocalPosition.y"
			,"m_LocalPosition.z" });
		/// <summary>
		/// <see cref="PREFAB_ROOT_GAMEOBJECT_IGNORE_MODIFICATION_PROPERTY_PATHS"/>
		/// <see cref="BatchRendering.RandomDisperseMesh"/>
		/// </summary>
		public static readonly HashSet<string> PREFAB_RANDOM_DISPERSE_MESH_UNIT_IGNORE_MODIFICATION_PROPERTY_PATHS = new HashSet<string>(new string[] { "MyRendererIn"
			, "m_Enabled"});
#endif

#if UNITY_EDITOR
		/// <summary>
		/// 用于测试开启和不开启时Static Batch Count是否发生变化
		/// <see cref="CombineStaticBatch"/>
		/// </summary>
		public const bool ENABLE_COMBINE_STATIC_BATCH_COMBINE = true;
#endif

		/// <summary>
		/// true: 会检测所有静态物体是否被修改了Transform
		/// <see cref="CombineStaticBatch"/>
		/// </summary>
		public const bool DEBUG_COMBINE_STATIC_BATCH_TRANFORM =
#if UNITY_EDITOR
			true
#else
			false
#endif
			;
	}
}