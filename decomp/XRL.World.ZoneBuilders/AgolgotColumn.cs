using System;
using System.Collections.Generic;
using Genkit;
using XRL.Rules;
using XRL.World.Parts;

namespace XRL.World.ZoneBuilders;

public class AgolgotColumn : GirshLairMakerBase
{
	public const string SECRET_ID = "$agolgotlair";

	public const int Z_LOW = 40;

	public const int Z_HIGH = 45;

	public const int DEPTH = 3;

	public int CradleLevel => ZoneBuilderSandbox.GetOracleIntColumn(Zone, 40, 45);

	public override string SecretID => "$agolgotlair";

	public override string CradleDisplayName => "The cradle of Agolgot";

	public override bool IsCradle
	{
		get
		{
			if (Zone.Z <= CradleLevel)
			{
				return Zone.Z > CradleLevel - 3;
			}
			return false;
		}
	}

	public override bool BuildLair(Zone Z)
	{
		Pitted pitted = new Pitted
		{
			MinWells = 1,
			MaxWells = 3,
			MinRadius = 2,
			MaxRadius = 5,
			XMargin = 8,
			PitTop = Z.Z,
			PitDepth = 2,
			PitDetailsRandom = true,
			Liquid = "SludgePuddle",
			Lazy = (Z.Z < CradleLevel)
		};
		bool isCradle = IsCradle;
		bool result = true;
		List<Location2D> list = new List<Location2D>();
		if (isCradle)
		{
			int num = 2 + Stat.Random(0, 1) + (3 - (CradleLevel - Z.Z));
			Location2D location2D = Location2D.Get(10, 12).Wiggle(2, 2);
			for (int i = 0; i < num; i++)
			{
				if (location2D == null)
				{
					break;
				}
				list.Add(location2D);
				int num2 = Stat.Random(6, 8);
				float num3 = (float)Stat.Random(80, 90) / 75f;
				float num4 = (float)Stat.Random(80, 90) / 75f;
				int num5 = (int)((float)num2 * num3);
				int num6 = (int)((float)num2 * num4);
				for (int j = -num5; j <= num5; j++)
				{
					for (int k = -num6; k <= num6; k++)
					{
						Cell cell = Z.GetCell(location2D.X + j, location2D.Y + k);
						if (cell != null)
						{
							int num7 = (int)Math.Sqrt(num3 * (float)Math.Abs(j) * (num3 * (float)Math.Abs(j)) + (float)Math.Abs(k) * num4 * ((float)Math.Abs(k) * num4));
							if (num7 <= num2 && num7 >= num2 - 2)
							{
								cell.ClearWalls();
								cell.RequireObject("BaseNephilimWall_Agolgot");
							}
							if (num7 < 3)
							{
								cell.ClearWalls();
							}
						}
					}
				}
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
			pitted = new Pitted
			{
				MinWells = 8,
				MaxWells = 16,
				MinRadius = 2,
				MaxRadius = 5,
				XMargin = 8,
				PitTop = CradleLevel - 3,
				PitDepth = 2,
				Liquid = "SludgePuddle",
				Lazy = true
			};
		}
		if (Z.Z > CradleLevel)
		{
			return true;
		}
		List<Location2D> list2 = new List<Location2D>();
		List<Location2D> list3 = new List<Location2D>();
		if (isCradle || Stat.Random(1, 100) <= 70)
		{
			pitted.BuildZone(Z, list2, list3);
		}
		else
		{
			new StairsDown().BuildZone(Z);
		}
		if (Z.Z == CradleLevel - 1 && list3.Count > 0)
		{
			ZoneBuilderSandbox.BridgeOver(Z, list2.GetRandomElement());
		}
		ZoneTemplateManager.Templates["AgolgotColumn"].Execute(Z);
		Z.FireEvent("FirmPitEdges");
		ZoneBuilderSandbox.EnsureAllVoidsConnected(Z, pathWithNoise: true, 0.5f);
		foreach (Location2D item in list)
		{
			List<Location2D> list4 = new List<Location2D>();
			foreach (Cell item2 in Z.GetCell(item).GetLocalAdjacentCellsCircular(3))
			{
				item2.ClearWalls();
				list4.Add(item2.Location);
			}
			ZoneBuilderSandbox.PlacePopulationInRegion(Z, list4, "AgolgotCyst");
		}
		if (Z.Z == CradleLevel && list.Count > 0)
		{
			int index = list.Count / 2;
			Z.GetCell(list[index]).GetCellOrFirstConnectedSpawnLocation().AddObject("Agolgot");
		}
		if (Z.Z != CradleLevel && list2.Count > 0)
		{
			List<GameObject> objects = Z.GetObjects((GameObject o) => o.IsOpenLiquidVolume());
			int num8 = 0;
			for (int num9 = Stat.Random(0, 4); num8 < num9; num8++)
			{
				GameObject randomElement = objects.GetRandomElement();
				if (randomElement == null)
				{
					break;
				}
				using Pathfinder pathfinder = Z.getPathfinder();
				Cell currentCell = randomElement.CurrentCell;
				if (currentCell == null || !pathfinder.FindPath(currentCell.Location, list2.GetRandomElement(), Display: false, CardinalDirectionsOnly: true, 24300))
				{
					continue;
				}
				foreach (PathfinderNode step in pathfinder.Steps)
				{
					Cell cell2 = Z.GetCell(step.X, step.Y);
					GameObject gameObject = GameObject.Create(randomElement.Blueprint);
					cell2.AddObject(gameObject);
				}
			}
		}
		int chance = 20;
		for (int num10 = 0; num10 < Z.Height; num10++)
		{
			for (int num11 = 0; num11 < Z.Width; num11++)
			{
				LiquidVolume liquidVolume = Z.GetCell(num11, num10).GetOpenLiquidVolume()?.LiquidVolume;
				if (liquidVolume != null && liquidVolume.Volume > 500 && chance.in100() && liquidVolume.IsWaste() && liquidVolume.IsOpenVolume())
				{
					Z.GetCell(num11, num10).AddObject("EelSpawn");
				}
			}
		}
		return result;
	}

	public override void MutateObject(GameObject Object)
	{
		if (!Object.HasPart(typeof(PoisonOnHit)))
		{
			Corpse corpse = Object.RequirePart<Corpse>();
			corpse.CorpseBlueprint = "RankPool";
			corpse.CorpseChance = 100;
			Object.AddPart(new PoisonOnHit
			{
				Chance = 50
			});
			Object.AddPart(new DiseaseOnHit
			{
				Chance = 50
			});
			Object.AddPart(new RustOnHit
			{
				Chance = 50
			});
			Object.AddPart(new NephilimCultistIconColor("&g"));
			Object.RequirePart<DisplayNameAdjectives>().RequireAdjective("rank");
		}
	}
}
