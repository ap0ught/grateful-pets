using XRL.Rules;

namespace XRL.World.ZoneBuilders;

public class Craters : ZoneBuilderSandbox
{
	public bool BuildZone(Zone Z)
	{
		Z.GetCell(0, 0).AddObject(GameObjectFactory.Factory.CreateObject("DaylightWidget"));
		Z.GetCell(0, 0).AddObject(GameObjectFactory.Factory.CreateObject("Dirty"));
		for (int i = 0; i < Z.Width; i++)
		{
			for (int j = 0; j < Z.Height; j++)
			{
				if (Stat.Random(1, 100) <= 10)
				{
					Z.GetCell(i, j).AddObject("TerrainCraters");
				}
			}
		}
		return true;
	}
}
