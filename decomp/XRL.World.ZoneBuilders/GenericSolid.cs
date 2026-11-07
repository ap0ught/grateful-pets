namespace XRL.World.ZoneBuilders;

public class GenericSolid
{
	public string Material = "Shale";

	public bool ClearFirst = true;

	public bool BuildZone(Zone Z)
	{
		for (int i = 0; i < Z.Height; i++)
		{
			for (int j = 0; j < Z.Width; j++)
			{
				if (ClearFirst)
				{
					Z.GetCell(j, i).Clear();
				}
				Z.GetCell(j, i).AddObject(Material);
			}
		}
		return true;
	}
}
