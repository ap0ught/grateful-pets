using UnityEngine;
using XRL.UI;

namespace XRL;

public static class Prefs
{
	public static NameValueBag Bag;

	public static void Init()
	{
		Debug.Log("Loading user prefs");
		Bag = new NameValueBag(DataManager.SyncedPath("UserPrefs.json"));
		Bag.Load();
	}

	public static bool HasString(string Name)
	{
		if (Bag == null)
		{
			Init();
		}
		return Bag.Bag.ContainsKey(Name);
	}

	public static string GetString(string Name, string Default = null)
	{
		if (Bag == null)
		{
			Init();
		}
		lock (Bag)
		{
			if (Bag.Bag.TryGetValue(Name, out var value))
			{
				return value;
			}
			return Default;
		}
	}

	public static void SetString(string Name, string Value)
	{
		if (Bag == null)
		{
			Init();
		}
		lock (Bag)
		{
			Bag.SetValue(Name, Value);
		}
	}
}
