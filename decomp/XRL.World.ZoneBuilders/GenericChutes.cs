using System;
using System.Collections.Generic;
using XRL.Core;
using XRL.Rules;
using XRL.World.Parts;

namespace XRL.World.ZoneBuilders;

public class GenericChutes
{
	public static Dictionary<string, string[]> Tiles;

	public int nChutes = 4;

	public bool chuteLevel = true;

	public static void GenTiles()
	{
		if (Tiles == null)
		{
			Tiles = new Dictionary<string, string[]>();
			Tiles.Add("", new string[4] { "....", "....", "....", "...." });
			Tiles.Add("NSTART", new string[3] { "NNN.", "NNN.", "CCC." });
			Tiles.Add("NEND", new string[3] { "SSS.", ">>>.", "...." });
			Tiles.Add("SSTART", new string[3] { "CCC.", "SSS.", "SSS." });
			Tiles.Add("SEND", new string[3] { "....", ">>>.", "NNN." });
			Tiles.Add("ESTART", new string[3] { "CEEE", "CEEE", "CEEE" });
			Tiles.Add("EEND", new string[3] { ".>WW", ".>WW", ".>WW" });
			Tiles.Add("WSTART", new string[3] { "WWWC", "WWWC", "WWWC" });
			Tiles.Add("WEND", new string[3] { "EE>.", "EE>.", "EE>." });
			Tiles.Add("NS", new string[3] { "SSS.", "SSS.", "SSS." });
			Tiles.Add("SN", new string[3] { "NNN.", "NNN.", "NNN." });
			Tiles.Add("EW", new string[3] { "WWWW", "WWWW", "WWWW" });
			Tiles.Add("WE", new string[3] { "EEEE", "EEEE", "EEEE" });
			Tiles.Add("NW", new string[3] { "WSS.", "WWS.", "WWW." });
			Tiles.Add("WN", new string[3] { "NNN.", "ENN.", "EEN." });
			Tiles.Add("SW", new string[3] { "WWW.", "WWN.", "WNN." });
			Tiles.Add("WS", new string[3] { "EES.", "ESS.", "SSS." });
			Tiles.Add("NE", new string[3] { "SSEE", "SEEE", "EEEE" });
			Tiles.Add("EN", new string[3] { "NNNW", "NNWW", "NWWW" });
			Tiles.Add("SE", new string[3] { "EEEE", "NEEE", "NNEE" });
			Tiles.Add("ES", new string[3] { "SWWW", "SSWW", "SSSW" });
		}
	}

	public static GenericChuteTemplate GenerateGolgothaTemplate()
	{
		GenericChuteTemplate genericChuteTemplate = new GenericChuteTemplate
		{
			MainBuilding = new Box(29, 1, 47, 6)
		};
		genericChuteTemplate.Chutes = Tools.GenerateBoxes(genericChuteTemplate.MainBuilding, BoxGenerateOverlap.NeverOverlap, new Range(4, 4), new Range(10, 76), new Range(8, 20), new Range(200, 2000));
		return genericChuteTemplate;
	}

	public bool BuildZone(Zone Z, int chutes, bool chuteLevel)
	{
		nChutes = chutes;
		this.chuteLevel = chuteLevel;
		return BuildZone(Z);
	}

	public bool BuildZone(Zone Z)
	{
		GenTiles();
		ZoneManager zoneManager = XRLCore.Core.Game.ZoneManager;
		GenericChuteTemplate genericChuteTemplate = null;
		if (zoneManager.GetZoneColumnProperty(Z.ZoneID, "Builder.GenericChuteTemplate.ChuteTemplate") != null)
		{
			genericChuteTemplate = (GenericChuteTemplate)zoneManager.GetZoneColumnProperty(Z.ZoneID, "Builder.GenericChuteTemplate.ChuteTemplate");
		}
		else
		{
			genericChuteTemplate = GenerateGolgothaTemplate();
			zoneManager.SetZoneColumnProperty(Z.ZoneID, "Builder.GenericChuteTemplate.ChuteTemplate", genericChuteTemplate);
		}
		int num = 0;
		foreach (Box chute in genericChuteTemplate.Chutes)
		{
			string text = (string)XRLCore.Core.Game.ZoneManager.GetZoneProperty(Z.GetZoneIDFromDirection("U"), "Chute" + num + "EndY");
			string text2 = (string)XRLCore.Core.Game.ZoneManager.GetZoneProperty(Z.GetZoneIDFromDirection("D"), "Chute" + num + "StartY");
			int num2 = (chute.Width - 2) / 4;
			int num3 = (chute.Height - 2) / 3;
			if (num2 < 1)
			{
				num2 = 2;
			}
			if (num3 < 1)
			{
				num3 = 2;
			}
			if (text == null)
			{
				text = Stat.Random(0, num3 - 1).ToString();
			}
			if (text2 == null)
			{
				text2 = Stat.Random(0, num3 - 1).ToString();
			}
			Convert.ToInt32(text);
			Convert.ToInt32(text2);
			The.ZoneManager.SetZoneProperty(Z.ZoneID, "Chute" + num + "StartY", text);
			The.ZoneManager.SetZoneProperty(Z.ZoneID, "Chute" + num + "EndY", text2);
			TunnelMaker tunnelMaker = ((Z.Z % 2 != 0) ? new TunnelMaker(num2, num3, text, text2, "WNS") : new TunnelMaker(num2, num3, text, text2, "ENS"));
			bool flag = Z.Z % 2 == 0;
			if (chuteLevel)
			{
				for (int i = 0; i < tunnelMaker.Height; i++)
				{
					for (int j = 0; j < tunnelMaker.Width; j++)
					{
						int num4 = j * 4 + chute.x1 + 1;
						int num5 = i * 3 + chute.y1 + 1;
						string text3 = tunnelMaker.Map[j, i];
						switch (text3)
						{
						case "N":
						case "S":
						case "E":
						case "W":
							text3 = (((!flag || j != 0) && (flag || j != tunnelMaker.Width - 1)) ? (text3 + "END") : (text3 + "START"));
							break;
						}
						string[] array = Tiles[text3];
						for (int k = 0; k < 4; k++)
						{
							for (int l = 0; l < 3; l++)
							{
								char c = array[l][k];
								switch (c)
								{
								case '>':
									Z.GetCell(num4 + k, num5 + l).Clear();
									Z.GetCell(num4 + k, num5 + l).AddObject("SlimyShaft");
									continue;
								case 'C':
									if (Z.Z > 10)
									{
										Z.GetCell(num4 + k, num5 + l).Clear();
										Z.GetCell(num4 + k, num5 + l).AddObject("ConveyorDrive");
									}
									continue;
								case '.':
									continue;
								}
								if (Z.Z > 10)
								{
									Z.GetCell(num4 + k, num5 + l).Clear();
									GameObject gameObject = GameObjectFactory.Factory.CreateObject("ConveyorPad");
									ConveyorPad part = gameObject.GetPart<ConveyorPad>();
									part.Direction = c.ToString();
									part.Connections = c.ToString();
									Z.GetCell(num4 + k, num5 + l).AddObject(gameObject);
								}
							}
						}
					}
				}
				if (Z.Z > 10)
				{
					int m = 0;
					int num6 = 0;
					int num7 = Stat.Random(6, 10);
					for (; m < 100; m++)
					{
						if (num6 >= num7)
						{
							break;
						}
						int x = Stat.Random(chute.x1, chute.x2);
						int y = Stat.Random(chute.y1, chute.y2);
						if (!Z.GetCell(x, y).HasWall())
						{
							continue;
						}
						bool flag2 = false;
						foreach (Cell cardinalAdjacentCell in Z.GetCell(x, y).GetCardinalAdjacentCells(bLocalOnly: true))
						{
							if (cardinalAdjacentCell.HasObjectWithPart("ConveyorPad"))
							{
								flag2 = true;
								break;
							}
						}
						if (flag2)
						{
							num6++;
							Z.GetCell(x, y).Clear();
							GameObject gameObject2 = null;
							if (num == 0)
							{
								gameObject2 = GameObjectFactory.Factory.CreateObject("WalltrapFire");
								Walltrap part2 = gameObject2.GetPart<Walltrap>();
								part2.TurnInterval = Stat.Random(4, 8);
								part2.CurrentTurn = Stat.Random(0, part2.TurnInterval - 2);
							}
							if (num == 1)
							{
								gameObject2 = GameObjectFactory.Factory.CreateObject("WalltrapAcid");
								Walltrap part3 = gameObject2.GetPart<Walltrap>();
								part3.TurnInterval = Stat.Random(12, 16);
								part3.CurrentTurn = Stat.Random(0, part3.TurnInterval - 2);
							}
							if (num == 2)
							{
								gameObject2 = GameObjectFactory.Factory.CreateObject("WalltrapShock");
								Walltrap part4 = gameObject2.GetPart<Walltrap>();
								part4.TurnInterval = Stat.Random(4, 8);
								part4.CurrentTurn = Stat.Random(0, part4.TurnInterval - 2);
							}
							if (num == 3)
							{
								gameObject2 = GameObjectFactory.Factory.CreateObject("WalltrapCrabs");
								Walltrap part5 = gameObject2.GetPart<Walltrap>();
								part5.TurnInterval = Stat.Random(4, 16);
								part5.CurrentTurn = Stat.Random(0, part5.TurnInterval - 2);
							}
							Z.GetCell(x, y).AddObject(gameObject2);
						}
					}
				}
			}
			else
			{
				for (int n = 0; n < tunnelMaker.Height; n++)
				{
					for (int num8 = 0; num8 < tunnelMaker.Width; num8++)
					{
						int num9 = num8 * 4 + chute.x1 + 1;
						int num10 = n * 3 + chute.y1 + 1;
						string text4 = tunnelMaker.Map[num8, n];
						switch (text4)
						{
						case "N":
						case "S":
						case "E":
						case "W":
							text4 = (((!flag || num8 != 0) && (flag || num8 != tunnelMaker.Width - 1)) ? (text4 + "END") : (text4 + "START"));
							break;
						}
						string[] array2 = Tiles[text4];
						for (int num11 = 0; num11 < 4; num11++)
						{
							for (int num12 = 0; num12 < 3; num12++)
							{
								switch (array2[num12][num11])
								{
								case '>':
									foreach (Cell localAdjacentCell in Z.GetCell(num9 + num11, num10 + num12).GetLocalAdjacentCells())
									{
										localAdjacentCell.Clear();
									}
									break;
								case 'C':
									foreach (Cell localAdjacentCell2 in Z.GetCell(num9 + num11, num10 + num12).GetLocalAdjacentCells())
									{
										localAdjacentCell2.Clear();
									}
									break;
								}
							}
						}
					}
				}
			}
			num++;
			if (num >= nChutes)
			{
				break;
			}
		}
		return true;
	}
}
