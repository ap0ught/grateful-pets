namespace XRL.World.ZoneBuilders;

public class SolidEarth
{
	public bool BuildZone(Zone Z)
	{
		for (int i = 0; i < Z.Width; i++)
		{
			for (int j = 0; j < Z.Height; j++)
			{
				Z.GetCell(i, j).Clear();
				Z.GetCell(i, j).AddObject(GameObjectFactory.Factory.CreateObject("Shale"));
			}
		}
		return true;
	}
}
