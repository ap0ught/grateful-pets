namespace XRL.World.ZoneBuilders;

public class IsCheckpoint
{
	public string Key;

	public bool BuildZone(Zone Z)
	{
		Z.GetCell(0, 0).RequireObject("CheckpointWidget").SetStringProperty("CheckpointKey", Key);
		return true;
	}

	public bool BuildZoneWithKey(Zone Z, string key)
	{
		Z.GetCell(0, 0).RequireObject("CheckpointWidget").SetStringProperty("CheckpointKey", key);
		return true;
	}
}
