namespace XRL.World.ZoneBuilders;

public class FlagInside
{
	public bool BuildZone(Zone Z)
	{
		Z.SetInside(true);
		foreach (GameObject @object in Z.GetObjects("DaylightWidget"))
		{
			@object.Destroy();
		}
		return true;
	}
}
