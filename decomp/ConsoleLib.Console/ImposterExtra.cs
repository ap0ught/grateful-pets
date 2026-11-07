using System.Collections.Generic;
using UnityEngine;
using XRL.UI;

namespace ConsoleLib.Console;

public class ImposterExtra : IConsoleCharExtra
{
	public struct ImposterInfo
	{
		public int layer;

		public string prefab;

		public string config;

		public Vector3 offset;

		public ImposterInfo(string prefab, int layer = 0, string config = null)
		{
			this.prefab = prefab;
			offset = new Vector3(0f, 0f, 0f);
			this.layer = layer;
			this.config = config;
		}

		public ImposterInfo(string prefab, Vector2 offset, int layer = 0, string config = null)
		{
			this.prefab = prefab;
			this.offset = new Vector3(offset.x, offset.y, 0f);
			this.layer = layer;
			this.config = config;
		}

		public ImposterInfo(string prefab, Vector3 offset, int layer = 0, string config = null)
		{
			this.prefab = prefab;
			this.offset = offset;
			this.layer = layer;
			this.config = config;
		}

		public bool SameAs(ImposterInfo info)
		{
			if (prefab == info.prefab && offset == info.offset)
			{
				return config == info.config;
			}
			return false;
		}
	}

	private static List<GameObject>[] renderedImposters = new List<GameObject>[2000];

	public List<ImposterInfo> imposters = new List<ImposterInfo>();

	private static Dictionary<GameObject, string> objectNameCache = new Dictionary<GameObject, string>();

	private static GameObject imposterRoot;

	private static Dictionary<string, GameObject> prototypes = new Dictionary<string, GameObject>();

	private static Dictionary<string, Queue<GameObject>> pools = new Dictionary<string, Queue<GameObject>>();

	public void Add(string prefab, int layer = 0, string config = null)
	{
		imposters.Add(new ImposterInfo(prefab, layer, config));
	}

	public void Add(string prefab, Vector2 offset, int layer = 0, string config = null)
	{
		imposters.Add(new ImposterInfo(prefab, offset, layer, config));
	}

	public void Add(string prefab, Vector3 offset, int layer = 0, string config = null)
	{
		imposters.Add(new ImposterInfo(prefab, offset, layer, config));
	}

	public void Add(ImposterInfo newImposter)
	{
		imposters.Add(newImposter);
	}

	public void CopyFrom(ImposterExtra extra)
	{
		if (imposters.Count > 0)
		{
			imposters.Clear();
		}
		if (extra != null && extra.imposters.Count > 0)
		{
			imposters.AddRange(extra.imposters);
		}
	}

	public override IConsoleCharExtra Copy()
	{
		ImposterExtra imposterExtra = new ImposterExtra();
		imposterExtra.imposters.AddRange(imposters);
		return imposterExtra;
	}

	public override void Clear(bool overtyping)
	{
		imposters.Clear();
	}

	public override void AfterRender(int x, int y, ConsoleChar ch, ex3DSprite2 sprite, ScreenBuffer buffer)
	{
		List<GameObject> list = renderedImposters[x + y * 80];
		if (list != null && list.Count > 0)
		{
			for (int i = 0; i < list.Count; i++)
			{
				string value = "";
				if (list[i] != null && imposters != null && imposters.Count > i)
				{
					objectNameCache.TryGetValue(list[i], out value);
				}
				if (list[i] != null && (imposters == null || imposters.Count <= i || imposters[i].prefab != value))
				{
					free(list[i]);
					list[i] = null;
				}
			}
		}
		if (imposters == null || imposters.Count == 0)
		{
			list?.Clear();
		}
		else
		{
			if (Options.DisableImposters)
			{
				return;
			}
			if (list == null)
			{
				list = new List<GameObject>();
				renderedImposters[x + y * 80] = list;
			}
			while (list.Count < imposters.Count)
			{
				list.Add(null);
			}
			for (int j = 0; j < imposters.Count; j++)
			{
				if (!(list[j] == null) || imposters[j].prefab == null)
				{
					continue;
				}
				list[j] = next(imposters[j].prefab);
				list[j].transform.position = sprite.transform.position;
				list[j].transform.position += new Vector3(0f, 0f, -10f) + imposters[j].offset;
				if (imposters[j].config == null)
				{
					continue;
				}
				IConfigurableExtra[] components = list[j].GetComponents<IConfigurableExtra>();
				if (components != null)
				{
					IConfigurableExtra[] array = components;
					for (int k = 0; k < array.Length; k++)
					{
						array[k].Configure(imposters[j].config);
					}
				}
			}
		}
	}

	private static void free(GameObject prefab)
	{
		if (prefab != null)
		{
			prefab.SetActive(value: false);
			if (!pools.TryGetValue(prefab.name, out var value))
			{
				value = new Queue<GameObject>();
				pools.Add(prefab.name, value);
			}
			value.Enqueue(prefab);
		}
	}

	public static GameObject next(string prefab)
	{
		if (prefab == null)
		{
			return null;
		}
		if (!pools.TryGetValue(prefab, out var value))
		{
			value = new Queue<GameObject>();
			pools.Add(prefab, value);
		}
		if (value.Count == 0)
		{
			if (!prototypes.TryGetValue(prefab, out var value2))
			{
				value2 = Resources.Load(prefab) as GameObject;
				value2.SetActive(value: false);
				prototypes.Add(prefab, value2);
			}
			GameObject gameObject = Object.Instantiate(Resources.Load(prefab) as GameObject);
			if (imposterRoot == null)
			{
				imposterRoot = new GameObject("ImposterRoot");
			}
			gameObject.transform.parent = imposterRoot.transform;
			value.Enqueue(gameObject);
		}
		GameObject gameObject2 = value.Dequeue();
		gameObject2.name = prefab;
		gameObject2.SetActive(value: true);
		if (gameObject2 != null)
		{
			objectNameCache[gameObject2] = gameObject2.name;
		}
		return gameObject2;
	}
}
