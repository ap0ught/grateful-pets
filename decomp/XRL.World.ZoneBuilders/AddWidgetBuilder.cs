namespace XRL.World.ZoneBuilders;

public class AddWidgetBuilder
{
	public string Blueprint;

	public string Object;

	public bool BuildZone(Zone Z)
	{
		if (!Blueprint.IsNullOrEmpty())
		{
			Z.GetCell(0, 0)?.RequireObject(Blueprint);
		}
		if (!Object.IsNullOrEmpty())
		{
			GameObject cachedObjects = The.ZoneManager.GetCachedObjects(Object);
			if (cachedObjects != null)
			{
				Z.GetCell(0, 0)?.AddObject(cachedObjects);
			}
		}
		return true;
	}
}
