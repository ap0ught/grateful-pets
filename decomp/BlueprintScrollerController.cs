using System.Collections.Generic;
using EnhancedUI.EnhancedScroller;
using UnityEngine;
using XRL.World;

public class BlueprintScrollerController : MonoBehaviour, IEnhancedScrollerDelegate
{
	private List<BlueprintScrollerData> _data = new List<BlueprintScrollerData>();

	public EnhancedScroller myScroller;

	public BlueprintCellView animalCellViewPrefab;

	public Dictionary<string, BlueprintScrollerData> scrollDataCache = new Dictionary<string, BlueprintScrollerData>();

	private List<GameObjectBlueprint> _bps;

	public List<GameObjectBlueprint> Blueprints
	{
		get
		{
			if (_bps != null)
			{
				return _bps;
			}
			_bps = new List<GameObjectBlueprint>(GameObjectFactory.Factory.BlueprintList);
			_bps.Sort((GameObjectBlueprint a, GameObjectBlueprint b) => string.Compare(a.Name, b.Name));
			return _bps;
		}
	}

	public void ResetCaches()
	{
		_bps = null;
		_data.Clear();
		scrollDataCache.Clear();
	}

	public void UpdateDataFilter(string ObjectFilter, HashSet<string> SecondaryFilter)
	{
		_data.Clear();
		string value = "";
		if (ObjectFilter != "")
		{
			value = ObjectFilter.ToUpper();
		}
		foreach (GameObjectBlueprint blueprint in Blueprints)
		{
			if ((ObjectFilter == "" || blueprint.Name.ToUpper().Contains(value)) && (SecondaryFilter == null || SecondaryFilter.Contains(blueprint.Name)))
			{
				if (!scrollDataCache.TryGetValue(blueprint.Name, out var value2))
				{
					value2 = new BlueprintScrollerData(blueprint.Name);
					scrollDataCache[blueprint.Name] = value2;
				}
				_data.Add(value2);
			}
		}
		myScroller.ReloadData();
	}

	public void UpdateSelected()
	{
		myScroller.RefreshActiveCellViews();
	}

	private void Start()
	{
		myScroller.Delegate = this;
	}

	public int GetNumberOfCells(EnhancedScroller scroller)
	{
		return _data.Count;
	}

	public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
	{
		return 24f;
	}

	public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
	{
		BlueprintCellView obj = scroller.GetCellView(animalCellViewPrefab) as BlueprintCellView;
		obj.SetData(_data[dataIndex]);
		return obj;
	}
}
