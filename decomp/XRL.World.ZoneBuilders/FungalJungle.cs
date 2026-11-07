using System.Collections.Generic;
using System.Text.RegularExpressions;
using Genkit;
using XRL.Core;
using XRL.Rules;
using XRL.World.Parts;
using XRL.World.ZoneBuilders.Utility;

namespace XRL.World.ZoneBuilders;

public class FungalJungle : ZoneBuilderSandbox
{
	public static Vector2i UpperLeft = new Vector2i(0, 0);

	public static Vector2i Middle = new Vector2i(53, 17);

	public static Vector2i LowerRight = new Vector2i(79, 24);

	public bool Underground;

	public bool Outskirts;

	public List<Vector2i> HutPath = new List<Vector2i>();

	public List<string> HutPathDir = new List<string>();

	public static void FungalUpAZone(Zone Z, int biomeTier = -1, bool bBiome = false)
	{
		if (biomeTier == 0)
		{
			return;
		}
		float num = 1f;
		switch (biomeTier)
		{
		case -1:
			biomeTier = 3;
			break;
		case 0:
			num = 0f;
			break;
		case 1:
			num = 0.15f;
			break;
		case 2:
			num = 0.525f;
			break;
		case 3:
			num = 0.9f;
			break;
		}
		InfluenceMap influenceMap = ZoneBuilderSandbox.GenerateInfluenceMapNRegions(Z, null, InfluenceMapSeedStrategy.RandomPoint, biomeTier, null, bDraw: false, bAddExtraConnectionRegions: false);
		NoiseMap noiseMap = new NoiseMap(80, 25, 10, 3, 3, 6, 80, 80, 4, 3, 0, 1, null);
		foreach (InfluenceMapRegion region in influenceMap.Regions)
		{
			string text = (bBiome ? PopulationManager.RollOneFrom("MinorLiquidWeeps").Blueprint : PopulationManager.RollOneFrom("LiquidWeeps").Blueprint);
			Match match = Regex.Match(text, "^.*?(?=Lichen)");
			List<string> list = ((match == null || string.IsNullOrEmpty(match.Value)) ? new List<string>(new string[2] { "y", "Y" }) : LiquidVolume.GetLiquidColors(match.Groups[0].Value));
			if (Z.GetZoneProperty("relaxedbiomes") != "true")
			{
				GameObject gameObject = GameObject.Create(text);
				if (!bBiome)
				{
					gameObject.RemovePart<SecretObject>();
				}
				Z.GetCell(region.Cells.GetRandomElement()).AddObject(gameObject);
			}
			foreach (Location2D cell in region.Cells)
			{
				int x = cell.X;
				int y = cell.Y;
				if (noiseMap.Noise[x, y] <= 1 || !Z.GetCell(x, y).IsPassable())
				{
					continue;
				}
				if ((20f * num).in100())
				{
					if (!Z.GetCell(x, y).IsOccluding())
					{
						GameObject gameObject2 = GameObject.Create("Spotted Shagspook");
						string randomElement = list.GetRandomElement();
						string randomElement2 = new List<string>(list).GetRandomElement();
						gameObject2.Render.SetForegroundColor(randomElement);
						gameObject2.Render.DetailColor = randomElement2;
						Z.GetCell(x, y).AddObject(gameObject2);
					}
				}
				else if ((20f * num).in100())
				{
					if (!Z.GetCell(x, y).IsOccluding())
					{
						GameObject gameObject3 = GameObject.Create("Spotted Shagspook");
						string randomElement = list.GetRandomElement();
						string randomElement2 = new List<string>(list).GetRandomElement();
						gameObject3.Render.SetForegroundColor(randomElement);
						gameObject3.Render.DetailColor = randomElement2;
						Z.GetCell(x, y).AddObject(gameObject3);
					}
				}
				else if ((30f * num).in100() && !Z.GetCell(x, y).IsOccluding())
				{
					GameObject gameObject4 = GameObject.Create("Dandy Cap");
					string randomElement = list.GetRandomElement();
					string randomElement2 = new List<string>(list).GetRandomElement();
					gameObject4.Render.SetForegroundColor(randomElement);
					gameObject4.Render.DetailColor = randomElement2;
					Z.GetCell(x, y).AddObject(gameObject4);
				}
			}
		}
		if (biomeTier == -1 && Z.Z == 10)
		{
			Z.GetCell(0, 0).AddObject("DaylightWidget");
		}
		Z.GetCell(0, 0).AddObject("Mushroomy");
	}

	public bool BuildZone(Zone Z)
	{
		Maze maze = RecursiveBacktrackerMaze.Generate((LowerRight.x - UpperLeft.x + 1) * 3, (LowerRight.y - UpperLeft.y + 1) * 3, bShow: false, "JungleMaze");
		if (!XRLCore.Core.Game.HasIntGameState("FungalJungleMazeSeed"))
		{
			XRLCore.Core.Game.SetIntGameState("FungalJungleMazeSeed", Stat.Random(0, 2147483646));
		}
		int intGameState = XRLCore.Core.Game.GetIntGameState("FungalJungleMazeSeed");
		Stat.ReseedFrom("FungalJungle" + Z.ZoneID + Z.BuildTries);
		int num = (Z.wX - UpperLeft.x) * 3 + Z.X;
		int num2 = (Z.wY - UpperLeft.y) * 3 + Z.Y;
		MazeCell mazeCell = maze.Cell[num, num2];
		int num3 = Z.wX * 3 + Z.X;
		int num4 = Z.wY * 3 + Z.Y;
		if (mazeCell.N)
		{
			Z.CacheZoneConnection("-", GetSeededRange(num4 + intGameState * Z.Z, 10, 70), 0, "RiverNorthMouth", null);
		}
		if (mazeCell.S)
		{
			Z.CacheZoneConnection("-", GetSeededRange(num4 + 1 + intGameState * Z.Z, 10, 70), 24, "RiverSouthMouth", null);
		}
		if (mazeCell.E)
		{
			Z.CacheZoneConnection("-", 79, GetSeededRange(num3 + 1 + intGameState * Z.Z, 5, 20), "RiverEastMouth", null);
		}
		if (mazeCell.W)
		{
			Z.CacheZoneConnection("-", 0, GetSeededRange(num3 + intGameState * Z.Z, 5, 20), "RiverWestMouth", null);
		}
		RiverBuilder riverBuilder = new RiverBuilder();
		riverBuilder.Puddle = "ProteanDeepPool";
		riverBuilder.BuildZone(Z);
		FungalUpAZone(Z);
		if (!Underground)
		{
			Z.ClearReachableMap();
			if (Z.BuildReachableMap(0, 0) < 400)
			{
				return false;
			}
		}
		return true;
	}
}
