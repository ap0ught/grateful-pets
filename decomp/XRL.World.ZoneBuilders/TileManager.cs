using System.Collections.Generic;
using System.Text;

namespace XRL.World.ZoneBuilders;

public class TileManager
{
	public Dictionary<string, int> Markers = new Dictionary<string, int>();

	public int TileSize = 6;

	public List<BuildingTile> Tiles = new List<BuildingTile>();

	public void AddTile(List<string> Tempalate, bool bMirror, bool bFlip)
	{
		Tiles.Add(new BuildingTile(Tempalate));
		if (bMirror)
		{
			Tiles.Add(new BuildingTile(Mirror(Tempalate)));
			Tiles.Add(new BuildingTile(Reverse(Tempalate)));
			if (bFlip)
			{
				Tiles.Add(new BuildingTile(Mirror(Reverse(Tempalate))));
			}
			Tiles.Add(new BuildingTile(RotateLeft(Tempalate)));
			if (bFlip)
			{
				Tiles.Add(new BuildingTile(Mirror(RotateLeft(Tempalate))));
			}
			Tiles.Add(new BuildingTile(RotateRight(Tempalate)));
			if (bFlip)
			{
				Tiles.Add(new BuildingTile(Mirror(RotateRight(Tempalate))));
			}
		}
	}

	public List<string> RotateRight(List<string> Source)
	{
		List<string> list = new List<string>();
		for (int i = 0; i < TileSize; i++)
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int j = 0; j < TileSize; j++)
			{
				stringBuilder.Append(Source[TileSize - 1 - j][i]);
			}
			list.Add(stringBuilder.ToString());
		}
		return list;
	}

	public List<string> RotateLeft(List<string> Source)
	{
		List<string> list = new List<string>();
		for (int i = 0; i < TileSize; i++)
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int j = 0; j < TileSize; j++)
			{
				stringBuilder.Append(Source[j][TileSize - 1 - i]);
			}
			list.Add(stringBuilder.ToString());
		}
		return list;
	}

	public List<string> Mirror(List<string> Source)
	{
		List<string> list = new List<string>();
		for (int num = TileSize - 1; num >= 0; num--)
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int num2 = TileSize - 1; num2 >= 0; num2--)
			{
				stringBuilder.Append(Source[num][num2]);
			}
			list.Add(stringBuilder.ToString());
		}
		return list;
	}

	public List<string> Reverse(List<string> Source)
	{
		List<string> list = new List<string>(Source);
		list.Reverse();
		return list;
	}
}
