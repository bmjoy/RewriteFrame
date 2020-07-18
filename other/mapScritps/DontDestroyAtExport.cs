#if UNITY_EDITOR
using UnityEngine;

namespace Map
{
	/// <summary>
	/// 这个gameObject在导出Map的时候不会被Destroy
	/// </summary>
	public class DontDestroyAtExport : MonoBehaviour
	{
	}
}
#endif