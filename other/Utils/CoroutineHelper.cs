using UnityEngine;

public class CoroutineHelper : MonoBehaviour
{
	private static CoroutineHelper ms_Instance;

	public static CoroutineHelper GetInstance()
	{
		if (ms_Instance == null)
		{
			ms_Instance = new GameObject().AddComponent<CoroutineHelper>();
			ms_Instance.Initialize();
		}
		return ms_Instance;
	}

	private void Initialize()
	{
		gameObject.hideFlags = HideFlags.HideInHierarchy
			| HideFlags.DontSave;
		DontDestroyOnLoad(gameObject);
	}
}