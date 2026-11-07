using System;
using System.Collections.Generic;
using Genkit;
using XRL.Rules;
using XRL.World.Parts;

namespace XRL.World.ZoneBuilders;

public class BethsaidaColumn : GirshLairMakerBase
{
	public const string SECRET_ID = "$bethsaidalair";

	public const int Z_LOW = 40;

	public const int Z_HIGH = 45;

	public const int DEPTH = 3;

	public int CradleLevel => ZoneBuilderSandbox.GetOracleIntColumn(Zone, 40, 45);

	public override string SecretID => "$bethsaidalair";

	public override string CradleDisplayName => "The cradle of Bethsaida";

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
			Liquid = "SludgePuddle"
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
								cell.RequireObject("BaseNephilimWall_Bethsaida");
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
				Liquid = "ConvalessenceDeepPool"
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
		ZoneTemplateManager.Templates["BethsaidaColumn"].Execute(Z);
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
			ZoneBuilderSandbox.PlacePopulationInRegion(Z, list4, "BethsaidaCyst");
		}
		if (Z.Z == CradleLevel && list.Count > 0)
		{
			int index = list.Count / 2;
			Z.GetCell(list[index]).GetCellOrFirstConnectedSpawnLocation().AddObject("Bethsaida");
		}
		if (Z.Z != CradleLevel && list2.Count > 0)
		{
			List<GameObject> objects = Z.GetObjects((GameObject o) => o.HasPart<LiquidVolume>() && o.GetPart<LiquidVolume>().IsOpenVolume());
			int num8 = 0;
			for (int num9 = Stat.Random(0, 4); num8 < num9; num8++)
			{
				using Pathfinder pathfinder = Z.getPathfinder();
				GameObject randomElement = objects.GetRandomElement();
				string blueprint = randomElement?.Blueprint;
				Cell currentCell = randomElement.Physics.CurrentCell;
				if (randomElement == null || currentCell == null || !pathfinder.FindPath(currentCell.Location, list2.GetRandomElement(), Display: false, CardinalDirectionsOnly: true, 24300))
				{
					continue;
				}
				foreach (PathfinderNode step in pathfinder.Steps)
				{
					Cell cell2 = Z.GetCell(step.X, step.Y);
					GameObject gameObject = GameObject.Create(blueprint);
					cell2.AddObject(gameObject);
				}
			}
		}
		return result;
	}

	public override void MutateObject(GameObject Object)
	{
		if (!Object.HasPart(typeof(SplitOnDeath)))
		{
			Object.AddPart(new SplitOnDeath
			{
				Blueprint = Object.Blueprint,
				Amount = "2",
				Message = "=subject.T= =verb:split= in two!"
			});
			Object.AddPart(new NephilimCultistIconColor("&W"));
			Object.RequirePart<DisplayNameAdjectives>().RequireAdjective("conjoined");
		}
	}
}
