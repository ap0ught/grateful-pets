using Genkit;
using XRL.Rules;

namespace XRL.World.ZoneBuilders;

public class Tarland
{
	public bool BuildZone(Zone Z)
	{
		CellularGrid cellularGrid = new CellularGrid();
		cellularGrid.Passes = 3;
		cellularGrid.SeedChance = 55;
		cellularGrid.SeedBorders = true;
		cellularGrid.Generate(Stat.Rand, Z.Width, Z.Height);
		CellularGrid cellularGrid2 = new CellularGrid();
		cellularGrid2.Passes = 3;
		cellularGrid2.SeedChance = 45;
		cellularGrid2.SeedBorders = true;
		cellularGrid2.Generate(Stat.Rand, Z.Width, Z.Height);
		for (int i = 0; i < Z.Height; i++)
		{
			for (int j = 0; j < Z.Width; j++)
			{
				if (cellularGrid.cells[j, i] == 0)
				{
					Z.GetCell(j, i).Clear().AddObject("AsphaltPuddle");
				}
				else if (cellularGrid2.cells[j, i] == 0)
				{
					Z.GetCell(j, i).Clear().AddObject("Shale");
				}
			}
		}
		Z.GetCell(0, 0).AddObject("Dirty");
		return true;
	}
}
