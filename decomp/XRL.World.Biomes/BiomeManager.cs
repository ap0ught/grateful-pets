using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleLib.Console;
using XRL.Messages;
using XRL.UI;
using XRL.Wish;

namespace XRL.World.Biomes;

[Serializable]
[HasWishCommand]
public static class BiomeManager
{
	public static Dictionary<string, IBiome> Biomes = new Dictionary<string, IBiome>
	{
		{
			"Slimy",
			new SlimyBiome()
		},
		{
			"Tarry",
			new TarryBiome()
		},
		{
			"Rusty",
			new RustyBiome()
		},
		{
			"Fungal",
			new FungalBiome()
		},
		{
			"Psychic",
			new PsychicBiome()
		}
	};

	private static List<IBiome> TopBiomes = new List<IBiome>(2);

	public static List<IBiome> GetTopBiomes(string ZoneID, int HowMany = 2)
	{
		TopBiomes.Clear();
		foreach (KeyValuePair<string, IBiome> biome in Biomes)
		{
			if (biome.Value.GetBiomeValue(ZoneID) > 0)
			{
				TopBiomes.Add(biome.Value);
			}
		}
		if (TopBiomes.Count > 1)
		{
			TopBiomes.Sort((IBiome a, IBiome b) => b.GetBiomeValue(ZoneID).CompareTo(a.GetBiomeValue(ZoneID)));
			for (int num = TopBiomes.Count; num > HowMany; num--)
			{
				TopBiomes.RemoveAt(num - 1);
			}
		}
		return TopBiomes;
	}

	public static int BiomeValue(string Biome, string ZoneID)
	{
		if (!Biomes.ContainsKey(Biome))
		{
			return 0;
		}
		if (!ZoneID.StartsWith("JoppaWorld"))
		{
			return 0;
		}
		return Biomes[Biome].GetBiomeValue(ZoneID);
	}

	public static string MutateZoneName(string Input, string ZoneID)
	{
		if (!ZoneID.StartsWith("JoppaWorld"))
		{
			return Input;
		}
		List<IBiome> topBiomes = GetTopBiomes(ZoneID);
		if (topBiomes.Count == 0)
		{
			return Input;
		}
		if (!The.ZoneManager.CheckBiomesAllowed(ZoneID))
		{
			return Input;
		}
		for (int num = topBiomes.Count - 1; num >= 0; num--)
		{
			Input = topBiomes[num].MutateZoneName(Input, ZoneID, num);
		}
		return Input;
	}

	public static void MutateZone(Zone Z)
	{
		if (!Z.ZoneID.StartsWith("JoppaWorld") || !The.ZoneManager.CheckBiomesAllowed(Z))
		{
			return;
		}
		List<IBiome> topBiomes = GetTopBiomes(Z.ZoneID);
		if (topBiomes.Count == 0)
		{
			return;
		}
		foreach (GameObject item in Z.YieldObjects())
		{
			if (item.Physics != null)
			{
				for (int i = 0; i < topBiomes.Count; i++)
				{
					topBiomes[i].MutateGameObject(item, Z.ZoneID);
				}
			}
		}
		for (int j = 0; j < topBiomes.Count; j++)
		{
			topBiomes[j].MutateZone(Z);
		}
	}

	[WishCommand("surfacebiomes", null)]
	public static void DisplaySurfaceDistribution(string Value)
	{
		if (!Biomes.TryGetValue(Value, out var value))
		{
			Popup.Show("No biome by name '" + Value + "' found.");
			return;
		}
		Zone zone = The.ZoneManager.GetZone("JoppaWorld");
		int[,] array = new int[zone.Width, zone.Height];
		int num = 1;
		for (int i = 0; i < zone.Height * Definitions.Height; i++)
		{
			for (int j = 0; j < zone.Width * Definitions.Width; j++)
			{
				int num2 = j / Definitions.Width;
				int num3 = i / Definitions.Height;
				if (value.GetBiomeValue(ZoneID.Assemble(zone.ZoneWorld, num2, num3, j - num2 * Definitions.Width, i - num3 * Definitions.Height, 10)) != 0)
				{
					array[num2, num3]++;
					num++;
				}
			}
		}
		int num4 = zone.Width * Definitions.Width * zone.Height * Definitions.Height;
		float num5 = (float)num * 1f / (float)num4 * 100f;
		MessageQueue.AddPlayerMessage($"{Value} biome: {num}/{num4}, {(int)num5}%.", "W");
		Keys keys = Keys.None;
		ScreenBuffer scrapBuffer = ScreenBuffer.GetScrapBuffer1();
		do
		{
			scrapBuffer.Clear();
			for (int k = 0; k < zone.Height; k++)
			{
				for (int l = 0; l < zone.Width; l++)
				{
					if (array[l, k] != 0)
					{
						scrapBuffer.WriteAt(l, k, (array[l, k] > 9) ? "+" : array[l, k].ToString());
					}
				}
			}
			if (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "LeftClick")
			{
				Dictionary<string, int> dictionary = new Dictionary<string, int>();
				int x = Keyboard.CurrentMouseEvent.x;
				int y = Keyboard.CurrentMouseEvent.y;
				for (int m = 0; m < Definitions.Height; m++)
				{
					for (int n = 0; n < Definitions.Width; n++)
					{
						string zoneDisplayName = The.ZoneManager.GetZoneDisplayName(ZoneID.Assemble(zone.ZoneWorld, x, y, n, m, 10));
						dictionary.TryGetValue(zoneDisplayName, out var value2);
						dictionary[zoneDisplayName] = value2 + 1;
					}
				}
				int num6 = 0;
				foreach (KeyValuePair<string, int> item in dictionary.OrderBy((KeyValuePair<string, int> p) => p.Key.Length))
				{
					scrapBuffer.WriteAt(0, num6++, "{{W|" + item.Key + ": " + item.Value + "}}");
				}
			}
			else if (keys == Keys.Escape || keys == Keys.Enter)
			{
				break;
			}
			scrapBuffer.Draw();
		}
		while ((keys = Keyboard.getvk(MapDirectionToArrows: false)) != Keys.Escape);
	}
}
