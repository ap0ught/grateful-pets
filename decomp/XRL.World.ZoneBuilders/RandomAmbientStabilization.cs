using XRL.World.ZoneParts;

namespace XRL.World.ZoneBuilders;

public class RandomAmbientStabilization
{
	public string Strength = "40";

	public bool BuildZone(Zone Zone)
	{
		Zone.RequirePart<AmbientStabilization>().Strength = Strength.RollCached();
		return true;
	}
}
