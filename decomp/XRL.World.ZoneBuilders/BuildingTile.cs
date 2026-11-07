using System.Collections.Generic;

namespace XRL.World.ZoneBuilders;

public class BuildingTile
{
	public List<string> Template;

	public TileType[,] Tile;

	public char[,] CustomTile;

	public BuildingTile()
	{
	}

	public BuildingTile(List<string> Template)
	{
		int length = Template[0].Length;
		for (int i = 0; i < length; i++)
		{
		}
		Tile = new TileType[length, length];
		CustomTile = new char[length, length];
		for (int j = 0; j < length; j++)
		{
			for (int k = 0; k < length; k++)
			{
				Tile[k, j] = TileType.Custom;
				if (Template[j][k] == '%')
				{
					Tile[k, j] = TileType.Garbage;
				}
				if (Template[j][k] == 'c')
				{
					Tile[k, j] = TileType.LittleChest;
				}
				if (Template[j][k] == 'C')
				{
					Tile[k, j] = TileType.BigChest;
				}
				if (Template[j][k] == '1')
				{
					Tile[k, j] = TileType.Creature1;
				}
				if (Template[j][k] == '.')
				{
					Tile[k, j] = TileType.Open;
				}
				if (Template[j][k] == ',')
				{
					Tile[k, j] = TileType.OpenConnect;
				}
				if (Template[j][k] == '#')
				{
					Tile[k, j] = TileType.Wall;
				}
				if (Template[j][k] == 'D')
				{
					Tile[k, j] = TileType.Door;
				}
				if (Template[j][k] == '?')
				{
					Tile[k, j] = TileType.Any;
				}
				if (Template[j][k] == '~')
				{
					Tile[k, j] = TileType.Liquid;
				}
				CustomTile[k, j] = Template[j][k];
			}
		}
		this.Template = Template;
	}

	public void Trace()
	{
		if (Template == null)
		{
			return;
		}
		foreach (string item in Template)
		{
			_ = item;
		}
	}

	public static bool Matches(TileType T1, TileType T2)
	{
		if (T1 == TileType.Any)
		{
			return true;
		}
		if (T2 == TileType.Any)
		{
			return true;
		}
		if (T1 == TileType.Open && T2 != TileType.Open)
		{
			return false;
		}
		if (T2 == TileType.Open && T1 != TileType.Open)
		{
			return false;
		}
		return true;
	}
}
