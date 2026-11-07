using System;
using System.Collections.Generic;
using System.Linq;
using Genkit;
using XRL.Rules;
using XRL.World.Effects;
using XRL.World.Parts;

namespace XRL.World.ZoneBuilders;

public class RermadonLair : GirshLairMakerBase
{
	public const string SECRET_ID = "$rermadonlair";

	public bool Nephal;

	public override string SecretID => "$rermadonlair";

	public override string CradleDisplayName => "The cradle of Rermadon";

	public void BuildSurface(Zone Z)
	{
		List<Location2D> pitCellsOut = new List<Location2D>();
		List<Location2D> list = new List<Location2D>();
		Pitted.BuildPits(Z, 2, 4, 2, 5, 8, 2, pitCellsOut, list, 10, 2, "AlgalWaterDeepPool", PitDetailsRandom: true, "CysticPit");
		foreach (Location2D item in list)
		{
			CreateCyst(Z, item, "BaseNephilimWall_Rermadon");
		}
	}

	public void PopulateCysticPitConnections(Zone Z, List<Location2D> Pits)
	{
		foreach (ZoneConnection item in Z.EnumerateConnections())
		{
			if (item.Type == "CysticPit")
			{
				Pits.Add(item.Loc2D);
			}
		}
		if (!Pits.IsNullOrEmpty() || Z.Z <= 10)
		{
			return;
		}
		Zone zoneFromDirection = Z.GetZoneFromDirection("U");
		if (zoneFromDirection.Built || zoneFromDirection.ZoneConnectionCache.IsNullOrEmpty())
		{
			return;
		}
		foreach (CachedZoneConnection item2 in zoneFromDirection.ZoneConnectionCache)
		{
			if (item2.TargetDirection == "d" && item2.Type == "CysticPit")
			{
				Pits.Add(item2.Loc2D);
			}
		}
	}

	public void BuildChuppah(Zone Z)
	{
		int num = Math.Max(Z.Z - 10, 0);
		List<Location2D> list = new List<Location2D>();
		List<Location2D> list2 = new List<Location2D>();
		PopulateCysticPitConnections(Z, list2);
		if (!Nephal)
		{
			List<Location2D> list3 = new List<Location2D>();
			Pitted.BuildPits(Z, 2 + num, 4 + num * 2, 2, 5, 8, 2, PitTop: Z.Z, pitCellsOut: new List<Location2D>(), centerCellsOut: list3, PitDepth: 2, Liquid: "AlgalWaterDeepPool", PitDetailsRandom: true, CenterConnectionType: "CysticPit", PitObject: "Pit", Avoid: list2);
			list.AddRange(list3);
		}
		int num2 = 2 + Stat.Random(0, 1) + num;
		Location2D location2D = (list.LastOrDefault() ?? Location2D.Get(10, 12)).Wiggle(2, 2);
		for (int i = list2.Count; i < num2; i++)
		{
			if (location2D == null)
			{
				break;
			}
			list2.Add(location2D);
			location2D += Location2D.Get(6, 0);
			if (location2D == null)
			{
				break;
			}
			location2D = location2D.Wiggle(3, 3);
			if (location2D == null)
			{
				break;
			}
		}
		foreach (Location2D item in list)
		{
			CreateCyst(Z, item, "BaseNephilimWall_Rermadon", 6, 8, null, FullClear: true, Paint: true);
		}
		foreach (Location2D item2 in list2)
		{
			CreateCyst(Z, item2, "BaseNephilimWall_Rermadon", 8, 10, null, FullClear: true, Paint: true);
		}
		List<Location2D> list4 = new List<Location2D>();
		foreach (Location2D item3 in list2)
		{
			foreach (Cell item4 in Z.GetCell(item3).GetLocalAdjacentCellsCircular(4))
			{
				item4.ClearWalls();
				list4.Add(item4.Location);
			}
			ZoneBuilderSandbox.PlacePopulationInRegion(Z, list4, "RermadonCyst");
			list4.Clear();
		}
		if (Nephal && list2.Count > 0)
		{
			int index = list2.Count / 2;
			Z.GetCell(list2[index]).GetCellOrFirstConnectedSpawnLocation().AddObject("Rermadon")
				.ApplyEffect(new Asleep(9999, forced: false, quicksleep: false, Voluntary: true));
		}
	}

	public override void MutateObject(GameObject Object)
	{
		if (Object.TryGetPart<Corpse>(out var Part))
		{
			if (Part.CorpseBlueprint == "Plasma1000")
			{
				return;
			}
		}
		else
		{
			Object.AddPart(Part = new Corpse());
		}
		Part.CorpseBlueprint = "Plasma1000";
		Part.CorpseChance = 100;
		Object.AddPart(new NephilimCultistIconColor("&G"));
		Object.RequirePart<DisplayNameAdjectives>().RequireAdjective("plasmatic");
	}

	public override bool BuildLair(Zone Z)
	{
		if (Z.Z == 10)
		{
			BuildSurface(Z);
			return true;
		}
		BuildChuppah(Z);
		ZoneTemplateManager.Templates["RermadonCradle"].Execute(Z);
		ZoneBuilderSandbox.EnsureAllVoidsConnected(Z, pathWithNoise: true, 0.5f);
		return true;
	}
}
