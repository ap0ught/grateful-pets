using System;
using ConsoleLib.Console;
using Kobold;
using UnityEngine;
using UnityEngine.UI;
using XRL.World;
using XRL.World.Parts;

namespace XRL.EditorFormats.Map;

public class MapFileCellRender
{
	public string Tile;

	public char Char;

	public Color Foreground = Color.grey;

	public Color Background = Color.black;

	public Color Detail = Color.black;

	public void Reset()
	{
		Tile = null;
		Char = '\0';
		Detail = (Background = (Foreground = The.Color.DarkBlack));
	}

	public void RenderBlueprint(GameObjectBlueprint bp, MapFileCellReference cref)
	{
		if (!bp.HasPart("Render"))
		{
			return;
		}
		Render.ProcessRenderString(bp.GetPartParameter<string>("Render", "RenderString"));
		string partParameter = bp.GetPartParameter("Render", "DetailColor", "k");
		try
		{
			if (partParameter.Length == 0)
			{
				Debug.Log("null detail color");
			}
			else if (!ConsoleLib.Console.ColorUtility.ColorMap.ContainsKey(partParameter[0]))
			{
				Debug.LogWarning($"Unknown detail color {partParameter[0]}");
			}
			else
			{
				Detail = ConsoleLib.Console.ColorUtility.ColorMap[partParameter[0]];
			}
		}
		catch (Exception)
		{
		}
		if (!bp.Tags.TryGetValue("PaintWith", out var value))
		{
			value = "";
		}
		string partParameter2 = bp.GetPartParameter<string>("Render", "RenderString");
		if (partParameter2.Length > 0)
		{
			Char = partParameter2[0];
		}
		Tile = bp.GetPartParameter<string>("Render", "Tile");
		if (Tile.IsNullOrEmpty())
		{
			Tile = null;
		}
		else
		{
			Char = '\0';
		}
		string partParameter3 = bp.GetPartParameter<string>("Render", "TileColor");
		if (partParameter3.IsNullOrEmpty() || Tile.IsNullOrEmpty())
		{
			partParameter3 = bp.GetPartParameter<string>("Render", "ColorString");
		}
		char? foreground = 'y';
		char? background = 'k';
		ConsoleLib.Console.ColorUtility.FindLastForegroundAndBackground(partParameter3, ref foreground, ref background);
		Foreground = ConsoleLib.Console.ColorUtility.ColorMap[foreground.Value];
		Background = ConsoleLib.Console.ColorUtility.ColorMap[background.Value];
		string value3;
		if (bp.Tags.TryGetValue("PaintedFence", out var value2))
		{
			if (bp.Tags.TryGetValue("PaintedFenceAtlas", out value3))
			{
				Tile = value3;
			}
			else
			{
				Tile = "Tiles/";
			}
			Tile = Tile + value2.GetRandomSubstring(',', Trim: false, new System.Random(cref.x ^ cref.y)) + "_";
			if (cref.region.width == 1 && cref.region.height == 1)
			{
				Tile += "ew";
			}
			else
			{
				Tile += (cref.region.HasBlueprintInCell(cref.x, cref.y - 1, bp.Name, value) ? "n" : "");
				Tile += (cref.region.HasBlueprintInCell(cref.x, cref.y + 1, bp.Name, value) ? "s" : "");
				Tile += (cref.region.HasBlueprintInCell(cref.x + 1, cref.y, bp.Name, value) ? "e" : "");
				Tile += (cref.region.HasBlueprintInCell(cref.x - 1, cref.y, bp.Name, value) ? "w" : "");
			}
			if (bp.Tags.TryGetValue("PaintedFenceExtension", out value2))
			{
				Tile += value2;
			}
			else
			{
				Tile += ".bmp";
			}
		}
		else if (bp.Tags.TryGetValue("PaintedWall", out value2))
		{
			if (bp.Tags.TryGetValue("PaintedWallAtlas", out value3))
			{
				Tile = value3;
			}
			else
			{
				Tile = "Tiles/";
			}
			Tile += value2.GetRandomSubstring(',', Trim: false, new System.Random(cref.x ^ cref.y));
			if (bp.Tags.ContainsKey("PaintedCheckerboard"))
			{
				Tile += (((cref.x + cref.y) % 2 == 0) ? "1" : "2");
			}
			Tile += "-";
			Tile += (cref.region.HasBlueprintInCell(cref.x, cref.y - 1, bp.Name, value) ? "1" : "0");
			Tile += (cref.region.HasBlueprintInCell(cref.x + 1, cref.y - 1, bp.Name, value) ? "1" : "0");
			Tile += (cref.region.HasBlueprintInCell(cref.x + 1, cref.y, bp.Name, value) ? "1" : "0");
			Tile += (cref.region.HasBlueprintInCell(cref.x + 1, cref.y + 1, bp.Name, value) ? "1" : "0");
			Tile += (cref.region.HasBlueprintInCell(cref.x, cref.y + 1, bp.Name, value) ? "1" : "0");
			Tile += (cref.region.HasBlueprintInCell(cref.x - 1, cref.y + 1, bp.Name, value) ? "1" : "0");
			Tile += (cref.region.HasBlueprintInCell(cref.x - 1, cref.y, bp.Name, value) ? "1" : "0");
			Tile += (cref.region.HasBlueprintInCell(cref.x - 1, cref.y - 1, bp.Name, value) ? "1" : "0");
			if (bp.Tags.TryGetValue("PaintedWallExtension", out value2))
			{
				Tile += value2;
			}
			else
			{
				Tile += ".bmp";
			}
		}
		else if (bp.Tags.TryGetValue("PaintedLiquid", out value2))
		{
			if (bp.Tags.TryGetValue("PaintedLiquidAtlas", out value3))
			{
				Tile = value3;
			}
			else
			{
				Tile = "Assets_Content_Textures_Water_";
			}
			Tile = Tile + value2 + "-";
			Tile += (cref.region.HasBlueprintInCell(cref.x, cref.y - 1, bp.Name, value) ? "1" : "0");
			Tile += (cref.region.HasBlueprintInCell(cref.x + 1, cref.y - 1, bp.Name, value) ? "1" : "0");
			Tile += (cref.region.HasBlueprintInCell(cref.x + 1, cref.y, bp.Name, value) ? "1" : "0");
			Tile += (cref.region.HasBlueprintInCell(cref.x + 1, cref.y + 1, bp.Name, value) ? "1" : "0");
			Tile += (cref.region.HasBlueprintInCell(cref.x, cref.y + 1, bp.Name, value) ? "1" : "0");
			Tile += (cref.region.HasBlueprintInCell(cref.x - 1, cref.y + 1, bp.Name, value) ? "1" : "0");
			Tile += (cref.region.HasBlueprintInCell(cref.x - 1, cref.y, bp.Name, value) ? "1" : "0");
			Tile += (cref.region.HasBlueprintInCell(cref.x - 1, cref.y - 1, bp.Name, value) ? "1" : "0");
			if (bp.Tags.TryGetValue("PaintedWallExtension", out value2))
			{
				Tile += value2;
			}
			else
			{
				Tile += ".bmp";
			}
		}
	}

	public void To3C(UIThreeColorProperties img)
	{
		if (Char == '\0')
		{
			img.image.sprite = SpriteManager.GetUnitySprite(Tile);
			img.SetColors(Foreground, Detail, Background);
			return;
		}
		Image image = img.image;
		int num = Char;
		image.sprite = SpriteManager.GetUnitySprite("Text/" + num + ".bmp");
		img.SetColors(Background, Foreground, Foreground);
	}
}
