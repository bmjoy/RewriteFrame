#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Map
{
	[System.Serializable]
	public class ExportSetting
	{
		/// <summary>
		/// 导出时是否导出MapScene
		/// 因为导出Scene非常耗时，大部分时候并不需要重复导出
		/// </summary>
		[Tooltip("是否导出场景")]
		public bool ExportMapScene;

        /// <summary>
        /// 是否只导出场景
        /// </summary>
        [Tooltip("是否只导出场景")]
        public bool OnlyExportMapScene;
		/// <summary>
		/// 如果没Bake场景，那导出Lightmap就没有意义
		/// </summary>
		[Tooltip("是否导出LightMap信息")]
		public bool ExportLightmap;

        [Tooltip("是否清除无用的unit")]
        public bool CleanUnUsedUnit;

        [Tooltip("是否合并公用的unit")]
        public bool MergeCommonUnit;

        [Tooltip("是否删除区域分割配置")]
        public bool DestroySplitSetting=true;
	}
}
#endif