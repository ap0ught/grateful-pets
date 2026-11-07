using System;
using UnityEngine;

namespace QupKit;

public static class GameObjectExtensions
{
	public static GameObject AddChild(this GameObject go, GameObject childGo)
	{
		if (childGo.transform.parent != go)
		{
			childGo.transform.SetParent(go.transform, worldPositionStays: false);
		}
		return childGo;
	}

	public static void ForeachChild(this GameObject go, Action<GameObject> action)
	{
		for (int num = go.transform.childCount - 1; num >= 0; num--)
		{
			action(go.transform.GetChild(num).gameObject);
		}
	}
}
