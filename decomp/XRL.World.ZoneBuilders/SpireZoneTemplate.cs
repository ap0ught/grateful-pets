using System;
using System.Collections.Generic;
using XRL.Core;
using XRL.Rules;

namespace XRL.World.ZoneBuilders;

[Serializable]
public class SpireZoneTemplate : IComposite
{
	private SpireTemplate Template;

	public int Width;

	public int Height;

	public int ZonesWide;

	public int ZonesHigh;

	public static int foo;

	public bool WantFieldReflection => false;

	public void Write(SerializationWriter Writer)
	{
		Writer.WriteObject(Template);
		Writer.WriteOptimized(Width);
		Writer.WriteOptimized(Height);
		Writer.WriteOptimized(ZonesWide);
		Writer.WriteOptimized(ZonesHigh);
	}

	public void Read(SerializationReader Reader)
	{
		Template = Reader.ReadObject() as SpireTemplate;
		Width = Reader.ReadOptimizedInt32();
		Height = Reader.ReadOptimizedInt32();
		ZonesWide = Reader.ReadOptimizedInt32();
		ZonesHigh = Reader.ReadOptimizedInt32();
	}

	public SpireZoneTemplate Copy()
	{
		return new SpireZoneTemplate
		{
			Width = Width,
			Height = Height,
			ZonesHigh = ZonesHigh,
			ZonesWide = ZonesWide,
			Template = new SpireTemplate(Template)
		};
	}

	public bool EnsureConnections(Zone Z)
	{
		foreach (ZoneConnection zoneConnection in XRLCore.Core.Game.ZoneManager.GetZoneConnections(Z.ZoneID))
		{
			if (Template.Map[zoneConnection.X, zoneConnection.Y] == SpireTemplateTile.Inside)
			{
				continue;
			}
			bool flag = false;
			if (zoneConnection.Y == 0)
			{
				for (int i = 0; i < Z.Height - 1; i++)
				{
					if (Template.Map[zoneConnection.X, i] == SpireTemplateTile.Inside)
					{
						flag = true;
						break;
					}
					Template.Map[zoneConnection.X, i] = SpireTemplateTile.Inside;
				}
			}
			else if (zoneConnection.Y == Z.Height - 1)
			{
				for (int num = zoneConnection.Y; num > 0; num--)
				{
					if (Template.Map[zoneConnection.X, num] == SpireTemplateTile.Inside)
					{
						flag = true;
						break;
					}
					Template.Map[zoneConnection.X, num] = SpireTemplateTile.Inside;
				}
			}
			else if (zoneConnection.X == 0)
			{
				for (int j = 0; j < Z.Width - 1; j++)
				{
					if (Template.Map[j, zoneConnection.Y] == SpireTemplateTile.Inside)
					{
						flag = true;
						break;
					}
					Template.Map[j, zoneConnection.Y] = SpireTemplateTile.Inside;
				}
			}
			else if (zoneConnection.X == Z.Width - 1)
			{
				for (int num2 = zoneConnection.X; num2 > 0; num2--)
				{
					if (Template.Map[num2, zoneConnection.Y] == SpireTemplateTile.Inside)
					{
						flag = true;
						break;
					}
					Template.Map[num2, zoneConnection.Y] = SpireTemplateTile.Inside;
				}
			}
			else
			{
				if (zoneConnection.X < 50)
				{
					for (int k = zoneConnection.X; k < Z.Width; k++)
					{
						if (Template.Map[k, zoneConnection.Y] == SpireTemplateTile.Inside)
						{
							flag = true;
							break;
						}
						Template.Map[k, zoneConnection.Y] = SpireTemplateTile.Inside;
					}
				}
				if (zoneConnection.X > 20)
				{
					for (int num3 = zoneConnection.X; num3 > 0; num3--)
					{
						if (Template.Map[num3, zoneConnection.Y] == SpireTemplateTile.Inside)
						{
							flag = true;
							break;
						}
						Template.Map[num3, zoneConnection.Y] = SpireTemplateTile.Inside;
					}
				}
				if (zoneConnection.Y < 20)
				{
					for (int l = zoneConnection.Y; l < Z.Height; l++)
					{
						if (Template.Map[zoneConnection.X, l] == SpireTemplateTile.Inside)
						{
							flag = true;
							break;
						}
						Template.Map[zoneConnection.X, l] = SpireTemplateTile.Inside;
					}
				}
				if (zoneConnection.Y > 10)
				{
					for (int num4 = zoneConnection.Y; num4 > 0; num4--)
					{
						if (Template.Map[zoneConnection.X, num4] == SpireTemplateTile.Inside)
						{
							flag = true;
							break;
						}
						Template.Map[zoneConnection.X, num4] = SpireTemplateTile.Inside;
					}
				}
			}
			if (!flag)
			{
				return false;
			}
		}
		foreach (CachedZoneConnection item in Z.ZoneConnectionCache)
		{
			if (!(item.TargetDirection == "-"))
			{
				continue;
			}
			bool flag2 = false;
			if (item.Y == 0)
			{
				for (int m = 0; m < Z.Height - 1; m++)
				{
					if (Template.Map[item.X, m] == SpireTemplateTile.Inside)
					{
						flag2 = true;
						break;
					}
					Template.Map[item.X, m] = SpireTemplateTile.Inside;
				}
			}
			else if (item.Y == Z.Height - 1)
			{
				for (int num5 = item.Y; num5 > 0; num5--)
				{
					if (Template.Map[item.X, num5] == SpireTemplateTile.Inside)
					{
						flag2 = true;
						break;
					}
					Template.Map[item.X, num5] = SpireTemplateTile.Inside;
				}
			}
			else if (item.X == 0)
			{
				for (int n = 0; n < Z.Width - 1; n++)
				{
					if (Template.Map[n, item.Y] == SpireTemplateTile.Inside)
					{
						flag2 = true;
						break;
					}
					Template.Map[n, item.Y] = SpireTemplateTile.Inside;
				}
			}
			else if (item.X == Z.Width - 1)
			{
				for (int num6 = item.X; num6 > 0; num6--)
				{
					if (Template.Map[num6, item.Y] == SpireTemplateTile.Inside)
					{
						flag2 = true;
						break;
					}
					Template.Map[num6, item.Y] = SpireTemplateTile.Inside;
				}
			}
			else
			{
				if (item.X > 20)
				{
					for (int num7 = item.X; num7 > 0; num7--)
					{
						if (Template.Map[num7, item.Y] == SpireTemplateTile.Inside)
						{
							flag2 = true;
							break;
						}
						Template.Map[num7, item.Y] = SpireTemplateTile.Inside;
					}
				}
				if (item.X < 50)
				{
					for (int num8 = item.X; num8 < Z.Width; num8++)
					{
						if (Template.Map[num8, item.Y] == SpireTemplateTile.Inside)
						{
							flag2 = true;
							break;
						}
						Template.Map[num8, item.Y] = SpireTemplateTile.Inside;
					}
				}
				if (item.Y > 10)
				{
					for (int num9 = item.Y; num9 < Z.Height; num9++)
					{
						if (Template.Map[item.X, num9] == SpireTemplateTile.Inside)
						{
							flag2 = true;
							break;
						}
						Template.Map[item.X, num9] = SpireTemplateTile.Inside;
					}
				}
				if (item.Y < 20)
				{
					for (int num10 = item.Y; num10 > 0; num10--)
					{
						if (Template.Map[item.X, num10] == SpireTemplateTile.Inside)
						{
							flag2 = true;
							break;
						}
						Template.Map[item.X, num10] = SpireTemplateTile.Inside;
					}
				}
			}
			if (!flag2)
			{
				return false;
			}
		}
		return true;
	}

	public void GenerateRooms(int Depth)
	{
		Template.GenerateRooms(Depth);
	}

	public void New(int nWidth, int nHeight, int ZonesWide, int ZonesHigh)
	{
		Template = new SpireTemplate(nWidth, nHeight);
		Width = nWidth;
		Height = nHeight;
		for (int i = 0; i < ZonesWide; i++)
		{
			for (int j = 0; j < ZonesHigh; j++)
			{
				int num = 80 / ZonesWide;
				int num2 = 25 / ZonesHigh;
				int startX = num * i;
				int startY = num2 * j;
				SpireTemplate spireTemplate = null;
				spireTemplate = new SpireTemplate(num, num2, 20);
				Template.AddMap(startX, startY, spireTemplate);
			}
		}
	}

	public void BuildZone(Zone Z, bool bUnderground)
	{
		foo++;
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		List<Cell> list = new List<Cell>();
		int num = Z.GetZoneZ() - 10;
		bool flag = false;
		for (int i = 0; i < Template.Height; i++)
		{
			for (int j = 0; j < Template.Width; j++)
			{
				SpireTemplateTile spireTemplateTile = Template.Map[j, i];
				Cell cell = Z.GetCell(j, i);
				switch (spireTemplateTile)
				{
				case SpireTemplateTile.OutsideWall:
				case SpireTemplateTile.Wall:
					cell.SetReachable(State: false);
					cell.AddObject("Fulcrete");
					break;
				case SpireTemplateTile.SecurityDoor:
				{
					string text = "Red Security Door";
					int num4 = "2d10".RollCached() + num;
					if (num4 < 10)
					{
						text = "Red Security Door";
					}
					else if (num4 < 15)
					{
						text = "Yellow Security Door";
					}
					else if (num4 < 20)
					{
						text = "Green Security Door";
					}
					else
					{
						text = ((num4 >= 25) ? "Purple Security Door" : "Blue Security Door");
					}
					cell.SetReachable(State: false);
					cell.AddObject(text);
					break;
				}
				case SpireTemplateTile.InsideSealed:
				{
					if (!dictionary.ContainsKey(Template.Rooms[j, i]))
					{
						dictionary.Add(Template.Rooms[j, i], Stat.Random(1, 100));
					}
					int num2 = dictionary[Template.Rooms[j, i]];
					if (num2 < 5)
					{
						cell.SetReachable(State: false);
						if (!flag && 2.in100())
						{
							cell.AddObject("SparkingBaetyl");
							flag = true;
						}
						else if (25.in100())
						{
							int num3 = ("2d30".RollCached() + num * 5) / 20;
							if (num3 < 1)
							{
								num3 = 1;
							}
							if (num3 > 8)
							{
								num3 = 8;
							}
							cell.AddObject(GameObjectFactory.create("Chest" + num3));
						}
						else if (45.in100())
						{
							cell.AddObject(GameObjectFactory.create(PopulationManager.RollOneFrom("Vault Robots").Blueprint));
						}
					}
					else if (num2 < 10)
					{
						cell.AddObject(PopulationManager.CreateOneFrom("Scrap 3"));
					}
					else if (num2 < 15)
					{
						cell.SetReachable(State: false);
						if (15.in100())
						{
							cell.AddObject("Locker");
						}
						else
						{
							cell.AddObject("Garbage");
						}
					}
					else if (num2 < 20)
					{
						cell.SetReachable(State: false);
						if (15.in100())
						{
							cell.AddObject("Garbage");
						}
						else
						{
							cell.AddObject(PopulationManager.CreateOneFrom("Scrap 1"));
						}
					}
					break;
				}
				case SpireTemplateTile.Door:
					cell.SetReachable(State: true);
					cell.AddObject("Door");
					break;
				case SpireTemplateTile.StairsDown:
					cell.SetReachable(State: true);
					if (Z.GetZoneZ() != 10)
					{
						if (Z.GetZoneZ() % 2 == 0)
						{
							cell.AddObject("StairsUp");
						}
						else
						{
							cell.AddObject("StairsDown");
						}
					}
					break;
				case SpireTemplateTile.StairsUp:
					cell.SetReachable(State: true);
					if (Z.GetZoneZ() % 2 == 0)
					{
						cell.AddObject("StairsDown");
					}
					else
					{
						cell.AddObject("StairsUp");
					}
					break;
				case SpireTemplateTile.Outside:
					if (bUnderground)
					{
						cell.AddObject("Sandstone");
						cell.SetReachable(State: false);
					}
					else
					{
						cell.SetReachable(State: true);
					}
					break;
				default:
					list.Add(cell);
					cell.SetReachable(State: true);
					break;
				}
			}
		}
	}
}
