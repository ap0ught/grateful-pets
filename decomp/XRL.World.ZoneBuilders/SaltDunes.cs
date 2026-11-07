using XRL.Rules;

namespace XRL.World.ZoneBuilders;

public class SaltDunes
{
	public bool BuildZone(Zone Z)
	{
		Z.GetCell(0, 0).AddObject("DaylightWidget");
		for (int i = 0; i < Z.Height; i++)
		{
			for (int j = 0; j < Z.Width; j++)
			{
				Cell cell = Z.GetCell(j, i);
				if (Stat.RandomCosmetic(1, 100) <= 5)
				{
					GameObject gameObject = cell.AddObject("HighSaltDune" + Stat.Random(1, 12) + (50.in100() ? "White" : "Cyan"));
					cell.PaintTile = gameObject.Render.Tile;
					cell.PaintColorString = gameObject.Render.ColorString;
				}
				else if (Stat.RandomCosmetic(1, 100) <= 60)
				{
					cell.PaintTile = "Terrain/sw_ground_desert_1.bmp";
					cell.PaintColorString = "&K";
				}
				else
				{
					cell.PaintTile = "Terrain/sw_ground_desert_" + Stat.Random(1, 12) + ".bmp";
					cell.PaintColorString = (50.in100() ? "&y" : "&c");
				}
			}
		}
		Z.ClearReachableMap();
		if (Z.BuildReachableMap(0, 0) < 400)
		{
			return false;
		}
		return true;
	}
}
