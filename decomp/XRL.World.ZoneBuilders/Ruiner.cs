using System.Collections.Generic;
using XRL.Core;
using XRL.Rules;
using XRL.World.Parts;

namespace XRL.World.ZoneBuilders;

public class Ruiner
{
	private Zone Z;

	public int RuinAmount = 50;

	public void BlowUp(int x, int y, int Force, bool bUnderground)
	{
		Physics.LegacyApplyExplosion(Z.GetCell(x, y), new List<Cell>(), new List<GameObject>(), Force, Local: true, Show: false, null, null, 1, 0.05f);
	}

	public bool BuildZone(Zone _Z)
	{
		return RuinZone(_Z, RuinAmount, bUnderground: true);
	}

	public bool RuinZone(Zone _Z, int RuinLevel, bool bUnderground, int SurfaceExplosionForce = 100000, int UndergroundExplosionForce = 500000)
	{
		Z = _Z;
		int num = Stat.Random(0, 3);
		num += RuinLevel / 5;
		int force = (bUnderground ? UndergroundExplosionForce : SurfaceExplosionForce);
		for (int i = 0; i < num; i++)
		{
			BlowUp(Stat.Random(0, Z.Width - 1), Stat.Random(0, Z.Height - 1), force, bUnderground);
		}
		if (!bUnderground)
		{
			_Z.GetCell(0, 0).AddObject(GameObjectFactory.Factory.CreateObject("DaylightWidget"));
		}
		foreach (CachedZoneConnection item in Z.ZoneConnectionCache)
		{
			if (item.TargetDirection != "-")
			{
				Z.GetCell(item.X, item.Y).Clear();
			}
		}
		foreach (ZoneConnection zoneConnection in XRLCore.Core.Game.ZoneManager.GetZoneConnections(Z.ZoneID))
		{
			Z.GetCell(zoneConnection.X, zoneConnection.Y).Clear();
		}
		CleanNarrowConnections(Z);
		CleanOrphanDoors(Z);
		Z.ClearReachableMap();
		foreach (CachedZoneConnection item2 in Z.ZoneConnectionCache)
		{
			Z.GetCell(item2.X, item2.Y).Clear();
			if (Z.BuildReachableMap(item2.X, item2.Y) > 400)
			{
				return true;
			}
			Z.ClearReachableMap();
		}
		for (int j = 0; j < 10; j++)
		{
			if (Z.BuildReachableMap(Stat.Random(0, Z.Width - 1), Stat.Random(0, Z.Height - 1)) > 400)
			{
				return true;
			}
			Z.ClearReachableMap();
		}
		return false;
	}

	public static void CleanNarrowConnections(Zone Z)
	{
		bool flag = true;
		while (flag)
		{
			flag = false;
			int i = 1;
			for (int num = Z.Height - 1; i < num; i++)
			{
				int j = 1;
				for (int num2 = Z.Width - 1; j < num2; j++)
				{
					if (!Z.GetCell(j, i).IsEmpty())
					{
						continue;
					}
					if (Z.GetCell(j + 1, i).HasWall() && Z.GetCell(j, i + 1).HasWall() && !Z.GetCell(j + 1, i + 1).HasWall())
					{
						if (Stat.Random(0, 1) == 0)
						{
							Z.GetCell(j + 1, i).Clear();
						}
						else
						{
							Z.GetCell(j, i + 1).Clear();
						}
						flag = true;
					}
					if (Z.GetCell(j + 1, i).HasWall() && Z.GetCell(j, i - 1).HasWall() && !Z.GetCell(j + 1, i - 1).HasWall())
					{
						flag = true;
						if (Stat.Random(0, 1) == 0)
						{
							Z.GetCell(j + 1, i).Clear();
						}
						else
						{
							Z.GetCell(j, i - 1).Clear();
						}
					}
				}
			}
		}
	}

	private void CleanOrphanDoors(Zone Z)
	{
		int i = 1;
		for (int num = Z.Height - 1; i < num; i++)
		{
			int j = 1;
			for (int num2 = Z.Width - 1; j < num2; j++)
			{
				if (Z.GetCell(j, i).HasObjectWithPart("Door"))
				{
					int num3 = 0;
					int num4 = 0;
					if (Z.GetCell(j - 1, i).HasWall())
					{
						num4++;
					}
					if (Z.GetCell(j + 1, i).HasWall())
					{
						num4++;
					}
					if (Z.GetCell(j, i - 1).HasWall())
					{
						num3++;
					}
					if (Z.GetCell(j, i + 1).HasWall())
					{
						num3++;
					}
					if (num3 + num4 != 2 || num3 == 1 || num4 == 1)
					{
						Z.GetCell(j, i).Clear();
					}
				}
			}
		}
	}
}
