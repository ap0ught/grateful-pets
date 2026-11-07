namespace XRL.World.ZoneBuilders;

public class PlaceAClam : ZoneBuilderSandbox
{
	public int clamNumber;

	public bool BuildZone(Zone Z)
	{
		GameObject gameObject = GameObject.Create("Giant Clam");
		gameObject.SetIntProperty("ClamId", clamNumber);
		ZoneBuilderSandbox.PlaceObject(gameObject, Z, "AdjacentToBlueprint:AlgalWaterDeepPool");
		return true;
	}
}
