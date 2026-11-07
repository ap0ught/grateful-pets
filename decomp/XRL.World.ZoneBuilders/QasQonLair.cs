using System;
using System.Collections.Generic;
using System.Linq;
using Genkit;
using XRL.Rules;
using XRL.World.Effects;
using XRL.World.Parts;
using XRL.World.ZoneBuilders.Utility;

namespace XRL.World.ZoneBuilders;

public class QasQonLair : GirshLairMakerBase
{
	public const string SECRET_ID = "$qasqonlair";

	public bool Nephal;

	public override string SecretID => "$qasqonlair";

	public override string CradleDisplayName => "The chuppah of Qas and Qon";

	public override int DiscoveryXP => 5000;

	public void BuildSurface(Zone Z)
	{
		List<Location2D> pitCellsOut = new List<Location2D>();
		List<Location2D> list = new List<Location2D>();
		Pitted.BuildPits(Z, 2, 4, 2, 5, 8, 2, pitCellsOut, list, 10, 2, "DilutedWarmStaticPuddle", PitDetailsRandom: true, "CysticPit");
		foreach (Location2D item in list)
		{
			CreateCyst(Z, item, "BaseNephilimWall_QasQon");
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
			Pitted.BuildPits(Z, 2 + num, 4 + num * 2, 2, 5, 8, 2, PitTop: Z.Z, pitCellsOut: new List<Location2D>(), centerCellsOut: list3, PitDepth: 2, Liquid: "DilutedWarmStaticPuddle", PitDetailsRandom: true, CenterConnectionType: "CysticPit", PitObject: "Pit", Avoid: list2);
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
			CreateCyst(Z, item, "BaseNephilimWall_QasQon", 6, 8, null, FullClear: true, Paint: true);
		}
		foreach (Location2D item2 in list2)
		{
			CreateCyst(Z, item2, "BaseNephilimWall_QasQon", 8, 10, null, FullClear: true, Paint: true);
		}
		List<Location2D> list4 = new List<Location2D>();
		foreach (Location2D item3 in list2)
		{
			foreach (Cell item4 in Z.GetCell(item3).GetLocalAdjacentCellsCircular(4))
			{
				item4.ClearWalls();
				list4.Add(item4.Location);
			}
			ZoneBuilderSandbox.PlacePopulationInRegion(Z, list4, "QasQonCyst");
			list4.Clear();
		}
		if (Nephal && list2.Count > 0)
		{
			int index = list2.Count / 2;
			GameObject gameObject = Z.GetCell(list2[index]).GetCellOrFirstConnectedSpawnLocation().AddObject("Qas");
			GameObject gameObject2 = Z.GetCell(list2[index]).GetCellOrFirstConnectedSpawnLocation().AddObject("Qon");
			gameObject.ApplyEffect(new Asleep(9999, forced: false, quicksleep: false, Voluntary: true));
			gameObject2.ApplyEffect(new Asleep(9999, forced: false, quicksleep: false, Voluntary: true));
		}
	}

	public override void MutateObject(GameObject Object)
	{
		int num = Stat.Random(1, 3);
		bool flag = num <= 2 && TryAddPolarized(Object);
		bool flag2 = num >= 2 && TryAddCryptic(Object);
		if (flag)
		{
			if (flag2)
			{
				Object.AddPart(new NephilimCultistIconColor("&C"));
				Object.RequirePart<DisplayNameAdjectives>().RequireAdjective("dazzling");
			}
			else
			{
				Object.AddPart(new NephilimCultistIconColor("&c"));
				Object.RequirePart<DisplayNameAdjectives>().RequireAdjective("polarized");
			}
		}
		else if (flag2)
		{
			Object.AddPart(new NephilimCultistIconColor("&b"));
			Object.RequirePart<DisplayNameAdjectives>().RequireAdjective("cryptic");
		}
	}

	public bool TryAddPolarized(GameObject Object)
	{
		if (Object.HasPart(typeof(ForcePylon)))
		{
			return false;
		}
		Object.AddPart(new ForcePylon
		{
			Walls = 2,
			Range = 5
		});
		return true;
	}

	public bool TryAddCryptic(GameObject Object)
	{
		if (Object.HasPart(typeof(ConfuseOnHit)))
		{
			return false;
		}
		Object.AddPart(new ConfuseOnHit
		{
			Chance = 100
		});
		if (!Object.HasPart(typeof(BurstOnDeath)))
		{
			Object.AddPart(new BurstOnDeath
			{
				Blueprint = "ConfusionGas200",
				SkipSolid = true
			});
			if (!Object.HasPart(typeof(SoundOnDestroy)))
			{
				Object.AddPart(new SoundOnDestroy
				{
					Sounds = "Sounds/Abilities/sfx_ability_gasMutation_passiveRelease",
					Volume = 0.7f
				});
			}
		}
		return true;
	}

	public void HoloZone(Zone Z)
	{
		NoiseMap noiseMap = new NoiseMap(Z.Width, Z.Height, 10, 5, 4, 4, 20, 20, 4, 3, 0, 1, null);
		for (int i = 0; i < Z.Height; i++)
		{
			for (int j = 0; j < Z.Width; j++)
			{
				if (noiseMap.Noise[j, i] > 1)
				{
					GameObject firstWall = Z.GetCell(j, i).GetFirstWall();
					if (firstWall != null && !firstWall.HasPart(typeof(HologramMaterial)) && !firstWall.HasPart(typeof(ConcealedHologramMaterial)))
					{
						firstWall.RequirePart<HologramMaterial>();
						firstWall.Physics.Solid = false;
					}
				}
			}
		}
	}

	public void HoloClear(GameObject Object)
	{
		if (Object.IsWall())
		{
			if (!Object.HasPart(typeof(HologramMaterial)) && !Object.HasPart(typeof(ConcealedHologramMaterial)))
			{
				Object.Physics.Solid = false;
				Object.RequirePart<ConcealedHologramMaterial>();
			}
		}
		else
		{
			Object.Obliterate();
		}
	}

	public void HoloCyst(Cell Cell)
	{
		Cell.ClearWalls();
		Cell.RequireObject("BaseNephilimWall_QasQonHolo");
	}

	public override bool BuildLair(Zone Z)
	{
		if (Z.Z == 10)
		{
			BuildSurface(Z);
			return true;
		}
		BuildChuppah(Z);
		ZoneTemplateManager.Templates["QasQonCradle"].Execute(Z);
		HoloZone(Z);
		ZoneBuilderSandbox.EnsureAllVoidsConnected(Z, pathWithNoise: true, 0.5f, HoloClear);
		return true;
	}
}
