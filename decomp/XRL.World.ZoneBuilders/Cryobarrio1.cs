using System;
using System.Collections.Generic;
using XRL.Rules;
using XRL.World.Parts;

namespace XRL.World.ZoneBuilders;

public class Cryobarrio1
{
	public static void Websplat(int x, int y, Zone Z, string Object)
	{
		for (double num = Stat.Random(1, 30); num < 360.0; num += (double)Stat.Random(30, 60))
		{
			int num2 = Stat.Random(3, 9);
			for (int i = 0; i < num2; i++)
			{
				int x2 = x + (int)((float)Math.Sin(num.toRadians()) * (float)i);
				int y2 = y + (int)((float)Math.Cos(num.toRadians()) * (float)i);
				Cell cell = Z.GetCell(x2, y2);
				if (cell != null && !cell.HasObjectWithBlueprint(Object))
				{
					cell.AddObject(Object);
				}
			}
		}
	}

	public void MakeCryochamber(int x, int y, Zone Z, int n)
	{
		Z.ClearBox(new Box(x - 2, y - 2, x + 2, y + 2));
		Z.GetCell(x + 2, y - 1).AddObject("VGlassWall");
		Z.GetCell(x + 2, y).AddObject("VGlassWall");
		Z.GetCell(x + 2, y + 1).AddObject("VGlassWall");
		Z.GetCell(x - 2, y - 1).AddObject("VGlassWall");
		Z.GetCell(x - 2, y).AddObject("VGlassWall");
		Z.GetCell(x - 2, y + 1).AddObject("VGlassWall");
		Z.GetCell(x - 1, y + 2).AddObject("HGlassWall");
		if (n == 2)
		{
			Z.GetCell(x, y + 2).AddObject("CryochamberPlaqueSpider");
		}
		else if (n <= 3)
		{
			Z.GetCell(x, y + 2).AddObject("CryochamberPlaque");
		}
		else
		{
			Z.GetCell(x, y + 2).AddObject("HGlassWall");
		}
		Z.GetCell(x + 1, y + 2).AddObject("HGlassWall");
		Z.GetCell(x - 1, y - 2).AddObject("HGlassWall");
		if (n > 3)
		{
			Z.GetCell(x, y - 2).AddObject("CryochamberPlaque");
		}
		else
		{
			Z.GetCell(x, y - 2).AddObject("HGlassWall");
		}
		Z.GetCell(x + 1, y - 2).AddObject("HGlassWall");
		if (n == 2 || 50.in100())
		{
			Physics.LegacyApplyExplosion(Z.GetCell(x, y), new List<Cell>(), new List<GameObject>(), 35000, Local: true, Show: false, null, null, 1, 0.05f);
			if (50.in100())
			{
				for (int i = x - 3; i <= x + 3; i++)
				{
					for (int j = y - 3; j <= y + 3; j++)
					{
						if (Stat.Random(1, 100) <= 60)
						{
							Z.GetCell(i, j).AddObject("ConvalessencePuddle");
						}
					}
				}
				Z.GetCell(x - 2, y - 2).AddObject("CryochamberWallBroken");
				Z.GetCell(x + 2, y - 2).AddObject("CryochamberWallBroken");
				Z.GetCell(x - 2, y + 2).AddObject("CryochamberWallBroken");
				Z.GetCell(x + 2, y + 2).AddObject("CryochamberWallBroken");
			}
			else
			{
				Z.GetCell(x - 2, y - 2).AddObject("CryochamberWallSE");
				Z.GetCell(x + 2, y - 2).AddObject("CryochamberWallSW");
				Z.GetCell(x - 2, y + 2).AddObject("CryochamberWallNE");
				Z.GetCell(x + 2, y + 2).AddObject("CryochamberWallNW");
			}
		}
		else
		{
			Z.FillHollowBox(new Box(x - 1, y - 1, x + 1, y + 1), "StableCryoGas");
			Z.GetCell(x - 2, y - 2).AddObject("CryochamberWallSE");
			Z.GetCell(x + 2, y - 2).AddObject("CryochamberWallSW");
			Z.GetCell(x - 2, y + 2).AddObject("CryochamberWallNE");
			Z.GetCell(x + 2, y + 2).AddObject("CryochamberWallNW");
		}
	}

	public bool BuildZone(Zone Z)
	{
		Z.Fill("Marble");
		Z.FillBox(new Box(0, 0, Z.Width - 1, Z.Height - 1), "Fulcrete");
		Z.ClearBox(new Box(10, 2, 70, 21));
		Z.FillHollowBox(new Box(9, 2, 71, 22), "Marble");
		MakeCryochamber(20, 6, Z, 1);
		MakeCryochamber(40, 6, Z, 2);
		MakeCryochamber(60, 6, Z, 3);
		MakeCryochamber(20, 18, Z, 4);
		MakeCryochamber(40, 18, Z, 5);
		MakeCryochamber(60, 18, Z, 6);
		Websplat(65, 8, Z, "PhaseWeb");
		for (int i = 0; i < 3; i++)
		{
			Websplat(Stat.Random(0, Z.Width - 1), Stat.Random(0, Z.Height - 1), Z, "PhaseWeb");
		}
		Z.FillRoundHollowBox(new Box(65, 8, 79, 16), "Marble");
		Z.GetCell(69, 12).AddObject("Phase Spider");
		Z.GetCell(65, 12).Clear();
		Z.GetCell(65, 12).AddObject("Fused Security Door");
		return true;
	}
}
