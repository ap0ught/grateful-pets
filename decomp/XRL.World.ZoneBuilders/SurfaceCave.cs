using XRL.World.ZoneBuilders.Utility;

namespace XRL.World.ZoneBuilders;

public class SurfaceCave
{
	public bool BuildZone(Zone Z)
	{
		Z.GetCell(0, 0).AddObject(GameObjectFactory.Factory.CreateObject("DaylightWidget"));
		NoiseMap noiseMap = new NoiseMap(80, 25, 10, 3, 2, 5, 50, 50, 4, 1, 2, 1, null);
		int num = -1;
		for (int i = 0; i < noiseMap.nAreas; i++)
		{
			if (noiseMap.AreaNodes[i].Count > num)
			{
				num = noiseMap.AreaNodes[i].Count;
			}
		}
		for (int j = 0; j < Z.Width; j++)
		{
			for (int k = 0; k < Z.Height; k++)
			{
				if (noiseMap.Noise[j, k] > 1)
				{
					Z.GetCell(j, k).AddObject(GameObjectFactory.Factory.CreateObject("Sandstone"));
				}
			}
		}
		Z.ClearReachableMap();
		if (Z.BuildReachableMap(0, 0, bClearFirst: false) < 400)
		{
			return false;
		}
		return true;
	}
}
