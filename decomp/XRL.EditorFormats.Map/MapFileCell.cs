using System;
using System.Collections.Generic;
using UnityEngine;
using XRL.World;

namespace XRL.EditorFormats.Map;

public class MapFileCell
{
	public bool Clear;

	public List<MapFileObjectBlueprint> Objects = new List<MapFileObjectBlueprint>();

	public HashSet<string> UsedBlueprints(HashSet<string> result)
	{
		if (result == null)
		{
			result = new HashSet<string>();
		}
		foreach (MapFileObjectBlueprint @object in Objects)
		{
			if (!result.Contains(@object.Name))
			{
				result.Add(@object.Name);
			}
		}
		return result;
	}

	public void Render(MapFileCellRender RenderCell, MapFileCellReference cref)
	{
		if (Objects.Count == 0)
		{
			RenderCell.Char = '.';
			RenderCell.Foreground = The.Color.Black;
			RenderCell.Background = The.Color.DarkBlack;
			RenderCell.Detail = The.Color.DarkBlack;
			return;
		}
		int num = -1;
		Dictionary<string, GameObjectBlueprint> blueprints = GameObjectFactory.Factory.Blueprints;
		foreach (MapFileObjectBlueprint @object in Objects)
		{
			try
			{
				if (!blueprints.TryGetValue(@object.Name, out var value))
				{
					RenderCell.Foreground = new Color(1f, 0f, 0f, 1f);
					RenderCell.Char = 'x';
					break;
				}
				int partParameter = value.GetPartParameter("Render", "RenderLayer", -1);
				if (partParameter > num)
				{
					num = partParameter;
					RenderCell.RenderBlueprint(value, cref);
				}
			}
			catch (Exception ex)
			{
				Debug.Log("Exception rendering " + @object.Name + " : " + ex.Message + ex.StackTrace);
			}
		}
	}

	public void ApplyTo(Cell C, bool CheckEmpty = true, Action<Cell> PreAction = null, Action<Cell> PostAction = null, Func<string, Cell, bool> ShouldPlace = null, Func<string, Cell, string> Replace = null, Action<XRL.World.GameObject> BeforeObjectCreated = null, Action<string, Cell> BeforePlacement = null, Action<string, Cell> AfterPlacement = null)
	{
		if (Clear)
		{
			C.Clear(null, Important: false, Combat: true);
		}
		if (CheckEmpty && Objects.Count == 0)
		{
			return;
		}
		PreAction?.Invoke(C);
		GameObjectFactory factory = GameObjectFactory.Factory;
		foreach (MapFileObjectBlueprint @object in Objects)
		{
			string text = @object.Name;
			if (ShouldPlace != null && !ShouldPlace(text, C))
			{
				continue;
			}
			if (Replace != null)
			{
				text = Replace(text, C);
			}
			if (text == null)
			{
				continue;
			}
			if (!factory.Blueprints.ContainsKey(text))
			{
				MetricsManager.LogError($"Unknown map object {text} at [{C.Y}, {C.Y}]");
				continue;
			}
			try
			{
				XRL.World.GameObject gameObject = @object.Create(BeforeObjectCreated);
				BeforePlacement?.Invoke(text, C);
				C.AddObject(gameObject);
				AfterPlacement?.Invoke(text, C);
			}
			catch (Exception x)
			{
				MetricsManager.LogError($"Error adding {@object.Name} at [{C.Y}, {C.Y}]:", x);
			}
		}
		PostAction?.Invoke(C);
	}
}
