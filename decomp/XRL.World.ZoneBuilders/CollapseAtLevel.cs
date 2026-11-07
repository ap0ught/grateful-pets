namespace XRL.World.ZoneBuilders;

public class CollapseAtLevel
{
	public int Level = 5;

	public bool BuildZone(Zone Z)
	{
		if (The.Player.Statistics["Level"].Value < Level)
		{
			return true;
		}
		Z.ClearReachableMap(bValue: true);
		for (int i = 0; i < Z.Height; i++)
		{
			for (int j = 0; j < Z.Width; j++)
			{
				Z.GetCell(j, i).Clear();
				Z.GetCell(j, i).AddObject("Halite");
			}
		}
		return true;
	}
}
