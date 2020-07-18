using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public sealed class ObjectPool : MonoBehaviour
{
	public enum StartupPoolMode { Awake, Start, CallManually };

	[System.Serializable]
	public class StartupPool
	{
		public int size;
		public GameObject prefab;
	}

	static ObjectPool _instance;
	static List<GameObject> tempList = new List<GameObject>();
	
	Dictionary<GameObject, List<GameObject>> pooledObjects = new Dictionary<GameObject, List<GameObject>>();
	Dictionary<GameObject, GameObject> spawnedObjects = new Dictionary<GameObject, GameObject>();
    Dictionary<GameObject, string> ObjectAndPathDic = new Dictionary<GameObject, string>();//《模板，路径》 容器

	
	public StartupPoolMode startupPoolMode;
	public StartupPool[] startupPools;

	bool startupPoolsCreated;


   // private string pathOrAddress; //路径/地址



    void Awake()
	{
		_instance = this;
		if (startupPoolMode == StartupPoolMode.Awake)
			CreateStartupPools();
	}

	void Start()
	{
		if (startupPoolMode == StartupPoolMode.Start)
			CreateStartupPools();
	}

	public static void CreateStartupPools()
	{
		if (!instance.startupPoolsCreated)
		{
			instance.startupPoolsCreated = true;
			var pools = instance.startupPools;
			if (pools != null && pools.Length > 0)
				for (int i = 0; i < pools.Length; ++i)
					CreatePool(pools[i].prefab, pools[i].size,string.Empty);
		}
	}

	public static void CreatePool<T>(T prefab, int initialPoolSize,string pathOrAddress ) where T : Component
	{
		CreatePool(prefab.gameObject, initialPoolSize, pathOrAddress);
	}
	public static void CreatePool(GameObject prefab, int initialPoolSize, string pathOrAddress)
	{
		if (prefab != null && !instance.pooledObjects.ContainsKey(prefab))
		{
            if (!instance.ObjectAndPathDic.ContainsKey(prefab))
            {
                instance.ObjectAndPathDic.Add(prefab, pathOrAddress);
            }

            var list = new List<GameObject>();
			instance.pooledObjects.Add(prefab, list);
			if (initialPoolSize > 0)
			{
				bool active = prefab.activeSelf;
				prefab.SetActive(false);
				Transform parent = instance.transform;
				while (list.Count < initialPoolSize)
				{
                    GameObject obj = null;
                    string path = string.Empty;
                    instance.ObjectAndPathDic.TryGetValue(prefab, out path);
                    if (string.IsNullOrEmpty(path))
                        obj = Instantiate(prefab);
                    else
                        obj =(GameObject) AssetUtil.InstantiateAsset(path, prefab);


                    obj.transform.SetParent(parent, false);
					list.Add(obj);
				}
				prefab.SetActive(active);
			}
		}
	}
	
	public static T Spawn<T>(T prefab, Transform parent, Vector3 position, Quaternion rotation) where T : Component
	{
		return Spawn(prefab.gameObject, parent, position, rotation).GetComponent<T>();
	}
	public static T Spawn<T>(T prefab, Vector3 position, Quaternion rotation) where T : Component
	{
		return Spawn(prefab.gameObject, null, position, rotation).GetComponent<T>();
	}
	public static T Spawn<T>(T prefab, Transform parent, Vector3 position) where T : Component
	{
		return Spawn(prefab.gameObject, parent, position, Quaternion.identity).GetComponent<T>();
	}
	public static T Spawn<T>(T prefab, Vector3 position) where T : Component
	{
		return Spawn(prefab.gameObject, null, position, Quaternion.identity).GetComponent<T>();
	}
	public static T Spawn<T>(T prefab, Transform parent) where T : Component
	{
		return Spawn(prefab.gameObject, parent, Vector3.zero, Quaternion.identity).GetComponent<T>();
	}
	public static T Spawn<T>(T prefab) where T : Component
	{
		return Spawn(prefab.gameObject, null, Vector3.zero, Quaternion.identity).GetComponent<T>();
	}
	public static GameObject Spawn(GameObject prefab, Transform parent, Vector3 position, Quaternion rotation)
	{
		List<GameObject> list;
		Transform trans;
		GameObject obj;
		if (instance.pooledObjects.TryGetValue(prefab, out list))
		{
			obj = null;
			if (list.Count > 0)
			{
				while (obj == null && list.Count > 0)
				{
					obj = list[0];
					list.RemoveAt(0);
				}
				if (obj != null)
				{
					trans = obj.transform;
                    trans.SetParent(parent,false);
					trans.localPosition = position;
					trans.localRotation = rotation;
					obj.SetActive(true);
					instance.spawnedObjects.Add(obj, prefab);
					return obj;
				}
			}

            string path = string.Empty;
            instance.ObjectAndPathDic.TryGetValue(prefab, out path);
            if (string.IsNullOrEmpty(path))
                obj = Instantiate(prefab);
            else
                obj = (GameObject)AssetUtil.InstantiateAsset(path, prefab);

			trans = obj.transform;
            trans.SetParent(parent,false);
            trans.localPosition = position;
			trans.localRotation = rotation;
			instance.spawnedObjects.Add(obj, prefab);
			return obj;
		}
		else
		{

            string path = string.Empty;
            instance.ObjectAndPathDic.TryGetValue(prefab, out path);
            if (string.IsNullOrEmpty(path))
            {
                CreatePool(prefab, 0, string.Empty);
                obj = Instantiate(prefab);
            }
            else
            {
                CreatePool(prefab, 0, path);
                obj = (GameObject)AssetUtil.InstantiateAsset(path, prefab);
            }

            trans = obj.transform;
            trans.SetParent(parent,false);
            trans.localPosition = position;
            trans.localRotation = rotation;
            instance.spawnedObjects.Add(obj, prefab);
            return obj;

            //obj = Instantiate(prefab);
            //trans = obj.GetComponent<Transform>();
            //trans.SetParent(parent);
            //trans.localPosition = position;
            //trans.localRotation = rotation;
            //return obj;
        }
	}
	public static GameObject Spawn(GameObject prefab, Transform parent, Vector3 position)
	{
		return Spawn(prefab, parent, position, Quaternion.identity);
	}
	public static GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
	{
		return Spawn(prefab, null, position, rotation);
	}
	public static GameObject Spawn(GameObject prefab, Transform parent)
	{
		return Spawn(prefab, parent, Vector3.zero, Quaternion.identity);
	}
	public static GameObject Spawn(GameObject prefab, Vector3 position)
	{
		return Spawn(prefab, null, position, Quaternion.identity);
	}
	public static GameObject Spawn(GameObject prefab)
	{
		return Spawn(prefab, null, Vector3.zero, Quaternion.identity);
	}

	public static void Recycle<T>(T obj) where T : Component
	{
		Recycle(obj.gameObject);
	}
	public static void Recycle(GameObject obj)
	{
        if (obj == null)
        {
            return;
        }
		GameObject prefab;
		if (instance.spawnedObjects.TryGetValue(obj, out prefab))
			Recycle(obj, prefab);
		else
			Destroy(obj);
	}
	static void Recycle(GameObject obj, GameObject prefab)
	{
		instance.pooledObjects[prefab].Add(obj);
		instance.spawnedObjects.Remove(obj);
		obj.transform.SetParent(instance.transform, false);
		obj.SetActive(false);
	}

	public static void RecycleAll<T>(T prefab) where T : Component
	{
		RecycleAll(prefab.gameObject);
	}
	public static void RecycleAll(GameObject prefab)
	{
		foreach (var item in instance.spawnedObjects)
			if (item.Value == prefab)
				tempList.Add(item.Key);
		for (int i = 0; i < tempList.Count; ++i)
			Recycle(tempList[i]);
		tempList.Clear();
	}
	public static void RecycleAll()
	{
		tempList.AddRange(instance.spawnedObjects.Keys);
		for (int i = 0; i < tempList.Count; ++i)
			Recycle(tempList[i]);
		tempList.Clear();
	}

	public static bool IsPooled(GameObject prefab)
	{
		return instance.pooledObjects.ContainsKey(prefab);
	}

	public static bool IsSpawned(GameObject obj)
	{
		return instance.spawnedObjects.ContainsKey(obj);
	}

	public static int CountPooled<T>(T prefab) where T : Component
	{
		return CountPooled(prefab.gameObject);
	}
	public static int CountPooled(GameObject prefab)
	{
		List<GameObject> list;
		if (instance.pooledObjects.TryGetValue(prefab, out list))
			return list.Count;
		return 0;
	}

	public static int CountSpawned<T>(T prefab) where T : Component
	{
		return CountSpawned(prefab.gameObject);
	}
	public static int CountSpawned(GameObject prefab)
	{
		int count = 0 ;
		foreach (var instancePrefab in instance.spawnedObjects.Values)
			if (prefab == instancePrefab)
				++count;
		return count;
	}

	public static int CountAllPooled()
	{
		int count = 0;
		foreach (var list in instance.pooledObjects.Values)
			count += list.Count;
		return count;
	}

	public static List<GameObject> GetPooled(GameObject prefab, List<GameObject> list, bool appendList)
	{
		if (list == null)
			list = new List<GameObject>();
		if (!appendList)
			list.Clear();
		List<GameObject> pooled;
		if (instance.pooledObjects.TryGetValue(prefab, out pooled))
			list.AddRange(pooled);
		return list;
	}
	public static List<T> GetPooled<T>(T prefab, List<T> list, bool appendList) where T : Component
	{
		if (list == null)
			list = new List<T>();
		if (!appendList)
			list.Clear();
		List<GameObject> pooled;
		if (instance.pooledObjects.TryGetValue(prefab.gameObject, out pooled))
			for (int i = 0; i < pooled.Count; ++i)
				list.Add(pooled[i].GetComponent<T>());
		return list;
	}

	public static List<GameObject> GetSpawned(GameObject prefab, List<GameObject> list, bool appendList)
	{
		if (list == null)
			list = new List<GameObject>();
		if (!appendList)
			list.Clear();
		foreach (var item in instance.spawnedObjects)
			if (item.Value == prefab)
				list.Add(item.Key);
		return list;
	}
	public static List<T> GetSpawned<T>(T prefab, List<T> list, bool appendList) where T : Component
	{
		if (list == null)
			list = new List<T>();
		if (!appendList)
			list.Clear();
		var prefabObj = prefab.gameObject;
		foreach (var item in instance.spawnedObjects)
			if (item.Value == prefabObj)
				list.Add(item.Key.GetComponent<T>());
		return list;
	}

	public static void DestroyPooled(GameObject prefab)
	{
		List<GameObject> pooled;
		if (instance.pooledObjects.TryGetValue(prefab, out pooled))
		{
			for (int i = 0; i < pooled.Count; ++i)
				GameObject.Destroy(pooled[i]);
			pooled.Clear();
		}
	}
	public static void DestroyPooled<T>(T prefab) where T : Component
	{
		DestroyPooled(prefab.gameObject);
	}

	public static void DestroyAll(GameObject prefab)
	{
		RecycleAll(prefab);
		DestroyPooled(prefab);
	}
	public static void DestroyAll<T>(T prefab) where T : Component
	{
		DestroyAll(prefab.gameObject);
	}

	public static ObjectPool instance
	{
		get
		{
			if (_instance != null)
				return _instance;

			_instance = FindObjectOfType<ObjectPool>();
			if (_instance != null)
				return _instance;

			var obj = new GameObject("ObjectPool");
			obj.transform.localPosition = Vector3.zero;
			obj.transform.localRotation = Quaternion.identity;
			obj.transform.localScale = Vector3.one;
			_instance = obj.AddComponent<ObjectPool>();
			DontDestroyOnLoad(obj);
			return _instance;
		}
	}
}

public static class ObjectPoolExtensions
{


    /// <summary>
    /// 创建对象池
    /// 特别注意：如果是prefab  通过 AssetUtil/AssetManager/UIManager 加载的，必须要设置 pathOrAddress
    /// 如果是其他的prefab下方子节点作为模板的，可不设置
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="prefab"></param>
    /// <param name="initialPoolSize"></param>
    /// <param name="pathOrAddress"></param>
    public static void CreatePool<T>(this T prefab, int initialPoolSize,string pathOrAddress) where T : Component
    {
        ObjectPool.CreatePool(prefab, initialPoolSize, pathOrAddress);
    }

    /// <summary>
    /// 创建对象池
    /// 特别注意：如果是prefab  通过 AssetUtil/AssetManager/UIManager 加载的，必须要设置 pathOrAddress
    /// 如果是其他的prefab下方子节点作为模板的，可不设置
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="prefab"></param>
    /// <param name="pathOrAddress"></param>
    public static void CreatePool<T>(this T prefab, string pathOrAddress) where T : Component
	{
		ObjectPool.CreatePool(prefab, 0, pathOrAddress);
	}

    // 	public static void CreatePool<T>(this T prefab, int initialPoolSize) where T : Component
    // 	{
    // 		ObjectPool.CreatePool(prefab, initialPoolSize);
    // 	}


    /// <summary>
    /// 创建对象池
    /// 特别注意：如果是prefab 通过AssetUtil/AssetManager/UIManager 加载的，必须要设置 pathOrAddress
    /// 如果是其他的prefab下方子节点作为模板的，可不设置
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="prefab"></param>
    /// <param name="pathOrAddress"></param>
    public static void CreatePool(this GameObject prefab,string pathOrAddress)
	{
		ObjectPool.CreatePool(prefab, 0, pathOrAddress);
	}


    /// <summary>
    /// 创建对象池
    /// 特别注意：如果是prefab 通过 AssetUtil/AssetManager/UIManager 加载的，必须要设置 pathOrAddress
    /// 如果是其他的prefab下方子节点作为模板的，可不设置
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="prefab"></param>
    /// <param name="pathOrAddress"></param>
    public static void CreatePool(this GameObject prefab, int initialPoolSize, string pathOrAddress )
	{
		ObjectPool.CreatePool(prefab, initialPoolSize, pathOrAddress);
	}
	
	public static T Spawn<T>(this T prefab, Transform parent, Vector3 position, Quaternion rotation) where T : Component
	{
		return ObjectPool.Spawn(prefab, parent, position, rotation);
	}
	public static T Spawn<T>(this T prefab, Vector3 position, Quaternion rotation) where T : Component
	{
		return ObjectPool.Spawn(prefab, null, position, rotation);
	}
	public static T Spawn<T>(this T prefab, Transform parent, Vector3 position) where T : Component
	{
		return ObjectPool.Spawn(prefab, parent, position, Quaternion.identity);
	}
	public static T Spawn<T>(this T prefab, Vector3 position) where T : Component
	{
		return ObjectPool.Spawn(prefab, null, position, Quaternion.identity);
	}
	public static T Spawn<T>(this T prefab, Transform parent) where T : Component
	{
		return ObjectPool.Spawn(prefab, parent, Vector3.zero, Quaternion.identity);
	}
	public static T Spawn<T>(this T prefab) where T : Component
	{
		return ObjectPool.Spawn(prefab, null, Vector3.zero, Quaternion.identity);
	}
	public static GameObject Spawn(this GameObject prefab, Transform parent, Vector3 position, Quaternion rotation)
	{
		return ObjectPool.Spawn(prefab, parent, position, rotation);
	}
	public static GameObject Spawn(this GameObject prefab, Vector3 position, Quaternion rotation)
	{
		return ObjectPool.Spawn(prefab, null, position, rotation);
	}
	public static GameObject Spawn(this GameObject prefab, Transform parent, Vector3 position)
	{
		return ObjectPool.Spawn(prefab, parent, position, Quaternion.identity);
	}
	public static GameObject Spawn(this GameObject prefab, Vector3 position)
	{
		return ObjectPool.Spawn(prefab, null, position, Quaternion.identity);
	}
	public static GameObject Spawn(this GameObject prefab, Transform parent)
	{
		return ObjectPool.Spawn(prefab, parent, Vector3.zero, Quaternion.identity);
	}
	public static GameObject Spawn(this GameObject prefab)
	{
		return ObjectPool.Spawn(prefab, null, Vector3.zero, Quaternion.identity);
	}
	
	public static void Recycle<T>(this T obj) where T : Component
	{
		ObjectPool.Recycle(obj);
	}
	public static void Recycle(this GameObject obj)
	{
		ObjectPool.Recycle(obj);
	}

	public static void RecycleAll<T>(this T prefab) where T : Component
	{
		ObjectPool.RecycleAll(prefab);
	}
	public static void RecycleAll(this GameObject prefab)
	{
		ObjectPool.RecycleAll(prefab);
	}

	public static bool IsPooled(this GameObject prefab)
	{
		return ObjectPool.IsPooled(prefab);
	}
	public static int CountPooled<T>(this T prefab) where T : Component
	{
		return ObjectPool.CountPooled(prefab);
	}
	public static int CountPooled(this GameObject prefab)
	{
		return ObjectPool.CountPooled(prefab);
	}

	public static int CountSpawned<T>(this T prefab) where T : Component
	{
		return ObjectPool.CountSpawned(prefab);
	}
	public static int CountSpawned(this GameObject prefab)
	{
		return ObjectPool.CountSpawned(prefab);
	}

	public static List<GameObject> GetSpawned(this GameObject prefab, List<GameObject> list, bool appendList)
	{
		return ObjectPool.GetSpawned(prefab, list, appendList);
	}
	public static List<GameObject> GetSpawned(this GameObject prefab, List<GameObject> list)
	{
		return ObjectPool.GetSpawned(prefab, list, false);
	}
	public static List<GameObject> GetSpawned(this GameObject prefab)
	{
		return ObjectPool.GetSpawned(prefab, null, false);
	}
	public static List<T> GetSpawned<T>(this T prefab, List<T> list, bool appendList) where T : Component
	{
		return ObjectPool.GetSpawned(prefab, list, appendList);
	}
	public static List<T> GetSpawned<T>(this T prefab, List<T> list) where T : Component
	{
		return ObjectPool.GetSpawned(prefab, list, false);
	}
	public static List<T> GetSpawned<T>(this T prefab) where T : Component
	{
		return ObjectPool.GetSpawned(prefab, null, false);
	}

	public static List<GameObject> GetPooled(this GameObject prefab, List<GameObject> list, bool appendList)
	{
		return ObjectPool.GetPooled(prefab, list, appendList);
	}
	public static List<GameObject> GetPooled(this GameObject prefab, List<GameObject> list)
	{
		return ObjectPool.GetPooled(prefab, list, false);
	}
	public static List<GameObject> GetPooled(this GameObject prefab)
	{
		return ObjectPool.GetPooled(prefab, null, false);
	}
	public static List<T> GetPooled<T>(this T prefab, List<T> list, bool appendList) where T : Component
	{
		return ObjectPool.GetPooled(prefab, list, appendList);
	}
	public static List<T> GetPooled<T>(this T prefab, List<T> list) where T : Component
	{
		return ObjectPool.GetPooled(prefab, list, false);
	}
	public static List<T> GetPooled<T>(this T prefab) where T : Component
	{
		return ObjectPool.GetPooled(prefab, null, false);
	}

	public static void DestroyPooled(this GameObject prefab)
	{
		ObjectPool.DestroyPooled(prefab);
	}
	public static void DestroyPooled<T>(this T prefab) where T : Component
	{
		ObjectPool.DestroyPooled(prefab.gameObject);
	}

	public static void DestroyAll(this GameObject prefab)
	{
		ObjectPool.DestroyAll(prefab);
	}
	public static void DestroyAll<T>(this T prefab) where T : Component
	{
		ObjectPool.DestroyAll(prefab.gameObject);
	}
}

public static class ComponentExtensions
{
	public static T GetOrAddComponent<T>(this Component comp, bool set_enable = false) where T : Component
	{
		T result = comp.GetComponent<T>();
		if (result == null)
			result = comp.gameObject.AddComponent<T>();

		var bcomp = result as Behaviour;
		if (set_enable && bcomp != null)
			bcomp.enabled = set_enable;

		return result;
	}

	public static T GetOrAddComponent<T>(this GameObject go, bool set_enable = false) where T : Component
	{
		T result = go.GetComponent<T>();
		if (result == null)
			result = go.AddComponent<T>();

		var bcomp = result as Behaviour;
		if (set_enable && bcomp != null)
			bcomp.enabled = set_enable;

		return result;
	}

	public static Component GetOrAddComponent(this Component comp, Type type, bool set_enable = false)
	{
		Component result = comp.GetComponent(type);
		if (result == null)
			result = comp.gameObject.AddComponent(type);

		var bcomp = result as Behaviour;
		if (set_enable && bcomp != null)
			bcomp.enabled = set_enable;

		return result;
	}

	public static Component GetOrAddComponent(this GameObject go, Type type, bool set_enable = false)
	{
		Component result = go.GetComponent(type);
		if (result == null)
			result = go.AddComponent(type);

		var bcomp = result as Behaviour;
		if (set_enable && bcomp != null)
			bcomp.enabled = set_enable;

		return result;
	}
}

public static class TransformExtentions
{
	public static Transform GetOrAddTransform(this Transform parent, string childName, Vector3 position, Vector3 roll)
	{
		Transform t = GetOrAddTransform(parent, childName);
		if (t != null)
		{
			t.localPosition = position;
			t.localRotation = Quaternion.Euler(roll);
		}

		return t;
	}

	public static Transform GetOrAddTransform(this Transform parent, string childName, Vector3 position, Quaternion rotation)
	{
		Transform t = GetOrAddTransform(parent, childName);
		if (t != null)
		{
			t.localPosition = position;
			t.localRotation = rotation;
		}

		return t;
	}

	public static Transform GetOrAddTransform(this Transform parent, string childName)
	{
		Transform t = parent.Find(childName);
		if (t == null)
		{
			t = new GameObject(childName).transform;
			t.SetParent(parent, false);
		}

		return t;
	}
}