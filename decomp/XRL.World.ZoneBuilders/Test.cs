namespace XRL.World.ZoneBuilders;

public class Test
{
	public bool BuildZone(Zone Z)
	{
		for (int i = 0; i < Z.Height; i++)
		{
			for (int j = 0; j < Z.Width; j++)
			{
				if (5.in100())
				{
					Z.GetCell(j, i).AddObject("Wall");
				}
				else if (5.in100())
				{
					Z.GetCell(j, i).AddObject("Battle Axe");
				}
				else if (5.in100())
				{
					Z.GetCell(j, i).AddObject("Wof").MakeActive();
				}
			}
		}
		return true;
	}
}
