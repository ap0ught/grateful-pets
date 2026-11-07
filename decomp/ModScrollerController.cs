using System.Collections.Generic;
using EnhancedUI.EnhancedScroller;
using UnityEngine;
using XRL;

public class ModScrollerController : MonoBehaviour, IEnhancedScrollerDelegate
{
	private List<ModScrollerData> _data = new List<ModScrollerData>();

	public EnhancedScroller myScroller;

	public ModCellView animalCellViewPrefab;

	public void Refresh()
	{
		_data.Clear();
		foreach (ModInfo mod in ModManager.Mods)
		{
			if (mod.Source == ModSource.Local)
			{
				_data.Add(new ModScrollerData(mod));
			}
		}
		myScroller.ReloadData();
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
		return 20f;
	}

	public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
	{
		ModCellView obj = scroller.GetCellView(animalCellViewPrefab) as ModCellView;
		obj.SetData(_data[dataIndex]);
		return obj;
	}
}
