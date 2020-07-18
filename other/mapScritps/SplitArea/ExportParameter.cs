#if UNITY_EDITOR
using System.Collections.Generic;

namespace Map
{
	public class ExportParameter
	{
		public HashSet<string> ExportedUnitAddressableKeys = null;
		public bool ThrowExceptionAtAbort = false;
	}

    /// <summary>
    /// 导出参数设置
    /// </summary>
    public class ExportData
    {
        public Map m_Map;
        public ExportParameter m_ExportParameter;
        public List<AreaSpawner> m_AreaSpawners;
    }
}
#endif