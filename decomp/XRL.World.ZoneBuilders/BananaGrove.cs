using System;
using System.Collections.Generic;
using Genkit;
using XRL.Rules;
using XRL.World.Parts;

namespace XRL.World.ZoneBuilders;

public class BananaGrove : ZoneBuilderSandbox
{
	private static List<PerlinNoise2D> NoiseFunctions;

	private static double[,] BananaGroveNoise;

	private const int MaxWidth = 1200;

	private const int MaxHeight = 375;

	private const int DACCA_CHANCE = 5;

	public bool Underground;

	public static void Save(SerializationWriter Writer)
	{
		if (BananaGroveNoise == null)
		{
			Writer.Write(0);
			return;
		}
		Writer.Write(1);
		for (int i = 0; i < 1200; i++)
		{
			for (int j = 0; j < 375; j++)
			{
				Writer.Write(BananaGroveNoise[i, j]);
			}
		}
	}

	public static void Load(SerializationReader Reader)
	{
		if (Reader.ReadInt32() == 0)
		{
			BananaGroveNoise = null;
			return;
		}
		BananaGroveNoise = new double[1200, 375];
		for (int i = 0; i < 1200; i++)
		{
			for (int j = 0; j < 375; j++)
			{
				BananaGroveNoise[i, j] = Reader.ReadDouble();
			}
		}
	}

	public bool BuildZone(Zone Z)
	{
		if (BananaGroveNoise == null)
		{
			NoiseFunctions = new List<PerlinNoise2D>();
			NoiseFunctions.Add(new PerlinNoise2D(1, 3f, Stat.Rand));
			NoiseFunctions.Add(new PerlinNoise2D(4, 2f, Stat.Rand));
			NoiseFunctions.Add(new PerlinNoise2D(8, 1f, Stat.Rand));
			NoiseFunctions.Add(new PerlinNoise2D(16, 2f, Stat.Rand));
			NoiseFunctions.Add(new PerlinNoise2D(32, 3f, Stat.Rand));
			NoiseFunctions.Add(new PerlinNoise2D(128, 4f, Stat.Rand));
			BananaGroveNoise = PerlinNoise2D.sumNoiseFunctions(1200, 375, 0, 0, NoiseFunctions);
		}
		int num = Z.wX * 240 + Z.X * 80;
		int num2 = Z.wY * 75 + Z.Y * 25;
		num %= 1200;
		num2 %= 375;
		for (int i = 0; i < Z.Height; i++)
		{
			for (int j = 0; j < Z.Width; j++)
			{
				if (!Z.GetCell(j, i).IsPassable())
				{
					continue;
				}
				double num3 = BananaGroveNoise[j + num, i + num2];
				int chance = 0;
				int chance2 = 0;
				int chance3 = 0;
				if (num3 >= 0.8)
				{
					chance = 20;
					chance2 = 80;
					chance3 = 5;
				}
				else if (num3 >= 0.7)
				{
					chance = 3;
					chance2 = 60;
					chance3 = 3;
				}
				else if (num3 >= 0.5)
				{
					chance = 1;
					chance2 = 1;
					chance3 = 30;
				}
				if (chance.in100())
				{
					Z.GetCell(j, i).AddObject("Starapple Tree");
				}
				else if (chance2.in100())
				{
					if (5.in100())
					{
						Z.GetCell(j, i).AddObject("Red Death Dacca");
					}
					else
					{
						Z.GetCell(j, i).AddObject("Banana Tree");
					}
				}
				else if (chance3.in100())
				{
					Z.GetCell(j, i).AddObject("Dicalyptus Tree");
				}
			}
		}
		Z.GetCell(0, 0).AddObject("Grassy");
		if (Z.GetTerrainNameFromDirection(".") == "TerrainTheSpindle" && Z.Z >= 12 && Z.Z <= 15)
		{
			int num4 = 10;
			int num5 = 10;
			Box b = new Box(20 - num4 / 2, 12 - num5 / 2, 20 - num4 / 2 + num4, 12 - num5 / 2 + num5);
			Box b2 = new Box(60 - num4 / 2, 12 - num5 / 2, 60 - num4 / 2 + num4, 12 - num5 / 2 + num5);
			Z.ClearBox(b);
			Z.FillBox(b, "EbonFulcrete");
			Z.ClearBox(b2);
			Z.FillBox(b2, "EbonFulcrete");
			Z.ProcessHollowBox(new Box(0, 0, 79, 24), (Cell c) =>
			{
				if (c.HasWall())
				{
					c.Clear();
					c.AddObject("EbonFulcrete");
				}
			});
			ZoneBuilderSandbox.EnsureAllVoidsConnected(Z);
		}
		List<Location2D> ruinWalkerStarts = new List<Location2D>();
		int CHANCE_RUIN_WALKER = 1;
		int MAX_RUIN_WALKER = 100;
		Action<string, Cell> afterPlacement = (string o, Cell c) =>
		{
			if (Stat.Random(1, MAX_RUIN_WALKER) <= CHANCE_RUIN_WALKER && !c.HasObject("EbonFulcrete"))
			{
				ruinWalkerStarts.Add(c.Location);
			}
		};
		if (Z.Y == 2 && Z.GetTerrainNameFromDirection("S") == "TerrainTheSpindle")
		{
			if (Z.X == 0)
			{
				ZoneBuilderSandbox.PlacePrefab(Z, "preset_tile_chunks/TombExteriorWall_NNW.rpm", PlacePrefabAlign.S, null, null, null, null, (string o, Cell c) =>
				{
					string result = ((Z.Z >= 14) ? ((c.Y < 23) ? null : "EbonFulcrete") : o);
					return result;
				}, (string o, Cell c) =>
				{
					c.Clear();
				}, afterPlacement);
			}
			if (Z.X == 1)
			{
				ZoneBuilderSandbox.PlacePrefab(Z, "preset_tile_chunks/TombExteriorWall_N.rpm", PlacePrefabAlign.S, null, null, null, null, (string o, Cell c) =>
				{
					string result = ((Z.Z >= 14) ? ((c.Y < 23) ? null : "EbonFulcrete") : o);
					return result;
				}, (string o, Cell c) =>
				{
					c.Clear();
				}, afterPlacement);
			}
			if (Z.X == 2)
			{
				ZoneBuilderSandbox.PlacePrefab(Z, "preset_tile_chunks/TombExteriorWall_NNE.rpm", PlacePrefabAlign.S, null, null, null, null, (string o, Cell c) =>
				{
					string result = ((Z.Z >= 14) ? ((c.Y < 23) ? null : "EbonFulcrete") : o);
					return result;
				}, (string o, Cell c) =>
				{
					c.Clear();
				}, afterPlacement);
			}
		}
		if (Z.Y == 0 && Z.GetTerrainNameFromDirection("N") == "TerrainTheSpindle")
		{
			if (Z.X == 0)
			{
				ZoneBuilderSandbox.PlacePrefab(Z, "preset_tile_chunks/TombExteriorWall_SSW.rpm", PlacePrefabAlign.N, null, null, null, null, (string o, Cell c) =>
				{
					string result = ((Z.Z >= 14) ? ((c.Y > 1) ? null : "EbonFulcrete") : o);
					return result;
				}, (string o, Cell c) =>
				{
					c.Clear();
				});
			}
			if (Z.X == 1)
			{
				ZoneBuilderSandbox.PlacePrefab(Z, "preset_tile_chunks/TombExteriorWall_S.rpm", PlacePrefabAlign.N, null, null, null, null, (string o, Cell c) =>
				{
					string result = ((Z.Z >= 14) ? ((c.Y > 1) ? null : "EbonFulcrete") : o);
					return result;
				}, (string o, Cell c) =>
				{
					c.Clear();
				});
			}
			if (Z.X == 2)
			{
				ZoneBuilderSandbox.PlacePrefab(Z, "preset_tile_chunks/TombExteriorWall_SSE.rpm", PlacePrefabAlign.N, null, null, null, null, (string o, Cell c) =>
				{
					string result = ((Z.Z >= 14) ? ((c.Y > 1) ? null : "EbonFulcrete") : o);
					return result;
				}, (string o, Cell c) =>
				{
					c.Clear();
				});
			}
			if (Z.X == 2 && Z.Z == 10)
			{
				ZoneBuilderSandbox.PlacePrefab(Z, "preset_tile_chunks/TombRuinedMuralCistern.rpm", Z.GetCell(31, 7), 0, null, (Cell c) =>
				{
					c.Clear();
				});
				Z.GetCell(0, 0).AddObject("SultanMuralControllerRandom2AlwaysRuined");
			}
			if (Z.Z == 11)
			{
				int num6 = 0;
				int num7 = 0;
				List<Location2D> list = new List<Location2D>();
				List<GameObject> list2 = new List<GameObject>();
				if (Z.X == 1)
				{
					num6 = 0;
					num7 = 17;
					list.AddRange(new Box(num6, 2, num7, 2).contents());
				}
				else if (Z.X == 0)
				{
					num6 = 44;
					num7 = 79;
					list.AddRange(new Box(num6, 2, num7, 2).contents());
					if (Z.Z == 11)
					{
						list.AddRange(new Box(num6, 0, num6, 2).contents());
						list.AddRange(new Box(num6 - 1, 0, num6 - 1, 1).contents());
					}
					if (Z.Z == 11)
					{
						Z.GetCell(num6 - 1, 1).AddObject("MediumBoulder");
					}
				}
				Z.Clear(list);
				foreach (Location2D item in list)
				{
					Cell cell = Z.GetCell(item);
					foreach (Cell adjacentCell in cell.GetAdjacentCells())
					{
						foreach (GameObject wall in adjacentCell.GetWalls())
						{
							if (!wall.HasPropertyOrTag("HasGraffiti") && !list2.Contains(wall))
							{
								list2.Add(wall);
							}
						}
					}
					foreach (PopulationResult item2 in PopulationManager.Generate("RobbersCutContents"))
					{
						for (int num8 = 0; num8 < item2.Number; num8++)
						{
							cell.AddObject(item2.Blueprint);
						}
					}
				}
				int num9 = 30;
				foreach (GameObject item3 in list2)
				{
					if (Stat.Random(1, 100) <= num9 && !item3.HasPropertyOrTag("HasGraffiti") && !item3.HasPart<Graffitied>())
					{
						Graffitied graffitied = new Graffitied();
						item3.AddPart(graffitied);
						graffitied.Graffiti(item3);
					}
				}
			}
		}
		if (Z.X == 0 && Z.GetTerrainNameFromDirection("W") == "TerrainTheSpindle")
		{
			if (Z.Y == 0)
			{
				ZoneBuilderSandbox.PlacePrefab(Z, "preset_tile_chunks/TombExteriorWall_ENE.rpm", PlacePrefabAlign.W, null, null, null, null, (string o, Cell c) =>
				{
					string result = ((Z.Z >= 14) ? ((c.X > 1) ? null : "EbonFulcrete") : o);
					return result;
				}, (string o, Cell c) =>
				{
					c.Clear();
				}, afterPlacement);
			}
			if (Z.Y == 1)
			{
				ZoneBuilderSandbox.PlacePrefab(Z, "preset_tile_chunks/TombExteriorWall_E.rpm", PlacePrefabAlign.W, null, null, null, null, (string o, Cell c) =>
				{
					string result = ((Z.Z >= 14) ? ((c.X > 1) ? null : "EbonFulcrete") : o);
					return result;
				}, (string o, Cell c) =>
				{
					c.Clear();
				}, afterPlacement);
			}
			if (Z.Y == 2)
			{
				ZoneBuilderSandbox.PlacePrefab(Z, "preset_tile_chunks/TombExteriorWall_ESE.rpm", PlacePrefabAlign.W, null, null, null, null, (string o, Cell c) =>
				{
					string result = ((Z.Z >= 14) ? ((c.X > 1) ? null : "EbonFulcrete") : o);
					return result;
				}, (string o, Cell c) =>
				{
					c.Clear();
				}, afterPlacement);
			}
		}
		if (Z.X == 2 && Z.GetTerrainNameFromDirection("E") == "TerrainTheSpindle")
		{
			if (Z.Y == 0)
			{
				ZoneBuilderSandbox.PlacePrefab(Z, "preset_tile_chunks/TombExteriorWall_WNW.rpm", PlacePrefabAlign.E, null, null, null, null, (string o, Cell c) =>
				{
					string result = ((Z.Z >= 14) ? ((c.X < 78) ? null : "EbonFulcrete") : o);
					return result;
				}, (string o, Cell c) =>
				{
					c.Clear();
				}, afterPlacement);
			}
			if (Z.Y == 1)
			{
				ZoneBuilderSandbox.PlacePrefab(Z, "preset_tile_chunks/TombExteriorWall_W.rpm", PlacePrefabAlign.E, null, null, null, null, (string o, Cell c) =>
				{
					string result = ((Z.Z >= 14) ? ((c.X < 78) ? null : "EbonFulcrete") : o);
					return result;
				}, (string o, Cell c) =>
				{
					c.Clear();
				}, afterPlacement);
			}
			if (Z.Y == 2)
			{
				ZoneBuilderSandbox.PlacePrefab(Z, "preset_tile_chunks/TombExteriorWall_WSW.rpm", PlacePrefabAlign.E, null, null, null, null, (string o, Cell c) =>
				{
					string result = ((Z.Z >= 14) ? ((c.X < 78) ? null : "EbonFulcrete") : o);
					return result;
				}, (string o, Cell c) =>
				{
					c.Clear();
				}, afterPlacement);
			}
		}
		if (Z.X == 0 && Z.Y == 0 && Z.GetTerrainNameFromDirection("NW") == "TerrainTheSpindle")
		{
			ZoneBuilderSandbox.PlacePrefab(Z, "preset_tile_chunks/TombExteriorWall_SE.rpm", PlacePrefabAlign.NW, null, null, null, null, (string o, Cell c) =>
			{
				string result = ((Z.Z >= 14) ? ((c.X > 1 && c.Y > 1 && c.X < 78 && c.Y < 23) ? null : "EbonFulcrete") : o);
				return result;
			}, (string o, Cell c) =>
			{
				c.Clear();
			}, afterPlacement);
		}
		if (Z.X == 2 && Z.Y == 0 && Z.GetTerrainNameFromDirection("NE") == "TerrainTheSpindle")
		{
			ZoneBuilderSandbox.PlacePrefab(Z, "preset_tile_chunks/TombExteriorWall_SW.rpm", PlacePrefabAlign.NE, null, null, null, null, (string o, Cell c) =>
			{
				string result = ((Z.Z >= 14) ? ((c.X > 1 && c.Y > 1 && c.X < 78 && c.Y < 23) ? null : "EbonFulcrete") : o);
				return result;
			}, (string o, Cell c) =>
			{
				c.Clear();
			}, afterPlacement);
		}
		if (Z.X == 0 && Z.Y == 2 && Z.GetTerrainNameFromDirection("SW") == "TerrainTheSpindle")
		{
			ZoneBuilderSandbox.PlacePrefab(Z, "preset_tile_chunks/TombExteriorWall_NE.rpm", PlacePrefabAlign.SW, null, null, null, null, (string o, Cell c) =>
			{
				string result = ((Z.Z >= 14) ? ((c.X > 1 && c.Y > 1 && c.X < 78 && c.Y < 23) ? null : "EbonFulcrete") : o);
				return result;
			}, (string o, Cell c) =>
			{
				c.Clear();
			}, afterPlacement);
		}
		if (Z.X == 2 && Z.Y == 2 && Z.GetTerrainNameFromDirection("SE") == "TerrainTheSpindle")
		{
			ZoneBuilderSandbox.PlacePrefab(Z, "preset_tile_chunks/TombExteriorWall_NW.rpm", PlacePrefabAlign.SE, null, null, null, null, (string o, Cell c) =>
			{
				string result = ((Z.Z >= 14) ? ((c.X > 1 && c.Y > 1 && c.X < 78 && c.Y < 23) ? null : "EbonFulcrete") : o);
				return result;
			}, (string o, Cell c) =>
			{
				c.Clear();
			}, afterPlacement);
		}
		foreach (Location2D item4 in ruinWalkerStarts)
		{
			Location2D location2D = item4;
			int num10 = 100;
			while ((location2D.X < 6 || location2D.X > 73 || location2D.Y < 6 || location2D.Y > 19) && location2D.X >= 0 && location2D.X <= 79 && location2D.Y >= 0 && location2D.Y <= 24 && location2D != null)
			{
				Cell cell2;
				while (true)
				{
					num10--;
					if (num10 < 1)
					{
						break;
					}
					int num11 = 60;
					if (Stat.Random(1, 100) <= num11)
					{
						Location2D location2D2 = location2D.FromDirection(location2D.CardinalDirectionToCenter());
						if (location2D2 == null || Z.GetCell(location2D2) == null || Z.GetCell(location2D2).HasObject("EbonFulcrete"))
						{
							continue;
						}
						location2D = location2D2;
					}
					else
					{
						Location2D location2D3 = location2D.FromDirection(Directions.GetRandomCardinalDirection());
						if (location2D3 == null || Z.GetCell(location2D3) == null || Z.GetCell(location2D3).HasObject("EbonFulcrete"))
						{
							continue;
						}
						location2D = location2D3;
					}
					cell2 = Z.GetCell(location2D);
					if (cell2 == null)
					{
						break;
					}
					goto IL_0e49;
				}
				break;
				IL_0e49:
				if (cell2.HasObject("EbonFulcrete"))
				{
					break;
				}
				if (!(location2D != null))
				{
					continue;
				}
				foreach (Cell cardinalAdjacentCell in Z.GetCell(location2D).GetCardinalAdjacentCells(bLocalOnly: true, BuiltOnly: true, IncludeThis: true))
				{
					int num12 = 50;
					if (cardinalAdjacentCell.HasWall() && !cardinalAdjacentCell.HasObject("EbonFulcrete") && Stat.Random(1, 100) <= num12)
					{
						cardinalAdjacentCell.ClearWalls();
						int num13 = 35;
						if (Stat.Random(1, 100) <= num13)
						{
							cardinalAdjacentCell.AddObject(PopulationManager.RollOneFrom("TombWallRuinRemains").Blueprint);
						}
					}
				}
			}
		}
		if (!Underground)
		{
			Z.GetCell(0, 0).AddObject(GameObjectFactory.Factory.CreateObject("DaylightWidget"));
			Z.ClearReachableMap();
			Z.BuildReachableMap(Z.Width / 2, Z.Height / 2);
		}
		return true;
	}
}
