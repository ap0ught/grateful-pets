using XRL.Rules;
using XRL.World.Parts;
using XRL.World.ZoneBuilders.Utility;

namespace XRL.World.ZoneBuilders;

public class ApegodCave
{
	public bool BuildZone(Zone Z)
	{
		NoiseMap noiseMap = new NoiseMap(80, 25, 10, 3, 3, 6, 80, 80, 4, 3, 0, 1, null);
		for (int i = 0; i < Z.Height; i++)
		{
			for (int j = 0; j < Z.Width; j++)
			{
				if (noiseMap.Noise[j, i] > 1)
				{
					if (Stat.Rnd.Next(100) < 20)
					{
						if (!Z.GetCell(j, i).IsOccluding())
						{
							Z.GetCell(j, i).AddObject(GameObjectFactory.Factory.CreateObject("Ziv Bough"));
						}
					}
					else if (Stat.Rnd.Next(100) < 20)
					{
						if (!Z.GetCell(j, i).IsOccluding())
						{
							Z.GetCell(j, i).AddObject(GameObjectFactory.Factory.CreateObject("Swarmshade Tree"));
						}
					}
					else if (Stat.Rnd.Next(100) < 10)
					{
						if (!Z.GetCell(j, i).IsOccluding())
						{
							Z.GetCell(j, i).AddObject(GameObjectFactory.Factory.CreateObject("Shimscale Mangrove Tree"));
						}
					}
					else if (Stat.Rnd.Next(100) < 1 && !Z.GetCell(j, i).IsOccluding())
					{
						Z.GetCell(j, i).AddObject(GameObjectFactory.Factory.CreateObject("Banana Tree"));
					}
				}
				Grassy.PaintCell(Z.GetCell(j, i));
			}
		}
		Z.GetCell(0, 0).AddObject(GameObjectFactory.Factory.CreateObject("DaylightWidget"));
		return true;
	}
}
