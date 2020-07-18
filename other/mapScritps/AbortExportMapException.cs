#if UNITY_EDITOR
namespace Map
{
	/// <summary>
	/// 用户取消导出时throw这个异常
	/// </summary>
	public class AbortExportMapException : System.Exception
	{
	}
}
#endif