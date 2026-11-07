using System;
using XRL.Core;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.ZoneBuilders;

public class BasicRoomHall
{
	public static readonly int MAX_ATTEMPTS = 100;

	public string WallFill;

	private int[,] Map;

	private Zone zZone;

	public bool BuildZone(Zone Z)
	{
		int num = 0;
		int num2;
		int num3;
		while (true)
		{
			IL_0002:
			num++;
			Popup._ScreenBuffer.Clear();
			zZone = Z;
			Map = new int[Z.Width, Z.Height];
			for (int i = 0; i < Z.Height; i++)
			{
				for (int j = 0; j < Z.Width; j++)
				{
					Map[j, i] = 0;
				}
			}
			num2 = Z.Width / 2;
			num3 = Z.Height / 2;
			MetricsManager.rngCheckpoint("roomhall1");
			foreach (ZoneConnection zoneConnection in XRLCore.Core.Game.ZoneManager.GetZoneConnections(Z.ZoneID))
			{
				if (zoneConnection.X <= 2 || zoneConnection.X >= Z.Width - 2 || zoneConnection.Y <= 2 || zoneConnection.Y >= Z.Height - 2)
				{
					continue;
				}
				num2 = zoneConnection.X;
				num3 = zoneConnection.Y;
				goto IL_01a5;
			}
			MetricsManager.rngCheckpoint("roomhall2");
			foreach (CachedZoneConnection item in Z.ZoneConnectionCache)
			{
				if (item.TargetDirection == "-" && item.X > 2 && item.X < Z.Width - 2 && item.Y > 2 && item.Y < Z.Height - 2)
				{
					num2 = item.X;
					num3 = item.Y;
					break;
				}
			}
			goto IL_01a5;
			IL_01a5:
			while (!BuildRoom(num2, num3, Directions.GetRandomDirection()))
			{
			}
			MetricsManager.rngCheckpoint("roomhall3");
			foreach (ZoneConnection zoneConnection2 in XRLCore.Core.Game.ZoneManager.GetZoneConnections(Z.ZoneID))
			{
				if (Map[zoneConnection2.X, zoneConnection2.Y] != 0)
				{
					continue;
				}
				Draw();
				bool flag = false;
				if (zoneConnection2.Y == 0)
				{
					for (int k = 0; k < Z.Height - 1; k++)
					{
						if (Map[zoneConnection2.X, k] != 0)
						{
							flag = true;
							break;
						}
						Map[zoneConnection2.X, k] = 3;
					}
				}
				else if (zoneConnection2.Y == Z.Height - 1)
				{
					for (int num4 = zoneConnection2.Y; num4 > 0; num4--)
					{
						if (Map[zoneConnection2.X, num4] != 0)
						{
							flag = true;
							break;
						}
						Map[zoneConnection2.X, num4] = 3;
					}
				}
				else if (zoneConnection2.X == 0)
				{
					for (int l = 0; l < Z.Width - 1; l++)
					{
						if (Map[l, zoneConnection2.Y] != 0)
						{
							flag = true;
							break;
						}
						Map[l, zoneConnection2.Y] = 3;
					}
				}
				else if (zoneConnection2.X == Z.Width - 1)
				{
					for (int num5 = zoneConnection2.X; num5 > 0; num5--)
					{
						if (Map[num5, zoneConnection2.Y] != 0)
						{
							flag = true;
							break;
						}
						Map[num5, zoneConnection2.Y] = 3;
					}
				}
				else
				{
					if (zoneConnection2.X < 50)
					{
						for (int num6 = zoneConnection2.X; num6 > 0; num6--)
						{
							if (Map[num6, zoneConnection2.Y] != 0)
							{
								flag = true;
								break;
							}
							Map[num6, zoneConnection2.Y] = 3;
						}
					}
					if (zoneConnection2.X > 20)
					{
						for (int m = zoneConnection2.X; m < Z.Width; m++)
						{
							if (Map[m, zoneConnection2.Y] != 0)
							{
								flag = true;
								break;
							}
							Map[m, zoneConnection2.Y] = 3;
						}
					}
					if (zoneConnection2.Y < 20)
					{
						for (int num7 = zoneConnection2.Y; num7 > 0; num7--)
						{
							if (Map[zoneConnection2.X, num7] != 0)
							{
								flag = true;
								break;
							}
							Map[zoneConnection2.X, num7] = 3;
						}
					}
					if (zoneConnection2.Y > 10)
					{
						for (int n = zoneConnection2.Y; n < Z.Height; n++)
						{
							if (Map[zoneConnection2.X, n] != 0)
							{
								flag = true;
								break;
							}
							Map[zoneConnection2.X, n] = 3;
						}
					}
				}
				if (!flag && num < MAX_ATTEMPTS)
				{
					goto IL_0002;
				}
			}
			MetricsManager.rngCheckpoint("roomhall4");
			foreach (CachedZoneConnection item2 in Z.ZoneConnectionCache)
			{
				if (!(item2.TargetDirection == "-"))
				{
					continue;
				}
				bool flag2 = false;
				if (item2.Y == 0)
				{
					for (int num8 = 0; num8 < Z.Height - 1; num8++)
					{
						if (Map[item2.X, num8] != 0)
						{
							flag2 = true;
							break;
						}
						Map[item2.X, num8] = 3;
					}
				}
				else if (item2.Y == Z.Height - 1)
				{
					for (int num9 = item2.Y; num9 > 0; num9--)
					{
						if (Map[item2.X, num9] != 0)
						{
							flag2 = true;
							break;
						}
						Map[item2.X, num9] = 3;
					}
				}
				else if (item2.X == 0)
				{
					for (int num10 = 0; num10 < Z.Width - 1; num10++)
					{
						if (Map[num10, item2.Y] != 0)
						{
							flag2 = true;
							break;
						}
						Map[num10, item2.Y] = 3;
					}
				}
				else if (item2.X == Z.Width - 1)
				{
					for (int num11 = item2.X; num11 > 0; num11--)
					{
						if (Map[num11, item2.Y] != 0)
						{
							flag2 = true;
							break;
						}
						Map[num11, item2.Y] = 3;
					}
				}
				else
				{
					if (item2.X > 20)
					{
						for (int num12 = item2.X; num12 > 0; num12--)
						{
							if (Map[num12, item2.Y] != 0)
							{
								flag2 = true;
								break;
							}
							Map[num12, item2.Y] = 3;
						}
					}
					if (item2.X < 50)
					{
						for (int num13 = item2.X; num13 < Z.Width; num13++)
						{
							if (Map[num13, item2.Y] != 0)
							{
								flag2 = true;
								break;
							}
							Map[num13, item2.Y] = 3;
						}
					}
					if (item2.Y > 10)
					{
						for (int num14 = item2.Y; num14 > 0; num14--)
						{
							if (Map[item2.X, num14] != 0)
							{
								flag2 = true;
								break;
							}
							Map[item2.X, num14] = 3;
						}
					}
					if (item2.Y < 20)
					{
						for (int num15 = item2.Y; num15 < Z.Height; num15++)
						{
							if (Map[item2.X, num15] != 0)
							{
								flag2 = true;
								break;
							}
							Map[item2.X, num15] = 3;
						}
					}
				}
				if (!flag2 && num < MAX_ATTEMPTS)
				{
					goto IL_0002;
				}
			}
			break;
		}
		MetricsManager.rngCheckpoint("roomhall5");
		for (int num16 = 0; num16 < Z.Height; num16++)
		{
			for (int num17 = 0; num17 < Z.Width; num17++)
			{
				if (Map[num17, num16] != 0)
				{
					Z.GetCell(num17, num16).Clear();
				}
				_ = Map[num17, num16];
				_ = 4;
			}
		}
		MetricsManager.rngCheckpoint("roomhall6");
		new RiverBuilder().BuildZone(Z);
		Z.ClearReachableMap();
		Z.BuildReachableMap(num2, num3);
		return true;
	}

	private bool BuildRoom(int x, int y, string Direction)
	{
		int num = Stat.Random(5, 8);
		int num2;
		int num3;
		if (Direction.Contains("N"))
		{
			num2 = y - num;
			num3 = y;
		}
		else if (Direction.Contains("S"))
		{
			num2 = y;
			num3 = y + num;
		}
		else
		{
			num2 = (int)((double)y - Math.Floor((double)(num / 2)));
			num3 = (int)((double)y + Math.Ceiling((double)(num / 2)));
		}
		int num4;
		int num5;
		if (Direction.Contains("W"))
		{
			num4 = x - num;
			num5 = x;
		}
		else if (Direction.Contains("E"))
		{
			num4 = x;
			num5 = x + num;
		}
		else
		{
			num4 = (int)((double)x - Math.Floor((double)(num / 2)));
			num5 = (int)((double)x + Math.Ceiling((double)(num / 2)));
		}
		if (num4 < 1)
		{
			return false;
		}
		if (num2 < 1)
		{
			return false;
		}
		if (num5 > zZone.Width - 2)
		{
			return false;
		}
		if (num3 > zZone.Height - 2)
		{
			return false;
		}
		for (int i = num4; i <= num5; i++)
		{
			for (int j = num2; j <= num3; j++)
			{
				if (Map[i, j] != 0)
				{
					return false;
				}
			}
		}
		for (int k = num4; k <= num5; k++)
		{
			for (int l = num2; l <= num3; l++)
			{
				Map[k, l] = 1;
			}
		}
		int m = 0;
		for (int num6 = Stat.Random(6, 8); m < num6; m++)
		{
			int num7 = m % 4 + 1;
			if (num7 == 1)
			{
				for (int n = 0; n < 10; n++)
				{
					if (BuildHallway(Stat.Random(num4, num5), num2 - 1, "N"))
					{
						break;
					}
				}
			}
			if (num7 == 2)
			{
				for (int num8 = 0; num8 < 10; num8++)
				{
					if (BuildHallway(Stat.Random(num4, num5), num3 + 1, "N"))
					{
						break;
					}
				}
			}
			if (num7 == 3)
			{
				for (int num9 = 0; num9 < 10; num9++)
				{
					if (BuildHallway(num4 - 1, Stat.Random(num2, num3), "W"))
					{
						break;
					}
				}
			}
			if (num7 != 4)
			{
				continue;
			}
			for (int num10 = 0; num10 < 10; num10++)
			{
				if (BuildHallway(num5 + 1, Stat.Random(num2, num3), "E"))
				{
					break;
				}
			}
		}
		return true;
	}

	private bool BuildHallway(int x, int y, string Direction)
	{
		if (x < 1)
		{
			return false;
		}
		if (y < 1)
		{
			return false;
		}
		if (x > zZone.Width - 2)
		{
			return false;
		}
		if (y > zZone.Height - 2)
		{
			return false;
		}
		int num = Stat.Random(1, 6) + Stat.Random(1, 6);
		int num2 = 0;
		int num3 = 0;
		if (Direction == "N")
		{
			num2 = x;
			num3 = y - num;
			if (Map[x + 1, y] != 0)
			{
				return false;
			}
			if (Map[x - 1, y] != 0)
			{
				return false;
			}
		}
		if (Direction == "S")
		{
			num2 = x;
			num3 = y - num;
			if (Map[x + 1, y] != 0)
			{
				return false;
			}
			if (Map[x - 1, y] != 0)
			{
				return false;
			}
		}
		if (Direction == "E")
		{
			num2 = x + num;
			num3 = y;
			if (Map[x, y - 1] != 0)
			{
				return false;
			}
			if (Map[x, y + 1] != 0)
			{
				return false;
			}
		}
		if (Direction == "W")
		{
			num2 = x - num;
			num3 = y;
			if (Map[x, y - 1] != 0)
			{
				return false;
			}
			if (Map[x, y + 1] != 0)
			{
				return false;
			}
		}
		if (num2 < 1)
		{
			return false;
		}
		if (num3 < 1)
		{
			return false;
		}
		if (num2 > zZone.Width - 2)
		{
			return false;
		}
		if (num3 > zZone.Height - 2)
		{
			return false;
		}
		int x2 = x;
		int y2 = y;
		for (int i = 0; i < num; i++)
		{
			if (Map[x2, y2] != 0)
			{
				return false;
			}
			Directions.ApplyDirection(Direction, ref x2, ref y2);
		}
		x2 = x;
		y2 = y;
		for (int j = 0; j < num; j++)
		{
			Map[x2, y2] = 2;
			Directions.ApplyDirection(Direction, ref x2, ref y2);
		}
		int num4 = 0;
		bool flag = false;
		while (!(flag = BuildRoom(x2, y2, Direction)) && num4 < 5)
		{
			num4++;
		}
		if (!flag)
		{
			x2 = x;
			y2 = y;
			for (int k = 0; k < num; k++)
			{
				Map[x2, y2] = 0;
				Directions.ApplyDirection(Direction, ref x2, ref y2);
			}
			return false;
		}
		return true;
	}

	public void Draw()
	{
	}
}
