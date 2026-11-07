using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using XRL.Rules;
using XRL.UI;
using XRL.Wish;
using XRL.World.Parts;
using XRL.World.ZoneBuilders;

namespace XRL.World.Biomes;

[HasGameBasedStaticCache]
[HasWishCommand]
public class PsychicBiome : IBiome
{
	public const int X1 = 72;

	public const int Y1 = 2;

	public const int X2 = 78;

	public const int Y2 = 17;

	public const int Width = 21;

	public const int Height = 48;

	public const int Depth = 10;

	public const int RootDepth = 40;

	public const float Cutoff = 0.85f;

	public const string Table = "BiomePsychic_Generic";

	[NonSerialized]
	[GameBasedStaticCache(true, false, CreateInstance = false)]
	public static ushort[,][] SeedMap = null;

	public static int CX = 229;

	public static int CY = 16;

	[NonSerialized]
	[GameBasedStaticCache(true, false, CreateInstance = false)]
	private static List<MutationEntry> Pool;

	private static List<MutationEntry> Result = new List<MutationEntry>(2);

	public override int GetBiomeValue(string ZoneID)
	{
		if (!XRL.World.ZoneID.Parse(ZoneID, out var World, out var ParasangX, out var ParasangY, out var ZoneX, out var ZoneY, out var ZoneZ) || ZoneZ < 10 || ParasangX < 72 || ParasangX > 78 || ParasangY < 2 || ParasangY > 17)
		{
			return 0;
		}
		if (SeedMap == null)
		{
			SeedMap = CreateSeedMap();
		}
		ushort[] array = SeedMap[ZoneX + (ParasangX - 72) * Definitions.Width, ZoneY + (ParasangY - 2) * Definitions.Height];
		ushort num = array[(ZoneZ - 10) % array.Length];
		if (num == 0)
		{
			return 0;
		}
		GameObject terrainObjectForZone = ZoneManager.GetTerrainObjectForZone(ParasangX, ParasangY, World);
		if (terrainObjectForZone == null || !terrainObjectForZone.GetBlueprint().DescendsFrom("TerrainMoonStair"))
		{
			return 0;
		}
		return num;
	}

	private ushort[,][] CreateSeedMap()
	{
		Cell cell = The.ZoneManager.GetZone("JoppaWorld")?.FindFirstObject("TerrainEynRoj")?.CurrentCell;
		if (cell != null)
		{
			CX = cell.X * 3 + 1;
			CY = cell.Y * 3 + 1;
		}
		ushort[,][] array = new ushort[21, 48][];
		for (int i = 0; i < 48; i++)
		{
			for (int j = 0; j < 21; j++)
			{
				array[j, i] = new ushort[(j == CX - 216 && i == CY - 6) ? 40 : 10];
			}
		}
		FastNoise fastNoise = new FastNoise(The.Game.GetWorldSeed("PsychicBiome1"));
		fastNoise.SetNoiseType(FastNoise.NoiseType.SimplexFractal);
		fastNoise.SetFrequency(0.3f);
		fastNoise.SetFractalType(FastNoise.FractalType.FBM);
		fastNoise.SetFractalOctaves(2);
		fastNoise.SetFractalLacunarity(8f);
		fastNoise.SetFractalGain(0.5f);
		System.Random random = new System.Random(The.Game.GetWorldSeed("PsychicBiomeValue"));
		for (int k = 0; k < 10; k++)
		{
			for (int l = 0; l < 48; l++)
			{
				for (int m = 0; m < 21; m++)
				{
					if (!(fastNoise.GetValue(m, l, k) < 0.85f) && !TryAssignNeighbour(array, m, l, k))
					{
						array[m, l][k] = (ushort)random.Next(255);
					}
				}
			}
		}
		fastNoise.SetSeed(The.Game.GetWorldSeed("PsychicBiome2"));
		for (int n = 0; n < 10; n++)
		{
			for (int num = 0; num < 48; num++)
			{
				for (int num2 = 0; num2 < 21; num2++)
				{
					if (!(fastNoise.GetValue(num2, num, n) < 0.85f) && !TryAssignNeighbour(array, num2, num, n, 65280))
					{
						array[num2, num][n] |= (ushort)(random.Next(255) << 8);
					}
				}
			}
		}
		if (cell != null)
		{
			int num3 = CX - 216;
			int num4 = CY - 6;
			int num5 = 0;
			ushort num6 = 0;
			array[num3, num4][0] = 0;
			for (int num7 = 1; num7 < 40; num7++)
			{
				if (num5 > 0)
				{
					num5--;
				}
				else if (random.NextDouble() < 0.5)
				{
					ushort num8 = num6;
					num6 = (ushort)random.Next(255);
					num5 = random.Next(1, 3);
					if (num8 != 0 && random.NextDouble() < 0.3)
					{
						array[num3, num4][num7] = (ushort)(num8 | (num6 << 8));
						continue;
					}
				}
				else
				{
					num6 = 0;
				}
				array[num3, num4][num7] = num6;
			}
		}
		return array;
	}

	private bool TryAssignNeighbour(ushort[,][] Map, int X, int Y, int Z, ushort Mask = ushort.MaxValue)
	{
		ushort num = (ushort)((X > 0) ? ((uint)(Map[X - 1, Y][Z] & Mask)) : 0u);
		ushort num2 = (ushort)((Y > 0) ? ((uint)(Map[X, Y - 1][Z] & Mask)) : 0u);
		if (num2 != 0)
		{
			Map[X, Y][Z] |= num2;
			if (num > 0 && num != num2)
			{
				ushort num3 = (ushort)(~Mask);
				int num4 = X - 1;
				while (num4 >= 0 && (Map[num4, Y][Z] & Mask) == num)
				{
					Map[num4, Y][Z] &= num3;
					Map[num4, Y][Z] |= num2;
					num4--;
				}
			}
			return true;
		}
		if (num != 0)
		{
			Map[X, Y][Z] |= num;
			return true;
		}
		return false;
	}

	public override void MutateZone(Zone Z)
	{
		Z.BuildReachabilityFromEdges();
		if (Z.wX * 3 + Z.X != CX || Z.wY * 3 + Z.Y != CY)
		{
			ZoneBuilderSandbox.PlacePopulationInRegion(Z, Z.area, "BiomePsychic_Generic");
		}
		int biomeValue = GetBiomeValue(Z.ZoneID);
		if (biomeValue >= 1)
		{
			Z.SetZoneProperty("ambient_bed_2", "Sounds/Ambiences/amb_biome_psionic");
		}
		foreach (MutationEntry item in GetMutationsFor(biomeValue))
		{
			if (!item.BiomeTable.IsNullOrEmpty())
			{
				ZoneBuilderSandbox.PlacePopulationInRegion(Z, Z.area, item.BiomeTable);
			}
		}
	}

	public override string MutateZoneName(string Name, string ZoneID, int NameOrder)
	{
		int biomeValue = GetBiomeValue(ZoneID);
		if (biomeValue == 0)
		{
			return Name;
		}
		foreach (MutationEntry item in GetMutationsFor(biomeValue))
		{
			Name = IBiome.MutateZoneNameWith(Name, item.BiomeEpithet, item.BiomeAdjective);
		}
		return Name;
	}

	public List<MutationEntry> GetMutationsFor(int Value)
	{
		Result.Clear();
		if (Value == 0)
		{
			return Result;
		}
		if (Pool == null)
		{
			Pool = MutationFactory.GetMutationsOfCategory("Mental").FindAll((MutationEntry x) => !x.BiomeAdjective.IsNullOrEmpty() || !x.BiomeEpithet.IsNullOrEmpty());
		}
		int num = Value & 0xFF;
		if (num != 0)
		{
			Result.Add(Pool.GetRandomElement(new System.Random(num)));
		}
		int num2 = Value >> 8;
		if (num2 != 0 && num2 != num)
		{
			MutationEntry randomElement = Pool.GetRandomElement(new System.Random(num2));
			if (!Result.Contains(randomElement))
			{
				Result.Add(randomElement);
			}
		}
		return Result;
	}

	public override GameObject MutateGameObject(GameObject Object, string ZoneID)
	{
		if (!Object.IsCombatObject())
		{
			return Object;
		}
		if (!Object.IsMutant())
		{
			return Object;
		}
		System.Random random = (Object.Property.TryGetValue("Batch", out var value) ? new System.Random(The.Game.GetWorldSeed(value)) : Stat.Rnd);
		int biomeValue = GetBiomeValue(ZoneID);
		Mutations mutations = Object.RequirePart<Mutations>();
		foreach (MutationEntry item in GetMutationsFor(biomeValue))
		{
			if (!Object.HasPart(item.Class) && mutations.IncludedInMutatePool(item) && !(random.NextDouble() < 0.25))
			{
				int num = Mathf.Clamp(Object.Level / 4, 1, 10);
				if (item.MaxLevel > 0 && num > item.MaxLevel)
				{
					num = item.MaxLevel;
				}
				mutations.AddMutation(item, num);
			}
		}
		return Object;
	}

	public override bool IsNotable(string ZoneID)
	{
		return false;
	}

	[WishCommand("chavvah:biomes", null)]
	public static void WishRootBiomes()
	{
		PsychicBiome psychicBiome = (PsychicBiome)BiomeManager.Biomes["Psychic"];
		StringBuilder stringBuilder = Event.NewStringBuilder();
		for (int i = 1; i < 40; i++)
		{
			string zoneID = ZoneID.Assemble("JoppaWorld", CX / 3, CY / 3, 1, 1, 10 + i);
			int biomeValue = psychicBiome.GetBiomeValue(zoneID);
			stringBuilder.Compound('[', '\n').Append(10 + i).Append("]: ");
			stringBuilder.AppendJoin(", ", from x in psychicBiome.GetMutationsFor(biomeValue)
				select x.Name);
		}
		Popup.Show(stringBuilder.ToString());
	}
}
