using XRL.Rules;

namespace XRL.World.Parts;

public static class DirtPicker
{
	public static string GetRandomGrassTile()
	{
		int num = Stat.Random(1, 4);
		if (num <= 1)
		{
			return "assets_content_textures_tiles_tile-grass1.png";
		}
		if (num <= 2)
		{
			return "Terrain/sw_grass1.bmp";
		}
		if (num <= 3)
		{
			return "Terrain/sw_grass2.bmp";
		}
		return "Terrain/sw_grass3.bmp";
	}
}
