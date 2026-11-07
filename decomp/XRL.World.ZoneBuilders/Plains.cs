using XRL.Rules;

namespace XRL.World.ZoneBuilders;

public class Plains
{
	public bool BuildZone(Zone Z)
	{
		for (int i = 0; i < Z.Height; i++)
		{
			for (int j = 0; j < Z.Width; j++)
			{
				if (2.in100())
				{
					if (!Z.GetCell(j, i).IsOccluding())
					{
						Z.GetCell(j, i).AddObject("Dogthorn Tree");
					}
				}
				else if (2.in100())
				{
					if (!Z.GetCell(j, i).IsOccluding())
					{
						Z.GetCell(j, i).AddObject("Witchwood Tree");
					}
				}
				else if (Stat.Rnd.Next(3000) < 2 && !Z.GetCell(j, i).IsOccluding())
				{
					Z.GetCell(j, i).AddObject(GameObjectFactory.Factory.CreateObject("Starapple Tree"));
				}
			}
		}
		Z.GetCell(0, 0).AddObject("DaylightWidget");
		Z.GetCell(0, 0).AddObject("Grassy");
		for (int k = 0; k < Z.Height; k++)
		{
			for (int l = 0; l < Z.Width; l++)
			{
				if (Z.GetCell(l, k).IsEmpty())
				{
					Z.ClearReachableMap(bValue: true);
					if (Z.BuildReachableMap(l, k) > 400)
					{
						return true;
					}
				}
			}
		}
		return true;
	}
}
