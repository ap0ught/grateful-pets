using XRL.Rules;

namespace XRL.World.ZoneBuilders;

public class AddObjectBuilder
{
	public string Object;

	public string X;

	public string Y;

	public bool BuildZone(Zone Z)
	{
		for (int i = 0; i < 500; i++)
		{
			int x = X?.RollCached() ?? Stat.Random(0, Z.Width - 1);
			int y = Y?.RollCached() ?? Stat.Random(0, Z.Height - 1);
			if (Z.GetCell(x, y).IsEmpty())
			{
				GameObject cachedObjects = The.ZoneManager.GetCachedObjects(Object);
				Z.GetCell(x, y).AddObject(cachedObjects);
				return true;
			}
		}
		return true;
	}
}
