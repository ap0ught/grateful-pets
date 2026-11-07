using System;
using System.Collections.Generic;
using Qud.API;
using UnityEngine;
using XRL.Names;
using XRL.Rules;

namespace XRL.World.WorldBuilders;

public class RingGateBuilder : WorldBuilder
{
	public string World;

	public string StaticGateZones;

	public static readonly int TELEPORT_GATE_RUINS_SURFACE_PERMILLAGE_CHANCE = 10;

	public static readonly int TELEPORT_GATE_RUINS_DEEP_PERMILLAGE_CHANCE = 1;

	public static readonly int TELEPORT_GATE_RUINS_DEPTH = 30;

	public static readonly int TELEPORT_GATE_BAROQUE_RUINS_SURFACE_PERMILLAGE_CHANCE = 10;

	public static readonly int TELEPORT_GATE_BAROQUE_RUINS_DEEP_PERMILLAGE_CHANCE = 1;

	public static readonly int TELEPORT_GATE_BAROQUE_RUINS_DEPTH = 30;

	public static readonly int TELEPORT_GATE_SECRET_RUIN_PERMILLAGE_CHANCE = 10;

	public static readonly int TELEPORT_GATE_HISTORIC_SITE_SURFACE_PERMILLAGE_CHANCE = 20;

	public static readonly int TELEPORT_GATE_HISTORIC_SITE_DEEP_PERMILLAGE_CHANCE = 20;

	public static readonly int TELEPORT_GATE_HISTORIC_SITE_CHECK_DEPTH = 30;

	public static readonly int TELEPORT_GATE_RANDOM_PROPORTION = 10;

	public static readonly int TELEPORT_GATE_RANDOM_SURFACE_TARGET_PERCENTAGE_CHANCE = 40;

	public static readonly int TELEPORT_GATE_RANDOM_DEEP_TARGET_DEPTH = 40;

	public static readonly bool TELEPORT_GATE_DEBUG = false;

	public override bool BuildWorld(string World)
	{
		this.World = World;
		try
		{
			MetricsManager.rngCheckpoint("teleportgates");
			MetricsManager.LogInfo("Placing teleport gates");
			BuildTeleportGates();
		}
		catch (Exception x)
		{
			MetricsManager.LogException("RingGateBuilder.BuildWorld", x);
		}
		return true;
	}

	public string ZoneIDFromXYz(string World, int xp, int yp, int zp)
	{
		int parasangX = (int)Math.Floor((float)xp / 3f);
		int parasangY = (int)Math.Floor((float)yp / 3f);
		return ZoneID.Assemble(World, parasangX, parasangY, xp % 3, yp % 3, zp);
	}

	public string ZoneIDFromXY(string World, int xp, int yp)
	{
		int parasangX = (int)Math.Floor((float)xp / 3f);
		int parasangY = (int)Math.Floor((float)yp / 3f);
		return ZoneID.Assemble(World, parasangX, parasangY, xp % 3, yp % 3, 10);
	}

	public void BuildTeleportGates()
	{
		List<string> list = GenerateTeleportGateZones();
		game.SetObjectGameState(World + "TeleportGateZones", list);
		PlaceTeleportGates(list);
	}

	public List<string> GenerateTeleportGateZones()
	{
		Zone zone = The.ZoneManager.GetZone(World);
		List<string> list = new List<string>();
		List<string> list2 = null;
		if (!StaticGateZones.IsNullOrEmpty())
		{
			list2 = StaticGateZones.CachedCommaExpansion();
			list.AddRange(list2);
			if (TELEPORT_GATE_DEBUG)
			{
				foreach (string item in list2)
				{
					Debug.LogError("STATIC GATE FOR " + item + " " + The.ZoneManager.GetZoneDisplayName(item));
				}
			}
		}
		for (int i = 0; i < 25; i++)
		{
			for (int j = 0; j < 80; j++)
			{
				GameObject firstObjectWithPart = zone.GetCell(j, i).GetFirstObjectWithPart("TerrainTravel");
				if (firstObjectWithPart == null)
				{
					continue;
				}
				if (firstObjectWithPart.Blueprint.Contains("TerrainRuins"))
				{
					for (int k = 0; k <= 2; k++)
					{
						for (int l = 0; l <= 2; l++)
						{
							if (TELEPORT_GATE_RUINS_SURFACE_PERMILLAGE_CHANCE.in1000())
							{
								string text = Zone.XYToID(World, j * 3 + l, i * 3 + k, 10);
								if (!list.Contains(text))
								{
									list.Add(text);
									if (TELEPORT_GATE_DEBUG)
									{
										Debug.LogError("GATE FOR " + text + " " + The.ZoneManager.GetZoneDisplayName(text) + " BASED ON RUINS SURFACE");
									}
								}
							}
							for (int m = 1; m <= TELEPORT_GATE_RUINS_DEPTH; m++)
							{
								if (!TELEPORT_GATE_RUINS_DEEP_PERMILLAGE_CHANCE.in1000())
								{
									continue;
								}
								string text2 = Zone.XYToID(World, j * 3 + l, i * 3 + k, 10 + m);
								if (!list.Contains(text2))
								{
									list.Add(text2);
									if (TELEPORT_GATE_DEBUG)
									{
										Debug.LogError("GATE FOR " + text2 + " " + The.ZoneManager.GetZoneDisplayName(text2) + " BASED ON RUINS DEEP");
									}
								}
							}
						}
					}
				}
				else
				{
					if (!firstObjectWithPart.Blueprint.Contains("TerrainBaroqueRuins"))
					{
						continue;
					}
					for (int n = 0; n <= 2; n++)
					{
						for (int num = 0; num <= 2; num++)
						{
							if (TELEPORT_GATE_BAROQUE_RUINS_SURFACE_PERMILLAGE_CHANCE.in1000())
							{
								string text3 = Zone.XYToID(World, j * 3 + num, i * 3 + n, 10);
								if (!list.Contains(text3))
								{
									list.Add(text3);
									if (TELEPORT_GATE_DEBUG)
									{
										Debug.LogError("GATE FOR " + text3 + " " + The.ZoneManager.GetZoneDisplayName(text3) + " BASED ON BAROQUE RUINS SURFACE");
									}
								}
							}
							for (int num2 = 1; num2 <= TELEPORT_GATE_BAROQUE_RUINS_DEPTH; num2++)
							{
								if (!TELEPORT_GATE_BAROQUE_RUINS_DEEP_PERMILLAGE_CHANCE.in1000())
								{
									continue;
								}
								string text4 = Zone.XYToID(World, j * 3 + num, i * 3 + n, 10 + num2);
								if (!list.Contains(text4))
								{
									list.Add(text4);
									if (TELEPORT_GATE_DEBUG)
									{
										Debug.LogError("GATE FOR " + text4 + " " + The.ZoneManager.GetZoneDisplayName(text4) + " BASED ON BAROQUE RUINS DEEP");
									}
								}
							}
						}
					}
				}
			}
		}
		if (World == "JoppaWorld")
		{
			foreach (JournalMapNote mapNote in JournalAPI.GetMapNotes((JournalMapNote note) => note.Has("ruins") || note.Has("historic")))
			{
				if (mapNote.Has("historic"))
				{
					string text5 = Zone.XYToID(World, mapNote.ResolvedX, mapNote.ResolvedY, 10);
					if (TELEPORT_GATE_HISTORIC_SITE_SURFACE_PERMILLAGE_CHANCE.in1000() && !list.Contains(text5))
					{
						list.Add(text5);
						if (TELEPORT_GATE_DEBUG)
						{
							Debug.LogError("GATE FOR " + text5 + " " + The.ZoneManager.GetZoneDisplayName(text5) + " BASED ON HISTORIC RUIN SURFACE");
						}
					}
					string text6 = The.ZoneManager.GetZoneProperty(text5, "HistoricSite") as string;
					if (text6.IsNullOrEmpty())
					{
						continue;
					}
					for (int num3 = 1; num3 <= TELEPORT_GATE_HISTORIC_SITE_CHECK_DEPTH; num3++)
					{
						string text7 = Zone.XYToID(World, mapNote.ResolvedX, mapNote.ResolvedY, 10 + num3);
						try
						{
							The.ZoneManager.GetZoneDisplayName(text7);
						}
						catch (Exception)
						{
						}
						if (!((string)The.ZoneManager.GetZoneProperty(text7, "HistoricSite") == text6))
						{
							break;
						}
						if (TELEPORT_GATE_HISTORIC_SITE_DEEP_PERMILLAGE_CHANCE.in1000() && !list.Contains(text7))
						{
							list.Add(text7);
							if (TELEPORT_GATE_DEBUG)
							{
								Debug.LogError("GATE FOR " + text7 + " " + The.ZoneManager.GetZoneDisplayName(text7) + " BASED ON HISTORIC RUIN DEEP");
							}
						}
					}
				}
				else
				{
					if (!TELEPORT_GATE_SECRET_RUIN_PERMILLAGE_CHANCE.in1000())
					{
						continue;
					}
					string text8 = Zone.XYToID(World, mapNote.ResolvedX, mapNote.ResolvedY, 10);
					if (!list.Contains(text8))
					{
						list.Add(text8);
						if (TELEPORT_GATE_DEBUG)
						{
							Debug.LogError("GATE FOR " + text8 + " " + The.ZoneManager.GetZoneDisplayName(text8) + " BASED ON SECRET RUIN");
						}
					}
				}
			}
		}
		int num4 = list.Count * TELEPORT_GATE_RANDOM_PROPORTION / 100;
		int num5 = 0;
		int num6 = 0;
		for (; num5 < num4; num5++)
		{
			int z = (TELEPORT_GATE_RANDOM_SURFACE_TARGET_PERCENTAGE_CHANCE.in100() ? 10 : Stat.Random(11, TELEPORT_GATE_RANDOM_DEEP_TARGET_DEPTH));
			int num7 = Stat.Random(0, 79);
			int num8 = Stat.Random(0, 24);
			int num9 = Stat.Random(0, 2);
			int num10 = Stat.Random(0, 2);
			string text9 = Zone.XYToID(World, num7 * 3 + num9, num8 * 3 + num10, z);
			if (list.Contains(text9))
			{
				if (++num6 < 100)
				{
					num5--;
				}
				continue;
			}
			list.Add(text9);
			if (TELEPORT_GATE_DEBUG)
			{
				Debug.LogError("GATE FOR " + text9 + " " + The.ZoneManager.GetZoneDisplayName(text9) + " BASED ON RANDOM " + (num5 + 1) + " OF " + num4);
			}
		}
		return list;
	}

	public void PlaceTeleportGates(List<string> zones = null)
	{
		if (zones == null)
		{
			zones = GenerateTeleportGateZones();
		}
		List<string> list = null;
		if (!StaticGateZones.IsNullOrEmpty())
		{
			list = StaticGateZones.CachedCommaExpansion();
		}
		List<string> list2 = new List<string>();
		List<string> list3 = new List<string>();
		List<string> list4 = new List<string>();
		List<string> list5 = new List<string>();
		foreach (string zone in zones)
		{
			switch (Stat.Random(0, 2))
			{
			case 0:
				list3.Add(zone);
				break;
			case 1:
				list4.Add(zone);
				break;
			case 2:
				list5.Add(zone);
				break;
			}
		}
		list3.ShuffleInPlace();
		list4.ShuffleInPlace();
		list5.ShuffleInPlace();
		int num = 0;
		while (list3.Count % 2 != 0 && num < 100)
		{
			string randomElement = list3.GetRandomElement();
			if (++num > 100 || list == null || !list.Contains(randomElement))
			{
				list2.Add(randomElement);
				list3.Remove(randomElement);
			}
		}
		num = 0;
		while (list4.Count % 3 != 0 && num < 100)
		{
			string randomElement2 = list4.GetRandomElement();
			if (++num > 100 || list == null || !list.Contains(randomElement2))
			{
				list2.Add(randomElement2);
				list4.Remove(randomElement2);
			}
		}
		num = 0;
		while (list5.Count % 4 != 0 && num < 100)
		{
			string randomElement3 = list5.GetRandomElement();
			if (++num > 100 || list == null || !list.Contains(randomElement3))
			{
				list2.Add(randomElement3);
				list5.Remove(randomElement3);
			}
		}
		List<List<string>> list6 = new List<List<string>>();
		List<List<string>> list7 = new List<List<string>>();
		List<List<string>> list8 = new List<List<string>>();
		Dictionary<string, List<List<string>>> nameRootMap = new Dictionary<string, List<List<string>>>(64);
		int i = 0;
		for (int count = list3.Count; i + 1 < count; i += 2)
		{
			string text = list3[i];
			string text2 = list3[i + 1];
			The.ZoneManager.SetZoneProperty(text, "TeleportGateDestinationZone", text2);
			The.ZoneManager.SetZoneProperty(text2, "TeleportGateDestinationZone", text);
			The.ZoneManager.SetZoneProperty(text, "TeleportGateRingSize", 2);
			The.ZoneManager.SetZoneProperty(text2, "TeleportGateRingSize", 2);
			List<string> list9 = new List<string> { text, text2 };
			list6.Add(list9);
			ConfigureRingName(list9, nameRootMap);
			if (TELEPORT_GATE_DEBUG)
			{
				Debug.LogError("DEFINED 2-RING FROM " + text + " " + The.ZoneManager.GetZoneDisplayName(text) + " TO " + text2 + " " + The.ZoneManager.GetZoneDisplayName(text2));
			}
		}
		int j = 0;
		for (int count2 = list4.Count; j + 2 < count2; j += 3)
		{
			string text3 = list4[j];
			string text4 = list4[j + 1];
			string text5 = list4[j + 2];
			The.ZoneManager.SetZoneProperty(text3, "TeleportGateDestinationZone", text4);
			The.ZoneManager.SetZoneProperty(text4, "TeleportGateDestinationZone", text5);
			The.ZoneManager.SetZoneProperty(text5, "TeleportGateDestinationZone", text3);
			The.ZoneManager.SetZoneProperty(text3, "TeleportGateRingSize", 3);
			The.ZoneManager.SetZoneProperty(text4, "TeleportGateRingSize", 3);
			The.ZoneManager.SetZoneProperty(text5, "TeleportGateRingSize", 3);
			List<string> list10 = new List<string> { text3, text4, text5 };
			list7.Add(list10);
			ConfigureRingName(list10, nameRootMap);
			if (TELEPORT_GATE_DEBUG)
			{
				Debug.LogError("DEFINED 3-RING FROM " + text3 + " " + The.ZoneManager.GetZoneDisplayName(text3) + " TO " + text4 + " " + The.ZoneManager.GetZoneDisplayName(text4) + " TO " + text5 + " " + The.ZoneManager.GetZoneDisplayName(text5));
			}
		}
		int k = 0;
		for (int count3 = list5.Count; k + 3 < count3; k += 4)
		{
			string text6 = list5[k];
			string text7 = list5[k + 1];
			string text8 = list5[k + 2];
			string text9 = list5[k + 3];
			The.ZoneManager.SetZoneProperty(text6, "TeleportGateDestinationZone", text7);
			The.ZoneManager.SetZoneProperty(text7, "TeleportGateDestinationZone", text8);
			The.ZoneManager.SetZoneProperty(text8, "TeleportGateDestinationZone", text9);
			The.ZoneManager.SetZoneProperty(text9, "TeleportGateDestinationZone", text6);
			The.ZoneManager.SetZoneProperty(text6, "TeleportGateRingSize", 4);
			The.ZoneManager.SetZoneProperty(text7, "TeleportGateRingSize", 4);
			The.ZoneManager.SetZoneProperty(text8, "TeleportGateRingSize", 4);
			The.ZoneManager.SetZoneProperty(text9, "TeleportGateRingSize", 4);
			List<string> list11 = new List<string> { text6, text7, text8, text9 };
			list8.Add(list11);
			ConfigureRingName(list11, nameRootMap);
			if (TELEPORT_GATE_DEBUG)
			{
				Debug.LogError("DEFINED 4-RING FROM " + text6 + " " + The.ZoneManager.GetZoneDisplayName(text6) + " TO " + text7 + " " + The.ZoneManager.GetZoneDisplayName(text7) + " TO " + text8 + " " + The.ZoneManager.GetZoneDisplayName(text8) + " TO " + text9 + " " + The.ZoneManager.GetZoneDisplayName(text9));
			}
		}
		foreach (string item in list2)
		{
			num = 0;
			string randomElement4;
			do
			{
				randomElement4 = zones.GetRandomElement();
			}
			while (++num < 10 && randomElement4 == item);
			The.ZoneManager.SetZoneProperty(item, "TeleportGateDestinationZone", randomElement4);
			if (TELEPORT_GATE_DEBUG)
			{
				Debug.LogError("DEFINED SECANT FROM " + item + " " + The.ZoneManager.GetZoneDisplayName(item) + " TO " + randomElement4 + " " + The.ZoneManager.GetZoneDisplayName(randomElement4));
			}
		}
		The.Game.SetObjectGameState(World + "TeleportGate2Rings", list6);
		The.Game.SetObjectGameState(World + "TeleportGate3Rings", list7);
		The.Game.SetObjectGameState(World + "TeleportGate4Rings", list8);
		The.Game.SetObjectGameState(World + "TeleportGateSecants", list2);
		foreach (string zone2 in zones)
		{
			if (list == null || !list.Contains(zone2))
			{
				The.ZoneManager.AddZonePostBuilderAfterTerrain(zone2, "ZoneTemplate:RingGate");
			}
		}
	}

	private string GetNameRoot(List<string> ring)
	{
		string text = null;
		List<string> list = null;
		foreach (string item in ring)
		{
			string text2 = The.ZoneManager.GetZoneProperty(item, "TeleportGateCandidateNameRoot") as string;
			if (!text2.IsNullOrEmpty())
			{
				list?.Add(text2);
				if (text == null)
				{
					text = text2;
					continue;
				}
				list = new List<string> { text, text2 };
			}
		}
		if (list != null)
		{
			return list.GetRandomElement();
		}
		if (!text.IsNullOrEmpty())
		{
			return text;
		}
		return NameMaker.MakeName(null, null, null, null, "Qudish", null, null, null, null, null, null, "Site");
	}

	private void ConfigureRingName(List<string> ring, Dictionary<string, List<List<string>>> nameRootMap)
	{
		string text = GetNameRoot(ring);
		bool flag = nameRootMap.TryGetValue(text, out var value);
		if (flag)
		{
			switch (value.Count)
			{
			case 1:
			{
				string text2 = "Aleph-" + text;
				switch (value[0].Count)
				{
				case 1:
					The.ZoneManager.SetZoneProperty(value[0][0], "TeleportGateName", text2 + " moon gate");
					The.ZoneManager.SetZoneProperty(value[0][0], "TeleportGateNameRoot", text2);
					break;
				case 2:
					The.ZoneManager.SetZoneProperty(value[0][1], "TeleportGateName", text2 + " sun gate");
					The.ZoneManager.SetZoneProperty(value[0][1], "TeleportGateNameRoot", text2);
					goto case 1;
				case 3:
					The.ZoneManager.SetZoneProperty(value[0][2], "TeleportGateName", text2 + " fool gate");
					The.ZoneManager.SetZoneProperty(value[0][2], "TeleportGateNameRoot", text2);
					goto case 2;
				default:
					The.ZoneManager.SetZoneProperty(value[0][3], "TeleportGateName", text2 + " milk gate");
					The.ZoneManager.SetZoneProperty(value[0][3], "TeleportGateNameRoot", text2);
					goto case 3;
				case 0:
					break;
				}
				text = "Bet-" + text;
				break;
			}
			case 2:
				text = "Gimel-" + text;
				break;
			case 3:
				text = "Daled-" + text;
				break;
			default:
				text = "He-" + text;
				break;
			case 0:
				break;
			}
		}
		switch (ring.Count)
		{
		case 1:
			The.ZoneManager.SetZoneProperty(ring[0], "TeleportGateName", text + " moon gate");
			The.ZoneManager.SetZoneProperty(ring[0], "TeleportGateNameRoot", text);
			break;
		case 2:
			The.ZoneManager.SetZoneProperty(ring[1], "TeleportGateName", text + " sun gate");
			The.ZoneManager.SetZoneProperty(ring[1], "TeleportGateNameRoot", text);
			goto case 1;
		case 3:
			The.ZoneManager.SetZoneProperty(ring[2], "TeleportGateName", text + " fool gate");
			The.ZoneManager.SetZoneProperty(ring[2], "TeleportGateNameRoot", text);
			goto case 2;
		default:
			The.ZoneManager.SetZoneProperty(ring[3], "TeleportGateName", text + " milk gate");
			The.ZoneManager.SetZoneProperty(ring[3], "TeleportGateNameRoot", text);
			goto case 3;
		case 0:
			break;
		}
		if (!flag)
		{
			nameRootMap[text] = new List<List<string>> { ring };
		}
	}
}
