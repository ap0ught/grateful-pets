namespace XRL.World.ZoneBuilders;

public class DenseBrinestalk
{
	public bool BuildZone(Zone Z)
	{
		for (int i = 0; i < Z.Height; i++)
		{
			for (int j = 0; j < Z.Width; j++)
			{
				if (Z.GetCell(j, i).IsEmpty() && 25.in100())
				{
					Z.GetCell(j, i).AddObject("Brinestalk");
				}
			}
		}
		return true;
	}
}
