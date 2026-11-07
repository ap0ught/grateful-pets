using System;
using System.Collections.Generic;
using Genkit;
using XRL.Rules;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;

namespace XRL.World.ZoneBuilders;

public class ShugLair : GirshLairMakerBase
{
	public const string SECRET_ID = "$shugruithlair";

	public int l;

	public override string SecretID => "$shugruithlair";

	public override string CradleDisplayName => "The cradle of Shug'ruith";

	public override bool BuildLair(Zone Z)
	{
		Pitted pitted = new Pitted
		{
			MinWells = 1,
			MaxWells = 1,
			MinRadius = 2,
			MaxRadius = 5,
			XMargin = 8,
			PitTop = Z.Z,
			PitDepth = 2,
			PitDetailsRandom = true,
			Liquid = "SludgePuddle"
		};
		bool result = true;
		List<Location2D> list = new List<Location2D>();
		int num = l + 1;
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
							cell.RequireObject("BaseNephilimWall_Shug'rith");
						}
						if (num7 < 3)
						{
							cell.ClearWalls();
							cell.RequireObject("BaseNephilimWall_Shug'rith");
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
			MinWells = 1,
			MaxWells = 1,
			MinRadius = 2,
			MaxRadius = 5,
			XMargin = 8,
			PitTop = Z.Z,
			PitDepth = 1,
			Liquid = "SludgePuddle"
		};
		if (l < 3)
		{
			List<Location2D> pitCellsOut = new List<Location2D>();
			List<Location2D> centerCellsOut = new List<Location2D>();
			pitted.BuildZone(Z, pitCellsOut, centerCellsOut);
		}
		ZoneTemplateManager.Templates["ShugruithTunnel"].Execute(Z);
		Z.FireEvent("FirmPitEdges");
		ZoneBuilderSandbox.EnsureAllVoidsConnected(Z, pathWithNoise: true, 0.5f);
		foreach (Location2D item in list)
		{
			List<Location2D> list2 = new List<Location2D>();
			foreach (Cell item2 in Z.GetCell(item).GetLocalAdjacentCellsCircular(3, includeSelf: true))
			{
				item2.ClearWalls();
				list2.Add(item2.Location);
			}
			ZoneBuilderSandbox.PlacePopulationInRegion(Z, list2, "ShugruithCyst");
		}
		if (l == 3 && list.Count > 0)
		{
			int index = list.Count / 2;
			Z.GetCell(list[index]).AddObject("Shugruith");
		}
		return result;
	}

	public override void MutateObject(GameObject Object)
	{
		if (!Object.HasPart(typeof(Burrowing)))
		{
			Object.RequirePart<Mutations>().AddMutation("Burrowing", 5);
			Object.AddPart(new LeaveTrailWhileHasEffect
			{
				Specification = "BaseNephilimWall_Shug'rith"
			});
			Object.AddPart(new NephilimCultistIconColor("&w"));
			Object.RequirePart<DisplayNameAdjectives>().RequireAdjective("burrowing");
		}
	}
}
