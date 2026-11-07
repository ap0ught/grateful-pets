using System.Collections.Generic;
using UnityEngine;
using XRL.UI;
using XRL.World;

namespace Qud.UI;

[ExecuteAlways]
public class StatusBarStatBlock : MonoBehaviour
{
	public UnityEngine.GameObject TextPrefab;

	public UnityEngine.GameObject SpacerPrefab;

	public List<string> Stats;

	public List<Color> Colors;

	public bool Refresh;

	private List<UnityEngine.GameObject> _texts = new List<UnityEngine.GameObject>();

	public void Awake()
	{
		Refresh = true;
	}

	public void Update()
	{
		if (!Refresh || !Application.isPlaying)
		{
			return;
		}
		Refresh = false;
		List<Transform> list = new List<Transform>(base.gameObject.transform.childCount);
		foreach (Transform item in base.gameObject.transform)
		{
			list.Add(item);
		}
		foreach (Transform item2 in list)
		{
			item2.gameObject.DestroyImmediate();
		}
		_texts.Clear();
		for (int i = 0; i < Stats.Count; i++)
		{
			if (i > 0)
			{
				SpacerPrefab.Instantiate().transform.SetParent(base.gameObject.transform, worldPositionStays: false);
			}
			UnityEngine.GameObject gameObject = TextPrefab.Instantiate();
			gameObject.transform.SetParent(base.gameObject.transform, worldPositionStays: false);
			gameObject.GetComponent<UITextSkin>().SetText(Stats[i].Substring(0, 2).ToUpper() + ": 99");
			gameObject.GetComponent<UITextSkin>().color = Colors[i];
			_texts.Add(gameObject);
		}
	}

	public void UpdateStats(Dictionary<string, string> stats)
	{
		for (int i = 0; i < Stats.Count && i < _texts.Count; i++)
		{
			if (stats.TryGetValue(Stats[i], out var value))
			{
				string statShortName = Statistic.GetStatShortName(Stats[i]);
				_texts[i].GetComponent<UITextSkin>().SetText($"{statShortName}: {value}");
			}
		}
	}
}
