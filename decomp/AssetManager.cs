using System.Collections.Generic;
using UnityEngine;

public static class AssetManager
{
	private static Dictionary<string, GameObject> Borders;

	private static GameObject _BuildingBackplatePrefab;

	private static GameObject _ResultsBackplatePrefab;

	private static GameObject _BrownButtonPrefab;

	private static GameObject _GenericButtonPrefab;

	private static GameObject _TinyFontPrefab;

	private static GameObject _SmallFontPrefab;

	private static GameObject _GenericDialogPrefab;

	private static GameObject _MediumFontPrefab;

	public static GameObject BuildingBackplateInstance
	{
		get
		{
			if (_BuildingBackplatePrefab == null)
			{
				_BuildingBackplatePrefab = (GameObject)Resources.Load("Prefabs/BuildingBackplate");
			}
			return Object.Instantiate(_BuildingBackplatePrefab);
		}
	}

	public static GameObject ResultsBackplatePrefab
	{
		get
		{
			if (_ResultsBackplatePrefab == null)
			{
				_ResultsBackplatePrefab = (GameObject)Resources.Load("Images/UI/Results/ResultTileBorderPrefab");
			}
			return Object.Instantiate(_ResultsBackplatePrefab);
		}
	}

	public static GameObject BrownButtonPrefab
	{
		get
		{
			if (_BrownButtonPrefab == null)
			{
				_BrownButtonPrefab = (GameObject)Resources.Load("Images/UI/BrownButtonPrefab");
			}
			return Object.Instantiate(_BrownButtonPrefab);
		}
	}

	public static GameObject GenericButtonInstance
	{
		get
		{
			if (_GenericButtonPrefab == null)
			{
				_GenericButtonPrefab = (GameObject)Resources.Load("Prefabs/GenericButton");
			}
			return Object.Instantiate(_GenericButtonPrefab);
		}
	}

	public static GameObject TinyFontInstance
	{
		get
		{
			if (_TinyFontPrefab == null)
			{
				_TinyFontPrefab = (GameObject)Resources.Load("Fonts/FontPrefabPixel2");
			}
			return Object.Instantiate(_TinyFontPrefab);
		}
	}

	public static GameObject SmallFontInstance
	{
		get
		{
			if (_SmallFontPrefab == null)
			{
				_SmallFontPrefab = (GameObject)Resources.Load("Fonts/FontPrefabPixel2");
			}
			return Object.Instantiate(_SmallFontPrefab);
		}
	}

	public static GameObject GenericDialogInstance
	{
		get
		{
			if (_GenericDialogPrefab == null)
			{
				_GenericDialogPrefab = (GameObject)Resources.Load("Prefabs/BuildingBackplate");
			}
			return Object.Instantiate(_GenericDialogPrefab);
		}
	}

	public static GameObject MediumFontInstance
	{
		get
		{
			if (_MediumFontPrefab == null)
			{
				_MediumFontPrefab = (GameObject)Resources.Load("Fonts/FontPrefabPixel2");
			}
			return Object.Instantiate(_MediumFontPrefab);
		}
	}

	public static GameObject NewBorder(string PrefabID)
	{
		if (Borders == null)
		{
			Borders = new Dictionary<string, GameObject>();
		}
		if (!Borders.ContainsKey(PrefabID))
		{
			Borders.Add(PrefabID, null);
		}
		if (Borders[PrefabID] == null)
		{
			Borders[PrefabID] = (GameObject)Resources.Load(PrefabID);
		}
		return Object.Instantiate(Borders[PrefabID]);
	}
}
