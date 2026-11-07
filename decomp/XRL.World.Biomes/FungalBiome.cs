using System;
using Genkit.SimplexNoise;
using XRL.Core;
using XRL.Rules;
using XRL.World.Parts.Mutation;
using XRL.World.ZoneBuilders;

namespace XRL.World.Biomes;

[HasGameBasedStaticCache]
public class FungalBiome : IBiome
{
	[NonSerialized]
	[GameBasedStaticCache(true, false, CreateInstance = false)]
	public static byte[,,] BiomeLevels;

	public override int GetBiomeValue(string ZoneID)
	{
		if (!ZoneID.Contains("."))
		{
			return 0;
		}
		string[] array = ZoneID.Split('.');
		int num = Convert.ToInt32(array[1]);
		int num2 = Convert.ToInt32(array[2]);
		int num3 = Convert.ToInt32(array[3]);
		int num4 = Convert.ToInt32(array[4]);
		int num5 = Convert.ToInt32(array[5]);
		if (num5 < 10)
		{
			return 0;
		}
		string objectTypeForZone = ZoneManager.GetObjectTypeForZone(num, num2, "JoppaWorld");
		if (GameObjectFactory.Factory.Blueprints.ContainsKey(objectTypeForZone) && GameObjectFactory.Factory.Blueprints[objectTypeForZone].Tags.ContainsKey("NoBiomes"))
		{
			return 0;
		}
		if (((string)XRLCore.Core.Game.ZoneManager.GetZoneProperty(ZoneID, "NoBiomes", bClampToLevel30: false, "")).EqualsNoCase("Yes"))
		{
			return 0;
		}
		int num6 = 240;
		int num7 = 75;
		int num8 = 10;
		int num9 = num * 3 + num3;
		int num10 = num2 * 3 + num4;
		int num11 = num5 % num8;
		if (BiomeLevels == null)
		{
			BiomeLevels = new byte[num6, num7, num8];
			LayeredNoise layeredNoise = LayeredNoise.CreateLinearOctiveLayers(3, 1.33f, 0.12f, XRLCore.Core.Game.GetWorldSeed("FungalNoise").ToString());
			float num12 = 0.72f;
			float num13 = 0f;
			float num14 = 1f;
			int num15 = 4;
			float[,,] array2 = layeredNoise.Generate3D(num6, num7, num8);
			for (int i = 0; i < num8; i++)
			{
				for (int j = 0; j < num7; j++)
				{
					for (int k = 0; k < num6; k++)
					{
						float num16 = array2[k, j, i];
						num16 = (array2[k, j, i] = ((!(num16 < num12)) ? ((num16 - num12) * (1f / (1f - num12))) : 0f));
						if (num16 > num13)
						{
							num13 = num16;
						}
						if (num16 < num14)
						{
							num14 = num16;
						}
					}
				}
			}
			for (int l = 0; l < num8; l++)
			{
				for (int m = 0; m < num7; m++)
				{
					for (int n = 0; n < num6; n++)
					{
						int num17 = (int)((array2[n, m, l] - num14) / ((num13 - num14) / (float)num15));
						if (num17 < 0)
						{
							num17 = 0;
						}
						if (num17 > num15 - 1)
						{
							num17 = num15 - 1;
						}
						BiomeLevels[n, m, l] = (byte)num17;
					}
				}
			}
		}
		return BiomeLevels[num9, num10, num11];
	}

	public override string MutateZoneName(string Name, string ZoneID, int NameOrder)
	{
		return IBiome.MutateZoneNameWith(Name, GetBiomeValue(ZoneID), "fungus patch", "fungus grove", "fungus forest");
	}

	public override void MutateZone(Zone Z)
	{
		int biomeValue = GetBiomeValue(Z.ZoneID);
		FungalJungle.FungalUpAZone(Z, biomeValue, bBiome: true);
		if (biomeValue >= 1)
		{
			Z.SetZoneProperty("ambient_bed_2", "Sounds/Ambiences/amb_biome_mushroom");
		}
		if (biomeValue == 1)
		{
			new PopTableZoneBuilder().BuildZone(Z, "FungalBiome1", "minimum", bApplyZoneFactionToObjects: true);
		}
		if (biomeValue == 2)
		{
			new PopTableZoneBuilder().BuildZone(Z, "FungalBiome2", "minimum", bApplyZoneFactionToObjects: true);
		}
		if (biomeValue == 3)
		{
			new PopTableZoneBuilder().BuildZone(Z, "FungalBiome3", "minimum", bApplyZoneFactionToObjects: true);
		}
	}

	public override GameObject MutateGameObject(GameObject GO, string ZoneID)
	{
		long num = 0L;
		num = ((!GO.HasProperty("Batch")) ? Stat.Random(0, 2147483646) : GO.GetLongProperty("Batch", 0L));
		int biomeValue = GetBiomeValue(ZoneID);
		if (biomeValue == 1 && Stat.SeededRandom(num.ToString(), 1, 100) <= 25)
		{
			FungusFriendTemplate.Apply(GO, SporePuffer.InfectionObjectList.GetRandomElement(), XRLCore.Core.Game.ZoneManager.GetZone(ZoneID));
		}
		if (biomeValue == 2)
		{
			if (Stat.SeededRandom(num.ToString(), 1, 100) <= 25)
			{
				FungusFriendTemplate.Apply(GO, SporePuffer.InfectionObjectList.GetRandomElement(), XRLCore.Core.Game.ZoneManager.GetZone(ZoneID));
			}
			else if (Stat.SeededRandom(num.ToString(), 1, 100) <= 50)
			{
				FungusColonizedTemplate.Apply(GO, SporePuffer.InfectionObjectList.GetRandomElement(), XRLCore.Core.Game.ZoneManager.GetZone(ZoneID));
			}
		}
		if (biomeValue == 3 && Stat.SeededRandom(num.ToString(), 1, 100) <= 70)
		{
			FungusColonizedTemplate.Apply(GO, SporePuffer.InfectionObjectList.GetRandomElement(), XRLCore.Core.Game.ZoneManager.GetZone(ZoneID));
		}
		return GO;
	}
}
