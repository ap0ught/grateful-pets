using XRL.Rules;

namespace XRL.World.ZoneBuilders;

public class TestHut
{
	public bool BuildZone(Zone Z)
	{
		int num = Stat.Random(0, 70);
		int num2 = Stat.Random(0, 19);
		for (int i = 0; i < 5; i++)
		{
			for (int j = 0; j < 5; j++)
			{
				Cell cell = Z.GetCell(i + num, j + num2);
				cell.Objects.Clear();
				cell.AddObject("BrinestalkWall");
			}
		}
		return true;
	}
}
