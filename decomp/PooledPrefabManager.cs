using System.Collections.Generic;
using QupKit;
using UnityEngine;

public static class PooledPrefabManager
{
	private static Dictionary<GameObject, Queue<GameObject>> pools = new Dictionary<GameObject, Queue<GameObject>>();

	public static void MakeTemporary(GameObject prefabToMakeTemporary, float timeToLive)
	{
		PooledPrefabTemporary component = prefabToMakeTemporary.GetComponent<PooledPrefabTemporary>();
		if (component == null)
		{
			Debug.Log("Didn't find temporary on: " + prefabToMakeTemporary.name);
		}
		component.isTemporary = true;
		component.timeToLive = timeToLive;
	}

	public static void Prewarm(GameObject prefabToPrewarm, int howMany)
	{
		List<GameObject> list = new List<GameObject>();
		for (int i = 0; i < howMany; i++)
		{
			list.Add(Instantiate(prefabToPrewarm));
		}
		for (int j = 0; j < howMany; j++)
		{
			Return(list[j]);
		}
	}

	public static GameObject InstantiateTemporary(GameObject prefab, float timeToLive)
	{
		GameObject gameObject = Instantiate(prefab);
		gameObject.GetComponent<PooledPrefabTemporary>().isTemporary = true;
		gameObject.GetComponent<PooledPrefabTemporary>().timeToLive = timeToLive;
		return gameObject;
	}

	public static GameObject Instantiate(string Prefab, string Folder = "Prefabs/")
	{
		return Instantiate(PrefabManager.Get(Prefab, Folder));
	}

	public static GameObject Instantiate(GameObject prefab)
	{
		if (!pools.ContainsKey(prefab))
		{
			pools.Add(prefab, new Queue<GameObject>());
		}
		GameObject gameObject = null;
		while (pools[prefab].Count > 0)
		{
			gameObject = pools[prefab].Dequeue();
			if (gameObject != null)
			{
				gameObject.SetActive(value: true);
				gameObject.SendMessage("PoolReset", SendMessageOptions.DontRequireReceiver);
				break;
			}
		}
		if (gameObject == null)
		{
			gameObject = Object.Instantiate(prefab);
			gameObject.AddComponent<PooledPrefab>().originalPrefab = prefab;
			gameObject.AddComponent<PooledPrefabTemporary>();
			gameObject.SetActive(value: true);
		}
		gameObject.GetComponent<PooledPrefabTemporary>().isTemporary = false;
		return gameObject;
	}

	public static GameObject InstantiateCanvasGroupStyle(GameObject prefab)
	{
		if (!pools.ContainsKey(prefab))
		{
			pools.Add(prefab, new Queue<GameObject>());
		}
		GameObject gameObject = null;
		while (pools[prefab].Count > 0)
		{
			gameObject = pools[prefab].Dequeue();
			if (gameObject != null)
			{
				CanvasGroup component = gameObject.GetComponent<CanvasGroup>();
				component.alpha = 1f;
				component.interactable = true;
				component.blocksRaycasts = true;
				gameObject.SendMessage("PoolReset", SendMessageOptions.DontRequireReceiver);
				break;
			}
		}
		if (gameObject == null)
		{
			gameObject = Object.Instantiate(prefab);
			gameObject.AddComponent<PooledPrefab>().originalPrefab = prefab;
			gameObject.AddComponent<PooledPrefabTemporary>();
		}
		gameObject.GetComponent<PooledPrefabTemporary>().isTemporary = false;
		return gameObject;
	}

	public static void ReturnCanvasGroupStyle(GameObject pooledPrefab)
	{
		pooledPrefab.SendMessage("OnReturnToPool", SendMessageOptions.DontRequireReceiver);
		if (pooledPrefab.GetComponent<PooledPrefab>() == null)
		{
			Object.Destroy(pooledPrefab);
		}
		else if (pooledPrefab.activeInHierarchy)
		{
			if (pooledPrefab.GetComponent<PooledPrefabTemporary>() != null)
			{
				pooledPrefab.GetComponent<PooledPrefabTemporary>().isTemporary = false;
			}
			GameObject originalPrefab = pooledPrefab.GetComponent<PooledPrefab>().originalPrefab;
			pooledPrefab.transform.parent = null;
			CanvasGroup component = pooledPrefab.GetComponent<CanvasGroup>();
			component.alpha = 0f;
			component.interactable = false;
			component.blocksRaycasts = false;
			pools[originalPrefab].Enqueue(pooledPrefab);
		}
	}

	public static void Return(GameObject pooledPrefab)
	{
		if (pooledPrefab == null)
		{
			return;
		}
		pooledPrefab.SendMessage("OnReturnToPool", SendMessageOptions.DontRequireReceiver);
		if (pooledPrefab.GetComponent<PooledPrefab>() == null)
		{
			Object.Destroy(pooledPrefab);
			return;
		}
		if (pooledPrefab.GetComponent<PooledPrefabTemporary>() != null)
		{
			pooledPrefab.GetComponent<PooledPrefabTemporary>().isTemporary = false;
		}
		GameObject originalPrefab = pooledPrefab.GetComponent<PooledPrefab>().originalPrefab;
		pooledPrefab.transform.SetParent(null);
		pooledPrefab.SetActive(value: false);
		pools[originalPrefab].Enqueue(pooledPrefab);
	}
}
