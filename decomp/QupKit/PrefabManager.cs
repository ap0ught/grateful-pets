using System.Collections.Generic;
using UnityEngine;

namespace QupKit;

public static class PrefabManager
{
	private static Dictionary<string, GameObject> PrefabCache = new Dictionary<string, GameObject>();

	private static Dictionary<string, GameObject> RootPrefabCache = new Dictionary<string, GameObject>();

	public static GameObject Create(string Prefab, string Folder = "Prefabs/")
	{
		if (!PrefabCache.ContainsKey(Prefab))
		{
			PrefabCache.Add(Prefab, (GameObject)Resources.Load(Folder.IsNullOrEmpty() ? Prefab : (Folder + Prefab)));
		}
		return Object.Instantiate(PrefabCache[Prefab]);
	}

	public static GameObject Get(string Prefab, string Folder = "Prefabs")
	{
		if (!PrefabCache.ContainsKey(Prefab))
		{
			PrefabCache.Add(Prefab, (GameObject)Resources.Load(Folder.IsNullOrEmpty() ? Prefab : (Folder + Prefab)));
		}
		return PrefabCache[Prefab];
	}

	public static GameObject CreateRoot(string Prefab)
	{
		if (!RootPrefabCache.ContainsKey(Prefab))
		{
			RootPrefabCache.Add(Prefab, (GameObject)Resources.Load(Prefab));
		}
		return Object.Instantiate(RootPrefabCache[Prefab]);
	}
}
