using XRL.Rules;

namespace XRL.World.ZoneBuilders;

public class IdolFight
{
	public bool BuildZone(Zone Z)
	{
		for (int i = 0; i < 2500; i++)
		{
			int num = Stat.Random(0, 79);
			int num2 = Stat.Random(0, 24);
			if (!Z.GetCell(num, num2).HasObjectWithPart("LiquidVolume"))
			{
				continue;
			}
			foreach (Cell localAdjacentCell in Z.GetCell(num, num2).GetLocalAdjacentCells())
			{
				if (localAdjacentCell.GetObjectCountWithPart("LiquidVolume") == 1)
				{
					Z.GetCell(num, num2).AddObject("Crazed Goatfolk Shaman 1");
					int num3 = num + Stat.Random(-5, 5);
					int num4 = num2 + Stat.Random(-5, 5);
					while (num3 < 0 || num4 < 0 || num3 > 79 || num4 > 24 || Z.GetCell(num3, num4).PathDistanceTo(Z.GetCell(num, num2)) < 3)
					{
						num3 = num + Stat.Random(-3, 3);
						num4 = num2 + Stat.Random(-3, 3);
					}
					Z.GetCell(num3, num4).AddObject("Crazed Goatfolk Shaman 2");
					return true;
				}
			}
		}
		return false;
	}
}
