using System.Collections.Generic;
using XRL.Rules;
using XRL.World.Parts;

namespace XRL.World.ZoneBuilders;

public class PossibleCryotube : ZoneBuilderSandbox
{
	public int Chance = 20;

	public int ExtraChancePerLevel = 10;

	public int ChanceOfAdditionalTubes = 10;

	public bool BuildZone(Zone Z)
	{
		if (Stat.Random(1, 10000) > Chance + ExtraChancePerLevel * (Z.Z - 10))
		{
			return true;
		}
		int num = 1;
		while (Stat.Random(1, 10000) <= ChanceOfAdditionalTubes)
		{
			num++;
		}
		for (int i = 0; i < num; i++)
		{
			Cell cellWithEmptyBorder = Z.GetCellWithEmptyBorder(2);
			if (cellWithEmptyBorder == null)
			{
				continue;
			}
			string text = PopulationManager.RollOneFrom("CryotubeContents").Blueprint;
			int x = cellWithEmptyBorder.X;
			int y = cellWithEmptyBorder.Y;
			for (int j = y - 2; j <= y + 2; j++)
			{
				for (int k = x - 2; k <= x + 2; k++)
				{
					if (Z.GetCell(k, j).HasObjectWithTag("Stairs"))
					{
						text = "*Destroyed";
					}
					string zoneIDFromDirection = Z.GetZoneIDFromDirection("U");
					if (The.ZoneManager.IsZoneLive(zoneIDFromDirection) && Z.GetZoneFromDirection("U").GetCell(k, j).HasObjectWithTag("Stairs"))
					{
						text = "*Destroyed";
					}
					string zoneIDFromDirection2 = Z.GetZoneIDFromDirection("D");
					if (The.ZoneManager.IsZoneLive(zoneIDFromDirection2) && Z.GetZoneFromDirection("D").GetCell(k, j).HasObjectWithTag("Stairs"))
					{
						text = "*Destroyed";
					}
				}
			}
			Cryobarrio2.MakeCryochamber(cellWithEmptyBorder.X, cellWithEmptyBorder.Y, Z, text);
			if (!(text == "*Destroyed"))
			{
				continue;
			}
			Physics.LegacyApplyExplosion(cellWithEmptyBorder, new List<Cell>(), new List<GameObject>(), 35000, Local: true, Show: false);
			for (int l = y - 1; l <= y + 1; l++)
			{
				for (int m = x - 1; m <= x + 1; m++)
				{
					if (5.in100())
					{
						Z.GetCell(m, l).AddObject("ConvalessencePuddle");
					}
				}
			}
			Z.GetCell(x - 2, y - 2).Clear();
			Z.GetCell(x + 2, y - 2).Clear();
			Z.GetCell(x - 2, y + 2).Clear();
			Z.GetCell(x + 2, y + 2).Clear();
			Z.GetCell(x - 2, y - 2).AddObject("CryochamberWallBroken");
			Z.GetCell(x + 2, y - 2).AddObject("CryochamberWallBroken");
			Z.GetCell(x - 2, y + 2).AddObject("CryochamberWallBroken");
			Z.GetCell(x + 2, y + 2).AddObject("CryochamberWallBroken");
		}
		return true;
	}
}
