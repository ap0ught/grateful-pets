using System.Collections.Generic;
using Genkit;
using XRL.Core;
using XRL.Rules;

namespace XRL.World.ZoneBuilders;

public class VillageOver : ZoneBuilderSandbox
{
	public bool BuildZone(Zone Z)
	{
		Z.Clear();
		Z.Fill("Air");
		Z.GetCell(0, 0).AddObject("DaylightWidget");
		Z.SetZoneProperty("relaxedbiomes", "true");
		List<Location2D> list = new List<Location2D>();
		foreach (CachedZoneConnection item2 in Z.ZoneConnectionCache)
		{
			if (item2.TargetDirection == "-" && item2.Type == "aerie")
			{
				list.Add(Location2D.Get(item2.X, item2.Y));
			}
		}
		foreach (ZoneConnection zoneConnection in XRLCore.Core.Game.ZoneManager.GetZoneConnections(Z.ZoneID))
		{
			if (zoneConnection.Type == "aerie")
			{
				list.Add(Location2D.Get(zoneConnection.X, zoneConnection.Y));
			}
		}
		List<Box> list2 = new List<Box>();
		string blueprint = PopulationManager.RollOneFrom("AerieTemplates").Blueprint;
		foreach (Location2D item3 in list)
		{
			ColorOutputMap randomElement = getWfcBuildingTemplate(blueprint).GetRandomElement();
			int num = randomElement.width / 2;
			int num2 = randomElement.height / 2;
			int num3 = Stat.Random(1, 3);
			Box item = new Box(item3.X - num3, item3.Y - num3, item3.X + num3, item3.Y + num3).clamp(1, 1, 78, 23);
			for (int i = 0; i < randomElement.width; i++)
			{
				for (int j = 0; j < randomElement.height; j++)
				{
					Cell cell = Z.GetCell(item3.X - num + i, item3.Y - num2 + j);
					if (cell != null && (ColorExtensionMethods.Equals(randomElement.getPixel(i, j), ColorOutputMap.BLACK) || item3.X - num + i == item3.X || item3.X - num + i == item3.Y))
					{
						cell.Clear(null, Important: true);
						cell.AddObject("WoodFloor");
					}
				}
			}
			Z.GetCell(item3).Clear(null, Important: true);
			Z.GetCell(item3).AddObject("StairsDown").SetIntProperty("IdleStairs", 1);
			list2.Add(item);
		}
		Z.GetCell(0, 0).AddObject("DaylightWidget");
		return true;
	}
}
