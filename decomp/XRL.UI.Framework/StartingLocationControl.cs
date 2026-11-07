using System;
using System.Collections.Generic;
using ConsoleLib.Console;
using Qud.UI;
using UnityEngine;

namespace XRL.UI.Framework;

public class StartingLocationControl : MonoBehaviour, IFrameworkControl
{
	public UITextSkin text;

	public GameObject grid;

	protected Dictionary<string, UIThreeColorProperties> _tiles;

	protected Dictionary<string, IRenderable> originals = new Dictionary<string, IRenderable>();

	public Color disabledColor = new Color(0.259f, 0.392f, 0.439f);

	private bool? selected;

	protected Dictionary<string, UIThreeColorProperties> tiles
	{
		get
		{
			if (_tiles == null)
			{
				_tiles = new Dictionary<string, UIThreeColorProperties>();
				foreach (Transform item in grid.transform)
				{
					_tiles.Add(item.name, item.GetComponent<UIThreeColorProperties>());
				}
			}
			return _tiles;
		}
	}

	public void setData(FrameworkDataElement data)
	{
		GetComponent<ImageTinyFrame>().image = null;
		if (!(data is StartingLocationData startingLocationData))
		{
			throw new ArgumentException("StartingLocationData expected StartingLocationControl data");
		}
		GetComponent<TitledIconButton>().SetTitle(startingLocationData.Name + (string.IsNullOrEmpty(startingLocationData.Hotkey) ? "" : ("\n" + startingLocationData.Hotkey)));
		originals.Clear();
		foreach (KeyValuePair<string, StartingLocationGridElement> item in startingLocationData.grid)
		{
			StartingLocationGridElement value = item.Value;
			originals.Add(item.Key, value);
		}
		UpdateTiles(toSelected: false);
	}

	public NavigationContext GetNavigationContext()
	{
		return GetComponent<FrameworkContext>()?.context;
	}

	private void UpdateTiles(bool toSelected)
	{
		if (selected == toSelected)
		{
			return;
		}
		selected = toSelected;
		foreach (KeyValuePair<string, UIThreeColorProperties> tile in tiles)
		{
			tile.Value.FromRenderable(originals[tile.Key]);
			if (selected == false)
			{
				tile.Value.SetColors(disabledColor, disabledColor, new Color(0f, 0f, 0f, 0f));
			}
		}
	}

	private void Update()
	{
		Sync();
	}

	public void Sync()
	{
		bool valueOrDefault = GetComponentInParent<FrameworkContext>()?.context?.IsActive() == true;
		UpdateTiles(valueOrDefault);
	}
}
